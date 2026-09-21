using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        readonly TabPage notificationBoard=new TabPage("报价提醒");
        readonly DataGridView notificationGrid=new DataGridView();
        readonly ComboBox readFilter=Choice("全部提醒","未读提醒","已读提醒");
        readonly Dictionary<string,CheckBox> preferenceChecks=new Dictionary<string,CheckBox>();
        readonly Dictionary<string,JObject> notificationCache=new Dictionary<string,JObject>();
        readonly List<string> notificationIds=new List<string>();
        readonly List<JObject> pendingNotifications=new List<JObject>();
        readonly HashSet<string> shownAlerts=new HashSet<string>();
        readonly Dictionary<string,bool> readRetries=new Dictionary<string,bool>();
        readonly System.Windows.Forms.Timer readRetryTimer=new System.Windows.Forms.Timer{Interval=5000};
        string notificationTopic,notificationStreamId,notificationViewId,readRetryPath;long notificationSequence;
        bool openingNotifications,loadingPreference,readingNotifications,suppressEnterMark;JObject notificationPreference;Form toast;

        readonly SemaphoreSlim notificationViewGate = new SemaphoreSlim(1, 1);
        bool notificationBufferOverflow;
        int notificationPageNumber = 1;
        const int NotificationPageSize = 200;
        int notificationTotalCount;
        bool notificationRefillPending;
        bool notificationViewStale;

        void InitializeNotificationBoard(){
            var layout=new TableLayoutPanel{Dock=DockStyle.Fill,RowCount=3,ColumnCount=1};layout.RowStyles.Add(new RowStyle(SizeType.Absolute,34));layout.RowStyles.Add(new RowStyle(SizeType.Absolute,38));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            var switches=Bar();string[] keys={"watchlistEnabled","filterGroupEnabled","counterQuoteEnabled","myBestEnabled"};string[] names={"关注债券","筛选分组","对价提醒","我的最优报价"};
            for(int i=0;i<keys.Length;i++){var box=new CheckBox{Text=names[i],AutoSize=true,Checked=true,Padding=new Padding(4)};preferenceChecks.Add(keys[i],box);box.CheckedChanged+=async(s,e)=>{if(!loadingPreference&&notificationPreference!=null)await SaveNotificationPreference();};switches.Controls.Add(box);}
            var controls=Bar();controls.Controls.Add(readFilter);AddButton(controls,"刷新提醒",OpenNotificationView);AddButton(controls,"标为已读",()=>MarkSelectedNotification(true));AddButton(controls,"标为未读",()=>MarkSelectedNotification(false));AddButton(controls,"查看相关报价",ShowNotificationQuotes);AddButton(controls,"关注录入栏债券",SaveReminderWatchlist);AddButton(controls,"添加 AAA 筛选组",SaveReminderFilter);
            AddButton(controls, "上一批", () => QueryNotificationPage(notificationPageNumber - 1));
            AddButton(controls, "完整条件", () => EditProductFilter("notification"));
            AddButton(controls, "下一批", () => QueryNotificationPage(notificationPageNumber + 1));
            AddButton(controls, "返回最新", () => QueryNotificationPage(1));
            notificationGrid.Scroll += async (sender, args) => {
                if (args.ScrollOrientation == ScrollOrientation.VerticalScroll
                    && notificationGrid.FirstDisplayedScrollingRowIndex >= 0
                    && notificationGrid.FirstDisplayedScrollingRowIndex + notificationGrid.DisplayedRowCount(false) >= notificationIds.Count) {
                    await QueryNotificationPage(notificationPageNumber + 1);
                }
            };
            ConfigurePriceGrid(notificationGrid);notificationGrid.Columns.Add(Column("security","债券代码",110));notificationGrid.Columns.Add(Column("reasons","提醒类型",220));notificationGrid.Columns.Add(Column("content","说明",350));notificationGrid.Columns.Add(Column("read","已读",60));notificationGrid.Columns.Add(Column("time","更新时间",220));
            notificationGrid.CellValueNeeded+=(s,e)=>{if(e.RowIndex>=notificationIds.Count)return;var n=notificationCache[notificationIds[e.RowIndex]];switch(e.ColumnIndex){case 0:e.Value=(string)n["securityId"];break;case 1:e.Value=string.Join("、",((JArray)n["reasons"]).Select(r=>ReasonName((string)r["type"])));break;case 2:e.Value=(string)n["content"];break;case 3:e.Value=NotificationUnread(n)?"未读":"已读";break;case 4:e.Value=(string)n["updateTime"];break;}};
            notificationGrid.CellDoubleClick+=async(s,e)=>{if(e.RowIndex>=0)try{await ShowNotificationQuotes();}catch(Exception error){detail.Text=error.Message;}};
            layout.Controls.Add(switches,0,0);layout.Controls.Add(controls,0,1);layout.Controls.Add(notificationGrid,0,2);notificationBoard.Controls.Add(layout);boards.TabPages.Add(notificationBoard);
            boards.SelectedIndexChanged+=async(s,e)=>{if(!suppressEnterMark&&boards.SelectedTab==notificationBoard&&notificationTopic!=null)await MarkNotifications(notificationIds.ToArray(),true);};
            readRetryTimer.Tick+=async(s,e)=>await FlushReadRetries();FormClosed+=(s,e)=>{readRetryTimer.Stop();readRetryTimer.Dispose();toast?.Close();};
        }
        static bool NotificationUnread(JToken notification)=>notification["readTime"]==null||notification["readTime"].Type==JTokenType.Null;
        static string ReasonName(string type){switch(type){case "WATCHLIST":return "关注债券";case "FILTER_GROUP":return "筛选分组";case "COUNTER_QUOTE":return "对价提醒";case "MY_BEST":return "我的最优报价";default:return type;}}
        async Task ConnectNotifications(){
            notificationPreference=(JObject)(await Request("ficc.notification.preference.get",new JObject()))["preference"];ApplyPreference(notificationPreference);
            readRetryPath=Path.Combine(Directory.GetParent(Directory.GetParent(client.LogDirectory).FullName).FullName,"client-state","read-retry-"+user.Text+".json");
            if(File.Exists(readRetryPath))foreach(var property in JObject.Parse(File.ReadAllText(readRetryPath)).Properties())readRetries[property.Name]=(bool)property.Value;
            var stream=await Request("ficc.notification.stream.open",new JObject());notificationTopic=(string)stream["pushTopic"];notificationStreamId=(string)stream["streamId"];openingNotifications=true;
            await Task.Run(()=>client.Subscribe(notificationTopic,false));await OpenNotificationView();readRetryTimer.Start();
        }
        void ApplyPreference(JObject p){loadingPreference=true;try{notificationPreference=p;foreach(var pair in preferenceChecks)pair.Value.Checked=(bool)p[pair.Key];}finally{loadingPreference=false;}}
        async Task SaveNotificationPreference(){
            var request=new JObject{["expectedRevision"]=notificationPreference["revision"].DeepClone()};foreach(var pair in preferenceChecks)request[pair.Key]=pair.Value.Checked;
            foreach(var box in preferenceChecks.Values)box.Enabled=false;
            try{ApplyPreference((JObject)(await Request("ficc.notification.preference.save",request))["preference"]);}catch(Exception e){ApplyPreference(notificationPreference);detail.Text=e.Message;Log("提醒设置保存失败："+e.Message);}finally{foreach(var box in preferenceChecks.Values)box.Enabled=true;}
        }
        async Task OpenNotificationView() {
            if (notificationTopic == null) throw new InvalidOperationException("Connect to GW first");
            await notificationViewGate.WaitAsync();
            openingNotifications = true;
            notificationBufferOverflow = false;
            pendingNotifications.Clear();
            try {
                if (notificationViewId != null) {
                    await Request("ficc.notification.view.close", new JObject { ["viewId"] = notificationViewId });
                }
                notificationViewId = "NV-" + Guid.NewGuid().ToString("N");
                JObject reply = await Request("ficc.notification.view.open", new JObject {
                    ["viewId"] = notificationViewId,
                    ["readState"] = new[] { "ALL", "UNREAD", "READ" }[readFilter.SelectedIndex],
                    ["pageSize"] = NotificationPageSize,
                    ["filter"] = notificationConditions.DeepClone()
                });
                notificationSequence = (long)reply["sequence"];
                notificationPageNumber = (int)reply["view"]["pageNumber"];
                notificationTotalCount = (int)reply["view"]["totalCount"];
                notificationIds.Clear();
                notificationCache.Clear();
                foreach (JObject row in (JArray)reply["notifications"]) notificationCache[(string)row["notificationId"]] = row;
                foreach (string id in ((JArray)reply["view"]["ids"]).Values<string>()) notificationIds.Add(id);
                notificationViewStale = false;
                RenderNotifications();
            } catch {
                notificationViewStale = true;
                throw;
            } finally {
                FinishNotificationRequest();
                notificationViewGate.Release();
            }
        }

        void FinishNotificationRequest() {
            openingNotifications = false;
            if (notificationBufferOverflow) notificationViewStale = true;
            var buffered = pendingNotifications.ToArray();
            pendingNotifications.Clear();
            foreach (JObject message in buffered) ReceiveNotification(message);
        }

        async Task QueryNotificationPage(int pageNumber) {
            if (!connected || openingNotifications || notificationViewId == null || notificationViewStale || pageNumber < 1) return;
            if (pageNumber > notificationPageNumber && (long)(pageNumber - 1) * NotificationPageSize >= notificationTotalCount) return;
            if (!await notificationViewGate.WaitAsync(0)) return;
            openingNotifications = true;
            notificationBufferOverflow = false;
            pendingNotifications.Clear();
            try {
                JObject reply = await Request("ficc.notification.view.query", new JObject {
                    ["viewId"] = notificationViewId, ["pageNumber"] = pageNumber, ["pageSize"] = NotificationPageSize
                });
                if ((string)reply["streamId"] != notificationStreamId || (string)reply["view"]["viewId"] != notificationViewId) {
                    throw new InvalidOperationException("Notification view reply mismatch");
                }
                notificationSequence = (long)reply["sequence"];
                notificationPageNumber = (int)reply["view"]["pageNumber"];
                notificationTotalCount = (int)reply["view"]["totalCount"];
                notificationIds.Clear();
                notificationCache.Clear();
                foreach (JObject row in (JArray)reply["notifications"]) notificationCache[(string)row["notificationId"]] = row;
                foreach (string id in ((JArray)reply["view"]["ids"]).Values<string>()) notificationIds.Add(id);
                RenderNotifications();
            } catch (Exception error) {
                notificationViewStale = true;
                Log("NOTIFICATION_PAGE " + error.Message);
            } finally {
                FinishNotificationRequest();
                notificationViewGate.Release();
            }
        }

        async void ScheduleNotificationRefill() {
            if (notificationRefillPending) return;
            notificationRefillPending = true;
            string requestedView = notificationViewId;
            try {
                await Task.Delay(40);
                while (!IsDisposed && openingNotifications) await Task.Delay(20);
                notificationRefillPending = false;
                if (!IsDisposed && requestedView == notificationViewId && notificationPageNumber == 1) await QueryNotificationPage(1);
            } finally {
                notificationRefillPending = false;
            }
        }

        void ReceiveNotification(JObject message) {
            if ((string)message["streamId"] != notificationStreamId) return;
            if (openingNotifications) {
                if (pendingNotifications.Count < 200) pendingNotifications.Add(message);
                else notificationBufferOverflow = true;
                return;
            }
            if (notificationViewStale) return;
            long next = (long?)message["sequence"] ?? notificationSequence;
            if (next <= notificationSequence) return;
            if (next != notificationSequence + 1 || (string)message["kind"] == "RESET_REQUIRED") {
                notificationViewStale = true;
                notificationBoard.Text = "Notifications: refresh required";
                return;
            }
            notificationSequence = next;
            if (message["preference"] is JObject preference) ApplyPreference(preference);
            if ((string)message["kind"] == "HEARTBEAT") return;
            try {
                if (ClientViewPage.Apply(message, notificationViewId, "notifications", "notificationId",
                    notificationPageNumber, NotificationPageSize, notificationIds, notificationCache)) ScheduleNotificationRefill();
                foreach (JObject view in (JArray)message["views"] ?? new JArray()) {
                    if ((string)view["viewId"] == notificationViewId) notificationTotalCount = (int)view["totalCount"];
                }
                RenderNotifications();
                var fresh = notificationIds.Select(id => notificationCache[id])
                    .Where(row => NotificationUnread(row) && shownAlerts.Add((string)row["alertKey"] ?? (string)row["notificationId"]))
                    .Take(3).ToArray();
                if (fresh.Length > 0 && boards.SelectedTab != notificationBoard && !automatedUi) ShowNotificationToast(fresh);
            } catch (Exception error) {
                notificationViewStale = true;
                Log("NOTIFICATION_UPDATE " + error.Message);
            }
        }
        void RenderNotifications(){notificationGrid.RowCount=notificationIds.Count;notificationGrid.Invalidate();int unread=notificationIds.Count(id=>NotificationUnread(notificationCache[id]));notificationBoard.Text="报价提醒"+(unread>0?"（"+unread+" 未读）":"");}
        string SelectedNotificationId(){int index=notificationGrid.CurrentCell?.RowIndex??-1;return index>=0&&index<notificationIds.Count?notificationIds[index]:null;}
        async Task MarkSelectedNotification(bool read){string id=SelectedNotificationId();if(id==null)throw new Exception("请先选择提醒。");await MarkNotifications(new[]{id},read);}
        async Task MarkNotifications(IEnumerable<string> ids,bool read){foreach(string id in ids)if(notificationCache.TryGetValue(id,out var n)&&(read==(NotificationUnread(n))))readRetries[id]=read;PersistReadRetries();await FlushReadRetries();}
        void PersistReadRetries(){if(readRetryPath==null)return;Directory.CreateDirectory(Path.GetDirectoryName(readRetryPath));File.WriteAllText(readRetryPath,JObject.FromObject(readRetries).ToString());}
        async Task FlushReadRetries(){
            if(readingNotifications||readRetries.Count==0||!connected)return;readingNotifications=true;
            try{foreach(bool read in new[]{true,false}){var ids=readRetries.Where(p=>p.Value==read).Select(p=>p.Key).Take(200).ToArray();if(ids.Length==0)continue;var result=await Request("ficc.notification.read",new JObject{["notificationIds"]=new JArray(ids),["read"]=read});foreach(JObject r in (JArray)result["results"])if((string)r["Code"]=="0000"||(string)r["Code"]=="1002"){string id=(string)r["notificationId"];if(readRetries.TryGetValue(id,out var pending)&&pending==read)readRetries.Remove(id);}}PersistReadRetries();}
            catch(Exception e){Log("已读状态等待重试："+e.Message);}finally{readingNotifications=false;}
        }
        async Task ShowNotificationQuotes(){
            string id=SelectedNotificationId();if(id==null)throw new Exception("请先选择提醒。");var notification=notificationCache[id];var quotes=new JArray();
            foreach(string quoteId in ((JArray)notification["quoteIds"]).Select(x=>(string)x).Take(20))try{quotes.Add((await Request("ficc.quote.get",new JObject{["quoteId"]=quoteId}))["quote"]);}catch(Exception e){quotes.Add(new JObject{["quoteId"]=quoteId,["error"]=e.Message});}
            detail.Text=new JObject{["notification"]=notification.DeepClone(),["relatedQuotes"]=quotes,["displayLimit"]=20}.ToString();
            var window=new Form{Text=(string)notification["securityId"]+" · 提醒关联报价",Width=1050,Height=380,StartPosition=FormStartPosition.CenterParent,Font=Font};var table=new DataGridView();ConfigurePriceGrid(table);
            string[] keys={"side","quoteType","price","quantity","settlType","ownerUserId","institutionName","contactName","quoteId","error"};string[] headers={"方向","计价方式","报价","数量(万)","交割","我方报价人","对手机构","对手联系人","报价ID","状态说明"};
            for(int i=0;i<keys.Length;i++)table.Columns.Add(Column(keys[i],headers[i],i==8?240:110));
            table.CellValueNeeded+=(s,e)=>{if(e.RowIndex>=quotes.Count)return;string value=(string)quotes[e.RowIndex][keys[e.ColumnIndex]];if(e.ColumnIndex==0)value=value=="1"?"买入":value=="2"?"卖出":value;if(e.ColumnIndex==1)value=value=="1"?"收益率":value=="2"?"净价":value=="3"?"全价":value;if(e.ColumnIndex==4)value=value=="1"?"T+0":value=="2"?"T+1":value;e.Value=value;};table.RowCount=quotes.Count;window.Controls.Add(table);
            window.Controls.Add(new Label{Dock=DockStyle.Top,Height=28,Text="显示前 "+quotes.Count+" / "+((JArray)notification["quoteIds"]).Count+" 条关联报价；对手机构及联系人按后端权限显示。"});window.Show(this);
        }
        void ShowNotificationToast(JObject[] fresh){
            toast?.Close();var popup=new Form{Text="新增报价提醒",Width=420,Height=210,StartPosition=FormStartPosition.Manual,ShowInTaskbar=false,FormBorderStyle=FormBorderStyle.FixedToolWindow};toast=popup;var area=Screen.FromControl(this).WorkingArea;popup.Location=new Point(area.Right-popup.Width-12,area.Bottom-popup.Height-12);
            var text=new Label{Dock=DockStyle.Fill,Padding=new Padding(12),Text=string.Join(Environment.NewLine+Environment.NewLine,fresh.Select(n=>(string)n["securityId"]+"  "+string.Join("、",((JArray)n["reasons"]).Select(r=>ReasonName((string)r["type"])))))};
            var bar=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=38};var view=new Button{Text="查看",AutoSize=true};var close=new Button{Text="关闭",AutoSize=true};
            view.Click+=async(s,e)=>{popup.Close();await MarkNotifications(fresh.Select(n=>(string)n["notificationId"]),true);suppressEnterMark=true;try{boards.SelectedTab=notificationBoard;}finally{suppressEnterMark=false;}int row=notificationIds.IndexOf((string)fresh[0]["notificationId"]);if(row>=0)notificationGrid.CurrentCell=notificationGrid[0,row];};close.Click+=(s,e)=>popup.Close();bar.Controls.Add(view);bar.Controls.Add(close);popup.Controls.Add(text);popup.Controls.Add(bar);popup.Show(this);
        }
        async Task SaveReminderWatchlist(){
            string sec=security.Text,market=sec.StartsWith("102")?"CREDIT":"RATE";var records=(JArray)(await Request("ficc.watchlist.query",new JObject{["market"]=market,["scope"]="MARKET"}))["watchlists"];var old=records.OfType<JObject>().FirstOrDefault(g=>(string)g["name"]=="平台关注债券");
            var req=new JObject{["name"]="平台关注债券",["market"]=market,["scope"]="MARKET",["securities"]=old?["securities"]?.DeepClone()??new JArray()};var securities=(JArray)req["securities"];if(!securities.Any(s=>(string)s["securityId"]==sec))securities.Add(new JObject{["securityId"]=sec});if(old!=null){req["watchlistId"]=old["watchlistId"].DeepClone();req["expectedRevision"]=old["revision"].DeepClone();}detail.Text=(await Request("ficc.watchlist.save",req)).ToString();
        }
        async Task SaveReminderFilter(){string market=security.Text.StartsWith("102")?"CREDIT":"RATE";detail.Text=(await Request("ficc.filterGroup.save",new JObject{["name"]="平台 AAA 提醒筛选组",["market"]=market,["scope"]="MARKET",["subgroups"]=new JArray(new JObject{["conditions"]=new JArray(new JObject{["field"]="bondRating",["operator"]="EQ",["value"]="AAA"})})})).ToString();}
    }
}
