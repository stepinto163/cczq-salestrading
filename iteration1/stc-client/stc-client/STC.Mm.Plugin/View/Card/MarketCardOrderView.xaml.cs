using STC.Mm.Plugin.ViewModel;
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

namespace STC.Mm.Plugin.View.Card
{
    /// <summary>
    /// MarketOrdrView.xaml 的交互逻辑
    /// </summary>
    public partial class MarketCardOrderView : UserControl
    {
        private LastExecutionViewModel _lastExecutionViewModel = new LastExecutionViewModel();
        private string LastSecurtityID = string.Empty;

        public MarketCardOrderView()
        {
            InitializeComponent();


            this.Unloaded += MarketOrderView_Unloaded;
            this.SizeChanged += MarketCardOrderView_SizeChanged;
        }

        private void MarketCardOrderView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Tab_DgPrice.Width = e.NewSize.Width;
            if (this.DataContext != null && this.DataContext is LastExecutionViewModel)
            {
                LastExecutionViewModel viewModel = (LastExecutionViewModel)this.DataContext;
                if (int.Parse(viewModel.OhlcHeight) > 0)
                {
                    if (e.NewSize.Height > 0)
                    {
                        if (e.NewSize.Height - 24 > 0)
                        {
                            Tab_DgPrice.Height = e.NewSize.Height - 24;
                        }
                        else
                        {
                            Tab_DgPrice.Height = 24;
                        }
                    }
                }
                else
                {
                    Tab_DgPrice.Height = e.NewSize.Height;
                }
            }
        }

        private void MarketOrderView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= MarketOrderView_Unloaded;

            if (_lastExecutionViewModel != null)
                _lastExecutionViewModel.Dispose();
        }

        private void MarketExecution_Initialized(object sender, EventArgs e)
        {
            this.DataContext = _lastExecutionViewModel;
        }

        public static readonly DependencyProperty SecurtityIDProperty = DependencyProperty.Register("SecurtityID", typeof(string),
            typeof(MarketCardOrderView), new PropertyMetadata("", new PropertyChangedCallback(OnSecurtityIDPropertyChange)));


        static void OnSecurtityIDPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
            MarketCardOrderView source = (MarketCardOrderView)sender;
            if (source.DataContext != null && source.DataContext is LastExecutionViewModel)
            {
                LastExecutionViewModel viewModel = (LastExecutionViewModel)source.DataContext;
                viewModel.ClearData();
                viewModel.SetItemSource(args.NewValue.ToString());
            }
        }


        public void SetSecurtityID(string securtityID, bool isReset = false)
        {
            if ((string.IsNullOrEmpty(LastSecurtityID)) || (securtityID != LastSecurtityID))
            {
                LastSecurtityID = securtityID;
                if (_lastExecutionViewModel == null)
				{
					_lastExecutionViewModel = new LastExecutionViewModel();
					this.DataContext = _lastExecutionViewModel;
				}
                LastExecutionViewModel viewModel = (LastExecutionViewModel)this.DataContext;
                viewModel.SetItemSource(securtityID);
            }
        }

        public string SecurtityID
        {
            get { return (string)GetValue(SecurtityIDProperty); }
            set { SetValue(SecurtityIDProperty, value); }
        }

        #region 新增成交图形插件处理功能
        private void btnListFloatButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btnListFloatButton.IsChecked.Value)
            {
                Tab_DgPrice.SelectedIndex = 0;
            }
            else
            {
                // 图表模式
                Tab_DgPrice.SelectedIndex = 1;
                if (this.DataContext != null && this.DataContext is LastExecutionViewModel)
                {
                    LastExecutionViewModel viewModel = (LastExecutionViewModel)this.DataContext;
                    if (viewModel.LinePageInstance == null)
                    {
                        // 动态加载外部程序集
                        viewModel.LoadAssembly();
                        MainFrame.Content = viewModel.LinePageInstance;
                    }

                    viewModel.UpdateChartLineData();
                }
            }
        }
        #endregion
    }
}
