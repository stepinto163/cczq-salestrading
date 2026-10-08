package sc.app.stc.parser.utils;

import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.parse.BondQuoteEngine;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.time.LocalDate;
import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.TimeUnit;

/** A native crash or hang is confined to this child JVM. Calls are serialized by the comparison worker. */
final class NativeShadowProcess implements BondQuoteEngine, AutoCloseable {
    static final String PREFIX = "FICC_NATIVE_REPLY ";
    private final Process process;
    private final BufferedWriter input;
    private final ArrayBlockingQueue<String> replies = new ArrayBlockingQueue<String>(4);

    NativeShadowProcess() {
        this(NativeShadowWorker.class.getName(), 1200);
    }

    NativeShadowProcess(String workerClass, int startupSeconds) {
        try {
            File logRoot = new File(System.getenv("ATS_ROOT") == null ? "." : System.getenv("ATS_ROOT"), "log");
            if (!logRoot.isDirectory() && !logRoot.mkdirs()) {
                throw new IOException("Cannot create native comparison log directory");
            }
            String executable = new File(System.getProperty("java.home"), "bin/java").getPath();
            process = new ProcessBuilder(executable, "-Xms32m", "-Xmx384m", "-XX:ActiveProcessorCount=1",
                    "-Dfile.encoding=UTF-8", "-cp", System.getProperty("java.class.path"),
                    workerClass)
                    .redirectError(ProcessBuilder.Redirect.appendTo(new File(logRoot, "ParserSvr-native-shadow.log")))
                    .start();
            input = new BufferedWriter(new OutputStreamWriter(process.getOutputStream(), StandardCharsets.UTF_8));
            Thread reader = new Thread(() -> readReplies(), "parser-native-protocol");
            reader.setDaemon(true);
            reader.start();
            Runtime.getRuntime().addShutdownHook(new Thread(() -> close(), "parser-native-cleanup"));
            try {
                await(startupSeconds);
            } catch (Exception error) {
                close();
                throw error;
            }
        } catch (Exception error) {
            throw new IllegalStateException("Native comparison process initialization failed", error);
        }
    }

    private void readReplies() {
        try (BufferedReader reader = new BufferedReader(new InputStreamReader(process.getInputStream(), StandardCharsets.UTF_8))) {
            String line;
            while ((line = reader.readLine()) != null) {
                if (line.startsWith(PREFIX)) {
                    replies.put(line.substring(PREFIX.length()));
                }
            }
        } catch (Exception error) {
            // await() observes process termination/timeouts; no successful result is fabricated.
            process.destroyForcibly();
        }
    }

    private JSONObject await(int seconds) throws Exception {
        long deadline = System.nanoTime() + TimeUnit.SECONDS.toNanos(seconds);
        while (System.nanoTime() < deadline) {
            String reply = replies.poll(200, TimeUnit.MILLISECONDS);
            if (reply != null) {
                JSONObject value = JSON.parseObject(reply);
                if (value.containsKey("error")) {
                    throw new NativeParseException(value.getString("error"));
                }
                return value;
            }
            if (!process.isAlive()) {
                throw new IllegalStateException("Native comparison exited: " + process.exitValue());
            }
        }
        close();
        throw new IllegalStateException("Native comparison exceeded " + seconds + " seconds");
    }

    private JSONObject invoke(JSONObject request, int seconds) {
        try {
            input.write(request.toJSONString());
            input.newLine();
            input.flush();
            return await(seconds);
        } catch (NativeParseException error) {
            throw error;
        } catch (Exception error) {
            close();
            throw new IllegalStateException("Native comparison failed", error);
        }
    }

    @Override
    public AllQuote parse(String text, LocalDate date) {
        JSONObject request = new JSONObject();
        request.put("text", text);
        request.put("date", date.toString());
        return invoke(request, 15).getObject("output", AllQuote.class);
    }

    @Override
    public void reload() {
        JSONObject request = new JSONObject();
        request.put("reload", true);
        invoke(request, 1200);
    }

    @Override
    public void close() {
        process.destroyForcibly();
    }

    private static final class NativeParseException extends IllegalStateException {
        NativeParseException(String message) {
            super(message);
        }
    }
}
