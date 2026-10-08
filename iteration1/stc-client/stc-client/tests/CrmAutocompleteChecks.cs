using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using netapi.Api;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Ficc;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.View;
using STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel;
using STC.SalesTrading.Plugin.Shared.Components;

internal static class CrmAutocompleteChecks
{
    private static int passed;
    private static readonly CrmChoice alice = new CrmChoice
    {
        InstitutionId = "I-A", InstitutionName = "测试机构甲", ContactId = "C-A", ContactName = "同名交易员"
    };
    private static readonly CrmChoice bob = new CrmChoice
    {
        InstitutionId = "I-B", InstitutionName = "测试机构乙", ContactId = "C-B", ContactName = "同名交易员"
    };

    [STAThread]
    public static int Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        int code = 1;
        app.Dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                if (args.Contains("--live")) await Live();
                else await Fixtures();
                Console.WriteLine("RESULT PASS " + passed + " checks; mode=" + (args.Contains("--live") ? "live read-only CRM" : "fixture WPF"));
                code = 0;
            }
            catch (Exception error) { Console.Error.WriteLine(error); }
            finally { app.Shutdown(); }
        }));
        app.Run();
        return code;
    }

    private static void Check(bool valid, string description)
    {
        if (!valid) throw new InvalidOperationException("FAIL " + description);
        Console.WriteLine("PASS " + description);
        passed++;
    }

    private static QuoteConfirmRow Row(string id) => new QuoteConfirmRow
    {
        RecognitionId = id, BatchId = "B", SourceType = "QT", Code = "240210.IB", Name = "24国开10",
        Price = 2, Volume = 1000, Direction = QuoteDirection.Bid, PriceType = PriceType.YieldToMaturity
    };

    private static async Task Fixtures()
    {
        var row = new QuoteConfirmRowViewModel(Row("row"));
        row.Trader = "同名";
        Check(row.InstitutionId == null && row.Row.ContactId == null, "free trader text does not invent CRM IDs");
        row.SelectContact(alice);
        row.Commit();
        Check(row.Institution == alice.InstitutionName && row.Row.ContactId == "C-A" && row.Row.InstitutionId == "I-A",
            "contact selection fills institution and IDs survive Commit");
        row.SelectInstitution(bob);
        Check(row.Row.ContactId == null && row.Trader == "", "changing institution clears incompatible trader");
        row.SelectContact(bob);
        row.Institution = "自由输入";
        Check(row.InstitutionId == null && row.Row.ContactId == null && row.Trader == "", "editing institution clears stale identities immediately");

        var service = new FixtureService();
        using (var precise = new PreciseQuoteViewModel(service))
        {
            precise.Rows.Add(new QuoteConfirmRowViewModel(Row("selected")) { IsSelected = true });
            precise.Rows.Add(new QuoteConfirmRowViewModel(Row("untouched")));
            await Page(new PreciseQuoteView { DataContext = precise }, precise.Rows[0], precise.CrmBatch, service, "精确报价");
            Check(precise.Rows[1].Row.ContactId == null, "precise toolbar leaves unchecked row unchanged");
        }
        using (var confirm = new QuoteConfirmViewModel(service))
        {
            await Task.Delay(100);
            var selected = confirm.Batches.SelectMany(batch => batch.Rows).First();
            selected.IsSelected = true;
            await Page(new QuoteConfirmView { DataContext = confirm }, selected, confirm.CrmBatch, service, "报价确认");
            Check(confirm.Batches.SelectMany(batch => batch.Rows).Skip(1).All(item => item.Row.ContactId == null),
                "confirmation toolbar leaves unchecked rows unchanged");
        }
    }

    private static async Task Page(UserControl page, QuoteConfirmRowViewModel row, CrmBatchEditor batch,
        FixtureService directory, string label)
    {
        var window = new Window { Content = page, Width = 1900, Height = 900, Title = "CRM regression — " + label };
        window.Show();
        window.Activate();
        await Task.Delay(150);
        page.UpdateLayout();
        var controls = Descendants<CrmLookup>(page).ToList();
        Check(controls.Count >= 4, label + " renders CRM row and toolbar controls");
        var contact = controls.First(control => control.DataContext == row && control.Kind == "CONTACT");
        Check(ReferenceEquals(CrmLookup.GetDirectory(contact), directory), label + " row inherits CRM directory");
        await Search(contact, "同名");
        Check(directory.LastInstitution == null && Items(contact).Items.Count == 2, label + " empty institution searches all traders");
        Check(((CrmChoice)Items(contact).Items[0]).DisplayName.Contains("测试机构甲"), label + " dropdown disambiguates by institution");
        Key(contact, System.Windows.Input.Key.Down);
        Key(contact, System.Windows.Input.Key.Enter);
        await Task.Delay(30);
        row.Commit();
        Check(row.Row.ContactId == "C-A" && row.Row.InstitutionId == "I-A", label + " keyboard selection preserves both IDs");

        await Search(contact, "同");
        Check(directory.LastInstitution == "I-A" && Items(contact).Items.Count == 1, label + " selected institution scopes contact search");
        var institution = controls.First(control => control.DataContext == row && control.Kind == "INSTITUTION");
        await Search(institution, "乙");
        Key(institution, System.Windows.Input.Key.Down);
        Key(institution, System.Windows.Input.Key.Enter);
        Check(row.InstitutionId == "I-B" && row.Row.ContactId == null && row.Trader == "", label + " institution selection clears old trader");

        var toolbar = controls.First(control => !(control.DataContext is QuoteConfirmRowViewModel) && control.Kind == "CONTACT");
        await Search(toolbar, "同名");
        Key(toolbar, System.Windows.Input.Key.Down);
        Key(toolbar, System.Windows.Input.Key.Enter);
        row.Commit();
        Check(row.Row.ContactId == "C-A" && batch.InstitutionId == "I-A", label + " toolbar selects CRM identity into checked rows");

        directory.Fail = true;
        await Search(contact, "失败");
        Check(Popup(contact).IsOpen && Status(contact).Text.Contains("查询失败"), label + " CRM failure is visible");
        directory.Fail = false;
        await Search(contact, "同名");
        Check(Items(contact).Items.Count == 1, label + " next query recovers without reopening page");

        directory.Pending = new TaskCompletionSource<IReadOnlyList<CrmChoice>>();
        await Search(contact, "慢请求");
        var pending = directory.Pending;
        directory.Pending = null;
        contact.Text = "不存在";
        await Task.Delay(400);
        pending.SetResult(new[] { bob });
        await Task.Delay(50);
        Check(Items(contact).Items.Count == 0 && Status(contact).Text.Contains("没有匹配"), label + " stale search cannot replace newer results");
        window.Close();
    }

    private static async Task Search(CrmLookup lookup, string text)
    {
        lookup.Focus();
        Keyboard.Focus(lookup);
        lookup.Text = text;
        lookup.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
        await Task.Delay(420);
    }

    private static void Key(CrmLookup lookup, Key key)
    {
        lookup.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(lookup), 0, key)
        { RoutedEvent = Keyboard.PreviewKeyDownEvent });
    }

    private static T Field<T>(CrmLookup lookup, string name) =>
        (T)typeof(CrmLookup).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(lookup);
    private static ListBox Items(CrmLookup lookup) => Field<ListBox>(lookup, "suggestions");
    private static Popup Popup(CrmLookup lookup) => Field<Popup>(lookup, "popup");
    private static TextBlock Status(CrmLookup lookup) => Field<TextBlock>(lookup, "status");

    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) yield return match;
            foreach (var descendant in Descendants<T>(child)) yield return descendant;
        }
    }

    private static async Task Live()
    {
        var api = new GWApi();
        api.NewApi();
        api.SetGzip(true);
        try
        {
            string user = Environment.GetEnvironmentVariable("FICC_TEST_USER");
            string response = await Task.Run(() => ((com.gateway.connector.tcp.GateWayApi)api.GetApiConnect())
                .connect("127.0.0.1", 28600, user, Environment.GetEnvironmentVariable("FICC_TEST_PASSWORD"),
                    false, 1, new Dictionary<string, object> { ["isManage"] = "N" }));
            var login = JObject.Parse(response);
            Check((string)login["Code"] == "0001", "live local STC login");
            api.UserName = user;
            api.Sid = (string)login["sid"];
            ApiManager.Instance.AddApi(api);
            ApiManager.Instance.SetDefaultApi(api.ApiKey);
            using (var service = new DpsSalesTradingServices())
            {
                var institutions = await service.SearchAsync("INSTITUTION", "长城", null);
                Check(institutions.Count > 0 && institutions.All(item => !string.IsNullOrWhiteSpace(item.InstitutionId)), "live institution search returns IDs");
                var contacts = await service.SearchAsync("CONTACT", "王", null);
                Check(contacts.Count > 0, "live global contact search");
                var choice = contacts.First(item => !string.IsNullOrEmpty(item.InstitutionId));
                var row = new QuoteConfirmRowViewModel(Row("live-preview-only"));
                var lookup = new CrmLookup
                {
                    Kind = "CONTACT", Width = 400, SelectionCommand = row.SelectContactCommand
                };
                CrmLookup.SetDirectory(lookup, service);
                var window = new Window { Content = lookup, Width = 650, Height = 240, Title = "Live CRM read-only test" };
                window.Show();
                window.Activate();
                await Search(lookup, choice.ContactName);
                await Task.Delay(1000);
                Check(Popup(lookup).IsOpen && Items(lookup).Items.Count > 0, "live CRM results appear in WPF dropdown");
                Key(lookup, System.Windows.Input.Key.Down);
                Key(lookup, System.Windows.Input.Key.Enter);
                row.Commit();
                Check(!string.IsNullOrEmpty(row.Row.ContactId) && !string.IsNullOrEmpty(row.Row.InstitutionId), "live contact selection fills IDs in preview; no quote submitted");
                window.Close();
            }
        }
        finally
        {
            api.DisConnect();
        }
    }

    private sealed class FixtureService : IQuoteService, ICrmDirectory
    {
        public string LastInstitution;
        public bool Fail;
        public TaskCompletionSource<IReadOnlyList<CrmChoice>> Pending;
        public Task<IReadOnlyList<CrmChoice>> SearchAsync(string kind, string text, string institutionId)
        {
            LastInstitution = institutionId;
            if (Fail) throw new InvalidOperationException("CRM OFFLINE fixture");
            if (Pending != null) return Pending.Task;
            IEnumerable<CrmChoice> choices = new[] { alice, bob };
            if (kind == "INSTITUTION") choices = choices.Where(item => item.InstitutionName.Contains(text));
            else choices = choices.Where(item => item.ContactName.Contains(text) && (institutionId == null || item.InstitutionId == institutionId));
            return Task.FromResult<IReadOnlyList<CrmChoice>>(choices.ToList());
        }
        public Task<IReadOnlyList<QuoteConfirmRow>> LoadPendingRowsAsync(CancellationToken ct) => Task.FromResult<IReadOnlyList<QuoteConfirmRow>>(new[] { Row("one"), Row("two") });
        public Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken ct) => throw new NotSupportedException();
        public Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken ct) => throw new NotSupportedException();
        public Task ConfirmQuotesAsync(IReadOnlyCollection<string> ids, CancellationToken ct) => throw new NotSupportedException();
        public Task SubmitQuotesAsync(IReadOnlyCollection<QuoteConfirmRow> rows, CancellationToken ct) => throw new NotSupportedException();
        public Task DeleteQuotesAsync(IReadOnlyCollection<string> ids, CancellationToken ct) => throw new NotSupportedException();
        public Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken ct) => throw new NotSupportedException();
    }
}
