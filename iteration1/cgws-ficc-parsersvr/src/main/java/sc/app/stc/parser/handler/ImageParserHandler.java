package sc.app.stc.parser.handler;

import ats.app.ym.ceda.server.ContentHandler;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;
import com.adaptiveMQ2.message.Message;
import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.model.BondQuote;
import org.apache.commons.lang3.StringUtils;
import org.springframework.util.CollectionUtils;
import sc.app.stc.parser.enums.ResultMessage;
import sc.app.stc.parser.image.ImageParser;
import sc.app.stc.parser.image.ImageParserFactory;
import sc.app.stc.parser.manager.ParserManager;
import sc.app.stc.parser.po.ParsePo;
import sc.app.stc.parser.po.TableIntentionQuote;
import sc.app.stc.parser.utils.*;

import java.util.HashMap;
import java.util.List;
import java.util.Map;


public class ImageParserHandler extends ContentHandler {
    @Override
    public Map<String, Object> handle(String topic, Message message, String content, Map<String, Object> map, boolean fromList) {
        Map<String, Object> result = new HashMap<String, Object>();
        logger.info("receive bond Parser topic: {}", topic);
        try {
            ParsePo req = MsgHelper.Deserialize(content, ParsePo.class, CharsetConvertType.None);
            String userName = req.getUserName();
            String parseContent = req.getContent();
            if (StringUtils.isEmpty(userName)) {
                return ResultUtils.getMapResult(ResultMessage.REQ_USER_EMPTY);
            }
            if (StringUtils.isEmpty(parseContent)) {
                return ResultUtils.getMapResult(ResultMessage.REG_CONTENT_EMPTY);
            }
            ImageParser parser = ImageParserFactory.getParser(Consts.ImageParserType);
            if (parser == null) {
                logger.warn("can not find image parser by: {}", Consts.ImageParserType);
                return ResultUtils.getMapResult(ResultMessage.REG_QUOTE_EMPTY);
            }
            String parserResult = parser.parse(parseContent);
            req.setContent(parserResult);
            AllQuote allQuote = BondQuoteHelper.getInstance().parsingAll(req.content);
            logger.info("allQuote result:{}", MsgHelper.Serializer(allQuote, CharsetConvertType.None));
            if(null == allQuote){
                return ResultUtils.getMapResult(ResultMessage.REG_QUOTE_EMPTY);
            }
            List<BondQuote> lstQuote = allQuote.lstQuote;
            if (CollectionUtils.isEmpty(lstQuote)) {
                return ResultUtils.getMapResult(ResultMessage.REG_CONTENT_EMPTY);
            }
            if (lstQuote.size() > Consts.IntentionQuoteSize) {
                return ResultUtils.getMapResult(ResultMessage.REG_QUOTE_LIMIT);
            }
            List<TableIntentionQuote> quoteList = ParserManager.convertIntentionQuote(allQuote, req);
            result.put(Consts.Code, Consts.SuccessCode);
            result.put(Consts.Msg, Consts.SuccessMsg);
            result.put(Consts.TotalSize, quoteList.size());
            result.put(Consts.DATAS,quoteList);
            logger.info("convert bond result:{}", MsgHelper.Serializer(quoteList, CharsetConvertType.None));
            return result;
        } catch (Exception e) {
            logger.error("ParserHandler error", e);
            return ResultUtils.getMapError(e.getMessage());
        }
    }

}
