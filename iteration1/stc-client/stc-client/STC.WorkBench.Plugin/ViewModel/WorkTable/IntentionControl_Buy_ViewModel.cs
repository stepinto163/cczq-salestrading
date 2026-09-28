using Ats.Foundation.Message;
using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.PP;
using STC.Common.Handler.Table;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Cal;
using STC.Common.Model.FinQ;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PP;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Order.InnerPanel;
using STC.Common.ViewModel.WorkTable;
using STC.WorkBench.Plugin.View;
using STC.WorkBench.Plugin.View.WorkTable;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class IntentionControl_Buy_ViewModel : MyOrderDefaultViewModel
    {

        private bool _IsInvestor = false;

        public bool IsInvestor
        {
            get { return _IsInvestor; }
            set
            {
                _IsInvestor = value;
                this.OnPropertyChanged();
            }
        }

        private StcTbookUsers selectInvestorItem;

        public StcTbookUsers SelectInvestorItem
        {
            get { return selectInvestorItem; }
            set
            {
                selectInvestorItem = value;

                if (LocalEnv.ConfInfo.IsPAYL)
                {
                    if (value != null)
                    {
                        List<StcTbookUsers> traders = CommonUtils.GetTraders();
                        List<string> traderList = AdminHandler.Instance.GetTraderList(value.Userid);
                        if (traderList != null)
                        {
                            List<StcTbookUsers> list = traders.Where(d => traderList.Contains(d.Userid)).ToList();//多条件筛选，新功能增加
                            if (this.SelectTraderItem != null)
                            {
                                StcTbookUsers current = list.FirstOrDefault<StcTbookUsers>(e => e.Username == this.SelectTraderItem.Username);
                                if (current == null)
                                {
                                    if (list != null && list.Count > 0)
                                    {
                                        this.SelectTraderText = list[0].Name;
                                        this.SelectTraderItem = list[0];
                                    }
                                }
                            }
                            else
                            {
                                if (list != null && list.Count > 0)
                                {
                                    this.SelectTraderText = list[0].Name;
                                    this.SelectTraderItem = list[0];
                                }
                            }
                        }
                    }
                }

                this.OnPropertyChanged();
            }
        }

        private string selectInvestorText;

        public string SelectInvestorText
        {
            get { return selectInvestorText; }
            set
            {
                selectInvestorText = value;
                this.OnPropertyChanged();
            }
        }

        private StcTbookUsers selectTraderItem;

        public StcTbookUsers SelectTraderItem
        {
            get { return selectTraderItem; }
            set
            {
                selectTraderItem = value;
                this.OnPropertyChanged();
            }
        }

        private string selectTraderText;

        public string SelectTraderText
        {
            get { return selectTraderText; }
            set
            {
                selectTraderText = value;
                this.OnPropertyChanged();
            }
        }

        //private ICommand _processCommand;

        //public ICommand ProcessCommand
        //{
        //    get
        //    {
        //        if (_processCommand == null)
        //        {
        //            _processCommand = new ComRelayCommand<Button>(OnProcessCommand);
        //        }
        //        return _processCommand;
        //    }
        //}

        //internal bool _isBatchOps = false;
        //internal List<IntentionQuoteItem> _cacheSelectedItems;

        public IntentionControl_Buy_ViewModel()
        {
            IsInvestor = CommonUtils.IsInvestor();

            statusFilterList.Clear();
            //statusFilterList.Add("0");
            //statusFilterList.Add("1");
            //statusFilterList.Add("80");
            //statusFilterList.Add("81");
            //statusFilterList.Add("82");

            //if (STC.Common.Utils.CommonUtils.IsInvestor())
            //{
            //    QueryTraders();
            //}
            //else
            //{
            //    QueryInvestors();
            //}

            DesignHandler.Instance.OnAgreeRef += OnAgreeRef;
        }

        public void OnAgreeRef(object obj)
        {
            if (obj is STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo)
            {
                STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo info = obj as STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo;
                IntentionQuoteItem item = new IntentionQuoteItem();
                item.IntentionId = info.intentionId;
                item.UserName = info.userName;
                item.GroupKey = $"{info.quotetype}_{info.partyTrader}_{info.securityId}_{info.side}";
                item.QuoteType = info.quotetype;
                item.Trader_Buyer = info.partyTrader;
                item.SecurityId = info.securityId;
                item.Side = info.side;
                item.MatchStatus = IntentionOldHandler.Instance.ExtractByKey(info.subStatus, "MATCH_STATUS");
                item.Status = info.status;
                GenStatusDisplay(item);


                List<IntentionQuoteItem> list = new List<IntentionQuoteItem>();
                list.Add(item);
                DoConfirmRefAll("82", list);
            }
        }

        private bool isDisplay_Buyer(IntentionQuoteItem item)
        {
            if (string.IsNullOrEmpty(this.FilterText) == false
                   && (item.SecurityId == null || !item.SecurityId.Contains(this.FilterText))
                   && (item.Symbol == null || !item.Symbol.Contains(this.FilterText))
                   && (item.IssuerName == null || !item.IssuerName.Contains(this.FilterText))
                   && (item.WarranterName == null || !item.WarranterName.Contains(this.FilterText))
                   && (item.TraderName_Buyer == null || !item.TraderName_Buyer.Contains(this.FilterText))
                   && (item.InvestorName == null || !item.InvestorName.Contains(this.FilterText)))
                return false;

            if (_sideFilterList != null && _sideFilterList.Count > 0 && !_sideFilterList.Contains(item.Side))
                return false;

            string investor = string.IsNullOrEmpty(item.Investor) ? " " : item.Investor;
            if ((myInvestorsList == null || myInvestorsList.Count == 0
                       || (myInvestorsList != null && myInvestorsList.Contains(investor))) == false)
                return false;

            //if (item.Trader_Buyer == LocalEnv.UserInfo.UserName && !isMySelf)
            //{
            //    if ((myTradersList == null || myTradersList.Count == 0
            //       || (myTradersList != null && myTradersList.Contains(item.Trader_Buyer))) == false)
            //        return false;
            //}
            //else
            //{
            //    if (isMySelf && (myTradersList == null || myTradersList.Count == 0))
            //    {
            //        return false;
            //    }
            //    else
            //    {
            //        if ((myTradersList == null || myTradersList.Count == 0 || (myTradersList != null && myTradersList.Contains(item.Trader_Buyer))) == false)
            //            return false;
            //    }
            //}
            if (myTradersList != null && myTradersList.Count > 0 && !myTradersList.Contains(item.Trader_Buyer))
                    return false;

            //&& (item.CloseBy == null || item.CloseBy == LocalEnv.UserInfo.UserName)
            if (((statusFilterList == null || statusFilterList.Count == 0
                || (statusFilterList != null && statusFilterList.Count > 0 && statusFilterList.Contains(item.Status)))
                && (item.Status == "80" || item.Status == "81" || item.Status == "82" || item.Status == "83" || item.Status == "84")) == false)
                return false;

            if(item.SignDic1.ContainsKey(Consts.BOND_SUB_TYPE))
            {
                string bondSubType_tag = item.SignDic1[Consts.BOND_SUB_TYPE];
                string securityType = CommonUtils.GetQQSecurityType(bondSubType_tag, item.BondSubType);
                if ((this.DisplayMode == "BNC" && securityType != "2")
                    || (this.DisplayMode == "BCO" && securityType != "1")
                    || (this.DisplayMode == "LOCAL" && securityType != "21")
                    || (this.DisplayMode == "NCD" && securityType != "3"))
                {
                    return false;
                }
            }
            

            //if ((this.DisplayMode == "BNC" && item.SecurityType != "BNC" && item.BondSubType != "PSB")
            //    || (string.IsNullOrEmpty(this.DisplayMode) == false && this.DisplayMode != "BNC" && item.SecurityType == "BNC" && item.BondSubType != "PSB")
            //    || (this.DisplayMode == "BNC" && item.BondSubType == "PSB"))
            //    return false;

            if (matchStatusFilterList != null && matchStatusFilterList.Count > 0 && !matchStatusFilterList.Contains(item.MatchStatus))
            {
                return false;
            }

            return true;
        }

        internal new void FilterData()
        {
            this.WorkingViewSource.View.Filter = x =>
            {
                IntentionQuoteItem item = (IntentionQuoteItem)x;

                bool flag = true;
                if (item == null)
                    return flag;

                if (item.Type == "INTENTION_GROUP")
                {
                    string groupkey = $"{item.QuoteType}_{item.Trader_Buyer}_{item.SecurityId}_{item.Side}";
                    if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupkey))
                    {
                        foreach (IntentionQuoteItem subItem in _Cache_Buy_InnerOrders_Dic[groupkey].Values)
                        {
                            if (isDisplay_Buyer(subItem)) return true;
                        }
                    }
                    return false;
                }
                else
                {
                    return isDisplay_Buyer(item);
                }
            };
        }


        public void EditCounterPartySubName()
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0
                || this.SelectInvestorItem == null || string.IsNullOrEmpty(this.SelectInvestorText))
            {
                return;
            }

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();
            StringBuilder sb = new StringBuilder();

            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in this.SelectedItems_Working)
                {
                    if (drv.Type == INTENTION_GROUP || drv.Status == "82" || drv.Status == "83" || drv.Status == "84")
                    {
                        continue;
                    }

                    if (LocalEnv.ConfInfo.IsI2T)
                    {
                        StcTbookUsers trader = CommonUtils.GetUsersInfoByUserName(drv.Trader_Buyer);
                        if (trader != null && !string.IsNullOrEmpty(trader.Userid))
                        {
                            List<string> managerList = AdminHandler.Instance.GetManagerList(trader.Userid);
                            StcTbookUsers currentInvestor = CommonUtils.GetUsersInfoByUserName(this.selectInvestorItem.Username);
                            if (managerList == null || currentInvestor == null || string.IsNullOrEmpty(currentInvestor.Userid) || !managerList.Contains(currentInvestor.Userid))
                            {
                                continue;
                            }
                        }
                    }

                    //if (string.IsNullOrEmpty(drv.InvestorName) || string.IsNullOrEmpty(drv.TraderName))
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        newIntentionOrder.counterPartyID = newIntentionOrder.institutionId = LocalEnv.UserInfo.Termids;
                        newIntentionOrder.counterPartyName = newIntentionOrder.institutionName = LocalEnv.UserInfo.biztype;

                        string managerid = null;
                        string managerName = null;
                        string traderid = drv.Trader_Buyer;
                        string traderName = drv.TraderName_Buyer;

                        //if (this.SelectTraderItem != null)
                        //{
                        //    traderid = this.SelectTraderItem.Username;
                        //    traderName = this.SelectTraderItem.Name;
                        //}
                        ////else
                        ////{
                        ////    traderid = this.SelectTraderText;
                        ////    traderName = this.SelectTraderText;
                        ////}

                        if (this.SelectInvestorItem != null)
                        {
                            managerid = this.SelectInvestorItem.Username;
                            managerName = this.SelectInvestorItem.Name;
                        }
                        //else
                        //{
                        //    managerid = this.SelectInvestorText;
                        //    managerName = this.SelectInvestorText;
                        //}

                        if (CommonUtils.IsInvestor())
                        {
                            if (string.IsNullOrEmpty(managerid))
                            {
                                managerid = LocalEnv.UserInfo.UserName;
                                managerName = LocalEnv.UserInfo.FullName;
                            }
                        }
                        //else
                        //{
                        //    if (string.IsNullOrEmpty(traderid))
                        //    {
                        //        traderid = LocalEnv.UserInfo.UserName;
                        //        traderName = LocalEnv.UserInfo.FullName;
                        //    }
                        //}

                        if (drv.RoleType == "0011")
                        {
                            newIntentionOrder.counterPartySubID = traderid;
                            newIntentionOrder.counterPartySubName = traderName;
                        }
                        else
                        {
                            newIntentionOrder.counterPartySubID = managerid;
                            newIntentionOrder.counterPartySubName = managerName;
                        }

                        newIntentionOrder.partyManager = managerid;
                        newIntentionOrder.partyManagerName = managerName;
                        newIntentionOrder.partyTrader = traderid;
                        newIntentionOrder.partyTraderName = traderName;

                        STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sameIntention = IntentionOldHandler.Instance.GetSameIntentionInfo(managerid, traderid, drv.SecurityId, drv.Side, drv.AccountID);

                        
                        if (sameIntention != null && sameIntention.intentionId != drv.IntentionId)
                        {
                            string side = ComboSourceUtil.Instance.FindEnum(drv.Side, "side");
                            string manager = string.IsNullOrEmpty(managerName) ? string.Empty : $"{managerName}(投资经理)";
                            string managerComma = string.IsNullOrEmpty(manager) ? string.Empty : "、";
                            string trader = string.IsNullOrEmpty(traderName) ? string.Empty : $"{traderName}(交易员),";
                            string account = string.Empty;
                            if (LocalEnv.ConfInfo.IsPAYL)
                            {
                                account = string.IsNullOrEmpty(drv.AccountName) ? string.Empty : $"在{drv.AccountName}(交易账户)下,";
                            }
                            if (drv.MatchedQty.HasValue && drv.MatchedQty > 0)
                            {
                                //string message = $"已有{manager}、{trader}{account}针对{drv.SecurityId}债券的{side}需求，请先解除匹配的成交再修改";
                                //sb.AppendLine(message);
                                //sb.AppendLine("----------------------------------------------------------------");
                                continue;
                            }
                            else
                            {
                               
                                string message = $"{manager}{managerComma}{trader}{account}针对{drv.SecurityId}债券的{side}需求";
                                sb.AppendLine(message);
                                sb.AppendLine("----------------------------------------------------------------");
                            }
                        }

                        //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                        drv.UpdateIntentionRequest = newIntentionOrder;
                        drv.Request = null;
                        drv.InquiryRequest = null;
                        orderDic[newIntentionOrder.intentionId] = drv;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            if (sb.Length > 0)
            {
                //是否覆盖已有需求/
                BatchCheckSameWin batchCheckSameWin = new BatchCheckSameWin();
                batchCheckSameWin.DetailsTxt.Text = sb.ToString();
                if(batchCheckSameWin.ShowDialog() == false)
                {
                    BatchOperateBtnEnable = true;
                    return;
                }
            }

            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count;
                    // 修改意向单
                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }

                    string text = string.Empty;
                    if (failCount > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });
        }

        

        internal void Sub_DoAllProcessCommand(IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                return;
            }

            IntentionQuoteItem currentItem = null;
            ObservableCollection<IntentionMarketItem> datas = null;
            SetDoAllBtnEnable(false);
            try
            {
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                StringBuilder sb = new StringBuilder();
                foreach (IntentionQuoteItem item in selectedItems)
                {
                    if (item.Status != "80" || item.Type == INTENTION_GROUP) continue;
                    if (item.Status == "80")
                    {
                        STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo sameIntention = IntentionOldHandler.Instance.GetSameIntentionInfo(item.Investor, item.Trader_Buyer, item.SecurityId, item.Side, item.AccountID);

                        if (sameIntention != null && sameIntention.intentionId != item.IntentionId)
                        {
                            string side = ComboSourceUtil.Instance.FindEnum(item.Side, "side");
                            string manager = string.IsNullOrEmpty(item.InvestorName) ? string.Empty : $"{item.InvestorName}(投资经理)";
                            string managerComma = string.IsNullOrEmpty(manager) ? string.Empty : "、";
                            string trader = string.IsNullOrEmpty(item.TraderName_Buyer) ? string.Empty : $"{item.TraderName_Buyer}(交易员),";
                            string account = string.Empty;
                            if (LocalEnv.ConfInfo.IsPAYL)
                            {
                                account = string.IsNullOrEmpty(item.AccountName) ? string.Empty : $"在{item.AccountName}(交易账户)下,";
                            }
                            string message = $"{manager}{managerComma}{trader}{account}针对{item.SecurityId}债券的{side}需求";
                            sb.AppendLine(message);
                            sb.AppendLine("----------------------------------------------------------------");
                        }

                        STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                        request.intentionId = item.IntentionId;
                        request.actionType = "81";// 处理
                        request.userName = item.BrokerId;
                        request.closeBy = LocalEnv.UserInfo.UserName;
                        request.closeByName = LocalEnv.UserInfo.FullName;
                        request.clOrdId = DataTimeSequence.Instance.GetId();
                        request.accountID = item.AccountID;
                        request.accountName = item.AccountName;
                        request.ifSubBond = item.ifSubBond;
                        request.vipCode = item.vipCode;
                        request.orgType = item.orgType;
                        request.validUntilTime = GetValidUntilTime(item.Info1);
                        request.inf1 = item.Info1;
                        request.info2 = item.Info2;
                        request.channel = item.Channel;
                        request.priceType = item.PriceType;
                        request.updateTime = item.UpdateTime;
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                        {
                            request.quoteType = "BUYER_INTENTION";
                        }
                        else
                        {
                            request.quoteType = "INTENTION";
                        }

                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (CommonUtils.IsInvestor())
                            {
                                request.actionFlag = "0";
                            }
                            else
                            {
                                request.actionFlag = "2";
                            }

                            request.riskManualCk = "1";
                        }
                        

                        item.Request = request;
                        orderDic[item.IntentionId] = item;

                        item.InquiryRequest = null;
                    }
                }

                if (sb.Length > 0)
                {
                    //是否覆盖已有需求/
                    BatchCheckSameWin batchCheckSameWin = new BatchCheckSameWin();
                    batchCheckSameWin.Title = "处理个需";
                    batchCheckSameWin.DetailsTxt.Text = sb.ToString();
                    if (batchCheckSameWin.ShowDialog() == false)
                    {
                        SetDoAllBtnEnable(true);
                        return;
                    }
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    // 删除部分
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (item.Status == "80")
                        {
                            System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                            {
                                GetErrorInfo(item.Request,
                                        ref failCount,
                                        ref totalCount,
                                        warnReplyDic,
                                        prohibitReplyDic,
                                        exceptionReplyDic);
                            });
                            try
                            {
                                Thread.Sleep(20);
                            }
                            catch (Exception ex)
                            { }
                        }
                    }

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                        }
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllIgnoreCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        internal new void Sub_DoAllIgnoreCommand(IList selectedItems, string quoteType = null)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量REF");
                return;
            }

            IEnumerable<IntentionQuoteItem> list = selectedItems.Cast<IntentionQuoteItem>().Where(r => r.Type != INTENTION_GROUP);

            if (!CommonUtils.IsInvestor())
            {
                int canRefCount = list.Count(r => r.MatchStatus != "5" && (r.Status == "80" || r.Status == "81") && r.Type != INTENTION_GROUP);
                if (canRefCount > 0)
                {
                    Common.View.Component.CustomMessageBox.Info("请先解除匹配的成交再撤销。", "撤销");
                    return;
                }
            }

            //if (!Common.View.Component.CustomMessageBox.Question("是否执行批量REF操作？", "批量REF"))
            //{
            //    return;
            //}

            IntentionQuoteItem currentItem = null;
            ObservableCollection<IntentionMarketItem> datas = null;
            List<string> InquiryNotRefList = null;
            ModifyRefView x = null;
            if (LocalEnv.ConfInfo.IsInquiry && list.Count() == 1 )
            {
                string groupKey = $"{list.First().QuoteType}_{list.First().Trader_Buyer}_{list.First().SecurityId}_{list.First().Side}";
                if (list.First() != null && !HasGroupRow(groupKey))
                {
                    currentItem = list.First();

                    string key = list.First().IntentionId;
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        key = groupKey;
                    }

                    if (!string.IsNullOrEmpty(key)
                       && this._Cache_InquiryQuote_Dic.ContainsKey(key)
                       && this._Cache_InquiryQuote_Dic[key].Count > 0)
                    {
                        datas = new ObservableCollection<IntentionMarketItem>(this._Cache_InquiryQuote_Dic[key].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                    }
                }
                if (datas != null && datas.Count > 0)
                {

                    x = new ModifyRefView(currentItem, datas);
                    //if (currentItem.Status == "81" || currentItem.Status == "80")
                    //{
                    //    x.refNote.Visibility = Visibility.Visible;
                    //    if (JsonSplit.IsJson(currentItem.Info5))
                    //    {
                    //        NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(currentItem.Info5);
                    //        if (info5tag.PERSONAL_REMARKS != null && info5tag.PERSONAL_REMARKS.ContainsKey(LocalEnv.UserInfo.UserName))
                    //        {
                    //            x.refNote.Text = info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName];
                    //        }
                    //    }
                    //}
                    if (x.ShowDialog() == false)
                    {
                        return;
                    }
                    InquiryNotRefList = x.InquiryNotRefList;
                }
            }

            //if (CommonUtils.IsInvestor())
            //{

            //}
            //else
            //{
            //    if (x == null)
            //    {
            //        foreach (IntentionQuoteItem item in list)
            //        {
            //            if (item.Status == "81" || item.Status == "80")
            //            {
            //                x = new ModifyRefView(null, null);
            //                if (list.Count() == 1)
            //                {
            //                    if (JsonSplit.IsJson(item.Info5))
            //                    {
            //                        NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(item.Info5);
            //                        if (info5tag.PERSONAL_REMARKS != null && info5tag.PERSONAL_REMARKS.ContainsKey(LocalEnv.UserInfo.UserName))
            //                        {
            //                            x.refNote.Text = info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName];
            //                        }
            //                    }
            //                }
            //                x.refNote.Visibility = Visibility.Visible;
            //                if (x.ShowDialog() == false)
            //                {
            //                    return;
            //                }
            //                break;
            //            }
            //        }
            //    }
            //}

            //string refNote = string.Empty;
            //if (x != null)
            //{
            //    refNote = x.refNote.Text;
            //}
            SetDoAllBtnEnable(false);
            try
            {
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                foreach (IntentionQuoteItem item in list)
                {
                    if (item.Status != "80" && item.Status != "81") continue;
                    if (item.Status == "80" || item.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                        request.intentionId = item.IntentionId;
                        if (CommonUtils.IsInvestor() && item.Status == "81")
                        {
                            request.actionType = "86";
                        }
                        else
                        {
                            request.actionType = "82";// REF
                        }
                        request.userName = item.BrokerId;
                        request.closeBy = LocalEnv.UserInfo.UserName;
                        request.closeByName = LocalEnv.UserInfo.FullName;
                        request.clOrdId = DataTimeSequence.Instance.GetId();
                        request.accountID = item.AccountID;
                        request.accountName = item.AccountName;
                        request.ifSubBond = item.ifSubBond;
                        request.vipCode = item.vipCode;
                        request.orgType = item.orgType;
                        request.validUntilTime = GetValidUntilTime(item.Info1);
                        request.inf1 = item.Info1;
                        request.info2 = item.Info2;

                        //if (!string.IsNullOrEmpty(refNote))
                        //{
                        //    NoteData info5tag = null;
                        //    if (JsonSplit.IsJson(item.Info5))
                        //    {
                        //        info5tag = JsonConvert.DeserializeObject<NoteData>(item.Info5);
                        //    }
                        //    else
                        //    {
                        //        info5tag = new NoteData();
                        //    }

                        //    info5tag.REF_NOTES = refNote;
                        //    if (info5tag.PERSONAL_REMARKS == null)
                        //    {
                        //        info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                        //    }
                        //    info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = refNote;
                        //    request.info5 = JsonConvert.SerializeObject(info5tag);
                        //}

                        request.channel = item.Channel;
                        request.priceType = item.PriceType;
                        request.updateTime = item.UpdateTime;
                        if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                        {
                            request.quoteType = "BUYER_INTENTION";
                        }
                        else
                        {
                            request.quoteType = "INTENTION";
                        }
                        item.Request = request;
                        orderDic[item.IntentionId] = item;

                        IntentionInquiryRequest inquiryRequest = GenIntentionInquiryRequest(item.IntentionId, item.GroupKey, InquiryNotRefList);
                        item.InquiryRequest = inquiryRequest;
                    }
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    // 删除部分
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    foreach (IntentionQuoteItem item in orderDic.Values)
                    {
                        if (item.Status == "80" || item.Status == "81")
                        {
                            System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                            {
                                int inquiryFail = failCount;
                                if (item.InquiryRequest != null)
                                {
                                    GetErrorInfo_Inquiry(item.InquiryRequest,
                                        ref failCount,
                                        ref totalCount,
                                        warnReplyDic,
                                        prohibitReplyDic,
                                        exceptionReplyDic);
                                }
                                else
                                {
                                    totalCount++;
                                }

                                if (inquiryFail - failCount < 1)
                                {
                                    GetErrorInfo(item.Request,
                                            ref failCount,
                                            ref totalCount,
                                            warnReplyDic,
                                            prohibitReplyDic,
                                            exceptionReplyDic);
                                }
                            });
                            try
                            {
                                Thread.Sleep(20);
                            }
                            catch (Exception ex)
                            { }
                        }
                    }

                    while (totalCount != orderDic.Count)
                    {
                        Thread.Sleep(100);
                    }

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (failCount > 0)
                        {
                            if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                            {
                                BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                    warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                batchResultWindow.Owner = Application.Current.MainWindow;
                                batchResultWindow.ShowDialog();
                            }
                        }
                    }));
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoAllIgnoreCommand", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
        }

        internal new void EditPersonalMarks(List<IntentionQuoteItem> modifyList, List<string> inquiryNotRefList, string remarks, bool isFinish = false)
        {
            BatchOperateBtnEnable = false;

            if (modifyList.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int totalCount = 0;
                    int failCount = 0;
                    Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                    Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                    // 修改意向单
                    foreach (IntentionQuoteItem requestItem in modifyList)
                    {
                        if (isFinish && requestItem.Status == "81")
                        {
                            orderDic[requestItem.IntentionId] = requestItem;

                            IntentionInquiryRequest inquiryRequest = GenIntentionInquiryRequest(requestItem.IntentionId, requestItem.GroupKey, inquiryNotRefList);
                            requestItem.InquiryRequest = inquiryRequest;

                            STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest finishReq = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                            finishReq.intentionId = requestItem.IntentionId;
                            finishReq.actionType = "83";// 完成
                            finishReq.userName = requestItem.BrokerId;
                            finishReq.closeBy = LocalEnv.UserInfo.UserName;
                            finishReq.closeByName = LocalEnv.UserInfo.FullName;
                            finishReq.updateTime = requestItem.UpdateTime;
                            finishReq.personalRemarks = remarks;
                            // info5
                            NoteData info5tag = null;
                            if (requestItem != null && JsonSplit.IsJson(requestItem.Info5))
                            {
                                info5tag = JsonConvert.DeserializeObject<NoteData>(requestItem.Info5);
                            }
                            else
                            {
                                info5tag = new NoteData();
                            }
                            if (info5tag.PERSONAL_REMARKS == null)
                            {
                                info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                            }
                            info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = remarks;
                            finishReq.info5 = JsonConvert.SerializeObject(info5tag);

                            if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                            {
                                finishReq.quoteType = "BUYER_INTENTION";
                            }
                            else
                            {
                                finishReq.quoteType = "INTENTION";
                            }

                            requestItem.Request = finishReq;
                            requestItem.UpdateIntentionRequest = null;
                        }
                        else
                        {
                            STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest request = GetIntentionOrder(requestItem);
                            // info2
                            TagData info2tag = null;
                            if (JsonSplit.IsJson(requestItem.Info2))
                            {
                                info2tag = JsonConvert.DeserializeObject<TagData>(requestItem.Info2);
                            }
                            else
                            {
                                info2tag = new TagData();
                            }

                            info2tag.PERSONAL_REMARKS = remarks;
                            request.info2 = JsonConvert.SerializeObject(info2tag);
                            // info5
                            NoteData info5tag = null;
                            if (requestItem != null && JsonSplit.IsJson(requestItem.Info5))
                            {
                                info5tag = JsonConvert.DeserializeObject<NoteData>(requestItem.Info5);
                            }
                            else
                            {
                                info5tag = new NoteData();
                            }
                            if (info5tag.PERSONAL_REMARKS == null)
                            {
                                info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                            }
                            info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = remarks;
                            request.info5 = JsonConvert.SerializeObject(info5tag);

                            orderDic[requestItem.IntentionId] = requestItem;
                            requestItem.Request = null;
                            requestItem.UpdateIntentionRequest = request;
                            requestItem.InquiryRequest = null;
                        }
                    }

                    if (orderDic != null && orderDic.Count > 0)
                    {
                        foreach (IntentionQuoteItem requestItem in orderDic.Values)
                        {
                            int inquiryFail = failCount;
                            if (requestItem.InquiryRequest != null)
                            {
                                GetErrorInfo_Inquiry(requestItem.InquiryRequest,
                                    ref failCount,
                                    ref totalCount,
                                    warnReplyDic,
                                    prohibitReplyDic,
                                    exceptionReplyDic);
                            }
                            else
                            {
                                totalCount++;
                            }

                            if (inquiryFail - failCount < 1)
                            {
                                if (isFinish)
                                {
                                    totalCount--;
                                    GetErrorInfo(requestItem.Request,
                                       ref failCount,
                                       ref totalCount,
                                       warnReplyDic,
                                       prohibitReplyDic,
                                       exceptionReplyDic);
                                }
                                else
                                {
                                    totalCount--;
                                    GetErrorInfo_UpdateIntention(requestItem.UpdateIntentionRequest,
                                       ref failCount,
                                       ref totalCount,
                                       warnReplyDic,
                                       prohibitReplyDic,
                                       exceptionReplyDic);
                                }
                            }
                        }

                        while (totalCount != orderDic.Count)
                        {
                            Thread.Sleep(100);
                        }

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (failCount > 0)
                            {
                                if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                                {
                                    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                        warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                    batchResultWindow.Owner = Application.Current.MainWindow;
                                    batchResultWindow.ShowDialog();
                                }
                            }
                        }));
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });
        }

        /// <summary>
        /// 82 : REF  87:REF拒绝
        /// </summary>
        /// <param name="item"></param>
        internal void DoConfirmRefAll(string actiontype, IList selectedItems)
        {
            if (selectedItems == null || selectedItems.Count == 0)
            {
                //Common.View.Component.CustomMessageBox.Info("请先选择操作记录", "批量REF");
                return;
            }

            IEnumerable<IntentionQuoteItem> list = selectedItems.Cast<IntentionQuoteItem>().Where(r => r.Type != INTENTION_GROUP);
            int canRefCount = list.Count(r => r.MatchStatus != "5");
            if (canRefCount > 0 && actiontype != "87")
            {
                Common.View.Component.CustomMessageBox.Info("请先解除匹配的成交再撤销。", "同意");
                return;
            }

            IntentionQuoteItem currentItem = null;
            ObservableCollection<IntentionMarketItem> datas = null;
            List<string> InquiryNotRefList = null;
            ModifyRefView x = null;
            if (actiontype == "82" && LocalEnv.ConfInfo.IsInquiry && list.Count() == 1)
            {
                string groupKey = $"{list.First().QuoteType}_{list.First().Trader_Buyer}_{list.First().SecurityId}_{list.First().Side}";
                if (list.First() != null && !HasGroupRow(groupKey))
                {
                    currentItem = list.First();
                    
                    string key = list.First().IntentionId;
                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        key = groupKey;
                    }

                    if (!string.IsNullOrEmpty(key)
                       && this._Cache_InquiryQuote_Dic.ContainsKey(key)
                       && this._Cache_InquiryQuote_Dic[key].Count > 0)
                    {
                        datas = new ObservableCollection<IntentionMarketItem>(this._Cache_InquiryQuote_Dic[key].Values.Where<IntentionMarketItem>(a => a.Status == "1" && a.SendType != 3 && a.OptionType != "sendIntentQQGroups").OrderByDescending(a => a.UpdateTime));
                    }
                }
                if (datas != null && datas.Count > 0)
                {

                    x = new ModifyRefView(currentItem, datas);
                    //if (currentItem.Status == "81" || currentItem.Status == "80")
                    //{
                    //    x.refNote.Visibility = Visibility.Visible;
                    //    if (JsonSplit.IsJson(item.Info5))
                    //    {
                    //        NoteData info5tag = JsonConvert.DeserializeObject<NoteData>(item.Info5);
                    //        if (info5tag.PERSONAL_REMARKS != null && info5tag.PERSONAL_REMARKS.ContainsKey(LocalEnv.UserInfo.UserName))
                    //        {
                    //            x.refNote.Text = info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName];
                    //        }
                    //    }
                    //}
                    if (x.ShowDialog() == false)
                    {
                        return;
                    }
                    InquiryNotRefList = x.InquiryNotRefList;
                }

                //string refNote = string.Empty;
                //if (x != null)
                //{
                //    refNote = x.refNote.Text;
                //}

                //if (!string.IsNullOrEmpty(refNote))
                //{
                //    NoteData info5tag = null;
                //    if (JsonSplit.IsJson(item.Info5))
                //    {
                //        info5tag = JsonConvert.DeserializeObject<NoteData>(item.Info5);
                //    }
                //    else
                //    {
                //        info5tag = new NoteData();
                //    }
                //    if (info5tag.PERSONAL_REMARKS == null)
                //    {
                //        info5tag.PERSONAL_REMARKS = new Dictionary<string, string>();
                //    }
                //    info5tag.PERSONAL_REMARKS[LocalEnv.UserInfo.UserName] = refNote;
                //    info5tag.REF_NOTES = refNote;
                //    request.info5 = JsonConvert.SerializeObject(info5tag);
                //    request.personalRemarks = refNote;
                //}
            }

            SetDoAllBtnEnable(false);
            try
            {
                Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
                foreach (IntentionQuoteItem item in list)
                {
                    if (item.Status != "82") continue;

                    STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest request = new STC.Common.Model.Table.WorkBrench.ConfirmIntentionQuoteRequest();
                    request.intentionId = item.IntentionId;
                    request.actionType = actiontype;
                    request.userName = item.UserName;
                    request.closeBy = LocalEnv.UserInfo.UserName;
                    request.closeByName = LocalEnv.UserInfo.FullName;
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE)
                    {
                        request.quoteType = "BUYER_INTENTION";
                    }
                    else
                    {
                        request.quoteType = "INTENTION";
                    }
                    item.Request = request;
                    orderDic[item.IntentionId] = item;

                    IntentionInquiryRequest inquiryRequest = GenIntentionInquiryRequest(item.IntentionId, item.GroupKey, InquiryNotRefList);
                    item.InquiryRequest = inquiryRequest;
                }

                System.Threading.ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        int totalCount = 0;
                        int failCount = 0;
                        Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                        Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                        Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();

                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            if (item.Status == "82")
                            {
                                System.Threading.ThreadPool.QueueUserWorkItem(o2 =>
                                {
                                    int inquiryFail = failCount;
                                    if (item.InquiryRequest != null)
                                    {
                                        GetErrorInfo_Inquiry(item.InquiryRequest,
                                            ref failCount,
                                            ref totalCount,
                                            warnReplyDic,
                                            prohibitReplyDic,
                                            exceptionReplyDic);
                                    }
                                    else
                                    {
                                        totalCount++;
                                    }

                                    if (inquiryFail - failCount < 1)
                                    {
                                        GetErrorInfo(item.Request,
                                                ref failCount,
                                                ref totalCount,
                                                warnReplyDic,
                                                prohibitReplyDic,
                                                exceptionReplyDic);
                                    }
                                });
                                try
                                {
                                    Thread.Sleep(20);
                                }
                                catch (Exception ex)
                                { }
                            }
                        }

                        while (totalCount != orderDic.Count)
                        {
                            Thread.Sleep(100);
                        }

                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (failCount > 0)
                            {
                                if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0 || exceptionReplyDic.Count > 0)
                                {
                                    BatchResultWindow batchResultWindow = new BatchResultWindow(orderDic,
                                        warnReplyDic, prohibitReplyDic, failCount, exceptionReplyDic);
                                    batchResultWindow.Owner = Application.Current.MainWindow;
                                    batchResultWindow.ShowDialog();
                                }
                                //else
                                //{
                                //    CustomMessageBox.Info($"共普通确认{totalCount}条, 其中失败{failCount}条", "普通确认结果");
                                //}
                            }
                            //else
                            //{
                            //    CustomMessageBox.Info($"共普通确认{totalCount}条, 全部成功", "普通确认结果");
                            //}
                        }));
                    }
                    catch (Exception e)
                    {
                        LogHelper.Error("DoConfirmCommand", e);
                    }
                    finally
                    {
                        SetDoAllBtnEnable(true);
                    }
                });
            }
            catch (Exception e)
            {
                LogHelper.Error("DoConfirmRefAll", e);
            }
            finally
            {
                SetDoAllBtnEnable(true);
            }
           

            
        }

        //#region 投资经理（弃用）

        //private const string BUYER_INVESTORS_FILTER = "BUYER_INVESTORS_FILTER";

        //private object _dataLock_Investor = new object();
        //private CollectionViewSource _viewSource_investor = new CollectionViewSource();
        //public CollectionViewSource ViewSource_Investor
        //{
        //    get { return _viewSource_investor; }
        //    set { _viewSource_investor = value; }
        //}

        //private ObservableCollection<InvestorFilter> _InvestorsList = new ObservableCollection<InvestorFilter>();
        //public ObservableCollection<InvestorFilter> InvestorList
        //{
        //    get { return _InvestorsList; }
        //    set { _InvestorsList = value; }
        //}

        //private InvestorFilter _selectedItem_investor;

        //public InvestorFilter SelectedItem_Investor
        //{
        //    get { return _selectedItem_investor; }
        //    set
        //    {
        //        if (_selectedItem_investor != value)
        //        {
        //            _selectedItem_investor = value;

        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        //private void QueryInvestors()
        //{
        //    System.Threading.ThreadPool.QueueUserWorkItem(o =>
        //    {
        //        SystemParameterReq req = new SystemParameterReq();
        //        req.ParamKey = BUYER_INVESTORS_FILTER;
        //        req.Username = LocalEnv.UserInfo.UserName;
        //        TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
        //        if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
        //        {
        //            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //            {
        //                string json = reply.Datas.tbookSystemParameter.param_value;
        //                if (JsonSplit.IsJson(reply.Datas.tbookSystemParameter.param_value))
        //                {
        //                    List<InvestorFilter> list = JsonConvert.DeserializeObject<List<InvestorFilter>>(reply.Datas.tbookSystemParameter.param_value);
        //                    foreach (InvestorFilter item in list)
        //                    {
        //                        InvestorList.Add(item);
        //                    }
        //                }
        //            }));
        //        }
        //    });
        //}

        //internal void AddInvestorFilter(StcTbookUsers investor)
        //{
        //    if (investor != null)
        //    {
        //        if (!InvestorList.Any(entry => entry.UserName == investor.Username))
        //        {
        //            InvestorFilter newItem = new InvestorFilter();
        //            newItem.UserName = investor.Username;
        //            newItem.name = investor.Name;
        //            InvestorList.Add(newItem);

        //            UpdateInvestors(JsonConvert.SerializeObject(InvestorList));
        //        }
        //    }
        //}

        //internal void DelInvesotrFilter(string username)
        //{
        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        InvestorFilter removeItem = InvestorList.FirstOrDefault(entry => entry.UserName == username);
        //        if (removeItem != null)
        //        {
        //            InvestorList.Remove(removeItem);

        //            UpdateInvestors(JsonConvert.SerializeObject(InvestorList));
        //        }
        //    }
        //}

        //private void UpdateInvestors(string value)
        //{
        //    System.Threading.ThreadPool.QueueUserWorkItem(o =>
        //    {
        //        SystemGlobalParamReq req = new SystemGlobalParamReq();
        //        req.action = "2";
        //        req.list = new List<TTbookSystemParameter>();
        //        TTbookSystemParameter param = new TTbookSystemParameter();
        //        param.param_key = BUYER_INVESTORS_FILTER;
        //        param.username = LocalEnv.UserInfo.UserName;
        //        param.param_value = value;// JsonConvert.SerializeObject(InvestorList);
        //        param.param_desc = "投资经理过滤选项";
        //        req.list.Add(param);

        //        PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
        //        if (reply != null)
        //        {
        //            if (reply.Code == "0000")
        //            {
        //            }
        //        }
        //    });
        //}


        //protected List<string> investorFilterList = new List<string>();

        //internal void ProcessInvestorFilterData(IList investorList)
        //{
        //    investorFilterList.Clear();

        //    foreach (InvestorFilter item in investorList)
        //    {
        //        investorFilterList.Add(item.UserName);
        //    }

        //    this.WorkingViewSource.View?.Refresh();
        //}

        //#endregion


        //#region 交易员（弃用）

        //private const string BUYER_TRADERS_FILTER = "BUYER_TRADERS_FILTER";

        //private object _dataLock_trader = new object();
        //private CollectionViewSource _viewSource_trader = new CollectionViewSource();
        //public CollectionViewSource ViewSource_trader
        //{
        //    get { return _viewSource_trader; }
        //    set { _viewSource_trader = value; }
        //}

        //private ObservableCollection<InvestorFilter> _TraderList = new ObservableCollection<InvestorFilter>();
        //public ObservableCollection<InvestorFilter> TraderList
        //{
        //    get { return _TraderList; }
        //    set { _TraderList = value; }
        //}

        //private InvestorFilter _selectedItem_Trader;

        //public InvestorFilter SelectedItem_Trader
        //{
        //    get { return _selectedItem_Trader; }
        //    set
        //    {
        //        if (_selectedItem_Trader != value)
        //        {
        //            _selectedItem_Trader = value;

        //            this.OnPropertyChanged();
        //        }
        //    }
        //}

        //private void QueryTraders()
        //{
        //    System.Threading.ThreadPool.QueueUserWorkItem(o =>
        //    {
        //        SystemParameterReq req = new SystemParameterReq();
        //        req.ParamKey = BUYER_TRADERS_FILTER;
        //        req.Username = LocalEnv.UserInfo.UserName;
        //        TbookSystemParameterReplyPo reply = AdminHandler.Instance.querySystemParameter(req);
        //        if (reply != null && reply.IsSuccess() && reply.Datas != null && reply.Datas.tbookSystemParameter != null && !string.IsNullOrEmpty(reply.Datas.tbookSystemParameter.param_value))
        //        {
        //            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //            {
        //                string json = reply.Datas.tbookSystemParameter.param_value;
        //                if (JsonSplit.IsJson(reply.Datas.tbookSystemParameter.param_value))
        //                {
        //                    List<InvestorFilter> list = JsonConvert.DeserializeObject<List<InvestorFilter>>(reply.Datas.tbookSystemParameter.param_value);
        //                    foreach (InvestorFilter item in list)
        //                    {
        //                        TraderList.Add(item);
        //                    }
        //                }
        //            }));
        //        }
        //    });
        //}

        //internal void AddTraderFilter(StcTbookUsers trader)
        //{
        //    if (trader != null)
        //    {
        //        if (!TraderList.Any(entry => entry.UserName == trader.Username))
        //        {
        //            InvestorFilter newItem = new InvestorFilter();
        //            newItem.UserName = trader.Username;
        //            newItem.name = trader.Name;
        //            TraderList.Add(newItem);

        //            UpdateTraders(JsonConvert.SerializeObject(TraderList));
        //        }
        //    }
        //}

        //internal void DelTraderFilter(string username)
        //{
        //    if (!string.IsNullOrEmpty(username))
        //    {
        //        InvestorFilter removeItem = TraderList.FirstOrDefault(entry => entry.UserName == username);
        //        if (removeItem != null)
        //        {
        //            TraderList.Remove(removeItem);

        //            UpdateTraders(JsonConvert.SerializeObject(TraderList));
        //        }
        //    }
        //}

        //private void UpdateTraders(string value)
        //{
        //    System.Threading.ThreadPool.QueueUserWorkItem(o =>
        //    {
        //        SystemGlobalParamReq req = new SystemGlobalParamReq();
        //        req.action = "2";
        //        req.list = new List<TTbookSystemParameter>();
        //        TTbookSystemParameter param = new TTbookSystemParameter();
        //        param.param_key = BUYER_TRADERS_FILTER;
        //        param.username = LocalEnv.UserInfo.UserName;
        //        param.param_value = value;// JsonConvert.SerializeObject(InvestorList);
        //        param.param_desc = "交易员过滤选项";
        //        req.list.Add(param);

        //        PResult reply = AdminHandler.Instance.updateSystemGlobalPara(req);
        //        if (reply != null)
        //        {
        //            if (reply.Code == "0000")
        //            {
        //            }
        //        }
        //    });
        //}


        //protected List<string> traderFilterList = new List<string>();

        //internal void ProcessTraderFilterData(IList traderList)
        //{
        //    traderFilterList.Clear();

        //    foreach (InvestorFilter item in traderList)
        //    {
        //        traderFilterList.Add(item.UserName);
        //    }

        //    this.WorkingViewSource.View?.Refresh();
        //}

        //#endregion

        #region 我的过滤
        protected List<string> myInvestorsList = new List<string>();
        internal void ProcessFilterMyInvestors(string[] arr)
        {
            myInvestorsList.Clear();

            if (arr != null)
            {
                myInvestorsList.AddRange(arr);
            }

            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.WorkingViewSource.View?.Refresh();
        }

        private bool isMySelf = true;
        internal void ProcessFilterIsMe(bool isChecked)
        {
            isMySelf = isChecked;
            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.WorkingViewSource.View?.Refresh();
        }

        #endregion


        private string INTENTION_GROUP = "INTENTION_GROUP";
        private string INTENTION_DETAIL = "INTENTION_DETAIL";
        internal new void OnIntentionQuotes(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            //TableIntentionInquiry inquiryInfo = ConvertToInquiryData(obj);
            //OnMarketQuotes(inquiryInfo);
            lock (_workingLock)
            {
                IntentionQuoteItem item = null;

                string groupKey = $"{obj.quotetype}_{obj.partyTrader}_{obj.securityId}_{obj.side}";
                if (_Cache_InnerOrders_Dic.ContainsKey(obj.securityId) && _Cache_InnerOrders_Dic[obj.securityId] != null
                    && _Cache_InnerOrders_Dic[obj.securityId].ContainsKey(obj.intentionId) && _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId] != null)
                {
                    item = _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId];
                }

                if (item != null)
                {
                    //(!string.IsNullOrEmpty(obj.closeBy) && obj.closeBy != LocalEnv.UserInfo.UserName && obj.status != "0") ||
                    if (obj.status == "00" || !(obj.quotetype == "INTENTION" || obj.quotetype == "BUYER_INTENTION"))
                    {
                        IntentionQuoteItem detialRemoveItem;
                        _Cache_Buy_InnerOrders_Dic[groupKey].TryRemove(obj.intentionId, out detialRemoveItem);

                        ProcessSummaryDic(groupKey);

                        this.IntentionQuoteInfo.Remove(item);
                        if (_Cache_InnerOrders_Dic.ContainsKey(obj.securityId))
                        {
                            ConcurrentDictionary<string, IntentionQuoteItem> removeDic = _Cache_InnerOrders_Dic[obj.securityId];

                            if (removeDic != null && removeDic.ContainsKey(obj.intentionId))
                            {
                                IntentionQuoteItem removeData;
                                removeDic.TryRemove(obj.intentionId, out removeData);
                            }

                            if (_Cache_InnerOrders_Dic[obj.securityId].Count == 0)
                            {
                                _Cache_InnerOrders_Dic.TryRemove(obj.securityId, out removeDic);
                            }
                        }

                        

                        ProcessVirtualItem1(groupKey, obj.securityId, obj.side, item.Trader_Buyer, item.TraderName_Buyer, obj);

                        if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey))
                        {
                            foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                            {
                                GenStatusDisplay(detailItem);
                            }
                        }

                        ////根据当前排序字段设置TempSort
                        //System.Reflection.PropertyInfo pi = item.GetType().GetProperty(_currentSortColumnName);
                        //object value = pi.GetValue(item);
                        //item.TempSort = value;

                        if (_Summary_Dic != null && _Summary_Dic.ContainsKey(groupKey))
                        {
                            // 明细行设置TempSort
                            if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Buy_InnerOrders_Dic[groupKey] != null)
                            {
                                foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                                {
                                    detailItem.TempSort = _Summary_Dic[groupKey];
                                }
                            }

                            // 汇总行设置TempSort
                            if (_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Group_InnerOrders_Dic[groupKey] != null)
                            {
                                _Cache_Group_InnerOrders_Dic[groupKey].TempSort = _Summary_Dic[groupKey];
                            }
                        }
                        return;
                    }


                    item.IsFlashing = true;
                    // 状态变化
                    int oriCount = 0;
                    if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey))
                    {
                        oriCount = _Cache_Buy_InnerOrders_Dic[groupKey].Count;
                    }
                    bool isStatusChange = false;
                    if (item.Status != "81" && obj.status == "81")
                    {
                        isStatusChange = true;
                        if (!_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey))
                        {
                            _Cache_Buy_InnerOrders_Dic[groupKey] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                        }
                        _Cache_Buy_InnerOrders_Dic[groupKey][obj.intentionId] = item;

                        item.SortKey = groupKey;
                        item.Type = INTENTION_DETAIL;
                    }
                    else if (item.Status == "81" && obj.status != "81")
                    {
                        isStatusChange = true;
                        if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey)
                            && _Cache_Buy_InnerOrders_Dic[groupKey].ContainsKey(obj.intentionId))
                        {
                            IntentionQuoteItem removeSubItem;
                            _Cache_Buy_InnerOrders_Dic[groupKey].TryRemove(obj.intentionId, out removeSubItem);

                            item.SortKey = obj.intentionId;
                            item.Type = string.Empty;

                        }
                    }

                    item.Status = obj.status;
                    if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) && oriCount != _Cache_Buy_InnerOrders_Dic[groupKey].Count)
                    {
                        if (_Cache_Buy_InnerOrders_Dic[groupKey].Count > 1)
                        {
                            foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                            {
                                detailItem.SortKey = groupKey;
                                detailItem.Type = INTENTION_DETAIL;
                                detailItem.InquiryStatus = string.Empty;
                            }
                        }
                        else
                        {
                            foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                            {
                                detailItem.SortKey = obj.intentionId;
                                detailItem.Type = string.Empty;
                                GenStatusDisplay(detailItem);
                            }
                        }
                    }


                    if (isStatusChange)
                    {
                        ProcessVirtualItem1(groupKey, obj.securityId, obj.side, item.Trader_Buyer, item.TraderName_Buyer, obj);
                    }

                    if (_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey))
                    {
                        IntentionQuoteItem virualItem = _Cache_Group_InnerOrders_Dic[groupKey];
                        GenStatusDisplay(virualItem);
                        item.InquiryStatus = string.Empty;
                    }
                    else
                    {
                        GenStatusDisplay(item);
                    }

                    isAdjustAllChkStatus = true;

                    //if ((obj.status == "3" && item.Status == "0")
                    //    || (obj.status == "0" && item.Status == "3"))
                    //{
                    //    isAdjustAllChkStatus_Ignore = true;
                    //}

                    
                    //item.Type = obj.type;

                    if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    {
                        if (string.IsNullOrEmpty(obj.orderId) == false)
                        {

                            // 更新我的订单/我的撤单
                            MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                               );
                            // 更新我的成交
                            MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                                );
                        }
                    }
                    item.TraderName_Edit = obj.traderName;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.InstitutionName_Edit = obj.institutionName;
                    item.BookId = obj.bookId;
                    item.BookName = obj.bookName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.BrokerId = obj.userName;
                    item.BrokerName = obj.name;
                    item.BrokerName_Edit = obj.name;
                    item.RoleId = obj.roleid;
                    item.RoleName = obj.rolename;

                    item.Side = obj.side;
                    item.CleanPrice = obj.cleanPrice;
                    item.FullPrice = obj.fullPrice;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    item.ExecutionQty = obj.executionqty;
                    item.SettlType = obj.settlType;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;
                    item.CloseBy = obj.closeBy;
                    item.CloseByName = obj.closeByName;
                    item.OrderID = obj.orderId;
                    item.QqNumber = obj.qqNumber;
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
                    item.Channel = item.Channel;
                    //item.InquiryStatus = "0";
                    if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
                    {
                        item.FQInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                        if (item.FQInfo != null)
                        {
                            //item.InquiryStatus = string.IsNullOrEmpty(item.FQInfo.inquiryStatus) ? "0" : item.FQInfo.inquiryStatus;
                            item.MatchedQty = item.FQInfo.MATCHED_QUANTITY.ConvertToLong();
                            item.IM = item.FQInfo.fromQQ;
                            item.ToGroupName = item.FQInfo.toGroupName;
                            //info2里面如果有ADD_WHITE_LIST=1,+号就显示
                            //只有私聊显示 
                            setAddVisible(item);
                            item.StrategyOrderId = item.FQInfo.StrategyOrderId;
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
                    if (item.CsiBondValuationExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationExercise) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationExercise - item.Price) * 100;
                    }
                    else if (item.CsiBondValuationDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationDue) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationDue - item.Price) * 100;
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
                    item.Tax_Free = SignParser.GetTaxFree(item.SignDic4);

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
                    item.RelatedUsers = obj.relatedUsers;
                    item.RelatedUsers_Display = GetRelatedUsersDisplay(obj.relatedUsers);
                    item.GroupKey = groupKey;
                    if (!string.IsNullOrEmpty(obj.inf1) && JsonSplit.IsJson(obj.inf1))
                    {
                        JObject jsonObj = JObject.Parse(obj.inf1);
                        if (jsonObj.ContainsKey("HsProductId"))
                        {
                            item.HsProductId = jsonObj["HsProductId"] == null ? string.Empty : jsonObj["HsProductId"].ToString();
                        }
                    }

                    if (obj.priceType == "1")
                    {
                        item.Price_Display = obj.cleanPrice;
                    }
                    else if (obj.priceType == "2")
                    {
                        item.Price_Display = obj.fullPrice;
                    }
                    else
                    {
                        item.Price_Display = obj.price;
                    }

                    ProcessSummaryDic(groupKey);

                    if (_Summary_Dic != null && _Summary_Dic.ContainsKey(groupKey))
                    {
                        // 明细行设置TempSort
                        if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Buy_InnerOrders_Dic[groupKey] != null)
                        {
                            foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                            {
                                detailItem.TempSort = _Summary_Dic[groupKey];
                            }
                        }

                        // 汇总行设置TempSort
                        if (_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Group_InnerOrders_Dic[groupKey] != null)
                        {
                            _Cache_Group_InnerOrders_Dic[groupKey].TempSort = _Summary_Dic[groupKey];
                        }
                    }
                    else
                    {
                        //根据当前排序字段设置TempSort
                        System.Reflection.PropertyInfo pi = item.GetType().GetProperty(_currentSortColumnName);
                        object value = pi.GetValue(item);
                        item.TempSort = value;
                    }
                }
                else
                {
                    //(!string.IsNullOrEmpty(obj.closeBy) && obj.closeBy != LocalEnv.UserInfo.UserName && obj.status != "0") ||
                    if (obj.status == "00" || !(obj.quotetype == "INTENTION" || obj.quotetype == "BUYER_INTENTION"))
                    {
                        return;
                    }

                    item = new IntentionQuoteItem();
                    int oriCount = _Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) ?  _Cache_Buy_InnerOrders_Dic[groupKey].Count : 0;
                    if (obj.status == "81")
                    {
                        if (!_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey))
                        {
                            _Cache_Buy_InnerOrders_Dic[groupKey] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                        }
                        _Cache_Buy_InnerOrders_Dic[groupKey][obj.intentionId] = item;
                    }
                    

                    if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    {
                        if (string.IsNullOrEmpty(obj.orderId) == false)
                        {

                            // 更新我的订单/我的撤单
                            MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                               );
                            // 更新我的成交
                            MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                               obj.institutionId, obj.institutionName,
                               obj.traderId, obj.traderName
                                );
                        }
                    }
                    item.IntentionId = obj.intentionId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.BookId = obj.bookId;
                    item.BookName = obj.bookName;
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

                    isAdjustAllChkStatus = true;

                    //if (obj.status == "3")
                    //{
                    //    isAdjustAllChkStatus_Ignore = true;
                    //}
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

                    if (marketindicator == "4")
                    {
                        try
                        {
                            if (!_symbolCountDic.ContainsKey(obj.securityId))
                            {
                                _symbolCountDic[obj.securityId] = null;
                                LogHandler.Info($"InnerBoardViewModel UpdateInnerOrder RegisterCTPrice :{obj.securityId}.");
                                PriceHandler.Instance.RegisterCTPrice(obj.securityId, UpdateCtPrice);
                            }

                            //CCDCValuaModel ccdcValueModel = Common.Utils.CommonUtils.GetCCDCValua(item.SecurityId);
                            //if (ccdcValueModel != null && !string.IsNullOrEmpty(ccdcValueModel.CCDCValuation))
                            //{
                            //    item.CCDCValuation = ccdcValueModel.CCDCValuation.ConvertToDecimal();
                            //    item.CCDCValuation_Date = ccdcValueModel.CCDCValuaDate;
                            //    item.ShowLastWorkDay = ccdcValueModel.ShowLastWorkDay;
                            //}
                        }
                        catch (Exception e)
                        {
                            LogHandler.Error(e);
                        }

                        //if (item.CCDCValuation.HasValue && item.Price.HasValue)
                        //{
                        //    if (item.Side == "1")
                        //        item.CCDCValuation_BP = (item.Price - item.CCDCValuation) * 100;
                        //    else if (item.Side == "2")
                        //        item.CCDCValuation_BP = (item.CCDCValuation - item.Price) * 100;
                        //}
                    }

                    if (_bestPriceDic.ContainsKey(obj.securityId))
                    {
                        item.Best_Bid = _bestPriceDic[obj.securityId][0];
                        item.Best_Ofr = _bestPriceDic[obj.securityId][1];
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
                    //item.InquiryStatus = "0";
                    if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
                    {
                        item.FQInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                        if (item.FQInfo != null)
                        {
                            item.MatchedQty = item.FQInfo.MATCHED_QUANTITY.ConvertToLong();
                            //item.InquiryStatus = string.IsNullOrEmpty(item.FQInfo.inquiryStatus) ? "0" : item.FQInfo.inquiryStatus;
                            item.IM = item.FQInfo.fromQQ;
                            item.ToGroupName = item.FQInfo.toGroupName;
                            //info2里面如果有ADD_WHITE_LIST=1,+号就显示
                            //只有私聊显示 
                            setAddVisible(item);
                            item.StrategyOrderId = item.FQInfo.StrategyOrderId;
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
                    if (item.CsiBondValuationExercise.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationExercise) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationExercise - item.Price) * 100;
                    }
                    else if (item.CsiBondValuationDue.HasValue && item.Price.HasValue)
                    {
                        if (item.Side == "1")
                            item.CsiValuation_BP_InnerBid = (item.Price - item.CsiBondValuationDue) * 100;
                        else if (item.Side == "2")
                            item.CsiValuation_BP_InnerOfr = (item.CsiBondValuationDue - item.Price) * 100;
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
                    item.Tax_Free = SignParser.GetTaxFree(item.SignDic4);

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
                    item.RelatedUsers = obj.relatedUsers;
                    item.RelatedUsers_Display = GetRelatedUsersDisplay(obj.relatedUsers);
                    item.GroupKey = groupKey;
                    if (!string.IsNullOrEmpty(obj.inf1) && JsonSplit.IsJson(obj.inf1))
                    {
                        JObject jsonObj = JObject.Parse(obj.inf1);
                        if (jsonObj.ContainsKey("HsProductId"))
                        {
                            item.HsProductId = jsonObj["HsProductId"] == null ? string.Empty : jsonObj["HsProductId"].ToString();
                        }
                    }

                    if (obj.priceType == "1")
                    {
                        item.Price_Display = obj.cleanPrice;
                    }
                    else if (obj.priceType == "2")
                    {
                        item.Price_Display = obj.fullPrice;
                    }
                    else
                    {
                        item.Price_Display = obj.price;
                    }

                    if (!_Cache_InnerOrders_Dic.ContainsKey(obj.securityId))
                    {
                        _Cache_InnerOrders_Dic[obj.securityId] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                    }
                    _Cache_InnerOrders_Dic[obj.securityId][obj.intentionId] = item;


                    ProcessSummaryDic(groupKey);

                    this.IntentionQuoteInfo.Add(item);

                    ProcessVirtualItem1(groupKey, obj.securityId, obj.side, item.Trader_Buyer, item.TraderName_Buyer, obj);

                    if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) && oriCount != _Cache_Buy_InnerOrders_Dic[groupKey].Count)
                    {
                        if (_Cache_Buy_InnerOrders_Dic[groupKey].Count > 1)
                        {
                            foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                            {
                                detailItem.SortKey = groupKey;
                                detailItem.Type = INTENTION_DETAIL;
                                detailItem.InquiryStatus = string.Empty;
                            }
                        }
                        else
                        {
                            foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                            {
                                detailItem.SortKey = obj.intentionId;
                                detailItem.Type = string.Empty;
                                GenStatusDisplay(detailItem);
                            }
                        }
                    }
                    else
                    {
                        item.SortKey = obj.intentionId;
                        item.Type = string.Empty;
                    }

                    if (_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey))
                    {
                        IntentionQuoteItem virualItem = _Cache_Group_InnerOrders_Dic[groupKey];
                        GenStatusDisplay(virualItem);
                        if (item.Status == "81")
                        {
                            item.InquiryStatus = string.Empty;
                        }
                        else
                        {
                            GenStatusDisplay(item);
                        }
                    }
                    else
                    {
                        GenStatusDisplay(item);
                    }

                    if (Common.Utils.CommonUtils.IsBDBAppName())
                    {
                        UpdateLastExecutions_BDB(item);
                    }
                    else
                    {
                        UpdateLastExecutions(item);
                    }
                    
                    if (_Summary_Dic != null && _Summary_Dic.ContainsKey(groupKey))
                    {
                        // 明细行设置TempSort
                        if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Buy_InnerOrders_Dic[groupKey] != null)
                        {
                            foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                            {
                                detailItem.TempSort = _Summary_Dic[groupKey];
                            }
                        }

                        // 汇总行设置TempSort
                        if (_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Group_InnerOrders_Dic[groupKey] != null)
                        {
                            _Cache_Group_InnerOrders_Dic[groupKey].TempSort = _Summary_Dic[groupKey];
                        }
                    }
                    else
                    {
                        //根据当前排序字段设置TempSort
                        System.Reflection.PropertyInfo pi = item.GetType().GetProperty(_currentSortColumnName);
                        object value = pi.GetValue(item);
                        item.TempSort = value;
                    }
                }
            }
            //((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
        }

        private void ProcessSummaryDic(string groupKey)
        {
            if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Buy_InnerOrders_Dic[groupKey].Count > 0)
            {
                if (_currentListSortDirection == System.ComponentModel.ListSortDirection.Ascending)
                {
                    object mainValue = _Cache_Buy_InnerOrders_Dic[groupKey].Values.Min(x => typeof(IntentionQuoteItem).GetProperty(_currentSortColumnName).GetValue(x));
                    _Summary_Dic[groupKey] = mainValue;
                }
                else
                {
                    object mainValue = _Cache_Buy_InnerOrders_Dic[groupKey].Values.Max(x => typeof(IntentionQuoteItem).GetProperty(_currentSortColumnName).GetValue(x));
                    _Summary_Dic[groupKey] = mainValue;
                }
            }
            else
            {
                object mainValue;
                _Summary_Dic.TryRemove(groupKey, out mainValue);
            }



            //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    if (this.SelectedItems_Working != null && _isBatchOps == false && this.SelectedItems_Working.Count > 0)
            //    {
            //        this._isBatchOps = true;
            //        this._cacheSelectedItems = this.SelectedItems_Working.Cast<IntentionQuoteItem>().ToList();
            //        //Console.WriteLine($"33333333333333333333333333333:{this._cacheSelectedItems.Count}");
            //    }

            //    if (isLoadHistory == false)
            //    {
            //        ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            //        this.WorkingViewSource.View?.Refresh();
            //        if (this.SelectedItems_Working != null && this.SelectedItems_Working.Count == 1)
            //        {
            //            // 单条更新尽然不触发SelectionChanged事件， 强制触发
            //            IntentionQuoteItem temp = this.SelectedItem_Working;
            //            this.SelectedItem_Working = null;
            //            //Console.WriteLine($"55555555555555555555555555555555555555555555:{this._cacheSelectedItems.Count}");
            //            this._isBatchOps = true;
            //            this.SelectedItem_Working = temp;
            //        }
            //    }
            //}));
        }

        private void ProcessVirtualItem1(string groupKey, string securityid, string side, string traderBuyer, string traderBuyerName, STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            string strategyOrderId = string.Empty;
            if (!string.IsNullOrEmpty(obj.info2) && JsonSplit.IsJson(obj.info2))
            {
                TagData fqInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<TagData>(obj.info2);
                strategyOrderId = fqInfo.StrategyOrderId;
            }

            if (_Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey) && _Cache_Buy_InnerOrders_Dic[groupKey].Count > 1)
            {
                //1.1 是否> 1,
                //1.2 本来没有虚拟行，生成虚拟行
                if (!_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey))
                {
                    IntentionQuoteItem virualItem = new IntentionQuoteItem();

                    virualItem.Type = INTENTION_GROUP;
                    virualItem.SortKey = groupKey;
                    virualItem.IntentionId = groupKey;
                    virualItem.GroupKey = groupKey;
                    virualItem.Trader_Buyer = traderBuyer;
                    virualItem.TraderName_Buyer = traderBuyerName;
                    virualItem.SecurityId = securityid;
                    virualItem.Sign1 = obj.sign1;
                    virualItem.InvestorName = "多";
                    virualItem.StrategyOrderId = strategyOrderId;
                    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityid);
                    if (bondsInfoPo != null)
                    {
                        virualItem.Symbol = bondsInfoPo.Short_Name;
                        virualItem.RemainingTime = bondsInfoPo.RemaingingTime;
                        virualItem.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                        virualItem.SecurityType = bondsInfoPo.SECURITY_TYPE;
                        virualItem.BondSubType = bondsInfoPo.Bond_Subtype;
                        //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                        //findData.Issuer = bondsInfoPo.issuer;
                        virualItem.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                        //findData.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
                    }
                    virualItem.Side = side;
                    virualItem.Status = "81";
                    virualItem.QuoteType = "BUYER_INTENTION";
                    virualItem.Sign = obj.sign;
                    virualItem.Sign1 = obj.sign1;
                    virualItem.Sign2 = obj.sign2;
                    virualItem.Sign3 = obj.sign3;
                    virualItem.Sign4 = obj.sign4;
                    virualItem.IssuerName = obj.issuerName;
                    virualItem.WarranterName = obj.warranterName;
                    virualItem.CouponRateCurrent = obj.couponRateCurrent;
                    virualItem.IssueAmount = obj.issueAmount / 10000;
                    virualItem.ChinaBondValuation = obj.chinaBondValuation;
                    virualItem.ChinaBondValuationFull = obj.chinaBondValuationFull;
                    virualItem.CsiBondValuation = obj.csiBondValuation;
                    virualItem.CsiBondValuationFull = obj.csiBondValuationFull;
                    if (SignParser.IsCumType(virualItem.SignDic1))
                    {
                        if (string.IsNullOrEmpty(obj.chinaBondValuationFull) == false)
                        {
                            int speIndex = obj.chinaBondValuationFull.IndexOf('|');
                            if (speIndex > -1)
                            {
                                virualItem.ChinaBondExercise = obj.chinaBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                                virualItem.ChinaBondDue = obj.chinaBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                            }
                            else
                            {
                                virualItem.ChinaBondDue = obj.chinaBondValuationFull.ConvertToDecimal();
                            }
                        }

                    }
                    else
                    {
                        virualItem.ChinaBondDue = obj.chinaBondValuation;
                    }
                    if (string.IsNullOrEmpty(obj.csiBondValuationFull) == false)
                    {
                        int speIndex = obj.csiBondValuationFull.IndexOf('|');
                        if (speIndex > -1)
                        {
                            virualItem.CsiBondValuationExercise = obj.csiBondValuationFull.Substring(0, speIndex).ConvertToDecimal();
                            virualItem.CsiBondValuationDue = obj.csiBondValuationFull.Substring(speIndex + 1).ConvertToDecimal();
                        }
                        else
                        {
                            virualItem.CsiBondValuationDue = obj.csiBondValuationFull.ConvertToDecimal();
                        }
                    }
                    else
                    {
                        virualItem.CsiBondValuationDue = obj.csiBondValuation;
                    }
                    if (!string.IsNullOrEmpty(obj.inf3) && obj.inf3 != "0")
                    {
                        JObject jsonObj = JObject.Parse(obj.inf3);
                        if (jsonObj.ContainsKey("Val_Modified_Duration"))
                        {
                            string Val_Modified_Duration = jsonObj["Val_Modified_Duration"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Val_Modified_Duration))
                            {
                                virualItem.ModifiedDuration = Val_Modified_Duration.ConvertToDecimal();
                            }
                        }
                        if (jsonObj.ContainsKey("Outstanding_Amount"))
                        {
                            string Outstanding_Amount = jsonObj["Outstanding_Amount"].ToString();
                            if (STC.Common.Utils.CommonUtils.IsNumeric(Outstanding_Amount))
                            {
                                virualItem.Outstanding_Amount = Outstanding_Amount.ConvertToDecimal() / 10000;
                            }
                        }
                    }
                    virualItem.BondCategory = SignParser.GenBondCategory(virualItem.SignDic, virualItem.SignDic1);
                    virualItem.Rating = SignParser.GenRating(virualItem.SignDic1, virualItem.SignDic3, virualItem.SecurityId);
                    virualItem.IssueType = SignParser.GenIssueType(virualItem.SignDic1);
                    virualItem.Cdc_Implited_Rating = SignParser.GetCDCImplitedRating(virualItem.SignDic3);
                    virualItem.If_Repay = SignParser.GenIfRepay(virualItem.SignDic3);
                    virualItem.Tax_Free = SignParser.GetTaxFree(virualItem.SignDic4);

                    if (_bestPriceDic.ContainsKey(obj.securityId))
                    {
                        virualItem.Best_Bid = _bestPriceDic[obj.securityId][0];
                        virualItem.Best_Ofr = _bestPriceDic[obj.securityId][1];
                    }

                    if (Common.Utils.CommonUtils.IsBDBAppName())
                    {
                        UpdateLastExecutions_BDB(virualItem);
                    }
                    else
                    {
                        UpdateLastExecutions(virualItem);
                    }

                    //virualItem.CreateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                    //virualItem.UpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                    this.IntentionQuoteInfo.Add(virualItem);

                    // 1.添加虚拟单
                    if (!_Cache_InnerOrders_Dic.ContainsKey(virualItem.SecurityId))
                    {
                        _Cache_InnerOrders_Dic[virualItem.SecurityId] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                    }
                    _Cache_InnerOrders_Dic[virualItem.SecurityId][virualItem.IntentionId] = virualItem;


                    _Cache_Group_InnerOrders_Dic[groupKey] = virualItem;
                }
            }
            else
            {
                //2.1 是否<1
                //2.2 如果有虚拟行， 删除虚拟行
                
                if (_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey))
                {
                    IntentionQuoteItem virualItem = _Cache_Group_InnerOrders_Dic[groupKey];

                    // 删除虚拟数据，
                    if (_Cache_InnerOrders_Dic.ContainsKey(virualItem.SecurityId))
                    {
                        ConcurrentDictionary<string, IntentionQuoteItem> removeDic = _Cache_InnerOrders_Dic[virualItem.SecurityId];

                        if (removeDic != null && removeDic.ContainsKey(virualItem.IntentionId))
                        {
                            IntentionQuoteItem removeData;
                            removeDic.TryRemove(virualItem.IntentionId, out removeData);
                        }

                        if (_Cache_InnerOrders_Dic[virualItem.SecurityId].Count == 0)
                        {
                            _Cache_InnerOrders_Dic.TryRemove(virualItem.SecurityId, out removeDic);
                        }
                    }
                    this.IntentionQuoteInfo.Remove(virualItem);


                    IntentionQuoteItem removeGroupItem;
                    _Cache_Group_InnerOrders_Dic.TryRemove(groupKey, out removeGroupItem);
                }
            }
        }

        private void ProcessVirtualItem(string groupKey, string securityid, string side, string traderBuyer, string traderBuyerName, string intentionid = null)
        {
            if (_Cache_Buy_InnerOrders_Dic[groupKey].Count > 1)
            {
                //1.1 是否> 1,
                //1.2 本来没有虚拟行，生成虚拟行
                //1.3 把当前加入到虚拟行中
                if (!_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey))
                {
                    IntentionQuoteItem virualItem = new IntentionQuoteItem();
                    
                    virualItem.Type = INTENTION_GROUP;
                    virualItem.IntentionId = groupKey;
                    virualItem.Trader_Buyer = traderBuyer;
                    virualItem.TraderName_Buyer = traderBuyerName;
                    virualItem.SecurityId = securityid;
                    virualItem.Side = side;
                    virualItem.Status = "81";
                    virualItem.QuoteType = "BUYER_INTENTION";
                    //virualItem.CreateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                    //virualItem.UpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss");
                    this.IntentionQuoteInfo.Add(virualItem);

                    _Cache_Group_InnerOrders_Dic[groupKey] = virualItem;

                    // 1.添加虚拟单
                    if (!_Cache_InnerOrders_Dic.ContainsKey(virualItem.SecurityId))
                    {
                        _Cache_InnerOrders_Dic[virualItem.SecurityId] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                    }
                    _Cache_InnerOrders_Dic[virualItem.SecurityId][virualItem.IntentionId] = virualItem;
                    // 2.把子单加入到虚拟单
                    foreach (IntentionQuoteItem detailItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                    {
                        // 2.1 删除子单
                        this.IntentionQuoteInfo.Remove(detailItem);
                        if (_Cache_InnerOrders_Dic.ContainsKey(detailItem.SecurityId))
                        {
                            ConcurrentDictionary<string, IntentionQuoteItem> removeDic = _Cache_InnerOrders_Dic[detailItem.SecurityId];

                            if (removeDic != null && removeDic.ContainsKey(detailItem.IntentionId))
                            {
                                IntentionQuoteItem removeData;
                                removeDic.TryRemove(detailItem.IntentionId, out removeData);
                            }

                            if (_Cache_InnerOrders_Dic[detailItem.SecurityId].Count == 0)
                            {
                                _Cache_InnerOrders_Dic.TryRemove(detailItem.SecurityId, out removeDic);
                            }
                        }

                        // 2.2 加入到虚拟单
                        virualItem.DetailIntentionQuoteInfo.Add(detailItem);
                    }
                }
                else
                {
                    IntentionQuoteItem groupItem = _Cache_Group_InnerOrders_Dic[groupKey];
                    
                    if (!string.IsNullOrEmpty(intentionid) && _Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey)
                        && _Cache_Buy_InnerOrders_Dic[groupKey].ContainsKey(intentionid))
                    {
                        IntentionQuoteItem detailItem = _Cache_Buy_InnerOrders_Dic[groupKey][intentionid];
                        if (_Cache_Buy_InnerOrders_Dic[groupKey].Count > groupItem.DetailIntentionQuoteInfo.Count)
                        {
                            this.IntentionQuoteInfo.Remove(detailItem);
                            if (_Cache_InnerOrders_Dic.ContainsKey(detailItem.SecurityId))
                            {
                                ConcurrentDictionary<string, IntentionQuoteItem> removeDic = _Cache_InnerOrders_Dic[detailItem.SecurityId];

                                if (removeDic != null && removeDic.ContainsKey(detailItem.IntentionId))
                                {
                                    IntentionQuoteItem removeData;
                                    removeDic.TryRemove(detailItem.IntentionId, out removeData);
                                }

                                if (_Cache_InnerOrders_Dic[detailItem.SecurityId].Count == 0)
                                {
                                    _Cache_InnerOrders_Dic.TryRemove(detailItem.SecurityId, out removeDic);
                                }
                            }


                            groupItem.DetailIntentionQuoteInfo.Add(detailItem);
                        }
                    }
                    else
                    {
                        IntentionQuoteItem detialItem = groupItem.DetailIntentionQuoteInfo.FirstOrDefault<IntentionQuoteItem>(a => a.IntentionId == intentionid);
                        if (detialItem != null)
                        {
                            groupItem.DetailIntentionQuoteInfo.Remove(detialItem);

                            if (!_Cache_InnerOrders_Dic.ContainsKey(detialItem.SecurityId))
                            {
                                _Cache_InnerOrders_Dic[detialItem.SecurityId] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                            }
                            _Cache_InnerOrders_Dic[detialItem.SecurityId][detialItem.IntentionId] = detialItem;
                            this.IntentionQuoteInfo.Add(detialItem);
                        }
                    }
                }
                
                
            }
            else
            {
                //2.1 是否<1
                //2.2 如果有虚拟行， 删除虚拟行
                //2.3 直接加入当前数据
                IntentionQuoteItem virualItem;
                if (_Cache_Group_InnerOrders_Dic.ContainsKey(groupKey))
                {
                    // 把虚拟数据中的Detail数据重新加到主列表中
                    virualItem = _Cache_Group_InnerOrders_Dic[groupKey];
                    if (virualItem.DetailIntentionQuoteInfo != null && virualItem.DetailIntentionQuoteInfo.Count > 0)
                    {
                        virualItem.DetailIntentionQuoteInfo.Clear();
                    }
                    foreach (IntentionQuoteItem detialItem in _Cache_Buy_InnerOrders_Dic[groupKey].Values)
                    {
                        if (!_Cache_InnerOrders_Dic.ContainsKey(detialItem.SecurityId))
                        {
                            _Cache_InnerOrders_Dic[detialItem.SecurityId] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                        }
                        _Cache_InnerOrders_Dic[detialItem.SecurityId][detialItem.IntentionId] = detialItem;
                        this.IntentionQuoteInfo.Add(detialItem);
                    }

                    // 删除虚拟数据，
                    if (_Cache_InnerOrders_Dic.ContainsKey(virualItem.SecurityId))
                    {
                        ConcurrentDictionary<string, IntentionQuoteItem> removeDic = _Cache_InnerOrders_Dic[virualItem.SecurityId];

                        if (removeDic != null && removeDic.ContainsKey(virualItem.IntentionId))
                        {
                            IntentionQuoteItem removeData;
                            removeDic.TryRemove(virualItem.IntentionId, out removeData);
                        }

                        if (_Cache_InnerOrders_Dic[virualItem.SecurityId].Count == 0)
                        {
                            _Cache_InnerOrders_Dic.TryRemove(virualItem.SecurityId, out removeDic);
                        }
                    }
                    this.IntentionQuoteInfo.Remove(virualItem);


                    IntentionQuoteItem removeGroupItem;
                    _Cache_Group_InnerOrders_Dic.TryRemove(groupKey, out removeGroupItem);
                }
                else
                {
                    if (!string.IsNullOrEmpty(intentionid) && _Cache_Buy_InnerOrders_Dic.ContainsKey(groupKey)
                        && _Cache_Buy_InnerOrders_Dic[groupKey].ContainsKey(intentionid))
                    {
                        IntentionQuoteItem item = _Cache_Buy_InnerOrders_Dic[groupKey][intentionid];

                        if (!_Cache_InnerOrders_Dic.ContainsKey(item.SecurityId))
                        {
                            _Cache_InnerOrders_Dic[item.SecurityId] = new ConcurrentDictionary<string, IntentionQuoteItem>();
                        }
                        _Cache_InnerOrders_Dic[item.SecurityId][item.IntentionId] = item;
                        this.IntentionQuoteInfo.Add(item);
                    }
                }
            }
        }

        internal new void OnMarketQuotes(TableIntentionInquiry obj)
        {
            lock (_inquiryLock)
            {
                IntentionMarketItem item = null;

                if (_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_InquiryQuote_Dic[obj.intentionId] != null
                    && _Cache_InquiryQuote_Dic[obj.intentionId].ContainsKey(obj.id) && _Cache_InquiryQuote_Dic[obj.intentionId][obj.id] != null)
                {
                    item = _Cache_InquiryQuote_Dic[obj.intentionId][obj.id];
                }
                if (item != null)
                {
                    if ((obj.receiverName == "内部" && obj.status != "1")
                        || (obj.receiverName != "内部" && obj.status != "1" && obj.status != "4"))
                    {
                        IntentionMarketItem removeItem = null;
                        _Cache_InquiryQuote_Dic[obj.intentionId].TryRemove(obj.id, out removeItem);

                        InquiryQuoteInfo.Remove(removeItem);

                        if (!_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId) 
                            || _Cache_InquiryQuote_Dic[obj.intentionId] == null
                            || _Cache_InquiryQuote_Dic[obj.intentionId].Count == 0)
                        {
                            // 没有明细行，去掉汇总行
                            if (_Cache_GroupInquiryQuote_Dic.ContainsKey(obj.intentionId))
                            {
                                InquiryQuoteInfo.Remove(_Cache_GroupInquiryQuote_Dic[obj.intentionId]);

                                IntentionMarketItem removeGroupItem = null;
                                _Cache_GroupInquiryQuote_Dic.TryRemove(obj.intentionId, out removeGroupItem);
                            }
                        }

                        ProcessSummaryDic_Inquiry(obj.intentionId);

                        if (_Summary_Dic_Inquiry != null && _Summary_Dic_Inquiry.ContainsKey(obj.intentionId))
                        {
                            // 明细行设置TempSort
                            if (_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_InquiryQuote_Dic[obj.intentionId] != null)
                            {
                                foreach (IntentionMarketItem detailItem in _Cache_InquiryQuote_Dic[obj.intentionId].Values)
                                {
                                    detailItem.TempSort = _Summary_Dic_Inquiry[obj.intentionId];
                                }
                            }

                            // 汇总行设置TempSort
                            if (_Cache_GroupInquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_GroupInquiryQuote_Dic[obj.intentionId] != null)
                            {
                                _Cache_GroupInquiryQuote_Dic[obj.intentionId].TempSort = _Summary_Dic_Inquiry[obj.intentionId];
                            }
                        }

                        return;
                    }

                    item.Id = obj.id;
                    item.QuoteId = obj.quoteId;

                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);
                        item.Receiver_Edit = toOption;
                        if (toOption != null)
                        {
                            item.ReceiverNumber = toOption.DispayName;
                            item.ReceiverNumber_Edit = toOption.DispayName;
                            item.Trader_Buyer = toOption.TraderBuyer;
                            item.TraderName_Buyer = toOption.TraderName_Buyer;
                            item.Symbol = toOption.Symbol;
                            item.RemainingTime = toOption.RemainingTime;
                            item.BondType = toOption.BondType;
                            item.RemaingingTime_convert = CommonUtils.ConvertRemainingTime(toOption.RemainingTime);
                        }
                    }
                    else
                    {
                        item.ReceiverNumber = obj.receiverName;
                    }
                    item.IntentionId = obj.intentionId;
                    item.SecurityId = obj.securityId;
                    item.SendType = obj.sendType;
                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);

                        if (toOption != null)
                        {
                            item.OptionType = toOption.OptionType;
                        }
                            
                    }
                    
                    if (obj.sendType == 3)
                    {
                        item.Side = obj.side == "1" ? "3" : "4";
                    }
                    else
                    {
                        item.Side = obj.side;
                    }

                    item.Price = obj.price;
                    item.Price_Edit = obj.price;
                    item.Qty = obj.qty;
                    item.Qty_Edit = obj.qty;
                    item.Name = obj.name;
                    item.UserName = obj.userName;
                    item.UserId = obj.userId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.SettlType = string.IsNullOrEmpty(obj.settlType) ? string.Empty : obj.settlType;

                    item.Status = obj.status;
                    item.Action = obj.action;

                    item.PFlag = item.PFlag_Edit = "0";
                    if (obj.bargainFlag == "1")
                    {
                        item.PFlag = item.PFlag_Edit = "1";
                    }
                    else if (obj.quoteDividedFlag == "Y")
                    {
                        item.PFlag = item.PFlag_Edit = "2";
                    }

                    item.OCOFlag = item.OCOFlag_Edit = obj.ocoFlag;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    //item.SenderNumber = obj.senderNumber;
                    //item.ReceiverNumber = obj.receiverNumber;
                    item.Remark = obj.info1;
                    if (obj.receiverName == "内部")
                        item.Sn = 0;
                    else if (obj.status == "1")
                        item.Sn = 1;
                    else
                        item.Sn = 2;
                    item.ClOrdID = obj.clOrdID;
                    item.ExecOrderId = obj.execOrderId;
                    item.Venue = obj.venue;
                    item.VenueTypeGW = obj.venueTypeGW;
                    item.Type = "D";
                    if (JsonSplit.IsJson(obj.info3))
                    {
                        InquiryInfo3 ii3 = JsonConvert.DeserializeObject<InquiryInfo3>(obj.info3);
                        if (ii3 != null)
                        {
                            item.PriceType = ii3.PriceType;
                        }
                    }

                    ProcessSummaryDic_Inquiry(obj.intentionId);

                    if (_Summary_Dic_Inquiry != null && _Summary_Dic_Inquiry.ContainsKey(obj.intentionId))
                    {
                        // 明细行设置TempSort
                        if (_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_InquiryQuote_Dic[obj.intentionId] != null)
                        {
                            foreach (IntentionMarketItem detailItem in _Cache_InquiryQuote_Dic[obj.intentionId].Values)
                            {
                                detailItem.TempSort = _Summary_Dic_Inquiry[obj.intentionId];
                            }
                        }

                        // 汇总行设置TempSort
                        if (_Cache_GroupInquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_GroupInquiryQuote_Dic[obj.intentionId] != null)
                        {
                            _Cache_GroupInquiryQuote_Dic[obj.intentionId].TempSort = _Summary_Dic_Inquiry[obj.intentionId];
                        }
                    }
                    else
                    {
                        //根据当前排序字段设置TempSort
                        System.Reflection.PropertyInfo pi = item.GetType().GetProperty(_currentSortColumnName_Inquiry);
                        object value = pi.GetValue(item);
                        item.TempSort = value;
                    }
                }
                else
                {
                    if ((obj.receiverName == "内部" && obj.status != "1")
                        || (obj.receiverName != "内部" && obj.status != "1" && obj.status != "4"))
                    {
                        return;
                    }

                    item = new IntentionMarketItem();
                    item.Id = obj.id;
                    item.QuoteId = obj.quoteId;

                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);
                        item.Receiver_Edit = toOption;
                        if (toOption != null)
                        {
                            item.ReceiverNumber = toOption.DispayName;
                            item.ReceiverNumber_Edit = toOption.DispayName;
                            item.Trader_Buyer = toOption.TraderBuyer;
                            item.TraderName_Buyer = toOption.TraderName_Buyer;
                            item.Symbol = toOption.Symbol;
                            item.RemainingTime = toOption.RemainingTime;
                            item.BondType = toOption.BondType;
                            item.RemaingingTime_convert = CommonUtils.ConvertRemainingTime(toOption.RemainingTime);
                        }
                    }
                    else
                    {
                        item.ReceiverNumber = obj.receiverName;
                    }

                    item.IntentionId = obj.intentionId;
                    item.SecurityId = obj.securityId;
                    item.SendType = obj.sendType;

                    if (JsonSplit.IsJson(obj.info5))
                    {
                        ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);

                        if (toOption != null)
                        {
                            item.OptionType = toOption.OptionType;
                        }

                    }

                    if (obj.sendType == 3)
                    {
                        item.Side = obj.side == "1" ? "3" : "4";
                    }
                    else
                    {
                        item.Side = obj.side;
                    }

                    item.Price = obj.price;
                    item.Price_Edit = obj.price;
                    item.Qty = obj.qty;
                    item.Qty_Edit = obj.qty;
                    item.Name = obj.name;
                    item.UserName = obj.userName;
                    item.UserId = obj.userId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.SettlType = string.IsNullOrEmpty(obj.settlType) ? string.Empty : obj.settlType;

                    item.Status = obj.status;
                    item.Action = obj.action;
                    item.PFlag = item.PFlag_Edit = "0";
                    if (obj.bargainFlag == "1")
                    {
                        item.PFlag = item.PFlag_Edit = "1";
                    }
                    else if (obj.quoteDividedFlag == "Y")
                    {
                        item.PFlag = item.PFlag_Edit = "2";
                    }
                    item.OCOFlag = item.OCOFlag_Edit = obj.ocoFlag;
                    item.CreateTime = obj.createTime;
                    item.UpdateTime = obj.updateTime;
                    //item.SenderNumber = obj.senderNumber;
                    //item.ReceiverNumber = obj.receiverNumber;
                    item.Remark = obj.info1;
                    if (obj.receiverName == "内部")
                        item.Sn = 0;
                    else if (obj.status == "1")
                        item.Sn = 1;
                    else
                        item.Sn = 2;
                    item.ClOrdID = obj.clOrdID;
                    item.ExecOrderId = obj.execOrderId;
                    item.Venue = obj.venue;
                    item.VenueTypeGW = obj.venueTypeGW;
                    item.Type = "D";
                    if (JsonSplit.IsJson(obj.info3))
                    {
                        InquiryInfo3 ii3 = JsonConvert.DeserializeObject<InquiryInfo3>(obj.info3);
                        if (ii3 != null)
                        {
                            item.PriceType = ii3.PriceType;
                        }
                    }

                    InquiryQuoteInfo.Add(item);

                    if (!_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId))
                    {
                        _Cache_InquiryQuote_Dic[obj.intentionId] = new ConcurrentDictionary<string, IntentionMarketItem>();
                    }
                    if (!_Cache_InquiryQuote_Dic[obj.intentionId].ContainsKey(obj.id))
                    {
                        _Cache_InquiryQuote_Dic[obj.intentionId][obj.id] = item;
                    }

                    if (_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId)
                        && _Cache_InquiryQuote_Dic[obj.intentionId].Count > 0)
                    {
                        // 添加汇总行
                        if (!_Cache_GroupInquiryQuote_Dic.ContainsKey(obj.intentionId))
                        {
                            IntentionMarketItem groupItem = new IntentionMarketItem();
                            //groupItem.Trader_Buyer = obj.traderId;
                            //groupItem.RemainingTime = obj.rema
                            groupItem.SecurityId = obj.securityId;
                            //groupItem.Symbol = obj.bond
                            if (JsonSplit.IsJson(obj.info5))
                            {
                                ToOption toOption = JsonConvert.DeserializeObject<ToOption>(obj.info5);
                                if (toOption != null)
                                {
                                    groupItem.Trader_Buyer = toOption.TraderBuyer;
                                    groupItem.TraderName_Buyer = toOption.TraderName_Buyer;
                                    groupItem.Symbol = toOption.Symbol;
                                    groupItem.RemainingTime = toOption.RemainingTime;
                                    groupItem.BondType = toOption.BondType;
                                    groupItem.RemaingingTime_convert = CommonUtils.ConvertRemainingTime(toOption.RemainingTime);
                                }
                            }
                            groupItem.IntentionId = obj.intentionId;
                            groupItem.Id = obj.intentionId;
                            groupItem.Type = "G";
                            groupItem.Sn = -1;
                            InquiryQuoteInfo.Add(groupItem);
                            _Cache_GroupInquiryQuote_Dic[obj.intentionId] = groupItem;
                        }
                    }

                    ProcessSummaryDic_Inquiry(obj.intentionId);

                    if (_Summary_Dic_Inquiry != null && _Summary_Dic_Inquiry.ContainsKey(obj.intentionId))
                    {
                        // 明细行设置TempSort
                        if (_Cache_InquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_InquiryQuote_Dic[obj.intentionId] != null)
                        {
                            foreach (IntentionMarketItem detailItem in _Cache_InquiryQuote_Dic[obj.intentionId].Values)
                            {
                                detailItem.TempSort = _Summary_Dic_Inquiry[obj.intentionId];
                            }
                        }

                        // 汇总行设置TempSort
                        if (_Cache_GroupInquiryQuote_Dic.ContainsKey(obj.intentionId) && _Cache_GroupInquiryQuote_Dic[obj.intentionId] != null)
                        {
                            _Cache_GroupInquiryQuote_Dic[obj.intentionId].TempSort = _Summary_Dic_Inquiry[obj.intentionId];
                        }
                    }
                    else
                    {
                        //根据当前排序字段设置TempSort
                        System.Reflection.PropertyInfo pi = item.GetType().GetProperty(_currentSortColumnName_Inquiry);
                        object value = pi.GetValue(item);
                        item.TempSort = value;
                    }
                }

                if (_Cache_InnerOrders_Dic.ContainsKey(obj.securityId))
                {
                    IEnumerable<IntentionQuoteItem> list = _Cache_InnerOrders_Dic[obj.securityId].Values.Where(a => a.Type != INTENTION_DETAIL);
                    foreach (IntentionQuoteItem findItem in list)
                    {
                        GenStatusDisplay(findItem);
                    }
                }
            }
        }

        #region 询价分组
        private void ProcessSummaryDic_Inquiry(string groupKey)
        {
            if (_Cache_InquiryQuote_Dic.ContainsKey(groupKey) && _Cache_InquiryQuote_Dic[groupKey].Count > 0)
            {
                if (_currentListSortDirection == System.ComponentModel.ListSortDirection.Ascending)
                {
                    object mainValue = _Cache_InquiryQuote_Dic[groupKey].Values.Min(x => typeof(IntentionMarketItem).GetProperty(_currentSortColumnName).GetValue(x));
                    _Summary_Dic_Inquiry[groupKey] = mainValue;
                }
                else
                {
                    object mainValue = _Cache_InquiryQuote_Dic[groupKey].Values.Max(x => typeof(IntentionMarketItem).GetProperty(_currentSortColumnName).GetValue(x));
                    _Summary_Dic_Inquiry[groupKey] = mainValue;
                }
            }
            else
            {
                object mainValue;
                _Summary_Dic_Inquiry.TryRemove(groupKey, out mainValue);
            }
        }

        internal void ResetCustomSort_Inquiry(ListSortDirection newDirection, string sortMemberPath)
        {
            _currentListSortDirection_Inquiry = newDirection;
            _currentSortColumnName_Inquiry = sortMemberPath;

            // 设置自定义比较器
            _Summary_Dic_Inquiry.Clear();
            if (this._Cache_GroupInquiryQuote_Dic != null)
            {
                foreach (string groupKey in this._Cache_GroupInquiryQuote_Dic.Keys)
                {
                    ProcessSummaryDic_Inquiry(groupKey);
                }
            }

            foreach (IntentionMarketItem item in InquiryQuoteInfo)
            {
                if ((item.Type == "G" || item.Type == "D") && _Summary_Dic_Inquiry != null && _Summary_Dic_Inquiry.ContainsKey(item.IntentionId))
                {
                    item.TempSort = _Summary_Dic_Inquiry[item.IntentionId];
                }
                else
                {
                    System.Reflection.PropertyInfo pi = item.GetType().GetProperty(_currentSortColumnName_Inquiry);
                    object value = pi.GetValue(item);
                    item.TempSort = value;
                }
            }
        }

        #endregion

        internal bool HasGroupRow(string groupKey)
        {
            return _Cache_Group_InnerOrders_Dic.ContainsKey(groupKey);
        }

        #region 分组

        internal void ResetCustomSort(ListSortDirection newDirection, string sortMemberPath)
        {
            _currentListSortDirection = newDirection;
            _currentSortColumnName = sortMemberPath;
            // 设置自定义比较器
            _Summary_Dic.Clear();
            if (this._Cache_Group_InnerOrders_Dic != null)
            {
                foreach(string groupKey in this._Cache_Group_InnerOrders_Dic.Keys)
                {
                    ProcessSummaryDic(groupKey);
                }
            }

            foreach (IntentionQuoteItem item in IntentionQuoteInfo)
            {
                if ((item.Type == INTENTION_GROUP || item.Type == INTENTION_DETAIL) && _Summary_Dic != null && _Summary_Dic.ContainsKey(item.GroupKey))
                {
                    item.TempSort = _Summary_Dic[item.GroupKey];
                }
                else
                {
                    System.Reflection.PropertyInfo pi = item.GetType().GetProperty(_currentSortColumnName);
                    object value = pi.GetValue(item);
                    item.TempSort = value;
                }
            }

            //((ListCollectionView)this.WorkingViewSource.View).CustomSort = new MyCustomComparer(sortMemberPath, newDirection, _Summary_Dic);
        }

        #endregion

        internal void BatchCheckRisk(bool isPrompt = false, Window win = null)
        {
            List<NewOrderSingle> batchRiskList = new List<NewOrderSingle>();
            // 过风控
            if (this.SelectedItems_Working != null)
            {
                foreach (IntentionQuoteItem item in this.SelectedItems_Working)
                {
                    if (item.Type == "INTENTION_GROUP") continue;

                    NewOrderSingle riskReq = new NewOrderSingle();
                    riskReq.Venues = "MANUAL";
                    riskReq.VenueTypeGW = "INNER";
                    riskReq.StrategyOrderId = item.OrderID;
                    riskReq.MarketIndicator = "4";
                    riskReq.SecurityID = item.SecurityId;
                    riskReq.ActionFlag = "6";
                    riskReq.Side = item.Side;
                    riskReq.OrderQty = item.Qty?.ToString("0");
                    riskReq.CounterPartyName = string.Empty;
                    riskReq.SettlType = item.SettlType;

                    if (LocalEnv.ConfInfo.IsPAYL)
                    {
                        // 持仓组合id
                        string bookid = string.Empty;
                        if (!string.IsNullOrEmpty(item.BookId))
                        {
                            bookid = item.BookId;
                        }
                        riskReq.BookID = bookid;
                        ////持仓组合名称
                        //string bookName = string.Empty;
                        //if (item.SelectedGroupText != null && item.SelectedGroupText.Count > 0)
                        //{
                        //    bookName = this.SelectedGroupText[0];
                        //}
                        //riskReq.bookName = bookName;
                        // 持仓组合账号id
                        string accountid = string.Empty;
                        if (!string.IsNullOrEmpty(item.AccountID))
                        {
                            accountid = item.AccountID;
                        }
                        riskReq.AccountID = accountid;
                        //// 持仓组合账号名称
                        //string accountName = string.Empty;
                        //if (!string.IsNullOrEmpty(item.SelectedTradeAccountName))
                        //{
                        //    accountName = item.SelectedTradeAccountName;
                        //}
                        //riskReq.AccountName = accountName;
                    }
                    riskReq.RiskReTryFlag = "0";
                    riskReq.UserName = LocalEnv.UserInfo.UserName;
                    riskReq.ClOrdID = item.IntentionId;
                    riskReq.OrdStatus = "N";
                    batchRiskList.Add(riskReq);
                }

            }

            if (batchRiskList != null && batchRiskList.Count > 0)
            {
                Task.Run(() => {

                    BatchCheckRiskReplyPo po = PPHandler.Instance.BatchCheckRisk(batchRiskList);
                    if (po == null || !po.IsSuccess())
                    {
                        if (po != null && po.Code == "10000" && po.Msg != null && po.Msg.Count > 0)
                        {
                            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
                            {
                                bool isSuccess = true;
                                foreach (IntentionQuoteItem item in this.SelectedItems_Working)
                                {
                                    string intentionid = item.IntentionId;
                                    if (po.Msg.ContainsKey(intentionid))
                                    {
                                        RiskResult riskResult = po.Msg[intentionid];
                                        if (riskResult != null)
                                        {
                                            // 组合代码
                                            // 组合名称
                                            // 交易账户代码
                                            // 交易账户名称
                                            // 投资账号
                                            item.AvailablePosition = riskResult.position.availablePosition; // 总持仓量
                                            item.StcAvailablePosition = riskResult.position.stcAvailablePosition;// 可用持仓量
                                            item.StcInSalesVolume = riskResult.position.stcInSalesVolume;// STC在途卖量
                                            item.StcInBuyVolume = riskResult.position.stcInBuyVolume;//STC在途买量
                                            item.Rate = riskResult.position.rate;//内部主体评级 
                                            item.Quotalimit = riskResult.position.quotalimit;//主体额度
                                            item.Bonddrating = riskResult.position.bonddrating;// 内部债券评级
                                            item.Investmentratio = riskResult.position.investmentratio;//可投比例
                                            item.TotalQty = riskResult.position.totalQty;// optim总持仓量(万)
                                            item.StcAvailableLimit = riskResult.position.stcAvailableLimit;//stc可用额度(万)
                                            StringBuilder sb = new StringBuilder();
                                            if (riskResult.FailMsg != null && riskResult.FailMsg.Count > 0)
                                            {
                                                foreach (TPlaceOrderResult failMsg in riskResult.FailMsg)
                                                {
                                                    if (failMsg.Code == "10001")
                                                    {
                                                        sb.AppendLine(failMsg.Reason);
                                                    }

                                                    if (failMsg.Code == "10002")
                                                    {
                                                        sb.AppendLine(failMsg.Reason);
                                                    }
                                                }
                                            }
                                            item.RiskNote = sb.ToString();// 风控检查备注
                                            if (!string.IsNullOrEmpty(item.RiskNote))
                                            {
                                                isSuccess = false;
                                            }
                                        }
                                    }
                                    await System.Windows.Threading.Dispatcher.Yield();
                                }

                                if (isPrompt && isSuccess)
                                {
                                    CustomMessageBox.Info("风控检查全部通过", "风控检查", win);
                                }
                            }));
                        }
                    }
                    else
                    {
                        if (isPrompt)
                        {
                            CustomMessageBox.Info("风控检查全部通过", "风控检查", win);
                        }
                    }
                });
            }
            else
            {
                CustomMessageBox.Info("请选择需要检查的个需记录", "风控检查", win);
            }
        }

        internal void EditPrice_Bp(decimal? bp)
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0 || bp.HasValue == false)
            {
                return;
            }

            IEnumerable<IntentionQuoteItem> list = this.SelectedItems_Working.Cast<IntentionQuoteItem>();

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();

            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in list)
                {
                    if (drv.Type == "INTENTION_GROUP")
                    {
                        continue;
                    }

                    // 批量修改点差操作，对报价标准为“净价(1)”“全价(2)”的记录不生效
                    if (drv.PriceType == "1" || drv.PriceType == "2")
                    {
                        continue;
                    }

                    if (drv.Status == "0" || drv.Status == "80" || drv.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        decimal? newPrice = null;

                        if (newIntentionOrder.price.HasValue)
                        {
                            //// bid方向时，收益率+点差/0.01
                            //// ofr方向时，收益率-点差/0.01
                            //if (newIntentionOrder.side == "1")
                            //{
                            //   newPrice = newIntentionOrder.price.Value + bp.Value / 100;
                            //}
                            //else if (newIntentionOrder.side == "2")
                            //{
                            //   newPrice = newIntentionOrder.price.Value - bp.Value / 100;
                            //}

                            // 不区分方向，统一 收益率+点差/0.01
                            newPrice = newIntentionOrder.price.Value + bp.Value / 100;
                        }
                        if (newPrice != null && newPrice.HasValue)
                        {
                            newIntentionOrder.price = newPrice;

                            if (newIntentionOrder.customerPrice.HasValue)
                            {
                                TagData info2tag = null;
                                if (newIntentionOrder != null && JsonSplit.IsJson(newIntentionOrder.info2))
                                {
                                    info2tag = JsonConvert.DeserializeObject<TagData>(newIntentionOrder.info2);
                                }
                                else
                                {
                                    info2tag = new TagData();
                                }
                                if (!string.IsNullOrEmpty(info2tag.CUSTOMER_PRICE))
                                {
                                    decimal customerPrice = info2tag.CUSTOMER_PRICE.ConvertToDecimal();
                                    if (newIntentionOrder.side == "1")
                                    {
                                        newIntentionOrder.customerSpread = (newIntentionOrder.price - newIntentionOrder.customerPrice) * 100;
                                    }
                                    else
                                    {
                                        newIntentionOrder.customerSpread = (newIntentionOrder.customerPrice - newIntentionOrder.price) * 100;
                                    }
                                    info2tag.CUSTOMER_SPREAD = newIntentionOrder.customerSpread.ToString();
                                }
                                newIntentionOrder.info2 = JsonConvert.SerializeObject(info2tag);
                            }
                            //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;

                            drv.UpdateIntentionRequest = newIntentionOrder;
                            drv.Request = null;
                            drv.InquiryRequest = null;
                            orderDic[newIntentionOrder.intentionId] = drv;
                        }

                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count;

                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        sendPricePo.HandlInst = "2";
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑报价", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
            });
        }

        internal new void EditPrice_Custom(decimal? price, string priceType, Action ATEFunc = null)
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0 || price.HasValue == false)
            {
                return;
            }

            IEnumerable<IntentionQuoteItem> list = this.SelectedItems_Working.Cast<IntentionQuoteItem>();

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();

            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in list)
                {
                    if (drv.Type == "INTENTION_GROUP")
                    {
                        continue;
                    }

                    string savePriceType = priceType;
                    if (string.IsNullOrEmpty(savePriceType))
                    {
                        BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(drv.SecurityId);
                        if (bondsInfoPo != null)
                        {
                            // 含权
                            if (!string.IsNullOrEmpty(bondsInfoPo.Option_Type) &&
                            ((!bondsInfoPo.Option_Type.Equals("NON") && bondsInfoPo.Option_Date != null) ||
                            (bondsInfoPo.Option_Type.Equals("ETS") && bondsInfoPo.Option_Date == null)))
                            {
                                savePriceType = string.IsNullOrEmpty(priceType) ? "7" : priceType;
                            }
                            else
                            {
                                savePriceType = string.IsNullOrEmpty(priceType) ? "3" : priceType;
                            }
                        }
                    }

                    if (drv.Status == "0" || drv.Status == "80" || drv.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        
                        if (price.HasValue == false)
                        {
                            newIntentionOrder.price = null;
                            newIntentionOrder.cleanPrice = null;
                            newIntentionOrder.fullPrice = null;
                        }
                        else
                        {
                            CalResult calResult = CommonUtils.CalBond_PriceType(newIntentionOrder.securityid, newIntentionOrder.settlType, price?.ToString(), savePriceType);
                            if (calResult != null && calResult.Data != null)
                            {
                                if (savePriceType == "7")
                                {
                                    newIntentionOrder.price = calResult.Data.YieldToExecution * 100;
                                }
                                else
                                {
                                    newIntentionOrder.price = calResult.Data.Yield * 100;
                                }
                                   
                                newIntentionOrder.cleanPrice = calResult.Data.CleanPrice;
                                newIntentionOrder.fullPrice = calResult.Data.FullPrice;
                            }
                        }
                        newIntentionOrder.priceType = savePriceType;
                        //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                        drv.UpdateIntentionRequest = newIntentionOrder;
                        drv.Request = null;
                        drv.InquiryRequest = null;
                        orderDic[newIntentionOrder.intentionId] = drv;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count;
                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        sendPricePo.HandlInst = "2";
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑报价", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
            });
        }

        internal new void EditQty_Custom(decimal? qty, Action ATEFunc = null)
        {
            if (this.SelectedItems_Working == null || this.SelectedItems_Working.Count == 0)
            {
                return;
            }

            IEnumerable<IntentionQuoteItem> list = this.SelectedItems_Working.Cast<IntentionQuoteItem>();

            BatchOperateBtnEnable = false;
            newOrderSingleDic.Clear();
            //newIntentionDic.Clear();
            int matchExefail = 0;

            Dictionary<string, IntentionQuoteItem> orderDic = new Dictionary<string, IntentionQuoteItem>();
            lock (_workingLock)
            {
                foreach (IntentionQuoteItem drv in list)
                {
                    if (drv.Type == "INTENTION_GROUP")
                    {
                        continue;
                    }

                    if (drv.Status == "0" || drv.Status == "80" || drv.Status == "81")
                    {
                        STC.Common.Model.Table.WorkBrench.UpdateIntentionQuoteRequest newIntentionOrder = GetIntentionOrder(drv);
                        newIntentionOrder.securityid = drv.SecurityId;
                        if (LocalEnv.ConfInfo.IsPAYL)
                        {
                            if (STC.Common.Utils.CommonUtils.IsInvestor())
                            {
                                newIntentionOrder.actionFlag = "0";
                            }
                            else
                            {
                                newIntentionOrder.actionFlag = "2";
                            }

                            newIntentionOrder.riskManualCk = "1";
                        }

                        //decimal qty = 0;
                        //if (newIntentionOrder.qty.HasValue)
                        //{
                        //    qty = modifyQty(newIntentionOrder.qty.Value, parameter);
                        //}
                        //else
                        //{
                        //    qty = modifyQty(decimal.Zero, parameter);
                        //}

                        if (qty.HasValue == false || qty.Value == decimal.Zero)
                        {
                            newIntentionOrder.qty = null;
                        }
                        else
                        {
                            newIntentionOrder.qty = qty.Value;
                        }
                        //个需的量不能修改为小于已匹配成交量
                        if (drv.MatchedQty.HasValue && qty.HasValue && drv.MatchedQty.Value > qty.Value)
                        {
                            if (list.Count() == 1)
                            {
                                CustomMessageBox.Info("个需的量不能修改为小于已匹配成交量", "修改报量");
                            }
                            matchExefail++;
                            continue;
                        }

                        //newIntentionDic[newIntentionOrder.intentionId] = newIntentionOrder;
                        drv.UpdateIntentionRequest = newIntentionOrder;
                        drv.Request = null;
                        drv.InquiryRequest = null;
                        orderDic[newIntentionOrder.intentionId] = drv;
                    }
                }
            }

            if (newOrderSingleDic.Count == 0 && orderDic.Count == 0)
            {
                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
                return;
            }


            Task.Run(() =>
            {
                try
                {
                    int failCount = 0;
                    int totalCount = orderDic.Count + newOrderSingleDic.Count + matchExefail;
                    // 修改意向单
                    if (LocalEnv.UserInfo.RoleType == RoleType.BUYER_ROLE && LocalEnv.ConfInfo.IsPAYL)
                    {
                        Sub_UpdateIntention(orderDic);
                    }
                    else
                    {
                        foreach (IntentionQuoteItem item in orderDic.Values)
                        {
                            PResult result = TableOldHandler.Instance.UpdateIntentionQuote(item.UpdateIntentionRequest);
                            if (result == null || !result.IsSuccess())
                            {
                                failCount++;
                            }
                        }
                    }

                    // 修改订单
                    foreach (STC.Common.Model.Fix.Message.NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        RecognizeIntentionQuoteReplyPo result = IntentionOldHandler.Instance.PublishIntention(sendPricePo);
                        if (result == null || !result.IsSuccess())
                        {
                            failCount++;
                        }
                    }

                    string text = string.Empty;
                    if (failCount + matchExefail > 0)
                    {
                        text = $"共执行 {totalCount} 条, 其中 {failCount + matchExefail} 条失败";
                        Common.Utils.CommonUtils.Prompt2Second("批量编辑报量", text);
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }

                BatchOperateBtnEnable = true;
                ATEFunc?.Invoke();
            });

        }


        private List<string> _sideFilterList = new List<string>();

        internal void ProcessSideFilter(List<string> sideFilterList)
        {
            _sideFilterList = sideFilterList;

            ((IEditableCollectionView)WorkingViewSource.View).CommitEdit();
            this.WorkingViewSource.View?.Refresh();
        }
    }


    public class InvestorFilter
    {
        private string _isChecked;

        public string IsChecked
        {
            get { return _isChecked; }
            set { _isChecked = value; }
        }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { _userName = value; }
        }

        private string _name;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

    }
}
