using com.gateway.log;
using STC.Common.Data;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View;
using STC.XBond.Plugin.CustomeUI;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.View;
using STC.XBond.Plugin.View.Component;
using System;
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
    /// firm与nofrim转换界面ConvertFirmOrNonFirmView的数据内容
    /// </summary>
    public class ConvertFirmOrNonFirmViewModel : ViewModelBase
    {


        public Dictionary<string, NewOrderSingle> newOrderSingleDic;
        public ChannelCheckParam _BNC_ChannelCheckParam;
        public ChannelCheckParam _BCO_ChannelCheckParam;
        private volatile bool _isCanceled = false;

        #region 属性

        /// <summary>
        /// 标题信息
        /// </summary>
        private string _header = string.Empty;
        public string Header
        {
            get { return _header; }
            set
            {
                if (_header != value)
                {
                    _header = value;
                    this.OnPropertyChanged();
                }
            }
        }

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
        /// 如果是转firm， 可以设置到点转Nonfirm功能
        /// </summary>
        private Visibility _isToFirm = Visibility.Collapsed;
        public Visibility IsToFirm
        {
            get
            {
                return _isToFirm;
            }
            set
            {
                if (_isToFirm != value)
                {
                    _isToFirm = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否启用到点转Nonfirm功能
        /// </summary>
        private bool _isCheckedFirmTimeLimit = false;
        public bool IsCheckedFirmTimeLimit
        {
            get
            {
                return _isCheckedFirmTimeLimit;
            }

            set
            {
                if (_isCheckedFirmTimeLimit != value)
                {
                    _isCheckedFirmTimeLimit = value;
                    FirmGridEnable = _isCheckedFirmTimeLimit;

                    AddEnable = DownEnable = _isCheckedFirmTimeLimit;

                    decimal d = 0;
                    if (decimal.TryParse(NonfirmTime, out d))
                    {
                        if (d >= 120)
                        {
                            AddEnable = false;
                            DownEnable = true;
                        }
                        else if (d <= 1)
                        {
                            AddEnable = true;
                            DownEnable = false;
                        }

                    }

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 是否可以使用到点转nonfirm控件
        /// </summary>
        private bool firmGridEnable = false;
        public bool FirmGridEnable
        {
            get
            {
                return firmGridEnable;
            }

            set
            {
                firmGridEnable = value;

                this.OnPropertyChanged();
            }
        }

        private bool addEnable = false;
        public bool AddEnable
        {
            get
            {
                return addEnable;
            }

            set
            {
                addEnable = value;

                this.OnPropertyChanged();
            }
        }


        private bool downEnable = false;
        public bool DownEnable
        {
            get
            {
                return downEnable;
            }

            set
            {
                downEnable = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// nonfirm time
        /// </summary>
        private string nonfirmTime;

        public string NonfirmTime
        {
            get
            {
                return nonfirmTime;
            }

            set
            {
                if(nonfirmTime != value)
                {
                    nonfirmTime = value;
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

        private bool firmChecked = false;
        public bool FirmChecked
        {
            get
            {
                return firmChecked;
            }

            set
            {
                firmChecked = value;
                this.OnPropertyChanged();
            }
        }

        private volatile int opsCount = 0;

        private int executedCount = 0;

        public int ExecutedCount
        {
            get
            {
                return executedCount;
            }

            set
            {
                executedCount = value;
                this.OnPropertyChanged();
            }
        }


        #endregion

        private Window _window;

        public ICommand OkCommand { get; set; }

        public ICommand AddCommand { get; set; }

        public ICommand DownCommand { get; set; }

        Thread updateUIThread = null;

        public ConvertFirmOrNonFirmViewModel(Window win)
        {
            _window = win;

            if (updateUIThread == null)
            {
                updateUIThread = new Thread(UpdateUI);
                updateUIThread.IsBackground = true;
            }
            
           
            AddCommand = new RelayCommand(OnAddClick);
            DownCommand = new RelayCommand(OnDownClick);
            OkCommand = new RelayCommand(OnConfirmClick);
        }

        ~ConvertFirmOrNonFirmViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            if (updateUIThread != null)
            {
                if (updateUIThread.IsAlive)
                {
                    updateUIThread.Abort();
                }
                updateUIThread = null;
            }

            base.Dispose();
        }

        private void OnAddClick()
        {
            if (string.IsNullOrEmpty(NonfirmTime))
                NonfirmTime = "1";
            else
            {
                decimal d = 0;
                if (decimal.TryParse(NonfirmTime, out d))
                {
                    if ((d >= 120))
                    {
                        AddEnable = false;
                    }
                    else
                        NonfirmTime = (d + 1).ToString();

                    DownEnable = d > 1;
                }
            }
        }

        private void OnDownClick()
        {
            decimal d = 0;
            if (decimal.TryParse(NonfirmTime, out d) && (d - 1) >= 0)
            {
                if (d <= 1)
                {
                    DownEnable = false;
                }
                else
                    NonfirmTime = (d - 1).ToString();

                AddEnable = (d - 1) < 120;
            }
        }


        public void Init(PriceMatchParam priceMatchParam)
        {
            //_priceMatchParam = priceMatchParam;
            //bool isOfr = priceMatchParam.Side.ToUpper().Equals("SELL");
            //InitUI(priceMatchParam.Symbol, !isOfr);


            //SendPriceItem.Amount = priceMatchParam.CustomFirmAmount;
            //CustomPrice = priceMatchParam.Price;
            //NetPrice = priceMatchParam.NetPrice;

            //FirmChecked = !priceMatchParam.IsFirm;

            //SendPriceItem.NonfirmTime = priceMatchParam.NonfirmTime;

            //if (!string.IsNullOrEmpty(priceMatchParam.WholeFlag) && priceMatchParam.WholeFlag.Equals("1"))
            //{
            //    SendPriceItem.AmountBoxIndex = 1;

            //}
            //else
            //    SendPriceItem.AmountBoxIndex = 0;

            //if (!string.IsNullOrEmpty(priceMatchParam.RandomNum))
            //{
            //    SendPriceItem.RandomNum = priceMatchParam.RandomNum;

            //    DefaultCheck = true;
            //}

            //MarketDataSnapshotFullRefresh bestPrice = MDHandler.Instance.GetCacheBestPriceBySecurity(sendPriceItem.Security);
            //ParseBestPrice(bestPrice);
            //MDHandler.Instance.Init(sendPriceItem.Security, null, OnBestPrice);

            //SetChannel(priceMatchParam);

            //isInit = false;
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
                    if (param.CrossVenues.Equals("4"))
                    {
                        errTxt = "撮合渠道不能单选X-bond";
                        break;
                    }

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
            if (updateUIThread != null && updateUIThread.IsAlive == false)
            {
                updateUIThread.Start();
            }
            UpdateNewOrderSingleList();
        }

        private void UpdateNewOrderSingleList()
        {
            Task.Run(() =>
            {
                try
                {
                    foreach (NewOrderSingle sendPricePo in newOrderSingleDic.Values)
                    {
                        //ThreadPool.QueueUserWorkItem(o =>
                        //{
                            if (_isCanceled)
                            {
                                Interlocked.Increment(ref opsCount);
                                return;
                            }

                            sendPricePo.FirmFlag = (IsToFirm == Visibility.Visible) ? "1" : "0";

                            sendPricePo.FirmToNonTime = string.Empty;
                            decimal d = 0;
                            if (IsCheckedFirmTimeLimit && decimal.TryParse(NonfirmTime, out d) && d > 0)
                                sendPricePo.FirmToNonTime = NonfirmTime.ToString();

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
                                sendPricePo.CrossVenues = param.CrossVenues;
                                sendPricePo.VenuesRdmNum = param.VenuesRdmNum;
                                sendPricePo.VenuesSelectF = param.VenuesSelectF;
                                sendPricePo.SelfSelectVenues = param.SelfSelectVenues;

                                if (IsToFirm == Visibility.Visible && !string.IsNullOrEmpty(sendPricePo.Terminal) && sendPricePo.Terminal.ToUpper().Equals("IDB"))
                                    sendPricePo.Venues = "MANUAL";
                            }

                            //sendPricePo.ValidUntilTime = DateTime.Now.AddDays(10).ToString("yyyy-MM-dd HH:mm:ss");

                            OrdersHandler.Instance.ModifyOrders(sendPricePo, sendPricePo.StrategyOrderId, UpdatePriceCallBack);
                            Interlocked.Increment(ref opsCount);
                        //});

                        try
                        {
                            Thread.Sleep(200);
                        }
                        catch (Exception e)
                        { }

                    }
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            });
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

        public void CancelOpt()
        {
            _isCanceled = true;
        }

        public bool FinishOpt()
        {
            return opsCount  == newOrderSingleDic.Count;
        }

        private void UpdatePriceCallBack(ModifyOrdersReply reply)
        {
            if (newOrderSingleDic != null)
            {
                if (reply.IsSuccess() == false)
                {
                    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (reply.Code == "10000")
                        {
                            SendPriceWarnView warnView = new SendPriceWarnView(reply, newOrderSingleDic[reply.OrderID]);
                            warnView.Owner = Application.Current.MainWindow;
                            warnView.Show();
                        }
                        else
                        {
                            Common.View.Component.CustomMessageBox.Warning(reply.Msg);
                        }
                    }));
                }
            }
            else
            {

            }
        }

        private void UpdateUI()
        {
            try
            {
                while (true)
                {
                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                        {
                            if (newOrderSingleDic != null)
                            {
                                SendPriceEnable = opsCount == newOrderSingleDic.Count;
                                ExecutedCount = opsCount;
                                if (executedCount == newOrderSingleDic.Count)
                                {
                                    SystemCommands.CloseWindow(_window);
                                }
                            }
                        }));
                    }
                    catch (Exception e)
                    {
                        LogHandler.Error(e);
                    }
                    Thread.Sleep(500);
                }
            }
            catch (ThreadAbortException threadAbortEx)
            {
                // ingore
            }
            catch (Exception e)
            {
                LogHandler.Error(e);
            }
        }
    }
}
