package cgws.ficc.framework.dps;

import java.util.Objects;

/** The existing /MDTService node format, also read by the unmodified STC GW. */
public final class ServiceEndpoint {
    private final String host;
    private final int port;
    private final boolean ready;

    public ServiceEndpoint(String host, int port, boolean ready) {
        if (host == null
                || host.trim().isEmpty()
                || host.indexOf(',') >= 0
                || port < 1
                || port > 65535) {
            throw new IllegalArgumentException("Invalid service endpoint");
        }
        this.host = host;
        this.port = port;
        this.ready = ready;
    }

    public String getHost() {
        return host;
    }

    public int getPort() {
        return port;
    }

    public boolean isReady() {
        return ready;
    }

    public String encode() {
        return "Host:" + host + ",Port:" + port + ",Stat:" + (ready ? "1" : "0");
    }

    public static ServiceEndpoint decode(String value) {
        String host = null;
        int port = 0;
        boolean ready = false;
        for (String field : value.split(",")) {
            String[] pair = field.split(":", 2);
            if (pair.length != 2) {
                throw new IllegalArgumentException("Invalid registry node field");
            }
            switch (pair[0]) {
                case "Host":
                    host = pair[1];
                    break;
                case "Port":
                    port = Integer.parseInt(pair[1]);
                    break;
                case "Stat":
                    ready = "1".equals(pair[1]);
                    break;
                default:
                    break;
            }
        }
        return new ServiceEndpoint(host, port, ready);
    }

    @Override
    public boolean equals(Object other) {
        if (!(other instanceof ServiceEndpoint)) {
            return false;
        }
        ServiceEndpoint endpoint = (ServiceEndpoint) other;
        return host.equals(endpoint.host) && port == endpoint.port && ready == endpoint.ready;
    }

    @Override
    public int hashCode() {
        return Objects.hash(host, port, ready);
    }

    @Override
    public String toString() {
        return host + ":" + port;
    }
}
