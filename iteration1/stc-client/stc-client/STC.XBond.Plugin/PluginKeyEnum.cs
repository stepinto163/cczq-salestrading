using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin
{
    public enum PluginKeyEnum 
    {
        // 消息提醒
        CustomPlugin = 60,
        // 历史查询
        HistoryPlugin = 50,
        // 首页
        HomePagePlugin = 80,
        // 风控指标
        RiskPlugin = 90,
        // 设置
        SettingPlugin = 70,
        // TradeBook
        TradeBookPlugin = 22,
        // 渠道执行
        VenuesExecutionPlugin = 40,
        // 我的交易部分
        XBondCommonPlugin = 21,
        // XBond
        XBondPlugin = 20,
    }
}
