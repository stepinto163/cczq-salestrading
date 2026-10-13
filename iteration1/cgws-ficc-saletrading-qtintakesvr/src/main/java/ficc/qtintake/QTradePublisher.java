package ficc.qtintake;

import com.alibaba.fastjson2.JSONObject;
import ficc.api.ProtocolCodec;
import ficc.api.QTradePublishData;
import ficc.api.QTradePublishRequest;
import ficc.common.ApiException;
import org.apache.log4j.Logger;
import quickfix.DataDictionary;
import quickfix.Group;
import quickfix.Message;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.StandardCopyOption;
import java.nio.file.StandardOpenOption;
import java.security.MessageDigest;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.util.ArrayDeque;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

/** U41 单次发送与 U29/Reject 关联。超时结果保持 UNKNOWN，绝不自动重发。 */
public final class QTradePublisher {
    private static final Logger LOGGER = Logger.getLogger(QTradePublisher.class);
    private final Path receipts;
    private final long timeoutMillis;
    private final DataDictionary ackDictionary;
    private final ArrayDeque<Long> sends = new ArrayDeque<>();
    private final Map<String, CompletableFuture<QTradePublishData>> pending = new HashMap<>();
    private final Map<Integer, String> sequences = new HashMap<>();

    @FunctionalInterface
    public interface Sender {
        boolean send(Message message) throws Exception;
    }

    public QTradePublisher(Path receipts, long timeoutMillis) throws Exception {
        if (timeoutMillis < 100 || timeoutMillis > 10000) {
            throw new IllegalArgumentException("publish.ackTimeoutMillis must be 100..10000");
        }
        this.receipts = receipts;
        this.timeoutMillis = timeoutMillis;
        Files.createDirectories(receipts);
        try (java.io.InputStream input = getClass().getResourceAsStream("/qtrade-publish-ack.xml")) {
            if (input == null) {
                throw new IllegalStateException("Missing qtrade-publish-ack.xml");
            }
            ackDictionary = new DataDictionary(input);
        }
    }

    public QTradePublishData publish(QTradePublishRequest request, Sender sender) throws Exception {
        Message message = QTradePublishMessage.create(request, LocalDateTime.now(ZoneId.of("Asia/Shanghai")));
        // 时间字段由服务生成，不属于同一 MsgID 的业务内容比较。
        message.removeField(60);
        String fingerprint = hash(message.toString());
        message.setString(60, LocalDateTime.now(ZoneId.of("Asia/Shanghai"))
                .format(java.time.format.DateTimeFormatter.ofPattern("yyyyMMdd-HH:mm:ss")));
        CompletableFuture<QTradePublishData> future;
        synchronized (this) {
            Path path = path(request.messageId);
            if (Files.exists(path)) {
                JSONObject receipt = read(path);
                if (!fingerprint.equals(receipt.getString("fingerprint"))) {
                    throw ApiException.forField("1001", "CONFLICT", "MsgID already used with different content", "messageId");
                }
                return ProtocolCodec.decode(receipt.getJSONObject("result"), QTradePublishData.class);
            }
            checkRate(System.currentTimeMillis());
            QTradePublishData initial = result(request.messageId, "UNKNOWN", "Awaiting QTrade acknowledgement; do not resend");
            JSONObject receipt = new JSONObject();
            receipt.put("fingerprint", fingerprint);
            receipt.put("result", initial.toJson());
            // 发出前落盘；进程在发送前后崩溃时宁可 UNKNOWN，不以新 MsgID 自动重发。
            writeNew(path, receipt.toJSONString());
            future = new CompletableFuture<>();
            pending.put(request.messageId, future);
            sends.addLast(System.currentTimeMillis());
        }
        try {
            boolean sent = sender.send(message);
            if (!sent) {
                complete(result(request.messageId, "REJECTED", "FIX session did not accept the message"));
            }
            try {
                return future.get(timeoutMillis, TimeUnit.MILLISECONDS);
            } catch (TimeoutException error) {
                LOGGER.warn("PUBLISH state=UNKNOWN messageId=" + request.messageId + " reason=ACK_TIMEOUT");
                synchronized (this) {
                    return ProtocolCodec.decode(read(path(request.messageId)).getJSONObject("result"), QTradePublishData.class);
                }
            }
        } catch (InterruptedException error) {
            Thread.currentThread().interrupt();
            throw error;
        } finally {
            synchronized (this) {
                pending.remove(request.messageId);
                sequences.values().removeIf(request.messageId::equals);
            }
        }
    }

    /** QuickFIX 在 toApp 中已分配 Tag34；必须在实际 socket 发送前登记。 */
    public synchronized void sending(Message message) throws Exception {
        String id = message.getString(18044);
        int sequence = message.getHeader().getInt(34);
        sequences.put(sequence, id);
        JSONObject receipt = read(path(id));
        receipt.getJSONObject("result").put("fixSequence", sequence);
        replace(path(id), receipt);
    }

    public void acknowledge(Message incoming) throws Exception {
        Message ack = new Message();
        String raw = incoming.toRawString();
        ack.fromString(raw == null ? incoming.toString() : raw, ackDictionary, false);
        String id = ack.getString(18044);
        String code = ack.getString(11251);
        if (!code.matches("[012]") || !ack.hasGroup(11261)) {
            throw new IllegalArgumentException("Malformed QTrade U29 acknowledgement for " + id);
        }
        QTradePublishData data = result(id, "0".equals(code) ? "ACKNOWLEDGED" : "1".equals(code) ? "PARTIAL" : "REJECTED",
                "QTrade business acknowledgement");
        data.applicationCode = code;
        for (Group group : ack.getGroups(11261)) {
            QTradePublishData.RecipientResult recipient = new QTradePublishData.RecipientResult();
            recipient.routingType = group.getInt(216);
            recipient.routingId = group.getString(217);
            recipient.description = group.getString(58);
            if (group.isSetField(18218)) {
                recipient.errorType = group.getInt(18218);
            }
            if (group.isSetField(18031)) {
                recipient.clientQuoteId = group.getString(18031);
            }
            data.recipients.add(recipient);
        }
        complete(data);
    }

    public synchronized void reject(Message message) throws Exception {
        String id = sequences.get(message.getInt(45));
        if (id != null) {
            complete(result(id, "REJECTED", message.isSetField(58) ? message.getString(58) : "FIX session reject"));
        }
    }

    public synchronized void disconnected() {
        sequences.clear(); // 序号重置后不能用旧连接的 Tag34 关联新消息。
        for (Map.Entry<String, CompletableFuture<QTradePublishData>> entry : pending.entrySet()) {
            entry.getValue().complete(result(entry.getKey(), "UNKNOWN", "FIX disconnected before acknowledgement; do not resend"));
        }
    }

    private synchronized void complete(QTradePublishData result) throws Exception {
        Path file = path(result.messageId);
        if (!Files.exists(file)) {
            LOGGER.warn("PUBLISH state=UNMATCHED_ACK messageId=" + result.messageId);
            return;
        }
        JSONObject receipt = read(file);
        result.fixSequence = receipt.getJSONObject("result").getInteger("fixSequence");
        receipt.put("result", result.toJson());
        replace(file, receipt);
        CompletableFuture<QTradePublishData> future = pending.get(result.messageId);
        if (future != null) {
            future.complete(result);
        }
        LOGGER.info("PUBLISH state=" + result.status + " messageId=" + result.messageId);
    }

    private void checkRate(long now) {
        while (!sends.isEmpty() && now - sends.peekFirst() >= 60000) {
            sends.removeFirst();
        }
        long recent = sends.stream().filter(time -> now - time < 1000).count();
        if (recent >= 2 || sends.size() >= 50) {
            throw ApiException.forField("1009", "RATE_LIMITED", "QTrade limit: 2 requests/second and 50/minute; nothing sent", null);
        }
    }

    private Path path(String messageId) throws Exception {
        // MsgID 允许 | 和 *，Windows 文件名不允许；文件名用哈希，原 MsgID 保存在回执内容中。
        return receipts.resolve(hash(messageId) + ".json");
    }

    private static String hash(String text) throws Exception {
        byte[] digest = MessageDigest.getInstance("SHA-256").digest(text.getBytes(StandardCharsets.UTF_8));
        StringBuilder value = new StringBuilder();
        for (byte part : digest) {
            value.append(String.format("%02x", part & 255));
        }
        return value.toString();
    }

    private static JSONObject read(Path path) throws Exception {
        return JSONObject.parseObject(new String(Files.readAllBytes(path), StandardCharsets.UTF_8));
    }

    private static void writeNew(Path path, String value) throws Exception {
        try (java.nio.channels.FileChannel file = java.nio.channels.FileChannel.open(path, StandardOpenOption.CREATE_NEW, StandardOpenOption.WRITE)) {
            java.nio.ByteBuffer bytes = StandardCharsets.UTF_8.encode(value);
            while (bytes.hasRemaining()) {
                file.write(bytes);
            }
            file.force(true);
        }
    }

    private static void replace(Path path, JSONObject receipt) throws Exception {
        Path temporary = Files.createTempFile(path.getParent(), "receipt-", ".tmp");
        try {
            Files.write(temporary, receipt.toJSONString().getBytes(StandardCharsets.UTF_8));
            Files.move(temporary, path, StandardCopyOption.ATOMIC_MOVE, StandardCopyOption.REPLACE_EXISTING);
        } finally {
            Files.deleteIfExists(temporary);
        }
    }

    private static QTradePublishData result(String id, String status, String description) {
        QTradePublishData result = new QTradePublishData();
        result.messageId = id;
        result.status = status;
        result.description = description;
        return result;
    }
}
