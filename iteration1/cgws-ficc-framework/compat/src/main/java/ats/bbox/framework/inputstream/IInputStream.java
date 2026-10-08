package ats.bbox.framework.inputstream;

import ats.bbox.framework.*;
import ats.bbox.framework.outputstream.IOutputStream;

import java.util.List;

public interface IInputStream extends ILifecycle, IComponent, IStreamable {
    void addOutputStream(IOutputStream stream);

    boolean removeOutputStream(IOutputStream stream);

    IOutputStream removeOutputStream(String name);

    List<IOutputStream> getOutputStream();

    IOutputStream getOutputStream(String name);
}
