using Ats.Foundation.Message;
using STC.Common.Handler.Base;
using STC.Common.Handler.TBook;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View.Component;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 交易包IDB数据编辑框IDBDetailListView的数据内容
    /// </summary>
    public class IDBDetailListViewModel : ViewModelBase
    {
        /// <summary>
        /// 需要需要添加数据的数据源 
        /// </summary>
        public DataTable TargetDatas = null;
        public BookEditViewModel bookEditViewModel = null;
        public bool IsSelf = false;
        public string Side;
        public StcTbookUsers PartySubID;

        /// <summary>
        /// 选择数量
        /// </summary>
        private long _SelectedQty = 0;

        public long SelectedQty
        {
            get
            {
                return _SelectedQty;
            }
            set
            {
                if (_SelectedQty != value)
                {
                    _SelectedQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 成交总量
        /// </summary>
        private long _TotalQty = 0;

        public long TotalQty
        {
            get
            {
                return _TotalQty;
            }
            set
            {
                if (_TotalQty != value)
                {
                    this._TotalQty = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 债券代码
        /// </summary>
        private string _SecurityID = string.Empty;

        public string SecurityID
        {
            get
            {
                return this._SecurityID;
            }

            set
            {
                if (this._SecurityID != value)
                {
                    this.ClearData();
                    // 切换债券
                    this._SecurityID = value;
                    this.OnPropertyChanged();
                    
                    PriceHandler.Instance.RegisterCTPrice(value, Instance_OnCTPrice);
                }
            }
        }

        private ThreadQueue<Common.Model.Fix.Message.MarketDataSnapshotFullRefresh> _priceThreadQueue;

        public const string ISDELETE_FIELD = "isDelete";
        public const string ISCHECKED_FIELD = "IsChecked";
        public const string MDENTRYID_FIELD = "MDEntryID";
        public const string REMARK_FIELD = "Remark";
        public const string LASTPRICE_FIELD = "LastPrice";
        public const string LASTQTY_FIELD = "LastQty";
        public const string SETTLTYPE_FIELD = "SettlType";
        public const string COUNTERPARTY_FIELD = "CounterPart";
        public const string INSTITUTIONID_FIELD = "'InstitutionID";
        public const string INSTITUTIONNAME_FIELD = "InstitutionName";
        public const string TRADEID_FIELD = "TraderID";
        public const string TRADENAME_FIELD = "TraderName";
        public const string CREATEIME_FIELD = "CreateDate";
        public const string SETTLDATE_FIELD = "SettlDate";
        public const string TRADEDATE_FIELD = "TradeDate";
        public const string JSONINFO_FILED = "JsonInfo";

        private object _datasLock = new object();

        public DataTable _datas;

        public DataTable Datas
        {
            get
            {
                if (_datas == null)
                {
                    _datas = new DataTable();
                    _datas.Columns.Add(ISDELETE_FIELD, typeof(string));
                    _datas.Columns.Add(ISCHECKED_FIELD, typeof(bool));
                    _datas.Columns.Add(MDENTRYID_FIELD, typeof(string));
                    _datas.Columns.Add(REMARK_FIELD, typeof(string));
                    _datas.Columns.Add(LASTPRICE_FIELD, typeof(string));
                    _datas.Columns.Add(LASTQTY_FIELD, typeof(long));
                    _datas.Columns.Add(SETTLTYPE_FIELD, typeof(string));
                    _datas.Columns.Add(COUNTERPARTY_FIELD, typeof(string));
                    _datas.Columns.Add(INSTITUTIONID_FIELD, typeof(string));
                    _datas.Columns.Add(INSTITUTIONNAME_FIELD, typeof(string));
                    _datas.Columns.Add(TRADEID_FIELD, typeof(string));
                    _datas.Columns.Add(TRADENAME_FIELD, typeof(string));
                    _datas.Columns.Add(CREATEIME_FIELD, typeof(string));
                    _datas.Columns.Add(SETTLDATE_FIELD, typeof(string));
                    _datas.Columns.Add(TRADEDATE_FIELD, typeof(string));
                    _datas.Columns.Add(JSONINFO_FILED, typeof(string));

                    _datas.PrimaryKey = new DataColumn[] { _datas.Columns[MDENTRYID_FIELD] };
                    this.OnPropertyChanged();
                }
                return _datas;
            }
        }

        public IDBDetailListViewModel()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
            Init();
        }
        ~IDBDetailListViewModel()
        {
            this.Dispose();
        }

        public void Init()
        {
            BindingOperations.EnableCollectionSynchronization(Datas.DefaultView, _datasLock);

            if (_priceThreadQueue == null)
            {
                _priceThreadQueue = new ThreadQueue<Common.Model.Fix.Message.MarketDataSnapshotFullRefresh>(UpdatePrice);
            }
        }


        new public void Dispose()
        {
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);
            this.ClearData();

            if (this._datas != null)
            {
                this._datas.Dispose();
                this._datas = null;
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHandler.Info("IDBDetailListViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ClearData();
                    this.Init();

                    if (string.IsNullOrEmpty(this.SecurityID) == false)
                    {
                        PriceHandler.Instance.RegisterCTPrice(this.SecurityID, Instance_OnCTPrice);
                    }
                }));
            }
        }

        public void ClearData()
        {
            PriceHandler.Instance.UnregisterCTPrice(this.SecurityID, Instance_OnCTPrice);

            if (this._datas != null)
            {
                this._datas.Rows.Clear();
            }
        }

        private void Instance_OnCTPrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                if (obj != null && obj.SecurityID == this.SecurityID)
                {
                    LogHandler.Debug($"IDBDetailListViewModel onCTPrice:{obj.SecurityID}");
                    if (_priceThreadQueue != null)
                    {
                        _priceThreadQueue.Enqueue(obj);
                    }
                }
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }

        private void UpdatePrice(Common.Model.Fix.Message.MarketDataSnapshotFullRefresh obj)
        {
            lock(_datasLock)
            {
                foreach (DataRow dr in this.Datas.Rows)
                {
                    dr[ISDELETE_FIELD] = "1";
                }

                if (obj != null && obj.NoMDEntries != null && obj.NoMDEntries.Count > 0)
                {
                    foreach (Common.Model.Fix.Message.GNoMDEntries entry in obj.NoMDEntries)
                    {
                        if (entry.Venues == "IDB"
                            && ((entry.MDEntryType == "0" && this.Side == "1") || (entry.MDEntryType == "1" && this.Side == "2"))
                            && (this.PartySubID != null && this.PartySubID.Userid == entry.PartySubID))
                        {
                            // side, qty, price, InstitutionName,TraderName
                            string key = entry.MDEntryID;

                            DataRow findDr = this.Datas.Rows.Find(key);

                            if (findDr == null)
                            {
                                findDr = this.Datas.NewRow();
                                findDr[ISCHECKED_FIELD] = false;
                                findDr[MDENTRYID_FIELD] = entry.MDEntryID;
                                findDr[ISDELETE_FIELD] = "0";
                                this.Datas.Rows.Add(findDr);
                            }

                            string side = entry.MDEntryType == "1" ? "Ofr" : "Bid";

                            string px = CommonUtils.ConvertToDecimal(entry.MDEntryPx).ToString("N4");
                            string qty = CommonUtils.ConvertToLong(entry.MDEntrySize).ToString();

                            findDr[REMARK_FIELD] = $"{side}  {entry.MDEntrySize}  {px}  {qty} {entry.InstitutionName} - {entry.TraderName}";
                            findDr[LASTPRICE_FIELD] = px;
                            findDr[LASTQTY_FIELD] = qty;
                            findDr[COUNTERPARTY_FIELD] = $"{entry.InstitutionName} - {entry.TraderName}";
                            findDr[INSTITUTIONID_FIELD] = entry.InstitutionID;
                            findDr[INSTITUTIONNAME_FIELD] = entry.InstitutionName;
                            findDr[TRADEID_FIELD] = entry.TraderID;
                            findDr[TRADENAME_FIELD] = entry.TraderName;
                            findDr[CREATEIME_FIELD] = DateTimeEx.ServerTime.ToString();
                            findDr[TRADEDATE_FIELD] = entry.MDEntryDate;
                            string settltype = entry.SettlType;
                            findDr[SETTLDATE_FIELD] = CommonUtils.CalSettlDate(DateTimeEx.ServerTime.ToString("yyyyMMdd"), entry.SettlType, out settltype);
                            findDr[SETTLTYPE_FIELD] = settltype;
                            findDr[JSONINFO_FILED] = MsgHelper.Serializer(entry);
                            findDr[ISDELETE_FIELD] = "0";

                         
                            this.TotalQty++;
                        }
                    }
                }

                DataRow[] drs = this.Datas.Select($"{ISDELETE_FIELD} = '1'");
                foreach(DataRow dr in drs)
                {
                    this.Datas.Rows.Remove(dr);
                }

                this.Datas.AcceptChanges();
            }
           
        }

        public bool Finish()
        {
            if (TargetDatas == null)
            {
                Common.View.Component.CustomMessageBox.Info("程序异常！", "添加代客IDB订单");
                return false;
            }

            //  判断勾选数量
            if (TargetDatas != null)
            {
                int count = 0;
                foreach (DataRow dr in this.Datas.Rows)
                {
                    if ((bool)dr[ISCHECKED_FIELD])
                    {
                        count++;
                    }
                }

                if ((TargetDatas.Rows.Count + count) > 10)
                {
                    // 添加数据超出10条限制，请检查
                    Common.View.Component.CustomMessageBox.Info("添加数据超出10条限制，请检查", "添加代客IDB订单");
                    return false;
                }
            }

            // 添加数据
            lock (_datasLock)
            {
                DataRow[] drs = this.Datas.Select($"{ISCHECKED_FIELD} = true");
                if (drs != null && drs.Length > 0)
                {
                    foreach (DataRow addDr in drs)
                    {

                        string key = CommonUtils.ConvertToString(addDr[MDENTRYID_FIELD]);
                        STC.Common.Model.TBook.OrderCancelRequest cancelPo = new STC.Common.Model.TBook.OrderCancelRequest();
                        cancelPo.AlgoName = "CROSS";
                        cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                        cancelPo.OrderID = key;
                        TBookCancelReplyPo cancelReply = TBookHandler.Instance.CancelOrder(cancelPo);

                        if (cancelReply.Code == "0000")
                        {
                            DataRow dr = this.TargetDatas.NewRow();
                            dr[BookEditItemViewModel.SOURCETRADER_FIELD] = "IDB";
                            dr[BookEditItemViewModel.COUNTER_PARTY_ID_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.INSTITUTIONID_FIELD]);
                            dr[BookEditItemViewModel.COUNTER_PARTY_NAME_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.INSTITUTIONNAME_FIELD]);
                            dr[BookEditItemViewModel.COUNTER_PARTY_SUB_ID_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.TRADEID_FIELD]);
                            dr[BookEditItemViewModel.COUNTER_PARTY_SUB_NAME_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.TRADENAME_FIELD]);
                            dr[BookEditItemViewModel.YIELD_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.LASTPRICE_FIELD]);
                            dr[BookEditItemViewModel.QTY_FIELD] = CommonUtils.ConvertToLong(addDr[IDBDetailListViewModel.LASTQTY_FIELD].ToString());
                            dr[BookEditItemViewModel.SETTLETYPE_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.SETTLTYPE_FIELD]);
                            dr[BookEditItemViewModel.TRADEDATE_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.TRADEDATE_FIELD]);
                            dr[BookEditItemViewModel.CREATEDATE_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.CREATEIME_FIELD]);
                            dr[BookEditItemViewModel.SETTLDATE_FIELD] = CommonUtils.ConvertToString(addDr[IDBDetailListViewModel.SETTLDATE_FIELD]);
                            if (bookEditViewModel != null)
                            {
                                long qty = CommonUtils.ConvertToLong(dr[BookEditItemViewModel.QTY_FIELD] == null ? "0" : dr[BookEditItemViewModel.QTY_FIELD].ToString());
                                if (IsSelf)
                                {
                                    bookEditViewModel.SelfQty += qty;
                                }
                                else
                                {
                                    bookEditViewModel.CounterPartQty += qty;
                                }
                            }

                            this.TargetDatas.Rows.Add(dr);
                        }
                    }
                }
            }

            return true;
        }
    }
}
