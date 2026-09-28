using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Cal
{
    public class CalCurveReq
    {
        public int scale {get;set;}

        public string[] terms { get; set; }


        public string[] yields { get; set; }
    }
}
