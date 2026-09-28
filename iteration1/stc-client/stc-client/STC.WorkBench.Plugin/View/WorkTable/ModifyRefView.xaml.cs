using STC.Common.Utils;
using STC.Common.View.Component;
using STC.WorkBench.Plugin.ViewModel.Data;
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
    /// ModifyRefView.xaml 的交互逻辑
    /// </summary>
    public partial class ModifyRefView : WindowBase
    {
        private ObservableCollection<IntentionMarketItem> _Datas;

        private IntentionQuoteItem _CurrentItem;

        public List<string> InquiryNotRefList;

        public ModifyRefView(IntentionQuoteItem currentItem = null, ObservableCollection<IntentionMarketItem> datas = null)
        {
            InitializeComponent();

            _CurrentItem = currentItem;
            if (currentItem != null)
            {
                remainingTime_tb.Text = currentItem.RemainingTime;
                securityId_tb.Text = currentItem.SecurityId;
                symbol_tb.Text = currentItem.Symbol;
                side_tb.Text = ComboSourceUtil.Instance.FindEnum(currentItem.Side, "side");

                if (datas != null && datas.Count > 0)
                {
                    foreach (IntentionMarketItem item in datas)
                    {
                        item.IsChecked = true;
                    }
                }
                _Datas = datas;

                this.inquiryDg.ItemsSource = _Datas;
            }
            else
            {
                dg_row.Height = new GridLength(0, GridUnitType.Auto);
                dg_part.Visibility = Visibility.Collapsed;
            }
        }

        private void Selected_Chk_Click(object sender, RoutedEventArgs e)
        {
            CheckBoxSingleClick();
        }

        private void CheckBoxSingleClick()
        {
            bool flag = true;
            int i = 0;
            if (_Datas != null && _Datas.Count > 0)
            {
                foreach (IntentionMarketItem item in this._Datas)
                {
                    flag = flag && item.IsChecked;
                    if (item.IsChecked)
                    {
                        i++;
                    }
                }
                CheckBox_All.IsChecked = this._Datas.Count > 0 && flag;
            }
        }

        private void CheckBox_All_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                CheckBoxAllValueChange(flag);
            }
        }

        private void CheckBoxAllValueChange(bool isAllChecked)
        {
            int workingDataCount = 0;

            if (_Datas != null && _Datas.Count > 0)
            {
                foreach (IntentionMarketItem item in _Datas)
                {
                    item.IsChecked = isAllChecked;
                    workingDataCount++;
                }


                if (workingDataCount == 0)
                {
                    CheckBox_All.IsChecked = false;
                }
                else
                {
                    CheckBox_All.IsChecked = isAllChecked;
                }
            }
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_Datas != null && _Datas.Count > 0)
            {
                if (InquiryNotRefList == null)
                {
                    InquiryNotRefList = new List<string>();
                }

                foreach (IntentionMarketItem item in _Datas)
                {
                    if (item.IsChecked == false)
                    {
                        InquiryNotRefList.Add(item.Id);
                    }
                }
            }

            this.DialogResult = true;
            this.Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
