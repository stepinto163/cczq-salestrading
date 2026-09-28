using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.ViewModel.Trade.Card;
using STC.Common.Model.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BDB.Trade.Plugin.View.Trade.Card
{
    /// <summary>
    /// PageCardItem.xaml 的交互逻辑
    /// </summary>
    public partial class PageCardItemView : UserControl
    {
        private PageCardItemViewModel pageCardItemViewModel = new PageCardItemViewModel();
        public static readonly int InitPanelWidth = 284, InitPanelHeight = 372, MinPanelHeight = 372;

        public PageCardItemView()
        {
            InitializeComponent();

            this.DataContext = pageCardItemViewModel;

            this.SizeChanged += PageCardItemView_SizeChanged; ;
        }

        public void PageCardItemView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //return;

            double calWidth = this.ActualWidth;
            double calHeight = this.ActualHeight;

            //if (QuoteConfigHandler.Instance.IsResetMainWin)
            //{
            //    calWidth = LayoutSetttingCache.Instance.layoutSettingPo.Col * 337 + 20;
            //    calHeight = LayoutSetttingCache.Instance.layoutSettingPo.Row * 424 + 128;
            //}

            //Console.WriteLine("QuoteUI_SizeChanged actual: {0},{1}", (int)grid.ActualWidth, (int)grid.ActualHeight);
            //Console.WriteLine("QuoteUI_SizeChanged this.actual: {0},{1}", (int)this.ActualWidth, (int)this.ActualHeight);
            if (calWidth > 0 && calHeight > 0)
            {
                //double perWidth = (this.ActualWidth - (LayoutSetttingCache.Instance.layoutSettingPo.Col * 4)) / LayoutSetttingCache.Instance.layoutSettingPo.Col;
                //double perHeight = (this.ActualHeight - -(LayoutSetttingCache.Instance.layoutSettingPo.Row * 4)) / LayoutSetttingCache.Instance.layoutSettingPo.Row;


                double perWidth = this.ActualWidth / LayoutSetttingCache.Instance.layoutSettingPo.Col;
                double perHeight = this.ActualHeight / LayoutSetttingCache.Instance.layoutSettingPo.Row;

                double preferHeight = perHeight / perWidth * InitPanelWidth;
                double preferWidth = InitPanelWidth;
                if (preferHeight < MinPanelHeight)
                {
                    preferHeight = MinPanelHeight;
                    preferWidth = perWidth / perHeight * MinPanelHeight;
                }
                //Console.WriteLine("---cal height:" + perHeight + ", withd:" + perWidth);
                //if (preferHeight < MinPanelHeight)
                //{
                //    preferHeight = MinPanelHeight;
                //    preferWidth = perWidth / perHeight * MinPanelHeight;
                //}

                //if (preferHeight < InitPanelHeight || preferWidth < InitPanelWidth)
                //{

                //}
                //else
                {
                    ResetCardWithAndHeight(preferHeight, preferWidth);
                }
            }
        }


        private void ResetCardWithAndHeight(double height, double width)
        {
            foreach (UIElement uiElement in grid.Children)
            {
                FrameworkElement frameworkElement = uiElement as FrameworkElement;
                frameworkElement.Height = height;
                frameworkElement.Width = width;

                Console.WriteLine("---height:" + height + ", width:" + width);
            }
        }


        public void Init(List<HomeGroupSecurityIDsInfo> securityIdList, int row, int col, string groupId, int showPageNum)
        {
            pageCardItemViewModel.Init(this, securityIdList, row, col, groupId, showPageNum);
        }


        public void ResetData(string groupId, int pageIndex, List<HomeGroupSecurityIDsInfo> securityIdList)
        {
            pageCardItemViewModel.ResetData(groupId, pageIndex, securityIdList);

            //PageCardItemView_SizeChanged(null, null);
        }

        public void ClearUI(bool clearCard)
        {
            pageCardItemViewModel.ClearUI(clearCard);
        }
    }
}
