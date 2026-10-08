package sc.app.stc.parser.utils;

import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.parse.BondQuoteEngine;
import com.alibaba.fastjson.JSON;
import com.app.common.db.PartyDBPwdManager;
import com.sumscope.BondQuoteParser;
import java.time.LocalDate;

/** Legacy-only adapter. The Java engine never constructs this class or the closed parser. */
final class NativeBondQuoteEngine implements BondQuoteEngine {
    private final BondQuoteParser parser;

    NativeBondQuoteEngine() {
        parser = new BondQuoteParser(false, PartyDBPwdManager.getInstance().getPartyDBPwd());
    }

    @Override
    public AllQuote parse(String text, LocalDate date) {
        BondQuoteParser.AllQuote legacy = parser.parsingAll(text, date.toString(), true);
        if (legacy == null) {
            throw new IllegalStateException("Native parser returned null");
        }
        return JSON.parseObject(JSON.toJSONString(legacy), AllQuote.class);
    }

    @Override
    public void reload() {
        parser.loadData();
    }
}
