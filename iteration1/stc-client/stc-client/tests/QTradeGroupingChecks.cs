using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;

internal static class QTradeGroupingChecks
{
    private static int passed;

    [STAThread]
    public static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        int result = 1;
        application.Dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                await RunAsync(args[0]);
                Console.WriteLine("PASS " + passed + " QTrade grouping checks; persisted backend fixture, local WPF controls.");
                result = 0;
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(error);
            }
            finally
            {
                application.Shutdown();
            }
        }));
        application.Run();
        return result;
    }

    private static void Check(bool success, string description)
    {
        if (!success)
        {
            throw new InvalidOperationException(description);
        }
        passed++;
        Console.WriteLine("PASS " + description);
    }

    private static async Task RunAsync(string fixture)
    {
        MethodInfo display = typeof(FiccApiClient).Assembly
            .GetType("STC.SalesTrading.Plugin.Ficc.DpsModelMapper").GetMethod("Display");
        Func<JObject, QuoteConfirmRow> map = quote =>
            ((JObject)display.Invoke(null, new object[] { quote })).ToObject<QuoteConfirmRow>();
        JObject[] quotes = JArray.Parse(System.IO.File.ReadAllText(fixture)).OfType<JObject>().ToArray();
        var service = new PendingService();
        service.Rows.AddRange(quotes.Select(map));
        Check(service.Rows[0].BatchId == service.Rows[1].BatchId
            && service.Rows[0].BatchId != service.Rows[2].BatchId, "DPS mapping retains original-message groups");
        JObject edited = (JObject)quotes[0].DeepClone();
        edited["remark"] = "edited remark";
        Check(map(edited).TextContent == (string)quotes[0]["source"]["text"], "original text survives remark edits");
        edited["sourceMessageId"] = JValue.CreateNull();
        Check(map(edited).BatchId == (string)edited["quoteId"], "null source falls back to separate quote identity");

        using (var model = new QuoteConfirmViewModel(service))
        {
            var view = new QuoteConfirmView { DataContext = model };
            var window = new Window
            {
                Content = view, Width = 1200, Height = 800,
                ShowActivated = false, ShowInTaskbar = false, Left = -20000, Top = -20000
            };
            try
            {
                window.Show();
                await Idle();
                window.UpdateLayout();
                Check(model.Batches.Count == 2 && model.Batches[0].Rows.Count == 2, "confirmation view displays one header for two source quotes");
                QuoteConfirmBatchViewModel batch = model.Batches[0];
                CheckBox groupCheck = Descendants<CheckBox>(view).First(control => ReferenceEquals(control.DataContext, batch));
                typeof(ToggleButton).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(groupCheck, null);
                await Idle();
                Check(model.SelectedCount == 2 && batch.Rows.All(row => row.IsSelected)
                    && !model.Batches[1].Rows[0].IsSelected, "rendered group checkbox selects only its message");
                batch.IsExpanded = false;
                JObject arriving = (JObject)quotes[0].DeepClone();
                arriving["quoteId"] = "Q-late-arrival";
                service.Rows.Add(map(arriving));
                await (Task)typeof(QuoteConfirmViewModel).GetMethod("InitializeAsync", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(model, null);
                await Idle();
                Check(model.Batches.Count == 2 && model.Batches[0].Rows.Count == 3, "late quote joins existing message group");
                Check(model.SelectedCount == 2 && !model.Batches[0].IsExpanded
                    && model.Batches[0].IsGroupChecked == null, "refresh preserves selection and collapse without selecting unseen quotes");
                Button confirm = Descendants<Button>(view).First(button => ReferenceEquals(button.Command, model.ConfirmSelectedCommand));
                typeof(Button).GetMethod("OnClick", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(confirm, null);
                if (model.ConfirmSelectedCommand.ExecutionTask != null)
                {
                    await model.ConfirmSelectedCommand.ExecutionTask;
                }
                Check(service.Confirmed.Count == 2 && model.Batches.Count == 2
                    && model.Batches[0].Rows.Count == 1, "partial confirmation keeps remaining group member");
                model.Batches[0].Rows[0].IsSelected = true;
                await model.DeleteSelectedCommand.ExecuteAsync(null);
                Check(service.Deleted.Single() == "Q-late-arrival" && model.Batches.Count == 1,
                    "removing last pending member removes only its group");
                QuoteConfirmRowViewModel selectedPending = model.Batches[0].Rows[0];
                selectedPending.IsSelected = true;
                selectedPending.Price = "2.6789";
                await (Task)typeof(QuoteConfirmViewModel).GetMethod("InitializeAsync", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(model, null);
                Check(ReferenceEquals(model.Batches[0].Rows[0], selectedPending)
                    && selectedPending.Price == "2.6789" && model.SelectedCount == 1,
                    "pending refresh preserves selected unsubmitted edits at unchanged revision");
                service.Rows.Clear();
                typeof(QuoteConfirmViewModel).GetMethod("OnBackendChanged", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(model, null);
                await Idle();
                Check(model.TotalRowCount == 0 && model.SelectedCount == 0,
                    "confirmed or deleted quote leaves pending view even while selected");
            }
            finally
            {
                window.Close();
            }
        }
    }

    private static async Task Idle()
    {
        await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                yield return match;
            }
            foreach (T descendant in Descendants<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private sealed class PendingService : IQuoteService
    {
        public readonly List<QuoteConfirmRow> Rows = new List<QuoteConfirmRow>();
        public readonly List<string> Confirmed = new List<string>();
        public readonly List<string> Deleted = new List<string>();
        public Task<IReadOnlyList<QuoteConfirmRow>> LoadPendingRowsAsync(CancellationToken token)
        {
            return Task.FromResult<IReadOnlyList<QuoteConfirmRow>>(Rows.ToArray());
        }
        public Task ConfirmQuotesAsync(IReadOnlyCollection<string> ids, CancellationToken token)
        {
            Confirmed.AddRange(ids);
            Rows.RemoveAll(row => ids.Contains(row.RecognitionId));
            return Task.CompletedTask;
        }
        public Task DeleteQuotesAsync(IReadOnlyCollection<string> ids, CancellationToken token)
        {
            Deleted.AddRange(ids);
            Rows.RemoveAll(row => ids.Contains(row.RecognitionId));
            return Task.CompletedTask;
        }
        public Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken token) { throw new NotSupportedException(); }
        public Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken token) { throw new NotSupportedException(); }
        public Task SubmitQuotesAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken token) { throw new NotSupportedException(); }
        public Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken token) { throw new NotSupportedException(); }
    }
}
