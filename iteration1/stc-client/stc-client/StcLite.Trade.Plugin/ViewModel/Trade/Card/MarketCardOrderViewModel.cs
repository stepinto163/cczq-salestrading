using com.gateway.log;
using Newtonsoft.Json;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using STC.Common.Model;
using STC.Common.Utils;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Base;
using STC.Common.Handler.Base;

using Stc.Common.Model.EMS;
using STC.Common.ViewModel.Data;
using STC.Common.Data;
using ExecutionsHandler = BDB.Trade.Plugin.Handler.ExecutionsHandler;
using BDB.Common.Model.OHLC;

namespace BDB.Trade.Plugin.ViewModel.Trade.Card
{
    public class MarketCardOrderViewModel : BaseViewModel
    {

        private Visibility ohlcVisibility = Visibility.Visible;

        public Visibility OhlcVisibility
        {
            get { return ohlcVisibility; }
            set 
            { 
                ohlcVisibility = value;

                this.OnPropertyChanged();

            }
        }


        #region Other
        private string _orderCount = CommonConstants.NO_PRICE;

        public string OrderCount
        {
            get
            {
                return _orderCount;
            }

            set
            {
                _orderCount = value;
                this.OnPropertyChanged();

            }
        }


        private string _closeYield = CommonConstants.NO_PRICE;

        public string CloseYield
        {
            get
            {
                return _closeYield;
            }

            set
            {
                _closeYield = value;
                this.OnPropertyChanged();

            }
        }

        private string _highYield = CommonConstants.NO_PRICE;

        public string HighYield
        {
            get
            {
                return _highYield;
            }

            set
            {
                _highYield = value;
                this.OnPropertyChanged();

            }
        }

        private string _lowYield = CommonConstants.NO_PRICE;

        public string LowYield
        {
            get
            {
                return _lowYield;
            }

            set
            {
                _lowYield = value;
                this.OnPropertyChanged();

            }
        }

        private string _openYld = CommonConstants.NO_PRICE;

        public string OpenYld
        {
            get
            {
                return _openYld;
            }

            set
            {
                _openYld = value;
                this.OnPropertyChanged();

            }
        }


        private string _searchText = string.Empty;

        public string SearchText
        {
            get
            {
                return _searchText;
            }

            set
            {
                //if (_searchText != value)
                {
                    _searchText = value;
                    this.OnPropertyChanged(nameof(SearchText));
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        GetData(0);
                    }));
                }
            }
        }
        #endregion

        /// <summary>
        /// 1:列表模式
        /// 2:卡片模式
        /// </summary>
        private TabShowModel _showModel = TabShowModel.Table;

        public TabShowModel ShowModel {  get { return _showModel; } set { _showModel = value; } }

        #region 记录当前第一个值
        public int Recno = 0;
        public int Recc = 0;
        #endregion

        #region 第一行数据
        private string _Side1 = string.Empty;

        public string Side1
        {
            get
            {
                return _Side1;
            }

            set
            {
                _Side1 = value;
                this.OnPropertyChanged(nameof(Side1));

            }
        }

        private string _ExecutePricePercent_Display1 = string.Empty;

        public string ExecutePricePercent_Display1
        {
            get
            {
                return _ExecutePricePercent_Display1;
            }

            set
            {
                _ExecutePricePercent_Display1 = value;
                this.OnPropertyChanged(nameof(ExecutePricePercent_Display1));

            }
        }

        private string _DownUp1 = string.Empty;

        public string DownUp1
        {
            get
            {
                return _DownUp1;
            }

            set
            {
                _DownUp1 = value;
                this.OnPropertyChanged(nameof(DownUp1));

            }
        }

        private string _Time1 = string.Empty;

        public string Time1
        {
            get
            {
                return _Time1;
            }

            set
            {
                _Time1 = value;
                this.OnPropertyChanged(nameof(Time1));

            }
        }

        private string _DownUpBrush1 = string.Empty;

        public string DownUpBrush1
        {
            get
            {
                return _DownUpBrush1;
            }

            set
            {
                _DownUpBrush1 = value;
                this.OnPropertyChanged(nameof(DownUpBrush1));

            }
        }

        private string _IsValid1 = string.Empty;

        public string IsValid1
        {
            get
            {
                return _IsValid1;
            }

            set
            {
                _IsValid1 = value;
                this.OnPropertyChanged(nameof(IsValid1));
            }
        }

        private string _SourceTrader1 = string.Empty;

        public string SourceTrader1
        {
            get
            {
                return _SourceTrader1;
            }

            set
            {
                _SourceTrader1 = value;
                this.OnPropertyChanged(nameof(SourceTrader1));
            }
        }
        #endregion

        #region 第二行数据
        private string _Side2 = string.Empty;

        public string Side2
        {
            get
            {
                return _Side2;
            }

            set
            {
                _Side2 = value;
                this.OnPropertyChanged(nameof(Side2));

            }
        }

        private string _ExecutePricePercent_Display2 = string.Empty;

        public string ExecutePricePercent_Display2
        {
            get
            {
                return _ExecutePricePercent_Display2;
            }

            set
            {
                _ExecutePricePercent_Display2 = value;
                this.OnPropertyChanged(nameof(ExecutePricePercent_Display2));

            }
        }

        private string _DownUp2 = string.Empty;

        public string DownUp2
        {
            get
            {
                return _DownUp2;
            }

            set
            {
                _DownUp2 = value;
                this.OnPropertyChanged(nameof(DownUp2));

            }
        }

        private string _Time2 = string.Empty;

        public string Time2
        {
            get
            {
                return _Time2;
            }

            set
            {
                _Time2 = value;
                this.OnPropertyChanged(nameof(Time2));

            }
        }

        private string _DownUpBrush2 = string.Empty;

        public string DownUpBrush2
        {
            get
            {
                return _DownUpBrush2;
            }

            set
            {
                _DownUpBrush2 = value;
                this.OnPropertyChanged(nameof(DownUpBrush2));

            }
        }

        private string _IsValid2 = string.Empty;

        public string IsValid2
        {
            get
            {
                return _IsValid2;
            }

            set
            {
                _IsValid2 = value;
                this.OnPropertyChanged(nameof(IsValid2));
            }
        }

        private string _SourceTrader2 = string.Empty;

        public string SourceTrader2
        {
            get
            {
                return _SourceTrader2;
            }

            set
            {
                _SourceTrader2 = value;
                this.OnPropertyChanged(nameof(SourceTrader2));
            }
        }
        #endregion

        #region 第三行数据
        private string _Side3 = string.Empty;

        public string Side3
        {
            get
            {
                return _Side3;
            }

            set
            {
                _Side3 = value;
                this.OnPropertyChanged(nameof(Side3));

            }
        }

        private string _ExecutePricePercent_Display3 = string.Empty;

        public string ExecutePricePercent_Display3
        {
            get
            {
                return _ExecutePricePercent_Display3;
            }

            set
            {
                _ExecutePricePercent_Display3 = value;
                this.OnPropertyChanged(nameof(ExecutePricePercent_Display3));

            }
        }

        private string _DownUp3 = string.Empty;

        public string DownUp3
        {
            get
            {
                return _DownUp3;
            }

            set
            {
                _DownUp3 = value;
                this.OnPropertyChanged(nameof(DownUp3));

            }
        }

        private string _Time3 = string.Empty;

        public string Time3
        {
            get
            {
                return _Time3;
            }

            set
            {
                _Time3 = value;
                this.OnPropertyChanged(nameof(Time3));

            }
        }

        private string _DownUpBrush3 = string.Empty;

        public string DownUpBrush3
        {
            get
            {
                return _DownUpBrush3;
            }

            set
            {
                _DownUpBrush3 = value;
                this.OnPropertyChanged(nameof(DownUpBrush3));

            }
        }

        private string _IsValid3 = string.Empty;

        public string IsValid3
        {
            get
            {
                return _IsValid3;
            }

            set
            {
                _IsValid3 = value;
                this.OnPropertyChanged(nameof(IsValid3));
            }
        }

        private string _SourceTrader3 = string.Empty;

        public string SourceTrader3
        {
            get
            {
                return _SourceTrader3;
            }

            set
            {
                _SourceTrader3 = value;
                this.OnPropertyChanged(nameof(SourceTrader3));
            }
        }
        #endregion


        ~MarketCardOrderViewModel()
        {
            Dispose();
        }

        new public void Dispose()
        {
            ExecutionsHandler.Instance.OnMarketExecutions -= Instance_OnMarketExecutions;
            OhlcHandler.Instance.UnSubscribeOhlc(this.SearchText, onOHLC);
            base.Dispose();
        }


        private object lockObj = new object();
        private object ohlcLock = new object();


        public void ClearData(bool isShowLabel)
        {
            lock (this.lockObj)
            {
                this.OpenYld = this.LowYield = this.OrderCount = this.HighYield = this.CloseYield = CommonConstants.NO_PRICE;
                Recno = 0;
                Recc = 0;
                ClearOrderData();

                if (!isShowLabel)
                {
                    OhlcVisibility = Visibility.Collapsed;

                    this.SearchText = string.Empty;
                }
                else
                {
                    OhlcVisibility = Visibility.Visible;
                }
            }
        }

        private void Instance_OnClear(ClearPo obj)
        {
            LogHelper.Info(string.Format("MarketOrderViewModel Clear, security:{0}", this.SearchText));
            if (obj != null && "Y".Equals(obj.IsClear))
            {

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearData(true);
                }));

            }
        }

        public void Init()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (ohlcVisibility == Visibility.Collapsed)
                    {
                        OhlcVisibility = Visibility.Visible;
                    }
                }

                BaseHandler.Instance.Init(Instance_OnClear);

                try
                {
                    Recno = 0;
                    #region 收MarketExec消息0
                    ExecutionsHandler.Instance.OnMarketExecutions += Instance_OnMarketExecutions;
                    #endregion


                    #region 订阅OHLC

                    OhlcHandler.Instance.SubscribeOhlc(this.SearchText, onOHLC);

                    #endregion

                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }));
        }

        public void onOHLC(OhlcMsg ohlcMsg)
        {

            if (ohlcMsg.securityid.ToLower() == SearchText.ToLower())
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (ohlcMsg.close != 0)
                        this.CloseYield = ohlcMsg.close.ToString();

                    if (ohlcMsg.high != 0)
                        this.HighYield = ohlcMsg.high.ToString();

                    if (ohlcMsg.numTrades != 0)
                        this.OrderCount = ohlcMsg.numTrades.ToString();

                    if (ohlcMsg.low != 0)
                        this.LowYield = ohlcMsg.low.ToString();

                    if (ohlcMsg.open != 0)
                        this.OpenYld = ohlcMsg.open.ToString();

                    if (ohlcVisibility == Visibility.Collapsed)
                    {
                        OhlcVisibility = Visibility.Visible;
                    }
                }));
            }
        }


        private void GetData(int recno)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                List<MarketExecutionsItem> FullData = MarketExecutionProcesser.Processer.MyMarketExes.ToList().FindAll(delegate (MarketExecutionsItem p) { return p.SecurityID.ToString().ToLower() == SearchText.ToLower(); }).OrderByDescending(o => o.Time).ToList();
            List<MarketExecutionsItem> lstMeI = new List<MarketExecutionsItem>();

            if (FullData.Count > 0)
            {
                lstMeI = FullData.GetRange(recno, FullData.Count- recno > 3 ? 3 : FullData.Count - recno);

                // 更新最新的 HOLC
                //MarketExecutionsItem newMEI = FullData.GetRange(0, 1)[0];
                //RefreshHOLC(newMEI.Info1);
            }

            ClearOrderData();

            Recc = FullData.Count;

            if (lstMeI.Count > 0)
            {
                MarketExecutionsItem mi = lstMeI[0];
                this.DownUp1 = mi.DownUp;
                this.DownUpBrush1 = mi.DownUpBrush;
                this.ExecutePricePercent_Display1 = mi.ExecutePricePercent_Display;
                this.IsValid1 = mi.IsValid;
                this.Side1 = mi.Side;
                this.SourceTrader1 = mi.SourceTrader;
                this.Time1 = mi.Time;
            }

            if (lstMeI.Count > 1)
            {
                MarketExecutionsItem mi = lstMeI[1];
                this.DownUp2 = mi.DownUp;
                this.DownUpBrush2 = mi.DownUpBrush;
                this.ExecutePricePercent_Display2 = mi.ExecutePricePercent_Display;
                this.IsValid2 = mi.IsValid;
                this.Side2 = mi.Side;
                this.SourceTrader2 = mi.SourceTrader;
                this.Time2 = mi.Time;
            }

                if (lstMeI.Count > 2)
                {
                    MarketExecutionsItem mi = lstMeI[2];
                    this.DownUp3 = mi.DownUp;
                    this.DownUpBrush3 = mi.DownUpBrush;
                    this.ExecutePricePercent_Display3 = mi.ExecutePricePercent_Display;
                    this.IsValid3 = mi.IsValid;
                    this.Side3 = mi.Side;
                    this.SourceTrader3 = mi.SourceTrader;
                    this.Time3 = mi.Time;
                }
            }));
        }

        /// <summary>
        /// 鼠标滚轮移动
        /// </summary>
        /// <param name="isup">1 正在向上滚动滑轮;0 正在向下滚动滑轮</param>
        public void MouseWheel(int isup)
        {
            int recc = 0;
            Recno = isup == 1 ? Recno -1 : Recno + 1;
            Recno = Recno > 0 ? Recno : 0;
            Recno = Recno > Recc - 1 ? Recc - 1 : Recno;
            GetData(Recno);
        }

        /// <summary>
        /// 收取成交数据
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnMarketExecutions(ExecutionReport obj)
        {
            if (obj.SecurityID.ToLower() == SearchText.ToLower())
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    //UpdateMarketExecutions(obj);
                    GetData(Recno);
                }));
            }
        }


        /// <summary>
        /// 更新数据， 好像没有删除数据？？还有很多字段不知道什么意思??key也不确定
        /// </summary>
        /// <param name="obj"></param>
        private void UpdateMarketExecutions(ExecutionReport obj)
        {
            //if (string.IsNullOrEmpty(SearchText) || (_showModel == TabShowModel.Card && !string.IsNullOrEmpty(SearchText) && !SearchText.Equals(obj.SecurityID)))
            //    return;

            //lock (lockObj)
            //{
            //    if (obj != null && !string.IsNullOrEmpty(obj.Info1))
            //    {
            //        MarketCount market = JsonConvert.DeserializeObject<MarketCount>(obj.Info1);

            //        if (!string.IsNullOrEmpty(SearchText) && obj.SecurityID.Equals(SearchText))
            //        {
            //            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //            {
            //                if (!string.IsNullOrEmpty(market.CloseYield))
            //                    this.CloseYield = market.CloseYield;
            //                else
            //                    this.CloseYield = CommonConstants.NO_PRICE;

            //                if (!string.IsNullOrEmpty(market.HighYield))
            //                    this.HighYield = market.HighYield;
            //                else
            //                    this.HighYield = CommonConstants.NO_PRICE;

            //                if (market.TotalNumTrades != 0)
            //                    this.OrderCount = market.TotalNumTrades.ToString();
            //                else
            //                    this.OrderCount = CommonConstants.NO_PRICE;

            //                if (!string.IsNullOrEmpty(market.LowYield))
            //                    this.LowYield = market.LowYield;
            //                else
            //                    this.LowYield = CommonConstants.NO_PRICE;

            //                if (!string.IsNullOrEmpty(market.OpenYld))
            //                    this.OpenYld = market.OpenYld;
            //                else
            //                    this.OpenYld = CommonConstants.NO_PRICE;
            //            }));

            //        }
            //    }
            //}
        }

        /// <summary>
        /// 更新 HOLC
        /// </summary>
        /// <param name="info1"></param>
        private void RefreshHOLC(string info1)
        {
            if (!string.IsNullOrEmpty(info1))
            {
                MarketCount market = JsonConvert.DeserializeObject<MarketCount>(info1);

                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!string.IsNullOrEmpty(market.CloseYield))
                        this.CloseYield = market.CloseYield;

                    if (!string.IsNullOrEmpty(market.HighYield))
                        this.HighYield = market.HighYield;

                    if (market.TotalNumTrades != 0)
                        this.OrderCount = market.TotalNumTrades.ToString();

                    if (!string.IsNullOrEmpty(market.LowYield))
                        this.LowYield = market.LowYield;

                    if (!string.IsNullOrEmpty(market.OpenYld))
                        this.OpenYld = market.OpenYld;
                }));

            }
        }
        /// <summary>
        /// 清空数据
        /// </summary>
        private void ClearOrderData()
        {
            DownUp1 = DownUpBrush1 = ExecutePricePercent_Display1 = IsValid1 = Side1 = SourceTrader1 = Time1 = "";
            DownUp2 = DownUpBrush2 = ExecutePricePercent_Display2 = IsValid2 = Side2 = SourceTrader2 = Time2 = "";
            DownUp3 = DownUpBrush3 = ExecutePricePercent_Display3 = IsValid3 = Side3 = SourceTrader3 = Time3 = "";
        }
    }
}
