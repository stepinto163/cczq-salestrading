package ats.bbox.framework.outputstream;

import ats.bbox.framework.*;

import java.util.Map;

public interface IOutputStream extends ILifecycle, IComponent, IStreamable {
    void onData(Map<String, Object> data) throws EDataException;
}
