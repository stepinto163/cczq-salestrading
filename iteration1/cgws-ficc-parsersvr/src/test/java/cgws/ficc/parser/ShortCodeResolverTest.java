package cgws.ficc.parser;

import cgws.ficc.parser.compare.Json;
import cgws.ficc.parser.identify.ShortCodeResolver;
import org.junit.jupiter.api.DynamicTest;
import org.junit.jupiter.api.TestFactory;

import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;

class ShortCodeResolverTest {
    @TestFactory
    List<DynamicTest> matchesLegacyJavaWrapper() throws Exception {
        ShortCodeResolver resolver = new ShortCodeResolver(new HashSet<String>(Arrays.asList(
                "260210.ib", "240210.ib", "240210.sh", "260301.sh", "260401.sz",
                "240001.ib", "240001.sh", "2400123.ib", "269901.ib")), LocalDate.of(2026, 9, 29));
        String json = new String(Files.readAllBytes(Paths.get(getClass()
                .getResource("/legacy-short-codes.json").toURI())), StandardCharsets.UTF_8);
        Map<String, Object> fixture = Json.asObject(Json.parse(json));
        List<DynamicTest> tests = new ArrayList<DynamicTest>();
        for (Object value : Json.asArray(fixture.get("cases"))) {
            Map<String, Object> row = Json.asObject(value);
            String input = Json.str(row, "input");
            String expected = Json.str(row, "expected");
            tests.add(DynamicTest.dynamicTest("Short code [" + input + "]",
                    () -> assertEquals(expected, resolver.replaceFirst(input))));
        }
        return tests;
    }
}
