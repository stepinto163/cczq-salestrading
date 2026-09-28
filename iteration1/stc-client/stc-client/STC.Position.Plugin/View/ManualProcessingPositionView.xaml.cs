using STC.Common.Utils;
using STC.Position.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// ManualInput.xaml 的交互逻辑
    /// </summary>
    public partial class ManualProcessingPositionView : Window
    {
        private ManualProcessingPositionViewModel viewModel = null;
        public ManualProcessingPositionView()
        {
            InitializeComponent();
            CommonUtils.SetIcon(TitleIcon);
            viewModel = new ManualProcessingPositionViewModel(this);
            this.DataContext = viewModel;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        public void ShowEMSMask(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                emsloadMask.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
            }));
        }


        public void SetTopmost(bool flag)
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                this.Topmost = flag;
            }));
        }


    }
}
