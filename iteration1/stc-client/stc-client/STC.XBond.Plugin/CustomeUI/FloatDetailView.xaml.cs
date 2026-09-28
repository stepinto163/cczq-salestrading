using com.gateway.log;
using STC.XBond.Plugin.CustomUI;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.XBond.Plugin.CustomeUI
{
    /// <summary>
    /// FloatDetailView.xaml 的交互逻辑
    /// </summary>
    public partial class FloatDetailView : UserControl
    {
        public FloatDetailView()
        {
            InitializeComponent();

            //this.Loaded += FloatDetailView_Loaded;
        }

        private string currentBgColor = "";
        private Color _redBig = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E");

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                currentBgColor = border.Background?.ToString().Replace("FF", "");
                if (currentBgColor == "#102A3B")
                {
                    border.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#0D3F60"));
                }
                else if (currentBgColor == "#392512")
                {
                    border.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#5F340E"));
                }
                else if (currentBgColor == "#172422")
                {
                    border.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#193D37"));
                }
                else if (currentBgColor == "#3D1A1B")
                {
                    border.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#6E2022"));
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug("Normal Error Msg...");
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                border.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(currentBgColor));
            } catch(Exception ex)
            { }
        }
    }
}
