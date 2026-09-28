using STC.Common.Utils;
using STC.Position.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryPositionView : UserControl
    {
        private bool isLoad = true;
        public bool IsLoad
        {
            get
            {
                return isLoad;
            }

            set
            {
                isLoad = value;
            }
        }

        HistoryPositionViewModel mainViewModel = new HistoryPositionViewModel();
        public HistoryPositionView()
        {
            InitializeComponent();
            mainViewModel.HistoryView = this;
            this.DataContext = mainViewModel;
            (DataContext as HistoryPositionViewModel).PageControl = this.pageControl;
        }

        public void load()
        {
            if (isLoad)
            {
                //isLoad = false;
                //mainViewModel.clear();
                //_viewModel.ProcessMsgsFilterCondition(this.historyFilter.RiskFilterDic);
            }
        }

        public void ShowEMSMask(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                positionMainMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            }));
        }

        public void SetPositionDetailSecurityId(string securityid)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UC_Hpod.SecurityId = securityid;
            }));
        }

        public void SetPositionDetailDealTimeStart(string dealtimebegin)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UC_Hpod.DealTimeBegin = dealtimebegin;
            }));
        }

        public void SetPositionDetailDealTimeEnd(string dealTimeEnd)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UC_Hpod.DealTimeEnd = dealTimeEnd;
            }));
        }

        public void SetPositionDetailQueryData()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UC_Hpod.QueryData.Execute(string.Empty);
            }));
        }


        public void SetPositionDetailClearData()
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UC_Hpod.ClearData.Execute(string.Empty);
            }));
        }


        public void SetPositionDetailBookId(string bookid)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                UC_Hpod.BookId = bookid;
            }));
        }
        private void dgDetail_Sorting(object sender, DataGridSortingEventArgs e)
        {
            (this.DataContext as HistoryPositionViewModel).Sort(e.Column.SortMemberPath, e.Column.SortDirection.ToString());
        }

        public void Dispose()
        {
            (this.DataContext as HistoryPositionViewModel).UnRegistEvent();
        }

        //private bool _horClose = false;
        //private void btnHor_Click(object sender, RoutedEventArgs e)
        //{
        //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        if (!_horClose)
        //        {
        //            gridMain.RowDefinitions[3].Height = new GridLength(0);
        //            _horClose = true;

        //            btnHor.Content = "∧";
        //        }
        //        else
        //        {
        //            gridMain.RowDefinitions[1].Height = new GridLength(60, GridUnitType.Star);
        //            gridMain.RowDefinitions[3].Height = new GridLength(35, GridUnitType.Star);
        //            _horClose = false;

        //            btnHor.Content = "∨";
        //        }
        //    }));
        //}

        private void dgDetail_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void Orders_dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.HistoryMainPosition_newdg.SelectedIndex >= 0)
            {
                if ((this.DataContext as HistoryPositionViewModel).SelectedPosition != null)
                {
                    string endDate = CommonUtils.ConvertToDateTime((this.DataContext as HistoryPositionViewModel).SelectedPosition.updatetime).ToString("yyyy-MM-dd");
                    string startDate = (this.DataContext as HistoryPositionViewModel).StartDate.ToString("yyyy-MM-dd");
                    string bookid = (this.DataContext as HistoryPositionViewModel).SelectedPosition.bookid;
                    SetPositionDetailDealTimeStart(startDate);
                    SetPositionDetailDealTimeEnd(endDate);
                    SetPositionDetailBookId(bookid);
                    SetPositionDetailSecurityId((this.DataContext as HistoryPositionViewModel).SelectedPosition.securityid);
                    SetPositionDetailQueryData();
                }
            }
        }

        /// <summary>
        /// 当按回车时
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        //private void SearchTextBox_AfterSearchBond(Common.Model.Base.SimpleBondsInfoPo arg1, object arg2)
        //{
        //    if (arg1 == null)
        //        mainViewModel.showMessage("请输入有效的债券代码！");
        //}

        private void TreeView_NodeList_SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            mainViewModel.SelectBook = (TreeView_NodeList.SelectedItem ?? "").ToString();
        }

        private void pageControl_PageChanged(object sender, Common.View.Component.PageChangedEventArgs e)
        {
            if (e != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    this.ShowEMSMask(true);
                    (this.DataContext as HistoryPositionViewModel).PageIndex = e.Page - 1;
                    (this.DataContext as HistoryPositionViewModel).FullPositionDetail.Clear();
                    (this.DataContext as HistoryPositionViewModel).RequeryPositionMvt();
                }));
            }
        }
    }
}
