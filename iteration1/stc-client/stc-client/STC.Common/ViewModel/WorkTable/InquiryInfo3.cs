using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.WorkTable
{
    public class InquiryInfo3
    {
        /// <summary>
        /// 1(净价)/2(全价)/3(到期收益率)/7(行权收益率)
        /// </summary>
        public string PriceType { get; set; }

        public string CleanPrice { get; set; }

        public string FullPrice { get; set; }
    }
}
