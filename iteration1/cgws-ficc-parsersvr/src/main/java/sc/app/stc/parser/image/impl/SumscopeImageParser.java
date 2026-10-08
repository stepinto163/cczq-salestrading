package sc.app.stc.parser.image.impl;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import cn.hutool.core.util.StrUtil;
import cn.hutool.http.HttpResponse;
import cn.hutool.http.HttpUtil;
import com.alibaba.fastjson.JSONArray;
import com.alibaba.fastjson.JSONObject;
import org.slf4j.Logger;
import sc.app.stc.parser.image.ImageParser;
import sc.app.stc.parser.utils.Consts;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.List;

/**
 * 森浦图片识别接口
 *
 * @author enhao.dong
 */
public class SumscopeImageParser implements ImageParser {
    private final Logger logger = AsyncSlf4jLoggerFactory.getLogger(this.getClass());

    @Override
    public String parse(String base64String) {
        List<String> codes = new ArrayList<>();
        JSONObject req = new JSONObject();
        JSONObject params = new JSONObject();
        String[] targetTag = {"bond"};
        req.put("appId", Consts.ImageParserApiAppId);
        req.put("appSecret", Consts.ImageParserApiSecret);
        req.put("service", "TokenizerServer");
        req.put("method", "call");
        params.put("service", "entity");
        params.put("picstr", base64String);
        params.put("target_tag", targetTag);
        req.put("params", params);
        try (HttpResponse httpResponse = HttpUtil.createPost(Consts.ImageParserApi)
                .header("Content-Type", "application/json")
                .body(req.toJSONString())
                .execute()) {
            logger.info("invoke sumscope image parser status:{}", httpResponse.getStatus());
            if (httpResponse.isOk()) {
                String response = httpResponse.body();
                logger.info("invoke sumscope image parser result: {}", response);
                if (StrUtil.isNotBlank(response)) {
                    JSONObject res = JSONObject.parseObject(response);
                    if ("0".equals(res.getString("errorCode")) && res.getJSONObject("result") != null && res.getJSONObject("result").getString("result").matches("^\\[.*]$")) //正则判断是JSONArray
                    {
                        JSONArray result = res.getJSONObject("result").getJSONArray("result");
                        if (result != null && result.size() > 0) {
                            for (int i = 0; i < result.size(); i++) {
                                JSONObject resultObj = result.getJSONObject(i);
                                JSONArray bonds = resultObj.getJSONArray("bond");
                                for (int j = 0; j < bonds.size(); j++) {
                                    JSONObject bond = bonds.getJSONObject(j);
                                    String code = bond.getString("code");
                                    codes.add(code);
                                }
                            }
                        }
                    }
                }
            }
        } catch (Exception e) {
            logger.error("invoke sumscope image parser api error: ", e);
        }
        return codes.size() > 0 ? String.join("\n", codes) : "";
    }

    public static void main(String[] args) throws IOException {
        SumscopeImageParser imageParser = new SumscopeImageParser();
        String image = new String(Files.readAllBytes(Paths.get("D:\\Projects\\stc-lite\\stc-tbook\\sc-app-stc-parsersvr\\src\\main\\java\\sc\\app\\stc\\parser\\image\\impl\\base64.txt")));
        ;
        imageParser.parse(image);
    }
}
