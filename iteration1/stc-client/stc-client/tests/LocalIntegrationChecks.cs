using System;
using System.IO;
using netapi.Api;
using Newtonsoft.Json.Linq;

internal static class LocalIntegrationChecks
{
    private static int passed;

    private static void Check(bool condition, string name)
    {
        if (!condition)
        {
            throw new Exception(name);
        }
        Console.WriteLine("PASS " + name);
        passed++;
    }

    private static void Reject(Action action, string name)
    {
        bool rejected = false;
        try
        {
            action();
        }
        catch (Exception)
        {
            rejected = true;
        }
        Check(rejected, name);
    }

    public static int Main(string[] args)
    {
        try
        {
            LocalIntegrationProfile local = LocalIntegrationProfile.Current;
            Check(local.Enabled, "explicit local mode");
            local.ValidateGateway("127.0.0.1");
            Reject(() => local.ValidateGateway("10.130.21.53"), "local fixtures cannot connect to a remote GW");
            Reject(() => new LocalIntegrationProfile("typo", "unused"), "invalid mode fails");
            Reject(() => new LocalIntegrationProfile("local", "missing-fixture.json"), "missing fixture fails");
            Check(local.UsesRealService("QuoteSvr", "ficc.quote.create"), "quotes go to real QuoteSvr");
            Check(local.UsesRealService("QuoteSvr", "ficc.price.stream.open"), "prices go to real QuoteSvr");
            Check(local.UsesRealService("NotificationSvr", "ficc.notification.preference.get"), "notifications go to real NotificationSvr");
            Check(!local.UsesRealService("AdminSvr", "ficc.quote.create"), "wrong service cannot pass through");
            Check(new LocalIntegrationProfile("stc", "unused").UsesRealService("AdminSvr", "anything"), "STC mode passes through without fixtures");
            Reject(() => local.Reply<JObject>("BaseSvr", "unimplemented", null, "trader1"), "unknown requests fail explicitly");
            Reject(() => local.Subscribe("MDSvr", "stc.md.anything", "trader1"), "unknown subscriptions fail explicitly");
            string login = local.CompleteLogin("{\"Code\":\"0001\",\"sid\":\"REAL-SESSION\",\"userId\":\"U-001\"}", "trader1");
            JObject identity = JObject.Parse(login);
            Check((string)identity["sid"] == "REAL-SESSION", "real session ID remains unchanged");
            Check(Math.Abs((long)identity["ServerTime"] - DateTimeOffset.UtcNow.ToUnixTimeSeconds()) < 5,
                "STC login timestamp uses seconds");
            Reject(() => local.CompleteLogin("{\"Code\":\"0001\",\"sid\":\"null\",\"userId\":\"U-001\"}", "trader1"), "missing sessions are rejected");
            Reject(() => local.CompleteLogin("{\"Code\":\"0001\",\"sid\":\"REAL-SESSION\",\"userId\":\"U-002\"}", "trader1"), "mismatched user IDs are rejected");
            JObject users = local.Reply<JObject>("AdminSvr", "accountInfoQuery.enable", null, "trader1");
            Check(((JArray)users["Datas"]).Count == 2, "two predictable fixture users");
            ((JArray)users["Datas"]).Clear();
            Check(((JArray)local.Reply<JObject>("AdminSvr", "accountInfoQuery.enable", null, "trader1")["Datas"]).Count == 2, "fixture replies are isolated copies");

            GWApi api = new GWApi();
            api.NewApi();
            try
            {
                foreach (string user in new[] { "trader1", "trader2" })
                {
                    api.SetGzip(true);
                    string reply = (string)api.Login(new LoginParam
                    {
                        Host = "127.0.0.1", Port = 18600, Protocal = "TCP", UserName = user, Pwd = "trader1"
                    });
                    Check((string)JObject.Parse(reply)["Code"] == "0001", user + " authenticates through real GW");
                    JObject quote = api.Request<JObject>("QuoteSvr", "ficc.quote.query", new JObject
                    {
                        ["requestId"] = Guid.NewGuid().ToString("N"), ["ownerScope"] = "MINE", ["limit"] = 1
                    });
                    Check((string)quote["Code"] == "0000", user + " queries real QuoteSvr with its session");
                    JObject notification = api.Request<JObject>("NotificationSvr", "ficc.notification.preference.get", new JObject
                    {
                        ["requestId"] = Guid.NewGuid().ToString("N")
                    });
                    Check((string)notification["Code"] == "0000", user + " queries real NotificationSvr with its session");
                    api.DisConnect();
                    api.NewApi();
                }
            }
            finally
            {
                api.DisConnect();
            }
            Console.WriteLine("RESULT passed=" + passed);
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}
