using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MM;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Mm.Plugin.Utils;
using STC.Mm.Plugin.Utils.Data;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace STC.Mm.Plugin.View.HomePage
{
    /// <summary>
    /// AddGroupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddBondWindow : WindowBase
    {
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
        Dictionary<string, string> groupdic = new Dictionary<string, string>();
        Dictionary<string, int> selectIndexdic = new Dictionary<string, int>();
        private MMGroupInfo _selectGroup;
        public MMGroupInfo SelectGroup
        {
            set 
            {
                _selectGroup = value;
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    if (_selectGroup != null && selectIndexdic.ContainsKey(_selectGroup.id))
                    {                       
                        comTemplet.SelectedIndex = selectIndexdic[_selectGroup.id];
                    }
                    else
                    {
                        comTemplet.SelectedIndex = 0;
                    }
                }));
            }
        }

        public string Groupid { get; set; }

        public AddBondWindow()
        {
            InitializeComponent();

            this.bond_txt.FocusText();
            selectIndexdic.Clear();
            if (MMGroupHandler.Instance.CacheSortedGroups != null)
            {
                foreach (MMGroupInfo item in MMGroupHandler.Instance.CacheSortedGroups)
                {
                    if (item.inf1 != MMConstants.MM_GROPE_TYPE_UNIFY)
                    {
                        groupdic.Add(item.id, item.groupname);
                    }
                }
            }
            int i = 0;
            foreach (KeyValuePair<string, string> kv in groupdic)
            {
                selectIndexdic.Add(kv.Key, i);
                i++;
            }
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                comTemplet.ItemsSource = null;
                comTemplet.ItemsSource = groupdic;
            }));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
            e.Handled = true;
        }

        private void bond_txt_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            if (obj != null)
            {
                symbol = obj.Short_Name;
                securityID = obj.Bond_ID;
                remainingTime = obj.RemaingingTime;
                String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                this.symbolTb.Text = obj.Short_Name;
                if (today.CompareTo(obj.Maturity_Date.ToString()) > 0)
                {
                    btnOK.IsEnabled = false;
                    MaturityPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    btnOK.IsEnabled = true;
                    MaturityPanel.Visibility = Visibility.Collapsed;
                }
                this.btnOK.Focus();
            }
        }

        private void bond_txt_ValueChanged(object sender, RoutedEventArgs e)
        {
            btnOK.IsEnabled = false;
            MaturityPanel.Visibility = Visibility.Collapsed;
        }

        private void comTemplet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ComboBox)
            {
                ComboBox comboBox = sender as ComboBox;

                if (comboBox.SelectedIndex >= 0)
                {
                    Groupid = comboBox.SelectedValue.ToString();
                }
                else 
                {
                    Groupid = "";
                }
            }
        }
    }
}
