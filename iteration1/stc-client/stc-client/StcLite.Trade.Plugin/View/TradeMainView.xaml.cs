using com.gateway.log;
using Dragablz;
using Dragablz.Custom;
using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.Component;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Model;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.View.Order;
using BDB.Trade.Plugin.View.Trade;
using BDB.Trade.Plugin.View.Trade.Card;
using BDB.Trade.Plugin.ViewModel;
using BDB.Trade.Plugin.ViewModel.Adapter;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using BDB.Trade.Plugin.ViewModel.UserSetting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BDB.Price.Plugin.View.Trade;
using STC.Common.Utils;
using HomePageDataHandler = BDB.Trade.Plugin.Handler.HomePageDataHandler;
using STC.Common.Model.Admin;
using STC.Frame.CustomUI.Component;
using STC.Common.Data;

namespace BDB.Trade.Plugin.View
{
    /// <summary>
    /// TradeMainView.xaml 的交互逻辑
    /// </summary>
    public partial class TradeMainView : UserControl
    {
        private TradeMainViewModel _tradeMainViewModel = null;
        
        private bool isInit = true;

        public TradeMainView()
        {
            InitializeComponent();

            if (LocalEnv.ConfInfo.IsBDBSplit)
            {
                btnPopup.Visibility = Visibility.Visible;
            }

            //CustomCommandUtils.CustomCommand.CanExecuteChanged += CustomCommand_CanExecuteChanged;
            tcPlugin.ClosingItemCallback = new Dragablz.ItemActionCallback(OnClosingItem);
            this.Loaded += TradeMainView_Loaded;

            //sfvSettle.SetButtonProperty(40, 24);

            _tradeMainViewModel = new TradeMainViewModel();
            this.DataContext = _tradeMainViewModel;

            BondChangeHandler.Instance.OnSelectBondChange += Instance_OnSelectBondChange;
            BondChangeHandler.Instance.OnOneSettleChange += Instance_OnOneSettleChange;
            BondChangeHandler.Instance.OnOneBrokerChange += Instance_OnOneBrokerChange;

            STC.Common.Data.DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;

            STC.Common.Data.DesignHandler.Instance.OnKeyDown += Instance_OnKeyDown;

            HomePageDataHandler.Instance.OnFavoritesChanged += Instance_OnFavoritesChanged;
            QuoteConfigHandler.Instance.OnGroupTableChange += Instance_OnGroupTableChange; ;

            //this.Unloaded += TradeMainView_Unloaded;

        }

        private void Instance_OnOneBrokerChange(OperationType obj, string broker)
        {
            if (obj == OperationType.CardToMain)
            {
                allBroker.SetSelectedIndex(-1);

                if (_currentConditionDic.ContainsKey(CommonConstants.BROKER))
                    _currentConditionDic.Remove(CommonConstants.BROKER);
            }
        }

        private void Instance_OnGroupTableChange(string obj, string id, string securityId, FavoriteTypeEnum favoriteTypeEnum)
        {
            this.isFirstClickCard = true;

            this.Dispatcher.BeginInvoke((Action)(() =>
            {
                try
                {
                    TabItem currentSelectedItem = (TabItem)tcPlugin.SelectedItem;
                    if (obj != null && currentSelectedItem != null && currentSelectedItem.Tag != null && currentSelectedItem.Tag is GroupItems)
                    {
                        GroupItems gi = currentSelectedItem.Tag as GroupItems;
                        if (!gi.GroupId.Equals(obj))
                        {
                            foreach (TabItem tabItem in tcPlugin.Items)
                            {
                                GroupItems subGi = tabItem.Tag as GroupItems;
                                if (subGi.GroupId.Equals(obj))
                                {
                                    tcPlugin.SelectedItem = tabItem;
                                    break;
                                }
                            }
                        }
                    }
                }
                catch { }


                if (rbCardMode.IsChecked.Value)
                {
                    SetCardMode(isFirstClickCard);
                    isFirstClickCard = false;
                    LayoutSetttingCache.Instance.layoutSettingPo.ShowModel = 2;
                    //LayoutSetttingCache.Instance.SaveModeSetting();
                }
            }));
        }

        private void Instance_OnFavoritesChanged(FavoriteTypeEnum actionEnum, string groupID, string securityID, string bondId)
        {
            if (actionEnum == FavoriteTypeEnum.AddBondFromTable)
            {
                this.isFirstClickCard = true;
            }
            else if (actionEnum == FavoriteTypeEnum.NoGroup)
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.positionloadMask.Visibility = Visibility.Collapsed;
                });
            }
        }



        #region 记录打开的tag页面
        public const string SelectdGroup = "SelectGroup";

        private void ReadHistory()
        {
            string key = string.Empty;
            Window w = Window.GetWindow(this);
            if (w != null)
            {
                key = w.GetType().ToString();
            }
            string selectItem = IniFileHelper.ReadValue(AppUtils.GetCacheKey(key), SelectdGroup);
            _tradeMainViewModel.SetPreItemName(selectItem);
        }

        private void WriteHistory()
        {
            if (tcPlugin.SelectedItem != null)
            {
                TabItem tabItem = tcPlugin.SelectedItem as TabItem;
                string header = tabItem.Header as string;
                string key = string.Empty;
                Window w = Window.GetWindow(this);
                if (w != null)
                {
                    key = w.GetType().ToString();
                }
                IniFileHelper.WriteValue(AppUtils.GetCacheKey(key), SelectdGroup, header);
            }
        }

        #endregion



        private void Instance_OnKeyDown(KeyEventArgs e)
        {
            TabablzControl tc = CommonUtils.GetParentObject<TabablzControl>(this);
            if (tc == null || (tc.SelectedItem != null && (tc.SelectedItem as TabItem).Header.ToString() != "行情看板"))
            {
                return;
            }

            if (_isCardModel)
            {
                if (tcPlugin.SelectedItem == null)
                {
                    e.Handled = true;
                    return;
                }

                TableAndCardView tableAndCardView = (tcPlugin.SelectedItem as TabItem).Content as TableAndCardView;

                switch (e.Key)
                {
                    case Key.Left:
                    case Key.PageUp:

                        tableAndCardView.pageCardView.ChangePage(-1);
                        e.Handled = true;

                        break;
                    case Key.Right:
                    case Key.PageDown:

                        tableAndCardView.pageCardView.ChangePage(1);
                        e.Handled = true;

                        break;
                    case Key.Home:

                        tableAndCardView.pageCardView.HomePage();
                        e.Handled = true;

                        break;
                    case Key.Up:
                        break;
                    case Key.End:

                        tableAndCardView.pageCardView.ChangePage(1);
                        e.Handled = true;

                        break;
                    case Key.Down:
                        break;
                    case Key.Tab:

                        tableAndCardView.pageCardView.LoopPage();
                        e.Handled = true;

                        break;
                        //case Key.F9:
                        //    if (tcContent.SelectedIndex == 0)
                        //    {
                        //        settingButton_Click(null, null);
                        //    }
                        //    e.Handled = true;
                        //    break;
                }
            }
        }


        private void TradeMainView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= TradeMainView_Unloaded;

            //WriteHistory();
            LogHelper.Warn("TradeMainView_Unloaded");
            BondChangeHandler.Instance.OnSelectBondChange -= Instance_OnSelectBondChange;
            BondChangeHandler.Instance.OnOneSettleChange -= Instance_OnOneSettleChange;

            //DesignHandler.Instance.OnSettingChange -= Instance_OnSettingChange;

            STC.Common.Data.DesignHandler.Instance.OnKeyDown -= Instance_OnKeyDown;
        }

        private void ChangeMainSettleToCustom()
        {
            sfvSettle.SetSelectedIndex(-1);

            if (_currentConditionDic.ContainsKey(CommonConstants.SETTLE))
                _currentConditionDic.Remove(CommonConstants.SETTLE);

            btnCustomerSettle.Background = (Brush)this.FindResource("Item.SelectedActive.Background");
        }

        private void Instance_OnSelectBondChange(BondUpdateType actionFrom, string groupId, string id, string security, string settle, bool needRefresh)
        {
            if (actionFrom == BondUpdateType.ChangeRowSettleSignal)
            {
                ChangeMainSettleToCustom();
            }
        }

        private void Instance_OnOneSettleChange(OperationType operationType, string groupId, string id, List<string> settle)
        {
            if (operationType == OperationType.CardToTable)
            {
                ChangeMainSettleToCustom();
            }
        }

        private void SetUserDrag()
        {
            Thread t = new Thread(UpdateDrag);
            t.IsBackground = true;
            t.Start();
        }

        private void UpdateDrag()
        {
            try
            {
                Thread.Sleep(5000);

                this.Dispatcher.Invoke(() =>
                {
                    LogHelper.Info(string.Format("UserDrag drag function: {0}.", LocalEnv.ConfInfo.UserDrag));
                    if (!LocalEnv.ConfInfo.UserDrag)
                    {
                        tcPlugin.InterTabController = null;
                    }
                });
            }
            catch (Exception e)
            { }

        }

        private void TradeMainView_Loaded(object sender, RoutedEventArgs e)
        {
            SetUserDrag();//停用拖动功能

            this.Loaded -= TradeMainView_Loaded;

            if (LayoutSetttingCache.Instance.layoutSettingPo.ShowModel == 1)
            {
                rbListMode.IsChecked = true;
                DistinctBondsBtn.Visibility = Visibility.Visible;
                //lbPirceBorder.Visibility = Visibility.Collapsed;
                AdjustBondsBtn.Visibility = Visibility.Collapsed;
                ExportBtn.Visibility = Visibility.Visible;
            }
            else
            {
                rbCardMode.IsChecked = true;
                DistinctBondsBtn.Visibility = Visibility.Collapsed;
                //lbPirceBorder.Visibility = Visibility.Visible;
                AdjustBondsBtn.Visibility = Visibility.Visible;
                ExportBtn.Visibility = Visibility.Collapsed;
            }

            this.ReadHistory();

            if (_tradeMainViewModel != null)
                _tradeMainViewModel.Init(this);

            bool result = PermisionHandler.Instance.GetIdcTraderPermission();

            if (!result)
            {
                grid.RowDefinitions[2].Height = grid.RowDefinitions[3].Height = new GridLength(0);
            }
            else
                Bid_OrderBtn.Visibility = Ofr_OrderBtn.Visibility = Visibility.Visible;

            sfvSettle.SetMainHandle();


            //priceView.Init(TabShowModel.Table, string.Empty, null, null);
            priceView.Init(TabShowModel.Table, string.Empty, string.Empty);

            //marketView.Init(TabShowModel.Table, string.Empty);


            this.Instance_OnSettingChange(LayoutSetttingCache.Instance.GetLayoutSetting());

        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tb = sender as TextBlock;
            ComboBox cbb = tb.TemplatedParent as ComboBox;
            if (cbb != null)
                cbb.IsDropDownOpen = !cbb.IsDropDownOpen;
        }

        public void OnClosingItem(ItemActionCallbackArgs<TabablzControl> args)
        {
            GroupItems gi = (args.DragablzItem.DataContext as TabItem).Tag as GroupItems;
            string title = "删除分组";
            string text = "确定要删除标签页" + gi.GroupName + "吗?";//"是否继续删除?";

            HomeGroupInfo homeGroupInfo = HomePageDataHandler.Instance.GetHomeGroupInfo(gi.GroupId);

            if (homeGroupInfo != null && homeGroupInfo.securityids != null && homeGroupInfo.securityids.Count > 0)
            {
                text = "该分组下有债券信息, 如果删除该分组\r\n将同步删除已维护债券信息, \r\n请问是否继续?";
            }
            bool result = CustomMessageBox.Question(text, title);

            args.Cancel();

            if (result)
                HomePageDataHandler.Instance.DeleteGroup(gi.GroupId);

            if (tcPlugin.Items.Count == 0)
            {
                PriceAndOrderViewModel priceAndOrderViewModel = priceView.DataContext as PriceAndOrderViewModel;
                if (priceAndOrderViewModel != null)
                {
                    priceAndOrderViewModel.OnDelete();
                }
            }

        }

        private void CustomCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            // 添加组
            AddGroupWindow addGroupWindow = new AddGroupWindow();
            addGroupWindow.Owner = Application.Current.MainWindow;
            addGroupWindow.ShowDialog();
            if (addGroupWindow.Result)
            {
                if (addGroupWindow.Result == true)
                {
                    HomePageDataHandler.Instance.AddGroup(addGroupWindow.GroupName);
                }
            }
        }

        private void RbCardMode_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RbListMode_Checked(object sender, RoutedEventArgs e)
        {

        }

        private int _currentSelectedIndex = -1;

        private void tcPlugin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_currentSelectedIndex == tcPlugin.SelectedIndex || AppUtils.AppClosing)
                return;

            _currentSelectedIndex = tcPlugin.SelectedIndex;
            LogHelper.Warn("tcPlugin_SelectionChanged reset");
            WriteHistory();

            TabItem tabItem = tcPlugin.SelectedItem as TabItem;
            if (tabItem != null && tabItem.Content != null)
            {
                isFirstClickCard = true;
                TableAndCardView tableAndCardView = tabItem.Content as TableAndCardView;
                if (tableAndCardView != null)
                {
                    if (_isCardModel)
                    {
                        LogHelper.Warn("tcPlugin_SelectionChanged reset card");
                        RefreshTabItemShow(DisplayMode.Card, true, tabItem);
                        //RefreshTabItemShow(DisplayMode.Card, isFromSetting ? true : false, tabItem);

                        //PageCardView pageCardView = tableAndCardView.FindName("pageCardView") as PageCardView;
                        //GroupItems groupItem = tabItem.Tag as GroupItems;
                        //pageCardView.Init(groupItem.GroupId, false);//RefreshPage();
                    }
                    else
                    {
                        LogHelper.Warn("tcPlugin_SelectionChanged reset table");
                        RefreshTabItemShow(DisplayMode.List, false, tabItem);

                        PriceTableView priceTableView = tableAndCardView.FindName("dgPriceTableView") as PriceTableView;
                        if (priceTableView != null)
                            priceTableView.OnTabItemChange();
                    }
                }

                PriceTableViewModel priceTableViewModel = GetPriceTableViewModel();

                //切换全局过滤条件
                if (priceTableViewModel != null && MainConditionCache.Instance.IsContain(priceTableViewModel.GroupID))
                {
                    Dictionary<string, IList> dic = MainConditionCache.Instance.GetConditionById(priceTableViewModel.GroupID) as Dictionary<string, IList>;

                    _currentConditionDic = dic;

                    SetEllipseVisibility(dic);

                    if (dic != null)
                    {
                        if (dic.ContainsKey(CommonConstants.SETTLE))
                        {
                            List<string> allSettle = ConditionHelper.GetMainSettle(dic);
                            BondChangeHandler.Instance.OnMainSettleChange(OperationType.Main, priceTableViewModel.GroupID, allSettle);

                            btnCustomerSettle.Background = (Brush)this.FindResource("Border.Bottom.Background");
                        }
                        else
                        {
                            ChangeMainSettleToCustom();
                        }

                        if(dic.ContainsKey(CommonConstants.SECURITYID))
                        {
                            IList list = dic[CommonConstants.SECURITYID];

                            btnSearch.Visibility = Visibility.Collapsed;
                            seSearch.Visibility = Visibility.Visible;
                            _currentSecurityId = (list != null && list.Count > 0) ? list[0].ToString() : string.Empty;
                            seSearch.SearchBox.Text = _currentSecurityId;
                        }
                        else
                        {
                            btnSearch.Visibility = Visibility.Collapsed; //Visibility.Visible;
                            seSearch.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    _currentSecurityId = string.Empty;
                    _currentConditionDic = new Dictionary<string, IList>();
                    SetEllipseVisibility(null);

                    if (priceTableViewModel != null)
                    {
                        bool isContain = SettleCache.Instance.IsContain(priceTableViewModel.GroupID);
                        if (!isContain)
                        {
                            List<string> allSettle = new List<string>();
                            allSettle.Add(CommonConstants.ALL_PARTY);
                            BondChangeHandler.Instance.OnMainSettleChange(OperationType.Main, priceTableViewModel.GroupID, allSettle);

                            btnCustomerSettle.Background = (Brush)this.FindResource("Border.Bottom.Background");
                        }
                        else
                        {
                            sfvSettle.SetSelectedIndex(-1);

                            btnCustomerSettle.Background = (Brush)this.FindResource("Item.SelectedActive.Background");
                        }
                    }
                    btnSearch.Visibility = Visibility.Collapsed;
                    seSearch.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SettleFilterView_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            Dictionary<string, IList> dic = PriceCalHelper.Instance.CopyDictionary(sfvSettle.FilterDic);
            btnCustomerSettle.Background = (Brush)this.FindResource("Border.Bottom.Background");

            TabItem tabItem = tcPlugin.SelectedItem as TabItem;

            Dictionary<string, IList> result = GetCondition(dic);
            if (tabItem != null)
            {
                _currentConditionDic.Clear();
                _currentConditionDic = result;

                PriceTableViewModel priceTableViewModel = GetPriceTableViewModel();

                string groupId = GetGroupId();
                

                BondChangeHandler.Instance.OnMainSettleChange(OperationType.Card, groupId, result);

                //priceView.OnSelectSettleChange(OperationType.Table, result);
                //MainConditionCache.Instance.AddOrUpdate(groupId, _currentConditionDic);

                UpdateMainCondition();

                if (priceTableViewModel != null)
                {
                    priceTableViewModel.IsClickRowSettle = false;

                    

                    priceTableViewModel.ProcessMyOrdersFilterCondition(result);

                }


            }
        }

        private void BtnCustomerSettle_Click(object sender, RoutedEventArgs e)
        {
            if (_currentConditionDic.ContainsKey(CommonConstants.SETTLE))
                _currentConditionDic.Remove(CommonConstants.SETTLE);

            PriceTableViewModel priceTableViewModel = GetPriceTableViewModel();
            if (priceTableViewModel != null)
            {
                priceTableViewModel.SetRemenberSettle(_currentConditionDic);
                
                btnCustomerSettle.Background = (Brush)this.FindResource("Item.SelectedActive.Background");

                //通知卡片刷新
                BondChangeHandler.Instance.OnMainSettleChange(OperationType.RemerberToCard, priceTableViewModel.GroupID, _currentConditionDic);
            }

            

            //priceView.OnSelectSettleChange(OperationType.RemerberToCard, _currentConditionDic);

            sfvSettle.SetSelectedIndex(-1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ConditionWindow conditionWindow = new ConditionWindow();
            conditionWindow.SetCondition(_currentConditionDic);
            conditionWindow.ShowDialog();

            _moreConditionDic = conditionWindow.FilterDic;
            if (conditionWindow.Result)
            {

                SetEllipseVisibility(_moreConditionDic);

                Filter();

                //PriceTableViewModel priceTableViewModel = GetPriceTableViewModel();

                //if (priceTableViewModel != null)
                //    MainConditionCache.Instance.AddOrUpdate(priceTableViewModel.GroupID, _currentConditionDic);

                //BondChangeHandler.Instance.OnMainSettleChange(OperationType.Card, priceTableViewModel.GroupID, _currentConditionDic);

                //priceView.OnSelectSettleChange(OperationType.Table, _currentConditionDic);
            }
        }

        private void SeSearch_SearchChanged(object sender, object e)
        {
            SeBondPo seBondPo = e as SeBondPo;

            if (string.IsNullOrEmpty(seBondPo.BondCode))
            {
                btnSearch.Visibility = Visibility.Collapsed; //Visibility.Visible;
                seSearch.Visibility = Visibility.Collapsed;

                _currentSecurityId = string.Empty;
            }

            _currentSecurityId = seBondPo.BondCode;


            Filter();
            
            //PriceTableViewModel priceTableViewModel = GetPriceTableViewModel();

            //if (priceTableViewModel != null)
            //{
            //    MainConditionCache.Instance.AddOrUpdate(priceTableViewModel.GroupID, _currentConditionDic);

            //    BondChangeHandler.Instance.OnMainSettleChange(OperationType.Card, priceTableViewModel.GroupID, _currentConditionDic);

            //}
        }

        private void Filter()
        {
            PriceTableViewModel priceTableViewModel = GetPriceTableViewModel();

            Dictionary<string, IList> result = GetCondition(_currentConditionDic);

            _currentConditionDic.Clear();
            _currentConditionDic = result;

            if (priceTableViewModel != null)
                priceTableViewModel.ProcessMyOrdersFilterCondition(result);

            //CardViewModel cardViewModel = GetCardViewModel();
            //if (cardViewModel != null)
            //    cardViewModel.FilterData(result);

            string groupId = string.Empty;
            if (priceTableViewModel != null)
            {
                groupId = priceTableViewModel.GroupID;
            }
            else
            {
                PageCardViewModel pageCardViewModel = GetPageCardViewModel();
                if (pageCardViewModel != null)
                {
                    groupId = pageCardViewModel.CurrentGroupId;
                }
            }

            if (!string.IsNullOrEmpty(groupId))
            {
                MainConditionCache.Instance.AddOrUpdate(groupId, _currentConditionDic);

                BondChangeHandler.Instance.OnMainSettleChange(OperationType.Card, groupId, _currentConditionDic);
            }


            //NewCardPriceViewModel newCardPriceViewModel = GetNewCardPriceViewModel();
            //if (newCardPriceViewModel != null)
            //    newCardPriceViewModel.FilterData();
        }

        

        #region 模式切换

        private bool _isCardModel = false;

        private void Instance_OnSettingChange(object obj)
        {
            LogHelper.Warn("Instance_OnSettingChange");
            if (obj is LayoutSettingPo)
            {
                LogHelper.Warn("Instance_OnSettingChange LayoutSetting");
                LayoutSettingPo layoutSettingPo = obj as LayoutSettingPo;

                _isCardModel = layoutSettingPo.ShowModel == 2;

                _currentSelectedIndex = -1;

                if (!isInit)
                {
                    tcPlugin_SelectionChanged(null, null);

                    if ((layoutSettingPo.Row == 2 && layoutSettingPo.Col == 6) || (layoutSettingPo.Row == 3 && layoutSettingPo.Col == 7)
                        || (layoutSettingPo.Row == 5 && layoutSettingPo.Col == 4))
                    {
                        STC.Common.Data.DesignHandler.Instance.SetToMaxWin(null);
                    }
                    else
                    {
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                        double newwidth = layoutSettingPo.Col * 284 + 20;
                        double oldminwidth = Application.Current.MainWindow.MinWidth;
                        Application.Current.MainWindow.MinWidth = newwidth < 600 ? newwidth : oldminwidth;
                        Application.Current.MainWindow.Width = newwidth < 600 ? 640 : newwidth;
                        Application.Current.MainWindow.Height = layoutSettingPo.Row * 372 + 132;
                    }
                        


                    if (layoutSettingPo.Row == 1)
                    {
                        Application.Current.MainWindow.Height += 50;
                    }

                    TitleBorker_Click(null, null);
                }
                else
                {
                    isInit = false;
                }
            }
        }


        private void SetCardMode(bool refresh)
        {
            DistinctBondsBtn.Visibility = Visibility.Collapsed;
            //lbPirceBorder.Visibility = Visibility.Visible;
            AdjustBondsBtn.Visibility = Visibility.Visible;
            ExportBtn.Visibility = Visibility.Collapsed;

            long start = DateTime.Now.Ticks;
            gsVer.Visibility = gsHor.Visibility = btnHor.Visibility = btnVer.Visibility = borderPostionView.Visibility = borderPriceView.Visibility = Visibility.Collapsed;

            Grid.SetRowSpan(tcPlugin, 4);
            Grid.SetColumnSpan(tcPlugin, 3);
            long end = DateTime.Now.Ticks;

            Console.WriteLine("执行一次RbCardMode_Click耗时:{0} 秒" + (end - start) / 10000000.0);

            foreach (TabItem tabItem in tcPlugin.Items)
            {
                if (tcPlugin.SelectedItem == tabItem)
                {
                    GroupItems groupItems = tabItem.Tag as GroupItems;
                    TableAndCardView view = tabItem.Content as TableAndCardView;
                    view.SetShowModel(groupItems.GroupId, Model.DisplayMode.Card, refresh);
                }
            }

            _isCardModel = true;
        }

        private bool isFirstClickCard = true;
        private void RbCardMode_Click(object sender, RoutedEventArgs e)
        {
            SetCardMode(isFirstClickCard);
            isFirstClickCard = false;

            LayoutSetttingCache.Instance.layoutSettingPo.ShowModel = 2;
            LayoutSetttingCache.Instance.SaveModeSetting();
        }

        private void SetListMode(bool refresh)
        {
            DistinctBondsBtn.Visibility = Visibility.Visible;
            //lbPirceBorder.Visibility = Visibility.Collapsed;
            AdjustBondsBtn.Visibility = Visibility.Collapsed;
            ExportBtn.Visibility = Visibility.Visible;

            gsVer.Visibility = gsHor.Visibility = btnHor.Visibility = btnVer.Visibility = borderPostionView.Visibility = borderPriceView.Visibility = Visibility.Visible;

            Grid.SetRowSpan(tcPlugin, 2);
            Grid.SetColumnSpan(tcPlugin, 1);

            foreach (TabItem tabItem in tcPlugin.Items)
            {
                if (tcPlugin.SelectedItem == tabItem)
                {
                    GroupItems groupItems = tabItem.Tag as GroupItems;
                    TableAndCardView view = tabItem.Content as TableAndCardView;
                    view.SetShowModel(groupItems.GroupId, Model.DisplayMode.List, refresh);
                }
            }

            _isCardModel = false;
        }

        private void RefreshTabItemShow(DisplayMode displayMode, bool refresh, TabItem tabItem)
        {
            if (displayMode == DisplayMode.Card)
            {
                gsVer.Visibility = gsHor.Visibility = btnHor.Visibility = btnVer.Visibility = borderPostionView.Visibility = borderPriceView.Visibility = Visibility.Collapsed;

                Grid.SetRowSpan(tcPlugin, 4);
                Grid.SetColumnSpan(tcPlugin, 3);
                long end = DateTime.Now.Ticks;

                GroupItems groupItems = tabItem.Tag as GroupItems;
                TableAndCardView view = tabItem.Content as TableAndCardView;
                view.SetShowModel(groupItems.GroupId, Model.DisplayMode.Card, refresh);


            }
            else if (displayMode == DisplayMode.List)
            {
                gsVer.Visibility = gsHor.Visibility = btnHor.Visibility = btnVer.Visibility = borderPostionView.Visibility = borderPriceView.Visibility = Visibility.Visible;

                Grid.SetRowSpan(tcPlugin, 2);
                Grid.SetColumnSpan(tcPlugin, 1);


                GroupItems groupItems = tabItem.Tag as GroupItems;
                TableAndCardView view = tabItem.Content as TableAndCardView;
                view.SetShowModel(groupItems.GroupId, Model.DisplayMode.List, refresh);


            }

            positionloadMask.Visibility = Visibility.Collapsed;
        }

        private void RbListMode_Click(object sender, RoutedEventArgs e)
        {
            SetListMode(false);

            LayoutSetttingCache.Instance.layoutSettingPo.ShowModel = 1;
            LayoutSetttingCache.Instance.SaveModeSetting();
        }

        

        private bool _horClose = false;
       
        private bool _verClose = false;
        
        private void BtnHor_Click(object sender, RoutedEventArgs e)
        {
            if (!_horClose)
            {
                grid.RowDefinitions[3].Height = new GridLength(0);
                _horClose = true;

                btnHor.Content = "∧";
            }
            else
            {
                grid.RowDefinitions[1].Height = new GridLength(70, GridUnitType.Star);
                grid.RowDefinitions[3].Height = new GridLength(30, GridUnitType.Star);
                _horClose = false;

                btnHor.Content = "∨";
            }
        }

        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            if (!_verClose)
            {
                grid.ColumnDefinitions[2].Width = new GridLength(0);
                _verClose = true;

                btnVer.Content = "<";
                
            }
            else
            {
                grid.ColumnDefinitions[2].Width = new GridLength(340);
                _verClose = false;

                btnVer.Content = ">";
            }
        }

        #endregion

        private void SetPrice(bool isClickBid)
        {
            TabItem tabItem = tcPlugin.SelectedItem as TabItem;
            TableAndCardView tableAndCardView = tabItem.Content as TableAndCardView;
            if (tableAndCardView != null)
            {
                string securityID = string.Empty;
                string side = isClickBid ? "1" : "2";
                if (_isCardModel)
                {
                    CardView cardView = CommonUtils.FindVisualChild<CardView>(tableAndCardView);
                    if (cardView != null)
                    {
                        CardItem cardItem = cardView.GetFocusItem();
                        if (cardItem != null)
                        {
                            securityID = cardItem.Security;
                        }
                    }
                }
                else
                {
                    PriceTableView priceTableView = CommonUtils.FindVisualChild<PriceTableView>(tableAndCardView);
                    if (priceTableView != null)
                    {
                        DataRowView drv = priceTableView.GetFocusDataRow();
                        if (drv != null)
                        {
                            securityID = CommonUtils.ConvertToString(drv[PriceTableViewModel.SECURITY_ID_FIELD]);
                            
                        }
                    }
                }

                PriceMatchParam priceMatchParam = new PriceMatchParam();
                priceMatchParam.Side = side == "1" ? "BUY" : "SELL";
                priceMatchParam.Price = string.Empty;
                priceMatchParam.UpdataPrice = false;
                priceMatchParam.CustomFirmAmount = string.Empty;
                priceMatchParam.IsFirm = true;
                priceMatchParam.Symbol = securityID;
                priceMatchParam.NonfirmTime = string.Empty;
                priceMatchParam.OperationType = "1";
                //设置默认结算方式
                //priceMatchParam.SettlType = GetSettle(drv);//"2";
                //priceMatchParam.NetPrice = GetNetPrice(drv);
                SendPriceViewLite view = new SendPriceViewLite(priceMatchParam);

                view.Show();
            }
        }

        private void Bid_OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            SetPrice(true);
        }

        private void Ofr_OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            SetPrice(false);
        }

        private Dictionary<string, IList> _moreConditionDic = new Dictionary<string, IList>();
        private Dictionary<string, IList> _currentConditionDic = new Dictionary<string, IList>();
        private string _currentSecurityId = string.Empty;

        private bool CheckAll(IList list)
        {
            bool isAll = false;

            foreach (ListBoxItem lbi in list)
            {
                if (lbi.Content.ToString() == CommonConstants.ALL)
                {
                    isAll = true;
                    break;
                }
            }

            return isAll;
        }

        private void SetEllipseVisibility(Dictionary<string, IList> dic)
        {
            Ellipse obj = btnFilter.Template.FindName("elli", btnFilter) as Ellipse;
            if (dic != null)
            {
                if (obj != null)
                {
                    bool isAll = true;
                    if (dic.ContainsKey(CommonConstants.BROKER))
                    {
                        isAll = CheckAll(dic[CommonConstants.BROKER]);
                    }

                    if (isAll && dic.ContainsKey(CommonConstants.BOND_TYPE))
                        isAll = CheckAll(dic[CommonConstants.BOND_TYPE]);

                    if (isAll && dic.ContainsKey(CommonConstants.REMAINING_TIME))
                        isAll = CheckAll(dic[CommonConstants.REMAINING_TIME]);

                    if (isAll)
                        obj.Visibility = Visibility.Collapsed;
                    else
                        obj.Visibility = Visibility.Visible;

                }
            }
            else
            {
                if (obj != null)
                    obj.Visibility = Visibility.Collapsed;
            }
        }

        

        private Dictionary<string, IList> GetCondition(Dictionary<string, IList> dic)
        {
            Dictionary<string, IList> result = new Dictionary<string, IList>();
            
            foreach (KeyValuePair<string, IList> kv in dic)
            {
                result.Add(kv.Key, kv.Value);
            }

            //temDic.Clear();

            if (_moreConditionDic != null)
            {
                foreach (KeyValuePair<string, IList> kv in _moreConditionDic)
                {
                    if (result.ContainsKey(kv.Key))
                        result[kv.Key] = kv.Value;
                    else
                        result.Add(kv.Key, kv.Value);
                }
            }

            if (_currentConditionDic != null)
            {
                foreach (KeyValuePair<string, IList> kv in _currentConditionDic)
                {
                    if (result.ContainsKey(kv.Key))
                    {
                        //result[kv.Key] = kv.Value;
                    }
                    else
                        result.Add(kv.Key, kv.Value);
                }
            }

            if (!string.IsNullOrEmpty(_currentSecurityId))
            {
                List<string> list = new List<string>();
                list.Add(_currentSecurityId);
                if (result.ContainsKey(CommonConstants.SECURITYID))
                    result[CommonConstants.SECURITYID] = list;
                else
                    result.Add(CommonConstants.SECURITYID, list);
            }
            else
            {
                if (result.ContainsKey(CommonConstants.SECURITYID))
                    result.Remove(CommonConstants.SECURITYID);
            }

            return result;
        }

        private PriceTableViewModel GetPriceTableViewModel()
        {
            PriceTableViewModel result = null;
            TabItem tabItem = tcPlugin.SelectedItem as TabItem;

            if (tabItem != null)
            {
                PriceTableView tableView = (tabItem.Content as TableAndCardView).FindName("dgPriceTableView") as PriceTableView;

                if (tableView != null)
                {
                    result = tableView.DataContext as PriceTableViewModel;

                }
            }

            return result;
        }

        public string GetGroupId()
        {
            if (tcPlugin.SelectedItem != null)
            {
                TabItem tabItem = tcPlugin.SelectedItem as TabItem;
                if (tabItem != null)
                {
                    GroupItems groupItems = tabItem.Tag as GroupItems;
                    if (groupItems != null)
                    {
                        return groupItems.GroupId;
                    }
                }
            }

            return string.Empty;
        }

        private PageCardViewModel GetPageCardViewModel()
        {
            PageCardViewModel result = null;
            TabItem tabItem = tcPlugin.SelectedItem as TabItem;

            if (tabItem != null)
            {
                PageCardView cardView = (tabItem.Content as TableAndCardView).FindName("pageCardView") as PageCardView;

                if (cardView != null)
                {
                    result = cardView.DataContext as PageCardViewModel;

                }
            }

            return result;
        }


        private CardViewModel GetCardViewModel()
        {
            CardViewModel result = null;
            TabItem tabItem = tcPlugin.SelectedItem as TabItem;

            if (tabItem != null)
            {
                CardView cardView = (tabItem.Content as TableAndCardView).FindName("dgCardView") as CardView;

                if (cardView != null)
                {
                    result = cardView.DataContext as CardViewModel;

                }
            }

            return result;
        }

        //private NewCardPriceViewModel GetNewCardPriceViewModel()
        //{
        //    NewCardPriceViewModel result = null;
        //    TabItem tabItem = tcPlugin.SelectedItem as TabItem;

        //    if (tabItem != null)
        //    {
        //        NewCardPriceView cardView = (tabItem.Content as TableAndCardView).FindName("dgCardView") as NewCardPriceView;

        //        if (cardView != null)
        //        {
        //            result = cardView.DataContext as NewCardPriceViewModel;

        //        }
        //    }

        //    return result;
        //}





        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            btnSearch.Visibility = Visibility.Collapsed;
            seSearch.Visibility = Visibility.Visible;

            seSearch.SetFocus();
        }

        private void allBroker_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            Dictionary<string, IList> dic = PriceCalHelper.Instance.CopyDictionary(allBroker.FilterDic);
            
            TabItem tabItem = tcPlugin.SelectedItem as TabItem;

            Dictionary<string, IList> result = GetCondition(dic);
            if (tabItem != null)
            {
                _currentConditionDic.Clear();
                _currentConditionDic = result;

                PriceTableViewModel priceTableViewModel = GetPriceTableViewModel();

                string groupId = GetGroupId();
                

                BondChangeHandler.Instance.OnMainSettleChange(OperationType.Card, groupId, result);

                //priceView.OnSelectSettleChange(OperationType.Table, result);
                //MainConditionCache.Instance.AddOrUpdate(groupId, _currentConditionDic);

                UpdateMainCondition();

                if (priceTableViewModel != null)
                {
                    priceTableViewModel.IsClickRowSettle = false;

                    

                    priceTableViewModel.ProcessMyOrdersFilterCondition(result);

                }


            }
        }

        private void UpdateMainCondition()
        {
            foreach (TabItem tabItem in tcPlugin.Items)
            {
                GroupItems groupItems = tabItem.Tag as GroupItems;

                MainConditionCache.Instance.AddOrUpdate(groupItems.GroupId, _currentConditionDic);

            }
        }

        public void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            IEnumerable<DragablzItem> items = tcPlugin.GetOrderedHeaders();
            
            FrameworkElement frameworkElement = sender as FrameworkElement;

            double calWid = frameworkElement.ActualWidth - 500;

            double averageWid = calWid / (1.4 * (items.Count() + 1));

            double totalTabWid = 0;
            double unitWid = Math.Ceiling(1.4 * averageWid);
            foreach (DragablzItem tab in items)
            {
                if (unitWid > 106)
                {
                    tab.MinWidth = tab.Width = 106;
                }
                //else
                //{
                //    tab.MinWidth = tab.Width = unitWid;
                //}
                totalTabWid += tab.MinWidth;
            }
            double actualWidth = Math.Min(totalTabWid, calWid); 
            var hcc = CommonUtils.GetVisualChild<DragablzItemsControl>(tcPlugin, v => v.Name == "PART_HeaderItemsControl");
            if (hcc != null)
            {
                //hcc.Width = 500;
                hcc.MaxWidth = actualWidth;
            }

            spSearch.Margin = new Thickness(actualWidth, spSearch.Margin.Top, 0, 0);

            if (averageWid > 69)
            {
                btnCustomAdd.Width = 69;
                AddBondsBtn.Width = 69;
                DistinctBondsBtn.Width = 69;
                AdjustBondsBtn.Width = 69;
            }
            else
            {
                btnCustomAdd.Width = 48;
                AddBondsBtn.Width = 48;
                DistinctBondsBtn.Width = 48;
                AdjustBondsBtn.Width = 48;
            }

            //sfvSettle.ChangeSize((int)averageWid*5);

            //allBroker.ChangeSize((int)averageWid * 7);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            TestWin testWin = new TestWin();
            testWin.Show();
        }

        private void AddBondsBtn_Click(object sender, RoutedEventArgs e)
        {
            string currentGroupId = GetGroupId();
            if (!string.IsNullOrEmpty(currentGroupId))
            {
                var ExistWin = WindowManager.Instance.GetExistWin("BatchImportWin");
                if (ExistWin != null && ExistWin is RecognizeIntentionQuoteWin)
                {
                    (ExistWin as RecognizeIntentionQuoteWin).SetGroupId(currentGroupId);
                }
                if (WindowManager.Instance.IsExist("BatchImportWin") == false)
                {
                    RecognizeIntentionQuoteWin win = new RecognizeIntentionQuoteWin();
                    win.SetGroupId(currentGroupId);
                    //win.ShowDialog();
                    WindowManager.Instance.OpenOnlyOneWindow("BatchImportWin", win);
                }
            }
        }

        private void DistinctBondsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CustomMessageBox.Question("是否删除重复债券?", "去重") == false)
            {
                return;
            }

            DisplayMode displayMode = DisplayMode.Card;
            if (rbListMode.IsChecked.Value)
            {
                displayMode = DisplayMode.List;
            }

            string currentGroupId = GetGroupId();
            IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> sortedGroups = HomePageDataHandler.Instance.RequestGroups(false);
            foreach(KeyValuePair<string, HomeGroupInfo> pair in sortedGroups)
            {
                if (pair.Key == currentGroupId)
                {
                    // 筛选重复数据
                    List<HomeGroupSecurityIDsInfo> securityids = pair.Value.securityids;
                    Dictionary<string, List<HomeGroupSecurityIDsInfo>> dic = new Dictionary<string, List<HomeGroupSecurityIDsInfo>>();
                    foreach(HomeGroupSecurityIDsInfo securityIdInfo in securityids)
                    {
                        if (dic.ContainsKey(securityIdInfo.securityid) == false)
                        {
                            dic[securityIdInfo.securityid] = new List<HomeGroupSecurityIDsInfo>();
                        }
                        dic[securityIdInfo.securityid].Add(securityIdInfo);
                    }

                    foreach (List<HomeGroupSecurityIDsInfo> list in dic.Values)
                    {
                        if (list.Count > 1)
                        {
                            for (int i = 0; i < list.Count - 1; i++)
                            {
                                HomeGroupSecurityIDsInfo delSecurity = list[i];
                                HomePageDataHandler.Instance.DelBond(displayMode, currentGroupId, delSecurity.id, delSecurity.securityid);
                            }
                        }
                    }
                }
            }
        }

        private void TitleBorker_Click(object sender, RoutedEventArgs e)
        {
            if (tcPlugin == null || tcPlugin.SelectedItem == null)
            {
                return;
            }

            Dictionary<string, IList> dic = new Dictionary<string, IList>();
            IList list = new List<int>();
            dic[CommonConstants.EXECTYPE] = list;

            int execType = 0;
            if (cbChart.IsChecked.Value)
            {
                execType = 1;
            }
            list.Add(execType);

            TabItem tabItem = tcPlugin.SelectedItem as TabItem;

            Dictionary<string, IList> result = GetCondition(dic);
            if (tabItem != null)
            {
                _currentConditionDic.Clear();
                _currentConditionDic = result;

                string groupId = GetGroupId();

                BondChangeHandler.Instance.OnMainSettleChange(OperationType.Card, groupId, result);

                UpdateMainCondition();
            }
        }

        private void AdjustBondsBtn_Click(object sender, RoutedEventArgs e)
        {
            string groupId = GetGroupId();
            QuoteConfigHandler.Instance.UpdateConfigByGroupId(groupId, null, null, FavoriteTypeEnum.Adjust);
            SetCardMode(true);
        }

        private void ExportBtn_Click(object sender, RoutedEventArgs e)
        {
            TableAndCardView tableAndCardView = (this.tcPlugin.SelectedItem as TabItem).Content as TableAndCardView;
            PriceTableView priceTableView = tableAndCardView.FindName("dgPriceTableView") as PriceTableView;
            if (priceTableView != null && priceTableView.DataContext is PriceTableViewModel)
            {
                PriceTableViewModel vm = priceTableView.DataContext as PriceTableViewModel;
                vm.ExportTXT();
            }
        }
    }
}
