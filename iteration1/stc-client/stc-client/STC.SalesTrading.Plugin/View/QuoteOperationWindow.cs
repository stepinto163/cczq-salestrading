using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc;

namespace STC.SalesTrading.Plugin.View
{
    // Reuses the existing CRM-aware editor and lifecycle client inside the colleague's product shell.
    public sealed class QuoteOperationWindow : Window
    {
        private readonly FiccApiClient api;
        private readonly TextBlock status = new TextBlock { Margin = new Thickness(12), TextWrapping = TextWrapping.Wrap };
        private readonly WrapPanel commands = new WrapPanel();
        private readonly string clientQuoteId = Guid.NewGuid().ToString("N");
        public QuoteEditor Editor { get; }
        public JObject CurrentQuote { get; private set; }
        private bool initialized;
        public bool IsReady => initialized && commands.IsEnabled;

        public QuoteOperationWindow(FiccApiClient api, string quoteId = null, string securityId = null)
        {
            this.api = api;
            Title = quoteId == null ? "手工报价" : "报价操作 · " + quoteId;
            Width = 940;
            Height = 540;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = (Brush)new BrushConverter().ConvertFromString("#0D1117");
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/STC.SalesTrading.Plugin;component/View/ProductTheme.xaml", UriKind.Relative)
            });
            Editor = new QuoteEditor(api, error => status.Text = error);
            DockPanel panel = new DockPanel();
            foreach (string command in new[] { "create|保存待确认", "confirm|确认", "update|保存修改", "refer|Refer", "unrefer|Unrefer", "delete|删除" })
            {
                string[] parts = command.Split('|');
                commands.Children.Add(UiElements.Button(parts[1], async () => await RunAsync(() => OperateAsync(parts[0]))));
            }
            commands.Children.Add(UiElements.Button("变更历史", async () => await RunAsync(async () =>
            {
                if (CurrentQuote == null)
                {
                    throw new InvalidOperationException("请先保存或读取报价。");
                }
                JObject history = await api.RequestAsync("ficc.quote.history.query", new JObject { ["quoteId"] = CurrentQuote["quoteId"], ["limit"] = 100 });
                new Window
                {
                    Owner = this, Title = "报价变更历史", Width = 800, Height = 600,
                    Content = new TextBox { Text = history.ToString(), IsReadOnly = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }
                }.ShowDialog();
            })));
            DockPanel.SetDock(commands, Dock.Bottom);
            DockPanel.SetDock(status, Dock.Bottom);
            panel.Children.Add(commands);
            panel.Children.Add(status);
            panel.Children.Add(new ScrollViewer { Content = Editor, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
            Content = panel;
            commands.IsEnabled = false;
            Loaded += async (sender, args) => await RunAsync(async () =>
            {
                Editor.SetDictionary(await api.RequestAsync("ficc.quote.dictionary.query", new JObject()));
                if (quoteId != null)
                {
                    CurrentQuote = await api.GetQuoteAsync(quoteId);
                    Editor.Set(CurrentQuote);
                }
                else if (securityId != null)
                {
                    Editor.Set(new JObject { ["securityId"] = securityId, ["side"] = "1", ["quoteType"] = "1", ["yieldType"] = "1",
                        ["settlType"] = "2", ["tradeDate"] = DateTime.Today.ToString("yyyy-MM-dd") });
                }
                initialized = true;
            });
        }

        public async Task OperateAsync(string operation)
        {
            if (operation == "create")
            {
                if (CurrentQuote != null)
                {
                    throw new InvalidOperationException("此报价已经保存，请使用确认或修改。");
                }
                JObject draft = Editor.Read();
                draft["clientQuoteId"] = clientQuoteId;
                JArray results = FiccApiClient.RequireQuoteResults(await api.CreateAsync(new[] { draft }));
                CurrentQuote = await api.GetQuoteAsync((string)results[0]["quoteId"]);
            }
            else
            {
                if (CurrentQuote == null)
                {
                    throw new InvalidOperationException("请先保存待确认报价。");
                }
                JObject changes = operation == "update" || operation == "confirm" ? Editor.Read() : null;
                FiccApiClient.RequireQuoteResults(await api.OperateAsync(operation, new[] { CurrentQuote }, changes));
                CurrentQuote = await api.GetQuoteAsync((string)CurrentQuote["quoteId"]);
            }
            Editor.Set(CurrentQuote);
            status.Text = CurrentQuote["quoteId"] + " · " + CurrentQuote["status"] + " · revision " + CurrentQuote["revision"];
        }

        private async Task RunAsync(Func<Task> action)
        {
            commands.IsEnabled = false;
            try
            {
                await action();
            }
            catch (Exception error)
            {
                com.gateway.log.LogHelper.Error(error);
                status.Text = error.Message;
            }
            finally
            {
                commands.IsEnabled = initialized;
            }
        }
    }
}
