using STC.Common.Utils;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace STC.Common.Model.Drp
{
    public class DRPListModel : ViewModelBase
    {
        private SolidColorBrush HighLight = new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F));

        private bool isChecked;
        public bool IsChecked { get { return isChecked; } set { isChecked = value; this.OnPropertyChanged(); } }

        private bool enabledBtn;
        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool EnabledBtn 
        { 
            get 
            { 
                return enabledBtn; 
            }
            set 
            { 
                enabledBtn = value;
                PointPriceBrush = enabledBtn ? PointPriceBrush : new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
                if (!string.IsNullOrEmpty(WholeFlag) && WholeFlag.Equals("1"))
                {
                    EnabledEditPointAmountBtn = false;
                }
                else
                {
                    EnabledEditPointAmountBtn = EnabledBtn ? true : false;
                }

                this.OnPropertyChanged(); 
            } 
        }

        private string symbol;
        public string Symbol { get { return symbol; } set { symbol = value; this.OnPropertyChanged(); } }
        private string venueTypeGW;
        public string VenueTypeGW { get { return venueTypeGW; } set { venueTypeGW = value; this.OnPropertyChanged(); } }

        private string venue;
        public string Venue { get { return venue; } set { venue = value; this.OnPropertyChanged(); } }

        private string side;
        /// <summary>
        ///  1 BID BUY   2 Ofr SELL
        /// </summary>
        public string Side
        {
            get
            {
                return side;
            }
            set
            {
                side = value;
                this.OnPropertyChanged();
            }
        }

        private string clearingMethod;
        public string ClearingMethod { get { return clearingMethod; } set { clearingMethod = value; this.OnPropertyChanged(); } }
        private string deliveryType;
        public string DeliveryType { get { return deliveryType; } set { deliveryType = value; this.OnPropertyChanged(); } }
        private string wholeFlag;
        public string WholeFlag 
        { 
            get 
            {
                return wholeFlag; 
            } 
            set 
            { 
                wholeFlag = value;

                if (!string.IsNullOrEmpty(value) && value.Equals("1"))
                {
                    WholeVisibility = Visibility.Visible;
                    EnabledEditPointAmountBtn = false;
                }
                else
                {
                    WholeVisibility = Visibility.Collapsed;
                    EnabledEditPointAmountBtn = EnabledBtn ? true : false;
                }
                this.OnPropertyChanged(); 
            } 
        }

        private string quoteType;
        public string QuoteType { get { return quoteType; } set { quoteType = value; this.OnPropertyChanged(); } }


        private string orderStatus;
        public string OrderStatus { get { return orderStatus; } set { orderStatus = value; this.OnPropertyChanged(); } }

        private string id;
        public string Id { get { return id; } set { id = value; this.OnPropertyChanged(); } }


        private Brush priceBrush;
        public Brush PriceBrush { get { return priceBrush; } set { priceBrush = value; this.OnPropertyChanged(); } }

        private string price;
        public string Price
        {
            get { return price; }
            set
            {
                price = value;
                //设置建议价前景色
                if (!string.IsNullOrEmpty(Price))
                {
                    if ((!IsIRS && Side == "2") || (IsIRS && Side == "1"))
                    {
                        PointPriceBrush = CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A)) : CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0x00, 0xB5, 0x63)) : new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7));

                        if (CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price))
                        {
                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A));
                            AlertSignToolTipText = $"*倒挂";
                            AlertSignVisible = Visibility.Visible;
                        }
                        else if (CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price))
                        {
                            decimal dNew = 0;
                            decimal dClose = 0;
                            if (decimal.TryParse(Price, out dNew) && decimal.TryParse(PointPrice, out dClose) && dClose != 0 && dNew != 0)
                            {
                                decimal bp = (dClose - dNew) * 100;
                                AlertSignToolTipText = bp == 0 ? string.Empty : $"*{CommonUtils.FormatLimitBp(bp)}";
                            }

                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0x00, 0xCD, 0xAD));
                            AlertSignVisible = Visibility.Visible;
                        }
                        else
                        {
                            AlertSignToolTipText = $"";
                            AlertSignVisible = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        PointPriceBrush = CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0x00, 0xB5, 0x63)) : CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A)) : new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7));
                        if (CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price))
                        {
                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A));
                            AlertSignToolTipText = $"*倒挂";
                            AlertSignVisible = Visibility.Visible;
                        }
                        else if (CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price))
                        {
                            decimal dNew = 0;
                            decimal dClose = 0;
                            if (decimal.TryParse(Price, out dNew) && decimal.TryParse(PointPrice, out dClose) && dClose != 0 && dNew != 0)
                            {
                                decimal bp = (dNew - dClose) * 100;
                                AlertSignToolTipText = bp == 0 ? string.Empty : $"*{CommonUtils.FormatLimitBp(bp)}";
                            }

                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0x00, 0xCD, 0xAD));
                            AlertSignVisible = Visibility.Visible;
                        }
                        else
                        {
                            AlertSignToolTipText = $"";
                            AlertSignVisible = Visibility.Hidden;
                        }
                    }
                }

                PointPriceBackGround = EnabledBtn ? CommonUtils.ConvertToDecimal(PointPrice).ToString("F4") == CommonUtils.ConvertToDecimal(OriPrice).ToString("F4") ? new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F)) : HighLight : new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F));
                AlertSignForground = EnabledBtn ? AlertSignForground : new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
                this.OnPropertyChanged();
            }
        }


        private string pointprice;
        public string PointPrice 
        { 
            get { return pointprice; } 
            set 
            { 
                pointprice = value;

                if (CommonUtils.ConvertToDecimal(PointPrice) == CommonUtils.ConvertToDecimal(Price))
                {
                    this.ReplyTooltip = "快速成交";
                }
                else
                {
                    this.ReplyTooltip = "议价";
                }

                //设置建议价前景色
                if (!string.IsNullOrEmpty(Price))
                {
                    if ((!IsIRS && Side == "2") || (IsIRS && Side == "1"))
                    {
                        PointPriceBrush = CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A)) : CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0x00, 0xB5, 0x63)) : new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7));

                        if (CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price))
                        {
                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A));
                            AlertSignToolTipText = $"*倒挂";
                            AlertSignVisible = Visibility.Visible;
                        }
                        else if (CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price))
                        {
                            decimal dNew = 0;
                            decimal dClose = 0;
                            if (decimal.TryParse(Price, out dNew) && decimal.TryParse(PointPrice, out dClose) && dClose != 0 && dNew != 0)
                            {
                                decimal bp = (dClose - dNew) * 100;
                                AlertSignToolTipText = bp == 0 ? string.Empty : $"*{CommonUtils.FormatLimitBp(bp)}";
                            }

                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0x00, 0xCD, 0xAD));
                            AlertSignVisible = Visibility.Visible;
                        }
                        else
                        {
                            AlertSignToolTipText = $"";
                            AlertSignVisible = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        PointPriceBrush = CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0x00, 0xB5, 0x63)) : CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price) ? new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A)) : new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7));
                        if (CommonUtils.ConvertToDecimal(PointPrice) > CommonUtils.ConvertToDecimal(Price))
                        {
                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A));
                            AlertSignToolTipText = $"*倒挂";
                            AlertSignVisible = Visibility.Visible;
                        }
                        else if (CommonUtils.ConvertToDecimal(PointPrice) < CommonUtils.ConvertToDecimal(Price))
                        {
                            decimal dNew = 0;
                            decimal dClose = 0;
                            if (decimal.TryParse(Price, out dNew) && decimal.TryParse(PointPrice, out dClose) && dClose != 0 && dNew != 0)
                            {
                                decimal bp = (dNew - dClose) * 100;
                                AlertSignToolTipText = bp == 0 ? string.Empty : $"*{CommonUtils.FormatLimitBp(bp)}";
                            }

                            AlertSignForground = new SolidColorBrush(Color.FromRgb(0x00, 0xCD, 0xAD));
                            AlertSignVisible = Visibility.Visible;
                        }
                        else
                        {
                            AlertSignToolTipText = $"";
                            AlertSignVisible = Visibility.Hidden;
                        }
                    }
                }

                PointPriceBackGround = EnabledBtn ? CommonUtils.ConvertToDecimal(PointPrice).ToString("F4") == CommonUtils.ConvertToDecimal(OriPrice).ToString("F4") ? new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F)) : HighLight : new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F));
                AlertSignForground = EnabledBtn ? AlertSignForground : new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
                this.OnPropertyChanged(); 
            } 
        }


        private Brush pointpriceBrush;
        /// <summary>
        /// 建议价前景色
        /// </summary>
        public Brush PointPriceBrush 
        { 
            get
            { 
                return pointpriceBrush; 
            } 
            set 
            { 
                pointpriceBrush = value;
                this.OnPropertyChanged();
            } 
        }


        private string firmType;
        public string FirmType { get { return firmType; } set { firmType = value; this.OnPropertyChanged(); } }

        private string amount;
        public string Amount
        {
            get { return amount; }
            set
            {
                amount = value;
                this.OnPropertyChanged();
            }
        }

        private string pointAmount;
        public string PointAmount
        {
            get
            {
                return pointAmount;
            }
            set
            {
                pointAmount = value;
                PointAmountBackGround = EnabledBtn ? CommonUtils.ConvertToDecimal(pointAmount).ToString("F4") == CommonUtils.ConvertToDecimal(OriAmount).ToString("F4") ? new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F)) : HighLight : new SolidColorBrush(Color.FromRgb(0x1F, 0x1F, 0x1F));
                AlertsignPointAmountforground = EnabledBtn ? CommonUtils.ConvertToDecimal(pointAmount) <= CommonUtils.ConvertToDecimal(Amount) ? new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7)) : new SolidColorBrush(Color.FromRgb(0xFF, 0x33, 0x3A)) : new SolidColorBrush(Color.FromRgb(0xD7, 0xD7, 0xD7));
                this.OnPropertyChanged(); 
            }
        }

        private string source;
        public string Source { get { return source; } set { source = value; this.OnPropertyChanged(); } }

        private string dealSettle;
        public string DealSettle { get { return dealSettle; } set { dealSettle = value; this.OnPropertyChanged(); } }

        private string settle;
        public string Settle 
        {
            get 
            { 
                return settle; 
            } 
            set 
            { 
                settle = value;

                if (string.IsNullOrEmpty(SettlType_yuan))
                {
                    PointSettleBackGround = new SolidColorBrush(Color.FromRgb(0x6E, 0x20, 0x22));
                }
                else
                {
                    PointSettleBackGround = new SolidColorBrush(Color.FromRgb(0x13, 0x6C, 0x5E));
                }
       

                this.OnPropertyChanged(); 
            } 
        }



        private Visibility _wholeVisibility = Visibility.Collapsed;
        public Visibility WholeVisibility
        {
            get
            {
                return _wholeVisibility;
            }

            set
            {
                _wholeVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private string netPrice;
        public string NetPrice { get { return netPrice; } set { netPrice = value; this.OnPropertyChanged(); } }

        private string clordid;
        public string ClOrdID { get { return clordid; } set { clordid = value; this.OnPropertyChanged(); } }

        private string securityid;
        public string SecurityID { get { return securityid; } set { securityid = value; this.OnPropertyChanged(); } }

        private string symbolType;
        public string SymbolType { get { return symbolType; } set { symbolType = value; this.OnPropertyChanged(); } }
        

        private string settlType;
        public string SettlType { get { return settlType; } set { settlType = value; this.OnPropertyChanged(); } }

        private string settltype_yuan;
        public string SettlType_yuan { get { return settltype_yuan; } set { settltype_yuan = value; this.OnPropertyChanged(); } }

        private string securitytype;
        public string SecurityType { get { return securitytype; } set { securitytype = value; this.OnPropertyChanged(); } }

        private string timeInForce;
        public string TimeInForce { get { return timeInForce; } set { timeInForce = value; this.OnPropertyChanged(); } }

        private string marketIndicator;
        public string MarketIndicator { get { return marketIndicator; } set { marketIndicator = value; this.OnPropertyChanged(); } }

        private string firmflag;
        public string FirmFlag { get { return firmflag; } set { firmflag = value; this.OnPropertyChanged(); } }


        private string userid;
        public string UserID { get { return userid; } set { userid = value; this.OnPropertyChanged(); } }

        private string name;
        public string Name { get { return name; } set { name = value; this.OnPropertyChanged(); } }

        private string username;
        public string UserName { get { return username; } set { username = value; this.OnPropertyChanged(); } }
        

        private string iocTimeOut;
        public string IocTimeOut { get { return iocTimeOut; } set { iocTimeOut = value; this.OnPropertyChanged(); } }

        private string validuntiltime;
        public string ValidUntilTime { get { return validuntiltime; } set { validuntiltime = value; this.OnPropertyChanged(); } }

        private string remainseconds;

        /// <summary>
        /// 显示用的倒计时
        /// </summary>
        public string RemainSeconds { get { return remainseconds; } set { remainseconds = value; this.OnPropertyChanged(); } }
        

        private string quoteid;
        public string QuoteID { get { return quoteid; } set { quoteid = value; this.OnPropertyChanged(); } }


        private string orderId;
        public string OrderId { get { return orderId; } set { orderId = value; this.OnPropertyChanged(); } }


        private string info4;
        public string Info4 { get { return info4; } set { info4 = value; this.OnPropertyChanged(); } }

        private Brush foregroundbrush = Brushes.White;
        public Brush ForegroundBrush { get { return foregroundbrush; } set { foregroundbrush = value; this.OnPropertyChanged(); } }


        private string replytooltip;
        public string ReplyTooltip { get { return replytooltip; } set { replytooltip = value; this.OnPropertyChanged(); } }

        private string transacttime;
        public string TransactTime { get { return transacttime; } set { transacttime = value; this.OnPropertyChanged(); } }


        private Brush pointpricebackground;
        /// <summary>
        /// 建议价背景色
        /// </summary>
        public Brush PointPriceBackGround
        {
            get
            {
                return pointpricebackground;
            }
            set
            {
                pointpricebackground = value;
                this.OnPropertyChanged();
            }
        }

        private string oriprice;
        public string OriPrice
        {
            get { return oriprice; }
            set
            {
                oriprice = value;
                PointPriceBackGround = EnabledBtn ? CommonUtils.ConvertToDecimal(PointPrice).ToString("F4") == CommonUtils.ConvertToDecimal(OriPrice).ToString("F4") ? new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F)) : HighLight : new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F));
                this.OnPropertyChanged();
            }
        }


        private Brush pointamountbackground;
        /// <summary>
        /// 建议量背景色
        /// </summary>
        public Brush PointAmountBackGround
        {
            get
            {
                return pointamountbackground;
            }
            set
            {
                pointamountbackground = value;
                this.OnPropertyChanged();
            }
        }

        private string oriamount;
        public string OriAmount
        {
            get { return oriamount; }
            set
            {
                oriamount = value;
                PointAmountBackGround = EnabledBtn ? CommonUtils.ConvertToDecimal(PointAmount).ToString("F4") == CommonUtils.ConvertToDecimal(OriAmount).ToString("F4") ? new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F)) : HighLight : new SolidColorBrush(Color.FromRgb(0x0F, 0x0F, 0x0F));
                this.OnPropertyChanged();
            }
        }

        private string orisettle;
        public string OriSettle
        {
            get { return orisettle; }
            set
            {
                orisettle = value;

                if (string.IsNullOrEmpty(SettlType_yuan))
                {
                    PointSettleBackGround = Settle == OriSettle ? new SolidColorBrush(Color.FromRgb(0x6E, 0x20, 0x22)) : HighLight;
                }
                else
                {
                    PointSettleBackGround = Settle == OriSettle ? new SolidColorBrush(Color.FromRgb(0x13, 0x6C, 0x5E)) : HighLight;
                }

                this.OnPropertyChanged();
            }
        }

        private Brush pointsettlebackground;
        /// <summary>
        /// 清算速度背景色  #6E2022
        /// </summary>
        public Brush PointSettleBackGround
        {
            get
            {
                return pointsettlebackground;
            }
            set
            {
                pointsettlebackground = value;
                this.OnPropertyChanged();
            }
        }

        private string oriSide;
        /// <summary>
        /// 母单的方向
        /// </summary>
        public string OriSide
        {
            get { return oriSide; }
            set
            {
                oriSide = value;
                this.OnPropertyChanged();
            }
        }


        private Brush alertsignforground;
        public Brush AlertSignForground
        {
            get { return alertsignforground; }
            set
            {
                alertsignforground = value;
                this.OnPropertyChanged();
            }
        }

        private Brush alertsignpointamountforground;
        /// <summary>
        /// 建议量>回价量 警示颜色
        /// </summary>
        public Brush AlertsignPointAmountforground
        {
            get { return alertsignpointamountforground; }
            set
            {
                alertsignpointamountforground = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility alertSignVisible = Visibility.Collapsed;
        public Visibility AlertSignVisible
        {
            get { return alertSignVisible; }
            set
            {
                alertSignVisible = value;
                this.OnPropertyChanged("AlertSignVisible");
            }
        }

        private string alertSignToolTipText = "";
        public string AlertSignToolTipText
        {
            get { return alertSignToolTipText; }
            set
            {
                alertSignToolTipText = value;
                this.OnPropertyChanged("AlertSignToolTipText");
            }
        }


        private bool enablededitpointamountbtn;
        /// <summary>
        /// 是否允许操作
        /// </summary>
        public bool EnabledEditPointAmountBtn
        {
            get
            {
                return enablededitpointamountbtn;
            }
            set
            {
                enablededitpointamountbtn = value;
                this.OnPropertyChanged();
            }
        }



        private bool _ChangeOrderPrice;
        /// <summary>
        /// 是否更新报价
        /// </summary>
        public bool ChangeOrderPrice
        {
            get
            {
                return _ChangeOrderPrice;
            }

            set
            {
                _ChangeOrderPrice = value;
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

        private bool isIRS = false;
        /// <summary>
        /// 是否是IRS
        /// </summary>
        public bool IsIRS
        {
            get
            {
                return isIRS;
            }
            set
            {
                isIRS = value;
                this.OnPropertyChanged();
            }
        }
    }
}
