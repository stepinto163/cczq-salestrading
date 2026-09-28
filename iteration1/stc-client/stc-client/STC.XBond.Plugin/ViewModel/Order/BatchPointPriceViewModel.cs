using netapi.Api;
using STC.Common.Api;
using STC.Common.Handler.Price;
using STC.Common.Handler.SystemParam;
using STC.Common.Model;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.DataProcesser;
using STC.Common.ViewModel.Data;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using STC.XBond.Plugin.View.Order;
using STC.XBond.Plugin.ViewModel.Adapter;
using STC.XBond.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using UserSetting = STC.Common.Model.TBook.UserSetting;

namespace STC.XBond.Plugin.ViewModel.Order
{
    public class BatchPointPriceViewModel : BaseViewModel
    {
        private FrameworkElement _parentUI = null;
        private BatchPointPriceDomain _batchPointPriceDomain = new BatchPointPriceDomain();

        public BatchPointPriceDomain BatchPointPriceDomain
        {
            get { return _batchPointPriceDomain; }
        }

        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }
        public ICommand AddAmountCommand { get { return new RelayCommand(AddAmountClick); } }
        public ICommand DownAmountCommand { get { return new RelayCommand(DownAmountClick); } }
        public ICommand OkCommand { get { return new RelayCommand(OkClick); } }

        /// <summary>
        /// 与5 秒钟内最新成交不一致提醒开关
        /// </summary>
        public bool PointPriceConsistencyCheck = false;
        public bool SendPriceConsistencyCheck = false;
        public string PointPriceQty;

        private DispatcherTimer timer = null;
        private bool isHvaeNewExecution = false;

        /// <summary>
        /// 最后成交时间
        /// </summary>
        private string _LastUpdateTime = string.Empty;
        public string LastUpdateTime
        {
            get { return _LastUpdateTime; }
            set
            {
                _LastUpdateTime = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// 最后成交价格
        /// </summary>
        private string _LastPrice = string.Empty;
        public string LastPrice
        {
            get { return _LastPrice; }
            set
            {
                _LastPrice = value;
                this.OnPropertyChanged();
            }
        }

        private bool _updateFromTable = false;

        public bool UpdateFromTable
        {
            get { return _updateFromTable; }
        }

        private SendPriceModelAdapter _sendPriceAdapter = new SendPriceModelAdapter();

        private SendPriceItem sendPriceItem = null;

        public SendPriceItem SendPriceItem
        {
            get { return sendPriceItem; }
            set
            {
                if (value != null)
                {
                    sendPriceItem = value;
                    this.OnPropertyChanged(nameof(SendPriceItem));
                }
            }
        }


        private void CloseClick()
        {
            Window window = _parentUI as Window;
            window.Close();
        }

        public void QuickQty(string qty)
        {
            decimal d = 0;
            if (decimal.TryParse(qty, out d))
            {
                _updateFromTable = false;
                _batchPointPriceDomain.Qty = d.ToString();
            }
        }

        public void QuickAllQty()
        {
            _updateFromTable = false;

            decimal total = GetPriceTableViewModel().GetAllFilterQty();

            BatchPointPriceDomain.Qty = total.ToString();
            
        }
        

        private void AddAmountClick()
        {
            _batchPointPriceDomain.ListBoxSelectIndex = -1;

            decimal d = 0;
            if (decimal.TryParse(_batchPointPriceDomain.Qty, out d))
            {
                _batchPointPriceDomain.Qty = (d + 1).ToString();

                _updateFromTable = false;
                GetPriceTableViewModel().ResetClickQty(d + 1);
            }
        }
        
        private void DownAmountClick()
        {
            _batchPointPriceDomain.ListBoxSelectIndex = -1;

            decimal d = 0;
            if (decimal.TryParse(_batchPointPriceDomain.Qty, out d) && (d - 1) > 0)
            {
                _updateFromTable = false;

                _batchPointPriceDomain.Qty = (d - 1).ToString();

                GetPriceTableViewModel().ResetClickQty(d - 1);
            }
        }

        public void UpdateTotal(decimal clickQty)
        {
            _updateFromTable = true;

            _batchPointPriceDomain.ListBoxSelectIndex = -1;

            BatchPointPriceDomain.Qty = clickQty.ToString();

            //_batchPointPriceDomain.MatchTotalQty = string.Format("/{0}", clickQty);
            _batchPointPriceDomain.ClickQty = clickQty.ToString();
        }

        public void ResetClickQty(string qty)
        {
            decimal d = 0;
            if (decimal.TryParse(qty, out d))
            {
                GetPriceTableViewModel().ResetClickQty(d);

                //_batchPointPriceDomain.MatchTotalQty = string.Format("/{0}", qty);
                _batchPointPriceDomain.ClickQty = qty;
            }
        }

        private PriceTableViewModel GetPriceTableViewModel()
        {
            PriceTableView priceTableView = this._parentUI.FindName("priceTableView") as PriceTableView;
            PriceTableViewModel priceTableViewModel = priceTableView.DataContext as PriceTableViewModel;

            return priceTableViewModel;
        }

        public void Filter(List<string> settleList)
        {
            PriceTableView priceTableView = this._parentUI.FindName("priceTableView") as PriceTableView;
            PriceTableViewModel priceTableViewModel = priceTableView.DataContext as PriceTableViewModel;

            priceTableViewModel.Filter(settleList);
        }

        private void GetBestPrice(string side, string securityId, string pointPrice)
        {
            string primaryKey = ApiManager.Instance.GetDefaultApi().getTopic(ApiConsts.Topic_stc_md_prefix, ApiConsts.CT, LocalEnv.UserInfo.Termids, securityId, string.Empty, string.Empty);
            MarketDataSnapshotFullRefresh ctPrice = MDHandler.Instance.GetCachePriceByTopic(primaryKey);
            
            List<GNoMDEntries> noMDEntries = ctPrice.NoMDEntries?.Where(x => x.MDEntryPx == pointPrice).ToList();


            decimal totalAmount = 0;
            List<string> settleList = new List<string>();

            if (noMDEntries != null && noMDEntries.Count > 0)
            {
                foreach (GNoMDEntries priceMatch in noMDEntries)
                {
                    decimal d = 0;
                    if (decimal.TryParse(priceMatch.MDEntrySize, out d))
                        totalAmount += d;

                    if (!settleList.Contains(priceMatch.SettlType))
                    {
                        settleList.Add(priceMatch.SettlType);
                    }
                }

                _batchPointPriceDomain.Qty = ((int)(totalAmount/1000)).ToString();
            }

            _batchPointPriceDomain.MatchTotalQty = string.Format("/{0}", (int)(totalAmount/1000));
            _batchPointPriceDomain.ClickQty = ((int)(totalAmount / 1000)).ToString();
            _batchPointPriceDomain.Price = pointPrice;

            SetSettle(settleList);

            SetPriceTable(noMDEntries);

        }

        private void SetSettle(List<string> settleList)
        {
            ListBox settleListBox = this._parentUI.FindName("LstSettleBox") as ListBox;

            bool addOne = false;
            bool tomAddOne = false;

            foreach (string settle in settleList)
            {
                if (settle == "2" || settle == "T+1")
                    addOne = true;
                else if (settle == "3" || settle == "明天+0")
                    tomAddOne = true;
                else if (settle == "2,3" || settle == "T+1(不限)")
                    addOne = tomAddOne = true;
                else
                {
                    ListBoxItemData listBoxItemData = new ListBoxItemData();
                    listBoxItemData.ItemEnable = true;
                    listBoxItemData.Name = ComboSourceUtil.Instance.FindEnum(settle, "settletype_myorder");

                    _batchPointPriceDomain.SettleSource.Add(listBoxItemData);
                }
            }

            if (addOne && tomAddOne)
                _batchPointPriceDomain.SettleSelectIndex = 0;
            else if (!addOne && !tomAddOne)
                _batchPointPriceDomain.SettleSelectIndex = -1;
            else 
            {
                _batchPointPriceDomain.SettleSelectIndex = addOne ? 1 : tomAddOne ? 2 : -1;
            }
        }

        private void SetPriceTable(List<GNoMDEntries> noMDEntries)
        {
            PriceTableView priceTableView = this._parentUI.FindName("priceTableView") as PriceTableView;
            PriceTableViewModel priceTableViewModel = priceTableView.DataContext as PriceTableViewModel;

            foreach (GNoMDEntries priceMatchParam in noMDEntries)
                priceTableViewModel.AddOrUpdateData(priceMatchParam);
        }

        public void Init(FrameworkElement frameworkElement, string side, string securityId, string pointPrice)
        {
            _parentUI = frameworkElement;
            if (side == "1" || side == "Bid")
            {
                _batchPointPriceDomain.Side = Constants.GVN;
                _batchPointPriceDomain.BgBrush = (Brush)_parentUI.FindResource("BidLinearGradientBrush");
                _batchPointPriceDomain.SideBrush = (Brush)_parentUI.FindResource("ColorBrush.Orange7");
                _batchPointPriceDomain.BorderBrush = (Brush)_parentUI.FindResource("ColorBrush.Gray15");
                _batchPointPriceDomain.BoxBgBrush = (Brush)_parentUI.FindResource("ColorBrush.Orange-9");
                _batchPointPriceDomain.BoxBorderBrush = (Brush)_parentUI.FindResource("ColorBrush.Orange4");

                _batchPointPriceDomain.SendPriceTxt = string.Format("批量{0}", "GVN");
            }
            else
            {
                _batchPointPriceDomain.Side = Constants.TKN;
                _batchPointPriceDomain.BgBrush = (Brush)_parentUI.FindResource("OfrLinearGradientBrush");
                _batchPointPriceDomain.SideBrush = (Brush)_parentUI.FindResource("ColorBrush.Blue10");
                _batchPointPriceDomain.BorderBrush = (Brush)_parentUI.FindResource("ColorBrush.PaleGrey");
                _batchPointPriceDomain.BoxBgBrush = (Brush)_parentUI.FindResource("ColorBrush.Blue-12");
                _batchPointPriceDomain.BoxBorderBrush = (Brush)_parentUI.FindResource("ColorBrush.Blue8");

                _batchPointPriceDomain.SendPriceTxt = string.Format("批量{0}", "TKN");
            }

            BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securityId);
            if (bondsInfoPo != null)
            {
                _batchPointPriceDomain.SecurityId = bondsInfoPo.Bond_ID;
                _batchPointPriceDomain.SymbolName = bondsInfoPo.Short_Name;
            }

            SendPriceItem = new SendPriceItem();
            SendPriceItem.Security = securityId;

            _sendPriceAdapter.SetFilterParam(false, "1,2,3,4");
            _sendPriceAdapter.Init(SendPriceItem);

            InitPrice(securityId);

            GetBestPrice(side, securityId, pointPrice);
            
        }

        private void InitPrice(string securityId)
        {
            UserSetting us = SystemParamHandler.Instance.GetUserSetting();
            
            if (!string.IsNullOrEmpty(us.inf2))
            {
                if (JsonSplit.IsJson(us.inf2))
                {
                    ConsistencyCheckPo ck = Newtonsoft.Json.JsonConvert.DeserializeObject<ConsistencyCheckPo>(us.inf2);
                    SendPriceConsistencyCheck = string.IsNullOrEmpty(ck.SendPriceConsistencyCheck) ? false : ck.SendPriceConsistencyCheck == "1" ? true : false;
                    PointPriceConsistencyCheck = string.IsNullOrEmpty(ck.PointPriceConsistencyCheck) ? false : ck.PointPriceConsistencyCheck == "1" ? true : false;

                    // 服务器时间
                    LastUpdateTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    if (SendPriceConsistencyCheck)
                    {
                        ExecutionReport lastExeItem = LastExecutionProcesser.Processer.GetExecutionReportDic(securityId);

                        if (lastExeItem != null)
                        {
                            Processer_OnLastExecutions(lastExeItem);
                        }

                        LastExecutionProcesser.Processer.OnLastExecutions += Processer_OnLastExecutions;

                        this.timer = new DispatcherTimer();
                        this.timer.Tick += Timer_Tick;
                        this.timer.Interval = TimeSpan.FromSeconds(1);
                        this.timer.Start();
                    }
                    
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            string serverTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            if (isHvaeNewExecution)
            {
                TimeSpan ts1 = new TimeSpan(DateTime.Parse(serverTime).Ticks);
                TimeSpan ts2 = new TimeSpan(DateTime.Parse(LastUpdateTime).Ticks);
                TimeSpan ts3 = ts1.Subtract(ts2).Duration();

                if (ts3.Seconds > 5)
                {
                    decimal dLast = 0;
                    decimal d = 0;
                    if (decimal.TryParse(this.LastPrice, out dLast) && decimal.TryParse(this._batchPointPriceDomain.Price, out d) && d != dLast)
                    {
                        _batchPointPriceDomain.WarnImageTooltip = $"与最新成交 {this.LastPrice} 不一致";
                        _batchPointPriceDomain.WarnImageVisibility = Visibility.Visible;
                        this.timer.Stop();
                    }
                }
            }
        }

        #region 成交单获取

        private void Processer_OnLastExecutions(ExecutionReport obj)
        {
            if (obj.SecurityID == SendPriceItem.Security)
            {
                // 更新时间
                string updateTime = obj.UpdateTime;
                string serverTime = DateTimeEx.ServerTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                if (DateTime.Parse(updateTime) > DateTime.Parse(LastUpdateTime))
                {
                    LastUpdateTime = obj.UpdateTime;
                    LastPrice = obj.Yield;
                    isHvaeNewExecution = true;
                }
            }
        }

        #endregion


        public void Clear()
        {
            if (_sendPriceAdapter != null)
            {
                _sendPriceAdapter.Clear();
                LastExecutionProcesser.Processer.OnLastExecutions -= Processer_OnLastExecutions;
                if (this.timer != null)
                    this.timer.Stop();
            }
        }


        private void OkClick()
        {
            
        }
    }
}
