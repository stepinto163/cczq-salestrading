using BDB.Trade.Plugin.ViewModel.Trade;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Component;
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
using System.Windows.Shapes;

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// AddGroupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddBondWindow : WindowBase
    {
        private AddBondViewModel _addBondViewModel = null;

        public AddBondWindow()
        {
            InitializeComponent();

            _addBondViewModel = new AddBondViewModel();
            this.DataContext = _addBondViewModel;

            _addBondViewModel.Init(this);

            this.Loaded += AddBondWindow_Loaded;

        }

        private void AddBondWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= AddBondWindow_Loaded;
            seSearch.SetFocus();
        }

        public void SetGroupId(string groupId)
        {
            _addBondViewModel.GroupId = groupId;
        }

        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string symbol { get; private set; }

        /// <summary>
        /// 代码
        /// </summary>
        public string securityID { get; private set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public string remainingTime { get; set; }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            //this.Close();
            e.Handled = true;
        }

        private void bond_txt_AfterSearch(BondsInfoPo obj)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.Bond_ID) && _addBondViewModel != null && _addBondViewModel.SelectedBond != null)
            {
                _addBondViewModel.SelectedBond.Symbol = obj.Short_Name;
                _addBondViewModel.SelectedBond.RemainingTime = obj.RemaingingTime;
                _addBondViewModel.SelectedBond.Security = obj.Bond_ID;

                symbol = obj.Short_Name;
                securityID = obj.Bond_ID;
                remainingTime = obj.RemaingingTime;
                String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                //if (today.CompareTo(obj.Maturity_Date.ToString()) > 0)
                //{
                //    this.symbolTb.Text = obj.Short_Name;
                //    btnOK.IsEnabled = false;
                //    MaturityPanel.Visibility = Visibility.Visible;
                //}
                //this.btnOK.Focus();
            }
        }

        private void bond_txt_AfterSearch(object obj)
        {

        }

        private void SeSearch_SearchChanged(object sender, object data)
        {
            if (_addBondViewModel != null)
                _addBondViewModel.AddSecurity(data);
        }

        private void BtnBatchOK_Click(object sender, RoutedEventArgs e)
        {
            string importStr = this.importTxt.Text;
            _addBondViewModel.RecognizeIntentionQuote(importStr);

            Result = true;
            e.Handled = true;
            //this.Close();
        }

        private void btnOK2_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            //// todo 验证
            //this.Close();
            // e.Handled = true;
            this.step2.Visibility = Visibility.Collapsed;
            this.btnBatchOK2.Visibility = Visibility.Collapsed;
            this.step3.Visibility = Visibility.Visible;
            this.btnBatchOK3.Visibility = Visibility.Visible;
        }

        private void btnOK3_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;

            // todo 验证
            //this.Close();
            e.Handled = true;
        }

        private void BtnSendOK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
