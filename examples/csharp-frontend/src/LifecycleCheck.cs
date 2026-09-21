using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Ficc.SalesTrading;

namespace Ficc.FrontendLearning
{
    // 显式运行的单条报价回归：使用真实窗体按钮，经 GW 调用服务，并删除本次测试报价。
    internal static class LifecycleCheck
    {
        internal static int Run()
        {
            string reportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lifecycle-check.txt");
            int exitCode = 1;
            using (var report = new StreamWriter(reportPath, false) { AutoFlush = true })
            using (var form = new LearningForm())
            {
                form.Shown += async (sender, args) =>
                {
                    string createdQuoteId = null;
                    var createdQuoteIds = new List<string>();
                    var api = Field<QuoteExamples>(form, "api");
                    try
                    {
                        VerifyTimestampOrdering(report);
                        SetInput(form, "host", Environment.GetEnvironmentVariable("FICC_HOST") ?? "127.0.0.1");
                        SetInput(form, "port", Environment.GetEnvironmentVariable("FICC_PORT") ?? "18600");
                        SetInput(form, "user", Environment.GetEnvironmentVariable("FICC_USER"));
                        SetInput(form, "password", Environment.GetEnvironmentVariable("FICC_PASSWORD"));
                        SetInput(form, "security", Environment.GetEnvironmentVariable("FICC_SECURITY") ?? "250001.IB");
                        await Click(form, "1. 登录");
                        if (!Field<bool>(form, "connected"))
                        {
                            throw new InvalidOperationException("Login did not connect");
                        }
                        report.WriteLine("PASS login through form button");
                        await Click(form, "3. 报价视图");
                        report.WriteLine("PASS quote view opened and subscribed through form button");
                        await Click(form, "2. 创建待确认报价");
                        createdQuoteId = Field<TextBox>(form, "quoteId").Text;
                        createdQuoteIds.Add(createdQuoteId);
                        await Verify(form, api, createdQuoteId, "PENDING_CONFIRM", 1, report);
                        await Click(form, "2. 创建待确认报价");
                        string newerQuoteId = Field<TextBox>(form, "quoteId").Text;
                        createdQuoteIds.Add(newerQuoteId);
                        await Verify(form, api, newerQuoteId, "PENDING_CONFIRM", 1, report);
                        SetInput(form, "quoteId", createdQuoteId);
                        await Click(form, "读取报价/版本");
                        await Click(form, "确认");
                        await Verify(form, api, createdQuoteId, "ACTIVE", 2, report);
                        report.WriteLine("PASS older quote moved above newer quote after confirmation");
                        SetInput(form, "price", "2.5100");
                        await Click(form, "修改收益率");
                        await Verify(form, api, createdQuoteId, "ACTIVE", 3, report);
                        if (decimal.Parse((string)Field<JObject>(form, "displayedQuote")["price"],
                            System.Globalization.CultureInfo.InvariantCulture) != 2.5100m)
                        {
                            throw new InvalidOperationException("Edited price was not saved");
                        }
                        await Click(form, "撤销");
                        await Verify(form, api, createdQuoteId, "REFERRED", 4, report);
                        await Click(form, "恢复");
                        await Verify(form, api, createdQuoteId, "ACTIVE", 5, report);
                        await Click(form, "撤销");
                        await Verify(form, api, createdQuoteId, "REFERRED", 6, report);
                        await Click(form, "删除");
                        await Verify(form, api, createdQuoteId, "DELETED", 7, report);
                        await Click(form, "变更历史");
                        report.WriteLine("PASS create / confirm / update / refer / unrefer / refer / delete / history buttons");
                        exitCode = 0;
                    }
                    catch (Exception error)
                    {
                        report.WriteLine("FAIL " + error);
                        report.WriteLine(Field<TextBox>(form, "log").Text);
                    }
                    finally
                    {
                        foreach (string cleanupQuoteId in createdQuoteIds.Where(id => !string.IsNullOrWhiteSpace(id)))
                        {
                            try
                            {
                                var quote = await api.Get(cleanupQuoteId);
                                if ((string)quote["status"] == "ACTIVE")
                                {
                                    QuoteExamples.RequireSuccess(await api.Operate("refer", quote));
                                    quote = await api.Get(cleanupQuoteId);
                                }
                                if ((string)quote["status"] != "DELETED")
                                {
                                    QuoteExamples.RequireSuccess(await api.Operate("delete", quote));
                                }
                                report.WriteLine("PASS cleanup " + cleanupQuoteId);
                            }
                            catch (Exception error)
                            {
                                report.WriteLine("FAIL cleanup " + error);
                                exitCode = 1;
                            }
                        }
                        form.Close();
                    }
                };
                Application.Run(form);
            }
            return exitCode;
        }

        private static async Task Verify(LearningForm form, QuoteExamples api, string quoteId,
            string expectedStatus, int expectedRevision, StreamWriter report)
        {
            var stored = await api.Get(quoteId);
            var displayed = Field<JObject>(form, "displayedQuote");
            if ((string)stored["status"] != expectedStatus || (int)stored["revision"] != expectedRevision
                || (string)displayed?["quoteId"] != quoteId || (int)displayed["revision"] != expectedRevision)
            {
                throw new InvalidOperationException("Unexpected server/form state: " + stored);
            }
            report.WriteLine("PASS server and form " + expectedStatus + " revision=" + expectedRevision);
            var page = Field<SubscribedPage>(form, "page");
            var timer = Stopwatch.StartNew();
            while ((string)page.Rows.FirstOrDefault()?["quoteId"] != quoteId
                || (int?)page.Rows.FirstOrDefault()?["revision"] != expectedRevision)
            {
                if (timer.ElapsedMilliseconds > 5000)
                {
                    throw new InvalidOperationException("Live first page did not move the changed quote to the top");
                }
                await Task.Delay(25);
            }
            var grid = Field<DataGridView>(form, "grid");
            if (grid.Columns.Cast<DataGridViewColumn>().Any(column => column.SortMode != DataGridViewColumnSortMode.NotSortable))
            {
                throw new InvalidOperationException("Column sorting can override update-time order");
            }
            if ((string)grid.Rows[0].Cells[0].Value != quoteId)
            {
                throw new InvalidOperationException("Grid order differs from subscribed page order");
            }
            report.WriteLine("PASS push refreshed first grid row, revision=" + expectedRevision);
        }

        private static void VerifyTimestampOrdering(StreamWriter report)
        {
            var older = JObject.Parse("{\"quoteId\":\"older\",\"updateTime\":\"2026-09-21T15:21:29.505198400+08:00\"}");
            var newer = JObject.Parse("{\"quoteId\":\"newer\",\"updateTime\":\"2026-09-21T15:21:29.690346+08:00\"}");
            var ids = new List<string> { "older" };
            var rows = new Dictionary<string, JObject> { ["older"] = older };
            var message = new JObject
            {
                ["quotes"] = new JArray(newer),
                ["views"] = new JArray(new JObject
                {
                    ["viewId"] = "time-check",
                    ["upsertIds"] = new JArray("newer")
                })
            };
            ClientViewPage.Apply(message, "time-check", "quotes", "quoteId", 1, 20, ids, rows);
            if (ids[0] != "newer")
            {
                throw new InvalidOperationException("Mixed JSON date/string timestamps lose fractional seconds");
            }
            ids = new List<string> { "older", "newer" };
            ClientViewPage.Apply(message, "time-check", "quotes", "quoteId", 2, 20, ids, rows);
            if (ids[0] != "older")
            {
                throw new InvalidOperationException("Browsing page must preserve its order");
            }
            report.WriteLine("PASS mixed timestamp precision sorts correctly; browsing page stays stable");
        }

        private static async Task Click(LearningForm form, string text)
        {
            var button = Controls(form).OfType<Button>().Single(item => item.Text == text);
            if (!button.Enabled)
            {
                throw new InvalidOperationException("Button disabled: " + text);
            }
            string previousLog = Field<TextBox>(form, "log").Text;
            button.PerformClick();
            var timer = Stopwatch.StartNew();
            while (Field<bool>(form, "busy"))
            {
                if (timer.ElapsedMilliseconds > 15000)
                {
                    throw new TimeoutException("Button did not complete: " + text);
                }
                await Task.Delay(25);
            }
            string currentLog = Field<TextBox>(form, "log").Text;
            string addedLog = currentLog.StartsWith(previousLog, StringComparison.Ordinal)
                ? currentLog.Substring(previousLog.Length) : currentLog;
            if (addedLog.Contains("  ERROR "))
            {
                throw new InvalidOperationException("Button failed: " + text + Environment.NewLine + addedLog);
            }
        }

        private static IEnumerable<Control> Controls(Control parent)
        {
            foreach (Control child in parent.Controls)
            {
                yield return child;
                foreach (var descendant in Controls(child))
                {
                    yield return descendant;
                }
            }
        }

        private static T Field<T>(LearningForm form, string name)
        {
            return (T)typeof(LearningForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
        }

        private static void SetInput(LearningForm form, string name, string value)
        {
            Field<TextBox>(form, name).Text = value ?? "";
        }
    }
}
