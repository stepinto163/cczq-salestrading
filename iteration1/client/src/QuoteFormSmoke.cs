using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    public sealed partial class QuoteForm {
        // One real quote through the UI's normal request and push paths. No benchmark.
        internal async Task RunUiSmoke(string resultFile){
            int checks=0;
            automatedUi=true;busy=true;foreach(var button in actions)button.Enabled=false;
            Func<bool,string,Task> check=async(ok,label)=>{if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);state.Text="自动测试 "+checks+" · "+label;await Task.Delay(350);};
            await Connect();await check(connected&&grid.VirtualMode&&entryGrid.VirtualMode,"UI connects with virtual grids enabled");
            security.SelectedIndex=0;side.SelectedIndex=0;settlement.SelectedIndex=0;price.Text="2.5000";quantity.Text="1000";
            priceMode.SelectedIndex=1;await OpenPriceView();
            await Create(1);string id=(string)JObject.Parse(detail.Text)["results"][0]["quoteId"];
            await AwaitQuote(id,"PENDING_CONFIRM");await check(SelectQuote(id),"new quote push inserts a selectable row");
            await Command("confirm");await AwaitQuote(id,"ACTIVE");await check(SelectQuote(id),"confirmation push updates the same UI row");
            string notice=await AwaitNotification(id);suppressEnterMark=true;try{boards.SelectedTab=notificationBoard;}finally{suppressEnterMark=false;}
            await check(notificationGrid.RowCount>0,"reminder grid receives the confirmed quote's notification");
            await MarkNotifications(new[]{notice},true);var readUntil=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<readUntil&&NotificationUnread(notificationCache[notice]))await Task.Delay(50);
            await check(!NotificationUnread(notificationCache[notice]),"reminder read action returns through live notification updates");
            using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Path.ChangeExtension(resultFile,null)+"-notifications.png",ImageFormat.Png);}
            boards.SelectedTab=priceBoard;await AwaitPriceEntry(id,"2.5000");await check(entryRows.Exists(entry=>(string)entry["MDEntryID"]=="FICC:"+id),"price subscription inserts the confirmed quote into the price grid");
            price.Text="2.4800";await Command("update");
            var until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until&&(!cache.ContainsKey(id)||(string)cache[id]["price"]!="2.4800"))await Task.Delay(50);
            await check((string)cache[id]["price"]=="2.4800","updated price reaches the grid cache");
            await AwaitPriceEntry(id,"2.4800");await check(true,"price grid updates from its separate live subscription");
            priceSettlement.SelectedIndex=2;await OpenPriceView();await check(!PriceHasEntry(id),"T+1 price filter excludes the T+0 quote");
            priceSettlement.SelectedIndex=0;priceMode.SelectedIndex=2;priceDepth.Value=2;await OpenPriceView();await check(bondGrid.VirtualMode&&entryGrid.VirtualMode,"depth-two price view opens with virtual grids");
            statusFilter.SelectedIndex=2;await OpenView();await check(SelectQuote(id),"active quote filter retains the row");
            await History();await check(((JArray)JObject.Parse(detail.Text)["records"]).Count==3,"history displayed through UI action");
            await PriceBook();await check(JObject.Parse(detail.Text)["prices"] is JArray,"separate price model displayed through UI action");
            Refresh();using(var bitmap=new Bitmap(Width,Height)){DrawToBitmap(bitmap,new Rectangle(0,0,Width,Height));bitmap.Save(Path.ChangeExtension(resultFile,"png"),ImageFormat.Png);}
            await Command("refer");
            until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until&&ids.Contains(id))await Task.Delay(50);
            await check(!ids.Contains(id),"refer push removes quote from active filter");
            until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until&&PriceHasEntry(id))await Task.Delay(50);
            await check(!PriceHasEntry(id),"refer push removes the quote from the price view");
            until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until&&notificationCache.Values.Any(n=>((JArray)n["quoteIds"]).Any(q=>(string)q==id)))await Task.Delay(50);
            await check(!notificationCache.Values.Any(n=>((JArray)n["quoteIds"]).Any(q=>(string)q==id)),"refer removes the quote from the reminder grid");
            boards.SelectedIndex=0;statusFilter.SelectedIndex=0;await OpenView();await check(SelectQuote(id),"all-status view restores referred quote for management");
            await Command("delete");await AwaitQuote(id,"DELETED");await check(true,"delete push updates management row");
            File.WriteAllText(resultFile,new JObject{["result"]="PASS",["checks"]=checks,["quotesCreated"]=1,["virtualMode"]=grid.VirtualMode,["loadedRows"]=rows.Count,["messageLogDirectory"]=client.LogDirectory,["screenshot"]=Path.ChangeExtension(resultFile,"png")}.ToString());
        }
        bool SelectQuote(string id){int row=rows.FindIndex(q=>(string)q["quoteId"]==id);if(row<0)return false;grid.CurrentCell=grid[0,row];return true;}
        bool PriceHasEntry(string id){foreach(var book in priceBooks.Values)foreach(JObject entry in (JArray)book["marketData"]["NoMDEntries"])if((string)entry["MDEntryID"]=="FICC:"+id)return true;return false;}
        async Task<string> AwaitNotification(string quoteId){var until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until){foreach(var n in notificationCache.Values)if(((JArray)n["quoteIds"]).Any(q=>(string)q==quoteId))return (string)n["notificationId"];await Task.Delay(50);}throw new TimeoutException("Reminder grid did not receive quote notification");}
        async Task AwaitPriceEntry(string id,string value){var until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until){foreach(var pair in priceBooks)foreach(JObject entry in (JArray)pair.Value["marketData"]["NoMDEntries"])if((string)entry["MDEntryID"]=="FICC:"+id&&(string)entry["MDEntryPx"]==value){bondGrid.CurrentCell=bondGrid[0,priceIds.IndexOf(pair.Key)];RenderPriceEntries();return;}await Task.Delay(50);}throw new TimeoutException("Price grid did not receive expected quote/yield");}
        async Task AwaitQuote(string id,string status){var until=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<until){if(cache.TryGetValue(id,out var q)&&(string)q["status"]==status)return;await Task.Delay(50);}throw new TimeoutException("UI did not receive quote state "+status);}
    }
}
