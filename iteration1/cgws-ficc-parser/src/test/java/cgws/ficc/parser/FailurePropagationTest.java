package cgws.ficc.parser;

import cgws.ficc.parser.compat.JavaParserFacade;
import cgws.ficc.parser.dict.BondDictionary;
import cgws.ficc.parser.model.Diagnostic;
import cgws.ficc.parser.model.ParseResponse;
import cgws.ficc.parser.parse.ParserCore;
import org.junit.jupiter.api.Test;

import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertThrows;
import static org.junit.jupiter.api.Assertions.assertTrue;

/** A missing dictionary or an unimplemented entry point must fail, never look like a miss. */
class FailurePropagationTest {

    @Test
    void parseWithoutDictionaryFails() {
        ParseResponse response = ParserCore.parse("2400001 2.35 1000", null, "20260929", true);
        assertTrue(response.failed, "no dictionary is a failure, not an empty result");
        assertTrue(response.quotes.isEmpty());
        assertTrue(response.diagnosticsAsString().contains("DICTIONARY_EMPTY"));

        BondDictionary empty = new BondDictionary.Builder().build("empty");
        ParseResponse withEmpty = ParserCore.parse("2400001 2.35 1000", empty, "20260929", true);
        assertTrue(withEmpty.failed);
        assertTrue(withEmpty.quotes.isEmpty());
    }

    @Test
    void parseEmptyTextFails() {
        BondDictionary.Builder builder = new BondDictionary.Builder();
        builder.put("2400001.ib", "24国债03", null, false);
        BondDictionary dictionary = builder.build("one-code");
        ParseResponse response = ParserCore.parse("   ", dictionary, "20260929", true);
        assertTrue(response.failed);
        assertTrue(response.diagnosticsAsString().contains("INPUT_EMPTY"),
                "diagnostics=" + response.diagnosticsAsString());
    }

    @Test
    void facadeWithoutDictionaryFails() {
        JavaParserFacade facade = JavaParserFacade.newInstance();
        assertFalse(facade.isReady());
        ParseResponse response = facade.Parse("2400001.ib 2.35 1000", "20260929");
        assertTrue(response.failed, "an unready parser must report failure");
        assertTrue(response.diagnosticsAsString().contains("PARSER_NOT_READY"));
        assertThrows(IllegalStateException.class,
                () -> facade.ParseBond("2400001.ib 2.35 1000", "", ""),
                "ParseBond must not report a missing dictionary as 'not recognised'");
    }

    @Test
    void helperNullInputFailsExplicitly() {
        JavaParserFacade facade = JavaParserFacade.newInstance();
        assertThrows(IllegalArgumentException.class, () -> facade.ParsePrice(null));
        assertThrows(IllegalArgumentException.class, () -> facade.ParseTenor(null));
        assertThrows(IllegalArgumentException.class, () -> facade.ParseAmount(null));
    }
}
