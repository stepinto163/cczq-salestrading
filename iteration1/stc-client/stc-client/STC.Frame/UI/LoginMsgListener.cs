using Ats.Foundation.Message;
using com.gateway.log;
using netapi.Api;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Frame.Utils;
using System;

namespace STC.Frame.UI
{
    public class LoginMsgListener
    {
        public LoginHandler LoginHandler { get; set; }

        public void OnNetMessage(object data)
        {
            try
            {
                NetMessage netMessage = data as NetMessage;
                if (netMessage == null || !"SYS.ATS.LOGIN".Equals(netMessage.Topic))
                {
                    return;
                }

                HandleLoginMessage(netMessage.Topic, netMessage.Data);
            }
            catch (Exception e)
            {
                LogHelper.Error(e.Message, e);
            }
        }

        private void HandleLoginMessage(string topic, string result)
        {
            LoginResult map = MsgHelper.Deserialize<LoginResult>(result);
            if (map == null)
            {
                return;
            }

            if ("logout".Equals(map.type))
            {
                if (IsCurrentSessionLogout(map))
                {
                    string logoutMessage = GetLogoutMessage(map);
                    LogHelper.Info("Login logout message received, sid:{0}, reason:{1}, msg:{2}", map.sid, map.reason, logoutMessage);
                    LoginHelper.Instance.SetLogoutMessage(logoutMessage);
                    LoginHelper.Instance.Logout2(logoutMessage);
                }
                return;
            }

            if (!string.IsNullOrEmpty(map.sid)
                && !string.IsNullOrEmpty(LocalEnv.UserInfo.SessionID)
                && !string.Equals(LocalEnv.UserInfo.SessionID, map.sid, StringComparison.Ordinal))
            {
                return;
            }

            LocalEnv.UserInfo.BonDPermissions = map.BonDPermissions;
            LocalEnv.UserInfo.ConnectPermissions = map.ConnectPermissions;
            LocalEnv.UserInfo.IDCPermissions = map.IDCPermissions;
            LocalEnv.UserInfo.BondTradePermissions = map.BondTradePermissions;
            LocalEnv.UserInfo.IdcTradePermissions = map.IdcTradePermissions;

            ApiManager.Instance.GetDefaultApi().Sid = LocalEnv.UserInfo.SessionID = map.sid;
            ApiManager.Instance.GetDefaultApi().UserName = map.userName;
            ApiManager.Instance.GetDefaultApi().EnabledMsgLog = LocalEnv.ConfInfo.EnabledMsgLog;
            ApiManager.Instance.GetDefaultApi().GroupId = !string.IsNullOrEmpty(map.groupids) ? ("G" + map.groupids) : string.Empty;

            LoginHandler.Instance.MsgCache["login"] = map;
            LoginHandler.MsgCache["loginStr"] = result;
        }

        private bool IsCurrentSessionLogout(LoginResult map)
        {
            return !string.IsNullOrEmpty(map.sid) && string.Equals(LocalEnv.UserInfo.SessionID, map.sid, StringComparison.Ordinal);
        }

        private string GetLogoutMessage(LoginResult map)
        {
            const string defaultMessage = "账号已在其他地方登录，请重新登录！";
            string reason = map.reason;
            string msg = map.msg;

            if ("USER_DISABLED".Equals(reason, StringComparison.OrdinalIgnoreCase))
            {
                return "账号已被禁用，请联系管理员";
            }

            if (!string.IsNullOrWhiteSpace(msg))
            {
                return msg;
            }

            return defaultMessage;
        }
    }
}
