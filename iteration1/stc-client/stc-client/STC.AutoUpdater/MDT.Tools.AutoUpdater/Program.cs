using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;

namespace MDT.Tools.AutoUpdater
{
    public static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                #region 配置
                ServicePointManager.ServerCertificateValidationCallback =
                  delegate
                  {
                      return true;
                  };
                  LogHelper.Debug(string.Format("DefaultConnectionLimit:{0}", ServicePointManager.DefaultConnectionLimit));
                string AutoUpdaterLib = System.Configuration.ConfigurationSettings.AppSettings["AutoUpdaterLib"];
                string AutoUpdaterClass = System.Configuration.ConfigurationSettings.AppSettings["AutoUpdaterClass"];
                string AppName = System.Configuration.ConfigurationSettings.AppSettings["AppName"];
                string AppExe = System.Configuration.ConfigurationSettings.AppSettings["AppExe"];
                string Type = System.Configuration.ConfigurationSettings.AppSettings["Type"];
                int type = 0;
                int.TryParse(Type, out type);
                string IsUpdate = "IsUpdate";
                string Update = "Update";
                string RollBack = "RollBack";
                string SetAppName = "SetAppName";
                string SetType = "SetType";
                string SetAppExe = "SetAppExe";
                string SetServerUrl = "SetServerUrl";
                string SetAppPid = "SetAppPid";
                string SetAppE = "SetAppE";
                string ei = "";
                string SetUser = "SetUser";
                string SetPwd = "SetPwd";
                string SetProxyInfo = "SetProxyInfo";
                string SetNewHostAndIp = "SetNewHostAndIp";
                string autoUpdaterUrl = "";
                int pid = 0;
                #endregion
                LogHelper.Info("读取配置成功");
                int isUpdate = 0;
                bool isCheck = true;
                string user = "";
                string psd = "";

                string proxyType = string.Empty;
                string proxyHost = string.Empty;
                string proxyPort = string.Empty;
                string proxyUser = string.Empty;
                string proxyPwd = string.Empty;

                //指定更新IP
                string replaceHost = string.Empty;
                string replacePort = string.Empty;

                #region 参数
                if (args != null && args.Length >= 0)
                {

                    for (int i = 0; i < args.Length; i = i + 2)
                    {
                        LogHelper.Info(string.Format("i:{0}", args[i]));
                        switch (args[i])
                        {
                            case "-i":
                                int.TryParse(args[i + 1], out isUpdate);
                                LogHelper.Info(string.Format("isUpdate:{0}", args[i + 1]));
                                break;

                            case "-c":
                                bool.TryParse(args[i + 1], out isCheck);
                                LogHelper.Info(string.Format("isCheck:{0}", args[i + 1]));
                                break;
                            case "-uc":
                                autoUpdaterUrl = args[i + 1];
                                autoUpdaterUrl = CheckN(autoUpdaterUrl);
                                LogHelper.Info(string.Format("autoUpdaterUrl:{0}", args[i + 1]));
                                break;
                            case "-p":
                                int.TryParse(args[i + 1], out pid);
                                LogHelper.Info(string.Format("pid:{0}", args[i + 1]));
                                break;
                            case "-e":
                                ei = args[i + 1];
                                LogHelper.Info(string.Format("e:{0}", args[i + 1]));
                                break;
                            case "-u1":
                                user = args[i + 1];
                                LogHelper.Info(string.Format("u1:{0}", args[i + 1]));
                                break;
                            case "-u2":
                                psd = args[i + 1];
                                LogHelper.Info(string.Format("u2:{0}", args[i + 1]));
                                break;

                                //代理设置
                            case "-p1":
                                proxyType = args[i + 1];
                                proxyType = CheckN(proxyType);
                                LogHelper.Info(string.Format("p1:{0}", args[i + 1]));
                                break;
                            case "-p2":
                                proxyHost = args[i + 1];
                                proxyHost = CheckN(proxyHost);
                                LogHelper.Info(string.Format("p2:{0}", args[i + 1]));
                                break;
                            case "-p3":
                                proxyPort = args[i + 1];
                                proxyPort = CheckN(proxyPort);
                                LogHelper.Info(string.Format("p3:{0}", args[i + 1]));
                                break;
                            case "-p4":
                                proxyUser = args[i + 1];
                                proxyUser = CheckN(proxyUser);
                                LogHelper.Info(string.Format("p4:{0}", args[i + 1]));
                                break;
                            case "-p5":
                                proxyPwd = args[i + 1];
                                proxyPwd = CheckN(proxyPwd);
                                LogHelper.Info(string.Format("p5:{0}", args[i + 1]));
                                break;

                            case "-rh":
                                replaceHost = args[i + 1];
                                replaceHost = CheckN(replaceHost);
                                LogHelper.Info(string.Format("rh:{0}", args[i + 1]));
                                break;
                            //case "-rp":
                            //    replacePort = args[i + 1];
                            //    replacePort = CheckN(replacePort);
                            //    LogHelper.Info(string.Format("rh:{0}", args[i + 1]));
                            //    break;

                        }
                        if (i != 0 && (args[i - 1].Equals("-u1")))
                        {
                            user = args[i];
                            LogHelper.Info(string.Format("..u1:{0}", args[i]));
                        }
                        else if (i != 0 && (args[i - 1].Equals("-u2")))
                        {
                            psd = args[i];
                            LogHelper.Info(string.Format("..u2:{0}", args[i]));
                        }
                    }

                }
                else
                {
                    LogHelper.Info(string.Format("args is null"));
                }
                //MessageBox.Show(string.Format("{0}{1}{2}",isCheck,isUpdate,autoUpdaterUrl));
                #endregion



                #region 检测
                LogHelper.Debug(string.Format("assembly Begin"));
                LogHelper.Debug(string.Format("AutoUpdaterLib:{0}", AutoUpdaterLib));
                Assembly assembly = readAssembly(AutoUpdaterLib);
                LogHelper.Debug(string.Format("assembly:{0}", assembly));
                LogHelper.Debug(string.Format("assembly End"));
                object o = assembly != null ? assembly.CreateInstance(AutoUpdaterClass) : null;
                LogHelper.Debug(string.Format("AutoUpdaterClass:{0}",o));
                if (o != null)
                {
                    o.GetType().GetMethod(SetAppName).Invoke(o, new object[] { AppName });
                    LogHelper.Debug(string.Format("SetAppName Invoke success[{0}]",AppName));
                    o.GetType().GetMethod(SetAppExe).Invoke(o, new object[] { AppExe });
                    LogHelper.Debug(string.Format("SetAppExe Invoke success[{0}]", AppExe));
                    o.GetType().GetMethod(SetType).Invoke(o, new object[] { type });
                    LogHelper.Debug(string.Format("SetType Invoke success[{0}]", type));
                    if (pid != 0)
                    {
                        o.GetType().GetMethod(SetAppPid).Invoke(o, new object[] {pid});
                        LogHelper.Debug(string.Format("SetAppPid Invoke success[{0}]", pid));
                    }
                    if (!string.IsNullOrEmpty(ei))
                    {
                        o.GetType().GetMethod(SetAppE).Invoke(o, new object[] { ei });
                        LogHelper.Debug(string.Format("SetAppE Invoke success[{0}]", ei));
                    }
                    if(!string.IsNullOrEmpty(autoUpdaterUrl))
                    {
                        o.GetType().GetMethod(SetServerUrl).Invoke(o, new object[] { autoUpdaterUrl });
                        LogHelper.Debug(string.Format("SetServerUrl Invoke success[{0}]", autoUpdaterUrl));
                    }
                    if (user != "")
                    {
                        o.GetType().GetMethod(SetUser).Invoke(o, new object[] { user,psd });
                        LogHelper.Debug(string.Format("SetUser Invoke success[{0}]", user, psd));
                    }
                    if (psd != "")
                    {
                        o.GetType().GetMethod(SetPwd).Invoke(o, new object[] { psd });
                        LogHelper.Debug(string.Format("SetPwd Invoke success[{0}]", psd));
                    }

                    o.GetType().GetMethod(SetProxyInfo).Invoke(o, new object[] { proxyType, proxyHost, proxyPort, proxyUser, proxyPwd });
                    LogHelper.Debug(string.Format("SetProxyInfo Invoke success[{0} {1} {2} {3} {4}]", proxyType, proxyHost, proxyPort, proxyUser, proxyPwd));


                    o.GetType().GetMethod(SetNewHostAndIp).Invoke(o, new object[] { replaceHost, replacePort });
                    LogHelper.Debug(string.Format("SetNewHostAndIp Invoke success[{0} {1}]", replaceHost, replacePort));

                    bool bHasError = false;
                   

                   if(isCheck)
                   {
                       LogHelper.Debug(string.Format("IsUpdate Invoke Begin")); ;
                        isUpdate = (int)o.GetType().GetMethod(IsUpdate).Invoke(o, null);
                        LogHelper.Debug(string.Format("IsUpdate Invoke End[{0}]", isUpdate)); ;
                    }

                    if (isUpdate > 0)
                    {
                        try
                        {
                            LogHelper.Debug(string.Format("Update Invoke Begin"));
                            o.GetType().GetMethod(Update).Invoke(o, null);
                            LogHelper.Debug(string.Format("Update Invoke End"));
                        }
                        catch (Exception e)
                        {
                            LogHelper.Debug(string.Format("1isUpdate....{0}", e));
                            LogHelper.Debug(string.Format("2isUpdate....{0}", isUpdate));
                            bHasError = true;
                        }

                        finally
                        {
                            if (bHasError)
                            {
                                LogHelper.Debug(string.Format("RollBack Invoke Begin")); ;
                                o.GetType().GetMethod(RollBack).Invoke(o, null);
                                LogHelper.Debug(string.Format("RollBack Invoke End"));

                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(string.Format("{0}已经是最新版本", AppName), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                //EventLog.WriteEntry("MDT.Tools.AutoUpdater", ex.Message, EventLogEntryType.Error);
                LogHelper.Error(ex); 
                MessageBox.Show(ex.StackTrace, "失败", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }

        }

        private static string CheckN(string str)
        {
            if (str == "N")
            {
                return string.Empty;
            }
            else
                return str;
        }

        private static Assembly readAssembly(string file)
        {
            Assembly asm = null;
            byte[] stream = readFileReturnBytes(file);
            if (stream != null)
            {
                asm = Assembly.Load(stream);
            }
            return asm;
        }

        private static byte[] readFileReturnBytes(string filePath)
        {
            if (!File.Exists(filePath))
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + filePath))
                {
                    filePath= AppDomain.CurrentDomain.BaseDirectory + filePath;
                }
                else
                    return null;
            }

            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var br = new BinaryReader(fs);

            byte[] buff = br.ReadBytes((int)fs.Length);

            br.Close();
            fs.Close();

            return buff;
        }
    }


    public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
    {
        public bool CheckValidationResult(System.Net.ServicePoint sp,
           System.Security.Cryptography.X509Certificates.X509Certificate cert,
           System.Net.WebRequest req, int problem)
        {
            return true;

        }

    }

}
