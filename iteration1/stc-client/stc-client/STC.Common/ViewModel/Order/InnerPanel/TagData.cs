using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Order.InnerPanel
{
    public class TagData
    {
        //public string VipCode { get; set; }

        //public string ClientType { get; set; }

        //public string SubTag { get; set; }

        #region 返回的不一样
        public string VIP { get; set; }
        public string ORG_TYPE { get; set; }
        public string IF_SUB_BOND { get; set; }
        public string HURRY_FLAG { get; set; }//急
        public string SUGGEST_FLAG { get; set; }//荐
        public string FIRE_FLAG { get; set; }//火
        public string FLOOR_PRICE { get; set; }//底价
        public string CUSTOMER_PRICE { get; set; }//客户报价
        public string CUSTOMER_SPREAD { get; set; }//点差
        public string PUBLIC_REMARKS { get; set; }//备注
        public string PERSONAL_REMARKS { get; set; }//本人备注

        public string OCO_FLAG { get; set; } // oco，打包

        public string CHANNEL { set; get; } // 渠道

        public string INTENTION_KEY { set; get; }

        public string INTENTION_GROUP_KEY { set; get; }

        public string SUB_INTENTIONIDS { get; set; }

        public string AUTO_INTENTION { get; set; }

        /**
         * QQ信息相关
         */
        public string fromCorpName;
        public string fromName;
        public string fromQQ;
        public string source;
        public string toCorpName;
        public string toGroupOpenID;
        public string toName;
        public string toQQ;
        public string toGroupName;
        public string toQQGroup;
        public string ADD_WHITE_LIST;//是否显示加号
        #endregion
        public string inquiryStatus;// 是否有询价 0：没有 1： 有
        public string MANAGER_ASSISTANTS;
        public string MATCHED_QUANTITY;
        public string StrategyOrderId;
    }

}
