using BDB.Trade.Plugin.Cache;
using BDB.Trade.Plugin.ViewModel.UserSetting;
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

namespace BDB.Trade.Plugin.View.UserSetting
{
    /// <summary>
    /// PageSetQuotation.xaml 的交互逻辑
    /// </summary>
    public partial class PageSetQuotation : UserControl
    {
        public bool isLayoutChanged;

        public Action<bool> callBack { get; set; }
        public PageSetQuotation()
        {
            InitializeComponent();
            if (TradingPlugin.BP_WITH_CCDC == "1")
            {
                tbCCDC.IsChecked = true;
            }
            Loaded += SettingUI_Loaded;
            layoutSetting.LayoutChanged += LayoutSetting_LayoutChanged;
        }

        private void SettingUI_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        /// <summary>
        /// 初始化设置界面
        /// </summary>
        private void Init()
        {
            if (LayoutSetttingCache.Instance.layoutSettingPo.ShowModel == 1)
            {
                modeList.IsChecked = true;
                modeCard.IsChecked = false;

                tbLayoutInfo.Text = string.Empty;
            }
            else
            {
                layoutSetting.SetRowColumn(LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);

                tbLayoutInfo.Text = string.Format("{0}X{1}", LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);
                modeList.IsChecked = false;
                modeCard.IsChecked = true;
            }

        }

        public LayoutSettingPo GetLayoutSetting()
        {
            LayoutSettingPo layoutSettingPo = new LayoutSettingPo();
            layoutSettingPo.Row = layoutSetting.Rows;
            layoutSettingPo.Col = layoutSetting.Columns;
            layoutSettingPo.ShowModel = modeList.IsChecked.Value ? 1 : modeCard.IsChecked.Value ? 2 : 1;

            return layoutSettingPo;
        }

        private void LayoutSetting_LayoutChanged(object sender, object e)
        {
            modeList.IsChecked = false;
            modeCard.IsChecked = true;
            isLayoutChanged = true;
            tbLayoutInfo.Text = string.Format("{0}X{1}", layoutSetting.Rows, layoutSetting.Columns);

        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            callBack(false);
        }
        public void btConfirm_Click(object sender, RoutedEventArgs e)
        {
            //判断当前选择的是什么模式
            //configHandler.ConfigPo.CurrentDisplayMode = modeList.IsChecked == true ? DisplayMode.List : DisplayMode.Card;
            //if (!modeList.IsChecked.Value)
            //{
            //    configHandler.ConfigPo.Cols = layoutSetting.Columns;
            //    configHandler.ConfigPo.Rows = layoutSetting.Rows;
            //}
        }

        /// <summary>
        /// 列表模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeListChecked(object sender, RoutedEventArgs e)
        {
            //layoutSetting.SetRowColumn(0, 0);
            tbLayoutInfo.Text = "";
        }

        /// <summary>
        /// 卡片模式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void modeCardChecked(object sender, RoutedEventArgs e)
        {
            tbLayoutInfo.Text = string.Format("{0}X{1}", LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);
            layoutSetting.SetRowColumn(LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);
        }

        private void smallBtn_Click(object sender, RoutedEventArgs e)
        {
            //LayoutSetttingCache.Instance.layoutSettingPo.Row = 2;
            //LayoutSetttingCache.Instance.layoutSettingPo.Col = 6;
            tbLayoutInfo.Text = string.Format("{0}X{1}", 2, 6);
            //layoutSetting.SetRowColumn(LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);

            layoutSetting.SetRowColumn(2, 6);
        }

        private void bigBtn_Click(object sender, RoutedEventArgs e)
        {
            //LayoutSetttingCache.Instance.layoutSettingPo.Row = 3;
            //LayoutSetttingCache.Instance.layoutSettingPo.Col = 7;
            tbLayoutInfo.Text = string.Format("{0}X{1}", 3, 7);
            layoutSetting.SetRowColumn(LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);

            layoutSetting.SetRowColumn(3, 7);
        }

        private void vBtn_Click(object sender, RoutedEventArgs e)
        {
            //LayoutSetttingCache.Instance.layoutSettingPo.Row = 5;
            //LayoutSetttingCache.Instance.layoutSettingPo.Col = 4;
            tbLayoutInfo.Text = string.Format("{0}X{1}", 5, 4);
            layoutSetting.SetRowColumn(LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);


            layoutSetting.SetRowColumn(5, 4);
        }

        private void small_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                cbMid.IsChecked = cbBig.IsChecked = false;

                tbLayoutInfo.Text = string.Format("{0}X{1}", 2, 6);
                layoutSetting.SetRowColumn(2, 6);
            }
        }

        private void mid_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                cbSmall.IsChecked = cbBig.IsChecked = false;

                tbLayoutInfo.Text = string.Format("{0}X{1}", 3, 7);
                layoutSetting.SetRowColumn(LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);

                layoutSetting.SetRowColumn(3, 7);
            }
        }

        private void big_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
            {
                cbMid.IsChecked = cbSmall.IsChecked = false;

                tbLayoutInfo.Text = string.Format("{0}X{1}", 5, 4);
                layoutSetting.SetRowColumn(LayoutSetttingCache.Instance.layoutSettingPo.Row, LayoutSetttingCache.Instance.layoutSettingPo.Col);


                layoutSetting.SetRowColumn(5, 4);
            }
        }
    }
}
