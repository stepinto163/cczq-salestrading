using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    static class Program {
        [STAThread] static int Main(string[] args) {
            try{return Run(args);}finally{MessageLog.Close();}
        }
        static int Run(string[] args){
            if (args.Length == 2 && args[0] == "--qa-autotest") {
                return QaFunctionalTest.Run(args[1]);
            }
            if (args.Length > 0 && args[0] == "--probe-services") {
                return ServiceDiscoveryProbe.Run(args[1]);
            }
            if(args.Length>0&&args[0]=="--autotest-multi-user")return MultiUserQuoteScenario.Run(args[1]);
            if(args.Length>0&&args[0]=="--autotest-notifications")return NotificationSmoke.Run(args[1]);
            if(args.Length>0&&(args[0]=="--smoke"||args[0]=="--autotest"))return GatewaySmoke.Run(args.Length>1?args[1]:Path.Combine(MessageLog.DirectoryPath,"autotest.json"), args.Length>2?int.Parse(args[2]):18600);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 2 && args[0] == "--filter-performance") {
                int benchmarkExitCode = 0;
                var benchmarkForm = new QuoteForm();
                benchmarkForm.Shown += async (sender, eventArgs) => {
                    try {
                        await benchmarkForm.RunFilterPerformance(args[1]);
                    } catch (Exception error) {
                        Console.Error.WriteLine(error.Message);
                        benchmarkExitCode = 1;
                    } finally {
                        benchmarkForm.Close();
                    }
                };
                Application.Run(benchmarkForm);
                return benchmarkExitCode;
            }
            if (args.Length == 2 && (args[0] == "--scroll-performance" || args[0] == "--autotest-scroll")) {
                int result = 0;
                var scrollingForm = new QuoteForm();
                scrollingForm.Shown += async (sender, eventArgs) => {
                    try {
                        if (args[0] == "--autotest-scroll") await scrollingForm.RunScrollingSmoke(args[1]);
                        else await scrollingForm.RunScrollingPerformance(args[1]);
                    } catch (Exception error) {
                        Console.Error.WriteLine(error);
                        result = 1;
                    } finally {
                        scrollingForm.Close();
                    }
                };
                Application.Run(scrollingForm);
                return result;
            }
            var form=new QuoteForm();int exitCode=0;
            if(args.Length>0&&(args[0]=="--autotest-panel"||args[0]=="--autotest-multi-user-panel"))form.Shown+=async(s,e)=>{
                try{if(args[0]=="--autotest-multi-user-panel")await form.RunMultiUserScenario();else await form.RunAutoTests();File.WriteAllText(args[1],form.LastAutoTestResult.ToString());if((string)form.LastAutoTestResult["result"]!="PASS")exitCode=1;}
                catch(Exception error){Console.Error.WriteLine(error);exitCode=1;}finally{form.Close();}
            };
            if(args.Length>0&&(args[0]=="--ui-smoke"||args[0]=="--autotest-ui"||args[0]=="--autotest-price"||args[0]=="--autotest-groups"||args[0]=="--autotest-counterparty"))form.Shown+=async(s,e)=>{
                string output=args.Length>1?args[1]:Path.Combine(MessageLog.DirectoryPath,"autotest-ui.json");
                try{if(args[0]=="--autotest-counterparty")await form.RunCounterpartyUiSmoke(output);else if(args[0]=="--autotest-groups")await form.RunGroupUiSmoke(output);else if(args[0]=="--autotest-price")await form.RunPriceSmoke(output);else await form.RunUiSmoke(output);}catch(Exception error){Console.Error.WriteLine(error);Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));File.WriteAllText(output,new JObject{["result"]="FAIL",["error"]=error.ToString(),["messageLogDirectory"]=MessageLog.DirectoryPath}.ToString());exitCode=1;}finally{form.Close();}
            };
            Application.Run(form);
            return exitCode;
        }
    }
}
