using com.gateway.log;
using STC.Broker.Plugin.View.HomePage;
using STC.Broker.Plugin.ViewModel.Price;
using STC.Broker.Plugin.ViewModel.UI;
using STC.Common.Data;
using STC.Common.Model.Admin;
using STC.Common.Model.PT;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Price
{
    /// <summary>
    /// PricingPanel.xaml 的交互逻辑
    /// </summary>
    public partial class PricingPanel : UserControl
    {
        private PricingPanelViewModel _PricingPanelViewModel = new PricingPanelViewModel();

        public PricingPanel()
        {
            InitializeComponent();
            _PricingPanelViewModel.MainView = this;

            this.DataContext = _PricingPanelViewModel;

            DesignHandler.Instance.OnSettingChange += Instance_OnSettingChange;

            //订阅全局快捷键
            DesignHandler.Instance.OnHotKey += Instance_OnHotKey;
        }

        public void Init(HomeGroupInfo gi, HomePageView ghv)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                _PricingPanelViewModel.Init(this, gi, ghv);
            }));
        }

        #region 模式切换
        private bool _isCardModel = false;

        private int _currentSelectedIndex = -1;


        private void Instance_OnSettingChange(object obj)
        {
            Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)(() =>
            {
                LogHelper.Warn("Instance_OnSettingChange");
                if (obj is GrLayoutSettingModel)
                {
                    LogHelper.Warn("Instance_OnSettingChange LayoutSetting");
                    GrLayoutSettingModel layoutSettingPo = obj as GrLayoutSettingModel;

                    _isCardModel = layoutSettingPo.ShowModel == 2;

                    _currentSelectedIndex = -1;

                    if (layoutSettingPo.ShowModel == 2)
                    {
                        var window = Window.GetWindow(Application.Current.MainWindow);//获取当前主窗口
                        var intPtr = new WindowInteropHelper(window).Handle;//获取当前窗口的句柄
                        System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.FromHandle(intPtr);//获取当前屏幕
                        Rect rect = new Rect(screen.Bounds.X, screen.Bounds.Y, screen.Bounds.Width, screen.Bounds.Height);
                        PresentationSource source = PresentationSource.FromVisual(window);
                        Matrix m = source.CompositionTarget.TransformFromDevice;
                        double widthRatio = m.M11;
                        double heightRatio = m.M22;

                        //Application.Current.MainWindow.MaxWidth = screen.WorkingArea.Width * widthRatio;
                        Application.Current.MainWindow.MaxHeight = screen.WorkingArea.Height * heightRatio;

                        int width = layoutSettingPo.Col * 284 + (layoutSettingPo.Col + 2) * 4 + 9;
                        int height = layoutSettingPo.Row * 398 + 97;
                        IniFileHelper.WriteValue($"MainWindow_{ LocalEnv.UserInfo.UserName}", "GrMinHeight", height.ToString());
                        IniFileHelper.WriteValue($"MainWindow_{ LocalEnv.UserInfo.UserName}", "GrMinWidth", width.ToString());
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                        Application.Current.MainWindow.Width = width;
                        Application.Current.MainWindow.Height = height;
                    }
                    
                    //重新刷新界面
                    (this.DataContext as PricingPanelViewModel).ReSetSize();
                }
            }));
        }
        
        private void Instance_OnHotKey(EHotKeySetting obj)
        {
            switch (obj)
            {
                case EHotKeySetting.GrBrokerLeft:
                case EHotKeySetting.GrBrokerPageUp:
                    if ((this.DataContext as PricingPanelViewModel).PageSelectIndex == 0)
                    {
                        (this.DataContext as PricingPanelViewModel).PageSelectIndex = (this.DataContext as PricingPanelViewModel).PageButtonList.Count - 1;
                    }
                    else
                    {
                        (this.DataContext as PricingPanelViewModel).OnPrevPageCommand(null);
                    }
                    break;
                case EHotKeySetting.GrBrokerRight:
                case EHotKeySetting.GrBrokerPageDown:
                    if ((this.DataContext as PricingPanelViewModel).PageSelectIndex == (this.DataContext as PricingPanelViewModel).PageButtonList.Count - 1)
                    {
                        (this.DataContext as PricingPanelViewModel).PageSelectIndex = 0;
                    }
                    else
                    {
                        (this.DataContext as PricingPanelViewModel).OnNextPageCommand(null);
                    }
                    break;
            }
        }

      

        #endregion

        private void Tab_DgPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.GetType() == typeof(TabControl))
            {
                if (((TabControl)sender).DataContext.GetType() == typeof(PricingPanelViewModel))
                {
                    ((((TabControl)sender).DataContext as PricingPanelViewModel).MainView as PricingPanel).Focus();
                }
            }
        }

        private void Tab_DgPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                e.Handled = true; // 阻止左右切换
            }
        }
    }
}
