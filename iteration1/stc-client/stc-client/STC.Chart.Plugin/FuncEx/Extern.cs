using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace STC.Chart.Plugin.FuncEx
{
    public class Extern
    {
        /// <summary>
        /// 查找指定类型的子控件
        /// </summary>
        /// <typeparam name="childItem">子控件类型</typeparam>
        /// <param name="obj">父控件</param>
        /// <returns></returns>
        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 查找某种类型的子控件，并返回一个List集合
        /// </summary>
        /// <example>List<Button> listButtons = GetChildObjects<Button>(parentPanel, typeof(Button))</example>
        /// <typeparam name="T">子控件类型</typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="type">子控件的类</param>
        /// <returns></returns>
        public static List<T> GetChildObjects<T>(DependencyObject obj, Type type) where T : DependencyObject
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).GetType() == type))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, type));
            }
            return childList;
        }

        /// <summary>
        /// 通过名称查找子控件，并返回一个List集合
        /// </summary>
        /// <example>List<Button> listButtons = GetChildObjects<Button>(parentPanel, "button1")</example>
        /// <typeparam name="T">子控件类型</typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="name">子控件名称，默认为空</param>
        /// <returns></returns>
        public static List<T> GetChildObjects<T>(DependencyObject obj, string name = null) where T : FrameworkElement
        {
            DependencyObject child = null;
            List<T> childList = new List<T>();

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    childList.Add((T)child);
                }
                childList.AddRange(GetChildObjects<T>(child, name));
            }
            return childList;
        }

        /// <summary>
        /// 通过名称查找某类型的子控件
        /// </summary>
        /// <example>StackPanel sp = GetChildObject<StackPanel>(this.LayoutRoot, "spDemoPanel")</example>
        /// <typeparam name="T">子控件类型</typeparam>
        /// <param name="obj">父控件</param>
        /// <param name="name">子控件名称，默认为空</param>
        /// <returns></returns>
        public static T GetChildObject<T>(DependencyObject obj, string name = null) where T : FrameworkElement
        {
            DependencyObject child = null;
            T grandChild = null;

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(obj) - 1; i++)
            {
                child = VisualTreeHelper.GetChild(obj, i);

                if (child is T && (((T)child).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)child;
                }
                else
                {
                    grandChild = GetChildObject<T>(child, name);
                    if (grandChild != null)
                        return grandChild;
                }
            }
            return null;
        }

        /// <summary>
        /// 通过名称查找父控件
        /// </summary>
        /// <example>Grid layoutGrid = VTHelper.GetParentObject<Grid>(this.spDemoPanel, "LayoutRoot")</example>
        /// <typeparam name="T">父控件类型</typeparam>
        /// <param name="obj">子控件</param>
        /// <param name="name">父控件名称，默认为空</param>
        /// <returns></returns>
        public static T GetParentObject<T>(DependencyObject obj, string name = null) where T : FrameworkElement
        {
            if (obj == null) return null;
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            while (parent != null)
            {
                if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)parent;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }

    public static class Funex
    {
        public static string ToStringNull(this object obj)
        {
            if (obj == null) return string.Empty;
            return obj.ToString();
        }
        public static string EscapeLikeValue(this string obj)
        {
            if (obj == null) return string.Empty;
            return obj.ToString();
        }
    }
}
