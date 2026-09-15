using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        internal async Task RunCounterpartyUiSmoke(string output){
            automatedUi=true;busy=true;foreach(var button in actions)button.Enabled=false;
            int checks=0;string quoteId=null;
            Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);};
            Func<Task<JObject>> get=async()=> (JObject)(await Request("ficc.quote.get",new JObject{["quoteId"]=quoteId}))["quote"];
            await Connect();
            var institutionRecords = (JArray)(await Request("ficc.base.query", new JObject { ["kind"] = "INSTITUTION", ["limit"] = 50 }))["records"];
            check(institutionRecords.Count >= 2, "local fixture has two selectable institutions");
            var firstInstitution = (JObject)institutionRecords[0];
            var secondInstitution = (JObject)institutionRecords[1];
            string firstInstitutionId = (string)firstInstitution["institutionId"];
            string secondInstitutionId = (string)secondInstitution["institutionId"];
            var contactRecords = (JArray)(await Request("ficc.base.query", new JObject {
                ["kind"] = "CONTACT", ["institutionId"] = secondInstitutionId, ["limit"] = 50
            }))["records"];
            JObject selectedContact = contactRecords.OfType<JObject>().First(record =>
                (string)record["institutionId"] != firstInstitutionId &&
                (record["institutionIds"] as JArray)?.Values<string>().Contains(firstInstitutionId) != true);
            string contactId = (string)selectedContact["contactId"];
            var crmInputs = counterpartyEditor.Controls.OfType<FlowLayoutPanel>().Single().Controls.OfType<ComboBox>().ToArray();
            crmInputs[0].Text = (string)secondInstitution["institutionName"];
            await counterpartyEditor.SearchAsync(false);
            crmInputs[0].SelectedItem = crmInputs[0].Items.Cast<object>().Single(item => item.ToString().EndsWith("[" + secondInstitutionId + "]"));
            crmInputs[0].DroppedDown = false;
            crmInputs[1].Text = (string)selectedContact["contactName"];
            await counterpartyEditor.SearchAsync(true);
            crmInputs[1].SelectedItem = crmInputs[1].Items.Cast<object>().Single(item => item.ToString().EndsWith("[" + contactId + "]"));
            crmInputs[1].DroppedDown = false;
            check((string)counterpartyEditor.Fields()["institutionId"] == secondInstitutionId &&
                (string)counterpartyEditor.Fields()["contactId"] == contactId, "CRM search controls preserve selected IDs, including duplicate names");
            try{
                security.SelectedIndex=0;side.SelectedIndex=0;price.Text="2.4500";quantity.Text="1000";
                counterpartyEditor.LoadFields(new JObject{["institutionName"]="机构简称（待匹配）",["contactName"]="联系人简称（待匹配）"});
                check(CounterpartyEditor.NeedsMatching(counterpartyEditor.Fields()),"free-text names require matching, not automatic ID assignment");
                await Create(1);quoteId=(string)JObject.Parse(detail.Text)["results"][0]["quoteId"];await AwaitQuote(quoteId,"PENDING_CONFIRM");SelectQuote(quoteId);
                var pending=await get();check((string)pending["institutionName"]=="机构简称（待匹配）"&&(string)pending["contactName"]=="联系人简称（待匹配）","names persist on pending quote");
                check(pending["institutionId"]==null||pending["institutionId"].Type==JTokenType.Null,"pending quote accepts missing institution ID");
                check(!PriceHasEntry(quoteId),"pending name-only quote is absent from market prices");
                var rejected=await Request("ficc.quote.confirm",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=quoteId,["expectedRevision"]=pending["revision"].DeepClone()})});
                check((string)rejected["results"][0]["error"]["type"]=="UNRESOLVED_COUNTERPARTY","server rejects confirmation with unresolved names");
                check((string)(await get())["revision"]==(string)pending["revision"]&&(string)(await get())["status"]=="PENDING_CONFIRM","rejected confirmation changes neither revision nor status");
                rejected=await Request("ficc.quote.confirm",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=quoteId,["expectedRevision"]=pending["revision"].DeepClone(),["institutionId"]=firstInstitutionId,["contactId"]=contactId})});
                check((string)rejected["results"][0]["Code"]=="1001","server rejects contact belonging to another institution");
                counterpartyEditor.LoadFields(new JObject{["institutionId"]=secondInstitutionId,["contactId"]=contactId,["institutionName"]=(string)secondInstitution["institutionName"],["contactName"]=(string)selectedContact["contactName"]});
                check(!CounterpartyEditor.NeedsMatching(counterpartyEditor.Fields()),"UI resolves names through explicit reference selection");
                await Command("confirm");await AwaitQuote(quoteId,"ACTIVE");var confirmed=await get();
                check((string)confirmed["quoteId"]==quoteId&&(string)confirmed["institutionId"]==secondInstitutionId&&(string)confirmed["contactId"]==contactId,"confirmation attaches selected IDs to the same quote");
                check((string)confirmed["institutionName"]==(string)secondInstitution["institutionName"]&&(string)confirmed["contactName"]==(string)selectedContact["contactName"],"selected IDs provide canonical display names");
                var history=(JArray)(await Request("ficc.quote.history.query",new JObject{["quoteId"]=quoteId}))["records"];
                check(history.Any(record=>(string)record["after"]?["institutionName"]=="机构简称（待匹配）"),"history retains the original entered names");
                using(var other=new DpsClient()){
                    await Task.Run(()=>other.Connect("trader2"));var publicQuote=(await Task.Run(()=>other.Data("ficc.quote.get",new JObject{["quoteId"]=quoteId})))["quote"];
                    check(new[]{"institutionId","institutionName","contactId","contactName"}.All(field=>publicQuote[field]==null||publicQuote[field].Type==JTokenType.Null),"counterparty names follow the same permissions as IDs");
                }
                rejected=await Request("ficc.quote.update",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=quoteId,["expectedRevision"]=confirmed["revision"].DeepClone(),["institutionId"]=null,["contactId"]=null})});
                check((string)rejected["results"][0]["error"]["type"]=="UNRESOLVED_COUNTERPARTY","active update cannot replace matched counterparties with unresolved names");
                Refresh();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Path.ChangeExtension(output,"png"),ImageFormat.Png);}
            }finally{
                if(quoteId!=null){var quote=await get();if((string)quote["status"]=="ACTIVE"){await Request("ficc.quote.refer",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=quoteId,["expectedRevision"]=quote["revision"].DeepClone()})});quote=await get();}if((string)quote["status"]!="DELETED")await Request("ficc.quote.delete",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=quoteId,["expectedRevision"]=quote["revision"].DeepClone()})});}
            }
            File.WriteAllText(output,new JObject{["result"]="PASS",["checks"]=checks,["quotesCreated"]=1,["messageLogDirectory"]=client.LogDirectory}.ToString());
        }
    }
}
