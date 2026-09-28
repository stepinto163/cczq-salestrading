using STC.Common.Model.Quant;
using STC.Common.Utils;
using STC.quant.plugin.Utils;
using STC.quant.plugin.Utils.Data;
using STC.quant.plugin.View.HomePage;
using STC.quant.plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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

namespace STC.quant.plugin.View
{
    /// <summary>
    /// UserControl2.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl
    {
        private MainViewModel _MainViewModel = new MainViewModel();

        public static volatile bool IsInited = false;

        private MMListBoxView mmlist = new MMListBoxView();

        public MainView()
        {
            InitializeComponent();
            mmlist.QuantListView.SelectionChanged += MMListView_SelectionChanged;
            this.DataContext = _MainViewModel;
        }

        private void MM_MainView_Initialized(object sender, EventArgs e)
        {
           
        }


        private void GroupTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.GroupTabControl.SelectedItem != null
                 && this.GroupTabControl.SelectedItem is HomePageTabItemWithClose)
            {
                HomePageTabItemWithClose selectedTagItem = this.GroupTabControl.SelectedItem as HomePageTabItemWithClose;

                TTbookQuantGroups gi = selectedTagItem.Tag as TTbookQuantGroups;
                LogHandler.Info($"--------click tab1 {gi.Groupname}:{gi.Id}:{selectedTagItem.IsVisible}");

                if (e.RemovedItems != null && e.RemovedItems.Count > 0)
                {
                    if (e.RemovedItems[0] is HomePageTabItemWithClose)
                    {
                        (e.RemovedItems[0] as HomePageTabItemWithClose).Content = null;
                    }
                }


                if (selectedTagItem.Content == null
                    || (mmlist.DataContext != null && mmlist.DataContext is MMListBoxViewModel && (mmlist.DataContext as MMListBoxViewModel).groupID != gi.Id))
                {
                    if (selectedTagItem.Tag != null && selectedTagItem.Tag is TTbookQuantGroups)
                    {

                        LogHandler.Info($"--------{DateTime.Now}--:GroupTabControl_SelectionChanged");
                        //MMGroupInfo gi = selectedTagItem.Tag as MMGroupInfo;
                        LogHandler.Info($"--------click tab {gi.Groupname}");
                        selectedTagItem.Content = mmlist;

                        mmlist.DataContext = _MainViewModel?.GetMMListBoxViewModelByGroupID(gi.Id);
                        mmlist.viewmodel = mmlist.DataContext as MMListBoxViewModel;
                      
                        this.OrderControl.DataContext = mmlist.viewmodel;
                        this.QuantIndicatort.DataContext = mmlist.viewmodel;
                        this.QuantLog.DataContext = mmlist.viewmodel;

                    }
                }
            }
        }


        private void MMListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.OrderControl.DataContext = mmlist.MMListView.SelectedItem;
            MMListBoxViewModel viewmodel = mmlist.DataContext as MMListBoxViewModel;
            viewmodel.logDataGrid = this.QuantLog.dgLog;
            viewmodel.qunatDataGrid = mmlist.QuantListView;
            DataRowView drv = (DataRowView)mmlist.QuantListView.SelectedItem;
            if (drv != null)
            {
                string quantID = drv[MMListBoxViewModel.Id] + "";
                string quantCode = drv[MMListBoxViewModel.QUANTID_FIELD] + "";
                (mmlist.DataContext as MMListBoxViewModel).quantID = quantCode;
                this.OrderControl.quantID = quantID;
                this.OrderControl.quantCode = quantCode;
                this.OrderControl.userName = drv[MMListBoxViewModel.USERNAME_FIELD] + "";
                this.OrderControl.info4 = drv[MMListBoxViewModel.INFO4_FIELD] + "";
                this.OrderControl.info5 = drv[MMListBoxViewModel.INFO5_FIELD] + "";
                this.OrderControl.setColumnVisibility(this.OrderControl.info4);

                viewmodel.Instance_OnParasList(QuantGroupHandler.Instance.GetParasByQuantID(quantCode));
                viewmodel.Instance_OnIndicatorList(QuantGroupHandler.Instance.GetCacheQuanIndicatorList(quantCode));

                ThreadPool.QueueUserWorkItem(o =>
                {
                    // viewmodel.ClearLog();
                    List<TTbookQuantLog> list = QuantGroupHandler.Instance.InitLog(quantCode, quantID);

                    viewmodel.Instance_QuantLogList(list);

                });
                viewmodel._myTraderViewModel = MyTradingControl._myTraderViewModel;
                MyTradingControl._myTraderViewModel.ChangeQuantID(quantCode);
                this.OrderControl.txTitle.Text = "参数管理-" + quantCode;
                this.QuantLog.txTitle.Text = "日志-" + quantCode;
                this.QuantIndicatort.txTitle.Text = "策略指标-" + quantCode;
            }
            this.OrderControl.DataContext = viewmodel;
            this.QuantIndicatort.DataContext = viewmodel;
            this.QuantLog.DataContext = viewmodel;
            
        }


        private const string ALL = "全部";

        private void ItemsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (e.AddedItems.Count > 0)
                {
                    foreach (object obj in e.AddedItems)
                    {
                        ListBoxItem item = obj as ListBoxItem;
                        if (item != null)
                        {
                            if (ALL.Equals(item.Content))
                            {
                                foreach (ListBoxItem lbi in listBox.Items)
                                {
                                    if (lbi.IsSelected && !ALL.Equals(lbi.Content))
                                        lbi.IsSelected = false;
                                }
                            }
                            else
                            {
                                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                                if (tmp.IsSelected)
                                    tmp.IsSelected = false;

                            }
                        }
                    }
                }

                if (e.RemovedItems.Count > 0)
                {
                    foreach (var item in e.RemovedItems)
                    {
                        if (listBox.SelectedItems.Count == 0)
                            listBox.SelectedItem = item;
                    }
                }
            }
        }


        private void MM_MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void MM_MainView_Initialized_1(object sender, EventArgs e)
        {
            try
            {
                //IOrderedEnumerable<TTbookQuantGroups> sortDic = Utils.Data.QuantGroupHandler.Instance.RequestGroups();

                //if (sortDic != null && sortDic.Count() > 0)
                //{
                //    foreach (TTbookQuantGroups gi in sortDic)
                //    {
                //        //if (gi.action != 3)
                //        //{
                //        this.GroupTabControl.AddGroupTabItem(gi, false);
                //        //}
                //    }
                //    this.GroupTabControl.SelectedIndex = 0;
                //}
                TTbookQuantGroups group = new TTbookQuantGroups();
                group.Id = QuantCommonUtils.groupID;
                group.Groupname = "策略概览";
                this.GroupTabControl.AddGroupTabItem(group, false);

                IsInited = true;
            }
            catch (Exception ex)
            {
                com.gateway.log.LogHelper.Error(ex);
            }
        }

        public void setButtonVisibility(Visibility visibility)
        {
            btnFullScreen.Visibility = visibility;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
