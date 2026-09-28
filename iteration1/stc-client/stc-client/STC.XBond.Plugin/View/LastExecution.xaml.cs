using STC.Common.ViewModel;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// MarketExecution.xaml 的交互逻辑
    /// </summary>
    public partial class LastExecution : UserControl
    {
        private LastExecutionViewModel _lastExecutionViewModel = new LastExecutionViewModel();

        public LastExecution()
        {
            InitializeComponent();
        }

        private void MarketExecution_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _lastExecutionViewModel;
        }

        public static readonly DependencyProperty SecurtityIDProperty = DependencyProperty.Register("SecurtityID", typeof(string),
            typeof(LastExecution), new PropertyMetadata("", new PropertyChangedCallback(OnSecurtityIDPropertyChange)));


        static async void OnSecurtityIDPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            LastExecution source = (LastExecution)sender;
            if (source.DataContext != null && source.DataContext is LastExecutionViewModel)
            {
                LastExecutionViewModel viewModel = (LastExecutionViewModel)source.DataContext;
                await viewModel .SetItemSource(args.NewValue.ToString());
            }
        }


        public string SecurtityID
        {
            get { return (string)GetValue(SecurtityIDProperty); }
            set { SetValue(SecurtityIDProperty, value); }
        }
    }
}
