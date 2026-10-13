using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using netapi.Api;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Modules.MarketCenter.View;
using STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel;

internal static class RecommendationChecks
{
    private static int checks;
    private static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception(message);
        checks++;
        Console.WriteLine("PASS " + message);
    }
    [STAThread]
    public static int Main(string[] args)
    {
        try
        {
            new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
            var fixtures = JArray.Parse(File.ReadAllText(args[0]));
            var transport = new FixtureTransport(fixtures);
            var service = new DpsRecommendationService(new FiccApiClient(transport), id => "内部-" + id);
            var model = new BondDetailViewModel { Code = "250001.IB", HasDetail = true, SelectedRecWindow = "90d" };
            model.AttachRecommendations(service);
            WaitUntil(() => !model.RecommendationLoading);
            Check(model.Recommendations.Count == 3 && model.RecBidCount == 2 && model.RecOfrCount == 1, "Backend PostgreSQL response reaches recommendation VM: " + model.RecommendationStatus);
            Check(model.SelectedRecSide == QuoteDirection.Bid && model.RecPageItems.Count == 2, "Default Bid and per-side counts");
            var own = model.Recommendations.Single(row => row.OwnerTrader == "内部-A");
            Check(own.InstitutionName == "private-institution-A" && own.Evidence[0].Trader == "private-contact-A", "Own list and evidence preserve identity");
            Check(model.Recommendations.Where(row => row.OwnerTrader == "内部-B").All(row => row.InstitutionName == null && row.TraderName == null
                && row.Evidence.All(e => e.Institution == null && e.Trader == null)), "Cross-owner typed UI model remains masked");
            model.SelectedRecSide = QuoteDirection.Ofr;
            Check(model.RecPageItems.Count == 1, "Ofr switches results");
            model.SelectedRecSide = QuoteDirection.Bid;
            var panel = new BondDetailPanel { DataContext = model };
            panel.Measure(new Size(1050, 1150));
            panel.Arrange(new Rect(0, 0, 1050, 1150));
            panel.UpdateLayout();
            Pump();
            var grid = (DataGrid)panel.FindName("RecommendationGrid");
            grid.ScrollIntoView(own);
            grid.UpdateLayout();
            Pump();
            var rowContainer = (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(own);
            Check(rowContainer != null, "Actual recommendation XAML renders row");
            var expand = Children(rowContainer).OfType<Button>().First(button => (string)button.Content == "展开");
            expand.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            grid.UpdateLayout();
            Pump();
            Check(rowContainer.DetailsVisibility == Visibility.Visible, "Evidence button expands actual row details");
            Check(Children(rowContainer).OfType<TextBlock>().Any(text => text.Text == "private-contact-A"), "Expanded evidence renders trader");
            grid.ScrollIntoView(own, grid.Columns.Last());
            grid.UpdateLayout();
            Pump();
            var feedback = Children(rowContainer).OfType<Button>().First(button => (string)button.Content == "有用");
            feedback.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            WaitUntil(() => !own.CanFeedback);
            Check(own.Feedback == "USEFUL" && own.FeedbackText.Contains("有用"), "UI feedback button uses typed API and displays persisted result");
            Check(!feedback.IsEnabled, "Feedback buttons lock after successful feedback");
            var bitmap = new RenderTargetBitmap(1050, 1150, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(panel);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var output = File.Create("recommendation-panel.png")) encoder.Save(output);
            int requests = transport.Queries;
            model.SelectedRecWindow = "7d";
            WaitUntil(() => !model.RecommendationLoading);
            Check(transport.Queries > requests && (DateTime.Parse((string)transport.LastRequest["endDate"]) - DateTime.Parse((string)transport.LastRequest["startDate"])).Days == 6,
                "Preset changes send inclusive seven-day window");
            model.RecStartDate = DateTime.Today.AddDays(-365);
            model.RecEndDate = DateTime.Today;
            WaitUntil(() => !model.RecommendationLoading);
            Check(model.RecommendationStatus.Contains("365") && model.Recommendations.Count == 0, "Oversized custom window shows error and clears stale rows");
            model.RecStartDate = DateTime.Today.AddDays(-364);
            WaitUntil(() => !model.RecommendationLoading);
            Check(model.Recommendations.Count == 3, "365-day window loads");
            transport.Fail = true;
            model.ReloadRecommendationsAsync();
            WaitUntil(() => !model.RecommendationLoading);
            Check(model.RecommendationStatus.Contains("失败") && model.Recommendations.Count == 0, "Service failure is visible without stale results");
            model.StopRecommendations();
            CheckStaleResponse();
            if (args.Length > 1 && args[1] == "live") CheckLive();
            Console.WriteLine("PASS RecommendationChecks: " + checks);
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
    private static void CheckStaleResponse()
    {
        var service = new DelayedService();
        var model = new BondDetailViewModel { Code = "240210.IB", HasDetail = true };
        model.AttachRecommendations(service);
        WaitUntil(() => service.Pending.Count == 1);
        model.SelectedRecWindow = "7d";
        WaitUntil(() => service.Pending.Count == 2);
        service.Pending[1].SetResult(new[] { new RecommendationRow { RelatedCode = "new", Side = QuoteDirection.Bid } });
        WaitUntil(() => !model.RecommendationLoading);
        service.Pending[0].SetResult(new[] { new RecommendationRow { RelatedCode = "old", Side = QuoteDirection.Bid } });
        Pump();
        Check(model.Recommendations.Single().RelatedCode == "new", "Late old-window reply cannot overwrite new selection");
        model.StopRecommendations();
    }
    private static void CheckLive()
    {
        var gateway = new GWApi();
        try
        {
            string user = Environment.GetEnvironmentVariable("FICC_TEST_USER");
            var connect = Task.Run(() =>
            {
                gateway.NewApi();
                gateway.SetGzip(true);
                return JObject.Parse(((com.gateway.connector.tcp.GateWayApi)gateway.GetApiConnect()).connect("127.0.0.1", 28600,
                    user, Environment.GetEnvironmentVariable("FICC_TEST_PASSWORD"), false, 1, new Dictionary<string, object> { ["isManage"] = "N" }));
            });
            WaitUntil(() => connect.IsCompleted);
            var login = connect.GetAwaiter().GetResult();
            Check((string)login["Code"] == "0001", "Live STC login");
            gateway.UserName = user;
            gateway.Sid = (string)login["sid"];
            var service = new DpsRecommendationService(new FiccApiClient(new LiveTransport(gateway)), id => id);
            var model = new BondDetailViewModel { Code = "2600004.IB", HasDetail = true, SelectedRecWindow = "90d" };
            model.AttachRecommendations(service);
            WaitUntil(() => !model.RecommendationLoading);
            Check(!model.RecommendationStatus.Contains("失败"), "Live gateway -> QuoteSvr -> history -> typed client -> VM");
            Check(model.Recommendations.Count > 0, "Live historical quotes produce actual recommendations");
            var panel = new BondDetailPanel { DataContext = model };
            panel.Measure(new Size(1050, 1100));
            panel.Arrange(new Rect(0, 0, 1050, 1100));
            panel.UpdateLayout();
            Pump();
            var grid = (DataGrid)panel.FindName("RecommendationGrid");
            Check(grid.Items.Count > 0, "Live recommendations render in real WPF panel");
            var bitmap = new RenderTargetBitmap(1050, 1100, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(panel);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var output = File.Create("live-recommendation-panel.png")) encoder.Save(output);
            Console.WriteLine("Live recommendations=" + model.Recommendations.Count);
            File.WriteAllText("live-recommendation-count.txt", model.Recommendations.Count.ToString());
            model.StopRecommendations();
        }
        finally { gateway.DisConnect(); }
    }
    private static void WaitUntil(Func<bool> condition)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (!condition())
        {
            if (watch.ElapsedMilliseconds > 8000) throw new TimeoutException("Functional check timed out");
            Pump();
            Thread.Sleep(10);
        }
        Pump();
    }
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }
    private static IEnumerable<DependencyObject> Children(DependencyObject root)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            yield return child;
            foreach (var nested in Children(child)) yield return nested;
        }
    }
    private sealed class FixtureTransport : IFiccTransport
    {
        private readonly JArray fixtures;
        public FixtureTransport(JArray fixtures) { this.fixtures = fixtures; }
        public bool Fail;
        public int Queries;
        public JObject LastRequest;
        public Task<JObject> RequestAsync(string service, string topic, JObject request)
        {
            if (Fail) throw new InvalidOperationException("test offline");
            if (topic.EndsWith(".query")) { Queries++; LastRequest = request; }
            var fixture = fixtures.OfType<JObject>().First(item => (string)item["topic"] == topic && (string)item["reply"]["Code"] == "0000");
            var reply = (JObject)fixture["reply"].DeepClone();
            reply["requestId"] = request["requestId"];
            return Task.FromResult(reply);
        }
        public IDisposable Subscribe(string service, string topic, Action<JObject> receive, bool withImage = false) { throw new NotSupportedException(); }
        public event Action ConnectionChanged { add { } remove { } }
    }
    private sealed class LiveTransport : IFiccTransport
    {
        private readonly GWApi api;
        public LiveTransport(GWApi api) { this.api = api; }
        public Task<JObject> RequestAsync(string service, string topic, JObject request) { return Task.Run(() => api.Request<JObject>(service, topic, request, 5000)); }
        public IDisposable Subscribe(string service, string topic, Action<JObject> receive, bool withImage = false) { throw new NotSupportedException(); }
        public event Action ConnectionChanged { add { } remove { } }
    }
    private sealed class DelayedService : IRecommendationService
    {
        public List<TaskCompletionSource<IReadOnlyList<RecommendationRow>>> Pending = new List<TaskCompletionSource<IReadOnlyList<RecommendationRow>>>();
        public Task<IReadOnlyList<RecommendationRow>> QueryRecommendationsAsync(string securityId, DateTime start, DateTime end, CancellationToken cancellation)
        {
            var completion = new TaskCompletionSource<IReadOnlyList<RecommendationRow>>();
            Pending.Add(completion);
            return completion.Task;
        }
        public Task<string> SubmitFeedbackAsync(string securityId, DateTime start, DateTime end, string recommendationId, string feedback) { return Task.FromResult(feedback); }
    }
}
