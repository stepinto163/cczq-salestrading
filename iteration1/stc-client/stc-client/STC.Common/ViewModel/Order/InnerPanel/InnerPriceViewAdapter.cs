using STC.Common.Data;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.ViewModel.Card;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace STC.Common.ViewModel.Order.InnerPanel
{
    public class InnerPriceViewAdapter
    {
        private static InnerPriceViewAdapter priceViewHelper = new InnerPriceViewAdapter();

        public static InnerPriceViewAdapter Instance { get { return priceViewHelper; } }

        private InnerPriceViewAdapter()
        { }

        private SendRequireViewModel sendPriceViewModel;
        private Window myWindow;

        public void Init(SendRequireViewModel sendPriceViewModel, Window myWindow)
        {
            this.sendPriceViewModel = sendPriceViewModel;
            this.myWindow = myWindow;
        }

        public void OnExpandClick()
        {


            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                if (myWindow != null)
                {
                    Button btn_Expand = (Button)myWindow.FindName("btn_Expand");

                    Border Border_Left1 = (Border)myWindow.FindName("Border_Left1");
                    Border Border_Left3 = (Border)myWindow.FindName("Border_Left3");
                    Border Border_Left4 = (Border)myWindow.FindName("Border_Left4");
                    Border Border_Left5 = (Border)myWindow.FindName("Border_Left5");
                    Border Border_Left7 = (Border)myWindow.FindName("Border_Left7");
                    //Border Border_Right1 = (Border)myWindow.FindName("Border_Right1");
                    Border Border_Right2 = (Border)myWindow.FindName("Border_Right2");
                    Line LineSplit = (Line)myWindow.FindName("LineSplit");
                    if (btn_Expand.Tag.ToString() == "Collapsed")
                    {
                        sendPriceViewModel.CardViewVisibility = Visibility.Visible;
                        //Border_Left1.BorderThickness = new Thickness(2, 2, 0, 0);
                        //Border_Left3.BorderThickness = new Thickness(2, 0, 0, 0);
                        //Border_Left4.BorderThickness = new Thickness(2, 0, 0, 0);
                        //Border_Left5.BorderThickness = new Thickness(2, 0, 0, 2);
                        //Border_Right1.BorderThickness = new Thickness(0.5, 2, 2, 2);
                        btn_Expand.Content = "<<";
                        myWindow.Width = 683;
                        btn_Expand.Tag = "Visible";
                        LineSplit.Stroke = (Brush)myWindow.FindResource("LostFocusBorderBrush");
                        LineSplit.Visibility = Visibility.Visible;
                        InitPriceView();

                        SetPriceAndOrderHeigh();
                    }
                    else
                    {
                        sendPriceViewModel.CardViewVisibility = Visibility.Collapsed;

                        Border_Left1.BorderThickness = new Thickness(2, 2, 2, 0);
                        Border_Left3.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left4.BorderThickness = new Thickness(2, 0, 2, 0);
                        Border_Left5.BorderThickness = new Thickness(2, 0, 2, 2);
                        //Border_Right1.BorderThickness = new Thickness(2, 2, 2, 2);
                        //Border_Right2.BorderThickness = new Thickness(2, 0, 2, 2);

                        btn_Expand.Content = ">>";
                        myWindow.Width = 395;
                        btn_Expand.Tag = "Collapsed";
                        PriceAndOrderView dgPrice = (PriceAndOrderView)myWindow.FindName("dgPrice");
                        LineSplit.Visibility = Visibility.Collapsed;
                        //dgPrice.SetBrokerSelect("ALL");
                    }

                }
            }));
        }


        public void InitPriceView(string searchText = "")
        {
            Application.Current.Dispatcher.BeginInvoke((Action)(() =>
            {
                string securityID = string.Empty;

                PriceMatchParam _priceMatchParam = sendPriceViewModel.GetPriceMatchParam();
                if (string.IsNullOrEmpty(searchText))
                {
                    if (_priceMatchParam != null)
                    {
                        securityID = _priceMatchParam.Symbol;
                    }
                    else
                    {
                        securityID = sendPriceViewModel.SendPriceItem.Security;
                    }
                }
                else
                {
                    securityID = searchText;
                }

                PriceAndOrderView tradeBookView = (PriceAndOrderView)myWindow.FindName("dgPrice");
                PriceAndOrderViewModel Qm = new PriceAndOrderViewModel();

                if (securityID != null)
                {
                    Qm.SecurityID = securityID;
                }

                Qm.GetLastExecutionsItem(securityID);
                tradeBookView.SetLastExecutionsSecurtityID(securityID);

                tradeBookView.RemoveDbDoubleClickEvent();
                tradeBookView.DataContext = Qm;
            }));
        }

        public void SetPriceAndOrderHeigh()
        {
            if (myWindow != null)
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Border listBox = myWindow.FindName("Border_Right1") as Border;

                    if (LocalEnv.ConfInfo.IsShowPriceCondition == "1")
                    {
                        listBox.Height = 565;
                    }
                    else
                    {
                        listBox.Height = 565;
                    }
                }));

            }
        }
    }
}
