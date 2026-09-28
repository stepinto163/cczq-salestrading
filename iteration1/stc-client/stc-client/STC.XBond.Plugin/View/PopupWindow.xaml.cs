using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.Command;
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
    /// PopupWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PopupWindow : WindowBase
    {
        public PopupWindow()
        {
            InitializeComponent();

            this.Loaded += PopupWindow_Loaded;
        }

        private void PopupWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= PopupWindow_Loaded;

            ResetPosition();
        }

        private void ResetPosition()
        {
            string key = this.Tag as string;

            LoadMemoryWin(key);
        }

        #region 加载市场成交、我的订单、我的成交记忆

        private readonly string WinHeight = "Height";
        private readonly string WinWidth = "Width";
        private readonly string PositionX = "PositionX";
        private readonly string PositionY = "PositionY";

        private void LoadMemoryWin(string key)
        {
            string memoryWinHeight = IniFileHelper.ReadValue($"{key}_{LocalEnv.UserInfo.UserName}", WinHeight);
            string memoryWinWidth = IniFileHelper.ReadValue($"{key}_{LocalEnv.UserInfo.UserName}", WinWidth);
            string memoryWinX = IniFileHelper.ReadValue($"{key}_{LocalEnv.UserInfo.UserName}", PositionX);
            string memoryrWinY = IniFileHelper.ReadValue($"{key}_{LocalEnv.UserInfo.UserName}", PositionY);

            if (!string.IsNullOrEmpty(memoryWinHeight) && !string.IsNullOrEmpty(memoryWinWidth) &&
                !string.IsNullOrEmpty(memoryWinX) && !string.IsNullOrEmpty(memoryrWinY))
            {
                double dY = 0;
                double d = 0;
                if (double.TryParse(memoryWinHeight, out d))
                    this.Height = d;
                else
                    this.Height = (PopupTypeEnum.MyOrders.ToString() == key || PopupTypeEnum.MyExecutions.ToString() == key) ? 500 : 300;


                if (double.TryParse(memoryWinWidth, out d))
                    this.Width = d;
                else
                    this.Width = (PopupTypeEnum.MyOrders.ToString() == key || PopupTypeEnum.MyExecutions.ToString() == key) ? 1340 : 290;

                if (double.TryParse(memoryWinX, out d) && double.TryParse(memoryrWinY, out dY))
                {
                    //bool isContain = false;
                    //foreach (System.Windows.Forms.Screen sc in System.Windows.Forms.Screen.AllScreens)
                    //{
                    //    if (sc.WorkingArea.Contains(new System.Drawing.Point((int)d, (int)dY)))
                    //    {
                    //        isContain = true;
                    //        break;
                    //    }
                    //}

                    //if (isContain)
                    {
                        this.Top = dY;
                        this.Left = d;
                    }
                }
            }

        }


        #endregion
    }
}
