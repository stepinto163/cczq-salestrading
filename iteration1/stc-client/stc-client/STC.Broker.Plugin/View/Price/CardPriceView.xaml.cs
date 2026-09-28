using STC.Broker.Plugin.Model;
using STC.Broker.Plugin.ViewModel.Price;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Model.GW;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace STC.Broker.Plugin.View.Price
{
    /// <summary>
    /// PricingDetail.xaml 的交互逻辑
    /// </summary>
    public partial class CardPriceView : UserControl
    {
        private CardPriceViewModel _cardPriceViewmodel = null;

        private bool _isInit = false;

        private NoBorderWin myTradingPopupWin;

        #region 开放接口
        public string SecurityID
        {
            get { return (string)GetValue(SecurityIDProperty); }
            set
            {
                SetValue(SecurityIDProperty, value);
            }
        }

        public static readonly DependencyProperty SecurityIDProperty = DependencyProperty.Register("SecurityID", typeof(string),
        typeof(CardPriceView), new PropertyMetadata("", new PropertyChangedCallback(OnShowTextPropertyChange)));

        static void OnShowTextPropertyChange(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null) return;
        }

        #endregion
        public CardPriceView()
        {
            InitializeComponent();
            this.Loaded += CardPriceView_Loaded;
        }


        public CardPriceView(NoBorderWin mw)
        {
            InitializeComponent();
            this.myTradingPopupWin = mw;
            this.Loaded += CardPriceView_Loaded;
        }

        private void CardPriceView_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= CardPriceView_Loaded;
            if (DataContext is CardItem)
            {
                CardItem ci = (CardItem)DataContext;
                string security = ci.SecurityID;
                this.Init(security, ci);
            }
        }

        /// <summary>
        /// 弹窗
        /// </summary>
        /// <param name="security"></param>
        /// <param name="ci"></param>
        /// <param name="mw"></param>
        public void Init(string security, CardItem ci, NoBorderWin mw)
        {
            ThreadPoolHelper.InvokeThreadPool((P) =>
            {
                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    this.Init(security, ci);
                    _cardPriceViewmodel.myWindow = mw;
                    _cardPriceViewmodel.PopupCloseButtonVisible = Visibility.Visible;
                    _cardPriceViewmodel.MoreMenuButtonVisible = Visibility.Collapsed;
                    _cardPriceViewmodel.IsAddSecurityIDEnable = false;
                }));
            });
        }

        public void Init(string security,  CardItem ci)
        {
            _isInit = true;

            _cardPriceViewmodel = new CardPriceViewModel();
            _cardPriceViewmodel.CurCardItem = ci;
            _cardPriceViewmodel.mywin = this;

            if (string.IsNullOrEmpty(security))
            {
                _cardPriceViewmodel.IsAddSecurityIDVisible = Visibility.Visible;
                _cardPriceViewmodel.IsAllControlVisible = Visibility.Hidden;
                _cardPriceViewmodel.MoreMenuButtonVisible = Visibility.Collapsed;
                _cardPriceViewmodel.IsSearchSecurityIDVisible = Visibility.Hidden;
                _cardPriceViewmodel.CurCardItem = ci;
                this.DataContext = _cardPriceViewmodel;
            }
            else
            {
                _cardPriceViewmodel.SecurityID = security;
                _cardPriceViewmodel.SymbolName = ci?.SymbolName;
                this.DataContext = _cardPriceViewmodel;
                //订阅
                _cardPriceViewmodel.SubscribeCtpPrice(security);
                //记忆默认选择
                if (ci != null)
                {
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        FilterPanel.SetSelectedIndex(int.Parse(ci.SettleIndex ?? "1"));
                    }));
                }
            }
        }




        /// <summary>
        /// 清算速度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterPanel_FilterDicChanged(object sender, RoutedEventArgs e)
        {
            if (this.FilterPanel.FilterDic != null)
            {
                if (this.DataContext != null && this.DataContext is CardPriceViewModel)
                {
                    CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                    // 过滤
                    viewModel.FilterData(this.FilterPanel.FilterDic);
                }
            }
        }

        #region 处理双击点价功能
        /// <summary>
        /// GVN 双击点价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bdBid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            if (e.ClickCount == 1)
            {
                var timer = new System.Timers.Timer(500);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                {
                    var timer2 = (System.Timers.Timer)element.Tag;
                    timer2.Stop();
                    timer2.Dispose();
                    //UIElement_Click("1");
                    e.Handled = true;
                })));
                timer.Start();
                element.Tag = timer;
            }
            if (e.ClickCount > 1)
            {
                var timer = element.Tag as System.Timers.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    string gid = element.ToolTip.ToString();
                    UIElement_DoubleClick("1", gid);
                    e.Handled = true;
                }
            }
        }
        /// <summary>
        /// TKN 双击点价
        /// </summary>
        private void bdOfr_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            if (e.ClickCount == 1)
            {
                var timer = new System.Timers.Timer(500);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                {
                    var timer2 = (System.Timers.Timer)element.Tag;
                    timer2.Stop();
                    timer2.Dispose();
                    //UIElement_Click("1");
                    e.Handled = true;
                })));
                timer.Start();
                element.Tag = timer;
            }
            if (e.ClickCount > 1)
            {
                var timer = element.Tag as System.Timers.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    string gid = element.ToolTip.ToString();
                    UIElement_DoubleClick("0", gid);
                    e.Handled = true;
                }
            }
        }
        /// <summary>
        /// +0 +1 远 点价处理
        /// </summary>
        /// <param name="isBid"></param>
        /// <param name="param"></param>
        private void UIElement_DoubleClick(string isBid, string param)
        {
            if (isBid == "1")
            {
                if (this.DataContext != null && this.DataContext is CardPriceViewModel)
                {
                    CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                    if (param != null)
                    {
                        viewModel.OnBestBidCommand(param);
                    }
                }
            }
            else
            {
                if (this.DataContext != null && this.DataContext is CardPriceViewModel)
                {
                    CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                    if (param != null)
                    {
                        viewModel.OnBestOfrCommand(param);
                    }
                }
            }
        }

        #endregion

        private void PopBtnREF_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is CardPriceViewModel)
            {
                CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                viewModel.GetSelfQuotations();
                if (viewModel.SelfQuotations.Count > 0)
                {
                    popBondCode.IsOpen = true;
                }
            }
        }

        private void dgBondCode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            popBondCode.IsOpen = false;
            if (sender is DataGrid)
            {
                DataGridRow row = GetDataGridRow(e.OriginalSource as FrameworkElement);
                if (row != null)
                {
                    Model.Transaction.GoodsVolumeVoModel gvv = row.Item as Model.Transaction.GoodsVolumeVoModel;
                    if (gvv != null)
                    {
                        if (gvv.MDPriceLevel != 999)
                        {
                            CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                            viewModel.UnRefQuotations(gvv.id);
                        }
                    }
                }
            }
        }

        private DataGridRow GetDataGridRow(FrameworkElement element)
        {
            // 根据元素查找 DataGridRow
            var row = DataGridRow.GetRowContainingElement(element);

            return row;
        }

        /// <summary>
        /// ... 下拉按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is CardPriceViewModel)
            {
                CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                viewModel.GetSelfQuotations();
                if (!string.IsNullOrEmpty(viewModel.SecurityID))
                {
                    PopupMenu.IsOpen = true;
                }
            }
        }

        /// <summary>
        /// ... 下拉按钮处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgPopMenu_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupMenu.IsOpen = false;
            if (sender is DataGrid)
            {
                if ((sender as DataGrid).SelectedItem != null)
                {
                    if (this.DataContext != null && this.DataContext is CardPriceViewModel)
                    {
                        CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                        SignalData gvv = (SignalData)((sender as DataGrid).SelectedItem);
                        if (gvv != null)
                        {
                            if (gvv.Topic == "Popup")
                            {
                                viewModel.PopupSecurityID();
                            }
                            else if (gvv.Topic == "DelBond")
                            {
                                viewModel.DelSecurityID();
                            }
                        }
                    }
                }
            }
        }

        //禁用鼠标滚轮
        private void DgBidQuotation_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is CardPriceViewModel)
            {
                e.Handled = true;
            }
        }

        //禁用鼠标滚轮
        private void DgOfrQuotation_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is CardPriceViewModel)
            {
                e.Handled = true;
            }
        }

        private void DgMultiBidQuotation_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is CardPriceViewModel)
            {
                e.Handled = true;
            }
        }

        private void DgMultiOfrQuotation_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is CardPriceViewModel)
            {
                e.Handled = true;
            }
        }

        #region 处理 多 
        private void bdMultiBid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            if (e.ClickCount == 1)
            {
                var timer = new System.Timers.Timer(500);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                {
                    var timer2 = (System.Timers.Timer)element.Tag;
                    timer2.Stop();
                    timer2.Dispose();
                    //UIElement_Click("1");
                    e.Handled = true;
                })));
                timer.Start();
                element.Tag = timer;
            }
            if (e.ClickCount > 1)
            {
                var timer = element.Tag as System.Timers.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    string gid = element.ToolTip.ToString();
                    UIMultiElement_DoubleClick("1", gid);
                    e.Handled = true;
                }
            }
        }
        private void bdMultiOfr_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var element = (FrameworkElement)sender;
            if (e.ClickCount == 1)
            {
                var timer = new System.Timers.Timer(500);
                timer.AutoReset = false;
                timer.Elapsed += new ElapsedEventHandler((o, ex) => element.Dispatcher.Invoke(new Action(() =>
                {
                    var timer2 = (System.Timers.Timer)element.Tag;
                    timer2.Stop();
                    timer2.Dispose();
                    //UIElement_Click("1");
                    e.Handled = true;
                })));
                timer.Start();
                element.Tag = timer;
            }
            if (e.ClickCount > 1)
            {
                var timer = element.Tag as System.Timers.Timer;
                if (timer != null)
                {
                    timer.Stop();
                    timer.Dispose();
                    string gid = element.ToolTip.ToString();
                    UIMultiElement_DoubleClick("2", gid);
                    e.Handled = true;
                }
            }
        }

        private void UIMultiElement_DoubleClick(string isBid, string param)
        {
            if (isBid == "1")
            {
                if (this.DataContext != null && this.DataContext is CardPriceViewModel)
                {
                    CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                    if (param != null)
                    {
                        viewModel.OnMultiBestBidCommand(param);
                    }
                }
            }
            else
            {
                if (this.DataContext != null && this.DataContext is CardPriceViewModel)
                {
                    CardPriceViewModel viewModel = (CardPriceViewModel)this.DataContext;
                    if (param != null)
                    {
                        viewModel.OnMultiBestOfrCommand(param);
                    }
                }
            }
        }
        #endregion

        private void dgPopMenu_MouseUp(object sender, RoutedEventArgs e)
        {

        }

        private void SerachText_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is CardPriceViewModel)
            {
                if (string.IsNullOrEmpty(_cardPriceViewmodel.SecurityID??"".Trim()))
                {
                    SerachText.SearchTermTextBox.Text = "";
                    _cardPriceViewmodel.DelSecurityID();
                }
                else
                {
                    if (string.IsNullOrEmpty(SerachText.SearchText.Trim()))
                    {
                        _cardPriceViewmodel.DelSecurityID();
                    }
                    else
                    {
                        SerachText.SearchTermTextBox.Text = SerachText.SearchText.Trim();
                    }
                }

            }
        }
    }
}
