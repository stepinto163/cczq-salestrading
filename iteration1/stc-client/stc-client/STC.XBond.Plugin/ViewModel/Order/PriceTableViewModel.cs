using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel.Order
{
    public class PriceTableViewModel : BaseViewModel
    {
        private DataGrid _ownDataGrid = null;

        public void SetOwnDataGrid(DataGrid dataGrid)
        {
            _ownDataGrid = dataGrid;
        }

        #region 数据源
        public const string KEY = "Key";
        public const string IS_CHECKED = "IsChecked";
        public const string PRICE = "Price";
        public const string IS_FIRM = "IsFirm";
        public const string QTY = "Qty";
        public const string SETTLE = "Settle";
        public const string SETTLE_SHOW = "SettleShow";
        public const string VENUE = "Venue";
        public const string CLICK_QTY = "ClickQty";
        public const string UPDATE_TIME = "UpdateTime";
        public const string SIDE = "Side";


        /// <summary>
        /// 我的订单资源锁
        /// </summary>
        private object _priceTableLock = new object();
        public object PriceTableLock
        {
            get
            {
                return _priceTableLock;
            }
        }

        private DataTable _priceTableDT;
        public DataTable PriceTableDT
        {
            get
            {
                lock (_priceTableLock)
                {
                    if (_priceTableDT == null)
                    {
                        _priceTableDT = new DataTable();
                        _priceTableDT.Columns.Add(KEY, typeof(string));
                        _priceTableDT.Columns.Add(IS_CHECKED, typeof(bool));
                        _priceTableDT.Columns.Add(PRICE, typeof(string));
                        _priceTableDT.Columns.Add(IS_FIRM, typeof(string));
                        _priceTableDT.Columns.Add(QTY, typeof(string));
                        _priceTableDT.Columns.Add(SETTLE, typeof(string));
                        _priceTableDT.Columns.Add(SETTLE_SHOW, typeof(string));
                        _priceTableDT.Columns.Add(VENUE, typeof(string));
                        _priceTableDT.Columns.Add(CLICK_QTY, typeof(string));
                        _priceTableDT.Columns.Add(UPDATE_TIME, typeof(string));
                        _priceTableDT.Columns.Add(SIDE, typeof(string));

                        _priceTableDT.PrimaryKey = new DataColumn[] { _priceTableDT.Columns[KEY] };

                        _priceTableDT.DefaultView.Sort = $"{UPDATE_TIME} desc";
                    }
                    return this._priceTableDT;
                }
            }
        }

        private DataView _priceTableDV;

        /// <summary>
        /// 我的订单数据源
        /// </summary>
        public DataView PriceTableDV
        {
            get
            {
                lock (_priceTableLock)
                {
                    if (_priceTableDV == null)
                    {
                        _priceTableDV = new DataView(_priceTableDT);
                        _priceTableDV.Sort = $"{UPDATE_TIME} desc";
                        this.OnPropertyChanged();
                    }
                    return this._priceTableDV;
                }
            }
        }

        private object _selectItem;
        public object SelectItem
        {
            get { return _selectItem; }
            set
            {
                _selectItem = value;
            }
        }

        #endregion

        private bool _isAllChecked;
        public bool IsAllChecked
        {
            get { return _isAllChecked; }
            set
            {
                _isAllChecked = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand AddCommand { get { return new RelayCommand(AddClick); } }
        public ICommand DownCommand { get { return new RelayCommand(DownClick); } }

        private decimal _currentTotal = 0;


        public event Action<object> OnTotalQtyChange;

        private void AddClick()
        {
            if (_selectItem != null)
            {
                DataRowView selectRow = _selectItem as DataRowView;
                string clickQtyStr = CommonUtils.ConvertToString(selectRow[CLICK_QTY]);
                decimal d = 0;

                if (decimal.TryParse(clickQtyStr, out d))
                {
                    selectRow[CLICK_QTY] = (d + 1).ToString();
                    selectRow[IS_CHECKED] = true;

                    decimal total = ComputeTotalQty();

                    OnTotalQtyChange?.Invoke(total);
                }

            }
            
        }

        private decimal ComputeTotalQty()
        {
            decimal result = 0;
            lock (_priceTableLock)
            {
                DataRowCollection rows = _priceTableDT.Rows;
                decimal d = 0;
                foreach (DataRow dr in rows)
                {
                    string clickQtyStr = CommonUtils.ConvertToString(dr[CLICK_QTY]);
                    if (decimal.TryParse(clickQtyStr, out d))
                    {
                        result += d;
                    }
                        
                }
            }

            return result;
        }

        private void DownClick()
        {
            if (_selectItem != null)
            {
                DataRowView selectRow = _selectItem as DataRowView;
                string clickQtyStr = CommonUtils.ConvertToString(selectRow[CLICK_QTY]);
                decimal d = 0;

                if (decimal.TryParse(clickQtyStr, out d) && (d - 1) >= 0)
                {
                    selectRow[CLICK_QTY] = (d - 1).ToString();

                    decimal total = ComputeTotalQty();

                    selectRow[IS_CHECKED] = true;

                    OnTotalQtyChange?.Invoke(total);
                }

            }
        }

        public decimal GetAllFilterQty()
        {
            decimal result = 0;

            lock (_priceTableLock)
            {
                DataTable filterTable = PriceTableDV.ToTable();

                DataRowCollection dataRowCollection = filterTable.Rows;
                decimal d = 0;

                foreach (DataRow dataRow in dataRowCollection)
                {
                    string qty = dataRow[QTY].ToString();
                    if (decimal.TryParse(qty, out d))
                        result += d;
                }
            }
            return result;
        }

        public void ResetClickQty(decimal totalQty)
        {
            _currentTotal = totalQty;

            UpdateRowBgAndClickQty();
        }

        public void AddOrUpdateData(GNoMDEntries priceMatchParam)
        {
            lock (_priceTableLock)
            {
                DataRow priceDr = this.PriceTableDT.Rows.Find(priceMatchParam.MDEntryID);

                if (priceDr != null)
                {
                    if (CommonUtils.ConvertToString(priceDr[UPDATE_TIME]).CompareTo(priceMatchParam.MDEntryTime) > 0)
                    {
                        // 如果时间早， 就不更新了。
                        return;
                    }
                }
                else
                {
                    priceDr = this._priceTableDT.NewRow();
                    priceDr[KEY] = priceMatchParam.MDEntryID;
                    priceDr[IS_CHECKED] = true;

                    this._priceTableDT.Rows.InsertAt(priceDr, 0);
                }

                if (priceDr != null)
                {
                    priceDr[PRICE] = CommonUtils.FormatNum(priceMatchParam.MDEntryPx);
                    priceDr[IS_FIRM] = priceMatchParam.FirmFlag;

                    decimal d = 0;
                    if (decimal.TryParse(priceMatchParam.MDEntrySize, out d))
                    {
                        priceDr[CLICK_QTY] = priceDr[QTY] = (int)d / 1000;
                    }
                    
                    priceDr[SETTLE] = priceMatchParam.SettlType;
                    priceDr[SETTLE_SHOW] = ComboSourceUtil.Instance.FindEnum(priceMatchParam.SettlType, "settletype_myorder");
                    priceDr[VENUE] = priceMatchParam.Venues;
                    
                    priceDr[UPDATE_TIME] = priceMatchParam.MDEntryTime;
                    priceDr[SIDE] = priceMatchParam.MDEntryType=="0"?"1":"0";

                    _priceTableDT.AcceptChanges();

                    decimal total = ComputeTotalQty();

                    OnTotalQtyChange?.Invoke(total);
                }
            }
        }

        public void Filter(List<string> settleFilter)
        {
            string filter = string.Empty;

            //settle只显示过滤的settle
            if (settleFilter != null && settleFilter.Count > 0)
            {
                foreach (string settle in settleFilter)
                {
                    filter += "'" + settle + "',";
                }

                filter = string.Format("{0} in ({1})", SETTLE_SHOW, filter.Substring(0, filter.Length - 1));
            }

            _priceTableDV.RowFilter = filter;
        }

        private void UpdateRowBgAndClickQty()
        {
            lock (_priceTableLock)
            {
                DataTable filterTable = PriceTableDV.ToTable();

                DataRowCollection dataRowCollection = filterTable.Rows;
                decimal d = 0;
                decimal total = _currentTotal;
                List<string> selectKeyList = new List<string>();

                foreach (DataRow dataRow in dataRowCollection)
                {
                    string key = dataRow[KEY].ToString();

                    DataRow findDr = _priceTableDV.Table.Rows.Find(key);

                    string clickQty = dataRow[CLICK_QTY].ToString();
                    string qty = dataRow[QTY].ToString();

                    if (total <= 0)
                    {
                        findDr[IS_CHECKED] = false;
                        findDr[CLICK_QTY] = 0;

                        continue;
                    }


                    if (decimal.TryParse(clickQty, out d))
                    {
                        if (d <= total)
                        {
                            findDr[IS_CHECKED] = true;

                            decimal tem = 0;
                            if (decimal.TryParse(qty, out tem))
                            {
                                findDr[CLICK_QTY] = tem <= total ? tem : total;

                                if (tem >= total)
                                    d = tem;

                            }

                            selectKeyList.Add(key);
                        }
                        else
                        {
                            if (total > 0)
                            {
                                findDr[IS_CHECKED] = true;
                                findDr[CLICK_QTY] = total;
                                selectKeyList.Add(key);
                            }
                            else
                            {
                                findDr[CLICK_QTY] = 0;

                                findDr[IS_CHECKED] = false;
                            }
                        }
                        if (total > 0)
                            total -= d;
                    }
                    else
                    {
                        findDr[IS_CHECKED] = false;
                    }
                }

                foreach (string dataKey in selectKeyList)
                {
                    DataRow findDr = _priceTableDV.Table.Rows.Find(dataKey);

                    _ownDataGrid.SelectedItems.Add(findDr);
                }


                _priceTableDV.Table.AcceptChanges();
            }
        }
    }
}
