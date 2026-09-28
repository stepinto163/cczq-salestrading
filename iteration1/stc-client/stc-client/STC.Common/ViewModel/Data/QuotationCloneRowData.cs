using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Data
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
        public bool IsPermission;
        public string symbol;
        public string partycode6;
        public string cnshortname;
        public string transacttime;
        public string yield;
        public DataRowView Item;
    }
}
