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
import sc.app.stc.parser.manager.ParserManager;
import sc.app.stc.parser.po.ParsePo;
import sc.app.stc.parser.po.TableIntentionQuote;
import sc.app.stc.parser.utils.BondQuoteHelper;
import sc.app.stc.parser.utils.Consts;
import sc.app.stc.parser.utils.ResultUtils;

import java.util.HashMap;
import java.util.List;
import java.util.Map;


/**
 * stc.parser.bond 的 STC 协议入口：校验用户/文本，调用解析引擎，再转换为旧客户端所需报价结构。
 * 返回 Code/Msg/Datas 和可选 Warnings；它不是 FICC 的 Data/requestId 应答格式。
 * 此操作只识别和补全预览，正式入库/确认由客户端后续调用 QuoteSvr 完成。
 */
public class BondParserHandler extends ContentHandler {
    @Override
    public Map<String, Object> handle(String topic, Message message, String content, Map<String, Object> map, boolean fromList) {
        Map<String, Object> result = new HashMap<String, Object>();
        logger.info("receive bond Parser topic: {} content : {}", topic,content);
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

            String replaceBlank = parseContent.replaceAll("(?m)^\\s*$(\\n|\\r\\n)", "");
            req.setContent(replaceBlank);

            AllQuote allQuote = BondQuoteHelper.getInstance().parsingAll(req.content);
            logger.info("allQuote result:{}", MsgHelper.Serializer(allQuote, CharsetConvertType.None));
            if(null == allQuote){
                return ResultUtils.getMapResult(ResultMessage.REG_QUOTE_EMPTY);
            }
            List<BondQuote> lstQuote = allQuote.lstQuote;
            if (CollectionUtils.isEmpty(lstQuote)) {
                return ResultUtils.getMapResult(ResultMessage.REG_QUOTE_EMPTY);
            }
            if (lstQuote.size() > Consts.IntentionQuoteSize) {
                return ResultUtils.getMapResult(ResultMessage.REG_QUOTE_LIMIT);
            }
            List<TableIntentionQuote> quoteList = ParserManager.convertIntentionQuote(allQuote, req);
            result.put(Consts.Code, Consts.SuccessCode);
            result.put(Consts.Msg, Consts.SuccessMsg);
            result.put(Consts.TotalSize, quoteList.size());
            result.put(Consts.DATAS,quoteList);
            if (allQuote.warnings != null && !allQuote.warnings.isEmpty()) {
                result.put("Warnings", allQuote.warnings);
            }
            logger.info("convert bond result:{}", MsgHelper.Serializer(quoteList, CharsetConvertType.None));
            return result;
        } catch (Exception e) {
            logger.error("ParserHandler error", e);
            return ResultUtils.getMapError(e.getMessage());
        }
    }
}
