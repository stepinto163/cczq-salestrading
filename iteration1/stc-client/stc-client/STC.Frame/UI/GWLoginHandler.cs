using Ats.Foundation.Message;
using com.gateway.connector.tcp;
using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Utils.Dps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace STC.Frame.UI
{
    public class GWLoginHandler : ILoginHandler
    {
        private LoginParam loginParam;

        public LoginParam LoginParam { get { return loginParam; } set { loginParam = value; } }

        public object Login(string username, string password, bool isEncrypt, string host, string port, string protocal, object loginMsgListener)
        {
            string loginResult = string.Empty;
            int intProtocal = 0;

            try
            {
                intProtocal = GetProtocal(protocal);

                Dictionary<string, object> extendParams = new Dictionary<string, object>();
                extendParams.Add("isManage", "N");// 客户端登录 Y-管理端 , N-客户端
                loginParam.Mac = loginParam.Mac ?? STC.Common.Utils.EngineHelper.GetMacByNetworkInterface();
                extendParams.Add("mac", loginParam.Mac);

                Dictionary<string, string> jsonDic = new Dictionary<string, string>();
                jsonDic.Add("versionId", STC.Common.Utils.LocalEnv.ServerInfo.Version);
                string json = MsgHelper.Serializer(jsonDic);
                extendParams.Add("infs", json);

                string pwd = loginParam.Pwd;
                if (!loginParam.isEncrypt)
                {
                    //外围系统登录验证方式
                    pwd = EncrypterHelper.AesEncrypt(Encoding.UTF8.GetBytes(loginParam.Pwd), null);
                }
                extendParams.Add("password", pwd);
                extendParams.Add("isEncrypt", "Y");
                extendParams.Add("ValidCode", STC.Common.Utils.LocalEnv.UserInfo.validCode);
                extendParams.Add("ValidCodeId", STC.Common.Utils.LocalEnv.UserInfo.validCodeId);

                List<string> ip = STC.Common.Utils.EngineHelper.GetLocalIpAddress(AddressFamily.InterNetwork.ToString());
                if (ip != null && ip.Count > 0)
                    extendParams.Add("ip", ip[0]);


                //内部系统登录验证方式
                Dictionary<string, object> pwdDic = null;
                if (loginParam.isEncrypt)
                {
                    pwdDic = JsonConvert.DeserializeObject<Dictionary<string, object>>(loginParam.Pwd);//MsgHelper.Deserialize<Dictionary<string, object>>(loginParam.Pwd);
                }
                else
                {
                    pwdDic = EncrypterHelper.getEncryptDic(loginParam.Pwd);
                }

                if (pwdDic != null && pwdDic.Count == 2)
                {
                    if (extendParams.ContainsKey("isEncrypt"))
                        extendParams.Remove("isEncrypt");
                    extendParams.Add("isEncrypt", "Y");
                    foreach (var key in pwdDic.Keys)
                    {
                        if (extendParams.ContainsKey(key))
                            extendParams.Remove(key);
                        extendParams.Add(key, pwdDic[key]);
                    }
                }

                if (STC.Common.Utils.LocalEnv.ConfInfo.StartDps == "1")
                {
                    SetServerInfo(STC.Common.Utils.LocalEnv.ServerInfo, extendParams);
                }

                if (!string.IsNullOrEmpty(STC.Common.Utils.LocalEnv.ConfInfo.ClientType))
                {
                    extendParams.Add("clientType", STC.Common.Utils.LocalEnv.ConfInfo.ClientType);
                }

                List<string> filterServerList = new List<string>();
                object apiConnect = ApiManager.Instance.GetDefaultApi().GetApiConnect();
                if (apiConnect != null)
                {
                    GateWayApi _gateWayApi = apiConnect as GateWayApi;
                    if (_gateWayApi != null)
                    {
                        if (STC.Common.Utils.LocalEnv.ConfInfo.StartDps != "1")
                        {
                            _gateWayApi.ProxyInfo = loginParam.ProxyInfo;
                        }
                       
                        loginResult = _gateWayApi.connect(loginParam.Host, loginParam.Port, loginParam.UserName, loginParam.Pwd, loginParam.isEncrypt, intProtocal, extendParams);
                        if (loginResult != null && !string.IsNullOrEmpty(loginResult))
                        {
                            string json2 = loginResult as string;
                            try
                            {
                                common.netapi.Api.NetapiLoginResult temLogin = JsonConvert.DeserializeObject<common.netapi.Api.NetapiLoginResult>(json2);
        
                                //如果登录成功
                                if (string.IsNullOrEmpty(temLogin.Code) || "0001".Equals(temLogin.Code + "", StringComparison.OrdinalIgnoreCase))
                                {
                                    STC.Common.Handler.Base.BaseHandler.Instance.SetloginOk(true);
                                    SubscribeLoginMessage(loginMsgListener);
                                    //兼容BDB
                                    STC.Common.Utils.ServerInfo si = STC.Common.Utils.LocalEnv.ServerInfo;
                                    si.Host = si.DpsHost;
                                    si.Port = si.DpsPort;
                                    si.Protocol = si.DpsProtocol;
                                    si.AppImage = null;
                                    BDBServerInfo bsi = STC.Common.Utils.CommonUtils.DeserializeObject<BDBServerInfo>(STC.Common.Utils.CommonUtils.SerializeObject(si));
                                    bsi.Protocal = si.DpsProtocol;
                                    string bsitext = STC.Common.Utils.CommonUtils.SerializeObject(bsi);
                                    com.gateway.log.LogHelper.Info("WriteServerInfo:" + bsitext);
                                    STC.Common.Utils.IniFileHelper.WriteValue("Login", "ServerInfo", bsitext);
                                    STC.Common.Utils.IniFileHelper.WriteValue("Login", "ServerName", bsi.Name);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error($"Login1 {STC.Common.Utils.LocalEnv.ServerInfo.DpsHost} Error: {ex.Message}", ex);
                                switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
                                {
                                    case "bdb":
                                    case "idcclient":
                                        loginResult = RetryLogin(filterServerList, STC.Common.Utils.LocalEnv.ServerInfo, extendParams, _gateWayApi, intProtocal);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
                            {
                                case "bdb":
                                case "idcclient":
                                    loginResult = RetryLogin(filterServerList, STC.Common.Utils.LocalEnv.ServerInfo, extendParams, _gateWayApi, intProtocal);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"Login2 {STC.Common.Utils.LocalEnv.ServerInfo.DpsHost} Error: {ex.Message}", ex);
            }
            
            return loginResult;
        }

        //BDB Add

        private void SubscribeLoginMessage(object loginMsgListener)
        {
            LoginMsgListener listener = loginMsgListener as LoginMsgListener;
            if (listener == null)
            {
                listener = new LoginMsgListener();
            }
            ApiManager.Instance.GetDefaultApi().SubScribe(listener.OnNetMessage, new string[] { "SYS.ATS.LOGIN" }, ApiConsts.GateWay, "");
        }

        private string RetryLogin(List<string> filterServerList, STC.Common.Utils.ServerInfo serverInfo, Dictionary<string, object> extendParams, GateWayApi _gateWayApi, int intProtocal)
        {
            LogHelper.Info("GWLoginHandler into RetryLogin");
            string loginResult = string.Empty;
            filterServerList.Add(serverInfo.DpsHost);
            foreach (STC.Common.Utils.ServerInfo info in STC.Common.Utils.LocalEnv.ConfInfo.ServerInfos)
            {
                if (filterServerList.Contains(info.DpsHost))
                    continue;

                if (STC.Common.Utils.LocalEnv.ConfInfo.StartDps == "1")
                {
                    loginParam.Host = info.Host;
                    int intPort = 0;
                    int.TryParse(info.Port, out intPort);
                    loginParam.Port = intPort;
                    loginParam.Protocal = info.Protocol;
                }
                else
                {
                    loginParam.Host = info.DpsHost;
                    int intPort = 0;
                    int.TryParse(info.DpsPort, out intPort);
                    loginParam.Port = intPort;
                    loginParam.Protocal = info.DpsProtocol;
                }

                intProtocal = GetProtocal(loginParam.Protocal);

                if (STC.Common.Utils.LocalEnv.ConfInfo.StartDps == "1")
                {
                    SetServerInfo(info, extendParams);
                }

                if (!string.IsNullOrEmpty(STC.Common.Utils.LocalEnv.ConfInfo.ClientType))
                {
                    extendParams.Add("clientType", STC.Common.Utils.LocalEnv.ConfInfo.ClientType);
                }

                STC.Common.Utils.LocalEnv.ServerInfo = info;

                object apiConnect = ApiManager.Instance.GetDefaultApi().GetApiConnect();
                _gateWayApi = apiConnect as GateWayApi;
                if (STC.Common.Utils.LocalEnv.ConfInfo.StartDps != "1")
                {
                    _gateWayApi.ProxyInfo = loginParam.ProxyInfo;
                }
                loginResult = _gateWayApi.connect(loginParam.Host, loginParam.Port, loginParam.UserName, loginParam.Pwd, loginParam.isEncrypt, intProtocal, extendParams);

                if (loginResult != null && (!string.IsNullOrEmpty(loginResult as string)))
                {
                    string json2 = loginResult as string;
                    try
                    {
                        common.netapi.Api.NetapiLoginResult temLogin = JsonConvert.DeserializeObject<common.netapi.Api.NetapiLoginResult>(json2);
                        if (temLogin != null)
                        {
                            //如果登录成功
                            if (string.IsNullOrEmpty(temLogin.Code) || "0001".Equals(temLogin.Code + "", StringComparison.OrdinalIgnoreCase))
                            {
                                STC.Common.Handler.Base.BaseHandler.Instance.SetloginOk(true);
                                SubscribeLoginMessage(null);
                                //兼容BDB
                                STC.Common.Utils.ServerInfo si = STC.Common.Utils.LocalEnv.ServerInfo;
                                si.Host = si.DpsHost;
                                si.Port = si.DpsPort;
                                si.Protocol = si.DpsProtocol;
                                si.AppImage = null;
                                BDBServerInfo bsi = STC.Common.Utils.CommonUtils.DeserializeObject<BDBServerInfo>(STC.Common.Utils.CommonUtils.SerializeObject(si));
                                bsi.Protocal = si.DpsProtocol;
                                string bsitext = STC.Common.Utils.CommonUtils.SerializeObject(bsi);
                                com.gateway.log.LogHelper.Info("WriteServerInfo:" + bsitext);
                                STC.Common.Utils.IniFileHelper.WriteValue("Login", "ServerInfo", bsitext);
                                STC.Common.Utils.IniFileHelper.WriteValue("Login", "ServerName", bsi.Name);
                            }

                            break;
                        }
                        else
                        {
                            filterServerList.Add(info.DpsHost);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error($"RetryLogin {info.DpsHost} Error: {ex.Message}");
                        filterServerList.Add(info.DpsHost);
                    }
                }
                else
                {
                    filterServerList.Add(info.DpsHost);
                }
            }

            return loginResult;
        }

        private void SetServerInfo(STC.Common.Utils.ServerInfo serverInfo, Dictionary<string, object> extendParams)
        {
            string host = "host";
            string port = "port";
            string protocol = "protocol";
            if (serverInfo != null)
            {
                if (extendParams.ContainsKey(host))
                {
                    extendParams.Remove(host);
                    extendParams.Add(host, serverInfo.DpsHost);
                }
                else
                {
                    extendParams.Add(host, serverInfo.DpsHost);
                }

                if (extendParams.ContainsKey(port))
                {
                    extendParams.Remove(port);
                    extendParams.Add(port, serverInfo.DpsPort);
                }
                else
                {
                    extendParams.Add(port, serverInfo.DpsPort);
                }

                int intProtocal = GetProtocal(serverInfo.DpsProtocol);

                if (extendParams.ContainsKey(protocol))
                {
                    extendParams.Remove(protocol);
                    extendParams.Add(protocol, intProtocal);
                }
                else
                {
                    extendParams.Add(protocol, intProtocal);
                }
            }
            else
            {
                if (extendParams.ContainsKey(host))
                    extendParams.Remove(host);

                if (extendParams.ContainsKey(port))
                    extendParams.Remove(port);

                if (extendParams.ContainsKey(protocol))
                    extendParams.Remove(protocol);
            }
        }

        private int GetProtocal(string protocal)
        {
            int intProtocal = 0;

            if (protocal == "TCP" || protocal == "PROTOCOL_TCP")
            {
                intProtocal = 1;
            }
            else if (protocal == "TCPS" || protocal == "PROTOCOL_TCPS")
            {
                intProtocal = 2;
            }
            else if (protocal == "HTTP" || protocal == "PROTOCOL_HTTP")
            {
                intProtocal = 3;
            }
            else if (protocal == "HTTPS" || protocal == "PROTOCOL_HTTPS")
            {
                intProtocal = 4;
            }

            return intProtocal;
        }
    }
}
