using com.gateway.log;
using STC.Common.Handler.Position;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Position;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.View.UI;
using STC.Position.Plugin.Command;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Position.Plugin.ViewModel
{
    public class HistoryPositionOrderDetailViewModel : ViewModelBase
    {
        #region 持仓明细构造

        /// <summary>
        /// 更新锁
        /// </summary>
        private object lockObj = new object();

        /// <summary>
        /// 字典锁
        /// </summary>
        private object LockDict = new object();

        /// <summary>
        /// 组合和组合ID对照表
        /// </summary>
        private Dictionary<string, string> BookDict = new Dictionary<string, string>();

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
        /// 持仓订单明细构造
        /// </summary>
        private ObservableCollection<HistoryOrderData> fullPositionOrderDetail = new ObservableCollection<HistoryOrderData>();
        public ObservableCollection<HistoryOrderData> FullPositionOrderDetail
        {
            get
            {
                return fullPositionOrderDetail;
            }
            set
            {
                fullPositionOrderDetail = value;
                this.OnPropertyChanged(nameof(FullPositionOrderDetail));
            }
        }


        private int _rowCount = 0;
        /// <summary>
        /// 订单个数
        /// </summary>
        public int RowCount
        {
            get { return _rowCount; }
            set
            {
                if (_rowCount != value)
                {
                    _rowCount = value;
                    this.OnPropertyChanged(nameof(RowCount));
                }
            }
        }

        private int _ofrrowCount = 0;
        /// <summary>
        /// Ofr订单个数
        /// </summary>
        public int OfrRowCount
        {
            get { return _ofrrowCount; }
            set
            {
                if (_ofrrowCount != value)
                {
                    _ofrrowCount = value;
                    this.OnPropertyChanged(nameof(OfrRowCount));
                }
            }
        }

        private int _bidrowcount = 0;
        /// <summary>
        /// Bid订单个数
        /// </summary>
        public int BidRowCount
        {
            get { return _bidrowcount; }
            set
            {
                if (_bidrowcount != value)
                {
                    _bidrowcount = value;
                    this.OnPropertyChanged(nameof(BidRowCount));
                }
            }
        }

        private decimal _totalbid = 0;
        /// <summary>
        /// Bid总量
        /// </summary>
        public decimal TotalBid
        {
            get { return _totalbid; }
            set
            {
                if (_totalbid != value)
                {
                    _totalbid = value;
                    this.OnPropertyChanged(nameof(TotalBid));
                }
            }
        }

        private decimal _totalofr = 0;
        /// <summary>
        /// Ofr总量
        /// </summary>
        public decimal TotalOfr
        {
            get { return _totalofr; }
            set
            {
                if (_totalofr != value)
                {
                    _totalofr = value;
                    this.OnPropertyChanged(nameof(TotalOfr));
                }
            }
        }

        private decimal _totalqty = 0;
        /// <summary>
        /// 总量
        /// </summary>
        public decimal TotalQty
        {
            get { return _totalqty; }
            set
            {
                if (_totalqty != value)
                {
                    _totalqty = value;
                    this.OnPropertyChanged(nameof(TotalQty));
                }
            }
        }


        public List<KeyValuePair<string, string>> LstBookList = new List<KeyValuePair<string, string>>();


        #endregion

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
                }
            }
        }

        private string _dealTimeBegin = string.Empty;
        /// <summary>
        /// DealTimeBegin
        /// </summary>
        public string DealTimeBegin
        {
            get { return _dealTimeBegin; }
            set
            {
                if (_dealTimeBegin != value)
                {
                    _dealTimeBegin = value;

                    this.OnPropertyChanged(nameof(DealTimeBegin));
                }
            }
        }

        private string _dealTimeEnd = string.Empty;
        /// <summary>
        /// DealTimeEnd
        /// </summary>
        public string DealTimeEnd
        {
            get { return _dealTimeEnd; }
            set
            {
                if (_dealTimeEnd != value)
                {
                    _dealTimeEnd = value;
                    this.OnPropertyChanged(nameof(DealTimeEnd));
                }
            }
        }

        private string _bookid = string.Empty;
        public string BookId
        {
            get
            {
                return _bookid;
            }
            set
            {
                if (_bookid != value)
                {
                    _bookid = value;
                    this.OnPropertyChanged(nameof(BookId));
                }
            }
        }

        private HistoryPositionOrderDetail historypositionview;
        public HistoryPositionOrderDetail HistoryView
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

        /// <summary>
        /// 成交手工录入
        /// </summary>
        public ICommand ExportCsvCommand { get { return new DelegateCommand(new Action<object>(OnExportCsvCommandClick)); } }
        public ICommand SelfRelatedCommand { get { return new DelegateCommand(new Action<object>(OnSelfRelatedCommandClick)); } }
        public ICommand SelfNotRelatedCommand { get { return new DelegateCommand(new Action<object>(OnSelfNotRelatedCommandClick)); } }
        public ICommand DelBtnCommand { get { return new DelegateCommand(new Action<object>(OnDelBtnCommandClick)); } }
        public ICommand RefreshCommand { get { return new ComRelayCommand(OnRefreshClick); } }


        #region Page Control


        private string _newbookid = string.Empty;
        public string NewBookId
        {
            get
            {
                return _newbookid;
            }
            set
            {
                _newbookid = value;
                this.OnPropertyChanged(nameof(NewBookId));
            }
        }

        private string _changedexecid = string.Empty;
        public string ChangedExecId
        {
            get
            {
                return _changedexecid;
            }
            set
            {
                _changedexecid = value;
                this.OnPropertyChanged(nameof(ChangedExecId));
            }
        }


        private int totalCount = 0;
        public int TotalCount
        {
            get
            {
                return totalCount;
            }

            set
            {
                totalCount = value;
                this.OnPropertyChanged();
            }
        }


        private int emsindex = 0;
        public int EMSIndex
        {
            get
            {
                return emsindex;
            }

            set
            {
                emsindex = value;
            }
        }


        private string emsPageVisible = "false";
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public string EMSPageVisible
        {
            get
            {
                return emsPageVisible;
            }

            set
            {
                emsPageVisible = value;
                this.OnPropertyChanged();
            }
        }


        private View.PageControl _emsPageControl;
        public View.PageControl EMSPageControl
        {
            get
            {
                return _emsPageControl;
            }

            set
            {
                _emsPageControl = value;
                this.OnPropertyChanged();
            }
        }




        #endregion
        public HistoryPositionOrderDetailViewModel()
        {
            //初始化视图
            InitView();
            //查询所有组合名称
            GetAllBook();
            //查询当前用户组合
            GetBookUser();
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        private void InitView()
        {
            this.Mv.Source = FullPositionOrderDetail;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            //this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
        }


        #region 查询数据和清空数据
        /// <summary>
        /// 清空数据
        /// </summary>
        public void ClearData()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                lock (lockObj)
                {
                    FullPositionOrderDetail.Clear();
                    this.Mv.View.Refresh();
                    RowCount = 0;
                    BidRowCount = 0;
                    OfrRowCount = 0;
                    TotalQty = 0;
                    TotalBid = 0;
                    TotalOfr = 0;
                    EMSPageVisible = "false";
                }
            }));
        }



        /// <summary>
        /// 查询数据
        /// </summary>
        public void QueryData()
        {
           ReQueryOrderDetail();
        }

        /// <summary>
        /// 异步查询子订单
        /// </summary>
        /// <param name="securityid"></param>
        private async void ReQueryOrderDetail()
        {
            await Task.Run(() =>
            {
                QueryExternalReq req = new QueryExternalReq();
                if (string.IsNullOrEmpty(this.InSecurityId))
                {
                    req.dealTimeBegin = this.DealTimeBegin;
                    req.dealTimeEnd = this.DealTimeEnd;
                }
                else
                {
                    req.securityid = this.InSecurityId;
                    if (!string.IsNullOrEmpty(this.DealTimeBegin))
                        req.dealTimeBegin = this.DealTimeBegin;
                    if (!string.IsNullOrEmpty(this.DealTimeEnd))
                        req.dealTimeEnd = this.DealTimeEnd;
                }

                if (!string.IsNullOrEmpty(this.BookId))
                {
                    req.bookid = this.BookId;
                }

                req.pageNum = this.EMSIndex;
                req.pageSize = CommonConstants.pageCount;
                //req.pageSize = 100;
                GetSecurityIdAllOrder(req);
            });
        }

        #endregion


        #region 查询关联所有订单
        public async void GetSecurityIdAllOrder(QueryExternalReq req)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                lock (lockObj)
                {
                    HistoryView.ShowEMSMask(true);
                    FullPositionOrderDetail.Clear();
                    EMSPageVisible = "false";
                    RowCount = 0;
                    BidRowCount = 0;
                    OfrRowCount = 0;
                    TotalQty = 0;
                    TotalBid = 0;
                    TotalOfr = 0;
                    //this.Mv.View.Refresh();
                }
            }));

            //if (string.IsNullOrEmpty(secId))
            //{
            //    HistoryView.ShowEMSMask(false);
            //    return;
            //}

            try
            {
                QueryExternalResult reply = await GetData(req);

                if (reply != null)
                {
                    List<HistoryOrderData> tmp = new List<HistoryOrderData>();
                    if (reply.Datas != null && reply.Datas.Count > 0)
                    {
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            TotalCount = reply.totalSize;
                            EMSPageControl.RecordCount = TotalCount;
                            EMSPageControl.PageSize = CommonConstants.pageCount;
                            if (TotalCount > CommonConstants.pageCount)
                            {
                                EMSPageVisible = "true";
                            }
                            else
                            {
                                EMSPageVisible = "false";
                            }
                        }));

                        foreach (QueryExternalResultData obj in reply.Datas)
                        {
                            HistoryOrderData hod = new HistoryOrderData();
                            hod.BookID = obj.bookid;

                            if (obj.bookid != null)
                            {
                                hod.BookName = BookDict.ContainsKey(obj.bookid) ? BookDict[obj.bookid] : "";
                            }

                            hod.ClOrdID = obj.clordid;
                            hod.Venueexecid = obj.venueexecid;
                            hod.CounterPartyName = obj.counterpartyname;
                            hod.CounterPartySubName = obj.counterpartysubname;
                            hod.CreateTime = obj.createtime; //成交时间
                            hod.ExecID = obj.execid;
                            hod.ExecutionTime = obj.transacttime;
                            hod.LastPx = CommonUtils.ConvertToDecimal(obj.yield);
                            hod.LastQty = obj.qty;
                            hod.NetPrice = CommonUtils.ConvertToDecimal(obj.netprice) == 0 ? "--" : string.Format("{0:N4}", CommonUtils.ConvertToDecimal(obj.netprice)); // 净价
                            hod.NetPrice1 = CommonUtils.ConvertToDecimal(obj.netprice);
                            hod.Qty = CommonUtils.ConvertToLong(obj.qty.ToString());
                            hod.Remark = obj.remark;
                            hod.SecurityID = obj.securityid;
                            hod.SettlType = obj.settltype; // 清算速度
                            hod.Side = obj.side; // 方向
                            hod.StrategyOrderId = obj.strategyorderid;
                            hod.Symbol = obj.symbol;
                            hod.SyncStatus = obj.synchflag;
                            hod.DataType = obj.datatype;
                            
                            string[] format = { "yyyy-MM-dd HH:mm:ss", "yyyy-mm-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyy-MM-dd HH:mm:ss", "yyyyMMdd HH:mm:ss", "M/d/yyyy" , "yyyyMMdd"};
                            DateTime date = DateTime.Now;
                            //if (obj.tradedate.ToString().Length >= 19)
                            {
                                if (DateTime.TryParseExact(obj.tradedate.ToString(), format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                                {
                                    hod.TradeDate = date.ToString("yyyy-MM-dd HH:mm:ss");
                                }
                            }

                            hod.Veneues = obj.venues;// 交易场所
                            //处理按钮是否可见 RelatedButtonVisible NotRelatedButtonVisible  DelBtnVisible
                            //1 EMS 2 Manual人工
                            hod.NotRelatedButtonVisible = string.IsNullOrWhiteSpace(obj.bookid) ? "true" : "false";
                            hod.RelatedButtonVisible = !string.IsNullOrWhiteSpace(obj.bookid) ? "true" : "false";
                            hod.DelBtnVisible = hod.DataType == "2" ? "true" : "false";
                            //hod.DelBtnVisible = "true";
                            tmp.Add(hod);
                        }

                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            lock (lockObj)
                            {
                                foreach (var x in tmp)
                                {
                                    FullPositionOrderDetail.Add(x);
                                }

                                RowCount = FullPositionOrderDetail.Count;

                                //arg.Add(new KeyValuePair<string, string>("1", "Bid"));
                                //arg.Add(new KeyValuePair<string, string>("2", "Ofr"));
                                //arg.Add(new KeyValuePair<string, string>("X", "TKN"));
                                //arg.Add(new KeyValuePair<string, string>("Y", "GVN"));
                                //arg.Add(new KeyValuePair<string, string>("Z", "TRD"));

                                ObservableCollection<HistoryOrderData> TmpBid = new ObservableCollection<HistoryOrderData>(FullPositionOrderDetail.Where(p => p.Side == "1"));
                                if (TmpBid.Count > 0)
                                {
                                    BidRowCount = TmpBid.Count;
                                    TotalBid = TmpBid.Sum(p => p.Qty);
                                }

                                ObservableCollection<HistoryOrderData> TmpOfr = new ObservableCollection<HistoryOrderData>(FullPositionOrderDetail.Where(p => p.Side == "2"));
                                if (TmpOfr.Count > 0)
                                {
                                    OfrRowCount = TmpOfr.Count;
                                    TotalOfr = TmpOfr.Sum(p => p.Qty);
                                }

                                TotalQty = TotalBid + TotalOfr;

                                this.Mv.View.Refresh();
                            }
                        }));
                    }
                    else
                    {
                        //if (reply.Code.ToString() != "0000")
                        //    showMessage(reply.Msg);
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
            finally
            {

            }

            this.HistoryView.ShowEMSMask(false);
        }

        private async Task<QueryExternalResult> GetData(QueryExternalReq req)
        {
            QueryExternalResult reply = null;
            await Task.Run(() =>
            {
                reply = PositionHandler.Instance.QueryExternal(req);
            });

            return reply;
        }

        #endregion

        #region 关联取消关联删除操作

        /// <summary>
        /// 已关联
        /// </summary>
        /// <param name="param"></param>
        private void OnSelfRelatedCommandClick(object param)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                string execid = param as string;
                this.ChangedExecId = execid;
                HistoryOrderData m2 = FullPositionOrderDetail.FirstOrDefault(o => o.ExecID == execid);
                string bookname = string.Empty;
                if (m2 != null)
                {
                    bookname =  m2.BookName;
                }

                MessageBoxGUI me = new MessageBoxGUI();
                me.MOK += Me_CancelRelated;
                me.Owner = Application.Current.MainWindow;
                me.Title = "提示";
                me.setWarnMsg($"确定要将订单【{execid}】\r\n移出组合【{bookname}】吗?");
                me.ShowDialog();
            }));
        }

        private void Me_CancelRelated()
        {
            try
            {
                string execid = this.ChangedExecId;
                UpdatePositionReq req = new UpdatePositionReq();
                req.operationType = "1";
                req.orderID = execid;
                req.newBookID = string.Empty;
                if (UpdatePosition(req))
                {
                    HistoryOrderData m2 = FullPositionOrderDetail.FirstOrDefault(o => o.ExecID == execid);
                    if (m2 != null)
                    {
                        m2.BookID = string.Empty;
                        m2.BookName = string.Empty;

                        //1 EMS 2 Manual人工
                        m2.NotRelatedButtonVisible = "true";
                        m2.RelatedButtonVisible = "false";
                        m2.DelBtnVisible = m2.DataType == "2" ? "true" : "false";
                    }

                    this.Mv.View.Refresh();
                }

                this.NewBookId = string.Empty;
                this.ChangedExecId = string.Empty;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }

        }

        /// <summary>
        /// 未关联操作
        /// </summary>
        /// <param name="param"></param>
        private void OnSelfNotRelatedCommandClick(object param)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    this.NewBookId = string.Empty;
                    this.ChangedExecId = string.Empty;
                    string execid = param as string;
                    RelatedPositionsView mi = new RelatedPositionsView(this, execid);
                    mi.Show();
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }));
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="own"></param>
        /// <returns></returns>
        public bool UpdatePosition(UpdatePositionReq req , Window own = null)
        {
            try
            {
                PResult reply = PositionHandler.Instance.UpdatePosition(req);
                if (reply != null && reply.IsSuccess())
                {
                    return true;
                }
                else
                {
                    string errmsg = string.Empty;
                    if (reply != null)
                    {
                        errmsg = $"失败{reply.Msg}";
                        CustomMessageBox.Error($"保存分组失败:\r\n {reply.Msg}", "提示", own);
                    }
                    else
                    {
                        errmsg = $"失败";
                        CustomMessageBox.Error($"保存分组失败！", "提示", own);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                CustomMessageBox.Error($"保存分组失败: \r\n{ex.Message}", "提示", own);
            }

            return false;
        }

        /// <summary>
        /// 保存关联
        /// </summary>
        /// <returns></returns>
        public bool CommitRelated()
        {
            HistoryOrderData m2 = FullPositionOrderDetail.FirstOrDefault(o => o.ExecID == this.ChangedExecId);
            if (m2 != null)
            {
                if (!string.IsNullOrEmpty(this.NewBookId))
                {
                    m2.BookID = this.NewBookId;
                    m2.BookName = BookDict.ContainsKey(this.NewBookId) ? BookDict[this.NewBookId] : "";

                    //1 EMS 2 Manual人工
                    m2.NotRelatedButtonVisible = string.IsNullOrWhiteSpace(this.NewBookId) ? "true" : "false";
                    m2.RelatedButtonVisible = !string.IsNullOrWhiteSpace(this.NewBookId) ? "true" : "false";
                    m2.DelBtnVisible = m2.DataType == "2" ? "true" : "false";
                }

                this.Mv.View.Refresh();
                this.NewBookId = string.Empty;
                this.ChangedExecId = string.Empty;
            }

            return true;
        }

        /// <summary>
        /// 删除记录
        /// </summary>
        /// <param name="param"></param>
        private void OnDelBtnCommandClick(object param)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                string execid = param as string;
                this.ChangedExecId = execid;
                MessageBoxGUI me = new MessageBoxGUI();
                me.MOK += Me_MOK;
                me.Owner = Application.Current.MainWindow;
                me.Title = "提示";
                me.setWarnMsg($"确定删除成交录入订单\r\n【{execid}】 吗?");
                me.ShowDialog();
            }));
        }

        /// <summary>
        /// 删除操作
        /// </summary>
        private void Me_MOK()
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                try
                {
                    string execid = this.ChangedExecId;
                    UpdatePositionReq req = new UpdatePositionReq();
                    req.operationType = "0";
                    req.orderID = execid;
                    if (UpdatePosition(req))
                    {
                        HistoryOrderData m2 = FullPositionOrderDetail.FirstOrDefault(o => o.ExecID == execid);
                        if (m2 != null)
                        {
                            FullPositionOrderDetail.Remove(m2);
                        }

                        this.Mv.View.Refresh();
                    }
                    
                    this.NewBookId = string.Empty;
                    this.ChangedExecId = string.Empty;
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }));
        }

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="param"></param>
        private void OnExportCsvCommandClick(object param)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = $"持仓成交流水记录_{DateTimeEx.ServerTime.ToString("yyyyMMdd")}";
                saveFileDialog.AddExtension = true;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    ExportCSV(fileName);
                }
            }));
        }

        private void ExportCSV(string fileName)
        {
            try
            {
                using (StreamWriter writer = CreateStream(fileName))
                {
                    // 输出表头
                    writer.WriteLine($"代码,方向,成交价(%),成交净价,成交量（万元）,渠道,清算速度,创建日期,交易日期,成交时间,对手方,对手交易员,成交编号,订单编号,组合,组合ID,备注");
                    //ObservableCollection<HistoryOrderData> FullPositionOrderDetail
                    foreach (HistoryOrderData rbm in FullPositionOrderDetail)
                    {
                        string cside = ComboSourceUtil.Instance.FindEnum(rbm.Side, "side");
                        string cLastPx = CommonUtils.FormatPrice(rbm.LastPx.ToString());
                        string cNetPrice = CommonUtils.FormatPrice(rbm.NetPrice);
                        string cVenues_exe = ComboSourceUtil.Instance.FindEnum(rbm.Veneues, "venues_exe");
                        string cSettlType = ComboSourceUtil.Instance.FindEnum(rbm.SettlType, "settletype_myOrder");
                        string cCreateTime = string.Empty;
                        if (!string.IsNullOrEmpty(rbm.CreateTime))
                            cCreateTime = CommonUtils.ConvertToDateTime(rbm.CreateTime).ToString("yyyy-MM-dd");
                        string cTradeDate = string.Empty;
                        string cTradeTime = string.Empty;
                        if (!string.IsNullOrEmpty(rbm.TradeDate))
                        {
                            cTradeDate = CommonUtils.ConvertToDateTime(rbm.TradeDate).ToString("yyyy-MM-dd");
                            cTradeTime = CommonUtils.ConvertToDateTime(rbm.TradeDate).ToString("HH:mm:ss");
                        }
                        
                        writer.WriteLine($"\"{rbm.SecurityID}\",\"{cside}\",\"{cLastPx}\",{cNetPrice},{rbm.Qty},\"{cVenues_exe}\",\"{cSettlType}\",\"{cCreateTime}\",\"{cTradeDate}\",\"{cTradeTime}\",\"{rbm.CounterPartyName}\",\"{rbm.CounterPartySubName}\",\"{rbm.ExecID}\",\"{rbm.StrategyOrderId}\",\"{rbm.BookName}\",\"{rbm.BookID}\",\"{rbm.Remark}\"");
                    }
                    // 释放资源
                    writer.Flush();
                    writer.Close();
                }
                
                showSuccessMessage($"订单导出成功!\r\n{fileName}");

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                STC.Common.View.Component.CustomMessageBox.Error($"订单导出失败!\r\n{e.Message}", "提示", null);
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
        #endregion

        #region 查询字典表
        public void GetAllBook()
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

        /// <summary>
        /// 查询用户有权限的组合
        /// </summary>
        public void GetBookUser()
        {
            QueryPositionBookReq req = new QueryPositionBookReq();
            req.username = LocalEnv.UserInfo.UserName;
            req.userid = LocalEnv.UserInfo.UserId;
            QueryPositionBookResult reply = PositionHandler.Instance.QueryPositionBook(req);
            if (reply != null && reply.IsSuccess())
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    foreach (PostionBookModel pbm in reply.Datas)
                    {
                        LstBookList.Add(new KeyValuePair<string, string>(pbm.id, $"{pbm.id}-{pbm.bookname}"));
                    }
                }
            }
        }
        #endregion

        #region 消息提醒
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

        /// <summary>
        /// 提示成功消息弹框
        /// </summary>
        /// <param name="message"></param>
        public void showSuccessMessage(string message)
        {
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                SuccessBoxView me = new SuccessBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.Owner = null;
                me.ShowDialog();
            }));
        }
        #endregion

        private void OnRefreshClick()
        {
            QueryData();
        }
    }
}
