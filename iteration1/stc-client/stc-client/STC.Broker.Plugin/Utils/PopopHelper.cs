
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace STC.Broker.Plugin.Utils
{
    public class PopopHelper
    {
        public static DependencyObject GetPopupPositionTarget(DependencyObject obj)
        {
            return (DependencyObject)obj.GetValue(PopupPositionTargetProperty);
        }

        public static void SetPopupPositionTarget(DependencyObject obj, DependencyObject value)
        {
            obj.SetValue(PopupPositionTargetProperty, value);
        }

        public static readonly DependencyProperty PopupPositionTargetProperty =
            DependencyProperty.RegisterAttached("PopupPositionTarget", typeof(DependencyObject), typeof(PopopHelper), new PropertyMetadata(null, OnPopupPlacementTargetChanged));

        private static void OnPopupPlacementTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                if (e.NewValue != null)
                {
                    DependencyObject popupPopupPlacementTarget = e.NewValue as DependencyObject;
                    Popup pop = d as Popup;

                    var win = VisualTreeHelper.GetParent(pop);
                    if (win != null)
                    {
                        win = VisualTreeHelper.GetParent(win);

                        //RecordDetail rd = null;
                        //Grid grid = win as Grid;
                        //if (grid != null)
                        //{
                        //    rd = grid.Parent as RecordDetail;
                        //}


                        //if (rd != null)
                        //{
                        //    rd.LayoutUpdated += delegate
                        //    {
                        //        try
                        //        {
                        //            if (pop.IsOpen)
                        //            {
                        //                var mi = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        //                if (mi != null)
                        //                {
                        //                    mi.Invoke(pop, null);
                        //                }
                        //            }
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //        }
                        //    };
                        //}
                    }
                }
            }
            catch (Exception ex)
            { }
        }
    }
}
