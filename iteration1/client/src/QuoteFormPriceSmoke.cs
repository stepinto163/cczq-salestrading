using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        // Two temporary quotes, sequentially. Exercises real GW requests and UI push handling.
        internal async Task RunPriceSmoke(string resultFile){
            automatedUi=true;busy=true;foreach(var button in actions)button.Enabled=false;
            var created=new List<string>();int checks=0;
            Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);};
            Func<string,string,Task> change=async(id,op)=>{
                var q=(await Request("ficc.quote.get",new JObject{["quoteId"]=id}))["quote"];
                var input=new JObject{["quoteId"]=id,["expectedRevision"]=q["revision"].DeepClone()};
                if(op=="update")input["price"]="2.4700";
                var result=await Request("ficc.quote."+op,new JObject{["quotes"]=new JArray(input)});
                if((string)result["results"][0]["Code"]!="0000")throw new Exception(result.ToString());
            };
            try{
                await Connect();boards.SelectedTab=priceBoard;priceMode.SelectedIndex=1;await OpenPriceView();
                foreach(string bond in new[]{"250002.IB","102500001.IB"}){
                    var input=new JObject{["clientQuoteId"]="MULTI-"+Guid.NewGuid().ToString("N"),["securityId"]=bond,["side"]="1",["quoteType"]="1",["yieldType"]="1",["price"]="2.4900",["quantity"]="1000",["settlType"]="1",["tradeDate"]=DateTime.Now.ToString("yyyy-MM-dd")};
                    var result=await Request("ficc.quote.create",new JObject{["quotes"]=new JArray(input)});
                    if((string)result["results"][0]["Code"]!="0000")throw new Exception(result.ToString());
                    created.Add((string)result["results"][0]["quoteId"]);
                }
                await OpenPriceView();check(created.All(id=>!PriceHasEntry(id)),"pending quotes do not enter market prices");
                await change(created[0],"confirm");await AwaitPriceEntry(created[0],"2.4900");
                check(selectedPriceSecurity=="250002.IB","first confirmed bond is selectable");
                await change(created[1],"confirm");await WaitPriceUi(()=>PriceHasEntry(created[1]));
                check(priceIds.Contains("250002.IB")&&priceIds.Contains("102500001.IB")&&bondGrid.RowCount==priceIds.Count,"live insert displays both bonds in the bond grid");
                check(selectedPriceSecurity=="250002.IB"&&(string)bondGrid[0,bondGrid.CurrentCell.RowIndex].Value=="250002.IB","live reordering preserves selected security identity");
                check(entryRows.Any(e=>(string)e["MDEntryID"]=="FICC:"+created[0]),"detail grid still shows the selected bond");
                await OpenPriceView();check(priceIds.Contains("250002.IB")&&priceIds.Contains("102500001.IB"),"all-bonds initial reply contains both symbols");
                await change(created[1],"update");await WaitPriceUi(()=>priceBooks.Values.Any(b=>((JArray)b["marketData"]["NoMDEntries"]).Any(e=>(string)e["MDEntryID"]=="FICC:"+created[1]&&(string)e["MDEntryPx"]=="2.4700")));
                check(selectedPriceSecurity=="250002.IB"&&PriceHasEntry(created[0]),"updating another bond preserves selection and the first book");
                await AwaitPriceEntry(created[1],"2.4700");check(entryRows.Any(e=>(string)e["MDEntryID"]=="FICC:"+created[1]),"selecting second bond displays its updated detail rows");
                priceBond.SelectedIndex=2;await OpenPriceView();check(priceIds.SequenceEqual(new[]{"250002.IB"})&&!PriceHasEntry(created[1]),"single-bond filter excludes the other bond");
                priceBond.SelectedIndex=0;await OpenPriceView();check(PriceHasEntry(created[0])&&PriceHasEntry(created[1]),"all-bonds filter restores both books");
                Refresh();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Path.ChangeExtension(resultFile,"png"),ImageFormat.Png);}
                await change(created[1],"refer");await WaitPriceUi(()=>!PriceHasEntry(created[1]));check(PriceHasEntry(created[0]),"referring one quote leaves the other bond's prices intact");
                check(bondGrid.VirtualMode&&entryGrid.VirtualMode,"both grids retain virtualization");
            }finally{
                // Only delete IDs created by this test; never alter the user's quotes.
                foreach(string id in created){var q=(await Request("ficc.quote.get",new JObject{["quoteId"]=id}))["quote"];if((string)q["status"]=="ACTIVE")await change(id,"refer");if((string)q["status"]!="DELETED")await change(id,"delete");}
            }
            File.WriteAllText(resultFile,new JObject{["result"]="PASS",["checks"]=checks,["quotesCreated"]=created.Count,["testQuoteIds"]=new JArray(created),["cleanup"]="DELETED",["messageLogDirectory"]=client.LogDirectory,["screenshot"]=Path.ChangeExtension(resultFile,"png")}.ToString());
        }
        async Task WaitPriceUi(Func<bool> predicate){var until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until){if(predicate())return;await Task.Delay(50);}throw new TimeoutException("Expected multi-bond price update did not reach the UI");}
    }
}
