using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class Order
    {
        public string WholeFlag { get; set; }
        public string Symbol { get; set; }
        public string Side { get; set; }
        public string SymbolName { get; set; }
        public string OrderId { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Price { get; set; }
        public string SettlType { get; set; }
        public string OrderStatus { get; set; }
        public decimal Qty { get; set; }
        public string Info3 { get; set; }
        public string CrossVenues { get; set; }
        public string SelfSelectVenues { get; set; }
        public decimal UnOpenQty { get; set; }

        public string IocTimeOut { get; set; }
        public string Name { get; set; }
        public string Partysubname { get; set; }
        public decimal Qty2 { get; set; }
        public string TransactTime { get; set; }

        public decimal CumQty { get; set; }
        
        /// <summary>
        /// 剩余量
        /// </summary>
        public decimal Surplus { get { return Qty - CumQty; } }

        public string RoleName { get; set; }

        public string InstitutionName { get; set; }

        /// <summary>
        /// 意向单ID
        /// </summary>
        public string Info4 { get; set; }

        /// <summary>
        /// 组合账户
        /// </summary>
        public string AccountID { get; set; }

        public bool IsFirmFlag { get; set; }
    }
}
    
