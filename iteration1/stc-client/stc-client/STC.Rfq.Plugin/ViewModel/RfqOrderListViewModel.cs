using STC.Common.Handler.RFQ;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.RFQ;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Rfq.Plugin.Command;
using STC.Rfq.Plugin.Model;
using STC.Rfq.Plugin.Utils;
using STC.Rfq.Plugin.Utils.Data;
using STC.Rfq.Plugin.View;
using System;
using System.Collections.Concurrent;
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
using System.Windows.Threading;
/// <summary>
/// RFQ列表
/// </summary>
namespace STC.Rfq.Plugin.ViewModel
{
    public class RfqOrderListViewModel : ViewModelBase
    {
        #region 构造

        /// <summary>
        /// 更新锁
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// 字典锁
        /// </summary>
        private object LockDict = new object();

        private string _inSecurityId = string.Empty;
        /// <summary>
        /// 债券Id
        /// </summary>
        public string InSecurityId
        {
            get { return _inSecurityId; }
            set
            {
                if (_inSecurityId != value)
                {
                    _inSecurityId = value;

                    this.OnPropertyChanged(nameof(InSecurityId));

                    //if (!string.IsNullOrEmpty(_inSecurityId))
                    //{
                    //    ReQueryOrderDetail();
                    //}
                }
            }
        }

        private RfqOrderList historypositionview;
        public RfqOrderList HistoryView
        {
            get
            {
                return historypositionview;
            }

            set
            {
                historypositionview = value;
            }
        }

        /// <summary>
        /// 方向
        /// </summary>
        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Side");
            }
        }


        /// <summary>
        /// Rfq列表
        /// </summary>
        private ObservableCollection<RfqOrderDetailData> fullrfqorder = new ObservableCollection<RfqOrderDetailData>();
        public ObservableCollection<RfqOrderDetailData> FullRfqOrder
        {
            get
            {
                return fullrfqorder;
            }

            set
            {
                fullrfqorder = value;
                this.OnPropertyChanged(nameof(FullRfqOrder));
            }
        }


        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }



        private List<string> keyword = new List<string>();
        public List<string> Keyword
        {
            get
            {
                return keyword;
            }
            set
            {
                keyword = value;
                this.OnPropertyChanged(nameof(Keyword));
                this.Mv.View.Refresh();
                this.ReCalcOrder();
            }
        }

        public List<string> _lstselectstatus = new List<string>();
        /// <summary>
        /// 获取状态对应的QuoteStatus
        /// </summary>
        public List<string> LstSelectStatus
        {
            get
            {
                return _lstselectstatus;
            }
            set
            {
                if (_lstselectstatus != value)
                {
                    _lstselectstatus = value;
                    List<string> tmpkeyworklist = new List<string>();
                    for (int i = 0; i < value.Count; i++)
                    {
                        List<string> tmp = RfqTools.GetQuoteStatusList(_lstselectstatus[i]);
                        tmpkeyworklist = tmpkeyworklist.Union(tmp).ToList();
                    }

                    tmpkeyworklist = tmpkeyworklist.Distinct().ToList();
                    this.Keyword = tmpkeyworklist;
                    this.OnPropertyChanged(nameof(LstSelectStatus));
                }
            }
        }

        private string rfqheader = "RFQ列表";
        public string RfqHeader
        {
            get
            {
                return rfqheader;
            }
            set
            {
                rfqheader = value;
                Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "RFQHeader", Msg = RfqHeader });
                this.OnPropertyChanged(nameof(RfqHeader));
            }
        }

        private int rowcount = 0;
        public int RowCount
        {
            get
            {
                return rowcount;
            }
            set
            {
                rowcount = value;
                this.OnPropertyChanged(nameof(RowCount));
            }
        }


        private int bidrowcount = 0;
        public int BidRowCount
        {
            get
            {
                return bidrowcount;
            }
            set
            {
                bidrowcount = value;
                this.OnPropertyChanged(nameof(BidRowCount));
            }
        }

        private int ofrrowcount = 0;
        public int OfrRowCount
        {
            get
            {
                return ofrrowcount;
            }
            set
            {
                ofrrowcount = value;
                this.OnPropertyChanged(nameof(OfrRowCount));
            }
        }

        // 搜索
        private string _searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged();
                    this.Mv.View.Refresh();
                    this.ReCalcOrder();
                }
            }
        }

       
        private bool IsSubscribe = false;

        public ICommand OpenHistoryRfqCommand { get { return new DelegateCommand(new Action<object>(OnOpenHistoryRfqCommandClick)); } }
        //new order
        public ICommand OpenRfqModiCommand { get { return new DelegateCommand(new Action<object>(OnRfqModiCommandClick)); } }
        public ICommand OpenRfqOrderCommand { get { return new DelegateCommand(new Action<object>(OnRfqQueryOrderCommandClick)); } }


        //撤单修改回复订单
        public ICommand CancelOrderCommand { get { return new ComRelayCommand<object>(OnCancelOrderCommand); } }
        public ICommand ChangeOrderPriceCommand { get { return new ComRelayCommand<object>(OnChangeOrderPriceCommand); } }
        public ICommand ReplyOrderPriceCommand { get { return new ComRelayCommand<object>(OnReplyOrderPriceCommand); } }
        public ICommand RefuseOrderCommand { get { return new ComRelayCommand<object>(OnRefuseOrderCommand); } }
        public ICommand RefuseAllOrderCommand { get { return new ComRelayCommand<object>(OnCancelAllOrderCommand); } }

        private event Action<QueryRfqOrderReq> OnRfqOrder;

        //Modi:cyc Func:增加倒计时
        DispatcherTimer descTimer = new DispatcherTimer();
        #endregion

        public RfqOrderListViewModel()
        {
            //初始化视图
            InitView();

            //查询所有的Rfq并订阅更新
            GetAndSubscribeAllRfq();
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        private void InitView()
        {
            this.Mv.Source = FullRfqOrder;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            ///待回复、已发送状态，以剩余请求有效时间排序，优先显示。
            ///已成交、已拒绝、已过期三类为终态，排至末尾，以更新时间排序。
            this.Mv.SortDescriptions.Add(new SortDescription("IsOutTime", ListSortDirection.Ascending)); 
            this.Mv.SortDescriptions.Add(new SortDescription("StatusOrder", ListSortDirection.Descending));
            this.Mv.SortDescriptions.Add(new SortDescription("RemainSeconds", ListSortDirection.Ascending));
            this.Mv.SortDescriptions.Add(new SortDescription("UpdateTime", ListSortDirection.Descending));
            this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();

            this.descTimer = new DispatcherTimer();
            this.descTimer.Interval = TimeSpan.FromSeconds(1);
            this.descTimer.Tick += DescTimer_Tick;
            this.descTimer.Start();
        }

        private void DescTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                foreach (RfqOrderDetailData pbm in FullRfqOrder)
                {
                    if (!string.IsNullOrEmpty(pbm.ValidUntilTime))
                    {
                        DateTime date = RfqTools.ConvertCfetsDataTime(pbm.ValidUntilTime);
                        if (date != null)
                        {
                            // 剩余时间
                            TimeSpan diffTime = date - DateTime.Now;
                            int res = Convert.ToInt32(diffTime.TotalSeconds);
                            pbm.ShowRemainSeconds = res < 0 ? "0" : RfqTools.FormatTime(res);
                            pbm.RemainSeconds = res >= 0 ? res : 0 ;
                            pbm.IsOutTime = pbm.RemainSeconds > 0 ? 0 : 1;
                            pbm.AlertTime = pbm.RemainSeconds > 300 ? "0" : "1";
                            if (pbm.IsOutTime == 1)
                            {
                                pbm.IsCanChangeCancelOrder = pbm.IsCanReplyRefuseOrder = false;
                            }
                        }
                    }
                    else
                    {
                        pbm.RemainSeconds = 0;
                        pbm.IsOutTime = 0;
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Mv_Filter(object sender, FilterEventArgs e)
        {
            RfqOrderDetailData pbd = e.Item as RfqOrderDetailData;
            bool isSearchTextAccecpted = true;
            //搜索框关键字搜索
            if (SearchText != null && this.SearchText.Length > 0)
            {
                if (pbd.SecurityID.Contains(SearchText) || pbd.Symbol.Contains(SearchText))
                {
                    isSearchTextAccecpted = true;
                    e.Accepted = true;
                }
                else
                {
                    isSearchTextAccecpted = false;
                    e.Accepted = false;
                }
            }

            if (this.Keyword.Count > 0)
            {
                if (this.Keyword.Contains(pbd.QuoteStatus))
                {
                    e.Accepted = isSearchTextAccecpted && true;
                }
                else
                {
                    e.Accepted = isSearchTextAccecpted && false;
                }
            }
            return;
        }

        #region 查询所有的Rfq并订阅更新
        /// <summary>
        /// 查询所有的Rfq并订阅更新
        /// </summary>
        public void GetAndSubscribeAllRfq()
        {
            QueryRfqOrderReq req = new QueryRfqOrderReq();
            req.queryTimeStart = DateTime.Now.ToString("yyyy-MM-dd") + " 0:00:00";
            req.queryTimeEnd = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
            //req.userName = LocalEnv.UserInfo.UserName;

            FullRfqOrder.Clear();

            GetData(req);

            // 订阅Rfq
            Subscribe();
        }


        /// <summary>
        /// 订阅Rfq订单
        /// </summary>
        private void Subscribe()
        {
            if (!IsSubscribe)
            {
                Task.Run(() => {
                    RfqHandler.Instance.SubscribeRfqOrder(OnRfqOrderCallBack);
                    IsSubscribe = true;
                });
            }
        }

        public async void GetData(QueryRfqOrderReq req)
        {
            QueryRfqOrderResult reply = null;
            await Task.Run(() =>
            {
                reply = RfqHandler.Instance.QueryRfqOrder(req);

                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null && reply.Datas.Count > 0)
                    {
                        OnRfqOrderCallBack(reply);
                    }

                }
                else
                {
                    if (reply != null)
                    {
                        com.gateway.log.LogHelper.Error($"RfqQueryOrder exception.{reply.Msg}");
                    }
                }
            });
        }
        #endregion


        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnRfqOrderCallBack(QueryRfqOrderResult riskIndicator)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (lockObj)
                {
                    List<ExecutionReport> selfData = new List<ExecutionReport>(riskIndicator.Datas);
                    if (selfData.Count == 0)
                    {
                        return;
                    }

                    foreach (ExecutionReport pbm in selfData)
                    {
                        //FullRfqOrder.FirstOrDefault(o => o.QuoteReqID == (string)param);

                        RfqOrderDetailData m2 = FullRfqOrder.FirstOrDefault(o => o.QuoteReqID == pbm.QuoteReqID);
                        if (m2 != null)
                        {
                            m2.UpdateTime = pbm.UpdateTime;
                            m2.TradeDate = pbm.TradeDate;
                            m2.QuoteStatus = pbm.QuoteStatus;
                            m2.QuoteStatusDesc = pbm.QuoteStatusDesc;
                            m2.OrdStatus = pbm.OrdStatus;

                            m2.StatusOrder = RfqTools.GetStautsOrder(pbm.QuoteStatus);
                            m2.Status = RfqTools.GetStatus(pbm.QuoteStatus);
                            if (CommonUtils.IsNumeric(pbm.OrderQty))
                            {
                                decimal tmpv = CommonUtils.ConvertToDecimal(pbm.OrderQty) / 1000m;
                                m2.OrderQty = tmpv.ToString("0.##");
                            }
                            else
                            {
                                m2.OrderQty = "0";
                            }
                            if (CommonUtils.IsNumeric(pbm.OrderQty2))
                            {
                                decimal tmpv = CommonUtils.ConvertToDecimal(pbm.OrderQty2) / 1000m;
                                m2.OrderQty2 = tmpv.ToString("0.##");
                            }
                            else
                            {
                                m2.OrderQty2 = "0";
                            }

                            m2.Price = pbm.Price;
                            m2.MakerOrderQty = pbm.MakerOrderQty;
                            m2.MakerPrice = pbm.Price;
                            m2.NetPrice = pbm.NetPrice;
                            m2.Yield = pbm.Yield;
                            m2.SettlType = pbm.SettlType;
                            m2.Info1 = pbm.Info1;
                            m2.Info2 = pbm.Info2;
                            m2.Venues = pbm.Venues;
                            m2.ClearingMethod = pbm.ClearingMethod;
                            m2.PartySubName = pbm.PartySubName;
                            m2.CounterPartySubID = pbm.CounterPartySubID;
                            m2.CounterPartyName = pbm.CounterPartyName;
                            m2.CounterPartySubName = pbm.CounterPartySubName;
                            m2.Symbol = pbm.Symbol;
                            m2.ClOrdID = pbm.ClOrdID;
                            m2.QuoteID = pbm.QuoteID;
                            m2.marketindicator = pbm.MarketIndicator;
                            m2.UserName = pbm.UserName;
                            m2.IsSelfRfq = pbm.UserName == LocalEnv.UserInfo.UserName ? "1" : "0";
                            m2.BookID = pbm.BookID;
                            m2.SettlDate = pbm.SettlDate;

                            if (!string.IsNullOrEmpty(pbm.ValidUntilTime))
                            {
                                DateTime date = RfqTools.ConvertCfetsDataTime(pbm.ValidUntilTime);
                                m2.ValidUntilTime = date.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else
                            {
                                m2.ValidUntilTime = "";
                            }

                            if (!string.IsNullOrEmpty(pbm.TransactTime))
                            {
                                DateTime date2 = RfqTools.ConvertCfetsDataTime(pbm.TransactTime);
                                m2.TransactTime = date2.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else
                            {
                                m2.TransactTime = "";
                            }

                            m2.IsCanChangeCancelOrder = RfqTools.GetIsCanChangeCancelOrder(m2.IsSelfRfq, m2.Status, m2.QuoteStatus, m2.IsOutTime);
                            m2.IsCanReplyRefuseOrder = RfqTools.GetIsCanReplyRefuseOrder(m2.IsSelfRfq, m2.Status, m2.QuoteStatus, m2.IsOutTime);
                        }
                        else
                        {
                            RfqOrderDetailData rd = Newtonsoft.Json.JsonConvert.DeserializeObject<RfqOrderDetailData>(Newtonsoft.Json.JsonConvert.SerializeObject(pbm));
                            rd.IsSelfRfq = rd.UserName == LocalEnv.UserInfo.UserName ? "1" : "0";
                            rd.marketindicator = pbm.MarketIndicator;
                            if (!string.IsNullOrEmpty(pbm.ValidUntilTime))
                            {
                                DateTime date = RfqTools.ConvertCfetsDataTime(pbm.ValidUntilTime);
                                rd.ValidUntilTime = date.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else
                            {
                                rd.ValidUntilTime = "";
                            }

                            if (!string.IsNullOrEmpty(pbm.TransactTime))
                            {
                                DateTime date2 = RfqTools.ConvertCfetsDataTime(pbm.TransactTime);
                                rd.TransactTime = date2.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            }
                            else
                            {
                                rd.TransactTime = "";   
                            }

                            if (CommonUtils.IsNumeric(pbm.OrderQty))
                            {
                                decimal tmpv = CommonUtils.ConvertToDecimal(pbm.OrderQty) / 1000m;
                                rd.OrderQty = tmpv.ToString("0.##");
                            }
                            else
                            {
                                rd.OrderQty = "0";    
                            }
                            if (CommonUtils.IsNumeric(pbm.OrderQty2))
                            {
                                decimal tmpv = CommonUtils.ConvertToDecimal(pbm.OrderQty2) / 1000m;
                                rd.OrderQty2 = tmpv.ToString("0.##");
                            }
                            else
                            {
                                rd.OrderQty2 = "0";
                            }

                            rd.SettlDate = pbm.SettlDate;

                            rd.StatusOrder = RfqTools.GetStautsOrder(pbm.QuoteStatus);
                            rd.Status = RfqTools.GetStatus(pbm.QuoteStatus);

                            rd.IsCanChangeCancelOrder = RfqTools.GetIsCanChangeCancelOrder(rd.IsSelfRfq, rd.Status, rd.QuoteStatus, rd.IsOutTime);
                            rd.IsCanReplyRefuseOrder = RfqTools.GetIsCanReplyRefuseOrder(rd.IsSelfRfq, rd.Status, rd.QuoteStatus, rd.IsOutTime);
                            this.FullRfqOrder.Add(rd);
                        }
                    }

                    //更新统计
                    ReCalcOrder();
                }
            }));

        }

        #region 查询数据和清空数据

        /// <summary>
        /// 更新持仓盈亏
        /// </summary>
        public void ReCalcOrder()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RfqHeader = $"{FullRfqOrder.Count}";

                ObservableCollection<RfqOrderDetailData> Tmp = new ObservableCollection<RfqOrderDetailData>();
                if (this.Keyword.Count > 0)
                {
                    
                    if (string.IsNullOrEmpty(this.SearchText))
                    {
                        Tmp = new ObservableCollection<RfqOrderDetailData>(FullRfqOrder.Where(p => this.Keyword.Contains(p.QuoteStatus)));
                    }
                    else
                    {
                        Tmp = new ObservableCollection<RfqOrderDetailData>(FullRfqOrder.Where(p => this.Keyword.Contains(p.QuoteStatus) && p.SecurityID.Contains(SearchText)));
                    }
                     

                    if (Tmp.Count >= 0)
                    {
                        RowCount = Tmp.Count;
                        BidRowCount = Tmp.Count(p => p.Side == "1");
                        OfrRowCount = Tmp.Count(p => p.Side == "2");
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(this.SearchText))
                    {
                        RowCount = FullRfqOrder.Count;
                        BidRowCount = FullRfqOrder.Count(p => p.Side == "1");
                        OfrRowCount = FullRfqOrder.Count(p => p.Side == "2");
                    }
                    else
                    {
                        Tmp = new ObservableCollection<RfqOrderDetailData>(FullRfqOrder.Where(p => p.SecurityID.Contains(SearchText)));
                        RowCount = Tmp.Count;
                        BidRowCount = Tmp.Count(p => p.Side == "1");
                        OfrRowCount = Tmp.Count(p => p.Side == "2");
                    }
                }
            }));
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        public void ClearData()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (lockObj)
                {
                    //FullPositionOrderDetail.Clear();
                    //this.Mv.View.Refresh();
                    //RowCount = 0;
                    //BidRowCount = 0;
                    //OfrRowCount = 0;
                    //TotalQty = 0;
                    //TotalBid = 0;
                    //TotalOfr = 0;
                    //EMSPageVisible = "false";

                    ReCalcOrder();
                }
            }));
        }


        #endregion


        #region Command
        /// <summary>
        /// 打开历史RFQ
        /// </summary>
        /// <param name="param"></param>
        private void OnOpenHistoryRfqCommandClick(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                //saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                //saveFileDialog.FileName = $"历史成交记录_{DateTimeEx.ServerTime.ToString("yyyyMMdd")}";
                //saveFileDialog.AddExtension = true;
                //saveFileDialog.RestoreDirectory = true;
                //if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                //{
                //    string fileName = saveFileDialog.FileName;
                //    ExportCSV(fileName);
                //}
            }));
        }


        /// <summary>
        /// 新订单
        /// </summary>
        /// <param name="param"></param>
        private void OnRfqModiCommandClick(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                //RfqNewReqView rrrv = new RfqNewReqView(null);
                //rrrv.SetAction("NewOrder");
                //rrrv.Show();
            }));
        }

        /// <summary>
        ///手工查询测试
        /// </summary>
        /// <param name="param"></param>
        private void OnRfqQueryOrderCommandClick(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                QueryRfqOrderReq req = new QueryRfqOrderReq();
                req.queryTimeStart = DateTime.Now.ToString("yyyy-MM-dd") + " 0:00:00";
                req.queryTimeEnd = DateTime.Now.ToString("yyyy-MM-dd") + " 23:59:59";
                QueryRfqOrderResult reply = RfqHandler.Instance.QueryRfqOrder(req);

                FullRfqOrder.Clear();
                if (reply != null && reply.IsSuccess())
                {
                    if (reply.Datas != null && reply.Datas.Count > 0)
                    {
                        OnRfqOrderCallBack(reply);
                    }
                }
                else
                {
                    if (reply != null)
                    {
                        com.gateway.log.LogHelper.Error($"RfqQueryOrder exception.{reply.Msg}");
                    }
                }

            }));
        }


        #region 回复Command
        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="param"></param>
        private void OnCancelOrderCommand(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                RfqOrderDetailData m2 = FullRfqOrder.FirstOrDefault(o => o.QuoteReqID == (string)param);
                if (m2 != null)
                {
                    bool result = CustomMessageBox.Question("是否撤单？", "撤单");
                    if (result == true)
                    {
                        RfqChangeAcceptOrderReq req = new RfqChangeAcceptOrderReq();
                        req.QuoteReqID = m2.QuoteReqID;
                        req.QuoteStatus = "40";
                        req.SecurityID = m2.SecurityID;
                        req.ClOrdID = m2.ClOrdID;
                        req.UserName = LocalEnv.UserInfo.UserName; //用户名

                        req.MacID = EngineHelper.GetMacByNetworkInterface();
                        req.InsideIP = EngineHelper.GetIp();
                        req.HardDiskID = EngineHelper.Getharddiskid2();
                        req.Info5 = "Manual";

                        PResult reply = await CancelRejectOrder(req);
                        if (reply != null)
                        {
                            if (reply.Code == "0000")
                            {
                                ShowPrompt2Seconds("撤单", "撤单成功！");
                            }
                            else
                            {
                                com.gateway.log.LogHelper.Error($"撤单失败, 原因: {reply.Msg}");
                                CustomMessageBox.Error(string.Format($"撤单失败, 原因:{reply.Msg}."), "撤单");
                            }
                        }
                    }
                }
            }));
        }

        public async Task<PResult> CancelRejectOrder(RfqChangeAcceptOrderReq cancelPo)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = RfqHandler.Instance.ChangeAcceptOrder(cancelPo);

            });
            return reply;
        }


        private async void OnCancelAllOrderCommand(object param)
        {
            StringBuilder error = await CancelSelectedOrdersAsync(param);
            ShowPrompt2Seconds(error.Length > 0 ? "撤单失败" : "撤单成功", error.Length > 0 ? error.ToString() : "撤单成功");
        }

        /// <summary>
        /// 全撤
        /// </summary>
        /// <param name="param"></param>
        private async Task<StringBuilder> CancelSelectedOrdersAsync(object param)
        {
            /// <summary>
            /// all=0, 待回复 1 已发送2 已成交3 已拒绝4 已过期5
            /// <summary>
            ///待回复1 ==>30,31,32,37,42,40
            ///已发送2 ==>33
            ///已成交3 ==>34,36
            ///已拒绝4 ==>38
            ///已失效5 ==>39,45
            /// ---------------------------
            /// * Taker发送请求 30  
            ///* Maker锁定  31
            ///* Maker解锁  32
            ///* Maker确认报价 33
            ///* Taker确认报价 34
            ///* Maker接受报价 35
            ///* 订单完成   36
            ///* Taker拒绝报价 37
            ///* Maker拒绝报价 38
            ///* 请求过期      39
            ///* Maker撤回报价  40
            ///* 回复过期   42
            ///* 取消报价  44
            ///* 对手方与其它机构成交 45
            /// </summary>
            StringBuilder error = new StringBuilder();
            //DispatcherOperation dispatcherOperation = Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            //{
            List<RfqChangeAcceptOrderReq> cancelOrdersList = new List<RfqChangeAcceptOrderReq>();
            int count = 0;

            foreach (RfqOrderDetailData m2 in FullRfqOrder)
            {
                if (m2.QuoteStatus == "33" && (m2.IsSelfRfq == "1"))
                {
                    RfqChangeAcceptOrderReq req = new RfqChangeAcceptOrderReq();
                    req.QuoteReqID = m2.QuoteReqID;
                    req.QuoteStatus = "40";
                    req.SecurityID = m2.SecurityID;
                    req.ClOrdID = m2.ClOrdID;
                    req.UpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                    req.UserName = LocalEnv.UserInfo.UserName;
                    req.MacID = EngineHelper.GetMacByNetworkInterface();
                    req.InsideIP = EngineHelper.GetIp();
                    req.HardDiskID = EngineHelper.Getharddiskid2();
                    req.BookID = m2.BookID;
                    req.Info5 = "Manual";
                    cancelOrdersList.Add(req);
                    count++;
                }
            }

            ConcurrentDictionary<string, PResult> dic = new ConcurrentDictionary<string, PResult>();

            if (cancelOrdersList != null && cancelOrdersList.Count > 0)
            {

                foreach (RfqChangeAcceptOrderReq cancelPo in cancelOrdersList)
                {
                    ThreadPool.QueueUserWorkItem(o =>
                    {
                        try
                        {
                            PResult reply = RfqHandler.Instance.ChangeAcceptOrder(cancelPo);
                            dic.TryAdd(cancelPo.QuoteReqID, reply);
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                            dic.TryAdd(cancelPo.QuoteReqID, new PResult() { Code = "1001", Msg = e.Message });
                        }
                    });
                }


                while (true)
                {
                    if (count == dic.Count)
                    {
                        break;
                    }
                    Thread.Sleep(500);
                }


                foreach (KeyValuePair<string, PResult> pair in dic)
                {
                    string orderID = pair.Key;
                    PResult reply = pair.Value;
                    if (reply != null)
                    {
                        if (reply.IsSuccess() == false)
                        {
                            error.AppendLine($"订单号{orderID}:{reply.Msg}");
                        }
                    }
                }
                //仅有已发送状态，可以撤单。
                //一键撤单：用户点击【一键撤单】按钮，对所有已发送的订单发起撤单。同时关闭该用户的自动回复开关。
                //注：仅有本人已发送的回复可以被选中撤单，机构其它交易员发送的回复，在blotter中不支持被选中。
            }
            else
            {
                error.AppendLine($"订单不符合撤单条件!");
            }
            //}));


            //停止所有已经开启的策略
            RfqGroupHandler.Instance.StopAllStg();


            return error;
        }

        /// <summary>
        /// 改价
        /// </summary>
        /// <param name="param"></param>
        private void OnChangeOrderPriceCommand(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RfqOrderDetailData m2 = FullRfqOrder.FirstOrDefault(o => o.QuoteReqID == (string)param);
                if (m2 != null)
                {
                    RfqReplayRefuseView rrrv = new RfqReplayRefuseView(m2);
                    rrrv.SetAction("ChangeOrder");
                    rrrv.Show();
                }
            }));
        }

        /// <summary>
        /// 回价
        /// </summary>
        /// <param name="param"></param>
        private void OnReplyOrderPriceCommand(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                RfqOrderDetailData m2 = FullRfqOrder.FirstOrDefault(o => o.QuoteReqID == (string)param);
                if (m2 != null)
                {
                    RfqReplayRefuseView rrrv = new RfqReplayRefuseView(m2);
                    rrrv.SetAction("ReplyOrder");
                    rrrv.Show();
                }
            }));
        }

        /// <summary>
        /// 拒绝
        /// </summary>
        /// <param name="param"></param>
        private void OnRefuseOrderCommand(object param)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                RfqOrderDetailData m2 = FullRfqOrder.FirstOrDefault(o => o.QuoteReqID == (string)param);
                if (m2 != null)
                {
                    bool result = CustomMessageBox.Question("是否拒绝该笔报价？", "拒绝订单");
                    if (result == true)
                    {
                        RfqChangeAcceptOrderReq req = new RfqChangeAcceptOrderReq();
                        req.QuoteReqID = m2.QuoteReqID;
                        req.QuoteStatus = "38";
                        req.SecurityID = m2.SecurityID;
                        req.ClOrdID = m2.ClOrdID;
                        req.UpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                        req.UserName = LocalEnv.UserInfo.UserName;
                        req.MacID = EngineHelper.GetMacByNetworkInterface();
                        req.InsideIP = EngineHelper.GetIp();
                        req.HardDiskID = EngineHelper.Getharddiskid2();
                        req.BookID = m2.BookID;
                        req.Info5 = "Manual";
                        PResult reply = await CancelRejectOrder(req);
                        if (reply != null)
                        {
                            if (reply.Code == "0000")
                            {
                                ShowPrompt2Seconds("拒绝", "拒绝订单成功！");
                            }
                            else
                            {
                                CustomMessageBox.Info(string.Format($"拒绝订单失败, 原因:{reply.Msg}."), "拒绝订单");
                            }
                        }
                    }
                }
            }));


        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Clear();
                    GetAndSubscribeAllRfq();
                }));
            }
        }

        ~RfqOrderListViewModel()
        {
            LogHandler.Info("RfqOrderListViewModel Dispose");
            this.Dispose();
        }

        new public void Dispose()



        {
            Clear();
            base.Dispose();
        }


        public void Clear()
        {
            this.FullRfqOrder.Clear();
            RfqHandler.Instance.UnSubscribeRfqOrder(OnRfqOrderCallBack);
            IsSubscribe = false;
        }


        #endregion


        private void ShowPrompt2Seconds(string title, string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Prompt2Seconds prompt = new Prompt2Seconds();
                prompt.PromptTitle = title;
                prompt.PromptContext = message;
                prompt.Show();
                prompt.Activate();
            }));
        }

        #endregion
    }
}