using STC.Common.View.Component;
using STC.WorkBench.Plugin.ViewModel.Data;
using STC.WorkBench.Plugin.ViewModel.WorkTable;
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

namespace STC.WorkBench.Plugin.View.WorkTable
{
    /// <summary>
    /// IntentionLogsView.xaml 的交互逻辑
    /// </summary>
    public partial class IntentionLogsView : WindowBase
    {
        private IntentionLogsViewModel _intentionLogsViewModel = new IntentionLogsViewModel();

        public IntentionLogsView(IntentionQuoteItem currentItem)
        {
            InitializeComponent();
            this.Title = $"变更日志 {currentItem.RemainingTime} {currentItem.SecurityId} {currentItem.Symbol}";
            _intentionLogsViewModel.CurrentItem = currentItem;
            this.DataContext = _intentionLogsViewModel;
            _intentionLogsViewModel.Query();
        }
    }
}
