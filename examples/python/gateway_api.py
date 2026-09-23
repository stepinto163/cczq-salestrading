import gzip
import json
import logging
import socket
import ssl
import struct
import threading
import time
from dataclasses import dataclass, field
from typing import Callable, Dict, Optional, Union


logger = logging.getLogger(__name__)


class Cmd:
    CONNECT = 0
    CLOSE = 1
    HEARTBEAT = 2
    SEND = 3


class Format:
    REQUEST = 0
    REPLY = 1
    NOTIFY = 2
    SEND = 3


HEADER_LENGTH = 20
DEFAULT_WAIT_TIME = 30.0
HEART_INTERVAL = 10.0


@dataclass
class Proto:
    packet_len: int = HEADER_LENGTH
    header_len: int = HEADER_LENGTH
    cmd: int = 0
    format: int = 0
    seq: int = 0
    session_id: str = ""
    body: Optional[bytes] = None


class GatewayApiError(Exception):
    pass


class GateWayApi:
    def __init__(
        self,
        *,
        timeout: float = DEFAULT_WAIT_TIME,
        use_ssl: bool = False,
        gzip_enabled: bool = False,
        verify_ssl: bool = True,
    ):
        self.timeout = timeout
        self.use_ssl = use_ssl
        self.gzip_enabled = gzip_enabled
        self.verify_ssl = verify_ssl

        self._sock: Optional[socket.socket] = None
        self._seq = 0
        self._seq_lock = threading.Lock()
        self._send_lock = threading.Lock()

        self._pending: Dict[int, "_PendingRequest"] = {}
        self._pending_lock = threading.Lock()
        self._subscriptions: Dict[str, Callable[[str, str], None]] = {}

        self._sid = ""
        self._connected = False
        self._logged_in = False
        self._stop = threading.Event()
        self._reader_thread: Optional[threading.Thread] = None
        self._heartbeat_thread: Optional[threading.Thread] = None
        self._last_received = time.time()

    def connect(self, host: str, port: int, userName: str, pwd: str, session_id: str = "") -> str:
        raw_sock = socket.create_connection((host, port), timeout=self.timeout)
        if self.use_ssl:
            context = ssl.create_default_context()
            if not self.verify_ssl:
                context.check_hostname = False
                context.verify_mode = ssl.CERT_NONE
            raw_sock = context.wrap_socket(raw_sock, server_hostname=host)

        self._sock = raw_sock
        self._connected = True
        self._stop.clear()
        self._start_reader()

        proto = self._request_proto(
            Cmd.CONNECT,
            Format.REQUEST,
            session_id or self._sid,
            {"userName": userName, "pwd": pwd},
            timeout=self.timeout,
        )
        self._sid = proto.session_id
        self._logged_in = bool(self._sid)
        if not self._logged_in:
            raise GatewayApiError("login failed: server returned an empty session id")

        self._start_heartbeat()
        return self._decode_body(proto)

    def requestSync(
        self,
        serverName: str,
        method: str,
        content: str,
        timeout: Optional[float] = None,
    ) -> str:
        self._require_login()
        proto = self._request_proto(
            Cmd.SEND,
            Format.REQUEST,
            self._sid,
            {"serverName": serverName, "method": method, "content": content},
            timeout=timeout or self.timeout,
        )
        return self._decode_body(proto)

    def subscribe(
        self,
        serverName: str,
        topic: str,
        message: Union[Callable[[str, str], None], object],
    ) -> bool:
        self._require_login()
        callback = self._normalize_message_callback(message)
        self._subscriptions[topic] = callback
        try:
            result = self.requestSync(serverName, "subscribe", topic)
        except Exception:
            self._subscriptions.pop(topic, None)
            raise
        if not result:
            self._subscriptions.pop(topic, None)
            return False

        return True

    def disconnect(self) -> None:
        self._connected = False
        self._logged_in = False
        self._stop.set()

        sock = self._sock
        self._sock = None
        if sock is not None:
            try:
                sock.shutdown(socket.SHUT_RDWR)
            except OSError:
                pass
            sock.close()

        with self._pending_lock:
            pending = list(self._pending.values())
            self._pending.clear()
        for request in pending:
            request.error = GatewayApiError("connection closed")
            request.event.set()

    def isConnected(self) -> bool:
        return self._connected

    def isLogin(self) -> bool:
        return self._logged_in

    def _request_proto(
        self,
        cmd: int,
        fmt: int,
        session_id: str,
        body_obj: Optional[dict],
        *,
        timeout: float,
    ) -> Proto:
        seq = self._next_seq()
        body = None if body_obj is None else json.dumps(body_obj, separators=(",", ":")).encode("utf-8")
        proto = Proto(cmd=cmd, format=fmt, seq=seq, session_id=session_id, body=body)
        pending = _PendingRequest()

        with self._pending_lock:
            self._pending[seq] = pending
        try:
            self._send_proto(proto)
            if not pending.event.wait(timeout):
                raise TimeoutError(f"request timeout: seq={seq}")
            if pending.error is not None:
                raise pending.error
            if pending.proto is None:
                raise GatewayApiError(f"empty response: seq={seq}")
            return pending.proto
        finally:
            with self._pending_lock:
                self._pending.pop(seq, None)

    def _send_proto(self, proto: Proto) -> None:
        sock = self._sock
        if sock is None:
            raise GatewayApiError("not connected")

        session_bytes = proto.session_id.encode("utf-8")
        body = proto.body
        if self.gzip_enabled and body is not None:
            body = gzip.compress(body)

        packet_len = HEADER_LENGTH + len(session_bytes) + (len(body) if body else 0)
        header = struct.pack(
            ">ihihi",
            packet_len,
            HEADER_LENGTH,
            proto.cmd,
            proto.format,
            proto.seq,
        )
        frame = header + struct.pack(">i", len(session_bytes)) + session_bytes
        if body:
            frame += body

        with self._send_lock:
            sock.sendall(frame)

    def _read_loop(self) -> None:
        while not self._stop.is_set():
            try:
                proto = self._read_proto()
                if proto is None:
                    break
                self._last_received = time.time()
                self._handle_proto(proto)
            except Exception as exc:
                if not self._stop.is_set():
                    self._fail_pending(exc)
                break
        self._connected = False
        self._logged_in = False

    def _read_proto(self) -> Optional[Proto]:
        header = self._recv_exact(HEADER_LENGTH)
        if header is None:
            return None

        packet_len, header_len, cmd, fmt, seq, sid_len = struct.unpack(">ihihii", header)
        if packet_len < HEADER_LENGTH or sid_len < 0 or sid_len > packet_len - HEADER_LENGTH:
            raise GatewayApiError(f"invalid frame: packet_len={packet_len}, sid_len={sid_len}")

        sid_bytes = self._recv_exact(sid_len) if sid_len else b""
        if sid_bytes is None:
            return None

        body_len = packet_len - HEADER_LENGTH - sid_len
        body = self._recv_exact(body_len) if body_len else None
        if body is None and body_len:
            return None
        if self.gzip_enabled and body is not None:
            body = gzip.decompress(body)

        return Proto(
            packet_len=packet_len,
            header_len=header_len,
            cmd=cmd,
            format=fmt,
            seq=seq,
            session_id=sid_bytes.decode("utf-8"),
            body=body,
        )

    def _recv_exact(self, size: int) -> Optional[bytes]:
        sock = self._sock
        if sock is None:
            return None

        chunks = bytearray()
        while len(chunks) < size:
            chunk = sock.recv(size - len(chunks))
            if not chunk:
                return None
            chunks.extend(chunk)
        return bytes(chunks)

    def _handle_proto(self, proto: Proto) -> None:
        if proto.format == Format.REPLY or proto.cmd == Cmd.CONNECT:
            with self._pending_lock:
                pending = self._pending.get(proto.seq)
            if pending is not None:
                pending.proto = proto
                pending.event.set()
            return

        if proto.format == Format.NOTIFY and proto.body:
            notify = json.loads(proto.body.decode("utf-8"))
            topic = notify.get("topic", "")
            content = notify.get("content", "")
            matched_callbacks = list(self._matching_subscription_callbacks(topic))
            if matched_callbacks:
                for subscribed_topic, callback in matched_callbacks:
                    if subscribed_topic != topic:
                        logger.debug("gateway wildcard subscription matched: subscribed=%s notify=%s", subscribed_topic, topic)
                    try:
                        callback(topic, content)
                    except Exception:
                        logger.exception("gateway subscription callback failed: subscribed=%s notify=%s", subscribed_topic, topic)
            else:
                logger.debug("gateway notify dropped; no subscription matched topic=%s", topic)

    def _matching_subscription_callbacks(self, topic: str):
        callback = self._subscriptions.get(topic)
        if callback is not None:
            yield topic, callback
        for subscribed_topic, subscribed_callback in list(self._subscriptions.items()):
            if subscribed_topic == topic:
                continue
            if self._topic_matches(subscribed_topic, topic):
                yield subscribed_topic, subscribed_callback

    @staticmethod
    def _topic_matches(subscribed_topic: str, notify_topic: str) -> bool:
        if subscribed_topic == notify_topic:
            return True
        if "*" not in subscribed_topic:
            return False
        if subscribed_topic == "**":
            return True
        wild_parts = subscribed_topic.split(".")
        dest_parts = notify_topic.split(".")
        diff = len(dest_parts) - len(wild_parts)
        if diff not in (0, -1) and "**" not in subscribed_topic:
            return False

        matched = False
        for wild, dest in zip(wild_parts, dest_parts):
            if wild == "*":
                matched = True
            elif wild == "**":
                return True
            elif wild == dest:
                matched = True
            else:
                return False
        return matched

    def un_subscribe(self, topic: str) -> None:
        self._subscriptions.pop(topic, None)

    def unsubscribe(self, topic: str) -> None:
        self.un_subscribe(topic)

    def _heartbeat_loop(self) -> None:
        while not self._stop.wait(1.0):
            if not self._logged_in or not self._sid:
                continue
            if time.time() - self._last_received > HEART_INTERVAL:
                try:
                    self._send_proto(Proto(cmd=Cmd.HEARTBEAT, format=Format.SEND, seq=self._next_seq(), session_id=self._sid))
                except Exception:
                    self.disconnect()
                    return

    def _start_reader(self) -> None:
        self._reader_thread = threading.Thread(target=self._read_loop, name="gateway-reader", daemon=True)
        self._reader_thread.start()

    def _start_heartbeat(self) -> None:
        self._heartbeat_thread = threading.Thread(target=self._heartbeat_loop, name="gateway-heartbeat", daemon=True)
        self._heartbeat_thread.start()

    def _next_seq(self) -> int:
        with self._seq_lock:
            self._seq += 1
            if self._seq >= 2_147_483_647:
                self._seq = 1
            return self._seq

    def _decode_body(self, proto: Proto) -> str:
        if not proto.body:
            return ""
        return proto.body.decode("utf-8")

    def _require_login(self) -> None:
        if not self._connected or not self._logged_in:
            raise GatewayApiError("not logged in")

    def _fail_pending(self, exc: Exception) -> None:
        with self._pending_lock:
            pending = list(self._pending.values())
            self._pending.clear()
        for request in pending:
            request.error = exc
            request.event.set()

    @staticmethod
    def _normalize_message_callback(message: Union[Callable[[str, str], None], object]) -> Callable[[str, str], None]:
        if callable(message):
            return message
        on_message = getattr(message, "onMessage", None)
        if callable(on_message):
            return on_message
        on_message = getattr(message, "on_message", None)
        if callable(on_message):
            return on_message
        raise TypeError("message must be callable or expose onMessage(topic, content)")


@dataclass
class _PendingRequest:
    event: threading.Event = field(default_factory=threading.Event)
    proto: Optional[Proto] = None
    error: Optional[Exception] = None
