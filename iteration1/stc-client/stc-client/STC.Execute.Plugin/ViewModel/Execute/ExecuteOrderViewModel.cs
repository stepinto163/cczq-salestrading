using com.gateway.log;
using Microsoft.Win32;
using STC.Common.Handler.Ptr;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Execute;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Execute.Plugin.Handler;
using STC.Execute.Plugin.Handler.Process;
using STC.Execute.Plugin.Utils;
using STC.Execute.Plugin.View.Component;
using STC.Common.View.UI;
using System;
using System.Collections;
using System.Collections.Generic;
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
using ComboSourceUtil = STC.Execute.Plugin.Utils.ComboSourceUtil;

namespace STC.Execute.Plugin.ViewModel.Execute
{
    public class ExecuteOrderViewModel : BaseViewModel
    {
        private FrameworkElement _ownUI;

        private DataView _exeOrderDV;
        private Thread _RefreshDefaultSelectThread = null;

        public ExecuteOrderViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(ExeOrderDV, ExecOrderProcesser.Instance.LockObj);
        }

        /// <summary>
        /// 清零回调方法
        /// </summary>
        /// <param name="obj"></param>
        public void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("ExecuteOrderViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Clear();
            }
        }

        /// <summary>
        /// 我的订单数据源
        /// </summary>
        public DataView ExeOrderDV
        {
            get
            {
                lock (ExecOrderProcesser.Instance.LockObj)
                {
                    if (_exeOrderDV == null)
                    {
                        _exeOrderDV = new DataView(ExecOrderProcesser.Instance.ExeOrdersDT);
                        _exeOrderDV.Sort = $"{ExecOrderProcesser.CreateTime_Sort} desc";
                        this.OnPropertyChanged();
                    }
                    return this._exeOrderDV;
                }
            }
        }

        private int selectedConditionIndex = 0;
        public int SelectedConditionIndex
        {
            get { return selectedConditionIndex; }
            set { selectedConditionIndex = value; this.OnPropertyChanged(); }
        }

        private string searchText;
        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;

                this.OnPropertyChanged();
            }
        }

        public ICommand ExportCommand { get { return new RelayCommand(OnExportClick); } }


        public void Init(FrameworkElement ownUI)
        {
            _ownUI = ownUI;



            if (_RefreshDefaultSelectThread == null)
            {
                _RefreshDefaultSelectThread = new Thread(RefreshDefaultSelect);
                _RefreshDefaultSelectThread.IsBackground = true;
                _RefreshDefaultSelectThread.Start();
            }
        }

        private void RefreshDefaultSelect()
        {
            while (true)
            {
                try
                {
                    SetDefaultSelected();

                    if (_isSelected)
                        break;

                    Thread.Sleep(500);
                }
                catch (ThreadAbortException ex)
                {

                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }
        }

        private bool _isSelected = false;
        private void SetDefaultSelected()
        {
            if (!_isSelected && ExeOrderDV.Table.Rows.Count > 0)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    object obj = _ownUI.FindName("Executions_dataGrid");
                    if (obj != null)
                    {
                        CustomExecDataGrid dg = obj as CustomExecDataGrid;

                        if (dg != null)
                        {
                            dg.SelectedIndex = 0;

                            _isSelected = true;
                        }

                    }
                }));
            }
        }


        public void Clear()
        {
            ExecuterOrderHandler.Instance.UnSubscribe();
            ExecuterOrderHandler.Instance.Clear();

            if (_RefreshDefaultSelectThread != null)
            {
                LogHelper.Info("ExecuteOrderViewModel Dispose RefreshDefaultSelectThread:");
                _RefreshDefaultSelectThread.Abort();
                _RefreshDefaultSelectThread = null;
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                lock (ExecOrderProcesser.Instance.LockObj)
                {
                    if (_exeOrderDV != null)
                    {
                        _exeOrderDV.Table.Rows.Clear();
                        // _exeOrderDV = null;
                    }
                }
            }));
        }


        #region 导出
        private DataTable ExportDV;
        private string fileName = "";
        AutoResetEvent autoEvent = new AutoResetEvent(false);
        private void OnExportClick()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "(文本文件*.csv)|*.csv";
            saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_成交确认记录";
            saveFileDialog.AddExtension = true;
            saveFileDialog.RestoreDirectory = true;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                fileName = saveFileDialog.FileName;
                ExportDV = this.ExeOrderDV.Table.Clone();
                ThreadPool.QueueUserWorkItem(new WaitCallback(RequestExportMethod), autoEvent);
            }
        }


        private void RequestExportMethod(object state)
        {
            lock (ExportDV.Rows.SyncRoot)
            {
                RequestExportOrders();
            }
            autoEvent.Set();
        }
        public void RequestExportOrders()
        {
            StreamWriter sw = null;
            try
            {
                LogHelper.Info("fileName:" + fileName);

                bool isOk = ExcelHelper.Instance.TableToCsv(ExeOrderDV.ToTable(), fileName, "");

                //UpdateExportOrders(obj);                                    
                //ExportPo po = toExportOrder(obj);
                //status = ExcelHelper.Instance.SaveDataToCSVFile(po, fileName, sw, count, Constants.HISOrders);

                //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    if (count == maxCount)
                //    {
                //        showMessage("导出成功");
                //    }
                //    else
                //    {
                //        showMessage("导出失败");
                //    }
                //}));
                if (isOk)
                {

                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        showMessage("导出失败");
                    }));
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("RequestExportOrders", e);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    showMessage("导出失败");
                }));
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
        }

        private void showMessage(string message)
        {
            MessageBoxGUI me = new MessageBoxGUI();
            me.setWarnMsg(message);
            me.ShowDialog();
        }

        #endregion

        private void filterDataByKeyword()
        {

        }

        public string copyRowData(IList SelectedItemsList)
        {
            StringBuilder plainTxt = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (DataRowView drv in SelectedItemsList)
                {

                    string side = drv[ExecOrderProcesser.Side] + "";
                    string securityId = drv[ExecOrderProcesser.SecurityID] + "";
                    string symbolName = drv[ExecOrderProcesser.Symbol] + "";


                    string qty = drv[ExecOrderProcesser.AllocQty] + "w";
                    //string partySubName = Constants.SHANXIZHENGQUAN;//drv[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string;
                    string settle = ComboSourceUtil.Instance.FindEnum(drv[ExecOrderProcesser.SettlType] + "", "settletype_execute");
                    string trade = drv[ExecOrderProcesser.TradeDate] + "";
                    settle = ExecOrderProcesser.Instance.getSettleType(trade, settle);

                    string tradeType = ComboSourceUtil.Instance.FindEnum(drv[ExecOrderProcesser.TrdType] + "", "trdType");
                    string color = side == "2" ? "#58B3FF" : "#FB943C";
                    string partyName = drv[ExecOrderProcesser.PartyName] + "";
                    string partySubName = drv[ExecOrderProcesser.PartySubName] + "";
                    string counterPartyName = drv[ExecOrderProcesser.CounterPartyName] + "";
                    string counterPartySubName = drv[ExecOrderProcesser.CounterPartySubName] + "";
                    string paymentFixedRate = drv[ExecOrderProcesser.PaymentFixedRate] + "";
                    if (!string.IsNullOrEmpty(paymentFixedRate))
                        paymentFixedRate = string.Format("F{0}", paymentFixedRate);
                    string transType = drv[ExecOrderProcesser.TransactionPriceType] + "";
                    string px = "";
                    if (!string.IsNullOrEmpty(transType))
                    {
                        switch (transType)
                        {
                            case "到期":
                                px = string.Format("{0:N4}", drv[ExecOrderProcesser.AvgPx]) + "%";
                                break;
                            case "行权":
                                px = string.Format("{0:N4}", drv[ExecOrderProcesser.StipulationValue]) + "%";
                                break;
                            case "净价":
                                px = drv[ExecOrderProcesser.CleanPrice] + "";
                                break;
                            case "全价":
                                px = drv[ExecOrderProcesser.DirtyPrice] + "";
                                break;
                        }
                    }
                    string endTxt = string.Empty;
                    if (side == "1")
                    {
                        endTxt = $"{counterPartyName}{counterPartySubName}\t出给\t{partyName}{partySubName}";
                    }
                    else
                        endTxt = $"{partyName}{partySubName}\t出给\t{counterPartyName}{counterPartySubName}";

                    if (plainTxt.Length > 0)
                    {
                        plainTxt.Append("\n\r");
                    }


                    plainTxt.Append($"{securityId}\t{symbolName}\t{px}\t{transType}\t{qty}\t{paymentFixedRate}\t{settle}\t{tradeType}\t{endTxt}");

                }
            }
            return plainTxt.ToString();
        }


        public void FilterData(Dictionary<string, IList> filterDic)
        {
            StringBuilder filter_orders = new StringBuilder();
            // 人工，IDB， 量化
            IList statueList = filterDic["Statue"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (statueList != null && statueList.Count > 0)
            {
                foreach (ListBoxItem item in statueList)
                {
                    if (item.Content.ToString() == Constants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item.Content.ToString() == Constants.WaitConfirm)
                    {
                        inFilterSB_venues.Append("'0'");
                    }
                    else if (item.Content.ToString() == Constants.Confirmed)
                    {
                        inFilterSB_venues.Append("'1'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_orders.Append($"{ExecOrderProcesser.TradeStat} in ({inFilterSB_venues})");
            }

            // Broker
            IList brokerlist = filterDic["Broker"];

            StringBuilder inFilterSB_Broker = new StringBuilder();
            if (brokerlist != null && brokerlist.Count > 0)
            {
                foreach (ListBoxItem item in brokerlist)
                {
                    if (item.Name == "All")
                    {
                        inFilterSB_Broker.Clear();
                        break;
                    }

                    if (inFilterSB_Broker.Length > 0)
                    {
                        inFilterSB_Broker.Append(",");
                    }

                    if (item.Content.ToString() == Constants.TBook)
                    {
                        inFilterSB_Broker.Append("'MANUAL','QUANT','IDB'");
                    }
                    else if (item.Content.ToString() == Constants.XBond)
                    {
                        inFilterSB_Broker.Append("'ODM'");
                    }
                    else if (item.Content.ToString() == Constants.AllBroker && item.Name == "AllBroker")
                    {
                        inFilterSB_Broker.Append("'CNEX','TPSC','CTTJ','PTCN','CCTB','UEDA'");
                    }
                    else if (item.Content.ToString() == Constants.CNEX)
                    {
                        inFilterSB_Broker.Append("'CNEX'");
                    }
                    else if (item.Content.ToString() == Constants.TPSC)
                    {
                        inFilterSB_Broker.Append("'TPSC'");
                    }
                    else if (item.Content.ToString() == Constants.CTTJ)
                    {
                        inFilterSB_Broker.Append("'CTTJ'");
                    }
                    else if (item.Content.ToString() == Constants.PTCN)
                    {
                        inFilterSB_Broker.Append("'PTCN'");
                    }
                    else if (item.Content.ToString() == Constants.CCTB)
                    {
                        inFilterSB_Broker.Append("'CCTB'");
                    }
                    else if (item.Content.ToString() == Constants.UEDA)
                    {
                        inFilterSB_Broker.Append("'UEDA'");
                    }
                    else if (item.Content.ToString() == Constants.QDMESP)
                    {
                        inFilterSB_Broker.Append("'MM'");
                    }
                }
            }

            if (inFilterSB_Broker.Length > 0)
            {
                if (filter_orders.Length > 0)
                {
                    filter_orders.Append(" and ");
                }

                filter_orders.Append($"{ExecOrderProcesser.OnBehalfOfCompID} in ({inFilterSB_Broker})");
            }

            if (filterDic.ContainsKey("Code"))
            {
                IList codeList = filterDic["Code"];
                if (codeList != null && codeList.Count > 0)
                {
                    if (filter_orders.Length > 0)
                    {
                        filter_orders.Append(" and ");
                    }

                    filter_orders.Append($"{ExecOrderProcesser.SecurityID} like '%{codeList[0]}%'");
                }
            }

            if (filterDic.ContainsKey("CounterParty"))
            {
                IList counterPartyList = filterDic["CounterParty"];
                if (counterPartyList != null && counterPartyList.Count > 0)
                {
                    if (filter_orders.Length > 0)
                    {
                        filter_orders.Append(" and ");
                    }

                    filter_orders.Append($"{ExecOrderProcesser.CounterPartyName} like '%{counterPartyList[0]}%'");
                }
            }
            if (filterDic.ContainsKey("UserName"))
            {
                IList PartyList = filterDic["UserName"];
                if (PartyList != null && PartyList.Count > 0)
                {
                    if (filter_orders.Length > 0)
                    {
                        filter_orders.Append(" and ");
                    }
                    filter_orders.Append($"{ExecOrderProcesser.PartySubName} like '%{PartyList[0]}%'");
                }
            }

            // 利率，信用， NCD
            IList SecurityTypeList = filterDic["SecurityType"];
            StringBuilder inFilterST = new StringBuilder();
            if (SecurityTypeList != null && SecurityTypeList.Count > 0)
            {
                foreach (ListBoxItem item in SecurityTypeList)
                {
                    if (item.Name == Constants.ALLST)
                    {
                        inFilterST.Clear();
                        break;
                    }
                    if (inFilterST.Length > 0)
                    {
                        inFilterST.Append(",");
                    }
                    if (item.Content.ToString() == Constants.CREDIT)
                    {
                        inFilterST.Append("'1'");
                    }
                    else if (item.Content.ToString() == Constants.RATE)
                    {
                        inFilterST.Append("'2'");
                    }
                    else if (item.Content.ToString() == Constants.NCD)
                    {
                        inFilterST.Append("'3'");
                    }
                }
            }
            if (inFilterST.Length > 0)
            {
                if (filter_orders.Length > 0)
                {
                    filter_orders.Append(" and ");
                }
                filter_orders.Append($"{ExecOrderProcesser.SecurityType} in ({inFilterST})");
            }

            lock (ExecOrderProcesser.Instance.LockObj)
            {
                this.ExeOrderDV.RowFilter = filter_orders.ToString();
            }

        }

    }
}
