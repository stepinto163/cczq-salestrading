using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SalesTrading.UiTests
{
    internal static class DesktopPresentationCheck
    {
        [STAThread]
        public static int Main()
        {
            var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            application.Dispatcher.BeginInvoke(new Action(async () =>
            {
                Window actor = null;
                Window observer = null;
                try
                {
                    int combinations = 0;
                    foreach (double scale in new[] { 1.0, 1.25, 1.5, 1.75, 2.0 })
                    foreach (int width in new[] { 1920, 2560, 3840 })
                    foreach (int origin in new[] { 0, -3840 })
                    {
                        var workArea = new Rect(origin / scale, 40 / scale, width / scale, 1040 / scale);
                        Rect first = DesktopPresentation.ClientBounds(workArea, true, false);
                        Rect second = DesktopPresentation.ClientBounds(workArea, true, true);
                        Rect caption = DesktopPresentation.CaptionBounds(workArea);
                        Require(workArea.Contains(first) && workArea.Contains(second), "Client escaped work area");
                        Require(!first.IntersectsWith(caption) && !second.IntersectsWith(caption), "Caption overlaps client");
                        Require(first != second, "Accounts must not occupy identical bounds");
                        if (workArea.Width >= 2236) Require(!first.IntersectsWith(second), "Wide screen must use separate columns");
                        combinations++;
                    }
                    actor = new Window { Title = "布局回归 · 操作账号", Width = 440, Height = 220,
                        Background = Brushes.DarkSlateBlue, WindowStyle = WindowStyle.None };
                    actor.Show();
                    Rect area = DesktopPresentation.WorkArea(actor);
                    actor.Left = area.Left + 50;
                    actor.Top = area.Top + 50;
                    actor.Activate();
                    await Task.Delay(100);
                    DesktopPresentation.VerifyOnScreen(actor);
                    DesktopPresentation.VerifyUncovered(actor);
                    observer = new Window { Title = "布局回归 · 遮挡检测", Left = actor.Left, Top = actor.Top,
                        Width = actor.Width, Height = actor.Height, Background = Brushes.DarkGreen,
                        WindowStyle = WindowStyle.None, Topmost = true };
                    observer.Show();
                    await Task.Delay(100);
                    bool rejected = false;
                    try { DesktopPresentation.VerifyUncovered(actor); }
                    catch (InvalidOperationException) { rejected = true; }
                    Require(rejected, "Occluded window incorrectly passed visibility check");
                    observer.Close();
                    observer = null;
                    actor.Left = area.Right + 100;
                    rejected = false;
                    try { DesktopPresentation.VerifyOnScreen(actor); }
                    catch (InvalidOperationException) { rejected = true; }
                    Require(rejected, "Off-screen window incorrectly passed visibility check");
                    Console.WriteLine("PASS: " + combinations + " layout combinations (100–200%, negative monitor origins); real on-screen, occlusion and off-screen rejection");
                    application.Shutdown(0);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                    application.Shutdown(1);
                }
                finally
                {
                    observer?.Close();
                    actor?.Close();
                }
            }));
            return application.Run();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
