using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Admin
{
    /// <summary>
    /// 代办事项/消息提醒查询请求结果
    /// </summary>
    public class TQueryMessage : BaseReplyPo
    {
        public int totalSize;
        public List<TTbookSystemMessage> Datas;
    }
    
    /// <summary>
    /// TBook系统参数消息
    /// </summary>
    public class TTbookSystemMessage
    {
        public string id;
        public string closeby;
        public string type;
        public string readflag;
        public string content;
        public string createtime;

        public string username;

        public string beginTime;
        public string endTime;
        public string delflag;
        public string updatetime;

        public string detail;
    }

    public class AdminConsts
    {
        public const string Admin_Normal = "N"; //正常消息
        public const string Admin_Warn = "A";   //警告消息
        public const string Admin_Critical = "C"; //严重消息
        //成交回报
        public const string Admin_ER = "ER";
        //信号消息
        public const string Admin_EV = "EV";
        //Nonfirm点价待确认
        public const string Admin_IT = "IT";
        //撤单通知
        public const string Admin_ST = "ST";
        //价格匹配消息
        public const string PriceMatch = "PM";
        //定向回价
        public const string Admin_RFQ = "RFQ";
        public const string Admin_MM = "ANS";
        public const string Admin_ExecutionReport = "[成交回报]";
        public const string Admin_RFQ_Txt = "[询价RFQ]";
        public const string Admin_Status_One = "1";
        public const string Admin_Status_Two = "2";
        public const string Admin_Status_Normal = "正常";
        public const string Admin_Status_Outtime = "已过期";
        public const string YYYYMMdd = "yyyy-MM-dd";
    }

    public class TMessageContent
    {
        public string id { get; set; }
        public string contentType { get; set; }
        public string content { get; set; }
        public string type { get; set; }
        public string securityid { get; set; }

        public string title { get; set; }
        public string counterpartyid;//对手交易员id
        public string counterpartyname;//对手交易员名字
        public string counterpartysubid; //对手机构id
        public string counterpartysubname;//对手机构名字
        public string status;//状态1 正常,2已过期 ;双边做市断报提醒整体做市状态(1：正常，2：需关注，3：已达标，4：失败)
        public string transacttime;//交易时间
        public string validtime;//有效时间 3分钟
        public string detail { get; set; }
        public string venue;
        public string presult;//处理结果
        public string ptime;//处理时间

        //是否显示
        public bool? show { get; set; }

        public string groupId; //内部看板tag组id
        public string groupname { get; set; }
        public string remark;

        //双边断报提醒字段
        public string creatTime;
        public int dangerCount;
        public int passiveCount;
        public string infos;
        public bool? needSend;

        public string inquiryid { get; set; }


    }
    public class TMessageDetail
    {
        public string symbol;
        public string securityid;
        public string strategyOrderId;
        public string orderID;
        public TMessageOrder order;
    }
    public class TMessageOrder
    {
        public string symbol;
        public string orderID;
    }


    public class TMessageRemark
    {
        public string focusType;//内部看板 bond 或 tag
        public string focusTags;//tags map
        public string focusBond;
    }
}
