using STC.Common.Handler.Position;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace STC.Common.Model.Position
{
    public class PositionManualPo : ViewModelBase
    {
        private string _SecurityId;
        public string SecurityId
        {
            get
            {
                return _SecurityId;
            }
            set
            {
                string old = _SecurityId;
                _SecurityId = value;
                BondsInfoPo info = CommonUtils.GetBondsInfoPoBySecurityID(_SecurityId);
                if (info != null)
                {
                    if (!string.IsNullOrEmpty(info.Option_Type) &&
                        ((!info.Option_Type.Equals("NON") && info.Option_Date != null) ||
                        (info.Option_Type.Equals("ETS") && info.Option_Date == null)))
                    {
                        IsExeBond = "1";//未行权含权债
                    }
                    else
                    {
                        IsExeBond = "0";
                    }
                    if (Consts.MARKETINDICATOR_FUTURE.Equals(info.MarketIndicator))
                    {
                        MarketIndicator = Consts.MARKETINDICATOR_FUTURE;
                    }
                    else
                    {
                        MarketIndicator = Consts.MARKETINDICATOR_BOND;
                    }
                }
                this.OnPropertyChanged(nameof(SecurityId));

                if (isCalStatus)
                    return;
                if (!string.IsNullOrEmpty(old) && !old.Equals(_SecurityId) && !string.IsNullOrEmpty(NetPrice))
                {
                    isCalStatus = true;
                    string _netprice = NetPrice;
                    bool _isExeBond = "1".Equals(IsExeBond) ? true : false;
                    CalcPrice(_netprice, _isExeBond, 2);
                }
            }
        }
        private string _Side;
        public string Side
        {
            get
            {
                return _Side;
            }
            set
            {
                _Side = value;
                this.OnPropertyChanged(nameof(Side));
            }
        }
        private List<KeyValuePair<string, string>> _SideItem;
        public List<KeyValuePair<string, string>> SideItem
        {
            get
            {
                return _SideItem;
            }
            set
            {
                _SideItem = value;
                this.OnPropertyChanged(nameof(SideItem));
            }
        }
        private string _OpenClose;
        public string OpenClose
        {
            get
            {
                return _OpenClose;
            }
            set
            {
                _OpenClose = value;
                this.OnPropertyChanged(nameof(OpenClose));
            }
        }
        private string _Position;
        public string Position
        {
            get
            {
                return _Position;
            }
            set
            {
                _Position = value;
                this.OnPropertyChanged(nameof(Position));
            }
        }
        private bool isCalStatus = false;
        private string _Price;
        public string Price
        {
            get
            {
                return _Price;
            }
            set
            {
                string old = _Price;
                _Price = value;
                this.OnPropertyChanged(nameof(Price));
                if (isCalStatus)
                    return;
                if (string.IsNullOrEmpty(old) || (!string.IsNullOrEmpty(old) && !old.Equals(_Price)))
                {
                    isCalStatus = true;
                    bool _isExeBond = "1".Equals(IsExeBond) ? true : false;
                    CalcPrice(_Price, _isExeBond, 4);
                }
            }
        }
        private string _NetPrice;
        public string NetPrice
        {
            get
            {
                return _NetPrice;
            }
            set
            {
                string old = _NetPrice;
                _NetPrice = value;
                this.OnPropertyChanged(nameof(NetPrice));
                if (isCalStatus)
                    return;
                if (string.IsNullOrEmpty(old) || !string.IsNullOrEmpty(old) && !old.Equals(_NetPrice))
                {
                    isCalStatus = true;
                    bool _isExeBond = "1".Equals(IsExeBond) ? true : false;
                    CalcPrice(_NetPrice, _isExeBond, 2);
                }
            }
        }
        private string _Venuetypegw;
        public string Venuetypegw
        {
            get
            {
                return _Venuetypegw;
            }
            set
            {
                _Venuetypegw = value;
                this.OnPropertyChanged(nameof(Venuetypegw));
            }
        }
        private string _TradeDate;
        public string TradeDate
        {
            get
            {
                return _TradeDate;
            }
            set
            {
                _TradeDate = value;
                this.OnPropertyChanged(nameof(TradeDate));
            }
        }
        private DateTime _CurrentDate;
        public DateTime CurrentDate
        {
            get
            {
                return _CurrentDate;
            }
            set
            {
                _CurrentDate = value;
                this.OnPropertyChanged(nameof(CurrentDate));
            }
        }
        private string _SettleType;
        public string SettleType
        {
            get
            {
                return _SettleType;
            }
            set
            {
                string old = _SettleType;
                _SettleType = value;
                this.OnPropertyChanged(nameof(SettleType));

                if (isCalStatus)
                    return;
                if (string.IsNullOrEmpty(old) || !string.IsNullOrEmpty(old) && !old.Equals(_SettleType))
                {
                    isCalStatus = true;
                    string _netprice = NetPrice;
                    bool _isExeBond = "1".Equals(IsExeBond) ? true : false;
                    CalcPrice(_netprice, _isExeBond, 2);
                }
            }
        }
        private string _CounterPartyName;
        public string CounterPartyName
        {
            get
            {
                return _CounterPartyName;
            }
            set
            {
                _CounterPartyName = value;
                this.OnPropertyChanged(nameof(CounterPartyName));
            }
        }
        private string _CounterPartySubName;
        public string CounterPartySubName
        {
            get
            {
                return _CounterPartySubName;
            }
            set
            {
                _CounterPartySubName = value;
                this.OnPropertyChanged(nameof(CounterPartySubName));
            }
        }
        private string _BookId;
        public string BookId
        {
            get
            {
                return _BookId;
            }
            set
            {
                _BookId = value;
                this.OnPropertyChanged(nameof(BookId));
            }
        }
        private string _BookName;
        public string BookName
        {
            get
            {
                return _BookName;
            }
            set
            {
                _BookName = value;
                this.OnPropertyChanged(nameof(BookName));
            }
        }
        private string _Remark;
        public string Remark
        {
            get
            {
                return _Remark;
            }
            set
            {
                _Remark = value;
                this.OnPropertyChanged(nameof(Remark));
            }
        }
        private string _EditBtnVisible;
        public string EditBtnVisible
        {
            get
            {
                return _EditBtnVisible;
            }
            set
            {
                _EditBtnVisible = value;
                this.OnPropertyChanged(nameof(EditBtnVisible));
            }
        }
        private string _SaveBtnVisible;
        public string SaveBtnVisible
        {
            get
            {
                return _SaveBtnVisible;
            }
            set
            {
                _SaveBtnVisible = value;
                this.OnPropertyChanged(nameof(SaveBtnVisible));
            }
        }
        private string _IsAdded;
        public string IsAdded
        {
            get
            {
                return _IsAdded;
            }
            set
            {
                _IsAdded = value;
                this.OnPropertyChanged(nameof(IsAdded));
            }
        }
        private string _Msg;
        public string Msg
        {
            get
            {
                return _Msg;
            }
            set
            {
                _Msg = value;
                this.OnPropertyChanged(nameof(Msg));
            }
        }
        private string _MarketIndicator;
        public string MarketIndicator
        {
            get
            {
                return _MarketIndicator;
            }
            set
            {
                _MarketIndicator = value;
                this.OnPropertyChanged(nameof(MarketIndicator));
            }
        }
        private string _IsExeBond;
        public string IsExeBond
        {
            get
            {
                return _IsExeBond;
            }
            set
            {
                _IsExeBond = value;
                this.OnPropertyChanged(nameof(IsExeBond));
            }
        }
        Regex regex = new Regex("^[0-9.-]+$");
        /// <summary>
        /// 界面自动计算收益率和净价
        /// </summary>
        /// <param name="_price"></param>
        /// <param name="calType">4 计算净价 2 计算收益率</param>
        public void CalcPrice(string _inprice, bool _isExeBond, int calType = 2)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string securityid = SecurityId as string;
                if (string.IsNullOrWhiteSpace(securityid))
                {
                    isCalStatus = false;
                    return;
                }

                string settletype = SettleType as string;
                if (string.IsNullOrWhiteSpace(settletype))
                {
                    isCalStatus = false;
                    return;
                }

                decimal d = 0;
                if (!string.IsNullOrWhiteSpace(_inprice))
                {
                    if (!(decimal.TryParse(_inprice, out d) && (regex.IsMatch(_inprice) || d == 0)))
                    {
                        isCalStatus = false;
                        return;
                    }
                }
                // 根据净价计算成交价
                if (calType == 2)
                {
                    //计算成交价
                    string _price = PositionHandler.Instance.CalBond(securityid, "2", settletype, _inprice, _isExeBond);
                    if (!string.IsNullOrEmpty(_price))
                        Price = _price;
                }

                if (calType == 4)
                {
                    //计算净价
                    string _netprice = PositionHandler.Instance.CalBond(securityid, _isExeBond ? "7" : "4", settletype, PositionHandler.Instance.NumChange(_inprice), _isExeBond);
                    if (!string.IsNullOrEmpty(_netprice))
                        NetPrice = _netprice;
                }

                Msg = string.Empty;
                isCalStatus = false;
            }));
        }
    }
}
