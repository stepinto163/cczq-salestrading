using STC.Common.Model;
using STC.Common.Utils;
using STC.Excel.Plugin.ViewModel.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace STC.Excel.Plugin.View.Setting
{
    /// <summary>
    /// UserInfoView.xaml 的交互逻辑
    /// </summary>
    public partial class ExcelView : UserControl, ISetting
    {
        private ExcelViewModel _excelViewModel;
      
        public ExcelView()
        {
            InitializeComponent();
          
            _excelViewModel = new ExcelViewModel();
            this.Loaded += ExcelView_Loaded;
            this.DataContext = _excelViewModel;
        }

         private void ExcelView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= ExcelView_Loaded;
            if (_excelViewModel != null)
                _excelViewModel.Init();
        }

        public void OnSave()
        {
        }

        public string GetName()
        {
            return "EXCEL插件";
        }

        public bool NeedSaveButton()
        {
            return false;
        }

        private void ApiStcButton_Click(object sender, RoutedEventArgs e)
        {
            openTabPage();
        }

        private void openTabPage()
        {
            //string url = "file:///C:/Work/Lite/client/stc-client-release/client/excelAddin/ExcelUserGuideBook.pdf";
            string url = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "excelAddin/ExcelUserGuideBook.pdf");
            Common.Model.Chrome.ChromeWinModel cw = new Common.Model.Chrome.ChromeWinModel() { Url = url, IsTopMost = false, IsOpenInMainTabPage = true, ForceTargetSelf = false };
            cw.Key = "ExcelApi";
            cw.Title = "Excel文档";
            cw.ForceTargetSelf = true;
            AppUtils.Application.PluginManager.Application.BroadCast("OpenChrome", cw);
        }
    }
}
