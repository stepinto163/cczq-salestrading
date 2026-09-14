using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    public sealed class ApiDebugForm : Form {
        DpsClient client;bool connected;
        readonly TextBox host=new TextBox{Text="127.0.0.1",Width=115},user=new TextBox{Text="demo",Width=80},password=new TextBox{Text="demo",UseSystemPasswordChar=true,Width=80};
        readonly NumericUpDown port=new NumericUpDown{Minimum=1,Maximum=65535,Value=18600,Width=70};
        readonly ComboBox server=Choice("QuoteSvr","NotificationSvr"),presets=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=430};
        readonly TextBox topic=new TextBox{Text="ficc.quote.dictionary.query",Width=420},subscribeTopic=new TextBox{Width=520};
        readonly CheckBox newId=new CheckBox{Text="每次生成新 requestId",Checked=true,AutoSize=true},image=new CheckBox{Text="行情订阅快照",AutoSize=true};
        readonly TextBox request=Editor(),response=Editor(true),pushBody=Editor(true),variables=Editor(),trace=Editor(true);
        readonly Label status=new Label{AutoSize=true,Text="未连接",Padding=new Padding(5)};
        readonly ListBox subscriptionList=new ListBox{Dock=DockStyle.Left,Width=210},historyList=new ListBox{Dock=DockStyle.Left,Width=440},messageList=new ListBox{Dock=DockStyle.Left,Width=440};
        readonly List<JObject> history=new List<JObject>(),messages=new List<JObject>();
        readonly List<JObject> catalog=new List<JObject>();
        readonly Button connect=new Button{Text="连接 GW",AutoSize=true},send=new Button{Text="发送请求",AutoSize=true};
        JObject lastReply;
        public ApiDebugForm(){
            Text="FICC DPS API 调试器";Width=1400;Height=880;MinimumSize=new Size(1000,650);StartPosition=FormStartPosition.CenterScreen;Font=new Font("Microsoft YaHei UI",9F);
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=5};layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,40));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,38));layout.RowStyles.Add(new RowStyle(SizeType.Percent,55));layout.RowStyles.Add(new RowStyle(SizeType.Percent,45));Controls.Add(layout);
            var login=Bar();login.Controls.AddRange(new Control[]{Label("GW"),host,port,Label("账户"),user,Label("密码"),password,connect,status});Button(login,"断开",Disconnect);layout.Controls.Add(login,0,0);connect.Click+=async(s,e)=>await Guard(Connect);
            var address=Bar();server.Width=150;address.Controls.AddRange(new Control[]{Label("服务"),server,Label("接口模板"),presets,Label("topic"),topic});layout.Controls.Add(address,0,1);
            var commands=Bar();commands.Controls.Add(send);commands.Controls.Add(newId);Button(commands,"格式化 JSON",()=>{request.Text=JObject.Parse(request.Text).ToString();return Task.CompletedTask;});Button(commands,"保存请求",SaveRequest);Button(commands,"载入请求",LoadRequest);commands.Controls.Add(Label("模板中的 ${变量} 在发送时替换；请求和回复分别显示。"));layout.Controls.Add(commands,0,2);send.Click+=async(s,e)=>await Guard(Send);
            var pair=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};pair.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));pair.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,50));pair.Controls.Add(Group("请求 JSON",request),0,0);pair.Controls.Add(Group("回复 JSON",response),1,0);layout.Controls.Add(pair,0,3);request.Text="{}";
            var tabs=new TabControl{Dock=DockStyle.Fill};var sub=new TabPage("订阅 / 发布");var subBar=Bar();subBar.Dock=DockStyle.Top;subBar.Height=38;subBar.Controls.Add(subscribeTopic);subBar.Controls.Add(image);Button(subBar,"订阅",Subscribe);Button(subBar,"退订",Unsubscribe);sub.Controls.Add(messageList);sub.Controls.Add(subscriptionList);sub.Controls.Add(subBar);messageList.Dock=DockStyle.Fill;
            var push=new TabPage("推送内容");push.Controls.Add(pushBody);messageList.SelectedIndexChanged+=(s,e)=>{int i=messageList.SelectedIndex;if(i>=0&&i<messages.Count)pushBody.Text=messages[i].ToString();};
            var vars=new TabPage("变量");variables.Text=new JObject{["quoteId"]="",["revision"]="1",["viewId"]="DBG-"+Guid.NewGuid().ToString("N"),["clientQuoteId"]="DBG-"+Guid.NewGuid().ToString("N"),["sourceMessageId"]="",["watchlistId"]="",["filterGroupId"]="",["notificationId"]="",["epoch"]=""}.ToString();vars.Controls.Add(variables);
            var past=new TabPage("请求历史（最近 100 条）");past.Controls.Add(historyList);historyList.Dock=DockStyle.Fill;historyList.SelectedIndexChanged+=(s,e)=>{int i=historyList.SelectedIndex;if(i<0||i>=history.Count)return;var record=history[i];server.Text=(string)record["serverName"];topic.Text=(string)record["topic"];request.Text=record["request"].ToString();response.Text=record["reply"].ToString();};
            var logs=new TabPage("消息日志");logs.Controls.Add(trace);var logBar=Bar();logBar.Dock=DockStyle.Top;logBar.Height=38;Button(logBar,"打开完整日志",()=>{Directory.CreateDirectory(MessageLog.DirectoryPath);Process.Start(new ProcessStartInfo("explorer.exe","\""+MessageLog.DirectoryPath+"\""){UseShellExecute=true});return Task.CompletedTask;});logBar.Controls.Add(new TextBox{ReadOnly=true,Width=950,Text=MessageLog.DirectoryPath});logs.Controls.Add(logBar);
            tabs.TabPages.AddRange(new[]{sub,push,vars,past,logs});layout.Controls.Add(tabs,0,4);
            string catalogPath=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"requests.json");if(File.Exists(catalogPath))foreach(JObject item in JArray.Parse(File.ReadAllText(catalogPath)))catalog.Add(item);
            foreach(var item in catalog)presets.Items.Add((string)item["topic"]+" · "+(string)item["description"]);
            presets.SelectedIndexChanged+=(s,e)=>{if(presets.SelectedIndex<0)return;var preset=catalog[presets.SelectedIndex];server.Text=(string)preset["serverName"];topic.Text=(string)preset["topic"];request.Text=preset["request"].ToString();};
            if(catalog.Count>0)presets.SelectedIndex=catalog.FindIndex(x=>(string)x["topic"]=="ficc.quote.dictionary.query");
            FormClosed+=(s,e)=>client?.Dispose();
        }
        static ComboBox Choice(params string[] values){var box=new ComboBox{DropDownStyle=ComboBoxStyle.DropDown};box.Items.AddRange(values);box.SelectedIndex=0;return box;}
        static TextBox Editor(bool readOnly=false)=>new TextBox{Multiline=true,ReadOnly=readOnly,ScrollBars=ScrollBars.Both,WordWrap=false,Dock=DockStyle.Fill,Font=new Font("Consolas",10F)};
        static FlowLayoutPanel Bar()=>new FlowLayoutPanel{Dock=DockStyle.Fill,WrapContents=false,AutoScroll=true,Padding=new Padding(4)};
        static Label Label(string text)=>new Label{Text=text,AutoSize=true,Padding=new Padding(2,5,2,0)};
        static GroupBox Group(string title,Control child){var group=new GroupBox{Text=title,Dock=DockStyle.Fill};group.Controls.Add(child);return group;}
        void Button(Control parent,string text,Func<Task> action){var button=new Button{Text=text,AutoSize=true};button.Click+=async(s,e)=>{button.Enabled=false;try{await Guard(action);}finally{button.Enabled=true;}};parent.Controls.Add(button);}
        async Task Guard(Func<Task> action){try{await action();}catch(Exception error){status.Text="失败："+error.Message;response.Text=error.ToString();}}
        void Ui(Action action){if(!IsDisposed&&IsHandleCreated){if(InvokeRequired)BeginInvoke(action);else action();}}
        async Task Connect(){
            if(connected)throw new Exception("请先断开现有连接。");client?.Dispose();connect.Enabled=false;var current=new DpsClient();client=current;
            current.Trace+=line=>Ui(()=>{if(trace.Lines.Length>=200)trace.Lines=trace.Lines.Skip(1).ToArray();trace.AppendText(line+Environment.NewLine);});
            current.Message+=(t,m)=>Ui(()=>{if(client!=current)return;if(messages.Count>=200){messages.RemoveAt(0);messageList.Items.RemoveAt(0);}messages.Add(new JObject{["time"]=DateTimeOffset.Now.ToString("o"),["topic"]=t,["message"]=m});messageList.Items.Add(DateTime.Now.ToString("HH:mm:ss.fff")+" "+(string)m["kind"]+" "+t);messageList.SelectedIndex=messages.Count-1;});
            string account=user.Text,address=host.Text,secret=password.Text;int p=(int)port.Value;
            try{await Task.Run(()=>current.Connect(account,p,address,secret));connected=true;status.Text="已连接 "+address+":"+p;}finally{connect.Enabled=true;}
        }
        Task Disconnect(){client?.Dispose();client=null;connected=false;subscriptionList.Items.Clear();status.Text="已断开";return Task.CompletedTask;}
        JToken Resolve(JToken value,JObject vars){
            if(value is JObject obj){foreach(var property in obj.Properties())property.Value=Resolve(property.Value,vars);}
            else if(value is JArray array){for(int i=0;i<array.Count;i++)array[i]=Resolve(array[i],vars);}
            else if(value.Type==JTokenType.String){string text=(string)value;if(text.StartsWith("${")&&text.EndsWith("}")){string name=text.Substring(2,text.Length-3);if(vars[name]==null)throw new Exception("缺少变量："+name);return vars[name].DeepClone();}}
            return value;
        }
        async Task Send(){
            if(!connected)throw new Exception("请先连接 GW。");send.Enabled=false;
            try{
                var vars=JObject.Parse(variables.Text);if(topic.Text.EndsWith(".create")||topic.Text.EndsWith(".clone"))vars["clientQuoteId"]="DBG-"+Guid.NewGuid().ToString("N");if(topic.Text.EndsWith(".view.open"))vars["viewId"]="DBG-"+Guid.NewGuid().ToString("N");
                var body=(JObject)Resolve(JObject.Parse(request.Text),vars);if(newId.Checked)body["requestId"]="DBG-"+Guid.NewGuid().ToString("N");string name=server.Text,t=topic.Text;
                var timer=Stopwatch.StartNew();lastReply=await Task.Run(()=>client.RequestTo(name,t,body));timer.Stop();response.Text=lastReply.ToString();status.Text="Code="+(string)lastReply["Code"]+" · "+timer.ElapsedMilliseconds+" ms";
                if(lastReply["Data"] is JObject data){if(data["pushTopic"]!=null)subscribeTopic.Text=(string)data["pushTopic"];CaptureVariables(data,vars);variables.Text=vars.ToString();}
                if(history.Count>=100){history.RemoveAt(0);historyList.Items.RemoveAt(0);}history.Add(new JObject{["serverName"]=name,["topic"]=t,["request"]=body,["reply"]=lastReply,["elapsedMs"]=timer.ElapsedMilliseconds});historyList.Items.Add(DateTime.Now.ToString("HH:mm:ss")+" "+t+" · "+(string)lastReply["Code"]);
            }finally{send.Enabled=true;}
        }
        static void CaptureVariables(JToken token,JObject vars){
            if(token is JObject obj){foreach(string key in new[]{"quoteId","revision","viewId","sourceMessageId","watchlistId","filterGroupId","notificationId","epoch"})if(obj[key]!=null&&obj[key].Type!=JTokenType.Null)vars[key]=obj[key].DeepClone();foreach(var property in obj.Properties())if(property.Value is JObject||property.Value is JArray)CaptureVariables(property.Value,vars);}
            else if(token is JArray array&&array.Count>0)CaptureVariables(array[0],vars);
        }
        async Task Subscribe(){if(!connected)throw new Exception("请先连接 GW。");string t=subscribeTopic.Text,name=server.Text;bool snapshot=image.Checked;if(snapshot&&!t.StartsWith("ficc.price."))throw new Exception("本原型只有行情 topic 提供订阅快照；报价和通知使用普通订阅。");await Task.Run(()=>client.Subscribe(t,snapshot,name));subscriptionList.Items.Add(t);status.Text="已订阅："+t;}
        async Task Unsubscribe(){string t=subscriptionList.SelectedItem as string??subscribeTopic.Text;if(client==null)return;await Task.Run(()=>client.Unsubscribe(t));subscriptionList.Items.Remove(t);status.Text="已退订："+t;}
        Task SaveRequest(){using(var dialog=new SaveFileDialog{Filter="DPS request (*.json)|*.json",FileName="dps-request.json"})if(dialog.ShowDialog(this)==DialogResult.OK)File.WriteAllText(dialog.FileName,new JObject{["serverName"]=server.Text,["topic"]=topic.Text,["request"]=JObject.Parse(request.Text),["variables"]=JObject.Parse(variables.Text)}.ToString());return Task.CompletedTask;}
        Task LoadRequest(){using(var dialog=new OpenFileDialog{Filter="DPS request (*.json)|*.json"})if(dialog.ShowDialog(this)==DialogResult.OK){var record=JObject.Parse(File.ReadAllText(dialog.FileName));server.Text=(string)record["serverName"];topic.Text=(string)record["topic"];request.Text=record["request"].ToString();if(record["variables"]!=null)variables.Text=record["variables"].ToString();}return Task.CompletedTask;}
        internal async Task SelfTest(string output){
            await Connect();topic.Text="ficc.quote.dictionary.query";request.Text="{}";await Send();if((string)lastReply["Code"]!="0000")throw new Exception("Quote request failed");
            server.Text="NotificationSvr";topic.Text="ficc.notification.preference.get";await Send();if(lastReply["Data"]?["preference"]==null)throw new Exception("Notification request failed");
            server.Text="QuoteSvr";topic.Text="ficc.price.stream.open";await Send();image.Checked=true;await Subscribe();
            var end=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<end&&!messages.Any(m=>(string)m["message"]["kind"]=="SNAPSHOT"))await Task.Delay(50);
            if(!messages.Any(m=>(string)m["message"]["kind"]=="SNAPSHOT"))throw new Exception("Native price subscription snapshot missing");
            await Unsubscribe();if(history.Count!=3||subscriptionList.Items.Count!=0)throw new Exception("History/unsubscribe failed");
            using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Path.ChangeExtension(output,"png"),ImageFormat.Png);}
            File.WriteAllText(output,new JObject{["result"]="PASS",["requests"]=3,["quotesCreated"]=0,["templateCount"]=catalog.Count,["nativePriceSnapshot"]=true,["messageLogDirectory"]=MessageLog.DirectoryPath}.ToString());
        }
    }
}
