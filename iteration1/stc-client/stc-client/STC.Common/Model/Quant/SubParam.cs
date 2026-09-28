using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
   public class SubParam
    {
        private string option_key;
        private List<QuantParameter> param;

        public string Option_Key
        {
            get { return option_key; }
            set { option_key = value; }
        }
        public List<QuantParameter> Param
        {
            get { return param; }
            set { param = value; }
        }
    }
}
