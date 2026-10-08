package sc.app.stc.parser.analyzer;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import cn.hutool.core.util.XmlUtil;
import org.slf4j.Logger;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TreeMap;

public class Direction {

    private static final Logger logger = AsyncSlf4jLoggerFactory.getLogger(Direction.class);
    private static final Map<String, String> actionMap = new TreeMap();

    public static Map<String, String> getActionMap() {
        return actionMap;
    }


    public static void init() {
        String mCfgPath = System.getProperty("user.dir") + "/config/WordMap.xml";
        //初始化同义词典
        Document doc = XmlUtil.readXML(mCfgPath);
        Element element = doc.getDocumentElement();
        List<Element> synonyms = XmlUtil.getElements(element, "Group");
        synonyms.forEach(item -> {
            String wordType = item.getAttribute("type");
            if(wordType.equals("action")){
                List<Element> synonymList = XmlUtil.getElements(item, "item");
                synonymList.forEach(synonym -> {
                    String input = synonym.getAttribute("input");
                    String output = synonym.getAttribute("output");
                    actionMap.put(input,output);
                });
            }
        });
    }

    public static Map<String, String> getDirection(String contents){
        Map<String, String> map = new HashMap<>();
        contents = contents.toLowerCase();
        String action = null;
        Map<String, String> actionMap = Direction.getActionMap();
        for(String act : actionMap.keySet()){
            if(contents.contains(act)){
                contents = contents.replace(act,"");
                action = actionMap.get(act);
                if(action!=null){
                    if(action.equalsIgnoreCase("借"))
                        action = "bid";
                    if(action.equalsIgnoreCase("出"))
                        action = "ofr";
                }
            }
        }
        contents = TermDays.changeTermDays(contents);
        map.put("action",action);
        map.put("contents",contents);
        return map;
    }

    public static void clear() {
        actionMap.clear();
    }

}
