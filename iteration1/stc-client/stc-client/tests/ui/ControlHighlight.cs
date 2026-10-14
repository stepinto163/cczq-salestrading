using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SalesTrading.UiTests
{
    // 仅由自动化进程创建。装饰层不改变业务控件的布局、焦点或鼠标命中。
    internal sealed class ControlHighlight : IDisposable
    {
        private AdornerLayer layer;
        private OutlineAdorner outline;
        private bool suspended;
        public bool VerificationPinned { get; set; }
        private bool disposed;

        public FrameworkElement Target { get; private set; }

        public ControlHighlight()
        {
            InputManager.Current.PostProcessInput += OnInput;
        }

        private void OnInput(object sender, ProcessInputEventArgs args)
        {
            var focus = args.StagingItem.Input as KeyboardFocusChangedEventArgs;
            if (!VerificationPinned && focus != null && focus.RoutedEvent == Keyboard.GotKeyboardFocusEvent)
            {
                Show(focus.NewFocus as FrameworkElement);
            }
        }

        public void Show(FrameworkElement target)
        {
            Clear();
            if (disposed || suspended || target == null || !target.IsVisible || !target.IsLoaded)
            {
                return;
            }
            layer = AdornerLayer.GetAdornerLayer(target);
            if (layer == null)
            {
                // 原生窗口或未提供装饰层的弹出内容不画错位边框。
                return;
            }
            Target = target;
            outline = new OutlineAdorner(target);
            layer.Add(outline);
            target.Unloaded += OnTargetUnloaded;
            target.IsVisibleChanged += OnTargetVisibilityChanged;
        }

        public void SetSuspended(bool value)
        {
            suspended = value;
            Clear();
        }

        public void Clear()
        {
            if (Target != null)
            {
                Target.Unloaded -= OnTargetUnloaded;
                Target.IsVisibleChanged -= OnTargetVisibilityChanged;
            }
            if (outline != null)
            {
                layer.Remove(outline);
            }
            outline = null;
            layer = null;
            Target = null;
        }

        private void OnTargetUnloaded(object sender, RoutedEventArgs args)
        {
            Clear();
        }

        private void OnTargetVisibilityChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (Target != null && !Target.IsVisible)
            {
                Clear();
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            InputManager.Current.PostProcessInput -= OnInput;
            Clear();
        }

        private sealed class OutlineAdorner : Adorner
        {
            private readonly Pen pen = new Pen(Brushes.Red, 3);

            public OutlineAdorner(UIElement target) : base(target)
            {
                IsHitTestVisible = false;
                Focusable = false;
                pen.Freeze();
            }

            protected override void OnRender(DrawingContext context)
            {
                Size size = AdornedElement.RenderSize;
                if (size.Width < 3 || size.Height < 3)
                {
                    return;
                }
                Rect visible = new Rect(size);
                // 装饰层可能在 ScrollViewer 外；显式裁剪，避免红框越过虚拟化表格的视口。
                DependencyObject ancestor = VisualTreeHelper.GetParent(AdornedElement);
                while (ancestor is Visual)
                {
                    var element = ancestor as UIElement;
                    if (element != null && (element.ClipToBounds || element is ScrollContentPresenter))
                    {
                        Rect bounds = element.TransformToDescendant(AdornedElement).TransformBounds(new Rect(element.RenderSize));
                        visible.Intersect(bounds);
                    }
                    ancestor = VisualTreeHelper.GetParent(ancestor);
                }
                if (visible.IsEmpty)
                {
                    return;
                }
                context.PushClip(new RectangleGeometry(visible));
                context.DrawRectangle(null, pen, new Rect(1.5, 1.5, size.Width - 3, size.Height - 3));
                context.Pop();
            }
        }
    }
}
