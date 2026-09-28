using com.gateway.log;
using Microsoft.Win32;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.GLADAPTERSvr;
using STC.Common.Handler.Position;
using STC.Common.Model.Admin;
using STC.Common.Model.Export;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Gladapter;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.Common.ViewModel.Drp;
using STC.Position.Plugin.Model;
using STC.Position.Plugin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Position.Plugin.ViewModel
{
    /// <summary>
    /// 历史持仓查询
    /// </summary>
    public class PositionCheckViewModel : ViewModelBase
    {
        public ICommand CheckCommand { get { return new ComRelayCommand(Requery); } }
        public ICommand SyncCommand { get { return new ComRelayCommand(SyncClick); } }
        public ICommand EnterCommand { get { return new ComRelayCommand(EnterClick); } }
        public ICommand ExportCommand { get { return new ComRelayCommand(OnExportClick); } }
        
        #region 构造
        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();

        /// <summary>
        /// 用户拥有权限的所有组合
        /// </summary>
        private Dictionary<string, object> lstBookList;
        public Dictionary<string, object> LstBookList
        {
            get { return lstBookList; }
            set
            {
                lstBookList = value;
                this.OnPropertyChanged(nameof(LstBookList));
            }
        }

        /// <summary>
        /// 参数持仓核对最后更新时间
        /// </summary>
        private string _LastUpdateTime = "";
        public string LastUpdateTime
        {
            get
            {
                return _LastUpdateTime;
            }
            set
            {
                _LastUpdateTime = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 持仓核对获取条数
        /// </summary>
        private int _LastHtCount = 0;
        public int LastHtCount
        {
            get
            {
                return _LastHtCount;
            }
            set
            {
                _LastHtCount = value;
                this.OnPropertyChanged();
            }
        }

        private DateTime _enddate;
        public DateTime EndDate
        {
            get { return _enddate; }
            set
            {
                if (value != _enddate)
                {
                    _enddate = value;
                    this.OnPropertyChanged();

                    if (this.IsInit)
                    {
                        RequeryPositionMvt();
                        RequeryTradeMvt();
                    }
                }
            }
        }

        
        /// <summary>
        /// 同步按钮
        /// </summary>
        private string _syncEnable;
        public string SyncEnable
        {
            get
            {
                return _syncEnable;
            }
            set
            {
                _syncEnable = value;
                this.OnPropertyChanged();
            }
        }

        TbookPositionReplyPo replyPosition;
        TbookTradeReplyPo replyDeal;
        /// <summary>
        /// 字典锁
        /// </summary>
        private object LockDict = new object();

        #region 持仓组合构造
        public string selectBook = string.Empty;
        /// <summary>
        /// 用户选择的筛选的 Bookid
        /// </summary>
        public string SelectBook
        {
            get { return selectBook; }
            set
            {
                selectBook = value;
                this.OnPropertyChanged(nameof(SelectBook));
                if (this.IsInit)
                {
                    RequeryPositionMvt();
                    RequeryTradeMvt();
                    //RefreshKeyWord();
                }
            }
        }

        public string resultSelectedValue = "全部";
        /// <summary>
        /// 用户选择的筛选的 Bookid
        /// </summary>
        public string ResultSelectedValue
        {
            get { return resultSelectedValue; }
            set
            {
                resultSelectedValue = value;
                this.OnPropertyChanged(nameof(ResultSelectedValue));
                if (this.IsInit)
                {
                    RequeryPositionMvt();
                    RequeryTradeMvt();
                    //RefreshKeyWord();
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                this.OnPropertyChanged(nameof(SearchText));
                if (this.IsInit)
                {
                    RequeryPositionMvt();
                    RequeryTradeMvt();
                }
            }
        }

        // 搜索
        //private string _searchText = string.Empty;
        //public string SearchText
        //{
        //    get
        //    {
        //        return _searchText;
        //    }
        //    set
        //    {
        //        if (_searchText != value)
        //        {
        //            _searchText = value;
        //            this.OnPropertyChanged();
        //            //FilterOrderData();

        //            this.Mv.View.Refresh();

        //            //更新持仓盈亏
        //            ReCalcPosition();
        //        }
        //    }
        //}

        public string selectContent = "交易核对";

        private const string positonStr = "持仓核对";
        private const string dealStr = "交易核对";
        public DataGrid dtDataGrid;
        #endregion

        #region 持仓核对构造
        private object lockObj = new object();
        /// <summary>
        /// 持仓核对构造
        /// </summary>
        private ObservableCollection<PositionCheckData> ownPositionDetail = new ObservableCollection<PositionCheckData>();
        public ObservableCollection<PositionCheckData> OwnPositionDetail
        {
            get
            {
                return ownPositionDetail;
            }
            set
            {
                ownPositionDetail = value;
                this.OnPropertyChanged(nameof(OwnPositionDetail));
            }
        }

        private ObservableCollection<PositionCheckData> htPositionDetail = new ObservableCollection<PositionCheckData>();
        public ObservableCollection<PositionCheckData> HTPositionDetail
        {
            get
            {
                return htPositionDetail;
            }
            set
            {
                htPositionDetail = value;
                this.OnPropertyChanged(nameof(HTPositionDetail));
            }
        }

        private PositionCheckData selectedPosition = null;
        /// <summary>
        /// 单击选择的持仓
        /// </summary>
        public PositionCheckData SelectedPosition
        {
            get
            {
                return selectedPosition;
            }

            set
            {
                selectedPosition = value;
                this.OnPropertyChanged(nameof(SelectedPosition));
            }
        }

        private PositionCheckData _selectedTradeItem;
        public PositionCheckData SelectedTradeItem
        {
            get
            {
                return _selectedTradeItem;
            }

            set
            {
                _selectedTradeItem = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        #region 交易核对构造
        private object lockTradeObj = new object();
        /// <summary>
        /// 持仓核对构造
        /// </summary>
        private ObservableCollection<PositionCheckData> ownTradeDetail = new ObservableCollection<PositionCheckData>();
        public ObservableCollection<PositionCheckData> OwnTradeDetail
        {
            get
            {
                return ownTradeDetail;
            }
            set
            {
                ownPositionDetail = value;
                this.OnPropertyChanged(nameof(ownTradeDetail));
            }
        }

        private ObservableCollection<PositionCheckData> htTradeDetail = new ObservableCollection<PositionCheckData>();
        public ObservableCollection<PositionCheckData> HTTradeDetail
        {
            get
            {
                return htTradeDetail;
            }
            set
            {
                htTradeDetail = value;
                this.OnPropertyChanged(nameof(HTTradeDetail));
            }
        }

       

        private PositionCheckData selectedTrade = null;
        /// <summary>
        /// 单击选择的持仓
        /// </summary>
        public PositionCheckData SelectedTrade
        {
            get
            {
                return selectedTrade;
            }

            set
            {
                selectedTrade = value;
                this.OnPropertyChanged(nameof(SelectedTrade));
            }
        }
        #endregion

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

        private CollectionViewSource ht = new CollectionViewSource();
        public CollectionViewSource Ht
        {
            get { return ht; }
            set
            {
                ht = value;
                this.OnPropertyChanged(nameof(Ht));
            }
        }

        private CollectionViewSource trade = new CollectionViewSource();
        public CollectionViewSource Trade
        {
            get { return trade; }
            set
            {
                trade = value;
                this.OnPropertyChanged(nameof(Trade));
            }
        }

        private CollectionViewSource htTrade = new CollectionViewSource();
        public CollectionViewSource HtTrade
        {
            get { return htTrade; }
            set
            {
                htTrade = value;
                this.OnPropertyChanged(nameof(HtTrade));
            }
        }

        private Visibility loadMaskVisible = Visibility.Visible;
        public Visibility LoadMaskVisible
        {
            get
            {
                return loadMaskVisible;
            }
            set
            {
                loadMaskVisible = value;
                this.OnPropertyChanged(nameof(LoadMaskVisible));
            }
        }

        private PositionCheckUI positionCheckView;
        public PositionCheckUI CheckView
        {
            get
            {
                return positionCheckView;
            }
            set
            {
                positionCheckView = value;
            }
        }

      


        /// <summary>
        /// 是否已经初始化
        /// </summary>
        private bool IsInit = false;

        #endregion



        public PositionCheckViewModel(PositionCheckUI win)
        {
            this.positionCheckView = win;
            //初始化变量
            Init();
            //查询所有组合名称
            GetBookUser();
            // 查询持仓组合
            //GetBookGroupTask();
            // 查询用户历史持仓
            PositionReq mvtreq = new PositionReq();
            QueryPositionMvt(mvtreq);
            QueryTradeMvt(mvtreq);
            //初始化视图
            InitView();
            QueryBookUserByExternal();

            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    GLADAPTERHandler.Instance.SubscribeTtrdOtcPool(Instance_OnConStatus);
                }
                catch (Exception e)
                {
                    LogHelper.Error("SubscribeTtrdOtcPool", e);
                }
            });
        }

        private void Instance_OnConStatus(TtrdOtcPoolReplyPo reply)
        {
            if (reply != null)
            {
                if ("0000".Equals(reply.Code))
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage("数据同步成功!");
                        SyncEnable = "True";
                        Requery();
                    }));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage(string.Format($"数据同步失败, 原因:{reply.Msg}"));
                        SyncEnable = "True";
                    }));
                }
            }
        }

        /// <summary>
        /// 初始化变量
        /// </summary>
        private void Init()
        {
            this.SelectBook = string.Empty;
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        private void InitView()
        {
            this.Mv.Source = OwnPositionDetail;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            this.Mv.View.Refresh();
            //this.Mv.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));

            this.Ht.Source = HTPositionDetail;
            this.Ht.IsLiveFilteringRequested = true;
            this.Ht.IsLiveSortingRequested = true;
            this.Ht.View.Refresh();
            //this.Ht.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));

            this.Trade.Source = OwnTradeDetail;
            this.Trade.IsLiveFilteringRequested = true;
            this.Trade.IsLiveSortingRequested = true;
            this.Trade.View.Refresh();
            //this.Trade.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));

            this.htTrade.Source = HTTradeDetail;
            this.htTrade.IsLiveFilteringRequested = true;
            this.htTrade.IsLiveSortingRequested = true;
            this.htTrade.View.Refresh();
            //this.htTrade.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));
            EndDate = DateTime.Now.AddDays(0);
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                CalendarDateRange calendarDateRange2 = new CalendarDateRange(EndDate.AddDays(1), DateTime.MaxValue);
                ((PositionCheckUI)CheckView).dpTo.BlackoutDates.Add(calendarDateRange2);
            }));
        }
    


        private void QueryBookUserByExternal()
        {
            LstBookList = new Dictionary<string, object>();
            Task.Run(() =>
            {
                BookBookReplyPo replyPo = AdminHandler.Instance.QueryBookUserByExternal();
                if (replyPo != null && "0000".Equals(replyPo.Code))
                {
                    if (replyPo.Datas != null && replyPo.Datas.Count > 0)
                    {
                        foreach (TTbookBook pbm in replyPo.Datas)
                        {
                            if (!LstBookList.ContainsKey(pbm.Id))
                            {
                                LstBookList.Add(pbm.Id, $"{pbm.Id}-{pbm.Bookname}");
                            }
                        }
                    }
                }
                //设置用户有权限查看的组下拉列表
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    this.CheckView.comBookId.ItemsSource = LstBookList;
                }));
            });
        }


        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnPostionDetail(TbookPositionReplyPo reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (lockObj)
                {
                    // OwnPositionDetail.Clear();
                    //HTPositionDetail.Clear();
                    // List<PositionDetailModel> selfData = new List<PositionDetailModel>(riskIndicator.Where(o => SelfBookDict.ContainsKey(o.bookid)));
                    List<PositionSum> selfData = reply.Datas.posDatas;
                    if (selfData.Count == 0)
                    {
                        return;
                    }
                    string[] format = { "yyyy-MM-dd", "yyyy-mm-dd", "yyyy/MM/dd", "yyy-MM-dd", "yyyyMMdd", "M/d/yyyy" };
                    
                    foreach (PositionSum pbm in selfData)
                    {
                        PositionCheckData newpdd = new PositionCheckData();
                        newpdd.bookid = pbm.BookID;
                        if (!string.IsNullOrEmpty(pbm.BookID))
                            newpdd.bookname = BookDict.ContainsKey(pbm.BookID) ? BookDict[pbm.BookID] : "";
                        newpdd.securityid = pbm.SecurityId;
                        newpdd.symbol = pbm.Symbol;
                        //newpdd.updatetime = pbm.Updatetime;
                        //if (!string.IsNullOrEmpty(pbm.Createtime))
                        //{
                        //    DateTime date = DateTime.Now;
                        //    string cdate = pbm.Createtime.Split(' ')[0];
                        //    if (DateTime.TryParseExact(cdate, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                        //    {
                        //        newpdd.createtime = date.ToString("yyyy-MM-dd");
                        //    }
                        //}
                        newpdd.NetPosition = pbm.NetPosition;
                        newpdd.htNetPosition = pbm.HtNetPosition;//CommonUtils.ConvertToDecimal(pbm.HtNetPosition.ToString("N0"));
                        newpdd.CheckResult = pbm.CheckResult;
                        newpdd.tempCode = pbm.TempCode;
                        OwnPositionDetail.Add(newpdd);
                        //if (!IsInit)
                        //{
                        //    if (!LstBookList.ContainsKey(pbm.BookID))
                        //    {
                        //        LstBookList.Add(pbm.BookID, $"{pbm.BookID}-{pbm.BookName}");
                        //    }
                        //}
                    }
                    //if (!IsInit && LstBookList.Count > 0)
                    //{
                    //    //设置用户有权限查看的组下拉列表
                    //    Application.Current.Dispatcher.Invoke((Action)(() =>
                    //    {
                    //        this.CheckView.comBookId.ItemsSource = LstBookList;
                    //    }));
                    //}
                    this.Mv.View.Refresh();


                    List<TtrdAccBalanceSecu> htData = reply.Datas.htDatas;
                    if (selfData.Count == 0)
                    {
                        return;
                    }
                    foreach (TtrdAccBalanceSecu pbm in htData)
                    {
                        PositionCheckData newpdd = new PositionCheckData();
                        newpdd.bookid = pbm.Secu_Accid;
                        newpdd.bookname = pbm.Accname;
                        newpdd.securityid = pbm.Security_Id;
                        if (pbm.Sum_amount.HasValue)
                            newpdd.NetPosition = pbm.Sum_amount.Value + "";
                            //CommonUtils.ConvertToDecimal(pbm.Sum_amount.Value.ToString("N0"));
                        HTPositionDetail.Add(newpdd);
                    }
                    this.Ht.View.Refresh();
                }
                IsInit = true;
            }));
        }


        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnTradeDetail(TbookTradeReplyPo reply)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (lockObj)
                {
                    //OwnTradeDetail.Clear();
                    //HTTradeDetail.Clear();
                    // List<PositionDetailModel> selfData = new List<PositionDetailModel>(riskIndicator.Where(o => SelfBookDict.ContainsKey(o.bookid)));
                    List<TradeSum> selfData = reply.Datas.tradeDatas;
                    if (selfData.Count == 0)
                    {
                        return;
                    }
                    string[] format = { "yyyy-MM-dd", "yyyy-mm-dd", "yyyy/MM/dd", "yyy-MM-dd", "yyyyMMdd", "M/d/yyyy" };
                    LstBookList = new Dictionary<string, object>();
                    foreach (TradeSum pbm in selfData)
                    {
                        PositionCheckData newpdd = new PositionCheckData();
                        newpdd.bookid = pbm.bookID;
                        if (!string.IsNullOrEmpty(pbm.bookID))
                            newpdd.bookname = BookDict.ContainsKey(pbm.bookID) ? BookDict[pbm.bookID] : "";
                        newpdd.securityid = pbm.securityId;
                        newpdd.symbol = pbm.symbol;
                        newpdd.accId = pbm.accId;
                        newpdd.execId = pbm.execId;
                        newpdd.htexecId = pbm.htexecId;
                        newpdd.venueExecId = pbm.venueExecId;
                        newpdd.traderdate = pbm.tradeDate;
                        newpdd.side = pbm.side;
                        newpdd.netprice = pbm.netPrice;
                        newpdd.yield = pbm.yield;
                        newpdd.amount = pbm.amount;
                        newpdd.settlDate = pbm.settleDate;
                        newpdd.CheckResult = pbm.checkResult;
                        newpdd.IsEnter = "1".Equals(pbm.isEnter) ? Visibility.Visible : Visibility.Hidden;
                        if (pbm.comment != null && pbm.comment.Contains("-"))
                        {
                            string[] str = pbm.comment.Split('-');
                            if (str != null && str.Length == 2)
                                newpdd.Comment = str[0];
                        }
                        else
                            newpdd.Comment = pbm.comment;
                        OwnTradeDetail.Add(newpdd);
                    }
                    this.Trade.View.Refresh();

                    List<TrptOtcTradeAll> htData = reply.Datas.htDatas;
                    if (selfData.Count == 0)
                    {
                        return;
                    }
                    foreach (TrptOtcTradeAll pbm in htData)
                    {
                        PositionCheckData newpdd = new PositionCheckData();
                        newpdd.bookid = pbm.bookid;
                        if(!string.IsNullOrEmpty(pbm.bookid))
                            newpdd.bookname = BookDict.ContainsKey(pbm.bookid) ? BookDict[pbm.bookid] : "";
                        newpdd.securityid = pbm.Security_Id;
                        newpdd.execId = pbm.Sysordid.HasValue ? pbm.Sysordid.Value + "" : "";
                        newpdd.accId = pbm.Secu_Accid;
                        newpdd.venueExecId = pbm.Extordid;
                        newpdd.traderdate = pbm.Orddate;
                        newpdd.side = pbm.Trdtype;
                        if (pbm.Bnd_Netprice.HasValue)
                            newpdd.netprice = pbm.Bnd_Netprice.Value + "";//CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.Bnd_Netprice.Value + "")) + "";
                        if (pbm.Bnd_Ytm.HasValue)
                            newpdd.yield = pbm.Bnd_Ytm.Value + "";//CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.Bnd_Ytm.Value + "")) + "";
                        //pbm.Bnd_Ytm.HasValue ? pbm.Bnd_Ytm.Value + "" : "";
                        newpdd.amount = pbm.Ordcount.HasValue ? pbm.Ordcount.Value + "" : "";
                        newpdd.settlDate = pbm.Setdate;
                        if (pbm.Ordcount.HasValue)
                            newpdd.NetPosition = pbm.Ordcount.HasValue ? pbm.Ordcount.Value + "" : "";
                        //CommonUtils.ConvertToDecimal(pbm.Ordcount.Value.ToString("N0"));
                        HTTradeDetail.Add(newpdd);
                    }
                    this.HtTrade.View.Refresh();
                }
                IsInit = true;
            }));
        }



        #region 查询字典表
        public void GetBookUser()
        {
            //BookBookReplyPo reply = AdminHandler.Instance.QueryBookUserByExternal();
            QueryPositionBookResult reply = PositionHandler.Instance.QueryBookUser();
            if (reply != null && "0000".Equals(reply.Code))
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    foreach (PostionBookModel pbm in reply.Datas)
                    {
                        lock (LockDict)
                        {
                            BookDict.Add(pbm.id, pbm.bookname);
                        }
                    }
                }
            }
        }
        #endregion

        #region 同步数据
        private void SyncClick()
        {
            SyncPosAndTrd req = new SyncPosAndTrd();
            req.date = EndDate.ToString("yyyy-MM-dd");//DateTime.Now.ToString("yyyy-MM-dd");
            Task.Run(() =>
            {
                SyncEnable = "False";
                PResult reply = GLADAPTERHandler.Instance.syncPosAndTrd(req);
                //if (reply != null)
                //{
                //    if ("0000".Equals(reply.Code))
                //    {
                //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //        {
                //            showMessage("数据同步成功!");
                //            Requery();
                //        }));
                //    }
                //    else
                //    {
                //        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //        {
                //            showMessage(string.Format($"更新失败, 原因:{reply.Msg}"));
                //        }));
                //    }
                //}
            });
        }
        #endregion


        public void EnterClick()
        {
            if (dtDataGrid == null || dtDataGrid.SelectedItems == null)
                return;

            List<PositionCheckData> list = new List<PositionCheckData>();
            string bookid = "";
            foreach (PositionCheckData c in dtDataGrid.SelectedItems)
            {
                if (string.IsNullOrEmpty(bookid))
                    bookid = c.bookid;
                else if (!bookid.Equals(c.bookid))
                {
                    showMessage("请选择相同的组合!");
                    return;
                }
                if (!Visibility.Visible.Equals(c.IsEnter))
                {
                    showMessage("请选择待补录的交易!");
                    return;
                }
                list.Add(c);
            }

            //PositionCheckData drv = SelectedTradeItem;

            AdditionalRecordUI mi = new AdditionalRecordUI();
            mi.positionCheckData = list;
            if (mi.ShowDialog() == false)
            {
                return;
            }
            Requery();
        }

        #region  查询当天持仓
        private void Requery()
        {
            RequeryPositionMvt();
            RequeryTradeMvt();
        }
        /// <summary>
        /// 重新查询历史持仓
        /// </summary>
        private void RequeryPositionMvt()
        {
            PositionReq mvtreq = new PositionReq();
            mvtreq.bookid = SelectBook;
            if ("全部".Equals(ResultSelectedValue))
                mvtreq.checkResult = "";
            else
                mvtreq.checkResult = ResultSelectedValue;
            mvtreq.date = EndDate.ToString("yyyy-MM-dd");
            mvtreq.securityIds = SearchText;
            QueryPositionMvt(mvtreq);
            this.CheckView.SetPositionDetailSecurityId(string.Empty);
            this.CheckView.SetPositionDetailQueryData();
        }

        public async void QueryPositionMvt(PositionReq req)
        {
            await Task.Run(() =>
            {
                GetHistoryPositionDetail(req);
            });
        }

        private async void GetHistoryPositionDetail(PositionReq req)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                lock (lockObj)
                {
                    CheckView.ShowEMSMask(true);
                    OwnPositionDetail.Clear();
                    HTPositionDetail.Clear();
                    this.Mv.View.Refresh();
                    this.Ht.View.Refresh();
                }
            }));

            replyPosition = await GetHistoryPositionCheckData(req);
            if (replyPosition != null)
            {
                if (replyPosition.Datas != null)
                {
                    OnPostionDetail(replyPosition);
                    seletChange();
                }
            }
            CheckView.ShowEMSMask(false);
        }

        public async Task<TbookPositionReplyPo> GetHistoryPositionCheckData(PositionReq req)
        {
            TbookPositionReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = AdminHandler.Instance.queryPosition(req);
            });

            return reply;
        }
        #endregion


        #region  查询当天交易
        /// <summary>
        /// 重新查询历史持仓
        /// </summary>
        private void RequeryTradeMvt()
        {
            PositionReq mvtreq = new PositionReq();
            mvtreq.bookid = SelectBook;
            if ("全部".Equals(ResultSelectedValue))
                mvtreq.checkResult = "";
            else
                mvtreq.checkResult = ResultSelectedValue;
            mvtreq.date = EndDate.ToString("yyyy-MM-dd");
            mvtreq.securityIds = SearchText;
            QueryTradeMvt(mvtreq);
            //this.CheckView.SetPositionDetailSecurityId(string.Empty);
            //this.CheckView.SetPositionDetailQueryData();
        }

        public async void QueryTradeMvt(PositionReq req)
        {
            await Task.Run(() =>
            {
                GetHistoryTrade(req);
            });
        }
       
        private async void GetHistoryTrade(PositionReq req)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                lock (lockObj)
                {
                    CheckView.ShowEMSMask(true);
                    OwnTradeDetail.Clear();
                    HTTradeDetail.Clear();

                    this.Trade.View.Refresh();
                    this.HtTrade.View.Refresh();
                }
            }));

            replyDeal = await GetHistoryTradeCheckData(req);
            if (replyDeal != null)
            {
                if (replyDeal.Datas != null)
                {
                    OnTradeDetail(replyDeal);
                    seletChange();
                }
            }
            CheckView.ShowEMSMask(false);
        }


        public void seletChange()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                if (positonStr.Equals(selectContent))
                {
                    if (replyPosition != null && replyPosition.Datas != null)
                    {
                        LastUpdateTime = replyPosition.Datas.UpdateTime;
                        LastHtCount = replyPosition.Datas.SyncHtCount;
                    }
                }
                else if (dealStr.Equals(selectContent))
                {
                    if (replyDeal != null && replyDeal.Datas != null)
                    {
                        LastUpdateTime = replyDeal.Datas.UpdateTime;
                        LastHtCount = replyDeal.Datas.SyncHtCount;
                    }
                }
            }));
        }

        public async Task<TbookTradeReplyPo> GetHistoryTradeCheckData(PositionReq req)
        {
            TbookTradeReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = AdminHandler.Instance.queryTrade(req);
            });

            return reply;
        }
        #endregion


        /// <summary>
        /// 提示Warn消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = null;
                me.ShowDialog();
            }));
        }

        private void OnExportClick()
        {
            switch (selectContent)
            {
                case positonStr:
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                        saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                        saveFileDialog.FileName = $"持仓核对_{DateTimeEx.ServerTime.ToString("yyyyMMdd")}";
                        saveFileDialog.AddExtension = true;
                        saveFileDialog.RestoreDirectory = true;
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string fileName = saveFileDialog.FileName;
                            ExportCSV(fileName);
                        }
                    }));
                    break;
                case dealStr:
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                        saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                        saveFileDialog.FileName = $"交易核对_{DateTimeEx.ServerTime.ToString("yyyyMMdd")}";
                        saveFileDialog.AddExtension = true;
                        saveFileDialog.RestoreDirectory = true;
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            string fileName = saveFileDialog.FileName;
                            ExportDeslCSV(fileName);
                        }
                    }));
                    break;

            }
           
        }

        private void ExportCSV(string fileName)
        {
            try
            {
                using (StreamWriter writer = CreateStream(fileName))
                {
                    // 输出表头
                    // writer.WriteLine($"组合,账户,交易编号,外部系统交易编号,外部成交编号,代码,交易日期,方向,净价(元),成交价(%),成交量(万),清算日期,核对结果");
                   
                    writer.WriteLine($"组合,代码,简称,总持仓(万),衡泰持仓(万),核对结果");
                    foreach (PositionCheckData rbm in OwnPositionDetail)
                    {
                        if ("失败".Equals(rbm.CheckResult))
                            writer.WriteLine($"\"{rbm.bookname}\",\"{rbm.securityid}\",\"{rbm.symbol}\",{rbm.NetPosition},{rbm.htNetPosition},\"{rbm.CheckResult}\"");
                    }
                    // 释放资源
                    writer.Flush();
                    writer.Close();
                }
                showMessage($"持仓核对导出成功!\r\n{fileName}");
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                STC.Common.View.Component.CustomMessageBox.Error($"持仓核对导出失败!\r\n{e.Message}", "提示", null);
            }
        }

        private void ExportDeslCSV(string fileName)
        {
            try
            {
                using (StreamWriter writer = CreateStream(fileName))
                {
                    // 输出表头
                     writer.WriteLine($"组合,账户,交易编号,外部系统交易编号,外部成交编号,代码,交易日期,方向,净价(元),成交价(%),成交量(万),清算日期,核对结果");
                    foreach (PositionCheckData rbm in OwnTradeDetail)
                    {
                        if ("失败".Equals(rbm.CheckResult))
                            writer.WriteLine($"\"{rbm.bookname}\",\"{rbm.accId}\",\"{rbm.execId}\",{rbm.htexecId},{rbm.venueExecId},\"{rbm.securityid}\"" +
                                $",\"{rbm.traderdate}\",\"{rbm.side}\",\"{rbm.netprice}\",\"{rbm.yield}\",\"{rbm.amount}\",\"{rbm.settlDate}\",\"{rbm.CheckResult}\"");
                    }
                    // 释放资源
                    writer.Flush();
                    writer.Close();
                }
                showMessage($"交易核对导出成功!\r\n{fileName}");
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                STC.Common.View.Component.CustomMessageBox.Error($"交易核对导出失败!\r\n{e.Message}", "提示", null);
            }
        }

        public StreamWriter CreateStream(string fileName)
        {
            FileStream fs;
            StreamWriter sw;
            try
            {
                string savePath = Path.GetDirectoryName(fileName);
                string saveFile = fileName;
                //创建路径和文件
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                if (!File.Exists(saveFile))
                {
                    fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                else
                {
                    File.Delete(saveFile);
                    fs = new FileStream(saveFile, FileMode.CreateNew, FileAccess.ReadWrite);
                }
                sw = new StreamWriter(fs, Encoding.UTF8);
                return sw;
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
            return null;
        }
    }

}
