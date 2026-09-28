using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Execute.Plugin.Utils
{
    /// <summary>
    /// CVS导出表头设置类
    /// </summary>
    public class TableHeaderUtils
    {
        private static TableHeaderUtils utils = new TableHeaderUtils();
        public static TableHeaderUtils Instance { get { return utils; } }
        
        public Dictionary<string, string> OrderTableHeader = new Dictionary<string, string>();

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
            OrderTableHeader.Add("Text", "内码");
            OrderTableHeader.Add("SecurityID", "债券代码");
            OrderTableHeader.Add("Symbol", "债券简称");
            OrderTableHeader.Add("Side", "方向");
            OrderTableHeader.Add("AvgPx", "成交价(%)");
            OrderTableHeader.Add("AllocQty", "成交量(万)");
            OrderTableHeader.Add("TradeDate", "交易日期");
            OrderTableHeader.Add("SettlType", "清算速度");
            OrderTableHeader.Add("TradeType", "类型");
            OrderTableHeader.Add("CounterPartyName", "对手方");
            OrderTableHeader.Add("CounterPartySubName", "对手方交易员");
            OrderTableHeader.Add("TrdType", "报价方式");
            //OrderTableHeader.Add("IndividualAllocID", "要素发至");
            OrderTableHeader.Add("UpdateTime", "更新时间");
            OrderTableHeader.Add("OnBehalfOfCompID", "渠道");
            OrderTableHeader.Add("AllocText", "经纪商备注");
            OrderTableHeader.Add("TradeStat", "状态");

            OrderTableHeader.Add("CreateTime", "创建日期");
            OrderTableHeader.Add("TradeID", "交易包编号");
            OrderTableHeader.Add("SecondaryTradeID", "外部成交单编号");
            OrderTableHeader.Add("MarketIndicator", "市场");
            OrderTableHeader.Add("SecurityType", "债券类型");
            OrderTableHeader.Add("StipulationValue", "行权收益率");
            OrderTableHeader.Add("CleanPrice", "净价");
            OrderTableHeader.Add("DirtyPrice", "全价");
            OrderTableHeader.Add("PartyID", "本方机构ID");
            OrderTableHeader.Add("PartyName", "本方机构名");
            OrderTableHeader.Add("PartySubID", "本方交易员ID");
            OrderTableHeader.Add("PartySubName", "本方交易员名称");
            OrderTableHeader.Add("CounterPartyID", "交易对手机构ID");
            OrderTableHeader.Add("CounterPartySubID", "对手方交易员ID");
            OrderTableHeader.Add("TradeMatchTimestamp", "加桥/代付方时间");
            OrderTableHeader.Add("DeliveryType", "清算方式");
            OrderTableHeader.Add("SettlDate", "清算日期");
            OrderTableHeader.Add("TrdSubType", "方向说明");
            //OrderTableHeader.Add("BrokerExecID", "中介核单编号");
            OrderTableHeader.Add("TransType", "交易单状态");
            OrderTableHeader.Add("BridgePaymentFixedRate", "代付费率");
            OrderTableHeader.Add("TrdMatchID", "约定号");
            OrderTableHeader.Add("PaymentFixedRate", "返点");
            OrderTableHeader.Add("EditRemark", "我的备注");
            OrderTableHeader.Add("TransactionPriceType", "价格备注");

        }
    }
}
