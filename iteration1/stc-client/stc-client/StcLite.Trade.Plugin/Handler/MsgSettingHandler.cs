using BDB.Price.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.Handler;
using com.gateway.log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Concurrent;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;
using STC.Common.Handler.MarketExec;
using STC.Common.Model.Common;
using STC.Common.View.UI;
using STC.Frame.UI;

namespace BDB.Price.Plugin.Handler
{
    public class MsgSettingHandler
    {
        public static string MSGSETTING_PARAM_KEY = "BDB_MSG_SETTING";

        private static MsgSettingHandler server = new MsgSettingHandler();
        public static MsgSettingHandler Instance { get { return server; } }

        private object _lock = new object();
        private ObservableCollection<MsgSettingItem> datas = new ObservableCollection<MsgSettingItem>();
        private ConcurrentDictionary<string, MsgSettingItem> _msgSettingDic = new ConcurrentDictionary<string, MsgSettingItem>();

        private ConcurrentDictionary<string, MarketDataSnapshotFullRefresh> oldPriceData_dic = new ConcurrentDictionary<string, MarketDataSnapshotFullRefresh>();

        private ConcurrentDictionary<string, BrokerInfo> oldBidBrokerInfoDic = new ConcurrentDictionary<string, BrokerInfo>();
        private ConcurrentDictionary<string, BrokerInfo> oldOfrBrokerInfoDic = new ConcurrentDictionary<string, BrokerInfo>();
        private ConcurrentDictionary<string, decimal?> oldBidT1TotalQtyDic = new ConcurrentDictionary<string, decimal?>();
        private ConcurrentDictionary<string, decimal?> oldOfrT1TotalQtyDic = new ConcurrentDictionary<string, decimal?>();
        // key: seucrityid; value: key:venueEn, value:BrokerInfo
        private ConcurrentDictionary<string, Dictionary<string, BrokerInfo>> _catch_BrokerInfo_dic = new ConcurrentDictionary<string, Dictionary<string, BrokerInfo>>();

        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _ProcessCtPriceThreadQueue;
        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;

        public MsgSettingItem GetMsgSettingItemBySecurtiyID(string securtiyID, bool isNew = false)
        {
            if (_msgSettingDic.ContainsKey(securtiyID))
            {
                return _msgSettingDic[securtiyID];
            }

            if (isNew)
            {
                return new Price.Plugin.ViewModel.Data.MsgSettingItem();
            }
            return null;
        }


        public MsgSettingHandler()
        {
            BaseHandler.Instance.Init(Instance_OnClear);

            if (_ProcessCtPriceThreadQueue == null)
            {
                _ProcessCtPriceThreadQueue = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDData);
            }
            if (_ProcessLastExecutionsThreadQueue == null)
            {
                _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info("MsgSettingHandler Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                lock (_lock)
                {
                    // 取消订阅行情
                    foreach (MsgSettingItem item in datas)
                    {
                        item.Clear();

                        PriceHandler.Instance.UnregisterCTPrice(item.SecurityID, Instance_OnMDPrice);
                    }

                    _ProcessCtPriceThreadQueue.Clear();
                    _ProcessLastExecutionsThreadQueue.Clear();

                    datas.Clear();
                    _msgSettingDic.Clear();
                    
                    oldPriceData_dic.Clear();
                    oldBidBrokerInfoDic.Clear();
                    oldOfrBrokerInfoDic.Clear();
                    oldBidT1TotalQtyDic.Clear();
                    oldOfrT1TotalQtyDic.Clear();
                    _catch_BrokerInfo_dic.Clear();
                }

                Init();
            }
        }

        public void Init()
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                SystemParameterReq req = new SystemParameterReq();
                req.ParamKey = MSGSETTING_PARAM_KEY;
                req.Username = LocalEnv.UserInfo.UserName;
                TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
                if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
                {
                    //InnerSettingViewModel.CopyInfoJson = reply.Datas.tbookSystemParameter.param_value;
                    string s = reply.Datas.tbookSystemParameter.param_value;
                    lock (_lock)
                    {
                        if (JsonSplit.IsJson(s))
                        {
                            datas = JsonConvert.DeserializeObject<ObservableCollection<MsgSettingItem>>(s);

                            foreach (MsgSettingItem item in datas)
                            {
                                item.Status = (item.IsBidLimit == "1" || item.IsOfrLimit == "1" || item.IsBestBp == "1" || item.IsBidPrice == "1" || item.IsOfrPrice == "1" || item.IsExecPrice == "1") ? "1" : "0";
                                _msgSettingDic[item.SecurityID] = item;
                            }
                        }
                    }
                }

                // 订阅行情
                foreach (MsgSettingItem item in datas)
                {
                    PriceHandler.Instance.RegisterCTPrice(item.SecurityID, Instance_OnMDPrice);
                }

                // 订阅成交
                MarketExecHandler.Instance.InitByBroker(Instance_OnLastExecutions, LocalEnv.UserInfo.UserName, PermisionHandler.Instance.GetBondPermission());
                //LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnLastExecutions;
            });
        }

        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (_ProcessCtPriceThreadQueue != null)
                _ProcessCtPriceThreadQueue.Enqueue(obj);
        }

        private void UpdateMDData(MarketDataSnapshotFullRefresh obj)
        {
            try
            {
                Sub_UpdateMDData(obj);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private void Sub_UpdateMDData(MarketDataSnapshotFullRefresh obj)
        {
            string securityID = obj.SecurityID;
            string symbolName = obj.Symbol;
            string remainingTime = string.Empty;
            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            
            if (bondsInfoPo != null)
            {
                remainingTime = bondsInfoPo.RemaingingTime;
                symbolName = bondsInfoPo.Short_Name;
            }

            GNoMDEntries oldBidBestPriceEntry = null;
            GNoMDEntries oldOfrBestPriceEntry = null;
            MarketDataSnapshotFullRefresh lastData;
            // 拿老数据
            if (oldPriceData_dic.ContainsKey(securityID) && oldPriceData_dic[securityID] != null)
            {
                lastData = oldPriceData_dic[securityID];
                var oldBestPrices = lastData.NoMDEntries.Where(p => p.MDPriceLevel == "1");
                foreach (GNoMDEntries entry in oldBestPrices)
                {
                    if (entry.MDEntryType == "0") // bid
                    {
                        oldBidBestPriceEntry = entry;
                    }
                    else if (entry.MDEntryType == "1") // ofr
                    {
                        oldOfrBestPriceEntry = entry;
                    }
                }
            }
           

            // 新建数据
            Dictionary<string, BrokerInfo> newData = new Dictionary<string, BrokerInfo>();
            _catch_BrokerInfo_dic[securityID] = newData;

            // 最优价
            GNoMDEntries bidBestPriceEntry = null;
            GNoMDEntries ofrBestPriceEntry = null;
            var bestPrices = obj.NoMDEntries.Where(p => p.MDPriceLevel == "1");
            foreach (GNoMDEntries entry in bestPrices)
            {
                if (entry.MDEntryType == "0") // bid
                {
                    bidBestPriceEntry = entry;
                }
                else if (entry.MDEntryType == "1") // ofr
                {
                    ofrBestPriceEntry = entry;
                }
            }

            BrokerInfo bidItem_T1 = null;
            BrokerInfo ofrItem_T1 = null;
            decimal? bestBidPx_T1 = null;
            decimal? bestOfrPx_T1 = null;

            // 计算
            // 添加和更新数据(普通)
            foreach (GNoMDEntries entry in obj.NoMDEntries)
            {
                if (!(entry.PriceType == null || entry.PriceType == PriceType.Yield))
                    continue;

                string key = entry.Venues;
                string time = entry.MDEntryTime;
                string text = entry.Text;
                string price = CommonUtils.FormatNum(CommonUtils.GetShowPrice(entry));//CommonUtils.ConvertToString(entry.MDEntryPx);
                decimal qty = CommonUtils.ConvertToDecimal(entry.MDEntrySize);
                string settlType = entry.SettlType;
                
                if (!newData.ContainsKey(key))
                {
                    newData[key] = new BrokerInfo();
                }
                BrokerInfo currnetItem = newData[key];

                
                if (newData != null)
                {
                    string[] settleArr = settlType.Split(',');

                    currnetItem.IsPermission = PermisionHandler.Instance.CheckTradePermission(entry.Venues);
                    if (entry.MDEntryType == CommonConstants.SIDE_BID)
                    {
                        if (settleArr.Contains("2") || settleArr.Contains("3"))
                        {
                            if (bestBidPx_T1 == null)
                            {
                                bestBidPx_T1 = price.ConvertToDecimal();
                            }
                            else
                            {
                                if (bestBidPx_T1 > price.ConvertToDecimal())
                                {
                                    bestBidPx_T1 = price.ConvertToDecimal();
                                }
                            }
                            currnetItem.BidT1Qty += qty;

                            string result_T1 = CalCornerMarket(currnetItem.BidCornerMarketText_T1, qty, text);
                            currnetItem.BidCornerMarketText_T1 = result_T1;
                        }

                        string result = CalCornerMarket(currnetItem.BidCornerMarketText, qty, text);
                        currnetItem.BidCornerMarketText = result;

                        decimal localQty = CommonUtils.ConvertToDecimal(currnetItem.BidAmount);
                        qty = localQty + qty;
                        currnetItem.BidAmount = qty == 0 ? CommonConstants.NO_PRICE : qty.ToString();

                        currnetItem.Bid = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
                    }
                    else if (entry.MDEntryType == CommonConstants.SIDE_OFR)
                    {
                        if (settleArr.Contains("2") || settleArr.Contains("3"))
                        {
                            if (bestOfrPx_T1 == null)
                            {
                                bestOfrPx_T1 = price.ConvertToDecimal();
                            }
                            else
                            {
                                if (bestOfrPx_T1 < price.ConvertToDecimal())
                                {
                                    bestOfrPx_T1 = price.ConvertToDecimal();
                                }
                            }

                            currnetItem.OfrT1Qty += qty;

                            string result_T1 = CalCornerMarket(currnetItem.OfrCornerMarketText_T1, qty, text);
                            currnetItem.OfrCornerMarketText_T1 = result_T1;
                        }

                        string result = CalCornerMarket(currnetItem.OfrCornerMarketText, qty, text);
                        currnetItem.OfrCornerMarketText = result;

                        decimal localQty = CommonUtils.ConvertToDecimal(currnetItem.OfrAmount);
                        qty = localQty + qty;
                        currnetItem.OfrAmount = qty == 0 ? CommonConstants.NO_PRICE : qty.ToString();
                            
                        currnetItem.Ofr = string.IsNullOrEmpty(price) ? CommonConstants.NO_PRICE : price;
                    }
                    //}
                    //else
                    //{
                    //    currnetItem.Bid = currnetItem.Ofr = currnetItem.BidAmount = currnetItem.OfrAmount = CommonConstants.NO_BROKER;
                    //}

                    currnetItem.UpdateTime = time;
                }
            }

            StringBuilder bidQtyAndSettle_T1 = new StringBuilder();
            StringBuilder ofrQtyAndSettle_T1 = new StringBuilder();
            decimal totalBidQty_T1 = 0;
            decimal totalOfrQty_T1 = 0;
            foreach (BrokerInfo currentItem in newData.Values)
            {
                // T + 1
                if (bestBidPx_T1.HasValue && currentItem.Bid.ConvertToDecimal() == bestBidPx_T1)
                {
                    if (currentItem.BidT1Qty > 0)
                    {
                        // 量最大的T1渠道
                        if (bidItem_T1 == null)
                        {
                            bidItem_T1 = currentItem;
                        }
                        else if (bidItem_T1.BidT1Qty < currentItem.BidT1Qty)
                        {
                            bidItem_T1 = currentItem;
                        }


                        if (bidQtyAndSettle_T1.Length > 0)
                        {
                            bidQtyAndSettle_T1.Append(" + ");
                        }

                        if (currentItem.BidCornerMarketText_T1.Length > 0)
                        {
                            bidQtyAndSettle_T1.Append($"[{currentItem.BidCornerMarketText_T1}]");
                        }
                        else
                        {
                            bidQtyAndSettle_T1.Append($"{currentItem.BidT1Qty}");
                        }

                        totalBidQty_T1 += currentItem.BidT1Qty;
                    }
                }

                // T + 1
                if (bestOfrPx_T1.HasValue && currentItem.Ofr.ConvertToDecimal() == bestOfrPx_T1)
                {
                    if (currentItem.OfrT1Qty > 0)
                    {
                        // 量最大的T1渠道
                        if (ofrItem_T1 == null)
                        {
                            ofrItem_T1 = currentItem;
                        }
                        else if (ofrItem_T1.OfrT1Qty < currentItem.OfrT1Qty)
                        {
                            ofrItem_T1 = currentItem;
                        }

                        if (ofrQtyAndSettle_T1.Length > 0)
                        {
                            ofrQtyAndSettle_T1.Append(" + ");
                        }

                        if (currentItem.OfrCornerMarketText_T1.Length > 0)
                        {
                            ofrQtyAndSettle_T1.Append($"[{currentItem.OfrCornerMarketText_T1}]");
                        }
                        else
                        {
                            ofrQtyAndSettle_T1.Append($"{currentItem.OfrT1Qty}");
                        }

                        totalOfrQty_T1 += currentItem.OfrT1Qty;
                    }
                }
            }


            //string bestBidPx = (bidBestPriceEntry == null || string.IsNullOrEmpty(bidBestPriceEntry.MDEntryPx)) ? "- -" : bidBestPriceEntry.MDEntryPx;
            //string bestOfrPx = (ofrBestPriceEntry == null || string.IsNullOrEmpty(ofrBestPriceEntry.MDEntryPx)) ? "- -" : ofrBestPriceEntry.MDEntryPx;
            //string t1BidQty = (bidItem_T1 == null || bidItem_T1.BidT1Qty == 0) ? "- -" : bidItem_T1.BidT1Qty.ToString();//T1最优价T1量最大的渠道
            //string t1OfrQty = (ofrItem_T1 == null || ofrItem_T1.OfrT1Qty == 0) ? "- -" : ofrItem_T1.OfrT1Qty.ToString();//T1最优中T1量最大的渠道

            string bestBidPx_T1_Display = bestBidPx_T1.HasValue == false ? "- -" : CommonUtils.FormatNum(bestBidPx_T1.Value.ToString());
            string bestOfrPx_T1_Display = bestOfrPx_T1.HasValue == false ? "- -" : CommonUtils.FormatNum(bestOfrPx_T1.Value.ToString());
            string bestBidQty_T1 = totalBidQty_T1 > 0 ? totalBidQty_T1.ToString() : "- -";// T1最优价的总量
            string bestOfrQty_T1 = totalOfrQty_T1 > 0 ? totalOfrQty_T1.ToString() : "- -";// T1最优价的总量

            if (_msgSettingDic.ContainsKey(securityID))
            {
                MsgSettingItem item = _msgSettingDic[securityID];

                if (item.IsBidLimit == "1")
                {
                    string oldBidPxT1 = string.Empty;
                    decimal? oldBidQtyT1 = null;
                    if (oldBidBrokerInfoDic.ContainsKey(securityID) && oldBidBrokerInfoDic[securityID] != null)
                    {
                        oldBidPxT1 = oldBidBrokerInfoDic[securityID].Bid;
                    }
                    if (oldBidT1TotalQtyDic.ContainsKey(securityID) && oldBidT1TotalQtyDic[securityID] != null)
                    {
                        oldBidQtyT1 = oldBidT1TotalQtyDic[securityID];
                    }

                    if (bidItem_T1 != null && (oldBidPxT1 != bidItem_T1.Bid || oldBidQtyT1 != totalBidQty_T1) && bidItem_T1.Bid.ConvertToDecimal() <= item.BestBidLimit.ConvertToDecimal())
                    {
                        string id = securityID + "_BestBidLimit";
                        string title = $"最优Bid提醒 ";
                        //string content = 
                        string s = "GVN";

                        StringBuilder html = null;
                        StringBuilder plainTxt = null;
                        string qty = bidItem_T1.BidT1Qty > 0 ? bidItem_T1.BidT1Qty.ToString() : string.Empty;
                        BestDoubleCopy(s, securityID, symbolName, bidItem_T1.Bid, qty, out html, out plainTxt);
                        string display = $"{remainingTime} {securityID} {symbolName}\r\n{bestBidPx_T1_Display}/{bestOfrPx_T1_Display} {bestBidQty_T1}/{bestOfrQty_T1}";
                        PublishSystemMsg(id, securityID, title, plainTxt.ToString(), display);
                        item.Status = "2";
                    }
                }

                if (item.IsOfrLimit == "1")
                {
                    string oldOfrPxT1 = string.Empty;
                    decimal? oldOfrQtyT1 = null;
                    if (oldOfrBrokerInfoDic.ContainsKey(securityID) && oldOfrBrokerInfoDic[securityID] != null)
                    {
                        oldOfrPxT1 = oldOfrBrokerInfoDic[securityID].Ofr;
                    }
                    if (oldOfrT1TotalQtyDic.ContainsKey(securityID) && oldOfrT1TotalQtyDic[securityID] != null)
                    {
                        oldOfrQtyT1 = oldOfrT1TotalQtyDic[securityID];
                    }

                    if (ofrItem_T1 != null && (oldOfrPxT1 != ofrItem_T1.Ofr || oldOfrQtyT1 != totalOfrQty_T1) && ofrItem_T1.Ofr.ConvertToDecimal() >= item.BestOfrLimit.ConvertToDecimal())
                    {
                        string id = securityID + "_BestOfrLimit";
                        string title = $"最优Ofr提醒";
                        //string content = 
                        string s = "TKN";

                        StringBuilder html = null;
                        StringBuilder plainTxt = null;
                        string qty = ofrItem_T1.OfrT1Qty > 0 ? ofrItem_T1.OfrT1Qty.ToString() : string.Empty;
                        BestDoubleCopy(s, securityID, symbolName, ofrItem_T1.Ofr, qty, out html, out plainTxt);
                        string display = $"{remainingTime} {securityID} {symbolName}\r\n{bestBidPx_T1_Display}/{bestOfrPx_T1_Display} {bestBidQty_T1}/{bestOfrQty_T1}";
                        PublishSystemMsg(id, securityID, title, plainTxt.ToString(), display);
                        item.Status = "2";
                    }
                }

                if (item.IsBestBp == "1")
                {
                    decimal? oldBp = null; 
                    if (oldBidBrokerInfoDic.ContainsKey(securityID) && oldBidBrokerInfoDic[securityID] != null && oldOfrBrokerInfoDic.ContainsKey(securityID) && oldOfrBrokerInfoDic[securityID] != null)
                    {
                        oldBp = (oldBidBrokerInfoDic[securityID].Bid.ConvertToDecimal() - oldOfrBrokerInfoDic[securityID].Ofr.ConvertToDecimal()) * 100;
                    }

                    if (bestBidPx_T1.HasValue && bestOfrPx_T1.HasValue)
                    {
                        decimal bp = (bestBidPx_T1.Value - bestOfrPx_T1.Value) * 100;

                        if ((!oldBp.HasValue || oldBp.Value != bp) && bp <= item.BestBp.ConvertToDecimal())
                        {
                            string id = securityID + "_BestBp";
                            string title = $"最优价差提醒";

                            StringBuilder html = null;
                            StringBuilder plainTxt = null;

                            BestSingleCopy(securityID, symbolName, remainingTime, bestBidPx_T1_Display, bestOfrPx_T1_Display, bidQtyAndSettle_T1.ToString(), ofrQtyAndSettle_T1.ToString(), out html, out plainTxt);
                            string display = $"{remainingTime} {securityID} {symbolName}\r\n{bestBidPx_T1_Display}/{bestOfrPx_T1_Display} {bestBidQty_T1}/{bestOfrQty_T1}";
                            PublishSystemMsg(id, securityID, title, plainTxt.ToString(), display);
                            item.Status = "2";
                        }
                    }
                }

                if (item.IsBidPrice == "1")
                {
                    if (oldPriceData_dic.ContainsKey(securityID) && oldBidBestPriceEntry == null && bidBestPriceEntry != null && !string.IsNullOrEmpty(bidBestPriceEntry.MDEntryPx))
                    {
                        string v = bidBestPriceEntry.Venues;
                        if (newData.ContainsKey(v))
                        {
                            string id = securityID + "_BidPrice";
                            string title = $"出现Bid提醒";
                            BrokerInfo bi = newData[v];
                            StringBuilder html = null;
                            StringBuilder plainTxt = null;
                            string source = ComboSourceUtil.Instance.FindEnum(v, "venues_exe");
                            string bidPx = CommonConstants.NO_PRICE;
                            string ofrPx = CommonConstants.NO_PRICE;
                            string bidQty = CommonConstants.NO_PRICE;
                            string ofrQty = CommonConstants.NO_PRICE;
                            string bidQty_display = CommonConstants.NO_PRICE;
                            string ofrQty_display = CommonConstants.NO_PRICE;
                           // if (bi.IsPermission)
                            {
                                bidPx = string.IsNullOrEmpty(bi.Bid) ? "- -" : bi.Bid;
                                ofrPx = string.IsNullOrEmpty(bi.Ofr) ? "- -" : bi.Ofr;
                                
                                bidQty_display = string.IsNullOrEmpty(bi.BidAmount) ? "- -" : bi.BidAmount;
                                ofrQty_display = string.IsNullOrEmpty(bi.OfrAmount) ? "- -" : bi.OfrAmount;

                                bidQty = string.IsNullOrEmpty(bi.BidCornerMarketText) ? bidQty_display : bi.BidCornerMarketText;
                                ofrQty = string.IsNullOrEmpty(bi.OfrCornerMarketText) ? ofrQty_display : bi.OfrCornerMarketText;
                            }
                            
                            BrokerSingleCopy(securityID, symbolName, remainingTime, bidPx, ofrPx, bidQty, ofrQty, source, out html, out plainTxt);
                            string display = $"{remainingTime} {securityID} {symbolName}\r\n{bidPx}/{ofrPx} {bidQty_display}/{ofrQty_display}";
                            PublishSystemMsg(id, securityID, title, plainTxt.ToString(), display);
                            item.Status = "2";
                        }
                    }
                }

                if (item.IsOfrPrice == "1")
                {
                    if (oldPriceData_dic.ContainsKey(securityID) && oldOfrBestPriceEntry == null && ofrBestPriceEntry != null && !string.IsNullOrEmpty(ofrBestPriceEntry.MDEntryPx))
                    {
                        string v = bidBestPriceEntry.Venues;
                        if (newData.ContainsKey(v))
                        {
                            string id = securityID + "_OfrPrice";
                            string title = $"出现Ofr提醒";
                            BrokerInfo bi = newData[v];
                            StringBuilder html = null;
                            StringBuilder plainTxt = null;
                            string source = ComboSourceUtil.Instance.FindEnum(v, "venues_exe");
                            string bidPx = CommonConstants.NO_PRICE;
                            string ofrPx = CommonConstants.NO_PRICE;
                            string bidQty = CommonConstants.NO_PRICE;
                            string ofrQty = CommonConstants.NO_PRICE;
                            string bidQty_display = CommonConstants.NO_PRICE;
                            string ofrQty_display = CommonConstants.NO_PRICE;
                            //if (bi.IsPermission)
                            {
                                bidPx = string.IsNullOrEmpty(bi.Bid) ? "- -" : bi.Bid;
                                ofrPx = string.IsNullOrEmpty(bi.Ofr) ? "- -" : bi.Ofr;

                                bidQty_display = string.IsNullOrEmpty(bi.BidAmount) ? "- -" : bi.BidAmount;
                                ofrQty_display = string.IsNullOrEmpty(bi.OfrAmount) ? "- -" : bi.OfrAmount;

                                bidQty = string.IsNullOrEmpty(bi.BidCornerMarketText) ? bidQty_display : bi.BidCornerMarketText;
                                ofrQty = string.IsNullOrEmpty(bi.OfrCornerMarketText) ? ofrQty_display : bi.OfrCornerMarketText;
                            }
                            BrokerSingleCopy(securityID, symbolName, remainingTime, bidPx, ofrPx, bidQty, ofrQty, source, out html, out plainTxt);
                            string display = $"{remainingTime} {securityID} {symbolName}\r\n{bidPx}/{ofrPx} {bidQty_display}/{ofrQty_display}";
                            PublishSystemMsg(id, securityID, title, plainTxt.ToString(), display);
                            item.Status = "2";
                        }
                    }
                }
            }

            oldPriceData_dic[securityID] = obj;
            oldBidBrokerInfoDic[securityID] = bidItem_T1;
            oldOfrBrokerInfoDic[securityID] = ofrItem_T1;
            oldBidT1TotalQtyDic[securityID] = totalBidQty_T1;
            oldOfrT1TotalQtyDic[securityID] = totalOfrQty_T1;
        }

        private void PublishSystemMsg(string id, string securtityid, string title, string content, string display = null)
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                TMessageContent messageContent = new TMessageContent();
                messageContent.content = display;
                messageContent.title = title;
                messageContent.type = "N";//紧急程度 C-紧急 A-重要 N-一般
                messageContent.id = id;//"BDB";
                messageContent.contentType = "BDB";
                messageContent.securityid = securtityid;
                messageContent.remark = content;

                TTbookSystemMessage tbookSystemMessage = new TTbookSystemMessage();
                tbookSystemMessage.type = "1";//  消息类型 1-消息通知,2-待办事项
                tbookSystemMessage.closeby = "BDB";
                tbookSystemMessage.readflag = "";
                tbookSystemMessage.delflag = "0";
                tbookSystemMessage.id = id;
                tbookSystemMessage.username = LocalEnv.UserInfo.UserName;
                tbookSystemMessage.content = JsonConvert.SerializeObject(messageContent);
                tbookSystemMessage.createtime = tbookSystemMessage.updatetime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

                if (content != null)
                {
                    Dictionary<string, object> detail = new Dictionary<string, object>();

                    detail.Add("eventname", "clipboard");

                    if (content != null)
                    {
                        detail.Add("data", content);
                    }

                    tbookSystemMessage.detail = JsonConvert.SerializeObject(detail);
                }


                //string cont = JsonConvert.SerializeObject(tbookSystemMessage);
                //Excel
                //ThreadPool.QueueUserWorkItem(GatewayHelper.systemMessage, cont);
                
                //// 发送到服务器
                //string result = AdminHandler.Instance.requestSystemMsg(cont);

                BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "MsgSetting", Msg = Newtonsoft.Json.JsonConvert.SerializeObject(new List<TTbookSystemMessage>() { tbookSystemMessage }) });
            });
        }

        private string CalCornerMarket(string oldTxt, decimal qty, string text)
        {
            string newText = qty.ToString();
            if (!string.IsNullOrEmpty(text))
                newText = string.Format("{0}", text);//string.Format("{0}({1})", newText, text);

            string oldText = oldTxt;

            if (!string.IsNullOrEmpty(oldText))
            {
                if (!string.IsNullOrEmpty(newText))
                    newText = string.Format("{0}+{1}", oldText, newText);
                else
                    newText = oldText;
            }

            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(oldText))
                return string.Empty;

            return newText;

        }

        #region 最新成交

        private void Instance_OnLastExecutions(ExecutionReport obj)
        {
            if (obj != null && _ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Enqueue(obj);
            }
        }

        private void UpdateLastExecutionsAsync(ExecutionReport obj)
        {
            StringBuilder html = null;
            StringBuilder plainTxt = null;
            string securityID = obj.SecurityID;
            string symbol = obj.Symbol;
            string remainingTime = string.Empty;
            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            if (bondsInfoPo != null)
            {
                remainingTime = bondsInfoPo.RemaingingTime;
            }
            string source = ComboSourceUtil.Instance.FindEnum(obj.Venues, "venues_exe");
            string side = ComboSourceUtil.Instance.FindEnum(obj.Side, "side");
            string px = CommonUtils.FormatPrice(obj.LastPx);
            string time = CommonUtils.ConvertToDateTime(obj.TransactTime).ToString("HH:mm:ss");
            if (_msgSettingDic.ContainsKey(securityID))
            {
                MsgSettingItem item = _msgSettingDic[securityID];

                if (item.IsExecPrice == "1")
                {
                    string id = obj.ExecID + "_ExecPrice";
                    string title = "成交提醒";
                    ExecCopy(remainingTime, securityID, symbol, source, side, px, time, out html, out plainTxt);
                    PublishSystemMsg(id, securityID, title, plainTxt.ToString(), plainTxt.ToString());
                    item.Status = "2";
                }
            }
        }

        #endregion

        #region 保存当前用户的设置

        private List<MsgSettingItem> GetSaveDatas(MsgSettingItem item, string saveTime, bool isDel)
        {
            List<MsgSettingItem> saveList = this.datas.ToList();
            if (isDel)
            {
                IEnumerable<MsgSettingItem> findDatas = saveList.Where(x => x.SecurityID == item.SecurityID);
                if (findDatas.Count() > 0)
                {
                    saveList.Remove(findDatas.First());
                }
            }
            else
            {
                IEnumerable<MsgSettingItem> findDatas = saveList.Where(x => x.SecurityID == item.SecurityID);
                if (findDatas.Count() > 0)
                {
                    MsgSettingItem data = findDatas.First();
                    data.IsBidLimit = data.IsBidLimit_Edit;
                    data.BestBidLimit = data.BestBidLimit_Edit;
                    data.IsOfrLimit = data.IsOfrLimit_Edit;
                    data.BestOfrLimit = data.BestOfrLimit_Edit;
                    data.IsBestBp = data.IsBestBp_Edit;
                    data.BestBp = data.BestBp_Edit;
                    data.IsBidPrice = data.IsBidPrice_Edit;
                    data.IsOfrPrice = data.IsOfrPrice_Edit;
                    data.IsExecPrice = data.IsExecPrice_Edit;
                    item.UpdateTime = saveTime;
                }
                else
                {
                    item.IsBidLimit = item.IsBidLimit_Edit;
                    item.BestBidLimit = item.BestBidLimit_Edit;
                    item.IsOfrLimit = item.IsOfrLimit_Edit;
                    item.BestOfrLimit = item.BestOfrLimit_Edit;
                    item.IsBestBp = item.IsBestBp_Edit;
                    item.BestBp = item.BestBp_Edit;
                    item.IsBidPrice = item.IsBidPrice_Edit;
                    item.IsOfrPrice = item.IsOfrPrice_Edit;
                    item.IsExecPrice = item.IsExecPrice_Edit;
                    item.UpdateTime = saveTime;

                    saveList.Add(item);
                }
            }

            return saveList;
        }
        public Task<bool> DeleteAllDatas(bool isShowDiaglog = true)
        {
            return Task.Run(() =>
            {
                SystemGlobalParamReq req = new SystemGlobalParamReq();
                req.action = "2";
                req.list = new List<TTbookSystemParameter>();
                TTbookSystemParameter param = new TTbookSystemParameter();
                param.param_key = MSGSETTING_PARAM_KEY;
                param.username = LocalEnv.UserInfo.UserName;
                param.param_value = string.Empty;
                param.param_desc = "消息提醒设置";
                req.list.Add(param);

                PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        lock (_lock)
                        {
                            // 取消订阅行情
                            foreach (MsgSettingItem item in datas)
                            {
                                item.Clear();

                                PriceHandler.Instance.UnregisterCTPrice(item.SecurityID, Instance_OnMDPrice);
                            }

                            this.datas.Clear();

                            _msgSettingDic.Clear();
                        }

                        if (isShowDiaglog)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBoxGUI22 msgBox = new MessageBoxGUI22();
                                msgBox.setWarnMsg("保存成功", true);
                                msgBox.ShowDialog();
                            });
                            return true;
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBoxGUI22 msgBox = new MessageBoxGUI22();
                            msgBox.setWarnMsg(string.Format($"更新失败, 原因:{reply.Msg}."), true);
                            msgBox.ShowDialog();
                        });
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxGUI22 msgBox = new MessageBoxGUI22();
                        msgBox.setWarnMsg(string.Format($"系统异常"), true);
                        msgBox.ShowDialog();
                    });
                }


                return false;
            });
        }


        /// <summary>
        /// 保存
        /// </summary>
        public Task<bool> SaveGlobalPara(MsgSettingItem item, bool isDel, bool isShowDiaglog = true)
        {
            return Task.Run(() =>
            {
                string saveTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                List<MsgSettingItem> saveList = GetSaveDatas(item, saveTime, isDel);

                string json = JsonConvert.SerializeObject(saveList);
                SystemGlobalParamReq req = new SystemGlobalParamReq();
                req.action = "2";
                req.list = new List<TTbookSystemParameter>();
                TTbookSystemParameter param = new TTbookSystemParameter();
                param.param_key = MSGSETTING_PARAM_KEY;
                param.username = LocalEnv.UserInfo.UserName;
                param.param_value = json;
                param.param_desc = "消息提醒设置";
                req.list.Add(param);

                PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
                if (reply != null)
                {
                    if (reply.Code == "0000")
                    {
                        lock (_lock)
                        {
                            if (isDel)
                            {
                                item.Clear();

                                this.datas.Remove(item);

                                MsgSettingItem removeItem;
                                _msgSettingDic.TryRemove(item.SecurityID, out removeItem);

                                PriceHandler.Instance.UnregisterCTPrice(item.SecurityID, Instance_OnMDPrice);
                            }
                            else
                            {
                                if (this.datas.Contains(item))
                                {
                                    item.IsBidLimit = item.IsBidLimit_Edit;
                                    item.BestBidLimit = item.BestBidLimit_Edit;
                                    item.IsOfrLimit = item.IsOfrLimit_Edit;
                                    item.BestOfrLimit = item.BestOfrLimit_Edit;
                                    item.IsBestBp = item.IsBestBp_Edit;
                                    item.BestBp = item.BestBp_Edit;
                                    item.IsBidPrice = item.IsBidPrice_Edit;
                                    item.IsOfrPrice = item.IsOfrPrice_Edit;
                                    item.IsExecPrice = item.IsExecPrice_Edit;
                                    item.UpdateTime = saveTime;

                                }
                                else
                                {
                                    this.datas.Add(item);

                                    _msgSettingDic[item.SecurityID] = item;

                                    PriceHandler.Instance.RegisterCTPrice(item.SecurityID, Instance_OnMDPrice);
                                }

                                item.Status = (item.IsBidLimit == "1" || item.IsOfrLimit == "1" || item.IsBestBp == "1" || item.IsBidPrice == "1" || item.IsOfrPrice == "1" || item.IsExecPrice == "1") ? "1" : "0";
                            }
                        }

                        if (isShowDiaglog)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBoxGUI22 msgBox = new MessageBoxGUI22();
                                msgBox.setWarnMsg("保存成功", true);
                                msgBox.ShowDialog();
                            });
                            return true;
                        }
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBoxGUI22 msgBox = new MessageBoxGUI22();
                            msgBox.setWarnMsg(string.Format($"更新失败, 原因:{reply.Msg}."), true);
                            msgBox.ShowDialog();
                        });
                    }
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxGUI22 msgBox = new MessageBoxGUI22();
                        msgBox.setWarnMsg(string.Format($"系统异常"), true);
                        msgBox.ShowDialog();
                    });
                }

                //Application.Current.Dispatcher.Invoke(() =>
                //{
                //    Common.Data.DesignHandler.Instance.SettingChange(new Model.MM.NewSettingMsg() { key = "MessageBellPopupStyle", value = PopMsgAlert ? "1" : "0" });
                //});
                return false;
            });
        }

        #endregion

        #region 复制方法
        public static void BestDoubleCopy(string s, string securityId, string symbolName, string px, string qty, out StringBuilder html, out StringBuilder plainTxt)
        {
            html = new StringBuilder();
            plainTxt = new StringBuilder();

            string htmlPx = string.Empty;
            string txtPx = string.Empty;
            if (px.Length > 0)
            {
                htmlPx = $"&#9;{px}";
                txtPx = $" {px}";
            }

            string htmlQty = string.Empty;
            string txtQty = string.Empty;
            if (qty.Length > 0)
            {
                htmlQty = $"&#9;{qty}w";
                txtQty = $" {qty}w";
            }

            html.Append($"<p>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>");

            plainTxt.Append($"{s} {securityId} {symbolName}{txtPx}{txtQty}");
        }

        public static void BestSingleCopy(string securityId, string symbolName, string remainingTime, string bidPx, string ofrPx, string bidQtyAndSettle, string ofrQtyAndSettle, out StringBuilder html, out StringBuilder plainTxt, string side = null)
        {
            html = new StringBuilder();
            plainTxt = new StringBuilder();

            string info = string.Empty;
            if (side == "1")
            {
                string s = "GVN";

                info = $",    {s} {securityId} {bidPx}";

                html.Append($"<p>[{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQtyAndSettle}/{ofrQtyAndSettle}]{info}</p>");

                plainTxt.Append($"[{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQtyAndSettle}/{ofrQtyAndSettle}]{info}");
            }
            else if (side == "2")
            {
                string s = "TKN";
                info = $",    {s} {securityId} {ofrPx}";

                html.Append($"<p>[{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQtyAndSettle}/{ofrQtyAndSettle}{info}]</p>");

                plainTxt.Append($"[{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQtyAndSettle}/{ofrQtyAndSettle}]{info}");
            }
            else
            {
                html.Append($"<p>{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQtyAndSettle}/{ofrQtyAndSettle}</p>");

                plainTxt.Append($"{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQtyAndSettle}/{ofrQtyAndSettle}");
            }
        }

        public static void BrokerDoubleCopy(string s, string securityId, string symbolName, string px, string qty, out StringBuilder html, out StringBuilder plainTxt)
        {
            html = new StringBuilder();
            plainTxt = new StringBuilder();

            string htmlPx = string.Empty;
            string txtPx = string.Empty;
            if (px.Length > 0)
            {
                htmlPx = $"&#9;{px}";
                txtPx = $" {px}";
            }

            string htmlQty = string.Empty;
            string txtQty = string.Empty;
            if (qty.Length > 0)
            {
                htmlQty = $"&#9;{qty}w";
                txtQty = $" {qty}w";
            }

            html.Append($"<p>{s}&#9;{securityId}&#9;{symbolName}{htmlPx}{htmlQty}</p>");

            plainTxt.Append($"{s} {securityId} {symbolName}{txtPx}{txtQty}");
        }

        public static void BrokerSingleCopy(string securityId, string symbolName, string remainingTime, string bidPx, string ofrPx, string bidQty, string ofrQty, string v, out StringBuilder html, out StringBuilder plainTxt, string side = null)
        {
            html = new StringBuilder();
            plainTxt = new StringBuilder();

            string htmlRemainingTime = string.Empty;
            string txtRemainingTime = string.Empty;
            if (remainingTime.Length > 0)
            {
                htmlRemainingTime = $"{remainingTime}&#9;";
                txtRemainingTime = $"{remainingTime}    ";
            }

            string htmlSymbolName = string.Empty;
            string txtSymbolName = string.Empty;
            if (symbolName.Length > 0)
            {
                htmlSymbolName = $"{symbolName}&#9;";
                txtSymbolName = $"{symbolName}    ";
            }

            string info = string.Empty;
            if (side == "1")
            {
                string s = "GVN";
                info = $",    {s} {securityId} {bidPx}";

                html.Append($"<p>[{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQty}/{ofrQty}]{info}</p>");

                plainTxt.Append($"[{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQty}/{ofrQty}]{info}");
            }
            else if (side == "2")
            {
                string s = "TKN";
                info = $",    {s} {securityId} {ofrPx}";

                html.Append($"<p>[{remainingTime}&#9;{securityId}&#9;{symbolName}&#9;{bidPx}/{ofrPx}&#9;{bidQty}/{ofrQty}{info}]</p>");

                plainTxt.Append($"[{remainingTime}    {securityId}    {symbolName}    {bidPx}/{ofrPx}    {bidQty}/{ofrQty}]{info}");
            }
            else
            {
                html.Append($"<p>{htmlRemainingTime}{securityId}&#9;{htmlSymbolName}{bidPx}/{ofrPx}&#9;{bidQty}/{ofrQty}&#9;{v}</p>");

                plainTxt.Append($"{txtRemainingTime}{securityId}    {txtSymbolName}{bidPx}/{ofrPx}    {bidQty}/{ofrQty}    {v}");
            }
        }

        public static void ExecCopy(string remainingTime, string securityId, string symbo, string source, string side, string px, string t, out StringBuilder html, out StringBuilder plainTxt)
        {
            html = new StringBuilder();
            plainTxt = new StringBuilder();

            string htmlTime = string.Empty;
            string txtTime = string.Empty;
            if (t.Length > 0)
            {
                htmlTime = $"&#9;{t}";
                txtTime = $" {t}";
            }

            html.Append($"<p>{remainingTime}&#9;{securityId}&#9;{symbo}&#9;{px}&#9;{side}&#9;{source}{htmlTime}</p>");

            plainTxt.Append($"{remainingTime} {securityId} {symbo} {px} {side} {source}{txtTime}");
        }
        #endregion
    }
}
