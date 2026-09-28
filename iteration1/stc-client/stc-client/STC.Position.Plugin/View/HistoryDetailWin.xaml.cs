using STC.Common.View.Component;
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
    /// HistoryDetailWin.xaml 的交互逻辑
    /// </summary>
    public partial class HistoryDetailWin : WindowBase
    {
        public HistoryDetailWin()
        {
            InitializeComponent();
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void WindowBase_KeyDown(object sender, KeyEventArgs e)
        {
            //判断用户的按键是否为Esc
            if (e.KeyStates == Keyboard.GetKeyStates(Key.Escape))
            {
                this.Close();
            }
        }
        public void SetMainControl(object element)
        {
            this.MainFrame.Content = element;
        }
    }
}
