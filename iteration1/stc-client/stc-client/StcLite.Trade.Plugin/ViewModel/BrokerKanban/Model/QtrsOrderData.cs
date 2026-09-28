using STC.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.ViewModel.BrokerKanban
{
    /// <summary>
    /// 订单反馈
    /// </summary>
    public class QtrsOrderData : BaseViewModel
    {
        private string _qmID;
        /// <summary>
        /// ID
        /// </summary>
        public string qmID
        {
            get
            {
                return _qmID;
            }
            set
            {
                _qmID = value;
                this.OnPropertyChanged(nameof(qmID));
            }
        }


        private int _side;

        /// <summary>
        /// 用户报价方向
        /// Bid：买入 
        /// Ofr：卖出
        /// 1=bid
        /// 2=ofr
        /// </summary>
        public int side
        {
            get
            {
                return _side;
            }
            set
            {
                _side = value;
                this.OnPropertyChanged(nameof(side));
            }
        }


        private string _securityID;
        /// <summary>
        /// 债券代码
        /// </summary>
        public string securityID
        {
            get
            {
                return _securityID;
            }
            set
            {
                _securityID = value;
                this.OnPropertyChanged(nameof(securityID));
            }
        }

        private string _securityType;
        /// <summary>
        /// 债券类型
        /// BNC=利率债
        /// BCO=信用债
        /// NCD=存单二级
        /// </summary>
        public string securityType
        {
            get
            {
                return _securityType;
            }
            set
            {
                _securityType = value;
                this.OnPropertyChanged(nameof(securityType));
            }
        }

        private string _symbol;
        /// <summary>
        /// 债券简称
        /// </summary>
        public string symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                _symbol = value;
                this.OnPropertyChanged(nameof(symbol));
            }
        }

        private string _execType;
        /// <summary>
        /// 执行状态
        ///0=new，新建订单
        ///4=canceled，订单被撤销
        ///5=replaced，订单被覆盖更新
        ///8=rejected，订单被拒绝
        ///C=expired，订单过期
        ///F=trade，订单被成交或部分成交
        ///M=locked，订单被锁"
        /// </summary>
        public string execType 
        {
            get
            {
                return _execType;
            }
            set
            {
                _execType = value;
                this.OnPropertyChanged(nameof(execType));
            }
        }

        private int _execTypeReason;
        /// <summary>
        /// 当ordStatus=4时，必传
        /// 3=order cancelled upon request，表示是由api客户发起的订单取消操作
        /// 4=unsolicited order cancellation，表示由broker发起的订单取消操作"
        /// </summary>
        public int execTypeReason
        {
            get
            {
                return _execTypeReason;
            }
            set
            {
                _execTypeReason = value;
                this.OnPropertyChanged(nameof(execTypeReason));
            }
        }

        private string _settlType;
        /// <summary>
        /// 清算速度   当ExecType=F时必传，表示清算速度：
        /// 1=T+0
        /// 2=T+1
        /// 3=明天+0 
        /// MM.DD+数字=远期
        /// </summary>
        public string settlType
        {
            get
            {
                return _settlType;
            }
            set
            {
                _settlType = value;
                this.OnPropertyChanged(nameof(settlType));
            }
        }
        /// <summary>
        /// 清算日期 当ExecType=F时必传，表示清算日期
        /// </summary>
        public string settlDate { get; set; }

        private string _tradeDate;
        /// <summary>
        /// 交易日期 当ExecType=F时必传，表示成交日期
        /// </summary>
        public string tradeDate
        {
            get
            {
                return _tradeDate;
            }
            set
            {
                _tradeDate = value;
                this.OnPropertyChanged(nameof(tradeDate));
            }
        }

        private string _ordStatus;
        /// <summary>
        /// 订单状态
        ///0 = 有效 New
        ///1 = 部分成交Partially filled
        ///2 = 全部成交 Filled
        ///4 = 撤销 Canceled
        ///8 = 拒绝 Reject
        ///C = 过期 Expired"
        /// </summary>
        public string ordStatus
        {
            get
            {
                return _ordStatus;
            }
            set
            {
                _ordStatus = value;
                this.OnPropertyChanged(nameof(ordStatus));
            }
        }

        private int _ordRejReason;
        /// <summary>
        /// 当OrdStatus=8时，描述订单拒绝理由
        /// </summary>
        public int ordRejReason
        {
            get
            {
                return _ordRejReason;
            }
            set
            {
                _ordRejReason = value;
                this.OnPropertyChanged(nameof(ordRejReason));
            }
        }

        private string _rejectText;
        /// <summary>
        /// 当OrdRejReason=99时，表示错误原因的文字说明
        /// </summary>
        public string rejectText
        {
            get
            {
                return _rejectText;
            }
            set
            {
                _rejectText = value;
                this.OnPropertyChanged(nameof(rejectText));
            }
        }

        private decimal _price;
        /// <summary>
        /// 报价
        /// 原始订单价格
        /// </summary>
        public decimal price
        {
            get
            {
                return _price;
            }
            set
            {
                _price = value;
                this.OnPropertyChanged(nameof(price));
            }
        }

        /// <summary>
        /// 原始订单报价方式，传输MATURITY
        /// </summary>
        public string yieldType { get; set; }

        private decimal _yield;
        /// <summary>
        /// 原始收益率（%）
        /// </summary>
        public decimal yield
        {
            get
            {
                return _yield;
            }
            set
            {
                _yield = value;
                this.OnPropertyChanged(nameof(yield));
            }
        }

        private decimal _lastPx;
        /// <summary>
        /// 收益率（%） 成交收益率
        ///当ExecType=F时必传，表示本次成交价格，收益率，单位%
        /// </summary>
        public decimal lastPx
        {
            get
            {
                return _lastPx;
            }
            set
            {
                _lastPx = value;
                this.OnPropertyChanged(nameof(lastPx));
            }
        }

        private decimal _cleanPrice;
        /// <summary>
        /// 净价
        /// </summary>
        public decimal cleanPrice
        {
            get
            {
                return _cleanPrice;
            }
            set
            {
                _cleanPrice = value;
                this.OnPropertyChanged(nameof(cleanPrice));
            }
        }

        private decimal _lastQty;
        /// <summary>
        /// 成交量（万元）
        ///当ExecType=F时必传，表示本次成交量，单位万元
        /// </summary>
        public decimal lastQty
        {
            get
            {
                return _lastQty;
            }
            set
            {
                _lastQty = value;
                this.OnPropertyChanged(nameof(lastQty));
            }
        }

        private decimal _orderQty;
        /// <summary>
        /// 数量（万元）
        /// "当OrdStatus=0时，表示原始订单数量；
        /// 当ExecType=F时必传，表示订单的原始数量，单位万元"
        /// </summary>
        public decimal orderQty
        {
            get
            {
                return _orderQty;
            }
            set
            {
                _orderQty = value;
                this.OnPropertyChanged(nameof(orderQty));
            }
        }

        private decimal _cumQty;
        /// <summary>
        /// 总成交量（万元）
        ///当ExecType=F时必传，表示本订单目前为止的总成交量，单位 万元
        /// </summary>
        public decimal cumQty
        {
            get
            {
                return _cumQty;
            }
            set
            {
                _cumQty = value;
                this.OnPropertyChanged(nameof(cumQty));
            }
        }

        private decimal _leavesQty;
        /// <summary>
        /// 剩余可成交量（万元）
        ///当ExecType=F时必传，表示本订单剩余可成交量，单位 万元
        /// </summary>
        public decimal leavesQty
        {
            get
            {
                return _leavesQty;
            }
            set
            {
                _leavesQty = value;
                this.OnPropertyChanged(nameof(leavesQty));
            }
        }

        private string _text;
        /// <summary>
        /// 当ExecType=F时必传，表示核单号的值
        /// </summary>
        public string text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                this.OnPropertyChanged(nameof(text));
            }
        }

        private string _firmflag;
        /// <summary>
        /// 意向  Firm"闪电标记" NonFirm 如果为nonfirm，标注初始点价时长
        /// </summary>
        public string firmflag
        {
            get
            {
                return _firmflag;
            }
            set
            {
                _firmflag = value;
                this.OnPropertyChanged(nameof(firmflag));
            }
        }

        private string _venues;
        /// <summary>
        /// 经纪商 **  TPSC,CNEX,CCTB,PTCN,CTTJ
        /// </summary>
        public string venues
        {
            get
            {
                return _venues;
            }
            set
            {
                _venues = value;
                this.OnPropertyChanged(nameof(venues));
            }
        }

        private string _senderCompID;
        /// <summary>
        /// 经纪商代码
        /// 1 = TPSC 国利
        /// 2 = CNEX 国际
        /// 3 = CCTB 中诚
        /// 4 = PTCN 平安
        /// 5 = CTTJ 信唐
        /// </summary>
        public string senderCompID
        {
            get
            {
                return _senderCompID;
            }
            set
            {
                _senderCompID = value;
                this.OnPropertyChanged(nameof(senderCompID));
            }
        }

        private string _senderSubID;
        /// <summary>
        /// 发送方经纪人标识
        /// </summary>
        public string senderSubID
        {
            get
            {
                return _senderSubID;
            }
            set
            {
                _senderSubID = value;
                this.OnPropertyChanged(nameof(senderSubID));
            }
        }

        private string _deliverToCompID;
        /// <summary>
        /// 接收方所在机构标识（SC码）
        /// </summary>
        public string deliverToCompID
        {
            get
            {
                return _deliverToCompID;
            }
            set
            {
                _deliverToCompID = value;
                this.OnPropertyChanged(nameof(deliverToCompID));
            }
        }


        private string _deliverToSubID;
        /// <summary>
        /// 接收方交易员标识（QB账户名） 本方交易员
        /// </summary>
        public string deliverToSubID
        {
            get
            {
                return _deliverToSubID;
            }
            set
            {
                _deliverToSubID = value;
                this.OnPropertyChanged(nameof(deliverToSubID));
            }
        }

        private int _marketIndicator;
        /// <summary>
        /// 市场：4=现券买卖
        /// </summary>
        public int marketIndicator
        {
            get
            {
                return _marketIndicator;
            }
            set
            {
                _marketIndicator = value;
                this.OnPropertyChanged(nameof(marketIndicator));
            }
        }

        private string _sendingTime;
        /// <summary>
        /// 报文发送时间，格式：yyyy-MM-dd HH:mm:ss.SSS
        /// </summary>
        public string sendingTime
        {
            get
            {
                return _sendingTime;
            }
            set
            {
                _sendingTime = value;
                this.OnPropertyChanged(nameof(sendingTime));
            }
        }

        private string _ordType;
        /// <summary>
        /// 订单类型：2=现价订单，Limit
        /// </summary>
        public string ordType
        {
            get
            {
                return _ordType;
            }
            set
            {
                _ordType = value;
                this.OnPropertyChanged(nameof(ordType));
            }
        }

        private string _clOrdID;
        /// <summary>
        /// 客户订单编号
        /// </summary>
        public string clOrdID
        {
            get
            {
                return _clOrdID;
            }
            set
            {
                _clOrdID = value;
                this.OnPropertyChanged(nameof(clOrdID));
            }
        }

        private string _orderID;
        /// <summary>
        /// 订单编号   qtr订单编号
        /// </summary>
        public string orderID
        {
            get
            {
                return _orderID;
            }
            set
            {
                _orderID = value;
                this.OnPropertyChanged(nameof(orderID));
            }
        }
        /// <summary>
        /// 订单执行操作编号
        /// </summary>
        public string _execID { get; set; }
        public string execID
        {
            get
            {
                return _execID;
            }
            set
            {
                _execID = value;
                this.OnPropertyChanged(nameof(execID));
            }
        }

        private string _comment;

        /// <summary>
        /// 备注 
        /// </summary>
        public string comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;
                this.OnPropertyChanged(nameof(comment));
            }
        }

        private string _updatetime;
        /// <summary>
        /// 最后更新时间 transactTime
        /// </summary>
        public string updatetime
        {
            get
            {
                return _updatetime;
            }
            set
            {
                _updatetime = value;
                this.OnPropertyChanged(nameof(updatetime));
            }
        }

        private string _transactTime;
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public string transactTime
        {
            get
            {
                return _transactTime;
            }
            set
            {
                _transactTime = value;
                this.OnPropertyChanged(nameof(transactTime));
            }
        }
    }
}
