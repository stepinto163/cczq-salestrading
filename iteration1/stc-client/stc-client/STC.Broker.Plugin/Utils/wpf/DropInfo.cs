
using STC.Broker.Plugin.Utils.wpf.Utils;
using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace STC.Broker.Plugin.Utils.wpf
{
    public class DropInfo
    {
        public object Data
        {
            get;
            private set;
        }

        public DragInfo DragInfo
        {
            get;
            private set;
        }

        public Type DropTargetAdorner
        {
            get;
            set;
        }

        public DragDropEffects Effects
        {
            get;
            set;
        }

        public int InsertIndex
        {
            get;
            private set;
        }

        public IEnumerable TargetCollection
        {
            get;
            private set;
        }

        public object TargetItem
        {
            get;
            private set;
        }

        public UIElement VisualTarget
        {
            get;
            private set;
        }

        public UIElement VisualTargetItem
        {
            get;
            private set;
        }

        public Orientation VisualTargetOrientation
        {
            get;
            private set;
        }

        public DropInfo(object sender, DragEventArgs e, DragInfo dragInfo, string dataFormat)
        {
            Data = (e.Data.GetDataPresent(dataFormat) ? e.Data.GetData(dataFormat) : e.Data);
            DragInfo = dragInfo;
            VisualTarget = (sender as UIElement);
            if (!(sender is ItemsControl))
            {
                return;
            }

            ItemsControl itemsControl = (ItemsControl)sender;
            UIElement itemContainerAt = itemsControl.GetItemContainerAt(e.GetPosition(itemsControl));
            VisualTargetOrientation = itemsControl.GetItemsPanelOrientation();
            if (itemContainerAt != null)
            {
                ItemsControl itemsControl2 = ItemsControl.ItemsControlFromItemContainer(itemContainerAt);
                InsertIndex = itemsControl2.ItemContainerGenerator.IndexFromContainer(itemContainerAt);
                TargetCollection = (itemsControl2.ItemsSource ?? itemsControl2.Items);
                TargetItem = itemsControl2.ItemContainerGenerator.ItemFromContainer(itemContainerAt);
                VisualTargetItem = itemContainerAt;
                if (VisualTargetOrientation == Orientation.Vertical)
                {
                    if (e.GetPosition(itemContainerAt).Y > itemContainerAt.RenderSize.Height / 2.0)
                    {
                        InsertIndex++;
                    }
                }
                else if (e.GetPosition(itemContainerAt).X > itemContainerAt.RenderSize.Width / 2.0)
                {
                    InsertIndex++;
                }
            }
            else
            {
                TargetCollection = (itemsControl.ItemsSource ?? itemsControl.Items);
                InsertIndex = itemsControl.Items.Count;
            }
        }
    }
}
