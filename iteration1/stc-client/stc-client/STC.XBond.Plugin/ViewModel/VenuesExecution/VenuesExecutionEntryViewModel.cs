using STC.XBond.Plugin.Utils;
using STC.Common.Handler.Inner;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Inner;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Component;
using System.Windows.Controls;
using com.gateway.log;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 渠道执行录入界面VenuesExecutionEntryView的数据内容
    /// </summary>
    public class VenuesExecutionEntryViewModel : ViewModelBase
    {
        private string _commonStr = "--";
        private readonly string EXECUTE = "录入成交";
        private readonly string CANCELRESULT = "撤销结果";
        private readonly string CANCEL = "取消";
        private readonly string CONFIRM = "确认";
        private readonly string CONFIRMSUCCES = "撤销成功";
        private readonly string CANCELFAIL = "撤销失败";
        private readonly string CANCELFAIL_EXEC = "撤销失败，已成交";

        private bool _isCancel = false;
        private Window _myWindow = null;
        private ExecutionReport _currentOrder = null;

        private bool _isFromCancel = false;

        #region 界面显示字段

        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;

                this.OnPropertyChanged();
            }
        }

        private string _side = string.Empty;
        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _sideBrush;
        public Brush SideBrush
        {
            get { return _sideBrush; }
            set
            {
                _sideBrush = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _cancelBrush;
        public Brush CancelBrush
        {
            get { return _cancelBrush; }
            set
            {
                _cancelBrush = value;

                this.OnPropertyChanged();
            }
        }

        private Style _cancelStyle;
        public Style CancelStyle
        {
            get { return _cancelStyle; }
            set
            {
                _cancelStyle = value;

                this.OnPropertyChanged();
            }
        }

        private Style _confirmStyle;
        public Style ConfirmStyle
        {
            get { return _confirmStyle; }
            set
            {
                _confirmStyle = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _confirmBrush;
        public Brush ConfirmBrush
        {
            get { return _confirmBrush; }
            set
            {
                _confirmBrush = value;

                this.OnPropertyChanged();
            }
        }

        private string _goodsCode = string.Empty;
        public string GoodsCode
        {
            get { return _goodsCode; }
            set
            {
                _goodsCode = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _venueBGBrush;
        public Brush VenueBGBrush
        {
            get { return _venueBGBrush; }
            set
            {
                _venueBGBrush = value;

                this.OnPropertyChanged();
            }
        }

        private string _venue = string.Empty;
        public string Venue
        {
            get { return _venue; }
            set
            {
                _venue = value;

                this.OnPropertyChanged();
            }
        }

        private string _price = string.Empty;
        public string Price
        {
            get { return _price; }
            set
            {
                _price = value;

                this.OnPropertyChanged();
            }
        }

        private string _amount = string.Empty;
        public string Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;

                this.OnPropertyChanged();
            }
        }

        private string _countpartyName = string.Empty;
        public string CountpartyName
        {
            get { return _countpartyName; }
            set
            {
                _countpartyName = value;

                this.OnPropertyChanged();
            }
        }

        private string _countpartyExchange = string.Empty;
        public string CountpartyExchange
        {
            get { return _countpartyExchange; }
            set
            {
                _countpartyExchange = value;

                this.OnPropertyChanged();
            }
        }

        private int _rowHeightLabel;
        public int RowHeightLabel
        {
            get { return _rowHeightLabel; }
            set
            {
                _rowHeightLabel = value;

                this.OnPropertyChanged();
            }
        }

        private int _rowHeightBox;
        public int RowHeightBox
        {
            get { return _rowHeightBox; }
            set
            {
                _rowHeightBox = value;

                this.OnPropertyChanged();
            }
        }

        private int _cancelBtnWidth;
        public int CancelBtnWidth
        {
            get { return _cancelBtnWidth; }
            set
            {
                _cancelBtnWidth = value;

                this.OnPropertyChanged();
            }
        }

        private int _confirmBtnWidth = 100;
        public int ConfirmBtnWidth
        {
            get { return _confirmBtnWidth; }
            set
            {
                _confirmBtnWidth = value;

                this.OnPropertyChanged();
            }
        }


        private string _cancelTxt = string.Empty;
        public string CancelTxt
        {
            get { return _cancelTxt; }
            set
            {
                _cancelTxt = value;

                this.OnPropertyChanged();
            }
        }

        private string _confirmTxt = string.Empty;
        public string ConfirmTxt
        {
            get { return _confirmTxt; }
            set
            {
                _confirmTxt = value;

                this.OnPropertyChanged();
            }
        }


        private ComboBoxItem _sendPriceType;
        public ComboBoxItem SendPriceType
        {
            get
            {
                return _sendPriceType;
            }

            set
            {
                _sendPriceType = value;

                this.OnPropertyChanged();


                if (value != null && value.Tag != null)
                {
                    string tag = value.Tag as string;
                    if (!string.IsNullOrEmpty(tag) && (tag == "4" || tag == "50"))
                    {
                        ExchangeTxtEnable = false;
                        CountpartyExchange = _commonStr;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(CountpartyExchange) && CountpartyExchange == _commonStr)
                            CountpartyExchange = string.Empty;

                        ExchangeTxtEnable = true;

                    }
                }

            }
        }

        private DateTime? _selectDate;
        public DateTime? SelectDate
        {
            get
            {
                return _selectDate;
            }

            set
            {
                _selectDate = value;

                this.OnPropertyChanged();
            }
        }

        private int _selectSetle;
        public int SelectSetle
        {
            get
            {
                return _selectSetle;
            }

            set
            {
                _selectSetle = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _positionVisibility;
        public Visibility PositionVisibility
        {
            get
            {
                return _positionVisibility;
            }

            set
            {
                _positionVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private string _postionTxt;
        public string PostionTxt
        {
            get
            {
                return _postionTxt;
            }

            set
            {
                _postionTxt = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _sendPriceTypeVisibility;
        public Visibility SendPriceTypeVisibility
        {
            get
            {
                return _sendPriceTypeVisibility;
            }

            set
            {
                _sendPriceTypeVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _settleTypeVisibility;
        public Visibility SettleTypeVisibility
        {
            get
            {
                return _settleTypeVisibility;
            }

            set
            {
                _settleTypeVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private bool _exchangeTxtEnable = true;
        public bool ExchangeTxtEnable
        {
            get { return _exchangeTxtEnable; }
            set
            {
                _exchangeTxtEnable = value;

                this.OnPropertyChanged();
            }
        }

        #endregion

        #region 命令事件

        public ICommand CancelCommand { get { return new RelayCommand(OnCancelClick); } }
        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }
        public ICommand OkCommand { get { return new RelayCommand(ConfirmClick); } }

        private void OnCancelClick()
        {
            _myWindow.Close();
        }

        private void CloseClick()
        {
            if (_isCancel)
            {
                //ThreadPoolHelper.InvokeThreadPool(CancelFail, GetInnerOrder("8"));//撤销失败
                ThreadPoolHelper.InvokeThreadPool(CancelSuccess, GetInnerOrder("4"));//已撤销
            }

            _myWindow.Close();
        }

        private void DealReply(PResult result)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!result.IsSuccess())
                {
                    Common.View.Component.CustomMessageBox.Warning(result.Msg);
                }
            }));
        }

        private void CancelFail()
        {
            VenuesExecutionEntryView view = new VenuesExecutionEntryView(_currentOrder, false, true);
            view.Show();
            
            //try
            //{
            //    PResult result = InnerHandler.Instance.CancelOrder(orderReq as InnerOrderReq);
            //}
            //catch (Exception e)
            //{
            //    LogHelper.Error(e);
            //}
        }

        private void ConfirmClick()
        {
            WaitCallback fun = null;
            InnerOrderReq orderReq = null;

            if (_isCancel)
            {
                //fun = CancelSuccess;
                CancelFail();
                //orderReq = GetInnerOrder("4");//已撤销
            }
            else
            {
                if (!_isFromCancel)
                {
                    if (!CheckParam())
                    {
                        //CustomMessageBox.Error(string.Format("量必须是100的整数倍且在范围（1000,1000000），当前输入值：{0}", Amount));

                        return;
                    }
                }

                fun = ConfirmOrder;
                if (_isFromCancel)
                {
                    if (!CheckParam())
                    {
                        //CustomMessageBox.Error(string.Format("量必须是100的整数倍且在范围（1000,1000000），当前输入值：{0}", Amount));

                        return;
                    }
                    orderReq = GetInnerOrder("7");//撤销中
                }
                else
                    orderReq = GetInnerOrder(string.Empty);

                ThreadPoolHelper.InvokeThreadPool(fun, orderReq);
            }
            
            _myWindow.Close();
        }

        private bool CheckParam()
        {
            bool result = true;
            decimal d = 0;

            if (string.IsNullOrEmpty(Price) || string.IsNullOrEmpty(Amount) || string.IsNullOrEmpty(CountpartyName)
                || SelectDate == null || SelectSetle < 0)
            {
                result = false;
                Common.View.Component.CustomMessageBox.Error("成交价/成交量/报价机构/结算方式未输入");
            }

            if (decimal.TryParse(Amount, out d) && (d % 100 > 0 || d < 1000))
            {
                result = false;
                Common.View.Component.CustomMessageBox.Error(string.Format("量必须是100的整数倍且在范围（1000,1000000），当前输入值：{0}", Amount));
            }
            return result;
        }

        private void ConfirmOrder(object orderReq)
        {
            try
            {
                PResult result = InnerHandler.Instance.TransactionOrder(orderReq as InnerOrderReq);

                DealReply(result);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        private void CancelSuccess(object orderReq)
        {
            try
            {
                PResult result = InnerHandler.Instance.CancelOrder(orderReq as InnerOrderReq);

                DealReply(result);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        #endregion

        private string GetSettle()
        {
            string result = string.Empty;
            if (SelectDate != null && SelectDate.HasValue)
            {
                if (SettleParseHelper.IsSpotSettle(SelectDate, SelectSetle))
                    result = SettleParseHelper.IsTomorrow(SelectDate, SelectSetle) ? SettleParseHelper.IsToday(SelectDate)?"2":"3":"1";
                else
                    result = SelectDate.Value.ToString("MM.dd") + (SelectSetle == 0 ? "+0" : "+1");
            }

            //if (result == "2" && !string.IsNullOrEmpty(_currentOrder.SettlType) && (_currentOrder.SettlType.Equals("明天+0") || _currentOrder.SettlType.Equals("3")))
            //    result = "3";

            return result;
        }


        private InnerOrderReq GetInnerOrder(string status)
        {
            InnerOrderReq orderReq = new InnerOrderReq();
            orderReq.ClOrdID = _currentOrder.ClOrdID;
            orderReq.ExecType = string.IsNullOrEmpty(status) ? CommonMapUtil.FindEnum(_currentOrder.ExecType, "venuestatus") : status;
            orderReq.OrderID = _currentOrder.OrderID;
            orderReq.Lastpartyname = LocalEnv.UserInfo.FullName;//_currentOrder.UserName;
            orderReq.CounterPartySubName = CountpartyExchange;
            orderReq.CounterPartyName = CountpartyName;
            orderReq.LastQty = Amount.Replace(",", "");
            orderReq.LastPx = Price.Replace(",", "");

            orderReq.Rejecterreason = !string.IsNullOrEmpty(status) ? CommonMapUtil.FindEnum(status, "rejecterreason") : status;

            if (SendPriceTypeVisibility == Visibility.Visible)
                orderReq.TradeMethod = SendPriceType == null ? string.Empty : SendPriceType.Tag as string;
            else
                orderReq.TradeMethod = PostionTxt;

            orderReq.SettlType = GetSettle();

            return orderReq;
        }


        public void Init(Window myWindow, ExecutionReport order, bool isCancel, bool isFromCancel)
        {
            _isCancel = isCancel;

            _isFromCancel = isFromCancel;
            _myWindow = myWindow;
            string goodCode = order.SecurityID;
            
            if (isCancel)
            {
                myWindow.Height = 127;
                myWindow.Width = 300;
                CancelBtnWidth = 100;
                ConfirmBtnWidth = 120;
                RowHeightBox = RowHeightLabel = 0;

                Title = string.Format("{0}-{1}", CANCELRESULT, order.OrderID);

                CancelTxt = CONFIRMSUCCES;
                ConfirmTxt = CANCELFAIL_EXEC;
                
                CancelBrush = (Brush)_myWindow.FindResource("ColorBrush.Red7");
                ConfirmBrush = (Brush)_myWindow.FindResource("ColorBrush.Gray15");

                CancelStyle = (Style)_myWindow.FindResource("SmallConfirmBtnStyle");
                ConfirmStyle = (Style)_myWindow.FindResource("SmallCancelBtnStyle");

                SettleTypeVisibility = Visibility.Collapsed;
                SendPriceTypeVisibility = Visibility.Collapsed;
                PositionVisibility = Visibility.Collapsed;
            }
            else
            {
                if (!string.IsNullOrEmpty(goodCode))
                {
                    if (goodCode.EndsWith("IB"))//银行间债券
                    {
                        SendPriceTypeVisibility = Visibility.Visible;
                        PositionVisibility = Visibility.Collapsed;
                    }
                    else//交易所债券SH,SZ
                    {
                        SendPriceTypeVisibility = Visibility.Collapsed;
                        PositionVisibility = Visibility.Visible;
                    }
                }

                ComboBox comboBox = _myWindow.FindName("cbSendPriceType") as ComboBox;
                if (comboBox != null && comboBox.Items != null && comboBox.Items.Count > 0)
                {
                    SendPriceType = comboBox.Items[0] as ComboBoxItem;
                    comboBox.SelectedIndex = 0;
                }

                if (!string.IsNullOrEmpty(order.SettlType))
                {
                    SelectSetle = order.SettlType.Equals("1") || order.SettlType.Contains("+0") || order.SettlType.Equals("3") ? 0 : order.SettlType.Equals("2") || order.SettlType.Contains("+1") ? 1 : -1;
                    
                }

                SelectDate = new DateTime?(CommonUtils.ConvertToDateTime2(order.SettlDate));

                SettleTypeVisibility = Visibility.Visible;

                myWindow.Height = 194;
                myWindow.Width = 590;

                RowHeightLabel = 20;
                RowHeightBox = 45;
                CancelBtnWidth = 60;
                Title = string.Format("{0}-{1}", EXECUTE, order.OrderID);

                CancelBrush = (Brush)_myWindow.FindResource("ColorBrush.Gray15");
                ConfirmBrush = (Brush)_myWindow.FindResource("ColorBrush.Red7");

                CancelStyle = (Style)_myWindow.FindResource("SmallCancelBtnStyle");
                ConfirmStyle = (Style)_myWindow.FindResource("SmallConfirmBtnStyle");

                CancelTxt = CANCEL;
                ConfirmTxt = CONFIRM;
            }

            
            _currentOrder = order;

            if (order.Side.Equals("1"))
            {
                Side = "Bid";
                SideBrush = (Brush)_myWindow.FindResource("ColorBrush.Orange7");
            }
            else
            {
                Side = "Ofr";
                SideBrush = (Brush)_myWindow.FindResource("ColorBrush.Blue10");
            }

            GoodsCode = order.SecurityID;

            Venue = CommonMapUtil.FindEnum(order.Venues, "venues");

            Price = order.Price;
            decimal qty = 0;
            string str = order.OrderQty.Replace(",", "");
            if (decimal.TryParse(str, out qty))
                Amount = ((int)qty).ToString();
            else
                Amount = order.OrderQty;
            //CountpartyName = order.UserName;

            VenueBGBrush = (Brush)_myWindow.FindResource("ColorBrush.Red13");

        }

    }
}
