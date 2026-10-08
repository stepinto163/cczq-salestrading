package sc.app.stc.parser.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;
import com.adaptiveMQ2.message.Message;
import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.util.EntityUtils;
import sc.app.stc.parser.cyberark.ESGTokenProcessor;
import sc.app.stc.parser.po.TradeInfo;
import sc.app.stc.parser.utils.Consts;
import sc.app.stc.parser.utils.ResultUtils;
import sc.app.stc.utils.IdUtil;
import sc.app.stc.utils.SSLClient;
import sc.app.stc.utils.TextUtil;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class DealParserHandler extends ContentHandler {
    @Override
    public Map<String, Object> handle(String topic, Message message, String content, Map<String, Object> map, boolean fromList) {
        Map<String, Object> result = new HashMap<String, Object>();
        logger.info("receive deal Parser topic: {} content : {}", topic,content);
        try {
            String responString = doPostai_trade_recog(Consts.DealParserApi,Consts.DealParserAuthorization,content);
            if(!TextUtil.isNull(responString)) {
                List<TradeInfo> dealList = MsgHelper.Deserialize2(responString, TradeInfo.class, CharsetConvertType.None);
                result.put(Consts.Code, Consts.SuccessCode);
                result.put(Consts.Msg, Consts.SuccessMsg);
                result.put(Consts.TotalSize, dealList.size());
                result.put(Consts.DATAS, dealList);
                logger.info("convert bond result:{}", MsgHelper.Serializer(dealList, CharsetConvertType.None));
            }else {
                result.put(Consts.Code, Consts.SuccessCode);
                result.put(Consts.Msg, Consts.SuccessMsg);
            }
            return result;
        }catch (Exception e){
            logger.error("ParserHandler error", e);
            return ResultUtils.getMapError(e.getMessage());
        }
    }

    public String doPostai_trade_recog(String url, String headerAuthorization, String content) {
        String txtJson = null;
        HttpPost httppost = null;
        SSLClient httpclient = null;
        try {
            logger.info("isCyberark:{}", Consts.isCyberark);
            if (Consts.isCyberark == 1) {
                String requestId = IdUtil.getId();
                String token = ESGTokenProcessor.getInstance().getToken();
                url = String.format("%s?esg_outer_access_token=%s&esg_outer_request_id=%s", url, token, requestId);
            }
            logger.info("doPostai_trade_recog  url={} headerAuthorization:{} content:{}", url,headerAuthorization,content);
            httpclient = new SSLClient();
            httppost = new HttpPost(url);
            httppost.addHeader("Content-Type", "application/json");
            httppost.addHeader("Authorization", headerAuthorization);
            StringEntity se = new StringEntity(content,"UTF-8");
            long start = System.currentTimeMillis();
            se.setContentType("application/json");
            httppost.setEntity(se);
            HttpResponse response = httpclient.execute(httppost);
            HttpEntity entity = response.getEntity();
            txtJson = EntityUtils.toString(entity, "UTF-8").trim();
            String printMsg = txtJson;
            if (txtJson.length() >= 2000) {
                printMsg = txtJson.substring(0, 2000) + "... too long";
            }
            logger.info("printMsg: {}", printMsg);
            long end = System.currentTimeMillis();
            logger.info("doPostai_trade_recog total: {}", end - start);
            httppost.releaseConnection();
            httpclient.getConnectionManager().shutdown();
        } catch (Exception e) {
            logger.error("doPostai_trade_recog error", e);
        } finally {
            if (httppost != null) {
                httppost.releaseConnection();
            }
            if(httpclient != null){
                httpclient.getConnectionManager().shutdown();
            }
        }
        return txtJson;
    }
}
