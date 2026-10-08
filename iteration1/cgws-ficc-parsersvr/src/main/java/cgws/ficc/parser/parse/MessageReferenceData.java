package cgws.ficc.parser.parse;

import cgws.ficc.parser.dict.BondDictionary;
import cgws.ficc.parser.model.BondReference;
import java.util.Collection;
import java.util.Collections;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

/** One atomically published snapshot, including dictionary, eligibility and enrichment data. */
public final class MessageReferenceData {
    public final BondDictionary dictionary;
    public final Set<String> shortCodes;
    private final Map<String, BondReference> bonds;

    public MessageReferenceData(Collection<BondReference> references, String version) {
        if (references == null || references.isEmpty()) {
            throw new IllegalArgumentException("Parser reference data must not be empty");
        }
        BondDictionary.Builder builder = new BondDictionary.Builder();
        Map<String, BondReference> records = new HashMap<String, BondReference>();
        Set<String> eligible = new HashSet<String>();
        for (BondReference reference : references) {
            if (records.put(reference.code, reference) != null) {
                throw new IllegalArgumentException("Duplicate parser bond code: " + reference.code);
            }
            builder.put(reference.code, reference.name, reference.key, reference.invalid);
            if (reference.shortCodeEligible && !reference.invalid) {
                eligible.add(reference.code);
            }
        }
        dictionary = builder.build(version);
        bonds = Collections.unmodifiableMap(records);
        shortCodes = Collections.unmodifiableSet(eligible);
    }

    public BondReference get(String code) {
        return bonds.get(BondDictionary.normalize(code));
    }
}
