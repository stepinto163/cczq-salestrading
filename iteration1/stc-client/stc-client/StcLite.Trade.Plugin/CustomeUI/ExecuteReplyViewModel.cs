using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using com.gateway.log;
using STC.Common.Model;
using STC.Common.Model.EMS;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.Model.TBook;
using STC.Common.Handler.TBook;
using STC.Common.Handler.EMS;
using STC.Common.View.UI;
using STC.Common.Handler.Price;
using STC.Common.Model.Fix.Message;
using BDB.Trade.Plugin.Utils;
using STC.Frame.UI;

namespace BDB.Trade.Plugin.CustomeUI
{
    public class ExecuteReplyViewModel : BaseViewModel
    {
        private readonly string MODULEID = "1001";

        private Window _myWindow = null;
        private readonly string IDB = "IDB";
        private readonly string SELL = "SELL";
        private readonly string BUY = "BUY";
        private readonly string BID = "Bid";
        private readonly string ASK = "Ofr";
        private readonly string NULLTXT = "--";
        private bool _isIDB = false;
        private string _tradeDate = string.Empty;
        //private string _settleType = string.Empty;

        #region 界面属性
        private string _goodsCode = string.Empty;
        public string GoodsCode
        {
            get { return _goodsCode; }
            set
            {
                _goodsCode = value;

                this.OnPropertyChanged();
            }
        }

        private string _shortname = string.Empty;
        public string Shortname
        {
            get { return _shortname; }
            set
            {
                _shortname = value;

                this.OnPropertyChanged();
            }
        }


        private string _settleType = string.Empty;
        public string SettleType
        {
            get { return _settleType; }
            set
            {
                _settleType = value;

                this.OnPropertyChanged();
            }
        }


        private string _side = string.Empty;
        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _sideForeground;
        public Brush SideForeground
        {
            get { return _sideForeground; }
            set
            {
                _sideForeground = value;

                this.OnPropertyChanged();
            }
        }

        private string _price = string.Empty;
        public string Price
        {
            get { return _price; }
            set
            {
                _price = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _priceForeground;
        public Brush PriceForeground
        {
            get { return _priceForeground; }
            set
            {
                _priceForeground = value;

                this.OnPropertyChanged();
            }
        }

        private string _amount = string.Empty;
        public string Amount
        {
            get { return _amount; }
            set
            {
                _amount = value;

                this.OnPropertyChanged();
            }
        }

        private string _execTime = string.Empty;
        public string ExecTime
        {
            get { return _execTime; }
            set
            {
                _execTime = value;

                this.OnPropertyChanged();
            }
        }

        private string _execId = string.Empty;
        public string ExecId
        {
            get { return _execId; }
            set
            {
                _execId = value;

                this.OnPropertyChanged();
            }
        }

        private string _venue = string.Empty;
        public string Venue
        {
            get { return _venue; }
            set
            {
                _venue = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _venueForeground;
        public Brush Venueoreground
        {
            get { return _venueForeground; }
            set
            {
                _venueForeground = value;

                this.OnPropertyChanged();
            }
        }

        private Brush _venueBackground;
        public Brush VenueBackground
        {
            get { return _venueBackground; }
            set
            {
                _venueBackground = value;

                this.OnPropertyChanged();
            }
        }

        private string _counterparty = string.Empty;
        public string Counterparty
        {
            get { return _counterparty; }
            set
            {
                _counterparty = value;

                this.OnPropertyChanged();
            }
        }

        private string _counterpartyExchanger = string.Empty;
        public string CounterpartyExchanger
        {
            get { return _counterpartyExchanger; }
            set
            {
                _counterpartyExchanger = value;

                this.OnPropertyChanged();
            }
        }

        private bool _priceCheck = false;
        public bool PriceCheck
        {
            get { return _priceCheck; }
            set
            {
                _priceCheck = value;

                if (value)
                {
                    RemarksCheck = false;
                    
                }
                IdbRemarksCheck = value;

                this.OnPropertyChanged();
            }
        }

        private ComboxItemData _comboxSelectedItem;
        public ComboxItemData ComboxSelectedItem
        {
            get { return _comboxSelectedItem; }
            set
            {
                _comboxSelectedItem = value;

                this.OnPropertyChanged();

                if (PriceCheck && _comboxSelectedItem != null)
                {
                    IdbYield = ComboxSelectedItem.NoMDEntry.MDEntryPx;
                    IdbClientInstName = ComboxSelectedItem.NoMDEntry.InstitutionName;
                    IdbClientExchanger = ComboxSelectedItem.NoMDEntry.TraderName;
                }
            }
        }

        private bool _comboxEnable = false;
        public bool ComboxEnable
        {
            get { return _comboxEnable; }
            set
            {
                _comboxEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool _remarksCheck = false;
        public bool RemarksCheck
        {
            get { return _remarksCheck; }
            set
            {
                _remarksCheck = value;

                if (value)
                {
                    PriceCheck = false;

                }
                this.OnPropertyChanged();
            }
        }

        private bool _remarksTxtEnable = false;
        public bool RemarksTxtEnable
        {
            get { return _remarksTxtEnable; }
            set
            {
                _remarksTxtEnable = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _IDBBorderVisibility = Visibility.Collapsed;
        public Visibility IDBBorderVisibility
        {
            get { return _IDBBorderVisibility; }
            set
            {
                _IDBBorderVisibility = value;

                this.OnPropertyChanged();
            }
        }

        private string _clientInstName = string.Empty;
        public string ClientInstName
        {
            get { return _clientInstName; }
            set
            {
                _clientInstName = value;

                this.OnPropertyChanged();
            }
        }

        private string _yield = string.Empty;
        public string Yield
        {
            get { return _yield; }
            set
            {
                _yield = value;

                this.OnPropertyChanged();
            }
        }

        private string _clientExchanger = string.Empty;
        public string ClientExchanger
        {
            get { return _clientExchanger; }
            set
            {
                _clientExchanger = value;

                this.OnPropertyChanged();
            }
        }

        private string _idbClientInstName;
        public string IdbClientInstName
        {
            get
            {
                return _idbClientInstName;
            }

            set
            {
                _idbClientInstName = value;

                this.OnPropertyChanged();
            }
        }

        private string _idbYield;
        public string IdbYield
        {
            get
            {
                return _idbYield;
            }

            set
            {
                _idbYield = value;

                this.OnPropertyChanged();
            }
        }

        private string _idbClientExchanger;
        public string IdbClientExchanger
        {
            get
            {
                return _idbClientExchanger;
            }

            set
            {
                _idbClientExchanger = value;

                this.OnPropertyChanged();
            }
        }

        private bool _idbRemarksCheck = true;
        public bool IdbRemarksCheck
        {
            get
            {
                return _idbRemarksCheck;
            }

            set
            {
                _idbRemarksCheck = value;

                this.OnPropertyChanged();
            }
        }

        private Visibility _addVisibility = Visibility.Collapsed;
        public Visibility AddVisibility
        {
            get
            {
                return _addVisibility;
            }

            set
            {
                _addVisibility = value;

                this.OnPropertyChanged();
            }
        }

        #endregion

        #region 命令事件

        public ICommand AddCommand { get { return new RelayCommand(AddClick); } }
        public ICommand OkCommand { get { return new RelayCommand(OKClick); } }
        public ICommand CloseCommand { get { return new RelayCommand(CloseClick); } }

        private void AddClick()
        {
            ValetDetailView valetDetailView = new ValetDetailView(this.ComboxDatas, Price, GoodsCode, _currentExecutionReport.TradeDate, _currentExecutionReport.SettlType, Amount, _currentExecutionReport);
            valetDetailView.Tag = DataTimeSequence.Instance.GetId();
            valetDetailView.OnDownClick += ValetDetailView_OnDownClick;
            WrapPanel spValet = _myWindow.FindName("spValet") as WrapPanel;
            spValet.Children.Add(valetDetailView);

            if (_myWindow.Height < 650)
                _myWindow.Height += valetDetailView.Height;

            if (spValet.Children.Count >= 10)
                AddVisibility = Visibility.Collapsed;
        }

        private void ValetDetailView_OnDownClick(string id)
        {
            WrapPanel spValet = _myWindow.FindName("spValet") as WrapPanel;
            ValetDetailView removeUI = null;
            foreach (UIElement ui in spValet.Children)
            {
                ValetDetailView valetDetailView = ui as ValetDetailView;
                string uiId = valetDetailView.Tag as string;
                if (uiId.Equals(id))
                {
                    removeUI = ui as ValetDetailView;
                    break;
                }
            }
            if (removeUI != null)
            {
                spValet.Children.Remove(removeUI);
                //if ((_myWindow.Height > 510 && spValet.Children.Count < 3) || (spValet.Children.Count == 0))
                if ((spValet.Children.Count < 3) || (spValet.Children.Count == 0))
                    _myWindow.Height -= removeUI.Height;

                if (spValet.Children.Count < 10)
                    AddVisibility = Visibility.Visible;
            }
        }

        private void GetValetInfo(UpdateOrderReq orderReq)
        {
            StringBuilder sbInfo1 = new StringBuilder();
            StringBuilder sbInstitutionID = new StringBuilder();
            StringBuilder sbInstitutionName = new StringBuilder();
            StringBuilder sbTraderID = new StringBuilder();
            StringBuilder sbTraderName = new StringBuilder();
            StringBuilder sbInfo2 = new StringBuilder();
            StringBuilder sbInfo5 = new StringBuilder();
            StringBuilder sbTradeMethod = new StringBuilder();
            StringBuilder sbInfo6 = new StringBuilder();
            WrapPanel spValet = _myWindow.FindName("spValet") as WrapPanel;
            string tem = ",";
            foreach (UIElement ui in spValet.Children)
            {
                ValetDetailView valetDetailView = ui as ValetDetailView;
                UpdateOrderReq updateOrder = valetDetailView.GetValetInfo();
                sbInfo1.Append(updateOrder.Info1 + tem);
                sbInstitutionID.Append(updateOrder.InstitutionID + tem);
                sbInstitutionName.Append(updateOrder.InstitutionName + tem);
                sbTraderID.Append(updateOrder.TraderID + tem);
                sbTraderName.Append(updateOrder.TraderName + tem);
                sbInfo2.Append(updateOrder.Info2 + tem);
                sbInfo5.Append(updateOrder.Info5 + tem);

                sbTradeMethod.Append(updateOrder.TradeMethod + tem);
                sbInfo6.Append(updateOrder.Info6 + tem);
            }


            string str = sbInfo1.ToString();
            if (!string.IsNullOrEmpty(str) && str.Length > 0)
            {
                orderReq.Info1 = str.Substring(0, str.Length - 1);
                str = sbInstitutionID.ToString();
                orderReq.InstitutionID = str.Substring(0, str.Length - 1);
                str = sbInstitutionName.ToString();
                orderReq.InstitutionName = str.Substring(0, str.Length - 1);
                str = sbTraderID.ToString();
                orderReq.TraderID = str.Substring(0, str.Length - 1);
                str = sbTraderName.ToString();
                orderReq.TraderName = str.Substring(0, str.Length - 1);

                str = sbInfo2.ToString();
                orderReq.Info2 = str.Substring(0, str.Length - 1);

                str = sbInfo5.ToString();
                orderReq.Info5 = str.Substring(0, str.Length - 1);

                str = sbTradeMethod.ToString();
                orderReq.TradeMethod = str.Substring(0, str.Length - 1);

                str = sbInfo6.ToString();
                orderReq.Info6 = str.Substring(0, str.Length - 1);
            }
        }

        private bool GetPriceCheck()
        {
            bool result = false;
            WrapPanel spValet = _myWindow.FindName("spValet") as WrapPanel;
            foreach (UIElement ui in spValet.Children)
            {
                ValetDetailView valetDetailView = ui as ValetDetailView;
                result = valetDetailView.GetPriceCheck();
                if (result)
                {
                    break;
                }
            }

            return result;
        }

        private ComboxItemData GetComboxItemData()
        {
            ComboxItemData result = null;
            WrapPanel spValet = _myWindow.FindName("spValet") as WrapPanel;
            foreach (UIElement ui in spValet.Children)
            {
                ValetDetailView valetDetailView = ui as ValetDetailView;
                bool isCheck = valetDetailView.GetPriceCheck();
                result = valetDetailView.GetComboxItemData();
                if (isCheck)
                {
                    break;
                }
            }

            return result;
        }

        private void OKClick()
        {
            UpdateOrderReq orderReq = new UpdateOrderReq();
            orderReq.Price = Price;

            if (_currentExecutionReport != null)
            {
                orderReq.OrderId = _currentExecutionReport.OrderID;

                if (_isIDB)
                {
                    //if (PriceCheck)
                    //{
                    //    orderReq.Info1 = IdbYield;

                    //    orderReq.InstitutionID = ComboxSelectedItem.NoMDEntry.CounterPartyID;
                    //    orderReq.InstitutionName = IdbClientInstName;
                    //    orderReq.TraderID = ComboxSelectedItem.NoMDEntry.CounterPartySubID;
                    //    orderReq.TraderName = IdbClientExchanger;

                    //}
                    //else if (RemarksCheck)
                    //{
                    //    orderReq.Info1 = Yield;

                    //    orderReq.InstitutionName = ClientInstName;
                    //    orderReq.TraderName = ClientExchanger;
                    //}

                    if (!CheckValetInfo())
                    {
                        CustomMessageBox.Warning("代客要素未填写完整或总输入量不等于成交量");
                        return;
                    }

                    GetValetInfo(orderReq);
                    PriceCheck = GetPriceCheck();
                    ComboxSelectedItem = GetComboxItemData();
                    ThreadPoolHelper.InvokeThreadPool(UpdateOrder, orderReq);
                }
                else
                    CloseClick();
            }
            else
                CloseClick();


        }

        private bool CheckValetInfo()
        {
            bool result = true;

            decimal total = 0;
            decimal custom = 0;
            decimal tem = 0;
            WrapPanel spValet = _myWindow.FindName("spValet") as WrapPanel;
            foreach (UIElement ui in spValet.Children)
            {
                ValetDetailView valetDetailView = ui as ValetDetailView;
                result = valetDetailView.CheckInfo();

                if (decimal.TryParse(valetDetailView.GetValetInfo().Info2, out tem))
                    custom += tem;

                if (!result)
                    break;
            }

            if (result && decimal.TryParse(Amount, out total) && custom != total)
            {
                result = false;
            }

            return result;
        }

        private void CancelIdbPrice()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ComboxItemData result = null;
                WrapPanel spValet = _myWindow.FindName("spValet") as WrapPanel;
                foreach (UIElement ui in spValet.Children)
                {
                    ValetDetailView valetDetailView = ui as ValetDetailView;
                    bool isCheck = valetDetailView.GetPriceCheck();
                    result = valetDetailView.GetComboxItemData();
                    if (isCheck)
                    {
                        STC.Common.Model.TBook.OrderCancelRequest cancelPo = new STC.Common.Model.TBook.OrderCancelRequest();
                        cancelPo.AlgoName = "CROSS";
                        cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                        if (result != null)
                        {
                            cancelPo.OrderID = result.NoMDEntry.MDEntryID;

                            ThreadPoolHelper.InvokeThreadPool(DealCancelIdbPrice, cancelPo);
                            //TBookCancelReplyPo cancelReply = TBookHandler.Instance.CancelOrder(cancelPo);
                        }
                    }
                }
            }));
        }

        private void DealCancelIdbPrice(object cancelPo)
        {
            try
            {
                TBookCancelReplyPo cancelReply = TBookHandler.Instance.CancelOrder(cancelPo as STC.Common.Model.TBook.OrderCancelRequest);
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }


        //代客单， 先撤销IDB报价 再更新价格
        private void UpdateOrder(object req)
        {
            try
            {
                BaseReplyPo result = null;

                CancelIdbPrice();

                //if (PriceCheck)
                //{
                //    BDB.Common.Model.TBook.OrderCancelRequest cancelPo = new BDB.Common.Model.TBook.OrderCancelRequest();
                //    cancelPo.AlgoName = "CROSS";
                //    cancelPo.ClOrdID = DataTimeSequence.Instance.GetId();
                //    if (ComboxSelectedItem != null)
                //    {
                //        cancelPo.OrderID = ComboxSelectedItem.NoMDEntry.MDEntryID;
                //        TBookCancelReplyPo cancelReply = TBookHandler.Instance.CancelOrder(cancelPo);
                //        result = cancelReply;
                //    }
                //}
                
                //if (result == null || result.IsSuccess())
                {
                    PResult updateResult = EMSHandler.Instance.updateOrder(req as UpdateOrderReq);

                    result = updateResult;
                }


                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (!result.IsSuccess())
                    {
                        MessageBoxGUI22 errView = new MessageBoxGUI22();
                        errView.setWarnMsg(result.Msg);

                        errView.ShowDialog();
                    }
                    CloseClick();
                }));

            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }

        public void Clear()
        {
            _msgTask.Clear();
            _msgTask.Dispose();
           // MDHandler.Instance.UnSubscribe(OnMarketData, null);
        }

        private void CloseClick()
        {
            _msgTask.Clear();
            _msgTask.Dispose();
            //MDHandler.Instance.UnSubscribe(OnMarketData, null);

            _myWindow.Close();
        }
        #endregion

        private ExecutionReport _currentExecutionReport = null;
        public void Init(FrameworkElement own, ExecutionReport er, string contentType)
        {
            if (_msgTask == null)
                _msgTask = new AsyncThreadQueue<MarketDataSnapshotFullRefresh>(Process);
            else
                _msgTask.Clear();
            
            _myWindow = own as Window;

            ExecutionReport order = null;
            _isIDB = false;//AdminHandler.Instance.CheckPermission(MODULEID, string.Empty);

            if (er != null)
            {
                _currentExecutionReport = order = er;
                GoodsCode = order.SecurityID;

                //GoodsCode = "180406.IB";

                Shortname = order.Symbol;

                if (er.SettlType == "2" || er.SettlType == "3")
                    SettleType = "T+1(不限)";
                else
                    SettleType = ComboSourceUtil.Instance.FindEnum(er.SettlType, "settletype_myOrder");

                Price = string.IsNullOrEmpty(order.LastPx) ?order.Price: order.LastPx;
                if (order.Side.Equals("1"))
                {
                    Side = BID;
                    PriceForeground = SideForeground = (Brush)_myWindow.FindResource("ColorBrush.Orange5");

                }
                else if (order.Side.Equals("2"))
                {
                    Side = ASK;
                    PriceForeground = SideForeground = (Brush)_myWindow.FindResource("ColorBrush.Blue8");
                }

                Amount = string.IsNullOrEmpty(order.LastQty) ?order.OrderQty : order.LastQty;
                if (!string.IsNullOrEmpty(order.TransactTime))
                {
                    int length = order.TransactTime.Length;
                    ExecTime = length > 20 ? order.TransactTime.Substring(0, length - 4) : order.TransactTime;
                } 
                ExecId = order.ExecID;

                Venue = ComboSourceUtil.Instance.FindEnum(order.Venues, "venues_exe");
                Counterparty = string.IsNullOrEmpty(order.CounterPartyName) ? NULLTXT : order.CounterPartyName;
                CounterpartyExchanger = string.IsNullOrEmpty(order.CounterPartySubName) ? NULLTXT : order.CounterPartySubName;

                Venueoreground = (Brush)_myWindow.FindResource("ColorBrush.Red13");

                //order.Venues = IDB;

                //if (order.Venues.Equals(IDB))
                if (!string.IsNullOrEmpty(contentType) && _isIDB)
                {
                    MarketDataSnapshotFullRefresh MarketDataSnapshotFullRefresh = MDHandler.Instance.GetCachePriceByTopic(MDHandler.Instance.GetCtTopic(GoodsCode));

                    OnMarketData(string.Empty, MarketDataSnapshotFullRefresh);

                    if (_comboxDatas != null && _comboxDatas.Count > 0)
                        ComboxSelectedItem = _comboxDatas[0];

                    MDHandler.Instance.SubscribeFull(GoodsCode, string.Empty, OnMarketData);
                }
            }
            //msgPo.ContentType = IDB;

           
            if (!string.IsNullOrEmpty(contentType) && _isIDB)
                //if (!string.IsNullOrEmpty(msgPo.ContentType))
            {
                IDBBorderVisibility = Visibility.Visible;
                //_myWindow.Height = 400;

                AddVisibility = Visibility.Visible;

                AddClick();
                
                _isIDB = true;
            }
            else
            {
                IDBBorderVisibility = Visibility.Collapsed;
                _myWindow.Height = 256;

                _isIDB = false;
            }
        }


        #region IDB渠道时，实时数据处理

        private AsyncThreadQueue<MarketDataSnapshotFullRefresh> _msgTask = null;

        private object _lockObj = new object();

        private ObservableCollection<ComboxItemData> _comboxDatas = new ObservableCollection<ComboxItemData>();
        public ObservableCollection<ComboxItemData> ComboxDatas
        {
            get
            {
                return _comboxDatas;
            }
        }

        private void OnMarketData(string topic, MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh)
        {
            _msgTask.Dequeue(marketDataSnapshotFullRefresh);
        }

        private void Process(MarketDataSnapshotFullRefresh marketDataSnapshotFullRefresh)
        {
            lock (_lockObj)
            {
                string side = GetSide();
                if (marketDataSnapshotFullRefresh != null)
                {
                    List<GNoMDEntries> idbNoMDEntries = marketDataSnapshotFullRefresh.NoMDEntries.Where(delegate (GNoMDEntries noMDEntries)
                    {
                        if ((noMDEntries.Venues+"").Equals(IDB) && noMDEntries.MDEntryType.Equals(side)
                            && (noMDEntries.PartySubName+"").Equals(LocalEnv.UserInfo.FullName) && marketDataSnapshotFullRefresh.SecurityID.Equals(GoodsCode))
                            return true;
                        else
                            return false;
                    }).ToList();

                    if (idbNoMDEntries != null && idbNoMDEntries.Count > 0)
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            //ComboxDatas.Clear();

                            foreach (GNoMDEntries noMDEntries in idbNoMDEntries)
                            {
                                UpdateComboxData(noMDEntries);
                            }

                            RemoveOld(idbNoMDEntries);
                        }));
                    }
                }
                
            }
        }

        private void RemoveOld(List<GNoMDEntries> idbNoMDEntries)
        {
            List<ComboxItemData> removeList = new List<ComboxItemData>();
            foreach (ComboxItemData itemData in _comboxDatas)
            {
                bool isExit = false;
                foreach (GNoMDEntries noMDEntries in idbNoMDEntries)
                {
                    if (itemData.Id.Equals(noMDEntries.MDEntryID))
                    {
                        isExit = true;
                        break;
                    }
                }
                if (!isExit)
                    removeList.Add(itemData);
            }

            if (removeList.Count > 0)
            {
                foreach (ComboxItemData itemData in removeList)
                    _comboxDatas.Remove(itemData);
            }
        }

        private void UpdateComboxData(GNoMDEntries noMDEntries)
        {
            List<ComboxItemData> result = _comboxDatas.Where(delegate (ComboxItemData itemData)
            {
                if (itemData.Id.Equals(noMDEntries.MDEntryID))
                    return true;
                else
                    return false;
            }).ToList();

            if (result != null && result.Count > 0)
            {
                result[0].ShowTxt = GetShowTxt(noMDEntries);
                result[0].NoMDEntry = noMDEntries;
            }
            else
            {
                ComboxItemData itemData = new ComboxItemData();
                itemData.Id = noMDEntries.MDEntryID;
                itemData.ShowTxt = GetShowTxt(noMDEntries);

                itemData.NoMDEntry = noMDEntries;
                
                ComboxDatas.Add(itemData);
            }

        }

        private string GetShowTxt(GNoMDEntries noMDEntries)
        {
            //方向+量+收益率+意向类型+客户机构名称+客户交易员名称+时间
            return string.Format(" {0} {1}万 {2} {3} {4} {5} {6}  ", Side, noMDEntries.MDEntrySize, noMDEntries.MDEntryPx, GetFirm(noMDEntries.FirmFlag),
                noMDEntries.InstitutionName, noMDEntries.TraderName, GetTime(noMDEntries.MDEntryTime));
        }

        private string GetSide()
        {
            return Side.Equals(BID) ? "0" : "1";
        }   

        private string GetFirm(string firm)
        {
            return firm.Equals("0") ? "nonfrim" : "firm";
        }

        private string GetTime(string time)
        {
            return time.Substring(0, time.Length - 4);
        }

        #endregion

    }

    public class ComboxItemData : BaseViewModel
    {
        public string ShowTxt { get; set; }

        public string Id { get; set; }
        
        public GNoMDEntries NoMDEntry { get; set; }
    }
}
