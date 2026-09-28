using com.gateway.log;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.MyTrading;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Frame.CustomUI;
using STC.WorkTable.Plugin.View.WorkTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.WorkTable.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 报价（工作台）
        /// </summary>
        IntentionWorkTable,

        /// <summary>
        /// 需求列表
        /// </summary>
        IntentionList,

        /// <summary>
        /// 历史需求
        /// </summary>
        HistoryIntentionList,

        /// <summary>
        /// 我的订单
        /// </summary>
        MyOrders,

        /// <summary>
        /// 我的成交
        /// </summary>
        MyExecutions,
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
                if (parameter is PopupTypeEnum)
                {
                    PopupTypeEnum popupTypeEnum = (PopupTypeEnum)parameter;

                    if (popupTypeEnum == PopupTypeEnum.IntentionWorkTable)
                    {
                        if (windowManager.IsExist($"STC.WorkTable.Plugin-{PopupTypeEnum.IntentionWorkTable.ToString()}") == false)
                        {
                            BorderWin intentionWorkTableWin = new BorderWin();
                            intentionWorkTableWin.SetTitleAndKey(PopupTypeEnum.IntentionWorkTable.ToString(), "意向报价", "STC.WorkTable.Plugin");
                            intentionWorkTableWin.SetCommandParameter($"{((int)PopupTypeEnum.IntentionWorkTable).ToString()}");

                            MyOrderDefaultView myOrderDefaultView = new MyOrderDefaultView();
                            myOrderDefaultView.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                            intentionWorkTableWin.SetMainControl(myOrderDefaultView);
                            intentionWorkTableWin.Width = 1340;
                            intentionWorkTableWin.Height = 500;
                            intentionWorkTableWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.WorkTable.Plugin-{PopupTypeEnum.IntentionWorkTable.ToString()}", intentionWorkTableWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.MyOrders)
                    {
                        if (windowManager.IsExist($"STC.WorkTable.Plugin-{PopupTypeEnum.MyOrders.ToString()}") == false)
                        {
                            BorderWin myTradingPopupWin = new BorderWin();
                            myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.MyOrders.ToString(), "我的订单", "STC.WorkTable.Plugin");
                            myTradingPopupWin.SetCommandParameter($"{((int)PopupTypeEnum.MyOrders).ToString()}");

                            MyOrders myTrading = new MyOrders();
                            myTradingPopupWin.SetMainControl(myTrading);
                            myTradingPopupWin.Width = 1340;
                            myTradingPopupWin.Height = 500;
                            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.WorkTable.Plugin-{PopupTypeEnum.MyOrders.ToString()}", myTradingPopupWin);
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.MyExecutions)
                    {
                        if (windowManager.IsExist($"STC.WorkTable.Plugin-{PopupTypeEnum.MyExecutions.ToString()}") == false)
                        {
                            BorderWin myTradingPopupWin = new BorderWin();
                            myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.MyExecutions.ToString(), "我的成交", "STC.WorkTable.Plugin");
                            myTradingPopupWin.SetCommandParameter($"{((int)PopupTypeEnum.MyExecutions).ToString()}");
                            MyExes myTrading = new MyExes();
                            myTradingPopupWin.SetMainControl(myTrading);
                            myTradingPopupWin.Width = 1340;
                            myTradingPopupWin.Height = 500;
                            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.WorkTable.Plugin-{PopupTypeEnum.MyExecutions.ToString()}", myTradingPopupWin);
                        }
                    }
                }
                else if (parameter is object[])
                {
                    object[] args = parameter as object[];
                    PopupTypeEnum popupTypeEnum = (PopupTypeEnum)args[0];
                    if (popupTypeEnum == PopupTypeEnum.IntentionList)
                    {
                        if (args[1].GetType() == typeof(string))
                        {
                            ViewModel.WorkTable.MainViewModel mainViewModel = new ViewModel.WorkTable.MainViewModel();
                            if (windowManager.IsExist($"STC.WorkTable.Plugin-{PopupTypeEnum.IntentionList.ToString()}") == false)
                            {
                                BorderWin intentionListWin = new BorderWin();
                                intentionListWin.SetTitleAndKey(PopupTypeEnum.IntentionList.ToString(), "需求列表", "STC.WorkTable.Plugin");
                                intentionListWin.SetCommandParameter($"{((int)PopupTypeEnum.IntentionList).ToString()},1");

                                IntentionList intentionList = new IntentionList();
                                intentionList.DataContext = mainViewModel;
                                intentionList.popupWindowBtn.Visibility = System.Windows.Visibility.Collapsed;
                                intentionListWin.SetMainControl(intentionList);
                                intentionListWin.Width = 1340;
                                intentionListWin.Height = 500;
                                intentionListWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                windowManager.OpenOnlyOneWindow($"STC.WorkTable.Plugin-{PopupTypeEnum.IntentionList.ToString()}", intentionListWin);
                            }
                        }
                        else
                        {
                            IntentionList control = args[1] as IntentionList;
                            if (windowManager.IsExist($"STC.WorkTable.Plugin-{PopupTypeEnum.IntentionList.ToString()}") == false)
                            {
                                BorderWin intentionListWin = new BorderWin();
                                intentionListWin.SetTitleAndKey(PopupTypeEnum.IntentionList.ToString(), "需求列表", "STC.WorkTable.Plugin");
                                intentionListWin.SetCommandParameter($"{((int)PopupTypeEnum.IntentionList).ToString()},1");

                                IntentionList intentionList = new IntentionList();
                                intentionList.DataContext = control.DataContext;
                                intentionList.popupWindowBtn.Visibility = System.Windows.Visibility.Collapsed;
                                intentionListWin.SetMainControl(intentionList);
                                intentionListWin.Width = 1340;
                                intentionListWin.Height = 500;
                                intentionListWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                windowManager.OpenOnlyOneWindow($"STC.WorkTable.Plugin-{PopupTypeEnum.IntentionList.ToString()}", intentionListWin);

                            }
                        }
                    }
                    else if (popupTypeEnum == PopupTypeEnum.HistoryIntentionList)
                    {
                        if (args[1].GetType() == typeof(string))
                        {
                            ViewModel.WorkTable.MainViewModel mainViewModel = new ViewModel.WorkTable.MainViewModel();
                            if (windowManager.IsExist($"STC.WorkTable.Plugin-{PopupTypeEnum.HistoryIntentionList.ToString()}") == false)
                            {
                                BorderWin historyIntentionListWin = new BorderWin();
                                historyIntentionListWin.SetTitleAndKey(PopupTypeEnum.HistoryIntentionList.ToString(), "历史需求", "STC.WorkTable.Plugin");
                                historyIntentionListWin.SetCommandParameter($"{((int)PopupTypeEnum.HistoryIntentionList).ToString()},1");

                                HistoryIntentionList historyIntentionList = new HistoryIntentionList();
                                historyIntentionList.DataContext = mainViewModel;
                                historyIntentionList.popupWindowBtn.Visibility = System.Windows.Visibility.Collapsed;
                                historyIntentionListWin.SetMainControl(historyIntentionList);
                                historyIntentionListWin.Width = 1340;
                                historyIntentionListWin.Height = 500;
                                historyIntentionListWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                windowManager.OpenOnlyOneWindow($"STC.WorkTable.Plugin-{PopupTypeEnum.HistoryIntentionList.ToString()}", historyIntentionListWin);
                            }
                        }
                        else
                        {
                            HistoryIntentionList control = args[1] as HistoryIntentionList;
                            if (windowManager.IsExist($"STC.WorkTable.Plugin-{PopupTypeEnum.HistoryIntentionList.ToString()}") == false)
                            {

                                BorderWin historyIntentionListWin = new BorderWin();
                                historyIntentionListWin.SetTitleAndKey(PopupTypeEnum.HistoryIntentionList.ToString(), "历史需求", "STC.WorkTable.Plugin");
                                historyIntentionListWin.SetCommandParameter($"{((int)PopupTypeEnum.HistoryIntentionList).ToString()},1");

                                HistoryIntentionList historyIntentionList = new HistoryIntentionList();
                                historyIntentionList.DataContext = control.DataContext;
                                historyIntentionList.popupWindowBtn.Visibility = System.Windows.Visibility.Collapsed;
                                historyIntentionListWin.SetMainControl(historyIntentionList);
                                historyIntentionListWin.Width = 1340;
                                historyIntentionListWin.Height = 500;
                                historyIntentionListWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                windowManager.OpenOnlyOneWindow($"STC.WorkTable.Plugin-{PopupTypeEnum.HistoryIntentionList.ToString()}", historyIntentionListWin);

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error($"PopupWindowCommand {parameter}", e);
            }
        }
    }
}
