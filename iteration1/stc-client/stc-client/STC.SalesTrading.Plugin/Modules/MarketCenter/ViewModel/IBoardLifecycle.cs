namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 看板子页激活态回调：KeepAlive 常驻的看板不可见时，推送只置脏不查询
    /// （省去隐藏页的实时查询开销）；激活时若带脏标记补一次静默刷新（保留闪烁）。
    /// </summary>
    public interface IBoardLifecycle
    {
        /// <summary>设置激活态；false→true 时若带脏标记则按各板语义补刷。</summary>
        void SetBoardActive(bool active);
    }
}
