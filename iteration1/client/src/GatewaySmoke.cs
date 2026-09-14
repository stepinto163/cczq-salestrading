using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    static class GatewaySmoke {
        static int checks;
        static readonly HashSet<string> topics=new HashSet<string>();
        static void Track(DpsClient client){client.Trace+=line=>{if(line.StartsWith("REQ "))topics.Add(line.Split(' ')[1]);};}
        static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;Console.WriteLine("PASS "+label);}
        static JObject Event(BlockingCollection<JObject> queue,string kind){var end=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<end){if(queue.TryTake(out var item,100)&&(string)item["kind"]==kind)return item;}throw new TimeoutException("Missing "+kind+" push");}
        public static int Run(string resultFile, int gatewayPort = 18600){
            try{using(var client=new DpsClient()){
                Track(client);
                var quoteEvents=new BlockingCollection<JObject>(64);var prices=new BlockingCollection<JObject>(64);
                client.Connect("demo", gatewayPort);Check(true,"C# DPS DLL connects through GW");
                var stream=client.Data("ficc.quote.stream.open",new JObject());string qt=(string)stream["pushTopic"];
                client.Message+=(t,m)=>{if(t==qt)quoteEvents.TryAdd(m);else if(t.StartsWith("ficc.price.report."))prices.TryAdd(m);};
                client.Subscribe(qt,false);
                Check(!quoteEvents.TryTake(out var unused,150),"quote subscription does not send snapshot");
                var view=client.Data("ficc.quote.view.open",new JObject{["viewId"]="C-"+Guid.NewGuid().ToString("N"),["status"]=new JArray("ACTIVE"),["ownerScope"]="MINE",["limit"]=20});Check(view["quotes"] is JArray,"quote view request returns initial records");
                var ps=client.Data("ficc.price.stream.open",new JObject());client.Subscribe((string)ps["pushTopic"],true);Check(Event(prices,"SNAPSHOT")["books"] is JArray,"price subscription supplies separate snapshot");
                var input=new JObject{["clientQuoteId"]="C#-"+Guid.NewGuid().ToString("N"),["securityId"]="250001.IB",["side"]="1",["quoteType"]="1",["yieldType"]="1",["price"]="2.5000",["quantity"]="1000",["settlType"]="1",["tradeDate"]=DateTime.Now.ToString("yyyy-MM-dd")};
                var created=client.Data("ficc.quote.create",new JObject{["quotes"]=new JArray(input)});var r=created["results"][0];Check((string)r["Code"]=="0000","create persisted via C# -> GW -> QuoteSvr");string id=(string)r["quoteId"];Event(quoteEvents,"UPDATE");
                Func<JObject> get=()=> (JObject)client.Data("ficc.quote.get",new JObject{["quoteId"]=id})["quote"];
                Action<string> action=op=>{var result=client.Data("ficc.quote."+op,new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=id,["expectedRevision"]=get()["revision"].DeepClone()})});Check((string)result["results"][0]["Code"]=="0000",op+" request/reply");Event(quoteEvents,"UPDATE");};
                action("confirm");
                var updated=client.Data("ficc.quote.update",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=id,["expectedRevision"]=get()["revision"].DeepClone(),["price"]="2.4900"})});
                Check((string)updated["results"][0]["Code"]=="0000"&&(string)get()["price"]=="2.4900","update persists changed price");Event(quoteEvents,"UPDATE");
                Check(client.Data("ficc.quote.dictionary.query",new JObject())["dictionaries"]["side"] is JArray,"quote dictionary");
                Check(client.Data("ficc.price.dictionary.query",new JObject())["dictionaries"]["mode"] is JArray,"price dictionary");
                string qv=(string)view["view"]["viewId"];
                Check((int)client.Data("ficc.quote.view.extend",new JObject{["viewId"]=qv,["limit"]=21})["view"]["limit"]==21,"extend quote view for scrolling");
                string pv="P-"+Guid.NewGuid().ToString("N");
                Check(client.Data("ficc.price.view.open",new JObject{["viewId"]=pv,["mode"]="DEPTH",["depth"]=2,["limit"]=2})["books"] is JArray,"separate price view open");
                Check((int)client.Data("ficc.price.view.extend",new JObject{["viewId"]=pv,["limit"]=3})["view"]["limit"]==3,"extend price view");
                var queried=client.Data("ficc.quote.query",new JObject{["filter"]=new JObject{["conditions"]=new JArray(new JObject{["field"]="securityId",["operator"]="EQ",["value"]="250001.IB"})}});Check(((JArray)queried["quotes"]).Count>0,"local SearchSvrClient finds confirmed quote");
                var book=client.Data("ficc.price.query",new JObject{["securityIds"]=new JArray("250001.IB"),["mode"]="BEST"});Check(book["prices"][0]["marketData"]["NoMDEntries"] is JArray,"independent price query/model");
                action("refer");action("unrefer");action("refer");action("delete");
                Check((string)get()["status"]=="DELETED","terminal state is persisted");
                Check(((JArray)client.Data("ficc.quote.history.query",new JObject{["quoteId"]=id})["records"]).Count==7,"continuous lifecycle history");
                string date=DateTime.Now.ToString("yyyy-MM-dd");
                Check(((JArray)client.Data("ficc.quote.history.search",new JObject{["fromDate"]=date,["toDate"]=date})["rows"]).Count>0,"historical database search");
                var cloned=client.Data("ficc.quote.clone",new JObject{["quotes"]=new JArray(new JObject{["sourceQuoteId"]=id,["expectedRevision"]=get()["revision"].DeepClone(),["clientQuoteId"]="CL-"+Guid.NewGuid().ToString("N")})})["results"][0];
                Check((string)cloned["Code"]=="0000"&&(string)cloned["quoteId"]!=id,"clone creates a distinct quote");
                Check((string)client.Data("ficc.quote.delete",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=cloned["quoteId"].DeepClone(),["expectedRevision"]=cloned["revision"].DeepClone()})})["results"][0]["Code"]=="0000","delete pending clone");
                foreach(string kind in new[]{"watchlist","filterGroup"}){
                    var save=new JObject{["name"]="C# functional check",["market"]="RATE",["scope"]="MARKET"};
                    if(kind=="watchlist")save["securities"]=new JArray(new JObject{["securityId"]="250001.IB"});
                    else save["subgroups"]=new JArray(new JObject{["conditions"]=new JArray(new JObject{["field"]="bondRating",["operator"]="EQ",["value"]="AAA"})});
                    var group=client.Data("ficc."+kind+".save",save)[kind];string key=kind+"Id";
                    Check(group[key]!=null,kind+" saved");
                    Check(((JArray)client.Data("ficc."+kind+".query",new JObject{[key]=group[key].DeepClone()})[kind=="watchlist"?"watchlists":"filterGroups"]).Count==1,kind+" queried");
                    Check((bool)client.Data("ficc."+kind+".delete",new JObject{[key]=group[key].DeepClone(),["expectedRevision"]=group["revision"].DeepClone()})["deleted"],kind+" deleted");
                }
                using(var intake=new DpsClient()){
                    Track(intake);intake.Connect("intake", gatewayPort);
                    var item=(JObject)input.DeepClone();item.Remove("clientQuoteId");item["externalQuoteId"]="QT-"+Guid.NewGuid().ToString("N");item["externalRevision"]="1";item["action"]="NEW";item["institutionName"]="QT输入机构";item["contactName"]="QT输入联系人";
                    var req=new JObject{["sourceType"]="QT",["externalMessageId"]="M-"+Guid.NewGuid().ToString("N"),["sourceUserId"]="QT-U-88",["sourceTime"]=DateTimeOffset.Now.ToString("o"),["text"]="250001 买入 2.5000 1000万",["quotes"]=new JArray(item)};
                    var applied=intake.Data("ficc.quote.intake.apply",req);var qr=applied["results"][0];
                    Check((string)qr["Code"]=="0000","QT intake maps source user and persists quote");
                    var pendingIntake=client.Data("ficc.quote.get",new JObject{["quoteId"]=qr["quoteId"].DeepClone()})["quote"];Check((string)pendingIntake["institutionName"]=="QT输入机构"&&(string)pendingIntake["contactName"]=="QT输入联系人"&&(string)pendingIntake["status"]=="PENDING_CONFIRM","QT intake preserves unresolved counterparty names");
                    Check((string)intake.Data("ficc.quote.intake.apply",req)["results"][0]["quoteId"]==(string)qr["quoteId"],"QT intake retry is idempotent");
                    var originals=(JArray)client.Data("ficc.quote.sourceMessage.query",new JObject{["sourceMessageId"]=applied["sourceMessageId"].DeepClone()})["sourceMessages"];
                    Check(originals.Count==1&&(string)originals[0]["text"]==(string)req["text"],"owner queries original Chinese text without encoding loss");
                    Check((string)client.Data("ficc.quote.delete",new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=qr["quoteId"].DeepClone(),["expectedRevision"]=qr["revision"].DeepClone()})})["results"][0]["Code"]=="0000","mapped owner can delete pending QT quote");
                }
                using(var search=new DpsClient()){
                    Track(search);search.Connect("search", gatewayPort);var state=search.Data("ficc.quote.state.query",new JObject{["limit"]=100});
                    Check(state["records"]["quotes"] is JArray,"internal state query");
                    Check(search.Data("ficc.quote.event.query",new JObject{["epoch"]=state["epoch"].DeepClone(),["afterSequence"]="0",["limit"]=100})["events"] is JArray,"internal event replay");
                }
                Check((bool)client.Data("ficc.quote.view.close",new JObject{["viewId"]=qv})["closed"],"close quote view");
                Check((bool)client.Data("ficc.price.view.close",new JObject{["viewId"]=pv})["closed"],"close price view");
                using(var other=new DpsClient()){other.Connect("demo2", gatewayPort);bool denied=false;try{other.Subscribe(qt,false);}catch{denied=true;}Check(denied,"GW rejects another user's private topic");}
                Check(topics.Count==32,"all 32 QuoteSvr request topics exercised");
                File.WriteAllText(resultFile,new JObject{["result"]="PASS",["checks"]=checks,["quotesCreated"]=3,["requestTopics"]=new JArray(topics),["messageLogDirectory"]=client.LogDirectory,["path"]="C# DPS -> GW -> STC QuoteSvr -> PostgreSQL"}.ToString());
                Console.WriteLine("PASS "+checks+" checks; three quotes; no performance load.");return 0;
            }}catch(Exception error){Console.Error.WriteLine(error.ToString());File.WriteAllText(resultFile,new JObject{["result"]="FAIL",["checksPassed"]=checks,["error"]=error.ToString(),["messageLogDirectory"]=MessageLog.DirectoryPath}.ToString());return 1;}
        }
    }
}
