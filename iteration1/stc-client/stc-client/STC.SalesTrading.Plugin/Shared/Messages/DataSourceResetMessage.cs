namespace STC.SalesTrading.Plugin.Shared.Messages
{
    /// <summary>
    /// 数据源重置（登出/切换账号/STC_Clear 广播）。各模块 VM 订阅后清空自身缓存集合
    /// （集合实例保持不变，绑定不断），下次访问按当前数据源重新装载。
    /// </summary>
    public class DataSourceResetMessage
    {
    }
}
