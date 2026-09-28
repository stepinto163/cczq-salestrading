using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class BrokerReq
    {
        public string UserName;
        public string Channel;
        /**
         * 是否在白名单中 0否 1是
         */
        public string IfWhiteList;
        public string PageSize;
        public string PageNum;
    }
}
