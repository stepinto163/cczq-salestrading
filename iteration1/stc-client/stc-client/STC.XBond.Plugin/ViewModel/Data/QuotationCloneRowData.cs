using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.XBond.Plugin.ViewModel.Data
{
    /// <summary>
    /// 行情克隆数据
    /// </summary>
    public class QuotationCloneRowData
    {
        public string securityID;
        public string side;
        public string entryPx;
        public string settle;
        public string netPrice;
        public string key;
        public bool IsMe;
        public bool IsMm;
        public string FirmFlag;
        public DataRowView Item;
    }
}
