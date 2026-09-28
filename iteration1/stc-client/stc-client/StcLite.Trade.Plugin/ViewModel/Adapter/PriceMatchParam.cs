using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.ViewModel.Adapter
{
    public class PriceMatchParam
    {
        public string WholeFlag { get; set; }
        public string Symbol { get; set; }
        public string Side { get; set; }
        public string SymbolName { get; set; }
        public string OrderId { get; set; }
        public string RandomNum { get; set; }
        public bool IsFirm { get; set; }
        public string CustomFirmAmount { get; set; }
        public string NetPrice { get; set; }
        public string Price { get; set; }
        public string SettlType { get; set; }

        public string NonfirmTime { get; set; }

        public string Terminal { get; set; }

        

        /// <summary>
        /// true: 先撤单再下单； false：直接下单
        /// </summary>
        public bool UpdataPrice = true;

        /// <summary>
        /// 1:所有
        /// 2:仅内部
        /// 3:xbond
        /// </summary>
        public string CrossVenues { get; set; }
        public string VenuesSelectF { get; set; }//渠道类型  不填写1,默认2，指定3 
        public string VenuesRdmNum { get; set; }//渠道类型默认，填写随机几家渠道
        public string SelfSelectVenues { get; set; }// 指定渠道参考3.1章节，多家用逗号隔开SOR_CTTJ,SOR_TPSC
        public string Venues { get; set; } //渠道
        public string InstitutionID{ get; set; }//客盘机构id
        public string InstitutionName { get; set; }//客盘机构名字
        public string TraderID { get; set; }//客盘交易员id
        public string TraderName { get; set; }//客盘交易员名字

        public NewOrderSingle OldOrder { get; set; }

        /// <summary>
        /// 发起方式
        /// 1:join
        /// 2:价格匹配nonfirm转firm
        /// </summary>
        public string OperationType;
        public string Info1 { get; set; }
        public string Info2 { get; set; }
        public string Info3 { get; set; }
        public string Info4 { get; set; }
        public string Info5 { get; set; }

        public string ClOrdId { get; set; }
    }
}
