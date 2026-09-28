
using STC.Broker.Plugin.Utils.wpf.Interface;
using STC.Broker.Plugin.Utils.wpf.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.Broker.Plugin.Utils.wpf
{
    public static class DragDrop
    {
        public static readonly DependencyProperty DragAdornerTemplateProperty = DependencyProperty.RegisterAttached("DragAdornerTemplate", typeof(DataTemplate), typeof(DragDrop));

        public static readonly DependencyProperty DragHandlerProperty = DependencyProperty.RegisterAttached("DragHandler", typeof(IDragSource), typeof(DragDrop));

        public static readonly DependencyProperty DropHandlerProperty = DependencyProperty.RegisterAttached("DropHandler", typeof(IDropTarget), typeof(DragDrop));

        public static readonly DependencyProperty IsDragSourceProperty = DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDrop), new UIPropertyMetadata(false, IsDragSourceChanged));

        public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDrop), new UIPropertyMetadata(false, IsDropTargetChanged));

        private static IDragSource m_DefaultDragHandler;

        private static IDropTarget m_DefaultDropHandler;

        private static DragAdorner m_DragAdorner;

        private static DragInfo m_DragInfo;

        private static DropTargetAdorner m_DropTargetAdorner;

        private static DataFormat m_Format = DataFormats.GetDataFormat("GongSolutions.Wpf.DragDrop");

        public static IDragSource DefaultDragHandler
        {
            get
            {
                if (m_DefaultDragHandler == null)
                {
                    m_DefaultDragHandler = new DefaultDragHandler();
                }

                return m_DefaultDragHandler;
            }
            set
            {
                m_DefaultDragHandler = value;
            }
        }

        public static IDropTarget DefaultDropHandler
        {
            get
            {
                if (m_DefaultDropHandler == null)
                {
                    m_DefaultDropHandler = new DefaultDropHandler();
                }

                return m_DefaultDropHandler;
            }
            set
            {
                m_DefaultDropHandler = value;
            }
        }

        private static DragAdorner DragAdorner
        {
            get
            {
                return m_DragAdorner;
            }
            set
            {
                if (m_DragAdorner != null)
                {
                    m_DragAdorner.Detatch();
                }

                m_DragAdorner = value;
            }
        }

        private static DropTargetAdorner DropTargetAdorner
        {
            get
            {
                return m_DropTargetAdorner;
            }
            set
            {
                if (m_DropTargetAdorner != null)
                {
                    m_DropTargetAdorner.Detatch();
                }

                m_DropTargetAdorner = value;
            }
        }

        public static DataTemplate GetDragAdornerTemplate(UIElement target)
        {
            return (DataTemplate)target.GetValue(DragAdornerTemplateProperty);
        }

        public static void SetDragAdornerTemplate(UIElement target, DataTemplate value)
        {
            target.SetValue(DragAdornerTemplateProperty, value);
        }

        public static bool GetIsDragSource(UIElement target)
        {
            return (bool)target.GetValue(IsDragSourceProperty);
        }

        public static void SetIsDragSource(UIElement target, bool value)
        {
            target.SetValue(IsDragSourceProperty, value);
        }

        public static bool GetIsDropTarget(UIElement target)
        {
            return (bool)target.GetValue(IsDropTargetProperty);
        }

        public static void SetIsDropTarget(UIElement target, bool value)
        {
            target.SetValue(IsDropTargetProperty, value);
        }

        public static IDragSource GetDragHandler(UIElement target)
        {
            return (IDragSource)target.GetValue(DragHandlerProperty);
        }

        public static void SetDragHandler(UIElement target, IDragSource value)
        {
            target.SetValue(DragHandlerProperty, value);
        }

        public static IDropTarget GetDropHandler(UIElement target)
        {
            return (IDropTarget)target.GetValue(DropHandlerProperty);
        }

        public static void SetDropHandler(UIElement target, IDropTarget value)
        {
            target.SetValue(DropHandlerProperty, value);
        }

        private static HashSet<string> registerNames = new HashSet<string>();

        private static bool _canDrop = true;
        public static bool CanDrop
        {
            get
            {
                return _canDrop;
            }
            set
            {
                _canDrop = value;
            }
        }

        private static void IsDragSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uIElement = (UIElement)d;
            FrameworkElement element;
            if ((bool)e.NewValue && d is FrameworkElement)
            {
                element = (FrameworkElement)d;
                registerNames.Add(element.Name);
                uIElement.PreviewMouseLeftButtonDown += DragSource_PreviewMouseLeftButtonDown;
                uIElement.PreviewMouseLeftButtonUp += DragSource_PreviewMouseLeftButtonUp;
                uIElement.PreviewMouseMove += DragSource_PreviewMouseMove;
            }
            else
            {
                uIElement.PreviewMouseLeftButtonDown -= DragSource_PreviewMouseLeftButtonDown;
                uIElement.PreviewMouseLeftButtonUp -= DragSource_PreviewMouseLeftButtonUp;
                uIElement.PreviewMouseMove -= DragSource_PreviewMouseMove;
            }
        }

        private static void IsDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UIElement uIElement = (UIElement)d;
            if ((bool)e.NewValue)
            {
                uIElement.AllowDrop = true;
                uIElement.PreviewDragEnter += DropTarget_PreviewDragEnter;
                uIElement.PreviewDragLeave += DropTarget_PreviewDragLeave;
                uIElement.PreviewDragOver += DropTarget_PreviewDragOver;
                uIElement.PreviewDrop += DropTarget_PreviewDrop;
            }
            else
            {
                uIElement.AllowDrop = false;
                uIElement.PreviewDragEnter -= DropTarget_PreviewDragEnter;
                uIElement.PreviewDragLeave -= DropTarget_PreviewDragLeave;
                uIElement.PreviewDragOver -= DropTarget_PreviewDragOver;
                uIElement.PreviewDrop -= DropTarget_PreviewDrop;
            }
        }

        private static void CreateDragAdorner()
        {
            DataTemplate dragAdornerTemplate = GetDragAdornerTemplate(m_DragInfo.VisualSource);
            if (dragAdornerTemplate == null)
            {
                return;
            }

            UIElement adornedElement = (UIElement)Application.Current.MainWindow.Content;
            UIElement uIElement = null;
            if (m_DragInfo.Data is IEnumerable && !(m_DragInfo.Data is string))
            {
                if (((IEnumerable)m_DragInfo.Data).Cast<object>().Count() <= 10)
                {
                    ItemsControl itemsControl = new ItemsControl();
                    itemsControl.ItemsSource = (IEnumerable)m_DragInfo.Data;
                    itemsControl.ItemTemplate = dragAdornerTemplate;
                    Border border = new Border();
                    border.Child = itemsControl;
                    uIElement = border;
                }
            }
            else
            {
                ContentPresenter contentPresenter = new ContentPresenter();
                contentPresenter.Content = m_DragInfo.Data;
                contentPresenter.ContentTemplate = dragAdornerTemplate;
                uIElement = contentPresenter;
            }

            if (uIElement != null)
            {
                uIElement.Opacity = 0.5;
                DragAdorner = new DragAdorner(adornedElement, uIElement);
            }
        }

        private static bool HitTestScrollBar(object sender, MouseButtonEventArgs e)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest((Visual)sender, e.GetPosition((IInputElement)sender));
            if (hitTestResult != null)
            {
                return hitTestResult.VisualHit.GetVisualAncestor<ScrollBar>() != null;
            }
            return false;
        }


        private static void Scroll(DependencyObject o, DragEventArgs e)
        {
            ScrollViewer visualDescendent = o.GetVisualDescendent<ScrollViewer>();
            if (visualDescendent != null)
            {
                Point position = e.GetPosition(visualDescendent);
                double num = Math.Min(visualDescendent.FontSize * 2.0, visualDescendent.ActualHeight / 2.0);
                if (position.X >= visualDescendent.ActualWidth - num && visualDescendent.HorizontalOffset < visualDescendent.ExtentWidth - visualDescendent.ViewportWidth)
                {
                    visualDescendent.LineRight();
                }
                else if (position.X < num && visualDescendent.HorizontalOffset > 0.0)
                {
                    visualDescendent.LineLeft();
                }
                else if (position.Y >= visualDescendent.ActualHeight - num && visualDescendent.VerticalOffset < visualDescendent.ExtentHeight - visualDescendent.ViewportHeight)
                {
                    visualDescendent.LineDown();
                }
                else if (position.Y < num && visualDescendent.VerticalOffset > 0.0)
                {
                    visualDescendent.LineUp();
                }
            }
        }

        private static void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 || !CanDrop)
            {
                return;
            }
            if (HitTestScrollBar(sender, e))
            {
                m_DragInfo = null;
                return;
            }

            m_DragInfo = new DragInfo(sender, e);
            ItemsControl itemsControl = sender as ItemsControl;
            if (m_DragInfo.VisualSourceItem != null && itemsControl != null && itemsControl.CanSelectMultipleItems())
            {
                IEnumerable selectedItems = itemsControl.GetSelectedItems();
                if (selectedItems.Cast<object>().Contains(m_DragInfo.SourceItem))
                {
                    e.Handled = true;
                }
            }
            else
            {
                //m_DragInfo = null;
            }
        }

        private static void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (m_DragInfo != null)
            {
                m_DragInfo = null;
            }
        }

        private static void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (m_DragInfo == null || !CanDrop)
            {
                return;
            }
            try
            {
                Point dragStartPosition = m_DragInfo.DragStartPosition;
                Point position = e.GetPosition(null);
                if (Math.Abs(position.X - dragStartPosition.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(position.Y - dragStartPosition.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    IDragSource dragHandler = GetDragHandler(m_DragInfo.VisualSource);
                    if (dragHandler != null)
                    {
                        dragHandler.StartDrag(m_DragInfo);
                    }
                    else
                    {
                        DefaultDragHandler.StartDrag(m_DragInfo);
                    }

                    if (m_DragInfo.Effects != 0 && m_DragInfo.Data != null)
                    {
                        DataObject data = new DataObject(m_Format.Name, m_DragInfo.Data);
                        System.Windows.DragDrop.DoDragDrop(m_DragInfo.VisualSource, data, m_DragInfo.Effects);
                        m_DragInfo = null;
                    }
                }
            }
            catch (Exception)
            {
            }


        }

        private static void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
        {
            //DropTarget_PreviewDragOver(sender, e);
        }

        private static void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
        {
            DragAdorner = null;
            DropTargetAdorner = null;
        }

        private static void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (CanDrop)
            {
                DropInfo dropInfo = new DropInfo(sender, e, m_DragInfo, m_Format.Name);
                IDropTarget dropHandler = GetDropHandler((UIElement)sender);
                if (dropHandler != null)
                {
                    dropHandler.DragOver(dropInfo);
                }
                else
                {
                    DefaultDropHandler.DragOver(dropInfo);
                }

                if (dropInfo.Effects != 0)
                {
                    if (DragAdorner == null && m_DragInfo != null)
                    {
                        CreateDragAdorner();
                    }

                    if (DragAdorner != null)
                    {
                        DragAdorner.MousePosition = e.GetPosition(DragAdorner.AdornedElement);
                        DragAdorner.InvalidateVisual();
                    }
                }
                else
                {
                    DragAdorner = null;
                }

                if (sender is ItemsControl)
                {
                    UIElement visualDescendent = ((ItemsControl)sender).GetVisualDescendent<ItemsPresenter>();
                    if ((object)dropInfo.DropTargetAdorner == null)
                    {
                        DropTargetAdorner = null;
                    }
                    else if (!dropInfo.DropTargetAdorner.IsInstanceOfType(DropTargetAdorner))
                    {
                        DropTargetAdorner = DropTargetAdorner.Create(dropInfo.DropTargetAdorner, visualDescendent);
                    }

                    if (DropTargetAdorner != null)
                    {
                        DropTargetAdorner.DropInfo = dropInfo;
                        DropTargetAdorner.InvalidateVisual();
                    }
                }

                e.Effects = dropInfo.Effects;
                e.Handled = true;
                Scroll((DependencyObject)sender, e);
            }



        }

        private static void DropTarget_PreviewDrop(object sender, DragEventArgs e)
        {
            if (CanDrop)
            {
                DropInfo dropInfo = new DropInfo(sender, e, m_DragInfo, m_Format.Name);
                IDropTarget dropHandler = GetDropHandler((UIElement)sender);
                DragAdorner = null;
                DropTargetAdorner = null;
                if (dropHandler != null)
                {
                    dropHandler.Drop(dropInfo);
                }
                else
                {
                    DefaultDropHandler.Drop(dropInfo);
                }
                e.Handled = true;
            }

        }
    }
}
