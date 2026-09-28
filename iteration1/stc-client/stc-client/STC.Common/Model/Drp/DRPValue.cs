
using System.Collections.Generic;

namespace STC.Common.Model.Drp
{
    public class DRPValue
    {
        public string algoName { get; set; }
        public decimal avgPx2 { get; set; }

        public string bizType { get; set; }
        public string bizTypeId { get; set; }
        public decimal cancelQty2 { get; set; }

        public string clOrderId { get; set; }
        public bool cross { get; set; }
        public string crossVenues { get; set; }
        public decimal cumQty { get; set; }
        public decimal cumQty2 { get; set; }
        public string currency { get; set; }
        public string delflag { get; set; }
        public bool emptyPrice { get; set; }
        public bool emptyQty { get; set; }
        public string execType2 { get; set; }
        public string execType4 { get; set; }
        public string firmFlag { get; set; }
        public string handlinst { get; set; }
        public string harddiskid { get; set; }
        public string insideip { get; set; }
        public string iocTimeOut { get; set; }
        public bool iocTimeOutLockFlag { get; set; }
        public decimal lastQty2 { get; set; }
        public decimal leftQty2 { get; set; }
        public bool lockQty { get; set; }
        public string gmacid { get; set; }
        public string wholeflag { get; set; }
        public string marketIndicator { get; set; }
        public string name { get; set; }
        public decimal netPrice { get; set; }
        public decimal openQty2 { get; set; }
        public string orderId { get; set; }
        public string orderStatus { get; set; }
        public string orderType { get; set; }
        public string partysubname { get; set; }
        public decimal price { get; set; }
        public decimal price2 { get; set; }
        public decimal priceAndOffSet { get; set; }
        public string gqmnumber { get; set; }
        public decimal qty { get; set; }
        public string quoteId { get; set; }
        public string rejCode { get; set; }
        public string rejReason { get; set; }
        public string rejecter { get; set; }
        public string roleId { get; set; }
        public string roleName { get; set; }
        public decimal sN { get; set; }
        public string salesAgentflag { get; set; }
        public string securityid { get; set; }
        public string settlDate { get; set; }
        public string settlType { get; set; }

        public string side { get; set; }
        public string symbol { get; set; }
        public string symbolType { get; set; }
        public string terminal { get; set; }
        public string text { get; set; }
        public string time { get; set; }
        public string timeInForce { get; set; }
        public string tradeDate { get; set; }
        public string transactTime { get; set; }
        public string unOpenQty { get; set; }
        public string updateTime { get; set; }
        public string userID { get; set; }
        public string userName { get; set; }
        /// <summary>
        /// 程序传过来的ValidUntilTime
        /// </summary>
        public string validuntitime { get; set; }
        public string valueDate { get; set; }
        public string valueDate2 { get; set; }
        public string venue { get; set; }
        public string venueGW { get; set; }

        public string venueusersid { get; set; }

        public string quotetype { get; set; }
        public string selfSelectVenues { get; set; }
        public string info2 { get; set; }

        //public object priceCondition { get; set; }
        public DqOrderInfo dqOrderInfo2 { get; set; }

        private string _validuntiltime;
        //以当前时间计算的过期时间
        public string ValidUntilTime
        {
            get
            {
                return _validuntiltime;
            }
            set
            {
                _validuntiltime = value;
            }
        }
    }
}


public class DqOrderInfo
{
    /// <summary>
    /// 3 忽略 1 议价
    /// </summary>
    public string actionType { get; set; }
    public string algoName { get; set; }
    public string bargainQty { get; set; }
    public string bargainYield { get; set; }
    public string orderId { get; set; }

    public string msgCode { get; set; }
}
