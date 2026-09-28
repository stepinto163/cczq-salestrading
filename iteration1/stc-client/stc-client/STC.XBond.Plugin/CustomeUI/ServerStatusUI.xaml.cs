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

namespace STC.XBond.Plugin.CustomUI
{
    /// <summary>
    /// ServerStatusUI.xaml 的交互逻辑
    /// </summary>
    public partial class ServerStatusUI : UserControl
    {
        public ServerStatusUI()
        {
            InitializeComponent();
        }

        public void SetShowBg(Brush bgColor, Brush forbgColor)
        {
            bdStatus.Background = bgColor;
            tbStatus.Foreground = forbgColor;
        }

        public bool CheckStatus(Brush brush)
        {
            return bdStatus.Background.Equals(brush);
        }

        public void SetShowTxt(string txt)
        {
            tbStatus.Text = txt;
        }
    }
}
