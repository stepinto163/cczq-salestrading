using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.Common
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
        // 双边做市
        MmPlugin = 84,
        // 做市统计
        MmStatisticsPlugin = 85,
        // 绘图
        ChartPlugin = 6,
        // 新需求工作台
        Workstation = 110,
        // 新需求工作台CRM
        WorkstationCrm = 111,
        // 需求工作台
        WorkBench = 112,
        // CRM数据插件
        CrmPlugin = 113,
        // 发行人， 担保人
        TagsPlugin = 115,
        // 策略控制台
        QuantPlugin = 120,
        // Excel插件
        ExcelPlugin = 130,
        // Chrome插件
        ChromePlugin = 7,
        // 交易所做市
        MmshePlugin = 86,
        // 交易所做市做市
        MmsheStatisticsPlugin = 87,
        // 持仓管理
        PositionPlugin = 88,
        // 内部看板
        InnerBoardPlugin = 89,
        //KLine插件
        KLinePlugin = 140,
        //Rfq插件
        RfqPlugin = 150,
        //快捷键查询债券插件
        FindSecurityPlugin = 160,
        //python插件
        PythonPlugin = 170,
        //下载中心
        DownloadCenterPlugin = 180,
        //交易后处理
        PTPlugin = 181,
        // irs
        IRSPlugin = 182,
        //GR 交易处理
        GRBroker = 9,
        //GR 成交处理
        GRTransaction = 11,
        //GR 历史记录
        GRHistory = 12,
        //经纪商成交
        ExecutePlugin = 13,
        //债券管理
        BondManagement = 190,
        //行情看板
        TradingPlugin = 10,
        // 消息提醒
        MsgPlugin = 210,
        // 历史消息
        HistoryMsgPlugin = 50,
        // 聊天窗口
        ChatPlugin = 81,
        // 模拟交易
        SimulatePlugin = 61,
        /// <summary>
        /// 经纪商看板
        /// </summary>
        BrokerKanbanPlugin = 95,
        // 历史达成
        HistoryDealPlugin = 40,
        // 券组管理
        BondGroupManagePlugin = 220,

        // 平安养老持仓管理
        PAPositionPlugin = 82,
        // 销交平台
        SalesTradingPlugin = 230,
    }
}
