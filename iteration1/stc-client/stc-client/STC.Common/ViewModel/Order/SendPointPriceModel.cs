using Ats.Foundation.Message;
using Ats.Foundation.Utils.Seqence;
using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Cal;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.DataProcesser;
using STC.Common.View;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using STC.Common.ViewModel.Adapter;
using STC.Common.Data;
using DataTimeSequence = STC.Common.Utils.DataTimeSequence;
using System.Collections.ObjectModel;
using STC.Common.Model.Admin;
using System.Text.RegularExpressions;
using STC.Common.View.Order.InnerView;
using STC.Common.ViewModel.Order.InnerView;
using STC.Common.ViewModel.Drp.Utils;
using System.Data;
using STC.Common.Model.Drp;
using System.Diagnostics.Eventing.Reader;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;
using STC.Common.View.Component;

namespace STC.Common.ViewModel.Order
{
    public class SendPointPriceModel : BaseViewModel
    {
        public static string InSecurity = "";
        private readonly string MODULEID = "1002";
        private readonly string CHECK_ALL_ACTIONID = "100203";
        private readonly string CHECK_INNER_ACTIONID = "100201";
        private readonly string CHECK_INNER_XBOND_ACTIONID = "100202";

        private readonly string XBOND_TYPE = "";//">> X-bond报价";
        private readonly string NORMAL_TYPE = ">> 普通报价";
        private readonly string ADD_ONE_NOT_LIMIT = "2,3";
        private SendPriceModelAdapter _sendPriceAdapter = null;
        private bool _isXBond = false;

        private bool _isFirm = false;
        public Window myWindow;
        private bool isBid = true;
        private const string YYYYMMDD = "yyyyMMdd";

        private string _currentSource = string.Empty;
        private string _currentVenueTypeGW = string.Empty;
        private string _currentId = string.Empty;
        private string _orderid = string.Empty;
        private string _partyId = string.Empty;
        private string _partyName = string.Empty;

        public string OrderId
        {
            get
            {
                return _orderid;
            }

            set
            {
                _orderid = value;
                this.OnPropertyChanged();
            }
        }
        private string _trsactTime = string.Empty;
        private string _deliveryType = string.Empty;
        private string _clearingMethod = string.Empty;
        private string _quoteType = string.Empty;
        private BondsInfoPo _bondsInfoPo = null;
        private STC.Common.Model.Fix.Message.OrderCancelRequest Ocr;
        public ICommand ClickBestBidCommand { get; set; }
        public ICommand ClickBestOfrCommand { get; set; }

        private ManualResetEvent manualevent = new ManualResetEvent(false);
        public SendPointPriceModel()
        {
            ClickBestBidCommand = new ComRelayCommand(OnClickBestBidCommand);
            ClickBestOfrCommand = new ComRelayCommand(OnClickBestOfrCommand);
            IsShowDRP = LocalEnv.ConfInfo.IsShowDRP == "1" ? Visibility.Visible : Visibility.Collapsed;
            IsShowNormal = LocalEnv.ConfInfo.IsShowDRP == "0" ? Visibility.Visible : Visibility.Collapsed;
            PendingCancelTime = int.Parse(LocalEnv.ConfInfo.PendingCancelTime);
        }

        #region 界面属性
        /// <summary>
        /// 交易方向
        /// </summary>
        private string titlebidOrOfr = string.Empty;
        public string TitleBidOrOfr
        {
            get { return titlebidOrOfr; }
            set
            {
                titlebidOrOfr = value;
                this.OnPropertyChanged("TitleBidOrOfr");
            }
        }

        /// <summary>
        /// Title交易方向
        /// </summary>
        private string bidOrOfr = string.Empty;
        public string BidOrOfr
        {
            get { return bidOrOfr; }
            set
            {
                bidOrOfr = value;
                this.OnPropertyChanged("BidOrOfr");
            }
        }

        /// <summary>
        /// Title交易方向 小写
        /// </summary>
        private string bidOrOfrLower = string.Empty;
        public string BidOrOfrLower
        {
            get { return bidOrOfrLower; }
            set
            {
                bidOrOfrLower = value;
                this.OnPropertyChanged("BidOrOfrLower");
            }
        }



        /// <summary>
        /// Title交易方向
        /// </summary>
        private string mybidOrOfr = string.Empty;
        public string MyBidOrOfr
        {
            get { return mybidOrOfr; }
            set
            {
                mybidOrOfr = value;
                this.OnPropertyChanged();
            }
        }
        /// <summary>
        /// 债券代码
        /// </summary>
        private string security = string.Empty;
        public string Security
        {
            get { return security; }
            set
            {
                security = value;
                this.OnPropertyChanged("Security");
            }
        }

        /// <summary>
        /// 债券简称
        /// </summary>
        private string chssecurity = string.Empty;
        public string ChsSecurity
        {
            get { return chssecurity; }
            set
            {
                chssecurity = value;
                this.OnPropertyChanged("ChsSecurity");
            }
        }

        public bool IsBid
        {
            get { return isBid; }
            set
            {
                isBid = value;

                if (LocalEnv.ConfInfo.DefaultSkinStyle == "DefaultSkin")
                {
                    ConfirmButtonTag ="1";
                }
                else
                {
                    ConfirmButtonTag = this.IsBid ? "2" : "3";
                }
                this.OnPropertyChanged("BidOrOfr");
            }
        }

        /// <summary>
        /// 渠道名称
        /// </summary>
        private string venues = string.Empty;
        public string Venues
        {
            get
            {
                return venues;
            }
            set
            {
                venues = value;
                this.OnPropertyChanged("Venues");
            }
        }

        /// <summary>
        /// 渠道名称 代码
        /// </summary>
        private string sourceTrade = string.Empty;
        public string SourceTrade
        {
            get
            {
                return sourceTrade;
            }
            set
            {
                sourceTrade = value;
                this.OnPropertyChanged("SourceTrade");
            }
        }

        /// <summary>
        /// 订单Tip信息
        /// </summary>
        private string orderinfotip = string.Empty;
        public string OrderInfoTip
        {
            get { return orderinfotip; }
            set
            {
                orderinfotip = value;
                if (orderinfotip.Length >= 36)
                {
                    OrderInfoTipLimit = orderinfotip.Substring(0, 36) + "...";
                }
                else
                {
                    OrderInfoTipLimit = OrderInfoTip;
                }
                this.OnPropertyChanged(nameof(OrderInfoTip));
            }
        }

        /// <summary>
        /// 截断的订单Tip信息
        /// </summary>
        private string orderinfotipLimit = string.Empty;
        public string OrderInfoTipLimit
        {
            get { return orderinfotipLimit; }
            set
            {
                orderinfotipLimit = value;
                this.OnPropertyChanged(nameof(OrderInfoTipLimit));
            }
        }

        /// <summary>
        /// Settle
        /// </summary>
        private string settle2 = "2,3";
        public string Settle
        {
            get { return settle2; }
            set
            {
                settle2 = value;
                this.OnPropertyChanged(nameof(Settle));
            }
        }

        /// <summary>
        /// 输入价格
        /// </summary>
        private string clickPrice = string.Empty;
        public string ClickPrice
        {
            get { return clickPrice; }
            set
            {
                clickPrice = value;
                this.OnPropertyChanged(nameof(ClickPrice));
            }
        }

        /// <summary>
        /// 输入净价价格
        /// </summary>
        private string clickNetPrice = string.Empty;
        public string ClickNetPrice
        {
            get { return clickNetPrice; }
            set
            {
                clickNetPrice = value;
                this.OnPropertyChanged(nameof(ClickNetPrice));
            }
        }

        /// <summary>
        /// 输入价格是否可以见
        /// </summary>
        private Visibility clickpricevisible = Visibility.Visible;
        public Visibility ClickPriceVisible
        {
            get { return clickpricevisible; }
            set
            {
                clickpricevisible = value;
                this.OnPropertyChanged(nameof(ClickPriceVisible));
            }
        }

        /// <summary>
        /// 下单客户端ID
        /// </summary>
        private string clOrdID = string.Empty;
        public string ClOrdID
        {
            get { return clOrdID; }
            set
            {
                clOrdID = value;
                this.OnPropertyChanged(nameof(ClOrdID));
            }
        }

        /// <summary>
        /// broker最优价
        /// </summary>
        private string bestPrice = string.Empty;
        public string BestPrice
        {
            get { return bestPrice; }
            set
            {
                bestPrice = value;
                this.OnPropertyChanged(nameof(BestPrice));
            }
        }
        /// <summary>
        /// 意向价格是否有效
        /// </summary>
        private bool pricevalid = true;
        public bool PriceValid
        {
            get { return pricevalid; }
            set
            {
                pricevalid = value;
                this.ChsPriceValid = value ? "有效" : "无效";
                this.OnPropertyChanged(nameof(PriceValid));
                if (!pricevalid)
                {
                    SendPriceEnable = false;
                }
            }
        }

        private bool sendpriceenable = true;
        public bool SendPriceEnable
        {
            get { return sendpriceenable; }
            set
            {
                sendpriceenable = value;
                this.OnPropertyChanged(nameof(SendPriceEnable));
            }
        }


        private string chspricevalid = string.Empty;
        public string ChsPriceValid
        {
            get
            {
                return chspricevalid;
            }
            set
            {
                chspricevalid = value;
                this.OnPropertyChanged(nameof(ChsPriceValid));
            }
        }

        private string _wholeflag = "0";
        public string WholeFlag
        {
            get
            {
                return _wholeflag;
            }
            set
            {
                _wholeflag = value;
                this.OnPropertyChanged(nameof(WholeFlag));
            }
        }

        private Visibility _addOneVisibility = Visibility.Visible;
        public Visibility AddOneVisibility
        {
            get { return _addOneVisibility; }
            set
            {
                _addOneVisibility = value;

                this.OnPropertyChanged();
            }
        }


        private bool _addOneNotLimitCheck = true;
        public bool AddOneNotLimitCheck
        {
            get
            {
                return _addOneNotLimitCheck;
            }

            set
            {
                _addOneNotLimitCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneTodayCheck = _addOneTomorrowCheck = false;
                }
            }
        }

        private bool _addOneTodayCheck = false;
        public bool AddOneTodayCheck
        {
            get
            {
                return _addOneTodayCheck;
            }

            set
            {
                _addOneTodayCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTomorrowCheck = false;
                }
            }
        }

        private bool _addOneTomorrowCheck = false;
        public bool AddOneTomorrowCheck
        {
            get
            {
                return _addOneTomorrowCheck;
            }

            set
            {
                _addOneTomorrowCheck = value;

                this.OnPropertyChanged();

                if (value)
                {
                    _addOneNotLimitCheck = _addOneTodayCheck = false;
                }
            }
        }

        /// <summary>
        /// Amount
        /// </summary>
        private string _AmountText = "";
        public string AmountText
        {
            get { return _AmountText; }
            set
            {
                if (value != _AmountText)
                {
                    _AmountText = value;
                    this.OnPropertyChanged(nameof(AmountText));
                }
            }
        }

        private bool _SettleIsDropDownOpen = false;
        public bool SettleIsDropDownOpen
        {
            get
            {
                return _SettleIsDropDownOpen;
            }

            set
            {
                _SettleIsDropDownOpen = value;

                NormalSpotIndex = -1;
                NormalAddOneOpenedTag = "1";

                DrpSpotIndex = -1;
                DrpAddOneOpenedTag = "1";
                this.OnPropertyChanged();

            }
        }

        private SendPriceItem sendPriceItem = null;

        public SendPriceItem SendPriceItem
        {
            get { return sendPriceItem; }
            set
            {
                if (value != null)
                {
                    sendPriceItem = value;
                    this.OnPropertyChanged(nameof(SendPriceItem));
                    UpdateUiBestBidOrOfrForground();
                }
            }
        }

        private string _iocOutTime = string.Empty;
        public string IocOutTime
        {
            get
            {
                return _iocOutTime;
            }

            set
            {
                _iocOutTime = value;
                this.ParseSecond(value);
                this.OnPropertyChanged(nameof(IocOutTime));
            }
        }

        private decimal _lockTimeOutHeight = decimal.Zero;

        public decimal LockTimeOutHeight
        {
            get { return _lockTimeOutHeight; }
            set
            {
                _lockTimeOutHeight = value;
                this.OnPropertyChanged();
            }
        }

        private bool _lockTimeOutCheck = false;
        public bool LockTimeOutCheck
        {
            get
            {
                return _lockTimeOutCheck;
            }

            set
            {
                _lockTimeOutCheck = value;
                this.OnPropertyChanged();
            }
        }

        private string _info4 = string.Empty;

        public string Info4
        {
            get
            {
                return _info4;
            }
            set
            {
                _info4 = value;
                this.OnPropertyChanged();
            }
        }

        private bool firmChecked = false;
        public bool FirmChecked
        {
            get
            {
                return firmChecked;
            }

            set
            {
                firmChecked = value;

                if (firmChecked)
                {
                    if (isBid)
                    {
                        sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_bid_hover.png";
                        sendPriceItem.FirmCheckIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_bid_normal.png";
                    }
                    else
                    {
                        sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_ofr_hover.png";
                        sendPriceItem.FirmCheckIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_ofr_normal.png";
                    }
                }
                else
                {
                    sendPriceItem.FirmMouseEnterIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_no_hover.png";
                    sendPriceItem.FirmCheckIS = "pack://application:,,,/StcLite.Trade.Plugin;component/Images/stc_button_nonfirm_no_normal.png";
                }
                this.OnPropertyChanged();
            }
        }

        private bool ofrButtonEnable = true;
        public bool OfrButtonEnable
        {
            get
            {
                return ofrButtonEnable;
            }

            set
            {
                ofrButtonEnable = value;
                this.OnPropertyChanged();
            }
        }

        private bool bidButtonEnable = true;
        public bool BidButtonEnable
        {
            get
            {
                return bidButtonEnable;
            }

            set
            {
                bidButtonEnable = value;
                this.OnPropertyChanged();
            }
        }

        #region listbox选中索引

        private int drpspotindex = -1;

        public int DrpSpotIndex
        {
            get
            {
                return drpspotindex;
            }

            set
            {
                int oldIndex = drpspotindex;
                drpspotindex = value;

                DrpAddOneOpenedTag = drpspotindex == -1 ? "1" : "0";
                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged();

                SetOtherVisibile();

                string newsettle = GetSetttle();
                _sendPriceAdapter.SetFilterParam(false, newsettle, Venues);
            }
        }


        private int normalspotindex = -1;

        public int NormalSpotIndex
        {
            get
            {
                return normalspotindex;
            }

            set
            {
                normalspotindex = value;

                this.NormalAddOneOpenedTag = normalspotindex == -1 ? "1" : "0";
                this.OnPropertyChanged();

                string newsettle = GetSetttle();
                _sendPriceAdapter.SetFilterParam(false, newsettle, Venues);
            }
        }
        

        /// <summary>
        /// SettleBox 字典
        /// </summary>
        private Dictionary<int, string> dictSettleBox = new Dictionary<int, string>();
        public Dictionary<int, string> DictSettleBox
        {
            get
            {
                return dictSettleBox;
            }

            set
            {
                dictSettleBox = value;
                this.OnPropertyChanged(nameof(DictSettleBox));
            }
        }



        private int setAmountTxtIndex = -1;

        public int SetAmountTxtIndex
        {
            get
            {
                return setAmountTxtIndex;
            }

            set
            {
                setAmountTxtIndex = value;
                if (setAmountTxtIndex == 0)
                {
                    OnOneClick();
                }
                else if (setAmountTxtIndex == 1)
                {
                    OnThreeClick();
                }
                else if (setAmountTxtIndex == 2)
                {
                    OnFiveClick();
                }
                this.OnPropertyChanged(nameof(SetAmountTxtIndex));
            }
        }

        /// <summary>
        /// 用于改变当点击 +1时，下拉菜单弹出，需要改变 +1 的上边框颜色，同时用于判断是否选中的 +1 ,用这个值改变默认的背景色  1 选中 0 未选中
        /// </summary>
        private string normaladdoneopenedtag = "1";
        public string NormalAddOneOpenedTag
        {
            get { return normaladdoneopenedtag; }
            set
            {
                normaladdoneopenedtag = value;
                OnPropertyChanged(nameof(NormalAddOneOpenedTag));
            }
        }

        private string drpaddOneOpenedTag = "1";
        /// <summary>
        /// Drp 的 +1 改变上边框颜色标识  1 选中 0 未选中
        /// </summary>
        public string DrpAddOneOpenedTag
        {
            get { return drpaddOneOpenedTag; }
            set
            {
                drpaddOneOpenedTag = value;
                OnPropertyChanged(nameof(DrpAddOneOpenedTag));
            }
        }

        private int normalsettleboxindex = 0;
        /// <summary>
        /// 清算速度 选项 
        /// </summary>
        public int NormalSettleBoxIndex
        {
            get
            {
                return normalsettleboxindex;
            }

            set
            {
                normalsettleboxindex = value;
                NormalSpotIndex = -1;

                if (normalsettleboxindex >= 0)
                {
                   NormalAddOneOpenedTag = "1";
                }

                if (NormalSettleBoxIndex == 0)
                {
                    AddOneNotLimitCheck = true;
                    AddOneTodayCheck = false;
                    AddOneTomorrowCheck = false;

                }
                else if (NormalSettleBoxIndex == 1)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = true;
                    AddOneTomorrowCheck = false;

                }
                else if (NormalSettleBoxIndex == 2)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = false;
                    AddOneTomorrowCheck = true;
                }

                OnPropertyChanged(nameof(NormalSettleBoxIndex));

                string newsettle = GetSetttle();
                _sendPriceAdapter.SetFilterParam(false, newsettle, Venues);
            }
        }

        private int drpSettleBoxIndex = -1;
        /// <summary>
        /// Drp +1 选项
        /// </summary>
        public int DrpSettleBoxIndex
        {
            get { return drpSettleBoxIndex; }
            set
            {
                drpSettleBoxIndex = value;
                DrpSpotIndex = -1;

                if (drpSettleBoxIndex >= 0)
                {
                    DrpAddOneOpenedTag = "1";
                }

                if (DrpSettleBoxIndex == 0)
                {
                    AddOneNotLimitCheck = true;
                    AddOneTodayCheck = false;
                    AddOneTomorrowCheck = false;

                }
                else if (DrpSettleBoxIndex == 1)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = true;
                    AddOneTomorrowCheck = false;

                }
                else if (DrpSettleBoxIndex == 2)
                {
                    AddOneNotLimitCheck = false;
                    AddOneTodayCheck = false;
                    AddOneTomorrowCheck = true;
                }

                OnPropertyChanged(nameof(DrpSettleBoxIndex));
            }
        }

        private string addOneShowText = "+1";
        public string AddOneShowText
        {
            get { return addOneShowText; }
            set
            {
                addOneShowText = value;
                OnPropertyChanged(nameof(AddOneShowText));
            }
        }


        /// <summary>
        /// 建议价
        /// </summary>
        private string bargainyield = string.Empty;
        public string BargainYield
        {
            get { return bargainyield; }
            set
            {
                bargainyield = value;
                this.OnPropertyChanged(nameof(BargainYield));
            }
        }

        /// <summary>
        /// 建议价是否显示
        /// </summary>
        private Visibility bargainyieldvisible = Visibility.Collapsed;
        public Visibility BargainYieldVisible
        {
            get { return bargainyieldvisible; }
            set
            {
                bargainyieldvisible = value;
                this.OnPropertyChanged(nameof(BargainYieldVisible));
            }
        }

        #endregion



        //private ListBoxItem selectedItem = null;

        //public ListBoxItem SelectedItem
        //{
        //    get
        //    {
        //        return selectedItem;
        //    }

        //    set
        //    {
        //        selectedItem = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        // 搜索
        private string searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return searchText;
            }

            set
            {
                searchText = value;
                this.OnPropertyChanged();
                SendPriceItem.Security = searchText;

                RefreshUI();
            }
        }

        private string customPrice = string.Empty;
        public string CustomPrice
        {
            get
            {
                return customPrice;
            }

            set
            {
                string oldPrice = customPrice;
                customPrice = !string.IsNullOrEmpty(value) ? value.Replace(",", "") : value;
                if (ListPointTypeIndex == 0 || this.isInit)
                {
                    this.OnPropertyChanged("CustomPrice");
                    return;
                }

                if (string.IsNullOrEmpty(customPrice))
                {
                    NetPrice = string.Empty;
                }

                if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value))
                {
                    netPrice = string.Empty;
                    this.OnPropertyChanged("NetPrice");
                    if (MarketIndicator != "2")
                    {
                        //isSendReq = true;
                        CalBond("4", GetSetttle(), NumChange(customPrice));
                    }
                }

                if (DrpWarningVisibility == Visibility.Visible)
                {
                    if (!string.IsNullOrEmpty(this.CustomPrice) && !string.IsNullOrEmpty(this.ClickPrice) && this.ClickPrice != "--")
                    {
                        if ((!IsIRS && BidOrOfrLower == "Ofr") || (IsIRS && BidOrOfrLower == "Bid"))
                        {
                            if (Decimal.Parse(this.CustomPrice) < Decimal.Parse(this.ClickPrice))
                            {
                                AlertSignToolTipText = $"*倒挂";
                                AlertSignForground = (Brush)myWindow.FindResource("ColorBrush.Red-1");
                                AlertSignVisible = Visibility.Visible;
                            }
                            else if (Decimal.Parse(this.CustomPrice) == Decimal.Parse(this.ClickPrice))
                            {
                                AlertSignToolTipText = $"";
                                AlertSignForground = (Brush)myWindow.FindResource("ColorBrush.Red-1");
                                AlertSignVisible = Visibility.Collapsed;
                            }
                            else
                            {
                                decimal dNew = 0;
                                decimal dClose = 0;
                                if (decimal.TryParse(CustomPrice, out dNew) && decimal.TryParse(ClickPrice, out dClose) && dClose != 0 && dNew != 0)
                                {
                                    decimal bp = (dNew - dClose) * 100;
                                    AlertSignToolTipText = bp == 0 ? string.Empty : $"*{CommonUtils.FormatLimitBp(bp)}";
                                }

                                AlertSignForground = (Brush)myWindow.FindResource("TextBlockAlertSignForground");
                                AlertSignVisible = Visibility.Visible;
                            }
                        }
                        else
                        {
                            if (Decimal.Parse(this.CustomPrice) > Decimal.Parse(this.ClickPrice))
                            {
                                AlertSignToolTipText = $"*倒挂";
                                AlertSignForground = (Brush)myWindow.FindResource("ColorBrush.Red-1");
                                AlertSignVisible = Visibility.Visible;
                            }
                            else if (Decimal.Parse(this.CustomPrice) == Decimal.Parse(this.ClickPrice))
                            {
                                AlertSignToolTipText = $"";
                                AlertSignForground = (Brush)myWindow.FindResource("ColorBrush.Red-1");
                                AlertSignVisible = Visibility.Collapsed;
                            }
                            else
                            {
                                decimal dNew = 0;
                                decimal dClose = 0;
                                if (decimal.TryParse(CustomPrice, out dNew) && decimal.TryParse(ClickPrice, out dClose) && dClose != 0 && dNew != 0)
                                {
                                    decimal bp = (dClose - dNew) * 100;
                                    AlertSignToolTipText = bp == 0 ? string.Empty : $"*{CommonUtils.FormatLimitBp(bp)}";
                                }

                                AlertSignForground = (Brush)myWindow.FindResource("TextBlockAlertSignForground");
                                AlertSignVisible = Visibility.Visible;
                            }
                        }
                    }
                }

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



        private string netPrice;
        public string NetPrice
        {
            get { return netPrice; }
            set
            {
                string oldPrice = netPrice;
                netPrice = value;

                if (ListPointTypeIndex == 0 || this.isInit)
                {
                    this.OnPropertyChanged("NetPrice");
                    return;
                }

                decimal d = 0;
                if (MarketIndicator != "2")
                {
                    if (!isSendReq && !string.IsNullOrEmpty(value) && !oldPrice.Equals(value) && decimal.TryParse(value, out d))
                    {
                        CalBond("2", GetSetttle(), netPrice);
                    }
                }

                if (oldPrice != null && value != null && !oldPrice.Equals(value) && string.IsNullOrEmpty(value))
                {
                    //CustomPrice = string.Empty;
                    customPrice = string.Empty;
                }

                this.OnPropertyChanged();
            }
        }


        private string NumChange(string price)
        {
            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                d = d / 100;
                price = d.ToString();
            }

            return price;
        }


        private int comboBoxSelectIndex = 0;

        public int ComboBoxSelectIndex
        {
            get
            {
                return comboBoxSelectIndex;
            }

            set
            {
                int oldIndex = comboBoxSelectIndex;
                comboBoxSelectIndex = value;

                this.OnPropertyChanged();
            }
        }

        private bool _checkAll = false;
        public bool CheckAll
        {
            get
            {
                return _checkAll;
            }

            set
            {
                _checkAll = value;

                if (value)
                {
                    CheckInner = false;
                }
                this.OnPropertyChanged();
            }
        }

        private bool _checkInner = false;
        public bool CheckInner
        {
            get
            {
                return _checkInner;
            }

            set
            {
                _checkInner = value;

                if (value)
                {
                    CheckAll = false;
                }

                this.OnPropertyChanged();
            }
        }

        private int spotSettleBoxIndex = 0;

        public int SpotSettleBoxIndex
        {
            get
            {
                return spotSettleBoxIndex;
            }

            set
            {
                spotSettleBoxIndex = value;

                this.OnPropertyChanged();

            }
        }

        private string _sendPriceType = string.Empty;
        public string SendPriceType
        {
            get
            {
                return _sendPriceType;
            }

            set
            {
                _sendPriceType = value;
                this.OnPropertyChanged();
            }
        }


        private SolidColorBrush allBestBidForground = new SolidColorBrush(Color.FromRgb(0xFB, 0x94, 0x3C));
        public SolidColorBrush AllBestBidForground
        {
            get { return allBestBidForground; }
            set
            {
                allBestBidForground = value;
                this.OnPropertyChanged("AllBestBidForground");
            }
        }

        private SolidColorBrush allBestOfrForground = new SolidColorBrush(Color.FromRgb(0x00, 0x9A, 0xFF));
        public SolidColorBrush AllBestOfrForground
        {
            get { return allBestOfrForground; }
            set
            {
                allBestOfrForground = value;
                this.OnPropertyChanged("AllBestOfrForground");
            }
        }

        /// <summary>
        /// Ofr Or Bid 用于改变金额，数量的背景色
        /// </summary>
        private string ofrOrBid = "0";
        public string OfrOrBid
        {
            get { return ofrOrBid; }
            set
            {
                ofrOrBid = value;
                this.OnPropertyChanged(nameof(OfrOrBid));
            }
        }

        private int maxPriceAreaHeight = 234;

        public int MaxPriceAreaHeight
        {
            get { return maxPriceAreaHeight; }
            set
            {
                maxPriceAreaHeight = value;
                this.OnPropertyChanged(nameof(MaxPriceAreaHeight));
            }
        }

        public bool isFirmEnabled = true;
        public bool IsFirmEnable
        {
            get { return isFirmEnabled; }
            set
            {
                isFirmEnabled = value;
                this.OnPropertyChanged(nameof(IsFirmEnable));
            }
        }

        /// <summary>
        /// 点击Hit方向颜色
        /// </summary>
        private Brush bidorofrtextblockforground = new SolidColorBrush(Colors.Blue);
        public Brush BidOrOfrTextBlockForground
        {
            get { return bidorofrtextblockforground; }
            set
            {
                bidorofrtextblockforground = value;
                this.OnPropertyChanged("BidOrOfrTextBlockForground");
            }
        }


        /// <summary>
        /// 点击Tabitem的背景色
        /// </summary>
        private Brush tabtitlebg;
        public Brush TabTitleBG
        {
            get { return tabtitlebg; }
            set
            {
                //if (value != null)
                {
                    tabtitlebg = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 边框颜色
        /// </summary>
        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        public Brush BorderBrush
        {
            get { return borderBrush; }
            set
            {
                if (value != null)
                {
                    borderBrush = value;
                    this.OnPropertyChanged(nameof(BorderBrush));
                }
            }
        }


        private string confirmButtonTag = null;
        /// <summary>
        /// Tag 1 Green Tag 2 Blue  Tag 3 Orange  Tag 4 GraySlate 随系统皮肤变化  Tag x:null GreenBlue随系统皮肤变化
        /// </summary>
        public string ConfirmButtonTag
        {
            get { return confirmButtonTag; }
            set
            {
                if (value != null)
                {
                    confirmButtonTag = value;
                    this.OnPropertyChanged(nameof(ConfirmButtonTag));
                }
            }
        }

        /// <summary>
        /// UI上用户选择点价类型
        /// </summary>
        private int _ListPointTypeIndex = 0;
        public int ListPointTypeIndex
        {
            get { return _ListPointTypeIndex; }
            set
            {
                _ListPointTypeIndex = value;
                if (value == 0)
                {
                    TitleBidOrOfr = this.IsBid ? "TKN" : "GVN";
                    ConfirmButtonContent = this.TitleBidOrOfr;

                    sendPriceItem.BorderBG = this.IsBid ? (Brush)myWindow.FindResource("OfrLinearGradientBrush") : (Brush)myWindow.FindResource("BidLinearGradientBrush");
                    TabTitleBG = this.IsBid ? (Brush)myWindow.FindResource("TabOfrBackground") : (Brush)myWindow.FindResource("TabBidBackground");
                    GetQtyViewModel().BorderBH = SendPriceItem.BorderBH = this.IsBid ? (Brush)myWindow.FindResource("ColorBrush.Blue8") : (Brush)myWindow.FindResource("ColorBrush.Orange5");
                    DrpWarningVisibility = Visibility.Collapsed;
                    CustomPrice = this.ClickPrice;
                    NetPrice = this.ClickNetPrice;
                    SetClickAmount(this.ClickAmount, WholeFlag == "1");
                    IsReadOnlyPrice = true;

                    if (this.ClickWholeflag == "1")
                    {
                        sendPriceItem.AmountBoxIndex = 1;
                    }
                    else
                    {
                        sendPriceItem.AmountBoxIndex = 0;
                    }

                    // 整量
                    WholeFlagChecked = this.ClickWholeflag == "1" ? true : false;


                    if (this.ClickWholeflag == "1")
                    {
                        GetQtyViewModel().IsReadOnly = true;
                        GetQtyViewModel().IsBtnEnable = false;
                    }

                    //定向回价背景色
                    sendPriceItem.ChatButtonBG = (Brush)myWindow.FindResource("LostFocusBorderBrush");
                    sendPriceItem.ChatButtonForeground = (Brush)myWindow.FindResource("ChatTextBlockForeground");


                    AlertSignForground = (Brush)myWindow.FindResource("ColorBrush.Red-1");
                    AlertSignVisible = Visibility.Collapsed;
                    ShowSingleSideMsgvisibility = Visibility.Visible;

                    //确认按钮样式
                    if (LocalEnv.ConfInfo.DefaultSkinStyle == "DefaultSkin")
                    {
                        ConfirmButtonTag = "1";
                    }
                    else
                    {
                        ConfirmButtonTag = this.IsBid ? "2" : "3";
                    }
                }
                else
                {
                    ConfirmButtonContent = "回价";
                    //定向回价背景色
                    if (LocalEnv.ConfInfo.DefaultSkinStyle == "DefaultSkin")
                    {
                        //反向颜色
                        sendPriceItem.BorderBG = this.IsBid ? (Brush)myWindow.FindResource("BidLinearGradientBrush") : (Brush)myWindow.FindResource("OfrLinearGradientBrush");
                        sendPriceItem.ChatButtonBG = this.IsBid ? (Brush)myWindow.FindResource("TabBidBackground") : (Brush)myWindow.FindResource("TabOfrBackground");
                    }
                    else
                    {
                        sendPriceItem.BorderBG = (Brush)myWindow.FindResource("BlueSkinLinearGradientBrush");
                        sendPriceItem.ChatButtonBG = (Brush)myWindow.FindResource("BlueSkinLinearGradientBrush");
                    }

                    sendPriceItem.ChatButtonForeground = this.IsBid ? (Brush)myWindow.FindResource("ChatOrange") : (Brush)myWindow.FindResource("ChatBlue");


                    GetQtyViewModel().BorderBH = SendPriceItem.BorderBH = this.IsBid ? (Brush)myWindow.FindResource("ColorBrush.Orange5") : (Brush)myWindow.FindResource("ColorBrush.Blue8");

                    DrpWarningVisibility = Visibility.Visible;
                    IsReadOnlyPrice = false;
                    GetQtyViewModel().IsReadOnly = false;
                    GetQtyViewModel().IsBtnEnable = true;
                    ShowSingleSideMsgvisibility = Visibility.Collapsed;

                    //确认按钮样式
                    ConfirmButtonTag = "1";
                }

                this.OnPropertyChanged(nameof(ListPointTypeIndex));
            }
        }


        private Visibility drpwarningvisibility = Visibility.Collapsed;
        /// <summary>
        /// 定向询价底部警示消息是否显示
        /// </summary>
        public Visibility DrpWarningVisibility
        {
            get
            {
                return drpwarningvisibility;
            }

            set
            {
                drpwarningvisibility = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility showsinglesidemsgvisibility = Visibility.Visible;
        /// <summary>
        /// 顶部我的报量信息是否显示
        /// </summary>
        public Visibility ShowSingleSideMsgvisibility
        {
            get
            {
                return showsinglesidemsgvisibility;
            }

            set
            {
                showsinglesidemsgvisibility = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 交易方向
        /// </summary>
        private string confirmbuttoncontent = string.Empty;
        public string ConfirmButtonContent
        {
            get { return confirmbuttoncontent; }
            set
            {
                confirmbuttoncontent = value;
                this.OnPropertyChanged("ConfirmButtonContent");
            }
        }

        private DateTime? fwdSelectDate;
        /// <summary>
        /// DRP 远期日期
        /// </summary>
        public DateTime? FwdSelectDate
        {
            get { return fwdSelectDate; }
            set
            {
                string oldDate = fwdSelectDate == null ? string.Empty : fwdSelectDate.Value.Date.ToString(YYYYMMDD);
                fwdSelectDate = value;
                string newDate = fwdSelectDate == null ? string.Empty : fwdSelectDate.Value.Date.ToString(YYYYMMDD);

                if (ListPointTypeIndex == 0 || this.isInit)
                {
                    this.OnPropertyChanged();
                    return;
                }

                if (!isSendReq && value != null && !oldDate.Equals(newDate))
                {
                    //isSendReq = true;
                    CalBond("4", GetSetttle(), NumChange(customPrice));
                }

                this.OnPropertyChanged();

                if (value != null && !oldDate.Equals(newDate))
                {
                }
            }
        }

        private int fwdComboBoxSelectIndex = 0;
        /// <summary>
        /// DRP 远期清算速度
        /// </summary>
        public int FwdComboBoxSelectIndex
        {
            get
            {
                return fwdComboBoxSelectIndex;
            }

            set
            {
                int oldIndex = fwdComboBoxSelectIndex;

                if (this.isInit)
                {
                    fwdComboBoxSelectIndex = value;
                    this.OnPropertyChanged();
                    return;
                }

                if (!isSendReq && !oldIndex.Equals(value))
                {
                    //isSendReq = true;
                    if (_sendPriceAdapter == null)
                    {
                        CalBond("4", GetSetttle(), NumChange(customPrice));
                    }
                }

                if (!oldIndex.Equals(value))
                {
                    UpdateBestPrice();
                }

                fwdComboBoxSelectIndex = value;

                this.OnPropertyChanged();
            }
        }

        private bool _wholeFlagChecked = false;
        /// <summary>
        /// 整量
        /// </summary>
        public bool WholeFlagChecked
        {
            get
            {
                return _wholeFlagChecked;
            }

            set
            {
                _wholeFlagChecked = value;
                WholeFlag = _wholeFlagChecked ? "1" : "0";
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 时长 - 分钟
        /// </summary>
        private string validMinute = string.Empty;
        public string ValidMinute
        {
            get { return validMinute; }
            set
            {
                if (value != null)
                {
                    validMinute = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///  时长 - 秒钟
        /// </summary>
        private string validSecond = string.Empty;
        public string ValidSecond
        {
            get { return validSecond; }
            set
            {
                if (value != null)
                {
                    validSecond = value;
                    this.OnPropertyChanged();
                }
            }
        }


        public bool isNormalPonitEnabled = true;
        /// <summary>
        /// 正常报价是否允许
        /// </summary>
        public bool IsNormalPonitEnabled
        {
            get { return isNormalPonitEnabled; }
            set
            {
                isNormalPonitEnabled = value;
                this.OnPropertyChanged(nameof(isNormalPonitEnabled));
            }
        }

        public bool isDrpPonitEnabled = true;
        /// <summary>
        /// 定向回价是否允许
        /// </summary>
        public bool IsDrpPonitEnabled
        {
            get { return isDrpPonitEnabled; }
            set
            {
                isDrpPonitEnabled = value;
                this.OnPropertyChanged(nameof(IsDrpPonitEnabled));
            }
        }


        /// <summary>
        /// 是否显示定向询价,1:显示
        /// </summary>
        private Visibility isshowdrp = Visibility.Collapsed;
        public Visibility IsShowDRP
        {
            get { return isshowdrp; }
            set
            {
                isshowdrp = value;
                this.OnPropertyChanged("IsShowDRP");
            }
        }

        /// <summary>
        /// 是否显示正常的问题
        /// </summary>
        private Visibility isShowNormal = Visibility.Visible;
        public Visibility IsShowNormal
        {
            get { return isShowNormal; }
            set
            {
                isShowNormal = value;
                this.OnPropertyChanged("IsShowNormal");
            }
        }

        /// <summary>
        /// 定向报价不可撤销时间
        /// </summary>
        private int pendingcanceltime = 15;
        public int PendingCancelTime
        {
            get { return pendingcanceltime; }
            set
            {
                pendingcanceltime = value;
                this.OnPropertyChanged("PendingCancelTime");
            }
        }

        /// <summary>
        /// 是否允许修改价格
        /// </summary>
        public bool isreadonlyprice = true;
        public bool IsReadOnlyPrice
        {
            get { return isreadonlyprice; }
            set
            {
                isreadonlyprice = value;
                this.OnPropertyChanged("IsReadOnlyPrice");
            }
        }

        /// <summary>
        /// 原始输入量
        /// </summary>
        private string _clickAmount = string.Empty;
        public string ClickAmount
        {
            get { return _clickAmount; }
            set
            {
                _clickAmount = value;
                this.OnPropertyChanged(nameof(ClickAmount));
            }
        }

        /// <summary>
        /// 原始输入整量标记
        /// </summary>
        private string _clickWholeflag = string.Empty;
        public string ClickWholeflag
        {
            get { return _clickWholeflag; }
            set
            {
                _clickWholeflag = value;
                this.OnPropertyChanged(nameof(ClickWholeflag));
            }
        }

        private string _partySubName = string.Empty;
        public string PartySubName
        {
            get { return _partySubName; }
            set
            {
                _partySubName = value;
                this.OnPropertyChanged(nameof(PartySubName));
            }
        }

        #region IRS
        private string marketindicator = "4";
        /// <summary>
        /// 债券类型
        /// </summary>
        public string MarketIndicator
        {
            get { return marketindicator; }
            set
            {
                marketindicator = value;
                if (value == "2")
                {
                    IsShowIRS = Visibility.Collapsed;
                    AccountUserVisibility = Visibility.Collapsed;
                    TextBlock qtyView = myWindow.FindName("PriceTxtPercent") as TextBlock;
                    qtyView.Margin = new Thickness(78, 24, 0, 0);
                    QtyView qtyView1 = myWindow.FindName("qtyView") as QtyView;
                    qtyView1.Margin = new Thickness(136, 0, 0, 0);
                    (myWindow as View.Order.SendPointPriceView).Width = 400;
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否显示 IRS模式
        /// </summary>
        private Visibility isShowIRS = Visibility.Visible;
        public Visibility IsShowIRS
        {
            get { return isShowIRS; }
            set
            {
                isShowIRS = value;
                this.OnPropertyChanged("IsShowIRS");
            }
        }

        private bool isIRS = false;
        /// <summary>
        /// 是否是IRS
        /// </summary>
        public bool IsIRS
        {
            get { return isIRS; }
            set { isIRS = value; }
        }


        #endregion

        #endregion

        #region 命令事件
        public ICommand BidCommand { get { return new ComRelayCommand(OnBidClick); } }
        public ICommand OfrCommand { get { return new ComRelayCommand(OnOfrClick); } }
        public ICommand CloseCommand { get { return new ComRelayCommand(CloseClick); } }

        public ICommand RefreshCommand { get { return new ComRelayCommand(OnRefreshClick); } }

        public ICommand CancelCommand { get { return new ComRelayCommand(OnCancelClick); } }
        public ICommand OkCommand { get { return new ComRelayCommand(OnConfirmClick); } }
        public ICommand AppointCommand { get { return new ComRelayCommand(AppointClick); } }
        public ICommand AddCommand { get { return new ComRelayCommand(OnAddClick); } }
        public ICommand DownCommand { get { return new ComRelayCommand(OnDownClick); } }

        // New Add Command
        // + - Price 
        public ICommand AddPriceCommand { get { return new ComRelayCommand(AddPriceClick); } }
        public ICommand ReducePriceCommand { get { return new ComRelayCommand(ReducePriceClick); } }

        // + - Amout
        public ICommand AddAmountCommand { get { return new ComRelayCommand(AddAmountClick); } }
        public ICommand ReduceAmountCommand { get { return new ComRelayCommand(ReduceAmountClick); } }
        #endregion

        private void OnOneClick()
        {
            this.AmountText = "2";
        }

        private void OnThreeClick()
        {
            this.AmountText = "3";
        }

        private void OnFiveClick()
        {
            this.AmountText = "5";
        }

        public void SetAmountTxtSelectIndex(string intxt = "")
        {
            List<string> valueList = new List<string>() { "2", "3", "5" };
            if (string.IsNullOrEmpty(intxt))
            {
                intxt = this.AmountText;
            }

            if (!valueList.Contains(intxt))
            {
                SetAmountTxtIndex = -1;
            }
            else
            {
                int tem2 = valueList.FindLastIndex(x => x == intxt);
                SetAmountTxtIndex = tem2;
            }
        }

        public QtyViewModel GetQtyViewModel()
        {
            QtyView qtyView = myWindow.FindName("qtyView") as QtyView;

            return qtyView.DataContext as QtyViewModel;
        }


        private void OnAddClick()
        {
            if (string.IsNullOrEmpty(sendPriceItem.NonfirmTime))
                sendPriceItem.NonfirmTime = "1";
            else
            {
                decimal d = 0;
                if (decimal.TryParse(sendPriceItem.NonfirmTime, out d))
                    sendPriceItem.NonfirmTime = (d + 1).ToString();
            }
        }

        private void OnDownClick()
        {
            decimal d = 0;
            if (decimal.TryParse(sendPriceItem.NonfirmTime, out d) && (d - 1) >= 0)
                sendPriceItem.NonfirmTime = (d - 1).ToString();

        }

        #region UI Amout,Price  调整  SetAmountTxtIndex
        private void AddPriceClick()
        {
            if (IsReadOnlyPrice)
            {
                return;
            }

            double price = 0;
            if (double.TryParse(CustomPrice, out price))
            {
                price = price + 0.0025;

                if (price > 0)
                {
                    decimal d = 0;
                    CustomPrice = price.ToString("F4");
                }
            }
        }
        private void ReducePriceClick()
        {
            if (IsReadOnlyPrice)
            {
                return;
            }

            double price = 0;
            if (double.TryParse(CustomPrice, out price))
            {
                price = price - 0.0025;
                if (price > 0)
                {
                    CustomPrice = price.ToString("F4");
                }
            }
        }
        private void AddAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(this.AmountText, out amount))
            {
                amount += 1;
                if (amount >= 1000)
                {
                    this.AmountText = "1000";
                }
                else
                {
                    this.AmountText = amount.ToString();
                }
            }
            else
            {
                this.AmountText = "1";
            }
        }
        private void ReduceAmountClick()
        {
            decimal amount = 0;
            if (decimal.TryParse(this.AmountText, out amount))
            {
                amount = amount - 1;
                if (amount > 0)
                {
                    this.AmountText = amount.ToString();
                }
                else
                {
                    this.AmountText = "1";
                }
            }
            else
            {
                this.AmountText = "5";
            }
        }
        #endregion

        /// <summary>
        /// 点价Normal单
        /// </summary>
        /// <param name="settle"></param>
        private void ParseSettle(string settle)
        {
            NormalSettleBoxIndex = 0;
            AddOneShowText = "+1";

            if (IsIRS)
                return;

            try
            {
                //判断是多清算速度，如果是则选择 +14
                List<string> lstsettle = new List<string>();
                string[] tmpsettle = settle.Split(':');
                if (!string.IsNullOrEmpty(settle))
                {
                    // 如果传入单个清算速度
                    if (tmpsettle.Length == 1)
                    {
                        if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                        {
                            AddOneNotLimitCheck = false;
                            NormalAddOneOpenedTag = "1";
                            if (settle.Equals("2,3"))
                            {
                                NormalSettleBoxIndex = 0;
                                AddOneNotLimitCheck = true;
                            }
                            else if (settle.Equals("3"))
                            {
                                NormalSettleBoxIndex = 2;
                                AddOneTomorrowCheck = true;
                            }
                            else if (settle.Equals("2"))
                            {
                                NormalSettleBoxIndex = 1;
                                AddOneTodayCheck = true;
                            }
                            else if (settle.Equals("+1"))
                            {
                                NormalSettleBoxIndex = 0;
                                AddOneNotLimitCheck = true;
                            }
                            else if (settle.Equals("全"))
                            {
                                NormalSettleBoxIndex = 0;
                                AddOneNotLimitCheck = true;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < DictSettleBox.Count; i++)
                            {
                                if (DictSettleBox[i] == settle)
                                {
                                    NormalSpotIndex = i;
                                    NormalAddOneOpenedTag = "0";
                                    break;
                                }
                            }

                            if (tmpsettle.Contains("2") || tmpsettle.Contains("3") || tmpsettle.Contains("2,3"))
                            {
                                NormalAddOneOpenedTag = "1";
                            }

                            if (settle.Split(',').Length >= 2)
                            {
                                NormalAddOneOpenedTag = "1";
                                NormalSpotIndex = 0;
                                AddOneNotLimitCheck = true;
                            }
                        }
                    }
                    else
                    {


                        if (tmpsettle.Contains("1"))
                        {
                            for (int i = 0; i < DictSettleBox.Count; i++)
                            {
                                if (DictSettleBox[i] == "1")
                                {
                                    NormalSpotIndex = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            NormalAddOneOpenedTag = "1";
                            AddOneNotLimitCheck = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {

            }
        }

        /// <summary>
        /// 解析清算速度
        /// </summary>
        /// <param name="settle"></param>
        private void ParseDRPSettle(string settle)
        {
            DrpSettleBoxIndex = 0;
            AddOneShowText = "+1";

            if (IsIRS)
                return;

            try
            {
                if (!string.IsNullOrEmpty(settle))
                {

                    if (settle.Equals("+1") || settle.Equals("2") || settle.Equals("T+1") || settle.Equals("2,3") || settle.Equals("3") || settle.Equals("T+1(明+0") || settle.Equals("T+1(今+1)") || settle.Equals("全"))
                    {
                        DrpSpotIndex = -1;
                        NormalAddOneOpenedTag = "1";

                        if (settle.Equals(ADD_ONE_NOT_LIMIT))
                        {
                            DrpSettleBoxIndex = 0;
                            AddOneNotLimitCheck = true;
                        }
                        else if (settle.Equals("3"))
                        {
                            DrpSettleBoxIndex = 2;
                            AddOneTomorrowCheck = true;
                        }
                        else if (settle.Equals("2"))
                        {
                            DrpSettleBoxIndex = 1;
                            AddOneTodayCheck = true;
                        }
                        else if (settle.Equals("+1"))
                        {
                            DrpSettleBoxIndex = 0;
                            AddOneNotLimitCheck = true;
                        }
                        else if (settle.Equals("全"))
                        {
                            DrpSettleBoxIndex = 0;
                            AddOneNotLimitCheck = true;
                        }
                    }
                    else
                    {
                        //如果多个清算速度就是 +1
                        if (settle.Contains(":"))
                        {
                            DrpSpotIndex = -1;
                            DrpAddOneOpenedTag = "0";

                            string[] tmpsettle = settle.Split(':');
                            if (tmpsettle.Contains("1"))
                            {
                                DrpSpotIndex = 0;
                            }
                            else if (tmpsettle.Contains("2") || tmpsettle.Contains("3") || tmpsettle.Contains("2,3"))
                            {
                                DrpAddOneOpenedTag = "1";
                            }

                            //这里还要判断是否有远期
                            foreach (string va in DictSettleBox.Values)
                            {
                                if (va.Contains("."))
                                {
                                    string[] arr = va.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                                    int month = 0;
                                    int day = 0;
                                    if (arr != null && arr.Length > 1)
                                    {

                                        string[] dtStr = arr[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                        if (dtStr != null && dtStr.Length > 1
                                                    && Int32.TryParse(dtStr[0], out month) && Int32.TryParse(dtStr[1], out day))
                                        {
                                            DateTime dt = new DateTime(DateTime.Now.Year, month, day);
                                            if (dt < DateTime.Now)
                                            {
                                                dt = new DateTime(DateTime.Now.Year, month, day).AddYears(1);
                                            }

                                            try
                                            {
                                                FwdSelectDate = new DateTime?(dt);
                                            }
                                            catch (Exception e) { }
                                        }

                                        if (arr[1].Equals("0"))
                                        {
                                            FwdComboBoxSelectIndex = 0;
                                        }
                                        else if (arr[1].Equals("1"))
                                        {
                                            FwdComboBoxSelectIndex = 1;
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {

                            if (settle.Equals("1") || settle.Equals("T+0") || settle.Equals("+0"))//T+0
                            {
                                DrpSpotIndex = 0;
                            }
                            else if (settle.Equals("明天+1") || settle.Equals("4"))
                            {
                                DrpSpotIndex = 0;
                                if (MarketIndicator != "2")
                                    this.CalDate("4", false);
                            }
                            else if (settle.Equals("远"))
                            {
                                DrpSpotIndex = 1;
                            }
                            else
                            {
                                DrpSpotIndex = 1;
                                string[] arr = settle.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries);
                                int month = 0;
                                int day = 0;
                                if (arr != null && arr.Length > 1)
                                {

                                    string[] dtStr = arr[0].Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                                    if (dtStr != null && dtStr.Length > 1
                                                && Int32.TryParse(dtStr[0], out month) && Int32.TryParse(dtStr[1], out day))
                                    {
                                        DateTime dt = new DateTime(DateTime.Now.Year, month, day);
                                        if (dt < DateTime.Now)
                                        {
                                            dt = new DateTime(DateTime.Now.Year, month, day).AddYears(1);
                                        }

                                        try
                                        {
                                            FwdSelectDate = new DateTime?(dt);
                                        }
                                        catch (Exception e) { }
                                    }
                                    //_needCalPrice = true;
                                    if (arr[1].Equals("0"))
                                    {
                                        FwdComboBoxSelectIndex = 0;
                                    }
                                    else if (arr[1].Equals("1"))
                                    {
                                        FwdComboBoxSelectIndex = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            finally
            {
                _needCalPrice = true;
            }
        }

        bool isInit = true;
        /// <summary>
        ///  初始化界面函数
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="security">债券代码 例如 180210.IB</param>
        /// <param name="isBid">方向 1 = Bid 2 = Ofr</param>
        /// <param name="clickPrice">收益率（%）</param>
        /// <param name="clickAmount">数量（万）</param>
        /// <param name="settle"></param>
        /// <param name="netPrice"></param>
        /// <param name="source"></param>
        /// <param name="wholeflag"></param>
        /// <param name="isFrim"></param>
        /// <param name="orderinfotip">订单盈利信息</param>
        /// <param name="clOrdID">客户端ID</param>
        /// <param name="Info4">意向单号</param>
        /// <param name="isDefaultDrp">默认打开直接回价页面</param>
        public void Init(Window myWindow, string security, bool isBid, string clickPrice, string clickAmount, string insettle, string netPrice, string source,
            string wholeflag, bool isFrim, string id = null, string trsactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null,
            string orderinfotip = "", string clOrdID = "", string venueTypeGW = null, string iocTimeOut = null, string info4 = null, bool isDefaultDrp = false, string partySubName = null, string orderPrice = null, string orderAmount = null, string orderwholeflag = null, string bargainyield = null, DRPParamInfoModel dRPParam = null)
        {
            InitAccountUsers();
            
            InSecurity = security;
            //payMoneyThread = new Thread(new ThreadStart(UpdateData));
            //payMoneyThread.Name = "更新价格";
            //payMoneyThread.Priority = ThreadPriority.AboveNormal;
            //payMoneyThread.Start();

            _sendPriceAdapter = new SendPriceModelAdapter();
            //venue = "CDH";
            this.IsBid = !isBid;
            this.BidOrOfr = this.IsBid ? "Ofr" : "Bid";
            this.MyBidOrOfr = IsBid ? "Bid" : "Ofr";
            this.BidOrOfrLower = this.BidOrOfr;
            this.TitleBidOrOfr = this.IsBid ? "TKN" : "GVN";


            this.ConfirmButtonContent = ListPointTypeIndex == 0 ? this.TitleBidOrOfr : "回价";

            this.myWindow = myWindow;
            this.AlertSignForground = (Brush)myWindow.FindResource("ColorBrush.Red-1");
            myWindow.Unloaded += MyWindow_Unloaded;
            this.ClOrdID = clOrdID;
            // 传入参数
            this.Venues = source;
            this.SourceTrade = ComboSourceUtil.Instance.FindEnum(source, "venues");
            
            var x = CommonUtils.GetBondsInfoPoBySecurityID(security);

            string goodsType = string.Empty;

            if (x != null)
            {
                goodsType = (x as BondsInfoPo).SECURITY_TYPE;
                _bondsInfoPo = (x as BondsInfoPo);
                ChsSecurity = (x as BondsInfoPo).Short_Name;
                MarketIndicator = (x as BondsInfoPo).MarketIndicator;
                IsIRS = MarketIndicator == "2";
            }

            if (dRPParam != null && !string.IsNullOrEmpty(dRPParam.PriceType) && !string.IsNullOrEmpty(dRPParam.SettlType))
            {
                decimal d = 0;
                if (decimal.TryParse(clickPrice, out d))
                {
                    decimal calValue = ConvertToYield(security, dRPParam.PriceType, dRPParam.SettlType, d);
                    clickPrice = calValue.ToString();
                }
            }
            if (dRPParam != null)
            {
                _partyId = dRPParam.PartyId;
                _partyName = dRPParam.PartyName;
            }

            if ((CommonUtils.ConvertToDecimal(clickPrice).ToString("F4") == "-1.0000") || (CommonUtils.ConvertToDecimal(clickPrice).ToString("F4") == "999.0000") || clickPrice=="")
            {
                clickPrice = "--";
            }

            if ((CommonUtils.ConvertToDecimal(orderPrice).ToString("F4") == "-1.0000") || (CommonUtils.ConvertToDecimal(orderPrice).ToString("F4") == "999.0000") || orderPrice == "")
            {
                orderPrice = "--";
            }

            this.ClickPrice = clickPrice;
            this.ClickNetPrice = netPrice;
            this.ClickAmount = clickAmount == "0" ? "--" : clickAmount;
            this.ClickWholeflag = wholeflag;
            this.WholeFlag = wholeflag;
            this.Settle = insettle;
            // 设置显示信息  3.2250% 2000*+4000(明+0)+1000(+0)+3000(远)
            this.OrderInfoTip = $"{clickPrice}% {orderinfotip}";
            this.Security = security;
            this.BargainYield = string.IsNullOrEmpty(bargainyield) ? "--" : bargainyield;
            this.BargainYieldVisible = this.BargainYield == "--" ? Visibility.Collapsed : Visibility.Visible;

            if ((orderPrice != null) && (orderAmount != null))
            {
                this.ClickPrice = orderPrice;
                this.ClickAmount = orderAmount == "0" ? "--" : orderAmount;
                this.ClickWholeflag = string.IsNullOrEmpty(orderwholeflag) ? "0" : orderwholeflag;
            }

            if (string.IsNullOrEmpty(this.ClickPrice) || this.ClickPrice == "--")
            {
                ClickPriceVisible = Visibility.Collapsed;
            }

            this.ChsPriceValid = "";
            if (!string.IsNullOrEmpty(partySubName))
            {
                if (!partySubName.Contains("-"))
                    partySubName = "-" + partySubName;

                this.PartySubName = partySubName.Split('-')[1];
            }

            if (isFrim)
            {
                IsShowDRP = Visibility.Collapsed;
                IsShowNormal = Visibility.Visible;
            }

            //如果是IRS
            if (MarketIndicator == "2")
            {
                IsShowDRP = Visibility.Visible;
                IsShowNormal = Visibility.Collapsed;
            }

            //if (!string.IsNullOrEmpty(venue) && venue.ToUpper().Equals("CDH"))
            {
                LockTimeOutHeight = 47;

                if (SystemParamHandler.Instance.GetUserSetting() != null && !string.IsNullOrEmpty(SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut))
                {
                    IocOutTime = SystemParamHandler.Instance.GetUserSetting().userNofirmIOCTimeOut;
                    LockTimeOutCheck = true;

                    UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
                    if (!string.IsNullOrEmpty(Us.inf2))
                    {
                        if (JsonSplit.IsJson(Us.inf2))
                        {
                            ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                            SendPriceConsistencyCheck = string.IsNullOrEmpty(ck.SendPriceConsistencyCheck) ? false : ck.SendPriceConsistencyCheck == "1" ? true : false;
                            PointPriceConsistencyCheck = string.IsNullOrEmpty(ck.PointPriceConsistencyCheck) ? false : ck.PointPriceConsistencyCheck == "1" ? true : false;
                            PointPriceQty = string.IsNullOrEmpty(ck.PointPriceQty) ? string.Empty : ck.PointPriceQty;
                        }
                    }
                }
                else
                {
                    IocOutTime = "30";
                    LockTimeOutCheck = false;
                }
            }

            _isFirm = isFrim;
            if (isFrim)
            {
                LockTimeOutCheck = !isFrim;
                IsFirmEnable = !isFrim;
            }

            if (!string.IsNullOrEmpty(iocTimeOut))
            {
                IocOutTime = iocTimeOut;
                IsFirmEnable = LockTimeOutCheck = true;
            }


            //else
            //{
            //    this.myWindow.Height = 300;
            //    LockTimeOutHeight = 0;
            //}

            GetQtyViewModel().Init(security);
            
            SendPriceItem = new SendPriceItem();
            GetQtyViewModel().BorderBH = SendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Gray15");
            SendPriceItem.Security = security;
            SearchText = SendPriceItem.Security;

            CheckAll = true;
            OfrButtonEnable = isBid;
            BidButtonEnable = !isBid;
            //NetPrice = netPrice;

            if (this.IsBid)
            {
                OnBidClick();
            }
            else
            {
                OnOfrClick();
            }

            //设置量
            SetClickAmount(clickAmount, wholeflag == "1");
            
            if ((this.ClickWholeflag == "1") && (ListPointTypeIndex == 0))
            {
                GetQtyViewModel().IsReadOnly = true;
                GetQtyViewModel().IsBtnEnable = false;
            }

            // 默认最优双边配色
            AllBestBidForground = (SolidColorBrush)myWindow.FindResource("ColorBrush.Orange7");
            AllBestOfrForground = (SolidColorBrush)myWindow.FindResource("ColorBrush.Blue8");

            _currentSource = source;
            _currentId = id;
            _trsactTime = trsactTime;
            _currentVenueTypeGW = venueTypeGW;
            _deliveryType = deliveryType;
            _clearingMethod = clearingMethod;
            _quoteType = quoteType;

            ParseSettle(insettle);
            ParseDRPSettle(insettle);
            
            if (FwdSelectDate == null)
            {
                if (MarketIndicator != "2")
                    CalDate("4", false);
            }

            if (!isFrim)
            {
                if (venueTypeGW.ToUpper() == "INNER")
                {
                    if (DRPTools.CheckInnerPermission(security))
                    {
                        IsDrpPonitEnabled = true;

                        if (clickAmount == "--")
                        {
                            isNormalPonitEnabled = false;
                        }
                        else if (ConvertDecimal(clickPrice) == 0 || ConvertDecimal(clickAmount) == 0 )
                        {
                            isNormalPonitEnabled = false;
                        }

                        if (isDefaultDrp)
                        {
                            ListPointTypeIndex = 1;
                        }
                    }
                }
                else
                {
                    IsDrpPonitEnabled = false;
                }
            }
            else
            {
                IsDrpPonitEnabled = false;
                //IsShowDRP = Visibility.Collapsed;
                //IsShowNormal = Visibility.Visible;
            }


            if (wholeflag == "1")
            {
                sendPriceItem.AmountBoxIndex = 1;
            }
            else
            {
                sendPriceItem.AmountBoxIndex = 0;
            }

            // 整量
            WholeFlagChecked = wholeflag == "1" ? true : false;

            if (dRPParam != null)
            {
                this.OrderId = dRPParam.OrderId ?? ""; 
            }

            if (id != null)
            {
                this.OrderId = id;
            }

            //CalBond("4", GetSetttle(), NumChange(customPrice));

                List<UserVenue> userVenues = SystemParamHandler.Instance.GetUserVenueByCondition(goodsType, "1");
            if (userVenues != null && userVenues.Count > 0)
                SendPriceItem.NonfirmTime = userVenues[0].firmtonontime;


            _sendPriceAdapter.SetFilterParam(false, insettle, Venues);
            _sendPriceAdapter.Init(SendPriceItem);
            // 订阅报价
            _sendPriceAdapter.OnMDPrice += _sendPriceAdapter_OnMDPrice;
            LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;
            this.PriceValid = true;

            // to do ...
            if (PointPriceConsistencyCheck)
            {
                this.timer = new DispatcherTimer();
                this.timer.Tick += Timer_Tick;
                this.timer.Interval = TimeSpan.FromSeconds(1);
                this.timer.Start();
            }
            // 服务器时间
            LastUpdateTime = DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

            Info4 = info4;

            if (!string.IsNullOrEmpty(clickPrice) && clickPrice != "--")
            {
                if (this.ClickPrice != clickPrice)
                {
                    DrpWarningVisibility = Visibility.Visible;
                }

                CustomPrice = ConvertDecimal(clickPrice).ToString("F4");
                NetPrice = ConvertDecimal(clickNetPrice).ToString("F4");
            }

            isInit = false;
        }

        private decimal ConvertToYield(string securityID, string priceType, string settleType, decimal value)
        {
            string calType = string.Empty;
            if (priceType == "1")// 原始为净价
            {
                calType = "2";
            }
            else if (priceType == "2")//原始为全价
            {
                calType = "3";
            }
            else if (priceType == "7")//原始为行权收益率
            {
                calType = "7";
            }

            if (!string.IsNullOrEmpty(calType))
            {
                // 计算收益率， 净价
                CalReq requst = new CalReq();
                requst.SecurityID = securityID;
                requst.CalType = calType;
                requst.SettlType = settleType;
                if (calType == "4" || calType == "7")
                {
                    // 如果是行权收益率，到期收益率 
                    requst.Value = (value / 100).ToString();
                }
                else
                {
                    requst.Value = value.ToString();
                }

                CalResult result = CalHandler.Instance.CalBond(requst);

                if (result != null && result.Data != null)
                {
                    return result.Data.Yield * 100;
                }
            }
            return value;
        }

        /// <summary>
        /// 定向报价撤单
        /// </summary>
        /// <param name="rs"></param>
        public void Init(STC.Common.Model.Fix.Message.OrderCancelRequest rs,Window myWindow, string security, bool isBid, string clickPrice, string clickAmount, string insettle, string netPrice, string source,
            string wholeflag, bool isFrim, string id = null, string trsactTime = null, string deliveryType = null, string clearingMethod = null, string quoteType = null,
            string orderinfotip = "", string clOrdID = "", string venueTypeGW = null, string iocTimeOut = null, string info4 = null, bool isDefaultDrp = false, string partySubName = null, string orderPrice = null, string orderAmount = null, string orderwholeflag = null, DRPParamInfoModel dRPParam = null)
        {
            this.Ocr = rs;
            this.Init(myWindow, security, isBid, clickPrice, clickAmount, insettle, netPrice, source, wholeflag, isFrim, id, trsactTime, deliveryType, clearingMethod, quoteType, orderinfotip, clOrdID, venueTypeGW, iocTimeOut, info4, isDefaultDrp, partySubName, orderPrice, orderAmount, orderwholeflag, null, dRPParam);
        }

        /// <summary>
        /// 设置量
        /// </summary>
        /// <param name="_clickAmount"></param>
        private void SetClickAmount(string _clickAmount, bool isForce = false)
        {

            decimal pointAmount = _clickAmount.ConvertToDecimal();
            decimal orderAmount = 0;
            decimal defaultAmount = PointPriceQty.ConvertToDecimal();

            //如果是IRS则 使用用户的点价报量
            if (IsIRS)
            {
                defaultAmount = pointAmount;
            }
            orderAmount = Math.Min(pointAmount, defaultAmount);
            orderAmount = orderAmount == 0 ? pointAmount : orderAmount;

            if (_clickAmount != null && _clickAmount != "--")
            {
                CheckAmount(_clickAmount);
                if (!string.IsNullOrEmpty(_clickAmount) && _clickAmount != "0" )
                {
                    if (isForce)
                    {
                        GetQtyViewModel().TrueAmount = decimal.Parse(_clickAmount);
                    }
                    else
                    {
                        GetQtyViewModel().TrueAmount = orderAmount;
                    }
                }
                else
                {
                    GetQtyViewModel().TrueAmount = null;
                }
                //if (sendPriceItem.Security.Contains("."))
                //    this.AmountText = (orderAmount / 1000).ToString();
                //else
                //    this.AmountText = orderAmount.ToString();
            }
        }

        private void CheckAmount(string amount)
        {
            decimal intAmount = 0;
            if (decimal.TryParse(amount, out intAmount))
            {
                if (intAmount % 1 != 0)
                {
                    SendPriceEnable = false;
                }
                else
                {
                    if (IsIRS)
                    {
                        intAmount /= 100;
                        SendPriceEnable = intAmount >= 1 && intAmount <= 100;
                    }
                    else
                    {
                        intAmount /= 1000;
                        SendPriceEnable = intAmount >= 1 && intAmount <= 1000;
                    }
                }
            }
            else
            {
                SendPriceEnable = false;
            }


            if (SendPriceEnable)
            {
                SendPriceEnable = PriceValid;
            }

        }


        public void SetPriceFocus()
        {
            TextBox AmountTxt = (TextBox)myWindow.FindName("AmountTxt");
            // 选择最后位数
            //int pos = AmountTxt.Text.IndexOf('.');

            //if (pos > 0)
            //{
            int sellength = AmountTxt.Text.Length;
            AmountTxt.Focus();
            // AmountTxt.SelectAll();
            AmountTxt.SelectionStart = 0;
            AmountTxt.SelectionLength = sellength;
            //}
        }
        /// <summary>
        /// 最后成交价格
        /// </summary>
        private string _LastPrice = string.Empty;
        public string LastPrice
        {
            get { return _LastPrice; }
            set
            {
                _LastPrice = value;
                this.OnPropertyChanged("LastPrice");
            }
        }

        /// <summary>
        /// 最后成交时间
        /// </summary>
        private string _LastUpdateTime = string.Empty;
        public string LastUpdateTime
        {
            get { return _LastUpdateTime; }
            set
            {
                _LastUpdateTime = value;
                this.OnPropertyChanged("_LastUpdateTime");
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

        private string alertSignToolTipText = "*5秒内与全市场最新成交不一致 偏离较大";
        public string AlertSignToolTipText
        {
            get { return alertSignToolTipText; }
            set
            {
                alertSignToolTipText = value;
                this.OnPropertyChanged("AlertSignToolTipText");
            }
        }

        /// <summary>
        /// 与5 秒钟内最新成交不一致提醒开关
        /// </summary>
        public bool PointPriceConsistencyCheck = false;
        public bool SendPriceConsistencyCheck = false;
        public string PointPriceQty = string.Empty;


        private DispatcherTimer timer = null;
        private bool isHvaeNewExecution = false;
        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            if (obj.SecurityID == SendPriceItem.Security)
            {
                // 更新时间
                string updateTime = obj.UpdateTime;
                string serverTime = DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                if (DateTime.Parse(updateTime) > DateTime.Parse(LastUpdateTime))
                {
                    LastUpdateTime = obj.UpdateTime;
                    LastPrice = obj.Yield;
                    //this.OrderInfoTip = $"{LastPrice}% {LastUpdateTime}";
                    isHvaeNewExecution = true;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string serverTime = DateTimeEx.ServerTimeNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            if (isHvaeNewExecution)
            {
                TimeSpan ts1 = new TimeSpan(DateTime.Parse(serverTime).Ticks);
                TimeSpan ts2 = new TimeSpan(DateTime.Parse(LastUpdateTime).Ticks);
                TimeSpan ts3 = ts1.Subtract(ts2).Duration();

                if (ts3.Seconds > 5)
                {
                    if (!string.IsNullOrEmpty(this.CustomPrice) && !string.IsNullOrEmpty(this.LastPrice))
                    {
                        if (Decimal.Parse(this.LastPrice) != Decimal.Parse(this.CustomPrice))
                        {
                            AlertSignToolTipText = $"与最新成交 {this.LastPrice} 不一致";
                            AlertSignVisible = Visibility.Visible;
                            this.timer.Stop();
                        }
                    }
                }
            }
        }

        private void _sendPriceAdapter_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (obj.SecurityID == SendPriceItem.Security)
            {
                //this.PriceValid = false;
                UpdateUiBestBidOrOfrForground();
            }
        }

        /// <summary>
        /// 查找是否包含 Settle
        /// </summary>
        /// <param name="allMarket"></param>
        /// <param name="insettle"></param>
        /// <returns></returns>
        private bool IsHaveSettle(MarketDataSnapshotFullRefresh allMarket, List<string> insettle)
        {
            bool isfoundsettle = false;
            if (allMarket != null)
            {
                List<GNoMDEntries> lstMarketDataSnapshot = allMarket.NoMDEntries;
                for (int j = 0; j < insettle.Count; j++)
                {
                    if (lstMarketDataSnapshot != null)
                    {
                        if (insettle[j] == "1")
                        {
                            for (int i = 0; i < lstMarketDataSnapshot.Count; i++)
                            {
                                if (lstMarketDataSnapshot[i].SettlType == "1")
                                {
                                    if (this.IsBid == (lstMarketDataSnapshot[i].MDEntryType == "1" ? true : false))
                                    {
                                        isfoundsettle = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (insettle[j] == "2,3")
                        {
                            for (int i = 0; i < lstMarketDataSnapshot.Count; i++)
                            {
                                if ((lstMarketDataSnapshot[i].SettlType == "2") || (lstMarketDataSnapshot[i].SettlType == "3") || (lstMarketDataSnapshot[i].SettlType == "2,3") || (lstMarketDataSnapshot[i].SettlType == "3,2"))
                                {
                                    if (this.IsBid == (lstMarketDataSnapshot[i].MDEntryType == "1" ? true : false))
                                    {
                                        isfoundsettle = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (insettle[j].Contains("."))
                        {
                            for (int i = 0; i < lstMarketDataSnapshot.Count; i++)
                            {
                                if (insettle.Contains(lstMarketDataSnapshot[i].SettlType))
                                {
                                    if (this.IsBid == (lstMarketDataSnapshot[i].MDEntryType == "1" ? true : false))
                                    {
                                        isfoundsettle = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (isfoundsettle)
                    {
                        break;
                    }
                }
            }

            return isfoundsettle;
        }


        private decimal ConvertDecimal(string instr)
        {
            decimal d = 0;
            if (string.IsNullOrEmpty(instr))
            {
                return 0;
            }
            if (decimal.TryParse(instr, out d))
            {
                return d;
            }

            return 0;
        }
        // to do ...
        /////// <summary>
        /////// 做市报价（SOR）回调
        /////// </summary>
        /////// <param name="obj"></param>
        ////private void Instance_OnMDPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        ////{
        ////    if (!string.IsNullOrEmpty(InSecurity) && InSecurity.Equals(obj.SecurityID))
        ////    {
        ////        manualevent.Set();
        ////    }
        ////}


        private void MyWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //if (payMoneyThread.IsAlive)
            //{
            //    payMoneyThread.Abort();
            //}

            // to do ...
            ////PriceHandler.Instance.UnregisterCTPrice(this.Security, Instance_OnMDPrice);
            // 订阅报价
            _sendPriceAdapter.OnMDPrice -= _sendPriceAdapter_OnMDPrice;

        }


        private void RefreshUI()
        {
            sendPriceItem.AmountBoxIndex = 0;
            sendPriceItem.BestBid = null;
            sendPriceItem.BestBidFloat = sendPriceItem.BestBidInt = string.Empty;
            sendPriceItem.BestOfr = null;
            sendPriceItem.BestOfrFloat = sendPriceItem.BestOfrInt = string.Empty;
            sendPriceItem.AmountBoxIndex = 0;
            sendPriceItem.Appoint = false;
            sendPriceItem.ChannelIndex = -1;
            //sendPriceItem.CustomPrice = string.Empty;
            //sendPriceItem.CustomSettleDate = string.Empty;
            //NetPrice = string.Empty;
            sendPriceItem.NonfirmTime = string.Empty;
            DrpSpotIndex = -1;
            //sendPriceItem.SettleType = string.Empty;
            sendPriceItem.SymbolShortname = string.Empty;
            sendPriceItem.WholeQuantitySwitch = false;

        }


        private void OnCancelClick()
        {
            if (myWindow != null)
                myWindow.Close();
        }

        private string GetSetttle()
        {
            string settle = string.Empty;
            // 如果是正常点价
            if (ListPointTypeIndex == 0)
            {
                if (NormalAddOneOpenedTag == "1")
                    settle = AddOneNotLimitCheck ? "2,3" : AddOneTodayCheck ? "2" : AddOneTomorrowCheck ? "3" : "2,3";
                if (NormalAddOneOpenedTag == "0")
                {
                    if (NormalSpotIndex >= 0)
                        settle = DictSettleBox[NormalSpotIndex];
                    else
                    {
                        settle = "2,3";
                    }
                }
            }
            else
            {
                // 如果是定向回价
                if (DrpAddOneOpenedTag == "1")
                {
                    if (NormalSettleBoxIndex == 0)
                    {
                    }
                    else if (NormalSettleBoxIndex == 1)
                    {
                        settle = "2";
                    }
                    else if (NormalSettleBoxIndex == 2)
                    {
                        settle = "3";
                    }
                }
                else
                {
                    if (DrpSpotIndex == 0)
                    {
                        settle = "1";
                    }
                    else if (DrpSpotIndex == -1)
                    {
                    }
                    else
                    {
                        if (FwdSelectDate != null && FwdSelectDate.HasValue)
                        {
                            settle = FwdSelectDate.Value.ToString("MM.dd") + (FwdComboBoxSelectIndex == 0 ? "+0" : "+1");
                        }
                    }
                }

                if (string.IsNullOrEmpty(settle))
                    settle = "2,3";//默认+1
            }

            return settle;
        }

        private BaseReplyPo GetErrorTxt(BaseReplyPo reply)
        {
            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10003";
            por.DoubleWarn = false;
            por.Reason = reply.Msg;

            result.Add(por);

            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

            return reply;
        }

        private void SendPointPrice(object newOrder)
        {
            try
            {
                PResult result = OrdersHandler.Instance.PlaceOrder(newOrder as NewOrderSingle);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        if (!result.IsSuccess())
                        {
                            if (result.Code == "10000")
                            {
                                // to do ...
                                ////RiskViewUI warnView = new RiskViewUI(result, newOrder as NewOrderSingle);
                                ////warnView.Owner = Application.Current.MainWindow;
                                ////warnView.Show();
                                SendPriceWarnView warnView = new SendPriceWarnView(result, newOrder as NewOrderSingle);
                                warnView.Owner = Application.Current.MainWindow;
                                warnView.Show();
                            }
                            else
                            {
                                MessageBoxGUI msgBox = new MessageBoxGUI();
                                msgBox.Owner = this.myWindow;
                                msgBox.setWarnMsg(result.Msg);
                                msgBox.ShowDialog();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                    finally
                    {
                        OnCancelClick();
                    }
                }));
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        private string GetPermission()
        {
            string result = string.Empty;
            bool isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_ALL_ACTIONID);
            if (isAllow)
            {
                result = "1";
            }
            else
            {
                isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_ACTIONID);
                if (isAllow)
                {
                    result = "2";
                }
                else
                {
                    isAllow = AdminHandler.Instance.CheckPermission(MODULEID, CHECK_INNER_XBOND_ACTIONID);
                    result = "3";
                }
            }
            return result;
        }

        private bool CheckBondType()
        {
            return _bondsInfoPo != null && !string.IsNullOrEmpty(_bondsInfoPo.MarketIndicator) && _bondsInfoPo.MarketIndicator.Equals("4");
        }

        private bool CheckParam()
        {
            bool result = true;
            decimal temD = 0;
            if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)//(string.IsNullOrEmpty(CustomPrice))
                result = false;
            else if (!GetQtyViewModel().TrueAmount.HasValue || temD == 0)//(string.IsNullOrEmpty(sendPriceItem.Amount))
                result = false;
            
            //定向报价，检查输入的时间
            if (this.ListPointTypeIndex == 1)
            {
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(ValidMinute))
                {
                    totalsecond = int.Parse(ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(ValidSecond))
                {
                    totalsecond += int.Parse(ValidSecond);
                }

                if (totalsecond < this.PendingCancelTime)
                {
                    result = false;
                }
            }
            
            return result;
        }

        private BaseReplyPo GetErrorTxt()
        {
            BaseReplyPo reply = new BaseReplyPo();
            reply.Code = "10001";

            List<TPlaceOrderResult> result = new List<TPlaceOrderResult>();

            TPlaceOrderResult por = new TPlaceOrderResult();
            por.Code = "10003";
            por.DoubleWarn = false;
            decimal temD = 0;

            if (!decimal.TryParse(CustomPrice, out temD) || temD == 0)// string.IsNullOrEmpty(CustomPrice))
                por.Reason = "未填写价格或价格为0";
            else if (!GetQtyViewModel().TrueAmount.HasValue || temD == 0)// string.IsNullOrEmpty(sendPriceItem.Amount))
                por.Reason = "未填写量或量为0";

            //定向报价，检查输入的时间
            if (this.ListPointTypeIndex == 1)
            {
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(ValidMinute))
                {
                    totalsecond = int.Parse(ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(ValidSecond))
                {
                    totalsecond += int.Parse(ValidSecond);
                }

                if (totalsecond < this.PendingCancelTime)
                {
                    por.Reason = $"<有效时长>需大于等于 {PendingCancelTime}s";
                }
            }
            //decimal d = 0;
            //if (decimal.TryParse(this.AmountText, out d) && (d % 100) > 0)
            //    por.Reason = string.Format("量必须是100的整数倍，当前输入值：{0}", this.AmountText);

            result.Add(por);

            reply.Msg = MsgHelper.Serializer<List<TPlaceOrderResult>>(result);

            return reply;
        }

        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle newOrder = new NewOrderSingle();
            // Add TradType
            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;
            //if (SystemParamHandler.Instance.GetUserSetting() != null)
            //{
            //    UserSetting Us = SystemParamHandler.Instance.GetUserSetting();
            //    if (!string.IsNullOrEmpty(Us.TradeType))
            //    {
            //        tradeType = Us.TradeType.ToLower();
            //    }
            //}

            if (tradeType == "1")
            {
                if (!string.IsNullOrEmpty(_currentSource) && _currentSource == "QDMESP")
                {
                    newOrder.AlgoName = "STP";
                    newOrder.VenueTypeGW = "QDMESP";
                    newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(_currentId))
                        newOrder.QuoteID = _currentId;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(_trsactTime))
                        newOrder.Info2 = _trsactTime;

                    if (!string.IsNullOrEmpty(_deliveryType))
                        newOrder.DeliveryType = _deliveryType;

                    if (!string.IsNullOrEmpty(_clearingMethod))
                        newOrder.ClearingMethod = _clearingMethod;

                    if (!string.IsNullOrEmpty(_quoteType))
                        newOrder.QuoteType = _quoteType;
                }
                else
                {
                    newOrder.AlgoName = "CROSS";
                    newOrder.VenueTypeGW = "INNER";
                    newOrder.Venues = "MANUAL";
                }
            }
            else if (tradeType == "2")
            {
                newOrder.AlgoName = "STP";
                if (!string.IsNullOrEmpty(this._currentVenueTypeGW) && this._currentVenueTypeGW == "CDH")
                    newOrder.VenueTypeGW = "SOR";
                else
                    newOrder.VenueTypeGW = this._currentVenueTypeGW;

                newOrder.Venues = this._currentSource;

                if (!string.IsNullOrEmpty(_currentSource) && _currentSource == "QDMESP")
                {
                    //newOrder.AlgoName = "STP";
                    //newOrder.VenueTypeGW = "QDMESP";
                    //newOrder.Venues = "QDMESP";
                    if (!string.IsNullOrEmpty(_currentId))
                        newOrder.QuoteID = _currentId;//_currentId.Substring(0, _currentId.Length - 1);

                    if (!string.IsNullOrEmpty(_trsactTime))
                        newOrder.Info2 = _trsactTime;

                    if (!string.IsNullOrEmpty(_deliveryType))
                        newOrder.DeliveryType = _deliveryType;

                    if (!string.IsNullOrEmpty(_clearingMethod))
                        newOrder.ClearingMethod = _clearingMethod;

                    if (!string.IsNullOrEmpty(_quoteType))
                        newOrder.QuoteType = _quoteType;
                }
                else
                {
                    if (!string.IsNullOrEmpty(_currentId))
                        newOrder.QuoteID = _currentId;
                }
            }

            //根据模式重新修改参数 0 直通，OCO  1 DQ
            if (this.ListPointTypeIndex == 1)
            {
                newOrder.AlgoName = "DQ";
                newOrder.VenueTypeGW = this._currentVenueTypeGW;
                if (!string.IsNullOrEmpty(_currentId))
                    newOrder.QuoteID = _currentId;
                int totalsecond = 0;
                if (!string.IsNullOrEmpty(ValidMinute))
                {
                    totalsecond = int.Parse(ValidMinute) * 60;
                }

                if (!string.IsNullOrEmpty(ValidSecond))
                {
                    totalsecond += int.Parse(ValidSecond);
                }

                if (totalsecond == 0)
                {
                    if (!string.IsNullOrEmpty(IocOutTime))
                    {
                        newOrder.ValidUntilTime = DateTimeEx.GetNow().AddSeconds(int.Parse((!string.IsNullOrEmpty(IocOutTime))? IocOutTime : "30")).ToString("yyyy-MM-dd HH:mm:ss.fff");
                        newOrder.IocTimeOut = (!string.IsNullOrEmpty(IocOutTime)) ? IocOutTime : "30";
                    }
                }
                else
                {
                    newOrder.ValidUntilTime = DateTimeEx.GetNow().AddSeconds(totalsecond).ToString("yyyy-MM-dd HH:mm:ss.fff");
                    newOrder.IocTimeOut = totalsecond.ToString();
                }

                DqInfo1Model otm = new DqInfo1Model() {CnPartySubName = this.PartySubName, CnBidOrOfr = this.BidOrOfr, ValidTimeType = "", Price = this.ClickPrice, Amount = ClickAmount, Wholeflag = WholeFlag };
                newOrder.Info1 = JsonConvert.SerializeObject(otm);
            }
            else
            {
                if (LockTimeOutHeight > 0 && LockTimeOutCheck && !string.IsNullOrEmpty(IocOutTime))
                {
                    newOrder.IocTimeOut = IocOutTime;
                }
            }


            if (_bondsInfoPo != null)
                newOrder.MarketIndicator = _bondsInfoPo.MarketIndicator;
            else
                newOrder.MarketIndicator = "4";

            newOrder.UserName = LocalEnv.UserInfo.UserName;
            if (CheckBondType())
                newOrder.NetPrice = netPrice;
            else
                newOrder.NetPrice = "100";

            newOrder.CrossVenues = GetPermission();//_checkAll ? "1" : _checkInner ? "2" : string.Empty;
            newOrder.ClOrdID = string.IsNullOrEmpty(this.ClOrdID) ? DataTimeSequence.Instance.GetId() : this.ClOrdID;//"c" + DateTime.Now.Ticks;
            newOrder.SecurityID = sendPriceItem.Security;
            //newOrder.ValidUntilTime = DateTime.Now.AddHours(3).ToString("yyyy-MM-dd HH:mm:ss");
            newOrder.Side = this.IsBid ? "1" : "2";
            newOrder.Price = CustomPrice;//"4.2";
            newOrder.OrdType = "2";
            newOrder.PriceType = "3"; //收益率
            newOrder.OrderQty = GetQtyViewModel().TrueAmount.ToString();
            newOrder.TimeInForce = "3";  //1=DAY，3=IOC
            newOrder.SettlType = GetSetttle();
            newOrder.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            //选中：0(nonfirm)，未选中：1(firm)
            //newOrder.FirmFlag = _isFirm ? "1" : "0";
            newOrder.FirmFlag = firmChecked ? "0" : "1";
            newOrder.MacID = EngineHelper.GetMacByNetworkInterface();
            newOrder.InsideIP = EngineHelper.GetIp();
            newOrder.HardDiskID = EngineHelper.Getharddiskid2();
            newOrder.WholeFlag = _wholeflag;

            newOrder.Info4 = Info4;


            if (LocalEnv.ConfInfo.IsAccountUser == "1")
            {
                newOrder.AccountID = SelectedItem;
                newOrder.AccountName = SelectedText;
            }

            if (MarketIndicator == "2")
            {
                newOrder.SettlType = null;
                newOrder.NetPrice = null;
                newOrder.WholeFlag = null;
            }
            //点价交易对手信息为价格的本方信息
            newOrder.CounterPartyID = _partyId;
            newOrder.CounterPartyName = _partyName;
            com.gateway.log.LogHelper.Info($"Now:{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ServerTicks:{LocalEnv.ServerInfo.Add_ServerTicks} Server:{DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
            return newOrder;
        }

        private async void OnConfirmClick()
        {
            if (!CheckParam())
            {
                // to do ...
                ////RiskViewUI warnView = new RiskViewUI(GetErrorTxt(), GetNewOrderSingle());
                ////warnView.Owner = myWindow;
                ////warnView.Show();
                SendPriceWarnView warnView = new SendPriceWarnView(GetErrorTxt(), GetNewOrderSingle());
                warnView.Owner = myWindow;
                warnView.Show();
            }
            else
            {
                if (Ocr != null)
                {
                    //定向报价撤单
                    Ocr.Info1 = "4";
                    STC.Common.Model.Fix.Message.PResult res = await Common.ViewModel.Drp.Utils.DRPTools.CancelOrder(Ocr);
                    if (res != null)
                    {
                        if (res.Code == "0000")
                        {
                        }
                        else
                        {
                            MessageBoxGUI msgBox = new MessageBoxGUI();
                            msgBox.Owner = this.myWindow;
                            msgBox.setWarnMsg(string.Format($"撤销定向报价订单失败, 原因:{res.Msg}."));
                            msgBox.ShowDialog();
                            return;
                        }
                    }
                }

                NewOrderSingle newOrder = GetNewOrderSingle();

                ThreadPoolHelper.InvokeThreadPool(SendPointPrice, newOrder);
            }
        }

        private void CloseClick()
        {
            myWindow.Close();
        }

        private void AppointClick()
        {
            if (sendPriceItem.Appoint)
                sendPriceItem.ChannelIndex = 0;
            else
                sendPriceItem.ChannelIndex = -1;
        }

        private void SetOtherVisibile()
        {
            if (DrpSpotIndex == 0 || DrpSpotIndex == -1)
            {
                SendPriceItem.DateVisible = Visibility.Collapsed;
                SendPriceItem.SettleTypeVisible = Visibility.Collapsed;

            }
            else if (DrpSpotIndex == 1)
            {
                SendPriceItem.DateVisible = Visibility.Visible;
                SendPriceItem.SettleTypeVisible = Visibility.Visible;
            }
        }


        private void OnBidClick()
        {
            IsBid = true;
            OfrOrBid = "0";
            sendPriceItem.RandomNum = string.Empty;
            FirmChecked = false;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.ShowOfrEffect = (Effect)myWindow.FindResource("OfrEffect");
            sendPriceItem.ShowBidEffect = null;
            GetQtyViewModel().BorderBH = sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("OfrLinearGradientBrush");
            BidOrOfrTextBlockForground = (Brush)myWindow.FindResource("ColorBrush.Blue8");
            TabTitleBG = (Brush)myWindow.FindResource("TabOfrBackground");
        }

        private void OnOfrClick()
        {
            IsBid = false;
            OfrOrBid = "1";
            FirmChecked = false;
            sendPriceItem.RandomNum = string.Empty;
            sendPriceItem.BidBG = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.OfrBG = (Brush)myWindow.FindResource("ColorBrush.Transparent");
            sendPriceItem.ShowBidEffect = (Effect)myWindow.FindResource("BidEffect");
            sendPriceItem.ShowOfrEffect = null;
            GetQtyViewModel().BorderBH = sendPriceItem.BorderBH = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            sendPriceItem.BorderBG = (Brush)myWindow.FindResource("BidLinearGradientBrush");
            BidOrOfrTextBlockForground = (Brush)myWindow.FindResource("ColorBrush.Orange5");
            TabTitleBG = (Brush)myWindow.FindResource("TabBidBackground");
        }

        private void OnRefreshClick()
        {

        }

        private void UpdateUiBestBidOrOfrForground()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string bestBrokerBid = this.SendPriceItem.BestBrokerBid == null ? "--" : this.SendPriceItem.BestBrokerBid.ToString();
                string bestBrokerOfr = this.SendPriceItem.BestBrokerOfr == null ? "--" : this.SendPriceItem.BestBrokerOfr.ToString();
                string bestBid = this.SendPriceItem.BestBid == null ? "--" : this.SendPriceItem.BestBid.ToString();
                string bestOfr = this.SendPriceItem.BestOfr == null ? "--" : this.SendPriceItem.BestOfr.ToString();
                // 比较最优价，修改前景色
                if (!string.IsNullOrEmpty(this.CustomPrice))
                {
                    // 判断 Ofr
                    if (IsBid)
                    {
                        if (bestOfr != CustomPrice)
                        {
                            AllBestOfrForground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            AllBestOfrForground = new SolidColorBrush(Color.FromRgb(0x00, 0x9A, 0xFF));
                        }
                    }
                    else
                    {
                        if (bestBid != CustomPrice)
                        {
                            AllBestBidForground = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            AllBestBidForground = new SolidColorBrush(Color.FromRgb(0xFB, 0x94, 0x3C));
                        }
                    }
                }
                else
                {
                    AllBestBidForground = new SolidColorBrush(Color.FromRgb(0xFB, 0x94, 0x3C));
                    AllBestOfrForground = new SolidColorBrush(Color.FromRgb(0x00, 0x9A, 0xFF));
                }

                // to do
                // 根据报价判断是否有该价格，已修改点价的有效性
            }));
        }

        #region 计算器
        private bool isSendReq = false;
        private string calBondErrMsg = string.Empty;
        private string _lastReqData = string.Empty;
        private bool _needCalPrice = true;
        private object _calLock = new object();
        private CalResult _lastCalResult = null;
        /// <summary>
        /// 计算收益率或者净价
        /// </summary>
        /// <param name="calType"></param>
        /// <param name="settleType"></param>
        /// <param name="val"></param>
        private void CalBond(string calType, string settleType, string val)
        {
            calBondErrMsg = string.Empty;
            decimal d = 0;
            if (string.IsNullOrEmpty(val) || string.IsNullOrEmpty(settleType) || !decimal.TryParse(val, out d) || !_needCalPrice)
                return;

            //isSendReq = true;

            _lastReqData = string.Format("{0}-{1}-{2}", calType, settleType, val);
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalReq requst = new CalReq();
                    requst.SecurityID = sendPriceItem.Security;
                    requst.CalType = calType;//"4";
                    requst.SettlType = settleType;// "1";
                    requst.Value = val;// customPrice;
                    if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.CalType))
                        requst.CalPath = LocalEnv.ConfInfo.CalType;

                    CalResult result = CalHandler.Instance.CalBond(requst);
                    if (requst != null)
                    {
                        if (result != null && result.Data != null && result.IsSuccess())
                        {
                            result.ReqData = string.Format("{0}-{1}-{2}", requst.CalType, requst.SettlType, requst.Value);

                            if (_lastReqData.Equals(result.ReqData))
                            {
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    if (result.IsSuccess() && sendPriceItem.CalWarnVisibility == Visibility.Visible)
                                        sendPriceItem.CalWarnVisibility = Visibility.Collapsed;

                                    lock (_calLock)
                                    {
                                        _lastCalResult = result;

                                    }

                                    if (calType.Equals("4"))
                                    {
                                        //NetPrice = result.Data.CleanPrice.ToString("F4");

                                        netPrice = result.Data.CleanPrice.ToString("F4");
                                        this.OnPropertyChanged("NetPrice");
                                    }
                                    else if (calType.Equals("2"))//净价
                                    {
                                        if (result.IsSuccess())
                                        {
                                            customPrice = (result.Data.Yield * 100).ToString("F4");

                                            this.OnPropertyChanged("CustomPrice");
                                        }
                                    }

                                    isSendReq = false;
                                }));
                            }
                        }
                        else
                        {
                            lock (_calLock)
                            {
                                _lastCalResult = result;
                            }
                        }
                    }
                    else
                    {
                        isSendReq = false;
                        calBondErrMsg = result.Msg;
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendReq = false;
                }
                finally
                {
                    //isSendReq = false;
                }
            });
        }

        private bool isSendCalDate = false;
        /// <summary>
        /// 计算清算时间其他模式下的日期
        /// </summary>
        /// <param name="settleType"></param>
        /// <param name="isTomorrow"></param>
        private void CalDate(string settleType, bool isTomorrow)
        {
            if (isSendCalDate)
                return;

            isSendCalDate = true;
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    CalWeekDateReq requst = new CalWeekDateReq();
                    requst.settlementType = settleType;// "1";

                    CalWeekDateResult result = CalHandler.Instance.calSettlementDate(requst);
                    if (result != null)
                    {
                        if (result.Data != null)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                try
                                {
                                    string year = result.Data.dealDate.Substring(0, 4);
                                    string month = result.Data.dealDate.Substring(4, 2);
                                    string day = result.Data.dealDate.Substring(6, 2);
                                    FwdSelectDate = CommonUtils.ConvertToDateTime(year, month, day);
                                    SendPriceItem.CustomSettleDate = string.Format("{0}-{1}-{2}", year, month, day);
                                }
                                catch (Exception e) { }

                                FwdComboBoxSelectIndex = isTomorrow ? 0 : 1;
                                SendPriceItem.SettleType = isTomorrow ? "T+0" : "T+1";

                                isSendCalDate = false;
                            }));
                        }
                    }
                    else
                        isSendCalDate = false;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                    isSendCalDate = false;
                }
            });
        }


        /// <summary>
        /// 更新最优行情
        /// </summary>
        private void UpdateBestPrice()
        {
            if (_sendPriceAdapter != null)
            {
                string settle = GetSetttle();
                _sendPriceAdapter.SetFilterParam(false, settle);
            }
        }

        #endregion

        #region 账户组合

        private Visibility _accountUserVisibility = Visibility.Collapsed;

        public Visibility AccountUserVisibility
        {
            get { return _accountUserVisibility; }
            set
            {
                _accountUserVisibility = value;
                this.OnPropertyChanged();
            }
        }

        //private AccountUserItem _accountUserItem = null;

        //public AccountUserItem AccountUserItem

        //{
        //    get { return _accountUserItem; }
        //    set
        //    {
        //        _accountUserItem = value;
        //        this.OnPropertyChanged();
        //    }
        //}

        //private ObservableCollection<AccountUserItem> _accountUserSource = new ObservableCollection<AccountUserItem>();

        //public ObservableCollection<AccountUserItem> AccountUserSource
        //{
        //    get
        //    {
        //        return _accountUserSource;
        //    }
        //}


        internal void InitAccountUsers()
        {
            if (LocalEnv.ConfInfo.IsAccountUser != "1")
                return;

            AccountUserVisibility = Visibility.Visible;

            SetDefaultAccountUsers();

            ObservableCollection<TBookBookTreeItemModel> BookList = AdminHandler.Instance.GetTCurrentBookCollection(LocalEnv.UserInfo.UserName);
            MultiTreeComboxItemsSource = CommonUtils.DeserializeObject<ObservableCollection<TreeNodeModel>>(CommonUtils.SerializeObject(BookList));
            
        }

        internal void SetDefaultAccountUsers()
        {
            if (LocalEnv.ConfInfo.IsAccountUser != "1")
                return;

            string defaultAccountID = string.Empty;
            UserSetting Us = SystemParamHandler.Instance.GetUserSetting();

            if (Us != null && !string.IsNullOrEmpty(Us.inf2))
            {
                if (JsonSplit.IsJson(Us.inf2))
                {
                    ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(Us.inf2);
                    defaultAccountID = ck.BookId;
                }
            }
            SelectedItem = defaultAccountID;
        }

        private ObservableCollection<TreeNodeModel> _TreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get { return _TreeComboxItemsSource; }
            set
            {
                if (_TreeComboxItemsSource != value)
                {
                    _TreeComboxItemsSource = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private string _SelectedItem = null;
        public string SelectedItem
        {
            get { return _SelectedItem; }
            set
            {
                _SelectedItem = value;
                this.OnPropertyChanged();
            }
        }


        private string _SelectedText = null;
        public string SelectedText
        {
            get { return _SelectedText; }
            set
            {
                _SelectedText = value;
                this.OnPropertyChanged();
            }
        }

        #endregion

        #region DRP Function 
        /// <summary>
        /// 解析个人设置的秒数为分秒
        /// </summary>
        /// <param name="sec"></param>
        private void ParseSecond(string sec)
        {
            if (string.IsNullOrEmpty(sec))
            {
                validMinute = string.Empty;
                ValidSecond = "30";
            }
            else
            {
                string result = DRPTools.FormatTime(int.Parse(sec));
                //result = $"{hh}小时{mm}分钟{relsec}秒";
                //result = $"{mm}分钟{relsec}秒";
                //result = $"{relsec}秒";
                if (result.Contains("小时"))
                {
                    int h = int.Parse(Regex.Split(result, "小时")[0]);
                    int m = int.Parse(Regex.Split(Regex.Split(result, "小时")[1], "分钟")[0]);
                    ValidMinute = (h * 60 + m).ToString();
                    ValidSecond = Regex.Split(result, "分钟")[1].Replace("秒", "");
                }
                else if (result.Contains("分钟") && result.Contains("秒"))
                {
                    ValidMinute = Regex.Split(result, "分钟")[0];
                    ValidSecond = Regex.Split(result, "分钟")[1].Replace("秒", "");
                }
                else
                {
                    ValidMinute = "";
                    ValidSecond = result.Replace("秒","");
                }

            }

        }

        /// <summary>
        /// 点击最优价
        /// </summary>
        private void OnClickBestBidCommand()
        {
            if (ListPointTypeIndex == 1)
            {
                CustomPrice = SendPriceItem.BestBid?.ToString("N4");
            }
        }

        /// <summary>
        /// 点击最优价
        /// </summary>
        private void OnClickBestOfrCommand()
        {
            if (ListPointTypeIndex == 1)
            {
                CustomPrice = SendPriceItem.BestOfr?.ToString("N4");
            }
        }


        #endregion
    }
}
