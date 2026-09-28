using com.gateway.log;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View;
using BDB.Trade.Plugin.View.BrokerKanban;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using STC.Common.Model;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.View.Component;
using STC.Common.Model.QTR;
using STC.Common.Handler.QTRS;

namespace BDB.Trade.Plugin.ViewModel.BrokerKanban
{
    /// <summary>
    /// 需求意向ViewModel
    /// </summary>
    public class QtrsOrderViewModel : BaseViewModel
    {
        #region 需求意向构造
        private object lockObj = new object();
        /// <summary>
        /// 持仓明细构造
        /// </summary>
        private ObservableCollection<QtrsOrderData> fulldi = new ObservableCollection<QtrsOrderData>();
        public ObservableCollection<QtrsOrderData> FullDI
        {
            get
            {
                return fulldi;
            }
            set
            {
                fulldi = value;
                this.OnPropertyChanged(nameof(FullDI));
            }
        }



        private QtrsOrderData selectedDI = null;
        /// <summary>
        /// 单击选择的需求意向
        /// </summary>
        public QtrsOrderData SelectedDI
        {
            get
            {
                return selectedDI;
            }

            set
            {
                selectedDI = value;
                this.OnPropertyChanged(nameof(SelectedDI));

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (selectedDI != null)
                    {
                        BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, null, null, selectedDI.securityID, "全", true);
                    }
                    else
                    {
                        BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, null, null, string.Empty, "全", true);
                    }
                }));
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



        private QtrsOrderView historypositionview;
        public QtrsOrderView HistoryView
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

        bool IsInit = false;

        #endregion

        #region Command 构造
        public ICommand FullScreenCommand { get { return new RelayCommand(OnFullScreenCommandClick); } }
        public ICommand ExportCsvCommand { get { return new RelayCommand(OnExportCsvCommandClick); } }
        public ICommand HistoryCommand { get { return new RelayCommand(OnHistoryCommandClick); } }
        #endregion


        public QtrsOrderViewModel()
        {
            ////初始化视图
            //InitView();

        }


        ~QtrsOrderViewModel()
        {

        }

        /// <summary>
        /// 订阅经纪商需求意向
        /// </summary>
        private void SubscribeQtrOrder()
        {
           BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);
           QtrsHandler.Instance.SubscribeQtrOrder(OnQtrOrderCallBack);
        }

        bool isLoadBond = false;
        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
               isLoadBond = true;
               lock (lockObj)
               {
                    foreach (QtrsOrderData pbm in FullDI)
                    {
                        BondsInfoPo result = CommonUtils.GetBondsInfoPoBySecurityID(pbm.securityID);
                        if (result != null)
                        {
                            pbm.securityType = result.SECURITY_TYPE;
                            pbm.symbol = result.Short_Name;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 订阅经纪商需求意向回调
        /// </summary>
        /// <param name="riskIndicator"></param>
        private void OnQtrOrderCallBack(List<QtrOrder> riskIndicator)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(K =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (QtrOrder pbm in riskIndicator)
                    {
                        lock (lockObj)
                        {
                            QtrsOrderData m2 = FullDI.FirstOrDefault(o => o.orderID == pbm.orderID);
                            ///0 = 有效 New
                            ///1 = 部分成交Partially filled
                            ///2 = 全部成交 Filled
                            ///4 = 撤销 Canceled
                            ///8 = 拒绝 Reject
                            ///C = 过期 Expired"
                            if (m2 != null)
                            {
                                m2.senderCompID = pbm.senderCompID;
                                m2.deliverToCompID = pbm.deliverToCompID;
                                m2.deliverToSubID = pbm.deliverToSubID;

                                m2.comment = pbm.comment;
                                m2.cumQty = pbm.cumQty;

                                m2.execID = pbm.execID;
                                m2.execType = pbm.execType;
                                m2.execTypeReason = pbm.execTypeReason;
                                m2.ordRejReason = pbm.ordRejReason;
                                m2.ordStatus = pbm.ordStatus;
                                m2.rejectText = pbm.rejectText;
                                m2.price = pbm.price;
                                m2.yieldType = pbm.yieldType;
                                m2.yield = pbm.yield;


                                ////m2.firmflag = pbm.firmflag;
                                m2.lastPx = pbm.lastPx;
                                m2.lastQty = pbm.lastQty;
                                m2.leavesQty = pbm.leavesQty;
                                m2.orderQty = pbm.orderQty;
                                m2.cumQty = pbm.cumQty;
                                m2.settlDate = pbm.settlDate;
                                m2.settlType = pbm.settlType;
                                m2.venues = pbm.venues;
                                m2.tradeDate = pbm.tradeDate;        //可能null
                                m2.transactTime = pbm.transactTime;  //可能null

                                m2.sendingTime = pbm.sendingTime;
                                m2.updatetime = pbm.sendingTime;

                                m2.marketIndicator = pbm.marketIndicator;
                            }
                            else
                            {
                                QtrsOrderData tmpqt = new QtrsOrderData();
                                tmpqt.qmID = pbm.qmID;
                                tmpqt.orderID = pbm.orderID;
                                tmpqt.symbol = pbm.symbol;
                                tmpqt.securityID = pbm.securityID;
                                tmpqt.securityType = "null";

                                if (BaseHandler.Instance.IsLoad())
                                {
                                    BondsInfoPo result = CommonUtils.GetBondsInfoPoBySecurityID(pbm.securityID);
                                    if (result != null)
                                    {
                                        tmpqt.securityType = result.SECURITY_TYPE;
                                        tmpqt.symbol = result.Short_Name;
                                    }
                                }

                                ////tmpqt.cleanPrice = pbm.cleanPrice;
                                tmpqt.clOrdID = pbm.clOrdID;
                                tmpqt.execID = pbm.execID;

                                tmpqt.comment = pbm.comment;
                                tmpqt.cumQty = pbm.cumQty;
                                tmpqt.deliverToCompID = pbm.deliverToCompID;
                                tmpqt.deliverToSubID = pbm.deliverToSubID;
                                tmpqt.execType = pbm.execType;
                                tmpqt.execTypeReason = pbm.execTypeReason;
                                ////tmpqt.firmflag = pbm.firmflag;
                                tmpqt.lastPx = pbm.lastPx;
                                tmpqt.lastQty = pbm.lastQty;
                                tmpqt.leavesQty = pbm.leavesQty;
                                tmpqt.marketIndicator = pbm.marketIndicator;
                                tmpqt.orderQty = pbm.orderQty;
                                tmpqt.ordRejReason = pbm.ordRejReason;
                                tmpqt.ordStatus = pbm.ordStatus;
                                tmpqt.ordType = pbm.ordType;
                                tmpqt.price = pbm.price;
                                tmpqt.rejectText = pbm.rejectText;
                                tmpqt.senderCompID = pbm.senderCompID;
                                tmpqt.senderSubID = pbm.senderSubID;
                                tmpqt.sendingTime = pbm.sendingTime;
                                tmpqt.settlDate = pbm.settlDate;
                                tmpqt.settlType = pbm.settlType == "4"? "2,3" : pbm.settlType;
                                tmpqt.side = pbm.side;
                                tmpqt.text = pbm.text;
                                tmpqt.tradeDate = pbm.tradeDate;
                                tmpqt.transactTime = pbm.transactTime;
                                tmpqt.updatetime = pbm.transactTime;
                                tmpqt.venues = pbm.venues;
                                tmpqt.yield = pbm.yield;
                                tmpqt.yieldType = pbm.yieldType;
                                FullDI.Add(tmpqt);
                            }
                        }
                    }

                    ////更新记录数
                    //if (RowCount != FullDI.Count)
                    //{
                        ReCalcDemandIntention();
                    //}
                }));
            });
        }

        

        /// <summary>
        /// 更新经纪商需求意向
        /// </summary>
        public void ReCalcDemandIntention()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(K =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.Mv.View.Refresh();
                    RowCount = this.Mv.View.Cast<object>().Count();
                }));
            });
        }

        public void UnRegistEvent()
        {
            BaseHandler.Instance.UnSubscribeLoadDataStatus(OnLoadDataStatus);
            QtrsHandler.Instance.UnSubscribeQtrOrder(OnQtrOrderCallBack);
            base.Dispose();
        }

        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitView()
        {
            //默认选中未完成
            LstOrdStatus.Clear();
            LstOrdStatus.Add("0");
            LstOrdStatus.Add("1");
            RowCount = 0;
            FullDI.Clear();
            this.Mv = new CollectionViewSource();
            this.Mv.Source = FullDI;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
            this.Mv.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));


            if (!IsInit)
            {
                //订阅经纪商需求意向
                SubscribeQtrOrder();
                IsInit = true;
            }
        }

        #region 筛选数据

        private List<string> lstvenues = new List<string>();
        /// <summary>
        /// 经纪商列表
        /// </summary>
        public List<string> LstVenues
        {
            get
            {
                return lstvenues;
            }
            set
            {
                lstvenues = value;
                this.OnPropertyChanged(nameof(LstVenues));
            }
        }

        private List<string> lstsecurityType = new List<string>();
        /// <summary>
        /// 债券类型
        /// </summary>
        public List<string> LstSecurityType
        {
            get
            {
                return lstsecurityType;
            }
            set
            {
                lstsecurityType = value;
                this.OnPropertyChanged(nameof(LstSecurityType));
            }
        }

        private List<string> lstordStatus = new List<string>();
        /// <summary>
        /// 状态
        /// </summary>
        public List<string> LstOrdStatus
        {
            get
            {
                return lstordStatus;
            }
            set
            {
                lstordStatus = value;
                this.OnPropertyChanged(nameof(LstOrdStatus));
            }
        }

        /// <summary>
        /// 筛选债券
        /// </summary>
        private string securityId = string.Empty;
        public string SecurityId
        {
            get
            {
                return securityId;
            }
            set
            {
                securityId = value;
                this.OnPropertyChanged(nameof(SecurityId));
                RefreshKeyWord();
            }
        }


        public void ProcessFilterCondition(Dictionary<string, IList> filterDic)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                LstVenues.Clear();
                if (filterDic.ContainsKey("Venues"))
                {
                    IList venuesList = filterDic["Venues"];
                    if (venuesList.Count > 0)
                    {
                        bool isContainsAlVenuesl = false;
                        foreach (ListBoxItem item in venuesList)
                        {
                            if (item.Tag.ToString().Contains(","))
                            {
                                LstVenues.AddRange(item.Tag.ToString().Split(',').ToList());
                                isContainsAlVenuesl = true;
                                break;
                            }
                        }

                        if (!isContainsAlVenuesl)
                        {
                            foreach (ListBoxItem item in venuesList)
                            {
                                LstVenues.Add(item.Tag.ToString());
                            }
                        }
                    }
                    else
                    {
                        LstVenues.Clear();
                    }
                }

                LstSecurityType.Clear();
                if (filterDic.ContainsKey("SecurityType"))
                {
                    IList typeList = filterDic["SecurityType"];
                    if (typeList.Count > 0)
                    {
                        bool isContainsAllType = false;
                        foreach (ListBoxItem item in typeList)
                        {
                            if (item.Tag.ToString().Contains(","))
                            {
                                LstSecurityType.AddRange(item.Tag.ToString().Split(',').ToList());
                                isContainsAllType = true;
                                break;
                            }
                        }

                        if (!isContainsAllType)
                        {
                            foreach (ListBoxItem item in typeList)
                            {
                                LstSecurityType.Add(item.Tag.ToString());
                            }
                        }
                    }
                    else
                    {
                        LstSecurityType.Clear();
                    }
                }


                LstOrdStatus.Clear();
                if (filterDic.ContainsKey("OrdStatus"))
                {
                    IList typeList = filterDic["OrdStatus"];
                    if (typeList.Count > 0)
                    {
                        bool isContainsAllType = false;
                        foreach (ListBoxItem item in typeList)
                        {
                            if (item.Tag.ToString().Contains(","))
                            {
                                LstOrdStatus.AddRange(item.Tag.ToString().Split(',').ToList());
                                isContainsAllType = true;
                                break;
                            }
                        }

                        if (!isContainsAllType)
                        {
                            foreach (ListBoxItem item in typeList)
                            {
                                LstOrdStatus.Add(item.Tag.ToString());
                            }
                        }
                    }
                    else
                    {
                        LstOrdStatus.Clear();
                    }
                }

                RefreshKeyWord();
            }));
        }

        /// <summary>
        /// 筛选数据
        /// </summary>
        public void RefreshKeyWord()
        {
            this.Mv.View.Refresh();
            RowCount = this.Mv.View.Cast<object>().Count();
        }

        private void Mv_Filter(object sender, FilterEventArgs e)
        {
            QtrsOrderData pbd = e.Item as QtrsOrderData;
            if (string.IsNullOrEmpty(this.SecurityId)) 
            {
                if (LstSecurityType.Count == 0 && LstVenues.Count == 0)
                {
                    if (LstOrdStatus.Contains(pbd.ordStatus))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
                else
                {
                    if (LstSecurityType.Contains(pbd.securityType) && LstVenues.Contains(pbd.venues) && LstOrdStatus.Contains(pbd.ordStatus))
                    {
                        e.Accepted = true;
                    }
                    else
                    {
                        e.Accepted = false;
                    }
                }
            }
            else 
            {
                if (LstSecurityType.Count == 0 && LstVenues.Count == 0)
                {
                    if ((pbd.securityID == this.SecurityId) && LstOrdStatus.Contains(pbd.ordStatus))
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
                else
                {
                    if (LstSecurityType.Contains(pbd.securityType) && LstVenues.Contains(pbd.venues) && LstOrdStatus.Contains(pbd.ordStatus) && (pbd.securityID == this.SecurityId))
                    {
                        e.Accepted = true;
                    }
                    else
                    {
                        e.Accepted = false;
                    }
                }
            }
        }

        #endregion

        #region Command
        /// <summary>
        /// 全屏
        /// </summary>
        private void OnFullScreenCommandClick()
        {
            BorderWin myTradingPopupWin = new BorderWin();
            myTradingPopupWin.WindowState = WindowState.Normal;

            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            try
            {
                QtrsOrderView chrom = new QtrsOrderView(false);
                chrom.DataContext = this;
                myTradingPopupWin.SetMainControl(chrom);
                myTradingPopupWin.SetTitleAndKey("QtrsOrderViewMaximized", "需求意向");
                myTradingPopupWin.SetWebPageTitle("需求意向");
                myTradingPopupWin.Owner = null;
                WindowManager.Instance.OpenOnlyOneWindow($"QtrsOrderViewMaximized", myTradingPopupWin, null);
            }
            catch (Exception ex)
            {
                LogHandler.Error(ex.Message);
            }
        }

        #region 导出csv
        /// <summary>
        /// 导出
        /// </summary>
        private void OnExportCsvCommandClick()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
                saveFileDialog.FileName = $"需求意向_{DateTimeEx.ServerTime.ToString("yyyyMMdd")}";
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
                    writer.WriteLine($"方向,代码,简称,清算速度,收益率（%）,数量（万元）,已确认量（万元）,经纪商,状态,本方交易员,订单序号,备注,更新时间,成交单号");
                    //ObservableCollection<HistoryOrderData> FullPositionOrderDetail
                    foreach (QtrsOrderData rbm in FullDI)
                    {
                        string cside = ComboSourceUtil.Instance.FindEnum(rbm.side.ToString(), "side");
                        string cLastPx = CommonUtils.FormatPriceZeroEqualsBar(rbm.lastPx.ToString());
                        //string cNetPrice = CommonUtils.FormatPrice(rbm.NetPrice);
                        string cVenues_exe = ComboSourceUtil.Instance.FindEnum(rbm.venues, "venues_exe");
                        string cSettlType = ComboSourceUtil.Instance.FindEnum(rbm.settlType, "settletype_myOrder");
                        string cordStatus = ComboSourceUtil.Instance.FindEnum(rbm.ordStatus, "ordstatus");
                        string cupdatetime = string.Empty;
                        if (!string.IsNullOrEmpty(rbm.updatetime))
                            cupdatetime = CommonUtils.ConvertToDateTime(rbm.updatetime).ToString("HH:mm:ss");

                        writer.WriteLine($"\"{cside}\",\"{rbm.securityID}\",\"{rbm.symbol}\",\"{cSettlType}\",{cLastPx},{rbm.orderQty},{CommonUtils.FormatPriceZeroEqualsBar(rbm.cumQty.ToString())},\"{cVenues_exe}\",\"{cordStatus}\",\"{rbm.deliverToSubID}\",\"{rbm.orderID}\",\"{rbm.comment}\",\"{cupdatetime}\",\"{rbm.execID}\"");
                    }
                    // 释放资源
                    writer.Flush();
                    writer.Close();
                }

                showSuccessMessage($"需求意向导出成功!\r\n{fileName}");

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                CustomMessageBox.Error($"需求意向导出失败!\r\n{e.Message}", "提示", null);
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

        /// <summary>
        /// 打开历史查询
        /// </summary>
        private void OnHistoryCommandClick()
        {
            //QtrsHandler.Instance.Init();
        }
        #endregion


        #region 复制
        public string CopyRowText(IList SelectedItemsList)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (QtrsOrderData rbm in SelectedItemsList)
                {
                    sb.Append(rbm.securityID+" ");
                    sb.Append(CommonUtils.FormatPrice(rbm.lastPx.ToString()) + " ");
                    sb.Append(ComboSourceUtil.Instance.FindEnum(rbm.side.ToString(), "side")+ " ");
                    if (string.IsNullOrEmpty(rbm.comment))
                    {
                        sb.Append(CommonUtils.FormatPriceZeroEqualsBar(rbm.orderQty.ToString()) + "w ");
                        sb.Append($"({rbm.orderID})");
                    }
                    else
                    {
                        sb.Append(rbm.orderQty.ToString() + "w ");
                        sb.Append($"({rbm.orderID}) ");
                        sb.Append($"<{rbm.comment}>");
                    }
                    sb.Append("\n");

                    //string cVenues_exe = ComboSourceUtil.Instance.FindEnum(rbm.venues, "venues_exe");
                    //string cSettlType = ComboSourceUtil.Instance.FindEnum(rbm.settlType, "settletype_myOrder");
                    //string cordStatus = ComboSourceUtil.Instance.FindEnum(rbm.ordStatus, "ordstatus");
                }
            }

            return sb.ToString();
        }
        #endregion

        #region ShowMessage
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
    }
}
