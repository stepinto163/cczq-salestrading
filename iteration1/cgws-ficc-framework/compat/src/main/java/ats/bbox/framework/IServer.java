package ats.bbox.framework;

import ats.bbox.framework.inputstream.IInputStream;
import ats.bbox.framework.outputstream.IOutputStream;
import ats.bbox.framework.resource.IResource;
import ats.bbox.framework.stream.IStream;

import java.util.List;
import java.util.Map;

public interface IServer extends ILifecycle {
    IStreamable getStream(String name);

    Map<String, IStreamable> getStreams();

    List<IInputStream> getInputStreams();

    List<IOutputStream> getOutputStreams();

    List<IStream> getNormalStreams();

    IResource getResource(String name);

    List<IResource> getResources();

    String getServerName();

    int getTraceLogLevel();

    String[] getUnstartList(String name);
}
