using STC.Common.Model;
using STC.Common.Utils;
using STC.Frame.Common;
using STC.Python.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Python.Plugin.View.Setting
{
    /// <summary>
    /// PythonView.xaml 的交互逻辑
    /// </summary>
    public partial class PythonView : UserControl, ISetting
    {
        private PythonViewModel _pythonViewModel = new PythonViewModel();

        public PythonView()
        {
            InitializeComponent();
            this.DataContext = _pythonViewModel;
        }

        public string GetName()
        {
            return "Python插件";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        public void OnSave()
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           
            openTabPage();
        }

        private void openTabPage()
        {
            //string url = "file:///C:/Work/Lite/client/stc-client-release/client/pythonAddin/pystc-docs/index.html";
            string url = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pythonAddin/pystc-docs/index.html");
            LogHandler.Info(url);
            FileInfo file = new FileInfo(url);
            if (!file.Exists)
            { 
                // 判断文件是否存在，不存在解压
                unzip();
            }
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { Url = url, IsTopMost = false, IsOpenInMainTabPage = true, ForceTargetSelf= false };
            cw.Key = "PythonApi";
            cw.Title = "pystc文档";
            cw.ForceTargetSelf = true;
            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
        }

        private void unzip()
        {
            string zipFilePath = AppDomain.CurrentDomain.BaseDirectory + $"pythonAddin/pystc-docs.zip";
            string unzipFilePath = AppDomain.CurrentDomain.BaseDirectory + $"pythonAddin/";

            SharpZip.UnpackFiles(zipFilePath, unzipFilePath);
        }
    }
}
