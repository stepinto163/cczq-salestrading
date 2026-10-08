// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;


public class IdUtil {
    public static String Prefix = "";
    private static SnowflakeIdWorker idWorker = null;

    public static void setWorkerId(int workerId) {
        idWorker = new SnowflakeIdWorker(workerId, 0L);
    }

    public static String getId() {
        if (idWorker == null) {
            idWorker = new SnowflakeIdWorker(0L, 0L);
        }
        return Prefix + idWorker.nextId();
    }
}
