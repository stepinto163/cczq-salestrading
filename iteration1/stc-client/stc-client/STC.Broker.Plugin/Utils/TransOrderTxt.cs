using com.gateway.log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
   public class TransOrderTxt
    {
        /// <summary>
        /// 当天成交的单子字体为细体
        /// </summary>
        public bool isliq { get; set; }
        public string Text { get; set; }
        /// <summary>
        /// 复制文本用于拷贝时使用
        /// </summary>
        public string CopyText { get; set; }
        /// <summary>
        /// 复制文本用于拷贝时使用
        /// </summary>
        public string SendTextA { get; set; }
        /// <summary>
        /// 复制文本用于拷贝时使用
        /// </summary>
        public string SendTextB { get; set; }
        /// <summary>
        /// 隐藏对手方文本
        /// </summary>
        public string HideCounterpartyText { get; set; }
        /// <summary>
        /// 助理A文本
        /// </summary>
        public string AssistantAText { get; set; }
        /// <summary>
        /// 助理B文本
        /// </summary>
        public string AssistantBText { get; set; }

        /// <summary>
        /// 缺桥字体显示红色
        /// </summary>
        public bool isShowColor { get; set; }
        public string TradeDate { get; set; }
        public string NextDate { get; set; }
        public bool IsShowDate
        {
            get
            {
                bool result = false;
                if (string.IsNullOrWhiteSpace(TradeDate) || string.IsNullOrWhiteSpace(NextDate))
                    return result;
                try
                {

                    if (TradeDate.Length == 8)
                    {
                        TradeDate = TradeDate.Insert(4, "-").Insert(7, "-");
                    }
                    if (NextDate.Length == 8)
                    {
                        NextDate = NextDate.Insert(4, "-").Insert(7, "-");
                    }
                    DateTime t1 = Convert.ToDateTime(TradeDate);
                    DateTime t2 = Convert.ToDateTime(NextDate);
                    int compNum = DateTime.Compare(t1, t2);
                    if (compNum > 0)
                    {
                        result = true;
                        string[] s = TradeDate.Split('-');
                        if (s.Length == 3)
                            Date = string.Format("{0}.{1}", s[1], s[2]);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                return result;
            }
        }
        public string Date { get; set; }
        /// <summary>
        /// 带交易员复制
        /// </summary>
        public string WithTraderText { get; set; }

        /// <summary>
        /// 隐藏发送文本
        /// </summary>
        public string HideSendQQMsgText { get; set; }
    }
}
