package ficc.api;

import java.util.List;
import java.util.Map;

/** BaseData */
public class BaseData extends ProtocolModel {
    public List<ReferenceRecord> records;
    public Boolean hasMore;
    public Integer nextOffset;
    public Integer totalCount;
    public String nextCursor;
}
