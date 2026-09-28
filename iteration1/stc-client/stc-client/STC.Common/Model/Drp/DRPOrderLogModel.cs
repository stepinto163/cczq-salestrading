using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Model.Drp
{
    public class DRPOrderLogModel : ViewModelBase
    {
        private string _id;
        public string ID 
        {
            get
            {
                return _id;
            }

            set
            {
                _id = value;
                this.OnPropertyChanged();
            }
        }

        private string _actiontype;
        /// <summary>
        /// 操作类型 1-议价，2-成交，3-拒绝（忽略），4-撤单(询价方)，5-撤单(报价方)，6-撤单(超时)，7-报价
        /// </summary>
        public string actiontype 
        {
            get
            {
                return _actiontype;
            }

            set
            {
                _actiontype = value;
                this.OnPropertyChanged();
            }
        }

        private string _actionuser;
        /// <summary>
        /// 操作人
        /// </summary>
        public string actionuser 
        {
            get
            {
                return _actionuser;
            }

            set
            {
                _actionuser = value;
                this.OnPropertyChanged();
            }
        }

        private string _bargainprice;
        /// <summary>
        /// 讨论价格
        /// </summary>
        public string bargainprice
        {
            get
            {
                return _bargainprice;
            }

            set
            {
                _bargainprice = value;
                this.OnPropertyChanged();
            }
        }

        private string _bargainqty;
        /// <summary>
        /// 讨论报量
        /// </summary>
        public string bargainqty 
        {
            get
            {
                return _bargainqty;
            }

            set
            {
                _bargainqty = value;
                this.OnPropertyChanged();
            }
        }

        private string _counterpartyname;
        /// <summary>
        /// 对手方
        /// </summary>
        public string counterpartyname
        {
            get
            {
                return _counterpartyname;
            }

            set
            {
                _counterpartyname = value;
                this.OnPropertyChanged();
            }
        }

        private string _createtime;
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime 
        {
            get
            {
                return _createtime;
            }

            set
            {
                _createtime = value;
                this.OnPropertyChanged();
            }
        }

        private string _lastpx;
        /// <summary>
        /// 成交价格
        /// </summary>
        public string lastpx 
        {
            get
            {
                return _lastpx;
            }

            set
            {
                _lastpx = value;
                this.OnPropertyChanged();
            }
        }

        private string _lastqty;
        /// <summary>
        /// 成交量
        /// </summary>
        public string lastqty 
        {
            get
            {
                return _lastqty;
            }

            set
            {
                _lastqty = value;
                this.OnPropertyChanged();
            }
        }

        private string _netprice;
        /// <summary>
        /// 净价
        /// </summary>
        public string netprice 
        {
            get
            {
                return _netprice;
            }

            set
            {
                _netprice = value;
                this.OnPropertyChanged();
            }
        }

        private string _orderstatus;
        /// <summary>
        /// 交易状态
        /// </summary>
        public string orderstatus 
        {
            get
            {
                return _orderstatus;
            }

            set
            {
                _orderstatus = value;
                this.OnPropertyChanged();
            }
        }

        private string _ordertype;
        /// <summary>
        /// 订单类型(1-母单，2-子单)
        /// </summary>
        public string ordertype 
        {
            get
            {
                return _ordertype;
            }

            set
            {
                _ordertype = value;
                this.OnPropertyChanged();
            }
        }

        private string _qty;
        /// <summary>
        /// 报量
        /// </summary>
        public string qty 
        {
            get
            {
                return _qty;
            }

            set
            {
                _qty = value;
                this.OnPropertyChanged();
            }
        }

        private string _quoteid;
        /// <summary>
        /// 报价编号
        /// </summary>
        public string quoteid 
        {
            get
            {
                return _quoteid;
            }

            set
            {
                _quoteid = value;
                this.OnPropertyChanged();
            }
        }

        private string _strategyorderid;
        /// <summary>
        /// 策略订单id
        /// </summary>
        public string strategyorderid 
        {
            get
            {
                return _strategyorderid;
            }

            set
            {
                _strategyorderid = value;
                this.OnPropertyChanged();
            }
        }

        private string _updatetime;
        /// <summary>
        /// 更新时间
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
                this.OnPropertyChanged();
            }
        }

        private string _yield;
        /// <summary>
        /// 收益率
        /// </summary>
        public string yield 
        {
            get
            {
                return _yield;
            }

            set
            {
                _yield = value;
                this.OnPropertyChanged();
            }
        }


        private string _side;
        /// <summary>
        /// 方向
        /// </summary>
        public string side
        {
            get
            {
                return _side;
            }

            set
            {
                _side = value;
                this.OnPropertyChanged();
            }
        }

        private string _showPrice;
        public string ShowPrice 
        {
            get
            {
                return _showPrice;
            }

            set
            {
                _showPrice = value;
                this.OnPropertyChanged();
            }
        }

        private string _showside;
        public string ShowSide
        {
            get
            {
                return _showside;
            }

            set
            {
                _showside = value;
                this.OnPropertyChanged();
            }
        }

        private string _showamount;
        /// <summary>
        /// 已确认量
        /// </summary>
        public string ShowAmount
        {
            get
            {
                return _showamount;
            }

            set
            {
                _showamount = value;
                this.OnPropertyChanged();
            }
        }



        private string msgCode;
        /// <summary>
        /// 报价状态
        /// </summary>
        public string MsgCode
        {
            get
            {
                return msgCode;
            }
            set
            {
                msgCode = value;
                this.OnPropertyChanged();
            }
        }

        private string cmsgCode;
        /// <summary>
        /// 报价状态
        /// </summary>
        public string CMsgCode
        {
            get
            {
                return cmsgCode;
            }
            set
            {
                cmsgCode = value;
                this.OnPropertyChanged();
            }
        }

    }
}
