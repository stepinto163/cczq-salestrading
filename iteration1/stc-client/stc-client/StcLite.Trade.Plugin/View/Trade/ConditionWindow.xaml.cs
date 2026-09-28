using com.gateway.log;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Utils;
using System;
using System.Collections;
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
using STC.Common.View.Component;
using STC.Common.Utils;

namespace BDB.Trade.Plugin.View.Trade
{
    /// <summary>
    /// ConditionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConditionWindow : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        public Dictionary<string, IList> FilterDic;

        public ConditionWindow()
        {
            InitializeComponent();

            Init();
        }


        private void Init()
        {
            try
            {
                string[] brokerSortArr = LocalEnv.UserInfo.BrokerSort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (brokerSortArr != null)
                {
                    int index = 1;
                    foreach (string bs in brokerSortArr)
                    {
                        if (CommonConstants.BrokerNumEnum.Contains(bs))
                        {
                            string cn = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");
                            ListBoxItem lb = lbtBroker.Items[index] as ListBoxItem;
                            lb.Visibility = Visibility.Visible;

                            lb.Content = ComboSourceUtil.Instance.FindEnum(bs, "venues_cn_tocn");

                            index++;
                        }
                    }

                }

                //string[] arr = LocalEnv.UserInfo.BonDPermissions.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                //List<String> cmbkey = ComboSourceUtil.Instance.GetComboSource("venues_cn").Select(x => x.Key).ToList();
                //if (arr != null)
                //{
                //    List<string> listC = arr.ToList();

                //    int index = 1;
                //    for (int i = listC.Count - 1; i >= 0; i--)
                //    {
                //        ListBoxItem lb = lbtBroker.Items[index] as ListBoxItem;
                //        lb.Visibility = Visibility.Visible;

                //        lb.Content = ComboSourceUtil.Instance.FindEnum(listC[i], "venues_cn");

                //        index++;
                //    }
                //}
                //else
                //{
                //    return;
                //}
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
            }
        }

        private ListBoxItem GetListBoxItem(string content, ListBox listBox)
        {
            ListBoxItem result = null;
            foreach (ListBoxItem item in listBox.Items)
            {
                if (item.Content.ToString() == content)
                {
                    result = item;

                    break;
                }
            }

            if (result != null)
                result.IsSelected = true;

            return result;
        }

        public void SetCondition(Dictionary<string, IList> filterDic)
        {
            if (filterDic.ContainsKey(CommonConstants.BOND_TYPE))
            {
                IList bondList = filterDic[CommonConstants.BOND_TYPE];
                foreach (ListBoxItem lbi in bondList)
                {
                    ListBoxItem tem = GetListBoxItem(lbi.Content.ToString(), lbBondType);
                }
            }

            if (filterDic.ContainsKey(CommonConstants.BROKER))
            {
                IList bondList = filterDic[CommonConstants.BROKER];
                foreach (ListBoxItem lbi in bondList)
                {
                    ListBoxItem tem = GetListBoxItem(lbi.Content.ToString(), lbtBroker);
                }
            }

            if (filterDic.ContainsKey(CommonConstants.REMAINING_TIME))
            {
                IList bondList = filterDic[CommonConstants.REMAINING_TIME];
                foreach (ListBoxItem lbi in bondList)
                {
                    ListBoxItem tem = GetListBoxItem(lbi.Content.ToString(), lbRemaingTime);
                }
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            FilterDic = new Dictionary<string, IList>();

            if (lbtBroker.SelectedItems.Count > 0)
            {
                FilterDic[CommonConstants.BROKER] = lbtBroker.SelectedItems;
            }

            if (lbBondType.SelectedItems.Count > 0)
            {
                FilterDic[CommonConstants.BOND_TYPE] = lbBondType.SelectedItems;
            }

            if (lbRemaingTime.SelectedItems.Count > 0)
            {
                FilterDic[CommonConstants.REMAINING_TIME] = lbRemaingTime.SelectedItems;
            }

            Result = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }

        private void LbtBroker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int isTrueCount = 0;
            foreach (ListBoxItem lbi in lbtBroker.Items)
            {
                if (lbi.IsSelected == true && lbi.Content.ToString() != CommonConstants.ALL)
                {
                    isTrueCount++;
                }
            }
            ListBoxItem tmp = (ListBoxItem)lbtBroker.Items[0];// 这个是AllBroker按钮
            if (isTrueCount == PermisionHandler.Instance.GetBrokerCount() || isTrueCount == 0)
            {
                tmp.IsSelected = true;
            }
            else if (isTrueCount > 0 && isTrueCount < PermisionHandler.Instance.GetBrokerCount())
            {
                tmp.IsSelected = false;
            }
        }

        private void LbBondType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int isTrueCount = 0;
            foreach (ListBoxItem lbi in lbBondType.Items)
            {
                if (lbi.IsSelected == true && lbi.Content.ToString() != CommonConstants.ALL)
                {
                    isTrueCount++;
                }
            }
            ListBoxItem tmp = (ListBoxItem)lbBondType.Items[0];// 这个是AllBroker按钮
            if (isTrueCount == 4 || isTrueCount == 0)
            {
                tmp.IsSelected = true;
            }
            else if (isTrueCount > 0 && isTrueCount < 4)
            {
                tmp.IsSelected = false;
            }
        }

        private void LbRemaingTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int isTrueCount = 0;
            foreach (ListBoxItem lbi in lbRemaingTime.Items)
            {
                if (lbi.IsSelected == true && lbi.Content.ToString() != CommonConstants.ALL)
                {
                    isTrueCount++;
                }
            }
            ListBoxItem tmp = (ListBoxItem)lbRemaingTime.Items[0];// 这个是AllBroker按钮
            if (isTrueCount == 6 || isTrueCount == 0)
            {
                tmp.IsSelected = true;
            }
            else if (isTrueCount > 0 && isTrueCount < 6)
            {
                tmp.IsSelected = false;
            }
        }
    }
}
