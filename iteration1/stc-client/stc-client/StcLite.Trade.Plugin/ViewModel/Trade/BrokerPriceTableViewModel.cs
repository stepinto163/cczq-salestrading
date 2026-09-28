using STC.Common.Model;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDB.Trade.Plugin.ViewModel.Trade
{
    public class BrokerPriceTableViewModel : BaseViewModel
    {

        private object _lockObj = new object();
        private string _currentBroker = string.Empty;
        public void SetCurrentBroker(string broker)
        {
            _currentBroker = broker;
        }

        #region 关联属性
        private string _bidPrice;
        public string BidPrice { get { return _bidPrice; } set { _bidPrice = value; this.OnPropertyChanged(); } }


        private string _bidTxt;
        public string BidTxt { get { return _bidTxt; } set { _bidTxt = value; this.OnPropertyChanged(); } }

        private string _ofrPrice;
        public string OfrPrice { get { return _ofrPrice; } set { _ofrPrice = value; this.OnPropertyChanged(); } }


        private string _ofrTxt;
        public string OfrTxt { get { return _ofrTxt; } set { _ofrTxt = value; this.OnPropertyChanged(); } }

        private string _bidCornerMarket;
        public string BidCornerMarket { get { return _bidCornerMarket; } set { _bidCornerMarket = value; this.OnPropertyChanged(); } }

        private string _ofrCornerMarket;
        public string OfrCornerMarket { get { return _ofrCornerMarket; } set { _ofrCornerMarket = value; this.OnPropertyChanged(); } }

        #endregion

        #region 表格

        public const string KEY_FIELD = "Key";//Source + price
        public const string SECURITY_ID_FIELD = "SecurityID";
        public const string SHOW_SETTLE_TYPE_FIELD = "ShowSettleType";
        public const string SETTLE_TYPE_FIELD = "SettleType";
        public const string SETTLE_FWD_TYPE_FIELD = "SettleFwdType";
        public const string BID_FIELD = "Bid";
        public const string BID_AMOUNT_FIELD = "BidAmount";
        public const string OFR_FIELD = "Ofr";
        public const string OFR_AMOUNT_FIELD = "OfrAmount";
        public const string SOURCE_FIELD = "Source";
        public const string UPDATE_TIME_FIELD = "UpdateTime";
        public const string DELETE_FLAG_FIELD = "DeleteFlag";

        public const string BID_CORNER_MARKET_FIELD = "BidCornerMarket";
        public const string OFR_CORNER_MARKET_FIELD = "OfrCornerMarket";


        private DataView _tableView;
        public DataView TableView
        {
            get
            {
                lock (_lockObj)
                {
                    if (_tableView == null)
                    {
                        _tableView = new DataTable().DefaultView;
                        _tableView.Table.Columns.Add(KEY_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(SECURITY_ID_FIELD, typeof(string));
                        //_tableView.Table.Columns.Add(SORT_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(SETTLE_TYPE_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(SHOW_SETTLE_TYPE_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(SETTLE_FWD_TYPE_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(BID_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(BID_AMOUNT_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(OFR_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(OFR_AMOUNT_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(SOURCE_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(UPDATE_TIME_FIELD, typeof(string));
                        _tableView.Table.Columns.Add(DELETE_FLAG_FIELD, typeof(string));

                        //_tableView.Sort = $"{SORT_FIELD} asc";
                        _tableView.Table.PrimaryKey = new DataColumn[] {
                            _tableView.Table.Columns[KEY_FIELD],
                        };
                        this.OnPropertyChanged();
                    }
                    return this._tableView;
                }
            }
        }

        #endregion


        public void ClearUI()
        {
            lock (this._lockObj)
            {
                _tableView.Table.Clear();
                BidPrice = OfrPrice = BidTxt = OfrTxt = CommonConstants.NO_PRICE;
                PriceCalHelper.Instance.InitTableDataByBroker(_tableView, _currentBroker);
            }
        }

        public void RefreshData(string broker, List<GNoMDEntries> bidMDEntryList, List<GNoMDEntries> ofrMDEntryList)
        {
            if (!broker.Equals(_currentBroker))
                return;

            lock (this._lockObj)
            {
                RefreshDataBySide(CommonConstants.SIDE_BID_ONE, broker, bidMDEntryList);
                RefreshDataBySide(CommonConstants.SIDE_OFR_TWO, broker, ofrMDEntryList);
            }
        }

        private void RefreshDataBySide(string side, string broker, List<GNoMDEntries> noMDEntryList)
        {
            StringBuilder sb = new StringBuilder();

            decimal fwdQty = decimal.Zero;
            decimal spotQty = decimal.Zero;
            string spotKey = string.Empty;
            string fwdKey = string.Empty;
            string price = string.Empty;
            foreach (GNoMDEntries mdEntry in noMDEntryList)
            {
                if (sb.Length > 0)
                    sb.Append("+");

                sb.Append(mdEntry.MDEntrySize);

                price = mdEntry.MDEntryPx;

                decimal d = 0;
                string key = string.Empty;

                DataRow[] drs = null;

                if (mdEntry.SettlType == CommonConstants.T1_ONE)
                {
                    key = string.Format("{0}{1}", broker, CommonConstants.T1_EN);
                    drs = _tableView.Table.Select($"{KEY_FIELD} = '{key}'");

                    if (drs.Length > 0)
                    {
                        if (side == CommonConstants.SIDE_BID_ONE)
                        {
                            drs[0][BID_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                            drs[0][BID_AMOUNT_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntrySize) ? CommonConstants.NO_PRICE : mdEntry.MDEntrySize;
                        }
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                        {
                            drs[0][OFR_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                            drs[0][OFR_AMOUNT_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntrySize) ? CommonConstants.NO_PRICE : mdEntry.MDEntrySize;
                        }
                    }
                }
                else if (CommonConstants.T2_T3.Contains(mdEntry.SettlType))
                {
                    spotKey = key = string.Format("{0}{1}", broker, CommonConstants.T2_EN);

                    if (decimal.TryParse(mdEntry.MDEntrySize, out d))
                        spotQty += d;
                    drs = _tableView.Table.Select($"{KEY_FIELD} = '{key}'");

                    if (drs.Length > 0)
                    {
                        if (side == CommonConstants.SIDE_BID_ONE)
                            drs[0][BID_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                        else if (side == CommonConstants.SIDE_OFR_TWO)
                            drs[0][OFR_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                    }
                }
                else if (PriceCalHelper.Instance.IsFwd(mdEntry.SettlType))
                {
                    fwdKey = key = string.Format("{0}{1}", broker, CommonConstants.FWD);

                    if (decimal.TryParse(mdEntry.MDEntrySize, out d))
                        fwdQty += d;
                }

                if (drs != null && drs.Length > 0)
                {
                    drs[0][BID_FIELD] = string.IsNullOrEmpty(mdEntry.MDEntryPx) ? CommonConstants.NO_PRICE : mdEntry.MDEntryPx;
                }
            }

            //更新+1
            if (spotQty > 0 && !string.IsNullOrEmpty(spotKey))
            {
                UpdateAmount(spotKey, side, spotQty.ToString());
            }
            else
            {
                spotKey = string.Format("{0}{1}", broker, CommonConstants.T2_EN);
                UpdateAmount(spotKey, side, CommonConstants.NO_PRICE);
            }


            //更新远期
            if (fwdQty > 0 && !string.IsNullOrEmpty(fwdKey))
            {
                UpdateAmount(fwdKey, side, fwdQty.ToString());

            }
            else
            {
                fwdKey = string.Format("{0}{1}", broker, CommonConstants.FWD);
                UpdateAmount(fwdKey, side, CommonConstants.NO_PRICE);
            }

            if (side == CommonConstants.SIDE_BID_ONE)
            {
                this.BidTxt = sb.ToString();
                this.BidPrice = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
            }
            else if (side == CommonConstants.SIDE_OFR_TWO)
            {
                this.OfrTxt = sb.ToString();
                this.OfrPrice = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
            }
        }

        private void UpdateAmount(string key, string side, string qty)
        {
            DataRow[] drs = _tableView.Table.Select($"{KEY_FIELD} = '{key}'");

            if (drs != null && drs.Length > 0)
            {
                if (side == CommonConstants.SIDE_BID_ONE)
                    drs[0][BID_AMOUNT_FIELD] = qty;
                else if (side == CommonConstants.SIDE_OFR_TWO)
                    drs[0][OFR_AMOUNT_FIELD] = qty;
            }
        }
    }
}
