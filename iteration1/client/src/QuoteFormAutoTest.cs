using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        readonly TabPage testTab=new TabPage("自动测试");
        readonly TextBox testOutput=new TextBox{Dock=DockStyle.Fill,Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Both,WordWrap=false};
        readonly Label testStatus=new Label{AutoSize=true,Text="自动测试依次检查接口、提醒、报价界面、分组界面、CRM对手确认；共 8 条报价、2 个分组。",Padding=new Padding(4)};
        readonly Button stopTest=new Button{Text="停止测试",AutoSize=true,Enabled=false};
        readonly Button openTestLogs=new Button{Text="打开测试记录",AutoSize=true,Enabled=false};
        Process testProcess;bool testCancelled;string testDirectory;
        internal JObject LastAutoTestResult {get;private set;}

        void InitializeAutoTestTab(){
            var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=38,WrapContents=false};
            bar.Controls.Add(stopTest);bar.Controls.Add(openTestLogs);bar.Controls.Add(testStatus);
            testTab.Controls.Add(testOutput);testTab.Controls.Add(bar);tabs.TabPages.Add(testTab);
            InitializeQaTestSettings();
            stopTest.Click += (sender, args) => {
                if (qaProcess != null) {
                    RequestQaStop();
                } else {
                    testCancelled = true;
                    StopTestProcess();
                }
            };
            openTestLogs.Click+=(s,e)=>{if(testDirectory!=null)Process.Start(new ProcessStartInfo("explorer.exe","\""+testDirectory+"\""){UseShellExecute=true});};
            FormClosing += (sender, args) => {
                if (qaProcess != null && !qaProcess.HasExited) {
                    RequestQaStop();
                    args.Cancel = true;
                    TestLine("正在清理测试数据，结束后可关闭窗口。");
                } else {
                    testCancelled = true;
                    StopTestProcess();
                }
            };
        }
        void StopTestProcess(){try{if(testProcess!=null&&!testProcess.HasExited)testProcess.Kill();}catch(InvalidOperationException){} }
        void TestLine(string line){if(line==null)return;Ui(()=>testOutput.AppendText(line+Environment.NewLine));}

        internal async Task RunAutoTests(){
            using(var single=new Semaphore(1,1,@"Local\FiccSalesTradingClient.AutoTests")){
                if(!single.WaitOne(0))throw new InvalidOperationException("另一个平台窗口正在运行自动测试。");
                try{
                    SetDiagnosticsVisible(true);tabs.SelectedTab=testTab;testOutput.Clear();testCancelled=false;LastAutoTestResult=null;
                    testDirectory=Path.Combine(client.LogDirectory,"autotest-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(testDirectory);
                    stopTest.Enabled=true;openTestLogs.Enabled=true;
                    TestLine("使用 trader1 固定测试账户。每个子进程最多 30 秒；失败即停止，不增加压力。");
                    testStatus.Text="1/5 正在检查 32 个报价及行情接口（3 条报价）…";
                    var apiResult=await RunTestProcess("--autotest","interfaces.json",false);
                    testStatus.Text="2/5 正在检查四类提醒（3 条报价）…";
                    var notificationResult=await RunTestProcess("--autotest-notifications","notifications.json",false);
                    testStatus.Text="3/5 正在检查界面操作（1 条报价）…";
                    TestLine("即将打开测试窗口，自动执行报价生命周期和筛选操作，完成后返回本窗口。");
                    var uiResult=await RunTestProcess("--autotest-ui","ui.json",true);
                    testStatus.Text="4/5 正在检查关注债券与筛选分组界面（2 个分组，0 条报价）…";
                    var groupResult=await RunTestProcess("--autotest-groups","groups.json",true);
                    testStatus.Text="5/5 CRM 对手选择与确认检查 (1 条报价)";
                    var counterpartyResult=await RunTestProcess("--autotest-counterparty","counterparty.json",true);
                    LastAutoTestResult=new JObject{["result"]="PASS",["interfaceChecks"]=apiResult["checks"].DeepClone(),["notificationChecks"]=notificationResult["checks"].DeepClone(),["uiChecks"]=uiResult["checks"].DeepClone(),["groupChecks"]=groupResult["checks"].DeepClone(),["quotesCreated"]=8,["counterpartyChecks"]=counterpartyResult["checks"].DeepClone(),["groupsCreated"]=2,["interfaces"]=apiResult,["notifications"]=notificationResult,["ui"]=uiResult,["groups"]=groupResult,["counterparty"]=counterpartyResult};
                    testStatus.Text="全部通过：接口 "+apiResult["checks"]+" 项，提醒 "+notificationResult["checks"]+" 项，界面 "+uiResult["checks"]+" 项，分组 "+groupResult["checks"]+" 项，CRM确认 "+counterpartyResult["checks"]+" 项";
                    TestLine(testStatus.Text);TestLine("每项结果中的 messageLogDirectory 指向完整收发消息日志。");
                }catch(Exception error){
                    LastAutoTestResult=new JObject{["result"]=testCancelled?"CANCELLED":"FAIL",["error"]=error.Message};
                    testStatus.Text=testCancelled?"测试已停止":"测试失败";TestLine(error.Message);
                }finally{
                    stopTest.Enabled=false;
                    if(testDirectory!=null&&LastAutoTestResult!=null)File.WriteAllText(Path.Combine(testDirectory,"summary.json"),LastAutoTestResult.ToString());
                    single.Release();
                }
            }
        }
        async Task<JObject> RunTestProcess(string mode,string file,bool showWindow){
            if(testCancelled)throw new OperationCanceledException("测试已停止。");
            string output=Path.Combine(testDirectory,file);
            using(var process=new Process()){
                process.StartInfo=new ProcessStartInfo(Application.ExecutablePath,mode+" \""+output+"\""){
                    WorkingDirectory=AppDomain.CurrentDomain.BaseDirectory,UseShellExecute=false,CreateNoWindow=true,
                    WindowStyle=showWindow?ProcessWindowStyle.Normal:ProcessWindowStyle.Hidden,
                    RedirectStandardOutput=true,RedirectStandardError=true
                };
                process.OutputDataReceived+=(s,e)=>TestLine(e.Data);process.ErrorDataReceived+=(s,e)=>TestLine(e.Data);
                testProcess=process;
                try{
                    process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();
                    bool completed=await Task.Run(()=>{bool done=process.WaitForExit(30000);if(!done&&!process.HasExited)process.Kill();return done;});
                    if(!completed){StopTestProcess();throw new TimeoutException("超过 30 秒，已终止本次测试子进程。");}
                    if(testCancelled)throw new OperationCanceledException("测试已停止。");
                    if(!File.Exists(output))throw new Exception("测试没有返回结果，退出码："+process.ExitCode);
                    var result=JObject.Parse(File.ReadAllText(output));
                    if(process.ExitCode!=0||(string)result["result"]!="PASS")throw new Exception((string)result["error"]??"测试失败，详情见结果文件。");
                    TestLine("结果："+output);TestLine("消息日志："+(string)result["messageLogDirectory"]);return result;
                }finally{testProcess=null;}
            }
        }
    }
}
