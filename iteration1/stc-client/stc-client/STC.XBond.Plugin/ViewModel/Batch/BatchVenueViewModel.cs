using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.SystemParam;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.ViewModel.Order.InnerView;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Batch;
using STC.XBond.Plugin.View.Component;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.XBond.Plugin.ViewModel.Batch
{
    public class BatchVenueViewModel : ViewModelBase
    {
        public Dictionary<string, NewOrderSingle> newOrderSingleDic;
        public ChannelCheckParam _BNC_ChannelCheckParam;
        public ChannelCheckParam _BCO_ChannelCheckParam;

        #region 属性
        /// <summary>
        /// 选中条数
        /// </summary>
        private int _SelectedCount = 0;
        public int SelectedCount
        {
            get
            {
                return _SelectedCount;
            }

            set
            {
                if (_SelectedCount != value)
                {
                    _SelectedCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否显示利率债部分
        /// </summary>
        private Visibility _IsShowInterestRateDebt = Visibility.Collapsed;
        public Visibility IsShowInterestRateDebt
        {
            get
            {
                return _IsShowInterestRateDebt;
            }
            set
            {
                if (_IsShowInterestRateDebt != value)
                {
                    _IsShowInterestRateDebt = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否显示信用债部分
        /// </summary>
        private Visibility _IsShowCreditDebt = Visibility.Collapsed;
        public Visibility IsShowCreditDebt
        {
            get
            {
                return _IsShowCreditDebt;
            }
            set
            {
                if (_IsShowCreditDebt != value)
                {
                    _IsShowCreditDebt = value;
                    this.OnPropertyChanged();
                }
            }
        }

        private bool sendPriceEnable = true;
        public bool SendPriceEnable
        {
            get
            {
                return sendPriceEnable;
            }

            set
            {
                sendPriceEnable = value;
                this.OnPropertyChanged();
            }
        }
        #endregion

        private Window _window;

        private int totalCount = 0;
        private int cancelCount = 0;

        private int callbackCount = 0;

        public int CallbackCount
        {
            get
            {
                return callbackCount;
            }

            set
            {
                if (callbackCount != value)
                {
                    callbackCount = value;
                    this.OnPropertyChanged();
                }
            }
        }

        ConcurrentDictionary<NewOrderSingle, PResult> dic = new ConcurrentDictionary<NewOrderSingle, PResult>();

        private volatile bool _isCanceled = false;

        public ICommand OkCommand { get; set; }

        public BatchVenueViewModel(Window win)
        {
            _window = win;
            OkCommand = new RelayCommand(OnConfirmClick);
        }

        private bool CheckParam()
        {
            bool result = true;
            string errTxt = string.Empty;
            ExecVenueViewModel execVenueViewModel = (_window.FindName("execVenueView") as ExecVenueView).DataContext as ExecVenueViewModel;
            OCOViewModel ocoViewModel = execVenueViewModel.GetOCOViewModel();

            string crossVenues = string.Empty;
            string venuesSelectF = string.Empty;
            string selfSelectVenues = string.Empty;
            if (execVenueViewModel.SelectTabIndex == 1)
            {
                if (ocoViewModel != null)
                {
                    crossVenues = ocoViewModel.CheckAll ? "1" : ocoViewModel.CheckInner ? "2" : string.Empty;

                    if (ocoViewModel.CheckXBond && ocoViewModel.CheckXBondVisibility == Visibility.Visible)
                    {
                        crossVenues = "3";
                    }


                    if (!(ocoViewModel.CheckInner && ocoViewModel.CheckXBondVisibility == Visibility.Collapsed))
                        venuesSelectF = ocoViewModel.CheckNotOut ? "1" : "3";

                    selfSelectVenues = execVenueViewModel.GetSelectItem();
                }
            }
            if (execVenueViewModel.SelectTabIndex == 2)
            {
                venuesSelectF = "1";
                crossVenues = "1";
                selfSelectVenues = string.Empty;
            }
            else if (execVenueViewModel.SelectTabIndex == 0)
            {
                if (execVenueViewModel.CrossVisibility == Visibility.Collapsed && execVenueViewModel.OCOVisibility == Visibility.Collapsed && execVenueViewModel.SniperVisibility == Visibility.Collapsed)
                {
                    errTxt = "没有执行权限";
                    Common.View.Component.CustomMessageBox.Warning(errTxt, "参数检查");

                    return false;
                }

                string selectItem = execVenueViewModel.GetSelectItem();
                string[] arr = selectItem.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                {
                    string selectVenueGwType = arr[1];
                    string venue = arr[0];

                    selfSelectVenues = venue;
                    crossVenues = selectVenueGwType;
                }
            }


            ExecVenueViewModel execVenueViewModelBCO = (_window.FindName("execVenueViewBCO") as ExecVenueView).DataContext as ExecVenueViewModel;
            
            OCOViewModel ocoViewModelBCO = execVenueViewModelBCO.GetOCOViewModel();
            string crossVenuesBCO = string.Empty;
            string venuesSelectFBCO = string.Empty;
            string selfSelectVenuesBCO = string.Empty;
            SniperViewModel sniperViewModelBCO = execVenueViewModelBCO.GetSniperViewModel();

            if (execVenueViewModelBCO.SelectTabIndex == 1)
            {
                if (ocoViewModelBCO != null)
                {
                    crossVenuesBCO = ocoViewModelBCO.CheckAll ? "1" : ocoViewModelBCO.CheckInner ? "2" : string.Empty;

                    if (ocoViewModelBCO.CheckXBond && ocoViewModelBCO.CheckXBondVisibility == Visibility.Visible)
                    {
                        crossVenuesBCO = "3";
                    }

                    if (!(ocoViewModelBCO.CheckInner && ocoViewModelBCO.CheckXBondVisibility == Visibility.Collapsed))
                        venuesSelectFBCO = ocoViewModelBCO.CheckNotOut ? "1" : "3";

                    selfSelectVenuesBCO = execVenueViewModelBCO.GetSelectItem();
                }
            }
            if (execVenueViewModelBCO.SelectTabIndex == 2)
            {
                if (sniperViewModelBCO != null)
                {
                    crossVenuesBCO = "1";
                    venuesSelectFBCO = "1";
                    selfSelectVenuesBCO = string.Empty;
                }
            }
            else if (execVenueViewModelBCO.SelectTabIndex == 0)
            {
                if (execVenueViewModelBCO.CrossVisibility == Visibility.Collapsed && execVenueViewModel.OCOVisibility == Visibility.Collapsed && execVenueViewModel.SniperVisibility == Visibility.Collapsed)
                {
                    errTxt = "没有执行权限";

                    Common.View.Component.CustomMessageBox.Warning(errTxt, "参数检查");

                    return false;
                }

                string selectItem = execVenueViewModelBCO.GetSelectItem();
                string[] arr = selectItem.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                {
                    string selectVenueGwType = arr[1];
                    string venue = arr[0];

                    selfSelectVenuesBCO = venue;
                    crossVenuesBCO = selectVenueGwType;
                }
            }


            foreach (NewOrderSingle sendPricePo in newOrderSingleDic.Values)
            {
                if (sendPricePo.SecurityType == "BNC")
                {
                    if (string.IsNullOrEmpty(crossVenues))
                    {
                        errTxt = string.Format("{0}--{1}", "利率", "必须选择一个撮合渠道");
                        break;
                    }
                    else if (string.IsNullOrEmpty(venuesSelectF) && crossVenues.Equals("1"))
                    {
                        errTxt = string.Format("{0}--{1}", "利率", "余量发送渠道必须选择一个");
                        break;
                    }
                    else if (!string.IsNullOrEmpty(venuesSelectF) && venuesSelectF.Equals("3") && string.IsNullOrEmpty(selfSelectVenues))
                    {
                        errTxt = string.Format("{0}--{1}", "利率", "余量发送渠道-指定，必须选择一个渠道");
                        break;
                    }
                }
                else if (sendPricePo.SecurityType == "BCO")
                {
                    if (string.IsNullOrEmpty(crossVenuesBCO))
                    {
                        errTxt = string.Format("{0}--{1}", "信用", "必须选择一个撮合渠道");
                        break;
                    }
                    else if (string.IsNullOrEmpty(venuesSelectFBCO) && crossVenuesBCO.Equals("1"))
                    {
                        errTxt = string.Format("{0}--{1}", "信用", "余量发送渠道必须选择一个");
                        break;
                    }
                    else if (!string.IsNullOrEmpty(venuesSelectFBCO) && venuesSelectFBCO.Equals("3") && string.IsNullOrEmpty(selfSelectVenuesBCO))
                    {
                        errTxt = string.Format("{0}--{1}", "信用", "余量发送渠道-指定，必须选择一个渠道");
                        break;
                    }
                }

            }

            result = string.IsNullOrEmpty(errTxt);
            if (!result)
            {
                Common.View.Component.CustomMessageBox.Warning(errTxt, "参数检查");
            }

            return result;
        }

        private void OnConfirmClick()
        {
            if (!CheckParam())
                return;

            SendPriceEnable = false;

            UpdateNewOrderSingleList();
        }

        private async void UpdateNewOrderSingleList()
        {
            if (newOrderSingleDic.Count == 0)
            {
                return;
            }

            // 利率债部分
            ExecVenueViewModel execVenueViewModel = (_window.FindName("execVenueView") as ExecVenueView).DataContext as ExecVenueViewModel;
            OCOViewModel ocoViewModel = execVenueViewModel.GetOCOViewModel();
            string crossVenues = string.Empty;
            string venuesSelectF = string.Empty;
            string selfSelectVenues = string.Empty;
            if (execVenueViewModel != null && ocoViewModel != null)
            {
                crossVenues = ocoViewModel.CheckAll ? "1" : ocoViewModel.CheckInner ? "2" : string.Empty;

                if (ocoViewModel.CheckXBond && ocoViewModel.CheckXBondVisibility == Visibility.Visible)
                {
                    crossVenues = "3";
                }

                if (!(ocoViewModel.CheckInner && ocoViewModel.CheckXBondVisibility == Visibility.Collapsed))
                    venuesSelectF = ocoViewModel.CheckNotOut ? "1" : "3";

                selfSelectVenues = execVenueViewModel.GetSelectItem();
            }    
            

            // 信用债部分
            ExecVenueViewModel execVenueViewModelBCO = (_window.FindName("execVenueViewBCO") as ExecVenueView).DataContext as ExecVenueViewModel;
            OCOViewModel ocoViewModelBCO = execVenueViewModel.GetOCOViewModel();
            string crossVenuesBCO = string.Empty;
            string venuesSelectFBCO = string.Empty;
            string selfSelectVenuesBCO = string.Empty;
            if (execVenueViewModelBCO != null && ocoViewModelBCO != null)
            {
                crossVenuesBCO = ocoViewModel.CheckAll ? "1" : ocoViewModel.CheckInner ? "2" : string.Empty;

                if (ocoViewModelBCO.CheckXBond && ocoViewModelBCO.CheckXBondVisibility == Visibility.Visible)
                {
                    crossVenuesBCO = "3";
                }

                if (!(ocoViewModelBCO.CheckInner && ocoViewModelBCO.CheckXBondVisibility == Visibility.Collapsed))
                    venuesSelectFBCO = ocoViewModel.CheckNotOut ? "1" : "3";

                selfSelectVenuesBCO = execVenueViewModelBCO.GetSelectItem();
            }

            //设置直通还是OCO
            foreach (NewOrderSingle sendPricePo in newOrderSingleDic.Values)
            {
                string crossVenues_param = crossVenues;
                string venuesSelectF_param = venuesSelectF;
                string selfSelectVenues_param = selfSelectVenues;
                if (sendPricePo.SecurityType == "BCO")
                {
                    crossVenues_param = crossVenuesBCO;
                    venuesSelectF_param = venuesSelectFBCO;
                    selfSelectVenues_param = selfSelectVenuesBCO;
                }

                if (execVenueViewModel.SelectTabIndex == 0)
                {
                    sendPricePo.AlgoName = "STP";

                    string selectItem = execVenueViewModel.GetSelectItem();
                    string[] arr = selectItem.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr != null && arr.Length > 0)
                    {
                        string selectVenueGwType = arr[1];
                        string venue = arr[0];

                        if (!string.IsNullOrEmpty(selectVenueGwType) && selectVenueGwType == "CDH")
                            sendPricePo.VenueTypeGW = "SOR";
                        else
                            sendPricePo.VenueTypeGW = venue;

                        sendPricePo.Venues = selectVenueGwType;
                    }
                }
                else if (execVenueViewModel.SelectTabIndex == 1)
                {
                    sendPricePo.AlgoName = "CROSS";
                    sendPricePo.VenueTypeGW = "INNER";
                    sendPricePo.Venues = "MANUAL";
                }
                else if (execVenueViewModel.SelectTabIndex == 2)
                {
                    sendPricePo.AlgoName = "CROSS";
                    sendPricePo.VenueTypeGW = "INNER";
                    sendPricePo.Venues = "MANUAL";
                    venuesSelectF = "1";
                    selfSelectVenues = string.Empty;
                }

                sendPricePo.SelfSelectVenues = selfSelectVenues_param;
                sendPricePo.VenuesSelectF = venuesSelectF_param;
                if (execVenueViewModel.SelectTabIndex == 0)
                {
                    sendPricePo.CrossVenues = string.Empty;
                }
                else
                {
                    sendPricePo.CrossVenues = crossVenues_param;
                }
            }


                await Task.Run(() =>
            {
                try
                {
                    UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
                    foreach (NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        string crossVenues_param = crossVenues;
                        string venuesSelectF_param = venuesSelectF;
                        string selfSelectVenues_param = selfSelectVenues;
                        if (sendPricePo.SecurityType == "BCO")
                        {
                            crossVenues_param = crossVenuesBCO;
                            venuesSelectF_param = venuesSelectFBCO;
                            selfSelectVenues_param = selfSelectVenuesBCO;
                        }

                        Interlocked.Increment(ref totalCount);
                        if (_isCanceled)
                        {
                            return;
                        }

                        if (userSetting != null)
                        {
                            string validUntiTimeSetting = userSetting.validUntiTimeSetting;
                            string now = DateTime.Now.ToString("yyyy-MM-dd");
                            sendPricePo.ValidUntilTime = string.Format("{0} {1}", now, validUntiTimeSetting);
                            now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            if (now.CompareTo(sendPricePo.ValidUntilTime) >= 0)
                                sendPricePo.ValidUntilTime = string.Empty;
                        }


                        ThreadPool.QueueUserWorkItem(o =>
                        {
                            try
                            {
                                if ((sendPricePo.WholeFlag != "1" && sendPricePo.FirmFlag != "0")
                                  || ((sendPricePo.WholeFlag == "1" || sendPricePo.FirmFlag == "0")
                                    && crossVenues_param == "2"
                                    && string.IsNullOrEmpty(venuesSelectF_param)
                                    && string.IsNullOrEmpty(selfSelectVenues_param)))
                                {
                                    PResult result = PlaceOrder(sendPricePo);
                                    dic[sendPricePo] = result;
                                }
                                else
                                {
                                    //PResult result = new PResult();
                                    //result.Code = "0000";
                                    PResult result = PlaceOrder(sendPricePo);
                                    dic[sendPricePo] = result;
                                }

                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }
                        });
                    }


                    while (true)
                    {
                        CallbackCount = dic.Count;

                        Thread.Sleep(300);

                        if (totalCount != 0
                         && totalCount == newOrderSingleDic.Count
                        && totalCount == dic.Count)
                        {
                            break;
                        }
                    }
                    CallbackCount = dic.Count;

                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        SendPriceEnable = true;


                        int failCount = 0;
                        Dictionary<string, List<TPlaceOrderResult>> warnReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                        Dictionary<string, List<TPlaceOrderResult>> prohibitReplyDic = new Dictionary<string, List<TPlaceOrderResult>>();
                        foreach (KeyValuePair<NewOrderSingle, PResult> pair in dic)
                        {
                            if (pair.Value.IsSuccess() == false)
                            {
                                failCount++;
                                if (pair.Value.Code == "10000")
                                {
                                    try
                                    {
                                        bool isProhibitory = false;
                                        List<TPlaceOrderResult> orderRisks = MsgHelper.Deserialize<List<TPlaceOrderResult>>(pair.Value.Msg);
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
                                            prohibitReplyDic[pair.Key.StrategyOrderId] = orderRisks;
                                        }
                                        else
                                        {
                                            warnReplyDic[pair.Key.StrategyOrderId] = orderRisks;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        LogHelper.Error(ex);
                                    }
                                }
                            }
                        }

                        if (warnReplyDic.Count > 0 || prohibitReplyDic.Count > 0)
                        {
                            //Dictionary<string, NewOrderSingle> singleOrderDic = newOrderSingleDic.ToDictionary(key => key.StrategyOrderId, ValueSource => ValueSource);
                            BatchResultWindow batchResultWindow = new BatchResultWindow(newOrderSingleDic,
                                warnReplyDic, prohibitReplyDic, failCount);
                            batchResultWindow.Owner = _window;
                            batchResultWindow.ShowDialog();
                        }

                        SystemCommands.CloseWindow(_window);
                    }));
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        public void CancelOpt()
        {
            _isCanceled = true;
        }

        public bool FinishOpt()
        {
            return dic.Count == newOrderSingleDic.Count;
        }

        /// <summary>
        /// 利率债设置
        /// </summary>
        public void SetBNCSetting(ChannelCheckParam param)
        {
            _BNC_ChannelCheckParam = param;
        }

        /// 信用债设置
        /// </summary>
        public void SetBCOSetting(ChannelCheckParam param)
        {
            _BCO_ChannelCheckParam = param;
        }

        private PResult PlaceOrder(NewOrderSingle sendPricePo)
        {
            SendPriceEnable = false;
            sendPricePo.MacID = EngineHelper.GetMacByNetworkInterface();
            sendPricePo.InsideIP = EngineHelper.GetIp();
            sendPricePo.HardDiskID = EngineHelper.Getharddiskid2();
            PResult reply = OrdersHandler.Instance.PlaceOrder(sendPricePo);
            return reply;
        }
    }
}
