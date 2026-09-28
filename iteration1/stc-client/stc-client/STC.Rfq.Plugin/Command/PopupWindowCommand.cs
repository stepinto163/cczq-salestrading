using com.gateway.log;
using STC.Common.Utils;
using STC.Frame.CustomUI;
using STC.Common.View.UI;
using STC.Rfq.Plugin.View;
using STC.Rfq.Plugin.View.Component;
using STC.Rfq.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace STC.Rfq.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 策略参数小窗
        /// </summary>
        OrderControl,

        /// <summary>
        /// 双边主表
        /// </summary>
        MMMainView,

        /// <summary>
        /// 做市统计
        /// </summary>
        MmStatisticsView,
    }

    /// <summary>
    /// 窗口弹出命令执行器
    /// </summary>
    public class PopupWindowCommand : ICommand
    {
        WindowManager windowManager = WindowManager.Instance;//new WindowManager();

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                if (parameter is object[])
                {
                    object[] args = parameter as object[];
                    PopupTypeEnum popupTypeEnum = (PopupTypeEnum)args[0];

                    //if (popupTypeEnum == PopupTypeEnum.OrderControl)
                    //{
                    //    if (args[1].GetType() == typeof(string))
                    //    {
                    //        string securityID = args[1].ToString();
                    //        MMOrderControlModel model = new MMOrderControlModel();
                    //        if (windowManager.IsExist($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}") == false && !string.IsNullOrEmpty(securityID))
                    //        {
                    //            NoBorderWin popOrderWin = new NoBorderWin(securityID);
                    //            popOrderWin.Title = securityID;
                    //            popOrderWin.Closed += PopOrderWin_Closed;
                    //            MMOrderControl orderControl = new MMOrderControl();
                    //            orderControl.securityID = securityID;
                    //            orderControl.popupBtn.Visibility = Visibility.Collapsed;
                    //            orderControl.btnFixFloatButton.Visibility = Visibility.Visible;
                    //            orderControl.btnShutDown.Visibility = Visibility.Visible;
                    //            popOrderWin.SetMainControl(orderControl);
                    //            popOrderWin.ResizeMode = ResizeMode.CanResize;
                    //            popOrderWin.Height = 550;
                    //            popOrderWin.Width = 300;
                    //            popOrderWin.MinWidth = 290;
                    //            popOrderWin.MaxWidth = 290;
                    //            popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                    //            popOrderWin.WindowStyle = WindowStyle.None;
                    //            popOrderWin.WindowState = WindowState.Normal;
                    //            popOrderWin.Owner = null;

                    //            popOrderWin.Show();
                    //            windowManager.OpenOnlyOneWindow($"{PopupTypeEnum.OrderControl.ToString()}-{securityID}", popOrderWin);
                    //        }
                    //    }
                    //    else
                    //    {
                    //        //该方法废弃
                    //        MMOrderControl control = args[1] as MMOrderControl;
                    //        if (control != null && control.DataContext != null && control.DataContext is MMOrderControlModel)
                    //        {
                    //            MMOrderControlModel model = control.DataContext as MMOrderControlModel;
                    //            string securityID = CommonUtils.ConvertToString(model.securityID);
                    //            if (windowManager.IsExist($"STC.Mm.Plugin-{PopupTypeEnum.OrderControl.ToString()}-{securityID}") == false && !string.IsNullOrEmpty(securityID))
                    //            {

                    //                NoBorderWin popOrderWin = new NoBorderWin(securityID);
                    //                popOrderWin.Title = securityID;
                    //                popOrderWin.Closed += PopOrderWin_Closed;
                    //                MMOrderControl orderControl = new MMOrderControl();
                    //                orderControl.securityID = securityID;
                    //                orderControl.popupBtn.Visibility = Visibility.Collapsed;
                    //                orderControl.btnFixFloatButton.Visibility = Visibility.Visible;
                    //                orderControl.btnShutDown.Visibility = Visibility.Visible;
                    //                popOrderWin.SetMainControl(orderControl);
                    //                popOrderWin.ResizeMode = ResizeMode.CanResize;
                    //                popOrderWin.Height = 550;
                    //                popOrderWin.Width = 300;
                    //                popOrderWin.MinWidth = 290;
                    //                popOrderWin.MaxWidth = 290;
                    //                popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                    //                popOrderWin.WindowStyle = WindowStyle.None;
                    //                popOrderWin.WindowState = WindowState.Normal;
                    //                popOrderWin.Owner = null;

                    //                popOrderWin.Show();
                    //                windowManager.OpenOnlyOneWindow($"STC.Mm.Plugin-{PopupTypeEnum.OrderControl.ToString()}-{securityID}", popOrderWin);
                    //            }
                    //        }
                    //    }
                    //}
                    //else if (popupTypeEnum == PopupTypeEnum.MMMainView)
                    //{
                    //    if (args.Length > 1)
                    //    {
                    //        MainView control = args[1] as MainView;
                    //        if (control != null && control.DataContext != null && control.DataContext is MainViewModel)
                    //        {
                    //            MainViewModel vm = control.DataContext as MainViewModel;

                    //            if (windowManager.IsExist($"STC.Mm.Plugin-{PopupTypeEnum.MMMainView.ToString()}") == false)
                    //            {
                    //                BorderWin popOrderWin = new BorderWin();
                    //                popOrderWin.SetTitleAndKey(PopupTypeEnum.MMMainView.ToString(), "双边列表", "STC.Mm.Plugin");
                    //                popOrderWin.SetCommandParameter($"{((int)PopupTypeEnum.MMMainView).ToString()}");
                    //                popOrderWin.Closed += PopOrderWin_Closed;
                    //                MainView orderControl = new MainView();
                    //                orderControl.Padding = new Thickness(0);
                    //                orderControl.DataContext = vm;
                    //                ////orderControl.btnPopup.Visibility = Visibility.Collapsed;
                    //                ////orderControl.OrderControl.Visibility = Visibility.Collapsed;
                    //                popOrderWin.SetMainControl(orderControl);
                    //                popOrderWin.ResizeMode = ResizeMode.CanResize;
                    //                popOrderWin.Height = 550;
                    //                popOrderWin.Width = 1000;
                    //                popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    //                popOrderWin.Show();
                    //                windowManager.OpenOnlyOneWindow($"STC.Mm.Plugin-{PopupTypeEnum.MMMainView.ToString()}", popOrderWin);
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        MainViewModel vm = new MainViewModel();
                    //        if (windowManager.IsExist($"STC.Mm.Plugin-{PopupTypeEnum.MMMainView.ToString()}") == false)
                    //        {
                    //            BorderWin popOrderWin = new BorderWin();
                    //            popOrderWin.SetTitleAndKey(PopupTypeEnum.MMMainView.ToString(), "双边列表", "STC.Mm.Plugin");
                    //            popOrderWin.SetCommandParameter($"{((int)PopupTypeEnum.MMMainView).ToString()}");
                    //            popOrderWin.Closed += PopOrderWin_Closed;
                    //            MainView orderControl = new MainView();
                    //            orderControl.Padding = new Thickness(0);
                    //            orderControl.DataContext = vm;
                    //            ////orderControl.btnPopup.Visibility = Visibility.Collapsed;
                    //            ////orderControl.OrderControl.Visibility = Visibility.Collapsed;
                    //            popOrderWin.SetMainControl(orderControl);
                    //            popOrderWin.ResizeMode = ResizeMode.CanResize;
                    //            popOrderWin.Height = 550;
                    //            popOrderWin.Width = 1000;
                    //            popOrderWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                    //            popOrderWin.Show();
                    //            windowManager.OpenOnlyOneWindow($"STC.Mm.Plugin-{PopupTypeEnum.MMMainView.ToString()}", popOrderWin);
                    //        }
                    //    }
                    //}
                }
            }
            catch(Exception e)
            {
                LogHelper.Error($"PopupWindowCommand {parameter}",e);
            }
        }

        private void PopOrderWin_Closed(object sender, EventArgs e)
        {
            if (sender != null  && sender is PopupWindow)
            {
                PopupWindow popupWindow = sender as PopupWindow;
                if (popupWindow != null)
                {
                    if (popupWindow.Content is MainView)
                    {
                        MainView mainView = popupWindow.Content as MainView;
                        mainView.Dispose();
                    }
                }
            }
            //to do...
            ////else if (sender != null && sender is NoBorderWin)
            ////{
            ////    NoBorderWin popupWindow = sender as NoBorderWin;
            ////    if (popupWindow != null)
            ////    {
            ////        if (popupWindow.Content is System.Windows.Controls.Frame
            ////            && (popupWindow.Content as System.Windows.Controls.Frame).Content is MMOrderControl)
            ////        {
            ////            MMOrderControl mmOrderControl = (popupWindow.Content as System.Windows.Controls.Frame).Content as MMOrderControl;
            ////            mmOrderControl.UnregistMM();
            ////        }
            ////    }

            ////}
        }

        /// <summary>
        /// '我的交易'弹出窗口关闭事件
        /// </summary>
        /// <param name="sender">'我的交易'弹出窗口实例</param>
        /// <param name="e">表示包含事件数据的类的基类，并提供要用于不包含事件数据的事件的值。</param>
        private void MyTradingPopupWin_Closed(object sender, EventArgs e)
        {
            //PopupWindow popupWindow = sender as PopupWindow;

            //if (popupwindow.mainframe.content != null 
            //    && popupwindow.mainframe.content is tradebookview)
            //{
            //    tradebookview tradebookview = popupwindow.mainframe.content as tradebookview;
            //    if (tradebookview != null && tradebookview.datacontext != null
            //        && tradebookview.datacontext is quotationlistviewmodel)
            //    {
            //        quotationlistviewmodel quotationlistviewmodel = tradebookview.datacontext as quotationlistviewmodel;
            //        quotationlistviewmodel.dispose();
            //        //quotationlistviewmodel = null;
            //    }
            //}
           
        }
    }
}
