using STC.Common.Utils;
using STC.Common.View.Component;
using System.Windows;
using System.Windows.Controls;

namespace STC.Broker.Plugin.View.HomePage
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

        public AddGroupWindow()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.GroupName = this.groupname_txt.Text;
            if (string.IsNullOrEmpty(GroupName))
            {
                error_tb.Text = "分组名称不能为空";
                e.Handled = true;
                return;
            }
            if (HomePageDataHandler.IsGroupNameExist(this.GroupName))
            {
                error_tb.Text = "分组名称已存在";
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
    }
}
