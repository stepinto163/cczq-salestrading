using com.gateway.log;
using STC.Common.Model;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
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
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.Handler.QTRS;
using STC.Common.Model.QTR;
using STC.Common.View.Component;

namespace BDB.Trade.Plugin.ViewModel.BrokerKanban
{
    /// <summary>
    /// 已完成ViewModel
    /// </summary>
    public class QtrsTradeViewModel : BaseViewModel
    {
        #region 已完成构造
        private object lockObj = new object();
        /// <summary>
        /// 已完成构造
        /// </summary>
        private ObservableCollection<QtrsTradeData> fullddo = new ObservableCollection<QtrsTradeData>();
        public ObservableCollection<QtrsTradeData> FullDDO
        {
            get
            {
                return fullddo;
            }
            set
            {
                fullddo = value;
                this.OnPropertyChanged(nameof(FullDDO));
            }
        }



        private QtrsTradeData selectedDDO = null;
        /// <summary>
        /// 单击选择的已完成
        /// </summary>
        public QtrsTradeData SelectedDDO
        {
            get
            {
                return selectedDDO;
            }

            set
            {
                selectedDDO = value;
                this.OnPropertyChanged(nameof(selectedDDO));

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (selectedDDO != null)
                    {
                        BondChangeHandler.Instance.OnBondChange(BondUpdateType.ChangeFocusRow, null, null, selectedDDO.securityID, "全", true);
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



        private QtrsTradeView historypositionview;
        public QtrsTradeView HistoryView
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

        bool IsInit = false;

        #endregion

        #region Command 构造
        public ICommand FullScreenCommand { get { return new RelayCommand(OnFullScreenCommandClick); } }
        public ICommand ExportCsvCommand { get { return new RelayCommand(OnExportCsvCommandClick); } }
        public ICommand HistoryCommand { get { return new RelayCommand(OnHistoryCommandClick); } }
        #endregion

        public QtrsTradeViewModel()
        {
            //初始化视图
            //InitView();
        }

        ~QtrsTradeViewModel()
        {
            GC.Collect(0);
        }

        /// <summary>
        /// 订阅经纪商需求意向
        /// </summary>
        private void SubscribeQtrTrade()
        {
           BaseHandler.Instance.SubscribeLoadDataStatus(OnLoadDataStatus);
           QtrsHandler.Instance.SubscribeQtrTrade(OnQtrTradeCallBack);
        }

        private bool isLoadBond = false;
        private void OnLoadDataStatus(bool obj)
        {
            if (obj)
            {
                isLoadBond = true;
                lock (lockObj)
                {
                    foreach (QtrsTradeData pbm in FullDDO)
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
        private void OnQtrTradeCallBack(List<QtrTrade> riskIndicator)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(K =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (QtrTrade pbm in riskIndicator)
                    {
                        lock (lockObj)
                        {
                            QtrsTradeData m2 = FullDDO.FirstOrDefault(o => o.dealID == pbm.dealID);
                            if (m2 != null)
                            {
                                m2.lastPx = pbm.price;
                                m2.cleanPrice = pbm.cleanPrice;

                                m2.side = pbm.direction == "given" ? "GVN" : (pbm.direction == "taken" ? "TKN" : "TRD");
                                m2.ordStatus = pbm.method == 1 ? "新增" : (pbm.method == 2 ? "修改" : "撤销");

                                m2.settlType = pbm.settlementType == "+1" ? "2" : "1";
                                m2.orderQty = pbm.volume;
                                m2.comment = pbm.comment;
                                m2.venues = ComboSourceUtil.Instance.FindEnum(pbm.companyID.ToString(), "venues_en");
                                m2.cleanPrice = pbm.cleanPrice;
                                m2.tradeDate = pbm.tradedDate;
                                m2.transactTime = pbm.dealTime;
                                m2.updatetime = pbm.sendingTime;
                                m2.comment = pbm.comment;

                                m2.orderID = pbm.orderID;
                            }
                            else
                            {
                                QtrsTradeData tmpqt = new QtrsTradeData();
                                tmpqt.orderID = pbm.orderID;
                                tmpqt.dealID = pbm.dealID;
  
                                tmpqt.symbol = pbm.bondShortName;
                                tmpqt.securityID = pbm.bondCode;
                                tmpqt.securityType = "null";

                                if (BaseHandler.Instance.IsLoad())
                                {
                                    BondsInfoPo result = CommonUtils.GetBondsInfoPoBySecurityID(pbm.bondCode);
                                    if (result != null)
                                    {
                                        tmpqt.securityType = result.SECURITY_TYPE;
                                        tmpqt.symbol = result.Short_Name;
                                    }
                                }


                                tmpqt.lastPx = pbm.price;
                                tmpqt.cleanPrice = pbm.cleanPrice;

                                tmpqt.side = pbm.direction == "given" ? "GVN" : (pbm.direction == "taken" ? "TKN" : "TRD");
                                tmpqt.ordStatus = pbm.method == 1 ? "新增" : (pbm.method == 2 ? "修改" : "撤销");

                                tmpqt.settlType = pbm.settlementType == "+1" ? "2" : "1";
                                tmpqt.orderQty = pbm.volume;
                                
                                tmpqt.deliverToSubID = pbm.direction == "given"? pbm.bidTraderName : pbm.ofrTraderName;

                                tmpqt.comment = pbm.comment;
                                tmpqt.venues = ComboSourceUtil.Instance.FindEnum(pbm.companyID.ToString(), "venues_en");
                                tmpqt.cleanPrice = pbm.cleanPrice;
                                tmpqt.tradeDate = pbm.tradedDate;
                                tmpqt.transactTime = pbm.dealTime;
                                tmpqt.updatetime = pbm.sendingTime;
                                tmpqt.comment = pbm.comment;
                                // gvn 取 ofr tkn 取bid


                             
                                FullDDO.Add(tmpqt);
                            }
                        }
                    }

                    //更新已完成
                    ReCalcDealDoneOrder();
                }));
            });
        }


        /// <summary>
        /// 更新经纪商需求意向
        /// </summary>
        public void ReCalcDealDoneOrder()
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
            QtrsHandler.Instance.UnSubscribeQtrTrade(OnQtrTradeCallBack);
            base.Dispose();
        }


        /// <summary>
        /// 初始化视图
        /// </summary>
        public void InitView()
        {
            //默认未完成
            LstOrdStatus.Clear();
            LstOrdStatus.Add("0");
            LstOrdStatus.Add("1");
            RowCount = 0;
            FullDDO.Clear();
            this.Mv.Source = FullDDO;
            this.Mv.IsLiveFilteringRequested = true;
            this.Mv.IsLiveSortingRequested = true;
            this.Mv.Filter += Mv_Filter;
            this.Mv.View.Refresh();
            this.Mv.SortDescriptions.Add(new SortDescription("updatetime", ListSortDirection.Descending));

            if (!IsInit)
            {
                //订阅经纪商需求意向
                SubscribeQtrTrade();
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

            QtrsTradeData pbd = e.Item as QtrsTradeData;
            if (string.IsNullOrEmpty(this.SecurityId))
            {
                if (LstSecurityType.Count == 0 && LstVenues.Count == 0)
                {
                    e.Accepted = true;
                }
                else
                {
                    if (LstSecurityType.Contains(pbd.securityType) && LstVenues.Contains(pbd.venues))
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
                    if (pbd.securityID == this.SecurityId)
                        e.Accepted = true;
                    else
                        e.Accepted = false;
                }
                else
                {
                    if (LstSecurityType.Contains(pbd.securityType) && LstVenues.Contains(pbd.venues) && (pbd.securityID == this.SecurityId))
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
                QtrsTradeView chrom = new QtrsTradeView(false);
                chrom.DataContext = this;
                myTradingPopupWin.SetMainControl(chrom);
                myTradingPopupWin.SetTitleAndKey("QtrsTradeViewMaximized", "已确认");
                myTradingPopupWin.SetWebPageTitle("已确认");
                myTradingPopupWin.Owner = null;
                WindowManager.Instance.OpenOnlyOneWindow($"QtrsTradeViewMaximized", myTradingPopupWin, null);
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
                saveFileDialog.FileName = $"已确认_{DateTimeEx.ServerTime.ToString("yyyyMMdd")}";
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
                    writer.WriteLine($"方向,代码,简称,状态,收益率（%）,交易日期, 清算速度,数量（万元）,对手方,对手交易员,本方交易员,订单序号,经纪商,备注,净价,成交时间,成交单号");
                    //ObservableCollection<HistoryOrderData> FullPositionOrderDetail
                    foreach (QtrsTradeData rbm in FullDDO)
                    {
                        //string cside = ComboSourceUtil.Instance.FindEnum(rbm.side.ToString(), "side");
                        string cside = rbm.side.ToString();
                        string cLastPx = CommonUtils.FormatPrice(rbm.lastPx.ToString());
                        //string cNetPrice = CommonUtils.FormatPrice(rbm.NetPrice);
                        string cVenues_exe = ComboSourceUtil.Instance.FindEnum(rbm.venues, "venues_exe");
                        string cSettlType = ComboSourceUtil.Instance.FindEnum(rbm.settlType, "settletype_myOrder");
                        //string cordStatus = ComboSourceUtil.Instance.FindEnum(rbm.ordStatus, "ordstatus");
                        string cordStatus = rbm.ordStatus;
                        string ctransactTime = string.Empty;
                        string ctradeDate = string.Empty;
                        if (!string.IsNullOrEmpty(rbm.transactTime))
                            ctransactTime = CommonUtils.ConvertToDateTime(rbm.transactTime).ToString("HH:mm:ss");
                        if (!string.IsNullOrEmpty(rbm.tradeDate))
                            ctradeDate = CommonUtils.ConvertToDateTime(rbm.tradeDate).ToString("yyyy-MM-dd");
                        writer.WriteLine($"\"{cside}\",\"{rbm.securityID}\",\"{rbm.symbol}\",\"{cordStatus}\",{cLastPx},\"{ctradeDate}\",\"{cSettlType}\",{CommonUtils.FormatPriceZeroEqualsBar(rbm.orderQty.ToString())},\"\",\"\",\"{rbm.deliverToSubID}\",\"{rbm.orderID}\",\"{cVenues_exe}\",\"{rbm.comment}\",{rbm.cleanPrice},\"{ctransactTime}\",\"{rbm.execID}\"");
                    }
                    // 释放资源
                    writer.Flush();
                    writer.Close();
                }

                showSuccessMessage($"已确认导出成功!\r\n{fileName}");

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                CustomMessageBox.Error($"已确认导出失败!\r\n{e.Message}", "提示", null);
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
            //QtrsHandler.Instance.Test();
            //BaseHandler.Instance.Reset();
        }
        #endregion


        #region 复制
        public string CopyRowText(IList SelectedItemsList)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (QtrsTradeData rbm in SelectedItemsList)
                {
                    sb.Append(rbm.securityID+" ");
                    sb.Append(CommonUtils.FormatPrice(rbm.lastPx.ToString()) + " ");
                    sb.Append(CommonUtils.FormatPriceZeroEqualsBar(rbm.orderQty.ToString()) + "w ");
                    sb.Append(rbm.side.ToString() == "1" ? "买入,已确定" : "卖出,已确定");
                    string cordStatus = ComboSourceUtil.Instance.FindEnum(rbm.ordStatus, "ordstatus");
                    sb.Append(cordStatus);
                    sb.Append("\n");
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
