using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Table;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.WorkTable.Plugin.View;
using STC.WorkTable.Plugin.View.WorkTable;
using STC.WorkTable.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.WorkTable.Plugin.ViewModel.WorkTable
{
    public class TradeRequestViewModel : ViewModelBase
    {
        private TradeRequestData _selectedItem_Working;
        public TradeRequestData SelectedItem_Working
        {
            get
            {
                return this._selectedItem_Working;
            }

            set
            {
                this._selectedItem_Working = value;
                this.OnPropertyChanged();
            }
        }

        private TradeRequestData _selectedItem_Ignore;
        public TradeRequestData SelectedItem_Ignore
        {
            get
            {
                return this._selectedItem_Ignore;
            }

            set
            {
                this._selectedItem_Ignore = value;
                this.OnPropertyChanged();
            }
        }

        public ICommand ConfirmCommand { get; set; }

        public ICommand IgnoreCommand { get; set; }

        public ICommand ReorderCommand { get; set; }


        private object _workingLock = new object();
        private CollectionViewSource workingViewSource = new CollectionViewSource();
        public CollectionViewSource WorkingViewSource
        {
            get { return workingViewSource; }
            set { workingViewSource = value; }
        }

        private object _workedLock = new object();
        private CollectionViewSource workedViewSource = new CollectionViewSource();
        public CollectionViewSource WorkedViewSource
        {
            get { return workedViewSource; }
            set { workedViewSource = value; }
        }

        private object _ignoreLock = new object();
        private CollectionViewSource ignoreViewSource = new CollectionViewSource();
        public CollectionViewSource IgnoreViewSource
        {
            get { return ignoreViewSource; }
            set { ignoreViewSource = value; }
        }

        private CustomCollection<TradeRequestData> tradeRequestInfo = new CustomCollection<TradeRequestData>();

        public CustomCollection<TradeRequestData> TradeRequestInfo
        {
            get { return tradeRequestInfo; }
            set { tradeRequestInfo = value; }
        }

        private Predicate<TradeRequestData> pStatus0 { get; } = new Predicate<TradeRequestData>(x => x.Status == "0");
        private Predicate<TradeRequestData> pStatus1 { get; } = new Predicate<TradeRequestData>(x => x.Status == "1");
        private Predicate<TradeRequestData> pStatus3 { get; } = new Predicate<TradeRequestData>(x => x.Status == "3");

        public TradeRequestViewModel()
        {
            try
            {
                BaseHandler.Instance.Init(Instance_OnClear);
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }

            IntentionHandler.Instance.OnOrigins += Instance_OnOrigins;

            this.WorkingViewSource.IsLiveFilteringRequested = true;
            this.WorkingViewSource.IsLiveSortingRequested = true;
            this.WorkingViewSource.LiveFilteringProperties.Add("Status");
            this.WorkingViewSource.Source = TradeRequestInfo;
            this.WorkingViewSource.View.Filter = x => pStatus0((TradeRequestData)x);
            BindingOperations.EnableCollectionSynchronization(this.WorkingViewSource.View, _workingLock);


            this.WorkedViewSource.IsLiveFilteringRequested = true;
            this.WorkedViewSource.IsLiveSortingRequested = true;
            this.WorkedViewSource.LiveFilteringProperties.Add("Status");
            this.WorkedViewSource.Source = TradeRequestInfo;
            this.WorkedViewSource.View.Filter = x => pStatus1((TradeRequestData)x);
            BindingOperations.EnableCollectionSynchronization(this.WorkedViewSource.View, _workedLock);


            this.IgnoreViewSource.IsLiveFilteringRequested = true;
            this.IgnoreViewSource.IsLiveSortingRequested = true;
            this.IgnoreViewSource.LiveFilteringProperties.Add("Status");
            this.IgnoreViewSource.Source = TradeRequestInfo;
            this.IgnoreViewSource.View.Filter = x => pStatus3((TradeRequestData)x);
            BindingOperations.EnableCollectionSynchronization(this.IgnoreViewSource.View, _ignoreLock);

            //this.OrderStatusEnum = TBookStatusEnum.Working;
            this.ConfirmCommand = new ComRelayCommand(this.DoConfirmCommand);
            this.IgnoreCommand = new ComRelayCommand(this.DoIgnoreCommand);
            this.ReorderCommand = new ComRelayCommand(this.DoReorderCommand);
        }

        private void DoConfirmCommand()
        {
            ConfirmIntentionWindow confirmWin = new ConfirmIntentionWindow(this.SelectedItem_Working);
            confirmWin.ShowDialog();
        }

        private void DoIgnoreCommand()
        {
            if (this.SelectedItem_Working != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行忽略操作？", "忽略"))
                {
                    return;
                }

                DoIgnoreOne(this.SelectedItem_Working);
            }
        }


        private async void DoIgnoreOne(TradeRequestData item)
        {
            UpdateIntentionQuoteRequest updateRequest = new UpdateIntentionQuoteRequest();
            updateRequest.userName = LocalEnv.UserInfo.UserName;
            updateRequest.nickName = LocalEnv.UserInfo.FullName;
            updateRequest.intentionId = item.IntentionId;
            //updateRequest.replaceId = Orignal.IntentionId;
            //updateRequest.institutionId = institutionId;
            //updateRequest.institutionName = institutionName;
            //updateRequest.traderId = traderId;
            //updateRequest.traderName = traderName;
            //updateRequest.securityId = securityId;
            //updateRequest.symbol = symbol;
            //updateRequest.side = side;
            //updateRequest.price = price;
            //updateRequest.qty = qty;
            //updateRequest.settlType = settlType;
            updateRequest.status = "3";

            bool isSuccess = await IntentionHandler.Instance.UpdateIntentionQuote(updateRequest);
        }

        private void DoReorderCommand()
        {
            if (this.SelectedItem_Ignore != null)
            {
                if (!Common.View.Component.CustomMessageBox.Question("是否执行恢复操作？", "恢复"))
                {
                    return;
                }

                DoReorderOne(this.SelectedItem_Ignore);
            }
        }

        private async void DoReorderOne(TradeRequestData item)
        {
            UpdateIntentionQuoteRequest updateRequest = new UpdateIntentionQuoteRequest();
            updateRequest.userName = LocalEnv.UserInfo.UserName;
            updateRequest.nickName = LocalEnv.UserInfo.FullName;
            updateRequest.intentionId = item.IntentionId;

            updateRequest.status = "0";

            await IntentionHandler.Instance.UpdateIntentionQuote(updateRequest);
        }


        private void GetErrorInfo(ConfirmIntentionQuoteRequest request,
            ref int failCount,
            ref int totalCount,
            Dictionary<string, List<TPlaceOrderResult>> warnReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic,
            Dictionary<string, List<TPlaceOrderResult>> exceptionReplyDic)
        {

            try
            {
                ConfirmIntentionQuoteReplyPo replyPo = IntentionHandler.Instance.ConfirmIntentionQuote(request);
                if (replyPo == null || (replyPo != null && !replyPo.IsSuccess()))
                {
                    failCount++;

                    if (replyPo != null)
                    {
                        if (replyPo.Code == "10000")
                        {
                            bool isProhibitory = false;
                            List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(replyPo.Msg);
                            foreach (TPlaceOrderResult placeOrderResult in orderRisks)
                            {
                                if (placeOrderResult.Code == "10002")
                                {
                                    isProhibitory = true;
                                    break;
                                }
                            }

                            if (isProhibitory)
                            {
                                prohibitReplyDic[request.intentionId] = orderRisks;
                            }
                            else
                            {
                                warnReplyDic[request.intentionId] = orderRisks;
                            }
                        }
                        else
                        {
                            List<TPlaceOrderResult> exceptionResult = new List<TPlaceOrderResult>();
                            TPlaceOrderResult result = new TPlaceOrderResult();
                            result.Code = "10003";
                            result.DoubleWarn = false;
                            result.Reason = replyPo.Msg;
                            exceptionResult.Add(result);
                            exceptionReplyDic[request.intentionId] = exceptionResult;
                        }
                    }
                }
                else
                {
                    //replyUpdateThread.Enqueue(replyPo);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("批量执行操作", e);
            }
            finally
            {
                totalCount++;
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            //LogHandler.Info("FavoriteBondsListBoxViewModel Clear");
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {

                        if (this.tradeRequestInfo != null)
                        {
                            this.tradeRequestInfo.Clear();
                        }
                        IntentionHandler.Instance.OnOrigins -= Instance_OnOrigins;
                        IntentionHandler.Instance.OnOrigins += Instance_OnOrigins;
                    }));
                }
            }
        }

        private void Instance_OnOrigins(IntentionQuoteInfo obj)
        {
            if (obj.createTime != null && obj.createTime.CompareTo(DateTimeEx.ServerTime.ToString("yyyy-MM-dd")) < 0)
            {
                return;
            }

            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                TradeRequestData item = null;
                var findList = TradeRequestInfo.Where(x => x.IntentionId == obj.intentionId);
                if (findList != null && findList.Count() > 0)
                {
                    item = findList.First();
                    //if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName
                    //    && obj.status != "0")
                    //{
                    //    this.TradeRequestInfo.Remove(item);
                    //    return;
                    //}
                    if (obj.status == "6")
                    {
                        this.TradeRequestInfo.Remove(item);
                        return;
                    }

                    item.Status = obj.status;

                    //if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    //{
                    //    if (string.IsNullOrEmpty(obj.orderId) == false)
                    //    {

                    //        // 更新我的订单/我的撤单
                    //        MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //           );
                    //        // 更新我的成交
                    //        MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //            );
                    //    }
                    //}
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.Side = obj.side;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    if (obj.qty.HasValue)
                        item.DisplayQty = (long)obj.qty / 1000;
                    item.SettlType = obj.settlType;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;

                }
                else
                {
                    if (obj.status == "6")
                    {
                        return;
                    }

                    item = new TradeRequestData();
                    //if (item.TraderName != obj.traderName || item.InstitutionName != obj.institutionName)
                    //{
                    //    if (string.IsNullOrEmpty(obj.orderId) == false)
                    //    {

                    //        // 更新我的订单/我的撤单
                    //        MyOrderProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //           );
                    //        // 更新我的成交
                    //        MyExecutionProcesser.Processer.UpdateCustomInfo(obj.intentionId,
                    //           obj.institutionId, obj.institutionName,
                    //           obj.traderId, obj.traderName
                    //            );
                    //    }
                    //}
                    item.IntentionId = obj.intentionId;
                    item.InstitutionId = obj.institutionId;
                    item.InstitutionName = obj.institutionName;
                    item.TraderId = obj.traderId;
                    item.TraderName = obj.traderName;
                    item.Side = obj.side;
                    item.SecurityId = obj.securityId;
                    item.Symbol = obj.symbol;
                    item.Price = obj.price;
                    item.Qty = obj.qty;
                    if (obj.qty.HasValue)
                        item.DisplayQty = (long)obj.qty / 1000;
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
                    item.Status = obj.status;

                    this.TradeRequestInfo.Insert(0, item);
                }
            }));
        }


        private void GenterateData()
        {
            TradeRequestData x1 = new TradeRequestData();
            x1.IntentionId = "1";
            x1.SecurityId = "210210.IB";
            x1.Side = "1";
            x1.Price = 3.3525M;
            x1.Qty = 10;
            x1.InstitutionName = "中国银行";
            x1.TraderName = "斯琴高娃";
            x1.Text = "210210.IB Bid 3.3525 10kw";
            x1.Status = "0";
            TradeRequestInfo.Add(x1);
        }
    }
}
