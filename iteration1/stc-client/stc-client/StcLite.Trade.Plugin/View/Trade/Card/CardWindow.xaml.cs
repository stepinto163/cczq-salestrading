using BDB.Price.Plugin.Cache;
using BDB.Price.Plugin.ViewModel.Trade.Card;
using BDB.Trade.Plugin.Handler;
using BDB.Trade.Plugin.Handler.DataProcesser;
using BDB.Trade.Plugin.ViewModel.Data;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using STC.Common.Utils;
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

namespace BDB.Price.Plugin.View.Trade.Card
{
    /// <summary>
    /// CardWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CardWindow : Window
    {
        private const string OpenWindowKey = "CardWindow";

        private const string SetTop = "置顶";
        private const string CancelTop = "取消置顶";

        public string SecurityId { get; set; }
        private CardWindowModel cardWindowModel = null;
        private bool isRestore = true;

        public CardWindow(bool isrestore = true)
        {
            InitializeComponent();
            isRestore = isrestore;
            this.Icon = LocalEnv.ServerInfo.AppImage;

            this.Loaded += CardWindow_Loaded;
            this.Unloaded += CardWindow_Unloaded;
            this.Closed += CardWindow_Closed;

            this.PreviewKeyDown += CardWindow_PreviewKeyDown;
            
            cardWindowModel = new CardWindowModel();
            this.DataContext = cardWindowModel;
        }

        private void CardWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }


        private void ReadPosition()
        {
            string showInfo = IniFileHelper.ReadValue(AppUtils.GetCacheKey(), string.Format("{0}-{1}", OpenWindowKey, SecurityId));
            if (!string.IsNullOrEmpty(showInfo))
            {
                string[] arr = showInfo.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length > 4)
                {
                    double d = 0;
                    if (double.TryParse(arr[0], out d))
                    {
                        this.Height = d;
                    }
                    if (double.TryParse(arr[1], out d))
                    {
                        this.Width = d;
                    }
                    if (double.TryParse(arr[2], out d))
                    {
                        this.Left = d;
                    }
                    if (double.TryParse(arr[3], out d))
                    {
                        this.Top = d;
                    }
                    bool isTop = false;
                    if (bool.TryParse(arr[4], out isTop))
                    {
                        btnTop.Style = this.FindResource("TopActiveButtonStyle") as Style;
                        this.Topmost = true;
                        btnTop.ToolTip = CancelTop;
                    }
                    else
                    {
                        btnTop.Style = this.FindResource("TopDeActiveButtonStyle") as Style;
                        btnTop.ToolTip = SetTop;
                    }
                }
            }
        }

        private void WritePosition()
        {
            IniFileHelper.WriteValue(AppUtils.GetCacheKey(), string.Format("{0}-{1}", OpenWindowKey, SecurityId), string.Format("{0},{1},{2},{3},{4}", this.Height, this.Width, this.Left, this.Top, this.Topmost));
        }

        private void CardWindow_Closed(object sender, EventArgs e)
        {
            this.Closed -= CardWindow_Closed;
            CardWinCache.Instance.DeleteBySecurity(SecurityId);

            WritePosition();
        }

        private void CardWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= CardWindow_Unloaded;

            if (cardWindowModel != null)
            {
                cardWindowModel.Clear();
            }
        }

        private void CardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= CardWindow_Loaded;

            this.Title = this.SecurityId;

            if (isRestore)
                ReadPosition();

            cardWindowModel.Init(this, SecurityId);
        }

        public void Reset(string securityId)
        {
            string oldSecurityId = this.SecurityId;
            this.SecurityId = securityId;
            this.Title = securityId;
            cardWindowModel.Reset(oldSecurityId, securityId);
        }

        private void mainWindow_Activated(object sender, EventArgs e)
        {
            bd.BorderBrush = this.FindResource("Green-3") as Brush;
        }

        private void mainWindow_Deactivated(object sender, EventArgs e)
        {
            bd.BorderBrush = this.FindResource("Green-1") as Brush;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;

            if (this.Topmost)
            {
                btnTop.ToolTip = CancelTop;

                btnTop.Style = this.FindResource("TopActiveButtonStyle") as Style;
            }
            else
            {
                btnTop.ToolTip = SetTop;

                btnTop.Style = this.FindResource("TopDeActiveButtonStyle") as Style;
            }
        }
    }
}
