using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Data;
using STC.Common.Handler.Table;
using STC.Common.Model.Base;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class IntentionLogsViewModel : ViewModelBase
    {
        private IntentionQuoteItem _currentItem;

        public IntentionQuoteItem CurrentItem
        {
            get { return _currentItem; }
            set { _currentItem = value; }
        }

        protected object _workingLock = new object();
        private CollectionViewSource workingViewSource = new CollectionViewSource();
        public CollectionViewSource WorkingViewSource
        {
            get { return workingViewSource; }
            set { workingViewSource = value; }
        }

        private CustomCollection<IntentionQuoteItem> intentionQuoteInfo = new CustomCollection<IntentionQuoteItem>();

        public CustomCollection<IntentionQuoteItem> IntentionQuoteInfo
        {
            get { return intentionQuoteInfo; }
            set { intentionQuoteInfo = value; }
        }


        public IntentionLogsViewModel()
        {
            this.WorkingViewSource.IsLiveFilteringRequested = true;
            this.WorkingViewSource.IsLiveSortingRequested = true;
            this.WorkingViewSource.LiveFilteringProperties.Add("Status");
            this.WorkingViewSource.SortDescriptions.Clear();
            this.WorkingViewSource.SortDescriptions.Add(new SortDescription("CreateTime", ListSortDirection.Descending));
            this.WorkingViewSource.Source = IntentionQuoteInfo;

            BindingOperations.EnableCollectionSynchronization(this.WorkingViewSource.View, _workingLock);
        }

        internal void Query()
        {
            ThreadPoolHelper.InvokeThreadPool(o =>
            {
                if (CurrentItem != null)
                {
                    STC.Common.Model.Table.WorkBrench.IntentionQuoteReplyPo reply = TableOldHandler.Instance.GetBuyerIntentionLogs(CurrentItem.IntentionId);
                    if (reply != null && reply.IsSuccess())
                    {
                        Sub_Instance_OnHistoryIntentionQuotes(reply.Datas);
                    }
                }
            });
        }

        private void Sub_Instance_OnHistoryIntentionQuotes(List<Common.Model.Table.WorkBrench.IntentionQuoteInfo> datas)
        {
            LogHelper.Info("OnHistoryIntentionQuotes");

            int batchSize = 500;
            var syncLock = new object(); // 新增集合访问锁

            Task.Run(() =>
            {
                try
                {
                    // 获取数据快照
                    List<Common.Model.Table.WorkBrench.IntentionQuoteInfo> snapshot;
                    lock (datas)
                    {
                        snapshot = datas.OrderBy(entry => entry.updateTime).ToList(); // 线程安全拷贝
                    }

                    for (int batchIndex = 0; batchIndex < snapshot.Count; batchIndex += batchSize)
                    {
                        var currentBatch = snapshot.Skip(batchIndex).Take(batchSize).ToList();

                        // 异步更新UI（非阻塞）
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (var data in currentBatch)
                            {
                                OnIntentionQuotes(data);
                            }
                        }, System.Windows.Threading.DispatcherPriority.Send); // 调整优先级

                        // 优化CPU释放策略
                        if (batchIndex % (batchSize * 2) == 0 && batchIndex != 0)
                        {
                            Thread.Sleep(10);
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    // 处理窗口关闭时的正常终止
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex); // UI线程记录日志
                }
            });
        }

        //internal ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionQuoteItem>> _Cache_InnerOrders_Dic = new ConcurrentDictionary<string, ConcurrentDictionary<string, IntentionQuoteItem>>();

        private IntentionQuoteItem _lastItem;
        private void OnIntentionQuotes(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            lock (_workingLock)
            {
                IntentionQuoteItem item = new IntentionQuoteItem();

                item.IntentionId = obj.intentionId;
                item.InstitutionId = obj.institutionId;
                item.InstitutionName = obj.institutionName;
                item.TraderId = obj.traderId;
                item.TraderName = obj.traderName;
                item.BrokerId = obj.userName;
                item.BrokerName = obj.name;
                item.BrokerName_Edit = obj.name;
                item.RoleId = obj.roleid;
                item.RoleName = obj.rolename;
                item.QqNumber = obj.qqNumber;
                item.Side = obj.side;
                item.SecurityId = obj.securityId;
                item.Symbol = obj.symbol;
                item.CleanPrice = obj.cleanPrice;
                item.FullPrice = obj.fullPrice;
                item.Price = obj.price;
                item.Qty = obj.qty;
                item.ExecutionQty = obj.executionqty;
                item.SettlType = obj.settlType;
                //string tradeDate = CommonUtils.CalTradeDate(obj.settlType);
                //item.edit_settltype_date = CommonUtils.ConvertToDateTime2(tradeDate);
                //DateTime settlDateTime;
                //int selectIndex;
                //CommonUtils.SetSettleType(obj.settlType, tradeDate, out settlDateTime, out selectIndex);
                //item.edit_settletype = selectIndex;

                //item.BrokerId = obj.brokerId;
                //item.BrokerName = obj.brokerName;
                item.Text = obj.text;
                item.CreateTime = obj.createTime;
                item.UpdateTime = obj.updateTime;
                item.UserName = obj.userName;
                item.CloseBy = obj.closeBy;
                item.CloseByName = obj.closeByName;
                //item.Type = obj.type;
                item.Status = obj.status;
                item.WholeFlag = obj.wholeFlag;
                item.AccountID = obj.accountID;
                item.AccountName = obj.accountName;
                item.ifSubBond = obj.ifSubBond;
                item.vipCode = obj.vipCode;
                item.validUntilTime = obj.validuntitime;
                item.Info1 = obj.inf1;
                item.Info2 = obj.info2;
                item.Info5 = obj.info5;
                item.orgType = obj.orgType;


                if (obj.status == "2" || (obj.status == "1" && obj.userName != LocalEnv.UserInfo.UserName))
                {
                    item.IsShow = false;
                }
                else
                {
                    item.IsShow = true;
                }


                item.OrderID = obj.orderId;

                BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(obj.securityId);
                string marketindicator = "";
                if (bondsInfoPo != null)
                {
                    //Symbol = bondsInfoPo.Short_Name;
                    //Security = bondsInfoPo.Bond_ID;
                    //SymbolName = bondsInfoPo.Short_Name;
                    item.RemainingTime = bondsInfoPo.RemaingingTime;
                    item.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                    item.SecurityType = bondsInfoPo.SECURITY_TYPE;
                    item.BondSubType = bondsInfoPo.Bond_Subtype;
                    //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                    //findData.Issuer = bondsInfoPo.issuer;
                    item.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                    //findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    marketindicator = bondsInfoPo.MarketIndicator;
                }


                item.PriceType = obj.priceType;
                item.FirmFlag = obj.firmFlag;

                item.CustomerPrice = obj.customerPrice;
                item.BP = obj.customerSpread;
                item.HurryFlag = obj.hurryFlag;
                item.SuggestFlag = obj.suggestFlag;
                item.FireFlag = obj.fireFlag;
                item.FloorPrice = obj.floorPrice;
                item.OCOFlag = obj.OcoFlag;

                item.HurryFlag_Edit = item.HurryFlag;
                item.SuggestFlag_Edit = item.SuggestFlag;
                item.FireFlag_Edit = item.FireFlag;
                item.FloorPrice_Edit = item.FloorPrice;
                item.OCOFlag_Edit = item.OCOFlag;

                item.Channel = obj.channel;
                item.InquiryStatus = "0";
                if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
                {
                    item.FQInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                    if (item.FQInfo != null)
                    {
                        item.MatchedQty = item.FQInfo.MATCHED_QUANTITY.ConvertToLong();
                        item.InquiryStatus = string.IsNullOrEmpty(item.FQInfo.inquiryStatus) ? "0" : item.FQInfo.inquiryStatus;
                        item.IM = item.FQInfo.fromQQ;
                        item.ToGroupName = item.FQInfo.toGroupName;
                    }
                }

                item.IsMe = obj.userName == LocalEnv.UserInfo.UserName;
                item.Sign = obj.sign;
                item.Sign1 = obj.sign1;
                item.Sign2 = obj.sign2;
                item.Sign3 = obj.sign3;
                item.Sign4 = obj.sign4;
                item.TermDays = obj.termDays;
                item.CouponRateCurrent = obj.couponRateCurrent;
                item.IssuerCode = obj.issuerCode;
                item.IssuerName = string.IsNullOrEmpty(obj.issuerName) ? "--" : obj.issuerName;
                item.WarranterCode = obj.warranterCode;
                item.WarranterName = string.IsNullOrEmpty(obj.warranterName) ? "--" : obj.warranterName;
                item.ChinaBondValuation = obj.chinaBondValuation;
                item.ChinaBondValuationFull = obj.chinaBondValuationFull;
                item.CsiBondValuation = obj.csiBondValuation;
                item.CsiBondValuationFull = obj.csiBondValuationFull;
                if (SignParser.IsCumType(item.SignDic1))
                {
                    if (string.IsNullOrEmpty(obj.chinaBondValuationFull) == false)
                    {
                        int speIndex = obj.chinaBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            item.ChinaBondExercise = obj.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            item.ChinaBondDue = obj.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            item.ChinaBondDue = obj.chinaBondValuationFull.ConvertToDecimal();
                        }
                    }
                }
                else
                {
                    item.ChinaBondDue = obj.chinaBondValuation;

                }
                if (string.IsNullOrEmpty(obj.csiBondValuationFull) == false)
                {
                    int speIndex = obj.csiBondValuationFull.IndexOf('|');
                    if (speIndex > -1)
                    {
                        item.CsiBondValuationExercise = obj.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                        item.CsiBondValuationDue = obj.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                    }
                    else
                    {
                        item.CsiBondValuationDue = obj.csiBondValuationFull.ConvertToDecimal();
                    }
                }
                else
                {
                    item.CsiBondValuationDue = obj.csiBondValuation;
                }
                if (item.ChinaBondExercise.HasValue && item.Price.HasValue)
                {
                    if (item.Side == "1")
                        item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondExercise) * 100;
                    else if (item.Side == "2")
                        item.CCDCValuation_BP_InnerOfr = (item.ChinaBondExercise - item.Price) * 100;
                }
                else if (item.ChinaBondDue.HasValue && item.Price.HasValue)
                {
                    if (item.Side == "1")
                        item.CCDCValuation_BP_InnerBid = (item.Price - item.ChinaBondDue) * 100;
                    else if (item.Side == "2")
                        item.CCDCValuation_BP_InnerOfr = (item.ChinaBondDue - item.Price) * 100;
                }
                item.ModifiedDuration = obj.modifiedDuration;
                if (!string.IsNullOrEmpty(obj.inf3) && obj.inf3 != "0")
                {
                    JObject jsonObj = JObject.Parse(obj.inf3);
                    if (jsonObj.ContainsKey("Val_Modified_Duration"))
                    {
                        //string Issue_Amount = jsonObj["Issue_Amount"].ToString();
                        string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                        //string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                        //string valuation_date = jsonObj["valuation_date"].ToString();
                        if (STC.Common.Utils.CommonUtils.IsNumeric(Val_Modified_Duration))
                        {
                            item.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                        }
                    }
                    if (jsonObj.ContainsKey("Outstanding_Amount"))
                    {
                        string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                        if (STC.Common.Utils.CommonUtils.IsNumeric(Outstanding_Amount))
                        {
                            item.Outstanding_Amount = Outstanding_Amount.ConvertToDecimal() / 10000;
                        }
                    }
                }
                item.DayOffStr = obj.dayOffStr;
                item.CopyInfo = SignParser.GetCopyInfo(item.Qty?.ToString("#"), obj);
                item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                item.Rating = SignParser.GenRating(item.SignDic1, item.SignDic3, item.SecurityId);
                item.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(item.SignDic3);
                item.BondCategory = SignParser.GenBondCategory(item.SignDic, item.SignDic1);
                item.Level = SignParser.GenLevel(item.SignDic3);
                item.RegionType = SignParser.GenRegionType(item.SignDic2);
                item.EnterpriseType = SignParser.GenEnterpriseType(item.SignDic1);
                item.If_Guarantee = SignParser.GenIfGuarantee(item.SignDic1);
                item.If_Repay = SignParser.GenIfRepay(item.SignDic3);
                item.IssueType = SignParser.GenIssueType(item.SignDic1);
                item.IssueAmount = obj.issueAmount / 10000;
                if (STC.Common.Utils.CommonUtils.IsNumeric(obj.outstandingAmount))
                {
                    item.Outstanding_Amount = obj.outstandingAmount.ConvertToDecimal() / 10000;
                }
                item.QuoteType = obj.quotetype;

                item.CounterPartyID = obj.counterPartyID;
                item.CounterPartyName = obj.counterPartyName;
                item.RoleType = obj.roletype;
                item.Investor = obj.partyManager;
                item.InvestorName = obj.partyManagerName;
                item.Trader_Buyer = obj.partyTrader;
                item.TraderName_Buyer = obj.partyTraderName;

                if (JsonSplit.IsJson(obj.readFlag))
                {
                    JObject jsonObj = JObject.Parse(obj.readFlag);
                    if (jsonObj != null && jsonObj.ContainsKey(LocalEnv.UserInfo.UserName))
                    {
                        item.ReadFlag = jsonObj[LocalEnv.UserInfo.UserName].ToString();
                    }
                }
                if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                {
                    if (JsonSplit.IsJson(obj.info5))
                    {
                        NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(obj.info5);
                        item.PublicRemarks = info5tag.PUBLIC_REMARKS;
                        item.CommandNote = info5tag.COMMAND_NOTES;
                        item.RiskNote = info5tag.RISK_CHECK_RESULT;
                        if (info5tag.PERSONAL_REMARKS != null && info5tag.PERSONAL_REMARKS.ContainsKey(LocalEnv.UserInfo.UserName))
                        {
                            item.PersonalRemarks = info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName];
                        }
                        //if (!string.IsNullOrEmpty(info5tag.REF_NOTES))
                        //{
                        //    if (!string.IsNullOrEmpty(item.PersonalRemarks))
                        //    {
                        //        item.PersonalRemarks += "\r\n";
                        //    }
                        //    item.PersonalRemarks += $"撤销备注:{info5tag.REF_NOTES}";
                        //}
                    }
                    else
                    {
                        item.PublicRemarks = string.Empty;
                        item.CommandNote = string.Empty;
                        item.RiskNote = string.Empty;
                        item.PersonalRemarks = string.Empty;
                    }
                }
                else
                {
                    item.PublicRemarks = obj.publicRemarks;
                    item.PersonalRemarks = obj.personalRemarks;
                }

                string matchStatus = IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "MATCH_STATUS");
                item.MatchStatus = matchStatus;
                string execStatus = IntentionOldHandler.Instance.ExtractByKey(obj.subStatus, "EXEC_STATUS");
                item.ExecStatus = execStatus;

                if (_lastItem == null)
                {
                    item.Operation = "新增";
                }
                else if ((_lastItem.MatchStatus == "7" || _lastItem.MatchStatus == "8") && _lastItem.CloseBy == "SYSTEM")
                {
                    item.Operation = "自动完成";
                }
                else
                {
                    StringBuilder diffSB = new StringBuilder();
                    
                    if (_lastItem.Price != item.Price)
                    {
                        if (diffSB.Length > 0)
                        {
                            diffSB.Append(",");
                        }

                        diffSB.Append("1");
                    }
                    if (_lastItem.Qty != item.Qty)
                    {
                        if (diffSB.Length > 0)
                        {
                            diffSB.Append(",");
                        }

                        diffSB.Append("2");
                    }
                    if (_lastItem.Side != item.Side)
                    {
                        if (diffSB.Length > 0)
                        {
                            diffSB.Append(",");
                        }

                        diffSB.Append("3");
                    }
                    if (_lastItem.SettlType != item.SettlType)
                    {
                        if (diffSB.Length > 0)
                        {
                            diffSB.Append(",");
                        }

                        diffSB.Append("4");
                    }
                    if (_lastItem.CommandNote != item.CommandNote)
                    {
                        if (diffSB.Length > 0)
                        {
                            diffSB.Append(",");
                        }

                        diffSB.Append("5");
                    }
                    if (_lastItem.Status != item.Status)
                    {
                        if (diffSB.Length > 0)
                        {
                            diffSB.Append(",");
                        }

                        diffSB.Append("6");
                    }
                    if (_lastItem.MatchStatus != item.MatchStatus)
                    {
                        if (diffSB.Length > 0)
                        {
                            diffSB.Append(",");
                        }

                        diffSB.Append("7");
                    }

                    item.Diff = diffSB.ToString();

                    if (diffSB.Length > 0)
                    {
                        string[] arr = item.Diff.Split(Consts.COMMA_CHAR);
                        if (arr.Length == 1)
                        {
                            if (arr[0] == "1")
                            {
                                item.Operation = "要素变更 - 报价";
                            }
                            else if (arr[0] == "2")
                            {
                                item.Operation = "要素变更 - 报量";
                            }
                            else if (arr[0] == "3")
                            {
                                item.Operation = "要素变更 - 方向";
                            }
                            else if (arr[0] == "4")
                            {
                                item.Operation = "要素变更 - 清算速度";
                            }
                            else if (arr[0] == "5")
                            {
                                item.Operation = "要素变更 - 指令备注";
                            }
                            else if (arr[0] == "6")
                            {
                                if (item.Status == "83")
                                {
                                    item.Operation = "REF";
                                }
                                else
                                {
                                    item.Operation = "状态变更";
                                }
                            }
                            else if (arr[0] == "7")
                            {
                                item.Operation = "匹配状态变更";
                            }
                        }
                        else
                        {
                            item.Operation = "要素变更 - 多";
                        }
                    }
                    
                }
                _lastItem = item;
                
                if (!string.IsNullOrEmpty(item.Operation))
                {
                    this.IntentionQuoteInfo.Insert(0, item);
                }
            }
        }
    }
}
