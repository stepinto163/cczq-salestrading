using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.RFQ
{
    public class RfqstgActReq
    {
        /// <summary>
        /// 启动、停止时ID列表
        /// </summary>
        public List<string> ids;
        /// <summary>
        /// 0:查询,1:插入,2:更新,3:删除,4:导入,5:启动,6:停止
        /// </summary>
        public int action;
        public RfqstgActStrategyReq strategy;

    }
}
