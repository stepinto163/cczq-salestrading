using STC.XBond.Plugin.View.Component;
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

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// TestChannelWin.xaml 的交互逻辑
    /// </summary>
    public partial class TestChannelWin : Window
    {
        public TestChannelWin()
        {
            InitializeComponent();
            this.Icon = Common.Utils.LocalEnv.ServerInfo.AppImage;
        }

        public void Init(string security, Brush bgColor)
        {
            //ChannelCheckUI channelCheckUI = new ChannelCheckUI(security, bgColor, string.Empty);
            //channelCheckUI.CheckValueChanged += ChannelCheckUI_CheckValueChanged;

            //border.Child = channelCheckUI;
        }

        private void ChannelCheckUI_CheckValueChanged(object sender, RoutedEventArgs e)
        {
            ChannelCheckUI channelCheckUI = sender as ChannelCheckUI;
            ChannelCheckParam checkValue = channelCheckUI.CheckValue;

            Console.WriteLine(checkValue.ToString());
        }
    }
}
