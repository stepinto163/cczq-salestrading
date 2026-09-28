using STC.Common.Handler.Price;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using STC.Common.ViewModel.Order;
using System.Security.Cryptography;
using STC.Common.Model.Base;
using System.IO;
using static STC.Common.Model.Fix.Message.MarketDataSnapshotFullRefresh;

namespace STC.Common.ViewModel.Adapter
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
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                MDHandler.Instance.UnSubscribe(OnCtPrice, null);
            });
            
        }

        /// <summary>
        /// 获取缓存的深度行情数据
        /// </summary>
        private void GetCtPrice()
        {
            string topic = MDHandler.Instance.GetCtTopic(_sendPriceItem.Security);
            MarketDataSnapshotFullRefresh bestPrice = MDHandler.Instance.GetCachePriceByTopic(topic);
            ParseCtPrice(bestPrice);
            ParseCtPriceOuter(bestPrice);
            GetCTAmount(bestPrice);

            if (!string.IsNullOrEmpty(_broker))
                ParseCtBrokerPrice(bestPrice);
        }

        /// <summary>
        /// 接收深度行情数据回调方法
        /// </summary>
        /// <param name="bestPrice"></param>
        private void OnCtPrice(MarketDataSnapshotFullRefresh bestPrice)
        {
            Task.Run(() =>
            {
                ParseCtPrice(bestPrice);
            });

            Task.Run(() =>
            {
                ParseCtPriceOuter(bestPrice);
                GetCTAmount(bestPrice);

                if (OnMDPrice != null)
                {
                    OnMDPrice?.Invoke(bestPrice);
                }
            });

            if (!string.IsNullOrEmpty(_broker))
                ParseCtBrokerPrice(bestPrice);


        }

        /// <summary>
        /// 计算最优量
        /// </summary>
        /// <param name="bestPrice"></param>
        private void GetCTAmount(MarketDataSnapshotFullRefresh ctPrice)
        {
            if (ctPrice == null || ctPrice.NoMDEntries == null || !ctPrice.SecurityID.Equals(_sendPriceItem.Security))
                return;

            decimal? bid = null;
            decimal? ofr = null;

            if (_settl != null)
            {
                string[] arrSettle = _settl.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (arrSettle != null)
                {
                    foreach (string settle in arrSettle)
                    {
                        decimal? temBid = null;
                        decimal? temOfr = null;

                        GetBestAmout(out temBid, out temOfr, settle, ctPrice);

                        if (bid == null)
                            bid = temBid;
                        else
                        {
                            if (temBid != null)
                                bid += temBid;
                        }

                        if (ofr == null)
                            ofr = temOfr;
                        else
                        {
                            if (temOfr != null)
                                ofr += temOfr;
                        }
                    }
                }
                else
                {
                    decimal? temBid = null;
                    decimal? temOfr = null;

                    GetBestAmout(out temBid, out temOfr, "", ctPrice);

                    if (bid == null)
                        bid = temBid;

                    if (ofr == null)
                        ofr = temOfr;
                }

            }
            if (bid == null)
            {
                _sendPriceItem.BestBidAmount = 0;
                _sendPriceItem.BestBidShowAmount = "";
            }
            else
            {
                _sendPriceItem.BestBidAmount = CommonUtils.ConvertToInt(bid.ToString());
                _sendPriceItem.BestBidShowAmount = $"({(_sendPriceItem.BestBidAmount/1000).ToString()}K)";
            }

            if (ofr == null)
            {
                _sendPriceItem.BestOfrAmount = 0;
                _sendPriceItem.BestOfrShowAmount = "";
            }
            else
            {
                _sendPriceItem.BestOfrAmount = CommonUtils.ConvertToInt(ofr.ToString());
                _sendPriceItem.BestOfrShowAmount = $"({(_sendPriceItem.BestOfrAmount / 1000).ToString()}K)";
            }
        }

        private void GetBestAmout(out decimal? bid, out decimal? ofr, string settle, MarketDataSnapshotFullRefresh ctPrice)
        {
            bid = null;
            ofr = null;

            foreach (GNoMDEntries price in ctPrice.NoMDEntries)
            {
                string[] arrSettle = price.SettlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                if (price.MDEntryType.Equals("0"))//bid
                {
                    if (_sendPriceItem.BestBid == null)
                    {
                        break;
                    }
                    else
                    {
                        decimal? temPx = price.PriceType == "3" ? CommonUtils.ConvertToDecimalOrNull(price.Yield) : CommonUtils.ConvertToDecimalOrNull(price.Yield);
                        string tem = temPx == null ? string.Empty : temPx.HasValue ? temPx.Value.ToString("F4") : string.Empty;
                        if (tem == _sendPriceItem.BestBid.ToString())
                        {
                            if (settle == "4")
                            {
                                if (price.SettlType.Contains(".") || price.SettlType == "4")
                                {
                                    if (bid == null)
                                    {
                                        bid = CommonUtils.ConvertToInt(price.MDEntrySize);
                                    }
                                    else
                                    {
                                        bid += CommonUtils.ConvertToInt(price.MDEntrySize);
                                    }
                                }
                            }
                            else if (arrSettle.Contains(settle))
                            {
                                if (bid == null)
                                {
                                    bid = CommonUtils.ConvertToInt(price.MDEntrySize);
                                }
                                else
                                {
                                    bid += CommonUtils.ConvertToInt(price.MDEntrySize);
                                }
                            }
                        }
                    }
                }
                else if (price.MDEntryType.Equals("1"))//ofr
                {
                    if (_sendPriceItem.BestOfr == null)
                    {
                        break;
                    }
                    else
                    {
                        decimal? temPx = price.PriceType == "3" ? CommonUtils.ConvertToDecimalOrNull(price.Yield) : CommonUtils.ConvertToDecimalOrNull(price.Yield);
                        string tem = temPx == null ? string.Empty : temPx.HasValue ? temPx.Value.ToString("F4") : string.Empty;
                        if (tem == _sendPriceItem.BestOfr.ToString())
                        {
                            if (settle == "4")
                            {
                                if (price.SettlType.Contains(".") || price.SettlType == "4")
                                {
                                    if (ofr == null)
                                    {
                                        ofr = CommonUtils.ConvertToInt(price.MDEntrySize);
                                    }
                                    else
                                    {
                                        ofr += CommonUtils.ConvertToInt(price.MDEntrySize);
                                    }
                                }
                            }
                            else if (arrSettle.Contains(settle))
                            {
                                if (ofr == null)
                                {
                                    ofr = CommonUtils.ConvertToInt(price.MDEntrySize);
                                }
                                else
                                {
                                    ofr += CommonUtils.ConvertToInt(price.MDEntrySize);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 得到最优价格
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="ofr"></param>
        /// <param name="settle"></param>
        /// <param name="ctPrice"></param>
        private void GetBestPrice(out decimal? bid, out decimal? ofr, string settle, MarketDataSnapshotFullRefresh ctPrice, bool isContainInner = true)
        {
            bid = null;
            ofr = null;

            decimal d = 0;
            foreach (GNoMDEntries price in ctPrice.NoMDEntries)
            {
                if (!isContainInner)
                {
                    if (price.VenueTypeGW.ToUpper() == "INNER")
                    {
                        continue;
                    }
                }

                string[] arrSettle = price.SettlType.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                //decimal? temPx = price.PriceType == "3" ? CommonUtils.ConvertToDecimalOrNull(price.Yield) : CommonUtils.ConvertToDecimalOrNull(price.Yield);
                decimal? temPx;
                if (!string.IsNullOrEmpty(price.YieldToExecution))
                {
                    temPx = CommonUtils.ConvertToDecimalOrNull(price.YieldToExecution);
                }
                else
                {
                    temPx = CommonUtils.ConvertToDecimalOrNull(price.Yield);
                }
                string tem = temPx == null ? string.Empty : temPx.HasValue ? temPx.Value.ToString("F4") : string.Empty;
                if (decimal.TryParse(tem, out d))
                {
                    if (price.MDEntryType.Equals("0"))//bid
                    {
                        if (ctPrice.MarketIndicator == "2")
                        {
                            //IRS 最优价是相反的
                            // 如果是远期
                            if (settle == "4")
                            {
                                if (price.SettlType.Contains(".") || price.SettlType == "4")
                                {
                                    if ((bid != null && bid.Value.CompareTo(d) < 0) || bid == null)
                                        bid = d;
                                }
                            }
                            else if (arrSettle.Contains(settle))
                            {
                                if ((bid != null && bid.Value.CompareTo(d) < 0) || bid == null)
                                    bid = d;
                            }
                        }
                        else
                        {
                            // 如果是远期
                            if (settle == "4")
                            {
                                if (price.SettlType.Contains(".") || price.SettlType == "4")
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
                    }
                    else if (price.MDEntryType.Equals("1"))//ofr
                    {
                        if (ctPrice.MarketIndicator == "2")
                        {
                            //IRS 最优价是相反的
                            // 如果是远期
                            if (settle == "4")
                            {
                                if (price.SettlType.Contains(".") || price.SettlType == "4")
                                {
                                    if ((ofr != null && ofr.Value.CompareTo(d) > 0) || ofr == null)
                                        ofr = d;
                                }
                            }
                            else if (arrSettle.Contains(settle))
                            {
                                if ((ofr != null && ofr.Value.CompareTo(d) > 0) || ofr == null)
                                    ofr = d;
                            }
                        }
                        else
                        {
                            // 如果是远期
                            if (settle == "4")
                            {
                                if (price.SettlType.Contains(".") || price.SettlType == "4")
                                {
                                    if ((ofr != null && ofr.Value.CompareTo(d) < 0) || ofr == null)
                                        ofr = d;
                                }
                            }
                            else if (arrSettle.Contains(settle))
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
        /// 得到国债期货涨停跌停价等信息
        /// </summary>
        /// <param name="bid"></param>
        /// <param name="ofr"></param>
        /// <param name="ctPrice"></param>
        private void GetTreasuryFuturePrice(SendPriceItem priceItem, MarketDataSnapshotFullRefresh ctPrice, int digit)
        {
            //最新价
            decimal? decPx = CommonUtils.ConvertToDecimalOrNull(ctPrice.LastPrice);
            if (decPx.HasValue)
            {
                //昨日结算价
                decimal? decSettlePx = CommonUtils.ConvertToDecimalOrNull(ctPrice.PreSettlePrice);
                //涨跌
                decimal? decDeltaPx = decimal.Subtract(decPx.Value, decSettlePx.Value);
                //涨停价
                decimal? decUpperLimitPx = CommonUtils.ConvertToDecimalOrNull(ctPrice.UpperLimitPrice);
                //跌停价
                decimal? decLowerLimitPx = CommonUtils.ConvertToDecimalOrNull(ctPrice.LowerLimitPrice);
                string strPx = decPx == null ? string.Empty : decPx.HasValue ? decPx.Value.ToString("F" + digit) : string.Empty;
                string strDelta = decDeltaPx == null ? string.Empty : decDeltaPx.HasValue ? decDeltaPx.Value.ToString("F" + digit) : string.Empty;
                string strUpperLimitPx = decUpperLimitPx == null ? string.Empty : decUpperLimitPx.HasValue ? decUpperLimitPx.Value.ToString("F" + digit) : string.Empty;
                string strLowerLimitPx = decLowerLimitPx == null ? string.Empty : decLowerLimitPx.HasValue ? decLowerLimitPx.Value.ToString("F" + digit) : string.Empty;
                //一档价格
                decimal d = 0;
                decimal? bid5 = null;
                decimal? ofr5 = null;
                foreach (GNoMDEntries price in ctPrice.NoMDEntries)
                {
                    decimal? temPx = CommonUtils.ConvertToDecimalOrNull(price.MDEntryPx);
                    string tem = temPx == null ? string.Empty : temPx.HasValue ? temPx.Value.ToString("F4") : string.Empty;
                    int amount = CommonUtils.ConvertToInt(price.TradeVolume);
                    if (decimal.TryParse(tem, out d))
                    {
                        string[] arr = tem.Split(new char[] { '.' });
                        if (price.MDEntryType.Equals("0"))          //bid
                        {
                            priceItem.BestBid = temPx;
                            priceItem.BestBidStr = temPx.Value.ToString("F" + digit);
                            priceItem.BestBidInt = string.Format("{0}.", arr[0]);
                            priceItem.BestBidFloat = arr[1];
                            priceItem.BestBidAmount = amount;
                            if ((bid5.HasValue && bid5.Value.CompareTo(d) > 0) || !bid5.HasValue)
                            {
                                bid5 = d;
                            }
                        }
                        else if (price.MDEntryType.Equals("1"))     //ofr
                        {
                            priceItem.BestOfr = temPx;
                            priceItem.BestOfrStr = temPx.Value.ToString("F" + digit);
                            priceItem.BestOfrInt = string.Format("{0}.", arr[0]);
                            priceItem.BestOfrFloat = arr[1];
                            priceItem.BestOfrAmount = amount;
                            if ((ofr5.HasValue && ofr5.Value.CompareTo(d) < 0) || !ofr5.HasValue)
                            {
                                ofr5 = d;
                            }
                            
                        }
                    }
                }
                priceItem.BestBid5 = bid5;
                priceItem.BestOfr5 = ofr5;
                priceItem.BestBid5Str = priceItem.BestBid5.HasValue ? priceItem.BestBid5.Value.ToString("F" + digit) : string.Empty;
                priceItem.BestOfr5Str = priceItem.BestOfr5.HasValue ? priceItem.BestOfr5.Value.ToString("F" + digit) : string.Empty;

                priceItem.LastPrice = decPx;
                priceItem.LastPriceStr = strPx;
                priceItem.DeltaLastPrice = decDeltaPx;
                priceItem.DeltaLastPriceStr = strDelta;
                priceItem.UpperLimitPrice = decUpperLimitPx;
                priceItem.UpperLimitPriceStr = strUpperLimitPx;
                priceItem.LowerLimitPrice = decLowerLimitPx;
                priceItem.LowerLimitPriceStr = strLowerLimitPx;
                //选择跟盘价
                if(priceItem.FollowingPrice)
                {
                    switch (priceItem.PriceType)
                    {
                        case 1://Bid对价
                            priceItem.CustomPrice = priceItem.BestOfrStr;
                            break;
                        case 2://Bid最新价
                            priceItem.CustomPrice = priceItem.LastPriceStr;
                            break;
                        case 3://Bid市价
                            priceItem.CustomPrice =  priceItem.UpperLimitPriceStr;
                            break;
                        case 4://Ofr对价
                            priceItem.CustomPrice = priceItem.BestBidStr;
                            break;
                        case 5://Ofr最新价
                            priceItem.CustomPrice = priceItem.LastPriceStr;
                            break;
                        case 6://Ofr市价
                            priceItem.CustomPrice = priceItem.LowerLimitPriceStr;
                            break;

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
                //如果是国债期货行情,获取涨跌停价等信息
                if (Consts.MARKETINDICATOR_FUTURE.Equals(ctPrice.MarketIndicator))
                {
                    GetTreasuryFuturePrice(_sendPriceItem, ctPrice, _sendPriceItem.Digit);
                }

                if (_settl != null)
                {
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
                }
                
                string bidStr = bid != null ? bid.Value.ToString() : string.Empty;
                string ofrStr = ofr != null ? ofr.Value.ToString() : string.Empty;
               
                if (!string.IsNullOrEmpty(bidStr))
                {
                    string[] arr = bidStr.Split(new char[] { '.' });
                    if (arr != null && arr.Length > 1)
                    {
                        _sendPriceItem.BestBid = CommonUtils.ConvertToDecimal(bidStr);
                        _sendPriceItem.BestBidShort = CommonUtils.FormatNum(bidStr);
                        _sendPriceItem.BestBidInt = string.Format("{0}.", arr[0]);
                        _sendPriceItem.BestBidFloat = arr[1];
                    }
                }
                else
                {
                    _sendPriceItem.BestBid = null;
                    _sendPriceItem.BestBidShort = null;
                    _sendPriceItem.BestBidInt = string.Empty;
                    _sendPriceItem.BestBidFloat = string.Empty;
                }

                if (!string.IsNullOrEmpty(ofrStr))
                {
                    string[] arr = ofrStr.Split(new char[] { '.' });
                    if (arr != null && arr.Length > 1)
                    {
                        _sendPriceItem.BestOfr = CommonUtils.ConvertToDecimal(ofrStr);
                        _sendPriceItem.BestOfrShort = CommonUtils.FormatNum(ofrStr);
                        _sendPriceItem.BestOfrInt = string.Format("{0}.", arr[0]);
                        _sendPriceItem.BestOfrFloat = arr[1];
                    }
                }
                else
                {
                    _sendPriceItem.BestOfr = null;
                    _sendPriceItem.BestOfrShort = null;
                    _sendPriceItem.BestOfrInt = string.Empty;
                    _sendPriceItem.BestOfrFloat = string.Empty;
                }
            }
        }

        /// <summary>
        /// 外部最优,不含INNER
        /// </summary>
        /// <param name="ctPrice"></param>
        private void ParseCtPriceOuter(MarketDataSnapshotFullRefresh ctPrice)
        {
            if (ctPrice == null || ctPrice.NoMDEntries == null || !ctPrice.SecurityID.Equals(_sendPriceItem.Security))
                return;

            lock (_lock)
            {
                decimal? bid = null;
                decimal? ofr = null;
                //string bid = string.Empty;
                //string ofr = string.Empty;
                _sendPriceItem.BestBidShortOuter = string.Empty;
                _sendPriceItem.BestOfrShortOuter = string.Empty;
                if (_settl != null)
                {
                    string[] arrSettle = _settl.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

                    if (arrSettle != null)
                    {
                        foreach (string settle in arrSettle)
                        {
                            decimal? temBid = null;
                            decimal? temOfr = null;

                            GetBestPrice(out temBid, out temOfr, settle, ctPrice, false);

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
                }

                string bidStr = bid != null ? bid.Value.ToString() : string.Empty;
                string ofrStr = ofr != null ? ofr.Value.ToString() : string.Empty;

                if (!string.IsNullOrEmpty(bidStr))
                {
                    string[] arr = bidStr.Split(new char[] { '.' });
                    if (arr != null && arr.Length > 1)
                    {
                        _sendPriceItem.BestBidShortOuter = CommonUtils.FormatNum(bidStr);
                    }
                }
                else
                {
                    _sendPriceItem.BestBidShortOuter = null;
                }

                if (!string.IsNullOrEmpty(ofrStr))
                {
                    string[] arr = ofrStr.Split(new char[] { '.' });
                    if (arr != null && arr.Length > 1)
                    {
                        _sendPriceItem.BestOfrShortOuter = CommonUtils.FormatNum(ofrStr);
                    }
                }
                else
                {
                    _sendPriceItem.BestOfrShortOuter = null;
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
                decimal? temPx = CommonUtils.ConvertToDecimalOrNull(price.Yield);
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

        /// <summary>
        /// 得到初始行情最优价格 - 商品期货根据最小价格变动单位决定显示位数
        /// </summary>
        /// <param name="priceType"></param>    跟盘价 1=Bid对价，2=Bid最新价，3=Bid市价，4=Ofr对价，5=Ofr最新价，6=Ofr市价
        /// <returns></returns>
        public string GetInitBestPrice3Decimal(int priceType, decimal minchange)
        {
            int digit = 4;
            switch (CommonUtils.ConvertToString(minchange))
            {
                case Consts.PRICE_MIN_CHANGE_0002:
                case Consts.PRICE_MIN_CHANGE_0005:
                    digit = 3;
                    break;
                case Consts.PRICE_MIN_CHANGE_002:
                case Consts.PRICE_MIN_CHANGE_005:
                    digit = 2;
                    break;
                case Consts.PRICE_MIN_CHANGE_01:
                case Consts.PRICE_MIN_CHANGE_02:
                case Consts.PRICE_MIN_CHANGE_05:
                    digit = 1;
                    break;
                case Consts.PRICE_MIN_CHANGE_1:
                case Consts.PRICE_MIN_CHANGE_2:
                case Consts.PRICE_MIN_CHANGE_5:
                case Consts.PRICE_MIN_CHANGE_10:
                    digit = 0;
                    break;
                default:
                    digit = 4;
                    break;
            }
            decimal price = decimal.Zero;
            switch (priceType)
            {
                case 1:
                    price = _sendPriceItem.BestOfr.HasValue ? _sendPriceItem.BestOfr.Value : decimal.Zero;
                    break;
                case 2:
                    price = _sendPriceItem.LastPrice.HasValue ? _sendPriceItem.LastPrice.Value : decimal.Zero;
                    break;
                case 3:
                    price = _sendPriceItem.UpperLimitPrice.HasValue ? _sendPriceItem.UpperLimitPrice.Value : decimal.Zero;
                    break;
                case 4:
                    price = _sendPriceItem.BestBid.HasValue ? _sendPriceItem.BestBid.Value : decimal.Zero;
                    break;
                case 5:
                    price = _sendPriceItem.LastPrice.HasValue ? _sendPriceItem.LastPrice.Value : decimal.Zero;
                    break;
                case 6:
                    price = _sendPriceItem.LowerLimitPrice.HasValue ? _sendPriceItem.LowerLimitPrice.Value : decimal.Zero;
                    break;
            }
            return price.ToString("F" + digit);
        }

        /// <summary>
        /// 得到初始行情最优价格 - 国债期货价格保留3位小数
        /// </summary>
        /// <param name="priceType"></param>    跟盘价 1=Bid对价，2=Bid最新价，3=Bid市价，4=Ofr对价，5=Ofr最新价，6=Ofr市价
        /// <returns></returns>
        public string GetInitBestPrice3Decimal(int priceType)
        {
            string price = string.Empty;
            switch (priceType)
            {
                case 1:
                    price = _sendPriceItem.BestOfrStr;
                    break;
                case 2:
                    price = _sendPriceItem.LastPriceStr;
                    break;
                case 3:
                    price = _sendPriceItem.UpperLimitPriceStr;
                    break;
                case 4:
                    price = _sendPriceItem.BestBidStr;
                    break;
                case 5:
                    price = _sendPriceItem.LastPriceStr;
                    break;
                case 6:
                    price = _sendPriceItem.LowerLimitPriceStr;
                    break;
            }
            return price;
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
                    itemData.ItemVisibility = uv.isvisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    if (LocalEnv.ConfInfo.IsUseVenueSort == "1")
                        itemData.Seq = int.Parse(SystemParamHandler.Instance.GetVenueSort(uv.venue));
                    else
                        itemData.Seq = uv.sn;

                    itemData.Level = uv.level;
                    itemData.Venuetypegw = uv.venuetypegw;
                    
                    if (uv.isvisible)
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
            if (listBox != null)
            {
                foreach (ListBoxItemData itemData in listBox.SelectedItems)
                {
                    if (itemData != null)
                    {
                        result += string.Format("{0}-{1},", itemData.Venuetypegw, itemData.Code);
                    }
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
