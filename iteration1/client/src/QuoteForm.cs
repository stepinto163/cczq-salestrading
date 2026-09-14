using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    // Bounded virtual grid; performance mode measures the same view path.
    public sealed partial class QuoteForm : Form {
        readonly DpsClient client=new DpsClient();
        readonly List<JObject> rows=new List<JObject>();
        readonly Dictionary<string,JObject> cache=new Dictionary<string,JObject>();
        readonly List<string> ids=new List<string>();
        readonly List<Tuple<string,JObject>> pending=new List<Tuple<string,JObject>>();
        readonly DataGridView grid=new DataGridView();
        readonly ComboBox user=Choice("demo","demo2","admin");
        readonly ComboBox security=Choice("250001.IB","250002.IB","102500001.IB","102500002.IB");
        readonly ComboBox side=Choice("1","2"), settlement=Choice("1","2");
        readonly ComboBox statusFilter=Choice("全部我的报价","待确认","有效报价","已撤出","已删除");
        readonly ComboBox rating=Choice("全部评级","AAA","AA+");
        readonly TextBox price=new TextBox{Text="2.5000",Width=85};
        readonly TextBox quantity=new TextBox{Text="1000",Width=80};
        readonly CounterpartyEditor counterpartyEditor=new CounterpartyEditor();
        readonly TextBox log=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Vertical,Dock=DockStyle.Fill};
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
            Text="FICC 报价生命周期原型 · GW / DPS";Width=1180;Height=760;MinimumSize=new Size(900,600);
            Font=new Font("Microsoft YaHei UI",9F);StartPosition=FormStartPosition.CenterScreen;
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,ColumnCount=1,Padding=new Padding(4)};
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,28));
            Controls.Add(layout);
            var connectBar=Bar();connectBar.Controls.Add(Label("账户"));connectBar.Controls.Add(user);AddButton(connectBar,"连接 GW",Connect);
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
            grid.Columns["quoteId"].DisplayIndex=8;grid.Columns["revision"].DisplayIndex=7;
            quoteLayout.Controls.Add(grid,0,4);
            var quoteBoard=new TabPage("报价管理");quoteBoard.Controls.Add(quoteLayout);boards.TabPages.Add(quoteBoard);InitializePriceBoard();InitializeNotificationBoard();
            var details=new TabPage("操作结果 / 历史 / 行情");details.Controls.Add(detail);var messages=new TabPage("DPS 消息（最近 200 条；完整记录见文件）");messages.Controls.Add(log);messages.Controls.Add(new TextBox{Dock=DockStyle.Top,ReadOnly=true,Text=client.LogDirectory});tabs.TabPages.AddRange(new[]{details,messages});InitializeAutoTestTab();
            var toolsBar=Bar();toolsBar.Dock=DockStyle.Top;toolsBar.Height=40;
            AddButton(toolsBar,"自动测试",RunAutoTests);AddButton(toolsBar,"多用户场景",RunMultiUserScenario);AddButton(toolsBar,"API 调试器",OpenApiDebugger);
            AddButton(toolsBar, "筛选性能测试(500条)", OpenFilterPerformanceTest);
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
        async Task Connect(){if(connected)throw new Exception("切换账户请关闭并重新打开原型。");string name=user.Text;await Task.Run(()=>client.Connect(name));connected=true;user.Enabled=false;var data=await Request("ficc.quote.stream.open",new JObject());streamId=(string)data["streamId"];topic=(string)data["pushTopic"];sequence=(long?)data["sequence"]??0;await Task.Run(()=>client.Subscribe(topic,false));await OpenView();await OpenPriceView();await ConnectNotifications();var institutions=await Request("ficc.base.query",new JObject{["kind"]="INSTITUTION"});var contacts=await Request("ficc.base.query",new JObject{["kind"]="CONTACT"});counterpartyEditor.SetReferenceData(((JArray)institutions["records"]).OfType<JObject>(),((JArray)contacts["records"]).OfType<JObject>());counterpartyEditor.LoadFields(Selected());}
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

        async Task OpenView() {
            if (!connected) {
                throw new Exception("请先连接 GW。");
            }
            filterPhase?.Invoke("close.start");
            if (viewId != null) {
                await Request("ficc.quote.view.close", new JObject { ["viewId"] = viewId });
            }
            filterPhase?.Invoke("close.end");
            viewId = "V-" + Guid.NewGuid().ToString("N");
            var statuses = statusFilter.SelectedIndex == 0
                ? new JArray("PENDING_CONFIRM", "ACTIVE", "REFERRED", "DELETED")
                : new JArray(new[] { "", "PENDING_CONFIRM", "ACTIVE", "REFERRED", "DELETED" }[statusFilter.SelectedIndex]);
            opening = true;
            pending.Clear();
            try {
                filterPhase?.Invoke("open.start");
                JObject reply = await Request("ficc.quote.view.open", new JObject {
                    ["viewId"] = viewId,
                    ["ownerScope"] = "MINE",
                    ["status"] = statuses,
                    ["limit"] = 200,
                    ["filter"] = Filter()
                });
                filterPhase?.Invoke("open.end");
                cache.Clear();
                ids.Clear();
                foreach (JObject quote in (JArray)reply["quotes"]) {
                    cache[(string)quote["quoteId"]] = quote;
                }
                foreach (JToken id in (JArray)reply["view"]["ids"]) {
                    ids.Add((string)id);
                }
                lastFilterTotalCount = (int)reply["view"]["totalCount"];
                sequence = (long)reply["sequence"];
                Render();
                filterPhase?.Invoke("apply.end");
            } finally {
                opening = false;
                var messages = pending.ToArray();
                pending.Clear();
                foreach (var message in messages) {
                    Receive(message.Item1, message.Item2);
                }
            }
        }
        void Receive(string t,JObject m){
            if(t==notificationTopic){ReceiveNotification(m);return;}
            if(t==priceTopic){ReceivePrice(m);return;}
            if(t!=topic)return;if(opening){if(pending.Count<500)pending.Add(Tuple.Create(t,m));else state.Text="等待回复期间消息过多，请重新筛选";return;}
            if((string)m["streamId"]!=streamId)return;
            long n=(long?)m["sequence"]??sequence;if(n<=sequence)return;
            if(n!=sequence+1||(string)m["kind"]=="RESET_REQUIRED"){state.Text="消息需要重新同步，请点击筛选";return;}sequence=n;
            foreach(JObject q in (JArray)m["quotes"]??new JArray())cache[(string)q["quoteId"]]=q;
            foreach(JObject v in (JArray)m["views"]??new JArray())if((string)v["viewId"]==viewId){foreach(var id in (JArray)v["removeIds"]??new JArray())ids.Remove((string)id);foreach(JObject p in (JArray)v["place"]??new JArray()){string id=(string)p["id"];ids.Remove(id);ids.Insert(Math.Min((int)p["index"],ids.Count),id);}}
            foreach(string key in cache.Keys.Where(k=>!ids.Contains(k)).ToArray())cache.Remove(key);Render();
        }
        void Render(){string selected=(string)Selected()?["quoteId"];rows.Clear();foreach(var id in ids)if(cache.TryGetValue(id,out var q))rows.Add(q);grid.RowCount=rows.Count;grid.Invalidate();int at=rows.FindIndex(q=>(string)q["quoteId"]==selected);if(at>=0)grid.CurrentCell=grid[0,at];state.Text="已连接 GW · "+rows.Count+" 条已加载 · 序号 "+sequence;}
        JObject Selected()=>grid.CurrentCell!=null&&grid.CurrentCell.RowIndex<rows.Count?rows[grid.CurrentCell.RowIndex]:null;
        JObject Input(){var input=new JObject{["securityId"]=security.Text,["side"]=side.Text,["quoteType"]="1",["yieldType"]=security.Text=="102500002.IB"?"2":"1",["price"]=price.Text,["quantity"]=quantity.Text,["settlType"]=settlement.Text,["tradeDate"]=DateTime.Now.ToString("yyyy-MM-dd")};foreach(var field in counterpartyEditor.Fields().Properties())input[field.Name]=field.Value.DeepClone();return input;}
        async Task Create(int count){var qs=new JArray();for(int i=0;i<count;i++){var q=Input();q["clientQuoteId"]="UI-"+Guid.NewGuid().ToString("N");qs.Add(q);}detail.Text=(await Request("ficc.quote.create",new JObject{["quotes"]=qs})).ToString();}
        async Task Command(string op){if(op=="create"){await Create(1);return;}var selected=Selected();if(selected==null)throw new Exception("请先选中一条报价。");var q=op=="update"?Input():new JObject();if(op=="confirm"){var fields=counterpartyEditor.Fields();if(CounterpartyEditor.NeedsMatching(fields)){fields=counterpartyEditor.ConfirmSelection(this);if(fields==null)return;counterpartyEditor.LoadFields(fields);}foreach(var field in fields.Properties())q[field.Name]=field.Value.DeepClone();}q["expectedRevision"]=selected["revision"].DeepClone();if(op=="clone"){q["sourceQuoteId"]=selected["quoteId"].DeepClone();q["clientQuoteId"]="UI-"+Guid.NewGuid().ToString("N");}else q["quoteId"]=selected["quoteId"].DeepClone();detail.Text=(await Request("ficc.quote."+op,new JObject{["quotes"]=new JArray(q)})).ToString();}
        async Task History(){var q=Selected();if(q==null)throw new Exception("请先选中报价。");detail.Text=(await Request("ficc.quote.history.query",new JObject{["quoteId"]=q["quoteId"].DeepClone(),["limit"]=100})).ToString();}
        async Task PriceBook(){detail.Text=(await Request("ficc.price.query",new JObject{["securityIds"]=new JArray(security.Text),["mode"]="FULL"})).ToString();}
        Task OpenApiDebugger(){var path=new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);while(path!=null&&!System.IO.File.Exists(System.IO.Path.Combine(path.FullName,"QuotePrototype.csproj")))path=path.Parent;if(path==null)throw new Exception("无法定位 API 调试器。");string exe=System.IO.Path.Combine(path.Parent.FullName,"api-debugger","bin","Debug","net472","Ficc.DpsApiDebugger.exe");if(!System.IO.File.Exists(exe))throw new Exception("请先构建 API 调试器。");System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(exe){WorkingDirectory=System.IO.Path.GetDirectoryName(exe),UseShellExecute=true});return Task.CompletedTask;}
    }
}
