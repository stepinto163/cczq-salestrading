import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;

import org.I0Itec.zkclient.ZkClient;
import org.I0Itec.zkclient.serialize.ZkSerializer;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;

/** Read-only registry verification. No business data is created. */
public final class RegistryProbe {
    public static void main(String[] args) throws Exception {
        ZkClient registry = new ZkClient(args[0], 10000, 5000);
        registry.setZkSerializer(
                new ZkSerializer() {
                    public byte[] serialize(Object value) {
                        return value.toString().getBytes(StandardCharsets.UTF_8);
                    }

                    public Object deserialize(byte[] value) {
                        return new String(value, StandardCharsets.UTF_8);
                    }
                });
        try {
            JSONObject services = new JSONObject();
            for (String service : new String[] {"QuoteSvr", "NotificationSvr"}) {
                String parent = "/MDTService/" + service;
                JSONArray nodes = new JSONArray();
                for (String child : registry.getChildren(parent)) {
                    JSONObject node = new JSONObject();
                    String path = parent + "/" + child;
                    node.put("path", path);
                    node.put("endpoint", (String) registry.readData(path));
                    nodes.add(node);
                }
                if (nodes.isEmpty()) {
                    throw new IllegalStateException(service + " is not registered");
                }
                services.put(service, nodes);
            }
            Files.write(
                    Paths.get(args[1]), services.toJSONString().getBytes(StandardCharsets.UTF_8));
            System.out.println(services.toJSONString());
        } finally {
            registry.close();
        }
    }
}
