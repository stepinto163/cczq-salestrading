using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SalesTrading.UiTests
{
    // 仅由自动化工作进程调用；普通客户端保留原有须知和确认流程。
    internal static class UserNoticeAutomation
    {
        public static void DismissVisible(Action<string> report)
        {
            foreach (Window window in Application.Current.Windows.Cast<Window>().ToArray())
            {
                if (!window.IsVisible || window.GetType().FullName != "STC.Frame.CustomeUI.UserNoticeView")
                {
                    continue;
                }
                CheckBox acknowledgement = window.FindName("lbAllOrder") as CheckBox;
                Button confirm = window.FindName("btnOk") as Button;
                if (acknowledgement == null || confirm == null)
                {
                    throw new InvalidOperationException("用户使用须知控件已变化，无法自动确认。");
                }
                // 走真实点击事件以启用确定按钮；直接 Close 会触发主窗口退出。
                if (acknowledgement.IsChecked != true)
                {
                    Click(acknowledgement);
                }
                Click(confirm);
                if (window.IsVisible)
                {
                    throw new InvalidOperationException("用户使用须知确认后仍未关闭。");
                }
                report("自动化工作进程已勾选用户使用须知并点击确定。");
            }
        }

        private static void Click(ButtonBase button)
        {
            if (!button.IsVisible || !button.IsEnabled)
            {
                throw new InvalidOperationException("用户使用须知按钮不可用：" + button.Name);
            }
            button.GetType().GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(button, null);
        }
    }
}
