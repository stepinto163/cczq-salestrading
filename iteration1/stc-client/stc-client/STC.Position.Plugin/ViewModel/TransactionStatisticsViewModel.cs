using STC.Common.Handler.Base;
using STC.Common.Handler.Position;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Position.Plugin.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.Position.Plugin.ViewModel
{
    public class TransactionStatisticsViewModel : ViewModelBase
    {

        #region 构造
        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();

        /// <summary>
        /// 有权限的组合
        /// </summary>
        private Dictionary<string, string> SelfBookDict = new Dictionary<string, string>();

        /// <summary>
        /// 字典锁
        /// </summary>
        private object lockObj = new object();

        private bool IsSubscribe = false;

        /// <summary>
        /// 成交统计
        /// </summary>
        private ObservableCollection<TradeStatisticsData> fullTradeStatistics = new ObservableCollection<TradeStatisticsData>();
        public ObservableCollection<TradeStatisticsData> FullTradeStatistics
        {
            get
            {
                return fullTradeStatistics;
            }
            set
            {
                fullTradeStatistics = value;
                this.OnPropertyChanged(nameof(FullTradeStatistics));
            }
        }

        private CollectionViewSource statisticsMv = new CollectionViewSource();
        public CollectionViewSource StatisticsMv
        {
            get { return statisticsMv; }
            set
            {
                statisticsMv = value;
                this.OnPropertyChanged(nameof(StatisticsMv));
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


        private bool isInit = false;
        #endregion

        #region 树状分组数据源

        private ObservableCollection<TreeNodeModel> _MultiTreeComboxItemsSourceNew = new ObservableCollection<TreeNodeModel>();
        public ObservableCollection<TreeNodeModel> MultiTreeComboxItemsSourceNew
        {
            get { return _MultiTreeComboxItemsSourceNew; }
            set
            {
                if (_MultiTreeComboxItemsSourceNew != value)
                {
                    _MultiTreeComboxItemsSourceNew = value;
                    this.OnPropertyChanged();
                }
            }
        }


        private string _SelectedItemNew = "";
        public string SelectedItemNew
        {
            get { return _SelectedItemNew; }
            set
            {
                if (_SelectedItemNew != value)
                {
                    _SelectedItemNew = value;

                    ////RefreshNodeKeyWord();
                    this.OnPropertyChanged();
                }
            }
        }

        #endregion

        public TransactionStatisticsViewModel()
        {
            InitView();
        }

        public void UnRegistEvent()
        {
            PositionHandler.Instance.UnSubscribeTradeStatistics(OnTradeStatisticsCallBack);
            IsSubscribe = false;
            this.Dispose();
        }
        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitView()
        {
            //成交统计
            this.StatisticsMv.Source = FullTradeStatistics;
            this.StatisticsMv.IsLiveFilteringRequested = true;
            this.StatisticsMv.IsLiveSortingRequested = true;
            this.StatisticsMv.Filter += StatisticsMv_Filter;
            this.StatisticsMv.View.Refresh();
            this.StatisticsMv.SortDescriptions.Add(new SortDescription("AlwaysTop", ListSortDirection.Descending));
            // 添加汇总
            if (!FullTradeStatistics.Any(o => o.bookid == "0"))
            {
                FullTradeStatistics.Add(new TradeStatisticsData() { bookid = "0", key = "0#0", bookname = "", securityid = "汇总", AlwaysTop = 1 });
            }

            BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);

            //设置树数据源
            MultiTreeComboxItemsSourceNew = STC.Common.Handler.PP.PPHandler.Instance.GetFundAccountTBookGroup();
        }

        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                if (!isInit)
                {
                    isInit = true;
                    ThreadPoolHelper.InvokeThreadPool((o) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                        {
                            // 查询持仓组合
                            GetBookGroup();
                            RefreshKeyWord();
                        }));
                    });
                }
            }
        }


        /// <summary>
        /// 排序算法，需要手工设置界面符号，因为要禁用系统自动排序功能
        /// </summary>
        /// <param name="key"></param>
        /// <param name="sortDirection"></param>
        public void SortStatistic(string key, string sortDirection)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.StatisticsMv.SortDescriptions.Clear();
                this.StatisticsMv.SortDescriptions.Add(new SortDescription("AlwaysTop", ListSortDirection.Descending));
                if (sortDirection == "Ascending")
                {
                    this.StatisticsMv.SortDescriptions.Add(new SortDescription(key, ListSortDirection.Ascending));
                }
                else
                {
                    this.StatisticsMv.SortDescriptions.Add(new SortDescription(key, ListSortDirection.Descending));
                }
            }));
        }

        #region 查询用户持仓组合并订阅持仓明细
        public async void GetBookGroup()
        {

            QueryPositionBookReq req = new QueryPositionBookReq();
            req.username = LocalEnv.UserInfo.UserName;
            req.userid = LocalEnv.UserInfo.UserId;
            QueryPositionBookResult reply = await GetData(req);

            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (PostionBookModel pbm in reply.Datas)
                        {
                            lstBook.Add(new PostionBookData() { IsChecked = false, id = int.Parse(pbm.id), bookname = pbm.bookname, closeby = pbm.closeby, createtime = pbm.createtime, remark = pbm.remark, userid = pbm.userid, username = pbm.username, updatetime = pbm.updatetime });
                            if (!SelfBookDict.ContainsKey(pbm.id))
                            {
                                SelfBookDict.Add(pbm.id, pbm.bookname);
                            }
                        }
                    });
                }

                // 订阅用户持仓组合
                Subscribe();
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


        /// <summary>
        /// 返回所有被选择的节点
        /// </summary>
        public void RefreshNodeKeyWord()
        {
            Application.Current.Dispatcher.Invoke((Action)(async () =>
            {
                this.Keyword.Clear();
                this.Keyword.Add("0");

                if (!string.IsNullOrEmpty(_SelectedItemNew))
                    this.Keyword.AddRange(_SelectedItemNew.Split(',').ToList());

                //更新持仓盈亏
                ReCalcStatistics();
            }), System.Windows.Threading.DispatcherPriority.Send);
        }
        /// <summary>
        /// 订阅持仓明细
        /// </summary>
        private void Subscribe()
        {
            if (LstBook.Count > 0)
            {
                List<string> bookidlist = new List<string>();
                foreach (PostionBookData p in LstBook)
                {
                    bookidlist.Add(p.id.ToString());
                }

                if (!IsSubscribe)
                {
                    PositionHandler.Instance.SubscribeTradeStatistics(bookidlist, OnTradeStatisticsCallBack);
                }

                IsSubscribe = true;

            }
        }

        /// <summary>
        /// 筛选数据
        /// </summary>
        public void RefreshKeyWord()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Keyword.Clear();
                this.Keyword.Add("0");

                foreach (PostionBookData pbd in LstBook)
                {
                    if (pbd.IsChecked)
                    {
                        this.Keyword.Add(pbd.id.ToString());
                    }
                }

                ReCalcStatistics();
            }), System.Windows.Threading.DispatcherPriority.Send);
        }

        private void StatisticsMv_Filter(object sender, FilterEventArgs e)
        {
            if (Keyword.Count == 1)
            {
                e.Accepted = true;
                return;
            }
            else
            {
                TradeStatisticsData tsd = e.Item as TradeStatisticsData;
                if (Keyword.Contains(tsd.bookid))
                {
                    e.Accepted = true;
                }
                else
                {
                    e.Accepted = false;
                }
            }
        }

        /// <summary>
        /// 成交统计回调
        /// </summary>
        /// <param name="tradeStatistics"></param>
        private void OnTradeStatisticsCallBack(List<TradeStatisticsModel> tradeStatistics)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (lockObj)
                {
                    List<TradeStatisticsModel> selfData = new List<TradeStatisticsModel>(tradeStatistics.Where(o => SelfBookDict.ContainsKey(o.bookId)));
                    if (selfData.Count == 0)
                    {
                        return;
                    }

                    foreach (TradeStatisticsModel pbm in selfData)
                    {
                        //加入筛选，如果不是自己的则舍弃
                        pbm.bookName = BookDict.ContainsKey(pbm.bookId) ? BookDict[pbm.bookId] : "";
                        pbm.key = $"{pbm.bookId}#{pbm.securityId}";

                        TradeStatisticsData m2 = FullTradeStatistics.FirstOrDefault(o => o.key == pbm.key);
                        if (m2 != null)
                        {
                            m2.lastUpdateTime = pbm.lastUpdateTime;

                            if (Consts.MARKETINDICATOR_FUTURE.Equals(pbm.marketIndicator))
                            {
                                // 买成交量（手）
                                m2.longLot = CommonUtils.ConvertToDecimal(pbm.longLot.ToString("N0"));
                                // 卖成交量（手）
                                m2.shortLot = CommonUtils.ConvertToDecimal(pbm.shortLot.ToString("N0"));
                                // 净成交量（手）
                                m2.netLot = CommonUtils.ConvertToDecimal(pbm.netLot.ToString("N0"));
                                // 总成交量（手）
                                m2.totalLot = CommonUtils.ConvertToDecimal(pbm.totalLot.ToString("N0"));
                                // 总成交金额（万元）
                                m2.totalAmt = CommonUtils.ConvertToDecimal(decimal.Divide(pbm.totalAmt, 10000).ToString("N4"));
                                // 成交均价（买）
                                m2.longAvgPrice = pbm.longAvgPrice.ToString("F4");
                                // 成交均价（卖）
                                m2.shortAvgPrice = pbm.shortAvgPrice.ToString("F4");
                            }
                            else
                            {
                                // 买成交量（万元）
                                m2.longVolume = CommonUtils.ConvertToDecimal(pbm.longVolume.ToString("N4"));
                                // 卖成交量（万元）
                                m2.shortVolume = CommonUtils.ConvertToDecimal(pbm.shortVolume.ToString("N4"));
                                // 净成交量（万元）
                                m2.netVolume = CommonUtils.ConvertToDecimal(pbm.netVolume.ToString("N4"));
                                // 总成交量（万元）
                                m2.totalVolume = CommonUtils.ConvertToDecimal(pbm.totalVolume.ToString("N4"));
                                // 总成交金额（万元）
                                m2.totalAmt = CommonUtils.ConvertToDecimal(pbm.totalAmt.ToString("N4"));
                                m2.longAvgPrice = pbm.longAvgPrice.ToString("F4");
                                m2.shortAvgPrice = pbm.shortAvgPrice.ToString("F4");
                            }
                        }
                        else
                        {
                            //成交统计无需根据最新价计算

                            //if (!SubscribeSecurityidDict.ContainsKey(pbm.securityId))
                            //{
                            //    SubscribeSecurityidLastExecution(pbm);
                            //}

                            TradeStatisticsData newts = new TradeStatisticsData();
                            newts.bookid = pbm.bookId;
                            newts.securityid = pbm.securityId;
                            newts.symbol = pbm.symbol;
                            newts.symboltype = pbm.symbolType;
                            newts.key = pbm.key;
                            newts.lastUpdateTime = pbm.lastUpdateTime;
                            newts.bookname = pbm.bookName;

                            if (Consts.MARKETINDICATOR_FUTURE.Equals(pbm.marketIndicator))
                            {
                                // 买成交量（手）
                                newts.longLot = CommonUtils.ConvertToDecimal(pbm.longLot.ToString("N0"));
                                // 卖成交量（手）
                                newts.shortLot = CommonUtils.ConvertToDecimal(pbm.shortLot.ToString("N0"));
                                // 净成交量（手）
                                newts.netLot = CommonUtils.ConvertToDecimal(pbm.netLot.ToString("N0"));
                                // 总成交量（手）
                                newts.totalLot = CommonUtils.ConvertToDecimal(pbm.totalLot.ToString("N0"));
                                // 总成交金额（万元）
                                newts.totalAmt = CommonUtils.ConvertToDecimal(decimal.Divide(pbm.totalAmt, 10000).ToString("N4"));
                                // 成交均价（买）
                                newts.longAvgPrice = pbm.longAvgPrice.ToString("F4");
                                // 成交均价（卖）
                                newts.shortAvgPrice = pbm.shortAvgPrice.ToString("F4");
                            }
                            else
                            {
                                // 买成交量（万元）
                                newts.longVolume = CommonUtils.ConvertToDecimal(pbm.longVolume.ToString("N4"));
                                // 卖成交量（万元）
                                newts.shortVolume = CommonUtils.ConvertToDecimal(pbm.shortVolume.ToString("N4"));
                                // 净成交量（万元）
                                newts.netVolume = CommonUtils.ConvertToDecimal(pbm.netVolume.ToString("N4"));
                                // 总成交量（万元）
                                newts.totalVolume = CommonUtils.ConvertToDecimal(pbm.totalVolume.ToString("N4"));
                                newts.totalAmt = CommonUtils.ConvertToDecimal(pbm.totalAmt.ToString("N4"));
                                newts.longAvgPrice = pbm.longAvgPrice.ToString("F4");
                                newts.shortAvgPrice = pbm.shortAvgPrice.ToString("F4");
                            }
                            newts.marketindicator = pbm.marketIndicator;

                            FullTradeStatistics.Add(newts);
                        }
                    }

                    //更新成交统计
                    ReCalcStatistics();
                }
            }));
        }

        /// <summary>
        /// 更新成交统计汇总
        /// </summary>
        public void ReCalcStatistics()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                // 买成交量（手）
                decimal total_longLot = 0m;
                // 卖成交量（手）
                decimal total_shortLot = 0m;
                // 净成交量（手）
                decimal total_netLot = 0m;
                // 总成交量（手）
                decimal total_totalLot = 0m;
                // 买成交量（元）
                decimal total_longVolume = 0m;
                // 卖成交量（元）
                decimal total_shortVolume = 0m;
                // 净成交量（元）
                decimal total_netVolume = 0m;
                // 总成交量（元）
                decimal total_totalVolume = 0m;
                // 总成交金额（元）
                decimal total_totalAmt = 0m;

                if (FullTradeStatistics.Count > 1)
                {
                    if (this.Keyword.Count > 1)
                    {
                        ObservableCollection<TradeStatisticsData> Tmp = new ObservableCollection<TradeStatisticsData>(FullTradeStatistics.Where(p => this.Keyword.Contains(p.bookid) && p.bookid != "0"));
                        if (Tmp.Count > 0)
                        {
                            total_longLot = Tmp.Sum(p => p.longLot);
                            total_shortLot = Tmp.Sum(p => p.shortLot);
                            total_netLot = Tmp.Sum(p => p.netLot);
                            total_totalLot = Tmp.Sum(p => p.totalLot);
                            total_longVolume = Tmp.Sum(p => p.longVolume);
                            total_shortVolume = Tmp.Sum(p => p.shortVolume);
                            total_netVolume = Tmp.Sum(p => p.netVolume);
                            total_totalVolume = Tmp.Sum(p => p.totalVolume);
                            total_totalAmt = Tmp.Sum(p => p.totalAmt);
                        }
                    }
                    else
                    {
                        ObservableCollection<TradeStatisticsData> Tmp2 = new ObservableCollection<TradeStatisticsData>(FullTradeStatistics.Where(p => p.bookid != "0"));
                        if (Tmp2.Count > 0)
                        {
                            total_longLot = Tmp2.Sum(p => p.longLot);
                            total_shortLot = Tmp2.Sum(p => p.shortLot);
                            total_netLot = Tmp2.Sum(p => p.netLot);
                            total_totalLot = Tmp2.Sum(p => p.totalLot);
                            total_longVolume = Tmp2.Sum(p => p.longVolume);
                            total_shortVolume = Tmp2.Sum(p => p.shortVolume);
                            total_netVolume = Tmp2.Sum(p => p.netVolume);
                            total_totalVolume = Tmp2.Sum(p => p.totalVolume);
                            total_totalAmt = Tmp2.Sum(p => p.totalAmt);
                        }
                    }

                    //更新数据
                    TradeStatisticsData m2 = FullTradeStatistics.FirstOrDefault(o => o.bookid == "0");
                    if (m2 != null)
                    {
                        m2.longLot = CommonUtils.ConvertToDecimal(total_longLot.ToString("N0"));
                        m2.shortLot = CommonUtils.ConvertToDecimal(total_shortLot.ToString("N0"));
                        m2.netLot = CommonUtils.ConvertToDecimal(total_netLot.ToString("N0"));
                        m2.totalLot = CommonUtils.ConvertToDecimal(total_totalLot.ToString("N0"));
                        m2.longVolume = CommonUtils.ConvertToDecimal(total_longVolume.ToString());
                        m2.shortVolume = CommonUtils.ConvertToDecimal(total_shortVolume.ToString());
                        m2.netVolume = CommonUtils.ConvertToDecimal(total_netVolume.ToString());
                        m2.totalVolume = CommonUtils.ConvertToDecimal(total_totalVolume.ToString());
                        m2.totalAmt = CommonUtils.ConvertToDecimal(total_totalAmt.ToString());
                    }
                }

                this.StatisticsMv.View.Refresh();
            }));
        }
    }
}
