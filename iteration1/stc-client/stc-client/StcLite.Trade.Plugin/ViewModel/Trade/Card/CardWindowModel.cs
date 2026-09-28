using BDB.Common.Model.OHLC;
using BDB.Price.Plugin.Model;
using BDB.Price.Plugin.View.Trade.Card;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.View.Trade.Card;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BDB.Price.Plugin.ViewModel.Trade.Card
{
    public class CardWindowModel : BaseViewModel
    {
        private FrameworkElement _ownUI;
        public FrameworkElement OwnUI { get { return _ownUI; } set { _ownUI = value; } }
        public string SecurityId { get; set; }

        #region 业务处理

        private object _lockObj = new object();

        private AsyncThreadQueue<ExecutionReport> _ProcessLastExecutionsThreadQueue;
        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _ProcessCtPriceThreadQueue;
        private AsyncThreadQueue<OhlcMsg> _ProcessOHLCThreadQueue;

        private void InitSource()
        {
            BaseHandler.Instance.Init(Instance_OnClear);
            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;
            if (_ProcessLastExecutionsThreadQueue == null)
            {
                _ProcessLastExecutionsThreadQueue = new AsyncThreadQueue<ExecutionReport>(UpdateLastExecutionsAsync);
            }

            if (_ProcessCtPriceThreadQueue == null)
            {
                _ProcessCtPriceThreadQueue = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(UpdateMDData);
            }

            if (_ProcessOHLCThreadQueue == null)
            {
                _ProcessOHLCThreadQueue = new AsyncThreadQueue<OhlcMsg>(UpdateOHLC);
            }
        }

        private void Instance_OnSettingChange(object obj)
        {
            if (obj is NewSettingMsg)
            {
                NewSettingMsg msg = (NewSettingMsg)obj;
                if (msg.key == "SettingBpMode")
                {
                    if (_ownUI != null)
                    {
                        CardWindow card = _ownUI as CardWindow;
                        PriceAndOrderViewModel priceAndOrderViewModel = card.DataContext as PriceAndOrderViewModel;
                        MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(card.SecurityId);
                        if (lastExeItem != null)
                        {
                            ExecutionReport temOrder = new ExecutionReport();
                            temOrder.SecurityID = card.SecurityId;
                            temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                            temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                            temOrder.Venues = lastExeItem.SourceTrader;

                            priceAndOrderViewModel.OnLastExec(temOrder);
                        }
                    }
                }
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("CardWindowModel Clear, SecurityId:{0}", SecurityId));
            if (obj != null && "Y".Equals(obj.IsClear))
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    lock (this._lockObj)
                    {
                        PriceAndOrderView priceAndOrderView = GetPriceAndOrderView();
                        if (priceAndOrderView != null)
                        {
                            PriceAndOrderViewModel priceAndOrderViewModel = priceAndOrderView.DataContext as PriceAndOrderViewModel;
                            if (priceAndOrderViewModel != null)
                            {
                                priceAndOrderViewModel.OnClear();

                                PriceHandler.Instance.UnregisterCTPrice(priceAndOrderViewModel.PriceModel.Security, Instance_OnMDPrice);


                                #region 订阅OHLC
                                bool isFuture = CommonUtils.IsFutureBond(priceAndOrderViewModel.PriceModel.Security);
                                if (isFuture)
                                {
                                    //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
                                }
                                else
                                {
                                    OhlcHandler.Instance.UnSubscribeOhlc(SecurityId, onOHLC);
                                }
                                #endregion
                            }
                        }

                        if (_ProcessLastExecutionsThreadQueue != null)
                        {
                            _ProcessLastExecutionsThreadQueue.Clear();
                        }

                        if (_ProcessCtPriceThreadQueue != null)
                        {
                            _ProcessCtPriceThreadQueue.Clear();
                        }


                        ResetSub();
                    }
                }));

            }
        }

        private void ResetSub()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    Thread.Sleep(3000);

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        lock (this._lockObj)
                        {
                            PriceAndOrderView priceAndOrderView = GetPriceAndOrderView();
                            if (priceAndOrderView != null)
                            {
                                PriceAndOrderViewModel priceAndOrderViewModel = priceAndOrderView.DataContext as PriceAndOrderViewModel;
                                if (priceAndOrderViewModel != null)
                                {
                                    PriceHandler.Instance.RegisterCTPrice(priceAndOrderViewModel.PriceModel.Security, Instance_OnMDPrice);

                                    #region 订阅OHLC
                                    bool isFuture = CommonUtils.IsFutureBond(priceAndOrderViewModel.PriceModel.Security);
                                    if (isFuture)
                                    {
                                        //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
                                    }
                                    else
                                    {
                                        OhlcHandler.Instance.SubscribeOhlc(priceAndOrderViewModel.PriceModel.Security, onOHLC);
                                    }
                                    #endregion

                                    priceAndOrderViewModel.InitData();
                                }
                            }
                        }
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error("ResetSub", e);
                }
            });
        }

        private void Instance_OnLastExecutions(ExecutionReport obj)
        {
            if (obj != null && _ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Enqueue(obj);
            }
        }

        private PriceAndOrderView GetPriceAndOrderView()
        {
            PriceAndOrderView priceAndOrderView = _ownUI.FindName("card") as PriceAndOrderView;

            return priceAndOrderView;
        }

        private void UpdateLastExecutionsAsync(ExecutionReport obj)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PriceAndOrderView priceAndOrderView = GetPriceAndOrderView();
                if (priceAndOrderView != null)
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = priceAndOrderView.DataContext as PriceAndOrderViewModel;
                    if (priceAndOrderViewModel != null && priceAndOrderViewModel.PriceModel.Security == obj.SecurityID)
                    {
                        priceAndOrderViewModel.OnLastExec(obj);
                    }
                }
            }));
        }

        private void UpdateMDData(MarketDataSnapshotFullRefresh data)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PriceAndOrderView priceAndOrderView = GetPriceAndOrderView();
                if (priceAndOrderView != null)
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = priceAndOrderView.DataContext as PriceAndOrderViewModel;
                    if (priceAndOrderViewModel != null && priceAndOrderViewModel.PriceModel.Security == data.SecurityID)
                    {
                        priceAndOrderViewModel.OnPrice(data, true);
                    }
                }
            }));
        }

        private void Instance_OnMDPrice(MarketDataSnapshotFullRefresh obj)
        {
            if (_ProcessCtPriceThreadQueue != null)
                _ProcessCtPriceThreadQueue.Enqueue(obj);

        }


        #region OHLC
        public void onOHLC(OhlcMsg ohlcMsg)
        {
            if (_ProcessOHLCThreadQueue != null)
                _ProcessOHLCThreadQueue.Enqueue(ohlcMsg);
        }

        private void UpdateOHLC(OhlcMsg data)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                PriceAndOrderView priceAndOrderView = GetPriceAndOrderView();
                if (priceAndOrderView != null)
                {
                    PriceAndOrderViewModel priceAndOrderViewModel = priceAndOrderView.DataContext as PriceAndOrderViewModel;
                    if (priceAndOrderViewModel != null && priceAndOrderViewModel.PriceModel.Security == data.securityid)
                    {
                        priceAndOrderViewModel.onOHLC(data);
                    }
                }
            }));
        }

        #endregion

        #endregion

        public void Reset(string oldSecurityId, string securityId)
        {
            if (_ownUI != null)
            {
                CardWindow card = _ownUI as CardWindow;
                PriceAndOrderViewModel priceAndOrderViewModel = card.DataContext as PriceAndOrderViewModel;

                priceAndOrderViewModel.OnClear();

                if (_ProcessLastExecutionsThreadQueue != null)
                {
                    _ProcessLastExecutionsThreadQueue.Clear();
                }

                if (_ProcessCtPriceThreadQueue != null)
                {
                    _ProcessCtPriceThreadQueue.Clear();
                }

                if (_ProcessOHLCThreadQueue != null)
                {
                    _ProcessOHLCThreadQueue.Clear();
                }

                LastExecutionProcesser.Processer.OnLastExecutions -= Instance_OnLastExecutions;
                PriceHandler.Instance.UnregisterCTPrice(oldSecurityId, Instance_OnMDPrice);

                #region 订阅OHLC
                bool isFuture = CommonUtils.IsFutureBond(priceAndOrderViewModel.PriceModel.Security);
                if (isFuture)
                {
                    //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
                }
                else
                {
                    OhlcHandler.Instance.UnSubscribeOhlc(oldSecurityId, onOHLC);
                }
                #endregion

                priceAndOrderViewModel.CardShowType = CardShowType.AloneWindow;

                priceAndOrderViewModel.PriceModel.PopVisibility = Visibility.Collapsed;
                priceAndOrderViewModel.PriceModel.Security = securityId;
                priceAndOrderViewModel.PriceModel.EditVisibility = Visibility.Visible;

                priceAndOrderViewModel.InitByPageChange(GetPriceAndOrderView(), securityId, string.Empty, string.Empty);
                priceAndOrderViewModel.SetAloneWindowModel();
                PriceHandler.Instance.RegisterCTPrice(securityId, Instance_OnMDPrice);

                #region 订阅OHLC
                
                if (isFuture)
                {
                    //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
                }
                else
                {
                    OhlcHandler.Instance.SubscribeOhlc(securityId, onOHLC);
                }
                #endregion

                ExecutionReport obj = new ExecutionReport();
                obj.SecurityID = securityId;

                LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnLastExecutions;

                MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(securityId);
                if (lastExeItem != null)
                {
                    ExecutionReport temOrder = new ExecutionReport();
                    temOrder.SecurityID = securityId;
                    temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                    temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                    temOrder.Venues = lastExeItem.SourceTrader;

                    priceAndOrderViewModel.OnLastExec(temOrder);
                }

                priceAndOrderViewModel.InitData();

                priceAndOrderViewModel.InitMarketDataByCardWind();

                BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(priceAndOrderViewModel.PriceModel.Security);
                if (bondsInfoPo != null)
                {
                    priceAndOrderViewModel.RemaingingTimeConvert = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);//剩余时间转换成时间段
                                                                                                                                //cardItem.BondType = CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);// 债券类型
                    priceAndOrderViewModel.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                }
            }
        }

        public void Init(FrameworkElement ui, string securityId)
        {
            if (_ownUI == null)
            {
                _ownUI = ui;
            }

            InitSource();

            CardWindow card = _ownUI as CardWindow;
            PriceAndOrderViewModel priceAndOrderViewModel = new PriceAndOrderViewModel();
            card.DataContext = priceAndOrderViewModel;

            priceAndOrderViewModel.CardShowType = Model.CardShowType.AloneWindow;
            
            priceAndOrderViewModel.PriceModel.PopVisibility = Visibility.Collapsed;
            priceAndOrderViewModel.PriceModel.Security = securityId;

            priceAndOrderViewModel.InitByPageChange(GetPriceAndOrderView(), securityId, string.Empty, string.Empty);
            priceAndOrderViewModel.SetAloneWindowModel();
            PriceHandler.Instance.RegisterCTPrice(securityId, Instance_OnMDPrice);

            #region 订阅OHLC
            bool isFuture = CommonUtils.IsFutureBond(securityId);
            if (isFuture)
            {
                //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
            }
            else
            {
                OhlcHandler.Instance.SubscribeOhlc(securityId, onOHLC);
            }
            #endregion

            ExecutionReport obj = new ExecutionReport();
            obj.SecurityID = securityId;

            LastExecutionProcesser.Processer.OnLastExecutions += Instance_OnLastExecutions;

            MarketExecutionsItem lastExeItem = LastExecutionProcesser.Processer.GetLastExecutionsItem(SecurityId);
            if (lastExeItem != null)
            {
                ExecutionReport temOrder = new ExecutionReport();
                temOrder.SecurityID = SecurityId;
                temOrder.Side = lastExeItem.Side == CommonConstants.SIDE_X ? CommonConstants.SIDE_OFR_TWO : lastExeItem.Side == CommonConstants.SIDE_Y ? CommonConstants.SIDE_BID_ONE : lastExeItem.Side == CommonConstants.SIDE_Z ? CommonConstants.SIDE_TRD_THREE : lastExeItem.Side;
                temOrder.LastPx = lastExeItem.ExecutePricePercent.ToString();
                temOrder.Venues = lastExeItem.SourceTrader;

                priceAndOrderViewModel.OnLastExec(temOrder);
            }

            priceAndOrderViewModel.InitData();

            priceAndOrderViewModel.InitMarketDataByCardWind();

            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(priceAndOrderViewModel.PriceModel.Security);
            if (bondsInfoPo != null)
            {
                priceAndOrderViewModel.RemaingingTimeConvert = CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);//剩余时间转换成时间段
                                                                                                                            //cardItem.BondType = CommonUtils.GetBondCategoryName(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);// 债券类型
                priceAndOrderViewModel.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
            }
        }

        public void Clear()
        {
            BaseHandler.Instance.UnSubscribe(Instance_OnClear);

            // 订阅我的订单
            LastExecutionProcesser.Processer.OnLastExecutions -= Instance_OnLastExecutions;

            PriceHandler.Instance.UnregisterCTPrice(SecurityId, Instance_OnMDPrice);

            #region 订阅OHLC
            bool isFuture = CommonUtils.IsFutureBond(SecurityId);
            if (isFuture)
            {
                //PriceHandler.Instance.RegisterCTPrice(security, Instance_OnMDPrice);
            }
            else
            {
                OhlcHandler.Instance.UnSubscribeOhlc(SecurityId, onOHLC);
            }
            #endregion

            if (_ProcessLastExecutionsThreadQueue != null)
            {
                _ProcessLastExecutionsThreadQueue.Dispose();
            }

            if (_ProcessCtPriceThreadQueue != null)
            {
                _ProcessCtPriceThreadQueue.Dispose();
            }

            if (_ProcessOHLCThreadQueue != null)
            {
                _ProcessOHLCThreadQueue.Dispose();
            }
        }

        new public void Dispose()
        {
            Clear();
        }

    }
}
