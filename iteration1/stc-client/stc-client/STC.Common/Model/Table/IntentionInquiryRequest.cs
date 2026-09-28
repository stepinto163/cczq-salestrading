using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Table
{
    public class IntentionInquiryRequest
    {
        public List<TableIntentionInquiry> dataList;

        /**
         * 0 : 查询 1：新增 2：修改 3：删除
         */
        public string action;

        public string inquiryKey; // 查询时使用

        public string intentionId;// 查询时使用

        // stc用户名
        public string userName;

        //// 日期，格式yyyy-MM-dd，不传代表所有
        //public string date;
        // 部门
        public string biztypeid;

        public string updateTimeStart;

        public string updateTimeEnd;

        public string quoteType;
    }
}
