using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        internal async Task RunMultiUserScenario(){
            using(var single=new Semaphore(1,1,@"Local\FiccSalesTradingClient.AutoTests")){
                if(!single.WaitOne(0))throw new InvalidOperationException("另一个测试正在运行，请等待完成。");
                try{
                    SetDiagnosticsVisible(true);tabs.SelectedTab=testTab;testOutput.Clear();testCancelled=false;LastAutoTestResult=null;
                    testDirectory=Path.Combine(client.LogDirectory,"multi-user-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(testDirectory);
                    stopTest.Enabled=true;openTestLogs.Enabled=true;testStatus.Text="多用户场景：2 个用户、2 只债券、4 条报价，固定 15 步。";
                    TestLine("A1/A2 属于 trader1；B1/B2 属于 trader2。固定数量 9876 万用于隔离测试行情。已有相同条件的有效／待确认／撤出报价时拒绝开始。");
                    TestLine("顺序执行；每一步等待两个订阅者达到预期状态，失败停止并清理本次报价。全程最多 30 秒。");
                    LastAutoTestResult=await RunTestProcess("--autotest-multi-user","multi-user.json",false);
                    testStatus.Text="多用户场景通过：15 步，"+LastAutoTestResult["checks"]+" 项检查；4 条测试报价已删除。";TestLine(testStatus.Text);
                }catch(Exception error){LastAutoTestResult=new JObject{["result"]=testCancelled?"CANCELLED":"FAIL",["error"]=error.Message};testStatus.Text=testCancelled?"测试已停止":"多用户场景失败";TestLine(error.Message);}
                finally{stopTest.Enabled=false;if(testDirectory!=null&&LastAutoTestResult!=null)File.WriteAllText(Path.Combine(testDirectory,"summary.json"),LastAutoTestResult.ToString());single.Release();}
            }
        }
    }
}
