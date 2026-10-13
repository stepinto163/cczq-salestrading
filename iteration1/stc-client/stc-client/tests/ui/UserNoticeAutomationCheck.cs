using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using STC.Frame.CustomeUI;

namespace SalesTrading.UiTests
{
    internal static class UserNoticeAutomationCheck
    {
        [STAThread]
        public static int Main()
        {
            STC.App application = new STC.App();
            application.InitializeComponent();
            application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            application.DispatcherUnhandledException += (sender, args) =>
            {
                Console.Error.WriteLine(args.Exception);
                Environment.Exit(1);
            };
            application.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Window unrelated = new Window { Title = "用户使用须知", Width = 250, Height = 100 };
                    unrelated.Show();
                    int dismissed = 0;
                    DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                    timer.Tick += (sender, args) => UserNoticeAutomation.DismissVisible(message => dismissed++);
                    timer.Start();
                    for (int index = 0; index < 2; index++)
                    {
                        bool accepted = false;
                        UserNoticeView notice = new UserNoticeView { Owner = unrelated };
                        notice.OnAction += (sender, checkedValue, doubleCheck) => accepted = checkedValue;
                        notice.ShowDialog();
                        if (!accepted || notice.IsVisible || !unrelated.IsVisible)
                        {
                            throw new Exception("Notice was not acknowledged, or an unrelated window was closed.");
                        }
                        if (((CheckBox)notice.FindName("lbAllOrder")).IsChecked != true)
                        {
                            throw new Exception("Acknowledgement was not selected.");
                        }
                    }
                    timer.Stop();
                    if (dismissed != 2) throw new Exception("Expected two dismissal log callbacks.");
                    Console.WriteLine("PASS: two real modal UserNoticeView dialogs acknowledged and closed; unrelated same-title window retained.");
                    Environment.Exit(0);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                    Environment.Exit(1);
                }
            }), DispatcherPriority.ApplicationIdle);
            return application.Run();
        }
    }
}
