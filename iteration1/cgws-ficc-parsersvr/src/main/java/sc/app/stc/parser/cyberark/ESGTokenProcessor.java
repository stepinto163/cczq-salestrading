package sc.app.stc.parser.cyberark;

import cn.hutool.http.HttpRequest;
import cn.hutool.http.HttpResponse;
import cn.hutool.http.HttpUtil;
import com.app.common.cyberark.CyberarkUtil;
import com.app.common.utils.DBConsts;
import com.app.common.utils.JsonUtils;
import com.app.common.utils.StringUtil;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import sc.app.stc.parser.utils.Consts;

@Component
public class ESGTokenProcessor {
    private Logger logger = LoggerFactory.getLogger(ESGTokenProcessor.class);

    private static ESGTokenProcessor tokenProcessor = new ESGTokenProcessor();

    @Autowired
    private Consts Consts;
    private String token = "";
    private long expiresIn;

    private ESGTokenProcessor() {
    }

    public static ESGTokenProcessor getInstance() {
        return tokenProcessor;
    }

    public String getToken() {

        if (StringUtil.isEmpty(Consts.esgClientSecret)) {
            getPwd();
        }

        if (StringUtil.isEmpty(token) || System.currentTimeMillis() >= expiresIn) {
            refreshToken();
        }
        return token;
    }

    public void getPwd() {
        if (Consts.isCyberark == 1) {
            CyberarkUtil cyberarkUtil = new CyberarkUtil();
            cyberarkUtil.setSSLVerify(DBConsts.SSLVerify);

            String dbPwd = cyberarkUtil.getPasswordFromRemote(
                    Consts.remoteUrl, Consts.appId, Consts.safe,
                    Consts.folder, Consts.object, Consts.reason, Consts.appKey
            );

            Consts.esgClientSecret = StringUtil.isNotEmpty(dbPwd) ? dbPwd : "";
            logger.info("esgClientSecret length:{}", Consts.esgClientSecret.length());
        }
    }


    private void refreshToken() {

        try {
            String url = getOauthURL();
            logger.info("refreshToken url:{}", url);
            //String data = RestTemplateConfig.getInstance().post(url, "");
            HttpRequest post = HttpUtil.createPost(url);
            post.header("Content-Type", "application/json");
            post.header("Accept", "application/json");
            HttpResponse response = post.execute();
            logger.info("refreshToken response:{}", response);
            //TokenResponse tokenResponse = JsonUtils.Deserialize(data, TokenResponse.class);

            if (response != null && response.body() != null) {
                TokenResponse tokenResponse = JsonUtils.Deserialize(response.body(), TokenResponse.class);
                if (tokenResponse != null && tokenResponse.getData() != null) {
                    token = tokenResponse.getData().getAccess_token();
                    expiresIn = System.currentTimeMillis() + tokenResponse.getData().getExpires_in() * 1000;
                }
            }

        } catch (Exception e) {
            logger.error("refreshToken", e);
        }
    }

    public String OAUTH_PARAM_FORMAT = "grant_type=%s&client_id=%s&client_secret=%s";


    // OAuth认证URL获取方法
    public String getOauthURL() {
        // 1. 格式化基础参数
        String param = String.format(OAUTH_PARAM_FORMAT,
                Consts.esgGrantType,
                Consts.esgClientId,
                Consts.esgClientSecret
        );

        // 3. 返回完整URL
        return Consts.esgOauthURL + "?" + param;
    }

}


