using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Fix.Message
{
    public class PResult : BaseReplyPo
    {
        public string RequestID = string.Empty;
        public String Info1 = string.Empty;
        public String Info2 = string.Empty;
        public String Info3 = string.Empty;
        public String Info4 = string.Empty;
        

        public override string ToString()
        {
            return $"PResult [Code={Code}, Msg={Msg}, RequestID={RequestID},Info1={Info1}, Info2={Info2}, Info3={Info3}, Info4={Info4}]";
        }
    }
}
