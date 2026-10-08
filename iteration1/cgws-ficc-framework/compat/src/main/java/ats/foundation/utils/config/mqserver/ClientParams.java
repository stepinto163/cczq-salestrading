// Recovered compatibility source; provenance: recovered-support.json.
package ats.foundation.utils.config.mqserver;

import java.util.ArrayList;
import java.util.List;

public class ClientParams extends RegisterParams {
    public String ServiceName = "";
    public Boolean RegisterEnable = false;
    public String RegisterServerList = "";
    public List<RegisterParams> RegisterConfigs = new ArrayList<RegisterParams>();
}
