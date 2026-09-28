using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.PP
{
    public class FundAccountModel
    {
        public string id { get; set; }
        public string idPortfolioInfo { get; set; }
        public string categoryName { get; set; }
        public string name { get; set; }
        public string um { get; set; }
        public string officeStartDate { get; set; }
        public string effectDate { get; set; }
        public string remark { get; set; }
        public int status { get; set; }
    }
}
