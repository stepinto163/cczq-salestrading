using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

namespace STC.Frame.Utils.MultiUI
{
    [ContentProperty(nameof(Child))]
    public sealed class DispatcherContainer : FrameworkElement
    {
        public DispatcherContainer()
        {
            _hostVisual = new InteractiveHostVisual();
        }

        private readonly HostVisual _hostVisual;
        private VisualTargetPresentationSource _targetSource;

        #region Child

        private bool _isUpdatingChild;
        private UIElement _child;
        public UIElement Child
        {
            get { return _child; }
        }

        public async Task SetChildAsync<T>(Dispatcher dispatcher = null)
            where T : UIElement, new()
        {
            await SetChildAsync(() => new T(), dispatcher);
        }

        public async Task SetChildAsync<T>(string name, Func<T> @new)
            where T : UIElement
        {
            var child = await MultDispaterHelper.InvokeByModelName(name, @new);
            await SetChildAsync(child);
        }


        public async Task SetChildAsync<T>(Func<T> @new, Dispatcher dispatcher = null)
            where T : UIElement
        {
            dispatcher = dispatcher ?? await UIDispatcher.RunNewAsync($"{typeof(T).Name}");
            var child = await dispatcher.InvokeAsync(@new);
            await SetChildAsync(child);
        }

        public async Task SetChildAsync(UIElement value)
        {
            if (_isUpdatingChild)
            {
                throw new InvalidOperationException("Child property should not be set during Child updating.");
            }

            _isUpdatingChild = true;
            try
            {
                await Task.Run(() => 
                {
                    var oldChild = _child;
                    var visualTarget = _targetSource;

                    if (Equals(oldChild, value))
                        return;

                    _targetSource = null;
                    if (visualTarget != null)
                    {
                        RemoveVisualChild(oldChild);
                        visualTarget.Dispatcher.InvokeAsync(visualTarget.Dispose);
                    }

                    _child = value;

                    if (value == null)
                    {
                        _targetSource = null;
                    }
                    else
                    {
                        value.Dispatcher.InvokeAsync(() =>
                        {
                            _targetSource = new VisualTargetPresentationSource(_hostVisual)
                            {
                                RootVisual = value,
                            };
                        });
                        AddVisualChild(_hostVisual);
                    }
                    InvalidateMeasure();
                });
            }

            finally
            {
                _isUpdatingChild = false;
            }
        }



        #endregion

        #region Tree & Layout

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _hostVisual;
        }

        protected override int VisualChildrenCount => _child != null ? 1 : 0;

        protected override Size MeasureOverride(Size availableSize)
        {
            var child = _child;
            if (child == null)
                return default(Size);

            child.Dispatcher.InvokeAsync(
                () => child.Measure(availableSize),
                DispatcherPriority.Loaded);

            return default(Size);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var child = _child;
            if (child == null)
                return finalSize;

            child.Dispatcher.InvokeAsync(
                () => child.Arrange(new Rect(finalSize)),
                DispatcherPriority.Loaded);

            return finalSize;
        }

        #endregion

        #region HitTest

        protected override HitTestResult HitTestCore(PointHitTestParameters htp)
        {
            var child = _child;

            var element = child?.Dispatcher.Invoke(() =>
            {
                double offsetX = 0d, offsetY = 0d;
                FrameworkElement fe;
                if (child is FrameworkElement )
                {
                    fe = (FrameworkElement)child;
                    offsetX = fe.Margin.Left;
                    offsetY = fe.Margin.Top;
                }
                return _child.InputHitTest(new Point(htp.HitPoint.X - offsetX, htp.HitPoint.Y - offsetY));
            }, DispatcherPriority.Normal);
            if (element == null)
            {
                return null;
            }

            return new PointHitTestResult(this, htp.HitPoint);
        }

        #endregion
    }
}
