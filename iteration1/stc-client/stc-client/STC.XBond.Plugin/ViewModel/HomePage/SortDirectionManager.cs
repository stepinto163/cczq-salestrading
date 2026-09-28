using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.XBond.Plugin.ViewModel.HomePage
{
    /// <summary>
    /// 主页的排序逻辑
    /// </summary>
    public enum MyListSortDirection
    {
        // 无排序。
        None = 0,
        // 按升序排序。
        Ascending = 1,
        // 按降序排序。
        Descending = 2
    }

    public class SortDirectionManager : DependencyObject
    {
        public static MyListSortDirection GetSortDirection(DependencyObject obj)
        {
            return (MyListSortDirection)obj.GetValue(SortDirectionProperty);
        }

        public static void SetSortDirection(DependencyObject obj, MyListSortDirection value)
        {
            obj.SetValue(SortDirectionProperty, value);
        }

        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.RegisterAttached("SortDirection", typeof(MyListSortDirection), typeof(SortDirectionManager));

        //private static void OnAngleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        //{
        //    var element = obj as UIElement;
        //    if (element != null)
        //    {
        //        element.RenderTransformOrigin = new Point(0.5, 0.5);
        //        element.RenderTransform = new RotateTransform((double)e.NewValue);
        //    }
        //}
    }
}
