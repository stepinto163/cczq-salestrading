package cgws.ficc.parser;

import cgws.ficc.parser.compare.Json;
import cgws.ficc.parser.compat.JavaParserFacade;
import org.junit.jupiter.api.DynamicTest;
import org.junit.jupiter.api.TestFactory;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;

/** Immutable expected results recorded from the working DLL; tests never load native code or a DB. */
class RecordedNativeBehaviorTest {
    @TestFactory
    List<DynamicTest> recordedNativeCalls() throws Exception {
        Map<String, Object> fixture;
        try (InputStream input = getClass().getResourceAsStream("/native-fields-windows.json")) {
            assertNotNull(input);
            ByteArrayOutputStream bytes = new ByteArrayOutputStream();
            byte[] buffer = new byte[4096];
            int count;
            while ((count = input.read(buffer)) != -1) {
                bytes.write(buffer, 0, count);
            }
            fixture = Json.asObject(Json.parse(new String(bytes.toByteArray(), StandardCharsets.UTF_8)));
        }
        final JavaParserFacade parser = JavaParserFacade.newInstance();
        parser.SetCodeName("start", "start", "", "");
        for (Object value : Json.asArray(fixture.get("dictionary"))) {
            Map<String, Object> row = Json.asObject(value);
            parser.SetCodeName(Json.str(row, "code"), Json.str(row, "name"), "", "");
        }
        parser.SetCodeName("end", "end", "", "");
        List<DynamicTest> tests = new ArrayList<DynamicTest>();
        for (Object value : Json.asArray(fixture.get("cases"))) {
            Map<String, Object> row = Json.asObject(value);
            final String method = Json.str(row, "method");
            final String text = Json.str(row, "input");
            final String expected = Json.str(row, "expected");
            tests.add(DynamicTest.dynamicTest(method + " [" + text + "]", () -> {
                String actual;
                if ("Bond".equals(method)) {
                    assertEquals(Json.str(row, "preprocessed"), parser.PreProcess(text));
                    actual = parser.ParseBond(text, "", "");
                } else {
                    actual = (String) JavaParserFacade.class.getMethod(method, String.class).invoke(parser, text);
                }
                assertEquals(expected, actual);
            }));
        }
        return tests;
    }
}
