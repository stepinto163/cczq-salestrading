using STC.Position.Plugin.ViewModel;
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

namespace STC.Position.Plugin.View
{
    /// <summary>
    /// HistoryProcessingPositionStatus.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryProcessingPositionStatus : UserControl
    {
        HistoryProcessingPositionStatusViewModel viewModel = null;

        public HistoryProcessingPositionStatus()
        {
            InitializeComponent();
            viewModel = new HistoryProcessingPositionStatusViewModel(this);
            this.DataContext = viewModel;
        }

        private void LstReplayStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender is ListBox)
            {
                ListBox listBox = sender as ListBox;
                if (this.DataContext != null)
                {
                    List<string> sel = new List<string>();
                    foreach (ListBoxItem item in listBox.SelectedItems)
                    {
                        sel.Add(item.Tag.ToString());

                    }

                    (this.DataContext as HistoryProcessingPositionStatusViewModel).LstSelectStatus = sel;
                }

            }
        }
    }
}
