using com.gateway.log;
using Newtonsoft.Json;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Position;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using STC.Position.Plugin.Model;
using STC.Position.Plugin.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Position.Plugin.ViewModel
{
    /// <summary>
    /// 历史持仓查询
    /// </summary>
    public class HistoryPositionViewModel : ViewModelBase
    {
        #region 构造

        #region 分页


        /// <summary>
        /// 当前页的索引
        /// </summary>
        private int pageindex = 0;
        public int PageIndex
        {
            get
            {
                return pageindex;
            }

            set
            {
                pageindex = value;
            }
        }
        private Common.View.Component.PageControl _PageControl;
        public Common.View.Component.PageControl PageControl
        {
            get
            {
                return _PageControl;
            }

            set
            {
                _PageControl = value;
                this.OnPropertyChanged();
            }
        }
        private string _PageVisible = "false";
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public string PageVisible
        {
            get
            {
                return _PageVisible;
            }

            set
            {
                _PageVisible = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();

        /// <summary>
        /// 有权限的组合
        /// </summary>
        private Dictionary<string, string> SelfBookDict = new Dictionary<string, string>();

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
        /// 字典锁
        /// </summary>
        private object LockDict = new object();

        #region 持仓组合构造
        private string _SecurityId = string.Empty;
        /// <summary>
        /// 债券
        /// </summary>
        public string SecurityId
        {
            get { return _SecurityId; }
            set
            {
                if (_SecurityId != value)
                {
                    _SecurityId = value;
                    this.OnPropertyChanged(nameof(SecurityId));
                    if (this.IsInit)
                    {
                        clear();
                        RefreshKeyWord(); 
                        RequeryPositionMvt();
                    }
                }
            }
        }

        private DateTime startdate;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate
        {
            get { return startdate; }
            set
            {
                if (startdate != value)
                {
                    startdate = value;
                    this.OnPropertyChanged(nameof(StartDate));

                    if (this.IsInit)
                    {
                        this.HistoryView.SetPositionDetailDealTimeStart(startdate.ToString("yyyy-MM-dd"));

                        clear();
                        RequeryPositionMvt();
                    }
                }
            }
        }


        private DateTime enddate;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndDate
        {
            get { return enddate; }
            set
            {
                if (enddate != value)
                {
                    enddate = value;
                    this.OnPropertyChanged(nameof(EndDate));
                    if (this.IsInit)
                    {
                        this.HistoryView.SetPositionDetailDealTimeEnd(EndDate.ToString("yyyy-MM-dd"));

                        clear();
                        RequeryPositionMvt();
                    }
                }
            }
        }

        public string selectBook = "";
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
                    this.HistoryView.SetPositionDetailSecurityId(selectBook);

                    clear();
                    RequeryPositionMvt();
                }
            }
        }

        /// <summary>
        /// 持仓组合
        /// </summary>
        private ObservableCollection<PostionBookData> lstBook = new ObservableCollection<PostionBookData>();
        public ObservableCollection<PostionBookData> LstBook
        {
            get
            {
                return lstBook;
            }

            set
            {
                lstBook = value;
                this.OnPropertyChanged(nameof(LstBook));
            }
        }

        #region 多选树分组数据源

        private ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSource = new ObservableCollection<TreeNodeModel>();
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSource
        {
            get { return _MultiTreeComboxItemsSource; }
            set
            {
                if (_MultiTreeComboxItemsSource != value)
                {
                    _MultiTreeComboxItemsSource = value;
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

        #endregion


        #endregion

        #region 持仓明细构造
        private object lockObj = new object();
        /// <summary>
        /// 持仓明细构造
        /// </summary>
        private ObservableCollection<PositionDetailData> fullPositionDetail = new ObservableCollection<PositionDetailData>();
        public ObservableCollection<PositionDetailData> FullPositionDetail
        {
            get
            {
                return fullPositionDetail;
            }
            set
            {
                fullPositionDetail = value;
                this.OnPropertyChanged(nameof(FullPositionDetail));
            }
        }



        private PositionDetailData selectedPosition = null;
        /// <summary>
        /// 单击选择的持仓
        /// </summary>
        public PositionDetailData SelectedPosition
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

        private Brush borderBrush = new SolidColorBrush(Color.FromRgb(0x3C, 0x3C, 0x3C));
        /// <summary>
        /// 边框颜色，激活和失去焦点后变化
        /// </summary>
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
            }
        }

        /// <summary>
        /// 成交手工录入
        /// </summary>
        public ICommand ManualInputCommand { get { return new ComRelayCommand(OnManualInputClick); } }

        private HistoryPositionView historypositionview;
        public HistoryPositionView HistoryView
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
        /// 是否已经初始化
        /// </summary>
        private bool IsInit = false;

        #endregion

        public HistoryPositionViewModel()
        {
            //初始化变量
            Init();
            //查询所有组合名称
            GetBookUser();
            // 查询持仓组合
            GetBookGroupTask();
            //初始化视图
            InitView();

            //设置树数据源
            MultiTreeComboxItemsSource = STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();
        }


        public void UnRegistEvent()
        {
            this.Dispose();
        }

        /// <summary>
        /// 初始化变量
        /// </summary>
        private void Init()
        {
            this.SecurityId = string.Empty;
            this.SelectBook = "";
            this.StartDate = DateTime.Now.AddDays(-30);
            this.EndDate = DateTime.Now.AddDays(-1);

        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        private void InitView()
        {
            this.Mv.Source = FullPositionDetail;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            //this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
            this.Mv.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));
        }



        /// <summary>
        /// 订阅回调
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnPostionDetail(List<PositionDetailModel> riskIndicator)
        {
            //Application.Current.Dispatcher.Invoke((Action)(() =>
            //{
                //lock (lockObj)
                //{
            List<PositionDetailModel> selfData = new List<PositionDetailModel>(riskIndicator.Where(o => SelfBookDict.ContainsKey(o.bookid)));
            if (selfData.Count == 0)
            {
                return;
            }

            string[] format = { "yyyy-MM-dd", "yyyy-mm-dd", "yyyy/MM/dd", "yyy-MM-dd", "yyyyMMdd", "M/d/yyyy" };
            foreach (PositionDetailModel pbm in selfData)
            {
                PositionDetailData newpdd = new PositionDetailData();
                newpdd.key = $"{pbm.bookid}#{pbm.securityid}#{pbm.settlDate}";
                newpdd.key = pbm.key;
                newpdd.bookid = pbm.bookid;
                newpdd.bookname = BookDict.ContainsKey(pbm.bookid) ? BookDict[pbm.bookid] : "";
                newpdd.securityid = pbm.securityid;
                newpdd.symbol = pbm.symbol;
                newpdd.updatetime = pbm.updatetime;

                if (!string.IsNullOrEmpty(pbm.createtime))
                {
                    DateTime date = DateTime.Now;
                    string cdate = pbm.createtime.Split(' ')[0];
                    if (DateTime.TryParseExact(cdate, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                    {
                        newpdd.createtime = date.ToString("yyyy-MM-dd");
                    }
                }
                newpdd.side = pbm.netposition > 0 ? "多" : "空";
                newpdd.recid = FullPositionDetail.Count + 1;
                newpdd.symboltype = pbm.symboltype;
                newpdd.bondcategory = CommonUtils.GetBondCategoryNameBySecurityId(pbm.securityid);
                newpdd.marketindicator = pbm.marketindicator;
                newpdd.settlType = pbm.settlType;
                newpdd.settlDate = pbm.settlDate;
                newpdd.traderdate = pbm.traderdate;
                newpdd.netprice = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.netprice.ToString("N4")));
                //总净持仓
                newpdd.netposition = CommonUtils.ConvertToDecimal(pbm.netposition.ToString("N0"));
                newpdd.netavgprice = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.netavgprice.ToString("N4")));
                newpdd.netavgyield = CommonUtils.ConvertToDecimal(CommonUtils.FormatNum(pbm.netavgyield.ToString("N4")));
                //日初头寸
                newpdd.initialPosition = CommonUtils.ConvertToDecimal(pbm.initialPosition.ToString("N0"));
                //总持仓量(多)
                newpdd.longposition = CommonUtils.ConvertToDecimal(pbm.longposition.ToString("N0"));
                newpdd.longavgprice = CommonUtils.ConvertToDecimal(pbm.longavgprice.ToString("N4"));
                newpdd.longpriceTotal = CommonUtils.ConvertToDecimal(pbm.longpriceTotal.ToString("N4"));
                newpdd.shortposition = CommonUtils.ConvertToDecimal(pbm.shortposition.ToString("N0"));
                newpdd.shortavgprice = CommonUtils.ConvertToDecimal(pbm.shortavgprice.ToString("N4"));
                newpdd.shortpricetotal = CommonUtils.ConvertToDecimal(pbm.shortpricetotal.ToString("N4"));

                newpdd.lastnetprice = CommonUtils.ConvertToDecimal(pbm.netprice.ToString("N4"));
                newpdd.lastprice = CommonUtils.ConvertToDecimal(pbm.yield.ToString("N4"));
                //今持仓量（多）
                newpdd.todaylongposition = CommonUtils.ConvertToDecimal(pbm.todaylongposition.ToString("N0"));
                //今持仓量（空）
                newpdd.todayshortposition = CommonUtils.ConvertToDecimal(pbm.todayshortposition.ToString("N0"));
                //今日净持仓
                newpdd.dailyNetPosition = CommonUtils.ConvertToDecimal(pbm.dailyNetPosition.ToString("N0"));
                // dv01
                newpdd.dv01 = CommonUtils.ConvertToDecimal(pbm.dv01.ToString("N2"));

                //昨持仓量(多)
                newpdd.ytdlongposition = newpdd.longposition - newpdd.todaylongposition;
                //昨持仓量(空)
                newpdd.ytdshortposition = newpdd.shortposition - newpdd.todayshortposition;
                //totalprofitandloss 当日盈亏(万元)
                newpdd.totalprofitandloss = CommonUtils.ConvertToDecimal((pbm.totalprofitandloss * 10000).ToString("N4"));
                //positionprofitandloss 持仓盈亏  当日盈亏 - 平仓盈亏
                newpdd.positionprofitandloss = CommonUtils.ConvertToDecimal((pbm.positionprofitandloss * 10000).ToString("N4"));
                //closingprofitandloss 平仓盈亏 
                newpdd.closingprofitandloss = CommonUtils.ConvertToDecimal((newpdd.closingprofitandloss * 10000).ToString("N4"));
                //positionmarket 持仓市值
                newpdd.positionmarket = CommonUtils.ConvertToDecimal((pbm.netposition / 100 * newpdd.lastnetprice * 10000).ToString("N4"));
                lock (lockObj)
                {
                    FullPositionDetail.Add(newpdd);
                }
            }

            this.Mv.View.Refresh();
                //}
            //}));
        }
        
        
        #region 筛选数据
        /// <summary>
        /// 筛选数据
        /// </summary>
        public void RefreshKeyWord()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                this.Keyword.Clear();
                //this.Mv.View.Refresh();
                //清空下方的订单列表
                //if (string.IsNullOrEmpty(this.SecurityId))
                //{
                //    this.HistoryView.SetPositionDetailSecurityId(string.Empty);
                //    this.HistoryView.SetPositionDetailClearData();
                //    this.HistoryView.SetPositionDetailQueryData(); 
                //}
                //else
                //{
                //    this.HistoryView.SetPositionDetailSecurityId(this.SecurityId);
                //    this.HistoryView.SetPositionDetailQueryData();
                //}
            }));
        }

        /// <summary>
        /// 排序算法，需要手工设置界面符号，因为要禁用系统自动排序功能
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sortDirection"></param>
        public void Sort(string key, string sortDirection)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                this.Mv.SortDescriptions.Clear();
                if (sortDirection == "Ascending")
                {
                    this.Mv.SortDescriptions.Add(new SortDescription(key, ListSortDirection.Ascending));
                }
                else
                {
                    this.Mv.SortDescriptions.Add(new SortDescription(key, ListSortDirection.Descending));
                }
            }));
        }

        private void Mv_Filter(object sender, FilterEventArgs e)
        {
            PositionDetailData pbd = e.Item as PositionDetailData;
            //搜索框关键字搜索
            if (this.SecurityId != null && this.SecurityId.Length > 0)
            {
                if (!string.IsNullOrEmpty(pbd.securityid) && (pbd.securityid.Contains(this.SecurityId)) || (!string.IsNullOrEmpty(pbd.symbol) && pbd.symbol.Contains(this.SecurityId)))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }
            else
                e.Accepted = true;
        }

        #endregion

        #region 查询字典表
        public void GetBookUser()
        {
            QueryPositionBookResult reply = PositionHandler.Instance.QueryBookUser();

            if (reply != null && reply.IsSuccess())
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

        #region 成交手工录入
        /// <summary>
        /// 成交手工录入
        /// </summary>
        private void OnManualInputClick()
        {
            if (LstBookList.Count == 0)
            {
                showMessage("你还没有配置拥有权限的组合！");
                return;
            }
            else
            {
                ManualInput mi = new ManualInput();
                mi.Show();
            }
        }
        #endregion
 
        #region 查询用户持仓组合
        public async void GetBookGroupTask()
        {
            LogHelper.Info("历史持仓加载开始");
            QueryPositionBookReq req = new QueryPositionBookReq();
            req.username = LocalEnv.UserInfo.UserName;
            req.userid = LocalEnv.UserInfo.UserId;
            QueryPositionBookResult reply = await GetData(req);
            this.HistoryView.ShowEMSMask(true);
            clear();
            try
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    LstBookList = new Dictionary<string, object>();
                    if (reply != null && reply.IsSuccess())
                    {
                        if (reply.Datas != null && reply.Datas.Count > 0)
                        {
                            foreach (PostionBookModel pbm in reply.Datas)
                            {
                                lstBook.Add(new PostionBookData() { IsChecked = true, id = int.Parse(pbm.id), bookname = pbm.bookname, closeby = pbm.closeby, createtime = pbm.createtime, remark = pbm.remark, userid = pbm.userid, username = pbm.username, updatetime = pbm.updatetime });
                                if (!SelfBookDict.ContainsKey(pbm.id))
                                {
                                    SelfBookDict.Add(pbm.id, pbm.bookname);
                                    LstBookList.Add(pbm.id, $"{pbm.id}-{pbm.bookname}");
                                }
                            }
                        }

                        //if (LstBookList.Count > 0)
                        //{
                        //    //设置用户有权限查看的组下拉列表
                        //    Application.Current.Dispatcher.Invoke((Action)(() =>
                        //    {
                        //        this.HistoryView.comBookId.ItemsSource = LstBookList;
                        //    }));
                        //}

                        // 查询用户历史持仓
                        QueryPositionMvtReq mvtreq = new QueryPositionMvtReq();
                        mvtreq.startTime = startdate.ToString("yyyy-MM-dd");
                        mvtreq.endTime = enddate.ToString("yyyy-MM-dd");
                        mvtreq.bookid = LstBookList.Count > 0 ? string.Join(",", LstBookList.Keys.ToList()) : string.Empty;
                        mvtreq.pageNum = PageIndex;
                        mvtreq.pageSize = CommonConstants.pageCount;
                        mvtreq.securityid = SecurityId;
                        ThreadPoolHelper.InvokeThreadPool((o) =>
                        {
                            QueryPositionMvt(mvtreq);
                        });
                    }
                    else
                    {
                        this.HistoryView.ShowEMSMask(false);
                        LogHelper.Info("历史持仓加载完成");
                    }
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    this.HistoryView.ShowEMSMask(false);
                    this.HistoryView.SetPositionDetailDealTimeStart(StartDate.ToString("yyyy-MM-dd"));
                    this.HistoryView.SetPositionDetailDealTimeEnd(EndDate.ToString("yyyy-MM-dd"));
                    this.HistoryView.SetPositionDetailSecurityId(string.Empty);
                    this.HistoryView.SetPositionDetailQueryData();
                }));
            }
        }


        public async Task<QueryPositionBookResult> GetData(QueryPositionBookReq req)
        {
            QueryPositionBookResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.QueryPositionBook(req);
            });

            return reply;
        }
        #endregion

        #region Requery

        /// <summary>
        /// 重新查询历史持仓
        /// </summary>
        public void RequeryPositionMvt()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                QueryPositionMvtReq mvtreq = new QueryPositionMvtReq();
                mvtreq.startTime = StartDate.ToString("yyyy-MM-dd");
                mvtreq.endTime = EndDate.ToString("yyyy-MM-dd");
                mvtreq.securityid = SecurityId;
                mvtreq.bookid = string.IsNullOrEmpty(SelectBook) ? string.Join(",", LstBookList.Keys.ToList()) : SelectBook;
                mvtreq.pageNum = PageIndex;
                mvtreq.pageSize = CommonConstants.pageCount;
                ThreadPoolHelper.InvokeThreadPool((o) =>
                {
                    QueryPositionMvt(mvtreq);
                });

                this.HistoryView.SetPositionDetailDealTimeStart(StartDate.ToString("yyyy-MM-dd"));
                this.HistoryView.SetPositionDetailDealTimeEnd(EndDate.ToString("yyyy-MM-dd"));
                this.HistoryView.SetPositionDetailBookId(string.IsNullOrEmpty(SelectBook) ? string.Join(",", LstBookList.Keys.ToList()) : SelectBook);
                this.HistoryView.SetPositionDetailSecurityId(string.IsNullOrEmpty(SecurityId) ? string.Empty : SecurityId);
                this.HistoryView.SetPositionDetailQueryData();
            }));
        }

        #endregion


        #region 查询所有历史持仓
        public async void QueryPositionMvt(QueryPositionMvtReq req)
        {
            await Task.Run(() =>
            {
                GetHistoryBookDetail(req);
            });
        }

        public void clear() 
        {
            try
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    PageIndex = 0;
                    PageControl.Page = 1;
                    HistoryView.ShowEMSMask(true);
                    FullPositionDetail.Clear();
                    if (this.Mv.View != null)
                        this.Mv.View.Refresh();

                }));
            }
            catch (Exception e)
            {

            }
        }

        private async void GetHistoryBookDetail(QueryPositionMvtReq req)
        {
            //clear();

            QureyHistoryDetailResult reply = await GetHistoryPositionDetailData(req);
            if (reply != null)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        PageControl.RecordCount = reply.totalSize;
                        PageControl.PageSize = CommonConstants.pageCount;
                        if (PageControl.RecordCount > CommonConstants.pageCount)
                        {
                            PageVisible = "true";
                        }
                        else
                        {
                            PageVisible = "false";
                        }

                        OnPostionDetail(reply.Datas);
                    }));
                    LogHelper.Info("历史持仓加载完成");
                }
                else
                {
                    //showMessage(reply.Msg);
                    LogHelper.Info("历史持仓加载完成");
                }
            }
            else
            {
                LogHelper.Info("历史持仓加载完成");
            }

            HistoryView.ShowEMSMask(false);

            IsInit = true;
        }

        public async Task<QureyHistoryDetailResult> GetHistoryPositionDetailData(QueryPositionMvtReq req)
        {
            QureyHistoryDetailResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.QueryHistoryPositionDetail(req);
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
    }

}
