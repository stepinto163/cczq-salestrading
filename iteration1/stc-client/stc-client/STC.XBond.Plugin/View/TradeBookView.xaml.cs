using Newtonsoft.Json;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Frame.Common;
using STC.XBond.Plugin.Command;
using STC.XBond.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace STC.XBond.Plugin.View
{
    /// <summary>
    /// TradeBookView.xaml 的交互逻辑
    /// </summary>
    public partial class TradeBookView : UserControl, IContentInterface
    {
        ////定义依赖项属性
        //public static DependencyProperty HeaderAmountProperty =
        //    DependencyProperty.Register("HeaderAmount", typeof(string), typeof(QuotationList), new PropertyMetadata(""));
        ////声明属性        
        ///// <summary>
        ///// 表头-量(手/万).
        ///// </summary>
        //public string HeaderAmount
        //{
        //    get { return (string)GetValue(HeaderAmountProperty); }
        //    set { SetValue(HeaderAmountProperty, value); }
        //}
        //// 债券代码切换属性
        //public static readonly DependencyProperty BondInfoProperty = DependencyProperty.Register("BondInfo", typeof(BondsInfoPo),
        //     typeof(TradeBookView));

        //public BondsInfoPo BondInfo
        //{
        //    get { return (BondsInfoPo)GetValue(BondInfoProperty); }
        //    set
        //    {
        //        SetValue(BondInfoProperty, value);
        //        RaiseEvent(new RoutedEventArgs { RoutedEvent = ValueChangedEvent });
        //    }
        //}

        //public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(QuotationFilterPanel));

        //public event RoutedEventHandler ValueChanged
        //{
        //    add { AddHandler(ValueChangedEvent, value); }
        //    remove { RemoveHandler(ValueChangedEvent, value); }
        //}

        public TradeBookView()
        {
            InitializeComponent();
            this.MouseEnter += TradeBookView_MouseEnter;
            QList.PriceTypeSelected += (s, e) =>
            {
                LastExecutions.UpdatePriceType(e.PriceTypeIndex);
            };
        }

        private void TradeBookView_MouseEnter(object sender, MouseEventArgs e)
        {
            SetFocus();
        }

        public void SetFocus()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (tbBondId.Text.ToLower() == "tradebook")
                {
                    lbCode.Visibility = Visibility.Hidden;
                    seBond.Visibility = Visibility.Visible;
                    seBond.SelectAll();
                    seBond.Focus();
                }
            }));
        }

        public void Clear()
        {
            if (this.LastExecutions.DataContext != null && this.LastExecutions.DataContext is LastExecutionViewModel)
            {
                LastExecutionViewModel marketExecutionVM = this.LastExecutions.DataContext as LastExecutionViewModel;
                marketExecutionVM.Dispose();
                this.LastExecutions.DataContext = null;
            }
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel quotationListVM = this.DataContext as QuotationListViewModel;
                quotationListVM.ClearData(true);
                quotationListVM.Dispose();
                QList.SetQtyHeaderText("4");
                //this.DataContext = null;
            }
        }

        private void Edit_Btn_Click(object sender, RoutedEventArgs e)
        {
            lbCode.Visibility = Visibility.Hidden;
            seBond.Visibility = Visibility.Visible;
            seBond.SelectAll();
        }

        private void lbCode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lbCode.Visibility = Visibility.Hidden;
            seBond.Visibility = Visibility.Visible;

            //if (this.Parent != null && this.Parent is TabItem)
            //{
            //    seBond.SearchText = ((TabItem)this.Parent).Header.ToString();
            //}

            
            seBond.SelectAll();
        }

        public void ChangeVisibility()
        {
            lbCode.Visibility = Visibility.Visible;
            seBond.Visibility = Visibility.Hidden;

            seBond.SearchText = string.Empty;
            seBond.ClearSerchTxt();
        }

        private void seBond_AfterSearch(Common.Model.Base.SimpleBondsInfoPo obj)
        {
            lbCode.Visibility = Visibility.Visible;
            seBond.Visibility = Visibility.Hidden;
            QList.AmountType = "0";
            SwitchBond(obj);
        }
        SolidColorBrush mouseOverColor = new SolidColorBrush(Color.FromArgb(100, 110, 32, 34));
        private Color _greenColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#136C5E");

        private void lbCode_MouseEnter(object sender, MouseEventArgs e)
        {
            lbCode.Background = Application.Current.MainWindow.FindResource("OperationBar.BorderBrush") as SolidColorBrush;
        }

        private void lbCode_MouseLeave(object sender, MouseEventArgs e)
        {
            lbCode.Background = new SolidColorBrush(Colors.Transparent);
        }

        public async void UpdateeLastExectu(string securityID)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {

                    this.LastExecutions.SetSecurtityID(securityID);
                    if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                    {
                        QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                        this.LastExecutions.UpdatePriceType(viewModel.PriceTypeIndex);
                    }
                }));
            });
        }


        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            //{
            //    QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
            //    this.LastExecutions.SecurtityID = string.IsNullOrEmpty(viewModel.SecurityID) ? "xxxxxx" : viewModel.SecurityID;
            //    this.LastExecutions.UpdatePriceType(viewModel.PriceTypeIndex);
            //}
        }

        private async void SwitchBond(SimpleBondsInfoPo BondInfo)
        {
            await Task.Run(() =>
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {

                    BorderWin popupWindow = CommonUtils.GetParentObject<BorderWin>(this);
                    if (popupWindow == null && BondInfo != null)
                    {
                        string findHeader = BondInfo.Bond_ID;
                        // 如果是信用债，显示债券名
                        if (CommonUtils.TradeBookHeader == "1")
                        {
                            findHeader = BondInfo.Short_Name;
                        }

                        if (TabPageHelper.IsExistTabItem(findHeader))
                        {
                            Common.View.Component.CustomMessageBox.Info($"{findHeader}已存在!");

                            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                            {
                                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;

                                seBond.SearchText = viewModel.SecurityID;
                            }

                            return;
                        }
                    }
                    else
                    {
                        if (BondInfo != null)
                        {
                            //popupWindow.Title = BondInfo.Bond_ID;
                            string oldBondId = popupWindow.SecurityID.Replace(PopupTypeEnum.TradeBook.ToString()+"-", "");
                            popupWindow.SetTitleAndKey($"{PopupTypeEnum.TradeBook.ToString()}-{BondInfo.Bond_ID}", BondInfo.Bond_ID, "STC.XBond.Plugin");
                            PopupTypeEnum pye = (PopupTypeEnum)Enum.Parse(typeof(PopupTypeEnum), PopupTypeEnum.TradeBook.ToString());
                            List<object> pobject = new List<object>();
                            pobject.Add(pye);
                            pobject.Add(BondInfo.Bond_ID);
                            popupWindow.SetCommandParameter(JsonConvert.SerializeObject(pobject.ToArray()));
                            WindowManager.Instance.ChangBorderWinDict(oldBondId, BondInfo.Bond_ID, popupWindow);
                            //改变债券后保存保存，防止因为未保存不能双击最大化后，再双击无法恢复正常
                            popupWindow.SaveWindowPos(false);

                        }
                    }



                    if (BondInfo != null)
                    {
                        string oldHead = string.Empty;
                        if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                        {
                            QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                            oldHead = viewModel.SecurityID;
                            viewModel.SecurityID = BondInfo.Bond_ID;
                            viewModel.Symbol = BondInfo.Short_Name;
                            viewModel.FullName = BondInfo.Short_Name;
                            viewModel.MarketIndicator = BondInfo.MarketIndicator;

                            QList.SetQtyHeaderText(BondInfo.MarketIndicator);
                            this.LastExecutions.UpdatePriceType(viewModel.PriceTypeIndex);

                        }
                        this.LastExecutions.SecurtityID = string.IsNullOrEmpty(BondInfo.Bond_ID) ? "xxxxx" : BondInfo.Bond_ID;
                        TabItem tabItem = GetOwnTabItem(oldHead);

                        if (tabItem != null)
                        {
                            tabItem.ToolTip = BondInfo.Bond_ID;
                            // 如果是信用债，显示债券名
                            if (CommonUtils.TradeBookHeader == "1")
                            {
                                tabItem.Header = BondInfo.Short_Name;
                            }
                            // 其他
                            else
                            {
                                tabItem.Header = BondInfo.Bond_ID;
                            }
                        }

                        //if (this.Parent != null && this.Parent is TabItem)
                        //{
                        //    ((TabItem)this.Parent).Header = BondInfo.Bond_ID;
                        //}
                    }
                }));
            });
        }

        private TabItem GetOwnTabItem(string header)
        {
            TabItem result = null;

            foreach (TabItem tabItem in AppUtils.Application.Panel.Items)
            {
                if ((int)tabItem.Tag == (int)PluginKeyEnum.TradeBookPlugin && tabItem.ToolTip.ConvertToString().Equals(header))
                {
                    result = tabItem;
                    break;
                }
            }

            return result;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is QuotationListViewModel)
            {
                QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                if (string.IsNullOrEmpty(viewModel.SecurityID) == false)
                {
                    AddFavoritesView addFavoritesView = new AddFavoritesView();
                    addFavoritesView.SecurityID = viewModel.SecurityID;
                    addFavoritesView.ShowDialog();
                }
                else
                {
                    Common.View.Component.CustomMessageBox.Info("请先选择债券!");
                }
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null)
            {
                if (this.DataContext != null && this.DataContext is QuotationListViewModel)
                {
                    QuotationListViewModel viewModel = (QuotationListViewModel)this.DataContext;
                    if ("行权".Equals(radioButton.Content))
                    {
                        viewModel.CheckExeRadio = true;
                        viewModel.CheckMarRadio = false;
                        if (viewModel.PriceTypeIndex == 0)
                        {
                            this.QList.Switch_Yield_Btn_Click(null, null);
                        }
                    }
                    else
                    {
                        viewModel.CheckExeRadio = false;
                        viewModel.CheckMarRadio = true;
                        if (viewModel.PriceTypeIndex == 0)
                        {
                            this.QList.Switch_Yield_Btn_Click(null, null);
                        }
                    }
                }
            }
        }

        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {

        }

        private Popup tagPopup = new Popup();

        private void InfoPart_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid g = sender as Grid;
            tagPopup.Child = this.FindResource("Float_CCDCValuaDate") as UserControl;
            tagPopup.DataContext = (sender as Grid).DataContext;
            tagPopup.Placement = PlacementMode.Bottom;
            tagPopup.PlacementTarget = g;
            tagPopup.IsOpen = true;
            
        }

        private void InfoPart_MouseLeave(object sender, MouseEventArgs e)
        {
            tagPopup.IsOpen = false;
        }
    }
}
