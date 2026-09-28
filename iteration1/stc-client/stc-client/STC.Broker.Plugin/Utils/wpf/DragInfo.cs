using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using  STC.Broker.Plugin.Utils.wpf.Utils;

namespace  STC.Broker.Plugin.Utils.wpf
{
    public class DragInfo
    {
        public object Data
        {
            get;
            set;
        }

        public Point DragStartPosition
        {
            get;
            private set;
        }

        public DragDropEffects Effects
        {
            get;
            set;
        }

        public MouseButton MouseButton
        {
            get;
            private set;
        }

        public IEnumerable SourceCollection
        {
            get;
            private set;
        }

        public object SourceItem
        {
            get;
            private set;
        }

        public IEnumerable SourceItems
        {
            get;
            private set;
        }

        public UIElement VisualSource
        {
            get;
            private set;
        }

        public UIElement VisualSourceItem
        {
            get;
            private set;
        }

        public DragInfo(object sender, MouseButtonEventArgs e)
        {
            DragStartPosition = e.GetPosition(null);
            Effects = DragDropEffects.None;
            MouseButton = e.ChangedButton;
            VisualSource = (sender as UIElement);
            if (sender is ItemsControl && e.OriginalSource is UIElement)
            {
                UIElement element = (UIElement)e.OriginalSource;
                ItemsControl itemsControl = (ItemsControl)sender;
                UIElement itemContainer = itemsControl.GetItemContainer(element);
                if (itemContainer != null)
                {
                    ItemsControl itemsControl2 = ItemsControl.ItemsControlFromItemContainer(itemContainer);
                    SourceCollection = (itemsControl2.ItemsSource ?? itemsControl2.Items);
                    SourceItem = itemsControl2.ItemContainerGenerator.ItemFromContainer(itemContainer);
                    SourceItems = itemsControl.GetSelectedItems();
                    if (SourceItems.Cast<object>().Count() <= 1)
                    {
                        SourceItems = Enumerable.Repeat(SourceItem, 1);
                    }

                    VisualSourceItem = itemContainer;
                }
                else
                {
                    SourceCollection = (itemsControl.ItemsSource ?? itemsControl.Items);
                }
            }

            if (SourceItems == null)
            {
                SourceItems = Enumerable.Empty<object>();
            }
        }
    }
}
