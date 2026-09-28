using STC.Common.Model.Fix.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    public class SystemGlobalParamReq
    {
        //2更新；3删除
        public String action;
        public List<TTbookSystemParameter> list = new List<TTbookSystemParameter>();
    }
}
