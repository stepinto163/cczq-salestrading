using STC.Common.Utils;
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
using System.Windows.Shapes;
using STC.Common.View.Component;

namespace STC.Frame.CustomUI.Component
{
    /// <summary>
    /// CustomMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class CustomMessageBox : WindowBase
    {
        /// <summary>
        /// 结果，用户点击确定Result=true;
        /// </summary>
        public bool Result { get; private set; }

        private static readonly Dictionary<string, Brush> _Brushes = new Dictionary<string, Brush>();

        private CustomMessageBox(EnumNotifyType type, string mes)
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

        private CustomMessageBox(EnumNotifyType type, List<Inline> runLines)
        {
            InitializeComponent();
            this.txtMessage.Inlines.AddRange(runLines);
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

        static bool isMutiSelfflag = false;
        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Close();
            e.Handled = true;
            isMutiSelfflag = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Result = false;
            this.Close();
            e.Handled = true;
            isMutiSelfflag = true;
        }

        /********************* public static method **************************/

        public static void ShowDialog(EnumNotifyType type, List<Inline> runLines, string title = null, Window owner = null, string okButtonText = null, string cancelButtonText = null)
        {
            Show(type, runLines, title, owner, okButtonText, cancelButtonText);
        }

        public static bool ShowDialogAndResult (EnumNotifyType type, List<Inline> runLines, string title = null, Window owner = null, string okButtonText = null, string cancelButtonText = null)
        {
            return Show(type, runLines, title, owner, okButtonText, cancelButtonText);
        }

        /// <summary>
        /// 提示错误消息
        /// </summary>
        public static void Error(string mes, string title = null, Window owner = null)
        {
            Show(EnumNotifyType.Error, mes, title, owner);
        }

        /// <summary>
        /// 提示普通消息
        /// </summary>
        public static void Info(string mes, string title = null, Window owner = null)
        {
            Show(EnumNotifyType.Info, mes, title, owner);
        }

        /// <summary>
        /// 提示警告消息
        /// </summary>
        public static void Warning(string mes, string title = null, Window owner = null)
        {
            Show(EnumNotifyType.Warning, mes,title, owner);
        }

        /// <summary>
        /// 提示询问消息
        /// </summary>
        public static bool Question(string mes, string title = null, Window owner = null)
        {
            return Show(EnumNotifyType.Question, mes, title, owner);
        }

        /// <summary>
        /// 提示询问消息
        /// </summary>
        public static bool Question1(string mes, string title = null,  string btnname = null, string btnname1 = null)
        {
            return Show(EnumNotifyType.Question, mes,title,btnname, btnname1);
        }
        /// <summary>
        /// 显示提示消息框，
        /// owner指定所属父窗体，默认参数值为null，则指定主窗体为父窗体。
        /// </summary>
        private static bool Show(EnumNotifyType type, string mes, string title=null, Window owner = null)
        {
            var res = true;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CustomMessageBox nb = new CustomMessageBox(type, mes);

                if (string.IsNullOrEmpty(title))
                {
                    nb.Title = type.GetOldDescription();
                }
                else
                {
                    nb.Title = title;
                }


                nb.Owner = owner ?? Application.Current.MainWindow;
                nb.ShowDialog();
                res = nb.Result;
            }));
            return res;
        }

        /// <summary>
        /// 显示提示消息框，
        /// owner指定所属父窗体，默认参数值为null，则指定主窗体为父窗体。
        /// </summary>
        private static bool Show(EnumNotifyType type, string mes, string title = null, string btnname=null, string btnname1 = null, Window owner = null)
        {
            var res = true;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CustomMessageBox nb = new CustomMessageBox(type, mes);

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
                if (isMutiSelfflag)
                {
                    res = nb.Result;
                    isMutiSelfflag = false;
                }
                else
                {
                    res = !nb.Result;
                }
            }));
            return res;
        }

        /// <summary>
        /// 显示提示消息框，
        /// owner指定所属父窗体，默认参数值为null，则指定主窗体为父窗体。
        /// </summary>
        private static bool Show(EnumNotifyType type, List<Inline> runLines, string title = null, Window owner = null, string okButtonText = null, string cancelButtonText = null)
        {
            var res = true;
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                CustomMessageBox nb = new CustomMessageBox(type, runLines);

                if (string.IsNullOrEmpty(title))
                {
                    nb.Title = type.GetOldDescription();
                }
                else
                {
                    nb.Title = title;
                }

                if (string.IsNullOrEmpty(okButtonText) == false)
                {
                    nb.btnOK.Content = okButtonText;
                }

                if (string.IsNullOrEmpty(cancelButtonText) == false)
                {
                    nb.btnCancel.Content = cancelButtonText;
                }

                nb.Owner = owner ?? Application.Current.MainWindow;
                nb.ShowDialog();
                res = nb.Result;
            }));
            return res;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            //// 窗体尺寸和内容尺寸不一致，说明已经受限于最大尺寸，此时缩放内容
            //if (ActualWidth < RootGrid.ActualWidth)
            //{
            //    RootViewbox.Stretch = Stretch.Uniform;
            //}
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

        private void WindowBase_Closing(object sender, CancelEventArgs e)
        {
            //this.Owner = null;
        }
    }

}
