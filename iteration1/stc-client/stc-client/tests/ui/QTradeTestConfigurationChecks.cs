using System;
using System.Collections.Generic;
using System.IO;

namespace SalesTrading.UiTests
{
    internal static class QTradeTestConfigurationChecks
    {
        private static int Main(string[] args)
        {
            var users = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("first", "1001"),
                new KeyValuePair<string, string>("second", "1002"),
                new KeyValuePair<string, string>("unmapped", "")
            };
            Assert(QTradeTestConfiguration.ResolveReceiver("first", users) == "1001", "first receiver");
            Assert(QTradeTestConfiguration.ResolveReceiver("second", users) == "1002", "login changes receiver");
            Reject(() => QTradeTestConfiguration.ResolveReceiver("unmapped", users), "missing inf1");
            Reject(() => QTradeTestConfiguration.ResolveReceiver("unknown", users), "unknown login");
            users.Add(new KeyValuePair<string, string>("duplicate", "1001"));
            Reject(() => QTradeTestConfiguration.ResolveReceiver("first", users), "duplicate mapping");
            Assert(!QTradeTestConfiguration.IsNumericAccount("QT1001"), "reject malformed account");

            string fixture = Path.Combine(args[1], "configuration-check-fixture");
            string root = Path.Combine(fixture, "ats_root");
            string control = Path.Combine(root, "control");
            string config = Path.Combine(fixture, "stc-client", "STC.exe.config");
            Directory.CreateDirectory(control);
            string auth = Path.Combine(control, "QtIntakeSvrAuth.properties");
            File.WriteAllText(auth, "# fixture only\nintake.token = fixture=value\n");
            Assert(QTradeTestConfiguration.LoadToken(config, null) == "fixture=value", "config-relative shared token");
            Assert(QTradeTestConfiguration.LoadToken("unused", root) == "fixture=value", "explicit ATS_ROOT");
            File.WriteAllText(auth, "intake.token=\n");
            Reject(() => QTradeTestConfiguration.LoadToken(config, null), "empty token");
            File.Delete(auth);
            Reject(() => QTradeTestConfiguration.LoadToken(config, null), "missing deployment config");

            Assert(!Actions.Fields.ContainsKey("fixture.pending"), "manual pending shortcut removed");
            int count = 0;
            var publishingCombinations = new HashSet<string>();
            foreach (string file in Directory.GetFiles(args[0], "*.json"))
            {
                CaseDefinition definition = CaseDefinition.Read(file);
                foreach (TestStep step in definition.Steps)
                {
                    if (step.Action != "qtrade.publish")
                    {
                        continue;
                    }
                    string audience = string.IsNullOrWhiteSpace((string)step.Parameters["groupId"]) ? "DIRECT" : "GROUP";
                    publishingCombinations.Add(audience + ":" + (string)step.Parameters["mode"]);
                    // 单聊可以省略群号；编辑器保存空群号也必须合法。
                    step.Parameters.Remove("groupId");
                    definition.Validate();
                    step.Parameters["groupId"] = "";
                    definition.Validate();
                    step.Parameters["receiverLogin"] = "obsolete";
                    Reject(definition.Validate, "obsolete receiver config");
                    step.Parameters.Remove("receiverLogin");
                }
                count++;
            }
            Assert(publishingCombinations.SetEquals(new[] { "DIRECT:TEXT", "DIRECT:STRUCTURED", "GROUP:TEXT", "GROUP:STRUCTURED" }),
                "direct/group x structured/text publishing coverage");
            Console.WriteLine("PASS: receiver mapping, shared configuration, and " + count + " case definitions.");
            return 0;
        }

        private static void Assert(bool condition, string name)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Failed: " + name);
            }
        }

        private static void Reject(Action action, string name)
        {
            try
            {
                action();
            }
            catch (InvalidOperationException)
            {
                return;
            }
            throw new InvalidOperationException("Expected rejection: " + name);
        }
    }
}
