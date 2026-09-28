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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.quant.plugin.View.HomePage
{
    /// <summary>
    /// AddParameterWindow.xaml 的交互逻辑
    /// </summary>
    public partial class AddParameterWindow : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }
        /// <summary>
        /// 组名
        /// </summary>
        public string Param_key { get; private set; }
        public string Param_type { get; private set; }
        public string Param_value { get; private set; }
        public string Param_desc { get; private set; }
        Dictionary<string, string> dic = new Dictionary<string, string>();
        public AddParameterWindow(string title)
        {
            InitializeComponent();
            this.Title = title;
            dic.Add("double", "double");
            dic.Add("int", "int");
            dic.Add("boolean", "boolean");
            dic.Add("String", "String");
            param_type.ItemsSource = dic;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Param_key = this.param_key.Text;
            this.Param_type = this.param_type.SelectedValue + "";
            this.Param_value = this.param_value.Text;
            this.Param_desc = this.param_desc.Text;
            //if (string.IsNullOrEmpty(GroupName))
            //{
            //    error_tb.Text = "参数名称不能为空";
            //    e.Handled = true;
            //    return;
            //}
            //if (Utils.Data.QuantGroupHandler.Instance.IsGroupNameExist(this.GroupName) != null)
            //{
            //    error_tb.Text = "参数名称已存在";
            //    e.Handled = true;
            //    return;
            //}
            //if (this.GroupName.Length > 10)
            //{
            //    error_tb.Text = "参数名称不可超过10个汉字";
            //    e.Handled = true;
            //    return;
            //}
            this.Result = true;
            this.Close();
            e.Handled = true;
        }

     
    }
}
