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
    /// BackLogListBox.xaml 的交互逻辑
    /// </summary>
    public partial class BackLogListBox : ListBox
    {
        private BackLogListBoxViewModel viewModel = new BackLogListBoxViewModel();

        public BackLogListBox()
        {
            InitializeComponent();
        }

        private void ListBox_Initialized(object sender, EventArgs e)
        {
            this.DataContext = this.viewModel;
            this.viewModel.Init();
        }
    }
}
