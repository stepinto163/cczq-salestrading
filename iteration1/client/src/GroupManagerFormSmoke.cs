using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class GroupManagerForm {
        // Exercises the editor's actual save/delete paths; creates one group and zero quotes.
        internal async Task<int> RunEditorSmoke(string screenshot,Func<JObject,Task> apply){
            automated=true;int checks=0;string createdId=null;
            Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+groupKind+": "+label);};
            await LoadData();
            try{
                NewGroup();groupName.Text="UI-GROUP-"+Guid.NewGuid().ToString("N");
                if(groupKind=="watchlist"){
                    check(bondChoices.Items.Count==2,"bond choices loaded from base-data API for rate market");
                    bondChoices.SetItemChecked(0,true);bondChoices.SetItemChecked(1,true);
                }else{
                    AddCondition();AddCondition(2,new JObject{["field"]="region",["operator"]="IN",["value"]=new JArray("110000","310000")});
                    check(conditionGrid.VirtualMode&&fieldCatalog.ContainsKey("bondRating"),"condition editor uses server field catalog");
                }
                await SaveGroup();createdId=(string)currentGroup[IdField];
                check(createdId!=null&&(string)currentGroup["revision"]=="1","create returns a persisted group and revision");
                var saved=await Call("query",new JObject{[IdField]=createdId});var records=(JArray)saved[groupKind=="watchlist"?"watchlists":"filterGroups"];
                check(records.Count==1,"saved group is queryable through GW");
                if(groupKind=="watchlist")check(((JArray)currentGroup["securities"]).Count==2,"both checked bonds saved");
                else check(((JArray)currentGroup["subgroups"]).Count==2&&currentGroup["subgroups"][1]["conditions"][0]["value"] is JArray,"subgroups and multi-value conditions preserved");
                groupName.Text+="-edited";
                if(groupKind=="watchlist")bondChoices.SetItemChecked(1,false);
                else{
                    conditionGrid.Rows[1].Cells["field"].Value="remainingYears";conditionGrid.Rows[1].Cells["operator"].Value="BETWEEN";conditionGrid.Rows[1].Cells["value"].Value="2, 3";
                }
                await SaveGroup();check((string)currentGroup["revision"]=="2"&&((string)currentGroup["name"]).EndsWith("-edited"),"editing preserves group ID and advances revision");
                await RefreshGroups(createdId);check((string)currentGroup[IdField]==createdId,"refresh restores the selected saved group");
                if(groupKind=="watchlist")check(bondChoices.CheckedItems.Count==1,"removed bond stays removed after reload");
                else check((string)currentGroup["subgroups"][1]["conditions"][0]["operator"]=="BETWEEN","numeric range reloads into the editor");
                var choice=(JObject)currentGroup.DeepClone();await apply(choice);check(true,"saved group applied through the price view API");
                groupName.Text=" ";bool rejected=false;try{BuildRequest();}catch{rejected=true;}check(rejected,"empty name rejected before saving");LoadGroup(choice);
                if(groupKind=="filterGroup"){
                    conditionGrid.Rows[0].Cells["operator"].Value="GT";rejected=false;try{BuildRequest();}catch{rejected=true;}check(rejected,"unsupported field/operator combination rejected");LoadGroup(choice);
                }
                Refresh();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(screenshot,ImageFormat.Png);}
                await DeleteGroup();check(!groupList.Items.Cast<RecordChoice>().Any(item=>(string)item.Record[IdField]==createdId),"delete removes group from management list");
                check(((JArray)(await Call("query",new JObject{[IdField]=createdId}))[groupKind=="watchlist"?"watchlists":"filterGroups"]).Count==0,"deleted group is absent in backend query");createdId=null;
            }finally{
                if(createdId!=null){var records=(JArray)(await Call("query",new JObject{[IdField]=createdId}))[groupKind=="watchlist"?"watchlists":"filterGroups"];if(records.Count>0)await Call("delete",new JObject{[IdField]=createdId,["expectedRevision"]=records[0]["revision"].DeepClone()});}
                dirty=false;
            }
            return checks;
        }
    }
    public sealed partial class QuoteForm {
        internal async Task RunGroupUiSmoke(string output){
            automatedUi=true;await Connect();int checks=0;
            foreach(string kind in new[]{"watchlist","filterGroup"}){
                using(var manager=new GroupManagerForm(kind,Request,false)){
                    manager.Show(this);
                    checks+=await manager.RunEditorSmoke(Path.ChangeExtension(output,null)+"-"+kind+".png",async group=>{
                        selectedPriceGroup=group;await OpenPriceView();
                        if(kind=="watchlist"){
                            var allowed=((JArray)group["securities"]).Select(b=>(string)b["securityId"]).ToArray();
                            if(priceIds.Any(id=>!allowed.Contains(id)))throw new Exception("Price view ignored watchlist");
                        }
                        // Close the filtered view before deleting its group.
                        selectedPriceGroup=null;await OpenPriceView();
                    });
                    manager.Close();
                }
            }
            File.WriteAllText(output,new JObject{["result"]="PASS",["checks"]=checks,["groupsCreated"]=2,["quotesCreated"]=0,["cleanup"]="DELETED",["messageLogDirectory"]=client.LogDirectory}.ToString());
        }
    }
}
