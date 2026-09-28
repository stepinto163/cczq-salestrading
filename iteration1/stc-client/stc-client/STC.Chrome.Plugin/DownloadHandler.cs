using CefSharp;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.Chrome.Plugin
{
    public class DownloadHandler: IDownloadHandler
    {
        private readonly Action<bool, DownloadItem> _downloadCallBackEvent;//第一个参数为true为update

        public DownloadHandler()
        { 
        }

        public DownloadHandler(Action<bool, DownloadItem> downloadCallBackEvent)
        {
            _downloadCallBackEvent = downloadCallBackEvent;
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            _downloadCallBackEvent?.Invoke(true, downloadItem);
        }

        //bool IDownloadHandler.CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
        //{
        //    return true;
        //}
        
        private string GetDownloadFullPath(string suggestedFileName)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.FileName = suggestedFileName;
            bool? res = saveFileDialog.ShowDialog();
            if (res != null && (bool)res)
            {
                string file = saveFileDialog.FileName;
                if (!string.IsNullOrEmpty(file))
                {
                    return file;
                }
                else
                {
                    var configPath = System.IO.Path.Combine(System.Environment.CurrentDirectory);
                    return configPath + suggestedFileName;
                }
            }
            return string.Empty;
        }

        void IDownloadHandler.OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            if (callback.IsDisposed) return;
            _downloadCallBackEvent?.Invoke(false, downloadItem);
            downloadItem.IsInProgress = true;
            var path = GetDownloadFullPath(downloadItem.SuggestedFileName);
            if (!string.IsNullOrEmpty(path))
                callback.Continue(path, false);
        }

        void IDownloadHandler.OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            if (downloadItem.IsComplete)
            {
                string s = $"{downloadItem.FullPath} 下载完成!";
                browser.ExecuteScriptAsync($"alert('{s}')");
            }
        }
    }
}
