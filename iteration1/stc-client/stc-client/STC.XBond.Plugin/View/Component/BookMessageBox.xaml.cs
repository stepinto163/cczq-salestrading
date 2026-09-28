using STC.Common.Utils;
using STC.XBond.Plugin.Utils;
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

namespace STC.XBond.Plugin.View.Component
{
    /// <summary>
    /// BookMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class BookMessageBox : WindowBase
    {
        public bool Result { get; private set; }
        public BookMessageBox()
        {
            InitializeComponent();
        }
        private BookMessageBox(EnumNotifyType type, string mes)
        {
            InitializeComponent();
            this.txtMessage.Text = mes;
            //type
            btnCancel.Visibility = Visibility.Collapsed;
            switch (type)
            {
                case EnumNotifyType.Error:
                    this.ErroIcon.Visibility = Visibility.Visible;
                    break;
                case EnumNotifyType.Warning:
                    this.ErroIcon.Visibility = Visibility.Visible;
                    break;
                case EnumNotifyType.Info:
                    this.InfoIcon.Visibility = Visibility.Visible;
                    break;
                case EnumNotifyType.Question:
                    this.QuestionIcon.Visibility = Visibility.Visible;
                    this.btnCancel.Visibility = Visibility.Visible;
                    break;
            }
        }
        /// <summary>
        /// 提示询问消息
        /// </summary>
        public static bool Question1(string mes, string title = null, string btnname = null, string btnname1 = null)
        {
            return Show(EnumNotifyType.Question, mes, title, btnname, btnname1);
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Close();
            e.Handled = true;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
        }
        /// <summary>
        /// 显示提示消息框，
        /// owner指定所属父窗体，默认参数值为null，则指定主窗体为父窗体。
        /// </summary>
        private static bool Show(EnumNotifyType type, string mes, string title = null, string btnname = null, string btnname1 = null, Window owner = null)
        {
            var res = true;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BookMessageBox nb = new BookMessageBox(type, mes);

                if (string.IsNullOrEmpty(title))
                {
                    nb.Title = type.GetOldDescription();
                }
                else
                {
                    nb.Title = title;
                }
                if (!string.IsNullOrEmpty(btnname))
                {
                    nb.btnCancel.Content = btnname;
                }

                if (!string.IsNullOrEmpty(btnname1))
                {
                    nb.btnOK.Content = btnname1;
                }

                nb.Owner = owner ?? Application.Current.MainWindow;
                nb.ShowDialog();
                res = nb.Result;
            }));
            return res;
        }
        private void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //this.Result = false;
            //this.Close();
        }
        /// <summary>
        /// 通知消息类型
        /// </summary>
        public enum EnumNotifyType
        {
            [Description("错误")]
            Error,
            [Description("警告")]
            Warning,
            [Description("提示信息")]
            Info,
            [Description("询问信息")]
            Question,
        }
    }
}
