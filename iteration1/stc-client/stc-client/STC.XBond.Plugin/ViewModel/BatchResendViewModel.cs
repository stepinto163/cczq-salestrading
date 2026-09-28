using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Data;
using STC.Common.Handler.SystemParam;
using STC.Common.Handler.TBook;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
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

namespace STC.XBond.Plugin.ViewModel
{
    /// <summary>
    /// 批量恢复BatchResendView的数据内容
    /// </summary>
    public class BatchResendViewModel : ViewModelBase
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

        public BatchResendViewModel(Window win)
        {
            _window = win;
            OkCommand = new RelayCommand(OnConfirmClick);
        }

        private bool CheckParam()
        {
            bool result = true;
            string errTxt = string.Empty;
            foreach (NewOrderSingle sendPricePo in newOrderSingleDic.Values)
            {
                ChannelCheckParam param = null;
                if (CommonUtils.IsRate(sendPricePo.SecurityType))
                {
                    param = _BNC_ChannelCheckParam;
                }
                else
                {
                    param = _BCO_ChannelCheckParam;
                }

                if (param != null)
                {
                    if (string.IsNullOrEmpty(param.CrossVenues))
                    {
                        errTxt = string.Format("{0}--{1}", sendPricePo.SecurityType == "BNC" ? "利率" : "信用", "必须选择一个撮合渠道");
                        break;
                    }
                    else if (string.IsNullOrEmpty(param.VenuesSelectF) && param.CrossVenues.Equals("1"))
                    {
                        errTxt = string.Format("{0}--{1}", sendPricePo.SecurityType == "BNC" ? "利率" : "信用", "余量发送渠道必须选择一个");
                        break;
                    }
                    else if (!string.IsNullOrEmpty(param.VenuesSelectF) && param.VenuesSelectF.Equals("3") && string.IsNullOrEmpty(param.SelfSelectVenues))
                    {
                        errTxt = string.Format("{0}--{1}", sendPricePo.SecurityType == "BNC" ? "利率" : "信用", "余量发送渠道-指定，必须选择一个渠道");
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
            await Task.Run(() =>
            {
                try
                {
                    UserSetting userSetting = SystemParamHandler.Instance.CopyUserSetting();
                    foreach (NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
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
                                ChannelCheckParam param = null;
                                if (CommonUtils.IsRate(sendPricePo.SecurityType))
                                {
                                    param = _BNC_ChannelCheckParam;
                                }
                                else
                                {
                                    param = _BCO_ChannelCheckParam;
                                }

                                if (param != null)
                                {
                                    // 整量和nonfirm单不能发送外部渠道
                                    sendPricePo.CrossVenues = param.CrossVenues;
                                    sendPricePo.VenuesRdmNum = param.VenuesRdmNum;
                                    sendPricePo.VenuesSelectF = param.VenuesSelectF;
                                    sendPricePo.SelfSelectVenues = param.SelfSelectVenues;
                                }

                                //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");

                                if ((sendPricePo.WholeFlag != "1" && sendPricePo.FirmFlag != "0")
                                  || ((sendPricePo.WholeFlag == "1" || sendPricePo.FirmFlag == "0") 
                                    && param.CrossVenues == "2"
                                    && string.IsNullOrEmpty(param.VenuesSelectF)
                                    && string.IsNullOrEmpty(param.SelfSelectVenues)))
                                {
                                    PResult result = PlaceOrder(sendPricePo);
                                    dic[sendPricePo] = result;
                                }
                                else
                                {
                                    PResult result = new PResult();
                                    result.Code = "0000";
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
