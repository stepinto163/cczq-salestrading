/*****************************************************************
 * Copyright (C) Knights Warrior Corporation. All rights reserved.
 * 
 * Author:   圣殿骑士（Knights Warrior） 
 * Email:    KnightsWarrior@msn.com
 * Website:  http://www.cnblogs.com/KnightsWarrior/       http://knightswarrior.blog.51cto.com/
 * Create Date:  5/8/2010 
 * Usage:
 *
 * RevisionHistory
 * Date         Author               Description
 * 
*****************************************************************/
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows;


namespace MDT.Tools.Wpf.AutoUpdater
{
    class CommonUnitity
    {
        private static string pubulicKey = ConfigurationSettings.AppSettings["key1"];
        private static string pwd = ConfigurationSettings.AppSettings["key2"];
        private static string accessId = "";
        private static string assessKey = "";
        private static string bucketName = "";
        public static string SystemBinUrl = AppDomain.CurrentDomain.BaseDirectory;
        public static string user = "";
        public static string psd = "";
        internal static bool isRunProcessName(string processName)
        {
            bool flag = false;
            try
            {


                Process[] prcs = Process.GetProcesses();

                foreach (Process p in prcs)
                {
                    if (p.ProcessName.ToLower().Equals(processName.ToLower()))
                    {
                        string path = p.MainModule.FileName.Replace("\\" + p.MainModule.ModuleName, "");
                        if (!string.IsNullOrEmpty(ConstFile.AppE) && !p.MainModule.FileName.Equals(ConstFile.AppE))
                        {
                            LogHelper.Debug(string.Format("isRunProcessName:StartupPath:{0},ProcPath:{1},continue2", AppDomain.CurrentDomain.BaseDirectory,
                                                 path));
                            continue;
                        }
                        flag = true;
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);

            }
            return flag;
        }

        public static void KillProcess(string processName)
        {
            try
            {

                LogHelper.Debug("kill " + ConstFile.AppExe);
                Process[] prcs = Process.GetProcesses();

                foreach (Process thisproc in Process.GetProcessesByName(processName))
                {
                    string path = thisproc.MainModule.FileName.Replace("\\" + thisproc.MainModule.ModuleName, "");
                    if (!AppDomain.CurrentDomain.BaseDirectory.Equals(path))
                    {
                        LogHelper.Debug(string.Format("StartupPath:{0},ProcPath:{1},continue1", AppDomain.CurrentDomain.BaseDirectory,
                                                      path));
                        continue;
                    }
                    if (!thisproc.CloseMainWindow())
                    {
                        thisproc.Kill();
                        GC.Collect();
                    }
                }

                foreach (Process p in prcs)
                {
                    if (p.ProcessName.ToLower().Equals(processName.ToLower()))
                    {
                        string path = p.MainModule.FileName.Replace("\\" + p.MainModule.ModuleName, "");
                        if (!string.IsNullOrEmpty(ConstFile.AppE) && !p.MainModule.FileName.Equals(ConstFile.AppE))
                        {
                            LogHelper.Debug(string.Format("StartupPath:{0},ProcPath:{1},continue2", AppDomain.CurrentDomain.BaseDirectory,
                                                 path));
                            continue;
                        }
                        p.Kill();
                        LogHelper.Debug(string.Format("{0} kill.", processName));
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);

            }
        }
        public static void KillProcess(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                if (p != null)
                {
                    try
                    {
                        p.Kill();
                        LogHelper.Debug(string.Format("{0} kill.", pid));

                    }
                    catch (Exception e)
                    {
                        LogHelper.Error(e);
                    }

                }

            }
            catch
            {

                LogHelper.Debug(string.Format("{0} is not exist.", pid));
            }


        }
        public static void ShowErrorAndRestartApplication()
        {
            try
            {
                Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.TEMPFOLDERNAME), true);

            }
            catch (Exception e)
            {

                LogHelper.Error(e);
            }
            MessageBox.Show(ConstFile.NOTNETWORK, ConstFile.MESSAGETITLE, MessageBoxButton.OK, MessageBoxImage.Information);
            CommonUnitity.RestartApplication();
        }
        public static void ShowErrorAndRestartApplication(string msg, bool isRestart)
        {

            try
            {
                Directory.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.TEMPFOLDERNAME), true);

            }
            catch (Exception e)
            {

                LogHelper.Error(e);
            }
            finally
            {
                try
                {

                }
                catch
                {
                }
            }
            MessageBox.Show(msg, ConstFile.MESSAGETITLE, MessageBoxButton.OK, MessageBoxImage.Information);


            if (isRestart)
                CommonUnitity.RestartApplication();
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch
            {
            }
        }
        public static void RestartApplication()
        {
            string exe = ConstFile.ROOLBACKFILE;
            if (!File.Exists(exe))
                exe = AppDomain.CurrentDomain.BaseDirectory + ConstFile.ROOLBACKFILE;
            //Process.Start(ConstFile.ROOLBACKFILE, "-N");
            LogHelper.Info(string.Format("user:{0},{1},{2}", user,psd, exe));
            Process.Start(exe, string.Format("-N -u1 {0} -u2 {1}",user,psd) );
            Environment.Exit(0);
        }

        public static string GetFolderUrl(DownloadFileInfo file)
        {
            string folderPathUrl = string.Empty;
            int folderPathPoint = file.DownloadUrl.IndexOf("/", 15) + 1;
            string filepathstring = file.DownloadUrl.Substring(folderPathPoint);
            int folderPathPoint1 = filepathstring.IndexOf("/");
            string filepathstring1 = filepathstring.Substring(folderPathPoint1 + 1);
            if (filepathstring1.IndexOf("/") != -1)
            {
                string[] ExeGroup = filepathstring1.Split('/');
                for (int i = 0; i < ExeGroup.Length - 1; i++)
                {
                    folderPathUrl += "\\" + ExeGroup[i];
                }
                if (!Directory.Exists(SystemBinUrl + ConstFile.TEMPFOLDERNAME + folderPathUrl))
                {
                    Directory.CreateDirectory(SystemBinUrl + ConstFile.TEMPFOLDERNAME + folderPathUrl);
                }
            }
            return folderPathUrl;
        }
    }
}
