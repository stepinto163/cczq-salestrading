using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class PositionReq
    {
        /**
      * 日期
      */
        public string date = DateTime.Now.ToString("yyyy-MM-dd");//"2024-05-21";
        /**
         * 证券代码
         */
        public string securityIds;
        /**
         * 绑定的衡泰证券名称
         */
        public List<string> accNames;

        public string bookid;
        public string checkResult;

    }
}
