namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 报价表单弹窗的打开方式（原型右键菜单区分「新报价」与「编辑报价」两个入口，
    /// 两者共用同一个表单弹窗，仅标题与预填策略不同）。
    /// </summary>
    public enum QuoteFormMode
    {
        /// <summary>新报价：从看板行预填债券与价格信息，清空备注与标签。</summary>
        New,

        /// <summary>编辑报价：完整预填该行报价（含备注与标签）。</summary>
        Edit
    }
}
