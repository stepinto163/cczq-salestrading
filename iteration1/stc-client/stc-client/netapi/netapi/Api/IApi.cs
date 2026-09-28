using com.gateway.connector.tcp;
using common.netapi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public interface IApi
    {
        List<string> LogFilterTopList { get; set; }
        string GroupId { get; set; }
        string UserName { get; set; }
        string EnabledMsgLog { get; set; }
        string Sid { get; set; }

        string ApiKey { get; set; }

        int TimeOut { get; set; }

        bool isBDB { get; set; }

        object Login(LoginParam loginParam, ILoginHandler loginHandler = null);

        void UnSubScribe(string svrId, string[] topics);

        void SubScribeWithImage(Action<object> callBack, string[] topics, string svrId, string signalID);

        void SubScribe(Action<object> callBack, string[] topics, string svrId, string signalID);

        T Request<T>(string svrId, string topic, object paras);

        T Request<T>(string svrId, string topic, object paras, int waitTime);

        void SubScribeConStatus(Action<object> callBack);

        void UnSubScribeConStatus(Action<Object> callBack);

        void DealMessage(object topic, object data);

        string getTopic(string topic, string gw,string groupid, string pair, string tenor,
            string type);

        string getRic(string gw, string pair, string tenor,
            string type);


        void DisConnect();

        void Init();

        void SetGzip(bool isGzip);

        void NewApi();
        //void SubScribeConnnetStatus(Action<int> action);

        object GetApiConnect();

    }

    public class LoginParam
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Protocal { get; set; }
        public string UserName { get; set; }
        public string Pwd { get; set; }
        public bool isEncrypt { get; set; }
        public string Mac { get; set; }
        public string IsManager { get; set; }

        public object OtherParam { get; set; }

        public bool isGzip { get; set; }

        public ProxyInfo ProxyInfo { get; set; }
        public override string ToString()
        {
            return string.Format("Host:{0}, Port:{1}, Protocal:{2}, UserName:{3}, Pwd:{4}, isEncrypt:{5}, Mac:{6}, IsManager:{7}，isGzip:{8}",
                Host, Port, Protocal, UserName, string.Empty, isEncrypt, Mac, IsManager, isGzip);
        }
    }
}
