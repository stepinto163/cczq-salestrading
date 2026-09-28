using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.ViewModel.Order.InnerPanel
{
    public class NoteData
    {
        public string PUBLIC_REMARKS { get; set; }//个人笔记-仅自己
        public Dictionary<string, string> PERSONAL_REMARKS { get; set; }//个人笔记-仅自己(keyValue)
        public string COMMAND_NOTES { get; set; }// 指令备注
        public string REF_NOTES { get; set; }//撤销备注

        public string RISK_CHECK_RESULT { get; set; }

        public string RISK_CHECK_DETAIL { get; set; }
    }
}
