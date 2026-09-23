"""通过 gateway_api.py 请求报价并订阅推送；不创建或修改报价。"""
import getpass
import json
import os
import threading
import uuid

from gateway_api import GateWayApi


def main():
    gateway = GateWayApi(timeout=30)
    login_gateway = gateway
    received_push = threading.Event()
    view_id = "PY-" + uuid.uuid4().hex
    view_opened = False

    def request(topic, body, server="QuoteSvr"):
        print("REQ", topic, json.dumps(body, ensure_ascii=False))
        content = body if isinstance(body, str) else json.dumps(body)
        reply = json.loads(gateway.requestSync(server, topic, content))
        print("REP", topic, json.dumps(reply, ensure_ascii=False))
        if reply.get("Code") != "0000":
            raise RuntimeError(f"{topic}: {reply.get('Code')} {reply.get('Msg')}")
        return reply.get("Data", reply)

    def on_message(topic, content):
        # 回调在接收线程执行；不要在这里调用 requestSync，会阻塞接收回复。
        message = json.loads(content)
        print("PUSH", topic, json.dumps(message, ensure_ascii=False), flush=True)
        received_push.set()

    try:
        username = os.environ.get("FICC_USER") or input("STC username: ")
        password = os.environ.get("FICC_PASSWORD") or getpass.getpass("Password: ")
        login = json.loads(gateway.connect(
            os.environ.get("FICC_HOST", "127.0.0.1"),
            int(os.environ.get("FICC_PORT", "18600")), username, password))
        if login.get("Code") not in ("0000", "0001"):
            raise RuntimeError(f"Login failed: {login.get('Code')} {login.get('Msg')}")
        print("LOGIN OK")  # 不打印登录回复中的密码和会话信息。
        subscription_server = "gateway"
        if os.environ.get("FICC_DIRECT") == "1":
            # 业务请求直连 QuoteSvr，但仍需先通过登录取得真实 sid。
            gateway = GateWayApi(timeout=30)
            gateway.connect(os.environ.get("FICC_QUOTE_HOST", "127.0.0.1"),
                            int(os.environ.get("FICC_QUOTE_PORT", "18680")),
                            username, "", session_id=login["sid"])
            subscription_server = "QuoteSvr"
            print("Quote requests/subscriptions now connect directly to QuoteSvr")

        # 1. 请求打开报价流，使用服务端返回的 topic，不自行拼接。
        stream = request("ficc.quote.stream.open", {"requestId": uuid.uuid4().hex})
        topic = stream["pushTopic"]

        # 2. 注册普通订阅；报价流不发送快照。
        if not gateway.subscribe(subscription_server, topic, on_message):
            raise RuntimeError("Subscription rejected")
        # 此 Python SDK 只检查回复非空；再核对注册回包 Code（重复注册是幂等的）。
        request("subscribe", topic, server=subscription_server)

        # 3. 请求当前筛选的第一批。之后变化由上面的回调收到。
        page = request("ficc.quote.view.open", {
            "requestId": uuid.uuid4().hex,
            "viewId": view_id,
            "ownerScope": "MINE",
            "status": ["PENDING_CONFIRM", "ACTIVE", "REFERRED", "DELETED"],
            "pageSize": 5,
            "filter": {"conditions": []},
        })
        view_opened = True
        print("Initial quotes:", len(page["quotes"]))
        print("Listening for 12 seconds. HEARTBEAT is normal when no quote changes.")
        if not received_push.wait(12):
            raise TimeoutError("No quote-stream push received within 12 seconds")
        print("PASS request/reply and subscription callback")
    finally:
        try:
            if view_opened and gateway.isLogin():
                request("ficc.quote.view.close", {
                    "requestId": uuid.uuid4().hex, "viewId": view_id})
        finally:
            gateway.disconnect()  # 关闭连接也释放订阅。
            if login_gateway is not gateway:
                login_gateway.disconnect()


if __name__ == "__main__":
    main()
