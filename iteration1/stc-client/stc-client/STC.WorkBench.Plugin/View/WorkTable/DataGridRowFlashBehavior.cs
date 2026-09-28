using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.WorkBench.Plugin.View.WorkTable
{
    public static class DataGridRowFlashBehavior
    {
        public static readonly DependencyProperty IsFlashingProperty =
        DependencyProperty.RegisterAttached(
            "IsFlashing",
            typeof(bool),
            typeof(DataGridRowFlashBehavior),
            new PropertyMetadata(false, OnIsFlashingChanged));

        public static void SetIsFlashing(DependencyObject element, bool value)
        {
            element.SetValue(IsFlashingProperty, value);
        }

        public static bool GetIsFlashing(DependencyObject element)
        {
            return (bool)element.GetValue(IsFlashingProperty);
        }

        private static void OnIsFlashingChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridRow && (bool)e.NewValue)
            {
                DataGridRow row = d as DataGridRow;
                var originalColor = row.Background ?? Brushes.Transparent;
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(300)
                };

                timer.Tick += (s, args) =>
                {
                    row.Background = originalColor;
                    if (row.DataContext is STC.WorkBench.Plugin.ViewModel.Data.IntentionQuoteItem)
                    {
                        STC.WorkBench.Plugin.ViewModel.Data.IntentionQuoteItem item = row.DataContext as STC.WorkBench.Plugin.ViewModel.Data.IntentionQuoteItem;
                        item.IsFlashing = false;
                    }
                    timer.Stop();
                };

                row.Background = new SolidColorBrush(Colors.DarkGreen);// 要变的颜色
                timer.Start();
            }
        }
    }
}
