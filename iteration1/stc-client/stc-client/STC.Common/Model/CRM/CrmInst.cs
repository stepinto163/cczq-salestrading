using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.CRM
{
   public class CrmInst
    {
        public string idbkey { get; set; }
        public string crmid { get; set; }
        public string id { get; set; }
        public string shortnamecn { get; set; }
        public string fullnamecn { get; set; }
        public string shortnameen { get; set; }

        public string instcode { get; set; }

        public string instname { get; set; }

        public string cityvalue { get; set; }

        public string insttypevalue { get; set; }

        public string industrytypevalue { get; set; }

        public string fundtypevalue { get; set; }

        public string countryareavalue { get; set; }

        public string managerdisplay { get; set; }

        public string description { get; set; }

        public int contactcount { get; set; }

        public string createtime { get; set; }

        public string updatetime { get; set; }

        public string updateaccount { get; set; }

        public string issuer_code { get; set; }

        public string isvip { get; set; }

        public string fullnameen { get; set; }

        public string citycode { get; set; }

        public string companyid { get; set; }

        public string pinyin { get; set; }

        public string pinyinfull { get; set; }

        public string idc_createtime { get; set; }

        public string idc_updatetime { get; set; }

        public string taglist { get; set; }

        public string bncshortnamelist { get; set; }

        public string ncdshortnamelist { get; set; }
        public string bcoshortnamelist { get; set; }
        public string irsshortnamelist { get; set; }

        public string TipsShortnamelist { get; set; }


        //上交所机构代码
        public string sseinstitutioncode { get; set; }
        //深交所-交易商代码
        public string szedealercode { get; set; }
        //深交所-交易主体代码
        public string ssetradingentitycode { get; set; }
    }
}
