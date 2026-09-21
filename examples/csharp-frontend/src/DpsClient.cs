using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using com.gateway.connector;
using com.gateway.connector.tcp;

namespace Ficc.SalesTrading
{
    public sealed class DpsClient : IEventListener, IDisposable
    {
        readonly GateWayApi api = new GateWayApi();
        readonly Dictionary<string, Listener> listeners = new Dictionary<string, Listener>();
        readonly string clientId = Guid.NewGuid().ToString("N"); string account;
        public string LogDirectory => MessageLog.DirectoryPath;
        public event Action<string, JObject> Message;
        public event Action<string> Trace;
        public bool LogHeartbeats
        {
            get => logHeartbeats;
            set => logHeartbeats = value;
        }
        volatile bool logHeartbeats;
        internal event Action<JObject> RequestMeasured;
        public DpsClient() { api.IsGzip = false; api.ReconnectFlag = false; api.Eventlistener = this; }
        public void Connect(string user, int port = 18600, string host = "127.0.0.1", string password = "trader1")
        {
            account = user;
            Record("SEND", "CONNECT", "SYS.ATS.LOGIN", new JObject
            {
                ["host"] = host,
                ["port"] = port,
                ["userName"] = user,
                ["password"] = "[REDACTED]"
            });
            // Credentials come from the login UI; GW selects the configured LoginSvr.
            string result;
            try
            {
                result = api.connect(host, port, user, password, false, 1, new Dictionary<string, object>());
            }
            catch (Exception error)
            {
                Error("SYS.ATS.LOGIN", error);
                throw;
            }
            RecordRaw("RECEIVE", "LOGIN_REPLY", "SYS.ATS.LOGIN", result);
            JObject reply = JObject.Parse(result ?? "{}");
            try
            {
                ValidateLoginReply(reply);
            }
            catch (Exception error)
            {
                Error("SYS.ATS.LOGIN", error);
                try
                {
                    api.disConnect();
                }
                catch (Exception disconnectError)
                {
                    Record("LOCAL", "DISCONNECT_FAILED", "SYS.ATS.LOGIN",
                        new JObject { ["type"] = disconnectError.GetType().Name });
                }
                throw;
            }
            Trace?.Invoke("DPS connected as " + user);
        }

        internal static void ValidateLoginReply(JObject reply)
        {
            string code = (string)reply?["Code"];
            if (code != "0000" && code != "0001")
            {
                throw new InvalidOperationException("Login failed: Code=" + code + ", Msg=" + (string)reply?["Msg"]);
            }
            string sessionId = (string)reply["sid"];
            if (string.IsNullOrWhiteSpace(sessionId)
                || string.Equals(sessionId.Trim(), "null", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "LoginSvr returned success without a valid session ID (sid). "
                    + "Use a registered STC business account; a sessionless default login cannot access quote operations.");
            }
        }
        public static string ServerFor(string topic) => topic.StartsWith("ficc.notification.") ? "NotificationSvr" : "QuoteSvr";
        public JObject Request(string topic, JObject request) => RequestTo(ServerFor(topic), topic, request);
        public JObject RequestTo(string serverName, string topic, JObject request)
        {
            if (request["requestId"] == null)
            {
                request["requestId"] = "C-" + Guid.NewGuid().ToString("N");
            }
            Action<JObject> measurement = RequestMeasured;
            Stopwatch stopwatch = measurement == null ? null : Stopwatch.StartNew();
            Record("SEND", "REQUEST", topic, request, serverName);
            Trace?.Invoke("REQ " + topic + " " + request.ToString(Formatting.None));
            double sdkStart = stopwatch?.Elapsed.TotalMilliseconds ?? 0;
            JObject result;
            try
            {
                result = api.requestSync<JObject>(serverName, topic, request.ToString(Formatting.None), 5000);
            }
            catch (Exception error)
            {
                Error(topic, error);
                throw;
            }
            double sdkEnd = stopwatch?.Elapsed.TotalMilliseconds ?? 0;
            if (result == null)
            {
                var error = new TimeoutException(topic + " timed out");
                Error(topic, error);
                throw error;
            }
            Record("RECEIVE", "REPLY", topic, result, serverName);
            Trace?.Invoke("REP " + topic + " " + result.ToString(Formatting.None));
            if (measurement != null)
            {
                double completed = stopwatch.Elapsed.TotalMilliseconds;
                measurement(new JObject
                {
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
        public JObject Data(string topic, JObject r) { var result = Request(topic, r); if ((string)result["Code"] != "0000") throw new Exception((string)result["Code"] + ": " + (string)result["Msg"]); return (JObject)result["Data"]; }
        public void Subscribe(string topic, bool image, string serverName = null)
        {
            serverName = serverName ?? ServerFor(topic);
            if (listeners.ContainsKey(topic))
            {
                throw new InvalidOperationException("Already subscribed: " + topic);
            }
            var listener = new Listener(ReceivePush);
            bool acceptedBySdk = false;
            SubscriptionLog("SEND", "SUBSCRIBE", topic,
                new JObject { ["snapshot"] = image, ["serverName"] = image ? serverName : "gateway" });
            try
            {
                acceptedBySdk = image ? api.subscribe(serverName, topic, listener) : api.subscribe(topic, listener);
                SubscriptionLog("LOCAL", "SDK_SUBSCRIBE_RESULT", topic,
                    new JObject { ["acceptedBySdk"] = acceptedBySdk });
                if (!acceptedBySdk)
                {
                    throw new InvalidOperationException("DPS subscription rejected: " + topic);
                }
                // SDK acceptance alone does not prove GW accepted the subscription.
                SubscriptionLog("SEND", "SUBSCRIBE_ACK_REQUEST", topic,
                    new JObject { ["serverName"] = "gateway", ["method"] = "subscribe", ["content"] = topic });
                var acknowledgement = api.requestSync<JObject>("gateway", "subscribe", topic, 5000);
                SubscriptionLog("RECEIVE", "SUBSCRIBE_ACK", topic, acknowledgement);
                if (acknowledgement == null || (string)acknowledgement["Code"] != "0000")
                {
                    throw new InvalidOperationException("DPS subscription rejected: " + (string)acknowledgement?["Msg"]);
                }
                listeners.Add(topic, listener);
            }
            catch (Exception error)
            {
                SubscriptionLog("LOCAL", "SUBSCRIBE_FAILED", topic,
                    new JObject { ["type"] = error.GetType().Name, ["message"] = error.Message });
                if (acceptedBySdk)
                {
                    try
                    {
                        SubscriptionLog("SEND", "UNSUBSCRIBE", topic, new JObject { ["reason"] = "subscription failed" });
                        api.unSubscribe(topic, listener);
                        SubscriptionLog("LOCAL", "SDK_UNSUBSCRIBE_COMPLETE", topic, new JObject());
                    }
                    catch (Exception cleanupError)
                    {
                        SubscriptionLog("LOCAL", "UNSUBSCRIBE_FAILED", topic,
                            new JObject { ["type"] = cleanupError.GetType().Name, ["message"] = cleanupError.Message });
                    }
                }
                throw;
            }
        }
        internal void ReceivePush(string topic, string json)
        {
            JObject message;
            try
            {
                message = JObject.Parse(json);
            }
            catch (Exception error)
            {
                RecordRaw("RECEIVE", "PUSH", topic, json);
                Error(topic, error);
                Trace?.Invoke("Message error: " + error.Message);
                return;
            }
            if ((string)message["kind"] != "HEARTBEAT" || LogHeartbeats)
            {
                Record("RECEIVE", "PUSH", topic, message);
                Trace?.Invoke("PUSH " + topic + " " + message.ToString(Formatting.None));
            }
            try
            {
                Message?.Invoke(topic, message);
            }
            catch (Exception error)
            {
                Error(topic, error);
                Trace?.Invoke("Message error: " + error.Message);
            }
        }
        public void Unsubscribe(string topic)
        {
            if (!listeners.TryGetValue(topic, out var listener)) return;
            SubscriptionLog("SEND", "UNSUBSCRIBE", topic, new JObject());
            try
            {
                api.unSubscribe(topic, listener);
                listeners.Remove(topic);
                SubscriptionLog("LOCAL", "SDK_UNSUBSCRIBE_COMPLETE", topic, new JObject());
            }
            catch (Exception error)
            {
                SubscriptionLog("LOCAL", "UNSUBSCRIBE_FAILED", topic,
                    new JObject { ["type"] = error.GetType().Name, ["message"] = error.Message });
                throw;
            }
        }

        private void SubscriptionLog(string direction, string operation, string topic, JToken body)
        {
            Record(direction, operation, topic, body);
            Trace?.Invoke(operation + " " + topic + " " + (body?.ToString(Formatting.None) ?? "null"));
        }
        public override void onEvent(int status) { Record("LOCAL", "CONNECTION_EVENT", null, new JObject { ["status"] = status }); Trace?.Invoke("DPS connection event " + status); }
        public void Dispose() { foreach (string topic in new List<string>(listeners.Keys)) Unsubscribe(topic); Record("SEND", "DISCONNECT", null, new JObject()); api.disConnect(); }
        void Record(string direction, string kind, string topic, JToken body, string serverName = null) { MessageLog.Write(clientId, account, direction, kind, topic, body, serverName); }
        void RecordRaw(string direction, string kind, string topic, string raw) { JToken body; try { body = raw == null ? JValue.CreateNull() : JToken.Parse(raw); } catch { body = new JValue(raw); } Record(direction, kind, topic, body); }
        void Error(string topic, Exception error) { Record("LOCAL", "ERROR", topic, new JObject { ["type"] = error.GetType().Name, ["message"] = error.Message }); }
        sealed class Listener : IMessage { readonly Action<string, string> callback; public Listener(Action<string, string> c) { callback = c; } public void onMessage(string topic, string data) { callback(topic, data); } }
    }
}
