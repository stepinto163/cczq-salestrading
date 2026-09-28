using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using com.gateway.log;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace netapi.Api
{
    // Explicit fixtures for host startup only. FICC business calls always use the real gateway.
    public sealed class LocalIntegrationProfile
    {
        private readonly JObject fixtures;
        public bool Enabled { get; private set; }
        public static readonly LocalIntegrationProfile Current = Load();

        public LocalIntegrationProfile(string mode, string fixtureFile)
        {
            if (string.IsNullOrWhiteSpace(mode) || mode == "stc")
            {
                return;
            }
            if (mode != "local")
            {
                throw new ConfigurationErrorsException("Ficc.IntegrationMode must be local or stc.");
            }
            Enabled = true;
            fixtures = JObject.Parse(File.ReadAllText(fixtureFile));
            if ((int?)fixtures["version"] != 1 || !(fixtures["requests"] is JObject)
                || !(fixtures["subscriptions"] is JArray) || !(fixtures["users"] is JObject))
            {
                throw new ConfigurationErrorsException("Invalid local STC fixture format.");
            }
        }

        private static LocalIntegrationProfile Load()
        {
            Stopwatch timer = Stopwatch.StartNew();
            string mode = ConfigurationManager.AppSettings["Ficc.IntegrationMode"] ?? "stc";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "local-stc-fixtures.json");
            LogState("STARTING", mode, path, timer.ElapsedMilliseconds, null);
            try
            {
                LocalIntegrationProfile profile = new LocalIntegrationProfile(mode, path);
                LogState("INITIALIZED", mode, profile.Enabled ? path : "STC", timer.ElapsedMilliseconds, null);
                return profile;
            }
            catch (Exception error)
            {
                LogState("FAILED", mode, path, timer.ElapsedMilliseconds, error);
                throw;
            }
        }

        private static void LogState(string state, string mode, string target, long elapsed, Exception error)
        {
            string message = "COMPONENT " + new JObject
            {
                ["service"] = "STCClient",
                ["component"] = "LOCAL_DEPENDENCIES",
                ["state"] = state,
                ["mode"] = mode,
                ["implementation"] = nameof(LocalIntegrationProfile),
                ["target"] = target,
                ["elapsedMillis"] = elapsed
            }.ToString(Formatting.None);
            Console.WriteLine(message);
            if (error == null)
            {
                LogHelper.Info(message);
            }
            else
            {
                Console.Error.WriteLine(error);
                LogHelper.Error(message, error);
            }
        }

        public void ValidateGateway(string host)
        {
            if (Enabled && !string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
                && (!IPAddress.TryParse(host, out IPAddress address) || !IPAddress.IsLoopback(address)))
            {
                throw new ConfigurationErrorsException("Local integration mode requires a loopback GW address.");
            }
        }

        public bool UsesRealService(string service, string topic)
        {
            if (!Enabled)
            {
                return true;
            }
            bool quote = service == "QuoteSvr" && (topic.StartsWith("ficc.quote.", StringComparison.Ordinal)
                || topic.StartsWith("ficc.price.", StringComparison.Ordinal)
                || topic.StartsWith("ficc.base.", StringComparison.Ordinal)
                || topic.StartsWith("ficc.watchlist.", StringComparison.Ordinal)
                || topic.StartsWith("ficc.filterGroup.", StringComparison.Ordinal));
            bool notification = service == "NotificationSvr"
                && topic.StartsWith("ficc.notification.", StringComparison.Ordinal);
            return quote || notification;
        }

        public T Reply<T>(string service, string topic, object request, string username)
        {
            if (!Enabled)
            {
                throw new InvalidOperationException("Local fixtures are disabled.");
            }
            string key = service + "/" + topic;
            JToken fixture = fixtures["requests"]?[key];
            if (fixture == null)
            {
                string message = "LOCAL_UNSUPPORTED request " + key;
                LogHelper.Error(message);
                throw new NotSupportedException(message);
            }
            JObject reply = (JObject)fixture.DeepClone();
            LogHelper.Info("LOCAL REQ {0} {1}", key, JsonConvert.SerializeObject(request));
            LogHelper.Info("LOCAL REP {0} {1}", key, reply.ToString(Formatting.None));
            return reply.ToObject<T>();
        }

        public void Subscribe(string service, string topic, string username)
        {
            string key = service + "/" + topic;
            bool known = fixtures["subscriptions"].Values<string>()
                .Any(value => value.Replace("{username}", username ?? "") == key);
            if (!known)
            {
                string message = "LOCAL_UNSUPPORTED subscription " + key;
                LogHelper.Error(message);
                throw new NotSupportedException(message);
            }
            // Static fixture metadata has no live changes; this is intentionally not a market-data feed.
            LogHelper.Info("LOCAL SUB {0} (static startup metadata)", key);
        }

        public string CompleteLogin(string response, string username)
        {
            JObject reply = JObject.Parse(response);
            if ((string)reply["Code"] != "0001")
            {
                return response;
            }
            JObject user = fixtures["users"]?[username] as JObject;
            string sid = (string)reply["sid"];
            if (user == null || string.IsNullOrWhiteSpace(sid) || sid == "null"
                || (string)reply["userId"] != (string)user["userId"])
            {
                throw new InvalidOperationException("Local LoginSvr reply does not match the configured test user/session.");
            }
            foreach (JProperty property in user.Properties())
            {
                if (property.Name != "sid" && property.Name != "Code")
                {
                    reply[property.Name] = property.Value.DeepClone();
                }
            }
            reply["userName"] = username;
            reply["ServerTime"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return reply.ToString(Formatting.None);
        }
    }
}
