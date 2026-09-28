using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using com.gateway.log;
using STC.Mmshe.Plugin.View.Component;
using STC.Mmshe.Plugin.ViewModel;

namespace STC.Mmshe.Plugin.View
{
    /// <summary>
    /// MmStatistics.xaml 的交互逻辑
    /// </summary>
    public partial class MmsheStatisticsView : UserControl
    {
        private MmsheStatisticsViewModel vm = new MmsheStatisticsViewModel();

        public MmsheStatisticsView()
        {
            InitializeComponent();
            this.DataContext = this.vm;

        }

        private void detailPageControl_PageChanged(object sender, PageChangedEventArgs e)
        {
            if (e != null)
            {
                vm.DetailIndex = e.Page - 1;
                vm.UpdateDetailRecords();
            }
        }

        private void outlinePageControl_PageChanged(object sender, PageChangedEventArgs e)
        {
            if (e != null)
            {
                vm.OutlineIndex = e.Page -1;
                vm.UpdateOutLineRecords();
            }
        }

        private void MainView_Initialized(object sender, EventArgs e)
        {
            vm.MMStatisticsView = this;
            vm.IsInited = true;
            vm.OutLinePageControl = this.outlinePageControl;
            vm.DetailPageControl = this.detailPageControl;
            vm.RequestDetailTodayData();
            vm.RequestOutLineTodayData();
        }

        private void outline_filter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.outline_filter.IsClick)
            {
                vm.ProcessOutLineFilterCondition(this.outline_filter.FilterDic);
            }
        }

        private void detailFilter_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.detail_filter.IsClick)
            {
                vm.ProcessDetailFilterCondition(this.detail_filter.FilterDic);
            }
        }

        public void ShowDetailMask(bool flag)
        {
            detailloadMask.Dispatcher.Invoke(() =>
            {
                detailloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        public void ShowOutlineMask(bool flag)
        {
            outlineloadMask.Dispatcher.Invoke(() =>
            {
                outlineloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void bondType_lbx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                int isTrueCount = 0;// 如果是true的个数是0或者2时， all按钮被选中 
                foreach (ListBoxItem lbi in listBox.Items)
                {
                    if (listBox.Items.IndexOf(lbi) != 0
                        && lbi.IsSelected == true)
                    {
                        isTrueCount++;
                    }
                }
                ListBoxItem tmp = (ListBoxItem)listBox.Items[0];
                if (isTrueCount == 0 || isTrueCount == 2)
                {
                    tmp.IsSelected = true;
                }
                else
                {
                    tmp.IsSelected = false;
                }

                //vm.UpdateBondType(listBox.SelectedItems);
                vm.BondTypeSelectedItems = null;
                vm.BondTypeSelectedItems = listBox.SelectedItems;
            }
        }

        private void MainView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (e.NewValue != null && e.NewValue is MmStatisticsViewModel)
            //{
            //    vm = e.NewValue as MmStatisticsViewModel;
            //    //detailFilter_FilterDicChanged(this.detail_filter, null);
            //    //outline_filter_FilterDicChanged(this.outline_filter, null);
            //    outline_filter.SetTimeSpanType(vm.OutLineReq.beginDate,vm.OutLineReq.endDate);
            //    detail_filter.SetTimeSpanType(vm.DetailReq.beginDate, vm.DetailReq.endDate);
            //    SyncBondTypeListBox();


            //}
        }

        public void SyncBondTypeListBox()
        {
            foreach (ListBoxItem item in vm.BondTypeSelectedItems)
            {
                string name = item.Content.ToString();
                switch (name)
                {
                    case "全部":
                        (bondType_lbx.Items[0] as ListBoxItem).IsSelected = true;
                        break;
                    case "利率债":
                        (bondType_lbx.Items[1] as ListBoxItem).IsSelected = true;
                        break;
                    case "信用债":
                        (bondType_lbx.Items[2] as ListBoxItem).IsSelected = true;
                        break;
                }
            }
        }

        public void Dispose()
        {
            LogHelper.Debug("dispose");

            if (this.DataContext != null
                && this.DataContext is MmsheStatisticsViewModel)
            {
                MmsheStatisticsViewModel viewModel = ((MmsheStatisticsViewModel)this.DataContext);
                this.DataContext = null;
                viewModel.Dispose();
                viewModel = null;
            }
            if (vm != null) vm = null;
        }
    }
}
