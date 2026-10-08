// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import org.apache.http.HeaderElement;
import org.apache.http.HttpEntity;
import org.apache.http.HttpEntityEnclosingRequest;
import org.apache.http.HttpRequest;
import org.apache.http.NoHttpResponseException;
import org.apache.http.client.HttpRequestRetryHandler;
import org.apache.http.client.config.RequestConfig;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.client.protocol.HttpClientContext;
import org.apache.http.config.Registry;
import org.apache.http.config.RegistryBuilder;
import org.apache.http.conn.ConnectTimeoutException;
import org.apache.http.conn.ConnectionKeepAliveStrategy;
import org.apache.http.conn.HttpClientConnectionManager;
import org.apache.http.conn.socket.PlainConnectionSocketFactory;
import org.apache.http.conn.ssl.SSLConnectionSocketFactory;
import org.apache.http.impl.client.CloseableHttpClient;
import org.apache.http.impl.client.HttpClients;
import org.apache.http.impl.conn.PoolingHttpClientConnectionManager;
import org.apache.http.message.BasicHeaderElementIterator;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.protocol.HttpContext;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.io.IOException;
import java.io.InterruptedIOException;
import java.io.UnsupportedEncodingException;
import java.net.UnknownHostException;
import java.security.KeyManagementException;
import java.security.NoSuchAlgorithmException;
import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import java.util.ArrayList;
import java.util.Map;
import java.util.Set;

import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLException;
import javax.net.ssl.SSLHandshakeException;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;

public class HttpClientUtil {
    private static Logger logger = LoggerFactory.getLogger(HttpClientUtil.class);
    private static final int TIMEOUT = 5000;
    private static final int MAX_TOTAL = 200;
    private static final int MAX_PER_ROUTE = 40;
    private static final int MAX_ROUTE = 100;
    private static final int MAX_RETRY_TIME = 5;
    private static CloseableHttpClient httpClient = null;
    private static PoolingHttpClientConnectionManager connectionManager;
    private static IdleConnectionMonitorThread monitorThread;
    private static final Object SYNC_LOCK;

    /*
     * WARNING - Removed try catching itself - possible behaviour change.
     */
    public static CloseableHttpClient getHttpClient()
            throws NoSuchAlgorithmException, KeyManagementException {
        if (httpClient == null) {
            Object object = SYNC_LOCK;
            synchronized (object) {
                if (httpClient == null) {
                    httpClient = HttpClientUtil.createHttpClient();
                    monitorThread = new IdleConnectionMonitorThread(connectionManager);
                    monitorThread.start();
                }
            }
        }
        return httpClient;
    }

    private static CloseableHttpClient createHttpClient()
            throws KeyManagementException, NoSuchAlgorithmException {
        PlainConnectionSocketFactory plainsf = PlainConnectionSocketFactory.getSocketFactory();
        SSLConnectionSocketFactory sslsf =
                new SSLConnectionSocketFactory(HttpClientUtil.createIgnoreVerifySSL());
        Registry registry =
                RegistryBuilder.create()
                        .register("http", (Object) plainsf)
                        .register("https", (Object) sslsf)
                        .build();
        RequestConfig requestConfig =
                RequestConfig.custom()
                        .setSocketTimeout(5000)
                        .setConnectTimeout(5000)
                        .setConnectionRequestTimeout(5000)
                        .build();
        connectionManager = new PoolingHttpClientConnectionManager(registry);
        connectionManager.setMaxTotal(200);
        connectionManager.setDefaultMaxPerRoute(40);
        HttpRequestRetryHandler httpRequestRetryHandler =
                (exception, executionCount, context) -> {
                    if (executionCount >= 5) {
                        logger.info("Exceeded max retry time, do not retry");
                        return false;
                    }
                    if (exception instanceof NoHttpResponseException) {
                        logger.info("NoHttpResponseException, retry");
                        return true;
                    }
                    if (exception instanceof SSLHandshakeException) {
                        logger.info("SSLHandshakeException, do not retry");
                        return false;
                    }
                    if (exception instanceof ConnectTimeoutException) {
                        logger.info("ConnectTimeoutException, retry");
                        return true;
                    }
                    if (exception instanceof InterruptedIOException) {
                        logger.info("InterruptedIOException, do not retry");
                        return false;
                    }
                    if (exception instanceof UnknownHostException) {
                        logger.info("UnknownHostException, do not retry");
                        return false;
                    }
                    if (exception instanceof SSLException) {
                        logger.info("SSLException, do not retry");
                        return false;
                    }
                    HttpClientContext clientContext =
                            HttpClientContext.adapt((HttpContext) context);
                    HttpRequest request = clientContext.getRequest();
                    return !(request instanceof HttpEntityEnclosingRequest);
                };
        ConnectionKeepAliveStrategy keepAliveStrategy =
                (response, context) -> {
                    BasicHeaderElementIterator it =
                            new BasicHeaderElementIterator(response.headerIterator("Keep-Alive"));
                    while (it.hasNext()) {
                        HeaderElement he = it.nextElement();
                        String param = he.getName();
                        String value = he.getValue();
                        if (value == null || !param.equalsIgnoreCase("timeout")) continue;
                        try {
                            logger.info("keep-alive timeout: {}", (Object) value);
                            return Long.parseLong(value) * 1000L;
                        } catch (NumberFormatException numberFormatException) {
                        }
                    }
                    return 60000L;
                };
        return HttpClients.custom()
                .setConnectionManager((HttpClientConnectionManager) connectionManager)
                .setDefaultRequestConfig(requestConfig)
                .setKeepAliveStrategy(keepAliveStrategy)
                .setRetryHandler(httpRequestRetryHandler)
                .build();
    }

    private static SSLContext createIgnoreVerifySSL()
            throws NoSuchAlgorithmException, KeyManagementException {
        SSLContext sslContext = SSLContext.getInstance("TLSv1.2");
        X509TrustManager trustManager =
                new X509TrustManager() {

                    @Override
                    public void checkClientTrusted(X509Certificate[] x509Certificates, String s)
                            throws CertificateException {}

                    @Override
                    public void checkServerTrusted(X509Certificate[] x509Certificates, String s)
                            throws CertificateException {}

                    @Override
                    public X509Certificate[] getAcceptedIssuers() {
                        return new X509Certificate[0];
                    }
                };
        sslContext.init(null, new TrustManager[] {trustManager}, null);
        return sslContext;
    }

    private static void setPostParams(HttpPost httpPost, Map<String, String> params) {
        ArrayList<BasicNameValuePair> nvps = new ArrayList<BasicNameValuePair>();
        Set<String> keys = params.keySet();
        for (String key : keys) {
            nvps.add(new BasicNameValuePair(key, params.get(key)));
        }
        try {
            httpPost.setEntity((HttpEntity) new UrlEncodedFormEntity(nvps, "utf-8"));
        } catch (UnsupportedEncodingException e) {
            e.printStackTrace();
        }
    }

    public static void closeConnectionPool() {
        try {
            httpClient.close();
            connectionManager.close();
            monitorThread.shutdown();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    static {
        SYNC_LOCK = new Object();
    }

    private static class IdleConnectionMonitorThread extends Thread {
        private final HttpClientConnectionManager connectionManager;
        private volatile boolean shutdown;

        public IdleConnectionMonitorThread(PoolingHttpClientConnectionManager connectionManager) {
            this.connectionManager = connectionManager;
        }

        /*
         * WARNING - Removed try catching itself - possible behaviour change.
         * Enabled aggressive block sorting
         * Enabled unnecessary exception pruning
         * Enabled aggressive exception aggregation
         */
        @Override
        public void run() {
            try {
                while (!this.shutdown) {
                    IdleConnectionMonitorThread idleConnectionMonitorThread = this;
                    synchronized (idleConnectionMonitorThread) {
                        this.wait(5000L);
                        this.connectionManager.closeExpiredConnections();
                    }
                }
                return;
            } catch (InterruptedException ex) {
                this.shutdown();
            }
        }

        /*
         * WARNING - Removed try catching itself - possible behaviour change.
         */
        public void shutdown() {
            this.shutdown = true;
            IdleConnectionMonitorThread idleConnectionMonitorThread = this;
            synchronized (idleConnectionMonitorThread) {
                this.notifyAll();
            }
        }
    }
}
