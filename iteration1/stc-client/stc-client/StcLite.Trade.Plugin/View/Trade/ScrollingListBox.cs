using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BDB.Trade.Plugin.View.Trade
{
    public class ScrollingListBox : ListBox
    {
        private ScrollViewer currentScrollViewer;
        private string currentSecurityId;
        private double currentVerticalOffset;
        private double currentExtentHeight;
        private double currentViewportHeight;
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null) return;

            if (currentScrollViewer == null)
            {
                currentScrollViewer = GetDescendantByType(this, typeof(ScrollViewer)) as ScrollViewer;

                //if (currentScrollViewer != null)
                //{
                //    currentScrollViewer.ScrollChanged += CurrentScrollViewer_ScrollChanged;
                //}
            }

            //var newItemCount = e.NewItems.Count;
            //if (newItemCount > 0)
            //{
            //    if (currentScrollViewer != null && currentScrollViewer.VerticalOffset != 0)
            //    {
            //        this.ScrollIntoView(this.Items[0]);
            //    }
            //    else
            //    {
            //        this.UpdateLayout();
            //    }
            //}
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                currentSecurityId = (this.DataContext as ViewModel.Trade.MarketOrderViewModel).SearchText;

                if (currentScrollViewer != null && currentScrollViewer.VerticalOffset == 0 
                    && currentScrollViewer.ExtentHeight != currentExtentHeight
                    && currentScrollViewer.ViewportHeight != currentViewportHeight)
                {
                    currentScrollViewer.ScrollToHome();
                }

                if (currentScrollViewer != null)
                {
                    currentExtentHeight = currentScrollViewer.ExtentHeight;
                    currentViewportHeight = currentScrollViewer.ViewportHeight;
                    currentVerticalOffset = currentScrollViewer.VerticalOffset;
                }
            }


            base.OnItemsChanged(e);
        }

        private void CurrentScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

        }

        public Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
                return null;
            if (element.GetType() == type)
                return element;
            Visual foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).ApplyTemplate();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }
    }
}
