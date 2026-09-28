using com.gateway.log;
using STC.Broker.Plugin.Handler;
using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.Model.Card;
using STC.Broker.Plugin.View.Component;
using STC.Broker.Plugin.View.HomePage;
using STC.Broker.Plugin.View.Price;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Command;
using STC.Common.Data;
using STC.Common.DataProcesser;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.PT;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace STC.Broker.Plugin.ViewModel.Price
{
    public class PricingPanelViewModel : BaseViewModel
    {
        #region 定义
        private string _groupID = string.Empty;
        public string groupID
        {
            get
            {
                return _groupID;
            }
            set
            {
                _groupID = value;
                this.OnPropertyChanged();
            }
        }

        private string _groupname;
        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName
        {
            get { return _groupname; }
            set { _groupname = value; this.OnPropertyChanged(); }
        }

        private object lockObj = new object();

        private Dictionary<int, ObservableCollection<CardItem>> _dictcardItems = new Dictionary<int, ObservableCollection<CardItem>>();
        public Dictionary<int, ObservableCollection<CardItem>> DictCardItems
        {
            get
            {
                return _dictcardItems;
            }
            set
            {
                _dictcardItems = value;
                this.OnPropertyChanged(nameof(DictCardItems));
            }
        }

        private string _dragTxt = string.Empty;

        public string DragTxt { get { return _dragTxt; } set { _dragTxt = value; this.OnPropertyChanged(); } }

        private HomePageView _mywin = null;
        /// <summary>
        /// 行情看板浏览面板
        /// </summary>
        public HomePageView mywin 
        { 
            get 
            { 
                return _mywin;
            }
            set
            {
                _mywin = value;
                this.OnPropertyChanged();
            }
        }

        private QuotePageModel curQuoteConfigPo;
        /// <summary>
        /// 配置
        /// </summary>
        public QuotePageModel CurQuoteConfigPo
        {
            get
            {
                return curQuoteConfigPo;
            }
            set
            {
                curQuoteConfigPo = value;
                this.OnPropertyChanged();
            }
        }

        private int row = 2;
        public int Rows
        {
            get
            {
                return row == 0 ? 2 : row;
            }
            set
            {
                row = value;
                this.OnPropertyChanged(nameof(Rows));
            }
        }

        private int col = 5;
        public int Cols
        {
            get
            {
                return col == 0 ? 5 : col;
            }

            set
            {
                col = value;
                this.OnPropertyChanged(nameof(Cols));
            }
        }


        public PricingPanel MainView;
        private FrameworkElement _ownUI = null;


        private int _curPageIndex = 0;
        public int CurPageIndex
        {
            get
            {
                return this._curPageIndex;
            }
            set
            {
                if (value != _curPageIndex)
                {
                    this._curPageIndex = value;
                    base.OnPropertyChanged("CurPageIndex");
                    //this.CardItemsMv.View.Refresh();
                }
                ////if (CurPageIndex>=0)
                ////    (MainView as PricingPanel).FavoriteListBox.ItemsSource = DictCardItems[value];
            }
        }


        #endregion

        #region Command
        /// <summary>
        /// 添加页
        /// </summary>
        public ICommand AddPageCommand { get; set; }
        /// <summary>
        /// 上一页
        /// </summary>
        public ICommand PrevPageCommand { get; set; }
        /// <summary>
        /// 下一页
        /// </summary>
        public ICommand NextPageCommand { get; set; }
        public ICommand TestCommand { get; set; }

        private ObservableCollection<string> pageButtonList = new ObservableCollection<string>();
        /// <summary>
        /// 当前TabPage页码列表
        /// </summary>
        public ObservableCollection<string> PageButtonList
        {
            get
            {
                return pageButtonList;
            }
            set
            {
                pageButtonList = value;
                this.OnPropertyChanged(nameof(PageButtonList));
            }
        }

        private int pageSelectIndex = 0;
        /// <summary>
        /// 当前页面的索引
        /// </summary>
        public int PageSelectIndex
        {
            get
            {
                return pageSelectIndex;
            }
            set
            {
                pageSelectIndex = value;
                this.OnPropertyChanged();

                PrevPageEnable = pageSelectIndex == 0 ? false : true;
                NextPageEnable = pageSelectIndex == PageButtonList.Count - 1 ? false : true;

                CurPageIndex = PageSelectIndex;
                
            }
        }


        private bool _PrevPageEnable = true;

        public bool PrevPageEnable 
        { 
            get 
            { return _PrevPageEnable; 
            } 
            set 
            { 
                _PrevPageEnable = value; 
                this.OnPropertyChanged(); 
            } 
        }

        private bool _NextPageEnable = true;

        public bool NextPageEnable 
        { 
            get 
            { 
                return _NextPageEnable; 
            } 
            set 
            { 
                _NextPageEnable = value; 
                this.OnPropertyChanged(); 
            } 
        }

        private Visibility _AddPageVisisble = Visibility.Visible;
        public Visibility AddPageVisisble
        {
            get
            {
                return _AddPageVisisble;
            }
            set
            {
                _AddPageVisisble = value;
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

        #region 优化算法
        private ObservableCollection<CardItem> _CardItems = new ObservableCollection<CardItem>();
        /// <summary>
        /// 所有的CardItems
        /// </summary>
        public ObservableCollection<CardItem> CardItems
        {
            get
            {
                return _CardItems;
            }
            set
            {
                _CardItems = value;
                this.OnPropertyChanged(nameof(_CardItems));
            }
        }

        private HomeGroupInfo hginfo;
        /// <summary>
        /// 订单明细分页是否可以见
        /// </summary>
        public HomeGroupInfo HGInfo
        {
            get
            {
                return hginfo;
            }

            set
            {
                hginfo = value;
                this.OnPropertyChanged(nameof(HGInfo));
            }
        }

        private object dictObj = new object();

        ////private CollectionViewSource _CardItemsMv = new CollectionViewSource();
        ////public CollectionViewSource CardItemsMv
        ////{
        ////    get 
        ////    { 
        ////        return _CardItemsMv; 
        ////    }
        ////    set
        ////    {
        ////        _CardItemsMv = value;
        ////        this.OnPropertyChanged(nameof(CardItemsMv));
        ////    }
        ////}
        #endregion

        #endregion


        public PricingPanelViewModel()
        {
            AddPageCommand = new DelegateCommand(new Action<object>(OnAddPageCommand));
            PrevPageCommand = new DelegateCommand(new Action<object>(OnPrevPageCommand));
            NextPageCommand = new DelegateCommand(new Action<object>(OnNextPageCommand));
            TestCommand = new DelegateCommand(new Action<object>(OnTestCommand));
            //windows tab切换
            //DesignHandler.Instance.OnTabChange += Instance_OnTabChange;
            HomePageDataHandler.OnFavoritesChanged += HomePageDataHandler_OnFavoritesChanged;
            //////全部
            ////this.CardItemsMv.Source = CardItems;
            ////this.CardItemsMv.IsLiveFilteringRequested = true;
            ////this.CardItemsMv.IsLiveSortingRequested = true;

            ////this.CardItemsMv.SortDescriptions.Add(new SortDescription("Page", ListSortDirection.Ascending));
            ////this.CardItemsMv.SortDescriptions.Add(new SortDescription("CustomSort", ListSortDirection.Ascending));
        }

       

        public void Init(FrameworkElement ownUI, HomeGroupInfo gi, HomePageView hgv)
        {
            
            _ownUI = ownUI;
            mywin = hgv;
            HGInfo = gi;
            groupID = gi.id;
            GroupName = gi.groupname;
            DictCardItems.Clear();
            this.Cols = LocalEnv.GrLayoutSetting.Col;
            this.Rows = LocalEnv.GrLayoutSetting.Row;

            BindSecurtitiesFirst();

            //接收borker事件
            QuoteConfigHandler.Instance.OnBondsChanged += Instance_OnBondsChanged;
        }

        /// <summary>
        /// 当债券信息发生变化时，保存到配置中
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <param name="arg4"></param>
        private void Instance_OnBondsChanged(HomePageDataHandler.FavoriteTypeEnum favorityType, string groupID, string securityID, CardItem cardItem)
        {
            if (favorityType == HomePageDataHandler.FavoriteTypeEnum.AddBond || favorityType == HomePageDataHandler.FavoriteTypeEnum.DelBond || favorityType == HomePageDataHandler.FavoriteTypeEnum.BatchImport || favorityType == HomePageDataHandler.FavoriteTypeEnum.UpdBond)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (groupID == this.groupID)
                    {
                        foreach (QuoteGroupConfigModel po in CurQuoteConfigPo.Panels)
                        {
                            if (po.Id == cardItem.Id)
                            {
                                if (favorityType == HomePageDataHandler.FavoriteTypeEnum.DelBond)
                                {
                                    po.SecurityID = string.Empty;
                                    po.SymbolName = "";
                                    po.BondCategory = "";
                                    po.BondType = "";
                                    po.RemaingingTime = "";
                                    po.RemaingingTimeConvert = 0;

                                }
                                else if (favorityType == HomePageDataHandler.FavoriteTypeEnum.AddBond)
                                {
                                    po.SecurityID = securityID;
                                    po.SymbolName = cardItem.SymbolName;
                                    po.BondCategory = cardItem.BondCategory;
                                    po.BondType = cardItem.BondType;
                                    po.RemaingingTime = cardItem.RemaingingTime;
                                    po.RemaingingTimeConvert = cardItem.RemaingingTimeConvert;
                                    AddPageVisisble = Visibility.Visible;
                                }
                                else if (favorityType == HomePageDataHandler.FavoriteTypeEnum.UpdBond)
                                {
                                    po.SettleIndex = cardItem.SettleIndex;
                                    po.SecurityID = securityID;
                                    po.SymbolName = cardItem.SymbolName;
                                    po.BondCategory = cardItem.BondCategory;
                                    po.BondType = cardItem.BondType;
                                    po.RemaingingTime = cardItem.RemaingingTime;
                                    po.RemaingingTimeConvert = cardItem.RemaingingTimeConvert;
                                }
                            }
                        }

                        //判断是否当前页面是否都是空的，如果都是空的，则删除该页面
                        TabControl tabcontrol = (this.MainView as PricingPanel).FindName("Tab_DgPanel") as TabControl;
                        int removePageIndex = -1;
                        foreach (TabItem ti in tabcontrol.Items)
                        {
                            string truepagetag = ti.Tag.ToString();
                            removePageIndex++;
                            if (truepagetag == cardItem.Page.ToString())
                            {
                                break;
                            }
                        }


                        int cnt = CurQuoteConfigPo.Panels.Where(p => p.Page == removePageIndex).ToList().Count(o => string.IsNullOrEmpty(o.SecurityID));

                        List<int> PageList = CurQuoteConfigPo.Panels.Select(p => p.Page).Distinct().ToList();
                        if (cnt == CurQuoteConfigPo.Panels.Where(p => p.Page == removePageIndex).ToList().Count)
                        {
                            if (PageList.Count > 1)
                            {
                                int pageindex = PageList.Count - 1;
                                PageButtonList.RemoveAt(pageindex);
                                PageSelectIndex = 0;
                                AddPageVisisble = Visibility.Visible;

                                TabControl curtabcontrol = (this.MainView as PricingPanel).FindName("Tab_DgPanel") as TabControl;
                                curtabcontrol.Items.RemoveAt(removePageIndex);
                            
                                List<QuoteGroupConfigModel> itemsToRemove = CurQuoteConfigPo.Panels.Where(p => p.Page == removePageIndex).ToList();
                                foreach (QuoteGroupConfigModel item in itemsToRemove)
                                {
                                    CurQuoteConfigPo.Panels.Remove(item);
                                }

                                //当删除页面时保存配置
                                SaveLastQuoteConfig();
                            }

                            //重置 CurQuoteConfigPo 的页码
                            CurQuoteConfigPo = CardInfoHandler.Instance.ResetCurQuoteConfigPageNo(this.groupID, this.GroupName, CurQuoteConfigPo);

                           
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 重新更新界面
        /// </summary>
        public void ReSetSize()
        {
            SaveLastQuoteConfig();

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (( this.Cols == LocalEnv.GrLayoutSetting.Col) && (this.Rows == LocalEnv.GrLayoutSetting.Row))
                {
                    return;
                }

                this.Cols = LocalEnv.GrLayoutSetting.Col;
                this.Rows = LocalEnv.GrLayoutSetting.Row;

                //首先查询配置
                if (CurQuoteConfigPo != null)
                {
                    CurQuoteConfigPo = CardInfoHandler.Instance.ResetCurQuoteConfigPo(this.groupID, this.GroupName, this.CardItems);
                }
                else
                {
                    IOrderedEnumerable<KeyValuePair<string, HomeGroupInfo>> groupList = HomePageDataHandler.RequestGroups(false);

                    foreach (KeyValuePair<string, HomeGroupInfo> group in groupList)
                    {
                        if (group.Key == this.groupID)
                        {
                            CurQuoteConfigPo = CardInfoHandler.Instance.ResetQuote(this.groupID, this.GroupName, group.Value.securityids);
                            break;
                        }
                    }
                }

                //保存配置
                SaveLastQuoteConfig();

                List<int> PageList = CurQuoteConfigPo.Panels.Select(p => p.Page).Distinct().ToList();
                //删除或增加TabControl
                TabControl tabcontrol = (this.MainView as PricingPanel).FindName("Tab_DgPanel") as TabControl;
                //删除  6  4
                int itemcount = tabcontrol.Items.Count; //3
                int pagecount = PageList.Count;  // 2

                if (itemcount > pagecount)
                {
                    for (int i = pagecount; i < itemcount; i++)
                    {
                        tabcontrol.Items.RemoveAt(i);
                        itemcount--;
                        i--;
                    }
                }

                //增加 TabPage
                //2 3
                if (itemcount < pagecount)
                {
                    for (int p = itemcount; p < pagecount; p++)
                    {
                        ListViewControl listView = new ListViewControl();
                        TabItem Ti = new TabItem();
                        Ti.Header = p;
                        Ti.Height = 0;
                        Ti.Content = listView;
                        tabcontrol.Items.Add(Ti);
                    }
                }

                //重置页码
                PageButtonList.Clear();
                for (int p = 0; p < PageList.Count; p++)
                {
                    PageButtonList.Add((p + 1).ToString());
                }

                DictCardItems.Clear();

                for (int p = 0; p < PageList.Count; p++)
                {
                    DictCardItems.Add(p, new ObservableCollection<CardItem>());
                }

                for (int p = 0; p < PageList.Count; p++)
                {
                    foreach (QuoteGroupConfigModel po in CurQuoteConfigPo.Panels)
                    {
                        if (po.Page == p)
                        {
                            CardItem cardItem = CardItems.Where(u => u.Id == po.Id).FirstOrDefault();
                            if (cardItem != null)
                            {
                                cardItem.CustomSort = po.CustomSort;
                                cardItem.Page = po.Page;
                                DictCardItems[p].Add(cardItem);
                            }
                            else
                            {
                                cardItem = new CardItem();
                                cardItem.GroupId = po.GroupId;
                                cardItem.Id = po.Id;
                                cardItem.Page = po.Page;
                                cardItem.ParentId = po.ParentId;
                                cardItem.SecurityID = po.SecurityID;
                                cardItem.SettleIndex = po.SettleIndex;
                                cardItem.SymbolName = po.SymbolName;
                                cardItem.RemaingingTime = po.RemaingingTime;
                                cardItem.RemaingingTimeConvert = po.RemaingingTimeConvert;
                                cardItem.CustomSort = po.CustomSort;
                                cardItem.CurCardPriceView = new CardPriceView();
                                cardItem.CurCardPriceView.DataContext = cardItem;
                                DictCardItems[p].Add(cardItem);
                            }
                        }
                    }
                }


                //重新设置绑定
                for (int p = 0; p < tabcontrol.Items.Count; p++)
                {
                    TabItem Ti = tabcontrol.Items[p] as TabItem;
                    ListViewControl listView = Ti.Content as ListViewControl;
                    listView.FavoriteListBox.ItemsSource = null;
                    listView.FavoriteListBox.ItemsSource = DictCardItems[p];
                    Ti.Header = p;
                    Ti.Tag = p.ToString();
                }

                PageSelectIndex = 0;
            });
        }

        /// <summary>
        /// 退出时保存最后的布局
        /// </summary>
        public void SaveLastQuoteConfig()
        {
            try
            {
                //更新保存配置
                CardInfoHandler.Instance.SaveConfig(CurQuoteConfigPo, this.groupID);
            }
            catch { }
        }

        /// <summary>
        /// 保存最后的布局到个人配置中
        /// </summary>
        public void SaveGrUserFavoritesSettings()
        {
            try
            {
                CardInfoHandler.Instance.SaveGrUserFavoritesSettings(CurQuoteConfigPo);
            }
            catch { }
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        public void BindSecurtitiesFirst()
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                LoadMaskVisible = Visibility.Visible;
                //首先查询配置
                CurQuoteConfigPo = CardInfoHandler.Instance.QueryQuotePageConfig(this.groupID);
                if (CurQuoteConfigPo == null)
                {
                    //如果本地没有配置，则从服务器中获取
                    List<QuotePageModel> list  = CardInfoHandler.Instance.QueryGrUserFavoritesSettings(this.groupID);
                    if (list.Count > 0)
                        CurQuoteConfigPo = list[0];
                    else
                        CurQuoteConfigPo = CardInfoHandler.Instance.ResetQuote(this.groupID, this.GroupName, HGInfo.securityids);
                }


                PageButtonList.Clear();
                DictCardItems.Clear();

                List<string> dictkey = CurQuoteConfigPo.Panels.Where(x => !string.IsNullOrEmpty(x.SecurityID)).ToList().Select(p => p.SecurityID).Distinct().ToList();

                List<int> PageList = CurQuoteConfigPo.Panels.Select(p => p.Page).Distinct().ToList();

                TabControl tabcontrol = (this.MainView as PricingPanel).FindName("Tab_DgPanel") as TabControl;
                for (int p = 0; p < PageList.Count; p++)
                {
                    PageButtonList.Add((p + 1).ToString());
                    DictCardItems.Add(p, new ObservableCollection<CardItem>());
                    ListViewControl listView = (tabcontrol.Items[p] as TabItem).Content as ListViewControl;
                    listView.FavoriteListBox.ItemsSource = DictCardItems[p];

                    ////TabItem Ti = new TabItem();
                    ////Ti.Header = p;
                    ////Ti.Height = 0;
                    ////Ti.Content = listView;
                    ////Ti.Tag = p.ToString();
                    ////tabcontrol.Items.Add(Ti);
                }

                PageSelectIndex = 0;


                ThreadPoolHelper.InvokeThreadPool((P) =>
                {
                    Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(()=>
                    {
                        for (int p = 0; p < PageList.Count; p++)
                        {
                            int sortid = 0;
                            foreach (QuoteGroupConfigModel po in CurQuoteConfigPo.Panels)
                            {

                                if (po.Page == p)
                                {
                                    CardItem cardItem = new CardItem();
                                    cardItem.Id = po.Id;
                                    cardItem.GroupId = this.groupID;
                                    cardItem.ParentId = po.ParentId;
                                    cardItem.SecurityID = po.SecurityID ?? "";
                                    cardItem.SymbolName = po.SymbolName ?? "";
                                    cardItem.RemaingingTimeConvert = po.RemaingingTimeConvert;
                                    cardItem.BondCategory = po.BondCategory;
                                    cardItem.BondType = po.BondType;
                                    cardItem.CustomSort = sortid;
                                    cardItem.Page = po.Page;
                                    cardItem.SettleIndex = po.SettleIndex;
                                    sortid++;
                                    cardItem.CurCardPriceView = new CardPriceView();
                                    cardItem.CurCardPriceView.DataContext = cardItem;
                                    CardItems.Add(cardItem);
                                    DictCardItems[p].Add(cardItem);
                                }
                            }
                        }
                    }));

                    //for (int p = 0; p < PageList.Count; p++)
                    //{
                    //    //DictCardItems[p] = new ObservableCollection<CardItem>() { CardItems.Where(o=>o.Page == p).ToList()};
                    //}

                });
            }));
        }

        #region  Command Proc

        /// <summary>
        /// 如果删除组
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        private void HomePageDataHandler_OnFavoritesChanged(HomePageDataHandler.FavoriteTypeEnum arg1, string arg2, string arg3)
        {
            if (arg1 == HomePageDataHandler.FavoriteTypeEnum.AddGroup)
            {
                if (groupID == arg2)
                {
                    CardInfoHandler.Instance.SaveConfig(CurQuoteConfigPo, arg2);
                }
            }

            if (arg1 == HomePageDataHandler.FavoriteTypeEnum.UpdateGroup)
            {
                if (groupID == arg2)
                {
                    CurQuoteConfigPo.GroupName = arg3;
                    CardInfoHandler.Instance.UpdateConfig(CurQuoteConfigPo, arg2);
                }
            }

            if (arg1 == HomePageDataHandler.FavoriteTypeEnum.DelGroup)
            {
                CardInfoHandler.Instance.DelAndSaveConfig(arg2);
            }
        }

        /// <summary>
        /// 添加页
        /// </summary>
        /// <param name="inparam"></param>
        public void OnAddPageCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                int pageCount = LocalEnv.GrLayoutSetting.Row * LocalEnv.GrLayoutSetting.Col;
                int cnt = CurQuoteConfigPo.Panels.Where(p => p.Page == CurPageIndex).ToList().Count(o => string.IsNullOrEmpty(o.SecurityID));
                if (cnt == pageCount)
                {
                    DeleteBoxView msgBox = new DeleteBoxView();
                    msgBox.Owner = Application.Current.MainWindow;
                    msgBox.setWarnMsg("当前收藏夹至少应该收藏一个债券！");
                    msgBox.ShowDialog();
                    return;
                }

                AddPageVisisble = Visibility.Collapsed;

                
                int addPageNum = DictCardItems.Count + 1;  //删除页面后，该变量不会清空
                int truepageNum = PageButtonList.Count + 1;
                PageButtonList.Add(truepageNum.ToString());
                if (truepageNum > 0)
                    PageSelectIndex = truepageNum - 1;

                DictCardItems.Add(DictCardItems.Count, new ObservableCollection<CardItem>());
                for (int i = 0; i < pageCount; i++)
                {
                    QuoteGroupConfigModel panelPo = new QuoteGroupConfigModel();
                    panelPo.GroupId = this.groupID;
                    panelPo.Page = addPageNum;   //后面代码会重新排序
                    panelPo.CustomSort = i;
                    CurQuoteConfigPo.Panels.Add(panelPo);

                    CardItem ci = new CardItem();
                    ci.SecurityID = "";
                    ci.SymbolName = "";
                    ci.Id = panelPo.Id;
                    ci.GroupId = this.groupID;
                    ci.CustomSort = i;
                    ci.Page = PageSelectIndex;
                    ci.CurCardPriceView = new CardPriceView();
                    ci.CurCardPriceView.DataContext = ci;
                    DictCardItems[DictCardItems.Count - 1].Add(ci);
                }

                ////重置 CurQuoteConfigPo 的页码
                CurQuoteConfigPo = CardInfoHandler.Instance.ResetCurQuoteConfigPageNo(this.groupID, this.GroupName, CurQuoteConfigPo);
                SaveLastQuoteConfig();

                TabControl curtabcontrol = (this.MainView as PricingPanel).FindName("Tab_DgPanel") as TabControl;
                ListViewControl listView = new ListViewControl();
                listView.FavoriteListBox.ItemsSource = DictCardItems[DictCardItems.Count - 1];
                TabItem Ti = new TabItem();
                Ti.Header = DictCardItems.Count - 1;
                Ti.Tag = PageSelectIndex;
                Ti.Height = 0;
                Ti.Content = listView;
                curtabcontrol.Items.Add(Ti);

            }));
        }


        
        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="inparam"></param>
        public void OnPrevPageCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                if (PageSelectIndex > 0)
                    PageSelectIndex --;
            }));
        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="inparam"></param>
        public void OnNextPageCommand(object inparam)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(async () =>
            {
                if (PageSelectIndex < PageButtonList.Count)
                    PageSelectIndex++;
            }));
        }


        #region 测试
        public void OnTestCommand(object inparam)
        {
            string s = FileHelper.ReadFullText("d:\\1.txt"); //441822929 244462757
            List <ExecutionReport> list = new List<ExecutionReport>() { CommonUtils.DeserializeObject<ExecutionReport>(s)};
            PopupERCollect popupERCollect = new PopupERCollect();
            popupERCollect.list.AddRange(list);
            popupERCollect.isUpdate = true;
            popupERCollect.isFromHistory = false;
            LogHelper.Info($"OnTestCommand:{Common.Utils.DateTimeEx.GetNow().ToString("yyyy/mm/dd HH:mm:ss.fff")}");
            Common.Handler.PT.TransactionPopupHandler.Instance.InsertEnqueue(popupERCollect);
        }
        #endregion

        #endregion

        #region Dispose
        ~PricingPanelViewModel()
        {
            this.Dispose();
        }

        new public void Dispose()
        {
            
        }

        #endregion

        
    }
}
