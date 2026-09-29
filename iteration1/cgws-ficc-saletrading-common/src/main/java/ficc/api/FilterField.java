package ficc.api;

import java.util.List;
import java.util.Map;

/** FilterField */
public class FilterField extends ProtocolModel {
    public String field;
    public String label;
    public String type;
    public List<String> operators;
    public List<DictionaryOption> enumValues;
    public String unit;
    public String source;
    public String group;
    public String scope;
    public Boolean enabled;
    public Boolean simulated;
}
