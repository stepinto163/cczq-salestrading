package ficc.api;

import java.util.List;
import java.util.Map;

/** 修改/确认/撤销等条目；未提供保留，显式 null 清除，false 关闭标记。 */
public class QuoteChangeItem extends QuoteInput {
    public String quoteId;
    public String expectedRevision;
    public String reason;
}
