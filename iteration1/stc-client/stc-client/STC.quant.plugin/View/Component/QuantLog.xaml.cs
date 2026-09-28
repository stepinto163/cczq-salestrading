using STC.Common.View.Component;
using STC.quant.plugin.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.quant.plugin.View.Component
{
    /// <summary>
    /// QuantLog.xaml 的交互逻辑
    /// </summary>
    public partial class QuantLog : UserControl
    {
        public string quantID = string.Empty;
        public string id = string.Empty;
        QuantLogViewModel viewModel = null;
        public QuantLog()
        {
            InitializeComponent();
            //this.Title = title;
            //viewModel = new QuantLogViewModel(this.dgLog);
            //this.DataContext = viewModel;
            //this.Loaded += QuantLog_Loaded;
        }

        private void QuantLog_Loaded(object sender, RoutedEventArgs e)
        {
           // viewModel.init(quantID, id, dgLog);
        }


        //public void WriteToLog(TTbookQuantLog message)
        //{
        //    //string strTime = "[" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] ";
        //    string txt = string.Format("{0}-{1}", message.Createtime, message.Content) + "\r\n";
        //    Application.Current.Dispatcher.BeginInvoke((Action)(() =>
        //    {
        //        txtRich.AppendText(txt);
        //        txtRich.ScrollToEnd();
        //    }));
        //}


        private void btClose_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           // this.DragMove();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
           // this.Close();
        }
    }
}
