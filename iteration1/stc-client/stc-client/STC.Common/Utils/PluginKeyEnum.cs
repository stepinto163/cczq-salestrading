using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Data
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
        //Data.Plugin
        DataPlugin = 2,
        //Sys.Plugin
        SysPlugin = 3,
        // 交易包管理
        BookManagementPlugin =100,
        // 做市统计
        MmStatisticsPlugin = 85,
        // 绘图
        ChartPlugin = 120,
        // 需求工作台
        Workstation = 110,
        // Excel插件
        ExcelPlugin = 7,
    }
}
