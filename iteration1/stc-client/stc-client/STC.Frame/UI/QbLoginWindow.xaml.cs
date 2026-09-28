using com.gateway.connector;
using com.gateway.log;
using common.netapi.Api;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Admin;
using STC.Common.Handler.Base;
using STC.Common.Model.Admin;
using STC.Common.Model.Fix.Message;
using STC.Common.Utils;
using STC.Common.Utils.Dps;
using STC.Frame.Update;
using STC.Frame.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EncrypterHelper = netapi.Api.EncrypterHelper;


namespace STC.Frame.UI
{
    /// <summary>
    /// QbLoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class QbLoginWindow : Window
    {
        public static List<string> ArgsList = new List<string>();

        private string _clientAutoExitTime = "";
        private DateTime _loginTime;
        private Thread _thread = null;

        public QbLoginWindow()
        {
            InitializeComponent();
            
            LogHelper.Info("QbLoginWindow path: {0}", System.AppDomain.CurrentDomain.BaseDirectory);
            Loaded += QbLoginWindow_Loaded;
            Closed += QbLoginWindow_Closed;
            this.Topmost = true;
            
            ThreadPool.QueueUserWorkItem(o => { login(); });
        }
        

        private void QbLoginWindow_Closed(object sender, EventArgs e)
        {
            LoginHelper.Instance.AppExit();
        }

        private void StartDps()
        {
            Task.Run(() =>
            {
                DpsUtil.Instance.StartDps();
            });

        }


        private void QbLoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LogHelper.Info("start login");
            ReadHistory();

            LogHelper.Info("after read hisotry");

            StartDps();

            STC.Common.View.Component.PopupEx.NativeMethods.ProcessMainWindowToFront();
            this.Topmost = false;
        }

        Window2 window2 = null;
        private const string LoginData = "Login";
        private const string ServerName = "ServerName";
        private void ReadHistory()
        {
            string serverName = IniFileHelper.ReadValue(LoginData, ServerName);

            if (!string.IsNullOrEmpty(serverName))
            {
                if (LocalEnv.ConfInfo.RemoteServerInfos != null && LocalEnv.ConfInfo.RemoteServerInfos.Count > 0)
                {
                    foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
                    {
                        if (LocalEnv.ConfInfo.RemoteServerInfos.Count > 0)
                        {
                            if (serverInfo.DpsHost == LocalEnv.ConfInfo.RemoteServerInfos[0].DpsHost)
                            {
                                LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = serverInfo;
                                LogHelper.Info("ServerInfo info:" + JsonConvert.SerializeObject(LocalEnv.ServerInfo));
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (LocalEnv.ConfInfo.ServerInfos != null && LocalEnv.ConfInfo.ServerInfos.Count > 0)
                    {
                        foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
                        {
                            if (serverInfo.Name.Equals(serverName))
                            {
                                LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = serverInfo;
                                LogHelper.Info("ServerInfo info:" + JsonConvert.SerializeObject(LocalEnv.ServerInfo));
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                LogHelper.Info("into read history");
                this.Dispatcher.Invoke(() =>
                {
                    LogHelper.Info("Conf info:" + JsonConvert.SerializeObject(LocalEnv.ConfInfo));
                    LogHelper.Info("ServerInfo info:" + JsonConvert.SerializeObject(LocalEnv.ServerInfo));
                });

                if (LocalEnv.ConfInfo.ServerInfos != null && LocalEnv.ConfInfo.ServerInfos.Count > 0)
                    LocalEnv.ServerInfo = LocalEnv.ConfInfo.ServerInfos[0];
            }
        }

        private void OnGateConnectStatus(object code)
        {
            if (code != null)
            {
                int status = (int)code;
                switch (status)
                {
                    case IEventListener.CONNECTION_CLOSED:
                        Console.WriteLine("CONNECTION_CLOSED");
                        LogHelper.Info("GateWay CONNECTION_CLOSED");

                        break;

                    case IEventListener.CONNECTION_LOGIN_SUCCESS:
                        Console.WriteLine("CONNECTION_LOGIN_SUCCESS");
                        LogHelper.Info("GateWay CONNECTION_LOGIN_SUCCESS");


                        ApiManager.Instance.GetDefaultApi().UnSubScribeConStatus(OnGateConnectStatus);

                        break;

                    case IEventListener.CONNECTION_SUCCESS:
                        Console.WriteLine("CONNECTION_SUCCESS");
                        LogHelper.Info("GateWay CONNECTION_SUCCESS");

                        break;

                    case IEventListener.CONNECTION_LOGOUT:
                        Console.WriteLine("CONNECTION_LOGOUT");

                        LogHelper.Info("GateWay CONNECTION_LOGOUT");

                        break;
                }
            }
        }


        private void login()
        {

            if (ArgsList.Count < 2)
            {
                return;
            }
            LocalEnv.UserInfo.UserName = ArgsList[0];
            //LocalEnv.UserInfo.UserName = "idb_trader09";
            LocalEnv.UserInfo.Password = ArgsList[1];
            LogHelper.Info("after set login param");
            bool isEncrypt = false;
            if (ArgsList.Count >= 3)
            {
                string str = ArgsList[2];
                isEncrypt = str == "encrypt";
            }

            if (ArgsList.Count >= 10)
            {
                string str = ArgsList[9];
                LocalEnv.UserInfo.CDCPermissions = str;
            }

            SetProcess(10);
            LogHelper.Info("check update begin");

            //this.Dispatcher.Invoke(() =>
            //{
            //    ClientUpdate.Instance.Init();
            //    ClientUpdate.Instance.StartUpdateClient(LocalEnv.ConfInfo.LoginType);
            //});

            LocalEnv.ConfInfo.IsOSVersionAfterWin7 = Environment.OSVersion.Version.CompareTo(new Version("6.2")) >= 0;

            ApiManager.Instance.GetApi(LocalEnv.ConfInfo.ApiKey).NewApi();

            ApiManager.Instance.SetDefaultApi(LocalEnv.ConfInfo.ApiKey);

            ApiManager.Instance.GetDefaultApi().SetGzip(LocalEnv.ConfInfo.IsGzip == "1");

            ApiManager.Instance.GetDefaultApi().SubScribeConStatus(OnGateConnectStatus);

            ApiManager.Instance.GetDefaultApi().isBDB = true;

            //增加日志过滤
            LogHelper.Info("FilterLog:" + LocalEnv.LogInfo.FilterLog);
            if (!string.IsNullOrEmpty(LocalEnv.LogInfo.FilterLog))
            {
                string[] logArr = LocalEnv.LogInfo.FilterLog.Split(new char[] { ',' });
                if (logArr != null && logArr.Length > 0)
                {
                    foreach (string str in logArr)
                    {
                        ApiManager.Instance.GetDefaultApi().LogFilterTopList.Add(str);
                    }
                }

            }


            LocalEnv.UserInfo.Password = EncrypterHelper.AesDecrypt(LocalEnv.UserInfo.Password, null);

            LoginParam loginParam = new LoginParam();
            loginParam.UserName = LocalEnv.UserInfo.UserName;
            loginParam.Pwd = LocalEnv.UserInfo.Password;
            loginParam.isEncrypt = false;
            if (LocalEnv.ConfInfo.StartDps == "1")
            {
                loginParam.Host = LocalEnv.ServerInfo.Host;
                int intPort = 0;
                int.TryParse(LocalEnv.ServerInfo.Port, out intPort);
                loginParam.Port = intPort;
                loginParam.Protocal = LocalEnv.ServerInfo.Protocol;
            }
            else
            {
                loginParam.Host = LocalEnv.ServerInfo.DpsHost;
                int intPort = 0;
                int.TryParse(LocalEnv.ServerInfo.DpsPort, out intPort);
                loginParam.Port = intPort;
                loginParam.Protocal = LocalEnv.ServerInfo.DpsProtocol;
            }

            loginParam.OtherParam = new LoginMsgListener();
            loginParam.Mac = EngineHelper.GetMacByNetworkInterface();

            if (STC.Common.Utils.LocalEnv.ConfInfo.StartDps != "1")
            {
                switch (LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        ProxyInfo proxyInfoQb = ProxyUtils.ReadQBProxyInfo();
                        loginParam.ProxyInfo = DpsUtil.Instance.ToTcpProxyInfo(proxyInfoQb);
                        break;
                    default:
                        ProxyInfo proxyInfo = ProxyUtils.ReadProxyInfo();
                        loginParam.ProxyInfo = DpsUtil.Instance.ToTcpProxyInfo(proxyInfo);
                        break;
                }
            }

            GWLoginHandler gWLoginHandler = new GWLoginHandler();
            string apiKey = ApiConsts.API_KEY_GW;
            loginParam.isGzip = LocalEnv.ConfInfo.IsGzip == "1";
            gWLoginHandler.LoginParam = loginParam;

            object loginObj = null;
            int loginCount = 0;

            while (true)
            {
                if (loginCount == 0)
                {
                    Thread.Sleep(2000);

                    SetProcess(30);
                }

                loginObj = ApiManager.Instance.GetApi(apiKey).Login(loginParam, gWLoginHandler);

                loginCount++;

                if (loginObj == null || string.IsNullOrEmpty(loginObj as string))
                {
                    Thread.Sleep(1000);

                    if (loginCount == 5)
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }




            LogHelper.Info("login result:" + JsonConvert.SerializeObject(loginObj));
            SetProcess(40);

            NetapiLoginResult loginResult = null;
            if (loginObj != null && !string.IsNullOrEmpty(loginObj as string))
            {
                string json = string.Empty;
                if (LocalEnv.ConfInfo.ApiKey.Equals(ApiConsts.API_KEY_CEDA))
                    json = LoginHandler.Instance.MsgCache["loginStr"] as string;//loginResult as string;//JsonConvert.SerializeObject(loginResult);
                else
                    json = loginObj as string;

                loginResult = JsonConvert.DeserializeObject<NetapiLoginResult>(json);

                {
                    DateTime dt = DateTimeEx.GetDatetime(loginResult.ServerTime);
                    LocalEnv.ServerInfo.Add_ServerTicks = dt.Ticks - DateTime.Now.Ticks + 12000000;// 1.2秒为网络延时
                    com.gateway.log.LogHelper.Info($"LoginThread  ServerTicks:{loginResult.ServerTime} ServerTime:{dt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                }

                if (LocalEnv.ConfInfo.ApiKey.Equals(ApiConsts.API_KEY_GW))
                {
                    LocalEnv.UserInfo.BonDPermissions = loginResult.BonDPermissions;

                    LocalEnv.UserInfo.ConnectPermissions = loginResult.ConnectPermissions;

                    if (!string.IsNullOrEmpty(LocalEnv.UserInfo.ConnectPermissions) && !LocalEnv.UserInfo.ConnectPermissions.Contains("6"))
                        LocalEnv.UserInfo.ConnectPermissions = "6," + LocalEnv.UserInfo.ConnectPermissions;//手动补齐 上田权限

                    LocalEnv.UserInfo.IDCPermissions = loginResult.IDCPermissions;

                    if (!string.IsNullOrEmpty(LocalEnv.UserInfo.IDCPermissions) && !LocalEnv.UserInfo.IDCPermissions.Contains("6"))
                        LocalEnv.UserInfo.IDCPermissions = "6," + LocalEnv.UserInfo.IDCPermissions;//手动补齐 上田权限

                    LocalEnv.UserInfo.BondTradePermissions = string.Empty;//"6," + loginResult.BondTradePermissions;
                    LocalEnv.UserInfo.IdcTradePermissions = string.Empty;//loginResult.IdcTradePermissions;

                    //BDB
                    LocalEnv.UserInfo.BrokerSort = loginResult.BrokerSort;
                    LocalEnv.UserInfo.BonDPermissions = loginResult.BonDPermissions ?? "6,5,4,3,2,1";
                    LocalEnv.UserInfo.ConnectPermissions = loginResult.ConnectPermissions;

                    if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BrokerSort) && !LocalEnv.UserInfo.BrokerSort.Contains("6"))
                        LocalEnv.UserInfo.BrokerSort = LocalEnv.UserInfo.BrokerSort + ",6";//手动补齐 上田权限

                    if (string.IsNullOrEmpty(LocalEnv.UserInfo.BrokerSort))
                        LocalEnv.UserInfo.BrokerSort = "1,2,3,4,5,6";

                    LocalEnv.UserInfo.BrokerSort = ResetBrokerSort(LocalEnv.UserInfo.BrokerSort);



                    //LocalEnv.UserInfo.IdcExcelPermissions = loginResult.IdcExcelPermissions;
                    //LocalEnv.UserInfo.IdcPythonPermissions = loginResult.IdcPythonPermissions;
                    //LocalEnv.UserInfo.IdcQTRSPermissions = loginResult.IdcQTRSPermissions;
                    LocalEnv.UserInfo.QeubeeFingPermissions = loginResult.QeubeeFinqPermissions;
                    LocalEnv.UserInfo.BdbInnerPermissions = loginResult.BdbInnerPermissions;
                    LocalEnv.UserInfo.BdbBboPermissions = loginResult.BdbBboPermissions;
                    LocalEnv.UserInfo.BdbStaratPermissions = loginResult.BdbStaratPermissions;
                    if (string.IsNullOrEmpty(LocalEnv.UserInfo.CDCPermissions))
                    {
                        LocalEnv.UserInfo.CDCPermissions = loginResult.CDCPermissions;
                    }

                    if (LocalEnv.UserInfo.QeubeeFingPermissions != "1")
                        LocalEnv.ConfInfo.IsForceUseWebFinQ = "0";
                }

                if (string.IsNullOrEmpty(loginResult.Code) || "0001".Equals(loginResult.Code + "", StringComparison.OrdinalIgnoreCase))
                {
                    LocalEnv.UserInfo.UserName = loginResult.userName;

                    BaseHandler.Instance.QueryTags();
                    SetProcess(50);
                    AdminHandler.Instance.stcUserQuery();
                    SetProcess(60);
                    AdminHandler.Instance.homeGroupSecurityidsQuery(null);
                    SetProcess(70);
                    RoleModulesActionQueryReq roleReq = new RoleModulesActionQueryReq();
                    if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                    {
                        StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.FirstOrDefault(item => item.Username == loginResult.userName);
                        if (currentUser != null)
                            loginResult.roleid = currentUser.Roleid;
                    }
                    roleReq.roleid = loginResult.roleid;

                    AdminHandler.Instance.roleModulesActionQuery(roleReq);

                    // 如果有需求工作台权限， 才调用此接口
                    if ((!CommonUtils.IsBDBAppName() && AdminHandler.Instance.CheckPermisionById("4000", "4008"))
                    || (CommonUtils.IsBDBAppName() && AdminHandler.Instance.CheckPermisionById("4000", "4008") && LocalEnv.UserInfo.BdbInnerPermissions == "1"))
                    {
                        AdminHandler.Instance.queryUserGroup();
                        AdminHandler.Instance.queryManagerUser();
                    }

                    SetProcess(80);

                    ApiManager.Instance.GetDefaultApi().Sid = LocalEnv.UserInfo.SessionID = loginResult.sid;
                    ApiManager.Instance.GetDefaultApi().UserName = loginResult.userName;
                    ApiManager.Instance.GetDefaultApi().GroupId = !string.IsNullOrEmpty(loginResult.groupids) ? ("G" + loginResult.groupids) : string.Empty;

                    LocalEnv.UserInfo.UserId = loginResult.userId;
                    LocalEnv.UserInfo.FullName = loginResult.Name;
                    LocalEnv.UserInfo.Termids = LocalEnv.UserInfo.biztypeid = loginResult.biztypeid ?? string.Empty;
                    LocalEnv.UserInfo.prods = string.IsNullOrEmpty(loginResult.prods) ? "BDB" : loginResult.prods;
                    LocalEnv.UserInfo.GroupIds = "G" + loginResult.groupids;
                    LocalEnv.UserInfo.GroupNames = loginResult.groupnames;
                    LocalEnv.UserInfo.RoleId = loginResult.roleid;
                    LocalEnv.UserInfo.RoleName = loginResult.rolename;
                    LocalEnv.UserInfo.SubCompanyId = loginResult.SubCompanyId;
                    LocalEnv.UserInfo.CompanyId = loginResult.companyId ?? "1";
                    LocalEnv.UserInfo.CompanyName = loginResult.companyName;
                    if (!string.IsNullOrEmpty(LocalEnv.UserInfo.CompanyName) && LocalEnv.UserInfo.CompanyName.Contains("麦高"))
                    {
                        LocalEnv.ConfInfo.RequiredIns = "1";
                    }
                    LocalEnv.UserInfo.QQNumber = CommonUtils.GetQQNumber(loginResult.userName);

                    string biztypeid = string.Empty;
                    string biztype = string.Empty;
                    CommonUtils.GetBizInfo(loginResult.userName, out biztypeid, out biztype);
                    //if (CommonUtils.IsBDBAppName())
                    //{
                        LocalEnv.UserInfo.Termids = LocalEnv.UserInfo.biztypeid = string.IsNullOrEmpty(biztypeid) ? "-1" : biztypeid;
                        LocalEnv.UserInfo.biztype = biztype;
                        TbookUserGroups currentUserGroup = AdminHandler.Instance.GetUserGroup(biztypeid);
                        if (currentUserGroup != null)
                        {
                            if (currentUserGroup.Inf4 == "1")
                            {
                                LocalEnv.UserInfo.RoleType = RoleType.BUYER_ROLE;// 1:买方机构 2：卖方机构
                            }
                            LocalEnv.UserInfo.biztype = currentUserGroup.Groupname;
                            //可查看的行情部门字段
                            if (!string.IsNullOrEmpty(currentUserGroup.mdgroupids))
                            {
                                string[] arr = currentUserGroup.mdgroupids.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                                LocalEnv.UserInfo.mdgroupids = new List<string>(arr);
                            }
                        }

                    //}
                    //else
                    //{
                    //    LocalEnv.UserInfo.Termids = biztypeid ?? string.Empty;
                    //}
                    

                    Task.Factory.StartNew(() => 
                    {
                        //等待加载Tags
                        while (!BaseHandler.Instance.FinishLoadTags())
                        {
                            Thread.Sleep(500);
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            SetProcess(80);

                            window2 = new Window2(this);
                            Application.Current.MainWindow = window2;
                            window2.Initialize();
                            SetProcess(100);

                            // 系统备妥
                            STC.Common.Data.DesignHandler.Instance.SystemIsReady(new object());

                            if (CommonUtils.IsBDBAppName() && LocalEnv.UserInfo.BdbInnerPermissions == "1")
                            {
                                _loginTime = DateTime.Now;
                                if (!string.IsNullOrEmpty(LocalEnv.ConfInfo.ClientAutoExitTime))
                                {
                                    LogHandler.Info($"clientAutoExitTime:{LocalEnv.ConfInfo.ClientAutoExitTime}");
                                    _clientAutoExitTime = LocalEnv.ConfInfo.ClientAutoExitTime;
                                    this.StartThread();
                                }
                            }
                        });
                    });
                }
                else
                {
                    SetProcess(0);
                    if (STC.Frame.Utils.ConstCode.Instance.isContainCode(loginResult.Code))
                        SetInfo(STC.Frame.Utils.ConstCode.Instance.getMsg(loginResult.Code));
                    else
                    {
                        SetInfo(loginResult.Msg);
                    }

                    ApiManager.Instance.GetApi(LocalEnv.ConfInfo.ApiKey).DisConnect();
                }
            }
            else
            {
                SetProcess(0);
                SetInfo("无法连接到服务器");
                ApiManager.Instance.GetApi(LocalEnv.ConfInfo.ApiKey).DisConnect();
            }
        }

        private void StartThread()
        {
            _thread = new Thread(CheckClientStatus);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void CheckClientStatus()
        {
            while (true)
            {
                try
                {
                    DateTime currentTime = DateTime.Now;

                    if (!string.IsNullOrEmpty(_clientAutoExitTime))
                    {
                        if (_clientAutoExitTime.ToString().ToLower() != "none")
                        {
                            DateTime clientAutoExitTime = Convert.ToDateTime(_clientAutoExitTime);
                            if (_loginTime.CompareTo(clientAutoExitTime) < 0)//登陆时间小于退出时间退出
                            {
                                if (currentTime.CompareTo(clientAutoExitTime) > 0)//当前时间大于退出时间
                                {
                                    Application.Current.Dispatcher.BeginInvoke((Action)(() => {
                                        DesignHandler.Instance.SystemExit(false);
                                    }));
                                    
                                    break;
                                }
                            }
                        }
                    }

                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    LogHelper.Error("CheckClientStatus error:" + e);
                    Thread.Sleep(5000);
                }
            }
        }

        private string ResetBrokerSort(string brokerSort)
        {
            string result = brokerSort;

            if (string.IsNullOrEmpty(brokerSort))
            {
                result = "1,2,3,4,5,6";
            }
            else
            {
                bool needOne = true;
                bool needTwo = true;
                bool needThree = true;
                bool needFour = true;
                bool needFive = true;
                bool needSix = true;
                string[] arr = brokerSort.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 0)
                {
                    foreach (string str in arr)
                    {
                        if (str == "1")
                        {
                            needOne = false;
                        }
                        else if (str == "2")
                        {
                            needTwo = false;
                        }
                        else if (str == "3")
                        {
                            needThree = false;
                        }
                        else if (str == "4")
                        {
                            needFour = false;
                        }
                        else if (str == "5")
                        {
                            needFive = false;
                        }
                        else if (str == "6")
                        {
                            needSix = false;
                        }
                    }

                    if (needOne)
                    {
                        result += ",1";
                    }
                    if (needTwo)
                    {
                        result += ",2";
                    }
                    if (needThree)
                    {
                        result += ",3";
                    }
                    if (needFour)
                    {
                        result += ",4";
                    }
                    if (needFive)
                    {
                        result += ",5";
                    }
                    if (needSix)
                    {
                        result += ",6";
                    }
                }
            }

            return result;
        }


        private void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            //SetProcess(100);
            try
            { }
            finally
            {
                this.Hide();
            }
        }

        private void SetInfo(string v)
        {
            this.Dispatcher.Invoke(() =>
            {
                tbInfo.Text = v;
            });
        }

        private void SetProcess(double v)
        {
            pbLoginProcess.Dispatcher.Invoke(() =>
            {
                tbProcess.Text = v + "%";
                pbLoginProcess.Visibility = Visibility.Visible;
                pbLoginProcess.Value = v;
            });
        }
        

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            //Application.Current.Shutdown();
            LoginHelper.Instance.AppExit();
        }
    }

    public class QbLoginResult
    {
        public string UserAccount { get; set; }
        public string Password { get; set; }
    }
}
