using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.Common.View.UI
{
    /// <summary>
    /// SysWarnGUI.xaml 的交互逻辑
    /// </summary>
    public partial class CloneMessageBoxGUI : Window
    {

        public event Action<int> CloneCountEvent;
        int CloneCount = 1;
        public CloneMessageBoxGUI()
        {
            InitializeComponent();
            //this.Icon = LocalEnv.ServerInfo.AppImage;
            this.Loaded += MessageBoxGUI4_Loaded;
        }

        private void MessageBoxGUI4_Loaded(object sender, RoutedEventArgs e)
        {
            tbCopyCount.Focus();
            tbCopyCount.SelectionStart = tbCopyCount.Text.Length;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();            
        }

        public void setWarnMsg(string warnMsg)
        {
            tbMsg.Text = warnMsg;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbCopyCount.Text.ToString()))
            {
                this.Close();
                CloneCountEvent?.Invoke(CloneCount);
            } 
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbCopyCount_TextChanged(object sender, TextChangedEventArgs e)
        {      
            int count ;
            if (!string.IsNullOrEmpty(tbCopyCount.Text.ToString()))
            {
                if (int.TryParse(tbCopyCount.Text.ToString(), out count))
                {
                    if ( count > LocalEnv.ConfInfo.CloneOrderCount)
                    {
                        count = LocalEnv.ConfInfo.CloneOrderCount;
                    }
                    if (count <= 0)
                    {
                        count = 1;
                    }
                    CloneCount = count;
                    tbCopyCount.Text = count.ToString();
                    tbCopyCount.SelectionStart = tbCopyCount.Text.Length;
                }
            }
        }

        private void tbCopyCount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");

            e.Handled = re.IsMatch(e.Text);
        }

        private void tbCopyCount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnConfirm_Click(null,null);
            }
        }
    }
}
