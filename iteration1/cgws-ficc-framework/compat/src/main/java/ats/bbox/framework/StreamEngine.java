package ats.bbox.framework;

import ats.bbox.framework.inputstream.IInputStream;
import ats.bbox.framework.outputstream.IOutputStream;
import ats.bbox.framework.stream.IStream;

import org.apache.commons.configuration.HierarchicalConfiguration;
import org.apache.commons.configuration.XMLConfiguration;

import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;

/** A synchronous graph. Category order and XML order intentionally match deployed BBox. */
public class StreamEngine {
    private final Map<String, IStreamable> streams = new LinkedHashMap<>();
    private final List<IOutputStream> outputs = new ArrayList<>();
    private final List<IStream> normal = new ArrayList<>();
    private final List<IInputStream> inputs = new ArrayList<>();

    public void load(String file) throws EException {
        if (!streams.isEmpty()) {
            throw new EException("Stream engine is already loaded");
        }
        try {
            XMLConfiguration configuration = new XMLConfiguration(file);
            add(configuration, "output-stream", IOutputStream.class, outputs);
            add(configuration, "stream", IStream.class, normal);
            add(configuration, "input-stream", IInputStream.class, inputs);
            link(configuration, "input-stream");
            link(configuration, "stream");
        } catch (EException error) {
            throw error;
        } catch (Exception error) {
            throw new EException("Cannot load stream graph " + file, error);
        }
    }

    private <T> void add(
            XMLConfiguration configuration, String category, Class<T> type, List<T> target)
            throws EException {
        for (IComponent component : ComponentBuilder.getComponents(configuration, category, type)) {
            if (streams.containsKey(component.getName())) {
                throw new EException("Duplicate stream name: " + component.getName());
            }
            streams.put(component.getName(), (IStreamable) component);
            target.add(type.cast(component));
        }
    }

    private void link(XMLConfiguration configuration, String category) throws EException {
        for (HierarchicalConfiguration entry : configuration.configurationsAt(category)) {
            String name = entry.getString("component[@name]");
            IInputStream source = (IInputStream) streams.get(name);
            for (String downstream : entry.getStringArray("output")) {
                IStreamable target = streams.get(downstream);
                if (!(target instanceof IOutputStream)) {
                    throw new EException("Invalid output " + downstream + " for stream " + name);
                }
                source.addOutputStream((IOutputStream) target);
            }
        }
    }

    public List<IInputStream> getInputStreams() {
        return inputs;
    }

    public List<IOutputStream> getOutputStreams() {
        return outputs;
    }

    public List<IStream> getNormalStreams() {
        return normal;
    }

    public Map<String, IStreamable> getAllStreams() {
        return streams;
    }

    public IStreamable getStream(String name) {
        return streams.get(name);
    }
}
