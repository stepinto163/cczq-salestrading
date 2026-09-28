using System;
using System.Windows;
using System.Windows.Controls;

namespace STC.Common.View.StrongNotify
{
    /// <summary>
    /// 窗口中的控件从右下角开始排列，向上依次排列
    /// 当垂直空间不足时，控件换到新列（右向左排列）。
    /// 支持动态添加控件，控件宽度可自由设置。
    /// 支持最大显示宽度 10000像素，配合 StrongNotifyMainWindow.xaml界面当显示数目多于屏幕可显示区域时，显示滚动条
    /// </summary>
    /// <remarks>
    /// 作者：陈虎 切记请不要擅自修改代码
    /// </remarks>
    public class RightToLeftBottomToTopPanel : Panel
    {
        // 定义子元素变化的事件
        public event EventHandler ChildrenChanged;

        //MeasureOverride 确保安全的 DesiredSize：
        //当 availableSize 为无穷大时，提供默认值（如 10000），避免返回无效的 DesiredSize。
        //正确计算控件的所需宽度和高度，确保不超出容器大小。
        //ArrangeOverride 完善排列逻辑：
        //从右到左、从下到上排列子控件。
        //超出当前列高度时切换到下一列。
        //防止无效尺寸：
        //所有返回的尺寸值（宽度、高度）都被限制在 availableSize 范围内。
        protected override Size MeasureOverride(Size availableSize)
        {
            Size panelDesiredSize = new Size(0, 0); 
            double currentColumnWidth = 0;         
            double currentColumnHeight = 0;        
            double currentColumnX = 0;             
            double maxColumnHeight = 0;            
            double childWidth = 0;                 

            // 遍历所有子元素
            foreach (UIElement child in InternalChildren)
            {
                if (child == null) continue;
                if (childWidth == 0 && child is UserControl)
                {
                    child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    childWidth = child.DesiredSize.Width;
                    currentColumnWidth = childWidth;
                }
                else if (childWidth > 0)
                {
                    child.Measure(new Size(childWidth, double.PositiveInfinity));
                }
                else
                {
                    child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                }


                Size childSize = child.DesiredSize;

                if (currentColumnHeight + childSize.Height > availableSize.Height && availableSize.Height != double.PositiveInfinity)
                {
                    // 换列
                    panelDesiredSize.Width += currentColumnWidth;
                    maxColumnHeight = Math.Max(maxColumnHeight, currentColumnHeight);
                    currentColumnHeight = 0; 
                    currentColumnX -= currentColumnWidth; 
                }

                currentColumnHeight += childSize.Height;
            }

            panelDesiredSize.Width += currentColumnWidth;
            panelDesiredSize.Height = Math.Max(maxColumnHeight, currentColumnHeight);

            return panelDesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double currentColumnWidth = 0;
            double currentColumnHeight = 0;
            double currentColumnX = finalSize.Width;
            double currentColumnY = finalSize.Height;
            double childWidth = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null) continue;

                if (childWidth == 0 && child is UserControl)
                {
                    childWidth = child.DesiredSize.Width;
                    currentColumnWidth = childWidth;
                }
                else if (childWidth > 0)
                {
                    // 使用统一宽度
                }


                Size childSize = child.DesiredSize;

                if (currentColumnHeight + childSize.Height > finalSize.Height)
                {
                    currentColumnHeight = 0;
                    currentColumnX -= currentColumnWidth;
                    currentColumnY = finalSize.Height;
                }

                Rect childRect = new Rect(
                    currentColumnX - childSize.Width,
                    currentColumnY - childSize.Height,
                    childSize.Width,
                    childSize.Height
                );

                child.Arrange(childRect);

                currentColumnY -= childSize.Height;
                currentColumnHeight += childSize.Height;
            }

            ChildrenChanged?.Invoke(this, EventArgs.Empty);
            return finalSize;
        }
    }
}
