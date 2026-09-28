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

namespace STC.XBond.Plugin.View.Component
{
    /// <summary>
    /// Switch.xaml 的交互逻辑
    /// </summary>
    public partial class Switch : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty =
     DependencyProperty.Register("IsChecked", typeof(bool), typeof(Switch), new PropertyMetadata(default(bool), OnIsCheckedChanged));

        public event RoutedEventHandler Checked;
        public event RoutedEventHandler UnChecked;

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public Switch()
        {
            InitializeComponent();
        }

        private static void OnIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as Switch).OnIsCheckedChanged(args);
        }

        private void OnIsCheckedChanged(DependencyPropertyChangedEventArgs args)
        {
            this.eOpen.Visibility = IsChecked ? Visibility.Visible : Visibility.Collapsed;
            this.eClose.Visibility = IsChecked ? Visibility.Collapsed : Visibility.Visible;
            SolidColorBrush closeBrushes = (SolidColorBrush)this.FindResource("Close.Background.ColorBrush");
            SolidColorBrush openBrushes = (SolidColorBrush)this.FindResource("Open.Background.ColorBrush");
            this.checkBoxBorder.Background = IsChecked ? openBrushes : closeBrushes;
            this.Background = Brushes.Transparent;

            if (IsChecked && Checked != null)
            {
                Checked(this, new RoutedEventArgs());
            }

            if (!IsChecked && UnChecked != null)
            {
                UnChecked(this, new RoutedEventArgs());
            }
        }


        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            args.Handled = true;
            //IsChecked ^= true;
            base.OnMouseLeftButtonUp(args);
        }
    }
}
