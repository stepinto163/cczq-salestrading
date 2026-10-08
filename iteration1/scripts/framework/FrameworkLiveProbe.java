import com.alibaba.fastjson2.*;
import com.gateway.connector.tcp.client.GateWayApi;

import quickfix.*;

import java.nio.charset.StandardCharsets;
import java.nio.file.*;
import java.util.*;
import java.util.concurrent.*;

/** Local isolated-service functional probe; never submits anything to QTrade. */
public final class FrameworkLiveProbe {
    static final List<GateWayApi> connections = new ArrayList<>();
    static GateWayApi authenticatedGateway;

    static GateWayApi connect(int port, String user, String password) {
        GateWayApi api = new GateWayApi();
        api.setReconnectFlag(false);
        connections.add(api);
        String reply = api.connect("127.0.0.1", port, user, password, false, 1);
        if (!api.isLogin())
            throw new AssertionError("Login failed port=" + port + " reply=" + reply);
        System.out.println("LOGIN port=" + port + " success=true");
        return api;
    }

    static JSONObject request(
            GateWayApi api, String sid, String service, String topic, JSONObject body)
            throws Exception {
        body.putIfAbsent("requestId", "framework-" + UUID.randomUUID());
        String response =
                api == authenticatedGateway
                        ? api.requestSync(
                                service.replace("SERVER.", ""),
                                topic,
                                body.toJSONString(),
                                String.class,
                                10000)
                        : api.requestSync(
                                sid,
                                service.replace("SERVER.", ""),
                                topic,
                                body.toJSONString(),
                                String.class,
                                10000);
        if (response == null) throw new AssertionError("No reply: " + topic);
        JSONObject result = JSON.parseObject(response);
        System.out.println(
                "REPLY topic="
                        + topic
                        + " code="
                        + result.getString("Code")
                        + " keys="
                        + result.keySet());
        return result;
    }

    static JSONObject success(JSONObject reply) {
        if (!"0000".equals(reply.getString("Code"))) throw new AssertionError(reply);
        return reply.getJSONObject("Data");
    }

    static void awaitGatewayRoute(GateWayApi gateway, String sid, String service, String topic)
            throws Exception {
        long deadline = System.nanoTime() + TimeUnit.SECONDS.toNanos(20);
        while (true) {
            JSONObject reply = request(gateway, sid, service, topic, obj("pageSize", 1));
            if ("0000".equals(reply.getString("Code"))) {
                System.out.println("PASS gateway route ready: " + service);
                return;
            }
            // A bound listener precedes the legacy GW's reconnect timer. Wait only for its
            // explicit offline response; business/authentication errors must fail immediately.
            if (!"1003".equals(reply.getString("Code"))
                    || !String.valueOf(reply.getString("Msg")).contains("[1015]")
                    || System.nanoTime() >= deadline) {
                throw new AssertionError("Gateway route did not become ready: " + reply);
            }
            Thread.sleep(200);
        }
    }

    static JSONObject obj(Object... pairs) {
        JSONObject result = new JSONObject();
        for (int index = 0; index < pairs.length; index += 2)
            result.put((String) pairs[index], pairs[index + 1]);
        return result;
    }

    public static void main(String[] args) throws Exception {
        org.apache.log4j.BasicConfigurator.configure();
        org.apache.log4j.Logger.getRootLogger().setLevel(org.apache.log4j.Level.WARN);
        try {
            GateWayApi crm = connect(32983, "FrameworkProbe", "FrameworkProbe");
            JSONObject institutions =
                    request(
                            crm,
                            "probe",
                            "SERVER.CRMSvr",
                            "stc.crm.search",
                            obj("kind", "INSTITUTION", "text", "长城", "offset", 0, "limit", 2));
            if (institutions.getJSONArray("records") == null)
                throw new AssertionError(institutions);
            GateWayApi parser = connect(32943, "FrameworkProbe", "FrameworkProbe");
            JSONObject parsed =
                    request(
                            parser,
                            "probe",
                            "SERVER.ParserSvr",
                            "stc.parser.bond",
                            obj(
                                    "userName",
                                    System.getenv("FICC_FRAMEWORK_TEST_USER"),
                                    "content",
                                    "Bid 240210.IB 2.5 1000 +1\nOfr 250210.IB 2.4 2000 +0"));
            if (!"0000".equals(parsed.getString("Code")) || parsed.getIntValue("totalSize") != 2)
                throw new AssertionError(parsed);
            GateWayApi gateway = new GateWayApi();
            authenticatedGateway = gateway;
            gateway.setGzip(true);
            gateway.setReconnectFlag(false);
            connections.add(gateway);
            Map<String, Object> loginOptions = new HashMap<>();
            loginOptions.put("clientType", "framework-test");
            String login =
                    gateway.connect(
                            "127.0.0.1",
                            32900,
                            System.getenv("FICC_FRAMEWORK_TEST_USER"),
                            System.getenv("FICC_FRAMEWORK_TEST_PASSWORD"),
                            false,
                            1,
                            loginOptions);
            JSONObject identity = JSON.parseObject(login);
            System.out.println(
                    "GW_LOGIN code="
                            + identity.getString("Code")
                            + " message="
                            + identity.getString("Msg"));
            String sid = identity.getString("sid");
            if (sid == null)
                throw new AssertionError(
                        "GW login returned no sid; code=" + identity.getString("Code"));
            GateWayApi quote = gateway;
            awaitGatewayRoute(gateway, sid, "QuoteSvr", "ficc.quote.query");
            awaitGatewayRoute(gateway, sid, "NotificationSvr", "ficc.notification.query");
            Path testRoot =
                    Paths.get(
                            new String(
                                            Files.readAllBytes(
                                                    Paths.get(
                                                            "iteration1/runtime/framework-live-current.txt")),
                                            StandardCharsets.UTF_8)
                                    .trim());
            Path checkpoint = testRoot.resolve("confirmed-quote.json");
            if (Files.exists(checkpoint)) {
                JSONObject expected =
                        JSON.parseObject(
                                new String(Files.readAllBytes(checkpoint), StandardCharsets.UTF_8));
                JSONObject recovered =
                        success(
                                        request(
                                                gateway,
                                                sid,
                                                "QuoteSvr",
                                                "ficc.quote.get",
                                                obj("quoteId", expected.getString("quoteId"))))
                                .getJSONObject("quote");
                for (String field :
                        Arrays.asList("quoteId", "status", "revision", "price", "quantity")) {
                    if (!Objects.equals(expected.getString(field), recovered.getString(field))) {
                        throw new AssertionError("Recovered quote differs in " + field);
                    }
                }
                System.out.println("PASS confirmed quote state survived primary process loss");
            }
            JSONObject stream =
                    success(
                            request(
                                    quote,
                                    sid,
                                    "SERVER.QuoteSvr",
                                    "ficc.quote.stream.open",
                                    obj()));
            BlockingQueue<String> pushes = new LinkedBlockingQueue<>();
            if (!quote.subscribe(
                    "QuoteSvr",
                    stream.getString("pushTopic"),
                    (topic, content) -> pushes.add(content),
                    String.class)) throw new AssertionError("subscribe failed");
            JSONObject notificationStream =
                    success(
                            request(
                                    gateway,
                                    sid,
                                    "NotificationSvr",
                                    "ficc.notification.stream.open",
                                    obj()));
            BlockingQueue<String> notifications = new LinkedBlockingQueue<>();
            if (!gateway.subscribe(
                    "NotificationSvr",
                    notificationStream.getString("pushTopic"),
                    (topic, content) -> notifications.add(content),
                    String.class)) throw new AssertionError("notification subscription failed");
            success(
                    request(
                            gateway,
                            sid,
                            "NotificationSvr",
                            "ficc.notification.view.open",
                            obj("viewId", "framework-notifications", "pageSize", 20)));
            success(
                    request(
                            quote,
                            sid,
                            "SERVER.QuoteSvr",
                            "ficc.quote.view.open",
                            obj(
                                    "viewId",
                                    "framework-live",
                                    "ownerScope",
                                    "MINE",
                                    "status",
                                    JSONArray.of("ACTIVE"),
                                    "pageSize",
                                    20)));
            JSONObject created =
                    success(
                            request(
                                    quote,
                                    sid,
                                    "SERVER.QuoteSvr",
                                    "ficc.quote.create",
                                    obj(
                                            "quotes",
                                            JSONArray.of(
                                                    obj(
                                                            "clientQuoteId",
                                                            "framework-" + UUID.randomUUID(),
                                                            "securityId",
                                                            "240210.IB",
                                                            "side",
                                                            "1",
                                                            "quoteType",
                                                            "1",
                                                            "yieldType",
                                                            "1",
                                                            "price",
                                                            "2.5",
                                                            "quantity",
                                                            "1000",
                                                            "settlType",
                                                            "1",
                                                            "tradeDate",
                                                            java.time.LocalDate.now()
                                                                    .toString())))));
            JSONObject item = created.getJSONArray("results").getJSONObject(0);
            if (!"0000".equals(item.getString("Code"))) throw new AssertionError(item);
            String id = item.getString("quoteId");
            JSONObject stored =
                    success(
                                    request(
                                            quote,
                                            sid,
                                            "SERVER.QuoteSvr",
                                            "ficc.quote.get",
                                            obj("quoteId", id)))
                            .getJSONObject("quote");
            success(
                    request(
                            quote,
                            sid,
                            "SERVER.QuoteSvr",
                            "ficc.quote.confirm",
                            obj(
                                    "quotes",
                                    JSONArray.of(
                                            obj(
                                                    "quoteId",
                                                    id,
                                                    "expectedRevision",
                                                    stored.getString("revision"))))));
            awaitContaining(pushes, id, "quote publication");
            JSONObject confirmed =
                    success(request(gateway, sid, "QuoteSvr", "ficc.quote.get", obj("quoteId", id)))
                            .getJSONObject("quote");
            Files.write(checkpoint, confirmed.toJSONString().getBytes(StandardCharsets.UTF_8));
            success(
                    request(
                            quote,
                            sid,
                            "SERVER.QuoteSvr",
                            "ficc.price.query",
                            obj(
                                    "securityIds",
                                    JSONArray.of("240210.IB"),
                                    "mode",
                                    "DEPTH",
                                    "depth",
                                    1)));
            awaitContaining(notifications, id, "MY_BEST notification publication");
            success(
                    request(
                            gateway,
                            sid,
                            "NotificationSvr",
                            "ficc.notification.query",
                            obj("pageSize", 20)));
            receiveFix(gateway, sid, identity.getString("userid"));
            System.out.println(
                    "PASS CRM, multiline parser, standard GW login/routing, quote"
                            + " lifecycle/publication, price, notification and FIX intake");
        } finally {
            for (GateWayApi api : connections) {
                api.setLogOut(true);
                api.disconnect();
            }
        }
    }

    static void awaitContaining(BlockingQueue<String> messages, String id, String description)
            throws Exception {
        long end = System.nanoTime() + TimeUnit.SECONDS.toNanos(12);
        while (System.nanoTime() < end) {
            String message = messages.poll(200, TimeUnit.MILLISECONDS);
            if (message != null && message.contains(id)) {
                System.out.println("PASS " + description);
                return;
            }
        }
        throw new AssertionError("Missing " + description);
    }

    static void receiveFix(GateWayApi gateway, String sid, String owner) throws Exception {
        Path root =
                Paths.get(
                        new String(
                                        Files.readAllBytes(
                                                Paths.get(
                                                        "iteration1/runtime/framework-live-current.txt")),
                                        StandardCharsets.UTF_8)
                                .trim());
        Files.write(
                root.resolve("control/FrameworkOwnerMap.properties"),
                ("FRAMEWORK_ACCOUNT=" + owner + "\n").getBytes(StandardCharsets.UTF_8));
        String configuration =
                "[default]\n"
                        + "ConnectionType=acceptor\n"
                        + "SocketAcceptAddress=127.0.0.1\n"
                        + "SocketAcceptPort=32984\n"
                        + "StartTime=00:00:00\n"
                        + "EndTime=00:00:00\n"
                        + "HeartBtInt=30\n"
                        + "UseDataDictionary=N\n"
                        + "ValidateIncomingMessage=N\n"
                        + "ValidateUserDefinedFields=N\n"
                        + "ResetOnLogon=Y\n"
                        + "[session]\n"
                        + "BeginString=FIX.4.4\n"
                        + "SenderCompID=FRAMEWORK_SERVER\n"
                        + "TargetCompID=FRAMEWORK_CLIENT\n";
        CountDownLatch connected = new CountDownLatch(1);
        SessionSettings settings =
                new SessionSettings(
                        new java.io.ByteArrayInputStream(
                                configuration.getBytes(StandardCharsets.UTF_8)));
        SocketAcceptor acceptor =
                new SocketAcceptor(
                        new ApplicationAdapter() {
                            @Override
                            public void onLogon(SessionID id) {
                                connected.countDown();
                            }
                        },
                        new MemoryStoreFactory(),
                        settings,
                        new ScreenLogFactory(false, false, false),
                        new DefaultMessageFactory());
        boolean started = false;
        try {
            acceptor.start();
            started = true;
            if (!connected.await(8, TimeUnit.SECONDS))
                throw new AssertionError("QtIntake FIX loopback logon timed out");
            String externalId = "FRAMEWORK_FIX_" + UUID.randomUUID();
            Message incoming = new Message();
            incoming.getHeader().setString(35, "S");
            incoming.setString(117, externalId);
            incoming.setString(18045, "N");
            incoming.setString(18035, "2");
            incoming.setString(18004, "FRAMEWORK_ACCOUNT");
            incoming.setString(
                    60,
                    java.time.LocalDateTime.now()
                            .format(
                                    java.time.format.DateTimeFormatter.ofPattern(
                                            "yyyyMMdd-HH:mm:ss")));
            incoming.setString(48, "240210");
            incoming.setString(100, "XCFE");
            incoming.setString(54, "1");
            incoming.setString(18042, "1");
            incoming.setString(132, "2.55");
            incoming.setString(134, "10000000");
            incoming.setString(63, "2");
            incoming.setString(18026, "Framework isolated loopback quote");
            if (!Session.sendToTarget(incoming, acceptor.getSessions().get(0)))
                throw new AssertionError("FIX send failed");
            long end = System.nanoTime() + TimeUnit.SECONDS.toNanos(12);
            while (System.nanoTime() < end) {
                JSONObject result =
                        success(
                                request(
                                        gateway,
                                        sid,
                                        "QuoteSvr",
                                        "ficc.quote.query",
                                        obj(
                                                "ownerScope",
                                                "MINE",
                                                "status",
                                                JSONArray.of("PENDING_CONFIRM"),
                                                "pageSize",
                                                20)));
                if (result.toJSONString().contains(externalId)) {
                    System.out.println(
                            "PASS incoming FIX quote persisted and returned through standard GW");
                    return;
                }
                Thread.sleep(200);
            }
            throw new AssertionError("FIX quote was not returned by QuoteSvr");
        } finally {
            if (started) acceptor.stop(true);
        }
    }
}
