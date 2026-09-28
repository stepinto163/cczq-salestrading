using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Model.Transaction
{
    public class AccountInfo
    {
        /// <summary>
        /// 指定人名字
        /// </summary>
        public string Name;

        /// <summary>
        /// 指定人帐号
        /// </summary>
        public string userName;


        public List<AccountInfo> Users;
    }
}
