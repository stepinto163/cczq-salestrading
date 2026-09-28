
using com.gateway.log;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Transaction;
using STC.Broker.Plugin.Model.Transaction.BridgeModel;
using STC.Broker.Plugin.Utils;
using STC.Broker.Plugin.View.Component;
using STC.Common.Handler.PT;
using STC.Common.Model.Base;
using STC.Common.Model.CRM;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Utils;
using STC.Common.View.Component;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.Broker.Plugin.View.Transaction
{
    /// <summary>
    /// IdcWindow.xaml 的交互逻辑
    /// 【明细】
    /// </summary>
    public partial class CopyDetailUI : UserControl, INotifyPropertyChanged//WindowBase
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //明细网格控件的数据源
        private ObservableCollection<PositionDetailPo> _data = new ObservableCollection<PositionDetailPo>();
        public ObservableCollection<PositionDetailPo> data = new ObservableCollection<PositionDetailPo>();

        private bool DgOrderIn = false;
        private Dictionary<string, string> dicColor = new Dictionary<string, string>();
        private Dictionary<string, string> dicTradeDate = new Dictionary<string, string>();
        //private string NextDate;
        /// <summary>
        /// 机构汇总的数据
        /// </summary>
        private Dictionary<DetailTrader, List<PositionDetailPo>> dicSum = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        /// <summary>
        /// 机构所有的数据
        /// </summary>
        private Dictionary<DetailTrader, List<PositionDetailPo>> dicData = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        /// <summary>
        /// 机构所有的成交单，包括其他人和无机构分组
        /// </summary>
        private Dictionary<DetailTrader, List<CopyDetailPo>> dicInst = new Dictionary<DetailTrader, List<CopyDetailPo>>();
        /// <summary>
        /// 机构方向是否显示
        /// </summary>
        public Dictionary<string, bool> dicSide = new Dictionary<string, bool>();
        private Dictionary<DetailTrader, List<PositionDetailPo>> dichistSum = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        private Dictionary<DetailTrader, List<PositionDetailPo>> dichistData = new Dictionary<DetailTrader, List<PositionDetailPo>>();
        private Dictionary<DetailTrader, List<CopyDetailPo>> dichistInst = new Dictionary<DetailTrader, List<CopyDetailPo>>();
        public Dictionary<string, bool> dichistSide = new Dictionary<string, bool>();
        private readonly string WinCopyDetail = "CopyDetail";
        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";
        private SearchPo condition = new SearchPo() { SeachDelFlag = "1" };
        private string currentCode = "";
        private string currentInst = "";
        private string currentInstID = "";
        private string currentTrader = "";
        private string currentTraderInst = "";
        private string currentinstTraderName = "";
        private string currentPrice = "";
        private string currentDate = "";
        //当前定位单子的序号
        private int currentOrderSeq = 0;

        private bool _dateIsShow = true;


        /// <summary>
        /// 刷新请求队列
        /// </summary>
        private BlockingCollection<int> refreshQueue = new BlockingCollection<int>();

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 
        /// </summary>
        private List<SearchPo> searchPos = new List<SearchPo>();
        /// <summary>
        /// 查询明细条件列表,用于记忆查询步骤
        /// </summary>
        Dictionary<int, SearchPo> dicSearch = new Dictionary<int, SearchPo>();

        /// <summary>
        /// 是否刷新
        /// </summary>
        private bool _isHasReflash = true;

        /// <summary>
        /// 是否有新订单
        /// </summary>
        private bool _isHasNewOrder = false;

        /// <summary>
        /// 是否点击了button,如果点击了按钮就强制刷新明细
        /// </summary>
        private bool _isClickedButton = false;

        /// <summary>
        /// 记忆索引
        /// </summary>
        private int keyIndex = 0;

        //方向配置
        private readonly string ShowSetUITag = "ShowSetUI";
        //方向配置
        private readonly string DirectionTag = "DirectionTag";
        /// <summary>
        /// 是否展示方向
        /// </summary>
        bool isShowDir = false;

        /// <summary>
        /// 是否需要强制刷新方向
        /// </summary>
        private bool isNeedUpdateDir = false;


        private TextBlockModel textBlockModel;
        private TextBlockModel textBlockModel2;

        public CopyDetailUI()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += InsCloseDetailed_MouseLeftButtonDown;
            this.Loaded += IdcWindow_Loaded;
            this.dgOrder.MouseEnter += DgOrder_MouseEnter;
            this.dgOrder.MouseLeave += DgOrder_MouseLeave;
            //this.Closed += CopyDetail_Closed;
            this.PreviewKeyDown += CopyDetail_KeyDown;
            this.PreviewKeyUp += CopyDetail_PreviewKeyUp;
            //this.dpMaturityDate.BlackoutDates.Add(new CalendarDateRange(new DateTime(), (DateTime.Now.Date).AddDays(-1)));
            this.dgOrder.PreviewMouseLeftButtonDown += DgOrder_PreviewMouseLeftButtonDown;
            //this.Icon = LocalEnv.ServerInfo.AppImage;
            this.dgOrder.AddHandler(DataGridRow.SelectedEvent, new RoutedEventHandler(this.dgOrder_Selected), true);
            this.AddHandler(Window.MouseLeftButtonDownEvent, new MouseButtonEventHandler(this.CopyDetail_MouseLeftButtonDown), true);


            var txb = new TextBlock();
            txb.FontFamily = new FontFamily("楷体");
            textBlockModel = new TextBlockModel
            {
                FontFamily = txb.FontFamily,
                FontStretch = txb.FontStretch,
                FontStyle = txb.FontStyle,
                FontWeight = txb.FontWeight,
                FontSize = 18
            };

            textBlockModel2 = new TextBlockModel
            {
                FontFamily = txb.FontFamily,
                FontStretch = txb.FontStretch,
                FontStyle = txb.FontStyle,
                FontWeight = txb.FontWeight,
                FontSize = 18
            };

            //string height = IniFileHelper.ReadValue(WinCopyDetail, WinHeight);
            //string width = IniFileHelper.ReadValue(WinCopyDetail, WinWidth);


            //从客户端数据文件中读取方向配置默认不展示
            GetDirTag();

            //double d = 0;
            //if (double.TryParse(height, out d))
            //    this.Height = d;
            //if (double.TryParse(width, out d))
            //    this.Width = d;

            //if (LocalEnv.ConfInfo.DetailPosition)
            //{
            //    string positionX = IniFileHelper.ReadValue(WinCopyDetail, PositionX);
            //    string positionY = IniFileHelper.ReadValue(WinCopyDetail, PositionY);
            //    double dY = 0;
            //    if (double.TryParse(positionX, out d) && double.TryParse(positionY, out dY))
            //    {
            //        bool isContain = false;
            //        foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
            //        {
            //            if (sc.WorkingArea.Contains(new System.Drawing.Point((int)d, (int)dY)))
            //            {
            //                isContain = true;
            //                break;
            //            }
            //        }
            //        if (isContain)
            //        {
            //            this.Top = dY;
            //            this.Left = d;
            //        }
            //    }
            //}
            dicSum.Clear();
            CopyDetailHelper.dicSum.Clear();
            dichistSum.Clear();
            CopyDetailHelper.dichistSum.Clear();
            //NextDate = ConvertTextHelper.GetNextDate(NextDate);
            ConvertTextHelper.GetSet();
            //refresh();

            Task.Factory.StartNew(() => { DoRefresh(); }, cancellationTokenSource.Token);

            ThreadPool.QueueUserWorkItem(o =>
            {
                a(o);
            });
            initUserCombox();
            this.PreviewKeyDown += CopyDetail_PreviewKeyDown;
            this.PreviewKeyUp += CopyDetail_PreviewKeyDown;
            CreatSearchPo();

            TransOrderHandler.Instance.OnUpdateOrder2 += Instance_OnUpdateOrder2; ;
        }

        private void Instance_OnUpdateOrder2(TransOrderServerData obj)
        {
            Task.Factory.StartNew(() =>
            {
                this.Dispatcher.Invoke((Action)(async () =>
                {
                    if (this.IsVisible)
                    {
                        this.Addline(obj);
                    }
                }),DispatcherPriority.Normal);
            });
        }

        /// <summary>
        /// 债券类型选择项字符串
        /// </summary>
        public static readonly string BondTypesStr = "国债、国开、进出口、农发、地方债、短融/超短融、中票、企业/公司债、次级债、PPN、其他";
        /// <summary>
        /// 交易场所选择项字符串
        /// </summary>
        public static readonly string TradingPlacesStr = "银行间、上交所、深交所";
        /// <summary>
        /// 成交明细 意向筛选框选择项字符串
        /// </summary>
        public static readonly Dictionary<string, object> OrderTypeDic = new Dictionary<string, object> { { "1", "二级" }, { "0", "意向" } };//{ { "1", "二级" }, { "2", "意向" } };

        public void Addline(TransOrderServerData obj)
        {


            if ((obj.OrdStatus.Equals("G") && !(obj.IsConfirmOperation == "1")) || EDelFlag.删除.EqualsToValue(obj.DelFlag))
            {
                _isHasNewOrder = true;
                if (!_isHasReflash)
                {
                    return;
                }
                isAdd = true;
                are.Set();
            }
            else if (obj.OrdStatus.Equals("G") && obj.IsConfirmOperation == "1")
            {
                foreach (PositionDetailPo pdp in data)
                {
                    if (pdp.ExecId == obj.ExecID)
                    {
                        if (LocalEnv.UserInfo.IsContainsUserId(obj.BrokerUnread))
                        {
                            pdp.Isbrokerunread = string.Format("Isbroker-{0}-{1}", pdp.Seq, 0);
                            pdp.IsBackgColor = true;
                        }
                        else
                        {
                            pdp.Isbrokerunread = string.Format("Isbroker-{0}-{1}", pdp.Seq, 1);
                            pdp.IsBackgColor = false;
                        }
                        //break;
                    }
                }
            }
        }

        private void CopyDetail_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.RightShift || e.Key == Key.LeftShift
                || e.Key == Key.RightCtrl || e.Key == Key.LeftCtrl)
            {
                _combinationKey = false;
                Reflash();
            }
            else
            {
                if (!_combinationKey)
                {
                    if (_listKey.Contains(e.Key))
                    {
                        Reflash();
                    }
                    if (e.Key == Key.Enter)
                    {
                        _isHasReflash = true;
                        //if (_isHasNewOrder) {
                        //    are.Set();
                        //}
                    }
                }
            }
        }

        private void a(object o)
        {
            while (true)
            {
                //等待刷新信号
                are.WaitOne();
                if (_isHasReflash)
                {
                    refresh();
                }
            };
        }


        private void CopyDetail_Closed(object sender, EventArgs e)
        {
            cancellationTokenSource.Cancel();
            are.Close();
            //this.Closed -= CopyDetail_Closed;
            btnClose_Click(null, null);
            if (operation != null)
            {
                operation.Abort();
                operation = null;
            }
        }

        private void DgOrder_MouseLeave(object sender, MouseEventArgs e)
        {
            DgOrderIn = false;
            _isHasReflash = true;
            Reflash();
        }
        private void DgOrder_MouseEnter(object sender, MouseEventArgs e)
        {
            DgOrderIn = true;
        }

        private void InsCloseDetailed_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                //dicColor.Clear();
                //if (!DgOrderIn)
                //    this.DragMove();
                //else
                //{
                //    var property = typeof(DataGrid).GetField(
                // "_isDraggingSelection",
                //     BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
                //    if (property != null)
                //    {
                //        property.SetValue(dgOrder, true);
                //    }
                //    //PositionDetailPo o = dgOrder.SelectedItem as PositionDetailPo;
                //    //if (!dicColor.ContainsKey(o.Seq))
                //    //    dicColor.Add(o.Seq, o.Isbrokerunread);
                //    //o.Isbrokerunread = string.Format("Isbroker-{0}-{1}", o.Seq, 1);
                //}
            }
            catch (Exception ex) { }
        }

        private void IdcWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 键盘事件处理，用于切换显示数据行的提示信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyDetail_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.OriginalSource is TextBox)
                return;
            if (currentRow == null)
                return;
            PositionDetailPo currentPos = currentRow.Item as PositionDetailPo;
            if (currentPos != null && currentPos.DataType == EDataType.数据)
            {
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && (currentPos.goodstype + "" == "1" || currentPos.goodstype + "" == "BCO"))
                {
                    string amount = currentPos.IssueAmount.HasValue ? $"{(currentPos.IssueAmount.Value / 10000).ToString("0.####")} 亿" : "";
                    currentRow.ToolTip = $"发行量：{ amount}，评级：{currentPos.IssuerRatingCurrent }";
                }
                else
                {
                    currentRow.ToolTip = currentPos.ToolText;
                }
            }
        }

        /// <summary>
        /// 初始化下拉框,包括债券类型和交易场所
        /// 场所：
        /// 银行间 Listed_Market = CIB
        /// 上交所 Listed_Market = SSE
        /// 深交所 Listed_Market = SZE
        /// 
        /// 债券类型：
        /// 国债	      BGB，EGB，SGB	
        /// 企业／公司债  CCB，COB，RAB&CEB，LEB，SCV，CVB＆CEB
        /// 短融/超短融   CCP，LCP，SSB，LSP，CSP
        /// 中票          CMN，LMN
        /// 次级债        CSB, NCB, PSB, SES, SPS
        /// 国开          PBB，PSB        Issuer_Code = G000124
        /// 进出口        PBB，PSB        Issuer_Code = Z000189
        /// 农发          PBB，PSB        Issuer_Code = Z000207
        /// 地方债        TLB，LLB
        /// PPN           债券简称中带有PPN字样
        /// 其他          非上述筛选条件内的债券
        /// </summary>
        void initUserCombox()
        {
            //ucBondTypes.DefaultShow = "类型不限";
            //ucBondTypes.Datas.Clear();
            //ucBondTypes.Datas.AddRange(SearchPo.BondTypesStr.Split(new char[] { '、' }));
            //ucBondTypes.DetailReport += UcBondTypes_DetailReport;

            //ucTradingPlaces.DefaultShow = "交易场所不限";
            //ucTradingPlaces.Datas.Clear();
            //ucTradingPlaces.Datas.AddRange(SearchPo.TradingPlacesStr.Split(new char[] { '、' }));
            //ucTradingPlaces.DetailReport += UcTradingPlaces_DetailReport;


            Dictionary<string, object> dic = new Dictionary<string, object>();
            string[] bondTypesStr = BondTypesStr.Split(new char[] { '、' });
            foreach (string s in bondTypesStr)
            {
                dic[s] = s;
            }
            this.ucBondTypes.ItemsSource = dic;

            Dictionary<string, object> dicTP = new Dictionary<string, object>();
            string[] tradingPlaces = TradingPlacesStr.Split(new char[] { '、' });
            foreach (string s in tradingPlaces)
            {
                dicTP[s] = s;
            }
            this.ucTradingPlaces.ItemsSource = dicTP;
            //添加broker列表
            if (SystemPowerHandler.Instance.IsBridge)
            {
                List<BrokerInfoPo> dataTable = Utils.CommonUtils.GetBrokerByTermids(0);
                if (dataTable != null && dataTable.Count > 0)
                {
                    Dictionary<string, object> dicB = new Dictionary<string, object>();
                    foreach (BrokerInfoPo item in dataTable)
                    {
                        dicB[item.AccountName] = item.AccountName;
                        CopyDetailHelper.CurrentUserBrokerId.Add(item.Idb_key);
                    }
                    this.uc_Broker.ItemsSource = dicB;
                }
            }
            else
            {
                List<UserRoleSettingDetail> queryOrders = new List<UserRoleSettingDetail>();
                UserRoleSettingDetail accountInfo = new UserRoleSettingDetail()
                {
                    userDisplayName = LocalEnv.UserInfo.FullName,
                    userName = LocalEnv.UserInfo.UserName,
                    userId = LocalEnv.UserInfo.UserId,
                };
                queryOrders.Add(accountInfo);
                CopyDetailHelper.CurrentUserBrokerId.Add(LocalEnv.UserInfo.UserId);

                if (GRSettingsHandler.PowerSettingModel.QueryOrders != null && GRSettingsHandler.PowerSettingModel.QueryOrders.Count > 0)
                {

                    List<UserRoleSettingDetail> accountInfos = GRSettingsHandler.PowerSettingModel.QueryOrders.FirstOrDefault(_ => _.userId == LocalEnv.UserInfo.UserId)?.relatedUsers;

                    if (accountInfos != null && accountInfos.Count > 0)
                    {
                        foreach (UserRoleSettingDetail item in accountInfos)
                        {
                            //uc_Broker.Datas.Add(item["AccountName"].ToString());
                            if (!queryOrders.Any(_ => _.userId == item.userId))
                            {
                                queryOrders.Add(item);
                                CopyDetailHelper.CurrentUserBrokerId.Add(item.userId);
                            }
                        }
                    }
                }

                if (GRSettingsHandler.PowerSettingModel.UpdateOrders != null && GRSettingsHandler.PowerSettingModel.UpdateOrders.Count > 0)
                {
                    List<UserRoleSettingDetail> accountInfos = GRSettingsHandler.PowerSettingModel.UpdateOrders.Where(_ => _.userId == LocalEnv.UserInfo.UserId).FirstOrDefault()?.relatedUsers;
                    if (accountInfos != null && accountInfos.Count > 0)
                    {
                        foreach (UserRoleSettingDetail item in accountInfos)
                        {
                            //uc_Broker.Datas.Add(item["AccountName"].ToString());
                            if (!queryOrders.Any(_ => _.userId == item.userId))
                            {
                                queryOrders.Add(item);
                                CopyDetailHelper.CurrentUserBrokerId.Add(item.userId);
                            }
                        }
                    }
                }
                Dictionary<string, object> dicB = new Dictionary<string, object>();
                foreach (UserRoleSettingDetail item in queryOrders)
                {
                    dicB[item.userDisplayName] = item.userDisplayName;
                }
                this.uc_Broker.ItemsSource = dicB;
            }

            this.ucflagInner.ItemsSource = OrderTypeDic;
        }

        private void uc_Broker_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            string[] selectedValues = this.uc_Broker.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<string> selectedList = new List<string>(selectedValues);
            condition.brokerNames = selectedList;
            //condition.brokerNames = uc_Broker.SelectedTexts;
            _isClickedButton = true;
            refresh();
            CreatSearchPo();
        }
        //private void Uc_Broker_DetailReport(object sender, RoutedEventArgs e)
        //{
        //    //throw new NotImplementedException();
        //    //UserComBoxSelectChangeEventArgs dre = e as UserComBoxSelectChangeEventArgs;
        //    //condition.brokerNames = dre.Selects;
        //    _isClickedButton = true;
        //    refresh();
        //    CreatSearchPo();
        //}

        /// <summary>
        /// 订单类型选中更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void UcflagInner_DetailReport(object sender, RoutedEventArgs e)
        //{
        //    //UserComBoxSelectChangeEventArgs dre = e as UserComBoxSelectChangeEventArgs;
        //    //condition.OrderTypes = dre.Selects;
        //    _isClickedButton = true;
        //    refresh();
        //    CreatSearchPo();
        //}
        private void ucflagInner_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            string[] selectedValues = this.ucflagInner.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<string> selectedList = new List<string>(selectedValues);
            condition.OrderTypes = selectedList;

            _isClickedButton = true;
            refresh();
            CreatSearchPo();
        }
        /// <summary>
        /// 债券类型选中更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void UcBondTypes_DetailReport(object sender, RoutedEventArgs e)
        //{
        //    //UserComBoxSelectChangeEventArgs dre = e as UserComBoxSelectChangeEventArgs;
        //    //condition.BondTypes = dre.Selects;
        //    _isClickedButton = true;
        //    refresh();
        //    CreatSearchPo();
        //}
        private void ucBondTypes_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            string[] selectedValues = this.ucBondTypes.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<string> selectedList = new List<string>(selectedValues);
            condition.BondTypes = selectedList;
            _isClickedButton = true;
            refresh();
            CreatSearchPo();
        }
        /// <summary>
        /// 交易场所选中更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void UcTradingPlaces_DetailReport(object sender, RoutedEventArgs e)
        //{
        //    //UserComBoxSelectChangeEventArgs dre = e as UserComBoxSelectChangeEventArgs;
        //    //condition.TradingPlaces = dre.Selects;
        //    _isClickedButton = true;
        //    refresh();
        //    CreatSearchPo();
        //}
        private void ucTradingPlaces_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            string[] selectedValues = this.ucTradingPlaces.SelectedValues.Split(STC.Common.Utils.CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
            List<string> selectedList = new List<string>(selectedValues);
            condition.TradingPlaces = selectedList;
            _isClickedButton = true;
            refresh();
            CreatSearchPo();
        }
        AutoResetEvent are = new AutoResetEvent(true);
        //public void Addline(OrderPo obj)
        //{
        //    //if (obj.execOrders != null && obj.execOrders.Any(o => o.orderStatus.Equals("G") || "1".Equals(o.delflag)))
        //    if (obj.execOrders != null && obj.execOrders.Any(o => (o.orderStatus.Equals("G") && !(o.isConfirmOperation == 1))
        //    || EDelFlag.删除.EqualsToValue(o.delflag)))
        //    {
        //        _isHasNewOrder = true;
        //        if (!_isHasReflash)
        //        {
        //            return;
        //        }
        //        isAdd = true;
        //        are.Set();
        //    }
        //    else if (obj.execOrders != null && obj.execOrders.Any(o => o.orderStatus.Equals("G") && o.isConfirmOperation == 1))
        //    {
        //        foreach (CopyDetailPo item in obj.execOrders)
        //        {
        //            if (item.isConfirmOperation == 1)
        //            {
        //                foreach (PositionDetailPo pdp in data)
        //                {
        //                    if (pdp.ExecId == item.execId)
        //                    {
        //                        if (LocalEnv.UserInfo.IsContainsUserId(item.brokerunread))
        //                        {
        //                            pdp.Isbrokerunread = string.Format("Isbroker-{0}-{1}", item.seq, 0);
        //                            pdp.IsBackgColor = true;
        //                        }
        //                        else
        //                        {
        //                            pdp.Isbrokerunread = string.Format("Isbroker-{0}-{1}", item.seq, 1);
        //                            pdp.IsBackgColor = false;
        //                        }
        //                        //break;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// 更新一键查看
        /// </summary>
        //public void UpdateLookOverOrder(OrderPo obj)
        //{
        //    foreach (CopyDetailPo item in obj.execOrders)
        //    {
        //        if (item.isConfirmOperation == 1)
        //        {
        //            foreach (PositionDetailPo pdp in data)
        //            {
        //                if (pdp.ExecId == item.execId)
        //                {
        //                    if (LocalEnv.UserInfo.IsContainsUserId(item.brokerunread))
        //                    {
        //                        pdp.Isbrokerunread = string.Format("Isbroker-{0}-{1}", item.seq, 0);
        //                        pdp.IsBackgColor = true;
        //                    }
        //                    else
        //                    {
        //                        pdp.Isbrokerunread = string.Format("Isbroker-{0}-{1}", item.seq, 1);
        //                        pdp.IsBackgColor = false;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        bool isAdd = false;
        DispatcherOperation operation = null;

        /// <summary>
        /// 刷新明细队列处理
        /// </summary>
        private void DoRefresh()
        {
            while (!refreshQueue.IsCompleted)
            {
                refreshQueue.Take();

                LogHelper.Debug($"清空重复刷新数量: {refreshQueue.Count}");
                // 刷新前清空刷新请求队列
                int item;
                while (refreshQueue.TryTake(out item))
                {
                }

                refresh(false, false);
            }
        }

        /// <summary>
        /// 刷新请求添加到请求队列
        /// </summary>
        public void refresh()
        {
            refreshQueue.Add(1);
        }

        /// <summary>
        /// 刷新明细界面
        /// </summary>
        /// <param name="isRefreshAll"></param>
        /// <param name="isAsync">是否使用同步。默认为异步</param>
        public void refresh(bool isRefreshAll, bool isAsync = true)
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                dicData.Clear();
                dicInst.Clear();

                dichistData.Clear();
                dichistInst.Clear();
                //清空汇总数据values 重新加载 不然的话删除的数据会一直保留在dicSum里面
                foreach (var item in dicSum.ToList())
                {
                    dicSum[item.Key].Clear();
                }
                foreach (var item in dichistSum.ToList())
                {
                    dichistSum[item.Key].Clear();
                }
                //dgOrder.DataContext = null;
                dtDetail.Columns.Clear();
                dtDetail.Clear();

                var templist = TransOrderHandler.Instance.GetAllOrder(false).Values.ToList();
                //今天的成交单
                //decimal today = DateTimeEx.GetTimeStamp(DateTime.Now.Date);
                string today = DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var list = from m in templist
                           where (m.TransactTime.CompareTo(today) >= 0)
                           select m;
                var tlist = list.GroupBy(c => c.Sequence).Select(c => c.First());//去重
                //如果点击了按钮就强制刷新
                if (!_isClickedButton)
                {
                    if (!_isHasReflash)
                    {
                        return;
                    }
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    var newData = CopyDetailHandler.Instance.GetDetail(tlist.ToList(), true, condition, isShowDir, isRefreshAll, textBlockModel, textBlockModel2);
                    //var newData = CopyDetailHelper.GetDetail(tlist.ToList(), true, condition, isShowDir, isRefreshAll, textBlockModel, textBlockModel2);
                    if (condition.IsMaturityOrder)
                    {
                        //newData= newData.OrderBy(_=>_.Tradera).ThenBy(_=>_.mu)
                    }
                    //data.Clear();
                    //data = new ObservableCollection<PositionDetailPo>();

                    bool isHistory = condition.IsShowHist;

                    //Dispatcher.Invoke(new Action(() =>
                    //{
                    //    isHistory = cbHist.IsChecked.HasValue && cbHist.IsChecked.Value;
                    //}));

                    //今日发单的历史成交单
                    if (isHistory)
                    {
                        var hlist = from m in templist
                                    where (m.TransactTime.CompareTo(today) < 0)
                                    select m;
                        List<PositionDetailPo> histlist = CopyDetailHandler.Instance.GetHistDetail(hlist.ToList(), true, condition, isShowDir, isRefreshAll, textBlockModel, textBlockModel2);
                        histlist.ForEach(p => newData.Add(p));
                    }

                    Stopwatch sw = Stopwatch.StartNew();

                    dgOrder.DataContext = null;

                    data.Clear();
                    data = new ObservableCollection<PositionDetailPo>(newData);
                    dgOrder.DataContext = data;//dtDetail;
                    //newData.ForEach(p => data.Add(p));

                    LogHelper.Debug($"刷新明细界面，耗时={sw.ElapsedMilliseconds}");


                    dicInst = CopyDetailHelper.dicInst;
                    dicSum = CopyDetailHelper.dicSum;
                    dicData = CopyDetailHelper.dicData;

                    dichistInst = CopyDetailHelper.dichistInst;
                    dichistSum = CopyDetailHelper.dichistSum;
                    dichistData = CopyDetailHelper.dichistData;

                    //机构方向是否显示
                    dicSide = CopyDetailHelper.dicSide;
                    dichistSide = CopyDetailHelper.dichistSide;

                    _isHasNewOrder = false;
                    _isClickedButton = false;
                    RefreshCheck();
                }), DispatcherPriority.Background);

                //每次刷新，重新过滤单子
                tbOrderSeq_TextChanged(null, null);


                LogHelper.Debug($"明细总耗时={stopwatch.ElapsedMilliseconds}");
            }
            catch (Exception e)
            {
                LogHelper.Error($"明细刷新错误", e);
            }

        }

        #region 重新刷汇总，方向check框
        private void RefreshCheck()
        {
            foreach (DetailTrader key in dicData.Keys)
            {
                if (dicData.ContainsKey(key))
                {
                    if (dicSum.ContainsKey(key))
                    {
                        if (dicData[key].Count > 0)
                            dicData[key][0].IsCheckSum = true;
                    }
                    else
                    {
                        if (dicData[key].Count > 0)
                            dicData[key][0].IsCheckSum = false;
                    }
                }
                string instAndTrader = key.Key;
                if (dicSide.ContainsKey(instAndTrader))
                {
                    if (dicData.ContainsKey(key) && dicData[key].Count > 0)
                        dicData[key][0].IsCheckSide = dicSide[instAndTrader];
                }
            }
            foreach (DetailTrader key in dichistData.Keys)
            {
                if (dichistData.ContainsKey(key))
                {
                    if (dichistSum.ContainsKey(key))
                    {
                        if (dichistData[key].Count > 0)
                            dichistData[key][0].IsCheckSum = true;
                    }
                    else
                    {
                        if (dichistData[key].Count > 0)
                            dichistData[key][0].IsCheckSum = false;
                    }
                }
                string instAndTrader = key.Key;
                if (dichistSide.ContainsKey(instAndTrader))
                {
                    if (dichistData.ContainsKey(key) && dichistData[key].Count > 0 && dichistSide.ContainsKey(instAndTrader))
                        dichistData[key][0].IsCheckSide = dichistSide[instAndTrader];
                }
            }
        }
        #endregion



        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            //BrokerCEDAServer.Instance.OnSendQQMsgFlage -= Instance_OnSendQQMsgFlage;

            //IniFileHelper.WriteValue(WinCopyDetail, WinHeight, this.Height + "");
            //IniFileHelper.WriteValue(WinCopyDetail, WinWidth, this.Width + "");

            //if (LocalEnv.ConfInfo.DetailPosition)
            //{
            //    IniFileHelper.WriteValue(WinCopyDetail, PositionX, this.Left + "");
            //    IniFileHelper.WriteValue(WinCopyDetail, PositionY, this.Top + "");
            //}
            //  this.Close();
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                ShowSetUI ui = new ShowSetUI();
                ui.dicChange.Clear();
                bool? b = ui.ShowDialog();
                if (b == true)
                {
                    Dictionary<DetailTrader, List<PositionDetailPo>> d2 = new Dictionary<DetailTrader, List<PositionDetailPo>>(dicSum);
                    foreach (KeyValuePair<string, UserRoleSettingDetail> kv in ui.dicChange)
                    {
                        foreach (KeyValuePair<DetailTrader, List<PositionDetailPo>> su in d2)
                        {
                            //if (dicSum.ContainsKey(kv.Value.Name) || su.Key.Contains(kv.Value.Name))
                            //    dicSum.Remove(su.Key);
                            if (su.Key.Key.Contains(kv.Value.userDisplayName))
                                dicSum.Remove(su.Key);
                        }
                    }
                    ConvertTextHelper.GetSet();

                    //从客户端数据文件中读取方向配置默认不展示
                    GetDirTag();

                    refresh(true);
                    isNeedUpdateDir = true;

                    ui.dicChange.Clear();
                }
            });
        }

        #region 汇总

        private void btSum_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            string key = "";
            PositionDetailPo f = null;
            DataRowView a = this.dgOrder.SelectedItem as DataRowView;
            if (a == null)
                f = this.dgOrder.SelectedItem as PositionDetailPo;
            else
            {
                DataRow b = a.Row;
                f = CopyDetailHelper.D2E<PositionDetailPo>(b);
            }
            if (f == null)
            {
                LogHelper.Error("slected item is null");
                return;
            }
            key = f.Key;
            DetailTrader detailTrader = f.DetailTrader;
            ///获取汇总数据的插入位置
            int size = 0;
            if (f.IsHist)
            {
                foreach (DetailTrader s in dicData.Keys)
                {
                    size += dicData[s].Count;
                }
                foreach (DetailTrader s in dichistData.Keys)
                {
                    if (s.Key.Equals(key))
                    {
                        if (dichistInst.ContainsKey(s))
                        {
                            size += dichistInst[s].Count + 1;
                            break;
                        }
                    }
                    else
                        size += dichistData[s].Count;
                }
                SumHist(detailTrader, size);
            }
            else
            {
                foreach (DetailTrader s in dicData.Keys)
                {
                    if (s.Key.Equals(key))
                    {
                        if (dicInst.ContainsKey(s))
                        {
                            size += dicInst[s].Count + 1;
                            break;
                        }
                    }
                    else
                        size += dicData[s].Count;
                }
                Sum(detailTrader, size);
                //if (dicSum.ContainsKey(detailTrader))
                //{
                //    if (dicData.ContainsKey(detailTrader))
                //        dicData[detailTrader][0].IsCheckSum = true;
                //}
                //else
                //{
                //    if (dicData.ContainsKey(detailTrader))
                //        dicData[detailTrader][0].IsCheckSum = false;
                //}

            }
        }


        /// <summary>
        /// 从客户端数据文件中读取方向配置默认不展示
        /// </summary>
        private void GetDirTag()
        {
            //从客户端数据文件中读取方向配置默认不展示
            string directionTagStr = IniFileHelper.ReadValue(string.Format("{0}_{1}", ShowSetUITag, LocalEnv.UserInfo.UserId), DirectionTag);
            if (string.IsNullOrWhiteSpace(directionTagStr))
            {
                isShowDir = false;
            }
            else
            {
                if (directionTagStr == "1")
                {
                    isShowDir = true;
                }
                else
                {
                    isShowDir = false;
                }
            }
        }

        /// <summary>
        /// 求某个机构的汇总信息
        /// </summary>
        /// <param name="key">机构</param>
        /// <param name="index">机构在成交单的末尾位置 </param>
        private void Sum(DetailTrader key, int index)
        {
            ///如果汇总已经显示，则删除汇总数据
            if (dicSum.ContainsKey(key))
            {
                dicSum.Remove(key);
                // PositionDetailPo o = dicData[key][dicData[key].Count - 1];
                //获取该机构所有汇总数据
                List<PositionDetailPo> templist = dicData[key].Where(o => o.IsSum).ToList();

                foreach (PositionDetailPo s in templist)
                {
                    dicData[key].Remove(s);
                    data.Remove(s);

                }

                //dicSum.Add(key, dicData[key]);
                //dgOrder.DataContext = null;
                //dgOrder.DataContext = data;
                return;
            }

            var pos = CopyDetailHelper.GetSumPositionDetailPo(dicInst, key);
            foreach (var po in pos)
            {
                if (index < data.Count)
                {
                    data.Insert(index, po);
                }
                else
                {
                    data.Add(po);
                }
                index++;
                //一条汇总数据加入机构
                dicData[key].Add(po);
            }
            //将该机构的所有汇总加入汇总列表
            dicSum.Add(key, dicData[key]);
            //dgOrder.DataContext = null;
            //dgOrder.DataContext = data;
        }



        private void SumHist(DetailTrader key, int index)
        {
            if (dichistSum.ContainsKey(key))
            {
                dichistSum.Remove(key);
                // PositionDetailPo o = dicData[key][dicData[key].Count - 1];
                List<PositionDetailPo> templist = dichistData[key].Where(o => o.IsSum).ToList();
                foreach (PositionDetailPo s in templist)
                {
                    dichistData[key].Remove(s);
                    data.Remove(s);
                }
                //dicSum.Add(key, dicData[key]);
                dgOrder.DataContext = null;
                dgOrder.DataContext = data;
                return;
            }

            var pos = CopyDetailHelper.GetSumPositionDetailPo(dichistInst, key);
            foreach (var po in pos)
            {
                if (index < data.Count)
                {
                    data.Insert(index, po);
                }
                else
                {
                    data.Add(po);
                }
                index++;
                dichistData[key].Add(po);
            }

            dichistSum.Add(key, dichistData[key]);
            dgOrder.DataContext = null;
            dgOrder.DataContext = data;
        }
        #endregion

        /// <summary>
        /// 键盘按键集合
        /// </summary>
        private HashSet<Key> _listKey = new HashSet<Key>() {
            Key.LeftShift,Key.RightShift,Key.LeftCtrl,Key.RightCtrl,
            Key.Down,Key.Up,Key.Left,Key.Right
        };
        private bool _combinationKey = false;
        private void CopyDetail_KeyDown(object sender, KeyEventArgs e)
        {
            LogHelper.Info("按下键盘:" + e.Key.ToString());
            if (e.Key == Key.Escape)//Esc键  
            {
                //  this.Close();
            }
            //加入键盘事件,明细有操作时不刷新
            if (_listKey.Contains(e.Key))
            {
                if (e.Key == Key.LeftShift || e.Key == Key.RightShift
                    || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                {
                    if (DgOrderIn)
                    {
                        _combinationKey = true;
                        _isHasReflash = false;
                    }
                    else
                    {
                        _isHasReflash = true;
                    }
                }
            }
        }

        //private void CommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        //{
        //    e.CanExecute = false;
        //    e.Handled = true;
        //}
        public void Clear()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (dicTradeDate != null)
                    dicTradeDate.Clear();
                //NextDate = "";
                dicInst.Clear();
                data.Clear();
                dicData.Clear();
                dgOrder.DataContext = null;
                //dpMaturityDate.Text = null;
            });
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isHasReflash = false;
                CopyExDataGrid.CopyType = ECopyType.带单号; //CopyExDataGrid.CopyType = 1;
                dgOrder.OnExecutedCopy(null);
                CopyExDataGrid.CopyType = ECopyType.Ctrl_C;  //CopyExDataGrid.CopyType = 0;
            }
            catch (Exception ex)
            {
                LogHelper.Error("复制订单失败:" + ex.ToString());
            }
            finally
            {
                _isHasReflash = true;
                Reflash();
            }
        }

        public readonly DataTable dtDetail = new DataTable("Detail");

        private void cbStar_Click(object sender, RoutedEventArgs e)
        {
            if (cbStar.IsChecked.Value)
                condition.IsSort = true;
            else
                condition.IsSort = false;
            _isClickedButton = true;
            refresh();
            CreatSearchPo();
        }

        #region 债券
        private void tbCode_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if(!isCodekey)
            //{
            if (!tbBCodePop.IsOpen)
                dgCodeResult_MouseDoubleClick(null, null);
            //}
            //isCodekey = false;
        }
        //bool isCodekey = false;
        private void tbCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _isClickedButton = true;
                //btSpare.Focus();
                e.Handled = false;
                if (tbBCodePop.IsOpen)
                    dgCodeResult_MouseDoubleClick(null, null);
                else
                {
                    condition.BondCode = currentCode = "";
                    condition.BondName = "";
                    refresh();
                    CreatSearchPo();
                }
            }
        }
        /// <summary>
        /// 券码更改的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbCode.Text))
            {
                string param = tbCode.Text.Replace("'", "");

                dgCodeResult.DataContext = STC.Common.Utils.CommonUtils.GetBondGridData(param, 20);
                //CRMService.Instance.GetBondGridData(param, 20);//CRMDataHandler.Instance.SearchBond2(param);
                dgCodeResult.SelectedIndex = 0;
                tbBCodePop.IsOpen = true;
            }
            else
            {
                tbBCodePop.IsOpen = false;
                condition.BondCode = "";
            }
        }

        private void dgCodeResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SimpleBondsInfoPo dv = dgCodeResult.SelectedItem as SimpleBondsInfoPo;
            if (dv != null && !string.IsNullOrEmpty(tbCode.Text))
            {
                tbCode.Text = dv.Short_Name;
                tbBCodePop.IsOpen = false;
                string code = dv.Bond_ID.Replace(".IB", "");
                if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(currentCode))
                    return;
                condition.BondCode = code;
                condition.BondName = dv.Short_Name;
                _isClickedButton = true;
                refresh();
                currentCode = code;
                CreatSearchPo();
            }
        }
        private void tbBCodePop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbCode.Text))
                tbBCodePop.IsOpen = false;
        }

        private void tbCode_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgCodeResult.SelectedIndex > 0)
                    dgCodeResult.SelectedIndex--;
                dgCodeResult.ScrollIntoView(dgCodeResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgCodeResult.SelectedIndex < dgCodeResult.Items.Count - 1)
                    dgCodeResult.SelectedIndex++;
                dgCodeResult.ScrollIntoView(dgCodeResult.SelectedItem);
            }
        }
        #endregion

        #region 机构
        //bool isInstkey = false;
        private void tbInst_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!tbInstPop.IsOpen)
            {
                dgInstResult_MouseDoubleClick(null, null);
            }
        }
        private void tbTrader_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!tbTraderPop.IsOpen)
            {
                dgTraderResult_MouseDoubleClick(null, null);
            }
        }

        /// <summary>
        /// 自动勾选 历史 如需刷新，则刷新
        /// </summary>
        /// <param name="needRefresh"></param>
        /// <returns></returns>
        private async Task AutoCheckHistory(bool needRefresh = false)
        {
            if (!cbHist.IsChecked.HasValue || !cbHist.IsChecked.Value)
            {
                cbHist.IsChecked = true;
            }
            else
            {
                needRefresh = false;
            }
            condition.IsShowHist = true;

            if (needRefresh)
            {
                await Task.Run(() => cbHist_Click(null, null));
            }
        }

        private async void tbInst_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AutoCheckHistory();

                //btSpare.Focus();
                e.Handled = false;
                dgInstResult_MouseDoubleClick(null, null);
                //isInstkey = true;
            }
        }
        private async void tbTrader_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AutoCheckHistory();

                //btSpare.Focus();
                e.Handled = false;
                dgTraderResult_MouseDoubleClick(null, null);
                //isInstkey = true;
            }
        }
        //同同时支持交易员和机构搜索
        //20200615 V4.4.0  机构交易员分开搜索，互不影响
        private void tbInst_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tbInst.Text))
            {
                string param = tbInst.Text.Replace("'", "");

                dgInstResult.DataContext = Utils.CommonUtils.GetInstitutionsTipsNameInfo(param, 20);
                //CRMService.Instance.GetInstByNameTerm(param, 20);
                dgInstResult.SelectedIndex = 0;
                tbInstPop.IsOpen = true;
            }
            else
            {
                tbInstPop.IsOpen = false;
                condition.Inst = "";
                condition.InstID = "";
            }
        }
        private void tbTrader_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!string.IsNullOrEmpty(tbTrader.Text))
            {
                List<DataGridDataPo> dataList = Utils.CommonUtils.GetContactByContactName(tbTrader.Text, 100);
                //CRMService.Instance.GetContactByContactName(tbTrader.Text, 100);
                if (dataList == null || dataList.Count <= 0)
                {
                    dataList = Utils.CommonUtils.GetContactByInstName(tbTrader.Text, 100, string.Empty);
                    //CRMService.Instance.GetContactByInstName(tbTrader.Text, 100, string.Empty);
                }
                if (dataList.Count > 0)
                {
                    dgTraderResult.DataContext = dataList;
                    dgTraderResult.SelectedIndex = 0;
                    if (!tbTraderPop.IsOpen)
                        tbTraderPop.IsOpen = true;
                }
                else
                {
                    dgTraderResult.DataContext = null;
                    tbTraderPop.IsOpen = false;
                }
            }
            else
            {
                dgTraderResult.DataContext = null;
                tbTraderPop.IsOpen = false;

                condition.Trader = "";
                condition.TraderInst = "";
            }
        }
        List<DataGridDataPo> ListMerge(List<DataGridDataPo> l1, List<DataGridDataPo> l2)
        {
            List<DataGridDataPo> l = new List<DataGridDataPo>();
            if (l1 != null)
            {
                foreach (var a in l1)
                {
                    l.Add(a);
                }
            }
            if (l2 != null)
            {
                foreach (var a in l2)
                {
                    l.Add(a);
                }
            }
            return l;
        }
        private void tbInstPop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbInst.Text))
                tbInstPop.IsOpen = false;
        }
        private void tbTraderPop_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbTrader.Text))
                tbTraderPop.IsOpen = false;
        }
        private async void dgInstResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string inst = string.Empty;
            string instID = string.Empty;
            //DataGridDataPo dv = dgInstResult.SelectedItem as DataGridDataPo;
            CrmInst dv = dgInstResult.SelectedItem as CrmInst;
            if (dv != null && !string.IsNullOrWhiteSpace(tbInst.Text))
            {
                tbInst.Text = dv.TipsShortnamelist;//.InstName;
                tbInst.Tag = dv.idbkey;//.InstIdbKey;
                tbInstPop.IsOpen = false;
                inst = dv.TipsShortnamelist;//.InstName;
                instID = dv.idbkey;//.InstIdbKey;
            }
            if (sender == null && dv == null && !string.IsNullOrWhiteSpace(tbInst.Text))
            {
                condition.Inst = currentInst;
                condition.InstID = currentInstID;
            }
            else
            {
                condition.Inst = inst;
                condition.InstID = instID;
            }

            await AutoCheckHistory();

            _isClickedButton = true;
            refresh();
            currentInst = condition.Inst;
            currentInstID = condition.InstID;
            CreatSearchPo();
        }
        private async void dgTraderResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string trader = string.Empty;
            string traderInst = string.Empty;
            string instTraderName = string.Empty;
            DataGridDataPo dv = dgTraderResult.SelectedItem as DataGridDataPo;
            if (dv != null && !string.IsNullOrWhiteSpace(tbTrader.Text))
            {
                tbTrader.Text = dv.InstTraderName;
                tbTraderPop.IsOpen = false;
                trader = dv.TraderName;
                traderInst = dv.InstName;
                instTraderName = dv.InstTraderName;
            }
            if (sender == null && dv == null && !string.IsNullOrWhiteSpace(tbTrader.Text))
            {
                condition.Trader = currentTrader;
                condition.TraderInst = currentTraderInst;
                condition.InstTraderName = currentinstTraderName;
            }
            else
            {
                condition.Trader = trader;
                condition.TraderInst = traderInst;
                condition.InstTraderName = instTraderName;
            }
            _isClickedButton = true;

            await AutoCheckHistory();

            refresh();
            currentTrader = condition.Trader;
            currentTraderInst = condition.TraderInst;
            currentinstTraderName = condition.InstTraderName;
            CreatSearchPo();
        }
        private void tbInst_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgInstResult.SelectedIndex > 0)
                    dgInstResult.SelectedIndex--;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgInstResult.SelectedIndex < dgInstResult.Items.Count - 1)
                    dgInstResult.SelectedIndex++;
                dgInstResult.ScrollIntoView(dgInstResult.SelectedItem);
            }
        }
        private void tbTrader_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                if (dgTraderResult.SelectedIndex > 0)
                    dgTraderResult.SelectedIndex--;
                dgTraderResult.ScrollIntoView(dgTraderResult.SelectedItem);
            }
            else if (e.Key == Key.Down)
            {
                if (dgTraderResult.SelectedIndex < dgTraderResult.Items.Count - 1)
                    dgTraderResult.SelectedIndex++;
                dgTraderResult.ScrollIntoView(dgTraderResult.SelectedItem);
            }
        }
        #endregion

        private void tbPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btSpare.Focus();
            }
        }

        private void tbPrice_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            string price = tbPrice.Text;
            if (!(string.IsNullOrEmpty(price) && string.IsNullOrEmpty(currentPrice)))
            {
                decimal temp = decimal.Zero;
                decimal.TryParse(price, out temp);
                condition.Priceformat = price;
                condition.Price = temp;
                _isClickedButton = true;
                refresh();
                currentPrice = tbPrice.Text;
                CreatSearchPo();
            }
        }
        private const string DateFormat = "yyyyMMdd";
        private void dpMaturityDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isShowAll || !_dateIsShow)
                return;
            string trdate = dpMaturityDate.SelectedDate == null ? "" : dpMaturityDate.SelectedDate.Value.ToString(DateFormat);
            if (string.IsNullOrEmpty(trdate) && string.IsNullOrEmpty(currentDate))
                return;
            condition.TradeDate = trdate;
            //if (dpMaturityDate.SelectedDate!) { 
            //}
            condition.TradeDateDisplay = dpMaturityDate.SelectedDate == null ? "" : dpMaturityDate.SelectedDate.Value.ToString("yyyy-MM-dd");
            _isClickedButton = true;
            refresh();
            currentDate = trdate;
            CreatSearchPo();
            // searchPo.Maturitydate = 
        }

        #region ShowAll
        bool isShowAll = false;
        /// <summary>
        /// 显示全部按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowAll_Click(object sender, RoutedEventArgs e)
        {
            isShowAll = true;
            condition = null;
            ShowAll(true);
            cbHist.IsChecked = false;
            cbMaturityOrder.IsChecked = false;
            _isClickedButton = true;
            refresh();
            isShowAll = false;
            condition = new SearchPo() { SeachDelFlag = "1" };
            CreatSearchPo();

        }
        private void ShowAll(bool isEnabled)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                _dateIsShow = false;
                cbStar.IsChecked = false;
                cbNeedBridge.IsChecked = false;
                //cbNoLookDel.IsChecked = true;
                tbCode.Text = "";
                tbInst.Text = "";
                tbTrader.Text = "";
                tbPrice.Text = "";
                dpMaturityDate.Text = "";
                _dateIsShow = true;
                tbOrderSeq.Text = "";
                cbStar.IsEnabled = isEnabled;
                tbCode.IsEnabled = isEnabled;
                tbInst.IsEnabled = isEnabled;
                tbTrader.IsEnabled = isEnabled;
                tbPrice.IsEnabled = isEnabled;
                dpMaturityDate.IsHitTestVisible = isEnabled;
                tbOrderSeq.IsEnabled = isEnabled;
                ucBondTypes.ClearSelection();//.ClearSelectedItem();
                ucTradingPlaces.ClearSelection();//.ClearSelectedItem();
                ucflagInner.ClearSelection();//.ClearSelectedItem();
                uc_Broker.ClearSelection();//.ClearSelectedItem();
            }));
        }

        /// <summary>
        /// 创建记忆动作缓存
        /// </summary>
        private void CreatSearchPo()
        {

            if (condition != null)
            {
                SearchPo searchPo = new SearchPo();
                searchPo.SeachBridgeFlag = condition.SeachBridgeFlag;
                searchPo.SeachDelFlag = condition.SeachDelFlag;
                searchPo.Seq = condition.Seq;
                searchPo.BondCode = condition.BondCode;
                searchPo.BondName = condition.BondName;
                searchPo.Inst = condition.Inst;
                searchPo.InstID = condition.InstID;
                searchPo.TraderInst = condition.TraderInst;
                searchPo.Trader = condition.Trader;
                searchPo.InstTraderName = condition.InstTraderName;

                if (condition.Ofr != null)
                {
                    searchPo.Ofr = new Ofr()
                    {
                        Insta = condition.Ofr.Insta,
                        BondCode = condition.Ofr.BondCode,
                        Price = condition.Ofr.Price,
                        Dealdate = condition.Ofr.Dealdate,
                        Settlementdate = condition.Ofr.Settlementdate,
                        bankAgentName = condition.Ofr.bankAgentName,
                    };
                }
                if (condition.Bid != null)
                {
                    searchPo.Bid = new Bid()
                    {
                        Instb = condition.Bid.Instb,
                        BondCode = condition.Bid.BondCode,
                        Price = condition.Bid.Price,
                        Dealdate = condition.Bid.Dealdate,
                        Settlementdate = condition.Bid.Settlementdate,
                        bankAgentName = condition.Bid.bankAgentName,
                    };
                }

                searchPo.Price = condition.Price;
                searchPo.Priceformat = condition.Priceformat;
                searchPo.TradeDate = condition.TradeDate;
                searchPo.TradeDateDisplay = condition.TradeDateDisplay;
                searchPo.OfrMerge = condition.OfrMerge;
                searchPo.BidMerge = condition.BidMerge;
                searchPo.IsSort = condition.IsSort;
                searchPo.IsOfr = condition.IsOfr;
                searchPo.IsBid = condition.IsBid;
                searchPo.ExcId = condition.ExcId;
                searchPo.IsShowHist = condition.IsShowHist;
                searchPo.IsMaturityOrder = condition.IsMaturityOrder;
                searchPo.BondTypes = new List<string>();
                if (condition.BondTypes != null && condition.BondTypes.Count > 0)
                {
                    foreach (string item in condition.BondTypes)
                    {
                        searchPo.BondTypes.Add(item);
                    }
                }
                searchPo.TradingPlaces = new List<string>();
                if (condition.TradingPlaces != null && condition.TradingPlaces.Count > 0)
                {
                    foreach (string item in condition.TradingPlaces)
                    {
                        searchPo.TradingPlaces.Add(item);
                    }
                }
                searchPo.OrderTypes.AddRange(condition.OrderTypes);

                searchPo.brokerNames.AddRange(condition.brokerNames);

                //校验undo的条件是否和上次一样，一样的话不计入undo
                bool isSame = true;
                if (searchPos.Count > 0)
                {
                    int countID = searchPos.Count - 1;
                    isSame = IsSameCondion(searchPos[countID], searchPo);
                }
                else
                {
                    isSame = false;
                }


                if (!isSame)
                {
                    if (searchPos.Count < 4)
                    {
                        searchPos.Add(searchPo);
                    }
                    else
                    {
                        searchPos.RemoveAt(0);
                        searchPos.Add(searchPo);
                    }
                }
                this.keyIndex = searchPos.Count - 1;
                SetButtonStaus();
            }
        }


        private bool IsSameCondion(SearchPo searchPoOld, SearchPo searchPoNew)
        {
            bool isSame = true;
            if (searchPoOld != null && searchPoNew != null)
            {
                isSame = searchPoOld.SeachBridgeFlag == searchPoNew.SeachBridgeFlag
                && searchPoOld.SeachDelFlag == searchPoNew.SeachDelFlag
                && searchPoOld.Seq == searchPoNew.Seq
                && searchPoOld.BondCode == searchPoNew.BondCode
                && searchPoOld.BondName == searchPoNew.BondName
                && (searchPoOld.Inst == searchPoNew.Inst || searchPoOld.InstID == searchPoNew.InstID)
                && searchPoOld.TraderInst == searchPoNew.TraderInst
                && searchPoOld.Trader == searchPoNew.Trader
                && searchPoOld.InstTraderName == searchPoNew.InstTraderName
                && searchPoOld.Price == searchPoNew.Price
                && searchPoOld.Priceformat == searchPoNew.Priceformat
                && searchPoOld.TradeDate == searchPoNew.TradeDate
                && searchPoOld.TradeDateDisplay == searchPoNew.TradeDateDisplay
                && searchPoOld.OfrMerge == searchPoNew.OfrMerge
                && searchPoOld.BidMerge == searchPoNew.BidMerge
                && searchPoOld.IsSort == searchPoNew.IsSort
                && searchPoOld.IsOfr == searchPoNew.IsOfr
                && searchPoOld.IsBid == searchPoNew.IsBid
                && searchPoOld.ExcId == searchPoNew.ExcId
                && searchPoOld.IsShowHist == searchPoNew.IsShowHist
                && searchPoOld.IsMaturityOrder == searchPoNew.IsMaturityOrder;

                if (isSame)
                {
                    if (searchPoOld.Ofr != null && searchPoNew.Ofr != null)
                    {
                        isSame = searchPoOld.Ofr.Insta == searchPoNew.Ofr.Insta
                            && searchPoOld.Ofr.BondCode == searchPoNew.Ofr.BondCode
                            && searchPoOld.Ofr.Price == searchPoNew.Ofr.Price
                             && searchPoOld.Ofr.Dealdate == searchPoNew.Ofr.Dealdate
                             && searchPoOld.Ofr.Settlementdate == searchPoNew.Ofr.Settlementdate
                             && searchPoOld.Ofr.bankAgentName == searchPoNew.Ofr.bankAgentName;
                    }
                    else
                    {
                        if ((searchPoOld.Ofr == null && searchPoNew.Ofr != null)
                            || (searchPoOld.Ofr != null && searchPoNew.Ofr == null))
                        {

                            return false;
                        }
                        else
                        {
                            isSame = true;
                        }
                    }
                }
                else
                {
                    return false;
                }

                if (isSame)
                {
                    if (searchPoOld.Bid != null && searchPoNew.Bid != null)
                    {
                        isSame = searchPoOld.Bid.Instb == searchPoNew.Bid.Instb
                            && searchPoOld.Bid.BondCode == searchPoNew.Bid.BondCode
                            && searchPoOld.Bid.Price == searchPoNew.Bid.Price
                             && searchPoOld.Bid.Dealdate == searchPoNew.Bid.Dealdate
                             && searchPoOld.Bid.Settlementdate == searchPoNew.Bid.Settlementdate
                             && searchPoOld.Bid.bankAgentName == searchPoNew.Bid.bankAgentName;
                    }
                    else
                    {
                        if ((searchPoOld.Bid == null && searchPoNew.Bid != null)
                            || (searchPoOld.Bid != null && searchPoNew.Bid == null))
                        {

                            isSame = false;
                        }
                        else
                        {
                            isSame = true;
                        }
                    }
                }
                else
                {
                    return false;
                }

                if (isSame)
                {
                    // isSame = searchPoNew.OrderTypes.IsSameList(searchPoOld.OrderTypes);
                }

                if (isSame)
                {
                    if (searchPoOld.BondTypes != null && searchPoOld.BondTypes.Count > 0
                        && searchPoNew.BondTypes != null && searchPoNew.BondTypes.Count > 0)
                    {

                        if (searchPoOld.BondTypes.Count == searchPoNew.BondTypes.Count)
                        {

                            foreach (var item in searchPoOld.BondTypes)
                            {
                                if (searchPoNew.BondTypes.Any(_ => _ != item))
                                {
                                    return false;
                                }

                            }
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        if ((searchPoOld.BondTypes == null || searchPoOld.BondTypes.Count == 0)
                            && (searchPoNew.BondTypes == null || searchPoNew.BondTypes.Count == 0))
                        {
                            isSame = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }



                if (isSame)
                {
                    if (searchPoOld.TradingPlaces != null && searchPoOld.TradingPlaces.Count > 0
                        && searchPoNew.TradingPlaces != null && searchPoNew.TradingPlaces.Count > 0)
                    {

                        if (searchPoOld.TradingPlaces.Count == searchPoNew.TradingPlaces.Count)
                        {
                            isSame = searchPoOld.TradingPlaces.SequenceEqual(searchPoNew.TradingPlaces);
                        }
                        else
                        {
                            return false;
                        }

                    }
                    else
                    {
                        if ((searchPoOld.TradingPlaces == null || searchPoOld.TradingPlaces.Count == 0)
                            && (searchPoNew.TradingPlaces == null || searchPoNew.TradingPlaces.Count == 0))
                        {
                            isSame = true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

                if (isSame)
                {
                    //isSame = searchPoNew.brokerNames.IsSameList(searchPoOld.brokerNames);
                }
                else
                {
                    return false;
                }
            }

            //isSame = VisualHelper.IsSameClass(searchPoOld, searchPoNew);
            return isSame;
        }


        /// <summary>
        /// 恢复条件值
        /// </summary>
        private void SetUnDoCondition(SearchPo searchPo)
        {
            if (searchPo != null)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    //智能排序
                    if (condition.IsSort)
                    {
                        cbStar.IsChecked = true;
                    }
                    else
                    {
                        cbStar.IsChecked = false;
                    }

                    //期限排序
                    cbMaturityOrder.IsChecked = condition.IsMaturityOrder;

                    //显示历史
                    if (condition.IsShowHist)
                    {
                        cbHist.IsChecked = true;
                    }
                    else
                    {
                        cbHist.IsChecked = false;
                    }

                    //缺桥
                    if (!string.IsNullOrWhiteSpace(condition.SeachBridgeFlag) && condition.SeachBridgeFlag == "1")
                    {
                        cbNeedBridge.IsChecked = true;
                    }
                    else
                    {
                        cbNeedBridge.IsChecked = false;
                    }

                    //债券
                    if (!string.IsNullOrWhiteSpace(condition.BondCode))
                    {
                        tbCode.Text = condition.BondName;
                        tbBCodePop.IsOpen = false;
                    }
                    else
                    {
                        tbCode.Text = "";
                        tbBCodePop.IsOpen = false;
                    }

                    //类型
                    if (condition.BondTypes != null && condition.BondTypes.Count > 0)
                    {
                        string strBondName = "";
                        foreach (string item in condition.BondTypes)
                        {
                            if (string.IsNullOrWhiteSpace(strBondName))
                            {
                                strBondName = item;
                            }
                            else
                            {
                                strBondName += "," + item;
                            }
                        }
                        //ucBondTypes.tbSearch.Content = $"共{condition.BondTypes.Count}条:{strBondName}";
                        //ucBondTypes.DeleteVisibility = Visibility.Visible;
                        //ucBondTypes.Selects = condition.BondTypes.ToList();
                        //ucBondTypes.SetContorl();

                    }
                    else
                    {
                        //ucBondTypes.tbSearch.Content = "类型不限";
                        //ucBondTypes.DeleteVisibility = Visibility.Collapsed;
                        //ucBondTypes.Selects = new List<string>();
                        ////ucBondTypes.SetContorl();
                    }

                    // 订单类型 意向单、成交单
                    if (condition.OrderTypes.Count > 0)
                    {
                        string orderType = "";
                        foreach (string item in condition.OrderTypes)
                        {
                            if (string.IsNullOrWhiteSpace(orderType))
                            {
                                orderType = item;
                            }
                            else
                            {
                                orderType += "," + item;
                            }
                        }
                        //ucflagInner.tbSearch.Content = $"共{condition.OrderTypes.Count}条:{orderType}";
                        //ucflagInner.DeleteVisibility = Visibility.Visible;
                        //ucflagInner.Selects = condition.OrderTypes.ToList();
                        ////ucTradingPlaces.SetContorl();

                    }
                    else
                    {
                        //ucflagInner.tbSearch.Content = "不限";
                        //ucflagInner.DeleteVisibility = Visibility.Collapsed;
                        //ucflagInner.Selects = new List<string>();
                        //ucflagInner.SetContorl();
                    }


                    // broker
                    if (condition.brokerNames.Count > 0)
                    {
                        string brokerName = "";
                        foreach (string item in condition.brokerNames)
                        {
                            if (string.IsNullOrWhiteSpace(brokerName))
                            {
                                brokerName = item;
                            }
                            else
                            {
                                brokerName += "," + item;
                            }
                        }
                        //uc_Broker.tbSearch.Content = $"共{condition.brokerNames.Count}条:{brokerName}";
                        //uc_Broker.DeleteVisibility = Visibility.Visible;
                        //uc_Broker.Selects = condition.brokerNames.ToList();
                        ////ucTradingPlaces.SetContorl();

                    }
                    else
                    {
                        //uc_Broker.tbSearch.Content = "";
                        //uc_Broker.DeleteVisibility = Visibility.Collapsed;
                        //uc_Broker.Selects = new List<string>();
                        //uc_Broker.SetContorl();
                    }


                    //交易场所
                    if (condition.TradingPlaces != null && condition.TradingPlaces.Count > 0)
                    {
                        string strBondName = "";
                        foreach (string item in condition.TradingPlaces)
                        {
                            if (string.IsNullOrWhiteSpace(strBondName))
                            {
                                strBondName = item;
                            }
                            else
                            {
                                strBondName += "," + item;
                            }
                        }
                        //ucTradingPlaces.tbSearch.Content = $"共{condition.TradingPlaces.Count}条:{strBondName}";
                        //ucTradingPlaces.DeleteVisibility = Visibility.Visible;
                        //ucTradingPlaces.Selects = condition.TradingPlaces.ToList();
                        ////ucTradingPlaces.SetContorl();

                    }
                    else
                    {
                        //ucTradingPlaces.tbSearch.Content = "交易场所不限";
                        //ucTradingPlaces.DeleteVisibility = Visibility.Collapsed;
                        //ucTradingPlaces.Selects = new List<string>();
                        //ucTradingPlaces.SetContorl();
                    }

                    //机构
                    if (!string.IsNullOrWhiteSpace(condition.Inst))
                    {
                        tbInst.Text = condition.Inst;
                        tbInst.Tag = condition.InstID;
                        tbInstPop.IsOpen = false;
                    }
                    else
                    {
                        tbInst.Text = "";
                        tbInst.Tag = "";
                        tbInstPop.IsOpen = false;
                    }

                    //交易员
                    if (!string.IsNullOrWhiteSpace(condition.InstTraderName))
                    {
                        tbTrader.Text = condition.InstTraderName;
                        tbTraderPop.IsOpen = false;
                    }
                    else
                    {
                        tbTrader.Text = "";
                        tbTraderPop.IsOpen = false;
                    }

                    //价格
                    if (!string.IsNullOrWhiteSpace(condition.Priceformat))
                    {
                        tbPrice.Text = condition.Priceformat;
                    }
                    else
                    {
                        tbPrice.Text = "";
                    }
                    //交易日
                    _dateIsShow = false;
                    if (!string.IsNullOrWhiteSpace(condition.TradeDate))
                    {
                        dpMaturityDate.Text = condition.TradeDateDisplay;
                    }
                    else
                    {
                        dpMaturityDate.Text = "";
                    }
                    _dateIsShow = true;

                    //控件状态恢复可用
                    bool isEnabled = true;
                    cbStar.IsEnabled = isEnabled;
                    tbCode.IsEnabled = isEnabled;
                    tbInst.IsEnabled = isEnabled;
                    tbTrader.IsEnabled = isEnabled;
                    tbPrice.IsEnabled = isEnabled;
                    dpMaturityDate.IsHitTestVisible = isEnabled;
                    tbOrderSeq.IsEnabled = isEnabled;
                }));
            }
        }

        /// <summary>
        /// 复制条件
        /// </summary>
        /// <returns></returns>
        private SearchPo CopySearchPo(SearchPo po)
        {
            SearchPo searchPo = new SearchPo();
            if (po != null)
            {
                searchPo.SeachBridgeFlag = po.SeachBridgeFlag;
                searchPo.SeachDelFlag = po.SeachDelFlag;
                searchPo.Seq = po.Seq;
                searchPo.BondCode = po.BondCode;
                searchPo.BondName = po.BondName;
                searchPo.Inst = po.Inst;
                searchPo.InstID = po.InstID;
                searchPo.TraderInst = po.TraderInst;
                searchPo.InstTraderName = po.InstTraderName;
                searchPo.Trader = po.Trader;

                if (po.Ofr != null)
                {
                    searchPo.Ofr = new Ofr()
                    {
                        Insta = po.Ofr.Insta,
                        BondCode = po.Ofr.BondCode,
                        Price = po.Ofr.Price,
                        Dealdate = po.Ofr.Dealdate,
                        Settlementdate = po.Ofr.Settlementdate,
                        bankAgentName = po.Ofr.bankAgentName,
                    };
                }
                if (po.Bid != null)
                {
                    searchPo.Bid = new Bid()
                    {
                        Instb = po.Bid.Instb,
                        BondCode = po.Bid.BondCode,
                        Price = po.Bid.Price,
                        Dealdate = po.Bid.Dealdate,
                        Settlementdate = po.Bid.Settlementdate,
                        bankAgentName = po.Bid.bankAgentName,
                    };
                }

                searchPo.Price = po.Price;
                searchPo.Priceformat = po.Priceformat;
                searchPo.TradeDate = po.TradeDate;
                searchPo.TradeDateDisplay = po.TradeDateDisplay;
                searchPo.OfrMerge = po.OfrMerge;
                searchPo.BidMerge = po.BidMerge;
                searchPo.IsSort = po.IsSort;
                searchPo.IsOfr = po.IsOfr;
                searchPo.IsBid = po.IsBid;
                searchPo.ExcId = po.ExcId;
                searchPo.IsShowHist = po.IsShowHist;
                searchPo.IsMaturityOrder = po.IsMaturityOrder;


                searchPo.brokerNames = new List<string>();
                if (po.brokerNames != null && po.brokerNames.Count > 0)
                {
                    foreach (string item in po.brokerNames)
                    {
                        searchPo.brokerNames.Add(item);
                    }
                }


                searchPo.BondTypes = new List<string>();
                if (po.BondTypes != null && po.BondTypes.Count > 0)
                {
                    foreach (string item in po.BondTypes)
                    {
                        searchPo.BondTypes.Add(item);
                    }
                }
                searchPo.TradingPlaces = new List<string>();
                if (po.TradingPlaces != null && po.TradingPlaces.Count > 0)
                {
                    foreach (string item in po.TradingPlaces)
                    {
                        searchPo.TradingPlaces.Add(item);
                    }
                }
                searchPo.OrderTypes.AddRange(po.OrderTypes);
            }

            return searchPo;
        }

        /// <summary>
        /// 设置按钮状态
        /// </summary>
        private void SetButtonStaus()
        {

            if (searchPos.Count <= 1)
            {
                btn_Next.IsEnabled = false;
                btn_Back.IsEnabled = false;
            }
            else
            {
                if (keyIndex == 0)
                {
                    btn_Next.IsEnabled = true;
                    btn_Back.IsEnabled = false;
                }
                else
                {
                    if (keyIndex == searchPos.Count - 1)
                    {
                        btn_Next.IsEnabled = false;
                        btn_Back.IsEnabled = true;
                    }
                    else
                    {
                        btn_Next.IsEnabled = true;
                        btn_Back.IsEnabled = true;
                    }
                }
            }
        }

        #endregion

        #region 合单
        private void dgOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //自动刷新后自动保存剪切板中的内容： 20200304
            CopyExDataGrid grid = sender as CopyExDataGrid;
            if (grid != null && grid.Items.Count == 0)
                return;
            if (this.dgOrder.SelectedCells.Count > 1)
            {
                btnOfrMerge.Visibility = Visibility.Hidden;
                btnBidMerge.Visibility = Visibility.Hidden;
                btn_CalcExec.Visibility = Visibility.Visible;
                btn_Mark.Visibility = Visibility.Visible;

                ///多选的时候设置合并按钮不显示
                //uiContextMenu.Height = 60;
            }
            else
            {
                btnOfrMerge.Visibility = Visibility.Visible;
                btnBidMerge.Visibility = Visibility.Visible;
                btn_CalcExec.Visibility = Visibility.Collapsed;
                btn_Mark.Visibility = Visibility.Collapsed;
                //uiContextMenu.Height = 110;
            }
            //CopyExDataGrid.CopyType = ECopyType.Ctrl_C;// 0;
            //dgOrder.OnExecutedCopy(null);
        }
        /// <summary>
        /// Ofr合并按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOfrMerge_Click(object sender, RoutedEventArgs e)
        {
            DataRowView a = this.dgOrder.SelectedItem as DataRowView;
            PositionDetailPo po = new PositionDetailPo();
            isShowAll = true;

            if (a == null)
                po = this.dgOrder.SelectedItem as PositionDetailPo;
            else
            {
                DataRow b = a.Row;
                po = CopyDetailHelper.D2E<PositionDetailPo>(b);
            }
            if (po == null)
                return;
            else if (po.Seq == null)
                return;
            if (string.IsNullOrWhiteSpace(condition.OfrMerge))
                condition.OfrMerge = po.Insta == "机构待定" ? "" : po.Insta;
            else
                return;
            condition.Ofr = new Ofr();
            condition.Ofr.BondCode = po.BondCode;
            condition.Ofr.Insta = po.Insta == "机构待定" ? "" : po.Insta;
            condition.Ofr.Price = po.Price;
            condition.Ofr.Dealdate = po.OfrDealdate;
            condition.Ofr.Settlementdate = po.OfrSettlementdate;
            condition.IsOfr = true;
            condition.ExcId = po.ExecId;
            condition.Ofr.bankAgentName = po.BankAgentNameA;
            _isClickedButton = true;
            refresh();
            ShowAll(false);
            isShowAll = false;
        }

        private void btnBidMerge_Click(object sender, RoutedEventArgs e)
        {
            DataRowView a = this.dgOrder.SelectedItem as DataRowView;
            PositionDetailPo po = new PositionDetailPo();
            isShowAll = true;
            if (a == null)
                po = this.dgOrder.SelectedItem as PositionDetailPo;
            else
            {
                DataRow b = a.Row;
                po = CopyDetailHelper.D2E<PositionDetailPo>(b);
            }
            if (po == null)
                return;
            else if (po.Seq == null)
                return;
            if (string.IsNullOrWhiteSpace(condition.BidMerge))
                condition.BidMerge = po.Instb == "机构待定" ? "" : po.Instb;
            else
                return;
            condition.Bid = new Bid();
            condition.Bid.BondCode = po.BondCode;
            condition.Bid.Instb = po.Instb == "机构待定" ? "" : po.Instb;
            condition.Bid.Price = po.Price;
            condition.Bid.Dealdate = po.BidDealdate;
            condition.Bid.Settlementdate = po.BidSettlementdate;
            condition.IsBid = true;
            condition.ExcId = po.ExecId;
            condition.Bid.bankAgentName = po.BankAgentNameB;
            _isClickedButton = true;
            refresh();
            ShowAll(false);
            isShowAll = false;
        }
        #endregion   

        private TextBlock getTextBlock(DependencyObject dep)
        {
            while ((dep != null) && !(dep is TextBlock))
            {
                int count = VisualTreeHelper.GetChildrenCount(dep);
                if (count == 0)
                    break;
                if (count >= 9)
                    dep = VisualTreeHelper.GetChild(dep, 9);
                else
                    dep = VisualTreeHelper.GetChild(dep, 0);
            }
            TextBlock tb = dep as TextBlock;
            return tb;
        }

        private void DgOrder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            DependencyObject dep = (DependencyObject)e.OriginalSource;
            if (dep is Border)
                dep = getTextBlock(dep);
            TextBlock tb = dep as TextBlock;

            if (tb == null)
            {
                Image image = dep as Image;
                while ((dep != null) && !(dep is Button))
                {
                    dep = VisualTreeHelper.GetParent(dep);
                }

                if (dep != null)
                {
                    Button button = dep as Button;
                    if (button.Name == "bt_SendFlagA")
                    {
                        bt_SendFlag_Click(dep, null);
                        e.Handled = true;
                        return;
                    }
                    else if (button.Name == "bt_SendQQMsg")
                    {
                        bt_SendQQMsg_Click(dep, null);
                        e.Handled = true;
                        return;
                    }
                }
            }

            // iteratively traverse the visual tree until get a row or null
            while ((dep != null) && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            //if this is a row (item)
            if (dep is DataGridRow)
            {
                if (tb != null && tb.Text.ToString() == "汇总")
                {
                    this.dgOrder.SelectedItem = (dep as DataGridRow).Item;
                    btSum_Click(null, null);
                    e.Handled = true;
                    return;
                }
                if (tb != null && tb.Text.ToString() == "方向")
                {
                    this.dgOrder.SelectedItem = (dep as DataGridRow).Item;
                    btSide_Click(null, null);
                    e.Handled = true;
                    return;
                }
                if (tb != null && tb.Text.ToString() == "发送")
                {
                    this.dgOrder.SelectedItem = (dep as DataGridRow).Item;
                    btSend_Click(null, null);
                    e.Handled = true;
                    return;
                }
                if (tb != null && tb.Text.ToString() == "隐藏发送")
                {
                    this.dgOrder.SelectedItem = (dep as DataGridRow).Item;
                    btHideSend_Click(null, null);
                    e.Handled = true;
                    return;
                }
                //if the pointed item is already selected do not reselect it, so the previous multi-selection will remain
                if (dgOrder.SelectedItems.Contains((dep as DataGridRow).Item))
                {
                    //btSpare.Focus();
                    if (this.dgOrder.SelectedCells == null || this.dgOrder.SelectedCells.Count == 0)
                        return;
                    List<PositionDetailPo> list = new List<PositionDetailPo>();
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        for (int z = 0; z < this.dgOrder.SelectedCells.Count; z++)
                        {
                            DataRowView a = this.dgOrder.SelectedCells[z].Item as DataRowView;

                            if (a == null)
                            {
                                PositionDetailPo p = this.dgOrder.SelectedCells[z].Item as PositionDetailPo;

                                list.Add(p);
                            }
                            if (a != null && a.Row != null)
                            {
                                DataRow b = a.Row;
                                PositionDetailPo p = CopyDetailHelper.D2E<PositionDetailPo>(b);
                                list.Add(p);
                            }
                        }
                        var m = list.Distinct();

                        var dragData = new List<DragOrderInfo>(m.Count());
                        foreach (var item in m)
                        {
                            dragData.Add(new DragOrderInfo { ExecId = item.ExecId, Seq = item.Seq.ToString() });
                        }
                        DragDrop.DoDragDrop(dgOrder, dragData, DragDropEffects.Copy);
                    }
                    //e.Handled = true;
                }

            }
        }

        /// <summary>
        /// 期限排序
        /// </summary>
        private void cbMaturityOrder_Click(object sender, RoutedEventArgs e)
        {
            condition.IsMaturityOrder = cbMaturityOrder.IsChecked.Value;
            _isClickedButton = true;
            refresh();
            CreatSearchPo();
        }

        private async void cbHist_Click(object sender, RoutedEventArgs e)
        {
            Action action = () =>
            {
                if (cbHist.IsChecked.HasValue && cbHist.IsChecked.Value)
                {
                    condition.IsShowHist = true;
                }
                else
                {
                    condition.IsShowHist = false;
                    isNeedUpdateDir = false;
                }
                _isClickedButton = true;
            };

            if (sender == null)
            {
                Dispatcher.Invoke(action);
                refresh(isNeedUpdateDir, false);
            }
            else
            {
                action();
                CreatSearchPo();
                await Task.Run(() => refresh(isNeedUpdateDir, false));
            }
            isNeedUpdateDir = false;
        }

        private void cbNeedBridge_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked.GetValueOrDefault(false))
            {
                condition.SeachBridgeFlag = "1";
            }
            else
            {
                condition.SeachBridgeFlag = null;
            }
            _isClickedButton = true;
            CreatSearchPo();
            refresh();
        }
        /// <summary>
        /// 单号键盘处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tbOrderSeq_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await AutoCheckHistory(true);

                string seq = tbOrderSeq.Text;
                decimal temp = decimal.Zero;
                if (string.IsNullOrEmpty(seq) || !decimal.TryParse(seq, out temp))
                    return;
                goToOrderBySeq();
            }
        }
        /// <summary>
        /// 单号更改处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbOrderSeq_TextChanged(object sender, TextChangedEventArgs e)
        {
            var seq = string.Empty;
            Dispatcher.Invoke(() =>
            {
                seq = tbOrderSeq.Text;
            });

            decimal temp = decimal.Zero;
            if (string.IsNullOrEmpty(seq) || !decimal.TryParse(seq, out temp))
                return;
            filterOrderBySeq(temp);
        }
        /// <summary>
        /// 过滤后的单子集合
        /// </summary>
        List<PositionDetailPo> filters = new List<PositionDetailPo>();
        /// <summary>
        /// 根据单号过滤单子
        /// </summary>
        /// <param name="seq"></param>
        void filterOrderBySeq(decimal seq)
        {
            filters.Clear();
            filters.AddRange(data.Where(_ => _.Seq == seq).ToList());
            currentOrderSeq = 0;
        }
        /// <summary>
        /// 定位单子
        /// </summary>
        void goToOrderBySeq()
        {
            if (filters.Count > currentOrderSeq)
            {
                //定位单子
                int index = dgOrder.Items.IndexOf(filters[currentOrderSeq]);
                if (index > 0)
                {
                    dgOrder.SelectedIndex = index;
                    dgOrder.ScrollIntoView(filters[currentOrderSeq]);
                    currentOrderSeq++;
                    if (currentOrderSeq == filters.Count)
                        currentOrderSeq = 0;
                }
            }
        }
        /// <summary>
        /// 明细网格双击定位到成交事件
        /// </summary>
        public event Action<string> GoToRecordDetial;
        /// <summary>
        /// 捕获双击事件，注意这里用的预处理鼠标事件，防止与选择改变事件形成冲突。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOrder_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                PositionDetailPo data = dgOrder.SelectedItem as PositionDetailPo;
                if (data != null)
                {
                    //解决序号与历史单会出现重复的问题：用成交编号来定位
                    Task.Run(() =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            //发布消息
                            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "CopyDetailUI_GoTo_ExecId", Info1 = data.ExecId });
                        });
                    });

                }
                e.Handled = true;
            }
        }

        private void btSide_Click(object sender, RoutedEventArgs e)
        {
            PositionDetailPo posDetail = this.dgOrder.SelectedItem as PositionDetailPo;
            SideShow(posDetail);
            dgOrder.SelectedItem = null;
            DetailTrader detailTrader = posDetail.DetailTrader;
            //if (dicSide.ContainsKey(posDetail.Key))
            //{
            //    dicData[detailTrader][0].IsCheckSide = true;
            //}
            //else
            //{
            //    dicData[detailTrader][0].IsCheckSide = false;
            //}
        }
        /// <summary>
        /// 展示成交单的方向信息
        /// </summary>
        /// <param name="posDetial"></param>
        private void SideShow(PositionDetailPo posDetial)
        {
            //key 机构+空格+交易员
            string instAndTrader = posDetial.Key;
            DetailTrader detailTrader = posDetial.DetailTrader;
            //获取机构名称
            string inst = null, trader = null, side = null;
            if (instAndTrader.Split(new char[] { ' ' }).Count() == 1)
                inst = instAndTrader.Split(new char[] { ' ' })[0];
            else if (instAndTrader.Split(new char[] { ' ' }).Count() == 2)
                trader = instAndTrader.Split(new char[] { ' ' })[1];


            ///获取机构第一条明细在data中的位置
            int size = 0;
            if (posDetial.IsHist)
            {
                if (!dichistSide.ContainsKey(instAndTrader))
                {
                    dichistSide[instAndTrader] = true;
                }
                else
                {
                    dichistSide[instAndTrader] = !dichistSide[instAndTrader];
                }
                foreach (DetailTrader s in dicData.Keys)
                {
                    size += dicData[s].Count;
                }
                foreach (DetailTrader s in dichistData.Keys)
                {
                    if (s.Key.Equals(instAndTrader))
                    {
                        size += 1;
                        break;
                    }
                    else
                        size += dichistData[s].Count;
                }
            }
            else
            {
                if (!dicSide.ContainsKey(instAndTrader))
                {
                    dicSide[instAndTrader] = true;
                }
                else
                {
                    dicSide[instAndTrader] = !dicSide[instAndTrader];
                }
                foreach (DetailTrader s in dicData.Keys)
                {
                    if (s.Key.Equals(instAndTrader))
                    {
                        size += 1;
                        break;
                    }
                    else
                        size += dicData[s].Count;
                }
            }
            //获取机构的成交单数量，去除头和尾
            int count = posDetial.IsHist ? dichistData[detailTrader].Count - 2 : dicData[detailTrader].Count - 2;
            //判断当前机构是否已显示方向
            bool SideIsShow = false;
            string text = data[size].Text;
            int index = text.IndexOf(')');
            string str = text.Substring(index + 1, 3);
            if (str == " 买入" || str == " 卖出")
                SideIsShow = true;

            if (SideIsShow)
            {
                //成交单删除方向
                data[size - 1].IsCheckSide = false;
                for (int i = size; i < size + count; i++)
                {
                    text = data[i].Text;
                    string copyText = data[i].CopyText;
                    string totalText = data[i].TotalText;
                    string withoutBrokerText = data[i].WithoutBrokerText;
                    string toolText = data[i].ToolText;
                    index = text.IndexOf(')');
                    string hidenText = data[i].HideCounterpartyText;
                    string withContactsText = data[i].WithContactsText;
                    string withTraderText = data[i].WithTraderText;
                    //隐藏发送
                    string hideSendQQMsgText = data[i].HideSendQQMsgText;
                    ///过滤汇总数据
                    if (index == -1)
                        continue;
                    string t1 = text.Substring(0, index + 1);
                    string t2 = text.Substring(index + 1, text.Count() - index - 1);
                    data[i].Text = data[i].ToolText = t1 + t2.Substring(3);
                    ///带单号
                    index = copyText.IndexOf(')');
                    t1 = copyText.Substring(0, index + 1);
                    t2 = copyText.Substring(index + 1, copyText.Count() - index - 1);
                    data[i].CopyText = t1 + t2.Substring(3);
                    ///ctrl-C
                    index = totalText.IndexOf(')');
                    t1 = totalText.Substring(0, index + 1);
                    t2 = totalText.Substring(index + 1, totalText.Count() - index - 1);
                    data[i].TotalText = t1 + t2.Substring(3);
                    //data[i].WithContactsText =;
                    ///不带broker
                    index = withoutBrokerText.IndexOf(')');
                    t1 = withoutBrokerText.Substring(0, index + 1);
                    t2 = withoutBrokerText.Substring(index + 1, withoutBrokerText.Count() - index - 1);
                    data[i].WithoutBrokerText = t1 + t2.Substring(3);
                    //提示文本
                    index = toolText.IndexOf(')');
                    t1 = toolText.Substring(0, index + 1);
                    t2 = toolText.Substring(index + 1, toolText.Count() - index - 1);
                    data[i].ToolText = t1 + t2.Substring(3);
                    //隐藏交易对手方复制
                    index = hidenText != null ? hidenText.IndexOf(')') : 0;
                    t1 = hidenText != null ? hidenText.Substring(0, index + 1) : "";
                    t2 = hidenText != null ? hidenText.Substring(index + 1, hidenText.Count() - index - 1) : "";
                    data[i].HideCounterpartyText = t1 + (t2.Length >= 3 ? t2.Substring(3) : "");

                    //带联系人复制
                    index = withContactsText.IndexOf(')');
                    t1 = withContactsText.Substring(0, index + 1);
                    t2 = withContactsText.Substring(index + 1, withContactsText.Count() - index - 1);
                    data[i].WithContactsText = t1 + t2.Substring(3);

                    //带交易员复制
                    index = withTraderText.IndexOf(')');
                    t1 = withTraderText.Substring(0, index + 1);
                    t2 = withTraderText.Substring(index + 1, withTraderText.Count() - index - 1);
                    data[i].WithTraderText = t1 + t2.Substring(3);

                    //隐藏发送
                    index = hideSendQQMsgText.IndexOf(')');
                    t1 = hideSendQQMsgText.Substring(0, index + 1);
                    t2 = hideSendQQMsgText.Substring(index + 1, hideSendQQMsgText.Count() - index - 1);
                    data[i].HideSendQQMsgText = t1 + t2.Substring(3);
                }
            }
            else
            {
                //成交单添加方向
                data[size - 1].IsCheckSide = true;
                for (int i = size; i < size + count; i++)
                {
                    text = data[i].Text;
                    //获取方向内容 看机构在"出给"的左边还是右边
                    side = data[i].Insta == inst ? " 卖出" : " 买入";
                    if (trader != null)
                        side = data[i].Tradera == trader ? " 卖出" : " 买入";

                    side = " " + data[i].SideShow;
                    string copyText = data[i].CopyText;
                    string totalText = data[i].TotalText;
                    string withoutBrokerText = data[i].WithoutBrokerText;
                    string toolText = data[i].ToolText;
                    string hidenText = data[i].HideCounterpartyText;
                    string withContactsText = data[i].WithContactsText;
                    string withTraderText = data[i].WithTraderText;
                    //隐藏发送
                    string hideSendQQMsgText = data[i].HideSendQQMsgText;
                    index = text.IndexOf(')');
                    ///过滤汇总数据
                    if (index == -1)
                        continue;

                    //  1)  3.91Y  140215  14国开15  12.00  1000 + 1 五台农商行 出给 其他公司1号
                    //  1) 卖出  3.91Y  140215  14国开15  12.00  1000 + 1 五台农商行 出给 其他公司1号
                    // 括号左边序号，右边是文本，
                    string t1 = text.Substring(0, index + 1);
                    string t2 = text.Substring(index + 1, text.Count() - index - 1);
                    data[i].Text = data[i].ToolText = t1 + side + t2;
                    //带单号
                    index = copyText.IndexOf(')');
                    t1 = copyText.Substring(0, index + 1);
                    t2 = copyText.Substring(index + 1, copyText.Count() - index - 1);
                    data[i].CopyText = t1 + side + t2;
                    //Ctrl-C
                    index = totalText.IndexOf(')');
                    t1 = totalText.Substring(0, index + 1);
                    t2 = totalText.Substring(index + 1, totalText.Count() - index - 1);
                    data[i].TotalText = t1 + side + t2;
                    //不带broker
                    index = withoutBrokerText.IndexOf(')');
                    t1 = withoutBrokerText.Substring(0, index + 1);
                    t2 = withoutBrokerText.Substring(index + 1, withoutBrokerText.Count() - index - 1);
                    data[i].WithoutBrokerText = t1 + side + t2;
                    //提示文本
                    index = toolText.IndexOf(')');
                    t1 = toolText.Substring(0, index + 1);
                    t2 = toolText.Substring(index + 1, toolText.Count() - index - 1);
                    data[i].ToolText = t1 + side + t2;

                    //隐藏交易对手方复制
                    index = hidenText != null ? hidenText.IndexOf(')') : 0;
                    t1 = hidenText != null ? hidenText.Substring(0, index + 1) : "";
                    t2 = hidenText != null ? hidenText.Substring(index + 1, hidenText.Count() - index - 1) : "";
                    data[i].HideCounterpartyText = t1 + side + t2;

                    //带联系人复制
                    index = withContactsText.IndexOf(')');
                    t1 = withContactsText.Substring(0, index + 1);
                    t2 = withContactsText.Substring(index + 1, withContactsText.Count() - index - 1);
                    data[i].WithContactsText = t1 + side + t2;

                    //带交易员复制
                    index = withTraderText.IndexOf(')');
                    t1 = withTraderText.Substring(0, index + 1);
                    t2 = withTraderText.Substring(index + 1, withTraderText.Count() - index - 1);
                    data[i].WithTraderText = t1 + side + t2;

                    //隐藏发送
                    index = hideSendQQMsgText.IndexOf(')');
                    t1 = hideSendQQMsgText.Substring(0, index + 1);
                    t2 = hideSendQQMsgText.Substring(index + 1, hideSendQQMsgText.Count() - index - 1);
                    data[i].HideSendQQMsgText = t1 + side + t2;
                }
            }

        }

        private void btnWithoutBrokerCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isHasReflash = false;
                CopyExDataGrid.CopyType = ECopyType.不带Broker;//CopyExDataGrid.CopyType = 2;
                dgOrder.OnExecutedCopy(null);
                CopyExDataGrid.CopyType = ECopyType.Ctrl_C;
            }//CopyExDataGrid.CopyType = 0;
            catch (Exception ex)
            {
                LogHelper.Error("不带Broker复制订单失败:" + ex.ToString());
            }
            finally
            {
                _isHasReflash = true;
                Reflash();
            }

        }

        /// <summary>
        /// 默认情况下自动刷新明细：20200304 wym
        /// 1.当设置为false时（不进行自动刷新明细）：说明用户想要进行批量拷贝操作
        /// 2.用户拷贝操作完成后，重新吧IsAutoReload设置为true（恢复到自动刷新模式）
        /// </summary>
        protected bool IsAutoReload { get; set; } = true;
        private void cbNoLookDel_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (cb.IsChecked.GetValueOrDefault(false))
            {
                condition.SeachDelFlag = "1";
            }
            else
            {
                condition.SeachDelFlag = null;
            }
            refresh();

        }

        private void Part_ClearButton_Click(object sender, RoutedEventArgs e)
        {
            Button bt = (sender as Button);
            ContentPresenter cp = bt.FindName("PART_ContentHost") as ContentPresenter;
            TextBox tb = bt.TemplatedParent as TextBox;
            tb.Clear();
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSend_Click(object sender, RoutedEventArgs e)
        {
            PositionDetailPo posDetail = this.dgOrder.SelectedItem as PositionDetailPo;
            dgOrder.SelectedIndex = -1;
            if (posDetail == null)
            {
                LogHelper.Error("btSend_Click:slected item is null");
                return;
            }
            SendData(posDetail, false);
        }
        /// <summary>
        /// 根据header解析出机构的数据发给后台 ,发送请求消息
        /// </summary>
        /// <param name="posDetail"></param>
        /// <param name="isHideSendToQQMsg">是否隐藏发送</param>
        private void SendData(PositionDetailPo posDetail, bool isHideSendToQQMsg)
        {
            string key = posDetail.Key.Trim();
            DetailTrader detailTrader = posDetail.DetailTrader;
            bool isHist = posDetail.IsHist;
            string txt = string.Empty;
            if (isHist)
            {
                List<PositionDetailPo> datas;
                if (dichistData.TryGetValue(detailTrader, out datas))
                {
                    foreach (PositionDetailPo order in dichistData[detailTrader])
                    {
                        if (order.DataType != EDataType.数据)
                            continue;
                        if (!string.IsNullOrWhiteSpace(order.TotalText))
                        {
                            if (isHideSendToQQMsg)
                            {
                                txt += string.Format("{0}\r\n", order.HideCounterpartyText.Replace("\r", ""));
                            }
                            else
                            {
                                txt += string.Format("{0}\r\n", order.TotalText.Replace("\r", ""));
                            }
                        }
                        else
                        {
                            txt += "\r\n";
                        }

                    }
                }
                List<PositionDetailPo> sums;
                if (dichistSum.TryGetValue(detailTrader, out sums))
                {
                    foreach (PositionDetailPo order in dichistSum[detailTrader])
                    {
                        if (order.DataType != EDataType.汇总)
                            continue;
                        if (!string.IsNullOrWhiteSpace(order.Text))
                        {
                            txt += string.Format("{0}\r\n", order.Text.Replace("\r", "").TrimStart());
                        }
                        else
                        {
                            txt += "\r\n";
                        }
                    }
                }
            }
            else
            {
                List<PositionDetailPo> datas;
                if (dicData.TryGetValue(detailTrader, out datas))
                {
                    foreach (PositionDetailPo order in dicData[detailTrader])
                    {
                        if (order.DataType != EDataType.数据)
                            continue;
                        if (!string.IsNullOrWhiteSpace(order.TotalText))
                        {
                            if (isHideSendToQQMsg)
                            {
                                txt += string.Format("{0}\r\n", order.HideCounterpartyText.Replace("\r", ""));
                            }
                            else
                            {
                                txt += string.Format("{0}\r\n", order.TotalText.Replace("\r", ""));
                            }
                        }
                        else
                        {
                            txt += "\r\n";
                        }
                    }
                }
                List<PositionDetailPo> sums;
                if (dicSum.TryGetValue(detailTrader, out sums))
                {
                    foreach (PositionDetailPo order in dicSum[detailTrader])
                    {
                        if (order.DataType != EDataType.汇总)
                            continue;
                        if (!string.IsNullOrWhiteSpace(order.Text))
                        {
                            txt += string.Format("{0}\r\n", order.Text.Replace("\r", "").TrimStart());
                        }
                        else
                        {
                            txt += "\r\n";
                        }
                    }
                }
            }
            //Console.Write(txt);
            //取交易员发给后台
            string trader = key.Substring(key.IndexOf(" ") + 1);
            string inst = key.Substring(0, key.IndexOf(" "));
            SendQQMsgReq request = new SendQQMsgReq();
            List<DataGridDataPo> dataList = Utils.CommonUtils.GetContactByContactName(trader, 10);
            ///需要考虑不同机构交易员同名字的情况
            dataList = dataList.Where(_ => _.IdbKey == detailTrader.TraderID).ToList();
            DataGridDataPo po = new DataGridDataPo();
            if (dataList != null && dataList.Count > 0)
            {
                po = dataList[0];
            }
            //if (dataList != null && dataList.Count > 0)
            //{
            //    request.bankAgentCode = dataList[0].IdbKey;
            //}
            //request.bankAgentCode = detailTrader.TraderID;

            request.institutionId = detailTrader.InstID;
            request.institutionName = po.InstName;
            request.traderId = detailTrader.TraderID;
            request.traderName = po.TraderName;
            request.message = txt;
            request.sendType = "1";

            request.partySubId = LocalEnv.UserInfo.UserId;
            request.userName = LocalEnv.UserInfo.FullName;
            LogHelper.Info(string.Format("明细发消息，机构：{0}；交易员：{1}，消息：\r\n{2}", inst, trader, txt));
            ThreadPool.QueueUserWorkItem(delegate
            {
                PResult reply = GRHandler.Instance.sendQQMsg(request);
                if (reply != null)
                {
                }
            });
        }
        /// <summary>
        /// 装载数据行处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOrder_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseEnter += Row_MouseEnter;
        }
        /// <summary>
        /// 当前鼠标悬浮的数据行
        /// </summary>
        DataGridRow currentRow;
        /// <summary>
        /// 鼠标进入数据行处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Row_MouseEnter(object sender, MouseEventArgs e)
        {
            currentRow = (DataGridRow)sender;
            PositionDetailPo currentPos = currentRow.Item as PositionDetailPo;
            if (currentPos != null && currentPos.DataType == EDataType.数据)
            {
                if ((System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift && currentPos.goodstype + "" == "1")
                {
                    string bondIssueAmount = ConvertTextHelper.FormatIssueaMount(currentPos.IssueAmount.ToString());
                    string amount = currentPos.IssueAmount.HasValue ? $"{bondIssueAmount} 亿" : "";
                    currentRow.ToolTip = $"发行量：{ amount}，评级：{currentPos.IssuerRatingCurrent }";
                }
                else
                {
                    currentRow.ToolTip = currentPos.ToolText;
                }
            }
        }
        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgOrder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            CopyExDataGrid.CopyType = ECopyType.Ctrl_C;// 0;
            dgOrder.OnExecutedCopy(null);
            _isHasReflash = true;
            if (_isHasNewOrder)
            {
                are.Set();
            }
        }

        private void btnWithContacts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyExDataGrid.CopyType = ECopyType.WithContacts;// 0;
                dgOrder.OnExecutedCopy(null);
            }
            catch (Exception ex)
            {
                LogHelper.Error("带联系人复制订单失败:" + ex.ToString());
            }
            finally
            {
                _isHasReflash = true;
                Reflash();
            }
        }
        //重写Closing方法
        //protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        //{
        //    this.Hide();
        //    e.Cancel = true;
        //}

        private void btn_Back_Click(object sender, RoutedEventArgs e)
        {
            if (keyIndex >= 1)
            {
                keyIndex--;
                SearchPo search = CopySearchPo(searchPos[keyIndex]);
                SetUnDoCondition(search);
                condition = CopySearchPo(search);
                //点击按钮刷新标志重置
                _isClickedButton = true;
                refresh();
            }
            SetButtonStaus();
        }

        private void btn_Next_Click(object sender, RoutedEventArgs e)
        {
            if (keyIndex <= 3)
            {
                keyIndex++;
                SearchPo search = CopySearchPo(searchPos[keyIndex]);
                SetUnDoCondition(search);
                condition = CopySearchPo(search);
                _isClickedButton = true;
                refresh();
            }
            SetButtonStaus();
        }

        private void dgOrder_Selected(object sender, RoutedEventArgs e)
        {
            _isHasReflash = false;
        }
        /// <summary>
        /// 右键菜单事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            _isHasReflash = false;

            if (dgOrder.SelectedItems != null && dgOrder.SelectedItems.Count > 1)
            {

            }

            PositionDetailPo positionDetail;

            if (dgOrder.SelectedItem is PositionDetailPo && dgOrder.SelectedItems.Count == 1)
            {
                positionDetail = (PositionDetailPo)(dgOrder.SelectedItem);
                if (!string.IsNullOrEmpty(positionDetail.OrderId))
                {
                    TransOrderServerData order = TransOrderHandler.Instance.GetTransOrderByExecID(positionDetail.OrderId);
                    //OrderHandler.Instance.GetOrderById(positionDetail.OrderId);
                    if (order != null && (order.ExecID == positionDetail.ExecId))
                    {
                        btn_Reminder.Visibility = CopyDetailHandler.IsLoginBrokerOrder(order) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                if (positionDetail.DetailMarkDisplay == "1")
                {
                    btn_Mark.Header = "取消标记";
                }
                else
                {
                    btn_Mark.Header = "标记";
                }
                btn_Mark.Visibility = Visibility.Visible;
            }
            else
            {
                btn_Mark.Visibility = Visibility.Collapsed;
                btn_Reminder.Visibility = Visibility.Collapsed;
            }

            if (btn_Reminder.Visibility == Visibility.Collapsed)
            {
                btnBidMerge.Margin = new Thickness(0, 0, 0, 10);
            }
            else
            {
                btnBidMerge.Margin = new Thickness(0, 0, 0, 0);
            }
        }

        private void Window_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isHasReflash = false;
        }

        private void uiContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            _isHasReflash = true;
            Reflash();
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isHasReflash = true;
            Reflash();
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            LogHelper.Info("松开鼠标左键");
            _isHasReflash = true;
            Reflash();
        }

        /// <summary>
        /// 操作后重刷
        /// </summary>
        private void Reflash()
        {
            _isHasReflash = true;
            if (_isHasNewOrder)
            {
                are.Set();
            }
        }
        private void CopyDetail_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isHasNewOrder = false;
            LogHelper.Info("按下鼠标左键");
        }

        /// <summary>
        /// 隐藏对手方复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_HideCounterpartyCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyExDataGrid.CopyType = ECopyType.HideCounterparty;// 0;
                dgOrder.OnExecutedCopy(null);
            }
            catch (Exception ex)
            {
                LogHelper.Error("带联系人复制订单失败:" + ex.ToString());
            }
            finally
            {
                _isHasReflash = true;
                Reflash();
            }
        }

        /// <summary>
        /// 点击按钮发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bt_SendFlag_Click(object sender, RoutedEventArgs e)
        {
            DgOrderIn = true;
            Button button = sender as Button;
            if (button != null)
            {
                PositionDetailPo CopyDetailPo = button.DataContext as PositionDetailPo;
                if (CopyDetailPo == null)
                {
                    return;
                }
                CopyDetailPo.SendManuallyTag = "3";

                if (dgOrder.SelectedItems != null)
                {

                    PositionDetailPo position = null;
                    foreach (var item in dgOrder.SelectedItems)
                    {
                        PositionDetailPo positionDetail = item as PositionDetailPo;
                        if (positionDetail != null)
                        {
                            if (CopyDetailPo.ExecId == positionDetail.ExecId && CopyDetailPo.SideShow == positionDetail.SideShow)
                            {
                                position = item as PositionDetailPo;
                                break;
                            }
                        }
                    }
                    dgOrder.SelectedItems.Remove(position);
                }
                if (!string.IsNullOrWhiteSpace(CopyDetailPo.IsBrokerATag))
                {

                    ECommandType field;
                    if (CopyDetailPo.IsBrokerATag == "1")
                    {
                        field = ECommandType.ManualSendDealA;
                    }
                    else if (CopyDetailPo.IsBrokerATag == "2")
                    {
                        field = ECommandType.ManualSendDealB;
                    }
                    else
                    {
                        return;
                    }
                    CopyDetailHandler.ExecOrderUpdate(CopyDetailHandler.GetUpdateOrderPoNew("3", field, CopyDetailPo.OrderId, CopyDetailPo.ExecId));
                    //OrderHelper.ChangeOrder(OrderHelper.GetUpdateOrderPoNew("3", field, CopyDetailPo.OrderId, CopyDetailPo.ExecId));
                }
            }
        }

        /// <summary>
        /// 带交易员复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_WithTraderCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyExDataGrid.CopyType = ECopyType.WithTrader;// 0;
                dgOrder.OnExecutedCopy(null);
            }
            catch (Exception ex)
            {
                LogHelper.Error("带联系人复制订单失败:" + ex.ToString());
            }
            finally
            {
                _isHasReflash = true;
                Reflash();
            }
        }

        private async void btn_Reminder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool? sendResult = null;
                PositionDetailPo positionDetail;
                if (dgOrder.SelectedItem is PositionDetailPo)
                {
                    positionDetail = (PositionDetailPo)(dgOrder.SelectedItem);
                    if (!string.IsNullOrEmpty(positionDetail.OrderId))
                    {
                        //var order = OrderHandler.Instance.GetOrderById(positionDetail.OrderId);
                        TransOrderServerData order = TransOrderHandler.Instance.GetTransOrderByExecID(positionDetail.ExecId);
                        CopyDetailPo po;
                        if (order != null && positionDetail.ExecId == order.ExecID)
                        //if (order != null && order.execOrders != null && (po = order.execOrders.FirstOrDefault(x => x.execId == positionDetail.ExecId)) != null)
                        {
                            var index = positionDetail.WithoutBrokerText.IndexOf(")");
                            var text = $"内码 {positionDetail.Seq}{positionDetail.WithoutBrokerText.Remove(0, index)} \r\n催催";

                            sendResult = await TransOrderHandler.Instance.SendQQMsgToBroker(order, text, "明细");
                            if (sendResult != null && sendResult.Value)
                            {
                                LogHelper.Info($"Broker发明细催单QQ成功，消息：\r\n{text}");
                            }
                            else
                            {
                                LogHelper.Info($"Broker发明" +
                                    $"" +
                                    $"" +
                                    $"细催单QQ失败，消息：\r\n{text}");
                            }
                        }
                    }
                }
                if (sendResult == null || !sendResult.Value)
                {
                    LogHelper.Info($"订单无法发送催单消息");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("明细催单失败", ex);
            }
        }

        /// <summary>
        /// 计算成交量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_CalcExec_Click(object sender, RoutedEventArgs e)
        {
            //(decimal buyQty, decimal sellQty)
            CalculateQtyModel result = dgOrder.OnCalculateQty(dicInst, dichistInst);
            ExecutionQtyWin.Instance.executionQtyViewModel = new ExecutionQtyViewModel
            {
                BuyQty = result.buyQty,
                SellQty = result.sellQty,
                VisibilityBuy = result.buyQty > 0 ? Visibility.Visible : Visibility.Collapsed,
                VisibilitySell = result.sellQty > 0 ? Visibility.Visible : Visibility.Collapsed
            };
            ExecutionQtyWin.Instance.DataContext = ExecutionQtyWin.Instance.executionQtyViewModel;
            ExecutionQtyWin.Instance.Show();
        }
        /// <summary>
        /// 隐藏部分内容发送qq消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btHideSend_Click(object sender, RoutedEventArgs e)
        {
            PositionDetailPo posDetail = this.dgOrder.SelectedItem as PositionDetailPo;
            dgOrder.SelectedIndex = -1;
            if (posDetail == null)
            {
                LogHelper.Error("btSend_Click:slected item is null");
                return;
            }
            SendData(posDetail, true);
        }

        private void bt_SendQQMsg_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            PositionDetailPo posDetail = button.DataContext as PositionDetailPo;
            dgOrder.SelectedIndex = -1;
            if (posDetail == null)
            {
                LogHelper.Error("btSend_Click:slected item is null");
                return;
            }
            if (string.IsNullOrWhiteSpace(posDetail.SendQQbankagentId))
            {
                return;
            }

            string brokerId = LocalEnv.UserInfo.UserId;
            posDetail.SendQQMsgVisible = Visibility.Collapsed;
            SendQQMsgReq request = new SendQQMsgReq();
            request.ifAppend = GRSettingsHandler.AppSettingIsTrue(Constant.WithQQElementChange) ? "1" : "0";
            request.institutionId = posDetail.SendQQbankId;
            request.traderId = posDetail.SendQQbankagentId;

            if (GRSettingsHandler.AppSettingIsTrue(Constant.WithAssistantQQSendMsg))
            {
                brokerId = posDetail.BrokerID;

            }

            request.partySubId = brokerId;

            if (GRSettingsHandler.GlobSystemParameter.HideCounterpartySend)
            {
                request.message = posDetail.HideCounterpartyText.Replace("\r", "");
                request.HideCounterpartySend = "1";
            }
            else
            {
                request.message = posDetail.TotalText.Replace("\r", "");
                request.HideCounterpartySend = "0";
            }

            request.sendType = "3";
            request.execId = posDetail.ExecId;
            request.commandType = (int)ECommandType.SendDetailQQMsgA + "";
            if (posDetail.SendQQbankagentId == posDetail.TxtBankagentIdA)
            {
                request.commandType = (int)ECommandType.SendDetailQQMsgA + "";

                //if (posDetail.BrokerID == LocalEnv.UserInfo.UserId)
                //{
                //    if (GRSettingsHandler.GlobSystemParameter.HideCounterpartySend)
                //    {
                //        request.message = posDetail.HideCounterpartyText.Replace("\r", "");
                //    }
                //    else
                //    {
                //        request.message = posDetail.TotalText.Replace("\r", "");
                //    }
                //}
                //else
                //{
                //    if (GRSettingsHandler.AppSettingIsTrue(Constant.WithAssistantQQSendMsg)
                //        && Constant.BrokerFlags.Contains(LocalEnv.ConfInfo.BrokerNameFlag))
                //    {
                //        request.message = posDetail.AssistantAText.Replace("\r", "");
                //    }
                //    else
                //    {
                //        if (GRSettingsHandler.GlobSystemParameter.HideCounterpartySend)
                //        {
                //            request.message = posDetail.HideCounterpartyText.Replace("\r", "");
                //        }
                //        else
                //        {
                //            request.message = posDetail.TotalText.Replace("\r", "");
                //        }
                //    }
                //}
            }
            else
            {
                request.commandType = (int)ECommandType.SendDetailQQMsgB + "";
                //updateExecOrderPo.singleQQMsgSendFlagb = "1";
                //if (posDetail.BrokerID == LocalEnv.UserInfo.UserId)
                //{
                //    if (GRSettingsHandler.GlobSystemParameter.HideCounterpartySend)
                //    {
                //        request.message = posDetail.HideCounterpartyText.Replace("\r", "");
                //    }
                //    else
                //    {
                //        request.message = posDetail.TotalText.Replace("\r", "");
                //    }
                //}
                //else
                //{
                //    if (GRSettingsHandler.AppSettingIsTrue(Constant.WithAssistantQQSendMsg)
                //       && Constant.BrokerFlags.Contains(LocalEnv.ConfInfo.BrokerNameFlag))
                //    {
                //        request.message = posDetail.AssistantBText.Replace("\r", "");
                //    }
                //    else
                //    {
                //        if (GRSettingsHandler.GlobSystemParameter.HideCounterpartySend)
                //        {
                //            request.message = posDetail.HideCounterpartyText.Replace("\r", "");
                //        }
                //        else
                //        {
                //            request.message = posDetail.TotalText.Replace("\r", "");
                //        }
                //    }
                //}
            }
            LogHelper.Info($"明细发消息，交易员：{request.traderId}，消息：\r\n{request.message}");
            ThreadPool.QueueUserWorkItem(delegate
            {
                PResult reply = GRHandler.Instance.sendQQMsg(request);
            });
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private void btn_Mark_Click(object sender, RoutedEventArgs e)
        {
            DgOrderIn = true;
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                if (dgOrder.SelectedItems != null)
                {
                    PositionDetailPo position = dgOrder.SelectedItems[0] as PositionDetailPo;
                    string detailMark = "1";
                    if (position.DetailMarkDisplay == "1")
                    {
                        detailMark = "";
                        position.DetailMarkDisplay = "0";
                        if (position.MarkDetailUserList.Contains(LocalEnv.UserInfo.UserId))
                        {
                            position.MarkDetailUserList.Remove(LocalEnv.UserInfo.UserId);
                        }
                        if (position.MarkDetailUserList.Count > 0)
                        {
                            foreach (var item in position.MarkDetailUserList)
                            {
                                detailMark += item + ",";
                            }
                        }
                    }
                    else
                    {
                        detailMark = "";
                        if (!position.MarkDetailUserList.Contains(LocalEnv.UserInfo.UserId))
                        {
                            position.MarkDetailUserList.Add(LocalEnv.UserInfo.UserId);
                        }
                        if (position.MarkDetailUserList.Count > 0)
                        {
                            foreach (var item in position.MarkDetailUserList)
                            {
                                detailMark += item + ",";
                            }
                        }
                        position.DetailMarkDisplay = "1";
                    }

                    detailMark = detailMark.TrimEnd(',');
                    ECommandType field = ECommandType.UpdateDetailMark;
                    CopyDetailHandler.ExecOrderUpdate(CopyDetailHandler.GetUpdateOrderPoNew(detailMark, field, position.OrderId, position.ExecId));
                    // OrderHelper.ChangeOrder(OrderHelper.GetUpdateOrderPoNew(detailMark, field, position.OrderId, position.ExecId));
                }
                else
                {
                    LogHelper.Info("btn_Mark_Click:未选中行");
                }
            }
        }

        private void uiContextMenu_Loaded(object sender, RoutedEventArgs e)
        {

        }


    }
}