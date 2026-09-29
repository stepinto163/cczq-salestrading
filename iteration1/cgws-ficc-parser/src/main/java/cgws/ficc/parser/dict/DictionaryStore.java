package cgws.ficc.parser.dict;

import cgws.ficc.parser.model.Diagnostic;

import java.util.ArrayList;
import java.util.List;

/**
 * Holds the published snapshot. Refresh publishes a new snapshot atomically; a failed refresh
 * keeps the previous one and records the failure instead of pretending the parser is ready.
 */
public final class DictionaryStore {

    private volatile BondDictionary current;
    private volatile String lastLoadError;

    public boolean isReady() {
        return current != null && !current.isEmpty();
    }

    public BondDictionary current() {
        return current;
    }

    public String lastLoadError() {
        return lastLoadError;
    }

    /** Publishes a freshly built snapshot; returns the previous version id for logging. */
    public String publish(BondDictionary snapshot) {
        if (snapshot == null) {
            throw new IllegalArgumentException("snapshot is null");
        }
        String previous = current == null ? null : current.version();
        current = snapshot;
        lastLoadError = null;
        return previous;
    }

    /** Records a failed refresh; the existing snapshot stays in service. */
    public void recordFailure(String message) {
        lastLoadError = message;
    }

    public List<Diagnostic> statusDiagnostics() {
        List<Diagnostic> out = new ArrayList<Diagnostic>();
        if (!isReady()) {
            out.add(new Diagnostic("DICTIONARY_NOT_READY",
                    lastLoadError == null ? "no dictionary published yet" : lastLoadError));
        } else if (lastLoadError != null) {
            out.add(new Diagnostic("DICTIONARY_REFRESH_FAILED", lastLoadError));
        }
        return out;
    }
}
