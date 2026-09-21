using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ficc.SalesTrading;

namespace Ficc.FrontendLearning
{
    public sealed class LearningForm : Form
    {
        private readonly DpsClient client = new DpsClient();
        private readonly QuoteExamples api;
        private readonly SubscribedPage page;
        private readonly TextBox host = Input("127.0.0.1");
        private readonly TextBox port = Input("18600", 65);
        private readonly TextBox user = Input("", 95);
        private readonly TextBox password = Input("", 95);
        private readonly TextBox security = Input("250001.IB");
        private readonly TextBox price = Input("2.5000", 75);
        private readonly TextBox quoteId = Input("", 260);
        private readonly TextBox search = Input("", 100);
        private readonly TextBox institutionId = Input("", 160);
        private readonly TextBox contactId = Input("", 160);
        private readonly TextBox notificationId = Input("", 260);
        private readonly TextBox log = new TextBox();
        private readonly DataGridView grid = new DataGridView();
        private readonly Label status = new Label { AutoSize = true };
        private readonly ComboBox filterExample = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 150
        };
        private JObject displayedQuote;
        private bool busy;
        private bool connected;
        private JObject customFilter;
        private long logEntryNumber;

        [STAThread]
        public static void Main(string[] args)
        {
            if (args.Contains("--filter-picker-check"))
            {
                Application.EnableVisualStyles();
                Environment.ExitCode = FilterPickerCheck.Run();
                return;
            }
            if (args.Contains("--check"))
            {
                Environment.ExitCode = ConnectionCheck.Run();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using (var form = new LearningForm())
            using (var closeTimer = new Timer { Interval = 1000 })
            {
                // 窗体启动检查：不连接服务、不写业务数据，1秒后正常关闭。
                if (args.Contains("--ui-check"))
                {
                    closeTimer.Tick += (sender, eventArgs) => form.Close();
                    form.Shown += (sender, eventArgs) => closeTimer.Start();
                }
                Application.Run(form);
            }
        }

        public LearningForm()
        {
            Text = "FICC C# 前端开发学习示例";
            Width = 1200;
            Height = 820;
            api = new QuoteExamples(client);
            page = new SubscribedPage(client, api);
            password.UseSystemPasswordChar = true;
            client.Trace += text => OnUi(() => WriteLog(text));
            client.Message += (topic, message) => OnUi(() => page.Receive(topic, message));
            page.Changed += RenderPage;
            page.Failed += WriteLog;

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 8 };
            for (int index = 0; index < 6; index++) layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            Controls.Add(layout);
            var login = Bar(layout);
            Add(login, "GW", host, "端口", port, "账号", user, "密码", password);
            Button(login, "1. 登录", async () =>
            {
                if (connected) throw new InvalidOperationException("已登录。切换账号请关闭并重启示例。");
                string loginUser = user.Text;
                string loginPassword = password.Text;
                string loginHost = host.Text;
                int loginPort = int.Parse(port.Text);
                await Task.Run(() => client.Connect(loginUser, loginPort, loginHost, loginPassword));
                connected = true;
                password.Clear();
                WriteLog("登录成功。完整消息日志：" + client.LogDirectory);
            }, false);

            var quotes = Bar(layout);
            Add(quotes, "债券", security, "收益率", price, "报价ID", quoteId);
            Button(quotes, "2. 创建待确认报价", async () =>
            {
                var result = QuoteExamples.RequireSuccess(await api.Create(security.Text, price.Text));
                quoteId.Text = (string)result["quoteId"];
                await ReadQuote();
            });
            Button(quotes, "读取报价/版本", ReadQuote);

            var actions = Bar(layout);
            foreach (var item in new[] { "confirm:确认", "update:修改收益率", "refer:撤销", "unrefer:恢复", "delete:删除" })
            {
                var parts = item.Split(':');
                Button(actions, parts[1], () => OperateQuote(parts[0]));
            }
            Button(actions, "变更历史", async () => WriteLog((await api.History(quoteId.Text)).ToString()));
            Add(actions, "搜索名称", search, "机构ID", institutionId, "联系人ID", contactId);
            Button(actions, "搜索机构", async () => WriteLog((await api.SearchInstitutions(search.Text)).ToString()));
            Button(actions, "搜索联系人", async () => WriteLog((await api.SearchContacts(institutionId.Text, search.Text)).ToString()));

            var views = Bar(layout);
            Button(views, "3. 报价视图", () => OpenFilteredView("quote"));
            Button(views, "行情视图", () => OpenFilteredView("price"));
            Button(views, "提醒视图", () => OpenFilteredView("notification"));
            Button(views, "上一页", () => page.QueryPage(Math.Max(1, page.PageNumber - 1)));
            Button(views, "下一页", () => page.QueryPage(page.PageNumber + 1));
            Button(views, "回到第一页", () => page.QueryPage(1));
            views.Controls.Add(status);

            var filters = Bar(layout);
            filterExample.Items.AddRange(FilterExamples.Names);
            filterExample.SelectedIndex = 0;
            filterExample.SelectedIndexChanged += (sender, args) => customFilter = null;
            Add(filters, "筛选样例", filterExample);
            Button(filters, "查看条件 JSON", () =>
            {
                WriteLog(CurrentFilter().ToString());
                return Task.CompletedTask;
            }, false);
            Button(filters, "应用到当前视图", async () =>
            {
                if (page.Topic == null) throw new InvalidOperationException("请先打开报价、行情或提醒视图。");
                await OpenFilteredView(page.Domain);
            });
            Button(filters, "读取字段字典", async () => WriteLog((await api.Request(
                "ficc.quote.dictionary.query", new JObject()))["filterFields"].ToString()));
            Button(filters, "全部产品条件", async () =>
            {
                var dictionary = await api.Request("ficc.quote.dictionary.query", new JObject());
                using (var editor = new ProductFilterForm((JArray)dictionary["filterFields"],
                    customFilter ?? FilterExamples.Build((string)filterExample.SelectedItem), (JObject)dictionary["dictionaries"]))
                {
                    if (editor.ShowDialog(this) != DialogResult.OK) return;
                    customFilter = editor.Filter;
                }
                WriteLog("已保存自定义条件；点击应用到当前视图或打开业务视图。\n" + CurrentFilter());
            });
            Add(filters, "债券代码留空表示跨债券筛选；外部属性当前可能为模拟值。");

            var notifications = Bar(layout);
            Add(notifications, "提醒ID", notificationId);
            Button(notifications, "标为已读", async () => WriteLog((await api.MarkRead(notificationId.Text)).ToString()));
            Button(notifications, "读取提醒偏好", async () => WriteLog((await api.Request(
                "ficc.notification.preference.get", new JObject())).ToString()));
            var heartbeat = new CheckBox { Text = "打印心跳", AutoSize = true };
            heartbeat.CheckedChanged += (sender, args) => client.LogHeartbeats = heartbeat.Checked;
            notifications.Controls.Add(heartbeat);
            var clearLog = new Button { Text = "清空日志", AutoSize = true };
            clearLog.Click += (sender, args) => log.Clear();
            notifications.Controls.Add(clearLog);

            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Columns.Add("id", "记录ID");
            grid.Columns.Add("security", "债券");
            grid.Columns.Add("status", "状态/类型");
            grid.Columns.Add("time", "更新时间");
            grid.CellDoubleClick += async (sender, args) =>
            {
                if (args.RowIndex < 0) return;
                var row = (JObject)grid.Rows[args.RowIndex].Tag;
                WriteLog(row.ToString());
                if (row["notificationId"] != null) notificationId.Text = (string)row["notificationId"];
                if (row["quoteId"] != null)
                {
                    quoteId.Text = (string)row["quoteId"];
                    await Run(ReadQuote);
                }
            };
            layout.Controls.Add(grid);
            log.Dock = DockStyle.Fill;
            log.Multiline = true;
            log.ReadOnly = true;
            log.ScrollBars = ScrollBars.Both;
            log.WordWrap = false;
            log.Font = new Font("Consolas", 10);
            layout.Controls.Add(log);
            WriteLog("先登录，再打开视图。写操作仅在点击按钮时执行；默认无自动创建/批量测试。双击一行看完整模型。");
            FormClosed += (sender, args) =>
            {
                client.Dispose();
                MessageLog.Close();
            };
        }

        private JObject CurrentFilter()
        {
            if (customFilter == null)
            {
                return FilterExamples.Build((string)filterExample.SelectedItem, security.Text.Trim());
            }
            var filter = (JObject)customFilter.DeepClone();
            if (!string.IsNullOrWhiteSpace(security.Text))
            {
                var conditions = (JArray)filter["conditions"];
                foreach (var condition in conditions.Where(item => (string)item["field"] == "securityId").ToList())
                {
                    condition.Remove();
                }
                conditions.Add(FilterExamples.Condition("securityId", "EQ", security.Text.Trim()));
            }
            return filter;
        }

        private Task OpenFilteredView(string domain)
        {
            // 条件改变时重新打开视图，回到第一页；翻页只复用 viewId。
            return page.Open(domain, CurrentFilter());
        }

        private async Task ReadQuote()
        {
            displayedQuote = await api.Get(quoteId.Text);
            price.Text = (string)displayedQuote["price"];
            WriteLog(displayedQuote.ToString());
        }

        private async Task OperateQuote(string operation)
        {
            if ((string)displayedQuote?["quoteId"] != quoteId.Text)
                throw new InvalidOperationException("报价ID已改变，请先读取报价。");
            var fields = new JObject();
            if (operation == "update") fields["price"] = price.Text;
            if (operation == "confirm")
            {
                if (!string.IsNullOrWhiteSpace(institutionId.Text)) fields["institutionId"] = institutionId.Text;
                if (!string.IsNullOrWhiteSpace(contactId.Text)) fields["contactId"] = contactId.Text;
            }
            QuoteExamples.RequireSuccess(await api.Operate(operation, displayedQuote, fields));
            await ReadQuote(); // 回包确认操作结果；列表仍由订阅推送驱动。
        }

        private void RenderPage()
        {
            grid.Rows.Clear(); // 教学示例上限20行，不是大量数据渲染方案。
            foreach (var row in page.Rows)
            {
                int index = grid.Rows.Add((string)(row["quoteId"] ?? row["notificationId"] ?? row["securityId"]),
                    (string)row["securityId"], (string)row["status"] ?? (row["marketData"] != null ? "价格簿" : "提醒"),
                    (string)row["updateTime"]);
                grid.Rows[index].Tag = row;
            }
            status.Text = "第 " + page.PageNumber + " 页，共 " + page.TotalCount + " 条";
        }

        private async Task Run(Func<Task> action, bool needsLogin = true)
        {
            if (busy) return;
            busy = true;
            try
            {
                if (needsLogin && !connected) throw new InvalidOperationException("请先登录。");
                await action();
            }
            catch (Exception error) { WriteLog("ERROR " + error); }
            finally { busy = false; }
        }

        private void Button(FlowLayoutPanel panel, string text, Func<Task> action, bool needsLogin = true)
        {
            var button = new Button { Text = text, AutoSize = true };
            button.Click += async (sender, args) => await Run(action, needsLogin);
            panel.Controls.Add(button);
        }

        private void OnUi(Action action)
        {
            if (!IsDisposed && IsHandleCreated) BeginInvoke(action);
        }

        private void WriteLog(string text)
        {
            if (log.TextLength > 100000)
            {
                log.Clear();
            }
            string heading = text ?? "";
            string body = "";
            int objectStart = heading.IndexOf('{');
            int arrayStart = heading.IndexOf('[');
            int jsonStart = objectStart < 0 ? arrayStart : arrayStart < 0 ? objectStart : Math.Min(objectStart, arrayStart);
            if (jsonStart >= 0)
            {
                try
                {
                    using (var reader = new JsonTextReader(new StringReader(heading.Substring(jsonStart))))
                    {
                        reader.DateParseHandling = DateParseHandling.None;
                        body = JToken.ReadFrom(reader).ToString(Formatting.Indented);
                        if (reader.Read()) throw new JsonReaderException("Text follows the JSON payload");
                    }
                    heading = heading.Substring(0, jsonStart).Trim();
                    if (heading.Length == 0) heading = "DATA";
                }
                catch (JsonReaderException)
                {
                    // 普通文字/异常堆栈可能含括号；保留原文，不因显示格式化丢日志。
                    body = "";
                }
            }
            logEntryNumber++;
            log.AppendText(new string('─', 90) + Environment.NewLine
                + "[" + logEntryNumber.ToString("D6") + "] " + DateTime.Now.ToString("HH:mm:ss.fff")
                + "  " + heading + Environment.NewLine
                + (body.Length == 0 ? "" : body + Environment.NewLine)
                + Environment.NewLine);
        }

        private static TextBox Input(string text, int width = 125)
        {
            return new TextBox { Text = text, Width = width };
        }

        private static FlowLayoutPanel Bar(TableLayoutPanel parent)
        {
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, WrapContents = true };
            parent.Controls.Add(panel);
            return panel;
        }

        private static void Add(FlowLayoutPanel panel, params object[] controls)
        {
            foreach (var value in controls)
                panel.Controls.Add(value as Control ?? new Label { Text = (string)value, AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        }
    }
}
