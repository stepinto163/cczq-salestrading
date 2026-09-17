using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    // Fixed functional scenario: two users, two bonds, four quotes. No concurrent writers.
    static class MultiUserQuoteScenario {
        const string TestQuantity="9876";
        static JObject Filter()=>new JObject{["conditions"]=new JArray(new JObject{["field"]="quantity",["operator"]="EQ",["value"]=TestQuantity},new JObject{["field"]="securityId",["operator"]="IN",["value"]=new JArray("250001.IB","250002.IB")})};
        sealed class QuoteFixture {
            public string Alias,Owner,Bond,Side,Yield,Id,Status="PENDING_CONFIRM",Quantity=TestQuantity;
            public int Revision=1;
        }
        sealed class ViewState {
            public readonly List<string> Ids=new List<string>();
            public readonly Dictionary<string,JObject> Records=new Dictionary<string,JObject>();
        }
        sealed class ScenarioClient : IDisposable {
            public readonly string User;
            public readonly DpsClient Client=new DpsClient();
            public readonly Dictionary<string,ViewState> Views=new Dictionary<string,ViewState>();
            readonly ConcurrentQueue<Tuple<string,JObject>> messages=new ConcurrentQueue<Tuple<string,JObject>>();
            readonly Dictionary<string,long> positions=new Dictionary<string,long>();
            readonly Dictionary<string,string> topics=new Dictionary<string,string>();
            readonly Dictionary<string,string> streamIds=new Dictionary<string,string>();
            bool overflow;
            public ScenarioClient(string user){User=user;Client.Connect(user);Client.Message+=(topic,message)=>{if(messages.Count>=128){overflow=true;return;}messages.Enqueue(Tuple.Create(topic,message));};}
            public void Open(){
                foreach(string domain in new[]{"quote","price"}){
                    var stream=Client.Data("ficc."+domain+".stream.open",new JObject());topics[domain]=(string)stream["pushTopic"];streamIds[domain]=(string)stream["streamId"];positions[domain]=0;Client.Subscribe(topics[domain],domain=="price");
                }
                OpenView("mine","quote",new JObject{["ownerScope"]="MINE",["status"]=new JArray("PENDING_CONFIRM","ACTIVE","REFERRED")});
                OpenView("visible","quote",new JObject{["ownerScope"]="VISIBLE",["status"]=new JArray("ACTIVE")});
                OpenView("full","price",new JObject{["mode"]="FULL"});OpenView("best","price",new JObject{["mode"]="BEST"});
            }
            void OpenView(string id,string domain,JObject request){
                request["viewId"]=id;request["filter"]=Filter();request["pageSize"]=10;
                var reply=Client.Data("ficc."+domain+".view.open",request);positions[domain]=(long)reply["sequence"];
                var view=new ViewState();Views[id]=view;view.Ids.AddRange(((JArray)reply["view"]["ids"]).Select(value=>(string)value));
                foreach(JObject record in (JArray)reply[domain=="quote"?"quotes":"books"])view.Records[(string)record[domain=="quote"?"quoteId":"securityId"]]=record;
            }
            public void Drain(){
                if(overflow)throw new Exception(User+": bounded message queue overflowed");
                while(messages.TryDequeue(out var queued)){
                    string domain=topics.FirstOrDefault(pair=>pair.Value==queued.Item1).Key;if(domain==null)continue;
                    var message=queued.Item2;if((string)message["streamId"]!=streamIds[domain])throw new Exception(User+": unexpected stream identity");
                    long sequence=(long?)message["sequence"]??0;if(sequence<=positions[domain])continue;
                    if(sequence!=positions[domain]+1||(string)message["kind"]=="RESET_REQUIRED")throw new Exception(User+": stream gap/reset at "+sequence);
                    positions[domain]=sequence;
                    foreach(JObject delta in (JArray)message["views"]??new JArray()){
                        string id=(string)delta["viewId"];if(!Views.TryGetValue(id,out var view))continue;
                        ClientViewPage.Apply(message, (string)delta["viewId"], domain == "quote" ? "quotes" : "books",
                            domain == "quote" ? "quoteId" : "securityId", 1, 10, view.Ids, view.Records);
                    }
                    if(domain=="quote")foreach(JObject quote in (JArray)message["quotes"]??new JArray())foreach(string id in new[]{"mine","visible"})Views[id].Records[(string)quote["quoteId"]]=quote;
                    if(domain=="price")foreach(JObject book in (JArray)message["books"]??new JArray())if(Views.TryGetValue((string)book["viewId"],out var view))view.Records[(string)book["securityId"]]=book;
                }
            }
            public void Dispose(){Client.Dispose();}
        }
        sealed class Scenario {
            readonly string output,runId=Guid.NewGuid().ToString("N");
            readonly Dictionary<string,ScenarioClient> clients=new Dictionary<string,ScenarioClient>();
            readonly Dictionary<string, JObject> counterparties = new Dictionary<string, JObject>();
            readonly QuoteFixture[] fixtures={
                new QuoteFixture{Alias="A1",Owner="trader1",Bond="250001.IB",Side="1",Yield="2.5000"},
                new QuoteFixture{Alias="A2",Owner="trader1",Bond="250002.IB",Side="1",Yield="2.7000"},
                new QuoteFixture{Alias="B1",Owner="trader2",Bond="250001.IB",Side="1",Yield="2.4000"},
                new QuoteFixture{Alias="B2",Owner="trader2",Bond="250001.IB",Side="2",Yield="2.6000"}
            };
            readonly JObject report=new JObject{["result"]="RUNNING",["steps"]=new JArray(),["cleanup"]=new JArray()};
            string activeStep;int checks;
            public Scenario(string path){output=Path.GetFullPath(path);Directory.CreateDirectory(Path.GetDirectoryName(output));report["runId"]=runId;report["messageLogDirectory"]=MessageLog.DirectoryPath;report["testQuantity"]=TestQuantity;}
            QuoteFixture Quote(string alias)=>fixtures.Single(quote=>quote.Alias==alias);
            JObject Get(QuoteFixture quote)=>(JObject)clients[quote.Owner].Client.Data("ficc.quote.get",new JObject{["quoteId"]=quote.Id})["quote"];
            string Alias(string id)=>fixtures.FirstOrDefault(quote=>quote.Id==id)?.Alias??"UNEXPECTED:"+id;
            void Save(){report["checks"]=checks;report["quotesCreated"]=fixtures.Count(quote=>quote.Id!=null);report["quoteIds"]=new JObject(fixtures.Where(quote=>quote.Id!=null).Select(quote=>new JProperty(quote.Alias,quote.Id)));File.WriteAllText(output,report.ToString());}
            void Check(bool ok,string text){checks++;if(!ok)throw new Exception(text);}
            void Step(string title,Action action,string full,string best){
                activeStep=title;Console.WriteLine("STEP "+title);action();Checkpoint(title,full,best);
            }
            public int Run(){
                try{
                    foreach(string user in new[]{"trader1","trader2"})clients[user]=new ScenarioClient(user);
                    foreach (var client in clients.Values) {
                        var records = (JArray)client.Client.Data("ficc.base.query", new JObject {
                            ["kind"] = "CONTACT", ["limit"] = 20
                        })["records"];
                        var contact = records.OfType<JObject>().FirstOrDefault(record =>
                            !string.IsNullOrEmpty((string)record["institutionId"]) ||
                            (record["institutionIds"] as JArray)?.Count > 0);
                        if (contact == null) {
                            throw new InvalidOperationException("No linked CRM contact available for " + client.User);
                        }
                        counterparties[client.User] = new JObject {
                            ["institutionId"] = (string)contact["institutionId"] ?? (string)contact["institutionIds"][0],
                            ["contactId"] = (string)contact["contactId"]
                        };
                    }
                    // Refuse conflicting live fixtures instead of changing existing user data.
                    foreach(var client in clients.Values){var found=(JArray)client.Client.Data("ficc.quote.query",new JObject{["ownerScope"]="VISIBLE",["status"]=new JArray("ACTIVE","PENDING_CONFIRM","REFERRED"),["filter"]=Filter(),["limit"]=10})["quotes"];Check(found.Count==0,"preflight "+client.User+": no existing live quotes in test filter");client.Open();}
                    Step("01 Create A1,A2,B1,B2 as pending",()=>{foreach(var quote in fixtures)Create(quote);},"","");
                    var hidden=clients["trader2"].Client.Request("ficc.quote.get",new JObject{["quoteId"]=Quote("A1").Id});Check((string)hidden["Code"]=="1002","Bob cannot read Alice's pending A1");
                    Step("02 Alice confirms A1",()=>Write("A1","confirm"),"250001.IB:A1","250001.IB:A1");
                    Step("03 Bob confirms B1 and B2",()=>{Write("B1","confirm");Write("B2","confirm");},"250001.IB:B1,A1,B2","250001.IB:B1,B2");
                    Step("04 Alice confirms A2 on the second bond",()=>Write("A2","confirm"),"250001.IB:B1,A1,B2|250002.IB:A2","250001.IB:B1,B2|250002.IB:A2");
                    Step("05 Alice cannot modify Bob's B1",()=>Write("B1","update",new JObject{["price"]="1.0000"},"trader1","1003"),"250001.IB:B1,A1,B2|250002.IB:A2","250001.IB:B1,B2|250002.IB:A2");
                    Step("06 Alice matches Bob's best bid",()=>Write("A1","update",new JObject{["price"]="2.4000"}),"250001.IB:A1,B1,B2|250002.IB:A2","250001.IB:A1,B1,B2|250002.IB:A2");
                    Step("07 Bob improves B1 to 2.3000",()=>Write("B1","update",new JObject{["price"]="2.3000"}),"250001.IB:B1,A1,B2|250002.IB:A2","250001.IB:B1,B2|250002.IB:A2");
                    Step("08 Reject Bob's stale revision 2",()=>Write("B1","update",new JObject{["price"]="1.0000",["expectedRevision"]="2"},null,"1004"),"250001.IB:B1,A1,B2|250002.IB:A2","250001.IB:B1,B2|250002.IB:A2");
                    Step("09 Bob withdraws B1; Alice becomes best",()=>Write("B1","refer"),"250001.IB:A1,B2|250002.IB:A2","250001.IB:A1,B2|250002.IB:A2");
                    Step("10 Bob restores the same B1",()=>Write("B1","unrefer"),"250001.IB:B1,A1,B2|250002.IB:A2","250001.IB:B1,B2|250002.IB:A2");
                    Step("11 Alice withdraws A2; only its bond disappears",()=>Write("A2","refer"),"250001.IB:B1,A1,B2","250001.IB:B1,B2");
                    Step("12 Alice deletes A2",()=>Write("A2","delete"),"250001.IB:B1,A1,B2","250001.IB:B1,B2");
                    Step("13 A1 quantity changes and leaves the filter",()=>Write("A1","update",new JObject{["quantity"]="9877"}),"250001.IB:B1,B2","250001.IB:B1,B2");
                    Step("14 A1 returns to the filter",()=>Write("A1","update",new JObject{["quantity"]=TestQuantity},null,"0000",runId+"-retry"),"250001.IB:B1,A1,B2","250001.IB:B1,B2");
                    Step("15 Repeat the identical request without another revision",()=>{
                        var quote=Quote("A1");var result=clients[quote.Owner].Client.Data("ficc.quote.update",new JObject{["requestId"]=runId+"-retry",["quotes"]=new JArray(new JObject{["quoteId"]=quote.Id,["expectedRevision"]=(quote.Revision-1).ToString(),["quantity"]=TestQuantity})});
                        Check((string)result["results"][0]["revision"]==quote.Revision.ToString(),"retry returns the original revision");
                    },"250001.IB:B1,A1,B2","250001.IB:B1,B2");
                    foreach(var quote in fixtures){var history=(JArray)clients[quote.Owner].Client.Data("ficc.quote.history.query",new JObject{["quoteId"]=quote.Id})["records"];Check(history.Count==quote.Revision,"history count matches committed revisions for "+quote.Alias);}
                    report["result"]="PASS";
                }catch(Exception error){report["result"]="FAIL";report["failedStep"]=activeStep;report["error"]=error.ToString();var last=((JArray)report["steps"]).LastOrDefault() as JObject;if(last!=null&&(string)last["step"]==activeStep){last["result"]="FAIL";last["error"]=error.Message;}Console.Error.WriteLine(error.Message);}
                finally{
                    Cleanup();Save();foreach(var client in clients.Values)client.Dispose();
                }
                Console.WriteLine("RESULT "+report["result"]+"; "+checks+" checks; "+fixtures.Count(quote=>quote.Id!=null)+" quotes; "+output);
                return (string)report["result"]=="PASS"?0:1;
            }
            void Create(QuoteFixture quote) {
                var input = new JObject {
                    ["clientQuoteId"] = runId + "-" + quote.Alias,
                    ["securityId"] = quote.Bond,
                    ["side"] = quote.Side,
                    ["quoteType"] = "1",
                    ["yieldType"] = "1",
                    ["price"] = quote.Yield,
                    ["quantity"] = TestQuantity,
                    ["settlType"] = "1",
                    ["tradeDate"] = DateTime.Now.ToString("yyyy-MM-dd"),
                    ["institutionId"] = counterparties[quote.Owner]["institutionId"].DeepClone(),
                    ["contactId"] = counterparties[quote.Owner]["contactId"].DeepClone()
                };
                var result = clients[quote.Owner].Client.Data("ficc.quote.create", new JObject {
                    ["quotes"] = new JArray(input)
                })["results"][0];
                Check((string)result["Code"] == "0000", "create " + quote.Alias + ": " + (string)result["Msg"]);
                quote.Id = (string)result["quoteId"];
                Save();
            }
            void Write(string alias,string operation,JObject fields=null,string caller=null,string expectedCode="0000",string requestId=null){
                var quote=Quote(alias);var input=fields==null?new JObject():(JObject)fields.DeepClone();input["quoteId"]=quote.Id;if(input["expectedRevision"]==null)input["expectedRevision"]=quote.Revision.ToString();
                var request=new JObject{["quotes"]=new JArray(input)};if(requestId!=null)request["requestId"]=requestId;
                var result=clients[caller??quote.Owner].Client.Data("ficc.quote."+operation,request)["results"][0];Check((string)result["Code"]==expectedCode,(caller??quote.Owner)+" "+operation+" "+alias+" returns "+expectedCode);
                if(expectedCode!="0000")return;
                quote.Revision++;Check((string)result["revision"]==quote.Revision.ToString(),alias+" advances exactly one revision");
                if(operation=="confirm"||operation=="unrefer")quote.Status="ACTIVE";else if(operation=="refer")quote.Status="REFERRED";else if(operation=="delete")quote.Status="DELETED";
                if(fields?["price"]!=null)quote.Yield=(string)fields["price"];if(fields?["quantity"]!=null)quote.Quantity=(string)fields["quantity"];
            }
            JObject Expected(string full,string best){
                var expected=new JObject();foreach(var client in clients.Values){
                    var mine=new JObject();var visible=new JObject();
                    foreach(var quote in fixtures.Where(quote=>quote.Id!=null&&quote.Quantity==TestQuantity)){
                        if(quote.Owner==client.User&&quote.Status!="DELETED")mine[quote.Alias]=quote.Status+"/"+quote.Revision+"/"+quote.Yield;
                        if(quote.Status=="ACTIVE")visible[quote.Alias]=quote.Status+"/"+quote.Revision+"/"+quote.Yield;
                    }
                    expected[client.User]=new JObject{["mine"]=mine,["visible"]=visible,["full"]=full,["best"]=best};
                }return expected;
            }
            JObject Actual(){
                var result=new JObject();foreach(var client in clients.Values){client.Drain();var state=new JObject();
                    foreach(string viewName in new[]{"mine","visible"}){var view=client.Views[viewName];var records=new JObject();foreach(string id in view.Ids.OrderBy(Alias)){var quote=view.Records[id];records[Alias(id)]=(string)quote["status"]+"/"+(string)quote["revision"]+"/"+(string)quote["price"];}state[viewName]=records;}
                    foreach(string viewName in new[]{"full","best"}){var view=client.Views[viewName];state[viewName]=string.Join("|",view.Ids.OrderBy(id=>id).Select(id=>id+":"+string.Join(",",((JArray)view.Records[id]["marketData"]["NoMDEntries"]).Select(entry=>Alias(((string)entry["MDEntryID"]).Substring(5))))));}
                    result[client.User]=state;
                }return result;
            }
            void Checkpoint(string title,string full,string best){
                var expected=Expected(full,best);JObject actual=null;var timer=Stopwatch.StartNew();
                do{actual=Actual();if(JToken.DeepEquals(expected,actual))break;Thread.Sleep(25);}while(timer.ElapsedMilliseconds<4000);
                var step=new JObject{["step"]=title,["expected"]=expected,["actual"]=actual,["result"]="CHECKING"};((JArray)report["steps"]).Add(step);Save();
                Check(JToken.DeepEquals(expected,actual),title+": both subscribers match expected quote and price views");
                foreach(var client in clients.Values){
                    foreach(string id in client.Views["visible"].Ids){var quote=client.Views["visible"].Records[id];var fixture=fixtures.Single(value=>value.Id==id);
                        bool owner=fixture.Owner==client.User;Check(new[]{"institutionId","institutionName","contactId","contactName"}.All(field=>owner?!string.IsNullOrEmpty((string)quote[field]):quote[field]==null||quote[field].Type==JTokenType.Null),client.User+" sees permitted counterparty fields for "+fixture.Alias);
                    }
                    foreach(string id in client.Views["full"].Ids)foreach(JObject entry in (JArray)client.Views["full"].Records[id]["marketData"]["NoMDEntries"]){var quote=Quote(Alias(((string)entry["MDEntryID"]).Substring(5)));Check((string)entry["MDEntryPx"]==quote.Yield&&(string)entry["MDEntryType"]==(quote.Side=="1"?"0":"1")&&(string)entry["UserID"]==(quote.Owner=="trader1"?"U-001":"U-002")&&(string)entry["MDEntrySize"]==quote.Quantity&&(string)client.Views["full"].Records[id]["quoteRefs"][(string)entry["MDEntryID"]]["quoteRevision"]==quote.Revision.ToString(),client.User+" receives correct price/side/owner/quantity/revision for "+quote.Alias);}
                }
                foreach(var quote in fixtures.Where(quote=>quote.Id!=null)){var current=Get(quote);Check((string)current["status"]==quote.Status&&(string)current["revision"]==quote.Revision.ToString()&&(string)current["price"]==quote.Yield,title+": persisted "+quote.Alias+" agrees");}
                step["result"]="PASS";Console.WriteLine("  EXPECT FULL "+(full.Length==0?"empty":full));Console.WriteLine("  ACTUAL trader1="+actual["trader1"]["full"]+"; trader2="+actual["trader2"]["full"]);Console.WriteLine("  PASS both users; BEST="+(best.Length==0?"empty":best));Save();
            }
            void Cleanup(){
                foreach(var quote in fixtures.Where(quote=>quote.Id!=null)){
                    try{
                        var current=Get(quote);var client=clients[quote.Owner].Client;
                        foreach(string operation in new[]{"refer","delete"}){
                            string status=(string)current["status"];if(operation=="refer"&&status!="ACTIVE"||operation=="delete"&&status=="DELETED")continue;
                            var result=client.Data("ficc.quote."+operation,new JObject{["quotes"]=new JArray(new JObject{["quoteId"]=quote.Id,["expectedRevision"]=current["revision"].DeepClone()})})["results"][0];
                            if((string)result["Code"]!="0000")throw new Exception(result.ToString());current=Get(quote);
                        }
                        ((JArray)report["cleanup"]).Add(new JObject{["alias"]=quote.Alias,["status"]=current["status"].DeepClone()});
                        Console.WriteLine("CLEANUP "+quote.Alias+" -> "+current["status"]);
                    }catch(Exception error){report["result"]="FAIL";((JArray)report["cleanup"]).Add(new JObject{["alias"]=quote.Alias,["error"]=error.Message});}
                    Save();
                }
            }
        }
        public static int Run(string output){
            using(var single=new Semaphore(1,1,@"Local\FiccSalesTradingClient.MultiUserScenario")){
                if(!single.WaitOne(0)){File.WriteAllText(output,new JObject{["result"]="FAIL",["error"]="Another multi-user scenario is running."}.ToString());return 1;}
                try{return new Scenario(output).Run();}finally{single.Release();}
            }
        }
    }
}
