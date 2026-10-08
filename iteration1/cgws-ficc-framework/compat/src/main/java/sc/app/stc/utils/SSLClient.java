// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import ats.app.ym.utils.encrypt.Base64Coder;

import cn.hutool.json.JSONObject;

import org.apache.http.Header;
import org.apache.http.HttpEntity;
import org.apache.http.NameValuePair;
import org.apache.http.client.ResponseHandler;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.CloseableHttpResponse;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.methods.HttpPut;
import org.apache.http.client.methods.HttpUriRequest;
import org.apache.http.client.protocol.HttpClientContext;
import org.apache.http.conn.ClientConnectionManager;
import org.apache.http.conn.scheme.Scheme;
import org.apache.http.conn.scheme.SchemeRegistry;
import org.apache.http.conn.scheme.SchemeSocketFactory;
import org.apache.http.conn.ssl.AllowAllHostnameVerifier;
import org.apache.http.conn.ssl.SSLSocketFactory;
import org.apache.http.conn.ssl.X509HostnameVerifier;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.BasicResponseHandler;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.message.BasicHeader;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.protocol.HttpContext;
import org.apache.http.util.EntityUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import sc.app.stc.encrypt.EncryptionUtil;
import sc.app.stc.po.FinQMessageReq;
import sc.app.stc.po.dist.QMSendParams;

import java.io.IOException;
import java.nio.charset.Charset;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.ArrayList;

import javax.net.ssl.SSLContext;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

public class SSLClient extends DefaultHttpClient {
    private static Logger logger = LoggerFactory.getLogger(SSLClient.class);
    private static final String APPLICATION_JSON = "application/json";
    private static final String CONTENT_TYPE_TEXT_JSON = "text/json";

    public SSLClient() throws Exception {
        SSLContext ctx = SSLContext.getInstance("SSL");
        X509TrustManager tm =
                new X509TrustManager() {

                    @Override
                    public void checkClientTrusted(X509Certificate[] chain, String authType)
                            throws CertificateException {}

                    @Override
                    public void checkServerTrusted(X509Certificate[] chain, String authType)
                            throws CertificateException {}

                    @Override
                    public X509Certificate[] getAcceptedIssuers() {
                        return null;
                    }
                };
        ctx.init(null, new TrustManager[] {tm}, null);
        SSLSocketFactory ssf = new SSLSocketFactory(ctx);
        ssf.setHostnameVerifier((X509HostnameVerifier) new AllowAllHostnameVerifier());
        ClientConnectionManager ccm = this.getConnectionManager();
        SchemeRegistry sr = ccm.getSchemeRegistry();
        sr.register(new Scheme("https", 443, (SchemeSocketFactory) ssf));
    }

    public static String doPost(String url, NameValuePair[] datas) throws Exception {
        SSLClient httpclient = new SSLClient();
        httpclient.getParams().setParameter("http.connection.timeout", (Object) 10000);
        httpclient.getParams().setParameter("http.socket.timeout", (Object) 10000);
        HttpPost httpget = new HttpPost(url);
        ArrayList<NameValuePair> lt = new ArrayList<NameValuePair>();
        for (NameValuePair nameValuePair : datas) {
            lt.add(nameValuePair);
        }
        httpget.setEntity((HttpEntity) new UrlEncodedFormEntity(lt, "UTF-8"));
        BasicResponseHandler responseHandler = new BasicResponseHandler();
        String txtJson =
                (String)
                        httpclient.execute(
                                (HttpUriRequest) httpget, (ResponseHandler) responseHandler);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPost(String url, String json, String... sid) throws Exception {
        SSLClient httpclient = new SSLClient();
        HttpPost httpget = new HttpPost(url);
        if (!TextUtil.isNull(json)) {
            httpget.addHeader("Content-Type", APPLICATION_JSON);
            StringEntity se = new StringEntity(json, Charset.forName("UTF-8"));
            se.setContentType(CONTENT_TYPE_TEXT_JSON);
            se.setContentEncoding((Header) new BasicHeader("Content-Type", APPLICATION_JSON));
            httpget.setEntity((HttpEntity) se);
        }
        if (sid != null && sid.length > 0) {
            httpget.addHeader((Header) new BasicHeader("Cookie", "JSESSIONID=" + sid[0]));
        }
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doGet(String url, String json, String... sid) throws Exception {
        SSLClient httpclient = new SSLClient();
        logger.info("doGet:url={},content={}", (Object) url, (Object) json);
        HttpGet httpget = new HttpGet(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(json);
        se.setContentType(CONTENT_TYPE_TEXT_JSON);
        se.setContentEncoding((Header) new BasicHeader("Content-Type", APPLICATION_JSON));
        if (sid != null && sid.length > 0) {
            httpget.addHeader((Header) new BasicHeader("Cookie", "JSESSIONID=" + sid[0]));
        }
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        String printMsg = txtJson;
        if (txtJson.length() >= 2000) {
            printMsg = txtJson.substring(0, 2000) + "... too long";
        }
        logger.info("doGet:respStr={}", (Object) printMsg);
        return txtJson;
    }

    public static String doPutSyncCrm(String url, String content) throws Exception {
        logger.info("syncCRMToIAM:url={},content={}", (Object) url, (Object) content);
        SSLClient httpclient = new SSLClient();
        HttpPut httpput = new HttpPut(url);
        httpput.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity stringEntity = new StringEntity(content, "UTF-8");
        httpput.setEntity((HttpEntity) stringEntity);
        BasicResponseHandler responseHandler = new BasicResponseHandler();
        String txtJson =
                (String)
                        httpclient.execute(
                                (HttpUriRequest) httpput, (ResponseHandler) responseHandler);
        httpput.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        logger.info("syncCRMToIAM:respStr={}", (Object) txtJson);
        return txtJson;
    }

    public static String doPostLoginIAM(
            String url, String clientID, String secret, String userName, String password)
            throws Exception {
        logger.info(
                "loginIAM:url={},clientID={},secret={},userName={},password={}",
                new Object[] {url, clientID, secret, userName, password});
        SSLClient httpclient = new SSLClient();
        HttpPost httppost = new HttpPost(url);
        String key = clientID + ":" + secret;
        String base64 = Base64Coder.encodeString(key);
        String headerAuthorization = "Basic " + base64;
        logger.debug("loginIAM:headerAuthorization={}", (Object) headerAuthorization);
        httppost.addHeader("Authorization", headerAuthorization);
        password = EncryptionUtil.md5(password);
        logger.debug("loginIAM:passwordMd5={}", (Object) password);
        NameValuePair[] datas =
                new NameValuePair[] {
                    new BasicNameValuePair("username", userName),
                    new BasicNameValuePair("password", password)
                };
        ArrayList<NameValuePair> lt = new ArrayList<NameValuePair>();
        for (NameValuePair nameValuePair : datas) {
            lt.add(nameValuePair);
        }
        httppost.setEntity((HttpEntity) new UrlEncodedFormEntity(lt, "UTF-8"));
        BasicResponseHandler responseHandler = new BasicResponseHandler();
        String txtJson =
                (String)
                        httpclient.execute(
                                (HttpUriRequest) httppost, (ResponseHandler) responseHandler);
        logger.info("loginIAM:respStr={}", (Object) txtJson);
        httppost.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doGetUserAuthzIAM(String url, String userId) throws Exception {
        logger.info("UserAuthzIAM:url={},userId={}", (Object) url, (Object) userId);
        SSLClient httpclient = new SSLClient();
        String finaurl = url + userId;
        HttpGet httpget = new HttpGet(finaurl);
        httpget.addHeader("Accept", APPLICATION_JSON);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("UserAuthzIAM:respStr={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doGetidctabpremission(String url, String userId, String function)
            throws Exception {
        logger.info("idctabpremission={},userId={}", (Object) url, (Object) userId);
        SSLClient httpclient = new SSLClient();
        String finaurl = url + userId + "/perm?functionCode=" + function;
        HttpGet httpget = new HttpGet(finaurl);
        httpget.addHeader("Accept", APPLICATION_JSON);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("idctabpremission:respStr={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doGetInstAuth(String url, String name, String pwd) throws Exception {
        logger.info("InstAuth={}", (Object) url);
        SSLClient httpclient = new SSLClient();
        HttpGet httpget = new HttpGet(url);
        String key = name + ":" + pwd;
        String base64 = Base64Coder.encodeString(key);
        String headerAuthorization = "Basic " + base64;
        httpget.addHeader("Authorization", headerAuthorization);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("doGetInstAuth={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostEndData(
            String url, String username, String Password, String Conditions) throws Exception {
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) 1);
        param2.put("pagesize", (Object) 100);
        param2.put("DataSourceId", (Object) 118);
        param2.put("ApiName", (Object) "bond_eod_statistics_broker");
        param2.put("Conditions", (Object) Conditions);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("doPostEndData={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostDoubleBestQuotation(
            String url,
            String username,
            String Password,
            String condition,
            int startpage,
            int pagesize)
            throws Exception {
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) startpage);
        param2.put("pagesize", (Object) pagesize);
        param2.put("DataSourceId", (Object) 106);
        param2.put("ApiName", (Object) "bond_market_stream_webbond_fmt");
        param2.put("Conditions", (Object) condition);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostDoubleBestQuotation param2: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("doPostDoubleBestQuotation={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostTransactionOnday(
            String url,
            String username,
            String Password,
            String condition,
            int startpage,
            int pagesize,
            int dealDataSourceId)
            throws Exception {
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) startpage);
        param2.put("pagesize", (Object) pagesize);
        param2.put("DataSourceId", (Object) dealDataSourceId);
        param2.put("ApiName", (Object) "qb_bond_deal");
        param2.put("Conditions", (Object) condition);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostTransactionOnday param2: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("doPostTransactionOnday={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostBrokerIdSort(
            String url,
            String username,
            String Password,
            String condition,
            int startpage,
            int pagesize)
            throws Exception {
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) startpage);
        param2.put("pagesize", (Object) pagesize);
        param2.put("DataSourceId", (Object) 107);
        param2.put("ApiName", (Object) "idb_user_preference");
        ArrayList<String> col = new ArrayList<String>();
        col.add("ID");
        col.add("PREF_VALUE");
        col.add("SAVE_TYPE");
        param2.put("columns", col);
        param2.put("Conditions", (Object) condition);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostBrokerIdSort request: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("doPostBrokerIdSort result={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostHoliday(
            String url,
            String username,
            String Password,
            String condition,
            int startpage,
            int pagesize)
            throws Exception {
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) startpage);
        param2.put("pagesize", (Object) pagesize);
        param2.put("DataSourceId", (Object) 100);
        param2.put("ApiName", (Object) "holiday_info");
        param2.put("Conditions", (Object) condition);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostHoliday request: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        logger.info("doPostHoliday result={}", (Object) txtJson);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostBond(
            String url,
            String username,
            String Password,
            int startpage,
            int pagesize,
            String condition)
            throws Exception {
        String txtJson;
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) startpage);
        param2.put("pagesize", (Object) pagesize);
        param2.put("DataSourceId", (Object) 100);
        param2.put("ApiName", (Object) "BOND");
        param2.put("Conditions", (Object) condition);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostBond request: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String printMsg =
                txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        if (txtJson.length() >= 2000) {
            printMsg = printMsg.substring(0, 2000) + "... too long";
        }
        logger.info("doPostBond result={}", (Object) printMsg);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostBondlist(
            String url,
            String username,
            String Password,
            int startpage,
            int pagesize,
            String condition)
            throws Exception {
        String txtJson;
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) startpage);
        param2.put("pagesize", (Object) pagesize);
        param2.put("DataSourceId", (Object) 100);
        param2.put("ApiName", (Object) "BOND_LIST_INFO");
        param2.put("Conditions", (Object) condition);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostBondlist request: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String printMsg =
                txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        if (txtJson.length() >= 2000) {
            printMsg = printMsg.substring(0, 2000) + "... too long";
        }
        logger.info("doPostBondlist result={}", (Object) printMsg);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostinstitution(
            String url,
            String username,
            String Password,
            int startpage,
            int pagesize,
            String condition)
            throws Exception {
        String txtJson;
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("User", (Object) username);
        param2.put("Password", (Object) Password);
        param2.put("ApiVersion", (Object) "N");
        param2.put("startpage", (Object) startpage);
        param2.put("pagesize", (Object) pagesize);
        param2.put("DataSourceId", (Object) 100);
        param2.put("ApiName", (Object) "institution_info");
        param2.put("Conditions", (Object) condition);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostinstitution request: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String printMsg =
                txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        if (txtJson.length() >= 2000) {
            printMsg = printMsg.substring(0, 2000) + "... too long";
        }
        logger.info("doPostinstitution result={}", (Object) printMsg);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    public static String doPostQMToken(String url, String app_id, String secret) throws Exception {
        String txtJson;
        logger.info("doPostQMToken  url={}", (Object) url);
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("app_id", (Object) app_id);
        param2.put("secret", (Object) secret);
        HttpPost httpget = new HttpPost(url);
        httpget.addHeader("Content-Type", APPLICATION_JSON);
        StringEntity se = new StringEntity(param2.toString());
        logger.info("doPostQMToken request: {}", (Object) param2);
        se.setContentEncoding("UTF-8");
        se.setContentType(APPLICATION_JSON);
        httpget.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
        HttpEntity entity = response.getEntity();
        String printMsg =
                txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        if (txtJson.length() >= 2000) {
            printMsg = txtJson.substring(0, 2000) + "... too long";
        }
        logger.info("doPostQMToken result={}", (Object) printMsg);
        httpget.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doPostQMToken2(String url, String app_id, String secret) {
        CloseableHttpResponse response = null;
        String txtJson = null;
        try {
            logger.info("doPostQMToken2  url={}", (Object) url);
            CloseableHttpClient httpclient = HttpClientUtil.getHttpClient();
            JSONObject param2 = new JSONObject();
            param2.put("app_id", (Object) app_id);
            param2.put("secret", (Object) secret);
            HttpPost httpget = new HttpPost(url);
            httpget.addHeader("Content-Type", APPLICATION_JSON);
            StringEntity se = new StringEntity(param2.toString());
            logger.info("doPostQMToken2 request: {}", (Object) param2);
            long start = System.currentTimeMillis();
            se.setContentEncoding("UTF-8");
            se.setContentType(APPLICATION_JSON);
            httpget.setEntity((HttpEntity) se);
            response = httpclient.execute((HttpUriRequest) httpget);
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            EntityUtils.consume((HttpEntity) entity);
            String printMsg = txtJson;
            if (txtJson.length() >= 2000) {
                printMsg = txtJson.substring(0, 2000) + "... too long";
            }
            logger.info("doPostQMToken2 result={}", (Object) printMsg);
            long end = System.currentTimeMillis();
            logger.info("doPostQMToken2 total: {}", (Object) (end - start));
        } catch (Exception e) {
            logger.error("doPostQMToken2 error", (Throwable) e);
        } finally {
            try {
                if (response != null) {
                    response.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return txtJson;
    }

    public static String doPostQMSendText(String url, String tboken, QMSendParams params)
            throws Exception {
        String txtJson;
        logger.info("doPostQMSendText  url={}", (Object) url);
        SSLClient httpclient = new SSLClient();
        JSONObject param2 = new JSONObject();
        param2.put("no_wait", (Object) params.no_wait);
        param2.put("qm_ids", params.qm_ids);
        param2.put("create_room", (Object) params.create_room);
        param2.put("data", (Object) params.data);
        param2.put("body", (Object) params.body);
        HttpPost httppost = new HttpPost(url);
        httppost.addHeader("Content-Type", APPLICATION_JSON);
        String headerAuthorization = "Bearer " + tboken;
        logger.debug("doPostQMSendText:headerAuthorization={}", (Object) headerAuthorization);
        httppost.addHeader("Authorization", headerAuthorization);
        StringEntity se = new StringEntity(param2.toString(), "UTF-8");
        logger.info("doPostQMSendText request: {}", (Object) param2);
        long start = System.currentTimeMillis();
        se.setContentType(APPLICATION_JSON);
        httppost.setEntity((HttpEntity) se);
        CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httppost);
        HttpEntity entity = response.getEntity();
        String printMsg =
                txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
        if (txtJson.length() >= 2000) {
            printMsg = txtJson.substring(0, 2000) + "... too long";
        }
        logger.info("doPostQMSendText result={}", (Object) printMsg);
        long end = System.currentTimeMillis();
        logger.info("doPostQMSendText total: {}", (Object) (end - start));
        httppost.releaseConnection();
        httpclient.getConnectionManager().shutdown();
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doPostQMSendText2(String url, String tboken, QMSendParams params) {
        CloseableHttpResponse response = null;
        String txtJson = null;
        try {
            logger.info("doPostQMSendText2  url={}", (Object) url);
            CloseableHttpClient httpclient = HttpClientUtil.getHttpClient();
            JSONObject param2 = new JSONObject();
            param2.put("no_wait", (Object) params.no_wait);
            param2.put("qm_ids", params.qm_ids);
            param2.put("create_room", (Object) params.create_room);
            param2.put("data", (Object) params.data);
            param2.put("body", (Object) params.body);
            HttpPost httppost = new HttpPost(url);
            httppost.addHeader("Content-Type", APPLICATION_JSON);
            String headerAuthorization = "Bearer " + tboken;
            logger.debug("doPostQMSendText2:headerAuthorization={}", (Object) headerAuthorization);
            httppost.addHeader("Authorization", headerAuthorization);
            StringEntity se = new StringEntity(param2.toString(), "UTF-8");
            logger.info("doPostQMSendText2 request: {}", (Object) param2);
            long start = System.currentTimeMillis();
            se.setContentType(APPLICATION_JSON);
            httppost.setEntity((HttpEntity) se);
            response =
                    httpclient.execute(
                            (HttpUriRequest) httppost, (HttpContext) HttpClientContext.create());
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            EntityUtils.consume((HttpEntity) entity);
            String printMsg = txtJson;
            if (txtJson.length() >= 2000) {
                printMsg = txtJson.substring(0, 2000) + "... too long";
            }
            logger.info("doPostQMSendText2 result={}", (Object) printMsg);
            long end = System.currentTimeMillis();
            logger.info("doPostQMSendText2 total: {}", (Object) (end - start));
        } catch (Exception e) {
            logger.error("doPostQMSendText2 error", (Throwable) e);
        } finally {
            try {
                if (response != null) {
                    response.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doPostFinqSendText(
            String url, String tboken, FinQMessageReq req, String content) {
        CloseableHttpResponse response = null;
        String txtJson = null;
        try {
            logger.info("doPostFinqSendText  url={}", (Object) url);
            CloseableHttpClient httpclient = HttpClientUtil.getHttpClient();
            JSONObject param = new JSONObject();
            logger.info("body\uff1a", (Object) tboken);
            HttpPost httppost = new HttpPost(url);
            httppost.addHeader("Content-Type", APPLICATION_JSON);
            String headerAuthorization = tboken;
            logger.info("doPostFinqSendText:headerAuthorization={}", (Object) headerAuthorization);
            httppost.addHeader("Authorization", headerAuthorization);
            StringEntity se = new StringEntity(content, "UTF-8");
            logger.info("doPostFinqSendText request: {}", (Object) req.getContent());
            long start = System.currentTimeMillis();
            se.setContentType(APPLICATION_JSON);
            httppost.setEntity((HttpEntity) se);
            response =
                    httpclient.execute(
                            (HttpUriRequest) httppost, (HttpContext) HttpClientContext.create());
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            EntityUtils.consume((HttpEntity) entity);
            String printMsg = txtJson;
            if (txtJson.length() >= 2000) {
                printMsg = txtJson.substring(0, 2000) + "... too long";
            }
            logger.info("doPostFinqSendText result={}", (Object) printMsg);
            long end = System.currentTimeMillis();
            logger.info("doPostFinqSendText total: {}", (Object) (end - start));
        } catch (Exception e) {
            logger.error("doPostFinqSendText error", (Throwable) e);
        } finally {
            try {
                if (response != null) {
                    response.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doPostFinqSendText1(
            String url, String tboken, FinQMessageReq req, String content) {
        CloseableHttpResponse response = null;
        String txtJson = null;
        try {
            logger.info("doPostFinqSendText  url={}", (Object) url);
            CloseableHttpClient httpclient = HttpClientUtil.getHttpClient();
            JSONObject param = new JSONObject();
            logger.info("body\uff1a", (Object) tboken);
            HttpPost httppost = new HttpPost(url);
            httppost.addHeader("Content-Type", APPLICATION_JSON);
            String headerAuthorization = "Bearer " + tboken;
            logger.info("doPostFinqSendText:headerAuthorization={}", (Object) headerAuthorization);
            httppost.addHeader("Authorization", headerAuthorization);
            StringEntity se = new StringEntity(content, "UTF-8");
            logger.info("doPostFinqSendText request: {}", (Object) req.getContent());
            long start = System.currentTimeMillis();
            se.setContentType(APPLICATION_JSON);
            httppost.setEntity((HttpEntity) se);
            response =
                    httpclient.execute(
                            (HttpUriRequest) httppost, (HttpContext) HttpClientContext.create());
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            EntityUtils.consume((HttpEntity) entity);
            String printMsg = txtJson;
            if (txtJson.length() >= 2000) {
                printMsg = txtJson.substring(0, 2000) + "... too long";
            }
            logger.info("doPostFinqSendText result={}", (Object) printMsg);
            long end = System.currentTimeMillis();
            logger.info("doPostFinqSendText total: {}", (Object) (end - start));
        } catch (Exception e) {
            logger.error("doPostFinqSendText error", (Throwable) e);
        } finally {
            try {
                if (response != null) {
                    response.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doGetAccountBroker(String url, String aseString) throws Exception {
        logger.info("doGetAccountBroker  url={},aseString={}", (Object) url, (Object) aseString);
        CloseableHttpResponse response = null;
        String txtJson = "";
        try {
            SSLClient httpclient = new SSLClient();
            HttpGet httpget = new HttpGet(url);
            httpget.addHeader("Content-Type", APPLICATION_JSON);
            httpget.addHeader("Authorization", aseString);
            response = httpclient.execute((HttpUriRequest) httpget);
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            logger.info("doGetAccountBroker:respStr={}", (Object) txtJson);
            httpget.releaseConnection();
            httpclient.getConnectionManager().shutdown();
            String printMsg = txtJson;
            if (txtJson.length() >= 2000) {
                printMsg = txtJson.substring(0, 2000) + "... too long";
            }
            logger.info("doGetAccountBroker:respStr={}", (Object) printMsg);
        } catch (Exception e) {
            logger.error("doGetAccountBroker error", (Throwable) e);
        } finally {
            try {
                if (response != null) {
                    response.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doGetUserauthz(String url, String userId) throws Exception {
        logger.info("doGetUserauthz  url={},userId={}", (Object) url, (Object) userId);
        CloseableHttpResponse response = null;
        String txtJson = "";
        try {
            String finalurl = url + "?userId=" + userId;
            logger.info("doGetUserauthz  url={}", (Object) finalurl);
            SSLClient httpclient = new SSLClient();
            HttpGet httpget = new HttpGet(finalurl);
            httpget.addHeader("Content-Type", APPLICATION_JSON);
            response = httpclient.execute((HttpUriRequest) httpget);
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            logger.info("doGetUserauthz:respStr={}", (Object) txtJson);
            httpget.releaseConnection();
            httpclient.getConnectionManager().shutdown();
            String printMsg = txtJson;
            if (txtJson.length() >= 2000) {
                printMsg = txtJson.substring(0, 2000) + "... too long";
            }
            logger.info("doGetUserauthz:respStr={}", (Object) printMsg);
        } catch (Exception e) {
            logger.error("doGetUserauthz error", (Throwable) e);
        } finally {
            try {
                if (response != null) {
                    response.close();
                }
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doGetFqOption(String url, String userId) throws Exception {
        logger.info("doGetFqOption={},userId={}", (Object) url, (Object) userId);
        String txtJson = "";
        SSLClient httpclient = new SSLClient();
        HttpGet httpget = new HttpGet(url);
        try {
            httpget.addHeader("Accept", APPLICATION_JSON);
            httpget.addHeader("userId", userId);
            CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            logger.info("doGetFqOption:respStr={}", (Object) txtJson);
            httpget.releaseConnection();
            httpclient.getConnectionManager().shutdown();
        } catch (Exception e) {
            logger.error("doGetFqOption error", (Throwable) e);
        } finally {
            try {
                if (httpget != null) {
                    httpget.releaseConnection();
                }
                if (httpclient != null) {
                    httpclient.getConnectionManager().shutdown();
                }
            } catch (Exception e) {
                e.printStackTrace();
                logger.error("doGetFqOption finally error", (Throwable) e);
            }
        }
        return txtJson;
    }

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static String doGetFqBondList(String url, String userId) throws Exception {
        logger.info("doGetFqBondList={},userId={}", (Object) url, (Object) userId);
        String txtJson = "";
        SSLClient httpclient = new SSLClient();
        HttpGet httpget = new HttpGet(url);
        try {
            httpget.addHeader("Accept", APPLICATION_JSON);
            httpget.addHeader("userId", userId);
            CloseableHttpResponse response = httpclient.execute((HttpUriRequest) httpget);
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString((HttpEntity) entity, (String) "UTF-8").trim();
            logger.info("doGetFqBondList:respStr={}", (Object) txtJson);
            httpget.releaseConnection();
            httpclient.getConnectionManager().shutdown();
        } catch (Exception e) {
            logger.error("doGetFqBondList error", (Throwable) e);
        } finally {
            try {
                if (httpget != null) {
                    httpget.releaseConnection();
                }
                if (httpclient != null) {
                    httpclient.getConnectionManager().shutdown();
                }
            } catch (Exception e) {
                e.printStackTrace();
                logger.error("doGetFqBondList finally error", (Throwable) e);
            }
        }
        return txtJson;
    }
}
