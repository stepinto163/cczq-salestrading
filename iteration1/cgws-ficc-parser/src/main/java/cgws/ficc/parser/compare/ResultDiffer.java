package cgws.ficc.parser.compare;

import cgws.ficc.parser.model.QuoteInfo;
import cgws.ficc.parser.parse.LegacyResultCodec;

import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

/**
 * Field level diff of a whole case result: preprocessed text, number of results, order of results
 * and every field of every result. Nothing is truncated to the first quote.
 */
public final class ResultDiffer {

    public static final class FieldDiff {

        public final String path;
        public final String baseline;
        public final String candidate;

        FieldDiff(String path, String baseline, String candidate) {
            this.path = path;
            this.baseline = baseline;
            this.candidate = candidate;
        }

        @Override
        public String toString() {
            return path + ": baseline=[" + baseline + "] candidate=[" + candidate + "]";
        }
    }

    private ResultDiffer() {
    }

    /**
     * @param baselinePreprocessed working text produced by the legacy {@code PreProcess}
     * @param candidatePreprocessed working text produced by {@code TextNormalizer.preProcess}
     * @param baselineRaws legacy result string per line, empty strings kept
     * @param candidateRaws Java result string per line, empty strings kept
     */
    public static List<FieldDiff> diff(String baselinePreprocessed, String candidatePreprocessed,
                                       List<String> baselineRaws, List<String> candidateRaws) {
        List<FieldDiff> diffs = new ArrayList<FieldDiff>();
        compareValue(diffs, "preprocessed", baselinePreprocessed, candidatePreprocessed);
        compareValue(diffs, "resultCount", String.valueOf(baselineRaws.size()),
                String.valueOf(candidateRaws.size()));
        int shared = Math.min(baselineRaws.size(), candidateRaws.size());
        for (int i = 0; i < shared; i++) {
            String base = baselineRaws.get(i);
            String cand = candidateRaws.get(i);
            compareValue(diffs, "result[" + i + "]", base, cand);
            diffFields(diffs, i, base, cand);
        }
        for (int i = shared; i < baselineRaws.size(); i++) {
            compareValue(diffs, "result[" + i + "]", baselineRaws.get(i), null);
        }
        for (int i = shared; i < candidateRaws.size(); i++) {
            compareValue(diffs, "result[" + i + "]", null, candidateRaws.get(i));
        }
        return diffs;
    }

    private static void diffFields(List<FieldDiff> diffs, int index, String baselineRaw, String candidateRaw) {
        QuoteInfo baseline = LegacyResultCodec.parse(baselineRaw == null ? "" : baselineRaw);
        QuoteInfo candidate = LegacyResultCodec.parse(candidateRaw == null ? "" : candidateRaw);
        Map<String, String> b = LegacyResultCodec.flatten(baseline);
        Map<String, String> c = LegacyResultCodec.flatten(candidate);
        Set<String> keys = new LinkedHashSet<String>();
        keys.addAll(b.keySet());
        keys.addAll(c.keySet());
        for (String key : keys) {
            compareValue(diffs, "result[" + index + "]." + key, b.get(key), c.get(key));
        }
    }

    private static void compareValue(List<FieldDiff> diffs, String path, String baseline, String candidate) {
        if (baseline == null && candidate == null) {
            return;
        }
        if (baseline == null || candidate == null || !baseline.equals(candidate)) {
            diffs.add(new FieldDiff(path, baseline, candidate));
        }
    }
}
