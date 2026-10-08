package ats.bbox.framework.inputstream;

import ats.bbox.framework.*;
import ats.bbox.framework.outputstream.IOutputStream;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public abstract class AbstractInputStream extends SimpleComponent implements IInputStream {
    private static final Logger LOGGER = LoggerFactory.getLogger(AbstractInputStream.class);
    private final ILifecycle lifecycle = new LifecycleAdapter();
    private final List<IOutputStream> outputs = new ArrayList<>();

    public ILifecycle getLifecycle() {
        return lifecycle;
    }

    public STATUS getStatus() {
        return lifecycle.getStatus();
    }

    public void addOutputStream(IOutputStream stream) {
        if (stream != null) {
            outputs.add(stream);
        }
    }

    public List<IOutputStream> getOutputStream() {
        return outputs;
    }

    public IOutputStream getOutputStream(String name) {
        for (IOutputStream stream : outputs) {
            if (stream.getName().equals(name)) {
                return stream;
            }
        }
        return null;
    }

    public boolean removeOutputStream(IOutputStream stream) {
        return outputs.remove(stream);
    }

    public IOutputStream removeOutputStream(String name) {
        IOutputStream stream = getOutputStream(name);
        outputs.remove(stream);
        return stream;
    }

    /** Preserve synchronous fan-out, original map identity and checked-error aggregation. */
    public List<EDataException> send(Map<String, Object> data) {
        List<EDataException> failures = null;
        for (IOutputStream stream : outputs) {
            try {
                stream.onData(data);
            } catch (EDataException error) {
                LOGGER.error(
                        "Stream delivery failed: " + getName() + " -> " + stream.getName(), error);
                if (failures == null) {
                    failures = new ArrayList<>();
                }
                failures.add(error);
            }
        }
        return failures;
    }
}
