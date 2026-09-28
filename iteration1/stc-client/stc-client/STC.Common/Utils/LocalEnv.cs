using com.gateway.log;
using STC.Common.Model.Cfg;
using STC.Common.Model.Drp;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STC.Common.Utils
{
    /// <summary>
    /// 客户端运行环境类
    /// </summary>
    public class LocalEnv
    {
        public static UserInfo UserInfo = new UserInfo();
        public static ServerInfo ServerInfo = new ServerInfo();
        public static LogInfo LogInfo = new LogInfo();
        public static ConfInfo ConfInfo = new ConfInfo();
        public static List<string> DictSkin = new List<string>() { "DefaultSkin", "BlueSkin", "BlackkishGreenSkin" };
        public static GrSystemParameter GrGlobSystemParameter = new GrSystemParameter();
        /// <summary>
        /// 是否有TradeBook权限
        /// </summary>
        public static bool IsHaveTradeBookPermission = true;
        /// <summary>
        /// GR页面布局设置
        /// </summary>
        public static GrLayoutSettingModel GrLayoutSetting = null;


        public static BrokerAutoConfirmParam BrokerAutoConfirmParam = new BrokerAutoConfirmParam();

        /// <summary>
        /// 手工单默认组合
        /// </summary>
        public static string ManualOrderDefaultBookId;

        /// <summary>
        /// 交易处理显示设置
        /// </summary>
        public static string TransactionModel { get; set; } = "3";
        public static void Load()
        {
            UserInfo = new UserInfo();
            ServerInfo = new ServerInfo();
            LogInfo = new LogInfo();
            ConfInfo = new ConfInfo();

            int num = 0;
            string startNum = ConfigurationManager.AppSettings["StartNum"];
            if (int.TryParse(startNum, out num))
                ConfInfo.StartNum = num;
            //ServerInfo.Host = ConfigurationManager.AppSettings["Host"];
            //ServerInfo.Port = ConfigurationManager.AppSettings["Port"];
            //ServerInfo.Protocal = ConfigurationManager.AppSettings["Protocal"];
            ServerInfo.LoginTitle = ConfigurationManager.AppSettings["LoginTitle"];
            ServerInfo.AppExe = ConfigurationManager.AppSettings["AppExe"];
            ServerInfo.AppName = ConfigurationManager.AppSettings["AppName"];
            switch (ServerInfo.AppName.ToLower())
            {
                case "dmart":
                    ServerInfo.AppImage = new BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/dmart_windows_ico_128.ico"));
                    break;
                case "gtja":
                    ServerInfo.AppImage = new BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/gtja_windows_ico_64.ico"));
                    break;
                case "idbpro":
                    ServerInfo.AppImage = new BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/idbpro_windows_icon_64.ico"));
                    break;
                case "bdb":
                case "idcclient":
                    ServerInfo.AppImage = new BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/bdb_windows_icon_128.ico"));
                    break;
                default:
                    ServerInfo.AppImage = new BitmapImage(new Uri("pack://application:,,,/STC.Common;component/Images/stc_win_logo.png"));
                    break;
            }

            ServerInfo.Version = ConfigurationManager.AppSettings["Version"];

            LogInfo.LogPath = ConfigurationManager.AppSettings["LogPath"];
            LogInfo.LogLevel = ConfigurationManager.AppSettings["LogLevel"];
            ConfInfo.LoginEnv = ConfigurationManager.AppSettings["LoginEnv"];
            ConfInfo.MaxWarningCount = ConfigurationManager.AppSettings["MaxWarningCount"];
            ConfInfo.CanGoodsQuickSearch = "true".Equals(ConfigurationManager.AppSettings["CanGoodsQuickSearch"], StringComparison.InvariantCultureIgnoreCase) ? true : false;

            ConfInfo.ResetDataTime = ConfigurationManager.AppSettings["ResetDataTime"];
            ConfInfo.TaskDelayTime = ConfigurationManager.AppSettings["TaskDelayTime"];
            ConfInfo.CalType = ConfigurationManager.AppSettings["CalType"];
            ConfInfo.EnabledMsgLog = ConfigurationManager.AppSettings["EnableMsgLog"];
            ConfInfo.UserDrag = "true".Equals(ConfigurationManager.AppSettings["UserDrag"], StringComparison.OrdinalIgnoreCase);

            ConfInfo.EnvironmentFlag = ConfigurationManager.AppSettings["EnvironmentFlag"];

            ConfInfo.IsGzip = ConfigurationManager.AppSettings["IsGzip"] ?? "0";
            ConfInfo.StartDps = ConfigurationManager.AppSettings["StartDps"];
            ConfInfo.ApiKey = ConfigurationManager.AppSettings["ApiKey"];
            ConfInfo.TradeType = ConfigurationManager.AppSettings["TradeType"];

            ConfInfo.LoginEnv = ConfigurationManager.AppSettings["LoginEnv"];

            if (string.IsNullOrEmpty(ConfInfo.TradeType))
            {
                ConfInfo.TradeType = "2";
            }

            ConfInfo.IsUseOHLCSvr = "1";

            string useVenueSort = ConfigurationManager.AppSettings["UseVenueSort"];
            ConfInfo.IsUseVenueSort = string.IsNullOrEmpty(useVenueSort) ? "0" : useVenueSort;

            string _LockScreenMinute = ConfigurationManager.AppSettings["LockScreenMinute"];
            ConfInfo.LockScreenMinute = string.IsNullOrEmpty(_LockScreenMinute) ? "0" : _LockScreenMinute;

            string _LockScreenStyle = ConfigurationManager.AppSettings["LockScreen"];
            ConfInfo.LockScreen = string.IsNullOrEmpty(_LockScreenStyle) ? "0" : _LockScreenStyle;

            string clientUpdateType = ConfigurationManager.AppSettings["ClientUpdateType"];
            ConfInfo.ClientUpdateType = string.IsNullOrEmpty(clientUpdateType) ? "1" : clientUpdateType;

            string _mmPriceLevelVisible = ConfigurationManager.AppSettings["MMPriceLevelVisible"];
            ConfInfo.MMPriceLevelVisible = string.IsNullOrEmpty(_mmPriceLevelVisible) ? "0" : _mmPriceLevelVisible;

            ConfInfo.HideIfSubBond_TBook = ConfigurationManager.AppSettings["HideIfSubBond_TBook"];

            string _mmExcelExportDataTime = ConfigurationManager.AppSettings["MMExcelExportDataTime"];
            ConfInfo.MMExcelExportDataTime = string.IsNullOrEmpty(_mmExcelExportDataTime) ? "" : _mmExcelExportDataTime;

            string _MMExcelSpread = ConfigurationManager.AppSettings["MMExcelSpread"];
            ConfInfo.MMExcelSpread = string.IsNullOrEmpty(_MMExcelSpread) ? "" : _MMExcelSpread;

            string _openQDM2Enable = ConfigurationManager.AppSettings["openQDM2Enable"];
            ConfInfo.openQDM2Enable = string.IsNullOrEmpty(_openQDM2Enable) ? "0" : _openQDM2Enable;

            string _openMMDHideSet = ConfigurationManager.AppSettings["openMMDHideSet"];
            ConfInfo.openMMDHideSet = string.IsNullOrEmpty(_openMMDHideSet) ? "1" : _openMMDHideSet;

            string _mmOpenTimeInterval = ConfigurationManager.AppSettings["MMOpenTimeInterval"];
            if (int.TryParse(_mmOpenTimeInterval, out num))
                ConfInfo.MMOpenTimeInterval = num;

            string _mmCloseTimeInterval = ConfigurationManager.AppSettings["MMCloseTimeInterval"];
            if (int.TryParse(_mmCloseTimeInterval, out num))
                ConfInfo.MMCloseTimeInterval = num;

            string _ChangePassMode = ConfigurationManager.AppSettings["ChangePassMode"];
            ConfInfo.ChangePassMode = _ChangePassMode == "0" ? "0" : "1";


            string dataPath = ConfigurationManager.AppSettings["DataPath"];

            if (!string.IsNullOrEmpty(dataPath) && dataPath.Contains("$username"))
            {
                dataPath = dataPath.Replace("$username", Environment.UserName);
            }
            else
            {
                dataPath = System.AppDomain.CurrentDomain.BaseDirectory;
            }

            if (!string.IsNullOrEmpty(LogInfo.LogPath) && LogInfo.LogPath.Contains("$username"))
            {
                LogInfo.LogPath = LogInfo.LogPath.Replace("$username", Environment.UserName);
            }

            //begin IDC
            //客户端单次克隆订单数
            if (!string.IsNullOrWhiteSpace(System.Configuration.ConfigurationManager.AppSettings["CloneOrderCount"]))
            {
                ConfInfo.CloneOrderCount = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CloneOrderCount"]);
            }
            ConfInfo.ExecPosition = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["ExecPosition"], StringComparison.OrdinalIgnoreCase);

            int cacheDigitLength = 0;
            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["CacheDigitLength"], out cacheDigitLength))
            {
                ConfInfo.CacheDigitLength = cacheDigitLength;
            }
            //成交不刷按钮
            ConfInfo.IsShowReserveButton = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["ReserveButtonIsShow"], StringComparison.OrdinalIgnoreCase);
            ConfInfo.ShowBridgeRoleSetting = GetAppSettingValue<bool>("ShowBridgeRoleSetting", true);
            ConfInfo.ShowPosition = System.Configuration.ConfigurationManager.AppSettings["ShowPosition"];
            ConfInfo.UISendQQ = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["UISendQQ"], StringComparison.OrdinalIgnoreCase);
            ConfInfo.ShowLookupSetting = GetAppSettingValue<bool>("ShowLookupSetting", true);
            ConfInfo.ShowLookupOtherSetting = GetAppSettingValue<bool>("ShowLookupOtherSetting", false);
            ConfInfo.ShowLookupSetting = GetAppSettingValue<bool>("ShowLookupSetting", true);
            ConfInfo.ShowAssistantSetting = GetAppSettingValue<bool>("ShowAssistantSetting", true);

            bool showSentLogBtn = true;
            if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["ShowSentLogBtn"], out showSentLogBtn))
            {
                ConfInfo.ShowSentLogBtn = showSentLogBtn;
            }
            int pageSize = 2000;
            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["OrderPageSize"], out pageSize))
            {
                ConfInfo.OrderPageSize = pageSize;
            }
            bool firstUpdateSettlementSync = false;
            if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["FirstUpdateSettlementSync"], out firstUpdateSettlementSync))
            {
                ConfInfo.IsFirstUpdateSettlementSync = firstUpdateSettlementSync;
            }


            int automaticQueryTime = 30;
            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["AutomaticQueryTime"], out automaticQueryTime))
            {
                ConfInfo.AutomaticQueryTime = automaticQueryTime;
            }

            //外发管理按钮
            ConfInfo.IsShowOutgoingButton = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["OutgoingButtonIsShow"], StringComparison.OrdinalIgnoreCase);
            ConfInfo.IsPaymentInst = System.Configuration.ConfigurationManager.AppSettings["CostSettingButtonIsShow"];

            //主订单背景色是否使用新的
            ConfInfo.IsNewOrderBackground = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["OrderBackgroundIsNew"], StringComparison.OrdinalIgnoreCase);
            //经纪商标识
            string BrokerNameFlag = System.Configuration.ConfigurationManager.AppSettings["BrokerNameFlag"];
            if (!string.IsNullOrEmpty(BrokerNameFlag))
                ConfInfo.BrokerNameFlag = BrokerNameFlag;
            //end IDC

            string _ChromeShowToolBar = ConfigurationManager.AppSettings["ChromeShowToolBar"];
            ConfInfo.ChromeShowToolBar = string.IsNullOrEmpty(_ChromeShowToolBar) ? "0" : _ChromeShowToolBar;

            string _EnableChromeGPU = ConfigurationManager.AppSettings["EnableChromeGPU"];
            ConfInfo.EnableChromeGPU = string.IsNullOrEmpty(_EnableChromeGPU) ? "1" : _EnableChromeGPU;

            string _EnableChromeDebug = ConfigurationManager.AppSettings["EnableChromeDebug"];
            ConfInfo.EnableChromeDebug = string.IsNullOrEmpty(_EnableChromeDebug) ? "0" : _EnableChromeDebug;

            string _ChromeProxyServer = ConfigurationManager.AppSettings["ChromeProxyServer"];
            ConfInfo.ChromeProxyServer = string.IsNullOrEmpty(_ChromeProxyServer) ? "" : _ChromeProxyServer;

            string _EnableChromeConsoleLog = ConfigurationManager.AppSettings["EnableChromeConsoleLog"];
            ConfInfo.EnableChromeConsoleLog = string.IsNullOrEmpty(_EnableChromeConsoleLog) ? "0" : _EnableChromeConsoleLog;

            string _NoSandbox = ConfigurationManager.AppSettings["NoSandbox"];
            ConfInfo.NoSandbox = string.IsNullOrEmpty(_NoSandbox) ? "1" : _NoSandbox;

            //string _EnableSendFQ = ConfigurationManager.AppSettings["EnableChromeSendFQ"];
            //ConfInfo.EnableChromeSendFQ = string.IsNullOrEmpty(_EnableSendFQ) ? "0" : _EnableSendFQ;

            string _WebFQAddress = ConfigurationManager.AppSettings["WebFQAddress"];
            string url = LocalEnv.ConfInfo.LoginEnv.ToLower() == "prd" ? "http://m.finq.com.cn/finq-message-send/?source=BDB#/todo" : "http://115.159.130.39/finq-message-send/?source=BDB#/todo";
            ConfInfo.WebFQAddress = string.IsNullOrEmpty(_WebFQAddress) ? url : _WebFQAddress;
            
            string _IsUseQBPrdTestFQNotOpenQB = ConfigurationManager.AppSettings["IsUseQBPrdTestFQNotOpenQB"];
            ConfInfo.IsUseQBPrdTestFQNotOpenQB = string.IsNullOrEmpty(_IsUseQBPrdTestFQNotOpenQB) ? "0" : _IsUseQBPrdTestFQNotOpenQB;

            string _IsForceUseWebFinQ = ConfigurationManager.AppSettings["IsUseWebFinQ"];
            ConfInfo.IsForceUseWebFinQ = string.IsNullOrEmpty(_IsForceUseWebFinQ) ? "0" : _IsForceUseWebFinQ;

            string _IsShowCopyQuoteAlert = ConfigurationManager.AppSettings["IsShowCopyQuoteAlert"];
            ConfInfo.IsShowCopyQuoteAlert = string.IsNullOrEmpty(_IsShowCopyQuoteAlert) ? "0" : _IsShowCopyQuoteAlert;

            int interval = 0;//默认500毫秒
            string intervaltime = ConfigurationManager.AppSettings["BatchOpenIntervalTime"];
            if (int.TryParse(intervaltime, out interval))
                ConfInfo.BatchOpenIntervalTime = interval;
            else
                ConfInfo.BatchOpenIntervalTime = 500;

            string autoQuoteRecognize = ConfigurationManager.AppSettings["IsAutoQuoteRecognize"];
            ConfInfo.IsAutoQuoteRecognize = "1" == autoQuoteRecognize ? "1" : "0";

            string _STPINNERAlgoName = ConfigurationManager.AppSettings["STPINNERAlgoName"];
            ConfInfo.STPINNERAlgoName = string.IsNullOrEmpty(_STPINNERAlgoName) ? "CROSS" : "STP".Equals(_STPINNERAlgoName) ? "STP" : "CROSS";
            ConfInfo.EnableIOC = GetAppSettingValue<bool>("EnableIOC", true);

            ConfInfo.IsShowRiskExposure = GetAppSettingValue<bool>("IsShowRiskExposure", false);

            ConfInfo.DataPath = dataPath;

            string _LocalTableType = ConfigurationManager.AppSettings["LocalTableType"];
            ConfInfo.LocalTableType = string.IsNullOrEmpty(_LocalTableType) ? "SQLITE" : _LocalTableType;

            ConfInfo.IsGroupFQNotEnter = GetAppSettingValue<bool>("IsGroupFQNotEnter", false);
            switch (ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    //读取BDB的app.config
                    try
                    {
                        // 获取当前应用程序的配置文件路径
                        string configFilePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                        // 读取exe.config的内容
                        var xmlDoc = new System.Xml.XmlDocument();
                        xmlDoc.Load(configFilePath);

                        // 将XML内容转换为字典
                        var sectionDict = new Dictionary<string, string>();
                        foreach (System.Xml.XmlNode section in xmlDoc.DocumentElement.ChildNodes)
                        {
                            sectionDict[section.Name] = section.InnerXml;
                        }

                        System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
                        string sgcxml = string.Empty;
                        string jsonText = string.Empty;

                        //ServerGroupCfg
                        if (sectionDict.ContainsKey("ServerGroupCfg"))
                        {
                            sgcxml = sectionDict["ServerGroupCfg"];
                            doc.LoadXml(sgcxml);
                            jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, false);
                            jsonText = ("[" + jsonText.Replace("\r\n", "").Replace("{\"ServerGroup\":", "").Replace("{\"Server\":[", "").Replace("}}", "").Replace("\"@", "\"").Replace("{\"Server\":{", "{") + "]").Replace("]]", "]");
                            if (JsonSplit.IsJson(jsonText))
                            {
                                ConfInfo.ServerInfos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServerInfo>>(jsonText);
                                foreach (ServerInfo serverInfo in ConfInfo.ServerInfos)
                                {
                                    serverInfo.AppExe = ServerInfo.AppExe;
                                    serverInfo.AppName = ServerInfo.AppName;
                                    serverInfo.AppImage = ServerInfo.AppImage;
                                    serverInfo.Version = ServerInfo.Version;
                                    serverInfo.LoginTitle = ServerInfo.LoginTitle;
                                }
                            }
                        }

                        //RemoteServerGroupCfg
                        if (sectionDict.ContainsKey("RemoteServerGroupCfg"))
                        {
                            sgcxml = sectionDict["RemoteServerGroupCfg"];
                            doc.LoadXml(sgcxml);
                            jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, false);
                            jsonText = ("[" + jsonText.Replace("\r\n", "").Replace("{\"RemoteServerGroup\":", "").Replace("{\"RemoteServer\":[", "").Replace("}}", "").Replace("\"@", "\"").Replace("{\"RemoteServer\":{", "{") + "]").Replace("]]", "]");
                            if (JsonSplit.IsJson(jsonText))
                            {
                                List<ServerInfo> NewServerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServerInfo>>(jsonText);
                                //热备登录
                                ConfInfo.IsUserHotBackupLogin = true;

                                //读取升级端口
                                UriBuilder updport = GetUpdateAddress();

                                if (ConfInfo.ServerInfos.Count > 0)
                                {
                                    ServerInfo src = ConfInfo.ServerInfos[0];
                                    foreach (ServerInfo si in NewServerInfo)
                                    {
                                        si.Add_ServerTicks = ServerInfo.Add_ServerTicks;
                                        si.AppExe = ServerInfo.AppExe;
                                        si.AppImage = ServerInfo.AppImage;
                                        si.Version = ServerInfo.Version;
                                        si.AppName = ServerInfo.AppName;
                                        si.LoginTitle = ServerInfo.LoginTitle;
                                        si.DpsHost = si.Host;
                                        si.DpsPort = si.Port;
                                        si.DpsProtocol = si.Protocol;
                                        si.UpdateHost = src.UpdateHost ?? si.Host;
                                        si.UpdatePort = src.UpdatePort ?? (updport.Port.ToString() == "65535" ? "0" : updport.Port.ToString());
                                        si.UpdateProtocol = src.UpdateProtocol ?? updport.Scheme;
                                        si.Host = src.Host;
                                        si.Port = src.Port;
                                        si.Protocol = src.Protocol;
                                        //BDB 的服务器名采用 ServerGroupCfg 中的名字，而不是 RemoteServerGroupCfg 的名字，否则降级后，老版本无法登录
                                        si.Name = src.Name;
                                    }
                                    //DNS解析判断

                                    foreach (ServerInfo si in NewServerInfo)
                                    {
                                        try
                                        {
                                            if (Dps.DpsUtil.Instance.IsValidDomain(si.DpsHost))
                                            {
                                                IPHostEntry hostEntry = Dns.GetHostEntry(si.DpsHost);
                                                string ipAddress = hostEntry.AddressList[0].ToString();
                                            }
                                            else if (!(Dps.DpsUtil.Instance.IsValidIPv4(si.DpsHost) || Dps.DpsUtil.Instance.IsValidIPv6(si.DpsHost)))
                                            {
                                                si.Add_ServerTicks = 9999;
                                                LogHelper.Error("Host:" + si.DpsHost + " Not Ipv4 and Ipv6." );
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            si.Add_ServerTicks = 9999;
                                            LogHelper.Error("Host:" + si.DpsHost + " parse fail:", ex);
                                            continue;
                                        }
                                    }

                                    //ServerInfos 判断可用网络，如果都不可用，则不改配置
                                    ConfInfo.ServerInfos = NewServerInfo.Where(o=>o.Add_ServerTicks != 9999).ToList();
                                    if (ConfInfo.ServerInfos.Count == 0)
                                    {
                                        ConfInfo.ServerInfos = NewServerInfo;
                                    }
                                }

                            }

                            //这里读取history.data 如果有历史记录则找到 服务器的 id
                            string col = IniFileHelper.ReadValue($"Login", "ServerInfo");
                            if (!string.IsNullOrEmpty(col))
                            {
                                if (JsonSplit.IsJson(col))
                                {
                                    ServerInfo rsi = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerInfo>(col);
                                    ConfInfo.RemoteServerInfos = ConfInfo.ServerInfos.Where(o => o.DpsHost == rsi.DpsHost).ToList();
                                }
                            }
                        }

                        //ServerStatusGroupCfg
                        if (sectionDict.ContainsKey("ServerStatusGroupCfg"))
                        {
                            sgcxml = sectionDict["ServerStatusGroupCfg"];
                            doc.LoadXml(sgcxml);
                            jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, false);
                            jsonText = ("[" + jsonText.Replace("\r\n", "").Replace("{\"ServerStatusGroup\":", "").Replace("{\"ServerStatus\":[", "").Replace("}}", "").Replace("\"@", "\"").Replace("{\"ServerStatus\":{", "{") + "]").Replace("]]", "]");
                            if (JsonSplit.IsJson(jsonText))
                            {
                                ConfInfo.ServerStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ServerStatus>>(jsonText);
                            }
                        }

                        //IsDcsBrokerGroupCfg
                        if (sectionDict.ContainsKey("IsDcsBrokerGroupCfg"))
                        {
                            sgcxml = sectionDict["IsDcsBrokerGroupCfg"];
                            doc.LoadXml(sgcxml);
                            jsonText = Newtonsoft.Json.JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.None, false);
                            jsonText = ("[" + jsonText.Replace("\r\n", "").Replace("{\"IsDcsBrokerGroup\":", "").Replace("{\"IsDcsBroker\":[", "").Replace("}}", "").Replace("\"@", "\"").Replace("{\"IsDcsBroker\":{", "{") + "]").Replace("]]", "]");
                            List<IsDcsBroker> lstisdcs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IsDcsBroker>>(jsonText);
                            if (JsonSplit.IsJson(jsonText))
                            {
                                ConfInfo.IsDcsBroker = lstisdcs.Where(p => p.Status == "1").ToDictionary(server => server.Broker, server => server);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHandler.Error($"Parse App.Config Error:{ex.Message}");
                    }

                    break;
                default:
                    ServerCfgSection serverCfgSection = ConfigurationManager.GetSection("ServerGroupCfg") as ServerCfgSection;
                    ConfInfo.ServerInfos = new List<ServerInfo>();

                    foreach (ServerSection ss in serverCfgSection.ServerGroup)
                    {
                        ServerInfo serverInfo = new ServerInfo();
                        serverInfo.Id = ss.Id;
                        serverInfo.Host = ss.Host;
                        serverInfo.Name = ss.Name;
                        serverInfo.Port = ss.Port;
                        serverInfo.Protocol = ss.Protocol;
                        serverInfo.DpsHost = ss.DpsHost;
                        serverInfo.DpsPort = ss.DpsPort;
                        serverInfo.DpsProtocol = ss.DpsProtocol;
                        serverInfo.UpdateHost = ss.UpdateHost;
                        serverInfo.UpdatePort = ss.UpdatePort;
                        serverInfo.UpdateProtocol = ss.UpdateProtocol;
                        serverInfo.AppExe = ServerInfo.AppExe;
                        serverInfo.AppName = ServerInfo.AppName;
                        serverInfo.AppImage = ServerInfo.AppImage;
                        serverInfo.Version = ServerInfo.Version;
                        ConfInfo.ServerInfos.Add(serverInfo);
                    }

                    ServerStatusCfgSection serverStatusCfgSection = ConfigurationManager.GetSection("ServerStatusGroupCfg") as ServerStatusCfgSection;
                    ConfInfo.ServerStatus = new List<ServerStatus>();
                    foreach (ServerStatusSection ss in serverStatusCfgSection.ServerGroup)
                    {
                        ServerStatus serverStatus = new ServerStatus();
                        serverStatus.VenueTypeGW = ss.VenueTypeGW;
                        serverStatus.TargetSessionId = ss.TargetSessionId;
                        serverStatus.Name = ss.Name;
                        serverStatus.Id = ss.Id;
                        ConfInfo.ServerStatus.Add(serverStatus);
                    }

                    IsDcsBrokerCfgSection isDcsBrokerCfgSection = ConfigurationManager.GetSection("IsDcsBrokerGroupCfg") as IsDcsBrokerCfgSection;
                    ConfInfo.IsDcsBroker = new Dictionary<string, IsDcsBroker>();
                    foreach (IsDcsBrokerSection ss in isDcsBrokerCfgSection.DcsBroker)
                    {
                        IsDcsBroker isDcsBroker = new IsDcsBroker();
                        if ("1".Equals(ss.Status))
                        {
                            isDcsBroker.Broker = ss.Broker;
                            isDcsBroker.Status = ss.Status;
                            isDcsBroker.Name = ss.Name;
                            isDcsBroker.Id = ss.Id;
                            ConfInfo.IsDcsBroker.Add(ss.Broker, isDcsBroker);
                        }
                    }

                    break;
            }


            LogInfo.FilterLog = ConfigurationManager.AppSettings["FilterLog"];
            string autoStart = ConfigurationManager.AppSettings["AutoStart"] ?? "0";
            ConfInfo.AutoStart = autoStart;

            ConfInfo.IsAccountUser = ConfigurationManager.AppSettings["IsAccountUser"];

            ConfInfo.IsLocalCacheCrm = ConfigurationManager.AppSettings["IsLocalCacheCrm"];

            ConfInfo.IsShowPriceCondition = ConfigurationManager.AppSettings["IsShowPriceCondition"];

            string IsShowEnterMarket = ConfigurationManager.AppSettings["IsShowEnterMarket"];
            ConfInfo.IsShowEnterMarket = string.IsNullOrEmpty(IsShowEnterMarket) ? "0" : IsShowEnterMarket;

            ConfInfo.IsShowDRP = ConfigurationManager.AppSettings["IsShowDRP"] ?? "0";

            ConfInfo.PendingCancelTime = ConfigurationManager.AppSettings["PendingCancelTime"] ?? "15";

            ConfInfo.IsShowInnerFuzzyRequire = ConfigurationManager.AppSettings["IsShowInnerFuzzyRequire"] ?? "0";

            

            ConfInfo.IsParserImage = ConfigurationManager.AppSettings["IsParserImage"] ?? "0";

            ConfInfo.ShowInstDeleteBtn = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["ShowInstDeleteBtn"], StringComparison.OrdinalIgnoreCase);


            //手动发送按钮
            ConfInfo.IsShowSendManuallyButton = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["SendManuallyButtonIsShow"], StringComparison.OrdinalIgnoreCase);

            //双击间隔,0取系统间隔，其他数字取配置时间
            int doubleIntervalTime = 0;
            string doubleIntervalTimeStr = ConfigurationManager.AppSettings["DoubleIntervalTime"];
            if (!string.IsNullOrWhiteSpace(doubleIntervalTimeStr) &&int.TryParse(doubleIntervalTimeStr, out doubleIntervalTime))
            {
                ConfInfo.DoubleIntervalTime = doubleIntervalTime;
            }
            else
            {
                ConfInfo.DoubleIntervalTime = System.Windows.Forms.SystemInformation.DoubleClickTime;
            }

            //BDB Add
            ConfInfo.AutoUpdateInterval = System.Configuration.ConfigurationManager.AppSettings["AutoUpdateInterval"];
            ConfInfo.BridgeVisible = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["BridgeVisible"], StringComparison.InvariantCultureIgnoreCase) ? true : false;
            ConfInfo.QuoteBtnVisual = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["QuoteBtnVisual"], StringComparison.OrdinalIgnoreCase);
            ConfInfo.DetailPosition = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["DetailPosition"], StringComparison.OrdinalIgnoreCase);
            ConfInfo.BrokerBConfirmTime = System.Configuration.ConfigurationManager.AppSettings["BrokerBConfirmTime"];
            ConfInfo.QQApiHost = System.Configuration.ConfigurationManager.AppSettings["QQApiHost"];
            ConfInfo.Authorization = System.Configuration.ConfigurationManager.AppSettings["Authorization"];
            ConfInfo.QQApiSender = System.Configuration.ConfigurationManager.AppSettings["QQApiSender"];
            ConfInfo.QQApiReceiver = System.Configuration.ConfigurationManager.AppSettings["QQApiReceiver"];
            ConfInfo.KLineUrl = System.Configuration.ConfigurationManager.AppSettings["KLineUrl"];
            ConfInfo.KLineUrl_F = System.Configuration.ConfigurationManager.AppSettings["KLineUrl_F"];
            ConfInfo.IsKLine = System.Configuration.ConfigurationManager.AppSettings["IsKLine"] == "1" ? true : false;
            ConfInfo.IsKLine_F = System.Configuration.ConfigurationManager.AppSettings["IsKLine_F"] == "1" ? true : false;

            ConfInfo.QQToTPSC = System.Configuration.ConfigurationManager.AppSettings["QQToTPSC"];
            ConfInfo.QQToCTTJ = System.Configuration.ConfigurationManager.AppSettings["QQToCTTJ"];
            ConfInfo.QQToCNEX = System.Configuration.ConfigurationManager.AppSettings["QQToCNEX"];
            ConfInfo.QQToCCTB = System.Configuration.ConfigurationManager.AppSettings["QQToCCTB"];
            ConfInfo.QQToPTCN = System.Configuration.ConfigurationManager.AppSettings["QQToPTCN"];
            ConfInfo.QQToUEDA = System.Configuration.ConfigurationManager.AppSettings["QQToUEDA"];

            string sleep = System.Configuration.ConfigurationManager.AppSettings["QQStartSleep"];
            int tmp;
            if (int.TryParse(sleep, out tmp))
            {
                ConfInfo.QQStartSleep = tmp;
            }
            sleep = System.Configuration.ConfigurationManager.AppSettings["QQSendSleep"];
            if (int.TryParse(sleep, out tmp))
            {
                ConfInfo.QQSendSleep = tmp;
            }

            ConfInfo.ShowBestSource = System.Configuration.ConfigurationManager.AppSettings["ShowBestSource"] ?? "0";
            // 加载配置
            //GrLayoutSetting = QuoteSettingMode();
            //启动 DPS，则 IsGzip 必须为 "1"
            //不启动DPS，则 IsGzip 必须为 "0"
            if (ConfInfo.StartDps != "1")
                ConfInfo.IsGzip = "1";
            else
                ConfInfo.IsGzip = "0";

            switch (ServerInfo.AppName.ToLower())
            {
                case "bdb":
                case "idcclient":
                    ConfInfo.DefaultSkinStyle = ConfigurationManager.AppSettings["DefaultSkinStyle"] ?? "DefaultSkin";
                    break;
                default:
                    ConfInfo.DefaultSkinStyle = ConfigurationManager.AppSettings["DefaultSkinStyle"] ?? "DefaultSkin";
                    break;
            }
            

            //是否支持FQ
            ConfInfo.IsFQ = "1".Equals(System.Configuration.ConfigurationManager.AppSettings["IsFQ"], StringComparison.OrdinalIgnoreCase);

            //是否支持内部看板最优显示样式
            ConfInfo.IsInnerQtyDetail = System.Configuration.ConfigurationManager.AppSettings["IsInnerQtyDetail"];

            //内部看板下单界面报量支持散量配置 只支持1-4位小数
            string d = System.Configuration.ConfigurationManager.AppSettings["QtyDecimalDigit"] ?? "1";
            if (CommonUtils.IsNumber(d, 1, 0))
            {
                int digit = d.ConvertToInt();
                if (digit >= 1 && digit <= 4)
                {
                    ConfInfo.QtyDecimalDigit = digit;
                }
                else
                {
                    ConfInfo.QtyDecimalDigit = 1;
                }
            }
            else
            {
                ConfInfo.QtyDecimalDigit = 1;
            }

            // BDB是否支持分屏
            ConfInfo.IsBDBSplit = "1".Equals(System.Configuration.ConfigurationManager.AppSettings["IsBDBSplit"], StringComparison.OrdinalIgnoreCase);

            //是否支持QQ
            ConfInfo.IsQQ = "1".Equals(System.Configuration.ConfigurationManager.AppSettings["IsQQ"], StringComparison.OrdinalIgnoreCase);

            // 客户端自动退出时间
            ConfInfo.ClientAutoExitTime = System.Configuration.ConfigurationManager.AppSettings["ClientAutoExitTime"];

            ConfInfo.IsWriteDBLog = GetAppSettingValue<bool>("IsWriteDBLog", false);

            ConfInfo.IsInquiry = GetAppSettingValue<bool>("IsInquiry", false);

            ConfInfo.PushQQGroup_PushView = System.Configuration.ConfigurationManager.AppSettings["PushQQGroup_PushView"];
            ConfInfo.HasSendDemandBtn_ParseView = System.Configuration.ConfigurationManager.AppSettings["HasSendDemandBtn_ParseView"];
            ConfInfo.HasCopyBtn_ParseView = System.Configuration.ConfigurationManager.AppSettings["HasCopyBtn_ParseView"];
            ConfInfo.IsPTBrokerExec = GetAppSettingValue<bool>("IsPTBrokerExec", false);
            ConfInfo.IsI2T = GetAppSettingValue<bool>("IsI2T", false);
            ConfInfo.IsPAYL = GetAppSettingValue<bool>("IsPAYL", false);
            ConfInfo.ValidCodeVisible = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["ValidCodeVisible"], StringComparison.InvariantCultureIgnoreCase) ? true : false;
            ConfInfo.IsRiskCheck = GetAppSettingValue<bool>("IsRiskCheck", false);
            ConfInfo.ShowPriceType = System.Configuration.ConfigurationManager.AppSettings["ShowPriceType"];
            ConfInfo.IsRiskCheck_Sell = GetAppSettingValue<bool>("IsRiskCheck_Sell", false);
            ConfInfo.ClientType = System.Configuration.ConfigurationManager.AppSettings["ClientType"];
        }

        public static T GetAppSettingValue<T>(string name, object defaultValue)
        {
            T value = default(T);
            var settingValue = System.Configuration.ConfigurationManager.AppSettings[name];
            switch (typeof(T).Name.ToLower())
            {
                case "int32":
                    {
                        int intValue;
                        if (!string.IsNullOrEmpty(settingValue) && int.TryParse(settingValue, out intValue))
                        {
                            value = (T)(object)(intValue);
                        }
                        else
                        {
                            value = (T)defaultValue;
                        }
                        break;
                    }
                case "boolean":
                    {
                        bool boolValue = false;
                        if (!string.IsNullOrEmpty(settingValue) && bool.TryParse(settingValue, out boolValue))
                        {
                            value = (T)(object)(boolValue);
                        }
                        else
                        {
                            value = (T)defaultValue;
                        }
                        break;
                    }
                default:
                    {
                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            value = (T)(object)settingValue;
                        }
                        else
                        {
                            value = (T)defaultValue;
                        }
                        break;
                    }
            }
            return value;
        }


        #region 读取Broker配置文件
        public static GrLayoutSettingModel QuoteSettingMode()
        {
            try
            {
                GrLayoutSettingModel layoutSetting = new GrLayoutSettingModel();
                string quotesshowmode = IniFileHelper.ReadValue($"GrLayoutSetting_{ LocalEnv.UserInfo.UserName}", "QuotesShowMode");
                if (!string.IsNullOrEmpty(quotesshowmode))
                {
                    layoutSetting.QuotesShowMode = int.Parse(quotesshowmode);
                }
                string showmodel = IniFileHelper.ReadValue($"GrLayoutSetting_{ LocalEnv.UserInfo.UserName}", "ShowModel");
                if (!string.IsNullOrEmpty(showmodel))
                {
                    layoutSetting.ShowModel = int.Parse(showmodel);
                }
                string row = IniFileHelper.ReadValue($"GrLayoutSetting_{ LocalEnv.UserInfo.UserName}", "Row");
                if (!string.IsNullOrEmpty(row))
                {
                    layoutSetting.Row = int.Parse(row);
                }
                string col = IniFileHelper.ReadValue($"GrLayoutSetting_{ LocalEnv.UserInfo.UserName}", "Col");
                if (!string.IsNullOrEmpty(col))
                {
                    layoutSetting.Col = int.Parse(col);
                }

                return layoutSetting;
            }
            catch (Exception e)
            {
                LogHelper.Error("LayoutSetttingCache QuoteSettingMode exception:", e);
            }
            return null;
        }

        #endregion

        #region 读取升级文件，获得升级端口
        public static UriBuilder GetUpdateAddress()
        {
            UriBuilder uriBuilder = new UriBuilder("ftp://127.0.0.1:65535");
            string fname = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "control\\autoupdater.config");
            try
            {
                
                if (File.Exists(fname))
                {
                    // 读取exe.config的内容
                    var xmlDoc = new System.Xml.XmlDocument();
                    xmlDoc.Load(fname);

                    // 将XML内容转换为字典
                    var sectionDict = new Dictionary<string, string>();
                    foreach (System.Xml.XmlNode section in xmlDoc.DocumentElement.ChildNodes)
                    {
                        sectionDict[section.Name] = section.InnerXml;
                    }

                    if (sectionDict.ContainsKey("ServerUrl"))
                    {
                        string jsonText = sectionDict["ServerUrl"];
                        // 使用Uri类来解析URL
                        uriBuilder = new UriBuilder(jsonText);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error($"读取升级文件 {fname} 错误! {ex.Message}");
            }

            return uriBuilder;
        }

        #endregion
    }

    public class UserInfo
    {
        //QB权限
        public string IDCPermissions;
        //机构里面角色权限
        public string ConnectPermissions;
        public string BonDPermissions;
        public string BondTradePermissions;
        public string IdcTradePermissions;
        public string IdcExcelPermissions;
        public string IdcPythonPermissions;
        public string IdcQTRSPermissions;
        public string BrokerSort;

        public string prods;
        public string UserId;
        private string userName;
        public string termids;

        public string Termids
        {
            get { return termids; }
            set
            {
                termids = value;
                TermIds = CommonUtils.BizTypeConvert(LocalEnv.UserInfo.Termids).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }

        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
            }
        }
        public string FullName;
        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;

                if ((!string.IsNullOrEmpty(_password)) && !string.IsNullOrEmpty(userName))
                {
                    Password2 = netapi.Api.EncrypterHelper.AesEncrypt(Encoding.UTF8.GetBytes(_password), null);
                }
            }
        }
        public string Password2;
        public string SessionID;
        public string Sid;
        public ServerInfo ServerInfo;
        public string GroupIds;
        public string GroupNames;
        public string RoleId;
        public string RoleName;
        public RoleType RoleType = RoleType.SELLER_ROLE;// 1：买方， 2：卖方
        public string validCode;
        public string validCodeId;

        /// <summary>
        /// 当前客户端台子
        /// </summary>
        public string SubCompanyId { get; set; }

        public bool IsMyself(string userid)
        {
            return UserId.Equals(userid);
        }

        public bool IsMySelfByName(string name)
        {
            return FullName.Equals(name);
        }

        private string userIdKey = string.Empty;
        public bool IsContainsUserId(string userIds)
        {
            if (string.IsNullOrEmpty(userIds))
                return false;
            if (string.IsNullOrEmpty(userIdKey))
                userIdKey = string.Format(",{0},", UserId);
            string keys = string.Format(",{0},", userIds);
            if (keys.Contains(userIdKey))
                return true;
            return false;
        }

        /// <summary>
        /// 当前CompanyId  TPSC：1，CNEX：2，BGC：3，PTCN：4，TJXT：5，UEDA：6
        /// </summary>
        public string CompanyId { get; set; }

        public string CompanyName { get; set; }

        // QF权限
        public string QeubeeFingPermissions;

        // 内部看板权限
        public string BdbInnerPermissions;

        // 中债估值显示权限
        public string CDCPermissions;

        // 市场最优
        public string BdbBboPermissions;

        // 策略定价
        public string BdbStaratPermissions;

        /// <summary>
        /// 有权限台子类型转换
        /// BCO——信用 1
        /// BNC——利率 2
        /// ABS——ABS  11
        /// NCD——NCD  12
        public List<string> TermIds = new List<string>();

        public string QQNumber;

        // 组id
        public string biztypeid;

        // 组名
        public string biztype;

        /// <summary>
        /// 可查看的行情部门字段
        /// </summary>
        public List<string> mdgroupids;
    }

    public class ServerInfo
    {
        public string Id;
        public string Name;
        public string Host;
        public string Port;
        public string Protocol;
        public string LoginTitle;
        public string DpsHost;
        public string DpsPort;
        public string DpsProtocol;
        public string UpdateHost;
        public string UpdatePort;
        public string UpdateProtocol;

        public long Add_ServerTicks;
        public string AppExe;
        public string AppName;
        public string Version;
        public ImageSource AppImage;
    }

    public class ServerStatus
    {
        public string Id;
        public string Name;
        public string VenueTypeGW;
        public string TargetSessionId;
    }

    public class LogInfo
    {
        public string LogPath;
        public string LogLevel;
        public string FilterLog;
    }

    public class IsDcsBroker
    {
        public string Id;
        public string Broker;
        public string Name;
        public string Status;
    }

    public enum RoleType
    {
        BUYER_ROLE,//买方机构
        SELLER_ROLE//卖方机构
    }

    public class ConfInfo
   {
        public string AutoStart;
        public string DataPath;
        public string ClientUpdateType;
        public string TradeType;
        public string LoginType;
        public string AutoUpdateInterval;
        public string StartDps;
        public string EnvironmentFlag;
        public string ApiKey;
        public string IsGzip;
        public string LoginEnv;

        public bool IsTest { get { return !string.IsNullOrEmpty(EnvironmentFlag) && EnvironmentFlag.Equals("1"); } }

        public string MaxWarningCount;
        public bool CanGoodsQuickSearch;
        public string ResetDataTime;
        public int StartNum;
        public List<ServerInfo> ServerInfos;
        public List<ServerInfo> RemoteServerInfos;
        /// <summary>
        /// 是否使用热备登录(轮询登录)，如果用户配置了多个RemoteServer则启用，如果配置了多个 ServerGroup 则不启用
        /// </summary>
        public bool IsUserHotBackupLogin = false;
        public string TaskDelayTime;
        public List<ServerStatus> ServerStatus;
        public string CalType;
        public string EnabledMsgLog;
        public bool UserDrag;
        public Dictionary<string, IsDcsBroker> IsDcsBroker;

        /// <summary>
        /// 1 使用OHLC 0 不显示 2 使用默认的 info1
        /// </summary>
        public string IsUseOHLCSvr = string.Empty;
        /// <summary>
        /// 1 渠道排序 0 默认排序
        /// </summary>
        public string IsUseVenueSort;
        /// <summary>
        /// 锁屏系统空闲时间 单位分钟
        /// </summary>
        public string LockScreenMinute;
        /// <summary>
        /// 锁屏类型 0 不锁屏 -1 直接退出系统 1 锁屏
        /// </summary>
        public string LockScreen;
        /// <summary>
        /// 双边策略列表是否展示我的报价档位
        /// </summary>
        public string MMPriceLevelVisible;

        /// <summary>
        /// 是否隐藏报价撤单条件中的【订阅】标签 1隐藏 其他或不配置显示，默认显示
        /// </summary>
        public string HideIfSubBond_TBook;
        /// <summary>
        /// 是否可修改密码 0：不可；默认或1：可以；
        /// </summary>
        public string ChangePassMode;

        /// <summary>
        /// Chrome框架是否显示工具栏状态栏和右键菜单 1显示 0隐藏 ，默认隐藏
        /// </summary>
        public string ChromeShowToolBar;

        /// <summary>
        /// 允许Chrome使用GPU
        /// </summary>
        public string EnableChromeGPU;

        /// <summary>
        /// 允许ChromeDebug日志
        /// </summary>
        public string EnableChromeDebug;

        /// <summary>
        /// 允许打印Chrome Console日志
        /// </summary>
        public string EnableChromeConsoleLog;

        /// <summary>
        /// 禁用CEF 的沙盒机制，如果设为0可能导致子进程启动失败或白屏
        /// </summary>
        public string NoSandbox;

        /// <summary>
        /// Chrome浏览器代理服务器地址
        /// </summary>
        public string ChromeProxyServer;

        /// <summary>
        /// 双边做市定时EXCEL导出(格式:HH:mm,以英文逗号分割)
        /// </summary>
        public string MMExcelExportDataTime;
        /// <summary>
        /// 边做市定时EXCEL导出(点差单位bp)
        /// </summary>
        public string MMExcelSpread;
        /// <summary>
        /// 双边做市批量开启策略时间间隔大于11，按配置间隔，小于11按11间隔
        /// </summary>
        public int MMOpenTimeInterval;
        /// <summary>
        /// 双边做市批量关闭策略时间间隔大于11，按配置间隔，小于11按11间隔，等于-1不间隔
        /// </summary>
        public int MMCloseTimeInterval;
        /// <summary>
        /// 是否显示指示性报价复选框，双边报价详情设置是否显示指示性复选框 0-不显示 1-显示
        /// </summary>
        public string openQDM2Enable;

        /// <summary>
        /// 隐藏【拉宽所有报价】按键;策略类型下拉选项去掉【接口】;算法报价中参考中间价下拉选项仅保留【中债估值】; 接口报价、成交报价设置隐藏; 0-不显示 1-显示  默认显示
        /// </summary>
        public string openMMDHideSet;
        /// <summary>
        /// 是否在消息提醒中显示报价识别 1显示 0不显示
        /// </summary>
        public string IsShowCopyQuoteAlert;
        /// <summary>
        /// 多个策略批量开启的时候，最小时间间隔(毫秒)
        /// </summary>
        public int BatchOpenIntervalTime;

        /// <summary>
        /// 是否批量识别中，系统支持主动读取剪贴板，复制粘贴到需求识别框中。1-是 0-不是
        /// </summary>
        public string IsAutoQuoteRecognize;

        /// <summary>
        /// 是否启用组合账户
        /// </summary>
        public string IsAccountUser;

        /// <summary>
        /// 是否本地缓存crm数据
        /// </summary>
        public string IsLocalCacheCrm;

        /// <summary>
        /// 是否显示条件撤销
        /// </summary>
        public string IsShowPriceCondition;

        /// <summary>
        /// 是否显示录入行情按钮 1显示 0不显示
        /// </summary>
        public string IsShowEnterMarket;

        /// <summary>
        /// 是否显示定向报价,1:显示
        /// </summary>
        public string IsShowDRP;

        /// <summary>
        /// 定向报价不可撤销时间
        /// </summary>
        public string PendingCancelTime;

        /// <summary>
        /// 是否是win7以后的版本
        /// </summary>
        public bool IsOSVersionAfterWin7 = false;

        /// <summary>
        /// 是否显示模糊需求,1:显示
        /// </summary>
        public string IsShowInnerFuzzyRequire;

        /// <summary>
        /// 样式  BlueSkin Or DefaultSkin Or BDB
        /// </summary>
        public string DefaultSkinStyle = "DefaultSkin";

        /// <summary>
        /// 是否图像识别
        /// </summary>
        public string IsParserImage;

        private int _CloneOrderCount = 30;
        /// <summary>
        /// 客户端单次克隆订单数
        /// </summary>
        public int CloneOrderCount
        {
            get { return _CloneOrderCount; }
            set { _CloneOrderCount = value; }
        }

        /// <summary>
        /// 记忆线下录入显示位置
        /// </summary>
        public bool ExecPosition { get; set; }

        private int cacheDigitLength = 5;
        /// <summary>
        /// 债券快速搜索缓存位数
        /// </summary>
        public int CacheDigitLength
        {
            get
            {
                return cacheDigitLength;
            }
            set
            {
                if (value < 0)
                {
                    cacheDigitLength = 0;
                }
                else if (value > 100)
                {
                    cacheDigitLength = 100;
                }
                else
                {
                    cacheDigitLength = value;
                }
            }
        }

        /// <summary>
        /// 成交不刷按钮
        /// </summary>
        private bool _IsShowReserveButton = false;
        public bool IsShowReserveButton { get { return _IsShowReserveButton; } set { _IsShowReserveButton = value; } }
        /// <summary>
        /// 是否显示桥专员配置列表
        /// </summary>
        public bool ShowBridgeRoleSetting { get; set; }

        /// <summary>
        /// 弹窗展示位置，0：按定位的x,y坐标，1：直接居中弹
        /// </summary>
        public string ShowPosition { get; set; }

        /// <summary>
        /// 是否显示查看其他人过桥成交单权限列表
        /// </summary>
        public bool ShowLookupOtherSetting { get; set; }

        /// <summary>
        /// 是否支持发送QQ
        /// </summary>
        public bool UISendQQ { get; set; }


        /// <summary>
        /// 是否显示查看权限列表
        /// </summary>
        public bool ShowLookupSetting { get; set; }

        /// <summary>
        /// 是否显示助理权限
        /// </summary>
        public bool ShowAssistantSetting { get; set; }

        /// <summary>
        /// 是否显示发送日志按钮
        /// </summary>
        private bool _ShowSentLogBtn = false;
        public bool ShowSentLogBtn { get { return _ShowSentLogBtn; } set { _ShowSentLogBtn = value; } }

        /// <summary>
        /// 日志发送to
        /// </summary>
        private string _sendlogto = "guangjie.li@sumscope.com;xinyi.cai@sumscope.com";
        public string SendLogTo { get { return _sendlogto; } set { _sendlogto = value; } }

        /// <summary>
        /// CC
        /// </summary>
        private string _SendLogCC = "yanwen.li@sumscope.com;mingzhu.ding@sumscope.com";
        public string SendLogCC { get { return _SendLogCC; } set { _SendLogCC = value; } }

        /// <summary>
        /// 发送的日志天数
        /// </summary>
        private int _sendDays = 5;
        public int SendDays { get { return _sendDays; } set { _sendDays = value; } }

        /// <summary>
        /// 加载订单的分页大小
        /// </summary>
        private int _orderpagesize = 1000;
        public int OrderPageSize { get { return _orderpagesize; } set { _orderpagesize = value; } }

        /// <summary>
        /// 加载历史订单查询间隔时间
        /// </summary>
        private int _automaticQueryTime = 30;
        public int AutomaticQueryTime { get { return _automaticQueryTime; } set { _automaticQueryTime = value; } }
        /// <summary>
        /// 加载基础数据的分页大小
        /// </summary>
        private int _basedatapagesize = 2000;

        public int BaseDataPageSize { get { return _basedatapagesize; } set { _basedatapagesize = value; } }
        /// <summary>
        /// 首次修改结算是否联动配置
        /// </summary>
        private bool _IsFirstUpdateSettlementSync = true;
        public bool IsFirstUpdateSettlementSync { get { return _IsFirstUpdateSettlementSync; } set { _IsFirstUpdateSettlementSync = value; } }
        /// <summary>
        /// 外发管理按钮按钮
        /// </summary>
        private bool _IsShowOutgoingButton = false;
        public bool IsShowOutgoingButton { get { return _IsShowOutgoingButton; } set { _IsShowOutgoingButton = value; } }
        /// <summary>
        /// 是否有设置代付机构功能
        /// </summary>
        public string IsPaymentInst { get; set; }
        /// <summary>
        /// 主订单背景色是否使用新的
        /// </summary>
        private bool _IsNewOrderBackground = false;
        public bool IsNewOrderBackground { get { return _IsNewOrderBackground; } set { _IsNewOrderBackground = value; } }
        /// <summary>
        /// 经纪商名称
        /// </summary>
        private string _BrokerNameFlag = "TP";
        public string BrokerNameFlag { get { return _BrokerNameFlag; } set { _BrokerNameFlag = value; } }

        /// <summary>
        /// 是否显示发单管理中机构分组删除按钮
        /// </summary>
        public bool ShowInstDeleteBtn { get; set; } = false;


        /// <summary>
        /// 手动发送按钮
        /// </summary>
        public bool IsShowSendManuallyButton { get; set; } = false;


        /// <summary>
        /// 双击间隔,0取系统间隔，其他数字取配置时间
        /// </summary>
        public int DoubleIntervalTime { get; set; } = 200;


        /// <summary>
        /// BDB
        /// </summary>
        public string QQApiHost;
        public string QQApiSender;
        public string QQApiReceiver;
        public string Authorization;

        /// <summary>
        /// 国利
        /// </summary>
        public string QQToTPSC;

        /// <summary>
        /// 信唐
        /// </summary>
        public string QQToCTTJ;

        /// <summary>
        /// 国际
        /// </summary>
        public string QQToCNEX;

        /// <summary>
        /// 中诚
        /// </summary>
        public string QQToCCTB;

        /// <summary>
        /// 平安
        /// </summary>
        public string QQToPTCN;

        /// <summary>
        /// 上田
        /// </summary>
        public string QQToUEDA;

        //BDB Add
        public bool BridgeVisible;
        public bool QuoteBtnVisual;
        public bool DetailPosition;
        public string BrokerBConfirmTime;

        /// <summary>
        /// kline 网页连接
        /// </summary>
        public string KLineUrl;

        /// <summary>
        /// 债券 kline 入口开关
        /// </summary>
        public bool IsKLine;

        /// <summary>
        /// kline 国债期货 网页连接
        /// </summary>
        public string KLineUrl_F;

        /// <summary>
        /// 国债期货 kline 入口开关
        /// </summary>
        public bool IsKLine_F;

        public int? QQStartSleep;
        public int? QQSendSleep;
        public string ShowBestSource;

        public string STPINNERAlgoName { get; set; }

        /// <summary>
        /// 是否支持FQ
        /// </summary>
        public bool IsFQ;

        /// <summary>
        /// 内部看板最优显示样式
        /// 1：是 100+100+100 
        /// 0：否 300
        /// </summary>
        public string IsInnerQtyDetail;

        /// <summary>
        /// 内部看看小数，默认是1
        /// </summary>
        public int QtyDecimalDigit;
        public bool EnableIOC { get; set; }

        /// <summary>
        /// 是否显示风险敞口
        /// </summary>
		public bool IsShowRiskExposure { get; set; }

        /// <summary>
        /// BDB是否分屏
        /// </summary>
        public bool IsBDBSplit;

        /// <summary>
        /// 缓存数据库类型 Sqlite,RocksDb
        /// </summary>
        public string LocalTableType = "SQLITE";

        /// <summary>
        /// 是否FQ群聊全部不进
        /// </summary>
        public bool IsGroupFQNotEnter { get; set; }

        /// <summary>
        /// 是否支持QQ
        /// </summary>
        public bool IsQQ;

        /// <summary>
        /// 自动退出时间
        /// </summary>
        public string ClientAutoExitTime { get; set; }

        /// <summary>
        /// 是否写DBLog
        /// </summary>
        public bool IsWriteDBLog { get; set; }

        /// <summary>
        /// 是否开启询价
        /// </summary>
        public bool IsInquiry { get; set; }

        /// <summary>
        /// 推送界面为自定义群号，默认为读crm， 0：读crm ；1：自定义群号，配置system参数YC_PUSH_QQ
        /// </summary>
        public string PushQQGroup_PushView { get; set; }

        /// <summary>
        /// 批量识别是否有发私域按钮，默认显示， 0：不显示； 1：显示
        /// </summary>
        public string HasSendDemandBtn_ParseView { get; set; }

        /// <summary>
        /// 批量识别是否有发私域按钮，默认显示， 0：不显示； 1：显示
        /// </summary>
        public string HasCopyBtn_ParseView { get; set; }

        public bool IsPTBrokerExec {  get; set; }

        public bool IsI2T { get; set; }

        public bool IsPAYL { get; set; }

        /// <summary>
        /// 平安养老 是否需风控检查 
        /// </summary>
        public bool IsRiskCheck { get; set; }

        /// <summary>
        /// 卖方是否需要风控检查
        /// </summary>
        public bool IsRiskCheck_Sell { get; set; }


        /// <summary>
        /// Chrome浏览器发送FQ
        /// </summary>
        public string EnableChromeSendFQ;

        /// <summary>
        /// FQ地址
        /// </summary>
        public string WebFQAddress;

        /// <summary>
        /// 验证码是否显示
        /// </summary>
        public bool ValidCodeVisible;

        /// <summary>
        /// 测试FinQ开关 如果没有QB权限也开启Webserver
        /// </summary>
        public string IsUseQBPrdTestFQNotOpenQB { get; set; }

        /// <summary>
        /// FQ地址
        /// 是否使用WebFinQ,开启后关闭QQ发送功能
        /// </summary>
        public string IsForceUseWebFinQ { get; set; }
		
        public int strongnotifyfollowstyle = 0;
        /// <summary>
        ///  0 强提醒窗口跟随主窗口 1 已固定强提醒窗口显示的屏幕 
        /// </summary>
        public int StrongNotifyFollowStyle
        {
            get
            {
                return strongnotifyfollowstyle;
            }
            set
            {
                strongnotifyfollowstyle = value;
            }
        }

        public string strongnotifyshowscreen = string.Empty;
        /// <summary>
        /// 强提醒显示窗口
        /// </summary>
        public string StrongNotifyShowScreen
        {
            get
            {
                return strongnotifyshowscreen;
            }
            set
            {
                strongnotifyshowscreen = value;
            }
        }

        /// <summary>
        /// 需求工作台 和 新增需求 是否必填
        /// </summary>
        public string RequiredIns { get; set; }

        /// <summary>
        /// 是否显示报价标准
        /// </summary>
        public string ShowPriceType { get; set; }

        /// <summary>
        /// 同账号多开配置
        /// </summary>
        public string ClientType { get; set; }
    }
}
