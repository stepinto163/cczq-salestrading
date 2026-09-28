using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Base
{
    /// <summary>
    /// 简单债券信息
    /// </summary>
    public class SimpleBondsInfoPo
    {
        public string Bond_ID { get; set; }

        public string Short_Name { get; set; }

        public string pinYin { get; set; }

        public string pinyin_full { get; set; }

        public string listed_Market { get; set; }

        public string Security_Type_Id { get; set; }

        public string MarketIndicator { get; set; }

        /// <summary>
        /// 计算剩余期限
        /// </summary>
        public decimal? Option_Date { get; set; }

        /// <summary>
        /// 计算剩余期限
        /// </summary>
        public string Option_Type { get; set; }

        /// <summary>
        /// 计算剩余期限
        /// </summary>
        public string Maturity_Date { get; set; }

        /// <summary>
        /// 债券搜索 显示用
        /// </summary>
        public string RemaingingTime { get; set; }

        /// <summary>
        /// Excel中使用
        /// </summary>
        public string flag { get; set; }

        /// <summary>
        /// 债券类型 BCO,BNC,ABS,NCD  转换关系：1-信用债 BCO 2-利率债 BNC 11-ABS 12-NCD 
        /// GR搜索使用
        /// </summary>
        public string symbolType { get; set; }

        /// <summary>
        /// GR搜索使用
        /// </summary>
        public string Full_Name_EN { get; set; }

        //public string searchKey { get; set; }
        
        ///// <summary>
        ///// 增量取数据用
        ///// </summary>
        //public string bondlistinfo_modify_date { get; set; }

        ///// <summary>
        ///// 增量取数据用
        ///// </summary>
        //public string bond_modify_date { get; set; }

        ///// <summary>
        ///// 增量取数据用
        ///// </summary>
        //public string bondmaster_modify_date { get; set; }
    }
}
