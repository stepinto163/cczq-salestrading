using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.View;

namespace SalesTrading.UiTests
{
    internal static class ManualDialogCheck
    {
        [STAThread]
        public static int Main()
        {
            Application application = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            application.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    Type workerType = typeof(CaseDefinition).Assembly.GetType("SalesTrading.UiTests.UiScenarioWorker", true);
                    object worker = Activator.CreateInstance(workerType, true);
                    BindingFlags instance = BindingFlags.NonPublic | BindingFlags.Instance;
                    BindingFlags shared = BindingFlags.NonPublic | BindingFlags.Static;
                    var definition = new CaseDefinition { Id = "dialog-check", Name = "窗口切换检查" };
                    definition.Steps.Add(new TestStep { Action = "quote.assert", Name = "核对入库" });
                    definition.Steps.Add(new TestStep { Action = "manual.click", Name = "继续编辑" });
                    workerType.GetField("definition", shared).SetValue(null, definition);
                    workerType.GetField("reportDirectory", shared).SetValue(null, Directory.GetCurrentDirectory());
                    workerType.GetField("stepNumber", instance).SetValue(worker, 1);
                    FieldInfo editorField = workerType.GetField("editor", instance);
                    MethodInfo prepare = workerType.GetMethod("PrepareManualEditorForStep", instance);
                    QuoteOperationWindow dialog = new QuoteOperationWindow(null);
                    dialog.Show();
                    editorField.SetValue(worker, dialog);
                    prepare.Invoke(worker, new object[] { definition.Steps[0] });
                    Require(dialog.IsVisible, "服务端核对后还要编辑，窗口应保留");
                    // 用固定测试记录验证关闭前保存清理账本，不访问业务服务。
                    typeof(QuoteOperationWindow).GetProperty("CurrentQuote").GetSetMethod(true).Invoke(dialog,
                        new object[] { new JObject { ["quoteId"] = "dialog-check-only" } });
                    definition.Steps[1].Action = "market.assert";
                    prepare.Invoke(worker, new object[] { definition.Steps[0] });
                    Require(!dialog.IsVisible && editorField.GetValue(worker) == null, "转入行情前窗口必须关闭");
                    Require(File.ReadAllText("owned-quotes.json").Contains("dialog-check-only"), "关闭丢失清理账本");
                    foreach (string action in new[] { "sidebar.range", "management.select", "parse", "manual.open", "history.assert" })
                    {
                        dialog = new QuoteOperationWindow(null);
                        dialog.Show();
                        editorField.SetValue(worker, dialog);
                        definition.Steps.Clear();
                        definition.Steps.Add(new TestStep { Action = action, Name = action });
                        prepare.Invoke(worker, new object[] { definition.Steps[0] });
                        Require(!dialog.IsVisible && editorField.GetValue(worker) == null, "未关闭窗口：" + action);
                    }
                    Console.WriteLine("PASS: real quote dialog retained for editing, closed before unrelated/end steps, cleanup IDs retained.");
                    application.Shutdown(0);
                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                    application.Shutdown(1);
                }
            }));
            return application.Run();
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
