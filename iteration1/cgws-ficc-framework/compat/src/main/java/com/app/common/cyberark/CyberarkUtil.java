// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.cyberark;

import com.app.common.utils.JsonUtils;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.BufferedReader;
import java.io.DataOutputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.security.KeyManagementException;
import java.security.NoSuchAlgorithmException;
import java.util.HashMap;
import java.util.Map;

import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;

public class CyberarkUtil {
    private Logger logger = LoggerFactory.getLogger(CyberarkUtil.class);
    private String remoteUrl;
    private String appId;
    private String appKey;

    public void setSSLVerify(Boolean verify) {
        if (verify == null || verify.booleanValue()) {
            return;
        }
        this.logger.info("setSSLVerify:{}", (Object) verify);
        HttpsURLConnection.setDefaultHostnameVerifier((hostname, session) -> true);
        try {
            SSLContext sc = SSLContext.getInstance("TLS");
            sc.init(null, null, null);
            HttpsURLConnection.setDefaultSSLSocketFactory(sc.getSocketFactory());
        } catch (KeyManagementException | NoSuchAlgorithmException e) {
            throw new RuntimeException(e);
        }
    }

    public String getPassword(String safe, String folder, String object, String reason) {
        return this.getPasswordFromRemote(
                this.remoteUrl, this.appId, safe, folder, object, reason, this.appKey);
    }

    public String getPasswordFromRemote(
            String url,
            String appId,
            String safe,
            String folder,
            String object,
            String reason,
            String appKey) {
        this.logger.info(
                "getPasswordFromRemote url: {}, appId: {}, safe: {}, folder: {}, object: {},"
                    + " reason: {}, appKey: {}",
                new Object[] {url, appId, safe, folder, object, reason, appKey});
        try {
            HashMap<String, Object> param = new HashMap<String, Object>();
            param.put("appId", appId);
            param.put("safe", safe);
            param.put("folder", folder);
            param.put("object", object);
            param.put("reason", reason);
            param.put("sign", SignUtil.makeSign(param, appKey));
            param.put("encrypt_ver", "2.0");
            URL postUrl = new URL(url);
            HttpURLConnection conn = (HttpURLConnection) postUrl.openConnection();
            conn.setDoOutput(true);
            conn.setDoInput(true);
            conn.setRequestMethod("POST");
            conn.setUseCaches(false);
            conn.setRequestProperty("Content-Type", "application/json");
            conn.connect();
            DataOutputStream out = new DataOutputStream(conn.getOutputStream());
            String req = JsonUtils.Serializer(param);
            this.logger.info("request: {}", (Object) req);
            out.writeBytes(req);
            out.flush();
            out.close();
            BufferedReader reader =
                    new BufferedReader(
                            new InputStreamReader(conn.getInputStream(), StandardCharsets.UTF_8));
            StringBuilder sb2 = new StringBuilder();
            String line = reader.readLine();
            while (line != null) {
                sb2.append(line);
                line = reader.readLine();
            }
            this.logger.info("repsonse: {}", (Object) sb2);
            Map result = JsonUtils.Deserialize(sb2.toString(), Map.class);
            if (result != null) {
                this.logger.info(
                        "pidms remote call appId: {}, object: {}, status code: {}",
                        new Object[] {appId, object, result.get("code")});
            }
            if (result != null && "200".equals(result.get("code"))) {
                return SecurityUtil.decrypt20((String) result.get("password"), appKey);
            }
            throw new Exception((String) result.get("msg"));
        } catch (Exception e) {
            throw new RuntimeException("getpassowrd from remote error", e);
        }
    }
}
