using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Position
{
    /// <summary>
    /// 修改手工订单请求/修改或增加组合接口请求
    /// </summary>
    public class UpdatePositionReq
    {
        /// <summary>
        /// 0-删除 1-修改
        /// </summary>
        public string operationType { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>
        public string orderID { get; set; }

        /// <summary>
        /// 原bookid
        /// </summary>
        public string oldBookID { get; set; }

        /// <summary>
        /// 新bookid
        /// </summary>
        public string newBookID { get; set; }
    }
}
