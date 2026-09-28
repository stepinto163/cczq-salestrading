using Ats.Foundation.Message;
using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.View.Component;
using STC.Broker.Plugin.View.Order;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.UI;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Broker.Plugin.ViewModel.Order
{
    public class SendBrokerPriceViewModel : BaseViewModel
    {
        #region 构造

        public Window myWindow;
        private object _lockObj = new object();
        public string TXT_KW = "kw";
        public string TXT_IRS = "百万";

        private string confirmButtonTag = null;
        /// <summary>
        /// Tag 1 Green Tag 2 Blue  Tag 3 Orange  Tag 4 GraySlate 随系统皮肤变化  Tag x:null GreenBlue 随系统皮肤变化
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
        /// 债券代码
        /// </summary>
        private string security = string.Empty;
        public string Security
        {
            get { return security; }
            set
            {
                security = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 债券简称
        /// </summary>
        private string symbol = string.Empty;
        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                this.OnPropertyChanged();
            }
        }

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
                this.OnPropertyChanged();
            }
        }

        private bool isBid = true;
        public bool IsBid
        {
            get { return isBid; }
            set
            {
                isBid = value;
                ConfirmButtonTag = this.IsBid ? "2" : "3";

                this.OnPropertyChanged("IsBid");
            }
        }

        private Brush borderBH;
        /// <summary>
        /// QTY Border颜色
        /// </summary>
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

        private Brush bgbackground = new SolidColorBrush(Color.FromRgb(0x8F, 0x95, 0x98));
        public Brush BGBackground
        {
            get { return bgbackground; }
            set
            {
                if (value != null)
                {
                    bgbackground = value;
                    this.OnPropertyChanged(nameof(BGBackground));
                }
            }
        }

        
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
                this.OnPropertyChanged();
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
                this.OnPropertyChanged();
            }
        }

        private bool _handlinstcheck = false;
        /// <summary>
        /// 不报市场   2明盘 1是暗盘
        /// </summary>
        public bool HandlInstCheck
        {
            get
            {
                return _handlinstcheck;
            }

            set
            {
                _handlinstcheck = value;
                this.OnPropertyChanged();
            }
        }

        private Visibility _warningvisibility = Visibility.Collapsed;
        /// <summary>
        /// 底部价格失效消息是否显示
        /// </summary>
        public Visibility WarningVisibility
        {
            get
            {
                return _warningvisibility;
            }

            set
            {
                _warningvisibility = value;
                this.OnPropertyChanged();
            }
        }

        private decimal _CustomPrice;

        /// <summary>
        /// 价格  -1 999 = --
        /// </summary>
        public decimal CustomPrice
        {
            get
            {
                return _CustomPrice;
            }
            set
            {
                _CustomPrice = value;
                PriceFormat = Common.Utils.MathEx.PriceFormat(CustomPrice);
                this.OnPropertyChanged();
            }
        }

        private string _PriceFormat = string.Empty;
        public string PriceFormat
        {
            get
            {
                return _PriceFormat;
            }
            set
            {
                _PriceFormat = value;
                this.OnPropertyChanged();
            }
        }


        private decimal _CustomQty;

        /// <summary>
        ///  最优 Bid Ofr Qty
        /// </summary>
        public decimal CustomQty
        {
            get
            {
                return _CustomQty;
            }
            set
            {
                _CustomQty = value;

                if (MarketIndicator == "2")
                {
                    QtyTxt = TXT_IRS;
                }
                else
                {
                    QtyTxt = TXT_KW;
                }

                if (value > 0)
                {
                    if (QtyTxt == TXT_IRS)
                    {
                        CustomShowQty = (value / 100).ToString("#");
                    }
                    else
                    {
                        CustomShowQty = (value / 1000).ToString("0.#");
                    }
                }
                else
                {
                    CustomShowQty = "0";
                }

                this.OnPropertyChanged();
            }
        }

        private string _CustomShowQty;

        /// <summary>
        /// 显示订单量
        /// </summary>
        public string CustomShowQty
        {
            get
            {
                return _CustomShowQty;
            }
            set
            {
                _CustomShowQty = value;
                this.OnPropertyChanged();
            }
        }


        private decimal _SumQty;

        /// <summary>
        ///  最优 Bid Ofr Qty
        /// </summary>
        public decimal SumQty
        {
            get
            {
                return _SumQty;
            }
            set
            {
                _SumQty = value;

                if (MarketIndicator == "2")
                {
                    QtyTxt = TXT_IRS;
                }
                else
                {
                    QtyTxt = TXT_KW;
                }

                if (value > 0)
                {
                    if (QtyTxt == TXT_IRS)
                    {
                        SumShowQty = (value / 100).ToString("#");
                    }
                    else
                    {
                        SumShowQty = (value / 1000).ToString("0.#");
                    }
                }
                else
                {
                    SumShowQty = "--";
                }

                this.OnPropertyChanged();
            }
        }

        private string _SumShowQty = "--";

        /// <summary>
        /// 总订单量
        /// </summary>
        public string SumShowQty
        {
            get
            {
                return _SumShowQty;
            }
            set
            {
                _SumShowQty = value;
                this.OnPropertyChanged();
            }
        }

        private string _QtyTxt;

        /// <summary>
        /// 显示订单量单位 kw 百万
        /// </summary>
        public string QtyTxt
        {
            get
            {
                return _QtyTxt;
            }
            set
            {
                _QtyTxt = value;
                this.OnPropertyChanged();
            }
        }

        private List<string> _selsettlelist = new List<string>();
        /// <summary>
        /// 选择的清算速度
        /// </summary>
        public List<string> SelSettleList
        {
            get
            {
                return _selsettlelist;
            }
            set
            {
                _selsettlelist = value;
                if (this.BestQuotationsMV.View != null)
                {
                    this.BestQuotationsMV.View.Refresh();
                }

                this.OnPropertyChanged();
            }
        }

        private ObservableCollection<GoodsVolumeVoModel> _BestQuotations = new ObservableCollection<GoodsVolumeVoModel>();
        /// <summary>
        /// Bid 最优价列表
        /// </summary>
        public ObservableCollection<GoodsVolumeVoModel> BestQuotations
        {
            get
            {
                return _BestQuotations;
            }
            set
            {
                _BestQuotations = value;
                this.OnPropertyChanged();
            }
        }

        private CollectionViewSource bestQuotationsmv = new CollectionViewSource();
        public CollectionViewSource BestQuotationsMV
        {
            get { return bestQuotationsmv; }
            set
            {
                bestQuotationsmv = value;
                this.OnPropertyChanged(nameof(BestQuotationsMV));
            }
        }


        private string quoteid = null;
        public string QuoteID
        {
            get { return quoteid; }
            set
            {
                quoteid = value;
                this.OnPropertyChanged(nameof(QuoteID));
            }
        }

        private bool isInit = false;
        #region Command
        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public ICommand AddQtyCommand { get; set; }
        public ICommand ReduceQtyCommand { get; set; }

        public ICommand Add1QtyCommand { get; set; }
        public ICommand Add3QtyCommand { get; set; }
        public ICommand Add5QtyCommand { get; set; }
        public ICommand AddAllQtyCommand { get; set; }
        #endregion

        private bool isEnableConfirm = true;
        /// <summary>
        /// 是否允许提交
        /// </summary>
        public bool IsEnableConfirm
        {
            get { return isEnableConfirm; }
            set
            {
                isEnableConfirm = value;
                this.OnPropertyChanged("IsEnableConfirm");
            }
        }

        #region 多Id
      
        private string _multiid = string.Empty;
        /// <summary>
        /// 多 报价 ID
        /// </summary>
        public string MultiId
        {
            get { return _multiid; }
            set
            {
                _multiid = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #endregion
        public SendBrokerPriceViewModel()
        {
            OkCommand = new DelegateCommand(new Action<object>(OnOkCommand));
            CancelCommand = new DelegateCommand(new Action<object>(OnCancelCommand));

            AddQtyCommand = new DelegateCommand(new Action<object>(OnAddQtyCommand));
            ReduceQtyCommand = new DelegateCommand(new Action<object>(OnReduceQtyCommand));

            Add1QtyCommand = new DelegateCommand(new Action<object>(OnAdd1QtyCommand));
            Add3QtyCommand = new DelegateCommand(new Action<object>(OnAdd3QtyCommand));
            Add5QtyCommand = new DelegateCommand(new Action<object>(OnAdd5QtyCommand));
            AddAllQtyCommand = new DelegateCommand(new Action<object>(OnAddAllQtyCommand));

            //订阅App消息

        }


        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="myWindow"></param>
        /// <param name="security"></param>
        /// <param name="isBid"></param>
        public void Init(Window myWindow, GoodsPo goodinfo, ObservableCollection<GoodsVolumeVoModel> lstgoodsvolume, bool isBid, decimal price, string quoteid, List<string> selsettlelist, string _multiid = null)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                this.myWindow = myWindow;
                this.IsBid = !isBid;
                this.TitleBidOrOfr = this.IsBid ? "TKN" : "GVN";
                BGBackground = this.TitleBidOrOfr == "TKN" ? (Brush)Application.Current.MainWindow.FindResource("OfrLinearGradientBrush") : (Brush)Application.Current.MainWindow.FindResource("BidLinearGradientBrush");
                BidOrOfrTextBlockForground = this.TitleBidOrOfr == "TKN" ? (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Blue8") : (Brush)Application.Current.MainWindow.FindResource("ColorBrush.Orange5");
                this.Security = goodinfo.GoodsCode;
                this.Symbol = goodinfo.GoodsShortname;
                this.MarketIndicator = goodinfo.MarketIndicator;
                this.CustomPrice = price;
                //点价量 默认全部量
                //this.CustomQty = lstgoodsvolume.Sum(p => p.qty);
                this.BestQuotations = lstgoodsvolume;
                this.QuoteID = quoteid;

                this.MultiId = _multiid;
                //最优价列表
                this.BestQuotationsMV.Source = lstgoodsvolume;
                this.BestQuotationsMV.IsLiveFilteringRequested = true;
                this.BestQuotationsMV.IsLiveSortingRequested = true;

                this.BestQuotationsMV.SortDescriptions.Add(new SortDescription("TransactTime", ListSortDirection.Descending));
                this.BestQuotationsMV.SortDescriptions.Add(new SortDescription("MDPriceLevel", ListSortDirection.Ascending));
                this.BestQuotationsMV.SortDescriptions.Add(new SortDescription("FirmFlag", ListSortDirection.Descending));
                this.SelSettleList = selsettlelist;
                SetQuotationsMvFilter();

                this.CustomQty = this.SumQty = BestQuotationsMV.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999 && p.quoteInternalFlag!="3").Sum(p => p.qty);
                this.BestQuotationsMV.View.CollectionChanged += BestQuotationsMV_CollectionChanged;
                this.BestQuotations.CollectionChanged += BestQuotations_CollectionChanged;

                //基础数据发生变化
                QuoteConfigHandler.Instance.BaseDataUpdated += Instance_BaseDataUpdated;
                //订阅最新报价
                isInit = true;

                //期货不允许提交
                if (MarketIndicator == "5")
                    IsEnableConfirm = false;


            }));
        }

        private void Instance_BaseDataUpdated(object obj)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                this.BestQuotationsMV.View.Refresh();
            }));
        }



        #region  计算清算速度
        private void SetQuotationsMvFilter()
        {
            this.BestQuotationsMV.View.Filter = x =>
            {
                GoodsVolumeVoModel data = x as GoodsVolumeVoModel;
                if (data.MDPriceLevel != 999 && data.PriceFormat == PriceFormat && data.quoteInternalFlag != "3")
                {
                    string settle = data.settlType;
                    List<string> uniqeset = settle.Split(',').ToList();
                    List<string> intersection = SelSettleList.Intersect(uniqeset).ToList();
                    return intersection.Count > 0;
                }
                else
                    return false;
            };
        }

        /// <summary>
        /// 原始表变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BestQuotations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isInit)
                {
                    List<GoodsVolumeVoModel> lstgvvm = BestQuotations.Where(o => SelSettleList.Intersect(o.settlType.Split(',').ToList()).ToList().Count() > 0 && o.MDPriceLevel != 999 && o.DeleteFlag == "0" && o.quoteInternalFlag != "3").OrderBy(p => p.MDPriceLevel).ToList();
                    if (lstgvvm.Count > 0)
                    {
                        GoodsVolumeVoModel gvvm = lstgvvm[0];
                        if (gvvm.PriceFormat != PriceFormat)
                        {
                            WarningVisibility = Visibility.Visible;
                            IsEnableConfirm = false;
                        }
                        else
                        {
                            WarningVisibility = Visibility.Collapsed;
                            IsEnableConfirm = true;
                        }
                    }
                    else
                    {
                        WarningVisibility = Visibility.Visible;
                        IsEnableConfirm = false;
                    }
                }
            });
        }

        /// <summary>
        /// 筛选后的表发生变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BestQuotationsMV_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.SumQty = BestQuotationsMV.View.Cast<GoodsVolumeVoModel>().Where(p => p.MDPriceLevel != 999 && p.quoteInternalFlag != "3").Sum(p => p.qty);
            int isNewFlash = BestQuotationsMV.View.Cast<GoodsVolumeVoModel>().Where(p => p.FlashCount > 0).Count();
            if (isInit && isNewFlash == 0)
            {
                this.CustomQty = SumQty;
            }
        }


        public List<string> GetSettleList(ObservableCollection<GoodsVolumeVoModel> listGoods)
        {
            Dictionary<string, string> unionDict = new Dictionary<string, string>();
            List<Dictionary<string,string>> listSettle = listGoods.Where(o => o.MDPriceLevel != 999).Select( p => p.DictSettleType ).ToList();
            
            foreach (Dictionary<string, string> dic in listSettle)
            {
                unionDict = unionDict.Union(dic).ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            return new List<string>(unionDict.Keys);
        }

        #endregion

        #region OnCommand
        
        #region 数量计算

        /// <summary>
        /// AddQty
        /// </summary>
        /// <param name="inparam"></param>
        public void OnAddQtyCommand(object inparam)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                decimal amount = 0;
                if (decimal.TryParse(CustomShowQty, out amount))
                {
                    amount += 1;
                    if (amount <= 1000)
                    {
                        CustomShowQty = amount.ToString();
                    }
                }
                else
                {
                    CustomShowQty = "1";
                }
            }));
        }

        /// <summary>
        /// ReduceQty
        /// </summary>
        /// <param name="inparam"></param>
        public void OnReduceQtyCommand(object inparam)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                decimal amount = 0;
                if (decimal.TryParse(CustomShowQty, out amount))
                {
                    amount = amount - 1;
                    if (amount >= 0)
                    {
                        CustomShowQty = amount.ToString();
                    }
                }
                else
                {
                    CustomShowQty = "5";
                }
            }));
        }


        private void OnAddAllQtyCommand(object obj)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                CustomQty = this.SumQty;
            }));
        }

        private void OnAdd5QtyCommand(object obj)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                CustomShowQty = "5";
            }));
        }

        private void OnAdd3QtyCommand(object obj)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                CustomShowQty = "3";
            }));
        }

        private void OnAdd1QtyCommand(object obj)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                CustomShowQty = "2";
            }));
        }
        #endregion

        /// <summary>
        /// GVN, TKN
        /// </summary>
        /// <param name="inparam"></param>
        public void OnOkCommand(object inparam)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                BaseReplyPo rp = GetErrorTxt();
                if (rp.Code == "10001" && !string.IsNullOrEmpty(rp.Msg))
                {
                    List<TPlaceOrderResult> rs = MsgHelper.Deserialize<List<TPlaceOrderResult>>(rp.Msg);
                    if (rs.Count > 0 && rs[0].Code == "10003")
                    {
                        MessageBoxGUI msgBox = new MessageBoxGUI();
                        msgBox.Owner = Application.Current.MainWindow;
                        msgBox.setWarnMsg(string.Join("\r\n", rs.Where(p=>p.Code == "10003").Select(o=>o.Reason).ToList()));
                        msgBox.Owner = myWindow as SendBrokerPriceView;
                        msgBox.ShowDialog();
                        SetEnable(true);
                    }
                    else
                    {
                        SetEnable(false);
                        NewOrderSingle newOrder = GetNewOrderSingle();
                        ThreadPoolHelper.InvokeThreadPool(SendPointPrice, newOrder);
                    }
                }
            }));
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

            if (!decimal.TryParse(CustomPrice.ToString(), out temD) || temD == 0)// string.IsNullOrEmpty(CustomPrice))
            {
                por.Reason = "未填写价格或价格为0";
            }
            else if (decimal.TryParse(CustomShowQty, out temD) && temD == 0)//(string.IsNullOrEmpty(CustomPrice))
            {
                por.Reason = "交易量不能为空或0，请修改";
            }
            else if (decimal.TryParse(CustomShowQty, out temD) && temD > 2000)//(string.IsNullOrEmpty(CustomPrice))
            {
                por.Reason = "交易量不能超过2000kw，请修改";
            }

            if (!string.IsNullOrEmpty(por.Reason))
            {
                result.Add(por);
            }

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
                                SendPriceWarnView warnView = new SendPriceWarnView(result, newOrder as NewOrderSingle);
                                warnView.Owner = myWindow as SendBrokerPriceView;
                                warnView.Show();
                                SetEnable(true);
                            }
                            else if (result.Code == "1014")
                            {
                                //code:1014,RiskManualCk=2(风控通过）
                                BrokerMessageBoxGUI msgBox = new BrokerMessageBoxGUI();
                                msgBox.Owner = Application.Current.MainWindow;
                                string side = (newOrder as NewOrderSingle).Side == "2" ? "GVN" : "TKN" ; // newOrder.Side = this.TitleBidOrOfr == "GVN" ? "2" : "1"; // IsGVN ? "2" : "1"; // 这里要反向
                                msgBox.setTitle("交易提示");
                                msgBox.setWarnMsg($"请确认价格 ");
                                msgBox.setPrice($"{(newOrder as NewOrderSingle).Price}", side);
                                msgBox.setSide(side);
                                msgBox.MOK += MsgBox_MOK;
                                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                msgBox.Owner = myWindow as SendBrokerPriceView;
                                msgBox.ShowDialog();

                            }
                            else
                            {
                                MessageBoxGUI msgBox = new MessageBoxGUI();
                                msgBox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                                msgBox.Owner = Application.Current.MainWindow;
                                msgBox.setWarnMsg(result.Msg);
                                msgBox.Owner = myWindow as SendBrokerPriceView;
                                msgBox.ShowDialog();
                                SetEnable(true);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }
                    finally
                    {
                        //Button_Click(null, null);
                        if (myWindow != null)
                        {
                            (myWindow as SendBrokerPriceView).Close();
                        }
                    }
                }));
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
        /// <summary>
        /// 风控
        /// </summary>
        private void MsgBox_MOK()
        {
            //code:1014,RiskManualCk=2(风控通过）
            NewOrderSingle newOrder = GetNewOrderSingle();
            newOrder.RiskManualCk = "2";
            ThreadPoolHelper.InvokeThreadPool(SendPointPrice, newOrder);
        }

        private void SetEnable(bool flag)
        {
            
        }

        private NewOrderSingle GetNewOrderSingle()
        {
            NewOrderSingle newOrder = new NewOrderSingle();
            // Add TradType
            string tradeType = "1";
            if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.TradeType))
                tradeType = LocalEnv.ConfInfo.TradeType;

            newOrder.AlgoName = "BROKER";
            newOrder.VenueTypeGW = "INNER";
            newOrder.Venues = "MANUAL";

            //newOrder.IocTimeOut = null; //不填，其它用途
            newOrder.MarketIndicator = this.MarketIndicator;

            newOrder.NetPrice = "100";
            newOrder.PriceType = "3"; //收益率
            newOrder.CrossVenues = "2"; //_checkAll ? "1" : _checkInner ? "2" : string.Empty;
            newOrder.ClOrdID = DataTimeSequence.Instance.GetId();
            //填选中的 id;
            newOrder.QuoteID = QuoteID;
            newOrder.SecurityID = this.Security;
            newOrder.Side = this.TitleBidOrOfr == "GVN" ? "2" : "1"; // IsGVN ? "2" : "1"; // 这里要反向
            newOrder.Price = CustomPrice.ToString("F4");
            newOrder.OrdType = "2";

            decimal temD = 0;
            if (decimal.TryParse(CustomShowQty, out temD) || temD == 0)//(string.IsNullOrEmpty(CustomPrice))
            {
                newOrder.OrderQty = (decimal.Parse(CustomShowQty) * 1000).ToString();
            }

            newOrder.TimeInForce = "3";  //1=DAY，3=IOC
            newOrder.SettlType = string.Join(",",SelSettleList.ToArray());
            newOrder.Terminal = string.Format("STClient_v{0}", LocalEnv.ServerInfo.Version);
            newOrder.FirmFlag = "1"; //始终放 1
            newOrder.MacID = EngineHelper.GetMacByNetworkInterface();
            newOrder.InsideIP = EngineHelper.GetIp();
            newOrder.HardDiskID = EngineHelper.Getharddiskid2();
            //newOrder.WholeFlag = BestQuotationsMV.View.Cast<GoodsVolumeVoModel>().First().wholeFlag ?? "0";  //不用传
            //HandlInst":"2" 2明盘 1是暗盘
            newOrder.HandlInst = HandlInstCheck ? "1" : "2";
            newOrder.OnLine = "1";
            return newOrder;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="inparam"></param>
        public void OnCancelCommand(object inparam)
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                this.BestQuotationsMV.View.CollectionChanged -= BestQuotationsMV_CollectionChanged;

                QuoteConfigHandler.Instance.BaseDataUpdated -= Instance_BaseDataUpdated;

                if (myWindow != null)
                    myWindow.Close();
            }));
        }
        #endregion


       
    }
}
