package sc.app.stc.parser.image;

/**
 * 图片识别接口
 *
 * @author enhao.dong
 */
public interface ImageParser {
    /**
     * 识别方法
     *
     * @param bytes 图片的base64编码
     * @return 识别结果
     */
    String parse(String base64String);
}
