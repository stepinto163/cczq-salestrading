using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Base
{
    public class TBondValuationPo
    {
		 public string Id = "";//ID
		 public string Bond_ID = "";//债券编码
		 public string Listed_Market = "";//流通市场
		 public string valuation_yield;//收益率
		 public string valuation_net_price;//净价
		 public string valuation_date;//日期
		 public string status = "";//状态位:1-有效 2-无效
		 public string modify_time;//修改时间
		 public string bond_key_listed_market;//债券识别码
		 public decimal Val_Modified_Duration;//久期
		 public decimal Val_Convexity;//凸性
		 public decimal Val_Basis_Point_Value;//PVBP
		 public string bondcode;//债券编码
	}
}
