using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Position.Plugin.Model
{
    /// <summary>
    ///  持仓核对
    /// </summary>
    public class PositionCheckData : ViewModelBase
    {
        private string _bookid = string.Empty;
        /// <summary>
        /// 组合ID
        /// </summary>
        public string bookid
        {
            get
            {
                return _bookid;
            }
            set
            {
                if (_bookid != value)
                {
                    _bookid = value;
                    this.OnPropertyChanged(nameof(bookid));
                }
            }
        }

        private string _bookname = string.Empty;

        /// <summary>
        /// 组合名称
        /// </summary>
        public string bookname
        {
            get
            {
                return _bookname;
            }
            set
            {
                if (_bookname != value)
                {
                    _bookname = value;
                    this.OnPropertyChanged(nameof(bookname));
                }
            }
        }

        private string _accId = string.Empty;

        /// <summary>
        /// 账户id
        /// </summary>
        public string accId
        {
            get
            {
                return _accId;
            }
            set
            {
                if (_accId != value)
                {
                    _accId = value;
                    this.OnPropertyChanged(nameof(accId));
                }
            }
        }

        private string _execId = string.Empty;
        /// <summary>
        /// STC内成交编号
        /// </summary>
        public string execId
        {
            get
            {
                return _execId;
            }
            set
            {
                if (_execId != value)
                {
                    _execId = value;
                    this.OnPropertyChanged(nameof(execId));
                }
            }
        }

        private string _htexecId = string.Empty;
        /// <summary>
        /// 衡泰交易编号
        /// </summary>
        public string htexecId
        {
            get
            {
                return _htexecId;
            }
            set
            {
                if (_htexecId != value)
                {
                    _htexecId = value;
                    this.OnPropertyChanged(nameof(htexecId));
                }
            }
        }


        private string _venueExecId = string.Empty;
        /// <summary>
        /// 外部成交编号
        /// </summary>
        public string venueExecId
        {
            get
            {
                return _venueExecId;
            }
            set
            {
                if (_venueExecId != value)
                {
                    _venueExecId = value;
                    this.OnPropertyChanged(nameof(venueExecId));
                }
            }
        }

        private string _securityid = string.Empty;
        /// <summary>
        /// 代码
        /// </summary>
        public string securityid
        {
            get
            {
                return _securityid;
            }
            set
            {
                if (_securityid != value)
                {
                    _securityid = value;
                    this.OnPropertyChanged(nameof(securityid));
                }
            }
        }

        private string _symbol = string.Empty;
        /// <summary>
        /// 简称
        /// </summary>
        public string symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    this.OnPropertyChanged(nameof(symbol));
                }
            }
        }


        private string _netPosition;
        /// <summary>
        /// 净持仓
        /// </summary>
        public string NetPosition
        {
            get
            {
                return _netPosition;
            }
            set
            {
                if (_netPosition != value)
                {
                    _netPosition = value;
                    this.OnPropertyChanged(nameof(NetPosition));
                }
            }
        }

        private string _htNetPosition;
        /// <summary>
        /// 衡泰净持仓
        /// </summary>
        public string htNetPosition
        {
            get
            {
                return _htNetPosition;
            }
            set
            {
                if (_htNetPosition != value)
                {
                    _htNetPosition = value;
                    this.OnPropertyChanged(nameof(htNetPosition));
                }
            }
        }




        private string _createtime = string.Empty;
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
                if (_createtime != value)
                {
                    _createtime = value;
                    this.OnPropertyChanged(nameof(createtime));
                }
            }
        }



        private string _updatetime = string.Empty;
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
                if (_updatetime != value)
                {
                    _updatetime = value;
                    this.OnPropertyChanged(nameof(updatetime));
                }
            }
        }



        private string _side = string.Empty;
        /// <summary>
        /// 方向 >0多 <0空
        /// </summary>
        public string side
        {
            get
            {
                return _side;
            }
            set
            {
                if (_side != value)
                {
                    _side = value;
                    this.OnPropertyChanged(nameof(side));
                }
            }
        }

        private string _tempCode = string.Empty;
        /// <summary>
        /// 临时代码
        /// </summary>
        public string tempCode
        {
            get
            {
                return _tempCode;
            }
            set
            {
                if (_tempCode != value)
                {
                    _tempCode = value;
                    this.OnPropertyChanged(nameof(tempCode));
                }
            }
        }


        #region 新增字段
        private string _settlDate;
        /// <summary>
        /// 起息日 = 交割日
        /// </summary>
        public string settlDate
        {
            get
            {
                return _settlDate;
            }
            set
            {
                if (_settlDate != value)
                {
                    _settlDate = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _traderdate;
        /// <summary>
        /// 交易日期
        /// </summary>
        public string traderdate
        {
            get
            {
                return _traderdate;
            }
            set
            {
                if (_traderdate != value)
                {
                    _traderdate = value;
                    this.OnPropertyChanged();
                }
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
                if (_netprice != value)
                {
                    _netprice = value;
                    this.OnPropertyChanged(nameof(netprice));
                }
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
                if (_yield != value)
                {
                    _yield = value;
                    this.OnPropertyChanged(nameof(yield));
                }
            }
        }

        private string _amount;
        /// <summary>
        /// 交易金额
        /// </summary>
        public string amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    this.OnPropertyChanged(nameof(amount));
                }
            }
        }

        private string _settleDate;
        /// <summary>
        /// 清算日期
        /// </summary>
        public string settleDate
        {
            get
            {
                return _settleDate;
            }
            set
            {
                if (_settleDate != value)
                {
                    _settleDate = value;
                    this.OnPropertyChanged(nameof(settleDate));
                }
            }
        }

        private string _checkResult;
        /// <summary>
        /// 核对结果
        /// </summary>
        public string CheckResult
        {
            get
            {
                return _checkResult;
            }
            set
            {
                if (_checkResult != value)
                {
                    _checkResult = value;
                    this.OnPropertyChanged(nameof(CheckResult));
                }
            }
        }

        private Visibility _isEnter;
        /// <summary>
        /// 是否补录
        /// </summary>
        public Visibility IsEnter
        {
            get
            {
                return _isEnter;
            }
            set
            {
                if (_isEnter != value)
                {
                    _isEnter = value;
                    this.OnPropertyChanged(nameof(IsEnter));
                }
            }
        }

        private string _comment;
        /// <summary>
        /// 备注
        /// </summary>
        public string Comment
        {
            get
            {
                return _comment;
            }
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    this.OnPropertyChanged(nameof(Comment));
                }
            }
        }
        #endregion
    }
}
