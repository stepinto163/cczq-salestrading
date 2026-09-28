using CefSharp.DevTools.IndexedDB;
using STC.Chrome.Plugin.Data;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.View.Drp.Component;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Chrome.Plugin
{
    /// <summary>
    /// MessageControl.xaml 的交互逻辑
    /// </summary>
    public partial class JsCSharpDemo : UserControl
    {

        public event RoutedEventHandler MOK
        {
            add { AddHandler(NotificationOkEvent, value); }
            remove { RemoveHandler(NotificationOkEvent, value); }
        }
        public static readonly RoutedEvent NotificationOkEvent = EventManager.RegisterRoutedEvent(
                 "MOK", RoutingStrategy.Bubble, typeof(RoutedEventArgs), typeof(JsCSharpDemo));

        private BorderWin myTradingPopupWin;
        public JsCSharpDemo()
        {
            InitializeComponent();
        }


        public JsCSharpDemo(BorderWin mw)
        {
            InitializeComponent();
            this.myTradingPopupWin = mw;

            foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
            {
                if (tabItem.Tag.ToString() == "7")
                {
                    cmbTarget.Items.Add(tabItem.Header);

                }
            }

            cmbTarget.SelectedIndex = cmbTarget.Items.Count - 1;
        } 
        public string Title { get; set; } = "Window"; // 标题

        public string WinTitle { get; set; } = ""; // 标题

        public string MsgId { get; set; } = ""; // 标题序号


        public string key { get; set; } = ""; // 消息
        public string value { get; set; } = ""; // 消息

        private void OnCloseButtonOnClick(object sender, RoutedEventArgs e)
        {
            myTradingPopupWin.Close();
        }


        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            key = UsernameBox.Text ?? "";
            value = PasswordBox.Text ?? "";
            if (string.IsNullOrWhiteSpace(key))
            {
                Common.View.Component.CustomMessageBox.Info(string.Format("请输入key"), "提醒", myTradingPopupWin);
                return;
            }

            object data = $"{cmbTarget.SelectedValue}:{key}:{value}";
            RaiseEvent(new RoutedEventArgs { RoutedEvent = NotificationOkEvent, Source = data });
            myTradingPopupWin.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            throw new InvalidOperationException("测试 UI 异常");
            UsernameBox.Text = "";
            PasswordBox.Text = "";
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQRefresh", Info3 = "" });
        }

        private void btnfq_Click(object sender, RoutedEventArgs e)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQSetAction", Info3 = "" });
        }

        private void btnChangeBond_Click(object sender, RoutedEventArgs e)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "FQChangeBond", Info3 = "" });
        }

        private void btnfqtoken_Click(object sender, RoutedEventArgs e)
        {
            Common.Handler.Base.BaseHandler.Instance.PublishAppMsg(new PResult() { Code = "getFQ", Info1 = "WebFQ", Info2 = "GetToken", Info3 = "" });
        }
    }
}
