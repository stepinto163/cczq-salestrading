using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    // Three quotes on one user-selected reference bond. Runs in a supervised child process.
    static class QaFunctionalTest {
        public static int Run(string output) {
            using (var single = new Mutex(false, "Local\\FiccQaFunctionalTest")) {
                bool acquired;
                try {
                    acquired = single.WaitOne(0);
                } catch (AbandonedMutexException) {
                    acquired = true;
                }
                if (!acquired) throw new InvalidOperationException("另一轮 QA 功能测试正在运行。");
                try {
                    var run = new Scenario(output);
                    return run.Execute(JObject.Parse(Console.ReadLine() ?? "{}"));
                } finally {
                    single.ReleaseMutex();
                }
            }
        }

        sealed class Scenario {
            readonly string output;
            readonly string runId = "QA-" + Guid.NewGuid().ToString("N");
            readonly Stopwatch clock = Stopwatch.StartNew();
            readonly DpsClient mine = new DpsClient();
            readonly DpsClient other = new DpsClient();
            readonly List<Tuple<DpsClient, string>> quotes = new List<Tuple<DpsClient, string>>();
            readonly List<Tuple<string, JObject>> groups = new List<Tuple<string, JObject>>();
            readonly ConcurrentQueue<JObject> pushes = new ConcurrentQueue<JObject>();
            readonly JObject report = new JObject {
                ["result"] = "RUNNING", ["steps"] = new JArray(), ["requests"] = new JArray(),
                ["createdQuotes"] = new JArray(), ["cleanupErrors"] = new JArray(),
                ["notCovered"] = new JArray("全市场遍历", "压力与渲染性能", "界面操作自动化", "CRM匹配", "QT来源接入")
            };
            JObject originalPreference;
            string savedPreferenceRevision;
            string securityId;
            string market;
            string yieldType;
            string quoteTopic;
            string priceTopic;
            string notificationTopic;
            bool cleaning;
            long cleanupDeadline;

            public Scenario(string output) {
                this.output = Path.GetFullPath(output);
                Directory.CreateDirectory(Path.GetDirectoryName(this.output));
                report["runId"] = runId;
                report["messageLogDirectory"] = MessageLog.DirectoryPath;
            }

            public int Execute(JObject configuration) {
                try {
                    securityId = Required(configuration, "securityId");
                    string host = Required(configuration, "host");
                    string user = Required(configuration, "username");
                    string secondUser = Required(configuration, "secondUsername");
                    if (user == secondUser) throw new InvalidOperationException("两个账号必须不同。");
                    report["securityId"] = securityId;
                    report["accounts"] = new JArray(user, secondUser);
                    report["referenceSource"] = "ficc.base.get -> configured BaseSvr adapter";
                    Save();
                    mine.Connect(user, (int)configuration["port"], host, Required(configuration, "password"));
                    other.Connect(secondUser, (int)configuration["port"], host, Required(configuration, "secondPassword"));
                    Check(true, "两个账号均取得登录会话");
                    Preflight();
                    Exercise();
                    report["result"] = "PASS";
                } catch (Exception error) {
                    if ((string)report["result"] != "PRECONDITION_FAILED") {
                        report["result"] = error is OperationCanceledException ? "CANCELLED" : "FAIL";
                    }
                    report["error"] = error.Message;
                } finally {
                    cleaning = true;
                    cleanupDeadline = clock.ElapsedMilliseconds + 20000;
                    Cleanup();
                    try {
                        mine.Dispose();
                    } catch (Exception error) {
                        CleanupError(error);
                    }
                    try {
                        other.Dispose();
                    } catch (Exception error) {
                        CleanupError(error);
                    }
                    if (((JArray)report["cleanupErrors"]).Count > 0 && (string)report["result"] == "PASS") {
                        report["result"] = "CLEANUP_FAILED";
                    }
                    report["elapsedMs"] = clock.ElapsedMilliseconds;
                    Save();
                }
                return (string)report["result"] == "PASS" ? 0 : 1;
            }

            void Preflight() {
                JObject reference = Call(mine, "ficc.base.get", new JObject {
                    ["kind"] = "SECURITY", ["ids"] = new JArray(securityId)
                });
                JObject bond = ((JArray)reference["records"]).OfType<JObject>()
                    .SingleOrDefault(item => (string)item["securityId"] == securityId);
                if (bond == null) {
                    report["result"] = "PRECONDITION_FAILED";
                    throw new InvalidOperationException("所选债券不在当前基础资料中；不会创建报价。");
                }
                report["reference"] = bond.DeepClone();
                market = (string)bond["market"];
                if (market != "RATE" && market != "CREDIT") {
                    report["result"] = "PRECONDITION_FAILED";
                    throw new InvalidOperationException("基础资料缺少 RATE/CREDIT 分类，四类提醒无法确定市场范围；不会创建报价。");
                }
                yieldType = (bool?)bond["hasExerciseOption"] == true ? "2" : "1";
                JObject book = Call(mine, "ficc.price.query", new JObject {
                    ["securityIds"] = new JArray(securityId), ["mode"] = "BEST"
                });
                if (((JArray)book["prices"]).Any(item => ((JArray)item["marketData"]["NoMDEntries"]).Count > 0)) {
                    report["result"] = "PRECONDITION_FAILED";
                    throw new InvalidOperationException("该债券已有有效行情。为保证最优报价测试可预测，请在 QA 选择没有有效行情的债券；不会修改已有报价。");
                }
                Check(true, "通过真实基础资料校验，测试开始前该券行情为空");
                originalPreference = (JObject)Call(mine, "ficc.notification.preference.get", new JObject())["preference"];
                report["preferenceBackup"] = originalPreference.DeepClone();
                Save();
                var preference = new JObject { ["expectedRevision"] = originalPreference["revision"].DeepClone() };
                foreach (string key in PreferenceKeys) preference[key] = true;
                savedPreferenceRevision = (string)Call(mine, "ficc.notification.preference.save", preference)["preference"]["revision"];
                report["testPreferenceRevision"] = savedPreferenceRevision;
                Save();
                mine.Message += (topic, message) => {
                    if (pushes.Count < 128) pushes.Enqueue(new JObject { ["topic"] = topic, ["body"] = message.DeepClone() });
                };
                quoteTopic = OpenStream("quote", false);
                priceTopic = OpenStream("price", true);
                notificationTopic = OpenStream("notification", false);
                foreach (string domain in new[] { "quote", "price", "notification" }) {
                    var view = new JObject { ["viewId"] = runId + "-" + domain, ["filter"] = Filter(), ["pageSize"] = 20 };
                    if (domain == "quote") {
                        view["ownerScope"] = "MINE";
                        view["status"] = new JArray("ACTIVE");
                    }
                    if (domain == "price") view["mode"] = "FULL";
                    Call(mine, "ficc." + domain + ".view.open", view);
                }
                foreach (string kind in new[] { "watchlist", "filterGroup" }) {
                    var group = new JObject { ["name"] = runId, ["market"] = market, ["scope"] = "MARKET" };
                    if (kind == "watchlist") group["securities"] = new JArray(new JObject { ["securityId"] = securityId });
                    else group["subgroups"] = new JArray(new JObject { ["conditions"] = Filter()["conditions"].DeepClone() });
                    JObject saved = (JObject)Call(mine, "ficc." + kind + ".save", group)[kind];
                    groups.Add(Tuple.Create(kind, saved));
                    report[kind] = saved.DeepClone();
                    Save();
                }
            }

            void Exercise() {
                string bid = Create(mine, "1", "2.5000", "1");
                Check((string)Get(mine, bid)["status"] == "PENDING_CONFIRM", "新增报价进入待确认");
                Change(mine, "confirm", bid);
                Change(mine, "update", bid, new JObject { ["price"] = "2.4900" });
                JObject current = Get(mine, bid);
                Check((string)current["status"] == "ACTIVE" && (string)current["price"] == "2.4900", "确认及修改结果可回查");
                Wait(() => pushes.Any(push => (string)push["topic"] == quoteTopic
                    && (push["body"]["reports"] as JArray)?.Any(item => (string)item["quoteId"] == bid
                        && (string)item["revision"] == (string)current["revision"]) == true), "报价报告关联 quoteId 与最新 revision");
                Wait(() => pushes.Any(push => (string)push["topic"] == priceTopic
                    && (push["body"]["books"] as JArray)?.Any(book => (string)book["securityId"] == securityId
                        && book["quoteRefs"]?["FICC:" + bid] != null) == true), "独立价格通道收到所选债券行情更新");
                Check(((JArray)Call(mine, "ficc.quote.query", new JObject {
                    ["filter"] = Filter(), ["ownerScope"] = "MINE", ["limit"] = 20
                })["quotes"]).Any(item => (string)item["quoteId"] == bid), "按所选债券筛选命中新建报价");
                AwaitNotifications(rows => rows.Any(item => Includes(item, bid) && Reason(item, "WATCHLIST")), "关注债券提醒");
                AwaitNotifications(rows => rows.Any(item => Includes(item, bid) && Reason(item, "FILTER_GROUP")), "筛选分组提醒");
                AwaitNotifications(rows => rows.Any(item => Includes(item, bid) && Reason(item, "MY_BEST")), "我的最优报价提醒");
                string ask = Create(other, "2", "3.0000", "2");
                Change(other, "confirm", ask);
                Check((string)Get(mine, bid)["ownerUserId"] != (string)Get(other, ask)["ownerUserId"], "两个账号对应不同真实用户 ID");
                JObject forbidden = Call(other, "ficc.quote.refer", Operation(mine, bid));
                Check((string)forbidden["results"]?[0]?["Code"] == "1003", "另一用户不能撤销我的报价（使用非管理员测试账号）");
                JArray notices = AwaitNotifications(rows => rows.Any(item => Includes(item, bid) && Includes(item, ask)
                    && Reason(item, "COUNTER_QUOTE")), "对价提醒（跨结算方式）");
                JToken counter = notices.First(item => Includes(item, bid) && Includes(item, ask) && Reason(item, "COUNTER_QUOTE"));
                Check((string)Call(mine, "ficc.notification.read", new JObject {
                    ["notificationIds"] = new JArray(counter["notificationId"].DeepClone()), ["read"] = true
                })["results"][0]["Code"] == "0000", "对价提醒可以标记已读");
                string better = Create(other, "1", "2.4800", "1");
                Change(other, "confirm", better);
                AwaitNotifications(rows => rows.All(item => !Includes(item, bid) || !Reason(item, "MY_BEST")), "他人更优报价取消我的最优资格");
                Change(other, "update", better, new JObject { ["price"] = "2.4900" });
                AwaitNotifications(rows => rows.Any(item => Includes(item, bid) && Reason(item, "MY_BEST")), "同价最优恢复提醒");
                Wait(() => pushes.Any(push => (string)push["topic"] == notificationTopic
                    && (string)push["body"]["kind"] == "UPDATE" && push["body"]["notifications"] is JArray), "提醒通过独立通道实时推送");
                Change(mine, "refer", bid);
                Check((string)Get(mine, bid)["status"] == "REFERRED", "撤销报价");
                Change(mine, "unrefer", bid);
                Check((string)Get(mine, bid)["quoteId"] == bid && (string)Get(mine, bid)["status"] == "ACTIVE", "恢复报价保持 quoteId");
                Change(mine, "refer", bid);
                Change(mine, "delete", bid);
                Check((string)Get(mine, bid)["status"] == "DELETED", "删除状态持久化");
                JArray history = (JArray)Call(mine, "ficc.quote.history.query", new JObject { ["quoteId"] = bid })["records"];
                Check(history.Count == 7, "同一报价的七次生命周期变化连续记录");
            }

            JObject Filter() => new JObject { ["conditions"] = new JArray(new JObject {
                ["field"] = "securityId", ["operator"] = "EQ", ["value"] = securityId
            }) };
            string OpenStream(string domain, bool image) {
                string topic = (string)Call(mine, "ficc." + domain + ".stream.open", new JObject())["pushTopic"];
                mine.Subscribe(topic, image);
                return topic;
            }
            string Create(DpsClient client, string side, string price, string settlement) {
                if (quotes.Count >= 3) throw new InvalidOperationException("本轮最多三条报价。");
                var input = new JObject {
                    ["clientQuoteId"] = runId + "-" + quotes.Count, ["securityId"] = securityId,
                    ["side"] = side, ["quoteType"] = "1", ["yieldType"] = yieldType, ["price"] = price,
                    ["quantity"] = "1", ["quantityUnit"] = "WAN", ["currency"] = "CNY",
                    ["settlType"] = settlement, ["tradeDate"] = DateTime.Today.ToString("yyyy-MM-dd")
                };
                JObject result = Item(Call(client, "ficc.quote.create", new JObject { ["quotes"] = new JArray(input) }));
                string id = (string)result["quoteId"];
                quotes.Add(Tuple.Create(client, id));
                ((JArray)report["createdQuotes"]).Add(new JObject { ["quoteId"] = id, ["clientQuoteId"] = input["clientQuoteId"].DeepClone() });
                Save();
                return id;
            }
            JObject Get(DpsClient client, string id) => (JObject)Call(client, "ficc.quote.get", new JObject { ["quoteId"] = id })["quote"];
            JObject Operation(DpsClient client, string id, JObject changes = null) {
                JObject item = changes ?? new JObject();
                item["quoteId"] = id;
                item["expectedRevision"] = Get(client, id)["revision"].DeepClone();
                return new JObject { ["quotes"] = new JArray(item) };
            }
            void Change(DpsClient client, string operation, string id, JObject changes = null) {
                Item(Call(client, "ficc.quote." + operation, Operation(client, id, changes)));
            }
            static JObject Item(JObject data) {
                JObject item = (JObject)data["results"][0];
                if ((string)item["Code"] != "0000") throw new InvalidOperationException("业务操作失败：" + item);
                return item;
            }
            static bool Includes(JToken item, string id) => (item["quoteIds"] as JArray)?.Any(value => (string)value == id) == true;
            static bool Reason(JToken item, string reason) => (item["reasons"] as JArray)?.Any(value => (string)value["type"] == reason) == true;
            JArray AwaitNotifications(Func<JArray, bool> predicate, string label) {
                JArray rows = null;
                Wait(() => {
                    rows = (JArray)Call(mine, "ficc.notification.query", new JObject { ["filter"] = Filter(), ["limit"] = 20 })["notifications"];
                    return predicate(rows);
                }, label);
                return rows;
            }
            void Wait(Func<bool> predicate, string label) {
                long deadline = clock.ElapsedMilliseconds + 6000;
                do {
                    Budget();
                    if (predicate()) {
                        Check(true, label);
                        return;
                    }
                    Thread.Sleep(200);
                } while (clock.ElapsedMilliseconds < deadline);
                throw new TimeoutException(label + "：六秒内未满足；请核对推送日志和外部行情干扰。");
            }
            JObject Call(DpsClient client, string topic, JObject request) {
                Budget();
                request["requestId"] = "QA-" + Guid.NewGuid().ToString("N");
                var entry = new JObject {
                    ["topic"] = topic, ["requestId"] = request["requestId"].DeepClone(),
                    ["request"] = request.DeepClone(), ["status"] = "SENT", ["cleanup"] = cleaning,
                    ["account"] = client == mine ? "FIRST" : "SECOND"
                };
                ((JArray)report["requests"]).Add(entry);
                Save();
                var timer = Stopwatch.StartNew();
                JObject reply;
                try {
                    reply = client.Request(topic, request);
                    entry["Code"] = reply?["Code"]?.DeepClone();
                    entry["status"] = "REPLIED";
                } finally {
                    entry["elapsedMs"] = timer.ElapsedMilliseconds;
                    Save();
                }
                if ((string)reply?["Code"] != "0000") throw new InvalidOperationException(topic + "：" + (string)reply?["Msg"]);
                return (JObject)reply["Data"];
            }
            void Check(bool condition, string label) {
                if (!condition) throw new InvalidOperationException(label);
                ((JArray)report["steps"]).Add(new JObject { ["name"] = label, ["result"] = "PASS", ["elapsedMs"] = clock.ElapsedMilliseconds });
                Console.WriteLine("PASS " + label);
                Save();
            }
            void Budget() {
                if (cleaning) {
                    if (clock.ElapsedMilliseconds > cleanupDeadline) throw new TimeoutException("清理超过二十秒，请按报告中的对象 ID 手动清理。");
                } else {
                    if (File.Exists(output + ".cancel")) throw new OperationCanceledException("用户停止测试。");
                    if (clock.ElapsedMilliseconds > 60000) throw new TimeoutException("功能测试超过六十秒，开始清理。");
                }
            }
            void Cleanup() {
                if (((JArray)report["requests"]).Any(item => (string)item["status"] == "SENT"
                    && ((string)item["topic"] == "ficc.quote.create" || ((string)item["topic"]).EndsWith(".save")))) {
                    CleanupError(new Exception("存在未收到回复的创建/保存请求，提交结果未知；请按报告 requestId/clientQuoteId 和偏好备份核对。"));
                }
                foreach (var quote in quotes) {
                    try {
                        string status = (string)Get(quote.Item1, quote.Item2)["status"];
                        if (status == "ACTIVE") Change(quote.Item1, "refer", quote.Item2);
                        if (status != "DELETED") Change(quote.Item1, "delete", quote.Item2);
                    } catch (Exception error) {
                        CleanupError(error);
                    }
                }
                foreach (var group in groups) {
                    try {
                        string key = group.Item1 + "Id";
                        Call(mine, "ficc." + group.Item1 + ".delete", new JObject {
                            [key] = group.Item2[key].DeepClone(), ["expectedRevision"] = group.Item2["revision"].DeepClone()
                        });
                    } catch (Exception error) {
                        CleanupError(error);
                    }
                }
                if (savedPreferenceRevision != null) {
                    try {
                        var restore = new JObject { ["expectedRevision"] = savedPreferenceRevision };
                        foreach (string key in PreferenceKeys) restore[key] = originalPreference[key].DeepClone();
                        Call(mine, "ficc.notification.preference.save", restore);
                    } catch (Exception error) {
                        CleanupError(error);
                    }
                }
            }
            void CleanupError(Exception error) {
                ((JArray)report["cleanupErrors"]).Add(error.Message);
            }

            void Save() {
                string temporaryPath = output + ".tmp";
                File.WriteAllText(temporaryPath, report.ToString());
                if (File.Exists(output)) {
                    File.Replace(temporaryPath, output, null);
                } else {
                    File.Move(temporaryPath, output);
                }
            }
            static readonly string[] PreferenceKeys = { "watchlistEnabled", "filterGroupEnabled", "counterQuoteEnabled", "myBestEnabled" };
            static string Required(JObject value, string key) {
                string text = (string)value[key];
                if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("请填写 " + key);
                return text;
            }
        }
    }
}
