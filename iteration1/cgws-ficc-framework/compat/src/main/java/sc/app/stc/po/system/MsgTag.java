// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.po.system;

public enum MsgTag {
    JY_1("1", "\u4ea4\u6613"),
    HJ_1_1("1_1", "\u56de\u4ef7"),
    DJ_1_2("1_2", "\u70b9\u4ef7"),
    CJHB_1_3("1_3", "\u6210\u4ea4\u56de\u62a5"),
    JGPP_1_4("1_4", "\u4ef7\u683c\u5339\u914d"),
    JYCW_1_6("1_6", "\u4ea4\u6613\u9519\u8bef\u63d0\u793a)"),
    NBKB_2("2", "\u5185\u90e8\u770b\u677f"),
    DDDY_2_1("2_1", "\u8ba2\u5355\u8ba2\u9605"),
    Rfq_3("3", "RFQ"),
    Rfq_3_1("3_1", "RFQ\u6d88\u606f"),
    SB_4("4", "\u53cc\u8fb9\u505a\u5e02"),
    SB_4_1("4_1", "\u53cc\u8fb9\u505a\u5e02\u6d88\u606f"),
    SB_4_2("4_2", "\u53cc\u8fb9\u4ea4\u6613\u5f15\u64ce"),
    CLKZT_5("5", "\u7b56\u7565\u63a7\u5236\u53f0"),
    CLKZT_5_1("5_1", "\u7b56\u7565\u63a7\u5236\u53f0"),
    XTXX_6("6", "\u7cfb\u7edf\u6d88\u606f"),
    JYYQTX_6_1("6_1", "\u4ea4\u6613\u5f15\u64ce\u63d0\u9192"),
    TB_6_1_1("6_1_1", "\u5185\u90e8"),
    SB_6_1_2("6_1_2", "\u53cc\u8fb9"),
    XB_6_1_3("6_1_3", "X-Bond"),
    JJS_6_1_4("6_1_4", "\u7ecf\u7eaa\u5546"),
    XREPO_6_1_5("6_1_5", "X-Repo"),
    NDM_6_1_6("6_1_6", "RepoNDM"),
    QDM_6_1_7("6_1_7", "RepoQDM"),
    SDN_6_1_8("6_1_8", "SDN"),
    REPO_6_1_9("6_1_9", "\u56de\u8d2d"),
    XTXX_7("7", ""),
    FWLJTX_7_1("7_1", "\u670d\u52a1\u8fde\u63a5\u63d0\u9192"),
    TB_7_1_1("7_1_1", "Tradebook"),
    SB_7_1_2("7_1_2", "\u53cc\u8fb9"),
    XB_7_1_3("7_1_3", "X-Bond"),
    JJS_7_1_4("7_1_4", "\u7ecf\u7eaa\u5546"),
    EMS_7_1_5("7_1_5", "EMS"),
    MD_7_1_6("7_1_6", "MD"),
    IDB_7_1_7("7_1_7", "IDB"),
    QQ_8_1_1("8_1_1", "IM"),
    JYS_8_1_2("8_1_2", "\u4ea4\u6613\u6240\u505a\u5e02"),
    XQGZT("9", "\u9700\u6c42\u5de5\u4f5c\u53f0"),
    XQGZT_9_1("9_1", "\u9700\u6c42\u5de5\u4f5c\u53f0\u8ba2\u9605\u7ec4"),
    PT("10", "\u6210\u4ea4\u7ba1\u7406");

    private final String id;
    private final String name;

    private MsgTag(String id, String name) {
        this.id = id;
        this.name = name;
    }

    public String getId() {
        return this.id;
    }

    public String getName() {
        return this.name;
    }
}
