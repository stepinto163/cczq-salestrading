using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.WorkTable.Plugin.Utils
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

        public static readonly string RECOGENIZEQUOTE = "RecognizeIntentionQuoteWin";
	    public static readonly string BATCHWARN = "BatchWarn";
        public static readonly string BATCHPROHIBIT = "BatchProhibit";
        public static readonly string BATCHEXCEPTION = "BatchException";
        public static readonly string BATCHWARN_INTENTION = "BatchWarn_Intention";
        public static readonly string BATCHPROHIBIT_INTENTION = "BatchProhibit_Intention";
        public static readonly string BATCHEXCEPTION_INTENTION = "BatchException_Intention";
        public static readonly string ENDINTENTIONWINDOW = "EndIntentionWindow";
        public static readonly string ADDEXECUTIONWIN = "AddExecutionWin";

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

        public static string GetBatchWarnWinKey()
        {
            return BATCHWARN;
        }

        public static string GetBatchProhibitWinKey()
        {
            return BATCHPROHIBIT;
        }

        public static string GetBatchExceptionWinKey()
        {
            return BATCHEXCEPTION;
        }

        public static string GetBatchWarnWinKey_Intention()
        {
            return BATCHWARN_INTENTION;
        }

        public static string GetBatchProhibitWinKey_Intention()
        {
            return BATCHPROHIBIT_INTENTION;
        }

        public static string GetBatchExceptionWinKey_Intention()
        {
            return BATCHEXCEPTION_INTENTION;
        }
    }
}
