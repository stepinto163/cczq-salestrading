using common.netapi.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netapi.Api
{
    public abstract class AbstractApi : IApi
    {
        private List<string> logFilterTopList = new List<string>();
        /// <summary>
        /// 过滤指定topic日志不输出
        /// </summary>
        public List<string> LogFilterTopList { get { return logFilterTopList; } set { logFilterTopList = value; } }

        /// <summary>
        /// 登陆用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 组ID
        /// </summary>
        public string GroupId { get; set; }

        /// <summary>
        /// 配置CDH/报价 日志是否输出
        /// </summary>
        public string EnabledMsgLog { get; set; }

        /// <summary>
        /// sessionId
        /// 调用API接口前，必须先初始化
        /// 登录成功后赋值
        /// </summary>
        public string Sid { get; set; }

        public abstract string ApiKey { get; set; }

        protected int _timeOut = 30000;

        public bool isBDB { get; set; }

        public int TimeOut { get { return _timeOut; } set { _timeOut = value; } }
        
        public abstract object Login(LoginParam loginParam, ILoginHandler loginHandler = null);

        public abstract T Request<T>(string svrId, string topic, object paras);

        public abstract T Request<T>(string svrId, string topic, object paras, int waitTime);

        public abstract void SubScribeWithImage(Action<object> callBack, string[] topics, string svrId, string signalID);

        public abstract void SubScribe(Action<object> callBack, string[] topics, string svrId, string signalID);

        public abstract void UnSubScribe(string svrId, string[] topics);

        public abstract void DealMessage(object topic, object data);

        public string getTopic(string topic, string gw, string groupid, string pair, string tenor,
            string type)
        {
            string topicPreFix = topic + gw.ToLower() + "." + (string.IsNullOrEmpty(groupid) || isBDB ?"":string.Format("{0}.",groupid));
            return topicPreFix + getRic(gw, pair, tenor, type);
        }

        public string getRic(string gw, string pair, string tenor,
            string type)
        {
            string ric = "";
            if (string.IsNullOrEmpty(pair))
                pair = string.Empty;

            ric = string.Format("{0}{1}{2}={3}", pair.Replace(".", ""), tenor == null ? "" : tenor, type, gw.ToLower());
            return ric;
        }

        public abstract void DisConnect();

        public abstract void SubScribeConStatus(Action<object> callBack);

        public abstract void UnSubScribeConStatus(Action<Object> callBack);

        public abstract void Init();

        public abstract void SetGzip(bool isGzip);

        public abstract void NewApi();

        public abstract object GetApiConnect();
    }

   
}
