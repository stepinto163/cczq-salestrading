package sc.app.stc.parser.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;
import com.adaptiveMQ2.message.Message;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sc.app.stc.parser.enums.ResultMessage;
import sc.app.stc.parser.manager.VirtualParserManager;
import sc.app.stc.parser.po.ParsePo;
import sc.app.stc.parser.po.VirtualPo;
import sc.app.stc.parser.utils.ResultUtils;
import sc.app.stc.utils.Consts;

import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class VirtualParserHandler extends ContentHandler {

    private Logger logger = LoggerFactory.getLogger(VirtualParserHandler.class);

    @Override
    public Map<String, Object> handle(String topic, Message message, String content, Map<String, Object> map, boolean fromList) {
        logger.info("receive virtual Parser topic: {} content : {}", topic, content);
        Map<String, Object> result = new HashMap<String, Object>();
        try {
            if (StringUtils.isEmpty(content)) {
                return ResultUtils.getMapResult(ResultMessage.REG_CONTENT_EMPTY);
            }
            ParsePo req = MsgHelper.Deserialize(content, ParsePo.class, CharsetConvertType.None);
            List<VirtualPo> resultList = VirtualParserManager.parseVirtual(req.contentList);
            result.put(Consts.DATAS, resultList);
            result.put(Consts.Msg, Consts.SuccessMsg);
            result.put(Consts.Code, Consts.SuccessCode);
            logger.info("convert virtual result:{}", MsgHelper.Serializer(result, CharsetConvertType.None));
        } catch (Exception e) {
            logger.error("VirtualParserHandler error", e);
            result.put(Consts.Msg, e.getMessage());
            result.put(Consts.Code, Consts.NoKnowCode);
        }
        return result;
    }
}
