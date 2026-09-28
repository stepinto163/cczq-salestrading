using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.View.Order;
using STC.Mm.Plugin.Command;
using STC.Mm.Plugin.Utils;
using STC.XBond.Plugin.Utils;
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

namespace STC.Mm.Plugin.ViewModel.Order
{
    public class RiskViewUIViewModel : BaseViewModel
    {
        private readonly string RISK_TITLE = "风控提示";
        private readonly string RISK_PARAM_TITLE = "字段检查";

        private RiskViewUI _myWindow = null;
        private NewOrderSingle _sendOrder = null;
        private List<TPlaceOrderResult> _placeOrderResult = null;
        private Dictionary<string, List<string>> RiskTypeDict = new Dictionary<string, List<string>>();

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

        // Title
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                title = value;

                if (_myWindow != null)
                    _myWindow.Title = value;

                this.OnPropertyChanged();
            }
        }

        private string _riskTxt;
        public string RiskTxt
        {
            get { return _riskTxt; }
            set
            {
                _riskTxt = value;
            }
        }

        /// <summary>
        /// Bid Or Ofr Color
        /// </summary>
        private Brush borderBH;
        public Brush BorderBH
        {
            get { return borderBH; }
            set
            {
                if (value != null)
                {
                    borderBH = value;
                    this.OnPropertyChanged();
                }
            }
        }
        /// <summary>
        /// 交易方向
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
        /// 订单Tip信息
        /// </summary>
        private string orderinfo = string.Empty;
        public string OrderInfo
        {
            get { return orderinfo; }
            set
            {
                orderinfo = value;
                this.OnPropertyChanged(nameof(OrderInfo));
            }
        }

        /// <summary>
        /// Ok按钮内容
        /// </summary>
        private string okContent = "继续";
        public string OkContent
        {
            get { return okContent; }
            set
            {
                okContent = value;
                this.OnPropertyChanged(nameof(OkContent));
            }
        }

        /// <summary>
        /// 你的报价
        /// </summary>
        private string yourPrice = "";
        public string YourPrice
        {
            get { return yourPrice; }
            set
            {
                yourPrice = value;
                this.OnPropertyChanged(nameof(YourPrice));
            }
        }
        /// <summary>
        /// ，已触发
        /// </summary>
        private string yourPriceTouch = "";
        public string YourPriceTouch
        {
            get { return yourPriceTouch; }
            set
            {
                yourPriceTouch = value;
                this.OnPropertyChanged(nameof(YourPriceTouch));
            }
        }

        /// <summary>
        /// 2倍
        /// </summary>
        private string yourPriceTwo = "";
        public string YourPriceTwo
        {
            get { return yourPriceTwo; }
            set
            {
                yourPriceTwo = value;
                this.OnPropertyChanged(nameof(YourPriceTwo));
            }
        }

        /// <summary>
        /// 风控警戒值：
        /// </summary>
        private string yourPriceRiskWarn = "";
        public string YourPriceRiskWarn
        {
            get { return yourPriceRiskWarn; }
            set
            {
                yourPriceRiskWarn = value;
                this.OnPropertyChanged(nameof(YourPriceRiskWarn));
            }
        }

        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }
        public ICommand CancelCommand { get { return new RelayCommand(CancelClick); } }
        public ICommand OkCommand { get { return new RelayCommand(ConfirmClick); } }

        /// <summary>
        /// 第一次风控，或成功提示，或字段检查
        /// </summary>
        /// <param name="ownWindow"></param>
        /// <param name="resp"></param>
        /// <param name="order"></param>
        public void Init(RiskViewUI ownWindow, BaseReplyPo resp, NewOrderSingle order)
        {
            // Init Control
            _myWindow = ownWindow;
            _myWindow.MinWidth = 472;
            _sendOrder = order;
            // 初始化字典
            InitRiskTypeDict();
            // 初始化 UI 基本信息
            this.Title = RISK_TITLE;
            this.BidOrOfr = order.Side == "1" ? "Bid" :"Ofr";
            this.BorderBH = order.Side == "1" ? (Brush)_myWindow.FindResource("ColorBrush.Orange5") : (Brush)_myWindow.FindResource("ColorBrush.Blue8");
            this.Security = order.SecurityID;
            this.OrderInfo = $"@{order.Price} x {order.OrderQty}万";
            this.OkContent = "继续";
            // 清空消息区
            StackPanel spLeft = (StackPanel)_myWindow.FindName("spRiskLeft");
            spLeft.Children.Clear();
            StackPanel sp = (StackPanel)_myWindow.FindName("spRisk");
            sp.Children.Clear();

            #region 首先进行字段合法性检测
            // 首先进行字段合法性检测
            if (resp != null)
            {
                // 字段检查不通过
                if (resp.Code == "10002")
                {
                    List<TPlaceOrderResult> brp = MsgHelper.Deserialize<List<TPlaceOrderResult>>(resp.Msg);
                    List<string> errmsg = System.Text.RegularExpressions.Regex.Split(brp[0].Reason, "\r\n").ToList();
                    this.Title = RISK_PARAM_TITLE;
                    TextBlock[] lstErrMsg = new TextBlock[errmsg.Count];
                    TextBox[] checkTitle = new TextBox[errmsg.Count];
                    ControlTemplate AlterTextBoxTemplate = (ControlTemplate)_myWindow.FindResource("AlterTextBox");
                    Style AlterTextBoxStyle = (Style)_myWindow.FindResource("AlterTextBoxStyle");
                    for (int i = 0; i < lstErrMsg.Length; i++)
                    {
                        checkTitle[i] = new TextBox();
                        lstErrMsg[i] = new TextBlock();
                        lstErrMsg[i].Height = 24;

                        checkTitle[i].Text = "字段检查";
                        checkTitle[i].Template = AlterTextBoxTemplate;
                        checkTitle[i].Style = AlterTextBoxStyle;
                        lstErrMsg[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = errmsg[i], Style = (Style)_myWindow.FindResource("GoldTextBlockStyle") });          //中债估值偏离
                        lstErrMsg[i].Margin = new Thickness(8, 0, 0, 8);
                        spLeft.Children.Add(checkTitle[i]);
                        sp.Children.Add(lstErrMsg[i]);
                    }

                    OkVisibility = Visibility.Collapsed;
                    return;
                }
            }
            #endregion

            #region 触发风控检测
            // 触发风控检测
            if (resp != null)
            {
                #region 第一次风控
                RiskTxt = resp.Msg;
                OkVisibility = Visibility.Visible;
                List<TPlaceOrderResult> orderRisks = null;
                try
                {
                    orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(resp.Msg);
                }
                catch (Exception ex)
                { }

                if (orderRisks != null)
                {
                    int num = orderRisks.Count;

                    // 警告标题
                    TextBox[] checkTitle = new TextBox[num];
                    TextBlock[] check = new TextBlock[num + 1];
                    Thickness th = new Thickness(8, 0, 0, 8);
                    Style GoldTextBlockStyle = (Style)_myWindow.FindResource("GoldTextBlockStyle");
                    Style LightYellowTextBlockStyle = (Style)_myWindow.FindResource("LightYellowTextBlockStyle");
                    Style WarnTextBlockStyle = (Style)_myWindow.FindResource("WarnTextBlockStyle");
                    ControlTemplate AlterTextBoxTemplate = (ControlTemplate)_myWindow.FindResource("AlterTextBox");
                    Style AlterTextBoxStyle = (Style)_myWindow.FindResource("AlterTextBoxStyle");

                    // 风控结果排序 
                    orderRisks = orderRisks.OrderBy(o => o.riskType).ToList();
                    for (int i = 0;i< orderRisks.Count;i++)
                    {
                        TPlaceOrderResult por  = orderRisks[i];
                        if (string.IsNullOrEmpty(por.riskType))
                        {
                            por.riskType = "-1";
                        }

                        checkTitle[i] = new TextBox();
                        // checkTitle[i].Height = 24;
                        check[i] = new TextBlock();
                        check[i].Height = 24;

                        if (por.riskType != "-1")
                        {
                            //设置checkbox属性
                            List<string> TipStr = RiskTypeDict[por.riskType.ToString()];         // Run Text 列表
                            List<string> lstReason = por.Reason.Split(',').ToList();            //值列表

                            // check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[0], Style = GoldTextBlockStyle });          // eg:中债估值偏离
                            checkTitle[i].Text = TipStr[0];
                            checkTitle[i].Template = AlterTextBoxTemplate;
                            checkTitle[i].Style = AlterTextBoxStyle;

                            if (por.riskType == "3")
                            {
                                // 倒挂
                                string revertside = this.BidOrOfr == "Bid" ? "Ofr" : "Bid";
                                check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[1].Replace("*", revertside), Style = LightYellowTextBlockStyle });   // 价格与中债估值 Bid Ofr替换
                            }
                            else
                            {
                                check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[1].Replace("*", this.BidOrOfr), Style = LightYellowTextBlockStyle }); // 价格与中债估值
                            }
                            // 数量超限
                            if (por.riskType == "0")
                            {
                                check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $"[{por.Reason}]", Style = GoldTextBlockStyle });               // 1000,1000
                            }
                            else
                            {
                                check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" ({CommonUtils.FormatNum(lstReason[0])}) ", Style = GoldTextBlockStyle });               // (3.52)
                            }

                            check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[2], Style = LightYellowTextBlockStyle });   // 偏离
                            if (TipStr.Count > 3)
                            {
                                check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" {CommonUtils.FormatNum(lstReason[1])} bp", Style = GoldTextBlockStyle });          // 7 bp
                                check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[3], Style = LightYellowTextBlockStyle });  // 超出设置值
                                check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" {CommonUtils.FormatNum(lstReason[2])} bp", Style = GoldTextBlockStyle });             // 5bp
                            }

                            check[i].Margin = th;
                            string tip = string.Empty;
                            foreach (var x in check[i].Inlines)
                            {
                                System.Windows.Documents.Run r = (System.Windows.Documents.Run)x;
                                tip += r.Text;
                            }
                            check[i].ToolTip = tip;
                            sp.Children.Add(check[i]);
                            spLeft.Children.Add(checkTitle[i]);

                        }
                        else
                        {
                            checkTitle[i].Text = por.Result;
                            checkTitle[i].Template = AlterTextBoxTemplate;
                            checkTitle[i].Style = AlterTextBoxStyle;
                            // 增加异常处理
                            //check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = por.Result, Style = GoldTextBlockStyle });       // eg:错误原因
                            check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" {por.Indicator}", Style = LightYellowTextBlockStyle });       // eg:错误原因
                            check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" {por.Reason}", Style = WarnTextBlockStyle }); // 原因

                            check[i].Margin = th;
                            sp.Children.Add(check[i]);
                            spLeft.Children.Add(checkTitle[i]);
                        }
                    }

                    // 设置取消，继续按钮位置
                    if (OkVisibility == Visibility.Visible)
                        CancelBtnThickness = new Thickness(0, 0, 90, 0);
                    okBtnThickness = new Thickness(80, 0, 0, 0);
                    #endregion
                }
                else
                {
                    #region 成功提交订单提示
                    // 成功提交订单
                    if (resp.IsSuccess())
                    {
                        Title = RISK_TITLE;
                        _myWindow.Height = 74;
                        _myWindow.Width = 283;
                        _myWindow.MinWidth = 283;
                        CancelBtnThickness = new Thickness(0);
                        SuccessVisibility = Visibility.Visible;
                        FailVisibility = Visibility.Collapsed;
                        return;
                    }
                    #endregion
                }
            }
            #endregion

        }

        /// <summary>
        /// 双倍风险
        /// </summary>
        /// <param name="ownWindow"></param>
        /// <param name="order"></param>
        /// <param name="orderResults"></param>
        public void Init(RiskViewUI ownWindow, NewOrderSingle order, List<TPlaceOrderResult> orderRisks)
        {
            // Init Control
            _myWindow = ownWindow;
            _sendOrder = order;
            // 初始化字典
            InitRiskTypeDict();
            // 初始化 UI 基本信息
            this.Title = RISK_TITLE;
            this.BidOrOfr = order.Side == "1" ? "Bid" : "Ofr";
            this.BorderBH = order.Side == "1" ? (Brush)_myWindow.FindResource("ColorBrush.Orange5") : (Brush)_myWindow.FindResource("ColorBrush.Blue8");
            this.Security = order.SecurityID;
            this.OrderInfo = $"@{order.Price} x {order.OrderQty}万";
            this.OkContent = "已确认，发出";
            // 清空消息区
            StackPanel spLeft = (StackPanel)_myWindow.FindName("spRiskLeft");
            spLeft.Children.Clear();
            StackPanel sp = (StackPanel)_myWindow.FindName("spRisk");
            sp.Children.Clear();

            OkVisibility = Visibility.Visible;

            if (orderRisks != null)
            {
                int num = orderRisks.Count;

                // 警告标题
                TextBox[] checkTitle = new TextBox[num];
                TextBlock[] check = new TextBlock[num+1];
                Thickness th = new Thickness(8, 0, 0, 8);
                Style GoldTextBlockStyle = (Style)_myWindow.FindResource("GoldTextBlockStyle");
                Style LightYellowTextBlockStyle = (Style)_myWindow.FindResource("LightYellowTextBlockStyle");
                Style WarnTextBlockStyle = (Style)_myWindow.FindResource("WarnTextBlockStyle");
                ControlTemplate AlterTextBoxTemplate = (ControlTemplate)_myWindow.FindResource("AlterTextBox");
                Style  AlterTextBoxStyle = (Style)_myWindow.FindResource("AlterTextBoxStyle");

                this.YourPrice = "你的报价 ";
                this.YourPriceTouch = "，已触发";
                this.YourPriceTwo = " 2倍 ";
                this.YourPriceRiskWarn = "风控警戒值：";
                TextBlock txt_security = (TextBlock)_myWindow.FindName("txt_Security");
                txt_security.FontSize = 12;
                TextBlock txt_bidOrOfr = (TextBlock)_myWindow.FindName("txt_BidOrOfr");
                txt_bidOrOfr.FontSize = 12;
                TextBlock txt_orderInfo = (TextBlock)_myWindow.FindName("txt_PriceAndAmout");
                txt_orderInfo.FontSize = 12;

                // 风控结果排序 
                orderRisks = orderRisks.OrderBy(o => o.riskType).ToList();
                for (int i = 0; i < orderRisks.Count; i++)
                {
                    TPlaceOrderResult por = orderRisks[i];

                    checkTitle[i] = new TextBox();
                    // checkTitle[i].Height = 24;
                    check[i] = new TextBlock();
                    check[i].Height = 24;
                    //设置checkbox属性
                    List<string> TipStr = RiskTypeDict[por.riskType.ToString()];         // Run Text 列表
                    List<string> lstReason = por.Reason.Split(',').ToList();            //值列表

                    // check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[0], Style = GoldTextBlockStyle });          // eg:中债估值偏离
                    checkTitle[i].Text = TipStr[0];
                    checkTitle[i].Template = AlterTextBoxTemplate;
                    checkTitle[i].Style = AlterTextBoxStyle;

                    if (por.riskType == "3")
                    {
                        // 倒挂
                        string revertside = this.BidOrOfr == "Bid" ? "Ofr" : "Bid";
                        check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[1].Replace("*", revertside), Style = LightYellowTextBlockStyle });   // 价格与中债估值 Bid Ofr替换
                    }
                    else
                    {
                        check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[1].Replace("*", this.BidOrOfr), Style = LightYellowTextBlockStyle }); // 价格与中债估值
                    }

                    // 提示框 Tooltip
                    string tip = string.Empty;
                    // 数量超限
                    if (por.riskType == "0")
                    {
                        check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $"[{por.Reason}]", Style = GoldTextBlockStyle });               // 1000,1000
                    }
                    else
                    {
                        check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" ({CommonUtils.FormatNum(lstReason[0])}) ", Style = GoldTextBlockStyle });               // (3.52)
                    }

                    check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[2], Style = LightYellowTextBlockStyle });   // 偏离
                    if (TipStr.Count > 3)
                    {
                        check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" {CommonUtils.FormatNum(lstReason[1])} bp", Style = GoldTextBlockStyle });          // 7 bp
                        check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = TipStr[3], Style = LightYellowTextBlockStyle });  // 超出设置值
                        check[i].Inlines.Add(new System.Windows.Documents.Run("") { Text = $" {CommonUtils.FormatNum(lstReason[2])} bp", Style = GoldTextBlockStyle });             // 5bp
                    }

                    for (int j = 0; j < check[i].Inlines.Count; j++)
                    {
                        // to do ...
                        
                    }

                    check[i].Margin = th;
                    sp.Children.Add(check[i]);
                    spLeft.Children.Add(checkTitle[i]);
                }

                // 请检查要素，并联系风控人员确认
                check[num] = new TextBlock();
                check[num].Height = 24;
                check[num].Inlines.Add(new System.Windows.Documents.Run("") { Text = $"请检查要素，并联系风控人员确认", Style = WarnTextBlockStyle });
                check[num].Margin = th;
                sp.Children.Add(check[num]);


                // 设置取消，继续按钮位置
                if (OkVisibility == Visibility.Visible)
                {
                    okBtnThickness = new Thickness(0, 0, 90, 0);
                }

                CancelBtnThickness = new Thickness(80, 0, 0, 0);
            }

        }
        private void InitRiskTypeDict()
        {
            RiskTypeDict.Add("0", new List<string>() { "数量超限", " 数量超出单笔报量范围 ", " 万" });
            RiskTypeDict.Add("1", new List<string>() { "中债估值偏离", " 价格与中债估值 ", " 偏离 ", " 超出设置值 " });
            RiskTypeDict.Add("2", new List<string>() { "市场最优偏离", " 价格与市场最优* ", " 偏离 ", " 超出设置值 " });
            RiskTypeDict.Add("3", new List<string>() { "价格倒挂", " 价格与市场最优* ", " 倒挂 ", " 超出设置值 " });
            RiskTypeDict.Add("4", new List<string>() { "最新成交偏离", " 价格与最新成交 ", " 偏离 ", " 超出设置值 " });
        }

        private void CloseClick()
        {
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
                    RiskViewUI doubleWarnView = new RiskViewUI(orders, _sendOrder);
                    doubleWarnView.SetSubscribe(_myWindow.GetSubscribe());
                    doubleWarnView.Show();
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

            bool needDoubleCheck = DoubleCheck();
            if (needDoubleCheck)
            {
                this.CancelWin(true, needDoubleCheck);
            }
            else
            {
                if (_placeOrderResult != null && _placeOrderResult.Count > 0)
                    _sendOrder.StrategyOrderId = _placeOrderResult[0].RequestID;
                else
                    _sendOrder.StrategyOrderId = GetStrategyOrderId();

                LogHelper.Info(string.Format("Send risk order, StrategyOrderId:{0}", _sendOrder.StrategyOrderId));
                _sendOrder.RiskManualCk = "2";//默认空值（检查）, 1：只检查风控禁止，2：风控不检查

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        PResult result = OrdersHandler.Instance.PlaceOrder(_sendOrder);
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (result != null)
                            {
                                if (result.IsSuccess())
                                {
                                    RiskViewUI warnView = new RiskViewUI(result, _sendOrder);
                                    warnView.Owner = Application.Current.MainWindow;
                                    warnView.Show();
                                }
                            }
                            CancelWin(false, false);
                        }));
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                });
            }
        }
    }
}
