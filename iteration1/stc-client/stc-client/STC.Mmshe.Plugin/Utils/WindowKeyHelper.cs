using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Mmshe.Plugin.Utils
{
    /// <summary>
    /// 窗口名公共类
    /// </summary>
    public class WindowKeyHelper
    {
        public static readonly string SENDPRICE = "SendPrice";
        public static readonly string OFR = "Ofr";
        public static readonly string BID = "Bid";
        //成交回报,key:$"ProcessIDBData-{exeID}"
        public static readonly string EXECBACK = "ProcessIDBData";
        //价格匹配,key:$"ProcessIDBData-{exeID}"
        public static readonly string PRICEMATCH = "PriceMatch";

        public static readonly string PRICEMATCH_SENDPRICE = "PriceMatch-SendPrice";
        public static readonly string PRICEMATCH_POINTPRICE = "PriceMatch-PointPrice";
        public static readonly string POINTPRICE = "PointPrice";
        public static readonly string BOOK = "Book";

        public static string GetPointWinKey(string id)
        {
            return string.Format("{0}-{1}", POINTPRICE, id);
        }

        public static string GetBookWinKey(string id)
        {
            return string.Format("{0}-{1}", BOOK, id);
        }

        public static string GetPointPriceWinKey(string id)
        {
            return string.Format("{0}-{1}", PRICEMATCH_POINTPRICE, id);
        }

        public static string GetSendPriceWinKey(string side, string security)
        {
            return string.Format("{0}-{1}-{2}", SENDPRICE, security, side);
        }

        public static string GetExecBackWinKey(string msgId)
        {
            return string.Format("{0}-{1}", EXECBACK, msgId);
        }

        public static string GetPriceMatchWinKey(string msgId)
        {
            return string.Format("{0}-{1}", PRICEMATCH, msgId);
        }

        public static string GetPriceMatchSendPriceWinKey(string msgId)
        {
            return string.Format("{0}-{1}", PRICEMATCH_SENDPRICE, msgId);
        }
    }
}
