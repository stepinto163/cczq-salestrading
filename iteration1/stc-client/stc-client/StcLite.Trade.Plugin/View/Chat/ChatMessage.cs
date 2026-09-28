using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDB.Trade.Plugin.View.Chat
{
    public class ChatMessage : BaseViewModel
    {
        /// <summary>
        /// 消息ID  == Version
        /// </summary>
        
        public int MsgID { get; set; }
        /// <summary>
        /// 经纪商
        /// </summary>
        private string _broker;
        public string Broker
        {
            get
            {
                return _broker;
            }
            set
            {
                this._broker = value;
                this.OnPropertyChanged(nameof(Broker));
            }
        }
        /// <summary>
        /// 订单状态  4,8,2不显示  TimeInForce
        /// </summary>
        public string ordstatus { get; set; }
        public string OrdStatus
        {
            get
            {
                return ordstatus;
            }
            set
            {
                this.ordstatus = value;
                this.OnPropertyChanged(nameof(OrdStatus));
            }
        }

        /// <summary>
        /// 显示内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public MsgType MsgType { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage {get;set;}

        /// <summary>
        /// 点价报价  0报价 1点价
        /// </summary>
        public string IsPoint { get; set; }

        /// <summary>
        /// 订单有效时间
        /// </summary>
        public string ValidUntilTime { get; set; }

        /// <summary>
        /// BDB 订单号
        /// </summary>
        public string ExecId { get; set; }

        /// <summary>
        /// 计时
        /// </summary>
        private string _timeremain;
        public string TimeRemain
        {
            get
            {
                return _timeremain;
            }
            set
            {
                this._timeremain = value;
                this.OnPropertyChanged(nameof(TimeRemain));
            }
        }
        /// <summary>
        /// 发送或接收时间
        /// </summary>
        public string TimeStamp { get; set; }



        public ChatMessage()
        {
            ErrorMessage = "";
            TimeRemain = "";
        }
    }

    public enum MsgType
    {
        Trader,
        Broker,
        Time,
    }

    public enum BrokerType
    {
        /// <summary>
        /// 国利
        /// </summary>
        TPSC,
        /// <summary>
        /// 信唐
        /// </summary>
        CTTJ,
        /// <summary>
        /// 国际
        /// </summary>
        CNEX,
        /// <summary>
        /// 中诚
        /// </summary>
        CCTB,
        /// <summary>
        /// 平安
        /// </summary>
        PTCN,
    }
}
