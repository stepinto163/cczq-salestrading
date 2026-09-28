using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.HomePage
{
    /// <summary>
    /// HomePageTabItemWithClose.xaml 的交互逻辑
    /// </summary>
    public partial class HomePageTabItemWithClose : TabItem
    {
        /// <summary>
        ///     动画速度
        /// </summary>
        private const int AnimationSpeed = 150;

        /// <summary>
        ///     选项卡是否处于拖动状态
        /// </summary>
        private static bool ItemIsDragging;

        /// <summary>
        ///     选项卡是否等待被拖动
        /// </summary>
        private bool _isWaiting;

        /// <summary>
        ///     拖动中的选项卡坐标
        /// </summary>
        private Point _dragPoint;

        /// <summary>
        ///     鼠标按下时选项卡位置
        /// </summary>
        private int _mouseDownIndex;

        /// <summary>
        ///     鼠标按下时选项卡横向偏移
        /// </summary>
        private double _mouseDownOffsetX;

        /// <summary>
        ///     鼠标按下时的坐标
        /// </summary>
        private Point _mouseDownPoint;

        /// <summary>
        ///     右侧可移动的最大值
        /// </summary>
        private double _maxMoveRight;

        /// <summary>
        ///     左侧可移动的最大值
        /// </summary>
        private double _maxMoveLeft;

        /// <summary>
        ///     选项卡宽度
        /// </summary>
        public double ItemWidth { get; internal set; }

        /// <summary>
        ///     选项卡拖动等待距离（在鼠标移动了超过20个像素无关单位后，选项卡才开始被拖动）
        /// </summary>
        private const double WaitLength = 20;

        /// <summary>
        ///     选项卡是否处于拖动状态
        /// </summary>
        private bool _isDragging;

        /// <summary>
        ///     选项卡是否已经被拖动
        /// </summary>
        private bool _isDragged;

        /// <summary>
        ///     目标横向位移
        /// </summary>
        internal double TargetOffsetX { get; set; }

        /// <summary>
        ///     当前编号
        /// </summary>
        private int _currentIndex;

        /// <summary>
        ///     标签容器横向滚动距离
        /// </summary>
        private double _scrollHorizontalOffset;

        private HomePageTabPanel _tabPanel;

        /// <summary>
        ///     标签容器
        /// </summary>
        internal HomePageTabPanel TabPanel
        {
            get
            {
                if (_tabPanel == null && TabControlParent != null)
                {
                    _tabPanel = TabControlParent.HeaderPanel;
                    //_tabPanel = CommonUtils.GetVisualChild<HomePageTabPanel>(TabControlParent, dp => dp.Name == "PART_HeaderPanel");
                }

                return _tabPanel;
            }
            set
            {
                _tabPanel = value;
            }
        }

        /// <summary>
        ///     当前编号
        /// </summary>
        internal int CurrentIndex
        {
            get { return _currentIndex; }
            set
            {
                if (_currentIndex == value || value < 0) return;
                var oldIndex = _currentIndex;
                _currentIndex = value;
                //UpdateItemOffsetX(oldIndex);
            }
        }

        /// <summary>
        ///     更新选项卡横向偏移
        /// </summary>
        /// <param name="oldIndex"></param>
        private void UpdateItemOffsetX(int oldIndex)
        {
            if (!_isDragging) return;
            var moveItem = TabPanel.ItemDic[CurrentIndex];
            moveItem.CurrentIndex -= CurrentIndex - oldIndex;
            var offsetX = moveItem.TargetOffsetX;
            var resultX = offsetX + (oldIndex - CurrentIndex) * ItemWidth;
            TabPanel.ItemDic[CurrentIndex] = this;
            TabPanel.ItemDic[moveItem.CurrentIndex] = moveItem;
            moveItem.CreateAnimation(offsetX, resultX);
        }
        private HomePageTabControl TabControlParent => ItemsControl.ItemsControlFromItemContainer(this) as HomePageTabControl;

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

            if (VisualTreeHelper.HitTest(this, e.GetPosition(this)) == null) return;
            IsSelected = true;

            Focus();
        }

        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            // 判断是否重复
            if(TabPanel != null)
            {
                foreach(KeyValuePair<int, HomePageTabItemWithClose> pair in TabPanel.ItemDic)
                {
                    if (pair.Key != this.CurrentIndex && pair.Value.Header.Equals(newHeader))
                    {
                        this.Header = oldHeader;
                        Common.View.Component.CustomMessageBox.Warning("分组名称已存在!");
                        return;
                    }
                }
            }

            base.OnHeaderChanged(oldHeader, newHeader);

            if (oldHeader != null && newHeader != null && oldHeader != newHeader)
            {
                TabItemOnHeaderChangeAct?.Invoke(this, newHeader);
            }
            

            if (TabPanel != null)
            {
                TabPanel.ForceUpdate = true;
                InvalidateMeasure();
                TabPanel.ForceUpdate = true;
            }
        }

        protected override void OnHeaderStringFormatChanged(string oldHeaderStringFormat, string newHeaderStringFormat)
        {
            base.OnHeaderStringFormatChanged(oldHeaderStringFormat, newHeaderStringFormat);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (VisualTreeHelper.HitTest(this, e.GetPosition(this)) == null) return;
            var parent = TabControlParent;
            if (parent == null) return;

            if (!ItemIsDragging && !_isDragging)
            {
                parent.UpdateScroll();
                TabPanel.SetValue(HomePageTabPanel.FluidMoveDurationPropertyKey, new Duration(TimeSpan.FromSeconds(0)));
                _mouseDownOffsetX = RenderTransform.Value.OffsetX;
                _scrollHorizontalOffset = parent.GetHorizontalOffset();
                var mx = TranslatePoint(new Point(), parent).X + _scrollHorizontalOffset;
                _mouseDownIndex = CalLocationIndex(mx);
                var subIndex = _mouseDownIndex - CalLocationIndex(_scrollHorizontalOffset);
                _maxMoveLeft = -subIndex * ItemWidth;
                _maxMoveRight = parent.ActualWidth - ActualWidth + _maxMoveLeft;

                _isDragging = false;
                ItemIsDragging = false;
                _isWaiting = true;
                _dragPoint = e.GetPosition(parent);
                _dragPoint = new Point(_dragPoint.X + _scrollHorizontalOffset, _dragPoint.Y);
                _mouseDownPoint = _dragPoint;
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (ItemIsDragging && _isDragging)
            {
                var parent = TabControlParent;
                if (parent == null) return;

                var subX = TranslatePoint(new Point(), parent).X + _scrollHorizontalOffset;
                CurrentIndex = CalLocationIndex(subX);

                var p = e.GetPosition(parent);
                p = new Point(p.X + _scrollHorizontalOffset, p.Y);

                var subLeft = p.X - _dragPoint.X;
                var totalLeft = p.X - _mouseDownPoint.X;

                if (Math.Abs(subLeft) <= WaitLength && _isWaiting) return;

                _isWaiting = false;
                _isDragged = true;

                var left = subLeft + RenderTransform.Value.OffsetX;
                if (totalLeft < _maxMoveLeft)
                {
                    left = _maxMoveLeft + _mouseDownOffsetX;
                }
                else if (totalLeft > _maxMoveRight)
                {
                    left = _maxMoveRight + _mouseDownOffsetX;
                }

                var t = new TranslateTransform(left, 0);
                RenderTransform = t;
                _dragPoint = p;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            ReleaseMouseCapture();

            if (_isDragged)
            {
                var parent = TabControlParent;
                if (parent == null) return;

                var subX = TranslatePoint(new Point(), parent).X + _scrollHorizontalOffset;
                var index = CalLocationIndex(subX);
                var left = index * ItemWidth;
                var offsetX = RenderTransform.Value.OffsetX;
                CreateAnimation(offsetX, offsetX - subX + left, index);
            }

            _isDragging = false;
            ItemIsDragging = false;
            _isDragged = false;
        }

        void AnimationCompleted(HomePageTabControl parent, double resultX, int index = -1)
        {
            RenderTransform = new TranslateTransform(resultX, 0);
            if (index == -1) return;

            var list = parent.GetActualList();
            if (list == null) return;

            var item = parent.ItemContainerGenerator.ItemFromContainer(this);
            if (item == null) return;

            TabPanel.CanUpdate = false;
            parent.IsInternalAction = true;

            int originalIndex = list.IndexOf(item);
            list.Remove(item);
            parent.IsInternalAction = true;
            list.Insert(index, item);
            _tabPanel.SetValue(HomePageTabPanel.FluidMoveDurationPropertyKey, new Duration(TimeSpan.FromMilliseconds(0)));
            TabPanel.CanUpdate = true;
            TabPanel.ForceUpdate = true;
            TabPanel.Measure(new Size(TabPanel.DesiredSize.Width, ActualHeight));
            TabPanel.ForceUpdate = false;

            Focus();
            IsSelected = true;

            if (!IsMouseCaptured)
            {
                parent.SetCurrentValue(Selector.SelectedIndexProperty, _currentIndex);
            }
            if (originalIndex != index)
            {
                TabItemUpdateAct?.Invoke();
            }
        }

        /// <summary>
        ///     创建动画
        /// </summary>
        internal void CreateAnimation(double offsetX, double resultX, int index = -1)
        {
            ////var parent = TabControlParent;

            ////TargetOffsetX = resultX;
            ////if (!parent.IsAnimationEnabled)
            ////{
            ////    AnimationCompleted(parent, resultX, index);
            ////    return;
            ////}
           
            ////var animation = AnimationHelper.CreateAnimation(resultX, AnimationSpeed);
            ////animation.FillBehavior = FillBehavior.Stop;
            ////animation.Completed += (s1, e1) => AnimationCompleted(parent, resultX, index);
            ////var f = new TranslateTransform(offsetX, 0);
            ////RenderTransform = f;
            ////f.BeginAnimation(TranslateTransform.XProperty, animation, HandoffBehavior.Compose);
        }

        /// <summary>
        ///     计算选项卡当前合适的位置编号
        /// </summary>
        /// <param name="left"></param>
        /// <returns></returns>
        private int CalLocationIndex(double left)
        {
            if (_isWaiting)
            {
                return CurrentIndex;
            }

            var maxIndex = TabControlParent.Items.Count - 1;
            var div = (int)(left / ItemWidth);
            var rest = left % ItemWidth;
            var result = rest / ItemWidth > .5 ? div + 1 : div;

            return result > maxIndex ? maxIndex : result;
        }


        public Func<HomePageTabItemWithClose, bool> TabItemClosedFunc;
        public Action TabItemUpdateAct;
        public Action<HomePageTabItemWithClose, object> TabItemOnHeaderChangeAct;

        public HomePageTabItemWithClose()
        {
            InitializeComponent();
        }

        #region 关闭按钮
        /// <summary>
        /// 关闭按钮
        /// </summary>
        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            if (TabControlParent == null)
                return;

            // 关闭tabItem的路由
            bool isSuccess = true;
            if (TabItemClosedFunc != null)
            {
                isSuccess = TabItemClosedFunc(this);
            }

            if (isSuccess == false)
            {
                return;
            }

            var parent = TabControlParent;
            if (parent == null) return;

            HomePageTabItemWithClose item = parent.ItemContainerGenerator.ItemFromContainer(this) as HomePageTabItemWithClose;

            TabPanel.SetValue(HomePageTabPanel.FluidMoveDurationPropertyKey, parent.IsAnimationEnabled
                    ? new Duration(TimeSpan.FromMilliseconds(200))
                    : new Duration(TimeSpan.FromMilliseconds(1)));

            parent.IsInternalAction = true;

            var list = parent.GetActualList();
            list?.Remove(item);
            ////if (item.Content is FavoriteBondsListBox )
            ////{
            ////    FavoriteBondsListBox favoriteBondListBox = item.Content as FavoriteBondsListBox;
            ////    favoriteBondListBox.Clear();
            ////}
        }
        #endregion

        private void templateRoot_MouseEnter(object sender, MouseEventArgs e)
        {
            Brush brush_MouseOver = (Brush)this.FindResource("TabItem.MouseOver.Border");
            Button btn = (Button)this.GetTemplateChild("btn_Close");
            if (btn != null)
            {
                btn.Visibility = Visibility.Visible;
            }
            this.Background = brush_MouseOver;
        }

        private void templateRoot_MouseLeave(object sender, MouseEventArgs e)
        {
            Button btn = (Button)this.GetTemplateChild("btn_Close");
            if (btn != null)
            {
                btn.Visibility = Visibility.Hidden;
            }
            this.Background = Brushes.Transparent;
        }
    }
}
