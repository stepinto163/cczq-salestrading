using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using com.gateway.connector;
using com.gateway.connector.tcp;

namespace Ficc.SalesTrading {
    public sealed class DpsClient : IEventListener, IDisposable {
        readonly GateWayApi api=new GateWayApi();
        readonly Dictionary<string,Listener> listeners=new Dictionary<string,Listener>();
        readonly string clientId=Guid.NewGuid().ToString("N");string account;
        public string LogDirectory=>MessageLog.DirectoryPath;
        public event Action<string,JObject> Message;
        public event Action<string> Trace;
        internal event Action<JObject> RequestMeasured;
        public DpsClient(){api.IsGzip=false;api.ReconnectFlag=false;api.Eventlistener=this;}
        public void Connect(string user, int port = 18600, string host = "127.0.0.1", string password = "trader1") {
            account = user;
            Record("SEND", "CONNECT", "SYS.ATS.LOGIN", new JObject {
                ["host"] = host, ["port"] = port, ["userName"] = user, ["password"] = "[REDACTED]"
            });
            // Credentials come from the login UI; GW selects the configured LoginSvr.
            string result;
            try {
                result = api.connect(host, port, user, password, false, 1, new Dictionary<string, object>());
            } catch (Exception error) {
                Error("SYS.ATS.LOGIN", error);
                throw;
            }
            RecordRaw("RECEIVE", "LOGIN_REPLY", "SYS.ATS.LOGIN", result);
            JObject reply = JObject.Parse(result ?? "{}");
            string code = (string)reply["Code"];
            if (code != "0000" && code != "0001") {
                throw new Exception("Login failed: Code=" + code + ", Msg=" + (string)reply["Msg"]);
            }
            Trace?.Invoke("DPS connected as " + user);
        }
        public static string ServerFor(string topic)=>topic.StartsWith("ficc.notification.")?"NotificationSvr":"QuoteSvr";
        public JObject Request(string topic,JObject request)=>RequestTo(ServerFor(topic),topic,request);
        public JObject RequestTo(string serverName, string topic, JObject request) {
            if (request["requestId"] == null) {
                request["requestId"] = "C-" + Guid.NewGuid().ToString("N");
            }
            Action<JObject> measurement = RequestMeasured;
            Stopwatch stopwatch = measurement == null ? null : Stopwatch.StartNew();
            Record("SEND", "REQUEST", topic, request, serverName);
            Trace?.Invoke("REQ " + topic + " " + request.ToString(Formatting.None));
            double sdkStart = stopwatch?.Elapsed.TotalMilliseconds ?? 0;
            JObject result;
            try {
                result = api.requestSync<JObject>(serverName, topic, request.ToString(Formatting.None), 5000);
            } catch (Exception error) {
                Error(topic, error);
                throw;
            }
            double sdkEnd = stopwatch?.Elapsed.TotalMilliseconds ?? 0;
            if (result == null) {
                var error = new TimeoutException(topic + " timed out");
                Error(topic, error);
                throw error;
            }
            Record("RECEIVE", "REPLY", topic, result, serverName);
            Trace?.Invoke("REP " + topic + " " + result.ToString(Formatting.None));
            if (measurement != null) {
                double completed = stopwatch.Elapsed.TotalMilliseconds;
                measurement(new JObject {
                    ["requestId"] = (string)request["requestId"],
                    ["topic"] = topic,
                    ["code"] = (string)result["Code"],
                    ["clientRequestMs"] = completed,
                    ["sendPreparationMs"] = sdkStart,
                    ["sdkRoundTripMs"] = sdkEnd - sdkStart,
                    ["replyLoggingMs"] = completed - sdkEnd
                });
            }
            return result;
        }
        public JObject Data(string topic,JObject r){var result=Request(topic,r);if((string)result["Code"]!="0000")throw new Exception((string)result["Code"]+": "+(string)result["Msg"]);return (JObject)result["Data"];}
        public void Subscribe(string topic,bool image,string serverName=null){
            serverName=serverName??ServerFor(topic);
            if(listeners.ContainsKey(topic))throw new InvalidOperationException("Already subscribed: "+topic);
            var l=new Listener((t,json)=>{RecordRaw("RECEIVE","PUSH",t,json);try{var o=JObject.Parse(json);Trace?.Invoke("PUSH "+t+" "+o.ToString(Formatting.None));Message?.Invoke(t,o);}catch(Exception e){Error(t,e);Trace?.Invoke("Message error: "+e.Message);}});
            Record("SEND","SUBSCRIBE",topic,new JObject{["snapshot"]=image,["serverName"]=image?serverName:null});
            bool ok;try{ok=image?api.subscribe(serverName,topic,l):api.subscribe(topic,l);}catch(Exception e){Error(topic,e);throw;}
            Record("LOCAL","SDK_SUBSCRIBE_RESULT",topic,new JObject{["acceptedBySdk"]=ok});
            if(!ok)throw new Exception("DPS subscription rejected: "+topic);
            // This DLL treats a nonempty error reply as success. Inspect a normal subscribe
            // acknowledgement explicitly; set registration is idempotent and sends no image.
            Record("SEND","SUBSCRIBE_ACK_REQUEST",topic,new JObject{["serverName"]="gateway",["method"]="subscribe",["content"]=topic});
            var ack=api.requestSync<JObject>("gateway","subscribe",topic,5000);
            Record("RECEIVE","SUBSCRIBE_ACK",topic,ack);
            if(ack==null||(string)ack["Code"]!="0000"){
                Record("SEND","UNSUBSCRIBE",topic,new JObject{["reason"]="subscription rejected"});
                api.unSubscribe(topic,l);
                throw new Exception("DPS subscription rejected: "+(string)ack?["Msg"]);
            }
            listeners.Add(topic,l);
        }
        public void Unsubscribe(string topic){if(listeners.TryGetValue(topic,out var l)){Record("SEND","UNSUBSCRIBE",topic,new JObject());api.unSubscribe(topic,l);listeners.Remove(topic);Record("LOCAL","SDK_UNSUBSCRIBE_COMPLETE",topic,new JObject());}}
        public override void onEvent(int status){Record("LOCAL","CONNECTION_EVENT",null,new JObject{["status"]=status});Trace?.Invoke("DPS connection event "+status);}
        public void Dispose(){foreach(string topic in new List<string>(listeners.Keys))Unsubscribe(topic);Record("SEND","DISCONNECT",null,new JObject());api.disConnect();}
        void Record(string direction,string kind,string topic,JToken body,string serverName=null){MessageLog.Write(clientId,account,direction,kind,topic,body,serverName);}
        void RecordRaw(string direction,string kind,string topic,string raw){JToken body;try{body=raw==null?JValue.CreateNull():JToken.Parse(raw);}catch{body=new JValue(raw);}Record(direction,kind,topic,body);}
        void Error(string topic,Exception error){Record("LOCAL","ERROR",topic,new JObject{["type"]=error.GetType().Name,["message"]=error.Message});}
        sealed class Listener : IMessage {readonly Action<string,string> callback;public Listener(Action<string,string> c){callback=c;}public void onMessage(string topic,string data){callback(topic,data);}}
    }
}
