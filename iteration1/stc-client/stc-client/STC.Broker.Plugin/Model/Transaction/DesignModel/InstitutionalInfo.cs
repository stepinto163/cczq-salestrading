using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction.DesignModel
{
    /// <summary>
    /// 机构信息
    /// </summary>
   public  class InstitutionalInfo {
        /// <summary>
        /// 机构id
        /// </summary>
        public string Idbkey { get; set; }
        /// <summary>
        /// 债券ID
        /// </summary>
        public string Shortnamecn { get; set; }
        /// <summary>
        /// shortnameen
        /// </summary>
        public string shortnameen { get; set; }
        /// <summary>
        /// instcode
        /// </summary>
        public string instcode { get; set; }
        /// <summary>
        /// cityvalue
        /// </summary>
        public string cityvalue { get; set; }
        /// <summary>
        /// insttypevalue
        /// </summary>
        public string insttypevalue { get; set; }
        /// <summary>
        /// issuer_code
        /// </summary>
        public string issuer_code { get; set; }
        /// <summary>
        /// citycode
        /// </summary>
        public string citycode { get; set; }
        /// <summary>
        /// pinyin
        /// </summary>
        public string pinyin { get; set; }
        /// <summary>
        /// pinyinfull
        /// </summary>
        public string pinyinfull{ get; set; }
        /// <summary>
        /// taglist
        /// </summary>
        public string taglist{ get; set; }
        /// <summary>
        /// bncshortnamelist
        /// </summary>
        public string bncshortnamelist{ get; set; }
        /// <summary>
        /// instbncshortname
        /// </summary>
        public string instbncshortname{ get; set; }
        /// <summary>
        /// bcoshortnamelist
        /// </summary>
        public string bcoshortnamelist{ get; set; }
        /// <summary>
        /// ncdshortnamelist
        /// </summary>
        public string ncdshortnamelist{ get; set; }
        /// <summary>
        /// irsshortnamelist
        /// </summary>
        public string irsshortnamelist{ get; set; }
    }
}
