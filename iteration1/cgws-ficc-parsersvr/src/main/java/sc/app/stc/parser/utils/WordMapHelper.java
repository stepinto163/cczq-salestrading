package sc.app.stc.parser.utils;

import cn.hutool.core.util.XmlUtil;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import java.util.List;
import java.util.Map;
import java.util.TreeMap;

public class WordMapHelper{

    private static final Map<String, String> actionMap = new TreeMap();

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
}
