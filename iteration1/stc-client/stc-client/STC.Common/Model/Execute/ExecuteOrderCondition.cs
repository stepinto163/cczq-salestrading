using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Execute
{
    public class ExecuteOrderCondition
    {
        private string user;
        public string User { get { return user; } set { user = value; } }
        private string tradeStat;
        public string TradeStat { get { return tradeStat; } set { tradeStat = value; } }
        private string onBehalfOfCompID;
        public string OnBehalfOfCompID { get { return onBehalfOfCompID; } set { onBehalfOfCompID = value; } }
        private string tradeDate;
        public string TradeDate { get { return tradeDate; } set { tradeDate = value; } }
        private string searchText;
        public string SearchText { get { return searchText; } set { searchText = value; } }
        private int pageNum;
        public int PageNum { get { return pageNum; } set { pageNum = value; } }
        private int pageSize;
        public int PageSize { get { return pageSize; } set { pageSize = value; } }
        private string sort;
        public string Sort { get { return sort; } set { user = sort; } }
    }
}
