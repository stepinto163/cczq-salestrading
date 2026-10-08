package sc.app.stc.parser.utils;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONObject;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.time.LocalDate;

/** Private subprocess protocol. EOF terminates the child even if legacy scheduler threads exist. */
public final class NativeShadowWorker {
    public static void main(String[] args) throws Exception {
        PrintStream protocol = System.out;
        System.setOut(System.err);
        // A parent crash closes stdin. The reader thread must exit even during native initialization.
        java.util.concurrent.BlockingQueue<String> inputs = new java.util.concurrent.ArrayBlockingQueue<String>(4);
        Thread reader = new Thread(() -> {
            try (BufferedReader input = new BufferedReader(new InputStreamReader(System.in, StandardCharsets.UTF_8))) {
                String line;
                while ((line = input.readLine()) != null) {
                    inputs.put(line);
                }
            } catch (Exception error) {
                error.printStackTrace(System.err);
            } finally {
                System.exit(0);
            }
        }, "native-parent-liveness");
        reader.setDaemon(true);
        reader.start();
        try {
            DbOperatorUtil.load();
            NativeBondQuoteEngine engine = new NativeBondQuoteEngine();
            send(protocol, JSON.parseObject("{\"ready\":true}"));
            while (true) {
                JSONObject request = JSON.parseObject(inputs.take());
                JSONObject reply = new JSONObject();
                try {
                    if (request.getBooleanValue("reload")) {
                        engine.reload();
                        reply.put("ready", true);
                    } else {
                        reply.put("output", engine.parse(request.getString("text"), LocalDate.parse(request.getString("date"))));
                    }
                } catch (Throwable error) {
                    error.printStackTrace(System.err);
                    reply.put("error", error.toString());
                }
                send(protocol, reply);
            }
        } catch (Throwable error) {
            error.printStackTrace(System.err);
            JSONObject reply = new JSONObject();
            reply.put("error", error.toString());
            send(protocol, reply);
            System.exit(1);
        }
    }

    private static void send(PrintStream protocol, JSONObject reply) {
        protocol.println(NativeShadowProcess.PREFIX + reply.toJSONString());
        protocol.flush();
    }
}
