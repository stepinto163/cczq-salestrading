using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.CRM
{
    public class InstTraderInfo
    {
        /// <summary>
        /// 机构id
        /// </summary>
        public string InstId { get; set; }

        /// <summary>
        /// 交易员ID
        /// </summary>
        public string TraderId { get; set; }


        public override bool Equals(object obj)
        {
            return InstId.Equals(((InstTraderInfo)obj).InstId) && TraderId.Equals(((InstTraderInfo)obj).TraderId);
        }

        public override int GetHashCode()
        {
            return (InstId + TraderId).GetHashCode();
        }

        public override string ToString()
        {
            return InstId + TraderId;
        }
    }
}
