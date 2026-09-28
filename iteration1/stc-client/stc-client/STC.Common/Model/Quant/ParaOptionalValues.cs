using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    public class ParaOptionalValues
    {
        private string param_desc;
        private string param_key;
        private string param_value;

        public string Param_Desc
        {
            get { return param_desc; }
            set { param_desc = value; }
        }

        public string Param_Key
        {
            get { return param_key; }
            set { param_key = value; }
        }

        public string Param_Value
        {
            get { return param_value; }
            set { param_value = value; }
        }

        //private string option_desc;
        //private string option_key;
        //private string option_value;

        //public string Option_Desc
        //{
        //    get { return option_desc; }
        //    set { option_desc = value; }
        //}
        //public string Option_Key
        //{
        //    get { return option_key; }
        //    set { option_key = value; }
        //}
        //public string Option_Value
        //{
        //    get { return option_value; }
        //    set { option_value = value; }
        //}
    }
}
