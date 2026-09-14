using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    static class NotificationSmoke {
        static int checks;
        static void Check(bool ok,string text){if(!ok)throw new Exception(text);checks++;Console.WriteLine("PASS "+text);}
        static JObject Call(DpsClient c,string suffix,JObject r)=>c.Data("ficc.notification."+suffix,r);
        static JArray Query(DpsClient c)=> (JArray)Call(c,"query",new JObject())["notifications"];
        static bool Reason(JToken n,string type)=>((JArray)n["reasons"]).Any(r=>(string)r["type"]==type);
        static JArray Await(DpsClient c,Func<JArray,bool> condition){var end=DateTime.UtcNow.AddSeconds(4);while(DateTime.UtcNow<end){var rows=Query(c);if(condition(rows))return rows;Thread.Sleep(80);}throw new TimeoutException("Reminder condition was not reached within four seconds");}
        static JObject Pref(DpsClient c,JObject values){var current=Call(c,"preference.get",new JObject())["preference"];var req=(JObject)values.DeepClone();req["expectedRevision"]=current["revision"].DeepClone();return Call(c,"preference.save",req);}
        static JObject Quote(DpsClient c,string id)=>(JObject)c.Data("ficc.quote.get",new JObject{["quoteId"]=id})["quote"];
        static void Action(DpsClient c,string op,string id,JObject fields=null){var q=fields??new JObject();q["quoteId"]=id;q["expectedRevision"]=Quote(c,id)["revision"].DeepClone();var result=c.Data("ficc.quote."+op,new JObject{["quotes"]=new JArray(q)})["results"][0];if((string)result["Code"]!="0000")throw new Exception(op+" failed: "+result);}
        static string Create(DpsClient c,string side,string yield,string settl){var r=c.Data("ficc.quote.create",new JObject{["quotes"]=new JArray(new JObject{["clientQuoteId"]="NTEST-"+Guid.NewGuid().ToString("N"),["securityId"]="250002.IB",["side"]=side,["quoteType"]="1",["yieldType"]="1",["price"]=yield,["quantity"]="1000",["settlType"]=settl,["tradeDate"]=DateTime.Now.ToString("yyyy-MM-dd")})})["results"][0];if((string)r["Code"]!="0000")throw new Exception(r.ToString());string id=(string)r["quoteId"];Action(c,"confirm",id);return id;}
        public static int Run(string output){
            try{using(var mine=new DpsClient())using(var other=new DpsClient()){
                mine.Connect("demo");other.Connect("demo2");
                var original=(JObject)Call(mine,"preference.get",new JObject())["preference"];
                string[] keys={"watchlistEnabled","filterGroupEnabled","counterQuoteEnabled","myBestEnabled"};
                var all=new JObject();foreach(string k in keys)all[k]=true;Pref(mine,all);Check(true,"notification preferences saved through independent service");
                string topic=(string)Call(mine,"stream.open",new JObject())["pushTopic"];var pushes=new ConcurrentQueue<JObject>();mine.Message+=(t,m)=>{if(t==topic&&pushes.Count<64)pushes.Enqueue(m);};mine.Subscribe(topic,false);
                string view="NV-"+Guid.NewGuid().ToString("N");Check(Call(mine,"view.open",new JObject{["viewId"]=view,["limit"]=20})["notifications"] is JArray,"notification view returns initial query records");
                Check((int)Call(mine,"view.extend",new JObject{["viewId"]=view,["limit"]=21})["view"]["limit"]==21,"notification scrolling extension");
                var watch=mine.Data("ficc.watchlist.save",new JObject{["name"]="提醒测试关注组",["market"]="RATE",["scope"]="MARKET",["securities"]=new JArray(new JObject{["securityId"]="250002.IB"})})["watchlist"];
                var filter=mine.Data("ficc.filterGroup.save",new JObject{["name"]="提醒测试筛选组",["market"]="RATE",["scope"]="MARKET",["subgroups"]=new JArray(new JObject{["conditions"]=new JArray(new JObject{["field"]="securityId",["operator"]="EQ",["value"]="250002.IB"})})})["filterGroup"];
                string bid=Create(mine,"1","2.5000","1");
                var rows=Await(mine,a=>a.Any(n=>((JArray)n["quoteIds"]).Any(x=>(string)x==bid)&&Reason(n,"WATCHLIST")&&Reason(n,"FILTER_GROUP")&&Reason(n,"MY_BEST")));
                Check(true,"watchlist, filter-group and my-best reasons merge for the same bond");
                string ask=Create(other,"2","1.0000","2");
                rows=Await(mine,a=>a.Any(n=>Reason(n,"COUNTER_QUOTE")&&((JArray)n["quoteIds"]).Any(x=>(string)x==bid)&&((JArray)n["quoteIds"]).Any(x=>(string)x==ask)));
                Check(true,"counter-quote reminder ignores different settlement and non-crossing yields");
                var counter=rows.First(n=>Reason(n,"COUNTER_QUOTE")&&((JArray)n["quoteIds"]).Any(x=>(string)x==bid));
                Check(((JArray)counter["reasons"]).Count==1,"counter quote remains a separate notification");
                var foreign=Call(other,"read",new JObject{["notificationIds"]=new JArray(counter["notificationId"].DeepClone()),["read"]=true});Check((string)foreign["results"][0]["Code"]=="1002","another user cannot mark my notification as read");
                string better=Create(other,"1","2.4000","1");
                Await(mine,a=>a.Where(n=>(string)n["securityId"]=="250002.IB").All(n=>!Reason(n,"MY_BEST")));Check(true,"another user's better bid removes my-best qualification");
                Action(other,"update",better,new JObject{["price"]="2.5000"});Await(mine,a=>a.Any(n=>(string)n["securityId"]=="250002.IB"&&Reason(n,"MY_BEST")));Check(true,"a tied best yield restores my-best qualification");
                rows=Query(mine);var normal=rows.First(n=>(string)n["securityId"]=="250002.IB"&&!Reason(n,"COUNTER_QUOTE"));string nid=(string)normal["notificationId"];
                Check((string)Call(mine,"read",new JObject{["notificationIds"]=new JArray(nid),["read"]=true})["results"][0]["Code"]=="0000","mark notification read");
                Check(!((JArray)Call(mine,"query",new JObject{["readState"]="UNREAD"})["notifications"]).Any(n=>(string)n["notificationId"]==nid),"read notification exits unread query");
                var off=new JObject();foreach(string key in keys)off[key]=false;Pref(mine,off);Check(Query(mine).Count==0,"disabling all preferences removes current reminders");
                Pref(mine,all);rows=Await(mine,a=>a.Any(n=>(string)n["notificationId"]==nid));Check(rows.First(n=>(string)n["notificationId"]==nid)["readTime"].Type!=JTokenType.Null,"read state survives preference off/on");
                var ng=Call(mine,"filterGroup.save",new JObject{["name"]="通知筛选测试",["market"]="RATE",["scope"]="NOTIFICATION",["subgroups"]=new JArray(new JObject{["conditions"]=new JArray(new JObject{["field"]="bondRating",["operator"]="EQ",["value"]="AAA"})})})["filterGroup"];
                Check(((JArray)Call(mine,"filterGroup.query",new JObject())["filterGroups"]).Any(g=>(string)g["filterGroupId"]==(string)ng["filterGroupId"]),"notification filter group saved and queried");
                Check((bool)Call(mine,"filterGroup.delete",new JObject{["filterGroupId"]=ng["filterGroupId"].DeepClone(),["expectedRevision"]=ng["revision"].DeepClone()})["deleted"],"notification filter group deleted");
                using(var search=new DpsClient()){search.Connect("search");var state=Call(search,"state.query",new JObject());Check(state["records"]["notifications"] is JArray,"notification internal state query");Check(Call(search,"event.query",new JObject{["epoch"]=state["epoch"].DeepClone(),["afterSequence"]="0"})["events"] is JArray,"notification events are persisted and replayable");}
                Action(mine,"refer",bid);Await(mine,a=>a.All(n=>!((JArray)n["quoteIds"]).Any(q=>(string)q==bid)));Check(true,"refer removes related quote from current reminders");
                Action(mine,"delete",bid);Action(other,"refer",ask);Action(other,"delete",ask);Action(other,"refer",better);Action(other,"delete",better);
                mine.Data("ficc.watchlist.delete",new JObject{["watchlistId"]=watch["watchlistId"].DeepClone(),["expectedRevision"]=watch["revision"].DeepClone()});mine.Data("ficc.filterGroup.delete",new JObject{["filterGroupId"]=filter["filterGroupId"].DeepClone(),["expectedRevision"]=filter["revision"].DeepClone()});
                var restore=new JObject();foreach(string key in keys)restore[key]=original[key].DeepClone();Pref(mine,restore);
                Check((bool)Call(mine,"view.close",new JObject{["viewId"]=view})["closed"],"notification view closes");
                Check(pushes.Any(p=>(string)p["kind"]=="UPDATE"&&p["notifications"] is JArray)&&pushes.All(p=>p["quotes"]==null&&p["books"]==null),"notification push stays on its own model/channel");
                File.WriteAllText(output,new JObject{["result"]="PASS",["checks"]=checks,["quotesCreated"]=3,["messageLogDirectory"]=mine.LogDirectory}.ToString());return 0;
            }}catch(Exception e){Console.Error.WriteLine(e);File.WriteAllText(output,new JObject{["result"]="FAIL",["checksPassed"]=checks,["error"]=e.ToString(),["messageLogDirectory"]=MessageLog.DirectoryPath}.ToString());return 1;}
        }
    }
}
