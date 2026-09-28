using STC.Common.Utils;
using STC.Common.View.Component;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// MatchExeView.xaml 的交互逻辑
    /// </summary>
    public partial class MatchExeView : WindowBase
    {

        MatchExeViewModel _matchExeViewModel = new MatchExeViewModel();

        public MatchExeView()
        {
            InitializeComponent();
            _matchExeViewModel.load();
            this.DataContext = _matchExeViewModel;
            InitVenuesComb();
            InitMatchStatusFQ();
        }

        private void InitVenuesComb()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            dic["MANUAL"] = "自谈";
            dic["TPSC"] = "国利";
            dic["CNEX"] = "国际";
            dic["PTCN"] = "平安";
            dic["CCTB"] = "中诚";
            dic["CTTJ"] = "信唐";
            dic["UEDA"] = "上田";
            venusCB.ItemsSource = dic;
        }

        private void InitMatchStatusFQ()
        {
            ObservableCollection<OptionItem> matchstauslist = new ObservableCollection<OptionItem>();
            matchstauslist.Add(new OptionItem("未匹配", "5"));
            matchstauslist.Add(new OptionItem("部分匹配", "6"));
            matchstauslist.Add(new OptionItem("等量匹配", "7"));
            matchstauslist.Add(new OptionItem("超量匹配", "8"));
            this.matchStatusFQ.ItemsSource = matchstauslist;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void venusCB_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            MultiSelectComboBox ccb = sender as MultiSelectComboBox;

            string[] orgArr = null;
            if (ccb != null)
            {
                if (!string.IsNullOrEmpty(ccb.SelectedValues))
                {
                    orgArr = ccb.SelectedValues.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            if (_matchExeViewModel != null)
            {
                _matchExeViewModel.ProcessVenuesFilter(orgArr);
            }
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (_matchExeViewModel != null)
                {
                    _matchExeViewModel.CheckBoxAllValueChange(flag);
                }
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            if (_matchExeViewModel != null)
            {
                _matchExeViewModel.CheckBoxSingleClick();
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_matchExeViewModel != null)
            {
                _matchExeViewModel.PTOrders.Clear();
                _matchExeViewModel.RefreshDatas();
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_matchExeViewModel != null)
            {
                _matchExeViewModel.SaveAssociate(this);
            }
        }

        private void matchStatusFQ_TagsListChanged(object sender, RoutedEventArgs e)
        {
            FilterOptions fo = sender as FilterOptions;
            if(fo != null)
            {
                if (_matchExeViewModel != null)
                {
                    _matchExeViewModel.ProcessMatchStatus(fo.TagsList);
                }
            }
        }

        private void SaveAndFinishBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_matchExeViewModel != null)
            {
                _matchExeViewModel.SaveAssociate(this, true);
            }
        }
    }
}
