using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.Utils
{
    public class TableHeaderUtils
    {
        private static TableHeaderUtils utils = new TableHeaderUtils();
        public static TableHeaderUtils Instance { get { return utils; } }

        public Dictionary<string, string> XBondTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> XBondTableHeader1 = new Dictionary<string, string>();
        public Dictionary<string, string> OrderTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> EMSOrderTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> DealsTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> RiskHistoryTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> HistoryMsgTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> HistoryAgencyTableHeader = new Dictionary<string, string>();
        public Dictionary<string, string> SettlTypeDic = new Dictionary<string, string>();
        public Dictionary<string, string> SettlDic = new Dictionary<string, string>();
        public Dictionary<string, string> ChineseDic = new Dictionary<string, string>();
        public Dictionary<string, string> SignDic = new Dictionary<string, string>();

        private string path = "";

        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value;
            }
        }
        public TableHeaderUtils()
        {
            init();
        }
        private void init()
        {
            XBondTableHeader.Add("Key", "债券代码");
            XBondTableHeader.Add("SettlType", "清算速度");
            XBondTableHeader.Add("SettlTypeText", "清算速度对应值");
            //XBondTableHeader.Add("symbol", "债券简称");
            XBondTableHeader.Add("CreateTime", "更新时间");

            XBondTableHeader1.Add("债券代码", "SecurityID");
            //XBondTableHeader1.Add("债券简称", "symbol");
            XBondTableHeader1.Add("清算速度", "SettlType");
            //XBondTableHeader1.Add("更新时间", "CreateTime");

            OrderTableHeader.Add("Side", "方向");
            OrderTableHeader.Add("SecurityID", "代码");
            OrderTableHeader.Add("Symbol", "简称");
            OrderTableHeader.Add("SettlType", "清算速度");
            OrderTableHeader.Add("IsPoint", "类型");
            OrderTableHeader.Add("OrderStatus", "状态");
            OrderTableHeader.Add("Price", "收益率(%)");
            OrderTableHeader.Add("Qty", "数量(万元)");
            OrderTableHeader.Add("ExecQty", "已确认(万元)");
            OrderTableHeader.Add("Veneues", "渠道");

            OrderTableHeader.Add("STCCode", "BDB单号");
            OrderTableHeader.Add("MyDealer", "本方交易员");
            OrderTableHeader.Add("Createdate", "创建日期");
            OrderTableHeader.Add("UpdateDate", "最后更新日期");
            OrderTableHeader.Add("UpdateTime", "最后更新时间");
            /*OrderTableHeader.Add("FirmFlag", "意向");
            OrderTableHeader.Add("TimeInForce", "类型");
            OrderTableHeader.Add("NetPrice", "净价(元)");
            OrderTableHeader.Add("Veneues", "报价方式");
            OrderTableHeader.Add("CrossVenues", "撮合范围");*/

            EMSOrderTableHeader.Add("Side", "方向");
            EMSOrderTableHeader.Add("SecurityID", "代码");
            EMSOrderTableHeader.Add("Symbol", "简称");
            EMSOrderTableHeader.Add("SettlType", "清算速度");
            EMSOrderTableHeader.Add("IsPoint", "类型");
            EMSOrderTableHeader.Add("OrderStatus", "状态");
            EMSOrderTableHeader.Add("Price", "收益率(%)");
            EMSOrderTableHeader.Add("Qty", "数量(万元)");
            EMSOrderTableHeader.Add("ExecQty", "已确认(万元)");
            EMSOrderTableHeader.Add("Veneues", "渠道");
            EMSOrderTableHeader.Add("StrategyOrderId", "BDB订单号");
            EMSOrderTableHeader.Add("STCCode", "外部订单");
            EMSOrderTableHeader.Add("MyDealer", "本方交易员");
            EMSOrderTableHeader.Add("Createdate", "创建日期");
            EMSOrderTableHeader.Add("UpdateDate", "最后更新日期");
            EMSOrderTableHeader.Add("UpdateTime", "最后更新时间");

            /*EMSOrderTableHeader.Add("FirmFlag", "意向");
            EMSOrderTableHeader.Add("TimeInForce", "类型");
            EMSOrderTableHeader.Add("NetPrice", "净价(元)");
            EMSOrderTableHeader.Add("Veneues", "报价方式");*/

            DealsTableHeader.Add("ExecID", "BDB成交单号");
            DealsTableHeader.Add("VenuesSelectDisplay", "渠道");
            DealsTableHeader.Add("ClOrdID", "外部成交单号");
            DealsTableHeader.Add("Side", "方向");
            DealsTableHeader.Add("SecurityID", "债券代码");
            DealsTableHeader.Add("Symbol", "债券简称");
            DealsTableHeader.Add("LastPx", "成交价(%)");
            DealsTableHeader.Add("NetPrice", "净价(元)");
            DealsTableHeader.Add("LastQty", "成交量(万元)");
            /*DealsTableHeader.Add("Createdate", "创建日期");*/
            DealsTableHeader.Add("ExecutionDate", "交易日期");
            DealsTableHeader.Add("SettlType", "清算速度");
            DealsTableHeader.Add("CounterPartyName", "对手方");
            DealsTableHeader.Add("CounterPartySubName", "对手交易员");
            DealsTableHeader.Add("ExecutionTime", "成交时间");
            DealsTableHeader.Add("SyncStatus", "同步状态");
            DealsTableHeader.Add("StrategyOrderId", "BDB订单号");
            DealsTableHeader.Add("RefOrderID", "对手订单号");
            DealsTableHeader.Add("Remark", "备注");

            RiskHistoryTableHeader.Add("Orderid", "BDB订单号");
            RiskHistoryTableHeader.Add("SecurityID", "债券代码");
            RiskHistoryTableHeader.Add("Side", "方向");
            RiskHistoryTableHeader.Add("FirmFlag", "意向");
            RiskHistoryTableHeader.Add("Price", "报价(%)");
            RiskHistoryTableHeader.Add("Qty", "数量(万元)");
            RiskHistoryTableHeader.Add("Tradedate", "交易日期");
            RiskHistoryTableHeader.Add("SettlType", "清算速度");
            RiskHistoryTableHeader.Add("Veneues", "方式");
            RiskHistoryTableHeader.Add("CrossVenues", "撮合范围");
            RiskHistoryTableHeader.Add("VenuesSelectDisplay", "渠道");
            RiskHistoryTableHeader.Add("Createtime", "触发时间");
            RiskHistoryTableHeader.Add("Type", "风控类型");
            RiskHistoryTableHeader.Add("Indicator", "指标");
            RiskHistoryTableHeader.Add("Indicatorvalue", "指标值");
            RiskHistoryTableHeader.Add("Indicatorlimitvalue", "指标限额");
            RiskHistoryTableHeader.Add("Indicatorsource", "指标来源");
            RiskHistoryTableHeader.Add("Result", "操作结果");
            RiskHistoryTableHeader.Add("UpdateTime", "操作时间");

            HistoryMsgTableHeader.Add("UpdateDate", "日期");
            HistoryMsgTableHeader.Add("UpdateTime", "更新时间");
            HistoryMsgTableHeader.Add("ContentType", "消息类型");
            HistoryMsgTableHeader.Add("Level", "程度");
            HistoryMsgTableHeader.Add("SecurityID", "债券代码");
            HistoryMsgTableHeader.Add("HeaderTitle", "消息标题");
            HistoryMsgTableHeader.Add("Content", "消息内容");
            HistoryMsgTableHeader.Add("Source", "来源");
            HistoryMsgTableHeader.Add("StrategyOrderId", "BDB订单号");

            HistoryAgencyTableHeader.Add("UpdateDate", "日期");
            HistoryAgencyTableHeader.Add("UpdateTime", "更新时间");
            HistoryAgencyTableHeader.Add("Ptime", "处理时间");
            HistoryAgencyTableHeader.Add("ContentType", "消息类型");
            HistoryAgencyTableHeader.Add("Level", "程度");
            HistoryAgencyTableHeader.Add("SecurityID", "债券代码");
            HistoryAgencyTableHeader.Add("HeaderTitle", "消息标题");
            HistoryAgencyTableHeader.Add("Content", "消息内容");
            HistoryAgencyTableHeader.Add("Presult", "处理结果");
            HistoryAgencyTableHeader.Add("Source", "来源");
            HistoryAgencyTableHeader.Add("StrategyOrderId", "BDB订单号");

            SettlTypeDic.Add("1","2");
            SettlTypeDic.Add("2", "");

            SettlDic.Add("1","T+1");
            SettlDic.Add("2", "T+0,T+1");

            ChineseDic.Add("Symbol", "1");
            ChineseDic.Add("TimeInForce", "2");
            ChineseDic.Add("OrderStatus", "3");
            ChineseDic.Add("MyDealer,", "4");
            ChineseDic.Add("Veneues", "5");
            ChineseDic.Add("CrossVenues", "6");
            ChineseDic.Add("VenuesSelectDisplay", "7");
            ChineseDic.Add("CounterPartyName", "8");
            ChineseDic.Add("CounterPartySubName", "9");
            ChineseDic.Add("SyncStatus", "10");
            ChineseDic.Add("Remark", "11");
            ChineseDic.Add("Type", "12");
            ChineseDic.Add("Indicator", "13");
            ChineseDic.Add("Indicatorsource", "14");
            ChineseDic.Add("Result", "15");
            ChineseDic.Add("ContentType", "16");
            ChineseDic.Add("Level", "17");
            ChineseDic.Add("HeaderTitle", "18");
            ChineseDic.Add("Content", "19");
            ChineseDic.Add("Source", "20");
            ChineseDic.Add("Presult", "21");

            SignDic.Add("Qty","1");
            SignDic.Add("ExecQty", "2");
            SignDic.Add("LastQty", "3");
            SignDic.Add("Indicatorvalue", "4");
            SignDic.Add("Indicatorlimitvalue", "5");
            SignDic.Add("STCCode", "6");
            SignDic.Add("Price", "7");
            SignDic.Add("NetPrice", "8");
            SignDic.Add("LastPx", "9");
        }
    }
}
