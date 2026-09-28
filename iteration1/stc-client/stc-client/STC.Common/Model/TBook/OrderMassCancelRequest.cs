 

namespace STC.Common.Model.TBook
{
    public class OrderMassCancelRequest
    {
        // 撤销全部订单
        public static string CANCEL_ALL_ORDERS = "7";
        // 撤销所有策略单
        public static string CANCEL_STRATEGY_ORDERS = "8";
        // 撤销所有手工单
        public static string CANCEL_MANUAL_ORDERS = "9";
        public string UserName;
        public string SecurityID;
        public string MarketIndicator;
        public string Price;
        public string Side;
        public string MassCancelRequestType;
        public string CreateBy;
        public string Terminal;
        public string Text;
        public string QuantID;
    }
}
