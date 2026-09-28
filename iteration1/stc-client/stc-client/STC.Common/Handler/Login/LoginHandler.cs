using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ats.Foundation.Message;
using STC.Common.Model.Login;
using STC.Common.Api;
using netapi.Api;
using com.gateway.log;
using System.Collections.Concurrent;

namespace STC.Common.Utils
{
    public class LoginHandler : ILoginHandler
    {
        private static LoginHandler loginHandler = new LoginHandler();
        public static LoginHandler Instance { get { return loginHandler; } }
        private LoginHandler()
        {
        }

        private Dictionary<string, object> msgCache = new Dictionary<string, object>();
        public bool isLoginSuccess = false;
        //private ClientInfo info = null;        

        public Dictionary<string, object> MsgCache { get { return msgCache; } set { msgCache = value; } }

        public object Login(string username, string password, bool isEncrypt, 
            string host, string port, string protocal, object loginMsgListener)
        {
            LoginResult map = null;
            return map;
        }

        public string getLoginInfo(string username,string password)
        {
            Dictionary<string, string> p = new Dictionary<string, string>();
            p.Add("userName", username);
            p.Add("password", password);

            string pstr = MsgHelper.Serializer(p);
            //return EncrypterHelper.getLoginInfo(pstr);

            return MsgHelper.Serializer(p);
        }

        
        public bool IsMySelf(string userId)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(userId) && msgCache != null && msgCache.ContainsKey("login"))
            {
                object data = msgCache["login"];
                if (data != null)
                {
                    LoginResult loginData = data as LoginResult;
                    if (loginData != null && loginData.userId.Equals(userId))
                        result = true;
                }
            }

            return false;
        }

        private ConcurrentDictionary<string, string> qqDic = new ConcurrentDictionary<string, string>();
        public QQAccountReply QueryAccount(string userid)
        {
            return ApiManager.Instance.GetDefaultApi().Request<QQAccountReply>(ApiConsts.LOGINSVR, ApiConsts.queryAccount_Topic, "{\"userId\":\"" + userid + "\"}");
        }

        //int index = 0;
        //public async Task<string> GetQQ(string securtityType, string orgId)
        //{
        //    if (qqDic.Count == 0 && index < 3)
        //    {
        //        index++;
        //        await Task.Run(() =>
        //        {
        //            QQAccountReply reply = QueryAccount(LocalEnv.UserInfo.UserId);
        //            if (reply != null && reply.IsSuccess())
        //            {
        //                foreach (QQItem qqItem in reply.Datas)
        //                {
        //                    if (qqItem.qq == null)
        //                    {
        //                        continue;
        //                    }
        //                    string k = $"{GetTp(qqItem.astParentTp)}#{qqItem.orgId}";
        //                    qqDic[k] = qqItem.qq.ToString();
        //                }
        //            }
        //        });
        //    }
        //    string key = $"{securtityType}#{orgId}";
        //    if (qqDic.ContainsKey(key))
        //    {
        //        return qqDic[key].ToString();
        //    }
        //    return string.Empty;
        //}

        private string GetTp(string s)
        {
            string tp = "";
            switch (s)
            {
                case "利率债":
                    tp = "BNC";
                    break;
                case "信用债":
                    tp = "BOC";
                    break;
                default:
                    tp = s;
                    break;
            }
            return tp;
        }
    }
}
