using Ats.Foundation.Message;
using MDT.Tools.Wpf.AutoUpdater.DiyMessageBox;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Threading;
using STC.Common.Utils;
using STC.Common.Model.Login;
using STC.Common.Model.Admin;
using STC.Common.Handler.Admin;
using netapi.Api;
using com.gateway.connector;
using common.netapi.Api;
using STC.Common.Api;
using Newtonsoft.Json;
using com.gateway.log;
using STC.Frame.Update;
using ServerInfo = STC.Common.Utils.ServerInfo;
using LocalEnv = STC.Common.Utils.LocalEnv;
using STC.Common.Data;
using STC.Common.View.UI;
using STC.Common.Utils.Dps;
using ConstCode = STC.Frame.Utils.ConstCode;
using STC.Common.Handler.Base;
using System.IO;

namespace STC.Frame.UI
{
    /// <summary>
    /// LoginGUI.xaml 的交互逻辑
    /// </summary>
    public partial class LoginGUI : Window
    {
        private const string LoginData = "Login";
        private const string UserName = "UserName";
        private const string Password = "Password";
        private const string Password2 = "Password2";
        private const string ServerName = "ServerName";
        private const string Number = "Number";
        private const string AutoLogin = "AutoLogin";
        private const string IsTest = "IsTest";

        private bool IsAutoLogin = false;
        
        private bool isMemory = false;
        private string memoryPwd = null;
        private string memoryPwdNum = null;
        Window1 window1 = null;
        Window2 window2 = null;
        private string _clientAutoExitTime = "";
        private DateTime _loginTime;
        private Thread _thread = null;

        public LoginGUI()
        {
            InitializeComponent();
            //初始化服务器列表
            foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
                comboBoxServer.Items.Add(serverInfo.Name);

            //默认选中第一个
            if (comboBoxServer.Items.Count > 0)
                comboBoxServer.SelectedIndex = 0;

            this.Loaded += LoginGUI_Loaded;
            pbPwd.GotKeyboardFocus += PbPwd_GotKeyboardFocus;
            pbPwd.PasswordChanged += PbPwd_PasswordChanged;
            pbPwd.LostKeyboardFocus += PbPwd_LostKeyboardFocus;
            this.Icon = LocalEnv.ServerInfo.AppImage;
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

        private void PbPwd_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(pbPwd.Password) && !string.IsNullOrWhiteSpace(memoryPwd))
            {
                if (!string.IsNullOrEmpty(memoryPwdNum))
                {
                    int num = 0;
                    string temPwd = string.Empty;
                    if (int.TryParse(memoryPwdNum, out num))
                    {
                        for (int i = 0; i < num; i++)
                            temPwd += "*";
                    }
                    pbPwd.Password = temPwd;
                }
                else
                    pbPwd.Password = memoryPwd;
                isMemory = true;
            }
        }

        private void PbPwd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            isMemory = false;
        }

        private void PbPwd_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (isMemory)
                pbPwd.Password = "";
        }

        private void StartDps()
        {
            Task.Run(() =>
            {
                DpsUtil.Instance.StartDps();
            });
        }

        private async void RestartDps()
        {
            await Task.Run(() => {
                DpsUtil.Instance.StopDps();
            });
        }

        private void LoginGUI_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.SetMaxThreads(50, 50);// 设置线程池最大线程数量
            ThreadPool.SetMinThreads(8, 8);

            tbVersion.Text = string.Format("{0}", LocalEnv.ServerInfo.Version);

            pbLoginProcess.Visibility = Visibility.Collapsed;

            ReadHistory();

            StartDps();

            this.comboBoxServer.SelectionChanged += comboBoxServer_SelectionChanged;

            LogoMenuIcon lmi = new LogoMenuIcon();

            switch (LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "dmart":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/dmart-logo-2x2.png";
                    lmi.BackImage = "pack://application:,,,/STC.Frame;component/Images/stc_image_loginguanli_bg.png";
                    lmi.Stretch = "Uniform";
                    lmi.Opacity = "1";
                    lmi.Width = "120";
                    lmi.Margin = "0,10,0,0";
                    break;
                case "gtja":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/stc_gtja_login.png";
                    lmi.BackImage = "pack://application:,,,/STC.Frame;component/Images/stc_image_loginguanli_bg.png";
                    lmi.Stretch = "Uniform";
                    lmi.Width = "190";
                    lmi.Opacity = "0.9";
                    lmi.Margin = "-50,10,0,0";
                    break;
                case "idbpro":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/idc_logo.png";
                    lmi.BackImage = "pack://application:,,,/STC.Frame;component/Images/stc_image_loginguanli_bg.png";
                    lmi.Stretch = "Uniform";
                    lmi.Opacity = "1";
                    lmi.Width = "120";
                    lmi.Margin = "0,30,0,0";
                    break;
                case "bdb":
                case "idcclient":
                    lmi.LogoImage = "pack://application:,,,/STC.Common;component/Images/bdb_logo.png";
                    lmi.BackImage = "pack://application:,,,/STC.Frame;component/Images/bdb_image_loginguanli_bg.png";
                    lmi.Stretch = "Uniform";
                    lmi.Opacity = "1";
                    if (LocalEnv.ConfInfo.IsUserHotBackupLogin)
                    {
                        lmi.Width = "134";
                        lmi.Margin = "0,36,0,-10";
                        btnSetting.Visibility = Visibility.Hidden;
                        serverBorder.Visibility = Visibility.Collapsed;
                        UserStackPanel.SetValue(Grid.RowProperty, 3);
                        PassStackPanel.SetValue(Grid.RowProperty, 4);
                        tbVersion.Margin = new Thickness(0, 0, 0, 0);
                    }
                    else
                    {
                        lmi.Width = "120";
                        lmi.Margin = "0,0,0,0";
                    }
                    break;
  
                default:
                    lmi.LogoImage = "pack://application:,,,/STC.Frame;component/Images/stc_logo.png";
                    lmi.BackImage = "pack://application:,,,/STC.Frame;component/Images/stc_image_loginguanli_bg.png";
                    lmi.Stretch = "Uniform";
                    lmi.Opacity = "1";
                    lmi.Width = "120";
                    lmi.Margin = "0,0,0,0";
                    break;
            }

            LoginLogo.DataContext = lmi;
            FullBorder.DataContext = lmi;
            logoBorder.DataContext = lmi;
            //生产
            if (!LocalEnv.ConfInfo.IsTest)
            {
                //btnLoginPrd.Visibility = Visibility.Collapsed;
                //btnLoginTest.Visibility = Visibility.Visible;
                this.BtnLoginPrd_Click(null, null);
                //btnPrdEnvironment.Visibility = Visibility.Visible;
                //btnTestEnvironment.Visibility = Visibility.Collapsed;
            }
            else
            {
                //btnLoginPrd.Visibility = Visibility.Visible;
                //btnLoginTest.Visibility = Visibility.Collapsed;
                this.BtnLoginTest_Click(null, null);
                //btnPrdEnvironment.Visibility = Visibility.Collapsed;
                //btnTestEnvironment.Visibility = Visibility.Visible;
            }

            string istart = System.Environment.GetEnvironmentVariable("isStart");

            LogHelper.Info("istart:" + istart);

            if (!string.IsNullOrEmpty(istart) && istart.Equals("1") && !string.IsNullOrEmpty(LocalEnv.ConfInfo.AutoStart) && LocalEnv.ConfInfo.AutoStart.Equals("1"))
                IsAutoLogin = true;

            if (IsAutoLogin)
            {
                btnLogin_Click(null, null);
            }
            //btnLogin_Click(null, null);

            STC.Common.Handler.Base.BaseHandler.Instance.SubscribeLoginOk(OnLoginOk);
        }

        
        private void ReadHistory()
        {
            if (netapi.Api.LocalIntegrationProfile.Current.Enabled)
            {
                cbRemember.IsEnabled = false;
                cbAutoLogin.IsEnabled = false;
                if (LocalEnv.ConfInfo.ServerInfos == null || LocalEnv.ConfInfo.ServerInfos.Count == 0)
                {
                    throw new InvalidOperationException("Local integration requires a configured loopback gateway.");
                }
                LocalEnv.ServerInfo = LocalEnv.ConfInfo.ServerInfos[0];
                LocalEnv.UserInfo.ServerInfo = LocalEnv.ServerInfo;
                comboBoxServer.SelectedIndex = 0;
                return;
            }
            ReadStcHistory();
        }

        private void ReadStcHistory()
        {
            string username = IniFileHelper.ReadValue(LoginData, UserName);
            //string pwd = IniFileHelper.ReadValue(LoginData, Password);
            //string pwd2 = IniFileHelper.ReadValue(LoginData, Password2);
            string serverName = IniFileHelper.ReadValue(LoginData, ServerName);
            memoryPwdNum = IniFileHelper.ReadValue(LoginData, Number);
            string autoLogin = IniFileHelper.ReadValue(LoginData, AutoLogin);


            string pwd = IniFileHelper.ReadValue(LoginData, Password);
            if (!string.IsNullOrWhiteSpace(pwd) && !string.IsNullOrWhiteSpace(username))
            {
                memoryPwd = netapi.Api.EncrypterHelper.Decrypt(pwd, username);
                memoryPwd = netapi.Api.EncrypterHelper.AesDecrypt(memoryPwd, null);
                pbPwd.Password = memoryPwd;

                isMemory = true;
            }


            bool.TryParse(autoLogin, out IsAutoLogin);

            if (IsAutoLogin)
                cbAutoLogin.IsChecked = true;
            else
                cbAutoLogin.IsChecked = false;

            if (!string.IsNullOrWhiteSpace(username))
                tbUsername.Text = username;
            if (!string.IsNullOrWhiteSpace(pwd) && !string.IsNullOrWhiteSpace(username))
            {
                cbRemember.IsChecked = true;
                //memoryPwd = EncrypterHelper.Decrypt(pwd, username);
                //int num = 0;
                //string temPwd = string.Empty;
                //if (int.TryParse(memoryPwdNum, out num))
                //{
                //    for (int i = 0; i < num; i++)
                //        temPwd += "*";
                //}
                //pbPwd.Password = temPwd;
                //isMemory = true;
            }
            //LocalEnv.UserInfo.Password2 = pwd2;


            switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    if (!string.IsNullOrEmpty(serverName))
                    {
                        if (LocalEnv.ConfInfo.RemoteServerInfos != null && LocalEnv.ConfInfo.RemoteServerInfos.Count > 0)
                        {
                            int _Index = 0;
                            foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
                            {
                                if (LocalEnv.ConfInfo.RemoteServerInfos.Count > 0)
                                {
                                    if (serverInfo.DpsHost == LocalEnv.ConfInfo.RemoteServerInfos[0].DpsHost)
                                    {
                                        comboBoxServer.Text = serverInfo.Name;
                                        comboBoxServer.SelectedIndex = _Index;
                                        LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = serverInfo;
                                        break;
                                    }

                                    _Index++;
                                }
                            }
                        }
                        else
                        {
                            //兼容BDB老的配置，如果存在服务器，则取服务器名称
                            if (LocalEnv.ConfInfo.ServerInfos.Where(o => o.Name == serverName).ToList().Count > 0)
                            {
                                int _Index = 0;
                                foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
                                {
                                    if (serverInfo.Name.Equals(serverName))
                                    {
                                        comboBoxServer.Text = serverInfo.Name;
                                        comboBoxServer.SelectedIndex = _Index;
                                        LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = serverInfo;
                                        break;
                                    }

                                    _Index++;
                                }
                            }
                            else
                            {
                                if (LocalEnv.ConfInfo.ServerInfos != null && LocalEnv.ConfInfo.ServerInfos.Count > 0)
                                    LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = LocalEnv.ConfInfo.ServerInfos[0];

                                comboBoxServer.SelectedIndex = 0;
                            }
                        }
                    }
                    else
                    {
                        if (LocalEnv.ConfInfo.ServerInfos != null && LocalEnv.ConfInfo.ServerInfos.Count > 0)
                            LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = LocalEnv.ConfInfo.ServerInfos[0];

                        comboBoxServer.SelectedIndex = 0;
                    }

                    break;
                default:

                    if (!string.IsNullOrEmpty(serverName))
                    {
                        bool isContain = false;

                        foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
                        {
                            if (serverInfo.Name.Equals(serverName))
                            {
                                isContain = true;
                                LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = serverInfo;
                                break;
                            }
                        }

                        if (isContain)
                        {
                            comboBoxServer.Text = serverName;
                        }
                        else
                        {
                            if (LocalEnv.ConfInfo.ServerInfos != null && LocalEnv.ConfInfo.ServerInfos.Count > 0)
                                LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = LocalEnv.ConfInfo.ServerInfos[0];

                            comboBoxServer.SelectedIndex = 0;
                        }
                    }
                    else
                    {
                        if (LocalEnv.ConfInfo.ServerInfos != null && LocalEnv.ConfInfo.ServerInfos.Count > 0)
                            LocalEnv.ServerInfo = LocalEnv.UserInfo.ServerInfo = LocalEnv.ConfInfo.ServerInfos[0];

                        comboBoxServer.SelectedIndex = 0;
                    }

                    break;
            }

            
        }

        private void SetLogoBorder(string id)
        {
            if (id.Equals("1"))
            {
                logoBorder.SetValue(Grid.ColumnProperty, 0);
            }
            else if (id.Equals("2"))
            {
                logoBorder.SetValue(Grid.ColumnProperty, 1);
            }
        }

        private void WriteHistory()
        {
            if (netapi.Api.LocalIntegrationProfile.Current.Enabled)
            {
                return;
            }
            WriteStcHistory();
        }

        private void WriteStcHistory()
        {
            Task.Run(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (cbRemember.IsChecked.HasValue && cbRemember.IsChecked.Value)
                    {
                        //string pwd = pbPwd.Password;
                        //if (!isMemory)
                        //{
                        //    var pwddic = EncrypterHelper.getEncryptDic(pbPwd.Password);
                        //    pwd = MsgHelper.Serializer(pwddic);
                        //}
                        //else
                        //    pwd = memoryPwd;

                        //pwd = EncrypterHelper.Encrypt(pwd, tbUsername.Text);                                  
                        IniFileHelper.WriteValue(LoginData, UserName, tbUsername.Text);
                        //IniFileHelper.WriteValue(LoginData, Password, pwd);
                        //IniFileHelper.WriteValue(LoginData, Password2, LocalEnv.UserInfo.Password2);
                        //IniFileHelper.WriteValue(LoginData, Number, pbPwd.Password.Length.ToString());

                        string pwd = netapi.Api.EncrypterHelper.AesEncrypt(Encoding.UTF8.GetBytes(pbPwd.Password), null);
                        pwd = netapi.Api.EncrypterHelper.Encrypt(pwd, tbUsername.Text);
                        IniFileHelper.WriteValue(LoginData, Password, pwd);
                    }
                    else
                    {
                        IniFileHelper.WriteValue(LoginData, UserName, string.Empty);
                        IniFileHelper.WriteValue(LoginData, Password, string.Empty);
                        //IniFileHelper.WriteValue(LoginData, Password2, string.Empty);
                        //IniFileHelper.WriteValue(LoginData, Number, string.Empty);
                    }

                    if (cbAutoLogin.IsChecked.HasValue && cbAutoLogin.IsChecked.Value)
                    {
                        IniFileHelper.WriteValue(LoginData, AutoLogin, cbAutoLogin.IsChecked.Value.ToString());
                    }
                    else
                        IniFileHelper.WriteValue(LoginData, AutoLogin, "false");

                });
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            STC.Frame.Utils.LoginHelper.Instance.AppExit();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void SetInfo(string str)
        {
            this.Dispatcher.Invoke(() =>
            {
                pbLoginProcess.Visibility = Visibility.Collapsed;

                btnLogin.Content = "登录";
                btnLogin.IsEnabled = true;

                tbTips.Visibility = Visibility.Visible;
                tbTips.Text = str;

                tbLoginPercent.Text = string.Empty;
            }
            );
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            btnLogin.IsEnabled = false;
            btnLogin.Content = "登录中...";
            if (string.IsNullOrWhiteSpace(tbUsername.Text))
            {
                SetProcess(0);
                SetInfo("请填写用户名后再登录！");
                return;
            }

            if (string.IsNullOrWhiteSpace(pbPwd.Password))
            {
                SetProcess(0);
                SetInfo("请填写密码后再登录！");
                return;
            }

            LocalEnv.UserInfo.UserName = tbUsername.Text;
            LocalEnv.UserInfo.Password = pbPwd.Password;
            LocalEnv.UserInfo.validCode = tbVlidCode.Text;
            if (isMemory)
            {
                LocalEnv.UserInfo.Password = memoryPwd;

                if (string.IsNullOrEmpty(memoryPwd))
                {
                    SetProcess(0);
                    SetInfo("密码失效，请重新输入");
                    return;
                }
            }

            tbTips.Visibility = Visibility.Collapsed;
            pbLoginProcess.Visibility = Visibility.Visible;

            SetProcess(10);

            if (!netapi.Api.LocalIntegrationProfile.Current.Enabled)
            {
                ClientUpdate.Instance.Init();
            }

            SetProcess(15);

            LocalEnv.ConfInfo.IsOSVersionAfterWin7 = Environment.OSVersion.Version.CompareTo(new Version("6.2")) >= 0;

            string severName = comboBoxServer.Text;

            Task.Run(() => 
            {
                if (netapi.Api.LocalIntegrationProfile.Current.Enabled)
                {
                    login();
                }
                else if (LocalEnv.ConfInfo.ClientUpdateType == "2")
                {
                    bool isUpdate = ClientUpdate.Instance.NeedUpdateClient(string.Empty);
                    SetProcess(20);
                    if (isUpdate)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SysWarnGUI messageBox = new SysWarnGUI();

                            messageBox.setWarnMsg("检查到新版本，请更新");
                            messageBox.Owner = Application.Current.MainWindow;
                            messageBox.Topmost = true;
                            messageBox.Show();

                            SetProcess(0);

                            btnLogin.IsEnabled = true;
                            btnLogin.Content = "登录";

                            return;
                        }));
                    }
                    else
                    {
                        Task.Run(() => { login(); });
                    }
                }
                else if (string.IsNullOrEmpty(LocalEnv.ConfInfo.ClientUpdateType) || LocalEnv.ConfInfo.ClientUpdateType == "1")
                {
                    //如果是STC，如果是BDB允许进入
                    switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
                    {
                        case "bdb":
                        case "idcclient":
                            break;
                        default:
                            ClientUpdate.Instance.StartUpdateClient(string.Empty);
                            break;
                    }

                    Task.Run(() => { login(); });
                }
                else if ("3".Equals(LocalEnv.ConfInfo.ClientUpdateType))
                {
                    Task.Run(() => { login(); });
                }
            });
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
            SetProcess(20);


            LogHelper.Info("LogPath:" + LocalEnv.LogInfo.LogPath);
            LogHelper.Info("DataPath:" + LocalEnv.ConfInfo.DataPath);

            ApiManager.Instance.GetApi(LocalEnv.ConfInfo.ApiKey).NewApi();

            ApiManager.Instance.SetDefaultApi(LocalEnv.ConfInfo.ApiKey);

            ApiManager.Instance.GetDefaultApi().SetGzip(LocalEnv.ConfInfo.IsGzip == "1");

            ApiManager.Instance.GetDefaultApi().SubScribeConStatus(OnGateConnectStatus);

            //增加日志过滤
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
                    string json = loginObj as string;

                    NetapiLoginResult loginResult = JsonConvert.DeserializeObject<NetapiLoginResult>(json);

                    //避免断网重连导致导致时间错乱
                    //if (string.IsNullOrEmpty(LocalEnv.UserInfo.UserId))
                    {
                        DateTime dt = DateTimeEx.GetDatetime(loginResult.ServerTime);
                        LocalEnv.ServerInfo.Add_ServerTicks = dt.Ticks - DateTime.Now.Ticks + 12000000;// 1.2秒为网络延时
                        com.gateway.log.LogHelper.Info($"LoginThread  ServerTicks:{loginResult.ServerTime} ServerTime:{dt.ToString("yyyy-MM-dd HH:mm:ss.fff")}");
                    }

                    break;
                }
            }


            string filename = DpsUtil.Instance.GetDpsFullName();
            string fullfilename = System.AppDomain.CurrentDomain.BaseDirectory + @"\" + filename;

            if ((!string.IsNullOrEmpty(filename)) && (!System.IO.File.Exists(filename)))
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    string errmsg = $"文件{filename}不存在！";
                    LogHelper.Error(errmsg);
                    ShowSysWarnGUI(errmsg);
                    btnLogin.IsEnabled = true;
                    btnLogin.Content = "登录";
                    SetProcess(0);
                }));

                return;
            }

            int RetryTime = 0;
            bool isFoundDpsProc = false;
            while (RetryTime < 20)
            {
                if (!IsFoundProc(filename))
                {
                    RetryTime++;
                    Thread.Sleep(500);
                    continue;
                }
                else
                {
                    isFoundDpsProc = true;
                    break;
                }
            }

            if (!isFoundDpsProc)
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    string errmsg = "没有找到 dps 进程！\r\n请重试一下！";
                    LogHelper.Error(errmsg);
                    ShowSysWarnGUI(errmsg);
                    btnLogin.IsEnabled = true;
                    btnLogin.Content = "登录";
                    SetProcess(0);
                }));

                ApiManager.Instance.GetApi(LocalEnv.ConfInfo.ApiKey).DisConnect();
                return;
            }
           

            LogHelper.Info(loginObj);
            SetProcess(40);
            // 登录线程
            LoginThread(loginObj);

        }


        private void LoginThread(object loginObj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NetapiLoginResult loginResult = null;

                if (loginObj != null && !string.IsNullOrEmpty(loginObj as string))
                {
                    string json = loginObj as string;

                    loginResult = JsonConvert.DeserializeObject<NetapiLoginResult>(json);


                    if (string.IsNullOrEmpty(loginResult.Code) || "0001".Equals(loginResult.Code + "", StringComparison.OrdinalIgnoreCase))
                    {
                        LocalEnv.UserInfo.UserName = loginResult.userName;
                        BaseHandler.Instance.QueryTags();
                        SetProcess(50);
                        AdminHandler.Instance.stcUserQuery();

                        RoleModulesActionQueryReq roleReq = new RoleModulesActionQueryReq();
                        if (AdminHandler.Instance.GetCachestcUserQuery() != null && AdminHandler.Instance.GetCachestcUserQuery().Datas != null)
                        {
                            StcTbookUsers currentUser = AdminHandler.Instance.GetCachestcUserQuery().Datas.FirstOrDefault(item => item.Username == loginResult.userName);
                            if (currentUser != null)
                                loginResult.roleid = currentUser.Roleid;
                        }
                        
                        roleReq.roleid = loginResult.roleid;

                        //STC.Common.Handler.SystemParam.SystemParamHandler.Instance.Init();
                        AdminHandler.Instance.roleModulesActionQuery(roleReq);

                        // 如果有需求工作台权限， 才调用此接口
                        if ((!CommonUtils.IsBDBAppName() && AdminHandler.Instance.CheckPermisionById("4000", "4008"))
                            || (CommonUtils.IsBDBAppName() && AdminHandler.Instance.CheckPermisionById("4000", "4008") && LocalEnv.UserInfo.BdbInnerPermissions == "1"))
                        {
                            AdminHandler.Instance.queryUserGroup();
                            AdminHandler.Instance.queryManagerUser();
                        }

                        SetProcess(60);
                        HomeGroupSecurityidsUpdateReq request = new HomeGroupSecurityidsUpdateReq();
                        request.type = CommonConstants.HOME_Type;
                        request.username = loginResult.userName;
                        AdminHandler.Instance.homeGroupSecurityidsQuery(request);
                        SetProcess(70);

                        WriteHistory();

                        ApiManager.Instance.GetDefaultApi().Sid = LocalEnv.UserInfo.SessionID = loginResult.sid;
                        LocalEnv.UserInfo.Sid = loginResult.usid;
                        ApiManager.Instance.GetDefaultApi().UserName = loginResult.userName;
                        ApiManager.Instance.GetDefaultApi().GroupId = !string.IsNullOrEmpty(loginResult.groupids) ? ("G" + loginResult.groupids) : string.Empty;

                        LocalEnv.UserInfo.UserId = loginResult.userId;
                        LocalEnv.UserInfo.FullName = loginResult.Name;
                        LocalEnv.UserInfo.Termids = LocalEnv.UserInfo.biztypeid = loginResult.biztypeid ?? string.Empty;
                      

                        switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
                        {
                            case "bdb":
                            case "idcclient":
                                LocalEnv.UserInfo.prods = "BDB";
                                ApiManager.Instance.GetDefaultApi().isBDB = true;
                                break;
                            default:
                                LocalEnv.UserInfo.prods = string.IsNullOrEmpty(loginResult.prods) ? "STC" : loginResult.prods;
                                ApiManager.Instance.GetDefaultApi().isBDB = false;
                                break;
                        }

                        LocalEnv.UserInfo.GroupIds = "G" + loginResult.groupids;
                        LocalEnv.UserInfo.GroupNames = loginResult.groupnames;
                        LocalEnv.UserInfo.RoleId =  loginResult.roleid;
                        LocalEnv.UserInfo.RoleName = loginResult.rolename;
                        LocalEnv.UserInfo.SubCompanyId = loginResult.SubCompanyId;
                        LocalEnv.UserInfo.CompanyId = loginResult.companyId ?? "1";
                        LocalEnv.UserInfo.CompanyName = loginResult.companyName;
                        if (!string.IsNullOrEmpty(LocalEnv.UserInfo.CompanyName) && LocalEnv.UserInfo.CompanyName.Contains("麦高"))
                        {
                            LocalEnv.ConfInfo.RequiredIns = "1";
                        }

                        //BDB
                        LocalEnv.UserInfo.BrokerSort = loginResult.BrokerSort;
                        LocalEnv.UserInfo.BonDPermissions = loginResult.BonDPermissions ?? "6,5,4,3,2,1";
                        LocalEnv.UserInfo.ConnectPermissions = loginResult.ConnectPermissions;

                        if (!string.IsNullOrEmpty(LocalEnv.UserInfo.BrokerSort) && !LocalEnv.UserInfo.BrokerSort.Contains("6"))
                            LocalEnv.UserInfo.BrokerSort = LocalEnv.UserInfo.BrokerSort + ",6";//手动补齐 上田权限

                        if (string.IsNullOrEmpty(LocalEnv.UserInfo.BrokerSort))
                            LocalEnv.UserInfo.BrokerSort = "1,2,3,4,5,6";

                        LocalEnv.UserInfo.BrokerSort = ResetBrokerSort(LocalEnv.UserInfo.BrokerSort);

                        //BDB 新曾字段 todo...
                        ////LocalEnv.UserInfo.IdcExcelPermissions = loginResult.IdcExcelPermissions;
                        ////LocalEnv.UserInfo.IdcPythonPermissions = loginResult.IdcPythonPermissions;
                        ////LocalEnv.UserInfo.IdcQTRSPermissions = loginResult.IdcQTRSPermissions;
                        LocalEnv.UserInfo.QeubeeFingPermissions = loginResult.QeubeeFinqPermissions;
                        LocalEnv.UserInfo.BdbInnerPermissions = loginResult.BdbInnerPermissions;
                        LocalEnv.UserInfo.BdbBboPermissions = loginResult.BdbBboPermissions;
                        LocalEnv.UserInfo.BdbStaratPermissions = loginResult.BdbStaratPermissions;
                        LocalEnv.UserInfo.QQNumber = CommonUtils.GetQQNumber(loginResult.userName);

                        if (LocalEnv.UserInfo.QeubeeFingPermissions != "1")
                            LocalEnv.ConfInfo.IsForceUseWebFinQ = "0";

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


                        if (string.IsNullOrEmpty(LocalEnv.UserInfo.CDCPermissions))
                        {
                            LocalEnv.UserInfo.CDCPermissions = loginResult.CDCPermissions;
                        }
                        //LocalEnv.UserInfo.BdbStaratPermissions = "1";
                        //LocalEnv.UserInfo.BdbInnerPermissions = "1";
                        //LocalEnv.UserInfo.CDCPermissions = "6";
                        Login70(loginResult);
                    }
                    else
                    {
                        if (LocalEnv.ConfInfo.ValidCodeVisible && "NEED_RAND_CODE".Equals(loginResult.Code))
                        {
                            try
                            {
                                Dictionary<string, object> map = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                                string data1 = (string)map["data"];
                                Dictionary<string, object> map2 = JsonConvert.DeserializeObject<Dictionary<string, object>>(data1);
                                string data2 = (string)map2["data"];
                                string reqId = (string)map2["requestId"];
                                LocalEnv.UserInfo.validCodeId = reqId;
                                LogHelper.Info("login:" + data2);
                                LogHelper.Info("login:" + LocalEnv.UserInfo.validCodeId);
                                string base64Image = data2.Split(',')[1];
                                this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                                {
                                    var bitmapImage = new BitmapImage();
                                    try
                                    {
                                        // 1. 解码Base64字符串为字节数组
                                        byte[] imageBytes = Convert.FromBase64String(base64Image);

                                        // 2. 创建内存流并写入字节数组
                                        using (var stream = new MemoryStream(imageBytes))
                                        {
                                            // 3. 创建BitmapImage并加载流
                                            bitmapImage.BeginInit();
                                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // 立即加载并释放流
                                            bitmapImage.StreamSource = stream;
                                            bitmapImage.EndInit();
                                            // bitmapImage.Freeze(); // 可选：冻结对象以提高性能（线程安全）
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Base64转ImageSource失败: {ex.Message}");
                                    }
                                    this.Height = 450;
                                    loGrid.RowDefinitions[5].Height = new GridLength(50);

                                    IMVlidCode.Source = bitmapImage;
                                    tbVlidCode.Visibility = Visibility.Visible;
                                }));
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error(ex);
                            }
                        } 
                        SetProcess(0);
                        if (STC.Frame.Utils.ConstCode.Instance.isContainCode(loginResult.Code))
                            SetInfo(STC.Frame.Utils.ConstCode.Instance.getMsg(loginResult.Code));
                        else
                        {
                            SetInfo(loginResult.Msg);
                        }

                        try
                        {
                            ApiManager.Instance.GetApi(LocalEnv.ConfInfo.ApiKey).DisConnect();
                        }
                        catch { }
                    }
                }
                else
                {
                    SetProcess(0);
                    SetInfo("无法连接到服务器");

                    try
                    {
                        ApiManager.Instance.GetApi(LocalEnv.ConfInfo.ApiKey).DisConnect();
                    }
                    catch { }
                }
            });
        }


        private async void Login70(NetapiLoginResult loginResult)
        {
            SetProcess(70);
            await Task.Run(() => {
                // 等待加载Tags
                while (!BaseHandler.Instance.FinishLoadTags())
                {
                    Thread.Sleep(500);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    switch (LocalEnv.ServerInfo.AppName.ToLower())
                    {
                        case "bdb":
                        case "idcclient":
                            window2 = new Window2(this);
                            Application.Current.MainWindow = window2;
                            window2.Initialize();
                            break;
                        default:
                            window1 = new Window1(this);
                            Application.Current.MainWindow = window1;
                            //window1.Loaded += Window1_Loaded;
                            window1.Initialize();
                            break;

                    }
                });
            });
            
            Login90(loginResult);
        }

        private async void Login90(NetapiLoginResult loginResult)
        {
            await Task.Delay(1);
            SetProcess(80);

            Application.Current.Dispatcher.Invoke(() =>
            {
                // 系统备妥
                DesignHandler.Instance.SystemIsReady(new object());

                SetProcess(100);
                _loginTime = DateTime.Now;
                if (!string.IsNullOrEmpty(loginResult.clientAutoExitTime))
                {
                    LogHandler.Info($"clientAutoExitTime:{loginResult.clientAutoExitTime}");
                    _clientAutoExitTime = loginResult.clientAutoExitTime;
                    this.StartThread();
                }
                if (!string.IsNullOrEmpty(loginResult.passwordExpireMsg))
                {
                    TSafeMangerPo reply = JsonConvert.DeserializeObject<TSafeMangerPo>(loginResult.passwordExpireMsg);
                    if (reply != null && "1019".Equals(reply.Code))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateUserPwdView _updateUserPwdView = new UpdateUserPwdView();
                            _updateUserPwdView.SetDefaultUpdateDay(reply.updatePwdTime, reply.powerPwd == "1" ? true : false, true);
                            _updateUserPwdView.ShowDialog();

                        });
                        return;
                    }
                }
            });
         
        }


        private void ShowSysWarnGUI(string msg)
        {
            SysWarnGUI messageBox = new SysWarnGUI();
            messageBox.setWarnMsg(msg);
            messageBox.Owner = Application.Current.MainWindow;
            messageBox.Topmost = true;
            messageBox.ShowDialog();
        }
        
        private bool IsFoundProc(string dpsname)
        {
            if (string.IsNullOrEmpty(dpsname))
                return true;

            dpsname = System.IO.Path.GetFileNameWithoutExtension(dpsname);
            Process[] arrayProcess = Process.GetProcesses();
            bool isFound = false;
            foreach (Process p in arrayProcess)
            {
                if (dpsname.ToLower() == p.ProcessName.ToLower())
                {
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }

        private string GetDpsFullName()
        {
            string dpsname = string.Empty;
            string startDps = LocalEnv.ConfInfo.StartDps;
            if (!string.IsNullOrEmpty(startDps) && startDps.Equals("1"))
            {
                string env = string.Empty;
                if (LocalEnv.ConfInfo.LoginEnv.Contains("."))
                {
                    env = LocalEnv.ConfInfo.LoginEnv.Split('.')[0];
                }
                else
                {
                    env = LocalEnv.ConfInfo.LoginEnv;
                }

                if (env == "PRD" || "PRO".Equals(env, StringComparison.OrdinalIgnoreCase))
                {
                    dpsname = "dps.exe";
                }
                else
                {
                    dpsname = string.Format("{0}-{1}.exe", "dps", LocalEnv.ConfInfo.LoginEnv);
                }
                
            }
            return dpsname;
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
                                    DesignHandler.Instance.SystemExit(true);
                                    break;
                                }
                            }
                        }
                    }

                    Thread.Sleep(5000);
                }
                catch (Exception e)
                {
                    LogHelper.Error("CheckClientStatus error:"+ e);
                    Thread.Sleep(5000);
                }
            }
        }



        //LoginResult dic = LoginHandler.Instance.Login(LocalEnv.UserInfo.UserName, LocalEnv.UserInfo.Password, isMemory,
        //        LocalEnv.ServerInfo.Host, LocalEnv.ServerInfo.Port, LocalEnv.ServerInfo.Protocal, new LoginMsgListener(), LocalEnv.ServerInfo.Version);
        //    LogHelper.Info(dic);
        //    SetProcess(40);
        //    if (dic != null)
        //    {
        //        if ("0001".Equals(dic.Code + "", StringComparison.OrdinalIgnoreCase))
        //        {

        //            RoleModulesActionQueryReq roleReq = new RoleModulesActionQueryReq();
        //            roleReq.roleid = dic.roleid;

        //            AdminHandler.Instance.roleModulesActionQuery(roleReq);
        //            SetProcess(60);
        //            AdminHandler.Instance.homeGroupSecurityidsQuery();
        //            SetProcess(70);
        //            AdminHandler.Instance.stcUserQuery();

        //            WriteHistory();

        //            LocalEnv.UserInfo.UserId = dic.userId;
        //            LocalEnv.UserInfo.FullName = dic.Name;
        //            LocalEnv.UserInfo.Termids = dic.accountTermIds;
        //            LocalEnv.UserInfo.prods = string.IsNullOrEmpty(dic.prods) ? "STC" : dic.prods;
        //            LocalEnv.UserInfo.GroupIds = "G" + dic.groupids;
        //            LocalEnv.UserInfo.GroupNames = dic.groupnames;

        //            DateTime dt = DateTimeEx.GetDatetime(dic.ServerTime);
        //            LocalEnv.ServerInfo.Add_ServerTicks = dt.Ticks - DateTime.Now.Ticks;



        //            this.Dispatcher.Invoke(() =>
        //            {
        //                SetProcess(80);

        //                window1 = new Window1();
        //                window1.Loaded += Window1_Loaded;
        //                window1.Initialize();
        //                Application.Current.MainWindow = window1;
        //                SetProcess(100);
        //                window1.Show();
        //                //SetProcess(100);
        //                //this.Hide();
        //            });
        //        }
        //        else
        //        {
        //            SetInfo(ConstCode.Instance.getMsg(dic.Code));
        //        }
        //    }
        //    else
        //    {
        //        SetInfo("无法连接到服务器");
        //    }
        //}

        #region 版本检查
        //MDT.Tools.Wpf.AutoUpdater.AutoUpdater autoUpdater = new MDT.Tools.Wpf.AutoUpdater.AutoUpdater();
        //检查版本，自动更新
        //private void versionCheck(string autoUpdateUrl)
        //{
        //try
        //{
        //autoUpdater.SetServerUrl(autoUpdateUrl);
        //LogHelper.Debug("Version check:IsUpdate begin");
        //int isUpdate = autoUpdater.IsUpdate();
        //LogHelper.Debug("Version check:IsUpdate end");
        //if (isUpdate > 0)
        //{
        //    int pid = Process.GetCurrentProcess().Id;
        //    string fileName = Process.GetCurrentProcess().MainModule.FileName + "|" + "admin" + "|" + "Beta版 本:4.0.0.00001";
        //    var b = Encoding.Default.GetBytes(fileName);
        //    fileName = Convert.ToBase64String(b);
        //    if (isUpdate == 2)
        //    {
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            MessageBoxOkCancel messageBox = new MessageBoxOkCancel();
        //            messageBox.Owner = this;
        //            messageBox.ShowDialog();
        //            if (messageBox.resultBox == ResultBox.Ok)
        //            {
        //                //uc为正确的url配置名，修改为uctemp表示不配置更新url
        //                LogHelper.Info("AutoUpdater Parameter: " + string.Format("-i {0} -c {1} -uc {2} -p {3} -e {4}", isUpdate, false, autoUpdateUrl, pid, fileName));
        //                Process.Start("MDT.Tools.AutoUpdater.exe", string.Format("-i {0} -c {1} -uctemp {2} -p {3} -e {4}", isUpdate, false, autoUpdateUrl, pid, fileName));
        //                LogHelper.Info(string.Format("AutoUpdate Logout [{0}]", isUpdate));
        //                Environment.Exit(0);
        //            }
        //        });
        //    }
        //    else if (isUpdate == 3)
        //    {
        //        LogHelper.Info("AutoUpdater Parameter: " + string.Format("-i {0} -c {1} -uc {2} -p {3} -e {4}", isUpdate, false, autoUpdateUrl, pid, fileName));
        //        Process.Start("MDT.Tools.AutoUpdater.exe", string.Format("-i {0} -c {1} -uctemp {2} -p {3} -e {4}", isUpdate, false, autoUpdateUrl, pid, fileName));
        //        LogHelper.Info(string.Format("AutoUpdate Logout [{0}]", isUpdate));
        //        Environment.Exit(0);
        //    }

        //    }
        //}
        //catch (Exception ex)
        //{
        //    LogHelper.Warn("check version failed", ex);
        //}
        //}

        #endregion

        public async void SetProcess(double v)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            //this.Cursor = this.pbPwd.Cursor = this.tbUsername.Cursor = Cursors.Wait;
                            this.tbUsername.ForceCursor = true;
                            this.pbPwd.ForceCursor = true;

                            pbLoginProcess.Visibility = Visibility.Visible;
                            pbLoginProcess.Value = v;
                            tbLoginPercent.Text = string.Format("{0}%", v);
                            double opvi = (100 - v) / 100.0;
                            //this.Opacity = opvi;
                            if (opvi >= 1)
                            {
                                //this.Cursor = this.pbPwd.Cursor = this.tbUsername.Cursor = Cursors.Arrow;
                                this.pbPwd.ForceCursor = false;
                                this.tbUsername.ForceCursor = false;
                            }
                        });
                    }
                    
                }
                catch(Exception e)
                {
                    LogHelper.Error(e);
                }
            });
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        { 
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnLoginPrd_Click(object sender, RoutedEventArgs e)
        {
            //ResetServerInfo("1");

            //LocalEnv.ConfInfo.IsTest = false;

            logoBorder.SetValue(Grid.ColumnProperty, 0);
            loginBorder.SetValue(Grid.ColumnProperty, 1);
            //btnLoginPrd.Visibility = Visibility.Collapsed;
            //btnLoginTest.Visibility = Visibility.Visible;
            btnTestEnvironment.Visibility = Visibility.Collapsed;
            btnPrdEnvironment.Visibility = Visibility.Visible;
        }

        private void BtnLoginTest_Click(object sender, RoutedEventArgs e)
        {
            //ResetServerInfo("2");

            //LocalEnv.ConfInfo.IsTest = true;

            logoBorder.SetValue(Grid.ColumnProperty, 1);
            loginBorder.SetValue(Grid.ColumnProperty, 0);
            //btnLoginPrd.Visibility = Visibility.Visible;
            //btnLoginTest.Visibility = Visibility.Collapsed;
            btnTestEnvironment.Visibility = Visibility.Visible;
            btnPrdEnvironment.Visibility = Visibility.Collapsed;
        }

        private void ResetServerInfo(string id)
        {
            bool isContain = false;
            foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
            {
                if (serverInfo.Id.Equals(id))
                {
                    isContain = true;
                    LocalEnv.ServerInfo = serverInfo;
                    break;
                }
            }
        }

        public void ShowMask(bool flag)
        {
            loadMask.Dispatcher.Invoke(() =>
            {
                loadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            if (!WindowManager.Instance.IsExist(nameof(NetworkSetting)))
            {
                NetworkSetting networkSetting = new NetworkSetting();
                WindowManager.Instance.OpenOnlyOneWindow(nameof(NetworkSetting), networkSetting);
            }
        }

        private void comboBoxServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }
            //获取选中的服务器并连接
            foreach (ServerInfo serverInfo in LocalEnv.ConfInfo.ServerInfos)
            {
                if (serverInfo.Name.Equals(e.AddedItems[0].ToString()))
                {
                    LocalEnv.UserInfo.ServerInfo = serverInfo;
                    LocalEnv.ServerInfo = serverInfo;
                    break;
                }
            }
            RestartDps();
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

        private void OnLoginOk(bool obj)
        {
            if (obj)
            {
                //如果是STC，如果是BDB允许进入
                switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
                {
                    case "bdb":
                    case "idcclient":
                        ThreadPoolHelper.InvokeThreadPool((o) =>
                        {
                            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
                            {
                                ClientUpdate.Instance.StartUpdateClient(string.Empty);
                            }));
                        });

                        break;
                    default:
                        break;
                }
               
            }
        }
    }
}
