package sc.app.stc.parser.po;

import lombok.Data;
import lombok.ToString;

import java.util.List;

@Data
@ToString
public class ParsePo {
    public String content;
    public String userName;
    public List<String> contentList;
}
