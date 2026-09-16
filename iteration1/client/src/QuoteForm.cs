using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    // Bounded virtual grid; performance mode measures the same view path.
    public sealed partial class QuoteForm : Form {
        readonly DpsClient client=new DpsClient();
        readonly List<JObject> rows=new List<JObject>();
        readonly Dictionary<string,JObject> cache=new Dictionary<string,JObject>();
        readonly List<string> ids=new List<string>();
        readonly List<Tuple<string,JObject>> pending=new List<Tuple<string,JObject>>();
        readonly QuoteGrid grid = new QuoteGrid();
        readonly ComboBox user=Choice("trader1","trader2","admin");
        readonly TextBox gatewayHost = new TextBox { Text = "127.0.0.1", Width = 110 };
        readonly NumericUpDown gatewayPort = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = 18600, Width = 70 };
        readonly TextBox loginPassword = new TextBox { Text = "trader1", Width = 90, UseSystemPasswordChar = true };
        readonly ComboBox security=Choice("250001.IB","250002.IB","102500001.IB","102500002.IB");
        readonly ComboBox side=Choice("1","2"), settlement=Choice("1","2");
        readonly ComboBox statusFilter=Choice("全部我的报价","待确认","有效报价","已撤出","已删除");
        readonly ComboBox rating=Choice("全部评级","AAA","AA+");
        readonly TextBox price=new TextBox{Text="2.5000",Width=85};
        readonly TextBox quantity=new TextBox{Text="1000",Width=80};
        readonly CounterpartyEditor counterpartyEditor=new CounterpartyEditor();
        readonly TextBox log=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill};
        readonly CheckBox logHeartbeats = new CheckBox { Text = "记录 HEARTBEAT", AutoSize = true };
        readonly TextBox detail=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Both,Dock=DockStyle.Fill,WordWrap=false};
        readonly Label state=new Label{Text="尚未连接 · 经本机 GW 18600",AutoSize=true,Padding=new Padding(6)};
        readonly List<Button> actions=new List<Button>();
        readonly TabControl tabs=new TabControl{Dock=DockStyle.Fill};
        readonly TabControl boards=new TabControl{Dock=DockStyle.Fill};
        readonly SplitContainer workspace=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,Panel2Collapsed=true};
        readonly Button diagnosticToggle=new Button{Text="操作结果 / 开发工具",AutoSize=true};
        string viewId,streamId,topic;long sequence;bool connected,busy,opening,automatedUi;
        int lastFilterTotalCount;
        readonly string[] fields={"quoteId","securityId","side","price","quantity","settlType","status","revision","updateTime"};
        public QuoteForm(){
            Text="FICC 报价生命周期平台 · GW / DPS";Width=1180;Height=760;MinimumSize=new Size(900,600);
            Font=new Font("Microsoft YaHei UI",9F);StartPosition=FormStartPosition.CenterScreen;
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,ColumnCount=1,Padding=new Padding(4)};
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,28));
            Controls.Add(layout);
            user.DropDownStyle = ComboBoxStyle.DropDown;
            var connectBar = Bar();
            connectBar.Controls.AddRange(new Control[] {
                Label("GW"), gatewayHost, gatewayPort, Label("账户"), user, Label("密码"), loginPassword
            });
            AddButton(connectBar, "连接 GW", Connect);
            connectBar.Controls.Add(diagnosticToggle);diagnosticToggle.Click+=(s,e)=>SetDiagnosticsVisible(workspace.Panel2Collapsed);
            layout.Controls.Add(connectBar,0,0);layout.Controls.Add(workspace,0,1);state.Dock=DockStyle.Fill;layout.Controls.Add(state,0,2);
            workspace.Panel1.Controls.Add(boards);workspace.Panel2.Controls.Add(tabs);
            var quoteLayout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=5,ColumnCount=1};
            foreach(int height in new[]{40,40,40,40})quoteLayout.RowStyles.Add(new RowStyle(SizeType.Absolute,height));quoteLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            var filterBar=Bar();filterBar.Controls.AddRange(new Control[]{Label("我的报价"),statusFilter,rating});AddButton(filterBar,"筛选",OpenView);AddButton(filterBar,"历史",History);AddButton(filterBar,"该债行情",PriceBook);quoteLayout.Controls.Add(filterBar,0,0);
            var editor=Bar();editor.Controls.AddRange(new Control[]{Label("债券"),security,Label("方向 1买/2卖"),side,Label("收益率(%)"),price,Label("数量(万)"),quantity,Label("交割 1当日/2次日"),settlement});quoteLayout.Controls.Add(editor,0,1);
            quoteLayout.Controls.Add(counterpartyEditor,0,2);
            var commandBar=Bar();foreach(var pair in new[]{new[]{"新建","create"},new[]{"确认","confirm"},new[]{"修改","update"},new[]{"撤出","refer"},new[]{"恢复","unrefer"},new[]{"删除","delete"},new[]{"克隆","clone"}}){string op=pair[1];AddButton(commandBar,pair[0],()=>Command(op));}
            AddButton(commandBar,"批量新建(2条)",()=>Create(2));quoteLayout.Controls.Add(commandBar,0,3);
            grid.Dock=DockStyle.Fill;grid.VirtualMode=true;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;grid.ReadOnly=true;grid.RowHeadersVisible=false;grid.MultiSelect=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.None;
            string[] labels={"报价ID","债券代码","方向","收益率(%)","数量(万)","交割","状态","版本","更新时间"};
            for(int i=0;i<fields.Length;i++)grid.Columns.Add(new DataGridViewTextBoxColumn{Name=fields[i],HeaderText=labels[i],Width=i==0?165:i==8?200:100,SortMode=DataGridViewColumnSortMode.NotSortable});
            grid.CellValueNeeded+=(s,e)=>{if(e.RowIndex<rows.Count)e.Value=(string)rows[e.RowIndex][fields[e.ColumnIndex]];};
            grid.CurrentCellChanged+=(s,e)=>{var q=Selected();if(q!=null){security.Text=(string)q["securityId"];side.Text=(string)q["side"];price.Text=(string)q["price"];quantity.Text=(string)q["quantity"];settlement.Text=(string)q["settlType"];counterpartyEditor.LoadFields(q);}};
            InitializeQuoteScrolling(filterBar);
            grid.Columns["quoteId"].DisplayIndex=8;grid.Columns["revision"].DisplayIndex=7;
            quoteLayout.Controls.Add(grid,0,4);
            var quoteBoard=new TabPage("报价管理");quoteBoard.Controls.Add(quoteLayout);boards.TabPages.Add(quoteBoard);InitializePriceBoard();InitializeNotificationBoard();
            var details=new TabPage("操作结果 / 历史 / 行情");details.Controls.Add(detail);
            var messages = new TabPage("DPS 消息（最近 200 条；完整记录见文件）");
            var messageOptions = Bar();
            messageOptions.Dock = DockStyle.Top;
            messageOptions.Height = 36;
            messageOptions.Controls.Add(logHeartbeats);
            messageOptions.Controls.Add(new TextBox { ReadOnly = true, Width = 650, Text = client.LogDirectory });
            logHeartbeats.CheckedChanged += (sender, args) => client.LogHeartbeats = logHeartbeats.Checked;
            messages.Controls.Add(log);
            messages.Controls.Add(messageOptions);
            tabs.TabPages.AddRange(new[]{details,messages});InitializeAutoTestTab();
            var toolsBar=Bar();toolsBar.Dock=DockStyle.Top;toolsBar.Height=40;
            AddButton(toolsBar, "自动测试", RunSelectedAutoTests);
            AddButton(toolsBar, "多用户场景（本机模拟）", async () => {
                if (testMode.SelectedIndex != 1) {
                    throw new InvalidOperationException("QA 多用户检查已包含在自动测试中；该按钮仅用于本机模拟回归。");
                }
                await RunMultiUserScenario();
            });
            AddButton(toolsBar, "API 调试器", OpenApiDebugger);
            AddButton(toolsBar, "筛选性能测试(500条)", OpenFilterPerformanceTest);
            AddButton(toolsBar, "滚动性能测试(500条)", OpenScrollingPerformanceTest);
            testTab.Controls.Add(toolsBar);
            detail.TextChanged+=(s,e)=>{SetDiagnosticsVisible(true);tabs.SelectedTab=details;};
            client.Trace+=s=>Ui(()=>Log(s));client.Message+=(t,m)=>Ui(()=>Receive(t,m));
            FormClosed+=(s,e)=>client.Dispose();
        }
        void SetDiagnosticsVisible(bool visible){
            workspace.Panel2Collapsed=!visible;
            if(visible&&workspace.Height>300)workspace.SplitterDistance=Math.Max(180,workspace.Height-200);
            diagnosticToggle.Text=visible?"收起操作结果 / 开发工具":"操作结果 / 开发工具";
        }
        static ComboBox Choice(params string[] values){var c=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=105};c.Items.AddRange(values);c.SelectedIndex=0;return c;}
        static Label Label(string text)=>new Label{Text=text,AutoSize=true,Padding=new Padding(0,6,0,0)};
        static FlowLayoutPanel Bar()=>new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false,Padding=new Padding(4),AutoScroll=true};
        void Ui(Action action){if(IsDisposed||!IsHandleCreated)return;if(InvokeRequired)BeginInvoke(action);else action();}
        void AddButton(Control parent,string text,Func<Task> action){var b=new Button{Text=text,AutoSize=true};actions.Add(b);b.Click+=async(s,e)=>{if(busy)return;busy=true;foreach(var x in actions)x.Enabled=false;try{await action();}catch(Exception ex){detail.Text=ex.Message;Log("ERROR "+ex.Message);}finally{busy=false;foreach(var x in actions)x.Enabled=true;}};parent.Controls.Add(b);}
        void Log(string message) {
            const int maximumLineLength = 1200;
            string line = message.Replace(Environment.NewLine, " ");
            if (line.Length > maximumLineLength) {
                line = line.Substring(0, maximumLineLength) + " …（完整消息见 JSONL 文件）";
            }
            string[] lines = log.Lines;
            if (lines.Length >= 200) {
                log.Lines = lines.Skip(lines.Length - 199).ToArray();
            }
            log.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + Environment.NewLine);
        }
        async Task Connect() {
            if (connected) {
                throw new Exception("切换账户请关闭并重新打开平台。");
            }
            string account = user.Text;
            string address = gatewayHost.Text;
            string password = loginPassword.Text;
            int portNumber = (int)gatewayPort.Value;
            await Task.Run(() => client.Connect(account, portNumber, address, password));
            connected = true;
            user.Enabled = false;
            gatewayHost.Enabled = false;
            gatewayPort.Enabled = false;
            loginPassword.Enabled = false;
            var data = await Request("ficc.quote.stream.open", new JObject());
            streamId = (string)data["streamId"];
            topic = (string)data["pushTopic"];
            sequence = (long?)data["sequence"] ?? 0;
            await Task.Run(() => client.Subscribe(topic, false));
            await OpenView();
            await OpenPriceView();
            await ConnectNotifications();
            counterpartyEditor.ConfigureSearch(Request);
            counterpartyEditor.SetReferenceData(Array.Empty<JObject>(), Array.Empty<JObject>());
            counterpartyEditor.LoadFields(Selected());
        }
        async Task<JObject> Request(string requestTopic, JObject request) {
            if (!connected) {
                throw new Exception("请先连接 GW。");
            }
            return await Task.Run(() => {
                string phase = requestTopic == "ficc.quote.view.open" ? "open" : "close";
                Action<string> measurement = filterPhase;
                measurement?.Invoke(phase + ".worker.start");
                JObject reply = client.Data(requestTopic, request);
                measurement?.Invoke(phase + ".worker.end");
                return reply;
            });
        }
        JObject Filter() {
            if (performanceFilter != null) {
                return (JObject)performanceFilter.DeepClone();
            }
            var filter = new JObject();
            if (rating.SelectedIndex > 0) {
                filter["conditions"] = new JArray(new JObject {
                    ["field"] = "bondRating",
                    ["operator"] = "EQ",
                    ["value"] = rating.Text
                });
            }
            return filter;
        }

        void Receive(string messageTopic, JObject message) {
            if (messageTopic == notificationTopic) {
                ReceiveNotification(message);
                return;
            }
            if (messageTopic == priceTopic) {
                ReceivePrice(message);
                return;
            }
            if (messageTopic == topic) ReceiveQuote(message);
        }
        JObject Selected()=>grid.CurrentCell!=null&&grid.CurrentCell.RowIndex<rows.Count?rows[grid.CurrentCell.RowIndex]:null;
        JObject Input(){var input=new JObject{["securityId"]=security.Text,["side"]=side.Text,["quoteType"]="1",["yieldType"]=security.Text=="102500002.IB"?"2":"1",["price"]=price.Text,["quantity"]=quantity.Text,["settlType"]=settlement.Text,["tradeDate"]=DateTime.Now.ToString("yyyy-MM-dd")};foreach(var field in counterpartyEditor.Fields().Properties())input[field.Name]=field.Value.DeepClone();return input;}
        async Task Create(int count){var qs=new JArray();for(int i=0;i<count;i++){var q=Input();q["clientQuoteId"]="UI-"+Guid.NewGuid().ToString("N");qs.Add(q);}detail.Text=(await Request("ficc.quote.create",new JObject{["quotes"]=qs})).ToString();}
        async Task Command(string op) {
            if (op == "create") {
                await Create(1);
                return;
            }
            var selected = Selected();
            if (selected == null) throw new Exception("请先选中一条报价。");
            var request = op == "update" ? Input() : new JObject();
            if (op == "confirm") {
                var fields = counterpartyEditor.Fields();
                if (CounterpartyEditor.NeedsMatching(fields)) {
                    if (automatedUi) {
                        throw new InvalidOperationException("自动测试的对手名称尚未匹配 ID，不能打开人工确认窗口。");
                    }
                    fields = counterpartyEditor.ConfirmSelection(this);
                    if (fields == null) return;
                    counterpartyEditor.LoadFields(fields);
                }
                foreach (var field in fields.Properties()) {
                    request[field.Name] = field.Value.DeepClone();
                }
            }
            request["expectedRevision"] = selected["revision"].DeepClone();
            if (op == "clone") {
                request["sourceQuoteId"] = selected["quoteId"].DeepClone();
                request["clientQuoteId"] = "UI-" + Guid.NewGuid().ToString("N");
            } else {
                request["quoteId"] = selected["quoteId"].DeepClone();
            }
            detail.Text = (await Request("ficc.quote." + op, new JObject { ["quotes"] = new JArray(request) })).ToString();
        }
        async Task History(){var q=Selected();if(q==null)throw new Exception("请先选中报价。");detail.Text=(await Request("ficc.quote.history.query",new JObject{["quoteId"]=q["quoteId"].DeepClone(),["limit"]=100})).ToString();}
        async Task PriceBook(){detail.Text=(await Request("ficc.price.query",new JObject{["securityIds"]=new JArray(security.Text),["mode"]="FULL"})).ToString();}
        Task OpenApiDebugger() {
            string executable = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "api-debugger", "Ficc.DpsApiDebugger.exe");
            if (!System.IO.File.Exists(executable)) {
                var directory = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                while (directory != null && !System.IO.File.Exists(
                        System.IO.Path.Combine(directory.FullName, "SalesTradingClient.csproj"))) {
                    directory = directory.Parent;
                }
                if (directory == null) {
                    throw new Exception("缺少 API 调试器，请复制完整客户端 bin 文件夹。");
                }
                executable = System.IO.Path.Combine(directory.Parent.FullName,
                        "api-debugger", "bin", "Debug", "net472", "Ficc.DpsApiDebugger.exe");
            }
            if (!System.IO.File.Exists(executable)) {
                throw new Exception("缺少 API 调试器，请复制完整客户端 bin 文件夹。");
            }
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(executable) {
                WorkingDirectory = System.IO.Path.GetDirectoryName(executable),
                UseShellExecute = true
            });
            return Task.CompletedTask;
        }
    }
}
