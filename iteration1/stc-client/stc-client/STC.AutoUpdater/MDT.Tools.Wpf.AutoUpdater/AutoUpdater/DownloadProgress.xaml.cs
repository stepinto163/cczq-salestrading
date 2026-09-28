using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MDT.Tools.Wpf.AutoUpdater
{
    /// <summary>
    /// DownloadProgress.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadProgress : Window
    {
        public DownloadProgress()
        {
            InitializeComponent();
         
        }

        void DownloadProgress_Loaded(object sender, RoutedEventArgs e)
        {
            setUI();
            evtDownload = new ManualResetEvent(true);
            evtDownload.Reset();
            ThreadPool.QueueUserWorkItem(new WaitCallback(this.ProcDownload));
        }

        void DownloadProgress_Closed(object sender, EventArgs e)
        {
            if (!isFinished && MessageBoxResult.No == MessageBox.Show(ConstFile.CANCELORNOT, ConstFile.MESSAGETITLE, MessageBoxButton.YesNo, MessageBoxImage.Question))
            {
                //e.Cancel = true;
                return;
            }
            else
            {
                if (clientDownload != null)
                    clientDownload.CancelAsync();

                evtDownload.Set();
                evtPerDonwload.Set();
            }
        }


        #region The private fields
        private bool isFinished = false;
        private List<DownloadFileInfo> downloadFileList = null;
        private List<DownloadFileInfo> allFileList = null;
        private ManualResetEvent evtDownload = null;
        private ManualResetEvent evtPerDonwload = null;
        private WebClient clientDownload = new HttpWebClient();

        #endregion

        #region The constructor of DownloadProgress
        public DownloadProgress(List<DownloadFileInfo> downloadFileListTemp)
        {
            InitializeComponent();
            this.Closed += DownloadProgress_Closed;
            this.Loaded += DownloadProgress_Loaded;


            this.downloadFileList = downloadFileListTemp;
            allFileList = new List<DownloadFileInfo>();
            foreach (DownloadFileInfo file in downloadFileListTemp)
            {
                allFileList.Add(file);
            }


            //Added the function to support proxy
            clientDownload.Proxy = null;
            //clientDownload.Proxy.Credentials = CredentialCache.DefaultCredentials;
            //clientDownload.Credentials = System.Net.CredentialCache.DefaultCredentials;
            //End added
            #region DownloadProgressChanged
            clientDownload.DownloadProgressChanged +=
                (object sender, DownloadProgressChangedEventArgs e) =>
                {
                    try
                    {
                        //this.SetProcessBar(e.ProgressPercentage,
                        //                   (int)
                        //                   ((nDownloadedTotal +
                        //                     e.BytesReceived) * 100 /
                        //                    total));

                        SetProcessBarCallBack cb = new SetProcessBarCallBack(SetProcessBar);
                        this.Dispatcher.Invoke(cb, new object[] { e.ProgressPercentage,
                                           (int)
                                           ((nDownloadedTotal +
                                             e.BytesReceived) * 100 /
                                            total) });
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error(ex);
                        //EventLog.WriteEntry("DownloadProgress", ex.Message,
                        //                   EventLogEntryType.Error);

                    }

                };
            #endregion

            #region DownloadFileCompleted
            clientDownload.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) =>
            {
                try
                {
                    //DealWithDownloadErrors();
                    DownloadFileInfo dfile =
                        e.UserState as DownloadFileInfo;
                    try
                    {
                        string newPath = System.IO.Path.Combine(
                            tempFolderPath,
                            dfile.FileFullName);
                        FileInfo f = new FileInfo(newPath);
                        if (dfile.Size != f.Length)
                        {
                            LogHelper.Error(
                                string.Format(
                                    "{0}.Size({1})!={2}.Size({3})",
                                    dfile.FileName,
                                    dfile.Size, f.Name,
                                    f.Length));
                            LogHelper.Error(e.Error);
                            isRetry = true;
                            retryCount++;
                        }
                        else
                        {
                            isRetry = false;
                            retryCount = 0;
                            LogHelper.Debug(
                                string.Format(
                                    "Download {0} Success",
                                    dfile.FileName));
                        }
                    }
                    catch (Exception ex)
                    {
                        isRetry = true;
                        retryCount++;
                        LogHelper.Debug(
                            string.Format(
                                "Download {0} Failed",
                                dfile.FileName));
                        LogHelper.Error(ex);
                    }
                    if (!isRetry)
                    {
                        nDownloadedTotal += dfile.Size;
                        //this.SetProcessBar(100,
                        //                   (int)
                        //                   (nDownloadedTotal *
                        //                    100 /
                        //                    total));

                        SetProcessBarCallBack cb = new SetProcessBarCallBack(SetProcessBar);
                        this.Dispatcher.Invoke(cb, new object[] { 100,
                                           (int)
                                           (nDownloadedTotal *
                                            100 /
                                            total) });
                    }



                    evtPerDonwload.Set();
                }
                catch (Exception ex)
                {
                    isRetry = true;
                    retryCount++;
                    LogHelper.Error(ex);
                }

            };
            #endregion

        }
        #endregion

        long total = 0;
        long nDownloadedTotal = 0;
        private bool bCancel = false;
        private bool isRetry = true;
        private int retryCount = 0;
        string tempFolderPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl, ConstFile.TEMPFOLDERNAME);

        private void setUI()
        {
            label1.Content = string.Format("正在更新 {0}", ConstFile.AppName);
            this.Title = string.Format("{0} 更新中", ConstFile.AppName);
            label4.Content = string.Format("名称： {0}", ConstFile.AppName);
        }


        private void ProcDownload(object o)
        {
            Thread.Sleep(1000);

            if (!Directory.Exists(tempFolderPath))
            {
                Directory.CreateDirectory(tempFolderPath);
            }


            evtPerDonwload = new ManualResetEvent(false);

            foreach (DownloadFileInfo file in this.downloadFileList)
            {
                total += file.Size;
            }
            try
            {
                while (!evtDownload.WaitOne(0, false) && !bCancel)
                {
                    if (this.downloadFileList.Count == 0)
                        break;
                    //this.SetProcessBar(100, (int)(nDownloadedTotal * 100 / total));


                    SetProcessBarCallBack cb = new SetProcessBarCallBack(SetProcessBar);
                    this.Dispatcher.Invoke(cb, new object[] { 100, (int)(nDownloadedTotal * 100 / total) });


                    DownloadFileInfo file = this.downloadFileList[0];
                    GC.Collect();
                    LogHelper.Debug(string.Format("Start Download:{0}", file.FileName));
                    //Debug.WriteLine(String.Format("Start Download:{0}", file.FileName));

                    //this.ShowCurrentDownloadFileName(file.FileName);
                    ShowCurrentDownloadFileNameCallBack cb2 = new ShowCurrentDownloadFileNameCallBack(ShowCurrentDownloadFileName);
                    this.Dispatcher.Invoke(cb2, new object[] { file.FileName });

                    //Download

                    //using (clientDownload = new HttpWebClient())
                    {
                        evtPerDonwload.Reset();

                        //Download the folder file
                        string tempFolderPath1 = CommonUnitity.GetFolderUrl(file);
                        if (!string.IsNullOrEmpty(tempFolderPath1))
                        {
                            tempFolderPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl, ConstFile.TEMPFOLDERNAME);
                            tempFolderPath += tempFolderPath1;
                        }
                        else
                        {
                            tempFolderPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl, ConstFile.TEMPFOLDERNAME);
                        }

                        LogHelper.Info("DownloadUrl:" + file.DownloadUrl);
                        clientDownload.DownloadFileAsync(new Uri(file.DownloadUrl),
                                                         System.IO.Path.Combine(tempFolderPath, file.FileFullName), file);

                        //Wait for the download complete
                        evtPerDonwload.WaitOne();


                        if (isRetry)
                        {
                            LogHelper.Debug(string.Format("{0} retry Download[retry count:{1}]", file.FileName,
                                                          retryCount));
                        }

                        if (!isRetry)
                        {
                            this.downloadFileList.Remove(file);
                        }
                        else if (retryCount >= 5)
                        {
                            LogHelper.Debug("Retry Timeout");
                            bCancel = true;
                        }
                    }
                }
                System.GC.Collect();

                if (bCancel)
                {
                    if (retryCount == 5)
                    {
                        CommonUnitity.ShowErrorAndRestartApplication(string.Format("{0}更新不成功[网络异常,请确保网络环境稳定],{0}现在将重新启动,尝试再次更新，请单击确定重新启动程序！", ConstFile.AppName, ""), true);
                    }
                    CommonUnitity.ShowErrorAndRestartApplication(string.Format("{0}更新不成功[{1}],{0}现在将重新启动,尝试再次更新，请单击确定重新启动程序！", ConstFile.AppName, "用户取消升级"), true);
                }

            }
            catch (Exception ex)
            {
                //EventLog.WriteEntry("DownloadProgress", ex.Message,
                LogHelper.Error(ex);                                                                                  //EventLogEntryType.Error);
                CommonUnitity.ShowErrorAndRestartApplication(string.Format("{0}[{1}]", ConstFile.NOTNETWORK, ex.Message), true);
                //throw;
            }

            //When the files have not downloaded,return.
            if (downloadFileList.Count > 0)
            {
                return;
            }


            foreach (DownloadFileInfo file in this.allFileList)
            {
                string tempUrlPath = CommonUnitity.GetFolderUrl(file);
                string oldPath = string.Empty;
                string newPath = string.Empty;
                if (!string.IsNullOrEmpty(tempUrlPath))
                {
                    oldPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1), file.FileName);
                    newPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl + ConstFile.TEMPFOLDERNAME + tempUrlPath, file.FileName);
                }
                else
                {
                    oldPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl, file.FileName);
                    newPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl + ConstFile.TEMPFOLDERNAME, file.FileName);
                }
            }

            LogHelper.Debug("All Downloaded");

            try
            {
                foreach (DownloadFileInfo file in this.allFileList)
                {
                    string tempUrlPath = CommonUnitity.GetFolderUrl(file);
                    string oldPath = string.Empty;
                    string newPath = string.Empty;

                    if (!string.IsNullOrEmpty(tempUrlPath))
                    {
                        oldPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1), file.FileName);
                        newPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl + ConstFile.TEMPFOLDERNAME + tempUrlPath, file.FileName);
                    }
                    else
                    {
                        oldPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl, file.FileName);
                        newPath = System.IO.Path.Combine(CommonUnitity.SystemBinUrl + ConstFile.TEMPFOLDERNAME, file.FileName);
                    }
                    //Added for dealing with the config file download errors
                    string newfilepath = string.Empty;
                    if (newPath.Substring(newPath.LastIndexOf(".") + 1).Equals(ConstFile.CONFIGFILEKEY))
                    {
                        if (System.IO.File.Exists(newPath))
                        {
                            if (newPath.EndsWith("_"))
                            {
                                newfilepath = newPath;
                                newPath = newPath.Substring(0, newPath.Length - 1);
                                oldPath = oldPath.Substring(0, oldPath.Length - 1);
                            }
                            File.Move(newfilepath, newPath);
                        }
                    }
                    //End added

                    if (File.Exists(oldPath))
                    {
                        MoveFolderToOld(oldPath, newPath);
                    }
                    else
                    {
                        //Edit for config_ file
                        if (!string.IsNullOrEmpty(tempUrlPath))
                        {
                            if (!Directory.Exists(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1)))
                            {
                                Directory.CreateDirectory(CommonUnitity.SystemBinUrl + tempUrlPath.Substring(1));
                                MoveFolderToOld(oldPath, newPath);
                            }
                            else
                            {
                                MoveFolderToOld(oldPath, newPath);
                            }
                        }
                        else
                        {
                            MoveFolderToOld(oldPath, newPath);
                        }

                    }
                }


            }
            catch (Exception ex)
            {
                LogHelper.Error(ex);
                CommonUnitity.ShowErrorAndRestartApplication(string.Format("{0}更新不成功[未知异常,请重启电脑或手动删除程序目录下面后缀为.old的所有文件],请单击退出程序", ConstFile.AppName), false);
            }
            finally
            {
                this.allFileList.Clear();

                if (this.downloadFileList.Count == 0)
                {
                    //    Exit(true);

                    ExitCallBack cb = new ExitCallBack(Exit);
                    this.Dispatcher.Invoke(cb, new object[] { true });
                }
                else
                {
                    //    Exit(false);
                    ExitCallBack cb = new ExitCallBack(Exit);
                    this.Dispatcher.Invoke(cb, new object[] { false });
                }

                evtDownload.Set();
            }
        }

        //To delete or move to old files
        private int killCount = 0;
        void MoveFolderToOld(string oldPath, string newPath)
        {

            if (oldPath.IndexOf(ConstFile.AppExe) >= 0)
            {
                CommonUnitity.KillProcess(ConstFile.AppExe);
            }
            LogHelper.Debug(string.Format("MoveFolderToOld oldPath:{0},newPath:{1}", oldPath, newPath));
            if (File.Exists(oldPath + ".old"))
            {
                LogHelper.Debug("1");
                File.Delete(oldPath + ".old");
            }

            if (File.Exists(oldPath))
            {
                LogHelper.Debug("2");
                File.Move(oldPath, oldPath + ".old");

            }
            LogHelper.Debug("3");
            File.Move(newPath, oldPath);
            LogHelper.Debug("4");

            try
            {
                File.Delete(oldPath + ".old");
            }
            catch
            {


            }

            LogHelper.Debug("5");

        }



        delegate void ShowCurrentDownloadFileNameCallBack(string name);
        private void ShowCurrentDownloadFileName(string name)
        {
            this.labelCurrentItem.Content = name;
        }


        delegate void SetProcessBarCallBack(int current, int total);
        private void SetProcessBar(int current, int total)
        {

            this.progressBarCurrent.Value = current;
            this.progressBarTotal.Value = total;
        }


        delegate void ExitCallBack(bool success);
        private void Exit(bool success)
        {
            this.isFinished = success;
            this.DialogResult = success;//? DialogResult.OK : DialogResult.Cancel;

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (clientDownload != null)
            {
                clientDownload.CancelAsync();
            }
            bCancel = true;
            evtDownload.Set();
            evtPerDonwload.Set();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
