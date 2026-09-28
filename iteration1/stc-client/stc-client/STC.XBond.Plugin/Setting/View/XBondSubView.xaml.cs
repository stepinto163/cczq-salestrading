using STC.XBond.Plugin.Setting.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

namespace STC.XBond.Plugin.Setting.View
{
    /// <summary>
    /// XBondSubView.xaml 的交互逻辑
    /// </summary>
    public partial class XBondSubView : UserControl
    {
        private XBondSubViewModel _xbondSubViewModel = new XBondSubViewModel();

        public XBondSubView()
        {
            InitializeComponent();
        }

        private void XBondSubView_Initialized(object sender, EventArgs e)
        {
            ((ICollectionView)this.dataGrid.Items).CollectionChanged += XBondSubView_CollectionChanged;
            this.DataContext = _xbondSubViewModel;

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this._xbondSubViewModel.init();
        }

        private void XBondSubView_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)//新增数据时才出发滚动
            {
                this.dataGrid.SelectedIndex = this.dataGrid.Items.Count - 1;
                this.dataGrid.ScrollIntoView(this.dataGrid.SelectedItem);
                this.dataGrid.UpdateLayout();
            }
        }

        /// <summary>
        /// 筛选债券
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void selectBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    this._xbondSubViewModel.FilterData();
        //}

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void SearchBond_TextBox_ValueChanged(object sender, RoutedEventArgs e)
        {
            this._xbondSubViewModel.setSymbol();
        }
    }
}
