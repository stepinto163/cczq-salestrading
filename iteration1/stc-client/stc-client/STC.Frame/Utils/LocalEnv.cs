using STC.Frame.Utils.Cfg;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace STC.Frame.Utils
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
        public static void Load()
        {
            UserInfo = new UserInfo();
            ServerInfo = new ServerInfo();
            LogInfo = new LogInfo();
            ConfInfo = new ConfInfo();

            int num = 0;
            string startNum = System.Configuration.ConfigurationManager.AppSettings["StartNum"];
            if (int.TryParse(startNum, out num))
                ConfInfo.StartNum = num;
            //ServerInfo.Host = System.Configuration.ConfigurationManager.AppSettings["Host"];
            //ServerInfo.Port = System.Configuration.ConfigurationManager.AppSettings["Port"];
            //ServerInfo.Protocal = System.Configuration.ConfigurationManager.AppSettings["Protocal"];
            ServerInfo.LoginTitle = System.Configuration.ConfigurationManager.AppSettings["LoginTitle"];
            ServerInfo.AppExe = System.Configuration.ConfigurationManager.AppSettings["AppExe"];
            ServerInfo.AppName = System.Configuration.ConfigurationManager.AppSettings["AppName"];
            switch (ServerInfo.AppExe)
            {
                case "STC":
                    ServerInfo.AppImage = new BitmapImage(new Uri("pack://application:,,,/Images/stc_windows_icon_128.ico"));
                    break;
                //case "IDBPlus":
                //    ServerInfo.AppImage = new BitmapImage(new Uri("pack://application:,,,/Images/128_128_2.png"));//new BitmapImage(new Uri("../Images/128_128_2.png", UriKind.Relative));
                //    break;
            }
            ServerInfo.Version = System.Configuration.ConfigurationManager.AppSettings["Version"];

            LogInfo.LogPath = System.Configuration.ConfigurationManager.AppSettings["LogPath"];
            LogInfo.LogLevel = System.Configuration.ConfigurationManager.AppSettings["LogLevel"];

            ConfInfo.MaxWarningCount = System.Configuration.ConfigurationManager.AppSettings["MaxWarningCount"];
            ConfInfo.CanGoodsQuickSearch = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["CanGoodsQuickSearch"], StringComparison.InvariantCultureIgnoreCase) ? true : false;

            ConfInfo.ResetDataTime = System.Configuration.ConfigurationManager.AppSettings["ResetDataTime"];
            ConfInfo.TaskDelayTime = System.Configuration.ConfigurationManager.AppSettings["TaskDelayTime"];
            ConfInfo.CalType = System.Configuration.ConfigurationManager.AppSettings["CalType"];
            ConfInfo.EnabledMsgLog = System.Configuration.ConfigurationManager.AppSettings["EnableMsgLog"];
            ConfInfo.UserDrag = "true".Equals(System.Configuration.ConfigurationManager.AppSettings["UserDrag"], StringComparison.OrdinalIgnoreCase);

            ConfInfo.EnvironmentFlag = System.Configuration.ConfigurationManager.AppSettings["EnvironmentFlag"];

            ConfInfo.IsGzip = System.Configuration.ConfigurationManager.AppSettings["IsGzip"];
            ConfInfo.StartDps = System.Configuration.ConfigurationManager.AppSettings["StartDps"];
            ConfInfo.ApiKey = System.Configuration.ConfigurationManager.AppSettings["ApiKey"];
            ConfInfo.TradeType = System.Configuration.ConfigurationManager.AppSettings["TradeType"];

            ConfInfo.LoginEnv = System.Configuration.ConfigurationManager.AppSettings["LoginEnv"];

            if (string.IsNullOrEmpty(ConfInfo.TradeType))
            {
                ConfInfo.TradeType = "2";
            }

            string useOHLCSvr = System.Configuration.ConfigurationManager.AppSettings["UseOHLCSvr"];
            ConfInfo.IsUseOHLCSvr = string.IsNullOrEmpty(useOHLCSvr) ? "0" : useOHLCSvr;

            string useVenueSort = System.Configuration.ConfigurationManager.AppSettings["UseVenueSort"];
            ConfInfo.IsUseVenueSort = string.IsNullOrEmpty(useVenueSort) ? "0" : useVenueSort;

            string _LockScreenMinute = System.Configuration.ConfigurationManager.AppSettings["LockScreenMinute"];
            ConfInfo.LockScreenMinute = string.IsNullOrEmpty(_LockScreenMinute) ? "0" : _LockScreenMinute;

            string _LockScreenStyle = System.Configuration.ConfigurationManager.AppSettings["LockScreen"];
            ConfInfo.LockScreen = string.IsNullOrEmpty(_LockScreenStyle) ? "0" : _LockScreenStyle;

            ServerCfgSection serverCfgSection = System.Configuration.ConfigurationManager.GetSection("ServerGroupCfg") as ServerCfgSection;
            ConfInfo.ServerInfos = new List<ServerInfo>();
            foreach (ServerSection ss in serverCfgSection.ServerGroup)
            {
                ServerInfo serverInfo = new ServerInfo();
                serverInfo.Id = ss.Id;
                serverInfo.Host = ss.Host;
                serverInfo.Name = ss.Name;
                serverInfo.Port = ss.Port;
                serverInfo.Protocal = ss.Protocol;
                serverInfo.AppExe = ServerInfo.AppExe;
                serverInfo.AppName = ServerInfo.AppName;
                serverInfo.AppImage = ServerInfo.AppImage;
                serverInfo.Version = ServerInfo.Version;
                ConfInfo.ServerInfos.Add(serverInfo);
            }

            ServerStatusCfgSection serverStatusCfgSection = System.Configuration.ConfigurationManager.GetSection("ServerStatusGroupCfg") as ServerStatusCfgSection;
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

            IsDcsBrokerCfgSection isDcsBrokerCfgSection = System.Configuration.ConfigurationManager.GetSection("IsDcsBrokerGroupCfg") as IsDcsBrokerCfgSection;
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
        }
    }

    public class UserInfo
    {
        public string prods;
        public string UserId;
        private string userName;
        public string Termids;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
            }
        }
        public string FullName;
        public string Password;
        public string Password2;
        public string SessionID;
        public ServerInfo ServerInfo;
        public string GroupIds;
        public string GroupNames;

        public bool IsMyself(string userid)
        {
            return UserId.Equals(userid);
        }

        public bool IsMySelfByName(string name)
        {
            return FullName.Equals(name);
        }
    }

    public class ServerInfo
    {
        public string Id;
        public string Name;
        public string Host;
        public string Port;
        public string Protocal;
        public string LoginTitle;

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
    }

    public class IsDcsBroker
    {
        public string Id;
        public string Broker;
        public string Name;
        public string Status;
    }

    public class ConfInfo
    {
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
        public string TaskDelayTime;
        public List<ServerStatus> ServerStatus;
        public string CalType;
        public string EnabledMsgLog;
        public bool UserDrag;
        public Dictionary<string,IsDcsBroker> IsDcsBroker;

        /// <summary>
        /// 1 使用OHLC 0 不显示 2 使用默认的 info1
        /// </summary>
        public string IsUseOHLCSvr;
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

    }
}
