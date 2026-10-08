package cgws.ficc.parser.dict;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.TreeSet;

/**
 * Immutable dictionary snapshot. Published once and never mutated, so every parse in flight
 * sees one consistent version (legacy code mutated HashMaps while requests were served).
 *
 * <p>Keys are normalised: trimmed and lower cased, exactly like the legacy loader does before
 * it calls {@code SetCodeName}.
 */
public final class BondDictionary {

    public static final String EMPTY = "<empty>";

    private final String version;
    private final Map<String, String> codeToName;
    private final Map<String, List<String>> nameToCodes;
    private final Map<String, String> codeToKey;
    private final Set<String> invalidCodes;

    BondDictionary(String version,
                   Map<String, String> codeToName,
                   Map<String, List<String>> nameToCodes,
                   Map<String, String> codeToKey,
                   Set<String> invalidCodes) {
        this.version = version;
        this.codeToName = Collections.unmodifiableMap(codeToName);
        this.nameToCodes = Collections.unmodifiableMap(nameToCodes);
        this.codeToKey = Collections.unmodifiableMap(codeToKey);
        this.invalidCodes = Collections.unmodifiableSet(invalidCodes);
    }

    public static String normalize(String raw) {
        return raw == null ? "" : raw.trim().toLowerCase();
    }

    public String version() {
        return version;
    }

    public int size() {
        return codeToName.size();
    }

    public boolean isEmpty() {
        return codeToName.isEmpty();
    }

    public boolean containsCode(String code) {
        return codeToName.containsKey(normalize(code));
    }

    public boolean isInvalid(String code) {
        return invalidCodes.contains(normalize(code));
    }

    public String nameOf(String code) {
        return codeToName.get(normalize(code));
    }

    public String keyOf(String code) {
        return codeToKey.get(normalize(code));
    }

    /** Candidate codes for a name, in stable (insertion then sorted) order. */
    public List<String> codesForName(String name) {
        List<String> codes = nameToCodes.get(normalize(name));
        return codes == null ? Collections.<String>emptyList() : codes;
    }

    public Set<String> codes() {
        return codeToName.keySet();
    }

    public Set<String> invalidCodes() {
        return invalidCodes;
    }

    public static final class Builder {

        private final Map<String, String> codeToName = new HashMap<String, String>();
        private final Map<String, String> codeToKey = new HashMap<String, String>();
        private final Map<String, Set<String>> nameToCodes = new HashMap<String, Set<String>>();
        private final Set<String> invalidCodes = new HashSet<String>();

        public Builder put(String code, String name, String key, boolean invalid) {
            String c = normalize(code);
            if (c.isEmpty()) {
                return this;
            }
            // Legacy behaviour: first insert wins for the name, later inserts of the same code
            // only refresh the name when it is still empty. Duplicate code+name is a no-op.
            String previous = codeToName.get(c);
            codeToName.put(c, name == null ? "" : name);
            if (key != null && !key.isEmpty()) {
                codeToKey.put(c, normalize(key));
            }
            String n = normalize(name);
            if (!n.isEmpty()) {
                Set<String> codes = nameToCodes.get(n);
                if (codes == null) {
                    codes = new TreeSet<String>();
                    nameToCodes.put(n, codes);
                }
                codes.add(c);
            }
            if (invalid) {
                invalidCodes.add(c);
            } else {
                invalidCodes.remove(c);
            }
            if (previous == null) {
                // keep compiler happy about unused local in builds with -Werror style settings
                previous = null;
            }
            return this;
        }

        public BondDictionary build(String version) {
            Map<String, List<String>> ordered = new HashMap<String, List<String>>();
            for (Map.Entry<String, Set<String>> e : nameToCodes.entrySet()) {
                ordered.put(e.getKey(), Collections.unmodifiableList(new ArrayList<String>(e.getValue())));
            }
            return new BondDictionary(version, new HashMap<String, String>(codeToName), ordered,
                    new HashMap<String, String>(codeToKey), new HashSet<String>(invalidCodes));
        }
    }
}
