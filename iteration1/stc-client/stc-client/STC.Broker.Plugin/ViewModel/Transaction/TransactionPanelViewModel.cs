using Ats.Foundation.Message;
using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Component;
using STC.Broker.Plugin.View.History;
using STC.Broker.Plugin.View.Transaction;
using STC.Broker.Plugin.View.Transaction.BridgeDesign;
using STC.Broker.Plugin.View.Transaction.UnderLineExec;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.Handler.PT;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.ViewModel;
using STC.Frame.Utils.MultiUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CommonUtils = STC.Common.Utils.CommonUtils;
using STC.Common.View.UI;
using STC.Common.Handler.Drp;
using STC.Common.Handler.SystemParam;
using STC.Frame.UI;
using STC.Common.View.Component;

namespace STC.Broker.Plugin.ViewModel.Transaction
{
    public class TransactionPanelViewModel : ViewModelBase
    {
        #region 构造

        private AutoResetEvent are = new AutoResetEvent(true);
        internal bool isInit = false;
        private TransactionPanel myWindow;
        private int PageMaxCount = 10000;
        public double RowDefaultHeight = 40;

        private bool IsBridgeWinShow = false;

        private bool _isShowDeleteWin = true;

        private readonly string TransactionSetting = "TransactionSetting";
        private readonly string TransactionModel = "TransactionModel";
        /// <summary>
        ///交易处理模式 1.卡片，2列表，3自适应
        /// </summary>
        string _transactionModel = "3";

        /// <summary>
        /// 点击数量
        /// </summary>
        int CkickCount = 0;
        /// <summary>
        /// 是否是双击
        /// </summary>
        bool IsDoubleClick = false;

        private TransactionPanel _transactionPanel;
        /// <summary>
        /// 当前数据模型
        /// </summary>
        private TransactionOrdermModel _curOrdermModel = null;

        /// <summary>
        /// 定位选中行
        /// </summary>
        private TransactionOrdermModel _selectOrder = null;

        /// <summary>
        /// 删除的订单窗体
        /// </summary>
        private DeleteOrderWin _delorderWin = null;

        /// <summary>
        /// 当前启用的数据表格
        /// </summary>
        private DataGrid _currentDataGrid = null;

        /// <summary>
        /// 查看窗口
        /// </summary>
        private bool _lookWinIsShow = false;

        /// <summary>
        /// 代付信息窗口
        /// </summary>
        PayFeeSettingWin _feeSettingWin = null;

        /// <summary>
        /// 创建时间
        /// </summary>
        string _dealDateEnd = "";


        private string _SearchText;
        /// <summary>
        /// ShowAll 时触发
        /// </summary>
        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                this.OnPropertyChanged();
                this.Mv.View?.Refresh();
            }
        }

        internal object _dataLock = new object();

        private CollectionViewSource mv = new CollectionViewSource();
        public CollectionViewSource Mv
        {
            get { return mv; }
            set
            {
                mv = value;
                this.OnPropertyChanged(nameof(Mv));
            }
        }

        private ObservableCollection<TransactionOrdermModel> _innerOrders = new ObservableCollection<TransactionOrdermModel>();

        public ObservableCollection<TransactionOrdermModel> InnerOrders
        {
            get
            {
                if (_innerOrders == null)
                    _innerOrders = new ObservableCollection<TransactionOrdermModel>();

                return _innerOrders;
            }

            set
            {
                _innerOrders = value;
                this.OnPropertyChanged(nameof(InnerOrders));
            }
        }

        private bool isAllChecked = true;
        /// <summary>
        /// 全部
        /// </summary>
        public bool IsAllChecked
        {
            get
            {
                return isAllChecked;
            }
            set
            {
                isAllChecked = value;
                dealTimeChecked = !value;
                this.OnPropertyChanged();
                if (value && isInit)
                {
                    QueryOrder();
                }
            }
        }

        private bool dealTimeChecked = false;
        /// <summary>
        /// 是否按时间查询
        /// </summary>
        public bool DealTimeChecked
        {
            get { return dealTimeChecked; }
            set
            {
                dealTimeChecked = value;
                isAllChecked = !value;
                this.OnPropertyChanged();

                if (value && isInit)
                {
                    if (DealTimeBegin != null && DealTimeEnd != null)
                        QueryOrder();
                }
            }
        }


        private DateTime? dealTimeBegin;
        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? DealTimeBegin
        {
            get { return dealTimeBegin; }
            set
            {
                dealTimeBegin = value;
                //CheckTimeLimitSelected();
                this.OnPropertyChanged();
            }
        }

        private DateTime? dealTimeEnd;
        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? DealTimeEnd
        {
            get { return dealTimeEnd; }
            set
            {
                dealTimeEnd = value;
                //CheckTimeLimitSelected();
                this.OnPropertyChanged();
                if (!IsAllChecked)
                {
                    if (isInit)
                    {
                        DealTimeBegin = value;
                        if (DealTimeBegin != null && DealTimeEnd != null)
                            QueryOrder();
                    }
                }
            }
        }


        private TransactionOrdermModel selectorder;
        public TransactionOrdermModel SelectOrder
        {
            get
            {
                return selectorder;
            }
            set
            {
                selectorder = value;
                this.OnPropertyChanged();
            }
        }

        private int emsindex = 0;
        public int EMSIndex
        {
            get
            {
                return emsindex;
            }

            set
            {
                emsindex = value;
            }
        }

        private string emsPageVisible = "false";
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public string EMSPageVisible
        {
            get
            {
                return emsPageVisible;
            }

            set
            {
                emsPageVisible = value;
                this.OnPropertyChanged();
            }
        }

        private string unreadTxt = $"忽略0条";
        /// <summary>
        /// 全部
        /// </summary>
        public string UnreadTxt
        {
            get
            {
                return unreadTxt;
            }
            set
            {
                unreadTxt = value;
                this.OnPropertyChanged();

            }
        }

        private Visibility loadmaskvisible = Visibility.Collapsed;
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public Visibility LoadMaskVisible
        {
            get
            {
                return loadmaskvisible;
            }

            set
            {
                loadmaskvisible = value;
                this.OnPropertyChanged(nameof(LoadMaskVisible));
            }
        }


        private Visibility _listModelVisibility = Visibility.Visible;
        /// <summary>
        /// 列表模板是否可以见
        /// </summary>
        public Visibility ListModelVisibility
        {
            get
            {
                return _listModelVisibility;
            }

            set
            {
                _listModelVisibility = value;
                this.OnPropertyChanged(nameof(ListModelVisibility));
            }
        }

        private Visibility _cardVisibility = Visibility.Collapsed;
        /// <summary>
        /// 卡片模板是否可以见
        /// </summary>
        public Visibility CardVisibility
        {
            get
            {
                return _cardVisibility;
            }

            set
            {
                _cardVisibility = value;
                this.OnPropertyChanged(nameof(CardVisibility));
            }
        }

        //private Common.View.Component.PageControl _emsPageControl;
        //public Common.View.Component.PageControl EMSPageControl
        //{
        //    get
        //    {
        //        return _emsPageControl;
        //    }

        //    set
        //    {
        //        _emsPageControl = value;
        //        this.OnPropertyChanged();
        //    }
        //}




        private int totalCount = 0;
        public int TotalCount
        {
            get
            {
                return totalCount;
            }

            set
            {
                totalCount = value;
                this.OnPropertyChanged();
            }
        }



        private int _rowCount = 0;
        public int RowCount
        {
            get
            {
                return _rowCount;
            }
            set
            {
                _rowCount = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridHeight = 0;
        public double GridHeight
        {
            get
            {
                return _gridHeight;
            }
            set
            {
                _gridHeight = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridWidth = 0;
        public double GridWidth
        {
            get
            {
                return _gridWidth;
            }
            set
            {
                _gridWidth = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridBondWidth = 125;
        /// <summary>
        /// 债券列宽
        /// </summary>
        public double GridBondWidth
        {
            get
            {
                return _gridBondWidth;
            }
            set
            {
                _gridBondWidth = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridSettWidth = 115;
        /// <summary>
        /// 清算速度列宽
        /// </summary>
        public double GridSettWidth
        {
            get
            {
                return _gridSettWidth;
            }
            set
            {
                _gridSettWidth = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridTraderWidth = 142;
        /// <summary>
        /// 交易员列宽
        /// </summary>
        public double GridTraderWidth
        {
            get
            {
                return _gridTraderWidth;
            }
            set
            {
                _gridTraderWidth = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridRemarkWidth = 50;
        /// <summary>
        /// 备注列宽
        /// </summary>
        public double GridRemarkWidth
        {
            get
            {
                return _gridRemarkWidth;
            }
            set
            {
                _gridRemarkWidth = value;
                this.OnPropertyChanged();
            }
        }


        private double _gridSendWidth = 0;
        public double GridSendWidth
        {
            get
            {
                return _gridSendWidth;
            }
            set
            {
                _gridSendWidth = value;
                this.OnPropertyChanged();
            }
        }
        #region 自动模板宽度
        //第一块
        private double _gridBondWidthAuto = 225;
        /// <summary>
        /// 自动模板债券列宽
        /// </summary>
        public double GridBondWidthAuto
        {
            get
            {
                return _gridBondWidthAuto;
            }
            set
            {
                _gridBondWidthAuto = value;
                this.OnPropertyChanged();
            }
        }


        private double _gridSettWidthAuto = 115;
        /// <summary>
        /// 自动模板清算速度列宽
        /// </summary>
        public double GridSettWidthAuto
        {
            get
            {
                return _gridSettWidthAuto;
            }
            set
            {
                _gridSettWidthAuto = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridTraderWidthAuto = 142;
        /// <summary>
        /// 自动模板交易员列宽
        /// </summary>
        public double GridTraderWidthAuto
        {
            get
            {
                return _gridTraderWidthAuto;
            }
            set
            {
                _gridTraderWidthAuto = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridSendWidthAuto = 0;
        /// <summary>
        /// 自动模板发给标准化列宽
        /// </summary>
        public double GridSendWidthAuto
        {
            get
            {
                return _gridSendWidthAuto;
            }
            set
            {
                _gridSendWidthAuto = value;
                this.OnPropertyChanged();
            }
        }
        private double _childSendWidthAuto = 0;
        /// <summary>
        /// 自动模板发给标准化列宽
        /// </summary>
        public double ChildSendWidthAuto
        {
            get
            {
                return _childSendWidthAuto;
            }
            set
            {
                _childSendWidthAuto = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridWidthAuto1 = 225;
        /// <summary>
        /// 自动模板债券列宽
        /// </summary>
        public double GridWidthAuto1
        {
            get
            {
                return _gridWidthAuto1;
            }
            set
            {
                _gridWidthAuto1 = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridWidthAuto2 = 225;
        /// <summary>
        /// 自动模板债券列宽
        /// </summary>
        public double GridWidthAuto2
        {
            get
            {
                return _gridWidthAuto2;
            }
            set
            {
                _gridWidthAuto2 = value;
                this.OnPropertyChanged();
            }
        }

        private double _gridWidthAuto3 = 225;
        /// <summary>
        /// 自动模板债券列宽
        /// </summary>
        public double GridWidthAuto3
        {
            get
            {
                return _gridWidthAuto3;
            }
            set
            {
                _gridWidthAuto3 = value;
                this.OnPropertyChanged();
            }
        }

        #endregion


        /// <summary>
        /// 明细是否需要更新
        /// </summary>
        private volatile bool isNeedUpdateCopyUI = false;

        //private CopyDetailUI copyUI = null;
        //private BridgeWinNew bridgeWin = null;
        /// <summary>
        /// 历史记录窗体
        /// </summary>
        private HisReocrdWin _hisReocrdWin = null;
        #endregion

        #region Command
        public ICommand DetailCommand { get; set; }
        public ICommand DealCommand { get; set; }
        public ICommand BridgeCommand { get; set; }

        public ICommand SearchUseDateCommand { get; set; }

        public ICommand NewRequireCommand { get; set; }

        public ICommand OpenRecognizeWinCommand { get; set; }


        public ICommand QueryHistoryCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnQueryHistoryCommand(o);
                });
            }
        }

        public ICommand GotoCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    List<string> statusStr = new List<string>();
                    switch (o.ToString())
                    {
                        case "0,1,2,E,I":
                            statusStr.Add("0");
                            statusStr.Add("1");
                            statusStr.Add("2");
                            statusStr.Add("E");
                            statusStr.Add("I");
                            break;
                        case "F,H":
                            statusStr.Add("F");
                            statusStr.Add("H");
                            break;
                    }
                    SetScrollPositionByOrderStatus(statusStr);
                });
            }
        }

        /// <summary>
        /// 点击桥按钮
        /// </summary>
        public ICommand ClickBridgeCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnBridgeFlagCommand(o);
                });
            }
        }

        /// <summary>
        /// broker确认
        /// </summary>
        public ICommand BrokerConfirmedCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnBrokerConfirmedCommand(o);
                });
            }
        }

        /// <summary>
        /// 推送市场成交
        /// </summary>
        public ICommand PushMarketCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnPushMarketCommand(o);
                });
            }
        }


        /// <summary>
        /// 推送市场成交发单
        /// </summary>
        public ICommand PushMarketOrderCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnPushMarketOrderCommand(o);
                });
            }
        }

        /// <summary>
        /// 删除订单弹窗
        /// </summary>
        public ICommand DeleteCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnDeleteCommand(o);
                });
            }
        }

        /// <summary>
        /// 查看弹窗
        /// </summary>
        public ICommand LookCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnLookCommand(o);
                });
            }
        }


        /// <summary>
        /// 删除订单操作
        /// </summary>
        public ICommand DeleteOrderCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnDeleteOrderCommand(o);
                });
            }
        }

        /// <summary>
        /// 克隆订单操作
        /// </summary>
        public ICommand CloneOrderCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnCloneOrderCommand(o);
                });
            }
        }


        /// <summary>
        /// broker免用取消免用操作
        /// </summary>
        public ICommand BrokerFreeCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnBrokerFreeCommand(o);
                });
            }
        }

        /// <summary>
        /// 批量查看确认
        /// </summary>
        public ICommand LookBatchCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnLookBatchCommand(o);
                });
            }
        }

        /// <summary>
        /// 开启发给标准化
        /// </summary>
        public ICommand SendToStandardizationCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnSendToStandardizationCommand(o);
                });
            }
        }

        /// <summary>
        /// 复制操作
        /// </summary>
        public ICommand CopyOrderTxtCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnCopyOrderTxtCommand(o);
                });
            }
        }

        /// <summary>
        /// 代付费操作
        /// </summary>
        public ICommand PayPeeCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnPayPeeCommand(o);
                });
            }
        }


        /// <summary>
        /// 发送qq
        /// </summary>
        public ICommand SendQQmsgCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnSendQQmsgCommand(o);
                });
            }
        }

        /// <summary>
        /// 催单
        /// </summary>
        public ICommand ReminderCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnReminderCommand(o);
                });
            }
        }


        public ICommand PopupWindowCommand
        {
            get
            {
                return new DelegateCommand(o =>
                {
                    OnPopupWindowCommand(o);
                });
            }
        }

        #endregion
        public TransactionPanelViewModel(TransactionPanel transactionPanel)
        {
            _transactionPanel = transactionPanel;
            //订阅App消息
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg);
            //主窗口状态改变消息
            //DesignHandler.Instance.OnNotifyWindowState += Instance_OnNotifyWindowState;
            //DesignHandler.Instance.OnGlobalActivate += Instance_OnGlobalActivate;
            //GRHandler.Instance.SubscribeGrExec(OnSubscribeGrExec);
            TransOrderHandler.Instance.OnUpdateOrder2 += Instance_OnUpdateOrder2;
            DesignHandler.Instance.OnTransactionModelChange += Instance_OnTransactionModelChange;
            TransOrderHandler.Instance.OnClearOrder += Instance_OnClearOrder;
            //订阅桥状态更新
            GRHandler.Instance.SubscribeBridgeStateChange(BridgeStateChange);

            ThreadPool.QueueUserWorkItem(o =>
            {
                AtatisticsCount(o);
            });

            Task.Factory.StartNew(() => { DoRefresh(); }, cancellationTokenSource.Token);
        }

        private void Instance_OnUpdateOrder2(TransOrderServerData obj)
        {
            ThreadPoolHelper.InvokeThreadPool((o) =>
            {
                are.Set();
                UpdateInnerOrder(obj);
            });
        }

        private void BridgeStateChange(BridgeReserveState obj)
        {
            Task.Factory.StartNew(() =>
            {
                //更新成交单
                TransOrderHandler.Instance.BridgeStateProcessOrder(obj);
            });
        }

        private void Instance_OnClearOrder()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (DealTimeChecked)
                {
                    this.DealTimeBegin = DateTime.Now;
                    this.DealTimeEnd = DateTime.Now;
                }
                SetTransModel("");
                QueryOrder();
            }));
        }

        private void Instance_OnTransactionModelChange(object obj)
        {
            if (obj != null)
            {
                SetTransModel(obj.ToString());
            }

        }


        /// <summary>
        /// 初始化参数
        /// </summary>
        private void InitUI()
        {
            this.DealTimeBegin = DateTime.Now;
            this.DealTimeEnd = DateTime.Now;
            IsAllChecked = true;
            this.DealCommand = new DelegateCommand(new Action<object>(OnDealCommand));
            SetTransModel("");

            isInit = true;
            QueryOrder();
        }



        private void SetTransModel(string transactionModel)
        {
            if (string.IsNullOrWhiteSpace(transactionModel))
            {
                transactionModel = IniFileHelper.ReadValue(string.Format("{0}_{1}", TransactionSetting, LocalEnv.UserInfo.UserId), TransactionModel);
            }

            if (!string.IsNullOrWhiteSpace(transactionModel))
            {
                if (transactionModel == "1")
                {
                    _transactionModel = transactionModel;
                    GridHeight = 106;
                    _transactionPanel.tbc_Data.SelectedIndex = 2;
                    _currentDataGrid = _transactionPanel.TransactionDataGridCard;
                }
                else if (transactionModel == "2")
                {
                    _transactionModel = transactionModel;
                    _transactionPanel.tbc_Data.SelectedIndex = 0;
                    _currentDataGrid = _transactionPanel.TransactionDataGrid;
                }
                else
                {
                    _transactionModel = transactionModel;
                    _transactionPanel.tbc_Data.SelectedIndex = 1;
                    _currentDataGrid = _transactionPanel.TransactionDataGridAuto;
                }
            }
            else
            {
                _transactionPanel.tbc_Data.SelectedIndex = 1;
                _currentDataGrid = _transactionPanel.TransactionDataGridAuto;
            }
            LocalEnv.TransactionModel = _transactionModel;
            ExcelChangeSize();
        }

        private void OnDealCommand(object o)
        {
            //Application.Current.Dispatcher.Invoke(() =>
            //{
            //    UnderLineExecWin ui = new UnderLineExecWin();
            //    ui.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //    ui.Show();
            //});
            UnderLineExecWin ui = new UnderLineExecWin();
            BorderWin copyUIBoardWin = new BorderWin();
            copyUIBoardWin.SetTitleAndKey("UnderLineExecWin", "线下成交", "Transaction");
            copyUIBoardWin.SetMainControl(ui);
            //copyUIBoardWin.OnCloseClick += bridgeWin.BridgeWinNew_Closed;
            copyUIBoardWin.Width = 490;
            copyUIBoardWin.Height = 440;
            //copyUIBoardWin.ResizeMode = ResizeMode.NoResize;
            copyUIBoardWin.SetMinVisible(false);
            copyUIBoardWin.SetMaxVisible(false);
            copyUIBoardWin.RemoveWindowsMouseDoubleClickEvent();
            copyUIBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            copyUIBoardWin.Closed += ui.UnderLineExecWin_Closed;
            STC.Common.Utils.WindowManager.Instance.OpenOnlyOneWindow($"Transaction-UnderLineExecWin", copyUIBoardWin);
        }

        private async void OnBridgeCommand(object o)
        {
            //if (bridgeWin == null)
            //{
            //    //await MultDispaterHelper.InvokeByModelName(Constant.OrderBridge, () =>
            //    //{
            //    bridgeWin = new BridgeWinNew();
            //    bridgeWin.Closed += BridgeWin_Closed;
            //    bridgeWin.Show();
            //    //});
            //}
            //else
            //{
            //    //await MultDispaterHelper.InvokeByModelName(Constant.OrderBridge, () =>
            //    //{
            //    if (bridgeWin.Visibility != Visibility.Visible)
            //    {
            //        bridgeWin.ReFlash();
            //        bridgeWin.Show();
            //    }
            //    else
            //    {
            //        bridgeWin.Activate();
            //    }
            //    //});
            //}
        }

        /// <summary>
        /// 查询历史流水
        /// </summary>
        /// <param name="o"></param>
        private async void OnQueryHistoryCommand(object o)
        {
            TransactionOrdermModel transactionOrdermModel = o as TransactionOrdermModel;
            if (transactionOrdermModel == null)
                return;

            GrExecOrderQueryReq request = new GrExecOrderQueryReq();
            request.execid = transactionOrdermModel.ExecID;
            request.pageNum = 0;
            request.pageSize = 10000;

            GrExecOrdersReplyPo reply = await GetExecOrderHistRecordData(request);

            if (reply != null && "0000".Equals(reply.Code) && reply.Datas != null && reply.Datas.Count > 0)
            {

                List<TransOrderServerData> keyValues = new List<TransOrderServerData>();
                foreach (var item in reply.Datas)
                {
                    TransOrderServerData trans = new TransOrderServerData();
                    trans.CopyParentData(item);
                    keyValues.Add(trans);
                }
                if (_hisReocrdWin == null)
                {
                    _hisReocrdWin = new HisReocrdWin();//Windows窗体
                    _hisReocrdWin.Closed += new EventHandler(delegate (object ob, EventArgs ev) { _hisReocrdWin = null; });
                    _hisReocrdWin.Owner = Window.GetWindow(this._transactionPanel);
                    _hisReocrdWin.Show();
                }
                _hisReocrdWin.RefreshDataEx(keyValues);
            }
        }

        /// <summary>
        /// qiao
        /// </summary>
        /// <param name="o"></param>
        private async void OnBridgeFlagCommand(object o)
        {
            //tbk_virFoucs.Focus();
            TransactionOrdermModel orderDetailPo = o as TransactionOrdermModel;

            if (orderDetailPo == null)
                return;

            if (!Check(orderDetailPo))
                return;
            _curOrdermModel = orderDetailPo;
            string bridgeflag = string.Empty;
            if (string.IsNullOrEmpty(orderDetailPo.BridgeFlag) || orderDetailPo.BridgeFlag.Equals("1"))
                bridgeflag = "2";
            else
                bridgeflag = "1";
            //不需要加桥：则要删除桥
            if (bridgeflag.Equals("1"))
            {
                ///桥类型非空，则要么是双桥，要么是单桥，真的有桥的话，需要弹出确认
                if (!string.IsNullOrEmpty(orderDetailPo.BridgeType))
                {
                    if (!IsBridgeWinShow)
                    {
                        MessageBoxGUI messageBoxGUI = new MessageBoxGUI();
                        messageBoxGUI.setWarnMsg("确认删除桥?");
                        messageBoxGUI.Data = orderDetailPo;
                        messageBoxGUI.OnOk += MessageBoxGUI_OnOk;
                        messageBoxGUI.MClosed += MessageBoxGUI_MClosed;
                        messageBoxGUI.Owner = Application.Current.MainWindow;

                        IsBridgeWinShow = true;
                        messageBoxGUI.Show();
                    }
                }
                ///桥类型为空，则此单还没要桥，直接删除
                else
                {
                    UpdateOrderInfo(bridgeflag, ECommandType.修改桥状态);
                    //OrderHelper.ChangeOrder(OrderHelper.GetUpdateOrderPoNew(bridgeflag, ECommandType.修改桥状态, orderPo.id, orderDetailPo.ExecId));
                    //BridgeUIDomain.DeleteBridge(OperationOrderUtils.ToOrderDetail(orderDetailPo), true, false, null);
                }
            }
            ///需要加桥
            else
            {

                UpdateOrderInfo(bridgeflag, ECommandType.修改桥状态);
            }
            //tbk_virFoucs.Focus();
        }



        private void OnBrokerConfirmedCommand(object o)
        {

            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                TransactionOrdermModel orderDetailPo = o as TransactionOrdermModel;
                if (orderDetailPo == null)
                    return;


                if (!string.IsNullOrWhiteSpace(orderDetailPo.Blocknum))
                {
                    List<TransOrderServerData> trans = TransOrderHandler.Instance.GetTransOrderByBlocknum(orderDetailPo.Blocknum);
                    if (trans != null && trans.Count > 0)
                    {
                        List<ExecutionReport> list = new List<ExecutionReport>();
                        foreach (var item in trans)
                        {
                            list.Add(item);
                        }
                        PopupERCollect popupERCollect = new PopupERCollect();
                        popupERCollect.list.AddRange(list);
                        popupERCollect.isUpdate = false;
                        popupERCollect.isFromHistory = true;
                        Common.Handler.PT.TransactionPopupHandler.Instance.InsertEnqueue(popupERCollect);
                    }
                }
                else
                {
                    ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);

                    TransOrderServerData valuePairs = null;
                    if (keyValues != null && keyValues.TryGetValue(orderDetailPo.ExecID, out valuePairs))
                    {
                        List<ExecutionReport> list = new List<ExecutionReport>();
                        list.Add(valuePairs);
                        PopupERCollect popupERCollect = new PopupERCollect();
                        popupERCollect.list.AddRange(list);
                        popupERCollect.isUpdate = false;
                        popupERCollect.isFromHistory = true;
                        Common.Handler.PT.TransactionPopupHandler.Instance.InsertEnqueue(popupERCollect);
                    }
                }


            }));
        }


        public void DoubleClickRow(object o)
        {
            if (o == null)
            {
                return;
            }
            OnBrokerConfirmedCommand(o);
        }

        /// <summary>
        /// 推送市场成交
        /// </summary>
        /// <param name="o"></param>
        private void OnPushMarketCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Button button = o as Button;
                if (button != null)
                {
                    _transactionPanel.tbpop_OrderSide.DataContext = button.DataContext;
                    _transactionPanel.tbpop_OrderSide.PlacementTarget = button;
                    _transactionPanel.tbpop_OrderSide.IsOpen = true;
                }
            }));
        }


        private void OnPushMarketOrderCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Button button = o as Button;
                if (button != null)
                {
                    TransactionOrdermModel orderDetailPo = button.DataContext as TransactionOrdermModel;
                    if (orderDetailPo == null)
                    {
                        return;
                    }
                    _curOrdermModel = orderDetailPo;
                    string forbidRefreshFlag = button.Tag.ToString().Trim();

                    UpdateOrderInfo(forbidRefreshFlag, ECommandType.InternalTransaction);
                    _transactionPanel.tbpop_OrderSide.IsOpen = false;
                }
            }));
        }

        private void OnDeleteCommand(object o)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    if (_delorderWin == null)
            //    {
            //        _delorderWin = new DeleteOrderWin();
            //        _delorderWin.Closed += _delorderWin_Closed;
            //        _delorderWin.Show();
            //    }
            //    else
            //    {
            //        _delorderWin.Activate();
            //    }
            //}));
        }


        private void OnLookCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!_lookWinIsShow)
                {
                    _lookWinIsShow = true;
                    Button bt = o as Button;
                    if (bt == null)
                    {
                        return;
                    }
                    TransactionOrdermModel transactionOrderm = bt.DataContext as TransactionOrdermModel;
                    if (transactionOrderm == null)
                        return;
                    _curOrdermModel = transactionOrderm;
                    ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);

                    TransOrderServerData valuePairs = null;
                    if (keyValues != null && keyValues.TryGetValue(transactionOrderm.ExecID, out valuePairs))
                    {
                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                ExecutionReport LastOrderDetailPo = GetLastExecOrder(valuePairs);
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    ExecUpdateWin updateWin = new ExecUpdateWin();
                                    if (bt != null)
                                    {
                                        ///点击一检查看的时候需要关闭窗口ExecUpdateWin
                                        bt.IsVisibleChanged += (x, y) =>
                                        {
                                            updateWin.Close();
                                        };
                                    }
                                    updateWin.OldOrderDetail = LastOrderDetailPo;
                                    updateWin.NewOrderDetail = valuePairs;
                                    updateWin.OnBtnClick += UpdateWin_OnBtnClick;
                                    updateWin.RefreshForm();

                                    var win = Window.GetWindow(this._transactionPanel);
                                    if (win == null)
                                        updateWin.Owner = Application.Current.MainWindow;
                                    else
                                        updateWin.Owner = win;

                                    updateWin.Show();

                                });
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                                _lookWinIsShow = false;
                            }
                        });
                    }

                }
            }));
        }


        private void OnLookBatchCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);
                List<ExecutionReport> listExecutionReport = new List<ExecutionReport>();
                foreach (var item in InnerOrders)
                {
                    if (TransOrderHelper.Instance.ContainsUserId(item.BrokerUnread, LocalEnv.UserInfo.UserId))
                    {
                        ExecutionReport execOrderPo = new ExecutionReport();
                        //execOrderPo.OrderID = arg1.OrderID;
                        execOrderPo.ExecID = item.ExecID;
                        TransOrderServerData valuePairs = null;
                        if (keyValues != null && keyValues.TryGetValue(execOrderPo.ExecID, out valuePairs))
                        {
                            //我是最后操作人
                            if (LocalEnv.UserInfo.IsMyself(valuePairs.ParamCloseById))
                            {
                                //我同时是brokerA和brokerB
                                bool flag = LocalEnv.UserInfo.IsMyself(valuePairs.PartySubID) && LocalEnv.UserInfo.IsMyself(valuePairs.CounterPartySubID);
                                if (!flag)
                                {
                                    continue;
                                }
                            }
                            execOrderPo.OrdStatus = valuePairs.OrdStatus;
                            execOrderPo.CloseById = LocalEnv.UserInfo.UserId;
                            execOrderPo.CommandType = ((int)ECommandType.确认).ToString();
                            listExecutionReport.Add(execOrderPo);
                        }
                    }
                }

                if (listExecutionReport.Count > 0)
                {
                    UpdateBatchExecOrderData(listExecutionReport);
                }
            }));
        }

        /// <summary>
        /// 删除订单
        /// </summary>
        /// <param name="o"></param>
        private void OnDeleteOrderCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_isShowDeleteWin)
                {
                    //OperationOrderDetail orderDetailPo = dgOrder.SelectedItem as OperationOrderDetail;
                    if (_curOrdermModel == null)
                        return;
                    TransactionOrdermModel orderDetailPo = _curOrdermModel;
                    Common.View.UI.MessageBoxGUI2 warn = new Common.View.UI.MessageBoxGUI2();
                    Window win = Application.Current.MainWindow;
                    if (win != null)
                        warn.Owner = win;
                    warn.tbTitle.Text = "提示";
                    string name = string.Empty;
                    if (LocalEnv.UserInfo.IsMyself(orderDetailPo.BrokerIdA))
                        name = orderDetailPo.BrokerNameB;
                    else if (LocalEnv.UserInfo.IsMyself(orderDetailPo.BrokerIdB))
                        name = orderDetailPo.BrokerNameA;
                    else
                    {
                        warn.tbMsg.Text = "确认删除";
                        name = orderDetailPo.BrokerNameA + "与" + name + orderDetailPo.BrokerNameB;
                    }

                    warn.MClosed += Warn_MClosed;
                    warn.MOK += Warn_MOK;

                    warn.tbMsg1.Text = name;
                    warn.tb_seq.Text = orderDetailPo.Seq;
                    warn.tb_goodCode.Text = orderDetailPo.BondCodeShow;
                    warn.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    _isShowDeleteWin = false;
                    warn.Show();
                }
            }));
        }

        /// <summary>
        /// 克隆订单
        /// </summary>
        /// <param name="o"></param>
        private void OnCloneOrderCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (_curOrdermModel == null)
                    return;
                //currentEditOrder = _orderDetailPo;

                CloneMessageBoxGUI warn = new CloneMessageBoxGUI();
                var win = Window.GetWindow(this._transactionPanel);
                if (win == null)
                    warn.Owner = Application.Current.MainWindow;
                else
                    warn.Owner = win;
                warn.CloneCountEvent += Warn_CloneCountEvent; ;
                warn.ShowDialog();
            }));
        }

        /// <summary>
        /// 复制订单文本
        /// </summary>
        /// <param name="o"></param>
        private void OnCopyOrderTxtCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (_curOrdermModel == null)
                        return;
                    TransactionOrdermModel orderDetailPo = _curOrdermModel;
                    //var op = TransOrderHelper.ToOrderDetail(orderDetailPo);

                    ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);

                    TransOrderServerData valuePairs = null;
                    if (keyValues != null && keyValues.TryGetValue(orderDetailPo.ExecID, out valuePairs))
                    {

                        var assitId = TransOrderHelper.Instance.IsLoginBrokerOrder(valuePairs) ? null : TransOrderHelper.Instance.GetBeAssistedId(valuePairs, false);

                        var sendText = TransOrderHelper.Instance.ConvertOrderDataToTxt(valuePairs, TransOrderHelper.Instance.IsDealHistDay(orderDetailPo), "", assitId).CopyText;

                        ClipboardHelper.CopyToClipboard(sendText);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }));
        }


        /// <summary>
        /// 代付费
        /// </summary>
        /// <param name="o"></param>
        private void OnPayPeeCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    Button button = o as Button;
                    TransactionOrdermModel operationOrder = button.DataContext as TransactionOrdermModel;
                    if (operationOrder == null)
                    {
                        return;
                    }
                    //OrderPo orderPo = OrderHandler.Instance.GetOrderById(operationOrder.OrderId);
                    if (!Check(operationOrder))
                        return;

                    if (_feeSettingWin == null)
                    {
                        _feeSettingWin = new PayFeeSettingWin(operationOrder);
                        _feeSettingWin.Closed += _feeSettingWin_Closed;
                        _feeSettingWin.Show();
                    }
                    else
                    {
                        _feeSettingWin.Activate();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }));
        }


        /// <summary>
        /// 发送qq
        /// </summary>
        /// <param name="o"></param>
        private void OnSendQQmsgCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    if (_curOrdermModel == null)
                        return;
                    TransactionOrdermModel orderDetailPo = _curOrdermModel;
                    if (orderDetailPo == null)
                    {
                        return;
                    }
                    //要发送的内容
                    ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);
                    TransOrderServerData valuePairs = null;
                    if (keyValues != null && keyValues.TryGetValue(orderDetailPo.ExecID, out valuePairs))
                    {

                        var assitId = TransOrderHelper.Instance.IsLoginBrokerOrder(valuePairs) ? null : TransOrderHelper.Instance.GetBeAssistedId(valuePairs, true);

                        var sendText = TransOrderHelper.Instance.ConvertOrderDataToTxt(valuePairs, TransOrderHelper.Instance.IsDealHistDay(orderDetailPo), "", assitId).CopyText;

                        //当前用户是brokerA的助理
                        bool isAssistantA = SystemPowerHandler.Instance.IsAssistant(orderDetailPo.BrokerIdA);
                        //当前用户是brokerb的助理
                        bool isAssistantB = SystemPowerHandler.Instance.IsAssistant(orderDetailPo.BrokerIdB);

                        ///是否brokerA
                        bool isBrokerA = true;

                        //文本集合对象
                        //OrderArgs orderArgs = ConvertTextHelper.ConvertText(order, OrderHelper.TodayReceipt(orderDetailPo), brokerId);
                        //要发送的内容
                        string sendTxt = "";
                        string hidetXT = "1";
                        //判断发送qq消息给谁
                        if (!string.IsNullOrEmpty(orderDetailPo.BrokerIdA))
                        {
                            if (LocalEnv.UserInfo.UserId == orderDetailPo.BrokerIdA || isAssistantA)
                            {
                                if (string.IsNullOrWhiteSpace(orderDetailPo.PartySubId))
                                {
                                    SysWarnGUI warn = new SysWarnGUI();
                                    Window win = Window.GetWindow(this._transactionPanel);
                                    if (win != null)
                                        warn.Owner = win;
                                    warn.tbTitle.Text = "提示";
                                    warn.tbMsg.Text = "交易员尚未填写，无法发送";
                                    warn.Show();
                                }
                                else
                                {
                                    string brokerID = LocalEnv.UserInfo.UserId;
                                    if (GRSettingsHandler.AppSettingIsTrue(Constant.WithAssistantQQSendMsg))
                                    {
                                        brokerID = orderDetailPo.BrokerIdA;
                                    }
                                    TransOrderTxt orderArgs = TransOrderHelper.Instance.ConvertOrderDataToTxt(valuePairs, TransOrderHelper.Instance.IsDealHistDay(orderDetailPo), "1", assitId);


                                    if (GRSettingsHandler.GlobSystemParameter.HideCounterpartySend)
                                    {
                                        sendTxt = orderArgs.HideCounterpartyText;
                                        hidetXT = "1";
                                    }
                                    else
                                    {
                                        sendTxt = orderArgs.CopyText;
                                        hidetXT = "0";
                                    }
                                    SendQQMsgReq request = new SendQQMsgReq();
                                    request.institutionId = orderDetailPo.InstitutionID;
                                    request.traderId = orderDetailPo.TraderID;
                                    request.message = sendTxt;
                                    request.sendType = "3";
                                    request.execId = orderDetailPo.ExecID;
                                    request.partySubId = brokerID;
                                    request.userName = LocalEnv.UserInfo.FullName;
                                    request.HideCounterpartySend = hidetXT;
                                    request.ifAppend = GRSettingsHandler.AppSettingIsTrue(Constant.WithQQElementChange) ? "1" : "0";

                                    request.commandType = ((int)ECommandType.SendDetailQQMsgA).ToString();
                                    // request.singleQQMsgSendFlaga = "1";
                                    //request.execOrders.Add(updateExecOrderPo);
                                    LogHelper.Info($"交易qq消息,交易员：{orderDetailPo.TraderAInfoDisplay}，消息：\r\n{sendTxt}");
                                    ThreadPool.QueueUserWorkItem(delegate
                                    {
                                        var reply = GRHandler.Instance.sendQQMsg(request);
                                        if (reply != null && reply.Code != "0000")
                                        {
                                            ShowMessage(reply.Msg);
                                        }
                                    });

                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(orderDetailPo.BrokerIdB))
                        {
                            if (LocalEnv.UserInfo.UserId == orderDetailPo.BrokerIdB || isAssistantB)
                            {
                                if (string.IsNullOrWhiteSpace(orderDetailPo.TraderIDB))
                                {
                                    if (!string.IsNullOrWhiteSpace(orderDetailPo.TraderIDB))
                                    {
                                        SysWarnGUI warn = new SysWarnGUI();
                                        Window win = Window.GetWindow(this._transactionPanel);
                                        if (win != null)
                                            warn.Owner = win;
                                        warn.tbTitle.Text = "提示";
                                        warn.tbMsg.Text = "交易员尚未填写，无法发送";
                                        warn.Show();
                                    }
                                }
                                else
                                {
                                    if (orderDetailPo.TraderID != orderDetailPo.TraderIDB)
                                    {
                                        string brokerID = LocalEnv.UserInfo.UserId;
                                        if (GRSettingsHandler.AppSettingIsTrue(Constant.WithAssistantQQSendMsg))
                                        {
                                            brokerID = orderDetailPo.BrokerIdB;
                                        }
                                        TransOrderTxt orderArgs = TransOrderHelper.Instance.ConvertOrderDataToTxt(valuePairs, TransOrderHelper.Instance.IsDealHistDay(orderDetailPo), "2", assitId);
                                        if (GRSettingsHandler.GlobSystemParameter.HideCounterpartySend)
                                        {
                                            sendTxt = orderArgs.HideCounterpartyText;
                                            hidetXT = "1";
                                        }
                                        else
                                        {
                                            sendTxt = orderArgs.CopyText;
                                            hidetXT = "0";
                                        }
                                        SendQQMsgReq request = new SendQQMsgReq();
                                        request.institutionId = orderDetailPo.CounterPartyInstitutionId;
                                        request.traderId = orderDetailPo.TraderIDB;
                                        request.message = sendTxt;
                                        request.sendType = "3";
                                        request.execId = orderDetailPo.ExecID;
                                        request.partySubId = brokerID;
                                        request.userName = LocalEnv.UserInfo.FullName;
                                        request.HideCounterpartySend = hidetXT;
                                        request.ifAppend = GRSettingsHandler.AppSettingIsTrue(Constant.WithQQElementChange) ? "1" : "0";
                                        request.commandType = ((int)ECommandType.SendDetailQQMsgB).ToString();
                                        //request.singleQQMsgSendFlagb = "1";

                                        LogHelper.Info($"交易qq消息,交易员：{orderDetailPo.TraderBInfoDisplay}，消息：\r\n{sendTxt}");
                                        ThreadPool.QueueUserWorkItem(delegate
                                        {
                                            var reply = GRHandler.Instance.sendQQMsg(request);
                                            if (reply != null && reply.Code != "0000")
                                            {
                                                ShowMessage(reply.Msg);
                                            }
                                        });

                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }));
        }


        /// <summary>
        /// 催单
        /// </summary>
        /// <param name="o"></param>
        private void OnReminderCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {

                try
                {
                    if (_curOrdermModel == null)
                        return;
                    TransactionOrdermModel orderDetailPo = _curOrdermModel;

                    bool? sendResult = null;
                    //要发送的内容
                    ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);

                    TransOrderServerData valuePairs = null;
                    if (keyValues != null && keyValues.TryGetValue(orderDetailPo.ExecID, out valuePairs))
                    {

                        var assitId = TransOrderHelper.Instance.IsLoginBrokerOrder(valuePairs) ? null : TransOrderHelper.Instance.GetBeAssistedId(valuePairs, true);

                        var sendText = TransOrderHelper.Instance.ConvertOrderDataToTxt(valuePairs, TransOrderHelper.Instance.IsDealHistDay(orderDetailPo), "", assitId).CopyText;

                        string sendTxt = $"内码 {orderDetailPo.Seq}) {sendText}\r\n催催";

                        TransOrderHandler.Instance.SendQQMsgToBroker(valuePairs, sendTxt, "Broker主订单");
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error("主订单催单失败", ex);
                }
                try
                {
                    if (_curOrdermModel == null)
                        return;
                    TransactionOrdermModel orderDetailPo = _curOrdermModel;
                    //var op = TransOrderHelper.ToOrderDetail(orderDetailPo);


                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
            }));
        }


        private void OnPopupWindowCommand(object o)
        {
            STC.Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new STC.Common.Model.Fix.Message.PResult() { Code = "TriggerGrCommand", Info1 = "TransactionPanel" });
        }

        private void _feeSettingWin_Closed(object sender, EventArgs e)
        {
            _feeSettingWin.Closed -= _feeSettingWin_Closed;
            _feeSettingWin = null;
        }

        private bool IsContianUser(string brokerName)
        {

            bool isContian = false;
            //SystemParameter currentSystemParamter = SystemParamHandler.GlobSystemParameter;
            //List<AccountInfo> updateOrders = null;
            //if (currentSystemParamter != null
            //    && currentSystemParamter.UpdateOrders != null
            //    && currentSystemParamter.UpdateOrders.Count > 0)
            //{
            //    foreach (AccountInfo item in currentSystemParamter.UpdateOrders)
            //    {
            //        if (item.userName == LocalEnv.UserInfo.UserName)
            //        {
            //            updateOrders = item.Users?.ToList();
            //            break;
            //        }
            //    }
            //}
            //if (updateOrders != null && updateOrders.Count > 0)
            //{
            //    if (updateOrders.Any(_ => _.Name == brokerName))
            //    {
            //        isContian = true;
            //    }
            //    else
            //    {
            //        isContian = false;
            //    }
            //}
            //else
            //{
            //    isContian = false;
            //}
            return isContian;
        }

        /// <summary>
        /// broker免用取消免用
        /// </summary>
        /// <param name="o"></param>
        private void OnBrokerFreeCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                Button button = o as Button;
                if (button == null)
                    return;
                TransactionOrdermModel orderDetailPo = button.DataContext as TransactionOrdermModel;
                if (orderDetailPo == null)
                    return;
                _curOrdermModel = orderDetailPo;
                // 检查成交单是否可以修改
                if (!Check(_curOrdermModel))
                    return;
                MessageBoxGUI messageBoxGUI = new MessageBoxGUI();

                if (button.Tag.ToString() == "A")
                {

                    if (string.IsNullOrEmpty(orderDetailPo.BrokerAFree) || EBrokerage.不免佣.EqualsToValue(orderDetailPo.BrokerAFree))
                    {
                        messageBoxGUI.setWarnMsg("确认免佣?");
                        messageBoxGUI.MOK += MessageBoxGUI_MOK;
                    }
                    else if (EBrokerage.免佣.EqualsToValue(orderDetailPo.BrokerAFree))
                    {
                        messageBoxGUI.setWarnMsg("确认取消免佣?");
                        messageBoxGUI.MOK += MessageBoxGUI_MOK1;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(orderDetailPo.BrokerBFree) || EBrokerage.不免佣.EqualsToValue(orderDetailPo.BrokerBFree))
                    {
                        messageBoxGUI.setWarnMsg("确认免佣?");
                        messageBoxGUI.MOK += MessageBoxGUI_MOK2;
                    }
                    else if (EBrokerage.免佣.EqualsToValue(orderDetailPo.BrokerBFree))
                    {
                        messageBoxGUI.setWarnMsg("确认取消免佣?");
                        messageBoxGUI.MOK += MessageBoxGUI_MOK3;
                    }
                }

                var win = Window.GetWindow(this._transactionPanel);
                if (win == null)
                    messageBoxGUI.Owner = Application.Current.MainWindow;
                else
                    messageBoxGUI.Owner = win;

                messageBoxGUI.ShowDialog();
            }));
        }


        /// <summary>
        /// 开启发给标准化
        /// </summary>
        /// <param name="o"></param>
        private void OnSendToStandardizationCommand(object o)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    Button button = o as Button;
                    if (button == null)
                    {
                        return;
                    }
                    TransactionOrdermModel orderDetailPo = button.DataContext as TransactionOrdermModel;
                    if (orderDetailPo == null)
                    {
                        return;
                    }
                    _curOrdermModel = orderDetailPo;
                    orderDetailPo.IssueState = "1";

                    UpdateOrderInfo("1", ECommandType.OpenSendStandardization);
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex.ToString());
                }
            }));
        }

        private void MessageBoxGUI_MOK3()
        {
            UpdateOrderInfo(EBrokerage.不免佣.ToChar(), ECommandType.修改BridgeB是否免佣金);
        }

        private void MessageBoxGUI_MOK2()
        {
            UpdateOrderInfo(EBrokerage.免佣.ToChar(), ECommandType.修改BridgeB是否免佣金);
        }

        private void MessageBoxGUI_MOK1()
        {
            UpdateOrderInfo(EBrokerage.不免佣.ToChar(), ECommandType.修改BridgeA是否免佣金);
        }

        private void MessageBoxGUI_MOK()
        {
            UpdateOrderInfo(EBrokerage.免佣.ToChar(), ECommandType.修改BridgeA是否免佣金);
        }

        private async void Warn_CloneCountEvent(int obj)
        {
            if (_curOrdermModel == null)
                return;
            string id = _curOrdermModel.ExecID;
            CloneRequestPo request = new CloneRequestPo();
            request.execId = id;
            request.num = obj;
            await CloneExecOrderData(request);

        }

        private void Warn_MOK()
        {
            _isShowDeleteWin = true;
            TransactionOrdermModel orderDetailPo = _currentDataGrid.SelectedItem as TransactionOrdermModel;
            if (orderDetailPo == null)
                return;

            if (orderDetailPo != null)
            {
                _curOrdermModel = orderDetailPo;
                UpdateOrderInfo("1", ECommandType.删除成交单);
            }
        }

        private void Warn_MClosed()
        {
            _isShowDeleteWin = true;
        }

        private void UpdateWin_OnBtnClick(ExecutionReport arg1, bool arg2)
        {
            _lookWinIsShow = false;
            if (arg2)
            {
                if (arg1 == null)
                    return;
                ExecutionReport execOrderPo = new ExecutionReport();
                //execOrderPo.OrderID = arg1.OrderID;
                execOrderPo.ExecID = arg1.ExecID;
                ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);

                TransOrderServerData valuePairs = null;
                if (keyValues != null && keyValues.TryGetValue(execOrderPo.ExecID, out valuePairs))
                {
                    //if (valuePairs.BrokerUnread != null && valuePairs.BrokerUnread.ContainsUserId(valuePairs.CounterPartySubID))
                    //{
                    execOrderPo.OrdStatus = valuePairs.OrdStatus;
                    execOrderPo.CloseBy = LocalEnv.UserInfo.UserId;
                    execOrderPo.CommandType = ((int)ECommandType.确认).ToString();
                    UpdateExecOrderData(execOrderPo);
                    //}

                }
            }
            //UpdateOrderInfo("1", ECommandType.确认);
        }

        private TransOrderServerData GetLastExecOrder(TransOrderServerData detailPo)
        {
            TransOrderServerData result = null;
            GrExecOrderQueryReq request = new GrExecOrderQueryReq();
            request.execid = detailPo.ExecID;
            request.pageNum = 0;
            request.pageSize = 10000000;

            GrExecOrdersReplyPo reply = GRHandler.Instance.ExecOrderHistRecordQuery(request);

            if (reply != null && "0000".Equals(reply.Code) && reply.Datas != null && reply.Datas.Count > 0)
            {
                if (reply.Datas != null && reply.Datas.Count > 0)
                {
                    List<TransOrderServerData> orderDatas = new List<TransOrderServerData>();
                    TransOrderHelper.Instance.ConvertBaseDataToTransData(orderDatas, reply.Datas);
                    if (!LocalEnv.UserInfo.IsMyself(detailPo.PartySubID) && !LocalEnv.UserInfo.IsMyself(detailPo.CounterPartySubID))
                        result = this.GetOrderDetailPoForC(orderDatas);
                    else
                    {
                        FilterOrder(orderDatas);
                        TransOrderServerData lastOrder = null;//最后一个从无到有
                        List<TransOrderServerData> sortList = orderDatas.Where(delegate (TransOrderServerData orderDetailPo)
                        {
                            if (orderDetailPo.OrdStatus.Equals("G"))
                            {
                                if (string.IsNullOrEmpty(orderDetailPo.BrokerUnread))
                                    return true;
                                else if (!orderDetailPo.BrokerUnread.ContainsUserId(orderDetailPo.PartySubID) && !orderDetailPo.BrokerUnread.ContainsUserId(orderDetailPo.CounterPartySubID))
                                    return true;
                                else
                                    return false;
                            }
                            else
                                return false;
                        }).ToList();


                        sortList = sortList.OrderBy(o => o.ID).ToList();
                        List<TransOrderServerData> temOrderList = new List<TransOrderServerData>();
                        for (int i = 0; i < sortList.Count; i++)
                        {
                            if (string.IsNullOrEmpty(sortList[i].BrokerUnread))
                                temOrderList.Add(sortList[i]);
                            else if (!sortList[i].BrokerUnread.ContainsUserId(sortList[i].PartySubID) && !sortList[i].BrokerUnread.ContainsUserId(sortList[i].CounterPartySubID))
                            {
                                ExecutionReport preDetailPo = temOrderList.Last();
                                //过滤线下成交时，修改TraderA从无到有得成交
                                if (CheckOrderConfirm(preDetailPo, sortList[i]))
                                {
                                    temOrderList.Add(sortList[i]);
                                }
                                else
                                {
                                    if (lastOrder != null)
                                    {
                                        if (CheckPropertyChange(lastOrder, sortList[i]))
                                            temOrderList.Add(sortList[i]);
                                    }
                                    lastOrder = sortList[i];
                                }
                            }
                        }
                        if (temOrderList.Count > 0)
                        {
                            temOrderList = temOrderList.OrderByDescending(o => o.ID).ToList();
                            result = temOrderList[0];
                        }
                    }
                }
            }

            return result;
        }





        /// <summary>
        /// 用于两次A确认操作之间的比较
        /// 比较成交单要素是否修改
        /// </summary>
        /// <param name="oldOrder"></param>
        /// <param name="newOrder"></param>
        /// <returns></returns>
        private bool CheckPropertyChange(ExecutionReport oldOrder, ExecutionReport newOrder)
        {
            if (!IDCHelper.CheckStr(oldOrder.SendTo, newOrder.SendTo))
                return true;
            if (!IDCHelper.CheckStr(oldOrder.SendTo2, newOrder.SendTo2))
                return true;
            if (!IDCHelper.CheckStr(oldOrder.SecurityID, newOrder.SecurityID))
                return true;
            if (!IDCHelper.CheckStr(oldOrder.Price, newOrder.Price))
                return true;
            if (!IDCHelper.CheckStr(oldOrder.LastQty + "", newOrder.LastQty + ""))
                return true;
            TransactionOrdermModel ordermModel = new TransactionOrdermModel()
            {

            };

            if (!IDCHelper.CheckStr(TransOrderHelper.Instance.GetDisplaySettle(oldOrder), TransOrderHelper.Instance.GetDisplaySettle(newOrder)))
                return true;
            if (!IDCHelper.CheckStr(oldOrder.PartySubName, newOrder.PartySubName))
                return true;
            if (!IDCHelper.CheckStr(oldOrder.CounterPartySubName, newOrder.CounterPartySubName))
                return true;
            if (!IDCHelper.CheckStr(GetTraderBInfo(oldOrder), GetTraderBInfo(newOrder)))
                return true;
            if (!IDCHelper.CheckStr(GetTraderAInfo(oldOrder), GetTraderAInfo(newOrder)))
                return true;

            return false;
        }


        private bool CheckOrderConfirm(ExecutionReport preDetailPo, ExecutionReport curDetailPo)
        {
            bool result = false;
            //过滤线下成交时，修改TraderA从无到有得成交
            //过滤线下录入，修改TraderA从无到有的成交
            //traderA没有从无到有&&sendto没有从无到有
            string traderAInfo = GetTraderAInfo(preDetailPo);
            string traderAInfo_c = GetTraderBInfo(preDetailPo);
            if (!(string.IsNullOrEmpty(traderAInfo) && !string.IsNullOrEmpty(traderAInfo_c))
                && !(string.IsNullOrWhiteSpace(preDetailPo.TraderName) && !string.IsNullOrWhiteSpace(preDetailPo.InstitutionName) && !string.IsNullOrEmpty(traderAInfo_c)))
                result = true;

            if (result && !(string.IsNullOrEmpty(preDetailPo.SendTo) && !string.IsNullOrEmpty(curDetailPo.SendTo))
                && !(string.IsNullOrEmpty(preDetailPo.SendTo2) && !string.IsNullOrEmpty(curDetailPo.SendTo2)))
                result = true;
            else
                result = false;
            return result;
        }
        /// <summary>
        /// 过滤掉查询操作
        /// </summary>
        /// <param name="detailPos"></param>
        private void FilterOrder(List<TransOrderServerData> detailPos)
        {
            List<TransOrderServerData> removeOrder = new List<TransOrderServerData>();
            foreach (TransOrderServerData detail in detailPos)
            {
                if ((detail.QueryFlag + "").Equals("1"))
                    removeOrder.Add(detail);
            }
            if (removeOrder.Count > 0)
            {
                foreach (TransOrderServerData detail in removeOrder)
                {
                    detailPos.Remove(detail);
                }
            }
        }
        private TransOrderServerData GetOrderDetailPoForC(List<TransOrderServerData> data)
        {
            TransOrderServerData result = null;

            List<TransOrderServerData> sortList = data.Where(_ => _.OrdStatus == "G").ToList();
            sortList = sortList.OrderByDescending(o => o.UpdateTime).ToList();
            for (int i = 0; i < sortList.Count; i++)
            {
                if (!LocalEnv.UserInfo.IsContainsUserId(sortList[i].BrokerUnread))
                {
                    result = sortList[i];
                    break;
                }
            }
            return result;
        }

        private void _delorderWin_Closed(object sender, EventArgs e)
        {
            _delorderWin = null;
        }

        private void MessageBoxGUI_MClosed()
        {
            IsBridgeWinShow = false;
        }

        private void MessageBoxGUI_OnOk(object obj)
        {
            IsBridgeWinShow = false;
            TransactionOrdermModel orderDetailPo = obj as TransactionOrdermModel;
            if (orderDetailPo != null)
            {
                ConcurrentDictionary<string, TransOrderServerData> keyValues = TransOrderHandler.Instance.GetAllOrder(false);

                TransOrderServerData valuePairs = null;
                if (keyValues != null && keyValues.TryGetValue(orderDetailPo.ExecID, out valuePairs))
                {
                    DeleteBridge(valuePairs, true, false, null);
                }
            }
            //    //BridgeUIDomain.DeleteBridge(OperationOrderUtils.ToOrderDetail(orderDetailPo), true, false, null);
        }

        private void BridgeWin_Closed(object sender, EventArgs e)
        {

        }

        public void Init(TransactionPanel ownWindow)
        {
            myWindow = ownWindow;
            InitUI();
        }

        public void SubscribeAppDetailMsg()
        {
            Common.Handler.Base.BaseHandler.Instance.SubscribeAppMsg(OnAppMsg1);
        }

        /// <summary>
        /// 删除桥。
        /// </summary>
        /// <param name="deleteOrderDetail">主订单或者桥订单，主订单删双桥，桥订单删单桥</param>
        /// <param name="isFromDetail">是否从成交明细删除</param>
        /// <param name="isBid"></param>
        /// <param name="defaultBridge">默认的桥。获取其渠道和发给</param>
        public async void DeleteBridge(ExecutionReport deleteOrderDetail, bool isFromDetail, bool isBid, UserBridge defaultBridge)
        {

            ExecutionReport execOrderPo = new ExecutionReport();
            execOrderPo.OrderID = deleteOrderDetail.OrderID;
            execOrderPo.ExecID = deleteOrderDetail.ExecID;
            execOrderPo.CommandType = ((int)ECommandType.删除桥).ToString();
            execOrderPo.OperateBorderID = "";

            //borderID传给后台 
            //传到后台，则只删MasterID对应的桥，不传，则两座桥都删掉
            //if (deleteOrderDetail is BridgeOrderDetail)
            //    execOrderPo.operateBorderID = ((BridgeOrderDetail)deleteOrderDetail)?.MasterID;
            //execOrderPo.operateBorderID = (deleteOrderDetail as BridgeOrderDetail)?.MasterID;
            execOrderPo.DoubleBridgeSettleSide = string.Empty;
            //删除双桥
            if (string.IsNullOrWhiteSpace(deleteOrderDetail.BridgeType) || deleteOrderDetail.BridgeType.Equals("B,O") || isFromDetail)
            {
                //桥
                execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId2 = string.Empty;
                //execOrderPo.bridgebankcode = execOrderPo.bridgebankcode2 = string.Empty;
                execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName2 = string.Empty;
                execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId2 = string.Empty;
                execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName2 = string.Empty;
                //计费
                //execOrderPo.feeInstId = execOrderPo.feeInstId2 = string.Empty;
                //execOrderPo.feeInstName = execOrderPo.feeInstName2 = string.Empty;
                //execOrderPo.feeInstTradeId = execOrderPo.feeInstTradeId2 = string.Empty;
                //execOrderPo.feeinstt = execOrderPo.feeInstTradeName2 = string.Empty;
                //桥 费用 渠道 备注 发给          
                execOrderPo.BridgeFee = execOrderPo.BridgeFee2 = string.Empty;
                execOrderPo.BridgeChannel = execOrderPo.BridgeChannel2 = string.Empty;
                execOrderPo.BridgeRemark = execOrderPo.BridgeRemark2 = string.Empty;
                execOrderPo.BridgeRemarkShow = execOrderPo.BridgeRemarkShow2 = "1";
                execOrderPo.BridgeSendTo = execOrderPo.BridgeSendTo2 = string.Empty;
                //execOrderPo.contactType = execOrderPo.contactType = string.Empty;
                //主发给
                execOrderPo.SendTo = execOrderPo.SendTo2 = string.Empty;
                //桥类型
                execOrderPo.BridgeType = execOrderPo.BridgeType2 = string.Empty;

                //删除交易要素备注
                //execOrderPo.combinedOrderRemark = string.Empty;
                //execOrderPo.combinedOrderRemark2 = string.Empty;
                //删除双桥信息
                execOrderPo.DoubleBridgeBidSettltype = execOrderPo.DoubleBridgeOffSettltype = string.Empty;
                execOrderPo.DoubleBridgeChannel = execOrderPo.DoubleBridgeFee = execOrderPo.DoubleBridgeRemark = execOrderPo.DoubleBridgeSendTo = string.Empty;
                if (isFromDetail)
                    execOrderPo.BridgeFlag = "1";
            }
            else
            {
                if (isBid)
                {
                    //execOrderPo.BidSpecialBridgeFee = deleteOrderDetail.bridgeOrder2.BidSpecialBridgeFee;
                    //execOrderPo.OfrSpecialBridgeFee = deleteOrderDetail.bridgeOrder.OfrSpecialBridgeFee;
                    //execOrderPo.BridgeSendTo2 = deleteOrderDetail.brigdeor.bidDir == 1 ? deleteOrderDetail.BidbankAgentName : defaultBridge.sendto;
                    execOrderPo.BridgeSendTo = deleteOrderDetail.BridgeSendTo;
                    ///删除bid桥，则保留ofr桥信息
                    execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId = deleteOrderDetail.BridgeInstitutionId;
                    execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName = deleteOrderDetail.BridgeInstitutionName;
                    execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId = deleteOrderDetail.BridgeTraderId;
                    execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName = deleteOrderDetail.BridgeTraderName;
                    //execOrderPo.bridgebankcode = execOrderPo.bridgebankcode2 = deleteOrderDetail.bridgebankcode;
                    //计费
                    //execOrderPo.feeInstId = execOrderPo.feeInstId2 = deleteOrderDetail.bridgeOrder?.feeInstId;
                    //execOrderPo.feeInstName = execOrderPo.feeInstName2 = deleteOrderDetail.bridgeOrder?.feeInstName;
                    //execOrderPo.feeInstTradeId = execOrderPo.feeInstTradeId2 = deleteOrderDetail.bridgeOrder?.feeInstTradeId;
                    //execOrderPo.feeInstTradeName = execOrderPo.feeInstTradeName2 = deleteOrderDetail.bridgeOrder?.feeInstTradeName;
                }
                else
                {
                    execOrderPo.BridgeSendTo = deleteOrderDetail.BridgeSendTo;
                    ///删除bid桥，则保留ofr桥信息
                    execOrderPo.BridgeInstitutionId = execOrderPo.BridgeInstitutionId = deleteOrderDetail.BridgeInstitutionId2;
                    execOrderPo.BridgeInstitutionName = execOrderPo.BridgeInstitutionName = deleteOrderDetail.BridgeInstitutionName2;
                    execOrderPo.BridgeTraderId = execOrderPo.BridgeTraderId = deleteOrderDetail.BridgeTraderId2;
                    execOrderPo.BridgeTraderName = execOrderPo.BridgeTraderName = deleteOrderDetail.BridgeTraderName2;
                    //execOrderPo.BidSpecialBridgeFee = deleteOrderDetail.bridgeOrder.BidSpecialBridgeFee;
                    //execOrderPo.OfrSpecialBridgeFee = deleteOrderDetail.bridgeOrder2.OfrSpecialBridgeFee;
                    //execOrderPo.bridgesendto = deleteOrderDetail.bridgeOrder.ofrDir == 1 ? deleteOrderDetail.OfrbankAgentName : defaultBridge.sendto;
                    //execOrderPo.bridgesendto2 = deleteOrderDetail.bridgesendto2;
                    /////删除ofr桥，则保留bid桥信息
                    //execOrderPo.bridgebankid = execOrderPo.bridgebankid2 = deleteOrderDetail.bridgebankid2;
                    //execOrderPo.bridgebankcodeshort = execOrderPo.bridgebankcodeshort2 = deleteOrderDetail.bridgebankcodeshort2;
                    //execOrderPo.bridgebankagentid = execOrderPo.bridgebankagentid2 = deleteOrderDetail.bridgebankagentid2;
                    //execOrderPo.bridgebankagentname = execOrderPo.bridgebankagentname2 = deleteOrderDetail.bridgebankagentname2;
                    //execOrderPo.bridgebankcode = execOrderPo.bridgebankcode2 = deleteOrderDetail.bridgebankcode2;
                    ////计费
                    //execOrderPo.feeInstId = execOrderPo.feeInstId2 = deleteOrderDetail.bridgeOrder2?.feeInstId;
                    //execOrderPo.feeInstName = execOrderPo.feeInstName2 = deleteOrderDetail.bridgeOrder2?.feeInstName;
                    //execOrderPo.feeInstTradeId = execOrderPo.feeInstTradeId2 = deleteOrderDetail.bridgeOrder2?.feeInstTradeId;
                    //execOrderPo.feeInstTradeName = execOrderPo.feeInstTradeName2 = deleteOrderDetail.bridgeOrder2?.feeInstTradeName;
                }

                //双桥结算和渠道
                execOrderPo.DoubleBridgeBidSettltype = execOrderPo.DoubleBridgeOffSettltype = string.Empty;
                execOrderPo.DoubleBridgeChannel = execOrderPo.DoubleBridgeFee = execOrderPo.DoubleBridgeRemark = execOrderPo.DoubleBridgeSendTo = string.Empty;
                //添加双桥渠道默认显示 20200318 
                //2个桥删掉一个再加桥的时候，双桥渠道不为空，会影响双桥的默认结算，结果为空 20200429
                //execOrderPo.doublebridgechannel = GetDefaultChannel(deleteOrderDetail, defaultBridge);
                //双桥删桥操作，设置成交单子的默认发给 20200320

                //重置主发给
                UserBridge BridgeTemp = defaultBridge == null ? new UserBridge() : UserBridge.Copy(defaultBridge);
                //BridgeTemp.channel = GetDefaultChannel(deleteOrderDetail, defaultBridge);
                //if (OrderHelper.BridgeIsAllConfirmed(deleteOrderDetail))
                //{
                //    string BridgeTempSendto = BridgeTemp.sendto;
                //    BridgeTemp.channel = deleteOrderDetail.bridgechannel;
                //    bool isSameSendtoWithDefault = execOrderPo.bridgesendto == BridgeTempSendto;
                //    bool isSameSendtoWithTrader = execOrderPo.bridgesendto == deleteOrderDetail.OfrbankAgentName;
                //    BridgeTemp.sendto = execOrderPo.bridgesendto;
                //    execOrderPo.sendto2 = SetSenttoNew(deleteOrderDetail, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, deleteOrderDetail.bridgeOrder.ofrDir, deleteOrderDetail.OfrInst);
                //    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.bridgefee))
                //    {
                //        execOrderPo.sendto2 += ", " + deleteOrderDetail.bridgefee;
                //    }
                //    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.bridgeremark))
                //    {
                //        execOrderPo.sendto2 += ", " + deleteOrderDetail.bridgeremark;
                //    }

                //    BridgeTemp.channel = deleteOrderDetail.bridgechannel2;
                //    isSameSendtoWithDefault = execOrderPo.bridgesendto2 == BridgeTempSendto;
                //    isSameSendtoWithTrader = execOrderPo.bridgesendto2 == deleteOrderDetail.BidbankAgentName;
                //    BridgeTemp.sendto = execOrderPo.bridgesendto2;
                //    execOrderPo.sendto = SetSenttoNew(deleteOrderDetail, BridgeTemp, isSameSendtoWithDefault, isSameSendtoWithTrader, deleteOrderDetail.bridgeOrder2.bidDir, deleteOrderDetail.BidInst);
                //    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.bridgefee2))
                //    {
                //        execOrderPo.sendto += ", " + deleteOrderDetail.bridgefee2;
                //    }
                //    if (!string.IsNullOrWhiteSpace(deleteOrderDetail.bridgeremark2))
                //    {
                //        execOrderPo.sendto += ", " + deleteOrderDetail.bridgeremark2;
                //    }
                //}

                //execOrderPo.bridgeType2 = execOrderPo.bridgeType = "B,O";
                execOrderPo.SendTo = execOrderPo.SendTo2 = "";
                //重置方向
                //execOrderPo.ofrDir = 2;
                //execOrderPo.bidDir = 2;

            }
            await UpdateExecOrderData(execOrderPo);
            //OrderHelper.ChangeOrder(request);
        }
        private void View_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    int ureadCount = 0;
            //    List<TransactionOrdermModel> list = this.Mv.View.Cast<TransactionOrdermModel>().ToList();
            //    RowCount = list.Where(_ => (_.BrokerIdA == LocalEnv.UserInfo.UserId || _.BrokerIdB == LocalEnv.UserInfo.UserId)
            //    && _.OrderStatus == "G" && !TransOrderHelper.Instance.IsDealHistDay(_)).Count();
            //    foreach (var item in list)
            //    {
            //        //查看成交修改按【查看】按钮
            //        if (!string.IsNullOrEmpty(item.BrokerUnread))
            //        {
            //            if (item.BrokerUnread.ContainsUserId(LocalEnv.UserInfo.UserId))
            //            {
            //                //我是最后操作人
            //                if (LocalEnv.UserInfo.IsMyself(item.BridgeCloseById))
            //                {
            //                    //我同时是brokerA和brokerB
            //                    if (LocalEnv.UserInfo.IsMyself(item.BrokerIdA) && LocalEnv.UserInfo.IsMyself(item.BrokerIdB))
            //                    {
            //                        ureadCount++;
            //                    }
            //                }
            //                else
            //                {
            //                    ureadCount++;
            //                }
            //            }
            //            //else
            //            //{
            //            //    ureadCount++;
            //            //}
            //        }

            //    }
            //    UnreadTxt = $"忽略{ureadCount}条";
            //    //BidRowCount = list.Count(p => p.Count_InnerBid > 0);
            //    //OfrRowCount = list.Count(p => p.Count_InnerOfr > 0);
            //}));
        }

        #region 查询和处理数据
        /// <summary>
        /// 查询 
        /// </summary>
        public void Query()
        {
            BeginQuery();

            if (IsAllChecked)
            {
                RequestIntentionOrder("", "");
                _dealDateEnd = "";
                return;
            }
            else
            {
                if (DealTimeChecked)
                {
                    string dealDateBegin = this.DealTimeBegin.HasValue ? $"{this.DealTimeBegin.Value.ToString("yyyy-MM-dd")}" : "";
                    string dealDateEnd = this.DealTimeEnd.HasValue ? $"{this.DealTimeEnd.Value.ToString("yyyy-MM-dd")}" : "";
                    _dealDateEnd = dealDateEnd;
                    RequestIntentionOrder(dealDateBegin, dealDateEnd);
                }
                else
                {
                    _dealDateEnd = "";
                }
            }
        }


        private void QueryOrder()
        {
            BeginQuery();
            if (IsAllChecked)
            {
                QueryOrderData("");
                _dealDateEnd = "";
                return;
            }
            else
            {
                if (DealTimeChecked)
                {
                    string dealDateEnd = this.DealTimeEnd.HasValue ? $"{this.DealTimeEnd.Value.ToString("yyyy-MM-dd")}" : "";
                    _dealDateEnd = dealDateEnd;
                    QueryOrderData(dealDateEnd);
                    //QuantityUnreadCount();
                }
                else
                {
                    _dealDateEnd = "";
                }
            }
        }
        /// <summary>
        /// 按日期查询数据，默认当天，当dealTimeChecked = false时查询所有
        /// </summary>
        /// <param name="dealTimeBegin"></param>
        /// <param name="dealTimeEnd"></param>
        /// <param name="dealTimeChecked"></param>
        private async void RequestIntentionOrder(string dealTimeBegin = "", string dealTimeEnd = "", string seq = "")
        {

            List<TransOrderServerData> executionReportList = new List<TransOrderServerData>();
            await Task.Factory.StartNew(() =>
            {
                TransOrderHandler instance = TransOrderHandler.Instance;
                ConcurrentDictionary<string, TransOrderServerData> concurrentDictionary = instance.GetAllOrder(false);
                if (concurrentDictionary != null && concurrentDictionary.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(dealTimeEnd))
                    {
                        executionReportList = concurrentDictionary.Values.ToList().Where(_ => _.TransactTime.Contains(dealTimeEnd)).ToList();
                    }
                    else
                    {
                        executionReportList = concurrentDictionary.Values.ToList();
                    }

                }
                else
                {
                    executionReportList = new List<TransOrderServerData>();
                }

                List<TransactionOrdermModel> transactionOrderms = new List<TransactionOrdermModel>();
                //_innerOrders.Clear();
                if (executionReportList != null && executionReportList.Count > 0)
                {
                    executionReportList = executionReportList.OrderByDescending(_ => _.TransactTime).ToList();
                    foreach (var item in executionReportList)
                    {
                        TransactionOrdermModel transactionOrdermModel = new TransactionOrdermModel();
                        ConvertDataModelToViewModel(transactionOrdermModel, item);
                        transactionOrderms.Add(transactionOrdermModel);
                    }

                    //UpdateQueryData(executionReportList);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoadMaskVisible = Visibility.Collapsed;
                    });
                }
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
               {
                   InnerOrders.Clear();
                   InnerOrders = new ObservableCollection<TransactionOrdermModel>(transactionOrderms);
                   _currentDataGrid.DataContext = InnerOrders;
                   //this.Mv.Source = InnerOrders;
                   LoadMaskVisible = Visibility.Collapsed;
               }), DispatcherPriority.Send);
            });

        }



        private async void QueryOrderData(string dealTimeEnd = "")
        {
            List<TransOrderServerData> executionReportList = new List<TransOrderServerData>();
            await Task.Factory.StartNew(() =>
            {
                TransOrderHandler instance = TransOrderHandler.Instance;
                ConcurrentDictionary<string, TransOrderServerData> concurrentDictionary = instance.GetAllOrder(false);
                if (concurrentDictionary != null && concurrentDictionary.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(dealTimeEnd))
                    {
                        executionReportList = concurrentDictionary.Values.ToList().Where(_ => _.TransactTime.Contains(dealTimeEnd)).ToList();
                    }
                    else
                    {
                        executionReportList = concurrentDictionary.Values.ToList();
                    }

                }
                else
                {
                    executionReportList = new List<TransOrderServerData>();
                }

                List<TransactionOrdermModel> transactionOrderms = new List<TransactionOrdermModel>();

                int unreadCount = 0;
                int orderCount = 0;
                if (executionReportList != null && executionReportList.Count > 0)
                {
                    executionReportList = executionReportList.OrderByDescending(_ => _.TransactTime).ToList();
                    foreach (var item in executionReportList)
                    {
                        if (IsUnRead(item))
                        {
                            unreadCount++;
                        }
                        if (IsTodayOrder(item))
                        {
                            orderCount++;
                        }
                        TransactionOrdermModel transactionOrdermModel = new TransactionOrdermModel();
                        ConvertDataModelToViewModel(transactionOrdermModel, item);
                        transactionOrderms.Add(transactionOrdermModel);
                    }

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        InnerOrders.Clear();
                        InnerOrders = new ObservableCollection<TransactionOrdermModel>(transactionOrderms);
                        _transactionPanel.TransactionDataGrid.DataContext = InnerOrders;
                        _transactionPanel.TransactionDataGridAuto.DataContext = InnerOrders;
                        _transactionPanel.TransactionDataGridCard.DataContext = InnerOrders;
                        if (_transactionModel == "3")
                        {
                            //_transactionPanel.TransactionDataGridAuto.DataContext = InnerOrders;
                            ScrollViewer scrollViewer = CommonUtils.FindVisualChild<ScrollViewer>(_transactionPanel.TransactionDataGridAuto);
                            if (scrollViewer != null)
                            {
                                scrollViewer.ScrollToTop();
                            }
                            _transactionPanel.tbc_Data.SelectedIndex = 1;
                        }
                        else if (_transactionModel == "2")
                        {
                            //_currentDataGrid.DataContext = InnerOrders;
                            ScrollViewer scrollViewer = CommonUtils.FindVisualChild<ScrollViewer>(_currentDataGrid);
                            if (scrollViewer != null)
                            {
                                scrollViewer.ScrollToTop();
                            }
                            _transactionPanel.tbc_Data.SelectedIndex = 0;
                        }
                        else
                        {
                            // _transactionPanel.TransactionDataGridCard.DataContext = InnerOrders;
                            ScrollViewer scrollViewer = CommonUtils.FindVisualChild<ScrollViewer>(_transactionPanel.TransactionDataGridCard);
                            if (scrollViewer != null)
                            {
                                scrollViewer.ScrollToTop();
                            }
                            _transactionPanel.tbc_Data.SelectedIndex = 2;
                        }

                        RowCount = orderCount;
                        UnreadTxt = $"忽略{unreadCount}条";
                        LoadMaskVisible = Visibility.Collapsed;
                    }), DispatcherPriority.Normal);
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        InnerOrders.Clear();
                        _transactionPanel.TransactionDataGrid.DataContext = InnerOrders;
                        _transactionPanel.TransactionDataGridAuto.DataContext = InnerOrders;
                        _transactionPanel.TransactionDataGridCard.DataContext = InnerOrders;
                        if (_transactionModel == "3")
                        {
                            _transactionPanel.tbc_Data.SelectedIndex = 1;
                        }
                        else if (_transactionModel == "2")
                        {
                            _transactionPanel.tbc_Data.SelectedIndex = 0;
                        }
                        else
                        {
                            _transactionPanel.tbc_Data.SelectedIndex = 2;
                        }
                        LoadMaskVisible = Visibility.Collapsed;
                    });
                }
            });

        }



        //private void BindingData(List<TransactionOrdermModel> transactionOrderms,DataGrid dataGrid) { 

        //}


        public async Task<GrExecOrdersReplyPo> GetExecOrderData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecOrderQuery(req);
            });

            return reply;
        }

        public async Task<GrExecOrdersReplyPo> GetExecOrderHistData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecOrderHistQuery(req);
            });

            return reply;
        }

        /// <summary>
        /// 查询流水
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<GrExecOrdersReplyPo> GetExecOrderHistRecordData(GrExecOrderQueryReq req)
        {
            GrExecOrdersReplyPo reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecOrderHistRecordQuery(req);
            });

            return reply;
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="er"></param>
        private void UpdateInnerOrder(TransOrderServerData er)
        {
            if (!string.IsNullOrWhiteSpace(_dealDateEnd))
            {
                if (!er.TransactTime.Contains(_dealDateEnd))
                {
                    return;
                }
            }
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                TransactionOrdermModel rf = this.InnerOrders.FirstOrDefault(o => o.ExecID == er.ExecID);
                if (rf != null)
                {
                    if (!TransOrderHandler.Instance.IsCanSeeOrder(er))
                    {
                        this.InnerOrders.Remove(this.InnerOrders.Where(_ => _.ExecID == er.ExecID).FirstOrDefault());
                    }
                    ConvertDataModelToViewModel(rf, er);
                    //QuantityUnreadCount();
                }
                else
                {
                    if (!TransOrderHandler.Instance.IsCanSeeOrder(er))
                    {
                        return;
                    }
                    TransactionOrdermModel transactionOrdermModel = new TransactionOrdermModel();
                    ConvertDataModelToViewModel(transactionOrdermModel, er);

                    //lock (_dataLock)
                    //{
                    if (TransOrderHelper.Instance.IsDealHistDay(er))
                    {
                        int insertIndex = GetTodayReceiptPosition(InnerOrders);
                        if (insertIndex > 0)
                        {
                            InnerOrders.Insert(insertIndex - 1, transactionOrdermModel);
                        }
                        else
                        {
                            InnerOrders.Add(transactionOrdermModel);
                        }
                    }
                    else
                    {
                        InnerOrders.Insert(0, transactionOrdermModel);
                    }
                    //QuantityUnreadCount();
                    //}
                }
            }));
        }


        public int GetTodayReceiptPosition(ObservableCollection<TransactionOrdermModel> innerOrders)
        {
            int result = 0;
            bool need = false;
            foreach (TransactionOrdermModel orderDetail in innerOrders)
            {
                result++;
                if (TransOrderHelper.Instance.IsDealHistDay(orderDetail))
                {
                    need = true;
                    break;
                }
            }

            if (!need)
                result = 0;

            return result;
        }


        private bool IsUnRead(TransOrderServerData er)
        {
            if (!string.IsNullOrEmpty(er.BrokerUnread))
            {
                if (er.BrokerUnread.ContainsUserId(LocalEnv.UserInfo.UserId))
                {
                    //我是最后操作人
                    if (LocalEnv.UserInfo.IsMyself(er.ParamCloseById))
                    {
                        //我同时是brokerA和brokerB
                        if (LocalEnv.UserInfo.IsMyself(er.PartySubID) && LocalEnv.UserInfo.IsMyself(er.CounterPartySubID))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        private bool IsTodayOrder(TransOrderServerData er)
        {
            if (((!string.IsNullOrWhiteSpace(er.PartySubID) && er.PartySubID == LocalEnv.UserInfo.UserId)
                || (!string.IsNullOrWhiteSpace(er.CounterPartySubID) && er.CounterPartySubID == LocalEnv.UserInfo.UserId))
                && er.OrdStatus == "G"
                && !TransOrderHelper.Instance.IsDealHistDay(er))
            {
                return true;
            }
            return false;
        }

        private void QuantityUnreadCount()
        {
            List<TransOrderServerData> executionReportList = new List<TransOrderServerData>();
            TransOrderHandler instance = TransOrderHandler.Instance;
            ConcurrentDictionary<string, TransOrderServerData> concurrentDictionary = instance.GetAllOrder(false);
            if (concurrentDictionary != null && concurrentDictionary.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(_dealDateEnd))
                {
                    executionReportList = concurrentDictionary.Values.ToList().Where(_ => _.TransactTime.Contains(_dealDateEnd)).ToList();
                }
                else
                {
                    executionReportList = concurrentDictionary.Values.ToList();
                }

            }
            else
            {
                executionReportList = new List<TransOrderServerData>();
            }

            List<TransactionOrdermModel> transactionOrderms = new List<TransactionOrdermModel>();

            int unreadCount = 0;
            int orderCount = 0;
            if (executionReportList != null && executionReportList.Count > 0)
            {
                foreach (var item in executionReportList)
                {
                    if (IsUnRead(item))
                    {
                        unreadCount++;
                    }
                    if (IsTodayOrder(item))
                    {
                        orderCount++;
                    }
                }

                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    RowCount = orderCount;
                    UnreadTxt = $"忽略{unreadCount}条";
                    //Thread.Sleep(100);
                }), DispatcherPriority.Normal);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RowCount = orderCount;
                    UnreadTxt = $"忽略{unreadCount}条";
                    //Thread.Sleep(100);
                });
            }
        }
        /// <summary>
        /// 数据模型转换
        /// </summary>
        /// <param name="transactionOrdermModel"></param>
        /// <param name="er"></param>
        private void ConvertDataModelToViewModel(TransactionOrdermModel transactionOrdermModel, TransOrderServerData er)
        {

            transactionOrdermModel.Blocknum = er.BlockNum;
            transactionOrdermModel.OrderID = er.OrderID;
            transactionOrdermModel.ExecID = er.ExecID;
            transactionOrdermModel.SecurityID = er.SecurityID;
            transactionOrdermModel.Symbol = er.Symbol;
            transactionOrdermModel.QuoteId = er.QuantID;
            transactionOrdermModel.QtyDisplay = er.LastQty;
            transactionOrdermModel.Qty = CommonUtils.ConvertToDecimal(er.LastQty == null ? string.Empty : er.LastQty.ToString());
            transactionOrdermModel.QtyChange = transactionOrdermModel.Qty.Value / 1000;
            transactionOrdermModel.Seq = er.Sequence;
            transactionOrdermModel.OrderStatus = er.OrdStatus;
            transactionOrdermModel.BrokerIdB = er.CounterPartySubID;
            transactionOrdermModel.BrokerIdA = er.PartySubID;
            transactionOrdermModel.BrokerNameA = er.PartySubName;
            transactionOrdermModel.BrokerNameB = er.CounterPartySubName;
            transactionOrdermModel.IsInternalQuote = !string.IsNullOrWhiteSpace(er.HandlInst) && er.HandlInst == "1" || !string.IsNullOrWhiteSpace(er.InternalFlag) && er.InternalFlag == "1";
            transactionOrdermModel.IssueTraderName = er.IssueInstName;
            transactionOrdermModel.ChannelComboxModel = EnumHelper.Instance.ChannelComboxModel;
            transactionOrdermModel.OnlineA = er.OnlineA;
            transactionOrdermModel.OnlineB = er.OnlineB;
            transactionOrdermModel.BidSendtoDisplay = er.SendTo;
            transactionOrdermModel.OfrSendtoDisplay = er.SendTo2;
            transactionOrdermModel.BidSendto = er.SendTo;
            transactionOrdermModel.OfrSendto = er.SendTo2;
            transactionOrdermModel.BridgeCloseById = er.ParamCloseById;

            transactionOrdermModel.BrokerAFree = er.BrokerageA;
            transactionOrdermModel.BrokerBFree = er.BrokerageB;
            transactionOrdermModel.NeeSendMsgForB = er.NeeSendMsgForB;





            transactionOrdermModel.CreateTime = er.TransactTime;
            DateTime dt;
            if (DateTime.TryParseExact(transactionOrdermModel.CreateTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                transactionOrdermModel.CreateTimeOrderBy = dt;
                if (TransOrderHelper.Instance.IsDealHistDay(transactionOrdermModel))
                {
                    transactionOrdermModel.CreateTimeDisplay = dt.ToString("MM-dd HH:mm:ss");
                    transactionOrdermModel.IsDealHistDay = true;
                }
                else
                {
                    transactionOrdermModel.CreateTimeDisplay = dt.ToString("HH:mm:ss");
                    transactionOrdermModel.IsDealHistDay = false;
                }

            }
            else
            {
                transactionOrdermModel.CreateTimeDisplay = transactionOrdermModel.CreateTime;
            }

            decimal d = 0;
            if (decimal.TryParse(er.FanDian, out d))
            {
                transactionOrdermModel.FandianDisplay = !string.IsNullOrEmpty(er.FanDian) ? "F" + er.FanDian : string.Empty;
                transactionOrdermModel.FandianFlag = "1";
            }
            else
            {
                transactionOrdermModel.FandianDisplay = er.FanDian;
                transactionOrdermModel.FandianFlag = "2";
            }
            transactionOrdermModel.Fandian = er.FanDian;
            transactionOrdermModel.FandianTxt = er.FanDian;

            transactionOrdermModel.Remarks = er.Remark;

            //结算
            transactionOrdermModel.Dealdate = er.TradeDate;
            transactionOrdermModel.Dealdate2 = er.TradeDate2;
            transactionOrdermModel.SettlementType = er.SettlType;
            transactionOrdermModel.Settlementtype2 = er.SettlType2;
            transactionOrdermModel.Settlementdate = er.SettlDate;
            transactionOrdermModel.Settlementdate2 = er.SettlDate2;

            if (TransOrderHelper.Instance.IsDealHistDay(transactionOrdermModel))
                transactionOrdermModel.SettleAllDisplay = TransOrderHelper.Instance.GetHistoryDisplaySettle(transactionOrdermModel);
            else
                transactionOrdermModel.SettleAllDisplay = TransOrderHelper.Instance.GetDisplaySettle(transactionOrdermModel);

            transactionOrdermModel.BondCodeShow = TransOrderHelper.Instance.GetBondCodeShow(er);


            // priceType    0-意向;1-净价;2- 全价;3或空-收益率;4-利差;8-经纪商未指定',
            // 改为由服务端处理
            transactionOrdermModel.Price_Display = er.Price;
            // 方向
            if (!string.IsNullOrWhiteSpace(er.OnlineA) && !string.IsNullOrWhiteSpace(er.OnlineB))
            {

                if (er.OnlineA.Equals("3") && er.OnlineB.Equals("3"))
                {
                    if (er.ForBidRefreshFlag == "3" && GRSettingsHandler.Instance.GetSystemParameter().showSourceDealSide)
                    {
                        transactionOrdermModel.SideDisplay = "TRD";
                    }
                    else
                    {
                        transactionOrdermModel.SideDisplay = er.Side == "2" ? "出给" : "买入";
                    }
                }
                else
                {
                    if (er.ForBidRefreshFlag == "3" && GRSettingsHandler.Instance.GetSystemParameter().showSourceDealSide)
                    {
                        transactionOrdermModel.SideDisplay = "TRD";
                    }
                    else
                    {
                        transactionOrdermModel.SideDisplay = er.Side == "2" ? "GVN" : "TKN";
                    }
                }
            }
            else
            {
                transactionOrdermModel.SideDisplay = er.Side == "2" ? "GVN" : "TKN";
            }

            //交易员A

            transactionOrdermModel.CounterPartyInstitutionId = er.CounterPartyInstitutionId;
            transactionOrdermModel.CounterPartyInstitutionName = er.CounterPartyInstitutionName;
            transactionOrdermModel.CounterPartyInstitutionShowName = er.CounterPartyInstitutionShowName;
            transactionOrdermModel.InstitutionID = er.InstitutionID;
            transactionOrdermModel.InstitutionName = er.InstitutionName;
            transactionOrdermModel.InstitutionShowName = er.InstitutionShowName;
            transactionOrdermModel.TraderID = er.TraderID;
            transactionOrdermModel.TraderIDB = er.CounterPartyTraderId;
            transactionOrdermModel.TraderAInfoDisplay = GetTraderAInfo(er);
            transactionOrdermModel.TraderBInfoDisplay = GetTraderBInfo(er);
            transactionOrdermModel.SecurityType = er.SecurityType;
            transactionOrdermModel.TimeToMaturity = er.TimeToMaturity;

            BondsInfoPo bondsInfoPo = Utils.CommonUtils.GetBondsInfoByCode(er.SecurityID);
            if (bondsInfoPo != null)
            {
                transactionOrdermModel.Symbol = bondsInfoPo.Short_Name;
                transactionOrdermModel.MaturtityDate = bondsInfoPo.Maturity_Date.ToString();
                transactionOrdermModel.SecurityType = bondsInfoPo.SECURITY_TYPE;
                transactionOrdermModel.BondSubType = bondsInfoPo.Bond_Subtype;
                //findData.BondCategory = CommonUtils.GetBondCategory(bondsInfoPo.Bond_Subtype, bondsInfoPo.Ent_Cor);
                transactionOrdermModel.RemaingingTime_convert = Utils.CommonUtils.ConvertRemainingTime(bondsInfoPo.RemaingingTime);
                transactionOrdermModel.IssuerRatingCurrentNPY = bondsInfoPo.Issuer_Rating_Current_NPY;
            }

            transactionOrdermModel.Side = er.Side;
            //findData.FirmFlag = "0";
            transactionOrdermModel.SettleType = er.SettlType;
            transactionOrdermModel.RoleId = er.RoleID;
            transactionOrdermModel.RoleName = er.RoleName;
            transactionOrdermModel.Price = CommonUtils.ConvertToDecimal(er.Price == null ? string.Empty : er.Price.ToString());
            transactionOrdermModel.NetPrice = CommonUtils.ConvertToDecimal(er.NetPrice == null ? string.Empty : er.NetPrice.ToString());
            //findData.CopyInfo = GetCopyInfo(findData.Qty.ToString(), er);
            transactionOrdermModel.PartySubId = er.PartySubID;
            //findData.PartySubName = er.closeBy;
            transactionOrdermModel.UpdateTime = er.UpdateTime;
            transactionOrdermModel.WholeFlag = er.WholeFlag;
            transactionOrdermModel.IsMe = er.CloseBy == LocalEnv.UserInfo.UserName;
            transactionOrdermModel.IntentionID = er.InstitutionID;
            //findData.FlashCount = 0;
            //Trader
            transactionOrdermModel.PartySubName_InnerBid = transactionOrdermModel.PartySubName_InnerOfr = er.Name;
            transactionOrdermModel.IsMe_InnerBid = transactionOrdermModel.IsMe_InnerOfr = er.Name == LocalEnv.UserInfo.FullName;
            transactionOrdermModel.PartySubName = er.PartySubName;
            transactionOrdermModel.Info1 = er.Info1;

            transactionOrdermModel.Trader = er.TraderID;
            //客户机构  500301仅查看销交客户标签  500302 查看销交客户机构   500303 查看销交客户机构及客户名

            //由于接口返回了多个(vip)标识，现保留一个
            if (!string.IsNullOrEmpty(transactionOrdermModel.InstitutionName))
            {
                string pattern = @"(\(vip\))+";
                transactionOrdermModel.InstitutionName = Regex.Replace(transactionOrdermModel.InstitutionName, pattern, "(vip)");
            }

            transactionOrdermModel.FirmFlag = string.IsNullOrEmpty(er.FirmFlag) ? "0" : er.FirmFlag;

            transactionOrdermModel.BrokerBConfirmQty = er.MatchQTY;
            transactionOrdermModel.MatchQTY = er.OrderQty;
            transactionOrdermModel.BrokerUnread = er.BrokerUnread;

            //broker状态显示
            switch (er.OrdStatus)
            {
                case "0":
                case "1":
                case "2":
                    transactionOrdermModel.BrokerAStatus = "1";
                    if (LocalEnv.UserInfo.IsMyself(er.CounterPartySubID))
                    {
                        transactionOrdermModel.BrokerBStatus = "0";
                    }
                    else
                    {
                        transactionOrdermModel.BrokerBStatus = "1";
                    }
                    break;
                case "I":
                    //if (!string.IsNullOrEmpty(transactionOrdermModel.BrokerUnread) && TransOrderHelper.Instance.ContainsUserId(transactionOrdermModel.BrokerUnread, er.PartySubID))
                    //{
                    //    transactionOrdermModel.BrokerAStatus = "1";
                    //}
                    //else
                    //{
                    //    transactionOrdermModel.BrokerAStatus = "4";
                    //}
                    transactionOrdermModel.BrokerAStatus = "4";
                    //if (!string.IsNullOrEmpty(transactionOrdermModel.BrokerUnread) && TransOrderHelper.Instance.ContainsUserId(transactionOrdermModel.BrokerUnread, er.CounterPartySubID))
                    //{
                    //    transactionOrdermModel.BrokerBStatus = "1";
                    //}
                    //else
                    //{
                    //    transactionOrdermModel.BrokerBStatus = "0";
                    //}
                    if (!LocalEnv.UserInfo.IsMyself(er.CounterPartySubID))
                    {
                        transactionOrdermModel.BrokerBStatus = "1";
                    }
                    else
                    {
                        transactionOrdermModel.BrokerBStatus = "0";
                    }
                    break;
                case "E":
                    if (LocalEnv.UserInfo.IsMyself(er.PartySubID))
                    {
                        transactionOrdermModel.BrokerAStatus = "0";
                    }
                    else
                    {
                        transactionOrdermModel.BrokerAStatus = "1";
                    }
                    if (transactionOrdermModel.IsPartlyConfirm)
                    {
                        transactionOrdermModel.BrokerBStatus = "2";
                    }
                    else
                    {
                        transactionOrdermModel.BrokerBStatus = "4";
                    }
                    break;
                case "H":
                    transactionOrdermModel.BrokerAStatus = "3";
                    if (transactionOrdermModel.IsPartlyConfirm)
                    {
                        transactionOrdermModel.BrokerBStatus = "2";
                    }
                    else
                    {
                        transactionOrdermModel.BrokerBStatus = "4";
                    }
                    break;
                case "F":
                    transactionOrdermModel.BrokerAStatus = "4";
                    transactionOrdermModel.BrokerBStatus = "3";
                    break;
                case "G":
                    transactionOrdermModel.BrokerAStatus = "4";
                    if (transactionOrdermModel.IsPartlyConfirm)
                    {
                        transactionOrdermModel.BrokerBStatus = "2";
                    }
                    else
                    {
                        transactionOrdermModel.BrokerBStatus = "4";
                    }
                    break;
            }

            transactionOrdermModel.IsPartConfirm = !string.IsNullOrWhiteSpace(transactionOrdermModel.BrokerBConfirmQty)
                && !string.IsNullOrWhiteSpace(transactionOrdermModel.MatchQTY)
                && transactionOrdermModel.BrokerBConfirmQty != transactionOrdermModel.MatchQTY
                && transactionOrdermModel.OrderStatus == "G";

            if (CheckBridgeStatus(transactionOrdermModel))
            {
                transactionOrdermModel.BridgeEnable = true;
            }
            else
            {
                transactionOrdermModel.BridgeEnable = false;
            }
            //桥sss
            transactionOrdermModel.BridgeFlag = er.BridgeFlag;
            transactionOrdermModel.BridgeType = er.BridgeType;
            transactionOrdermModel.DoubleBridgeSettleSide = er.DoubleBridgeSettleSide;
            //transactionOrdermModel. = er.OperateBorderID;

            if (transactionOrdermModel.BridgeFlag == "2")
            {
                transactionOrdermModel.BridgeSendVisible = Visibility.Visible;
                transactionOrdermModel.SendStandVisible = Visibility.Collapsed;
                transactionOrdermModel.StandSendtoValVisible = Visibility.Collapsed;
                transactionOrdermModel.CustomBtnVisibility = Visibility.Collapsed;

                if (er.IsGVN)
                {

                    if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrdermModel.BrokerIdA))
                    {
                        transactionOrdermModel.OfrSendtoShowVisibility = Visibility.Visible;
                        transactionOrdermModel.OfrSendtoEditVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        transactionOrdermModel.OfrSendtoShowVisibility = Visibility.Collapsed;
                        transactionOrdermModel.OfrSendtoEditVisibility = Visibility.Visible;
                    }
                    if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrdermModel.BrokerIdB))
                    {
                        transactionOrdermModel.BidSendtoShowVisibility = Visibility.Visible;
                        transactionOrdermModel.BidSendtoEditVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        transactionOrdermModel.BidSendtoShowVisibility = Visibility.Collapsed;
                        transactionOrdermModel.BidSendtoEditVisibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrdermModel.BrokerIdB))
                    {
                        transactionOrdermModel.OfrSendtoShowVisibility = Visibility.Visible;
                        transactionOrdermModel.OfrSendtoEditVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        transactionOrdermModel.OfrSendtoShowVisibility = Visibility.Collapsed;
                        transactionOrdermModel.OfrSendtoEditVisibility = Visibility.Visible;
                    }
                    if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrdermModel.BrokerIdA))
                    {
                        transactionOrdermModel.BidSendtoShowVisibility = Visibility.Visible;
                        transactionOrdermModel.BidSendtoEditVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        transactionOrdermModel.BidSendtoShowVisibility = Visibility.Collapsed;
                        transactionOrdermModel.BidSendtoEditVisibility = Visibility.Visible;
                    }
                }

            }
            else
            {
                transactionOrdermModel.BridgeSendVisible = Visibility.Collapsed;
                transactionOrdermModel.SendStandVisible = Visibility.Visible;
                transactionOrdermModel.StandSendtoValVisible = Visibility.Visible;
                transactionOrdermModel.CustomBtnVisibility = Visibility.Visible;
            }

            transactionOrdermModel.IsRefreshOrderShow = LocalEnv.ConfInfo.IsShowReserveButton ? Visibility.Visible : Visibility.Collapsed;
            transactionOrdermModel.IsRefreshOrderEnable = IsShowRefreshOrderButton(transactionOrdermModel, er);
            //设置查看按钮状态
            SetLookButton(transactionOrdermModel);

            //代付费是否启用
            //先判断权限
            if (!CheckEnable(er))
            {
                transactionOrdermModel.IsPayFeeEnable = false;
                transactionOrdermModel.CustomBtnIsEnable = false;
            }
            else
            {
                if ((!string.IsNullOrEmpty(er.BridgeFlag) && er.BridgeFlag.Equals("1"))
                       || string.IsNullOrEmpty(er.BridgeFlag))
                {
                    if (!string.IsNullOrWhiteSpace(er.TraderID)
              && !string.IsNullOrWhiteSpace(er.CounterPartyTraderId))
                    {
                        transactionOrdermModel.IsPayFeeEnable = true;
                        transactionOrdermModel.CustomBtnIsEnable = true;
                    }
                    else
                    {
                        transactionOrdermModel.IsPayFeeEnable = false;
                        transactionOrdermModel.CustomBtnIsEnable = false;
                    }
                }
                else
                {
                    transactionOrdermModel.IsPayFeeEnable = false;
                }
            }

            //发给标准化
            if (!CheckEnable(er))
            {
                transactionOrdermModel.SendBtnIsEnable = false;
            }
            else
            {
                transactionOrdermModel.SendBtnIsEnable = true;
            }


            //标准化是否开启
            if (!string.IsNullOrWhiteSpace(er.IssueState)
               && er.IssueState == "1")
            {
                transactionOrdermModel.CustomBtnVisibility = Visibility.Collapsed;
                //transactionOrdermModel.StandSendtoValVisible = Visibility.Visible;
                transactionOrdermModel.OrderDirVisibility = Visibility.Visible;
                transactionOrdermModel.PayInstChangeVisible = Visibility.Visible;
                transactionOrdermModel.PayInstTextVisible = Visibility.Visible;
                transactionOrdermModel.ChannelVisible = Visibility.Visible;
            }
            else
            {
                if (transactionOrdermModel.BridgeFlag == "2")
                {
                    transactionOrdermModel.CustomBtnVisibility = Visibility.Collapsed;
                }
                else
                {
                    transactionOrdermModel.CustomBtnVisibility = Visibility.Visible;
                }
                //transactionOrdermModel.StandSendtoValVisible = Visibility.Visible;
                transactionOrdermModel.OrderDirVisibility = Visibility.Collapsed;
                transactionOrdermModel.PayInstChangeVisible = Visibility.Collapsed;
                transactionOrdermModel.PayInstTextVisible = Visibility.Collapsed;
                transactionOrdermModel.ChannelVisible = Visibility.Collapsed;
            }

            //发给标准化
            transactionOrdermModel.IssueDir = er.IssueDir;
            transactionOrdermModel.IssueChannel = er.IssueChannel;
            transactionOrdermModel.IssueInstId = er.IssueInstId;
            transactionOrdermModel.IssueInstName = er.IssueInstName;
            transactionOrdermModel.IssueInstNameShort = er.IssueInstNameShort;
            transactionOrdermModel.IssueTraderId = er.IssueTraderId;
            transactionOrdermModel.IssueTraderName = er.IssueTraderName;
            transactionOrdermModel.IssueRemark = er.IssueRemark;
            transactionOrdermModel.IssueEditRemark = er.IssueRemark;
            transactionOrdermModel.IssueDesc = er.IssueDesc;
            transactionOrdermModel.IssueProxyInstId = er.ProxyInstId;
            transactionOrdermModel.IssueProxyFee = er.ProxyFee;
            transactionOrdermModel.IssueProxyFlag = er.ProxyFlag;
            transactionOrdermModel.IssueState = er.IssueState;
            transactionOrdermModel.IssueShowType = er.IssueShowType;

            transactionOrdermModel.AssociatedFlag = er.AssociatedFlag;


            SetIssueChannel(transactionOrdermModel);
            SetIssueShowName(transactionOrdermModel);
            SetSendDirFlag(transactionOrdermModel, er);
            SetIssueControl(transactionOrdermModel);

            if (CheckOrderStatus(transactionOrdermModel))
            {
                transactionOrdermModel.BrokerFeeEnable = true;
                transactionOrdermModel.CanUpdateOrder = true;
                //transactionOrdermModel.RemarkEditVisibility = Visibility.Visible;


                transactionOrdermModel.BondShowVisible = Visibility.Visible;
                transactionOrdermModel.BondShowNoEditVisible = Visibility.Collapsed;

                transactionOrdermModel.PriceShowVisible = Visibility.Visible;
                transactionOrdermModel.PriceShowNoEditVisible = Visibility.Collapsed;

                transactionOrdermModel.QtyShowVisible = Visibility.Visible;
                transactionOrdermModel.QtyShowNoEditVisible = Visibility.Collapsed;
                transactionOrdermModel.TraderAShowVisible = Visibility.Visible;
                transactionOrdermModel.TraderAShowNoEditVisible = Visibility.Collapsed;
                transactionOrdermModel.TraderBShowVisible = Visibility.Visible;
                transactionOrdermModel.TraderBShowNoEditVisible = Visibility.Collapsed;
                transactionOrdermModel.BrokerAShowVisible = Visibility.Visible;
                transactionOrdermModel.BrokerAShowNoEditVisible = Visibility.Collapsed;
                transactionOrdermModel.BrokerBShowVisible = Visibility.Visible;
                transactionOrdermModel.BrokerBShowNoEditVisible = Visibility.Collapsed;

            }
            else
            {
                transactionOrdermModel.BrokerFeeEnable = false;
                transactionOrdermModel.CanUpdateOrder = false;
                //transactionOrdermModel.RemarkEditVisibility = Visibility.Collapsed;

                transactionOrdermModel.BondShowVisible = Visibility.Collapsed;
                transactionOrdermModel.BondShowNoEditVisible = Visibility.Visible;

                transactionOrdermModel.PriceShowVisible = Visibility.Collapsed;
                transactionOrdermModel.PriceShowNoEditVisible = Visibility.Visible;

                transactionOrdermModel.QtyShowVisible = Visibility.Collapsed;
                transactionOrdermModel.QtyShowNoEditVisible = Visibility.Visible;
                transactionOrdermModel.TraderAShowVisible = Visibility.Collapsed;
                transactionOrdermModel.TraderAShowNoEditVisible = Visibility.Visible;
                transactionOrdermModel.TraderBShowVisible = Visibility.Collapsed;
                transactionOrdermModel.TraderBShowNoEditVisible = Visibility.Visible;
                transactionOrdermModel.BrokerAShowVisible = Visibility.Collapsed;
                transactionOrdermModel.BrokerAShowNoEditVisible = Visibility.Visible;
                transactionOrdermModel.BrokerBShowVisible = Visibility.Collapsed;
                transactionOrdermModel.BrokerBShowNoEditVisible = Visibility.Visible;
            }

            if (CheckOrderRemarkEdit(transactionOrdermModel))
            {
                transactionOrdermModel.CanUpdateRemark = true;
                if (transactionOrdermModel.FandianFlag == "1")
                {
                    transactionOrdermModel.FandianTag = "true";
                }
                else
                {
                    transactionOrdermModel.FandianTag = "false";
                }

                transactionOrdermModel.RemarkDisplayVisibility = Visibility.Visible;
                transactionOrdermModel.RemarkDisplayNoEditVisibility = Visibility.Collapsed;
            }
            else
            {
                transactionOrdermModel.FandianTag = "false";
                transactionOrdermModel.CanUpdateRemark = false;
                transactionOrdermModel.RemarkDisplayVisibility = Visibility.Collapsed;
                transactionOrdermModel.RemarkDisplayNoEditVisibility = Visibility.Visible;
            }

            if (transactionOrdermModel.FandianTag == "true")
            {
                transactionOrdermModel.FandianShowVisible = Visibility.Visible;
                transactionOrdermModel.FandianShowReadVisible = Visibility.Collapsed;
            }
            else
            {
                transactionOrdermModel.FandianShowVisible = Visibility.Collapsed;
                transactionOrdermModel.FandianShowReadVisible = Visibility.Visible;
            }


            if (!string.IsNullOrWhiteSpace(er.IssueRemark))
            {
                transactionOrdermModel.RemarkContentVisibility = Visibility.Visible;
                transactionOrdermModel.RemarkWaterVisibility = Visibility.Collapsed;
            }
            else
            {
                transactionOrdermModel.RemarkContentVisibility = Visibility.Collapsed;
                transactionOrdermModel.RemarkWaterVisibility = Visibility.Visible;
            }

        }

        /// <summary>
        /// 设置默认渠道
        /// </summary>
        private void SetIssueChannel(TransactionOrdermModel operationOrderDetail)
        {
            if (!string.IsNullOrWhiteSpace(operationOrderDetail.IssueChannel)
                && operationOrderDetail.ChannelComboxModel != null
                && operationOrderDetail.ChannelComboxModel.Count > 0)
            {
                foreach (ComboxBaseModel item in operationOrderDetail.ChannelComboxModel)
                {
                    if (item.ValueMemberPath == operationOrderDetail.IssueChannel)
                    {
                        operationOrderDetail.ComboxBaseSelect = item;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///发给展示
        /// </summary>
        /// <param name="operationOrderDetail"></param>
        private void SetIssueShowName(TransactionOrdermModel operationOrderDetail)
        {
            string issueInstName = operationOrderDetail.IssueInstNameShort;
            if (string.IsNullOrWhiteSpace(issueInstName))
            {
                issueInstName = operationOrderDetail.IssueInstName;
            }

            if (!string.IsNullOrWhiteSpace(operationOrderDetail.SecurityID))
            {
                if (operationOrderDetail.SecurityID.EndsWith(".IB") && !string.IsNullOrWhiteSpace(issueInstName))
                {
                    switch (operationOrderDetail.IssueChannelEnum)
                    {
                        case ChannelEnum.Request:
                            operationOrderDetail.IssueTraderShowNameDisplay = operationOrderDetail.IssueDesc + issueInstName;
                            break;
                        case ChannelEnum.FixedIncome:
                        case ChannelEnum.Bulk:
                        case ChannelEnum.Bidding:
                        case ChannelEnum.Dialogue:
                        case ChannelEnum.Bilateral:
                        case ChannelEnum.xbond:
                        case ChannelEnum.iDeal:
                        case ChannelEnum.Xbargain:
                            if (!string.IsNullOrWhiteSpace(operationOrderDetail.IssueTraderName))
                            {
                                operationOrderDetail.IssueTraderShowNameDisplay = operationOrderDetail.IssueDesc + issueInstName + "(" + operationOrderDetail.IssueTraderName + ")";
                            }
                            else
                            {
                                operationOrderDetail.IssueTraderShowNameDisplay = operationOrderDetail.IssueDesc + issueInstName;
                            }
                            break;
                        case ChannelEnum.Intention:
                        case ChannelEnum.Anonymity:
                            if (!string.IsNullOrWhiteSpace(operationOrderDetail.IssueTraderName))
                            {
                                operationOrderDetail.IssueTraderShowNameDisplay = issueInstName + "(" + operationOrderDetail.IssueTraderName + ")";
                            }
                            else
                            {
                                operationOrderDetail.IssueTraderShowNameDisplay = issueInstName;
                            }
                            break;
                    }
                }
                else
                {
                    operationOrderDetail.IssueTraderShowNameDisplay = issueInstName;
                }
            }
        }

        /// <summary>
        /// 设置发给方向标识
        /// </summary>
        private void SetSendDirFlag(TransactionOrdermModel operationOrderDetail, ExecutionReport er)
        {

            if (operationOrderDetail.IsGVN)
            {
                if (operationOrderDetail.IssueDir == "2")
                {
                    operationOrderDetail.SendRightVisible = Visibility.Collapsed;
                    operationOrderDetail.SendLeftVisible = Visibility.Visible;
                }
                else if (operationOrderDetail.IssueDir == "1")
                {
                    operationOrderDetail.SendRightVisible = Visibility.Visible;
                    operationOrderDetail.SendLeftVisible = Visibility.Collapsed;
                }
                else
                {
                    operationOrderDetail.SendRightVisible = Visibility.Collapsed;
                    operationOrderDetail.SendLeftVisible = Visibility.Collapsed;
                }
            }
            else
            {
                if (operationOrderDetail.IssueDir == "2")
                {
                    operationOrderDetail.SendRightVisible = Visibility.Visible;
                    operationOrderDetail.SendLeftVisible = Visibility.Collapsed;
                }
                else if (operationOrderDetail.IssueDir == "1")
                {
                    operationOrderDetail.SendRightVisible = Visibility.Collapsed;
                    operationOrderDetail.SendLeftVisible = Visibility.Visible;
                }
                else
                {
                    operationOrderDetail.SendRightVisible = Visibility.Collapsed;
                    operationOrderDetail.SendLeftVisible = Visibility.Collapsed;
                }
            }

            //设置标识
            if (!string.IsNullOrWhiteSpace(operationOrderDetail.BrokerIdA)
                && (LocalEnv.UserInfo.UserId == operationOrderDetail.BrokerIdA
                || SystemPowerHandler.Instance.IsCanEditOrder(operationOrderDetail.BrokerIdA)))
            {
                operationOrderDetail.BrokerASelfFlag = Visibility.Visible;
            }
            else
            {
                operationOrderDetail.BrokerASelfFlag = Visibility.Hidden;
            }

            if (!string.IsNullOrWhiteSpace(operationOrderDetail.BrokerIdB)
                && (LocalEnv.UserInfo.UserId == operationOrderDetail.BrokerIdB
                || SystemPowerHandler.Instance.IsCanEditOrder(operationOrderDetail.BrokerIdB)))
            {
                operationOrderDetail.BrokerBSelfFlag = Visibility.Visible;
            }
            else
            {
                operationOrderDetail.BrokerBSelfFlag = Visibility.Hidden;
            }

            //代付费是否启用
            //先判断权限
            if (!CheckEnable(er))
            {
                operationOrderDetail.IsPayFeeEnable = false;
                operationOrderDetail.CustomBtnIsEnable = false;
            }
            else
            {
                if ((!string.IsNullOrEmpty(operationOrderDetail.BridgeFlag) && operationOrderDetail.BridgeFlag.Equals("1"))
                       || string.IsNullOrEmpty(operationOrderDetail.BridgeFlag))
                {
                    if (!string.IsNullOrWhiteSpace(operationOrderDetail.TraderID)
              && !string.IsNullOrWhiteSpace(operationOrderDetail.TraderIDB))
                    {
                        operationOrderDetail.IsPayFeeEnable = true;
                        operationOrderDetail.CustomBtnIsEnable = true;
                    }
                    else
                    {
                        operationOrderDetail.IsPayFeeEnable = false;
                        operationOrderDetail.CustomBtnIsEnable = false;
                    }
                }
                else
                {
                    operationOrderDetail.IsPayFeeEnable = false;
                }
            }
        }

        /// <summary>
        /// 设置控件是否展示
        /// </summary>
        private void SetIssueControl(TransactionOrdermModel operationOrderDetail)
        {
            if (!string.IsNullOrWhiteSpace(operationOrderDetail.IssueShowType)
               && operationOrderDetail.IssueShowType == "1")
            {
                if (!string.IsNullOrWhiteSpace(operationOrderDetail.IssueState)
                   && operationOrderDetail.IssueState == "1")
                {
                    operationOrderDetail.PayInstTextVisible = Visibility.Visible;
                    operationOrderDetail.PayInstChangeVisible = Visibility.Collapsed;
                    operationOrderDetail.OrderDirVisibility = Visibility.Visible;
                    operationOrderDetail.ChannelVisible = Visibility.Visible;
                    return;
                }
                else
                {
                    operationOrderDetail.PayInstTextVisible = Visibility.Collapsed;
                    operationOrderDetail.PayInstChangeVisible = Visibility.Collapsed;
                    operationOrderDetail.OrderDirVisibility = Visibility.Collapsed;
                    operationOrderDetail.ChannelVisible = Visibility.Collapsed;
                    return;
                }
            }
            operationOrderDetail.PayInstTextVisible = Visibility.Collapsed;
            operationOrderDetail.PayInstChangeVisible = Visibility.Collapsed;
            operationOrderDetail.OrderDirVisibility = Visibility.Collapsed;
            if (operationOrderDetail.SecurityID.EndsWith(".SZ") || operationOrderDetail.SecurityID.EndsWith(".SH"))
            {
                if (string.IsNullOrWhiteSpace(operationOrderDetail.TraderID) || string.IsNullOrWhiteSpace(operationOrderDetail.TraderIDB)
                    || operationOrderDetail.CustomBtnVisibility == Visibility.Visible)
                {
                    operationOrderDetail.ChannelVisible = Visibility.Collapsed;
                    return;
                }
            }
            operationOrderDetail.ChannelVisible = Visibility.Visible;
        }

        /// <summary>
        /// 是否显示推市场成交，v2.14.0
        /// </summary>
        /// <param name="orderDetailPo"></param>
        /// <returns></returns>
        private static bool IsShowRefreshOrderButton(TransactionOrdermModel transactionOrdermModel, ExecutionReport orderDetailPo)
        {
            bool isShow = false;
            if (LocalEnv.ConfInfo.IsShowReserveButton)
            {
                if ((SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.PartySubID)
                    || SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.CounterPartySubID))
                    && orderDetailPo.ForBidRefreshFlag == "0"
                    && !string.IsNullOrWhiteSpace(orderDetailPo.Price)
                    && (!string.IsNullOrWhiteSpace(orderDetailPo.Price) && Convert.ToDecimal(orderDetailPo.Price) - 0 != 0)
                    && !TransOrderHelper.Instance.IsDealHistDay(transactionOrdermModel)
                    && orderDetailPo.OrdStatus == "G")
                {
                    isShow = true;
                }
            }
            return isShow;

        }

        /// <summary>
        /// 获取交易员展示
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        private string GetTraderAInfo(ExecutionReport orderDetail)
        {
            if (string.IsNullOrWhiteSpace(orderDetail.PartyCode) && string.IsNullOrWhiteSpace(orderDetail.InstitutionName) && string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                return "";

            if (!string.IsNullOrWhiteSpace(orderDetail.TraderName) && (!string.IsNullOrWhiteSpace(orderDetail.InstitutionName) || !string.IsNullOrEmpty(orderDetail.InstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                    return string.Format("{0}({1})", orderDetail.InstitutionShowName, orderDetail.TraderName.Trim());
                else
                    return string.Format("{0}({1})", orderDetail.InstitutionName, orderDetail.TraderName.Trim());
            }

            if (string.IsNullOrWhiteSpace(orderDetail.TraderName) && (!string.IsNullOrWhiteSpace(orderDetail.InstitutionName) || !string.IsNullOrEmpty(orderDetail.InstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                    return orderDetail.InstitutionShowName;
                else
                    return orderDetail.InstitutionName;
            }
            if (!string.IsNullOrWhiteSpace(orderDetail.TraderName) && string.IsNullOrWhiteSpace(orderDetail.InstitutionName) && string.IsNullOrWhiteSpace(orderDetail.InstitutionShowName))
                return orderDetail.TraderName;

            return "";
        }

        /// <summary>
        /// 获取交易员展示
        /// </summary>
        /// <param name="orderDetail"></param>
        /// <returns></returns>
        private string GetTraderBInfo(ExecutionReport orderDetail)
        {
            if (string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                return "";

            if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) || !string.IsNullOrEmpty(orderDetail.CounterPartyInstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                    return string.Format("{0}({1})", orderDetail.CounterPartyInstitutionShowName, orderDetail.CounterPartyTraderName.Trim());
                else
                    return string.Format("{0}({1})", orderDetail.CounterPartyInstitutionName, orderDetail.CounterPartyTraderName.Trim());
            }


            if (string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) || !string.IsNullOrEmpty(orderDetail.CounterPartyInstitutionShowName)))
            {
                if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                    return orderDetail.CounterPartyInstitutionShowName;
                else
                    return orderDetail.CounterPartyInstitutionName;
            }

            if (!string.IsNullOrWhiteSpace(orderDetail.CounterPartyTraderName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionName) && string.IsNullOrWhiteSpace(orderDetail.CounterPartyInstitutionShowName))
                return orderDetail.CounterPartyTraderName;

            return "";
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        /// <param name="isfirst"></param>
        private void BeginQuery(bool isfirst = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isfirst)
                {
                    isInit = true;
                }
                else
                {
                    InnerOrders.Clear();

                    //if (this.Mv.View != null)
                    //    this.Mv.View.Refresh();
                }

                LoadMaskVisible = Visibility.Visible;
            });
        }


        #region 设置过滤条件
        /// <summary>
        /// 处理过滤条件
        /// </summary>
        private void SetMvFilter()
        {
            this.Mv.View.Filter = x =>
            {
                TransactionOrdermModel ordermModel = x as TransactionOrdermModel;
                if (ordermModel != null)
                {
                    if (DealTimeChecked)
                    {
                        string dealDateEnd = this.DealTimeEnd.HasValue ? $"{this.DealTimeEnd.Value.ToString("yyyy-MM-dd")}" : "";
                        if (ordermModel.CreateTime.Contains(dealDateEnd))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                return true;
            };
        }

        #endregion

        #endregion

        #region 消息处理
        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "CopyDetailUI_GoTo_ExecId")
            {
                GoToRecordDetial(obj.Info1);
            }

        }

        /// <summary>
        /// 接收应用程序消息
        /// </summary>
        /// <param name="msg"></param>
        private void OnAppMsg1(object msg)
        {
            Common.Model.Fix.Message.PResult obj = (Common.Model.Fix.Message.PResult)msg;

            if (obj.Code == "CopyDetailUI_GoTo_ExecId")
            {
                GoToRecordDetial(obj.Info1);
            }

        }


        public void GoToRecordDetial(String id)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                SetScrollPositionByExecId(id);
                Window w = Window.GetWindow(this._transactionPanel);
                w.WindowState = WindowState.Normal;
                w.Activate();
            });

        }
        private void Instance_OnGlobalActivate(object obj)
        {
            WindowState ws = DesignHandler.Instance.winstate;
            if (ws != WindowState.Maximized)
            {
                object col = myWindow.FindName("dycolumn");
                if (col != null)
                {
                    DataGridTemplateColumn dtc = col as DataGridTemplateColumn;
                    if (dtc != null)
                    {
                        dtc.Width = myWindow.ActualWidth;
                    }
                }
            }
        }

        /// <summary>
        /// Windows 窗口状态改变
        /// </summary>
        /// <param name="obj"></param>
        private void Instance_OnNotifyWindowState(object obj)
        {
            if (obj.GetType() == typeof(WindowState))
            {
                WindowState ws = (WindowState)obj;
                if (ws == WindowState.Maximized)
                {
                    object col = myWindow.FindName("dycolumn");
                    if (col != null)
                    {
                        DataGridTemplateColumn dtc = col as DataGridTemplateColumn;
                        if (dtc != null)
                        {
                            dtc.Width = DataGridLength.Auto;
                        }
                    }
                }
            }
        }
        #endregion

        #region 业务逻辑处理
        /// <summary>
        /// 检查成交单是否可以修改
        /// </summary>
        /// <param name="transactionOrderm"></param>
        /// <returns></returns>
        public bool Check(TransactionOrdermModel transactionOrderm)
        {
            if (!("G".Equals(transactionOrderm.OrderStatus) || "I".Equals(transactionOrderm.OrderStatus)))
                return false;
            //线下录入在brokerB未确认的时候，brokerB不可以修改
            if (("I".Equals(transactionOrderm.OrderStatus) && LocalEnv.UserInfo.IsMyself(transactionOrderm.BrokerIdB)))
                return false;
            if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdA) && !SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdB))
                return false;

            //if (orderPo.NeedSendMsg)
            //    return false;

            ////线上拆单场景时，没有全部确认，增加背景色控制
            //if (orderPo.online.Equals("2") && orderPo.execOrders.Count > 1)
            //{
            //    foreach (OrderDetailPo detailPo in orderPo.execOrders)
            //    {
            //        if (!detailPo.orderStatus.Equals("G") && (orderPo.NeedSendMsg || orderPo.NeeSendMsgForB))
            //            return false;
            //    }
            //}
            ///G：已确认的成交单 I：线下录入brokerB还未确认的成交单
            if (("G".Equals(transactionOrderm.OrderStatus) || "I".Equals(transactionOrderm.OrderStatus)))
            {
                if (transactionOrderm.LookButtonVisible == Visibility.Visible)
                {
                    if (!LocalEnv.UserInfo.IsMyself(transactionOrderm.BridgeCloseById))
                    {
                        SysWarnGUI warnGUI = new SysWarnGUI();
                        warnGUI.setWarnMsg("请确认对方修改后再编辑！");

                        warnGUI.Owner = Application.Current.MainWindow;

                        warnGUI.ShowDialog();//.Show();
                        return false;
                    }

                }
                else
                {
                    if (transactionOrderm.NeeSendMsgForB && LocalEnv.UserInfo.IsMyself(transactionOrderm.BrokerIdB))
                        return false;
                }

                return true;
            }
            else
            {
                SysWarnGUI warnGUI = new SysWarnGUI();
                warnGUI.setWarnMsg("请双方确认后再修改！");

                warnGUI.Owner = Application.Current.MainWindow;

                warnGUI.ShowDialog();
                return false;
            }
            return true;
        }


        /// <summary>
        /// 检查成交单是否可以修改
        /// </summary>
        /// <param name="transactionOrderm"></param>
        /// <returns></returns>
        public bool CheckOrderStatus(TransactionOrdermModel transactionOrderm)
        {
            if (!("G".Equals(transactionOrderm.OrderStatus) || "I".Equals(transactionOrderm.OrderStatus)))
                return false;
            //线下录入在brokerB未确认的时候，brokerB不可以修改
            if (("I".Equals(transactionOrderm.OrderStatus) && LocalEnv.UserInfo.IsMyself(transactionOrderm.BrokerIdB)))
                return false;
            if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdA) && !SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdB))
                return false;
            return true;
        }

        public bool CheckOrderRemarkEdit(TransactionOrdermModel transactionOrderm)
        {
            if (!"G".Equals(transactionOrderm.OrderStatus))
                return false;
            if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdA) && !SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdB))
                return false;
            return true;
        }

        public bool CheckBridgeStatus(TransactionOrdermModel transactionOrderm)
        {
            if (!"G".Equals(transactionOrderm.OrderStatus))
                return false;
            //线下录入在brokerB未确认的时候，brokerB不可以修改
            if (("I".Equals(transactionOrderm.OrderStatus) && LocalEnv.UserInfo.IsMyself(transactionOrderm.BrokerIdB)))
                return false;
            if (!SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdA) && !SystemPowerHandler.Instance.IsCanEditOrder(transactionOrderm.BrokerIdB))
                return false;
            return true;
        }

        /// <summary>
        /// 编辑债券
        /// </summary>
        public void EditBondInfo(TextBlock textBlock)
        {
            TransactionOrdermModel orderDetailPo = textBlock.DataContext as TransactionOrdermModel;
            if (orderDetailPo == null)
                return;

            if (!Check(orderDetailPo))
                return;

            orderDetailPo.BondEditVisible = Visibility.Visible;
            orderDetailPo.BondShowVisible = Visibility.Collapsed;
            orderDetailPo.BondShowNoEditVisible = Visibility.Collapsed;

            //SearchBondUserControl searchBond = GetTemplateSelectorControl("st_bondEdit") as SearchBondUserControl;
            SearchBondUserControl searchBond = GetDataGridControl("st_bondEdit") as SearchBondUserControl;
            if (searchBond != null)
            {
                //searchBond.SelectAll();
                //searchBond.Focus();
                searchBond.Data = orderDetailPo;
                searchBond.SetButtonVisible(Visibility.Collapsed);
                searchBond.SetSearchTxt(orderDetailPo.SecurityID);
            }
            //TextBox searchBond = GetDataGridControl("tbSearch") as TextBox;
            //if (searchBond!=null) {
            //    searchBond.Text = orderDetailPo.SecurityID;
            //    searchBond.SelectAll();
            //    searchBond.Focus();
            //}
            //searchBond.

            _curOrdermModel = orderDetailPo;
        }

        public void EditOrderInfo(FrameworkElement textBlock, ECommandType field)
        {
            TransactionOrdermModel orderDetailPo = textBlock.DataContext as TransactionOrdermModel;
            if (orderDetailPo == null)
                return;

            if (!Check(orderDetailPo))
                return;

            switch (field)
            {
                case ECommandType.修改结算类型A:// 1:

                    break;
                case ECommandType.修改债券代码:

                    break;
                case ECommandType.修改价格:
                    //if (!recordDetailDomain.Check(orderDetailPo, OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId)))
                    //    return;

                    orderDetailPo.PriceEditVisible = Visibility.Visible;
                    orderDetailPo.PriceShowVisible = Visibility.Collapsed;
                    orderDetailPo.PriceShowNoEditVisible = Visibility.Collapsed;


                    TextBox tbPriceChange = GetDataGridControl("tbPriceChange") as TextBox;
                    //TextBox tbPriceChange = GetControl("tbPriceChange", "tbPriceChange_2") as TextBox;
                    if (tbPriceChange != null)
                    {
                        tbPriceChange.SelectAll();
                        tbPriceChange.Focus();
                    }
                    break;
                //case 4:
                case ECommandType.修改数量:
                    orderDetailPo.QtyEditVisible = Visibility.Visible;
                    orderDetailPo.QtyShowVisible = Visibility.Collapsed;
                    orderDetailPo.QtyShowNoEditVisible = Visibility.Collapsed;
                    orderDetailPo.QtyChange = orderDetailPo.Qty.Value / 1000;

                    TextBox tbAmountChange = GetDataGridControl("tbAmountChange") as TextBox;
                    //TextBox tbAmountChange = GetControl("tbAmountChange", "tbAmountChange_2") as TextBox;
                    if (tbAmountChange != null)
                    {
                        tbAmountChange.SelectAll();
                        tbAmountChange.Focus();
                    }
                    break;
                //case 5:
                case ECommandType.修改BrokerA:
                    if (!"G".Equals(orderDetailPo.OrderStatus))
                    {
                        return;
                    }
                    orderDetailPo.BrokerAEditVisible = Visibility.Visible;
                    orderDetailPo.BrokerAShowVisible = Visibility.Collapsed;
                    ContentPresenter cp = _currentDataGrid.CurrentCell.Column.GetCellContent(_currentDataGrid.SelectedItem) as ContentPresenter;

                    SearchBrokerControl searchBrokerEditUI = null;
                    //if (this._transactionModel == "1")
                    //{
                    //    searchBrokerEditUI = cp.ContentTemplate.FindName("seBrokerAChange_2", cp) as SearchBrokerControl;
                    //}
                    //else
                    //{
                    //    searchBrokerEditUI = cp.ContentTemplate.FindName("seBrokerAChange", cp) as SearchBrokerControl;
                    //}
                    searchBrokerEditUI = GetDataGridControl("seBrokerAChange") as SearchBrokerControl;
                    if (searchBrokerEditUI != null)
                    {
                        searchBrokerEditUI.SecurityType = orderDetailPo.SecurityType;
                        searchBrokerEditUI.SetSearchTxt(orderDetailPo.BrokerNameA);
                    }
                    break;
                //case 6:
                case ECommandType.修改BrokerB:
                    if (!"G".Equals(orderDetailPo.OrderStatus))
                    {
                        return;
                    }

                    orderDetailPo.BrokerBEditVisible = Visibility.Visible;
                    orderDetailPo.BrokerBShowVisible = Visibility.Collapsed;
                    ContentPresenter cpB = _currentDataGrid.CurrentCell.Column.GetCellContent(_currentDataGrid.SelectedItem) as ContentPresenter;
                    SearchBrokerControl searchBrokerEditUIB = null;
                    //if (this._transactionModel == "1")
                    //{
                    //    searchBrokerEditUIB = cpB.ContentTemplate.FindName("seBrokerAChangeB_2", cpB) as SearchBrokerControl;
                    //}
                    //else
                    //{
                    //    searchBrokerEditUIB = cpB.ContentTemplate.FindName("seBrokerAChangeB", cpB) as SearchBrokerControl;
                    //}
                    searchBrokerEditUIB = GetDataGridControl("seBrokerAChangeB") as SearchBrokerControl;

                    if (searchBrokerEditUIB != null)
                    {
                        searchBrokerEditUIB.SecurityType = orderDetailPo.SecurityType;
                        searchBrokerEditUIB.SetSearchTxt(orderDetailPo.BrokerNameB);
                    }
                    break;
                //case 7:
                case ECommandType.修改TraderA:
                    break;
                //case 8:
                case ECommandType.修改TraderB:
                    break;
                //case 9:
                case ECommandType.修改桥状态:

                    break;
                //case 10:
                case ECommandType.修改Bid发给:

                    break;
                //case 11:
                case ECommandType.修改Ofr发给:

                    break;
                //case 12:
                //case ECommandType.确认:
                //    execOrderPo.brokerreadflag = newVal;
                //    break;
                //case 13:
                case ECommandType.修改结算类型B:

                    break;
                //case 14:
                //case ECommandType.删除成交单:
                //    execOrderPo.delflag = newVal;
                //    break;
                //case 15:
                case ECommandType.修改返点:
                    orderDetailPo.FanDianEditVisible = Visibility.Visible;
                    orderDetailPo.FandianShowVisible = Visibility.Collapsed;
                    orderDetailPo.FandianShowReadVisible = Visibility.Collapsed;

                    TextBox txt_Fandian = GetDataGridControl("txt_Fandian") as TextBox;
                    //TextBox txt_Fandian = GetControl("txt_Fandian", "txt_Fandian_2") as TextBox;
                    if (txt_Fandian != null)
                    {
                        txt_Fandian.Text = orderDetailPo.Fandian;
                        txt_Fandian.SelectAll();
                        txt_Fandian.Focus();
                    }
                    break;
                //case 16:
                case ECommandType.修改BridgeA是否免佣金:

                    break;
                //case 17:
                case ECommandType.修改BridgeB是否免佣金:

                    break;
                case ECommandType.InternalTransaction:
                case ECommandType.NoInternalTransaction:
                    break;


            }

            _curOrdermModel = orderDetailPo;
        }

        /// <summary>
        /// 修改债券
        /// </summary>
        /// <param name="obj"></param>
        public async void UpdateBondInfo(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            try
            {
                _curOrdermModel.BondEditVisible = Visibility.Collapsed;
                _curOrdermModel.BondShowVisible = Visibility.Visible;
                _curOrdermModel.BondShowNoEditVisible = Visibility.Collapsed;
                if (obj == null || this._curOrdermModel == null)
                {
                    return;
                }

                bool needRequest = true;
                if (_curOrdermModel.SecurityID.Equals(obj.Bond_ID))
                    needRequest = false;

                if (needRequest)
                {
                    ExecutionReport request = GetExecutionReportPo(obj.Bond_ID, ECommandType.修改债券代码, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                    request.Symbol = obj.Short_Name;
                    request.TimeToMaturity = obj.RemaingingTime;

                    //根据债券类型设置默认渠道和发给 渠道变了发给应该也跟着变目前发给由客户自己在过桥界面手动去改
                    request.BridgeChannel = request.BridgeChannel2 = request.DoubleBridgeChannel = "1";
                    if (request.SecurityID.EndsWith(".SH") || request.SecurityID.EndsWith(".SZ"))
                    {
                        request.BridgeChannel = request.BridgeChannel2 = request.DoubleBridgeChannel = "6";
                    }
                    await UpdateExecOrderData(request);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                LogHelper.Error(ex.StackTrace);
                LogHelper.Error(obj);
            }
            //var win = Window.GetWindow(this);
            //if (win != null)
            //    win.Focus();
        }

        object obj = new object();
        /// <summary>
        /// 修改债券
        /// </summary>
        /// <param name="obj"></param>
        public async void UpdateOrderInfo(object element, ECommandType field)
        {
            try
            {
                if (element == null || this._curOrdermModel == null)
                {
                    return;
                }
                if (field != ECommandType.删除成交单)
                {
                    if (!Check(_curOrdermModel))
                        return;
                }
                bool needRequest = true;
                decimal newPrice = 0;

                switch (field)
                {
                    case ECommandType.修改结算类型A:// 1:

                        break;
                    case ECommandType.修改债券代码:

                        break;
                    case ECommandType.修改价格:
                        _curOrdermModel.PriceShowVisible = Visibility.Visible;
                        _curOrdermModel.PriceEditVisible = Visibility.Collapsed;
                        _curOrdermModel.PriceShowNoEditVisible = Visibility.Collapsed;
                        if (_curOrdermModel.Price != null && _curOrdermModel.Price.HasValue && decimal.TryParse(element.ToString(), out newPrice)
                            && newPrice != _curOrdermModel.Price.Value)
                            needRequest = true;
                        if (needRequest)
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), ECommandType.修改价格, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    //case 4:
                    case ECommandType.修改数量:
                        _curOrdermModel.QtyEditVisible = Visibility.Collapsed;
                        _curOrdermModel.QtyShowVisible = Visibility.Visible;
                        _curOrdermModel.QtyShowNoEditVisible = Visibility.Collapsed;
                        _curOrdermModel.QtyChange = _curOrdermModel.Qty.Value / 1000;
                        needRequest = true;
                        if (_curOrdermModel.Qty != null && _curOrdermModel.Qty.HasValue && decimal.TryParse(element.ToString(), out newPrice)
                    && _curOrdermModel.Qty.Value == (newPrice * 1000))
                            needRequest = false;

                        if (needRequest)
                        {
                            ExecutionReport request = GetExecutionReportPo((newPrice * 1000).ToString(), ECommandType.修改数量, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    //case 5:
                    case ECommandType.修改BrokerA:
                        _curOrdermModel.BrokerAEditVisible = Visibility.Collapsed;
                        _curOrdermModel.BrokerAShowVisible = Visibility.Visible;
                        needRequest = true;
                        if (element.ToString() == "" || (_curOrdermModel.BrokerIdA != null && _curOrdermModel.BrokerIdA == element.ToString()))
                            needRequest = false;

                        if (needRequest)
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), ECommandType.修改BrokerA, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    case ECommandType.修改BrokerB:
                        _curOrdermModel.BrokerBEditVisible = Visibility.Collapsed;
                        _curOrdermModel.BrokerBShowVisible = Visibility.Visible;
                        needRequest = true;
                        if (element.ToString() == "" || (_curOrdermModel.BrokerIdB != null && _curOrdermModel.BrokerIdB == element.ToString()))
                            needRequest = false;

                        if (needRequest)
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), ECommandType.修改BrokerB, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    case ECommandType.修改TraderA:
                        _curOrdermModel.TraderAEditVisible = Visibility.Collapsed;
                        _curOrdermModel.TraderAShowVisible = Visibility.Visible;
                        _curOrdermModel.TraderAShowNoEditVisible = Visibility.Collapsed;
                        string traderID = _curOrdermModel.TraderID;
                        if (_curOrdermModel.TraderID != element.ToString())
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), ECommandType.修改TraderA, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        //一批多的单子首次修改
                        #region 原来逻辑暂时废弃
                        //if (string.IsNullOrWhiteSpace(traderID))
                        //{
                        //    string blockNum = _curOrdermModel.Blocknum;
                        //    if (!string.IsNullOrWhiteSpace(blockNum)) {
                        //        List<TransOrderServerData> trans = TransOrderHandler.Instance.GetTransOrderByBlocknum(blockNum).ToList();
                        //        foreach (var item in trans)
                        //        {
                        //            if (item.ExecID!= _curOrdermModel.ExecID&& string.IsNullOrWhiteSpace(item.TraderID)) {
                        //                ExecutionReport request = GetExecutionReportPo(element.ToString(), ECommandType.修改TraderA, item.OrderID, item.ExecID);
                        //                await UpdateExecOrderData(request);
                        //            }
                        //        }
                        //    }
                        //}
                        #endregion
                        if (!string.IsNullOrWhiteSpace(_curOrdermModel.Blocknum))
                        {
                            SettlRequest request = new SettlRequest();
                            request.execId = _curOrdermModel.ExecID;
                            SettlResult reply = await GetExecOrderSettlData(request);
                            if (reply != null && "0000".Equals(reply.Code))
                            {
                                if (reply.SettlData.UpdateTraderA == "false" && reply.SettlData.SameBatchExecids != null && reply.SettlData.SameBatchExecids.Count > 1)
                                {
                                    foreach (var item in reply.SettlData.SameBatchExecids)
                                    {
                                        if (item != _curOrdermModel.ExecID)
                                        {
                                            TransOrderServerData trans = TransOrderHandler.Instance.GetTransOrderByExecID(item);
                                            bool isCanUpdate = false;
                                            if (string.IsNullOrEmpty(trans.BrokerUnread))
                                            {
                                                isCanUpdate = true;
                                            }
                                            else
                                            {
                                                if (trans.BrokerUnread.ContainsUserId(LocalEnv.UserInfo.UserId))
                                                {
                                                    //我是最后操作人
                                                    if (LocalEnv.UserInfo.IsMyself(trans.BridgeCloseById))
                                                    {
                                                        isCanUpdate = true;
                                                    }
                                                    else
                                                    {
                                                        //我同时是brokerA和brokerB
                                                        if (LocalEnv.UserInfo.IsMyself(trans.PartySubID) && LocalEnv.UserInfo.IsMyself(trans.CounterPartySubID))
                                                        {
                                                            isCanUpdate = true;
                                                        }
                                                        else
                                                        {
                                                            isCanUpdate = false;
                                                        }

                                                    }
                                                }
                                                else
                                                {
                                                    isCanUpdate = true;
                                                }
                                            }
                                            if (isCanUpdate)
                                            {
                                                ExecutionReport requestTrans = GetExecutionReportPo(element.ToString(), ECommandType.修改TraderA, trans.OrderID, trans.ExecID);
                                                UpdateExecOrderData(requestTrans);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    case ECommandType.修改TraderB:
                        _curOrdermModel.TraderBEditVisible = Visibility.Collapsed;
                        _curOrdermModel.TraderBShowVisible = Visibility.Visible;
                        if (_curOrdermModel.TraderIDB != element.ToString())
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), ECommandType.修改TraderB, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    case ECommandType.修改桥状态:
                        if (_curOrdermModel.BridgeFlag != element.ToString())
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    case ECommandType.修改Bid发给:
                        if (_curOrdermModel.BidSendto != element.ToString())
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    case ECommandType.修改Ofr发给:
                        if (_curOrdermModel.OfrSendto != element.ToString())
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    //case 12:
                    case ECommandType.确认:
                        ExecutionReport request1 = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                        await UpdateExecOrderData(request1);
                        break;
                    //case 13:
                    case ECommandType.修改结算类型B:

                        break;
                    //case 14:
                    case ECommandType.删除成交单:
                        ExecutionReport request2 = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                        await UpdateExecOrderData(request2);
                        break;
                    //case 15:
                    case ECommandType.修改返点:
                        _curOrdermModel.FandianShowVisible = Visibility.Visible;
                        _curOrdermModel.FandianShowReadVisible = Visibility.Collapsed;
                        _curOrdermModel.FanDianEditVisible = Visibility.Collapsed;
                        needRequest = true;
                        if ((!string.IsNullOrEmpty(_curOrdermModel.Fandian) && _curOrdermModel.Fandian.Equals(element.ToString()))
                               || (string.IsNullOrEmpty(_curOrdermModel.Fandian) && string.IsNullOrEmpty(element.ToString())))
                        {
                            needRequest = false;
                        }
                        if (needRequest)
                        {
                            ExecutionReport request = GetExecutionReportPo(element.ToString(), ECommandType.修改返点, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        break;
                    //case 16:
                    case ECommandType.修改BridgeA是否免佣金:
                    case ECommandType.修改BridgeB是否免佣金:
                        ExecutionReport request3 = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                        await UpdateExecOrderData(request3);
                        break;
                    //case 17:
                    case ECommandType.InternalTransaction:
                    case ECommandType.NoInternalTransaction:
                        ExecutionReport requestTransaction = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                        await UpdateExecPushOrderData(requestTransaction);
                        break;
                    case ECommandType.OpenSendStandardization:
                        ExecutionReport requestTransaction1 = GetExecutionReportPo(element.ToString(), field, _curOrdermModel.OrderID, _curOrdermModel.ExecID);
                        await UpdateExecOrderData(requestTransaction1);
                        break;

                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.ToString());
                LogHelper.Error(ex.StackTrace);
                LogHelper.Error(element);
            }
            //var win = Window.GetWindow(this);
            //if (win != null)
            //    win.Focus();
        }


        private ExecutionReport GetExecutionReportPo(string newVal, ECommandType field, string orderId, string execId)
        {
            ExecutionReport execOrderPo = new ExecutionReport();
            //execOrderPo.OrderID = orderId;
            execOrderPo.ExecID = execId;
            decimal d = 0;
            switch (field)
            {
                case ECommandType.修改结算类型A:// 1:
                    execOrderPo.SettlType = newVal;
                    break;
                //case 2:
                case ECommandType.修改债券代码:
                    execOrderPo.SecurityID = newVal;
                    break;
                //case 3:
                case ECommandType.修改价格:
                    if (decimal.TryParse(newVal, out d))
                        execOrderPo.Price = newVal;
                    break;
                //case 4:
                case ECommandType.修改数量:
                    if (decimal.TryParse(newVal, out d))
                        execOrderPo.LastQty = newVal;
                    break;
                //case 5:
                case ECommandType.修改BrokerA:
                    execOrderPo.PartySubID = newVal;
                    break;
                //case 6:
                case ECommandType.修改BrokerB:
                    execOrderPo.CounterPartySubID = newVal;
                    break;
                //case 7:
                case ECommandType.修改TraderA:
                    execOrderPo.TraderID = newVal;
                    break;
                //case 8:
                case ECommandType.修改TraderB:
                    execOrderPo.CounterPartyTraderId = newVal;
                    break;
                //case 9:
                case ECommandType.修改桥状态:
                    execOrderPo.BridgeFlag = newVal;
                    break;
                //case 10:
                case ECommandType.修改Bid发给:
                    execOrderPo.SendTo = newVal;
                    break;
                //case 11:
                case ECommandType.修改Ofr发给:
                    execOrderPo.SendTo2 = newVal;
                    break;
                //case 12:
                case ECommandType.确认:
                    //execOrderPo.brokerreadflag = newVal;
                    break;
                //case 13:
                case ECommandType.修改结算类型B:
                    execOrderPo.SettlType2 = newVal;
                    break;
                //case 14:
                case ECommandType.删除成交单:
                    execOrderPo.DelFlag = newVal;
                    break;
                //case 15:
                case ECommandType.修改返点:
                    execOrderPo.FanDian = newVal;
                    break;
                //case 16:
                case ECommandType.修改BridgeA是否免佣金:
                    execOrderPo.BrokerageA = newVal;
                    break;
                //case 17:
                case ECommandType.修改BridgeB是否免佣金:
                    execOrderPo.BrokerageB = newVal;
                    break;
                case ECommandType.InternalTransaction:
                case ECommandType.NoInternalTransaction:
                    execOrderPo.ForBidRefreshFlag = newVal;
                    execOrderPo.Side = newVal;
                    break;
                //case ECommandType.ManualSendDealA:
                //    execOrderPo.sendSorFlaga = newVal;
                //    break;
                //case ECommandType.ManualSendDealB:
                //    execOrderPo.sendSorFlagb = newVal;
                //    break;
                //case ECommandType.UpdateSendDirection:
                //    execOrderPo.issueDir = newVal;
                //    break;
                //case ECommandType.UpdateSendChannel:
                //    execOrderPo.issueChannel = newVal;
                //    break;
                //case ECommandType.UpdateIssuer:
                //    execOrderPo.issueTraderId = newVal;
                //    break;
                //case ECommandType.UpdateSendRemarks:
                //    execOrderPo.issueRemark = newVal;
                //    break;
                //case ECommandType.UpdatePayFee:
                //    execOrderPo.proxyFee = newVal;
                //    break;
                //case ECommandType.CancelPayFee:
                //    execOrderPo.proxyFee = "";
                //    break;
                case ECommandType.OpenSendStandardization:
                    execOrderPo.IssueState = newVal;
                    break;
                    //case ECommandType.UpdateDetailMark:
                    //    execOrderPo.detailMark = newVal;
                    //    break;

            }
            execOrderPo.CommandType = Convert.ToString((int)field);
            return execOrderPo;
        }

        private object GetDataGridControl(string controlName)
        {
            ContentPresenter cp = _currentDataGrid.CurrentCell.Column.GetCellContent(_currentDataGrid.SelectedItem) as ContentPresenter;
            object framework = cp.ContentTemplate.FindName(controlName, cp);
            return framework;
        }
        /// <summary>
        /// 获取表格控件
        /// </summary>
        /// <param name="controlName"></param>
        /// <param name="controlNameCard"></param>
        private object GetControl(string controlName, string controlNameCard)
        {
            ContentPresenter cp = _currentDataGrid.CurrentCell.Column.GetCellContent(_currentDataGrid.SelectedItem) as ContentPresenter;
            object framework = null;

            if (this._transactionModel == "1")
            {
                framework = cp.ContentTemplate.FindName(controlNameCard, cp);
            }
            else
            {
                framework = cp.ContentTemplate.FindName(controlName, cp);
            }
            return framework;
        }

        public async Task<PResult> UpdateExecOrderData(ExecutionReport req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                req.CloseById = LocalEnv.UserInfo.UserId;
                reply = GRHandler.Instance.ExecOrderUpdate(req);
                if (reply != null && reply.Code != "0000")
                {
                    ShowMessage(reply.Msg);
                }
            });

            return reply;
        }

        /// <summary>
        /// 推市场成交
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PResult> UpdateExecPushOrderData(ExecutionReport req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecPushOrderUpdate(req);
                if (reply != null && reply.Code != "0000")
                {
                    ShowMessage(reply.Msg);
                }
            });

            return reply;
        }


        public async Task<PResult> UpdateBatchExecOrderData(List<ExecutionReport> req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecBatchOrderUpdate(req);
                if (reply != null && reply.Code != "0000")
                {
                    ShowMessage(reply.Msg);
                }
            });

            return reply;
        }

        /// <summary>
        /// 克隆订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<PResult> CloneExecOrderData(CloneRequestPo req)
        {
            PResult reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecOrderClone(req);
                if (reply != null && reply.Code != "0000")
                {
                    ShowMessage(reply.Msg);
                }
            });

            return reply;
        }

        /// <summary>
        /// 修改结算
        /// </summary>
        /// <param name="field"></param>
        /// <param name="obj"></param>
        /// <param name="item"></param>
        /// <param name="updateTag"> 1 只修改ofr 方向，2只修改bid 方向 3 双向修改</param>
        private async void UpdateSettyle(ECommandType field, SettlementPo obj, ExecutionReport item, int updateTag)
        {
            ExecutionReport execOrderPo = new ExecutionReport();
            execOrderPo.OrderID = item.OrderID;
            execOrderPo.ExecID = item.ExecID;
            execOrderPo.CloseById = LocalEnv.UserInfo.UserId;
            if ((!(item.SettlType2 + "").Equals(obj.SettleDisplay + "") || !(item.SettlType + "").Equals(obj.BidSettleDisplay + ""))
                || ((item.SettlType2 + "").Equals(obj.SettleDisplay + "") && !(item.Remark + "").Equals(obj.Remarks + "")))
            {
                if (updateTag == 1)
                {
                    if ((item.SettlType2 + "").Equals(obj.SettleDisplay + "") && (item.Remark + "").Equals(obj.Remarks + ""))
                    {
                        return;
                    }
                    UpdateSettyleInit(ECommandType.修改结算类型A, obj, item, 1, execOrderPo);
                }
                else if (updateTag == 2)
                {
                    if ((item.SettlType + "").Equals(obj.BidSettleDisplay + "") && (item.Remark + "").Equals(obj.Remarks + ""))
                    {
                        return;
                    }
                    UpdateSettyleInit(ECommandType.修改结算类型B, obj, item, 2, execOrderPo);
                }
                else if (updateTag == 3)
                {
                    if ((item.SettlType + "").Equals(obj.BidSettleDisplay + "")
                        && (item.SettlType2 + "").Equals(obj.SettleDisplay + "")
                        && (item.Remark + "").Equals(obj.Remarks + ""))
                    {
                        return;
                    }

                    bool TypeA = !(item.SettlType2 + "").Equals(obj.SettleDisplay + "") || !(item.Remark + "").Equals(obj.Remarks + "");
                    bool TypeB = !(item.SettlType + "").Equals(obj.BidSettleDisplay + "") || !(item.Remark + "").Equals(obj.Remarks + "");

                    if (!(item.SettlType2 + "").Equals(obj.SettleDisplay + "")
                       || !(item.Remark + "").Equals(obj.Remarks + ""))
                    {
                        UpdateSettyleInit(ECommandType.修改结算类型A, obj, item, 1, execOrderPo);
                    }

                    if (!(item.SettlType + "").Equals(obj.BidSettleDisplay + "")
                       || !(item.Remark + "").Equals(obj.Remarks + ""))
                    {
                        UpdateSettyleInit(ECommandType.修改结算类型B, obj, item, 2, execOrderPo);
                    }

                    //如果ofr和bid都有修改，则合并成一次请求
                    if (TypeA && TypeB)
                    {
                        execOrderPo.CommandType = ((int)ECommandType.修改结算类型A和B).ToString();
                    }
                }

                await UpdateExecOrderData(execOrderPo);

                //修改双桥结算
                if (item.BridgeType + "" == "B" || item.BridgeType + "" == "O")
                {
                    SettlementPo settlementPo = new SettlementPo()
                    {
                        BidSettle = string.IsNullOrWhiteSpace(execOrderPo.SettlType) ? item.SettlType : execOrderPo.SettlType,
                        Settle = string.IsNullOrWhiteSpace(execOrderPo.SettlType2) ? item.SettlType2 : execOrderPo.SettlType2
                    };

                    ExecutionReport result = new ExecutionReport();
                    result.OrderID = item.OrderID;
                    result.ExecID = item.ExecID;
                    //非错期
                    if (settlementPo.BidSettle == settlementPo.Settle)
                    {
                        result.DoubleBridgeSettleSide = string.Empty;
                    }
                    //修改双桥结算
                    result.DoubleBridgeOffSettltype = GetSettle(settlementPo, true);
                    result.DoubleBridgeBidSettltype = GetSettle(settlementPo, false);
                    //操作类型
                    result.CommandType = ((int)ECommandType.修改桥要素).ToString();
                    ///borderID传给后台
                    //result.BorderId = item.bridgeOrder?.borderID;
                    //更新成交单状态

                    await UpdateExecOrderData(result);
                }
            }
        }

        /// <summary>
        /// 获取单子的双桥结算
        /// </summary>
        /// <param name="settle"></param>
        /// <param name="isOfr"></param>
        /// <returns></returns>
        private string GetSettle(SettlementPo settle, bool isOfr)
        {
            string settleStrOfr = settle.Settle;
            string settleStrBid = settle.BidSettle;
            switch (settleStrOfr)
            {
                case "1":
                    settleStrOfr = "+0";
                    break;
                case "2":
                    settleStrOfr = "+1";
                    break;
                case "3":
                    settleStrOfr = "明天+0";
                    break;
            }

            switch (settleStrBid)
            {
                case "1":
                    settleStrBid = "+0";
                    break;
                case "2":
                    settleStrBid = "+1";
                    break;
                case "3":
                    settleStrBid = "明天+0";
                    break;
            }
            if (isOfr)
            {
                if (settle.Settle == settle.BidSettle)
                {

                    return settleStrOfr;
                }
                else
                {
                    if (_curOrdermModel.DoubleBridgeSettleSide + "" == "B")
                    {
                        return settleStrOfr;
                    }
                    else
                    {
                        return settleStrOfr + "/" + settleStrBid;
                    }

                }
            }
            else
            {
                if (settle.Settle == settle.BidSettle)
                {
                    return settleStrOfr;
                }
                else
                {
                    if (_curOrdermModel.DoubleBridgeSettleSide + "" == "B")
                        return settleStrOfr + "/" + settleStrBid;
                    else
                        return settleStrBid;
                }
            }
        }


        /// <summary>
        /// 修改结算
        /// </summary>
        public async void EditSettyle(object sender, SettlementPo obj)
        {
            if (_curOrdermModel == null)
                return;
            TransactionOrdermModel temp = _curOrdermModel;
            //SettleUIWin settleUIWin = sender as SettleUIWin;
            //if (settleUIWin != null)
            //    settleUIWin.OnSettyleChange -= SettleWin_OnSettyleChange;
            //UpdateSettyle(ECommandType.修改结算类型A, obj, temp, 3);
            TransOrderServerData transa = TransOrderHandler.Instance.GetTransOrderByExecID(temp.ExecID);
            //查询有没有修改过结算
            if (!LocalEnv.ConfInfo.IsFirstUpdateSettlementSync)
            {
                UpdateSettyle(ECommandType.修改结算类型A, obj, transa, 3);
            }
            else
            {
                //UpdateSettyle(ECommandType.修改结算类型A, obj, transa, 3);
                //UpdateSettyle(ECommandType.修改结算类型A, obj, temp, 3);
                //OrderPo orderPo = OrderHandler.Instance.GetOrderById(temp.OrderId);
                //if (orderPo != null && orderPo.execOrders != null && orderPo.execOrders.Count == 1)
                //{

                //}
                //else
                //{
                SettlRequest request = new SettlRequest();
                request.execId = temp.ExecID;
                SettlResult reply = await GetExecOrderSettlData(request);
                if (reply != null && "0000".Equals(reply.Code))
                {
                    string settlementUpdateRecord = "false";
                    if (temp.IsGVN)
                    {
                        settlementUpdateRecord = reply.SettlData.UpdateOfrSettltype;
                    }
                    else
                    {
                        settlementUpdateRecord = reply.SettlData.UpdateBidSettltype;
                    }
                    if (settlementUpdateRecord == "true")
                    {
                        UpdateSettyle(ECommandType.修改结算类型A, obj, transa, 3);
                    }
                    else
                    {

                        foreach (string item in reply.SettlData.SameBatchExecids)
                        {
                            TransOrderServerData trans = TransOrderHandler.Instance.GetTransOrderByExecID(item);
                            if (trans != null)
                            {
                                if (trans.ExecID == temp.ExecID)
                                {
                                    UpdateSettyle(ECommandType.修改结算类型A, obj, trans, 3);
                                    Thread.Sleep(50);
                                }
                                else
                                {
                                    if (temp.IsGVN)
                                    {
                                        UpdateSettyle(ECommandType.修改结算类型A, obj, trans, 1);
                                        Thread.Sleep(50);
                                    }
                                    else
                                    {
                                        UpdateSettyle(ECommandType.修改结算类型B, obj, trans, 2);
                                        Thread.Sleep(50);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    UpdateSettyle(ECommandType.修改结算类型A, obj, transa, 3);
                }
            }


            //Application.Current.MainWindow.Focus();
        }
        /// <summary>
        /// 修改结算执行方法
        /// </summary>
        /// <param name="field"></param>
        /// <param name="obj"></param>
        /// <param name="item"></param>
        /// <param name="updateTag">1 只修改ofr 方向，2只修改bid 方向</param>
        private void UpdateSettyleInit(ECommandType field, SettlementPo obj, ExecutionReport item, int updateTag, ExecutionReport updatePoA)
        {
            updatePoA.CommandType = ((int)field).ToString();
            string settleStr2 = obj.Settle;
            switch (obj.Settle)
            {
                case "+0":
                    settleStr2 = "1";
                    break;
                case "+1":
                    settleStr2 = "2";
                    break;
                case "明天+0":
                    settleStr2 = "3";
                    break;
            }
            string settleStrBid = obj.BidSettle;
            switch (obj.BidSettle)
            {
                case "+0":
                    settleStrBid = "1";
                    break;
                case "+1":
                    settleStrBid = "2";
                    break;
                case "明天+0":
                    settleStrBid = "3";
                    break;
            }
            if (updateTag == 1)
            {
                updatePoA.SettlType2 = settleStr2;
                updatePoA.TradeDate2 = obj.TradeDate.Replace("-", "");
                updatePoA.SettlDate2 = obj.SettlementDate.Replace("-", "");
                updatePoA.Remark = obj.Remarks;
            }
            else if (updateTag == 2)
            {
                updatePoA.SettlType = settleStrBid;
                updatePoA.TradeDate = obj.BidTradeDate.Replace("-", "");
                updatePoA.SettlDate = obj.BidSettlementDate.Replace("-", "");
                updatePoA.Remark = obj.Remarks;
            }
        }

        public void SetEditOrderModel(TransactionOrdermModel orderDetailPo)
        {
            _curOrdermModel = orderDetailPo;
        }


        public async Task<SettlResult> GetExecOrderSettlData(SettlRequest req)
        {
            SettlResult reply = null;
            await Task.Run(() =>
            {
                reply = GRHandler.Instance.ExecSettInfoQuery(req);
            });

            return reply;
        }


        /// <summary>
        /// 编辑交易员
        /// </summary>
        public void EditOrderTrader(object sender, Popup pop, MouseButtonEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            TransactionOrdermModel orderDetailPo = button.DataContext as TransactionOrdermModel;
            if (orderDetailPo == null)
            {
                return;
            }
            _curOrdermModel = orderDetailPo;
            string traderId = string.Empty;

            if (button.Tag.ToString() == "A")
            {
                if (!Check(orderDetailPo))
                {
                    return;
                }
                //if (!recordDetailDomain.TraderACheck(orderDetailPo, OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId)))
                //{
                //    return;
                //}
                traderId = orderDetailPo.TraderID;
            }
            else
            {
                if (!Check(orderDetailPo))
                {
                    return;
                }
                traderId = orderDetailPo.TraderIDB;
            }
            ++CkickCount;
            DispatcherTimer ClickTimer = new DispatcherTimer();
            ClickTimer.Interval = new TimeSpan(0, 0, 0, 0, LocalEnv.ConfInfo.DoubleIntervalTime);
            ClickTimer.Tick += (s, e1) =>
            {
                ClickTimer.IsEnabled = false;
                CkickCount = 0;
                if (!IsDoubleClick)
                {
                    if (!string.IsNullOrWhiteSpace(traderId) && LocalEnv.UserInfo.CompanyId != "3")
                    {
                        ClickCommand(sender, pop, e);
                    }
                }
                IsDoubleClick = false;
            };
            ClickTimer.IsEnabled = true;
            if (CkickCount % 2 == 0)
            {
                ClickTimer.IsEnabled = false;
                CkickCount = 0;
                IsDoubleClick = true;
                DoubleClickCommand(sender, e);
            }
        }


        private Popup pop = null;
        TransactionOrdermModel _operation = null;
        private bool _isDoubleClickGrid = true;
        private void ClickCommand(object sender, Popup pop, MouseButtonEventArgs e)
        {
            _transactionPanel.tb_seqTxt.Text = "";
            Button button = sender as Button;
            _transactionPanel.tb_seqTxt.Tag = button.Tag;
            _operation = button.DataContext as TransactionOrdermModel;
            pop.PlacementTarget = button;
            pop.StaysOpen = false;
            _transactionPanel.dg_orderList.DataContext = null;
            pop.IsOpen = true;
            _transactionPanel.tb_seqTxt.Focus();
            e.Handled = true;
        }

        private void DoubleClickCommand(object sender, MouseButtonEventArgs e)
        {
            _transactionPanel.tbpop_Seq.IsOpen = false;
            _isDoubleClickGrid = false;
            Button button = sender as Button;
            if (button.Tag.ToString() == "A")
            {
                button.Focus();
                e.Handled = true;
                TransactionOrdermModel orderDetailPo = button.DataContext as TransactionOrdermModel;
                if (orderDetailPo == null)
                    return;
                //修改traderA，无需先确认对方修改
                //if (!recordDetailDomain.TraderACheck(orderDetailPo, OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId)))
                //    return;
                orderDetailPo.TraderAEditVisible = Visibility.Visible;
                orderDetailPo.TraderAShowVisible = Visibility.Collapsed;

                ContentPresenter cp = _currentDataGrid.CurrentCell.Column.GetCellContent(_currentDataGrid.SelectedItem) as ContentPresenter;

                SearchTraderControl searchTraderEditUI = null;
                //if (this._transactionModel == "1")
                //{
                //    searchTraderEditUI = cp.ContentTemplate.FindName("seTraderAChange_2", cp) as SearchTraderControl;
                //}
                //else
                //{
                //    searchTraderEditUI = cp.ContentTemplate.FindName("seTraderAChange", cp) as SearchTraderControl;
                //}


                searchTraderEditUI = GetDataGridControl("seTraderAChange") as SearchTraderControl;

                if (searchTraderEditUI != null)
                {
                    searchTraderEditUI.SecurityType = orderDetailPo.SecurityType;
                    searchTraderEditUI.Data = orderDetailPo;
                    searchTraderEditUI.SetSearchTxt(orderDetailPo.TraderAInfoDisplay);
                }
            }
            else
            {
                button.Focus();
                e.Handled = true;
                TransactionOrdermModel orderDetailPo = _currentDataGrid.SelectedItem as TransactionOrdermModel;

                if (!Check(orderDetailPo))
                    return;

                orderDetailPo.TraderBEditVisible = Visibility.Visible;
                orderDetailPo.TraderBShowVisible = Visibility.Collapsed;

                ContentPresenter cp = _currentDataGrid.CurrentCell.Column.GetCellContent(_currentDataGrid.SelectedItem) as ContentPresenter;

                SearchTraderControl searchTraderEditUI = null;
                //if (this._transactionModel == "1")
                //{
                //    searchTraderEditUI = cp.ContentTemplate.FindName("seTraderBChange_2", cp) as SearchTraderControl;
                //}
                //else
                //{
                //    searchTraderEditUI = cp.ContentTemplate.FindName("seTraderBChange", cp) as SearchTraderControl;
                //}

                searchTraderEditUI = GetDataGridControl("seTraderBChange") as SearchTraderControl;
                if (searchTraderEditUI != null)
                {
                    searchTraderEditUI.SecurityType = orderDetailPo.SecurityType;
                    searchTraderEditUI.Data = orderDetailPo;
                    searchTraderEditUI.SetSearchTxt(orderDetailPo.TraderBInfoDisplay);
                }
            }
        }

        /// <summary>
        /// 获取模板选择器中的控件
        /// </summary>
        /// <param name="controlName">控件名称</param>
        /// <returns></returns>
        private object GetTemplateSelectorControl(string controlName)
        {
            ContentPresenter cp = _currentDataGrid.CurrentCell.Column.GetCellContent(_currentDataGrid.SelectedItem) as ContentPresenter;
            object framework = null;

            TransactionSelector transactionSelector = cp.ContentTemplateSelector as TransactionSelector;
            DataTemplate dataTemplate = null;
            if (this._transactionModel == "1")
            {
                dataTemplate = transactionSelector.CardDataemplate;
            }
            else if (this._transactionModel == "2")
            {
                dataTemplate = transactionSelector.ListFixDataTemplate;
            }
            else
            {
                dataTemplate = transactionSelector.ListDataTemplate;
            }
            framework = dataTemplate.FindName(controlName, cp);
            return framework;
        }


        /// <summary>
        /// 定位到指定序号的成交单
        /// </summary>
        /// <param name="seq"></param>
        public void SetScrollPositionByOrderStatus(List<string> oderStatus)
        {
            LogHelper.Info("订单状态：" + oderStatus.ToString());
            List<TransactionOrdermModel> selectOrders = InnerOrders.Where(delegate (TransactionOrdermModel order)
            {
                if (order.IsSelect)
                    return true;
                else
                    return false;
            }).ToList();

            if (selectOrders != null && selectOrders.Count > 0)
            {
                selectOrders[0].IsSelect = false;
            }

            if (oderStatus != null && oderStatus.Count > 0)
            {
                selectOrders = InnerOrders.Where(delegate (TransactionOrdermModel order)
                {
                    if (oderStatus.Contains(order.OrderStatus))
                        return true;
                    else
                        return false;
                }).ToList();

                if (selectOrders != null && selectOrders.Count > 0)
                {
                    int index = -1;
                    foreach (TransactionOrdermModel od in selectOrders)
                    {
                        index++;
                        if (_selectOrder == null)
                        {
                            _selectOrder = od;
                            break;
                        }
                        else
                        {
                            if (_selectOrder.Equals(od))
                            {
                                if (index < (selectOrders.Count - 1))
                                {
                                    _selectOrder = selectOrders[index + 1];
                                    break;
                                }
                                else if (index == (selectOrders.Count - 1))
                                {
                                    _selectOrder = selectOrders[0];
                                    break;
                                }
                            }
                            else
                            {
                                if (index == (selectOrders.Count - 1))
                                {
                                    _selectOrder = selectOrders[0];
                                    break;
                                }
                                //else {
                                //    _selectOrder = selectOrders[index];
                                //    break;
                                //}
                            }
                        }

                    }
                    _selectOrder.IsSelect = true;
                    _currentDataGrid.ScrollIntoView(_selectOrder);
                }
                else
                {
                    _selectOrder = null;
                }
            }
            else
            {
                _selectOrder = null;
            }
        }


        private void ShowMessage(string message)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                DeleteBoxView me = new DeleteBoxView();

                me.Title = "提示";
                me.setWarnMsg(message);
                me.ShowDialog();

            }));
        }


        /// <summary>
        /// 设置查看按钮
        /// </summary>
        /// <param name="transaction"></param>
        public void SetLookButton(TransactionOrdermModel transaction)
        {
            //查看成交修改按【查看】按钮
            if (string.IsNullOrEmpty(transaction.BrokerUnread))
            {
                transaction.LookButtonVisible = Visibility.Collapsed;
            }
            else
            {
                if (transaction.BrokerUnread.ContainsUserId(LocalEnv.UserInfo.UserId))
                {
                    //我是最后操作人
                    if (LocalEnv.UserInfo.IsMyself(transaction.BridgeCloseById))
                    {
                        //我同时是brokerA和brokerB
                        if (LocalEnv.UserInfo.IsMyself(transaction.BrokerIdA) && LocalEnv.UserInfo.IsMyself(transaction.BrokerIdB))
                            transaction.LookButtonStatus = "1";
                        else
                        {
                            transaction.LookButtonStatus = "2";
                        }
                    }
                    else
                    {
                        transaction.LookButtonStatus = "1";
                    }
                    transaction.LookButtonVisible = Visibility.Visible;
                }
                else
                {
                    transaction.LookButtonVisible = Visibility.Collapsed;
                    transaction.LookButtonStatus = "1";
                }
            }
        }


        public bool CheckEnable(ExecutionReport orderDetailPo)
        {
            if (!"G".Equals(orderDetailPo.OrdStatus))
                return false;
            //线下录入在brokerB未确认的时候，brokerB不可以修改

            if (!SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.PartySubID) && !SystemPowerHandler.Instance.IsCanEditOrder(orderDetailPo.CounterPartySubID))
                return false;
            return true;
        }

        public void SearchOrderBySeq(TextBox tb_seqTxt)
        {
            int number;
            if (!string.IsNullOrWhiteSpace(tb_seqTxt.Text))
            {
                if (int.TryParse(tb_seqTxt.Text, out number))
                {
                    if (number == 0)
                    {
                        _transactionPanel.dg_orderList.DataContext = null;
                        return;
                    }
                    string seqNum = number.ToString();
                    List<TransactionOrdermModel> selectOrders = InnerOrders.Where(_ => _.Seq == seqNum
                    && _operation.ExecID != _.ExecID
                    && (SystemPowerHandler.Instance.IsCanEditOrder(_.BrokerIdA) || SystemPowerHandler.Instance.IsCanEditOrder(_.BrokerIdB))
                    && _operation.SecurityID == _.SecurityID
                    && _operation.Price == _.Price
                    && _operation.QtyDisplay == _.QtyDisplay).ToList();

                    List<TransactionOrdermModel> operationOrderDetails = new List<TransactionOrdermModel>();
                    foreach (TransactionOrdermModel item in selectOrders)
                    {
                        DateTime dt;
                        if (DateTime.TryParseExact(item.CreateTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            if (dt.ToShortDateString() == DateTime.Now.ToShortDateString())
                            {
                                item.DateExchangeShow = "今天";
                            }
                            else
                            {
                                item.DateExchangeShow = dt.ToShortDateString();
                            }
                        }
                        else
                        {
                            item.DateExchangeShow = item.CreateTime;
                        }
                        if (item.Side == _operation.Side)
                        {
                            if (tb_seqTxt.Tag.ToString() == "A")
                            {
                                item.TraderExchangeShow = item.TraderAInfoDisplay;
                                item.TraderExchangeID = item.TraderID;
                            }
                            else
                            {
                                item.TraderExchangeShow = item.TraderBInfoDisplay;
                                item.TraderExchangeID = item.TraderIDB;
                            }
                        }
                        else
                        {
                            if (tb_seqTxt.Tag.ToString() == "A")
                            {
                                item.TraderExchangeShow = item.TraderBInfoDisplay;
                                item.TraderExchangeID = item.TraderIDB;
                            }
                            else
                            {
                                item.TraderExchangeShow = item.TraderAInfoDisplay;
                                item.TraderExchangeID = item.TraderID;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(item.TraderExchangeID))
                        {
                            continue;
                        }
                        operationOrderDetails.Add(item);
                    }
                    _transactionPanel.dg_orderList.DataContext = operationOrderDetails;
                    if (operationOrderDetails.Count > 0)
                    {
                        _transactionPanel.dg_orderList.SelectedIndex = 0;
                    }
                    else
                    {
                        _transactionPanel.dg_orderList.SelectedIndex = -1;
                    }
                }
                else
                {
                    _transactionPanel.dg_orderList.DataContext = null;
                }
            }
            else
            {
                _transactionPanel.dg_orderList.DataContext = null;
            }
        }

        /// <summary>
        /// 交换交易员
        /// </summary>
        /// <param name="tb_seqTxt"></param>
        public async void ExchangeTrader(TextBox tb_seqTxt)
        {
            TransactionOrdermModel operationOrderDetailSelect = _transactionPanel.dg_orderList.SelectedItem as TransactionOrdermModel;
            if (operationOrderDetailSelect != null)
            {
                if (_operation != null)
                {
                    if (tb_seqTxt.Tag.ToString() == "A")
                    {
                        if (_operation.TraderID != operationOrderDetailSelect.TraderExchangeID)
                        {
                            ExecutionReport request = GetExecutionReportPo(operationOrderDetailSelect.TraderExchangeID, ECommandType.修改TraderA, _operation.OrderID, _operation.ExecID);
                            await UpdateExecOrderData(request);
                        }
                        if (_operation.Side == operationOrderDetailSelect.Side)
                        {
                            if (_operation.TraderID != operationOrderDetailSelect.TraderID)
                            {
                                ExecutionReport request = GetExecutionReportPo(_operation.TraderID, ECommandType.修改TraderA, operationOrderDetailSelect.OrderID, operationOrderDetailSelect.ExecID);
                                await UpdateExecOrderData(request);
                            }
                        }
                        else
                        {
                            if (_operation.TraderID != operationOrderDetailSelect.TraderIDB)
                            {

                                ExecutionReport request = GetExecutionReportPo(_operation.TraderID, ECommandType.修改TraderB, operationOrderDetailSelect.OrderID, operationOrderDetailSelect.ExecID);
                                await UpdateExecOrderData(request);
                            }
                        }
                    }
                    else
                    {
                        if (_operation.TraderIDB != operationOrderDetailSelect.TraderExchangeID)
                        {
                            ExecutionReport request = GetExecutionReportPo(operationOrderDetailSelect.TraderExchangeID, ECommandType.修改TraderB, _operation.OrderID, _operation.ExecID);
                            await UpdateExecOrderData(request);
                        }

                        if (_operation.Side == operationOrderDetailSelect.Side)
                        {
                            if (_operation.TraderIDB != operationOrderDetailSelect.TraderIDB)
                            {
                                ExecutionReport request = GetExecutionReportPo(_operation.TraderIDB, ECommandType.修改TraderB, operationOrderDetailSelect.OrderID, operationOrderDetailSelect.ExecID);
                                await UpdateExecOrderData(request);
                            }
                        }
                        else
                        {
                            if (_operation.TraderIDB != operationOrderDetailSelect.TraderID)
                            {
                                ExecutionReport request = GetExecutionReportPo(_operation.TraderIDB, ECommandType.修改TraderA, operationOrderDetailSelect.OrderID, operationOrderDetailSelect.ExecID);
                                await UpdateExecOrderData(request);
                            }
                        }

                    }

                }

            }
            _transactionPanel.tbpop_Seq.IsOpen = false;
        }



        public void ExcelChangeSize()
        {
            GridWidth = (_transactionPanel.ActualWidth - 20) < 600 ? 600 : (_transactionPanel.ActualWidth - 20);
            if (_transactionModel == "1")
            {
                double autoWidth = GridWidth;
                if (autoWidth > 0)
                {
                    GridBondWidth = (125 / 849.0) * autoWidth < 125 ? 125 : (125 / 849.0) * autoWidth;
                    GridSettWidth = (115 / 849.0) * autoWidth < 115 ? 115 : (115 / 849.0) * autoWidth;
                    GridTraderWidth = (142 / 849.0) * autoWidth < 142 ? 142 : (142 / 849.0) * autoWidth;
                    GridRemarkWidth = (50.0 / 849.0) * autoWidth < 50 ? 50 : (50 / 849.0) * autoWidth;
                    GridSendWidth = (GridWidth - 190) / 2;
                }
                else
                {
                    GridBondWidth = 125;
                    GridSettWidth = 115;
                    GridTraderWidth = 142;
                    GridRemarkWidth = 50;
                    GridSendWidth = 135;
                }

            }
            else if (_transactionModel == "2")
            {

                double autoWidth = GridWidth - 800;
                if (autoWidth > 0)
                {
                    GridBondWidth = (125 / 849.0) * autoWidth < 125 ? 125 : (125 / 849.0) * autoWidth;
                    GridSettWidth = (115 / 849.0) * autoWidth < 115 ? 115 : (115 / 849.0) * autoWidth;
                    GridTraderWidth = (142 / 849.0) * autoWidth < 142 ? 142 : (142 / 849.0) * autoWidth;
                    GridRemarkWidth = (50.0 / 849.0) * autoWidth < 50 ? 50 : (50 / 849.0) * autoWidth;
                    GridSendWidth = (((325.0 / 849.0) * autoWidth < 325 ? 325 : (325.0 / 849.0) * autoWidth) - 60) / 2;
                }
                else
                {
                    GridBondWidth = 125;
                    GridSettWidth = 115;
                    GridTraderWidth = 142;
                    GridRemarkWidth = 50;
                    GridSendWidth = 135;
                }
            }
            else
            {
                ///  345   328  218
                double autoWidth = GridWidth - 20;
                double widthAll1 = 615 + 612 + 543;
                double widthAll2 = 615 + 612;
                double widthAll3 = 615;
                if (autoWidth >= widthAll1)
                {
                    double newautoWidth = GridWidth - 891;
                    GridBondWidthAuto = (155 / 879.0) * newautoWidth < 155 ? 155 : (int)((155 / 879.0) * newautoWidth);
                    GridSettWidthAuto = (115 / 879.0) * newautoWidth < 115 ? 115 : (int)((115 / 879.0) * newautoWidth);
                    GridTraderWidthAuto = (142 / 879.0) * newautoWidth < 142 ? 142 : (int)((142 / 879.0) * newautoWidth);
                    GridSendWidthAuto = (325 / 879.0) * newautoWidth < 325 ? 325 : (int)((325 / 879.0) * newautoWidth);
                    GridSendWidth = (GridSendWidthAuto - 60) / 2;
                    ChildSendWidthAuto = GridSendWidthAuto - 5;
                    //GridWidthAuto1 = (int)((615 / 1770.0) * autoWidth);
                    //GridWidthAuto2 = (int)((612 / 1770.0) * autoWidth);
                    //GridWidthAuto3 = (int)((543 / 1770.0) * autoWidth);
                }
                else if (autoWidth < widthAll1 && widthAll2 < autoWidth)
                {
                    double newautoWidth = GridWidth - 673 - 5;
                    double newautoWidth2 = GridWidth - 225;
                    GridBondWidthAuto = (155 / 554.0) * newautoWidth < 155 ? 155 : (int)((155 / 554.0) * newautoWidth);
                    GridSettWidthAuto = (115 / 554.0) * newautoWidth < 115 ? 115 : (int)((115 / 554.0) * newautoWidth);
                    GridTraderWidthAuto = (142 / 554.0) * newautoWidth < 142 ? 142 : (int)((142 / 554.0) * newautoWidth);
                    GridSendWidthAuto = newautoWidth2 < 325 ? 325 : newautoWidth2;
                    GridSendWidth = (GridSendWidthAuto - 62) / 2;
                    ChildSendWidthAuto = GridSendWidthAuto - 5;
                }
                else if (autoWidth < widthAll2 && widthAll3 < autoWidth)
                {
                    double newautoWidth = GridWidth - 345 - 5;
                    double newautoWidth1 = GridWidth - 328 - 4;
                    double newautoWidth2 = GridWidth - 218;
                    GridBondWidthAuto = (155 / 270.0) * newautoWidth < 155 ? 155 : (155 / 270.0) * newautoWidth;
                    GridSettWidthAuto = (115 / 270.0) * newautoWidth < 115 ? 115 : (115 / 270.0) * newautoWidth;
                    GridTraderWidthAuto = (142 / 284.0) * newautoWidth1 < 142 ? 142 : (142 / 284.0) * newautoWidth1;
                    GridSendWidthAuto = newautoWidth2 - 8 < 325 ? 325 : newautoWidth2 - 8;
                    GridSendWidth = (GridSendWidthAuto - 65) / 2;
                    ChildSendWidthAuto = GridSendWidthAuto - 5;
                }
                else
                {
                    GridBondWidthAuto = 155;
                    GridSettWidthAuto = 115;
                    GridTraderWidthAuto = 142;
                    GridSendWidthAuto = GridWidth - 226;
                    GridSendWidth = 135;
                    ChildSendWidthAuto = 325;
                }
            }

        }


        public async void tb_remarks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string remarks = "";

                TextBox textBox = sender as TextBox;
                if (textBox == null)
                {
                    return;
                }
                remarks = textBox.Text.Trim();
                TransactionOrdermModel orderDetailPo = textBox.DataContext as TransactionOrdermModel;
                if (orderDetailPo == null || orderDetailPo.IssueRemark == remarks)
                {
                    return;
                }

                if (!Check(orderDetailPo))
                {
                    textBox.Text = orderDetailPo.IssueRemark;
                    return;
                }
                ExecutionReport execOrderPo = new ExecutionReport();
                execOrderPo.ExecID = orderDetailPo.ExecID;
                execOrderPo.CommandType = Convert.ToString((int)ECommandType.UpdateSendRemarks);
                execOrderPo.IssueRemark = remarks;
                execOrderPo.CloseById = LocalEnv.UserInfo.UserId;
                //OrderPo orderPo = OrderHandler.Instance.GetOrderById(orderDetailPo.OrderId);
                await UpdateExecOrderData(execOrderPo);
                // OrderHelper.ChangeOrder(OrderHelper.GetUpdateOrderPoNew(remarks, ECommandType.UpdateSendRemarks, orderDetailPo.OrderId, orderDetailPo.ExecId));
            }
        }

        public void SetSeqPosition(string seq)
        {

            LogHelper.Info("搜索单号：" + seq);
            List<TransactionOrdermModel> selectOrders = InnerOrders.Where(delegate (TransactionOrdermModel order)
            {
                if (order.IsSelect)
                    return true;
                else
                    return false;
            }).ToList();

            if (selectOrders != null && selectOrders.Count > 0)
            {
                selectOrders[0].IsSelect = false;
            }

            if (seq.Trim() != "0")
            {
                selectOrders = InnerOrders.Where(delegate (TransactionOrdermModel order)
                {
                    if (!string.IsNullOrWhiteSpace(order.Seq) && order.Seq == seq)
                        return true;
                    else
                        return false;
                }).ToList();

                if (selectOrders != null && selectOrders.Count > 0)
                {
                    int index = -1;
                    foreach (TransactionOrdermModel od in selectOrders)
                    {
                        index++;
                        if (_selectOrder == null)
                        {
                            _selectOrder = od;
                            break;
                        }
                        else
                        {
                            if (_selectOrder.Equals(od))
                            {
                                if (index < (selectOrders.Count - 1))
                                {
                                    _selectOrder = selectOrders[index + 1];
                                    break;
                                }
                                else if (index == (selectOrders.Count - 1))
                                {
                                    _selectOrder = selectOrders[0];
                                    break;
                                }
                            }
                            else
                            {
                                if (index == (selectOrders.Count - 1))
                                {
                                    _selectOrder = selectOrders[0];
                                    break;
                                }
                            }
                        }

                    }

                    _selectOrder.IsSelect = true;
                    _currentDataGrid.ScrollIntoView(_selectOrder);
                }
                else
                    _selectOrder = null;
            }
            else
                _selectOrder = null;
        }


        /// <summary>
        /// 根据execid定位到成交单
        /// </summary>
        /// <param name="id">execid</param>
        public void SetScrollPositionByExecId(string id)
        {
            List<TransactionOrdermModel> selectOrders = InnerOrders.Where(delegate (TransactionOrdermModel order)
            {
                if (order.IsSelect)
                    return true;
                else
                    return false;
            }).ToList();

            if (selectOrders != null && selectOrders.Count > 0)
            {
                selectOrders[0].IsSelect = false;
            }

            if (id + "" != "")
            {
                selectOrders = InnerOrders.Where(delegate (TransactionOrdermModel order)
                {
                    if (order.ExecID == id)
                        return true;
                    else
                        return false;
                }).ToList();

                if (selectOrders != null && selectOrders.Count > 0)
                {
                    foreach (TransactionOrdermModel od in selectOrders)
                    {
                        if (_selectOrder == null)
                        {
                            _selectOrder = od;
                            break;
                        }
                        else if (_selectOrder.Equals(od))
                        {
                            int index = selectOrders.IndexOf(_selectOrder);
                            if (index < (selectOrders.Count - 1))
                            {
                                _selectOrder = selectOrders[index + 1];
                                break;
                            }
                            else if (index == (selectOrders.Count - 1))
                            {
                                _selectOrder = selectOrders[0];
                                break;
                            }
                        }
                        else
                        {
                            _selectOrder = selectOrders[0];
                            break;
                        }
                    }
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        _selectOrder.IsSelect = true;
                        _currentDataGrid.ScrollIntoView(_selectOrder);
                    }));
                }
                else
                    _selectOrder = null;
            }
            else
                _selectOrder = null;
        }


        /// <summary>
        /// 刷新请求队列
        /// </summary>
        private BlockingCollection<int> refreshQueue = new BlockingCollection<int>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private void AtatisticsCount(object o)
        {
            while (true)
            {
                //等待刷新信号
                are.WaitOne();
                refresh();
            };
        }
        /// <summary>
        /// 刷新请求添加到请求队列
        /// </summary>
        public void refresh()
        {
            refreshQueue.Add(1);
        }

        /// <summary>
        /// 刷新订单队列处理
        /// </summary>
        private void DoRefresh()
        {
            while (!refreshQueue.IsCompleted)
            {
                refreshQueue.Take();

                LogHelper.Debug($"清空订单数量: {refreshQueue.Count}");
                // 刷新前清空刷新请求队列
                int item;
                while (refreshQueue.TryTake(out item))
                {
                }
                QuantityUnreadCount();
            }
        }

        /// <summary>
        /// 获取当前选中行
        /// </summary>
        /// <returns></returns>
        public TransactionOrdermModel GetSelectedOrder()
        {
            return _currentDataGrid.SelectedItem as TransactionOrdermModel;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetImageTag(StackPanel spTag, TransactionOrdermModel transaction)
        {

            spTag.Children.Clear();
            if ("0".Equals(transaction.BrokerAStatus))
            {
                Button star = (Button)spTag.FindResource("im_brokerA_OK");
                spTag.Children.Add(star);
            }
            else if ("1".Equals(transaction.BrokerAStatus))
            {
                Image star = (Image)spTag.FindResource("im_brokerA_TBC");
                spTag.Children.Add(star);
            }
            if ("2".Equals(transaction.BrokerAStatus))
            {
                Image oco = (Image)spTag.FindResource("im_brokerA_PartOk");
                spTag.Children.Add(oco);
            }
            else if ("3".Equals(transaction.BrokerAStatus))
            {
                Image oco = (Image)spTag.FindResource("im_brokerA_Refuse");
                spTag.Children.Add(oco);
            }
            else if ("4".Equals(transaction.BrokerAStatus))
            {
                Image oco = (Image)spTag.FindResource("im_brokerA_ROK");
                spTag.Children.Add(oco);
            }
        }

        #endregion
    }
}
