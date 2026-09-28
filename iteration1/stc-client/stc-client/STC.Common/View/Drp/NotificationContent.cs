using STC.Common.Model.Drp;
using STC.Common.View.Drp.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.View.Drp
{
    public class NotificationContent
    {
        public string Title { get; set; }

        public string Message { get; set; }

        public System.Windows.Forms.Screen CurScreen { get; set; }

        public NotificationType Type { get; set; }

        /// <summary>
        /// DRPDetail
        /// </summary>
        public string Detail { get; set; }

        public string Id { get; set; }

        public string ContentType { get; set; }

        public bool IsShow { get; set; }

        public string DelFlag { get; set; }

        public NotificationArea CurrArea { get; set; }
        public TimeSpan? expirationTime { get;set;}
        public Action onClick { get; set; }
        public Action onClose { get; set; }
        public string contentType { get; set; }

        public string contentskey { get; set; }

    }

    public enum NotificationType   
    {
        /// <summary>
        /// 定向回价窗口
        /// </summary>
        NotificationWindow,
        /// <summary>
        /// 被点价窗口
        /// </summary>
        NofirmPointPriceView,
        /// <summary>
        /// 成交回报
        /// </summary>
        ER,
        /// <summary>
        /// 价格匹配
        /// </summary>
        PM,
        /// <summary>
        /// UnRef
        /// </summary>
        LastWorkDayUnRefView,
        /// <summary>
        /// 双边断报提醒
        /// </summary>
        ANS,
        /// <summary>
        /// 经纪商
        /// </summary>
        BROKER,
        /// <summary>
        /// 报价确认
        /// </summary>
        ORDERCOMFIRM,

        /// <summary>
        /// REF确认
        /// </summary>
        REFCOMFIRM,
    }
}
