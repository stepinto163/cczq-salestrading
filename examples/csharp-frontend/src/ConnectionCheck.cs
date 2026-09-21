using System;
using System.IO;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json.Linq;
using Ficc.SalesTrading;

namespace Ficc.FrontendLearning
{
    // 可选的只读连接检查，不生成报价，不改变业务设置。
    internal static class ConnectionCheck
    {
        internal static int Run()
        {
            string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "connection-check.txt");
            try
            {
                using (var client = new DpsClient())
                using (var report = new StreamWriter(reportPath, false))
                {
                    var screenLog = new ConcurrentQueue<string>();
                    client.Trace += screenLog.Enqueue;
                    client.Connect(Environment.GetEnvironmentVariable("FICC_USER"),
                        int.Parse(Environment.GetEnvironmentVariable("FICC_PORT") ?? "18600"),
                        Environment.GetEnvironmentVariable("FICC_HOST") ?? "127.0.0.1",
                        Environment.GetEnvironmentVariable("FICC_PASSWORD"));
                    report.WriteLine("PASS login");
                    var dictionary = client.Data("ficc.quote.dictionary.query", new JObject());
                    var fields = (JArray)dictionary["filterFields"];
                    using (var editor = new ProductFilterForm(fields, FilterExamples.Build("不限属性")))
                    {
                        editor.CreateControl();
                        report.WriteLine("PASS full filter editor constructed from " + fields.Count + " server fields");
                    }
                    foreach (string domain in new[] { "quote", "price", "notification" })
                    {
                        var stream = client.Data("ficc." + domain + ".stream.open", new JObject());
                        string topic = (string)stream["pushTopic"];
                        client.Subscribe(topic, false);
                        var examples = domain == "quote" ? FilterExamples.Names : new[] { "组合条件" };
                        foreach (string example in examples)
                        {
                            string viewId = "CHECK-" + Guid.NewGuid().ToString("N");
                            var request = new JObject
                            {
                                ["viewId"] = viewId,
                                ["pageSize"] = 1,
                                ["filter"] = FilterExamples.Build(example)
                            };
                            if (domain == "quote") request["ownerScope"] = "MINE";
                            if (domain == "price") request["mode"] = "BEST";
                            if (domain == "notification") request["readState"] = "ALL";
                            client.Data("ficc." + domain + ".view.open", request);
                            client.Data("ficc." + domain + ".view.query", new JObject
                            {
                                ["viewId"] = viewId, ["pageNumber"] = 1, ["pageSize"] = 1
                            });
                            client.Data("ficc." + domain + ".view.close", new JObject { ["viewId"] = viewId });
                            report.WriteLine("PASS " + domain + " / " + example + " / filtered open / query / close");
                        }
                        client.Unsubscribe(topic);
                        foreach (string operation in new[] { "SUBSCRIBE", "SDK_SUBSCRIBE_RESULT", "SUBSCRIBE_ACK_REQUEST", "SUBSCRIBE_ACK", "UNSUBSCRIBE", "SDK_UNSUBSCRIBE_COMPLETE" })
                        {
                            if (!screenLog.Any(line => line.StartsWith(operation + " " + topic + " ")))
                            {
                                throw new InvalidOperationException("Missing on-screen log: " + operation + " " + topic);
                            }
                        }
                        report.WriteLine("PASS " + domain + " subscription request / GW acknowledgement / unsubscribe reach UI Trace");
                        report.WriteLine("PASS " + domain + " stream / subscription / first page / query / close");
                    }
                    report.WriteLine("PASS read-only check; no business writes");
                    File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "subscription-trace-check.txt"),
                        screenLog.Where(line => line.StartsWith("SUBSCRIBE") || line.StartsWith("SDK_SUBSCRIBE")
                            || line.StartsWith("UNSUBSCRIBE") || line.StartsWith("SDK_UNSUBSCRIBE")));
                }
                return 0;
            }
            catch (Exception error)
            {
                File.AppendAllText(reportPath, "FAIL " + error + Environment.NewLine);
                return 1;
            }
        }
    }
}
