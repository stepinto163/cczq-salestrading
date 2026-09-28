using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
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
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;

internal static class ParserIntegrationChecks
{
    private static int passed;

    [STAThread]
    public static int Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        STC.Common.Utils.LogHandler.InitLog("parser-check-logs/Parser", "INFO");
        Application application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        int exitCode = 1;
        application.Dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                await RunAsync();
                Console.WriteLine("RESULT PASS: " + passed + " checks; fixture responses, no live parser or database writes.");
                exitCode = 0;
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
        return exitCode;
    }

    private static void Check(bool condition, string name)
    {
        if (!condition)
        {
            throw new InvalidOperationException("FAIL " + name);
        }
        passed++;
        Console.WriteLine("PASS " + name);
    }

    private static JObject Reply()
    {
        // Field names and price codes are from ParserManager.convertIntentionQuote.
        return JObject.Parse(@"{
            'Code':'0000', 'Msg':'成功', 'TotalSize':2, 'Datas':[
                {'securityId':'240210.IB','symbol':'24国开10','side':'1','priceType':'3',
                 'price':2.5,'qty':1000,'settlType':'2','text':'24国开10 bid 2.5 1000',
                 'institutionId':'I-1','institutionName':'测试机构','traderId':'C-1','traderName':'测试交易员',
                 'clordid':'parser-only-id','status':'0','chinaBondValuation':2.45},
                {'securityId':'240210.IB','side':'2','priceType':'1','price':101.12345,
                 'qty':2000,'settlType':'1','text':'240210 ofr 净价 101.12345 2000'}
            ]}");
    }

    private static async Task RunAsync()
    {
        FakeTransport transport = new FakeTransport { Response = Reply() };
        StcQuoteParserClient parser = new StcQuoteParserClient(transport, "test-parser");
        QuoteParseResult parsed = await parser.ParseAsync("原始多行文本\n第二行", CancellationToken.None);
        Check(transport.Service == "ParserSvr" && transport.Topic == "stc.parser.bond", "uses STC parser service/topic");
        Check((string)transport.Request["userName"] == "test-parser"
            && (string)transport.Request["content"] == "原始多行文本\n第二行"
            && transport.Request.Properties().Count() == 2, "request uses authenticated username and intact text");
        Check(parsed.Rows.Count == 2 && transport.Calls == 1, "recognition only requests parser; no quote writes");
        QuoteConfirmRow row = parsed.Rows[0];
        Check(row.PriceType == PriceType.YieldToMaturity && row.Volume == 1000
            && row.Direction == QuoteDirection.Bid && row.SettlementCode == "2", "yield/side/quantity/settlement mapping");
        Check(row.InstitutionId == "I-1" && row.ContactId == "C-1" && row.Trader == "测试交易员", "CRM IDs and names retained");
        Check(row.RecognitionId.StartsWith("PARSE-") && row.RecognitionId != "parser-only-id"
            && row.Revision == 0, "parser identity is separate from persisted quote identity");
        Check(parsed.Rows[1].PriceType == PriceType.CleanPrice && parsed.Rows[1].Price == 101.12345m, "clean price mapping retains precision");

        QuoteConfirmRowViewModel edit = new QuoteConfirmRowViewModel(parsed.Rows[1]);
        Check(edit.Price == "101.12345", "preview does not round parsed price");
        edit.Commit();
        MethodInfo inputMethod = typeof(FiccApiClient).Assembly
            .GetType("STC.SalesTrading.Plugin.Ficc.DpsModelMapper").GetMethod("Input");
        JObject input = (JObject)inputMethod.Invoke(null, new object[] { edit.Row });
        input = JObject.Parse(input.ToString());
        Check((string)input["quoteType"] == "2" && input["yieldType"].Type == JTokenType.Null
            && (string)input["price"] == "101.12345" && (string)input["quantityUnit"] == "WAN", "clean price submission matches QuoteSvr dictionary");
        Check((string)input["remark"] == edit.TextContent, "original text is retained on submission");
        edit.Price = "invalid";
        try { edit.Commit(); throw new Exception("Invalid price accepted"); }
        catch (InvalidOperationException) { Check(true, "invalid edit cannot silently submit previous price"); }

        QuoteConfirmRowViewModel crmEdit = new QuoteConfirmRowViewModel(row);
        crmEdit.Institution = "另一机构";
        crmEdit.Commit();
        Check(row.InstitutionId == null && row.ContactId == null, "editing institution invalidates both CRM IDs");

        transport.Response = Reply();
        ((JObject)transport.Response["Datas"][0])["priceType"] = "2";
        ((JObject)transport.Response["Datas"][1])["priceType"] = "7";
        parsed = await parser.ParseAsync("text", CancellationToken.None);
        Check(parsed.Rows[0].PriceType == PriceType.DirtyPrice && parsed.Rows[1].PriceType == PriceType.YieldToExercise, "full price/exercise-yield mapping");

        transport.Response = Reply();
        ((JObject)transport.Response["Datas"][0])["institutionId"] = null;
        parsed = await parser.ParseAsync("text", CancellationToken.None);
        Check(parsed.Warnings.Any(w => w.Contains("CRM")), "unresolved CRM produces review warning");

        int previousCalls = transport.Calls;
        try { await parser.ParseAsync("text", new CancellationToken(true)); throw new Exception("Cancellation ignored"); }
        catch (OperationCanceledException) { Check(transport.Calls == previousCalls, "cancelled request sends no message"); }

        TaskCompletionSource<JObject> pending = new TaskCompletionSource<JObject>();
        transport.Pending = pending.Task;
        CancellationTokenSource cancellation = new CancellationTokenSource();
        Task<QuoteParseResult> cancelled = parser.ParseAsync("text", cancellation.Token);
        cancellation.Cancel();
        pending.SetResult(Reply());
        try { await cancelled; throw new Exception("Stale reply applied"); }
        catch (OperationCanceledException) { Check(true, "reply arriving after cancellation is discarded"); }
        transport.Pending = null;

        ParserQuoteService quoteService = new ParserQuoteService(parser);
        using (PreciseQuoteViewModel viewModel = new PreciseQuoteViewModel(quoteService))
        {
            transport.Response = Reply();
            viewModel.InputText = "240210 bid 2.5 1000";
            await viewModel.IdentifyCommand.ExecuteAsync(null);
            Check(viewModel.Rows.Count == 2 && !viewModel.IsParsing, "existing precise-quote command populates preview");
            await CheckRenderedSelectionAsync(viewModel, quoteService);
            viewModel.InputText = "240210 bid 2.5 1000";
            await viewModel.IdentifyCommand.ExecuteAsync(null);

            transport.Response = JObject.Parse("{'Code':'2003','Msg':'未识别到报价'}");
            viewModel.InputText = "unrecognized original";
            await viewModel.IdentifyCommand.ExecuteAsync(null);
            Check(viewModel.Rows.Count == 2 && viewModel.InputText == "unrecognized original"
                && viewModel.StatusMessage.Contains("2003") && !viewModel.IsParsing,
                "service error retains input/preview, exposes code and releases busy state");

            transport.Response = JObject.Parse("{'Code':'0000','Datas':[]}");
            await viewModel.IdentifyCommand.ExecuteAsync(null);
            Check(viewModel.Rows.Count == 0 && viewModel.InputText == "unrecognized original"
                && viewModel.StatusMessage.Contains("未识别到报价"), "empty result is visible and retains text");
        }

        transport.Response = JObject.Parse("{'Code':'0000'}");
        try { await parser.ParseAsync("text", CancellationToken.None); throw new Exception("Malformed reply accepted"); }
        catch (InvalidOperationException error) { Check(error.Message.Contains("Datas"), "malformed successful reply fails explicitly"); }

        await CheckSubmissionRetryAsync();

        List<string> messages = new List<string>();
        using (StcFiccTransport hostTransport = new StcFiccTransport(
            (netapi.Api.IApi)new HostApiProxy().GetTransparentProxy(), SynchronizationContext.Current))
        {
            hostTransport.MessageLogged += messages.Add;
            await new StcQuoteParserClient(hostTransport, "test-parser").ParseAsync("LOG-CHECK 原始文本", CancellationToken.None);
            await Application.Current.Dispatcher.InvokeAsync(() => { });
            Check(messages.Any(message => message.Contains("REQ stc.parser.bond") && message.Contains("LOG-CHECK"))
                && messages.Any(message => message.Contains("REP stc.parser.bond") && message.Contains("Datas")),
                "real host transport emits complete parser request and response logs");
        }
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

    private static async Task CheckRenderedSelectionAsync(PreciseQuoteViewModel viewModel, ParserQuoteService quoteService)
    {
        PreciseQuoteView view = new PreciseQuoteView { DataContext = viewModel };
        Window window = new Window
        {
            Content = view, Width = 1200, Height = 800,
            ShowActivated = false, ShowInTaskbar = false, Left = -20000, Top = -20000
        };
        try
        {
            window.Show();
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            window.UpdateLayout();
            DataGrid grid = Descendants<DataGrid>(view).Single();
            QuoteConfirmRowViewModel row = viewModel.Rows[0];
            grid.ScrollIntoView(row);
            grid.UpdateLayout();
            CheckBox checkbox = Descendants<CheckBox>(grid).First(control => ReferenceEquals(control.DataContext, row));
            MethodInfo click = typeof(ToggleButton).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
            click.Invoke(checkbox, null);
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
            Check(checkbox.IsChecked == true && row.IsSelected && viewModel.SelectedCount == 1,
                "rendered checkbox immediately updates selected row and count without leaving the row");
            click.Invoke(checkbox, null);
            Check(checkbox.IsChecked == false && !row.IsSelected && viewModel.SelectedCount == 0,
                "rendered checkbox immediately clears selection");
            click.Invoke(checkbox, null);
            Button confirm = Descendants<Button>(view).First(button => ReferenceEquals(button.Command, viewModel.ConfirmSelectedCommand));
            typeof(Button).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(confirm, null);
            Task execution = viewModel.ConfirmSelectedCommand.ExecutionTask;
            if (execution != null)
            {
                await execution;
            }
            Check(quoteService.SubmittedRows.Count == 1 && ReferenceEquals(quoteService.SubmittedRows[0], row.Row),
                "rendered confirm button submits exactly the checked quote");
            Check(viewModel.Rows.Count == 1 && viewModel.SelectedCount == 0 && !viewModel.Rows.Contains(row),
                "confirmation removes submitted row and retains unchecked row");
        }
        finally
        {
            window.Close();
        }
    }

    private static async Task CheckSubmissionRetryAsync()
    {
        FakeTransport transport = new FakeTransport();
        QuoteConfirmRow row = new QuoteConfirmRow
        {
            Code = "240210.IB", Price = 2.5m, Volume = 1000,
            PriceType = PriceType.YieldToMaturity, Settlement = SettlementType.T1
        };
        int creates = 0;
        int confirms = 0;
        string status = "PENDING_CONFIRM";
        transport.Responder = (topic, request) =>
        {
            JObject data;
            if (topic == "ficc.quote.create")
            {
                creates++;
                data = new JObject { ["results"] = new JArray(new JObject
                {
                    ["clientQuoteId"] = row.ClientQuoteId, ["quoteId"] = "Q-test",
                    ["Code"] = "0000", ["revision"] = 1
                }) };
            }
            else if (topic == "ficc.quote.get")
            {
                data = new JObject { ["quote"] = new JObject
                {
                    ["quoteId"] = "Q-test", ["revision"] = 1, ["status"] = status
                } };
            }
            else if (topic == "ficc.quote.confirm")
            {
                confirms++;
                if (confirms == 2)
                {
                    Check((string)request["quotes"][0]["price"] == "2.6", "retry confirms edited fields");
                    status = "ACTIVE";
                }
                data = new JObject { ["results"] = new JArray(new JObject
                {
                    ["quoteId"] = "Q-test", ["Code"] = confirms == 1 ? "1001" : "0000",
                    ["Msg"] = "confirmation fixture"
                }) };
            }
            else
            {
                throw new InvalidOperationException("Unexpected submission topic: " + topic);
            }
            return new JObject { ["Code"] = "0000", ["requestId"] = request["requestId"], ["Data"] = data };
        };
        ParsedQuoteSubmitter submitter = new ParsedQuoteSubmitter(new FiccApiClient(transport));
        try
        {
            await submitter.SubmitAsync(new[] { row }, CancellationToken.None);
            throw new Exception("Confirmation failure hidden");
        }
        catch (InvalidOperationException)
        {
            Check(row.RecognitionId == "Q-test" && row.Revision == 1, "confirmation failure retains created quote identity");
        }
        row.Price = 2.6m;
        await submitter.SubmitAsync(new[] { row }, CancellationToken.None);
        await submitter.SubmitAsync(new[] { row }, CancellationToken.None);
        Check(creates == 1 && confirms == 2, "retry reuses pending quote and skips already confirmed row");
    }

    private sealed class HostApiProxy : RealProxy
    {
        public HostApiProxy() : base(typeof(netapi.Api.IApi)) { }

        public override IMessage Invoke(IMessage message)
        {
            IMethodCallMessage call = (IMethodCallMessage)message;
            object result = null;
            if (call.MethodName == "Request")
            {
                if ((string)call.Args[0] != "ParserSvr" || (string)call.Args[1] != "stc.parser.bond")
                {
                    throw new InvalidOperationException("Unexpected service call in recognition.");
                }
                result = Reply();
            }
            else if (call.MethodName != "SubScribeConStatus" && call.MethodName != "UnSubScribeConStatus")
            {
                throw new InvalidOperationException("Unexpected host API call: " + call.MethodName);
            }
            return new ReturnMessage(result, null, 0, call.LogicalCallContext, call);
        }
    }

    private sealed class FakeTransport : IFiccTransport
    {
        public JObject Response;
        public JObject Request;
        public string Service;
        public string Topic;
        public int Calls;
        public Task<JObject> Pending;
        public Func<string, JObject, JObject> Responder;
        public event Action ConnectionChanged { add { } remove { } }

        public Task<JObject> RequestAsync(string service, string topic, JObject request)
        {
            Service = service;
            Topic = topic;
            Request = request;
            Calls++;
            return Pending ?? Task.FromResult(Responder == null
                ? (JObject)Response.DeepClone() : Responder(topic, request));
        }

        public IDisposable Subscribe(string service, string topic, Action<JObject> receive, bool withImage = false)
        {
            throw new InvalidOperationException("Recognition must not subscribe.");
        }
    }

    private sealed class ParserQuoteService : IQuoteService
    {
        private readonly StcQuoteParserClient parser;
        public List<QuoteConfirmRow> SubmittedRows = new List<QuoteConfirmRow>();
        public ParserQuoteService(StcQuoteParserClient parser) { this.parser = parser; }
        public Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken ct) { return parser.ParseAsync(text, ct); }
        public Task<IReadOnlyList<QuoteConfirmRow>> LoadPendingRowsAsync(CancellationToken ct) { throw new NotSupportedException(); }
        public Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken ct) { throw new NotSupportedException(); }
        public Task ConfirmQuotesAsync(IReadOnlyCollection<string> ids, CancellationToken ct) { throw new NotSupportedException(); }
        public Task SubmitQuotesAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken ct)
        {
            SubmittedRows.AddRange(rows);
            return Task.CompletedTask;
        }
        public Task DeleteQuotesAsync(IReadOnlyCollection<string> ids, CancellationToken ct) { throw new NotSupportedException(); }
        public Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken ct) { throw new NotSupportedException(); }
    }
}
