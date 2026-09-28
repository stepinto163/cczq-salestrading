using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Risk
{
    /// <summary>
    /// 债券指定
    /// </summary>
    public class AppointRiskParameter
    {
        public String securityId;//债券代码
        public String shortName;//简称
        public String securityType;
        public String qtyMin;//报量下限
        public String qtyMax;//报量上限
        public String bid;//中债估值Bid
        public String ofr;//中债估值ofr
        public String dropAwayFromBid;//中债倒挂Bid
        public String dropAwayFromOfr;//中债倒挂ofr
        public String marketBid;//市场Bid
        public String marketOfr;//市场ofr
        public String update;//更新时间
    }
}
