package sc.app.stc.parser.image;

import sc.app.stc.parser.image.impl.SumscopeImageParser;

import java.util.HashMap;

/**
 * 图片识别接口加载工厂
 *
 * @author enhao.dong
 */
public class ImageParserFactory {
    private static final HashMap<String, ImageParser> parsers = new HashMap<>();

    static {
        parsers.put("Sumscope", new SumscopeImageParser());
    }

    public static ImageParser getParser(String type) {
        return parsers.get(type);
    }
}
