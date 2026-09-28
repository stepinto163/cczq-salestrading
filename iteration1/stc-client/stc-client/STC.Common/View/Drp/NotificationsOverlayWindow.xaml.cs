using STC.Common.View.Drp.Component;
using System.Linq;
using System.Windows;

namespace STC.Common.View.Drp
{
    /// <summary>
    /// Interaction logic for ToastWindow.xaml
    /// </summary>
    public partial class NotificationsOverlayWindow : Window
    {
        public string AreaName
        {
            get { return (string)GetValue(AreaNameProperty); }
            set { SetValue(AreaNameProperty, value); }
        }

        public static readonly DependencyProperty AreaNameProperty =
            DependencyProperty.Register("AreaName", typeof(string), typeof(NotificationsOverlayWindow), new PropertyMetadata(""));

        public NotificationsOverlayWindow(string name = "")
        {
            InitializeComponent();
            CtrlArea.Add();
            CtrlArea.AreaName = name;
        }
    }
}
