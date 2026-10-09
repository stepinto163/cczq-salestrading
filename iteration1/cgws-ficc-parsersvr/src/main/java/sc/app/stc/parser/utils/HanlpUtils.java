package sc.app.stc.parser.utils;

import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.util.StrUtil;
import com.hankcs.hanlp.HanLP;
import com.hankcs.hanlp.corpus.tag.Nature;
import com.hankcs.hanlp.dictionary.CustomDictionary;
import com.hankcs.hanlp.model.crf.CRFLexicalAnalyzer;
import com.hankcs.hanlp.seg.Segment;
import com.hankcs.hanlp.seg.common.Term;
import com.hankcs.hanlp.tokenizer.BasicTokenizer;
import com.hankcs.hanlp.tokenizer.StandardTokenizer;
import com.hankcs.hanlp.utility.LexiconUtility;
import com.hankcs.hanlp.utility.Predefine;

import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.*;
import java.util.stream.Collectors;

import static com.hankcs.hanlp.corpus.tag.Nature.n;
import static com.hankcs.hanlp.utility.Predefine.logger;

public class HanlpUtils {
    public static String HANLP_ROOT = null;

    static {
        Properties p = new Properties();
        ClassLoader loader = Thread.currentThread().getContextClassLoader();
        if (loader == null) {
            loader = HanLP.Config.class.getClassLoader();
        }
        try {
            p.load(new InputStreamReader(Predefine.HANLP_PROPERTIES_PATH == null ?
                    loader.getResourceAsStream("hanlp.properties") :
                    new FileInputStream(Predefine.HANLP_PROPERTIES_PATH)
                    , "UTF-8"));
            // Resolve relative paths against the service working directory, including IDE runs
            // where Hutool otherwise treats them as paths beneath target/classes.
            HANLP_ROOT = java.nio.file.Paths.get(p.getProperty("root", ""))
                    .toAbsolutePath().normalize().toString();
        } catch (Exception e) {
            HANLP_ROOT = System.getProperty("HANLP_ROOT");
            if (HANLP_ROOT == null) {
                HANLP_ROOT = System.getenv("HANLP_ROOT");
            }
            if (HANLP_ROOT != null) {
                HANLP_ROOT = HANLP_ROOT.trim();
                p = new Properties();
                p.setProperty("root", HANLP_ROOT);
                logger.info("使用环境变量 HANLP_ROOT=" + HANLP_ROOT);
            }
        }
        System.out.println(p.getProperty("root"));
    }

    public static void main(String[] args) {
        String[] testCase = new String[]{
                "FR007_2Y-bid-300mio-2.45",
                "询价再进广东15年 湖南10年 浙江10年陕西7年",
                "repo1y 100mio rec 99",
                "cdb10 1y 100mio rec 99",
                "d10/g10 1y 100mio rec 99",
                "1x5 42我还是挂100个bid",
                "2Y    FR007_2Y    3.22/--    3*/--"
        };
        for (String sentence : testCase) {
//            List<Term> termList = parseWithCustomDictionary(sentence);
//            System.out.println(termList);
            List<String> split = StrUtil.split(sentence, " ");
            System.out.println(split);
        }
    }

    /**
     * 使用用户自定义词典分词
     * <p>
     * 自定义词典目录为：HANLP_ROOT/data/dictionary/custom
     * 每次修改自定义词典后，需要删除CustomDictionary.txt.bin文件，否则不会生效
     * 新增词典文件，修改hanlp.properties中CustomDictionaryPath配置
     *
     * @param sentence
     * @return
     */
    public static List<Term> parseWithCustomDictionary(String sentence) {
        //创建分词器
        Segment segment = HanLP.newSegment()
                //使用自定义词典
                .enableCustomDictionary(true)
                //提高自定义词典的优先级
                .enableCustomDictionaryForcing(true)
                //开启地名识别
                .enablePlaceRecognize(true);
        List<Term> termList = new ArrayList<>();
        if (StrUtil.isNotBlank(sentence)) {
            termList.addAll(segment.seg(sentence.toUpperCase(Locale.ROOT)));
        }
        return termList;
    }

}
