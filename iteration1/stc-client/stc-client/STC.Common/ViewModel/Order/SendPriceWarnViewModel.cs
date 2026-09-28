using Ats.Foundation.Message;
using STC.Common.Handler.TBook;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
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
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Table;

namespace STC.Common.ViewModel
{
    /// <summary>
    /// 报价警告界面SendPriceWarnView的数据内容
    /// </summary>
    public class SendPriceWarnViewModel : ViewModelBase
    {
        private readonly string RISK_TITLE = "风控检查";
        private readonly string RISK_REMIND = "[风险警示]";
        private readonly string RISK_STOP = "[风险禁止]";
        private readonly string RISK_REMIND_VAL = "风控警戒值：";
        private readonly string RISK_STOP_VAL = "风控禁止值：";

        private readonly string RISK_PARAM_TITLE = "字段检查";
        private readonly string RISK_PARAM = "[报价检查不通过] ";

        private readonly string BID = "Bid";
        private readonly string OFR = "Ofr";
      
        private SendPriceWarnView _myWindow = null;
        private NewOrderSingle _sendOrder = null;
        private object _intentionOrder = null;
        private List<TPlaceOrderResult> _placeOrderResult = null;

        private string _riskTxt;
        public string RiskTxt
        {
            get { return _riskTxt; }
            set
            {
                _riskTxt = value;
            }
        }

        private string _marketIndicator = string.Empty;
        public string MarketIndicator
        {
            get
            {
                return this._marketIndicator;
            }

            set
            {
                if ((this._marketIndicator != value))
                {
                    this._marketIndicator = value;
                    this.OnPropertyChanged();
                }
            }
        }
        private static readonly Object m__OnConfirmEventLock = new Object();
        private static Action<Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo> _OnConfirmChanged;
        public static event Action<Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo> OnConfirmChanged
        {
            add
            {
                lock (m__OnConfirmEventLock)
                {
                    _OnConfirmChanged += value;
                }
            }
            remove
            {
                lock (m__OnConfirmEventLock)
                {
                    _OnConfirmChanged -= value;
                }
            }
        }

        #region 界面属性

        private string title;
        public string Title
        { get { return title; }
            set
            {
                title = value;

                if (_myWindow != null)
                    _myWindow.Title = value;

                this.OnPropertyChanged();
            }
        }

        private Brush showTitleBG;
        public Brush ShowTitleBG { get { return showTitleBG; } set { showTitleBG = value; this.OnPropertyChanged(); } }

        private Brush showBG;
        public Brush ShowBG { get { return showBG; } set { showBG = value; this.OnPropertyChanged(); } }

        private Brush showBdBG;
        public Brush ShowBdBG { get { return showBdBG; } set { showBdBG = value; this.OnPropertyChanged(); } }

        private Thickness cancelBtnThickness;//0, 0, 90, 0
        public Thickness CancelBtnThickness { get { return cancelBtnThickness; } set { cancelBtnThickness = value; this.OnPropertyChanged(); } }

        private Thickness okBtnThickness;//80, 0, 0, 0
        public Thickness OkBtnThickness { get { return okBtnThickness; } set { okBtnThickness = value; this.OnPropertyChanged(); } }

        private Visibility okVisibility;
        public Visibility OkVisibility { get { return okVisibility; } set { okVisibility = value; this.OnPropertyChanged(); } }

        private Visibility successVisibility = Visibility.Collapsed;
        public Visibility SuccessVisibility { get { return successVisibility; } set { successVisibility = value; this.OnPropertyChanged(); } }

        private Visibility failVisibility = Visibility.Visible;
        public Visibility FailVisibility { get { return failVisibility; } set { failVisibility = value; this.OnPropertyChanged(); } }

        #endregion

        public ICommand CloseCommand { get { return new ComRelayCommand(CloseClick); } }
        
        public ICommand CancelCommand { get { return new ComRelayCommand(CancelClick); } }
        public ICommand OkCommand { get { return new ComRelayCommand(ConfirmClick); } }
        public Window currentWindow = null;

        // 双倍风险
        public void Init(SendPriceWarnView ownWindow, NewOrderSingle order, List<TPlaceOrderResult> orderResults)
        {
            _myWindow = ownWindow;
            _sendOrder = order;
            _placeOrderResult = orderResults;

            StackPanel sp = (StackPanel)_myWindow.FindName("spRisk");

            int inputqty = 0;
            if (order.OrderQty != null)
            {
                inputqty = int.Parse(order.OrderQty) / 1000;
            }

            string priceDetail = string.Format("{0} {1} {2}千万", order.SecurityID, order.Side.Equals("1") ? BID : OFR, inputqty.ToString());

            foreach (TPlaceOrderResult por in orderResults)
            {
                SendPriceRiskView riskView = new SendPriceRiskView();
                //riskView.Height = 200;
                sp.Children.Add(riskView);

                RiskDetailPo riskDetailPo = new RiskDetailPo();
                riskView.DataContext = riskDetailPo;

                riskDetailPo.RiskVisibility = Visibility.Visible;
                riskDetailPo.ParamVisibility = Visibility.Collapsed;
                riskDetailPo.RiskPromptVisibility = Visibility.Visible;

                if (por.Code.Equals("10001"))
                {
                    riskDetailPo.RiskName = RISK_REMIND;
                    riskDetailPo.RiskType = RISK_REMIND_VAL;
                }
                else if (por.Code.Equals("10002"))
                {
                    riskDetailPo.RiskName = RISK_STOP;
                    riskDetailPo.RiskType = RISK_STOP_VAL;
                    Title = RISK_PARAM_TITLE;
                }
                riskDetailPo.Detail = priceDetail;
                riskDetailPo.TwoVisibility = Visibility.Visible;
                riskDetailPo.RiskReason = por.Reason;
                riskDetailPo.ShowBorderBG = (Brush)_myWindow.FindResource("ColorBrush.Red7");
            }
            
            Title = RISK_TITLE;
            
            ShowBdBG = (Brush)_myWindow.FindResource("ColorBrush.Red7");
            CancelBtnThickness = new Thickness(110, 0, 0, 0);
            okBtnThickness = new Thickness(0, 0, 60, 0);

            ShowTitleBG = (Brush)_myWindow.FindResource("ColorBrush.Red14");
            ShowBG = (Brush)_myWindow.FindResource("ColorBrush.Red15");

            if (_myWindow != null)
            {
                _myWindow.Height = 267 + (orderResults.Count - 1) * 200;//207 + (orderResults.Count - 1) * 140; ;
            }
        }

        public void Init(SendPriceWarnView ownWindow, BaseReplyPo resp, NewOrderSingle order, object intentionOrder = null)
        {
            _myWindow = ownWindow;
            RiskTxt = resp.Msg;
            _sendOrder = order;
            _intentionOrder = intentionOrder;

            List<TPlaceOrderResult> orderRisks = null;
            try
            {
                orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(resp.Msg);
            }
            catch (Exception ex)
            { }
            StackPanel sp = (StackPanel)_myWindow.FindName("spRisk");
            sp.Children.Clear();

            int index = 0;

            decimal inputqty = 0;
            if (!string.IsNullOrEmpty(order.OrderQty))
            {
                if (!string.IsNullOrEmpty(order.SecurityID))
                {
                    //如果是国债期货- 单位为手，不用转成千万
                    if (Consts.MARKETINDICATOR_FUTURE.Equals(MarketIndicator))
                    {
                        inputqty = decimal.Parse(order.OrderQty);
                    }
                    else {
                        inputqty = decimal.Parse(order.OrderQty) / 1000;
                    }
                }
                else
                {
                    inputqty = decimal.Parse(order.OrderQty);
                }
            }

            string priceDetail = order == null ? string.Empty : string.Format("{0} {1} {2}千万", order.SecurityID+"", (order.Side+"").Equals("1") ? BID : OFR, inputqty.ToString() + "");
            //如果是国债期货
            if(Consts.MARKETINDICATOR_FUTURE.Equals(MarketIndicator))
            {
                priceDetail = order == null ? string.Empty : string.Format("{0} {1} {2}手", order.SecurityID + "", (order.Side + "").Equals("1") ? BID : OFR, inputqty.ToString() + "");
            }
            if (orderRisks != null)
            {
                foreach (TPlaceOrderResult por in orderRisks)
                {
                    SendPriceRiskView riskView = new SendPriceRiskView();
                    //riskView.Height = 200;
                    sp.Children.Add(riskView);

                    index++;

                    RiskDetailPo riskDetailPo = new RiskDetailPo();
                    riskView.DataContext = riskDetailPo;

                    Title = RISK_TITLE;

                    riskDetailPo.RiskVisibility = Visibility.Visible;
                    riskDetailPo.ParamVisibility = Visibility.Collapsed;
                    riskDetailPo.RiskPromptVisibility = Visibility.Visible;

                    if (por.Code.Equals("10001")
                        || (por.Code.Equals("10002") && LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL && CommonUtils.IsInvestor()))
                    {
                        riskDetailPo.RiskName = RISK_REMIND;
                        riskDetailPo.RiskType = RISK_REMIND_VAL;
                    }
                    else if (por.Code.Equals("10002"))
                    {
                        riskDetailPo.RiskName = RISK_STOP;
                        riskDetailPo.RiskType = RISK_STOP_VAL;

                        OkVisibility = Visibility.Collapsed;
                        CancelBtnThickness = new Thickness(0);

                    }
                    else if (por.Code.Equals("10003"))
                    {
                        riskDetailPo.RiskName = RISK_PARAM;
                        riskDetailPo.RiskVisibility = Visibility.Collapsed;
                        riskDetailPo.ParamVisibility = Visibility.Visible;
                        riskDetailPo.RiskPromptVisibility = Visibility.Collapsed;

                        Title = RISK_PARAM_TITLE;

                        OkVisibility = Visibility.Collapsed;
                    }
                    else if (por.Code.Equals("10004"))
                    {
                        riskDetailPo.RiskName = RISK_PARAM;
                        riskDetailPo.RiskVisibility = Visibility.Collapsed;
                        riskDetailPo.ParamVisibility = Visibility.Collapsed;
                        riskDetailPo.RiskPromptVisibility = Visibility.Collapsed;

                        Title = RISK_PARAM_TITLE;

                        OkVisibility = Visibility.Collapsed;
                        priceDetail = por.Reason;
                        por.Reason = string.Empty;
                        riskDetailPo.PreTextVisibility = Visibility.Collapsed;
                    }
                    

                    riskDetailPo.Detail = priceDetail;
                    riskDetailPo.TwoVisibility = Visibility.Collapsed;
                    riskDetailPo.RiskReason = por.Reason;
                    riskDetailPo.ShowBorderBG = (Brush)_myWindow.FindResource("ColorBrush.Gray20");

                    ShowBdBG = (Brush)_myWindow.FindResource("ColorBrush.Gray20");
                    if (OkVisibility == Visibility.Visible)
                        CancelBtnThickness = new Thickness(0, 0, 90, 0);

                    okBtnThickness = new Thickness(80, 0, 0, 0);

                    ShowTitleBG = (Brush)_myWindow.FindResource("ColorBrush.Gray18");
                    ShowBG = (Brush)_myWindow.FindResource("ColorBrush.Gray13");
                    
                }

                _myWindow.Height = 267 + (index - 1) * 200;// 140;
            }
            else
            {
                if (resp.IsSuccess())
                {
                    SendPriceRiskView riskView = new SendPriceRiskView();
                    //riskView.Height = 200;
                    sp.Children.Add(riskView);

                    index++;

                    RiskDetailPo riskDetailPo = new RiskDetailPo();
                    riskView.DataContext = riskDetailPo;

                    Title = RISK_TITLE;

                    riskDetailPo.RiskVisibility = Visibility.Visible;
                    riskDetailPo.ParamVisibility = Visibility.Collapsed;
                    riskDetailPo.RiskPromptVisibility = Visibility.Visible;

                    _myWindow.Height = 74;
                    _myWindow.Width = 283;

                    CancelBtnThickness = new Thickness(0);


                    SuccessVisibility = Visibility.Visible;
                    FailVisibility = Visibility.Collapsed;


                }
            }
        }
        

        private void CloseClick()
        {
            //if (_myWindow.Owner != null || _myWindow.IsActive)
            //{
            //    _myWindow.DialogResult = false;
            //}
               
            _myWindow.OnClose(_myWindow, true, false);

            if (_myWindow.Owner != null)
                _myWindow.Owner.Focus();

            _myWindow.Close();
        }

        private void CancelClick()
        {
            _myWindow.OnClose(_myWindow, true, false);

            if (_myWindow.Owner != null)
                _myWindow.Owner.Focus();

            _myWindow.Close();
        }

        private void CancelWin(bool isCancel, bool isDoubleCheck)
        {
            _myWindow.OnClose(_myWindow, isCancel, isDoubleCheck);
            _myWindow.Close();
        }

        private bool DoubleCheck()
        {
            bool result = false;
            List<TPlaceOrderResult> orderRisks = null;
            try
            {
                if (string.IsNullOrEmpty(RiskTxt) == false)
                    orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(RiskTxt);
            }
            catch (Exception ex)
            { }

            if (orderRisks != null && orderRisks.Count > 0)
            {
                List<TPlaceOrderResult> orders = new List<TPlaceOrderResult>();
                foreach (TPlaceOrderResult por in orderRisks)
                {
                    if (por.DoubleWarn)
                    {
                        result = true;

                        orders.Add(por);

                    }
                }

                if (orders.Count > 0)
                {
                    SendPriceWarnView doubleWarnView = new SendPriceWarnView(orders, _sendOrder);
                    doubleWarnView.SetSubscribe(_myWindow.GetSubscribe());
                    doubleWarnView.Topmost = true;
                    doubleWarnView.ShowDialog();
                }
            }

            return result;
        }

        private string GetStrategyOrderId()
        {
            string orderId = string.Empty;
            List<TPlaceOrderResult> orderRisks = null;
            try
            {
                orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(RiskTxt);
            }
            catch (Exception ex)
            { }

            if (orderRisks != null && orderRisks.Count > 0)
            {
                foreach (TPlaceOrderResult por in orderRisks)
                {
                    orderId = por.RequestID;

                    break;
                }
            }

            return orderId;
        }

        private void ConfirmClick()
        {
            object obj = this._myWindow.FindName("btnOk");

            if (obj != null)
            {
                Button btnOk = obj as Button;
                if (btnOk != null)
                    btnOk.IsEnabled = false;
            }
            //_myWindow.DialogResult = true;
            _myWindow.Hide();

            bool needDoubleCheck = DoubleCheck();

            if (needDoubleCheck)
            {
                this.CancelWin(true, needDoubleCheck);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        if (_intentionOrder != null)
                        {
                             if (_intentionOrder is STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq)
                            {
                                STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq insertReq = _intentionOrder as STC.Common.Model.Table.WorkBrench.InsertIntentionQuoteReq;
                                insertReq.riskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
                                insertReq.riskReTryFlag = "1";//试算
                                Common.Model.Table.WorkBrench.InsertIntentionQuoteReplyPo result;
                                if (currentWindow == null)
                                    result = TableOldHandler.Instance.InsertIntentionQuote(insertReq);
                                else
                                {
                                    result = IntentionOldHandler.Instance.InsertIntentionQuote_Common_New(insertReq);
                                    SendPriceWarnViewModel._OnConfirmChanged?.Invoke(result);
                                }
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    if (result != null)
                                    {
                                        if (result.IsSuccess())
                                        {
                                           
                                            SendPriceWarnView warnView = new SendPriceWarnView(result, _sendOrder);

                                            warnView.Owner = currentWindow != null ? currentWindow : Application.Current.MainWindow;
                                            warnView.Show();
                                        }
                                    }
                                    CancelWin(false, false);
                                }));
                            }
                            else if (_intentionOrder is STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest)
                            {
                                STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest updateReq = _intentionOrder as STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest;
                                updateReq.riskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
                                updateReq.riskReTryFlag = "1";//试算
                                PResult result = TableOldHandler.Instance.UpdateIntentionQuote(updateReq);
                                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                                {
                                    if (result != null)
                                    {
                                        if (result.IsSuccess())
                                        {
                                            
                                            SendPriceWarnView warnView = new SendPriceWarnView(result, _sendOrder, updateReq);

                                            warnView.Owner = currentWindow != null ? currentWindow : Application.Current.MainWindow;
                                            warnView.Show();
                                        }
                                    }
                                    CancelWin(false, false);
                                }));
                            }
                        }
                        else
                        {
                            if (_placeOrderResult != null && _placeOrderResult.Count > 0)
                                _sendOrder.StrategyOrderId = _placeOrderResult[0].RequestID;
                            else
                                _sendOrder.StrategyOrderId = GetStrategyOrderId();

                            LogHelper.Info(string.Format("Send risk order, StrategyOrderId:{0}", _sendOrder.StrategyOrderId));
                            _sendOrder.RiskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查
                            _sendOrder.RiskReTryFlag = "1";//试算

                            PResult result = OrdersHandler.Instance.PlaceOrder(_sendOrder);


                            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                            {
                                //if (result != null)
                                //{
                                //    SendPriceWarnView warnView = new SendPriceWarnView(result, _sendOrder);
                                ////warnView.Init(result.Msg, sendPricePo);
                                //warnView.Show();
                                //}

                                if (result != null)
                                {
                                    if (result.IsSuccess())
                                    {
                                        SendPriceWarnView warnView = new SendPriceWarnView(result, _sendOrder);

                                        warnView.Owner = currentWindow != null ? currentWindow : Application.Current.MainWindow;
                                        warnView.Show();
                                    }
                                }
                                CancelWin(false, false);
                            }));
                        }
                        
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
        }
    }
    

    public class RiskDetailPo : BaseViewModel
    {
        public TPlaceOrderResult placeOrderResult;
        public string orderID;

        private string riskName;
        public string RiskName { get { return riskName; } set { riskName = value; this.OnPropertyChanged(); } }

        private string msgType = "您的报价";
        public string MsgType { get { return msgType; } set { msgType = value; this.OnPropertyChanged(); } }


        private string detail;
        public string Detail { get { return detail; } set { detail = value; this.OnPropertyChanged(); } }
        
        private Visibility twoVisibility;
        public Visibility TwoVisibility { get { return twoVisibility; } set { twoVisibility = value; this.OnPropertyChanged(); } }

        private string riskType;
        public string RiskType { get { return riskType; } set { riskType = value; this.OnPropertyChanged(); } }

        private string riskReason;
        public string RiskReason { get { return riskReason; } set { riskReason = value; this.OnPropertyChanged(); } }

        private Brush showBorderBG;
        public Brush ShowBorderBG { get { return showBorderBG; } set { showBorderBG = value; this.OnPropertyChanged(); } }

        private Brush showBG;
        public Brush ShowBG { get { return showBG; } set { showBG = value; this.OnPropertyChanged(); } }

        private Visibility riskVisibility;
        public Visibility RiskVisibility { get { return riskVisibility; } set { riskVisibility = value; this.OnPropertyChanged(); } }

        private Visibility paramVisibility;
        public Visibility ParamVisibility { get { return paramVisibility; } set { paramVisibility = value; this.OnPropertyChanged(); } }

        private Visibility riskPromptVisibility;
        public Visibility RiskPromptVisibility { get { return riskPromptVisibility; } set { riskPromptVisibility = value; this.OnPropertyChanged(); } }

        private Visibility confirmWarnVisiblity = Visibility.Collapsed;
        public Visibility ConfirmWarnVisiblity { get { return confirmWarnVisiblity; } set { confirmWarnVisiblity = value; this.OnPropertyChanged(); } }

        private Visibility stateVisibility = Visibility.Collapsed;
        public Visibility StateVisibility { get { return stateVisibility; } set { stateVisibility = value; this.OnPropertyChanged(); } }

        private string state;

        public string State { get { return state;} set { state = value; this.OnPropertyChanged(); } }

        private Visibility preTextVisibility = Visibility.Visible;
        public Visibility PreTextVisibility { get { return preTextVisibility; } set { preTextVisibility = value; this.OnPropertyChanged(); } }
    }
}
