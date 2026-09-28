using MDT.Tools.Wpf.AutoUpdater;
using MDT.Tools.Wpf.AutoUpdater.DiyMessageBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml;

using LogHelper = com.gateway.log.LogHelper;
using STC.Common.Utils;
using com.gateway.connector.tcp;
using Newtonsoft.Json;

namespace STC.Frame.Update
{
    /// <summary>
    /// 处理客户端更新版本逻辑
    /// </summary>
    public class ClientUpdate
    {
        private bool bNeedRestart = false;
        private static ClientUpdate clientUpdate = new ClientUpdate();
        private Config config = null;
        private FileDetail fileConfig = null;
        MDT.Tools.Wpf.AutoUpdater.AutoUpdater autoUpdater = new MDT.Tools.Wpf.AutoUpdater.AutoUpdater();

        public static ClientUpdate Instance {  get { return clientUpdate; } }

        private ClientUpdate()
        {
            
        }

        public void Init()
        {
            ServicePointManager.ServerCertificateValidationCallback =
              delegate
              {
                  return true;
              };

            config = Config.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.FILENAME));
            fileConfig = FileDetail.LoadConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Consts.FILENAME));
        }


        public bool CheckLastVersion()
        {
            return CheckVersion() > 0;
        }

        #region 静默版本更新

        private void Update()
        {
            try
            {
                int checkResult = CheckVersion();
                LogHelper.Info(string.Format("checkResult:{0}", checkResult));
                if (checkResult > 0)
                {
                    try
                    {
                        Thread.Sleep(2000);
                    }
                    catch (Exception e)
                    { }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBoxOkCancel messageBox = new MessageBoxOkCancel();
                        messageBox.Owner = Application.Current.MainWindow;
                        if (checkResult == 3)
                            messageBox.ForceChange();

                        messageBox.ShowDialog();

                        if (messageBox.resultBox == ResultBox.Ok)
                        {
                            string fileName = Process.GetCurrentProcess().MainModule.FileName + "|" + "admin" + "|" + "Beta版本:4.0.0.00002";
                            int pid = Process.GetCurrentProcess().Id;
                            //uc为正确的url配置名，修改为uctemp表示不配置更新url
                            LogHelper.Info("AutoUpdater Parameter: " + string.Format("-i {0} -c {1} -uc {2} -p {3} -e {4} -p1 {5} -p2 {6} -p3 {7} -p4 {8} -p5 {9}", 
                                checkResult, false, string.Empty, pid, fileName, 
                                ProxyUtils.ProxyInfo.ProxyType, ProxyUtils.ProxyInfo.ProxyAddress, ProxyUtils.ProxyInfo.ProxyPort, ProxyUtils.ProxyInfo.ProxyUser, ProxyUtils.ProxyInfo.ProxyPasswd));
                            Process.Start("MDT.Tools.AutoUpdater.exe", string.Format("-i {0} -c {1} -uctemp {2} -p {3} -e {4} -p1 {5} -p2 {6} -p3 {7} -p4 {8} -p5 {9}", 
                                checkResult, false, string.Empty, pid, fileName,
                                ProxyUtils.ProxyInfo.ProxyType, ProxyUtils.ProxyInfo.ProxyAddress, ProxyUtils.ProxyInfo.ProxyPort, ProxyUtils.ProxyInfo.ProxyUser, ProxyUtils.ProxyInfo.ProxyPasswd));
                            LogHelper.Info(string.Format("AutoUpdate Logout [{0}]", true));
                            Application.Current.Shutdown();
                            //Environment.Exit(0);
                        }
                    });
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(string.Format("Update client exception:{0}", e));
            }
        }

        public void UpdateClient()
        {
            Thread t = new Thread(Update);
            t.IsBackground = true;
            t.Start();
            
        }

        public void SetProxyInfo()
        {
            MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyType setPrxoyType = MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyType.None;
            if (!string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyType) && !string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyAddress) && !string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPort))
            {
                string proxyType = ProxyUtils.ConvertToUpdate();
                if (proxyType == "1")
                {
                    setPrxoyType = MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyType.http;
                }
                else if (proxyType == "2")
                {
                    setPrxoyType = MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyType.socks4;
                }
                else if (proxyType == "3")
                {
                    setPrxoyType = MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyType.socks5;
                }
                else if (proxyType == "4")
                {
                    setPrxoyType = MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyType.IE;
                }
                int port = 0;
                int.TryParse(ProxyUtils.ProxyInfo.ProxyPort, out port);
                MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyUtils.ProxyInfo = new MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyInfo(setPrxoyType, 
                    ProxyUtils.ProxyInfo.ProxyAddress, port, ProxyUtils.ProxyInfo.ProxyUser, ProxyUtils.ProxyInfo.ProxyPasswd);
            }
            LogHelper.Info(string.Format("SetProxyInfo:{0}", JsonConvert.SerializeObject(MDT.Tools.Wpf.AutoUpdater.Proxy.ProxyUtils.ProxyInfo)));
        }
        public int CheckVersion()
        {
            int result = 0;

            if (config.Enabled)
            {
                string urls = "";
                LogHelper.Info(string.Format("ServerUrl:{0}", config.ServerUrl));
                SetProxyInfo();
                if (config.Type == 0)
                {
                    using (var webClient = new HttpWebClient())
                    {
                        try
                        {
                            urls = webClient.DownloadString(HostHelper.ReplaceIpAndPort(config.ServerUrl));

                            switch (STC.Common.Utils.LocalEnv.ServerInfo.AppName.ToLower())
                            {
                                case "bdb":
                                case "idcclient":
                                    urls = HostHelper.ReplaceIpAndPort(urls);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    }

                    LogHelper.Info(string.Format("AutoUpdaterUrl:{0}", urls));
                }
                else if (config.Type == 1)
                {
                    urls = HostHelper.ReplaceIpAndPort(config.ServerUrl);
                }
                
                LogHelper.Info("urls:" + urls);
                string[] strs = string.IsNullOrEmpty(urls) ? null : urls.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                if (strs != null)
                    LogHelper.Info(string.Format("strs:{0}", strs));
                if (strs != null && strs.Length > 0)
                {
                    RemoteConfig remoteConfig = null;

                    foreach (var str in strs)
                    {
                        try
                        {
                            LogHelper.Info(string.Format("str:{0}", str));
                            remoteConfig = ParseRemoteXml(str);
                            LogHelper.Info(string.Format("remoteConfig:{0}", remoteConfig));
                            break;
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }
                    }



                    if (remoteConfig != null)
                    {
                        string mversion = remoteConfig.MinimumRequiredVersion;
                        LogHelper.Info(string.Format("remote MinimumRequiredVersion:{0}", mversion));
                        try
                        {
                            Version mv = new Version(mversion);
                            Version vc = new Version(config.Version);
                            LogHelper.Info(string.Format("mv:{0}", mv));
                            LogHelper.Info(string.Format("vc:{0}", vc));
                            if (vc < mv)
                            {
                                LogHelper.Info(string.Format("local version:{0}", vc));
                                result = 3;

                                return result;
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex);
                        }


                        Dictionary<string, RemoteFile> listRemotFile = remoteConfig.RemoteFiles;

                        List<DownloadFileInfo> downloadList = new List<DownloadFileInfo>();

                        foreach (LocalFile file in config.UpdateFileList)
                        {
                            if (listRemotFile.ContainsKey(file.Path))
                            {
                                LogHelper.Info(string.Format("file.Path:{0}", file.Path));
                                try
                                {
                                    RemoteFile rf = listRemotFile[file.Path];
                                    Version v1 = new Version(rf.LastVer);
                                    Version v2 = new Version(file.LastVer);
                                    LogHelper.Info(string.Format("v1:{0}, v2:{1}, rf.Size:{2}, file.Size:{3}, rf.MD5:{4}, file.MD5:{5}", 
                                        v1, v2, rf.Size, file.Size, rf.MD5, file.MD5));
                                    if (v1 > v2 || rf.Size != file.Size || rf.MD5 != file.MD5)
                                    {
                                        LogHelper.Info(string.Format("file:{0} param not the same", file.Path));
                                        downloadList.Add(new DownloadFileInfo(HostHelper.ReplaceIpAndPort(rf.Url), file.Path, rf.LastVer, rf.Size));
                                        file.LastVer = rf.LastVer;
                                        file.Size = rf.Size;

                                        if (rf.NeedRestart)
                                            bNeedRestart = true;
                                    }
                                    listRemotFile.Remove(file.Path);
                                }
                                catch (Exception ex)
                                {
                                    LogHelper.Error(ex);
                                }


                            }
                        }

                        bool needUpdate = CheckFilePermission(listRemotFile, downloadList, LocalEnv.UserInfo.prods);

                        if (needUpdate)
                            result = 2;

                        return result;
                    }
                    else
                    {
                        LogHelper.Error("AutoUpdater IsUpdate no autoUpdaterUrl:" + config.ServerUrl);
                        //EventLog.WriteEntry("AutoUpdater IsUpdate", "no autoUpdaterUrl:" + config.ServerUrl, EventLogEntryType.Error);
                    }
                }
                else
                {
                    LogHelper.Error("AutoUpdater IsUpdate no autoUpdaterUrl:" + config.ServerUrl);
                    //EventLog.WriteEntry("AutoUpdater IsUpdate", "no autoUpdaterUrl:" + config.ServerUrl, EventLogEntryType.Error);

                }
            }

            return result;
        }

        private string GetFileType(string filePath, FileDetail remoteFileConfig)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(filePath))
            {
                foreach (LocalFileParam fileParam in remoteFileConfig.UpdateFileList)
                {
                    if (fileParam.Path.Equals(filePath))
                    {
                        result = fileParam.Filetype;
                        break;
                    }
                }
            }
            return result;
        }

        private bool CheckFilePermission(Dictionary<string, RemoteFile> listRemotFile, List<DownloadFileInfo> downloadList, string loginType)
        {
            bool result = false;

            foreach (RemoteFile rf in listRemotFile.Values)
            {
                downloadList.Add(new DownloadFileInfo(HostHelper.ReplaceIpAndPort(rf.Url), rf.Path, rf.LastVer, rf.Size));
                LocalFile lf = new LocalFile(rf.Path, rf.LastVer, rf.Size, rf.MD5);
                config.UpdateFileList.Add(lf);
            }

            if (downloadList != null && downloadList.Count > 0)
            {
                //关闭文件权限检查
                return true;


                FileDetail remoteFileConfig = null;

                try
                {
                    LogHelper.Debug("FileConfig serverurl:" + fileConfig.ServerUrl);
                    remoteFileConfig = this.ParseRemoteFileXml(HostHelper.ReplaceIpAndPort(fileConfig.ServerUrl));
                }
                catch (Exception ex)
                {
                    LogHelper.Error(ex);
                }
                
                if (remoteFileConfig != null)
                {

                    foreach (DownloadFileInfo df in downloadList)
                    {
                        string fileType = GetFileType(df.FileName, remoteFileConfig);
                        LogHelper.Debug("FileName:" + df.FileName + ", FileType:" + fileType);
                        if (!string.IsNullOrEmpty(fileType))
                        {
                            if (fileType.ToUpper() == "System".ToUpper())
                            {
                                result = true;
                                break;
                            }
                            else if (loginType.ToUpper().Contains(fileType.ToUpper()))
                            //&& df.FileType == FileType.Bond) || (loginType == "Bill" && df.FileType == FileType.Bill)
                            //    || (loginType == "Matching" && df.FileType == FileType.MP))
                            {
                                result = true;
                                break;

                            }
                        }
                    }
                }
                else
                {
                    LogHelper.Error("AutoUpdater FileDetail no serverurl:" + HostHelper.ReplaceIpAndPort(config.ServerUrl));
                }
            }
            
            return result;
        }

        private RemoteConfig ParseRemoteXml(string xml)
        {
            using (var webClient = new HttpWebClient())
            {
                string xmlDocument = string.Empty;
                try
                {
                    xmlDocument = webClient.DownloadString(xml);
                }
                catch
                {
                    try
                    {
                        xmlDocument = webClient.DownloadString(xml);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                }

                XmlDocument document = new XmlDocument();
                document.LoadXml(xmlDocument);

                RemoteConfig remoteConfig = new RemoteConfig(document);
                return remoteConfig;
            }
        }

        private FileDetail ParseRemoteFileXml(string url)
        {
            using (var webClient = new HttpWebClient())
            {
                string xmlDocument = string.Empty;
                try
                {
                    xmlDocument = webClient.DownloadString(url);
                }
                catch
                {
                    try
                    {
                        xmlDocument = webClient.DownloadString(url);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                    }
                }

                XmlDocument document = new XmlDocument();
                document.LoadXml(xmlDocument);

                FileDetail remoteConfig = new FileDetail(document);
                return remoteConfig;
            }
        }
        #endregion


        public bool NeedUpdateClient(string startType)
        {
            bool result = false;

            int checkResult = CheckVersion();
            LogHelper.Info(string.Format("checkResult:{0}", checkResult));
            

            return checkResult > 0;
        }

        private void ParseLocalHoST()
        {
            if (LocalEnv.ServerInfo  != null)
            {
                HostHelper.ReplaceHost = LocalEnv.ServerInfo.UpdateHost;

                int intPort = 0;

                if (int.TryParse(LocalEnv.ServerInfo.UpdatePort, out intPort))
                {
                    HostHelper.ReplacePort = intPort;
                }

                HostHelper.ReplaceProtocol = LocalEnv.ServerInfo.UpdateProtocol;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startType">1:QB, 2:loginUI</param>
        public void StartUpdateClient(string startType)
        {
            ParseLocalHoST();

            int checkResult = CheckVersion();
            LogHelper.Info(string.Format("checkResult:{0}", checkResult));
            if (checkResult > 0)
            {
                //try
                //{
                //    Thread.Sleep(2000);
                //}
                //catch (Exception e)
                //{ }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBoxOkCancel messageBox = new MessageBoxOkCancel();
                    messageBox.Owner = Application.Current.MainWindow;
                    messageBox.Topmost = true;
                    if (checkResult == 3)
                    {
                        //#if DEBUG
                        //return;

                        //#else

                        messageBox.ForceChange();
                        //#endif
                    }
                    messageBox.ShowDialog();

                    if (messageBox.resultBox == ResultBox.Ok)
                    {
                        string proxyType = ProxyUtils.ConvertToUpdate();
                        string fileName = Process.GetCurrentProcess().MainModule.FileName + "|" + "admin" + "|" + "Beta版本:4.0.0.00002";
                        int pid = Process.GetCurrentProcess().Id;
                        //uc为正确的url配置名，修改为uctemp表示不配置更新url
                        LogHelper.Info("AutoUpdater Parameter: " + string.Format("-i {0} -c {1} -uc {2} -p {3} -e {4} -p1 {5} -p2 {6} -p3 {7} -p4 {8} -p5 {9} -rh {10}", 
                            checkResult, false, string.Empty, pid, fileName,
                            proxyType, ProxyUtils.ProxyInfo.ProxyAddress, ProxyUtils.ProxyInfo.ProxyPort, ProxyUtils.ProxyInfo.ProxyUser, ProxyUtils.ProxyInfo.ProxyPasswd,
                            string.IsNullOrEmpty(HostHelper.ReplaceHost) ? "N" : HostHelper.ReplaceHost));
                        //Process.Start("MDT.Tools.AutoUpdater.exe", string.Format("-i {0} -c {1} -uctemp {2} -p {3} -e {4}", checkResult, false, string.Empty, pid, fileName));
                        //LogHelper.Info(string.Format("AutoUpdate Logout [{0}]", true));
                        //Application.Current.Shutdown();
                        //Environment.Exit(0);
                        LogHelper.Info(string.Format("ReplaceHost:{0}, ReplacePort:{1}", HostHelper.ReplaceHost, HostHelper.ReplacePort));
                        LogHelper.Info("Environment.CurrentDirectory:" + Environment.CurrentDirectory);
                        LogHelper.Info("System.AppDomain.CurrentDomain.BaseDirectory:" + System.AppDomain.CurrentDomain.BaseDirectory);
                        string exe = string.Format("{0}MDT.Tools.AutoUpdater.exe", System.AppDomain.CurrentDomain.BaseDirectory);
                        LogHelper.Warn("Start exe:" + exe);
                        if (!File.Exists(exe))
                            exe = string.Format("{0}IDCClient\\MDT.Tools.AutoUpdater.exe", System.AppDomain.CurrentDomain.BaseDirectory);
                        if (!string.IsNullOrEmpty(startType) && startType.Equals("1"))
                        {
                            Process.Start(exe, string.Format("-i {0} -c {1} -uctemp {2} -p {3} -e {4} -u1 {5} -u2 {6} -p1 {7} -p2 {8} -p3 {9} -p4 {10} -p5 {11} -rh {12}",
                                checkResult, false, "N", pid, fileName, LocalEnv.UserInfo.UserName, LocalEnv.UserInfo.Password2,
                                proxyType,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyAddress) ? "N" : ProxyUtils.ProxyInfo.ProxyAddress,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPort) ? "N" : ProxyUtils.ProxyInfo.ProxyPort,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyUser) ? "N" : ProxyUtils.ProxyInfo.ProxyUser,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPasswd) ? "N" : ProxyUtils.ProxyInfo.ProxyPasswd,
                                string.IsNullOrEmpty(HostHelper.ReplaceHost) ? "N" : HostHelper.ReplaceHost));
                        }
                        else
                        {
                            Process.Start(exe, string.Format("-i {0} -c {1} -uctemp {2} -p {3} -e {4} -p1 {5} -p2 {6} -p3 {7} -p4 {8} -p5 {9} -rh {10}",
                                checkResult, false, "N", pid, fileName,
                                proxyType,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyAddress) ? "N" : ProxyUtils.ProxyInfo.ProxyAddress,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPort) ? "N" : ProxyUtils.ProxyInfo.ProxyPort,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyUser) ? "N" : ProxyUtils.ProxyInfo.ProxyUser,
                                string.IsNullOrEmpty(ProxyUtils.ProxyInfo.ProxyPasswd) ? "N" : ProxyUtils.ProxyInfo.ProxyPasswd,
                                string.IsNullOrEmpty(HostHelper.ReplaceHost) ? "N" : HostHelper.ReplaceHost));
                        }

                        Application.Current.Shutdown();
                        LogHelper.Info(string.Format("AutoUpdate Logout [{0}]", checkResult));
                        // 启动后台线程监控，如果 2 秒后进程还没退出，则强制 Kill
                        new Thread(() =>
                        {
                            Thread.Sleep(2000); // 等待 2 秒
                            try
                            {
                                Environment.Exit(0);
                            }
                            catch { }
                        })
                        {
                            IsBackground = true // 后台线程，不阻止程序退出
                        }.Start();
                    }
                });
            }
        }
    }
}
