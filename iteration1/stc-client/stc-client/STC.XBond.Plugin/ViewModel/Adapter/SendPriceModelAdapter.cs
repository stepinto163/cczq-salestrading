using STC.Common.Handler.Price;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.XBond.Plugin.ViewModel.Adapter
{
    /// <summary>
    /// 下单界面显示内容适配类
    /// </summary>
    public class SendPriceModelAdapter
    {
        private SendPriceItem _sendPriceItem = null;
        public event Action<MarketDataSnapshotFullRefresh> OnMDPrice;
        private bool _isFirm = false;
        private string _settl = string.Empty;
        private string _broker = string.Empty;
        private bool _isInit = true;
        private bool _isSubcribe = false;
        private object _lock = new object();

        /// <summary>
        /// 设置过滤参数
        /// </summary>
        /// <param name="isFirm"></param>
        /// <param name="settle"></param>
        /// <param name="broker"></param>
        public void SetFilterParam(bool isFirm, string settle, string broker = "")
        {
            _isFirm = isFirm;
            _settl = settle;
            _broker = broker;

            if (!_isInit && _sendPriceItem != null)
            {
                GetCtPrice();

                if (!_isSubcribe)
                {
                    if (!string.IsNullOrEmpty(_sendPriceItem.Security))
                    {
                        MDHandler.Instance.Init(_sendPriceItem.Security, OnCtPrice, null);
                        _isSubcribe = true;
                    }
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="sendPriceItem"></param>
        public void Init(SendPriceItem sendPriceItem)
        {
            _sendPriceItem = sendPriceItem;

            GetCtPrice();

            if (!string.IsNullOrEmpty(sendPriceItem.Security))
            {
                MDHandler.Instance.Init(sendPriceItem.Security, OnCtPrice, null);
                _isSubcribe = true;
            }

            _isInit = false;
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            MDHandler.Instance.UnSubscribe(OnCtPrice, null);
        }

        /// <summary>
        /// 获取缓存的深度行情数据
        /// </summary>
        private void GetCtPrice()
        {
            string topic = MDHandler.Instance.GetCtTopic(_sendPriceItem.Security);
            MarketDataSnapshotFullRefresh bestPrice = MDHandler.Instance.GetCachePriceByTopic(topic);
            ParseCtPrice(bestPrice);
            if (!string.IsNullOrEmpty(_broker))
                ParseCtBrokerPrice(bestPrice);
        }

        /// <summary>
        /// 接收深度行情数据回调方法
        /// </summary>
        /// <param name="bestPrice"></param>
        private void OnCtPrice(MarketDataSnapshotFullRefresh bestPrice)
        {
            ParseCtPrice(bestPrice);
            if (!string.IsNullOrEmpty(_broker))
                ParseCtBrokerPrice(bestPrice);

            if (OnMDPrice != null)
            {
                OnMDPrice?.Invoke(bestPrice);
            }
        }

        /// <summary>
        /// 得到最优价格
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="ofr"></param>
        /// <param name="settle"></param>
        /// <param name="ctPrice"></param>
        private void GetBestPrice(out decimal? bid, out decimal? ofr, string settle, MarketDataSnapshotFullRefresh ctPrice)
        {
            bid = null;
            ofr = null;

            decimal d = 0;
            foreach (GNoMDEntries price in ctPrice.NoMDEntries)
            {
                string[] arrSettle = price.SettlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                decimal? temPx = CommonUtils.ConvertToDecimalOrNull(price.MDEntryPx);
                string tem = temPx == null ? string.Empty : temPx.HasValue ? temPx.Value.ToString("F4") : string.Empty;
                if (decimal.TryParse(tem, out d))
                {
                    if (price.MDEntryType.Equals("0"))//bid
                    {
                        //if (_isFirm)
                        //{
                        //    if (price.FirmFlag == "1")
                        //    {
                        //        if (bid == null && arrSettle.Contains(settle))//price.SettlType.Equals(settle))
                        //            bid = d;

                        //        if (arrSettle.Contains(settle) && bid != null && bid.Value.CompareTo(d) > 0)
                        //            bid = d;
                        //    }
                        //}
                        //else if ((bid != null && bid.Value.CompareTo(d) > 0) || bid == null)
                        //    bid = d;

                        // 如果是远期
                        if (settle == "4")
                        {
                            if (price.SettlType.Contains(".") || price.SettlType =="4")
                            {
                                if ((bid != null && bid.Value.CompareTo(d) > 0) || bid == null)
                                    bid = d;
                            }
                        }
                        else if (arrSettle.Contains(settle))
                        {
                            if ((bid != null && bid.Value.CompareTo(d) > 0) || bid == null)
                                bid = d;
                        }
                    }
                    else if (price.MDEntryType.Equals("1"))//ofr
                    {
                        //if (_isFirm)
                        //{
                        //    if (price.FirmFlag == "1")
                        //    {
                        //        if (ofr == null && arrSettle.Contains(settle))//price.SettlType.Equals(settle))
                        //            ofr = d;

                        //        if (arrSettle.Contains(settle) && ofr != null && ofr.Value.CompareTo(d) < 0)
                        //            ofr = d;
                        //    }
                        //}
                        //else if ((ofr != null && ofr.Value.CompareTo(d) < 0) || ofr == null)
                        //    ofr = d;

                        // 如果是远期
                        if (settle == "4")
                        {
                            if (price.SettlType.Contains(".") || price.SettlType == "4")
                            {
                                if ((ofr != null && ofr.Value.CompareTo(d) < 0) || ofr == null)
                                    ofr = d;
                            }
                        }
                        else
                        if (arrSettle.Contains(settle))
                        {
                            if ((ofr != null && ofr.Value.CompareTo(d) < 0) || ofr == null)
                                ofr = d;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解析行情数据并赋值下单界面数据源
        /// </summary>
        /// <param name="ctPrice"></param>
        private void ParseCtPrice(MarketDataSnapshotFullRefresh ctPrice)
        {
            if (ctPrice == null || ctPrice.NoMDEntries == null || !ctPrice.SecurityID.Equals(_sendPriceItem.Security))
                return;

            lock (_lock)
            {
                decimal? bid = null;
                decimal? ofr = null;
                //string bid = string.Empty;
                //string ofr = string.Empty;
                _sendPriceItem.BestOfr = null;
                _sendPriceItem.BestOfrInt = string.Empty;
                _sendPriceItem.BestOfrFloat = string.Empty;
                _sendPriceItem.BestBid = null;
                _sendPriceItem.BestBidInt = string.Empty;
                _sendPriceItem.BestBidFloat = string.Empty;

                string[] arrSettle = _settl.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (arrSettle != null)
                {
                    foreach (string settle in arrSettle)
                    {
                        decimal? temBid = null;
                        decimal? temOfr = null;

                        GetBestPrice(out temBid, out temOfr, settle, ctPrice);

                        if (bid == null)
                            bid = temBid;
                        else
                        {
                            if (temBid != null && bid.Value > temBid.Value)
                                bid = temBid;
                        }

                        if (ofr == null)
                            ofr = temOfr;
                        else
                        {
                            if (temOfr != null && ofr.Value < temOfr.Value)
                                ofr = temOfr;
                        }
                    }
                }
                
                string bidStr = bid != null ? bid.Value.ToString() : string.Empty;
                string ofrStr = ofr != null ? ofr.Value.ToString() : string.Empty;
               
                if (!string.IsNullOrEmpty(bidStr))
                {
                    string[] arr = bidStr.Split(new char[] { '.' });
                    if (arr != null && arr.Length > 1)
                    {
                        _sendPriceItem.BestBid = CommonUtils.ConvertToDecimal(bidStr);

                        _sendPriceItem.BestBidInt = string.Format("{0}.", arr[0]);
                        _sendPriceItem.BestBidFloat = arr[1];
                    }
                }
                else
                {
                    _sendPriceItem.BestBid = null;
                    _sendPriceItem.BestBidInt = string.Empty;
                    _sendPriceItem.BestBidFloat = string.Empty;
                }

                if (!string.IsNullOrEmpty(ofrStr))
                {
                    string[] arr = ofrStr.Split(new char[] { '.' });
                    if (arr != null && arr.Length > 1)
                    {
                        _sendPriceItem.BestOfr = CommonUtils.ConvertToDecimal(ofrStr);

                        _sendPriceItem.BestOfrInt = string.Format("{0}.", arr[0]);
                        _sendPriceItem.BestOfrFloat = arr[1];
                    }
                }
                else
                {
                    _sendPriceItem.BestOfr = null;
                    _sendPriceItem.BestOfrInt = string.Empty;
                    _sendPriceItem.BestOfrFloat = string.Empty;
                }
            }
        }

        /// <summary>
        /// 解析行情数据得到Broker最优价格
        /// </summary>
        /// <param name="ctPrice"></param>
        private void ParseCtBrokerPrice(MarketDataSnapshotFullRefresh ctPrice)
        {
            if (ctPrice == null || ctPrice.NoMDEntries == null || !ctPrice.SecurityID.Equals(_sendPriceItem.Security))
                return;

            lock (_lock)
            {
                decimal? bid = null;
                decimal? ofr = null;
                //string bid = string.Empty;
                //string ofr = string.Empty;
                _sendPriceItem.BestBrokerBid = null;
                _sendPriceItem.BestBrokerOfr = null;


                decimal? temBid = null;
                decimal? temOfr = null;

                GetBestBrokerPrice(out temBid, out temOfr, ctPrice);

                if (bid == null)
                    bid = temBid;
                else
                {
                    if (temBid != null && bid.Value > temBid.Value)
                        bid = temBid;
                }

                if (ofr == null)
                    ofr = temOfr;
                else
                {
                    if (temOfr != null && ofr.Value < temOfr.Value)
                        ofr = temOfr;
                }

                string bidStr = bid != null ? bid.Value.ToString() : string.Empty;
                string ofrStr = ofr != null ? ofr.Value.ToString() : string.Empty;

                if (!string.IsNullOrEmpty(bidStr))
                {
                   _sendPriceItem.BestBrokerBid = CommonUtils.ConvertToDecimal(bidStr);
                }
                else
                {
                    _sendPriceItem.BestBrokerBid = null;
                }

                if (!string.IsNullOrEmpty(ofrStr))
                {
                      _sendPriceItem.BestBrokerOfr = CommonUtils.ConvertToDecimal(ofrStr);
                }
                else
                {
                    _sendPriceItem.BestBrokerOfr = null;
                }
            }
        }

        /// <summary>
        /// 得到Broker最优价格
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="ofr"></param>
        /// <param name="settle"></param>
        /// <param name="ctPrice"></param>
        private void GetBestBrokerPrice(out decimal? bid, out decimal? ofr, MarketDataSnapshotFullRefresh ctPrice)
        {
            bid = null;
            ofr = null;

            decimal d = 0;
            foreach (GNoMDEntries price in ctPrice.NoMDEntries)
            {
                string[] arrSettle = price.SettlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                decimal? temPx = CommonUtils.ConvertToDecimalOrNull(price.MDEntryPx);
                string tem = temPx == null ? string.Empty : temPx.HasValue ? temPx.Value.ToString("F4") : string.Empty;
                if (decimal.TryParse(tem, out d))
                {
                    if (price.MDEntryType.Equals("0"))//bid
                    {
                        if (string.IsNullOrEmpty(this._broker))
                        {
                            if ((bid != null && bid.Value.CompareTo(d) > 0) || bid == null)
                                bid = d;
                        }
                        else
                        {
                            if (price.Venues.Trim() == this._broker)
                            {
                                if ((bid != null && bid.Value.CompareTo(d) > 0) || bid == null)
                                    bid = d;
                            }
                        }
                    }
                    else if (price.MDEntryType.Equals("1"))//ofr
                    {
                        if (string.IsNullOrEmpty(this._broker))
                        {
                            if ((ofr != null && ofr.Value.CompareTo(d) < 0) || ofr == null)
                                ofr = d;
                        }
                        else
                        {
                            if (price.Venues.Trim() == this._broker)
                            {
                                if ((ofr != null && ofr.Value.CompareTo(d) < 0) || ofr == null)
                                    ofr = d;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 得到初始行情最优价格
        /// </summary>
        /// <param name="isBid"></param>
        /// <returns></returns>
        public string GetInitBestPrice(bool isBid)
        {
            return isBid ? (_sendPriceItem.BestBidInt + _sendPriceItem.BestBidFloat) : (_sendPriceItem.BestOfrInt + _sendPriceItem.BestOfrFloat);
        }



        #region 渠道设置显示

        public List<ListBoxItemData> VenueToListBoxItemData(List<Venue> venues)
        {
            List<ListBoxItemData> result = null;

            if (venues != null && venues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (Venue uv in venues)
                {
                    ListBoxItemData itemData = new ListBoxItemData();
                    itemData.Code = uv.venue;
                    itemData.Name = uv.name;

                    if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                        itemData.Seq = int.Parse(SystemParamHandler.Instance.GetVenueSort(uv.venue));
                    else
                        itemData.Seq = uv.sn;

                    itemData.Level = uv.level;
                    itemData.Venuetypegw = uv.venuetypegw;

                    result.Add(itemData);

                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        public List<ListBoxItemData> UserVenueToListBoxItemData(List<UserVenue> userVenues, string level)
        {
            List<ListBoxItemData> result = null;

            if (userVenues != null && userVenues.Count > 0)
            {
                result = new List<ListBoxItemData>();
                foreach (UserVenue uv in userVenues)
                {
                    string[] arr = uv.selfselectvenues.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string str in arr)
                    {
                        ListBoxItemData itemData = new ListBoxItemData();
                        itemData.Code = str;
                        Venue venue = SystemParamHandler.Instance.GetVenueByCondition(uv.type, str);
                        if (venue != null)
                        {
                            itemData.Name = venue.name;
                            itemData.Seq = venue.sn;
                            itemData.Level = venue.level;
                            //itemData.SendPriceItem = sendPriceItem;
                            itemData.Venuetypegw = venue.venuetypegw;

                            if (string.IsNullOrEmpty(level) || (!string.IsNullOrEmpty(level) && itemData.Level.ToString().Equals(level)))
                                result.Add(itemData);
                        }
                    }
                }
                result.Sort(new ListBoxItemSort());
            }

            return result;
        }

        #endregion

        #region 清算速度逻辑
        //selectSettle:T+0:0, T+1:1
        public bool IsSpotSettle(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string now = DateTime.Now.ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                DateTime dtNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                DateTime dtSelect = new DateTime(selectDate.Value.Year, selectDate.Value.Month, selectDate.Value.Day);

                int days = (dtSelect - dtNow).Days;

                if ((days == 0 && (selectSettle == 0 || selectSettle == 1)) || (days == 1 && selectSettle == 0))
                    result = true;
            }

            return result;
        }

        public bool IsTomorrow(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                string now = DateTime.Now.AddDays(1).ToString("yyyyMMdd");
                string select = selectDate.Value.ToString("yyyyMMdd");

                string tem = DateTime.Now.ToString("yyyyMMdd");

                //明天+0=T+1
                if ((now.CompareTo(select) == 0 && selectSettle == 0) || (tem.CompareTo(select) == 0 && selectSettle == 1))
                    result = true;
                
            }

            return result;
        }

        public bool IsFwdSettle(DateTime? selectDate, int selectSettle)
        {
            bool result = false;
            if (selectDate != null && selectDate.Value != null)
            {
                TimeSpan ts = selectDate.Value - DateTime.Now;
                
                int days = ts.Days;

                if ((days == 1 || ts.TotalMinutes > 0) && selectSettle == 1)
                {
                    result = true;
                }
                else if ((days > 1) || (days == 1 && ts.TotalMinutes > 0))
                    result = true;
            }

            return result;
        }

        #endregion

        #region 获取界面设置渠道

        public string GetSelectItem(ListBox listBox)
        {
            string result = string.Empty;

            foreach (ListBoxItemData itemData in listBox.SelectedItems)
            {
                if (itemData != null)
                {
                    result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }

        //xbond报价时，指定渠道固定获取XBOND
        public string GetXBondVenue(ListBox listBox, string xbond)
        {
            string result = string.Empty;

            foreach (ListBoxItemData itemData in listBox.SelectedItems)
            {
                if (itemData != null && itemData.Name.ToUpper().Equals(xbond.ToUpper()))
                {
                    result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                }
            }

            return string.IsNullOrEmpty(result) ? result : result.Substring(0, result.Length - 1);
        }

        #endregion

        #region 获取参数
        #endregion

    }
}
