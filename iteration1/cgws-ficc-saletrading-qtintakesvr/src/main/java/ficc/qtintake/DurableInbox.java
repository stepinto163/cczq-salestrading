package ficc.qtintake;

import com.alibaba.fastjson2.JSONObject;

import ficc.common.JsonUtils;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.channels.FileChannel;
import java.nio.channels.FileLock;
import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.Comparator;
import java.util.ArrayList;
import java.util.List;
import java.util.TreeMap;
import org.apache.log4j.Logger;
import java.util.UUID;
import java.util.stream.Stream;

/**
 * 本地持久收件箱：pending 保存待转发请求，sent/rejected 保存明确处理结果。
 * 目录由单进程加锁持有；入箱先落盘再通知转发线程，arrivalSequence 保存本地接收顺序。
 * 它保障 FIX 接收以后到 QuoteSvr 应答之间的恢复，不替代 FIX 会话自身的序列与重传机制。
 * capacity 限制积压条数，不是吞吐配额；满时抛出异常，不能假装已保存消息。
 */
public final class DurableInbox implements AutoCloseable {
    private static final Logger LOGGER = Logger.getLogger(DurableInbox.class);
    private final TreeMap<Long, Path> pendingFiles = new TreeMap<>();
    private long lastSequence;
    private boolean closed;
    private final Path root;
    private final int capacity;
    private final FileChannel lockChannel;
    private final FileLock lock;

    public DurableInbox(Path root, int capacity) throws IOException {
        if (capacity < 1) {
            throw new IllegalArgumentException("Inbox capacity must be positive");
        }
        this.root = root.toAbsolutePath();
        this.capacity = capacity;
        Files.createDirectories(this.root);
        lockChannel =
                FileChannel.open(
                        this.root.resolve("owner.lock"),
                        StandardOpenOption.CREATE,
                        StandardOpenOption.WRITE);
        FileLock acquired;
        try {
            acquired = lockChannel.tryLock();
            if (acquired == null) {
                throw new IOException("QtIntakeSvr inbox is owned by another process");
            }
        } catch (IOException | RuntimeException error) {
            lockChannel.close();
            throw error;
        }
        lock = acquired;
        try {
            for (String state : new String[] {"pending", "sent", "rejected"}) {
                Files.createDirectories(this.root.resolve(state));
            }
            restorePendingOrder();
        } catch (IOException | RuntimeException error) {
            close();
            throw error;
        }
    }

    public synchronized void enqueue(JSONObject request) throws IOException {
        String file = filename(request);
        for (String state : new String[] {"pending", "sent", "rejected"}) {
            Path existing = root.resolve(state).resolve(file);
            if (Files.exists(existing)) {
                JSONObject previous = read(existing).getJSONObject("request");
                previous.getJSONObject("intake").remove("ownerUserId");
                JSONObject incoming = JsonUtils.deepCopy(request);
                if (!previous.getJSONObject("intake").containsKey("originalMessageId")) {
                    // Already queued/sent events keep their original contract on upgrade.
                    incoming.getJSONObject("intake").remove("originalMessageId");
                }
                if (!JsonUtils.canonicalHash(previous).equals(JsonUtils.canonicalHash(incoming))) {
                    throw new IOException(
                            "Conflicting QTrade replay for requestId="
                                    + request.getString("requestId"));
                }
                return;
            }
        }
        if (closed) {
            throw new IOException("Inbox is closed");
        }
        if (pendingFiles.size() >= capacity) {
            throw new IOException("QtIntakeSvr inbox capacity reached; incoming request not saved");
        }
        long sequence = Math.addExact(lastSequence, 1);
        Path target = root.resolve("pending").resolve(file);
        write(target, JsonUtils.object("arrivalSequence", sequence, "request", request));
        lastSequence = sequence;
        pendingFiles.put(sequence, target);
        notifyAll();
    }

    /** 重启后按持久到达序号恢复；旧格式无序号时只能按文件时间迁移，并明确记录原顺序不可知。 */
    private void restorePendingOrder() throws IOException {
        List<Path> legacy = new ArrayList<>();
        for (String state : new String[] {"pending", "sent", "rejected"}) {
            try (Stream<Path> files = Files.list(root.resolve(state))) {
                for (Path file : (Iterable<Path>) files.filter(path -> path.toString().endsWith(".json"))::iterator) {
                    JSONObject record = read(file);
                    Long sequence = record.getLong("arrivalSequence");
                    if (sequence != null) {
                        if (sequence < 1) {
                            throw new IOException("Invalid inbox arrival sequence: " + file);
                        }
                        lastSequence = Math.max(lastSequence, sequence);
                        if ("pending".equals(state) && pendingFiles.put(sequence, file) != null) {
                            throw new IOException("Duplicate pending arrival sequence: " + sequence);
                        }
                    } else if ("pending".equals(state)) {
                        legacy.add(file);
                    }
                }
            }
        }
        legacy.sort(Comparator.comparingLong((Path path) -> path.toFile().lastModified())
                .thenComparing(path -> path.getFileName().toString()));
        if (!legacy.isEmpty()) {
            LOGGER.warn("INBOX state=MIGRATING legacyPending=" + legacy.size()
                    + " order=LAST_MODIFIED originalArrivalOrder=UNAVAILABLE");
        }
        for (Path file : legacy) {
            JSONObject record = read(file);
            long sequence = Math.addExact(lastSequence, 1);
            record.put("arrivalSequence", sequence);
            write(file, record);
            lastSequence = sequence;
            pendingFiles.put(sequence, file);
        }
    }

    public synchronized Path next() {
        return pendingFiles.isEmpty() ? null : pendingFiles.firstEntry().getValue();
    }

    public synchronized boolean awaitPending() throws InterruptedException {
        while (pendingFiles.isEmpty() && !closed) {
            wait();
        }
        return !closed;
    }

    public synchronized int pendingCount() {
        return pendingFiles.size();
    }

    public JSONObject request(Path file) throws IOException {
        return read(file).getJSONObject("request");
    }

    public synchronized void saveRequest(Path file, JSONObject request) throws IOException {
        JSONObject record = read(file);
        record.put("request", request);
        write(file, record);
    }

    public synchronized void finish(Path pending, JSONObject reply, boolean success)
            throws IOException {
        JSONObject record = read(pending);
        record.put("reply", reply);
        write(pending, record);
        Files.move(
                pending,
                root.resolve(success ? "sent" : "rejected").resolve(pending.getFileName()),
                StandardCopyOption.ATOMIC_MOVE);
        pendingFiles.remove(record.getLong("arrivalSequence"));
    }

    public synchronized void rejectRaw(String fix, String reason) throws IOException {
        write(
                root.resolve("rejected").resolve("invalid-" + UUID.randomUUID() + ".json"),
                JsonUtils.object("fix", fix, "reason", reason));
    }

    private static String filename(JSONObject request) {
        return JsonUtils.canonicalHash(
                        JsonUtils.object("requestId", request.getString("requestId")))
                + ".json";
    }

    private static JSONObject read(Path path) throws IOException {
        return JsonUtils.parseObject(new String(Files.readAllBytes(path), StandardCharsets.UTF_8));
    }

    private static void write(Path target, JSONObject record) throws IOException {
        Path temporary = target.resolveSibling(target.getFileName() + ".tmp");
        try (FileChannel file =
                FileChannel.open(
                        temporary,
                        StandardOpenOption.CREATE,
                        StandardOpenOption.TRUNCATE_EXISTING,
                        StandardOpenOption.WRITE)) {
            ByteBuffer bytes = StandardCharsets.UTF_8.encode(JsonUtils.toJson(record));
            while (bytes.hasRemaining()) {
                file.write(bytes);
            }
            file.force(true);
        }
        Files.move(
                temporary,
                target,
                StandardCopyOption.REPLACE_EXISTING,
                StandardCopyOption.ATOMIC_MOVE);
    }

    @Override
    public synchronized void close() throws IOException {
        if (closed) {
            return;
        }
        closed = true;
        notifyAll();
        try {
            lock.release();
        } finally {
            lockChannel.close();
        }
    }
}
