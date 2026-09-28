using com.gateway.log;
using STC.Common.DataProcesser;
using STC.Common.Model;
using STC.Common.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace STC.Common.ViewModel.MyTrading
{
    public class MyExesViewModel : ViewModelBase
    {
        

        private DataView _MyExecutionsDV;

        /// <summary>
        /// 我的成交数据源
        /// </summary>
        public DataView MyExecutionsDV
        {
            get
            {
                lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
                {
                    if (_MyExecutionsDV == null)
                    {
                        _MyExecutionsDV = new DataView(MyExecutionProcesser.Processer.MyExecutionsDT);
                        _MyExecutionsDV.Sort = $"{MyExecutionProcesser.EXECUTIONTIME_FIELD} desc";
                        this.OnPropertyChanged(nameof(MyExecutionsDV));
                    }
                    return this._MyExecutionsDV;
                }
            }
        }

        private DataRowView _selectedItem_MyExecutions;

        /// <summary>
        /// 我的成交选中行记录
        /// </summary>
        public DataRowView SelectedItem_MyExecutions
        {
            get
            {
                return _selectedItem_MyExecutions;
            }

            set
            {
                _selectedItem_MyExecutions = value;
                this.OnPropertyChanged();
            }
        }

        public static List<KeyValuePair<string, string>> SideItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Side");
            }
        }

        public static List<KeyValuePair<string, string>> SettleTypeItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("SettleType");
            }
        }

        public static List<KeyValuePair<string, string>> VenuesItemSource
        {
            get
            {
                return ComboSourceUtil.Instance.GetComboSource("Venues");
            }
        }

        /// <summary>
        /// 我的成交Tab标题
        /// </summary>
        private string _TotalCountExecutionsHeader = "我的成交";
        public string TotalCountExecutionsHeader
        {
            get
            {
                return _TotalCountExecutionsHeader;
            }
            set
            {
                _TotalCountExecutionsHeader = value;
                this.OnPropertyChanged(nameof(TotalCountExecutionsHeader));
            }
        }

        /// <summary>
        /// 我的成交是否显示小红点 +0 显示，""不显示
        /// </summary>
        private string isMyExecutionsShowRedDot = "";
        public string IsMyExecutionsShowRedDot
        {
            get
            {
                return isMyExecutionsShowRedDot;
            }
            set
            {
                isMyExecutionsShowRedDot = value;
                this.OnPropertyChanged(nameof(IsMyExecutionsShowRedDot));
            }
        }

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
                    FilterExeData();
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public MyExesViewModel()
        {
            try
            {
                // 我的成交
                BindingOperations.EnableCollectionSynchronization(MyExecutionsDV, MyExecutionProcesser.Processer.LockObj_MyExecutions);
                
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        

        ~MyExesViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            // 释放我的成交数据
            lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
            {
                if (_MyExecutionsDV != null)
                {
                    _MyExecutionsDV.Dispose();
                    _MyExecutionsDV = null;
                }
            }
            base.Dispose();
        }

        StringBuilder filter_executions = new StringBuilder();
        /// <summary>
        /// 处理我的成交的过滤条件
        /// </summary>
        /// <param name="filterDic"></param>
        public void ProcessMyExecutionsFilterConditon(Dictionary<string, IList> filterDic)
        {
            filter_executions.Remove(0, filter_executions.Length);
            // venues
            IList venuesList = filterDic["Venues"];
            StringBuilder inFilterSB_venues = new StringBuilder();
            if (venuesList.Count > 0)
            {
                foreach (ListBoxItem item in venuesList)
                {
                    if (item.Name == "All")
                    {
                        inFilterSB_venues.Remove(0, inFilterSB_venues.Length);
                        break;
                    }

                    if (inFilterSB_venues.Length > 0)
                    {
                        inFilterSB_venues.Append(",");
                    }

                    if (item.Content.ToString() == CommonConstants.TBook)
                    {
                        inFilterSB_venues.Append("'MANUAL','QUANT','IDB'");
                    }
                    else if (item.Content.ToString() == CommonConstants.XBond)
                    {
                        inFilterSB_venues.Append("'ODM'");
                    }
                    else if (item.Content.ToString() == CommonConstants.AllBroker && item.Name == "AllBroker")
                    {
                        inFilterSB_venues.Append("'CNEX','TPSC','CTTJ','PTCN','CCTB','UEDA'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CNEX)
                    {
                        inFilterSB_venues.Append("'CNEX'");
                    }
                    else if (item.Content.ToString() == CommonConstants.TPSC)
                    {
                        inFilterSB_venues.Append("'TPSC'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CTTJ)
                    {
                        inFilterSB_venues.Append("'CTTJ'");
                    }
                    else if (item.Content.ToString() == CommonConstants.PTCN)
                    {
                        inFilterSB_venues.Append("'PTCN'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CCTB)
                    {
                        inFilterSB_venues.Append("'CCTB'");
                    }
                    else if (item.Content.ToString() == CommonConstants.UEDA)
                    {
                        inFilterSB_venues.Append("'UEDA'");
                    }
                    else if (item.Content.ToString().Equals("双边"))
                    {
                        inFilterSB_venues.Append("'QDMESP'");
                    }
                }
            }

            if (inFilterSB_venues.Length > 0)
            {
                filter_executions.Append($"{MyExecutionProcesser.VENUES_FIELD} in ({inFilterSB_venues})");
            }

            // syncStatus
            IList syncStatusList = filterDic["SyncStatus"];
            StringBuilder inFilterSB_syncStatus = new StringBuilder();
            if (syncStatusList.Count > 0)
            {
                foreach (ListBoxItem item in syncStatusList)
                {
                    if (item.Content.ToString() == CommonConstants.ALL)
                    {
                        continue;
                    }

                    if (inFilterSB_syncStatus.Length > 0)
                    {
                        inFilterSB_syncStatus.Append(" or ");
                    }

                    if (item.Content.ToString() == CommonConstants.SEND)
                    {
                        inFilterSB_syncStatus.Append($"{ MyExecutionProcesser.SYNC_STATUS_FIELD} = 'Y'");
                    }
                    else if (item.Content.ToString() == CommonConstants.SOLVE)
                    {
                        inFilterSB_syncStatus.Append($"{MyExecutionProcesser.SYNC_STATUS_FIELD} = 'S'");
                    }
                }
            }

            if (inFilterSB_syncStatus.Length > 0)
            {
                if (filter_executions.Length > 0)
                {
                    filter_executions.Append(" and ");
                }
                filter_executions.Append($"({inFilterSB_syncStatus})");
            }

            // 利率/信用
            IList codelist = filterDic["CodeType"];
            StringBuilder inFilterSB_Code = new StringBuilder();
            if (codelist.Count > 0)
            {
                foreach (ListBoxItem item in codelist)
                {
                    if (item.Content.ToString() == CommonConstants.AllBroker)
                    {
                        continue;
                    }

                    if (inFilterSB_Code.Length > 0)
                    {
                        inFilterSB_Code.Append(",");
                    }

                    if (item.Content.ToString() == CommonConstants.RATE)
                    {
                        inFilterSB_Code.Append("'BNC'");
                    }
                    else if (item.Content.ToString() == CommonConstants.CREDIT)
                    {
                        inFilterSB_Code.Append("'BCO'");
                    }
                }
            }

            if (inFilterSB_Code.Length > 0)
            {
                if (filter_executions.Length > 0)
                {
                    filter_executions.Append(" and ");
                }

                filter_executions.Append($"{MyOrderProcesser.SECURITY_TYPE} in ({inFilterSB_Code})");
            }

            FilterExeData();
        }

        /// <summary>
        /// 过滤我的成交数据
        /// </summary>
        private void FilterExeData()
        {
            StringBuilder tempFilter = new StringBuilder();
            if (this.SearchText.Length > 0)
            {
                tempFilter.Append($"({MyOrderProcesser.SECURITYID_ORDER_FIELD} like '%{this.SearchText}%' or {MyOrderProcesser.SYMBOL_ORDER_FIELD} like '%{this.SearchText}%')");
            }

            if (filter_executions.Length > 0)
            {
                if (tempFilter.Length > 0)
                {
                    tempFilter.Append(" and ");
                }

                tempFilter.Append(filter_executions);
            }

            lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
            {
                LogHandler.Info($"----------this.MyExecutionsDV.RowFilter-------{tempFilter}");
                this.MyExecutionsDV.RowFilter = tempFilter.ToString();
            }
        }

        public string CopySTCCodeText(IList SelectedItemsList, string tabName)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (DataRowView dgr in SelectedItemsList)
                {
                    if (tabName.Equals("Orders"))
                    {
                        string stcCodeText = dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        string stcCodeText = dgr[MyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string;
                        sb.Append(stcCodeText + "\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        string stcCodeText = dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string;
                        sb.Append(stcCodeText + "\n");
                    }
                }
            }
            return sb.ToString();
        }

        public string CopyRowText(IList SelectedItemsList, string tabName)
        {
            StringBuilder sb = new StringBuilder();
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (DataRowView dgr in SelectedItemsList)
                {
                    if (tabName.Equals("Orders"))
                    {
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SIDE_ORDER_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[MyOrderProcesser.SECURITYID_ORDER_FIELD] as string + "\t");
                        sb.Append(dgr[MyOrderProcesser.SYMBOL_ORDER_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string , "settletype") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.ORDERSTATUS_ORDER_FIELD] as string, "ordstatus") + "\t");
                        //sb.Append(dgr[MyOrderProcesser.ORDERTYPE_FIELD] as string + "\t");
                        sb.Append(string.Format("{0:N4}", dgr[MyOrderProcesser.PRICE_ORDER_FIELD]) + "%\t");
                        //sb.Append(this.Covert(dgr[MyOrderProcesser.WHOLE_FLAG_FIELD] , "wholeflag") + "\t");
                        sb.Append(dgr[MyOrderProcesser.QTY_ORDER_FIELD] + "w\t");
                        //sb.Append(this.NumCovert(dgr[MyOrderProcesser.EXECQTY_ORDER_FIELD]) + "\t");
                        //sb.Append(Constants.SHANXIZHENGQUAN + "\t");//dgr[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        //sb.Append(dgr[MyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        //sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[MyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");

                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype_myorder"));

                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Deals"))
                    {
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SIDE_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[MyExecutionProcesser.SECURITYID_FIELD] as string + "\t");
                        sb.Append(dgr[MyExecutionProcesser.SYMBOL_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SYNC_STATUS_FIELD] as string, "synchflag") + "\t");
                        sb.Append(string.Format("{0:N4}%", dgr[MyExecutionProcesser.DISPLAY_LASTPX_FIELD]) + "\t");
                        //sb.Append(this.NumCovert(dgr[MyExecutionProcesser.NETPRICE_FIELD]) + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SETTLTYPE_FIELD] as string, "settletype") + "\t");
                        sb.Append(dgr[MyExecutionProcesser.LASTQTY_FIELD] + "w\t");

                        string side = dgr[MyExecutionProcesser.SIDE_FIELD] as string;// ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SIDE_FIELD] as string, "side");

                        if (side.Equals("1"))
                        {
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\tto\t");
                            //sb.Append(Constants.SHANXIZHENGQUAN + "\t");
                            //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.PARTYNAME_FIELD]) + "\t");
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.INSTITUTIONNAME_FIELD]) + "\t");
                        }
                        else
                        {
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.INSTITUTIONNAME_FIELD]) + "\tto\t");
                            //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.PARTYNAME_FIELD]) + "\tto\t");
                            //sb.Append(Constants.SHANXIZHENGQUAN + "\tto\t");
                            sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]) + "\t");
                        }

                        //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.COUNTERPARTYSUBNAME_FIELD]) + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.EXECID_FIELD] as string + "\t");
                        //sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.VENUES_FIELD] as string, "venues_exe") + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.PARTYSUBNAME_FIELD] as string + "\t");
                        //sb.Append(TimeUtils.Instance.ToTime(dgr[MyExecutionProcesser.EXECUTIONTIME_FIELD] as string) + "\t");
                        //sb.Append(dgr[MyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string + "\t");
                        //sb.Append(this.StringConvert(dgr[MyExecutionProcesser.REFORDERID_FIELD]) + "\t");     
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyExecutionProcesser.SETTLTYPE_FIELD] as string, "settletype") + "\t");
                        sb.Append("\n");
                    }
                    else if (tabName.Equals("Cancels"))
                    {
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SIDE_ORDER_FIELD] as string, "side") + "\t");
                        sb.Append(dgr[MyOrderProcesser.SECURITYID_ORDER_FIELD] as string + "\t");
                        sb.Append(dgr[MyOrderProcesser.SYMBOL_ORDER_FIELD] as string + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.FIRMFLAG_ORDER_FIELD] as string, "firmflag") + "\t");
                        sb.Append(string.Format("{0:N4}", dgr[MyOrderProcesser.PRICE_ORDER_FIELD]) + "\t");
                        sb.Append(this.Covert(dgr[MyOrderProcesser.WHOLE_FLAG_FIELD], "wholeflag") + "\t");
                        sb.Append(dgr[MyOrderProcesser.QTY_ORDER_FIELD] + "\t");
                        sb.Append(this.NumCovert(dgr[MyOrderProcesser.EXECQTY_ORDER_FIELD]) + "\t");
                        sb.Append(dgr[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venues") + "\t");
                        sb.Append(ComboSourceUtil.Instance.FindEnum(dgr[MyOrderProcesser.CROSS_VENUES_FIELD] as string, "CrossVenues") + "\t");
                        sb.Append(dgr[MyOrderProcesser.VENUES_SELECT_DISPLAY_FIELD] as string + "\t");
                        sb.Append(dgr[MyOrderProcesser.STCCODE_ORDER_FIELD] as string + "\t");
                        sb.Append(TimeUtils.Instance.ToTime(dgr[MyOrderProcesser.UPDATETIME_ORDER_FIELD] as string) + "\t");
                        sb.Append("\n");
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 双击选中当前行
        /// </summary>
        /// <param name="SelectedItemsList"></param>
        public void SetCurrentSelected(IList SelectedItemsList)
        {
            if (SelectedItemsList != null && SelectedItemsList.Count > 0)
            {
                foreach (DataRowView dgr in SelectedItemsList)
                {
                    dgr[MyOrderProcesser.ISCHECKED_ORDER_FIELD] = dgr[MyOrderProcesser.ISCHECKED_ORDER_FIELD].ToString().ToLower() == "true" ? false : true;
                }
            }
        }

        private string Covert(object value, string parameter)
        {
            if (value != null)
            {
                string key = value.ToString();
                string[] keyArr = key.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                string param = parameter.ToString();

                StringBuilder sb = new StringBuilder();
                foreach (string item in keyArr)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(CommonConstants.COMMA_CHAR);
                    }
                    sb.Append(ComboSourceUtil.Instance.FindEnum(item.Trim(), param));
                }
                return sb.ToString();

            }
            return "";
        }
        private string NumCovert(object value)
        {
            if (value != null)
            {
                if (value is decimal)
                {
                    decimal temp = CommonUtils.ConvertToDecimal(value.ToString());
                    if (temp != decimal.Zero)
                    {
                        return temp.ToString("N4");
                    }
                }
                else if (value is long)
                {
                    long temp = CommonUtils.ConvertToLong(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
                else if (value is int)
                {
                    int temp = CommonUtils.ConvertToInt(value.ToString());
                    if (temp != 0)
                    {
                        return temp.ToString();
                    }
                }
            }
            return "--";
        }
        public object StringConvert(object value)
        {
            if (value == null || value.ToString() == string.Empty)
            {
                return "--";
            }
            return value;
        }

        internal void RefreshID()
        {
            lock (MyExecutionProcesser.Processer.LockObj_MyExecutions)
            {
                int recc = this.MyExecutionsDV.Table.Rows.Count;
                foreach (DataRow myOrder in this.MyExecutionsDV.Table.Rows)
                {
                    myOrder.BeginEdit();
                    myOrder["ID"] = recc;
                    myOrder.EndEdit();
                    myOrder.AcceptChanges();
                    recc--;
                }
            }
        }
    }
}
