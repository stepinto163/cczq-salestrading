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
import java.util.Optional;
import java.util.UUID;
import java.util.stream.Stream;

/** One process owns the directory. Received work is flushed before the FIX callback returns. */
public final class DurableInbox implements AutoCloseable {
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
                throw new IOException("QtIntake inbox is owned by another process");
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
        } catch (IOException error) {
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
                if (!JsonUtils.canonicalHash(previous).equals(JsonUtils.canonicalHash(request))) {
                    throw new IOException(
                            "Conflicting QTrade replay for requestId="
                                    + request.getString("requestId"));
                }
                return;
            }
        }
        try (Stream<Path> files = Files.list(root.resolve("pending"))) {
            if (files.filter(path -> path.toString().endsWith(".json")).limit(capacity).count()
                    >= capacity) {
                throw new IOException(
                        "QtIntake inbox capacity reached; stop intake and recover before replay");
            }
        }
        write(root.resolve("pending").resolve(file), JsonUtils.object("request", request));
    }

    public synchronized Path next() throws IOException {
        try (Stream<Path> files = Files.list(root.resolve("pending"))) {
            Optional<Path> first =
                    files.filter(path -> path.toString().endsWith(".json"))
                            .min(Comparator.comparingLong(path -> path.toFile().lastModified()));
            if (first.isPresent()) {
                // An unmapped account must not block later quotes indefinitely.
                Files.setLastModifiedTime(
                        first.get(),
                        java.nio.file.attribute.FileTime.fromMillis(System.currentTimeMillis()));
            }
            return first.orElse(null);
        }
    }

    public JSONObject request(Path file) throws IOException {
        return read(file).getJSONObject("request");
    }

    public synchronized void saveRequest(Path file, JSONObject request) throws IOException {
        write(file, JsonUtils.object("request", request));
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
    public void close() throws IOException {
        try {
            lock.release();
        } finally {
            lockChannel.close();
        }
    }
}
