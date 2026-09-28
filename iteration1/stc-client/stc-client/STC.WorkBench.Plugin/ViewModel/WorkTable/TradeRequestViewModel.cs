using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Base;
using STC.Common.Model.Base;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.WorkBench.Plugin.ViewModel.WorkTable
{
    public class TradeRequestViewModel : ViewModelBase
    {
        public ICommand ConfirmCommand { get; set; }

        public ICommand IgnoreCommand { get; set; }

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
        private Predicate<TradeRequestData> pStatus1_2 { get; } = new Predicate<TradeRequestData>(x => (x.Status == "1" || x.Status == "2"));
        private Predicate<TradeRequestData> pStatus3 { get; } = new Predicate<TradeRequestData>(x => x.Status == "3");

        public TradeRequestViewModel()
        {
            GenterateData();

            try
            {
                BaseHandler.Instance.Init(Instance_OnClear);
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }

            IntentionOldHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;

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
            this.WorkedViewSource.View.Filter = x => pStatus1_2((TradeRequestData)x);
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
        }

        private void DoConfirmCommand()
        {
            throw new NotImplementedException();
        }

        private void DoIgnoreCommand()
        {
            throw new NotImplementedException();
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
                        IntentionOldHandler.Instance.OnIntentionQuotes -= Instance_OnIntentionQuotes;
                        IntentionOldHandler.Instance.OnIntentionQuotes += Instance_OnIntentionQuotes;
                    }));
                }
            }
        }

        private void Instance_OnIntentionQuotes(STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                TradeRequestData item = null;
                var findList = TradeRequestInfo.Where(x => x.IntentionId == obj.intentionId);
                if (findList != null && findList.Count() > 0)
                {
                    item = findList.First();
                    if (!string.IsNullOrEmpty(obj.userName) && obj.userName != LocalEnv.UserInfo.UserName
                        && obj.status != "0")
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
                    item.SettlType = obj.settlType;
                    item.UpdateTime = obj.updateTime;
                    item.UserName = obj.userName;

                }
                else
                {
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

                    this.TradeRequestInfo.Add(item);
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
