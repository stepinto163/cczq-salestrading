package ats.bbox.framework;

public class ServerShell {
    public static IServer startServer(String name, String configuration) {
        if (name == null
                || name.trim().isEmpty()
                || configuration == null
                || configuration.trim().isEmpty()) {
            throw new IllegalArgumentException("Service name and configuration file are required");
        }
        Server.main(new String[] {"-name", name, "-cfg", configuration});
        return Server.getServer();
    }

    public static void startServerWithGUI(String[] arguments) {
        throw new UnsupportedOperationException("Legacy BBox management GUI has not been migrated");
    }

    public static void startServerWithGUI(String name, String configuration) {
        startServerWithGUI(new String[] {"-name", name, "-cfg", configuration});
    }
}
