using STC.Common.Utils;
using STC.DownloadCenter.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
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

namespace STC.DownloadCenter.Plugin.View
{
    /// <summary>
    /// DownloadView.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadView : UserControl
    {
        private DownloadViewModel downloadViewModel = new DownloadViewModel();

        public DownloadView()
        {
            InitializeComponent();
            this.DataContext = downloadViewModel;
        }

        private void CheckBox_All_Click_Ignore(object sender, RoutedEventArgs e)
        {
            if (sender != null && sender is CheckBox)
            {
                CheckBox chk = sender as CheckBox;
                bool flag = chk.IsChecked == true ? true : false;
                if (downloadViewModel != null)
                {
                    downloadViewModel.CheckBoxAllValueChange_Ignore(flag);
                }
            }
        }

        private void Selected_Chk_Click_Ignore(object sender, RoutedEventArgs e)
        {
            if (downloadViewModel != null)
            {
                downloadViewModel.CheckBoxSingleClick_Ignore();
            }
        }

        private void Btn_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DownloadDataGrid.SelectedItem = ((Button)sender).DataContext;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            AddTaskView addTaskView = new AddTaskView();
            addTaskView.Show();
        }
    }
}
