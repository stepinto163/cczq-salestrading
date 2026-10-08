// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.api;

import com.alibaba.fastjson.annotation.JSONField;

public class Venue {
    public int sn;
    public String venue;
    public String venuetypegw;
    public String name;
    public String type;
    public int level;
    public int weight;

    @JSONField(serialize = false)
    public int minWeight;

    @JSONField(serialize = false)
    public int maxWeight;

    public boolean onoffenable = true;
    public boolean onoffedit = false;
    public boolean isvisible = true;
}
