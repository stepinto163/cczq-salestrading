package ficc.quote;

import ficc.api.QuoteRecord;

/** 返回非阻塞的计算状态；price 使用百分数收益率，不能用每日估值替代报价换算。 */
interface QuoteYieldResolver {
    Result resolve(QuoteRecord quote, String yieldType, String referenceVersion);

    default void forget(String quoteId) {}

    final class Result {
        final String price;
        final String code;

        Result(String price, String code) {
            this.price = price;
            this.code = code;
        }
    }
}
