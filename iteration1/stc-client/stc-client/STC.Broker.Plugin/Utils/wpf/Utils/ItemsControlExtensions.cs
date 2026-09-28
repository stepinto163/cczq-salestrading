using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace STC.Broker.Plugin.Utils.wpf.Utils
{
    public static class ItemsControlExtensions
    {
        public static bool CanSelectMultipleItems(this ItemsControl itemsControl)
        {
            if (itemsControl is MultiSelector)
            {
                return (bool)itemsControl.GetType().GetProperty("CanSelectMultipleItems", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(itemsControl, null);
            }

            if (itemsControl is ListBox)
            {
                return ((ListBox)itemsControl).SelectionMode != SelectionMode.Single;
            }

            return false;
        }

        public static UIElement GetItemContainer(this ItemsControl itemsControl, UIElement child)
        {
            Type itemContainerType = itemsControl.GetItemContainerType();
            if ((object)itemContainerType != null)
            {
                return (UIElement)child.GetVisualAncestor(itemContainerType);
            }

            return null;
        }

        public static UIElement GetItemContainerAt(this ItemsControl itemsControl, Point position)
        {
            IInputElement inputElement = itemsControl.InputHitTest(position);
            UIElement uIElement = inputElement as UIElement;
            if (uIElement != null)
            {
                return itemsControl.GetItemContainer(uIElement);
            }

            return null;
        }

        public static Type GetItemContainerType(this ItemsControl itemsControl)
        {
            if (itemsControl.Items.Count > 0)
            {
                IEnumerable<ItemsPresenter> visualDescendents = itemsControl.GetVisualDescendents<ItemsPresenter>();
                foreach (ItemsPresenter item in visualDescendents)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(item, 0);
                    DependencyObject child2 = VisualTreeHelper.GetChild(child, 0);
                    if (child2 != null && itemsControl.ItemContainerGenerator.IndexFromContainer(child2) != -1)
                    {
                        return child2.GetType();
                    }
                }
            }

            return null;
        }

        public static Orientation GetItemsPanelOrientation(this ItemsControl itemsControl)
        {
            ItemsPresenter visualDescendent = itemsControl.GetVisualDescendent<ItemsPresenter>();
            DependencyObject child = VisualTreeHelper.GetChild(visualDescendent, 0);
            PropertyInfo property = child.GetType().GetProperty("Orientation", typeof(Orientation));
            if ((object)property != null)
            {
                return (Orientation)property.GetValue(child, null);
            }

            return Orientation.Vertical;
        }

        public static IEnumerable GetSelectedItems(this ItemsControl itemsControl)
        {
            if (itemsControl is MultiSelector)
            {
                return ((MultiSelector)itemsControl).SelectedItems;
            }

            if (itemsControl is ListBox)
            {
                ListBox listBox = (ListBox)itemsControl;
                if (listBox.SelectionMode == SelectionMode.Single)
                {
                    return Enumerable.Repeat(listBox.SelectedItem, 1);
                }

                return listBox.SelectedItems;
            }

            if (itemsControl is TreeView)
            {
                return Enumerable.Repeat(((TreeView)itemsControl).SelectedItem, 1);
            }

            if (itemsControl is Selector)
            {
                return Enumerable.Repeat(((Selector)itemsControl).SelectedItem, 1);
            }

            return Enumerable.Empty<object>();
        }
    }
}
