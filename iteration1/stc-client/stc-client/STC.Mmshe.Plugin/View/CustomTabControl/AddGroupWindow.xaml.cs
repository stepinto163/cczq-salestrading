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

namespace STC.Mmshe.Plugin.View.HomePage
{
    /// <summary>
    /// AddGroupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddGroupWindow : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// 组名
        /// </summary>
        public string GroupName { get; private set; }

        /// <summary>
        /// 是否整租
        /// </summary>
        public bool IsUnify = false;

        /// <summary>
        /// 交易场所
        /// </summary>
        public string Venue { get; private set; }

        Dictionary<string, string> venuedic = new Dictionary<string, string>();
        public AddGroupWindow()
        {
            InitializeComponent();
            venuedic.Add("SSE","上新债");
            venuedic.Add("SSF", "上固收");
            venuedic.Add("SZF", "深点击");
            venuedic.Add("SZE", "深匹配");
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                comVenue.ItemsSource = venuedic;
            }));
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.GroupName = this.groupname_txt.Text.Replace(" ", "");
            this.IsUnify = this.isGroupCk.IsChecked.Value;
            if (string.IsNullOrEmpty(GroupName))
            {
                error_tb.Text = "分组名称不能为空";
                e.Handled = true;
                return;
            }
            if (Utils.Data.MMsheGroupHandler.Instance.IsGroupNameExist(this.GroupName) != null)
            {
                error_tb.Text = "分组名称已存在";
                e.Handled = true;
                return;
            }
            if (this.GroupName.Length > 10)
            {
                error_tb.Text = "分组名称不可超过10个汉字";
                e.Handled = true;
                return;
            }
            if (string.IsNullOrEmpty(Venue))
            {
                error_tb.Text = "交易场所不能为空";
                e.Handled = true;
                return;
            }
            this.Result = true;
            this.Close();
            e.Handled = true;
        }

        private void groupname_txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            error_tb.Text = string.Empty;
        }

        private void comVenue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ComboBox)
            {
                ComboBox comboBox = sender as ComboBox;

                if (comboBox.SelectedIndex >= 0)
                {
                    Venue = comboBox.SelectedValue.ToString();
                }
                else
                {
                    Venue = "";
                }
            }
        }
    }
}
