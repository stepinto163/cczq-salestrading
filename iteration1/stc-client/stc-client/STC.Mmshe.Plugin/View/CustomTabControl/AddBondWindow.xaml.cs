using STC.Common.Handler.Admin;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.MMshe;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Mmshe.Plugin.Utils;
using STC.Mmshe.Plugin.Utils.Data;
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

namespace STC.Mmshe.Plugin.View.HomePage
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
        Dictionary<string, MMsheGroupInfo> groupPodic = new Dictionary<string, MMsheGroupInfo>();
        Dictionary<string, int> selectIndexdic = new Dictionary<string, int>();
        private MMsheGroupInfo _selectGroup;
        public MMsheGroupInfo SelectGroup
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
        public string Venue { get; set; }

        public AddBondWindow()
        {
            InitializeComponent();

            this.bond_txt.FocusText();
            selectIndexdic.Clear();
            if (MMsheGroupHandler.Instance.CacheSortedGroups != null)
            {
                foreach (MMsheGroupInfo item in MMsheGroupHandler.Instance.CacheSortedGroups)
                {
                    if (item.inf1 != MMsheConstants.MM_GROPE_TYPE_UNIFY)
                    {
                        groupdic.Add(item.id, item.groupname);
                        groupPodic.Add(item.id,item);
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
                bool status = true;
                if (string.IsNullOrEmpty(Groupid))
                {
                    error_tb.Text = "债券分组不能为空";
                    status = false;
                }
                if (string.IsNullOrEmpty(Venue))
                {
                    error_tb.Text = "组交易场所不能为空";
                    status = false;
                }
                if (("SSE".Equals(Venue) || "SSF".Equals(Venue)) && !obj.Bond_ID.EndsWith(".SH")) 
                {
                    error_tb.Text = "非此交易场所的债券";
                    status = false;
                }
                if (("SZF".Equals(Venue) || "SZE".Equals(Venue)) && !obj.Bond_ID.EndsWith(".SZ"))
                {
                    error_tb.Text = "非此交易场所的债券";
                    status = false;
                }
                securityID = obj.Bond_ID;
                symbol = obj.Short_Name;
                remainingTime = obj.RemaingingTime;
                String today = DateTimeEx.ServerTime.ToString("yyyyMMdd");
                this.symbolTb.Text = obj.Short_Name;
                if (today.CompareTo(obj.Maturity_Date.ToString()) > 0)
                {
                    btnOK.IsEnabled = false;
                    MaturityPanel.Visibility = Visibility.Visible;
                }
                else if (!status) 
                {
                    btnOK.IsEnabled = false;
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
            error_tb.Text = "";
        }

        private void comTemplet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ComboBox)
            {
                ComboBox comboBox = sender as ComboBox;

                if (comboBox.SelectedIndex >= 0)
                {
                    Groupid = comboBox.SelectedValue.ToString();
                    MMsheGroupInfo groupInfo = null;
                    if (groupPodic.TryGetValue(Groupid, out groupInfo)) 
                    {
                        Venue = groupInfo.venuetypegw;
                    }
                }
                else
                {
                    Groupid = "";
                    Venue = "";
                }
            }
        }
    }
}
