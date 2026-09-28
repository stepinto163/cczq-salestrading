using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class IntentionExecRequest
    {
        // stc用户名
        public string userName;
        // 意向单编号
        public string intentionId;
        // 关联标记
        public string relationFlag;
        // 日期，格式yyyy-MM-dd，不传代表所有
        public string date;
        // 日期 yyyy-MM-dd
        public string startDate;
        // 日期 yyyy-MM-dd
        public string endDate;
        // 状态，多个逗号分割
        public string status;
        // 页号码
        public int pageNum;
        // 页大小
        public int pageSize;
    }
}
