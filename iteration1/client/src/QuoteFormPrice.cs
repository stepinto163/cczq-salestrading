using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        readonly TabPage priceBoard=new TabPage("行情中心");
        readonly ComboBox priceBond=Choice("全部债券","250001.IB","250002.IB","102500001.IB","102500002.IB");
        readonly ComboBox priceMode=Choice("最优报价","全部报价","指定档数");
        readonly ComboBox priceSettlement=Choice("全部交割","T+0","T+1");
        readonly NumericUpDown priceDepth=new NumericUpDown{Minimum=1,Maximum=20,Value=2,Width=45,Enabled=false};
        readonly Label priceState=new Label{AutoSize=true,Text="连接 GW 后自动订阅行情",Padding=new Padding(4)};
        readonly Label priceDetailState=new Label{Dock=DockStyle.Top,Height=28,Text="左侧选择债券，右侧显示该债行情"};
        readonly Label priceGroupState=new Label{AutoSize=true,Text="未应用分组",Padding=new Padding(4)};
        JObject selectedPriceGroup;
        readonly DataGridView bondGrid=new DataGridView(),entryGrid=new DataGridView();
        readonly Dictionary<string,JObject> priceBooks=new Dictionary<string,JObject>();
        readonly List<string> priceIds=new List<string>();
        readonly List<JObject> entryRows=new List<JObject>();
        readonly List<JObject> pendingPrices=new List<JObject>();
        string priceTopic,priceStreamId,priceViewId,selectedPriceSecurity;long priceSequence;bool openingPrice,renderingPrices;

        readonly SemaphoreSlim priceViewGate = new SemaphoreSlim(1, 1);
        bool priceBufferOverflow;
        int pricePageNumber = 1;
        int pricePageSize = 20;
        int priceTotalCount;
        bool priceRefillPending;
        bool priceViewStale;

        void InitializePriceBoard(){
            var bar=Bar();bar.Height=38;bar.Dock=DockStyle.Top;priceBond.Width=125;
            bar.Controls.AddRange(new Control[]{Label("债券"),priceBond,priceMode,priceDepth,priceSettlement});
            AddButton(bar,"应用筛选",OpenPriceView);bar.Controls.Add(priceState);
            AddButton(bar,"清除分组",async()=>{selectedPriceGroup=null;priceGroupState.Text="未应用分组";await OpenPriceView();});bar.Controls.Add(priceGroupState);
            AddButton(bar, "上一批", () => QueryPricePage(pricePageNumber - 1));
            AddButton(bar, "下一批", () => QueryPricePage(pricePageNumber + 1));
            AddButton(bar, "返回最新", () => QueryPricePage(1));
            AddButton(bar, "完整条件", () => EditProductFilter("price"));
            bondGrid.Scroll += async (sender, args) => {
                if (args.ScrollOrientation == ScrollOrientation.VerticalScroll && !renderingPrices
                    && bondGrid.FirstDisplayedScrollingRowIndex >= 0
                    && bondGrid.FirstDisplayedScrollingRowIndex + bondGrid.DisplayedRowCount(false) >= priceIds.Count) {
                    await QueryPricePage(pricePageNumber + 1);
                }
            };
            priceMode.SelectedIndexChanged+=(s,e)=>priceDepth.Enabled=priceMode.SelectedIndex==2;
            ConfigurePriceGrid(bondGrid);ConfigurePriceGrid(entryGrid);
            bondGrid.Columns.Add(Column("securityId","债券代码",110));bondGrid.Columns.Add(Column("securityName","名称",100));
            foreach(var col in new[]{Column("side","方向",60),Column("yield","收益率(%)",90),Column("quantity","数量(万)",90),Column("settlement","交割",70),Column("owner","报价人",90),Column("position","序号",50),Column("quoteId","报价ID",220)})entryGrid.Columns.Add(col);
            bondGrid.CellValueNeeded+=(s,e)=>{if(e.RowIndex>=priceIds.Count)return;string id=priceIds[e.RowIndex];e.Value=e.ColumnIndex==0?id:(string)priceBooks[id]["marketData"]["Symbol"];};
            bondGrid.CurrentCellChanged+=(s,e)=>{if(renderingPrices)return;int row=bondGrid.CurrentCell?.RowIndex??-1;selectedPriceSecurity=row>=0&&row<priceIds.Count?priceIds[row]:null;RenderPriceEntries();};
            string[] entryFields={"MDEntryType","MDEntryPx","MDEntrySize","SettlType","UserID","MDPriceLevel","MDEntryID"};
            entryGrid.CellValueNeeded+=(s,e)=>{
                if(e.RowIndex>=entryRows.Count)return;string value=(string)entryRows[e.RowIndex][entryFields[e.ColumnIndex]];
                if(e.ColumnIndex==0)value=value=="0"?"买入":"卖出";
                if(e.ColumnIndex==3)value=value=="1"?"T+0":value=="2"?"T+1":value;
                if(e.ColumnIndex==6&&value!=null&&value.StartsWith("FICC:"))value=value.Substring(5);e.Value=value;
            };
            entryGrid.CellFormatting+=(s,e)=>{if(e.RowIndex<entryRows.Count)e.CellStyle.ForeColor=(string)entryRows[e.RowIndex]["MDEntryType"]=="0"?Color.DarkRed:Color.DarkGreen;};
            var content=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,270));content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));content.Controls.Add(bondGrid,0,0);
            var detailPanel=new Panel{Dock=DockStyle.Fill};detailPanel.Controls.Add(entryGrid);detailPanel.Controls.Add(priceDetailState);content.Controls.Add(detailPanel,1,0);
            var hint=new Label{Dock=DockStyle.Top,Height=26,Text="仅显示有效报价形成的行情：新建报价需先确认。左侧为匹配债券，右侧为选中债券的买卖盘。"};
            var groupBar=Bar();groupBar.Dock=DockStyle.Top;groupBar.Height=38;
            AddButton(groupBar,"关注债券",()=>ManageGroup("watchlist"));AddButton(groupBar,"筛选分组",()=>ManageGroup("filterGroup"));
            priceBoard.Controls.Add(content);priceBoard.Controls.Add(hint);priceBoard.Controls.Add(bar);priceBoard.Controls.Add(groupBar);boards.TabPages.Add(priceBoard);
        }
        static DataGridViewTextBoxColumn Column(string name,string title,int width)=>new DataGridViewTextBoxColumn{Name=name,HeaderText=title,Width=width,SortMode=DataGridViewColumnSortMode.NotSortable};
        static void ConfigurePriceGrid(DataGridView view){view.Dock=DockStyle.Fill;view.VirtualMode=true;view.ReadOnly=true;view.AllowUserToAddRows=false;view.AllowUserToDeleteRows=false;view.RowHeadersVisible=false;view.MultiSelect=false;view.SelectionMode=DataGridViewSelectionMode.FullRowSelect;view.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.None;}
        async Task OpenPriceView() {
            if (!connected) throw new Exception("请先连接 GW。");
            await priceViewGate.WaitAsync();
            openingPrice = true;
            pendingPrices.Clear();
            priceBufferOverflow = false;
            try {
                if (priceTopic == null) {
                    var stream = await Request("ficc.price.stream.open", new JObject());
                    priceStreamId = (string)stream["streamId"];
                    priceTopic = (string)stream["pushTopic"];
                    await Task.Run(() => client.Subscribe(priceTopic, true));
                }
                if (priceViewId != null) {
                    await Request("ficc.price.view.close", new JObject { ["viewId"] = priceViewId });
                }
                priceViewId = "PV-" + Guid.NewGuid().ToString("N");
                var conditions = (JArray)priceConditions["conditions"].DeepClone();
                if (priceBond.SelectedIndex > 0) {
                    conditions.Add(new JObject { ["field"] = "securityId", ["operator"] = "EQ", ["value"] = priceBond.Text });
                }
                if (priceSettlement.SelectedIndex > 0) {
                    conditions.Add(new JObject { ["field"] = "settlType", ["operator"] = "EQ", ["value"] = priceSettlement.SelectedIndex.ToString() });
                }
                var request = new JObject {
                    ["viewId"] = priceViewId,
                    ["mode"] = new[] { "BEST", "FULL", "DEPTH" }[priceMode.SelectedIndex],
                    ["pageSize"] = pricePageSize,
                    ["filter"] = new JObject { ["conditions"] = conditions }
                };
                if (selectedPriceGroup != null) {
                    var filter = (JObject)request["filter"];
                    filter["market"] = selectedPriceGroup["market"].DeepClone();
                    string key = selectedPriceGroup["watchlistId"] != null ? "watchlistId" : "filterGroupId";
                    filter[key + "s"] = new JArray(selectedPriceGroup[key].DeepClone());
                }
                if (priceMode.SelectedIndex == 2) request["depth"] = (int)priceDepth.Value;
                var result = await Request("ficc.price.view.open", request);
                InstallPricePage(result);
                priceViewStale = false;
            } catch {
                priceViewStale = true;
                throw;
            } finally {
                FinishPriceRequest();
                priceViewGate.Release();
            }
        }

        void InstallPricePage(JObject result) {
            if ((string)result["streamId"] != priceStreamId || (string)result["view"]["viewId"] != priceViewId) {
                throw new InvalidOperationException("Price view reply mismatch");
            }
            priceSequence = (long)result["sequence"];
            pricePageNumber = (int)result["view"]["pageNumber"];
            priceTotalCount = (int)result["view"]["totalCount"];
            priceBooks.Clear();
            priceIds.Clear();
            foreach (JObject book in (JArray)result["books"]) priceBooks[(string)book["securityId"]] = book;
            foreach (string id in ((JArray)result["view"]["ids"]).Values<string>()) priceIds.Add(id);
            RenderPrices();
        }

        void FinishPriceRequest() {
            openingPrice = false;
            if (priceBufferOverflow) priceViewStale = true;
            var buffered = pendingPrices.ToArray();
            pendingPrices.Clear();
            foreach (JObject message in buffered) ReceivePrice(message);
        }

        async Task QueryPricePage(int pageNumber) {
            if (!connected || openingPrice || priceViewId == null || priceViewStale || pageNumber < 1) return;
            if (pageNumber > pricePageNumber && (long)(pageNumber - 1) * pricePageSize >= priceTotalCount) return;
            if (!await priceViewGate.WaitAsync(0)) return;
            openingPrice = true;
            priceBufferOverflow = false;
            pendingPrices.Clear();
            try {
                var result = await Request("ficc.price.view.query", new JObject {
                    ["viewId"] = priceViewId, ["pageNumber"] = pageNumber, ["pageSize"] = pricePageSize
                });
                InstallPricePage(result);
            } catch (Exception error) {
                priceViewStale = true;
                priceState.Text = error.Message;
                Log("PRICE_PAGE " + error.Message);
            } finally {
                FinishPriceRequest();
                priceViewGate.Release();
            }
        }

        async void SchedulePriceRefill() {
            if (priceRefillPending) return;
            priceRefillPending = true;
            string requestedView = priceViewId;
            try {
                await Task.Delay(40);
                while (!IsDisposed && openingPrice) await Task.Delay(20);
                priceRefillPending = false;
                if (!IsDisposed && requestedView == priceViewId && pricePageNumber == 1) await QueryPricePage(1);
            } finally {
                priceRefillPending = false;
            }
        }

        void ReceivePrice(JObject message) {
            if ((string)message["streamId"] != priceStreamId) return;
            if (openingPrice) {
                if (pendingPrices.Count < 200) pendingPrices.Add(message);
                else priceBufferOverflow = true;
                return;
            }
            if (priceViewStale) return;
            long next = (long?)message["sequence"] ?? priceSequence;
            if ((string)message["kind"] == "SNAPSHOT") {
                if (next < priceSequence) return;
                JObject view = ((JArray)message["views"]).OfType<JObject>()
                    .FirstOrDefault(value => (string)value["viewId"] == priceViewId);
                if (view == null) return;
                InstallPricePage(new JObject {
                    ["streamId"] = priceStreamId, ["sequence"] = next, ["view"] = view.DeepClone(),
                    ["books"] = new JArray(((JArray)message["books"]).OfType<JObject>()
                        .Where(book => (string)book["viewId"] == priceViewId).Select(book => book.DeepClone()))
                });
                return;
            }
            if (next <= priceSequence) return;
            if (next != priceSequence + 1 || (string)message["kind"] == "RESET_REQUIRED") {
                priceViewStale = true;
                priceState.Text = "Refresh required: price sequence gap";
                return;
            }
            priceSequence = next;
            if ((string)message["kind"] == "HEARTBEAT") return;
            try {
                if (ClientViewPage.Apply(message, priceViewId, "books", "securityId",
                    pricePageNumber, pricePageSize, priceIds, priceBooks)) SchedulePriceRefill();
                foreach (JObject view in (JArray)message["views"] ?? new JArray()) {
                    if ((string)view["viewId"] == priceViewId) priceTotalCount = (int)view["totalCount"];
                }
                RenderPrices();
            } catch (Exception error) {
                priceViewStale = true;
                priceState.Text = error.Message;
                Log("PRICE_UPDATE " + error.Message);
            }
        }
        void RenderPrices(){
            renderingPrices=true;
            try{
                int index=selectedPriceSecurity==null?-1:priceIds.IndexOf(selectedPriceSecurity);
                if(index<0)index=priceIds.Count>0?0:-1;
                selectedPriceSecurity=index>=0?priceIds[index]:null;
                bondGrid.RowCount=priceIds.Count;
                if(index>=0)bondGrid.CurrentCell=bondGrid[0,index];
                bondGrid.Invalidate();
            }finally{renderingPrices=false;}
            RenderPriceEntries();priceState.Text=priceIds.Count==0?"暂无匹配行情；确认报价后实时显示":priceIds.Count+" 只债券 · 行情序号 "+priceSequence;
        }
        void RenderPriceEntries(){entryRows.Clear();if(selectedPriceSecurity!=null&&priceBooks.TryGetValue(selectedPriceSecurity,out var book))foreach(JObject entry in (JArray)book["marketData"]["NoMDEntries"])entryRows.Add(entry);entryGrid.RowCount=entryRows.Count;entryGrid.Invalidate();priceDetailState.Text=selectedPriceSecurity==null?"未选中债券":selectedPriceSecurity+" · "+entryRows.Count+" 条行情明细";}
        async Task ManageGroup(string kind){
            if(!connected)throw new Exception("请先连接 GW。");
            using(var manager=new GroupManagerForm(kind,Request)){
                manager.ShowDialog(this);
                if(manager.AppliedGroup!=null){selectedPriceGroup=manager.AppliedGroup;priceBond.SelectedIndex=0;priceSettlement.SelectedIndex=0;boards.SelectedTab=priceBoard;}
                if(selectedPriceGroup!=null){
                    string activeKind=selectedPriceGroup["watchlistId"]!=null?"watchlist":"filterGroup",key=activeKind+"Id";
                    var result=await Request("ficc."+activeKind+".query",new JObject{[key]=selectedPriceGroup[key].DeepClone()});
                    selectedPriceGroup=((JArray)result[activeKind=="watchlist"?"watchlists":"filterGroups"]).OfType<JObject>().FirstOrDefault();
                    if(selectedPriceGroup!=null&&(string)selectedPriceGroup["scope"]!="MARKET")selectedPriceGroup=null;
                }
                priceGroupState.Text=selectedPriceGroup==null?"未应用分组":"分组："+(string)selectedPriceGroup["name"];
                await OpenPriceView();
            }
        }
    }
}
