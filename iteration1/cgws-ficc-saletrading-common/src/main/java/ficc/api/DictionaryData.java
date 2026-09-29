package ficc.api;

import java.util.List;
import java.util.Map;

/** DictionaryData */
public class DictionaryData extends ProtocolModel {
    public Map<String,List<DictionaryOption>> dictionaries;
    public List<FilterField> filterFields;
}
