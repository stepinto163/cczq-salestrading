using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    static class Program {
        [STAThread]
        static int Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            int exitCode = 0;
            var form = new ApiDebugForm();
            if (args.Length == 2 && args[0] == "--self-test") {
                form.Shown += async (sender, eventArgs) => {
                    try {
                        await form.SelfTest(args[1]);
                    } catch (Exception error) {
                        Console.Error.WriteLine(error);
                        File.WriteAllText(args[1], new JObject {
                            ["result"] = "FAIL",
                            ["error"] = error.ToString()
                        }.ToString());
                        exitCode = 1;
                    } finally {
                        form.Close();
                    }
                };
            }
            try {
                Application.Run(form);
            } finally {
                MessageLog.Close();
            }
            return exitCode;
        }
    }
}
