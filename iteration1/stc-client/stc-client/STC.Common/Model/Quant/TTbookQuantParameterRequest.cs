using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Quant
{
    public class TTbookQuantParameterRequest
    {
        public string action { get; set; }
        public string quantid { get; set; }
        public List<TTbookQuantParameter> list = new List<TTbookQuantParameter>();
    }
}
