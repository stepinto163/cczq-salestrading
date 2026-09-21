using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class GroupManagerForm : Form {
        readonly Func<string,JObject,Task<JObject>> request;
        readonly string groupKind;
        readonly ListBox groupList=new ListBox{Dock=DockStyle.Fill};
        readonly TextBox groupName=new TextBox{Width=250};
        readonly ComboBox groupMarket=Choice("利率债","信用债"),groupScope=Choice("行情","历史");
        readonly CheckedListBox bondChoices=new CheckedListBox{Dock=DockStyle.Fill,CheckOnClick=true};
        readonly DataGridView conditionGrid=new DataGridView{Dock=DockStyle.Fill,VirtualMode=true,AllowUserToAddRows=false,AllowUserToDeleteRows=false,RowHeadersVisible=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.None};
        readonly Label feedback=new Label{Dock=DockStyle.Bottom,Height=46,Padding=new Padding(8)};
        readonly Panel editor=new Panel{Dock=DockStyle.Fill};
        readonly List<Button> buttons=new List<Button>();
        readonly List<JObject> securities=new List<JObject>();
        readonly Dictionary<string,JObject> fieldCatalog=new Dictionary<string,JObject>();
        readonly List<object[]> conditionValues=new List<object[]>();
        JObject currentGroup;
        bool loading,busy,dirty,automated;
        public JObject AppliedGroup {get;private set;}
        string IdField=>groupKind+"Id";
        string Market=>groupMarket.SelectedIndex==0?"RATE":"CREDIT";
        string Scope=>groupScope.SelectedIndex==0?"MARKET":"HISTORY";

        sealed class RecordChoice {
            public JObject Record;
            public string Caption;
            public override string ToString()=>Caption;
        }
        static readonly Dictionary<string,string> FieldLabels=new Dictionary<string,string>{
            ["securityId"]="债券代码",["securityName"]="债券名称",["issuerId"]="发行机构代码",["issuerType"]="发行机构类型",
            ["bondRating"]="债项评级",["issuerRating"]="主体评级",["region"]="地域",["remainingYears"]="剩余期限(年)",
            ["side"]="买卖方向",["quoteType"]="报价类型",["yieldType"]="收益率类型",["price"]="报价价格",["quantity"]="数量(万)",
            ["settlType"]="交割方式",["urgent"]="急",["recommended"]="荐",["whole"]="整",["oco"]="OCO",["packing"]="打包"
        };
        public GroupManagerForm(string kind,Func<string,JObject,Task<JObject>> send,bool loadOnShow=true){
            groupKind=kind;request=send;Text=kind=="watchlist"?"关注债券管理":"筛选分组管理";
            Width=1050;Height=620;MinimumSize=new Size(950,560);StartPosition=FormStartPosition.CenterParent;Font=new Font("Microsoft YaHei UI",9F);
            var content=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1};content.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,260));content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            var left=new Panel{Dock=DockStyle.Fill,Padding=new Padding(8)};left.Controls.Add(groupList);
            var listBar=Bar();AddButton(listBar,"新建",()=>{if(CanDiscard())NewGroup();return Task.CompletedTask;});AddButton(listBar,"刷新",async()=>{if(CanDiscard())await RefreshGroups();});left.Controls.Add(listBar);content.Controls.Add(left,0,0);
            var nameBar=Bar();nameBar.Controls.AddRange(new Control[]{Label("名称"),groupName,groupMarket,groupScope});
            var commandBar=Bar();commandBar.Dock=DockStyle.Bottom;AddButton(commandBar,"保存",SaveGroup);AddButton(commandBar,"删除",DeleteGroup);AddButton(commandBar,"应用到行情",ApplyToPrices);
            var hint=new Label{Dock=DockStyle.Top,Height=54,Padding=new Padding(4),Text=kind=="watchlist"?"勾选本组关注的债券。当前为 BaseSvr 模拟数据，共 4 只债券；按利率债／信用债显示。":"同字段条件满足任一项，不同字段条件需同时满足；子组用于整理条件。\r\n包含任一／范围值用逗号分隔；方向 1买/2卖，交割 1当日/2次日，标签用 true/false。"};
            if(kind=="watchlist")editor.Controls.Add(bondChoices);
            else{
                conditionGrid.Columns.Add(new DataGridViewTextBoxColumn{Name="subgroup",HeaderText="子组序号",Width=75,SortMode=DataGridViewColumnSortMode.NotSortable});
                conditionGrid.Columns.Add(new DataGridViewComboBoxColumn{Name="field",HeaderText="筛选字段",Width=180,DisplayMember="Value",ValueMember="Key",FlatStyle=FlatStyle.Flat,SortMode=DataGridViewColumnSortMode.NotSortable});
                conditionGrid.Columns.Add(new DataGridViewComboBoxColumn{Name="operator",HeaderText="条件",Width=140,FlatStyle=FlatStyle.Flat,DisplayMember="Value",ValueMember="Key",DataSource=new[]{Pair("CONTAINS","\u5305\u542b\u6587\u672c"),Pair("EQ","等于"),Pair("IN","包含任一"),Pair("GT","大于"),Pair("GTE","大于等于"),Pair("LT","小于"),Pair("LTE","小于等于"),Pair("BETWEEN","范围（含边界）"),Pair("EXISTS","是否有值")},SortMode=DataGridViewColumnSortMode.NotSortable});
                conditionGrid.Columns.Add(new DataGridViewTextBoxColumn{Name="value",HeaderText="条件值",Width=220,SortMode=DataGridViewColumnSortMode.NotSortable});
                conditionGrid.DataError+=(s,e)=>{e.ThrowException=false;feedback.Text="条件值不在当前字典中，请重新选择。";};
                conditionGrid.CurrentCellDirtyStateChanged+=(s,e)=>{if(conditionGrid.IsCurrentCellDirty)conditionGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);};
                conditionGrid.CellValueChanged+=(s,e)=>Changed();
                conditionGrid.CellValueNeeded+=(s,e)=>{if(e.RowIndex<conditionValues.Count)e.Value=conditionValues[e.RowIndex][e.ColumnIndex];};
                conditionGrid.CellValuePushed+=(s,e)=>{if(e.RowIndex<conditionValues.Count){conditionValues[e.RowIndex][e.ColumnIndex]=e.Value;Changed();}};
                var rowBar=Bar();AddButton(rowBar,"添加条件",()=>{AddCondition();Changed();return Task.CompletedTask;});AddButton(rowBar,"删除选中条件",()=>{if(conditionGrid.CurrentCell!=null){int index=conditionGrid.CurrentCell.RowIndex;conditionValues.RemoveAt(index);conditionGrid.Rows.RemoveAt(index);Changed();}return Task.CompletedTask;});
                editor.Controls.Add(conditionGrid);editor.Controls.Add(rowBar);
            }
            editor.Controls.Add(hint);editor.Controls.Add(nameBar);editor.Controls.Add(commandBar);content.Controls.Add(editor,1,0);Controls.Add(content);Controls.Add(feedback);
            groupList.SelectedIndexChanged+=(s,e)=>{
                if(loading)return;var selected=groupList.SelectedItem as RecordChoice;
                if(!CanDiscard()){SelectListItem((string)currentGroup?[IdField]);return;}
                LoadGroup(selected?.Record);
            };
            groupName.TextChanged+=(s,e)=>Changed();groupScope.SelectedIndexChanged+=(s,e)=>Changed();
            groupMarket.SelectedIndexChanged+=(s,e)=>{if(!loading){RenderBonds(new JArray());Changed();}};
            bondChoices.ItemCheck+=(s,e)=>Changed();
            FormClosing+=(s,e)=>{if(busy||!CanDiscard())e.Cancel=true;};
            if(loadOnShow)Shown+=async(s,e)=>await Execute(LoadData);
        }
        static KeyValuePair<string,string> Pair(string value,string label)=>new KeyValuePair<string,string>(value,label);
        static ComboBox Choice(params string[] values){var control=new ComboBox{DropDownStyle=ComboBoxStyle.DropDownList,Width=90};control.Items.AddRange(values);control.SelectedIndex=0;return control;}
        static Label Label(string text)=>new Label{Text=text,AutoSize=true,Padding=new Padding(0,6,0,0)};
        static FlowLayoutPanel Bar()=>new FlowLayoutPanel{Dock=DockStyle.Top,Height=40,WrapContents=false,Padding=new Padding(4),AutoScroll=true};
        void AddButton(Control parent,string title,Func<Task> action){var button=new Button{Text=title,AutoSize=true};buttons.Add(button);button.Click+=async(s,e)=>await Execute(action);parent.Controls.Add(button);}
        async Task Execute(Func<Task> action){if(busy)return;busy=true;editor.Enabled=false;groupList.Enabled=false;foreach(var button in buttons)button.Enabled=false;try{await action();}catch(Exception error){feedback.Text=error.Message+"；编辑内容已保留。版本冲突时请先刷新。";}finally{busy=false;editor.Enabled=true;groupList.Enabled=true;foreach(var button in buttons)button.Enabled=true;}}
        void Changed(){if(!loading){dirty=true;feedback.Text="有未保存的修改。";}}
        bool CanDiscard()=>!dirty||automated||MessageBox.Show(this,"放弃尚未保存的修改？","未保存的修改",MessageBoxButtons.YesNo,MessageBoxIcon.Question)==DialogResult.Yes;
        async Task<JObject> Call(string operation,JObject payload)=>await request("ficc."+groupKind+"."+operation,payload);
        async Task LoadData(){
            var reference=await request("ficc.base.query",new JObject{["kind"]="SECURITY",["limit"]=100});securities.Clear();securities.AddRange(((JArray)reference["records"]).OfType<JObject>());
            var dictionary=await request("ficc.price.dictionary.query",new JObject());fieldCatalog.Clear();foreach(JObject field in (JArray)dictionary["filterFields"])fieldCatalog[(string)field["field"]]=field;
            if(groupKind=="filterGroup")((DataGridViewComboBoxColumn)conditionGrid.Columns["field"]).DataSource=fieldCatalog.Keys.Select(key=>Pair(key,(string)fieldCatalog[key]["label"] ?? (FieldLabels.TryGetValue(key,out var label)?label:key))).ToArray();
            await RefreshGroups();
        }
        async Task RefreshGroups(string selectId=null){
            var data=await Call("query",new JObject());var records=(JArray)data[groupKind=="watchlist"?"watchlists":"filterGroups"];
            loading=true;try{groupList.Items.Clear();foreach(JObject group in records)groupList.Items.Add(new RecordChoice{Record=group,Caption=(string)group["name"]+" · "+((string)group["market"]=="RATE"?"利率":"信用")+" / "+((string)group["scope"]=="MARKET"?"行情":"历史")});}finally{loading=false;}
            SelectListItem(selectId);var selected=groupList.SelectedItem as RecordChoice;LoadGroup(selected?.Record);
            feedback.Text=records.Count+" 个分组；选择左侧分组编辑，或点击新建。";
        }
        void SelectListItem(string id){loading=true;try{groupList.SelectedIndex=-1;for(int index=0;index<groupList.Items.Count;index++)if((string)((RecordChoice)groupList.Items[index]).Record[IdField]==id){groupList.SelectedIndex=index;break;}}finally{loading=false;}}
        void NewGroup(){SelectListItem(null);LoadGroup(null);groupName.Focus();feedback.Text="新建分组：填写名称并设置内容后保存。";}
        void LoadGroup(JObject group){
            loading=true;try{
                currentGroup=group==null?null:(JObject)group.DeepClone();groupName.Text=(string)group?["name"]??"";groupMarket.SelectedIndex=(string)group?["market"]=="CREDIT"?1:0;groupScope.SelectedIndex=(string)group?["scope"]=="HISTORY"?1:0;
                RenderBonds((JArray)group?["securities"]??new JArray());
                if(groupKind=="filterGroup"){
                    conditionValues.Clear();conditionGrid.Rows.Clear();int subgroup=0;foreach(JObject section in (JArray)group?["subgroups"]??new JArray()){
                        subgroup++;foreach(JObject condition in (JArray)section["conditions"]??new JArray())AddCondition(subgroup,condition,section);
                    }
                }
                dirty=false;
            }finally{loading=false;}
            feedback.Text=group==null?"新建分组": "已加载："+groupName.Text+"；版本 "+group["revision"];
        }
        void RenderBonds(JArray selected){
            bool wasLoading=loading;loading=true;try{bondChoices.Items.Clear();foreach(var bond in securities.Where(b=>(string)b["market"]==Market)){
                string id=(string)bond["securityId"];int index=bondChoices.Items.Add(new RecordChoice{Record=bond,Caption=id+"  "+(string)bond["securityName"]});bondChoices.SetItemChecked(index,selected.Any(s=>(string)s["securityId"]==id));
            }}finally{loading=wasLoading;}
        }
        void AddCondition(int subgroup=1,JObject condition=null,JObject section=null){
            var value=condition?["value"];string text=value is JArray array?string.Join(", ",array.Select(v=>v.ToString())):value?.ToString()??"AAA";
            int index=conditionValues.Count;conditionValues.Add(new object[]{subgroup.ToString(),(string)condition?["field"]??"bondRating",(string)condition?["operator"]??"EQ",text});conditionGrid.RowCount=conditionValues.Count;
            conditionGrid.Rows[index].Tag=new JObject{["condition"]=condition?.DeepClone(),["section"]=section?.DeepClone(),["originalSubgroup"]=subgroup};
        }
        JObject BuildRequest(){
            conditionGrid.EndEdit();if(string.IsNullOrWhiteSpace(groupName.Text))throw new Exception("请输入分组名称");
            var result=new JObject{["name"]=groupName.Text.Trim(),["market"]=Market,["scope"]=Scope};
            if(currentGroup!=null){result[IdField]=currentGroup[IdField].DeepClone();result["expectedRevision"]=currentGroup["revision"].DeepClone();}
            if(groupKind=="watchlist"){
                var selected=new JArray();foreach(RecordChoice choice in bondChoices.CheckedItems)selected.Add(new JObject{["securityId"]=choice.Record["securityId"].DeepClone(),["displayOrder"]=selected.Count});result["securities"]=selected;
            }else{
                var sections=new SortedDictionary<int,JObject>();
                foreach(DataGridViewRow row in conditionGrid.Rows){
                    if(!int.TryParse(Convert.ToString(row.Cells["subgroup"].Value),out int number)||number<1)throw new Exception("子组序号必须是正整数");
                    string field=Convert.ToString(row.Cells["field"].Value),operation=Convert.ToString(row.Cells["operator"].Value),text=Convert.ToString(row.Cells["value"].Value).Trim();
                    if(!fieldCatalog.TryGetValue(field,out var catalog)||!((JArray)catalog["operators"]).Any(value=>(string)value==operation))throw new Exception("该字段不支持所选条件，请重新选择");
                    var original=(JObject)row.Tag;var condition=original?["condition"] as JObject;condition=condition==null?new JObject():(JObject)condition.DeepClone();
                    condition["field"]=field;condition["operator"]=operation;condition["value"]=ParseConditionValue(field,operation,text,(string)catalog["type"]=="decimal");
                    if(!sections.TryGetValue(number,out var section)){
                        var previous=(int?)original?["originalSubgroup"]==number?original?["section"] as JObject:null;
                        section=previous==null?new JObject():(JObject)previous.DeepClone();section["conditions"]=new JArray();sections.Add(number,section);
                    }
                    condition["displayOrder"]=((JArray)section["conditions"]).Count;((JArray)section["conditions"]).Add(condition);
                }
                result["subgroups"]=new JArray(sections.Values);
            }
            return result;
        }
        static JToken ParseConditionValue(string field,string operation,string text,bool numeric){
            if(operation=="EXISTS") {if(!bool.TryParse(text,out bool exists))throw new Exception("是否有值请填写 true 或 false");return exists;}
            Func<string,JToken> parse=value=>{
                value=value.Trim();if(value.Length==0)throw new Exception("条件值不能为空");
                if(numeric){if(!decimal.TryParse(value,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var number))throw new Exception("请输入有效数值，使用小数点");return new JValue(number.ToString(CultureInfo.InvariantCulture));}
                if(new[]{"urgent","recommended","whole","oco","packing","hasExerciseOption","isMunicipal","isCrossMarket","isMortgage","isPerpetual","isSubordinated"}.Contains(field)){if(!bool.TryParse(value,out bool flag))throw new Exception("标签值请填写 true 或 false");return new JValue(flag);}
                return new JValue(value);
            };
            if(operation=="IN"||operation=="BETWEEN"){
                var values=text.Split(new[]{',','，'},StringSplitOptions.None).Select(parse).ToArray();if(operation=="BETWEEN"&&values.Length!=2)throw new Exception("范围需要两个值，用逗号分隔");return new JArray(values);
            }
            return parse(text);
        }
        async Task SaveGroup(){var saved=await Call("save",BuildRequest());currentGroup=(JObject)saved[groupKind];dirty=false;await RefreshGroups((string)currentGroup[IdField]);feedback.Text="已保存："+groupName.Text+"；版本 "+currentGroup["revision"];}
        async Task DeleteGroup(){
            if(currentGroup==null)throw new Exception("请先选择已保存的分组");
            if(!automated&&MessageBox.Show(this,"删除分组“"+(string)currentGroup["name"]+"”？","删除分组",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;
            await Call("delete",new JObject{[IdField]=currentGroup[IdField].DeepClone(),["expectedRevision"]=currentGroup["revision"].DeepClone()});dirty=false;await RefreshGroups();feedback.Text="分组已删除。";
        }
        Task ApplyToPrices(){
            if(currentGroup==null||dirty)throw new Exception("请先保存分组");if((string)currentGroup["scope"]!="MARKET")throw new Exception("历史分组已保存；本按钮仅应用行情分组");
            AppliedGroup=(JObject)currentGroup.DeepClone();busy=false;Close();return Task.CompletedTask;
        }
    }
}
