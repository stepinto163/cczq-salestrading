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
using System.Windows.Shapes;

namespace STC.XBond.Plugin.Setting.View
{
    /// <summary>
    /// NewRoleUI.xaml 的交互逻辑
    /// </summary>
    public partial class PassworVerificationU : Window
    {
        public string PassWord;
        public PassworVerificationU()
        {
            InitializeComponent();
        }

        private void tbWeight_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void btOK_Click(object sender, RoutedEventArgs e)
        {
            PassWord = tbPassword.Text;
            var win = Window.GetWindow(this);
            if (win != null)
            {
                this.DialogResult = true;
                win.Close();
            }
        }
    }
}
