package ficc.api;

import java.util.List;
import java.util.Map;

/** QuoteHistoryData */
public class QuoteHistoryData extends PageData {
    public String quoteId;
    public Integer pageNumber;
    public Integer pageSize;
    /** 固定浏览时的最大版本；刷新时省略，获取最新边界。 */
    public String snapshotRevision;
    public List<QuoteHistoryRecord> records;
}
