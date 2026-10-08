package cgws.ficc.parser.compare;

import cgws.ficc.parser.dict.DictionaryRow;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStreamReader;
import java.io.Reader;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/** Fixed input set: one dictionary, many cases. Same bytes for baseline and candidate. */
public final class CaseSet {

    public String id;
    public String platform = "unknown";
    public String note;
    public final List<DictionaryRow> dictionary = new ArrayList<DictionaryRow>();
    public final List<Case> cases = new ArrayList<Case>();
    public String baselineSource;
    public String baselineNativeSha256;
    public String baselineJarSha256;

    public static final class Case {

        public String id;
        public String text;
        public String businessDate;
        /** Capability group, so a suite can be run per area. */
        public String capability;
        /** Where the expectation comes from: BASELINE_BINARY / PRODUCT_CONTRACT / INFERRED. */
        public String expectedSource = "BASELINE_BINARY";
        public String note;
    }

    public static CaseSet load(File file) throws Exception {
        Reader reader = new InputStreamReader(new FileInputStream(file), "UTF-8");
        StringBuilder b = new StringBuilder();
        try {
            char[] buf = new char[8192];
            int n;
            while ((n = reader.read(buf)) > 0) {
                b.append(buf, 0, n);
            }
        } finally {
            reader.close();
        }
        Map<String, Object> root = Json.asObject(Json.parse(b.toString()));
        CaseSet set = new CaseSet();
        set.id = Json.str(root, "id");
        set.platform = Json.str(root, "platform");
        set.note = Json.str(root, "note");
        set.baselineSource = Json.str(root, "baselineSource");
        set.baselineNativeSha256 = Json.str(root, "baselineNativeSha256");
        set.baselineJarSha256 = Json.str(root, "baselineJarSha256");
        for (Object item : Json.asArray(root.get("dictionary"))) {
            Map<String, Object> row = Json.asObject(item);
            String code = Json.str(row, "code");
            String name = Json.str(row, "name");
            String key = Json.str(row, "key");
            Object invalid = row.get("invalid");
            set.dictionary.add(new DictionaryRow(code, name, key, Boolean.TRUE.equals(invalid)));
        }
        for (Object item : Json.asArray(root.get("cases"))) {
            Map<String, Object> c = Json.asObject(item);
            Case one = new Case();
            one.id = Json.str(c, "id");
            one.text = Json.str(c, "text");
            one.businessDate = Json.str(c, "businessDate");
            one.capability = Json.str(c, "capability");
            if (c.get("expectedSource") != null) {
                one.expectedSource = Json.str(c, "expectedSource");
            }
            one.note = Json.str(c, "note");
            set.cases.add(one);
        }
        return set;
    }

    /**
     * Canonical form of every input a baseline depends on. Every field that reaches the parser is
     * part of it, including the dictionary key and the business date; adding a new input to the
     * suite means adding it here, otherwise an old baseline stays valid although the input changed.
     */
    public Map<String, Object> canonical() {
        Map<String, Object> root = new LinkedHashMap<String, Object>();
        root.put("inputSet", "parser-input/1");
        root.put("id", text(id));
        root.put("platform", text(platform));
        root.put("note", text(note));
        root.put("baselineSource", text(baselineSource));
        root.put("baselineNativeSha256", text(baselineNativeSha256));
        root.put("baselineJarSha256", text(baselineJarSha256));
        List<Object> rows = new ArrayList<Object>();
        for (DictionaryRow row : dictionary) {
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("code", text(row.code));
            entry.put("name", text(row.name));
            entry.put("key", text(row.key));
            entry.put("invalid", row.invalid);
            rows.add(entry);
        }
        root.put("dictionary", rows);
        List<Object> casesJson = new ArrayList<Object>();
        for (Case one : cases) {
            Map<String, Object> entry = new LinkedHashMap<String, Object>();
            entry.put("id", text(one.id));
            entry.put("text", text(one.text));
            entry.put("businessDate", text(one.businessDate));
            entry.put("capability", text(one.capability));
            entry.put("expectedSource", text(one.expectedSource));
            entry.put("note", text(one.note));
            casesJson.add(entry);
        }
        root.put("cases", casesJson);
        return root;
    }

    private static String text(String value) {
        return value == null ? "" : value;
    }

    public Case find(String id) {
        for (Case c : cases) {
            if (c.id.equals(id)) {
                return c;
            }
        }
        return null;
    }
}
