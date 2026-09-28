using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

internal static class NativeStartupCheck
{
    private static bool submitted;
    private static int ticks;
    private static int captureIndex;

    [STAThread]
    public static int Main()
    {
        try
        {
            STC.App app = new STC.App();
            app.InitializeComponent();
            app.DispatcherUnhandledException += (sender, args) =>
            {
                File.WriteAllText("native-startup-result.txt", "FAIL: " + args.Exception);
                Environment.Exit(1);
            };
            DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (sender, args) =>
            {
                ticks++;
                STC.Frame.Window1 shell = app.Windows.OfType<STC.Frame.Window1>().FirstOrDefault();
                var sales = shell?.Panel.Items.OfType<TabItem>().Select(item => item.Content)
                    .OfType<STC.SalesTrading.Plugin.Shell.View.MainView>().FirstOrDefault();
                if (shell != null && shell.IsVisible && sales != null && ((STC.SalesTrading.Plugin.Shell.MainViewModel)sales.DataContext).Backend != null && ticks >= 5)
                {
                    shell.WindowState = WindowState.Normal;
                    shell.Width = 1440;
                    shell.Height = 1000;
                    shell.UpdateLayout();
                    double scale = Math.Min(1.0, 1440.0 / shell.ActualWidth);
                    DrawingVisual drawing = new DrawingVisual();
                    using (DrawingContext context = drawing.RenderOpen())
                    {
                        context.DrawRectangle(new VisualBrush(shell), null,
                            new Rect(0, 0, shell.ActualWidth * scale, shell.ActualHeight * scale));
                    }
                    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)(shell.ActualWidth * scale),
                        (int)(shell.ActualHeight * scale), 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(drawing);
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    string[] images = { "native-salestrading.png", "native-precise.png", "native-market.png", "native-management.png" };
                    using (FileStream file = File.Create(images[captureIndex]))
                    {
                        encoder.Save(file);
                    }
                    if (captureIndex == 3)
                    {
                        RenderTargetBitmap contentBitmap = new RenderTargetBitmap((int)sales.ActualWidth, (int)sales.ActualHeight,
                            96, 96, PixelFormats.Pbgra32);
                        DrawingVisual contentDrawing = new DrawingVisual();
                        using (DrawingContext context = contentDrawing.RenderOpen())
                        {
                            context.DrawRectangle(new VisualBrush(sales), null,
                                new Rect(0, 0, sales.ActualWidth, sales.ActualHeight));
                        }
                        contentBitmap.Render(contentDrawing);
                        PngBitmapEncoder contentEncoder = new PngBitmapEncoder();
                        contentEncoder.Frames.Add(BitmapFrame.Create(contentBitmap));
                        using (FileStream file = File.Create("native-management-content.png"))
                        {
                            contentEncoder.Save(file);
                        }
                        File.WriteAllText("native-management-size.txt", ((int)sales.ActualWidth) + "," + ((int)sales.ActualHeight));
                    }
                    captureIndex++;
                    if (captureIndex < images.Length)
                    {
                        if (captureIndex == 1)
                        {
                            var model = (STC.SalesTrading.Plugin.Shell.MainViewModel)sales.DataContext;
                            model.SelectedNavItem = model.NavItems.First(item => item.Key == "quote");
                        }
                        else
                        {
                            var model = (STC.SalesTrading.Plugin.Shell.MainViewModel)sales.DataContext;
                            string key = captureIndex == 2 ? "market" : "mgmt";
                            model.SelectedNavItem = model.NavItems.First(item => item.Key == key);
                        }
                        shell.UpdateLayout();
                        return;
                    }
                    File.WriteAllText("native-startup-result.txt", "PASS: native STC login, sales-trading plugin and four live views initialized through local GW.");
                    timer.Stop();
                    app.Shutdown(0);
                    return;
                }
                STC.Frame.UI.LoginGUI login = app.Windows.OfType<STC.Frame.UI.LoginGUI>().FirstOrDefault();
                if (login != null && login.IsLoaded && !submitted)
                {
                    submitted = true;
                    ((TextBox)login.FindName("tbUsername")).Text = "trader1";
                    ((PasswordBox)login.FindName("pbPwd")).Password = "trader1";
                    ((Button)login.FindName("btnLogin")).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
                if (ticks >= 25)
                {
                    File.WriteAllText("native-startup-result.txt", "FAIL: native startup timed out.");
                    timer.Stop();
                    app.Shutdown(1);
                }
            };
            timer.Start();
            int exitCode = app.Run();
            Environment.Exit(exitCode);
            return exitCode;
        }
        catch (Exception error)
        {
            File.WriteAllText("native-startup-result.txt", "FAIL: " + error);
            Environment.Exit(1);
            return 1;
        }
    }
}
