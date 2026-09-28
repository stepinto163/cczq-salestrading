using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDC.Data.Plugin.Model.Base
{
    public class BondsInfoPo
    {
        public string BondID { get; set; }
        public string BondKey { get; set; }
        public string BondListInfoModifyDate { get; set; }
        public string BondModifyDate { get; set; }
        public string Delflag { get; set; }
        public string MaturityDate { get; set; }
        public string RemaingingTime { get; set; }
        public string ShortName { get; set; }
        public string ListedMarket { get; set; }
        //public string BondSubtype { get; set; }
        //public string IssuerRatingCurrent { get; set; }
        public string pinYin { get; set; }
        public string pinYinFull { get; set; }
        public  string goodstype { get; set; }
    }
}
