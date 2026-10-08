package ats.app.ym.ceda.server;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;

public class CedaResultMap extends HashMap<String, Object> {
    public Map<String, Object> data = new Hashtable<>();
    public List<String> noPermitTopics = new ArrayList<>();
}
