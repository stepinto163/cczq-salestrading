using System.Windows.Controls;
using System.Windows.Media;

namespace STC.Frame.CustomUI
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
