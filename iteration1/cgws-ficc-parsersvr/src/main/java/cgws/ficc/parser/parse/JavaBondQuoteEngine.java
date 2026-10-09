package cgws.ficc.parser.parse;

import cgws.ficc.parser.model.AllQuote;
import java.time.LocalDate;

/**
 * Java 解析引擎入口，持有当前完整基础数据快照对应的 QuoteMessageParser。
 * reload 先构建新快照，再通过 volatile 引用整体替换；进行中的解析继续使用旧实例。
 * 刷新失败保留旧快照并抛异常，首次加载失败则不能宣称引擎可用。
 */
public final class JavaBondQuoteEngine implements BondQuoteEngine {
    public interface ReferenceLoader {
        MessageReferenceData load() throws Exception;
    }

    private final ReferenceLoader loader;
    private final boolean shortCodesEnabled;
    private volatile QuoteMessageParser parser;

    public JavaBondQuoteEngine(ReferenceLoader loader, boolean shortCodesEnabled) {
        if (loader == null) {
            throw new IllegalArgumentException("Reference loader is required");
        }
        this.loader = loader;
        this.shortCodesEnabled = shortCodesEnabled;
        reload();
    }

    @Override
    public AllQuote parse(String text, LocalDate businessDate) {
        return parser.parse(text, businessDate, shortCodesEnabled);
    }

    @Override
    public synchronized void reload() {
        try {
            MessageReferenceData next = loader.load();
            parser = new QuoteMessageParser(next);
        } catch (Exception error) {
            // Startup fails; a refresh failure propagates while preserving the previous snapshot.
            throw new IllegalStateException("Parser reference snapshot load failed", error);
        }
    }
}
