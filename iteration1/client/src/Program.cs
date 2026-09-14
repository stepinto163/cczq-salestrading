using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.Prototype {
    static class Program {
        [STAThread] static int Main(string[] args) {
            try{return Run(args);}finally{MessageLog.Close();}
        }
        static int Run(string[] args){
            if (args.Length > 0 && args[0] == "--probe-services") {
                return ServiceDiscoveryProbe.Run(args[1]);
            }
            if(args.Length>0&&args[0]=="--autotest-multi-user")return MultiUserQuoteScenario.Run(args[1]);
            if(args.Length>0&&args[0]=="--autotest-notifications")return NotificationSmoke.Run(args[1]);
            if(args.Length>0&&(args[0]=="--smoke"||args[0]=="--autotest"))return GatewaySmoke.Run(args.Length>1?args[1]:Path.Combine(MessageLog.DirectoryPath,"autotest.json"));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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
