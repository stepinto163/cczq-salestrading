using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace STC.Broker.Plugin.Utils.wpf.Utils
{
    public static class VisualTreeExtensions
    {
        public static T GetVisualAncestor<T>(this DependencyObject d) where T : class
        {
            for (DependencyObject parent = VisualTreeHelper.GetParent(d); parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                T val = parent as T;
                if (val != null)
                {
                    return val;
                }
            }

            return null;
        }

        public static DependencyObject GetVisualAncestor(this DependencyObject d, Type type)
        {
            for (DependencyObject parent = VisualTreeHelper.GetParent(d); parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                if ((object)parent.GetType() == type)
                {
                    return parent;
                }
            }

            return null;
        }

        public static T GetVisualDescendent<T>(this DependencyObject d) where T : DependencyObject
        {
            return d.GetVisualDescendents<T>().FirstOrDefault();
        }

        public static IEnumerable<T> GetVisualDescendents<T>(this DependencyObject d) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(d);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(d, i);
                if (child is T)
                {
                    yield return (T)child;
                }

                foreach (T visualDescendent in child.GetVisualDescendents<T>())
                {
                    yield return visualDescendent;
                }
            }
        }
    }
}
