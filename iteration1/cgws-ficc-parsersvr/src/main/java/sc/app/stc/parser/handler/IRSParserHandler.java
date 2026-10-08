package sc.app.stc.parser.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;
import com.adaptiveMQ2.message.Message;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sc.app.stc.parser.manager.IRSParserManager;
import sc.app.stc.parser.po.ParsePo;
import sc.app.stc.utils.Consts;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class IRSParserHandler extends ContentHandler {

    private Logger logger = LoggerFactory.getLogger(IRSParserHandler.class);

    @Override
    public Map<String, Object> handle(String topic, Message message, String content, Map<String, Object> map, boolean fromList) {
        logger.info("receive irs Parser topic: {} content : {}", topic, content);
        Map<String, Object> result = new HashMap<String, Object>();
        try {
            ParsePo req = MsgHelper.Deserialize(content, ParsePo.class, CharsetConvertType.None);
            List<Map<String, Object>> resultList = IRSParserManager.parseIrs(req.contentList);
            result.put(Consts.DATAS, resultList);
            result.put(Consts.Msg, Consts.SuccessMsg);
            result.put(Consts.Code, Consts.SuccessCode);
            logger.info("convert irs result:{}", MsgHelper.Serializer(resultList, CharsetConvertType.None));
        } catch (Exception e) {
            logger.error("IRSParserHandler error", e);
            result.put(Consts.Msg, e.getMessage());
            result.put(Consts.Code, Consts.NoKnowCode);
        }
        return result;
    }
}
