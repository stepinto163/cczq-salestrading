using CefSharp;
using CefSharp.Wpf;
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

namespace STC.BilateralMarket.Plugin.UI
{
    /// <summary>
    /// WebUI.xaml 的交互逻辑
    /// </summary>
    public partial class WebUI : UserControl
    {
        private bool isLoad = false;
        public WebUI()
        {
            //string locale = "zh-CN";
            ////string locale = "en-US";
            //CefSettings settings = new CefSettings();
            //settings.Locale = locale;
            //settings.AcceptLanguageList = locale;
            //bool b = Cef.Initialize(settings);
            //Console.WriteLine("Cef.Initialize: {0}", b);

            InitializeComponent();

            Loaded += WebUI_Loaded; ;
        }

        private void WebUI_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoad)
            {
                isLoad = true;
                Button_Click(null, null);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            webBrowser.Address = tbAddress.Text;
        }
    }
}
