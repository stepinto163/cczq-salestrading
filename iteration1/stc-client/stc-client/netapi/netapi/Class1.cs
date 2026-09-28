using com.gateway.connector.tcp;
using netapi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi
{
    public class Class1
    {

        static void Main(string[] args)
        {
            //dps.utils.ProcessWatcher pw = new dps.utils.ProcessWatcher();
            try
            {

                string topic = "stc.sc.sysMessage.**";

                bool result = BaseHelper.IsMatch(new string[] { topic }, "stc.sc.sysMessage.idb_trader07");




                //pw.start();

                string host = "172.16.97.119";// "127.0.0.1";

                //Console.WriteLine("启动dps...");

                //GateWayApi client = new GateWayApi();
                //client.Eventlistener = new Eventlistener();//连接状态              
                //string loginResult1 = client.connect(host, 2000, "kong", "000000", false, 1);//连接
                //Console.WriteLine("request  result{0}:", loginResult1);


                ApiManager.Instance.SetDefaultApi("1");
                LoginParam loginParam = new LoginParam();
                loginParam.UserName = "rjq";
                loginParam.Pwd = "123456";
                loginParam.isEncrypt = false;
                loginParam.Host = host;
                int intPort = 2000;
                loginParam.Port = intPort;
                loginParam.Protocal = "TCP";
                //loginParam.OtherParam = new LoginMsgListener();
                object loginResult = ApiManager.Instance.GetDefaultApi().Login(loginParam);







                //GateWayApi client = new GateWayApi();
                //client.Eventlistener = new Eventlistener();//连接状态              
                //string loginResult = client.connect(host, 2000, "kong", "000000", false, 1);//连接
                Console.WriteLine("request  result{0}:", loginResult);
                int count = 1;


                Console.ReadLine();



            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);

            }
            Console.ReadLine();
            //pw.stop();

        }

    }
}
