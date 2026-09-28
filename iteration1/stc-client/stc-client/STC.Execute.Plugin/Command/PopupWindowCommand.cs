using com.gateway.log;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Execute.Plugin.View;
using STC.Execute.Plugin.View.Execute;
using System;
using System.Windows.Input;

namespace STC.Execute.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 成交确认
        /// </summary>
        ExecutionConfirm
        
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
                    
                    
                }
                else if (parameter is PopupTypeEnum)
                {
                    PopupTypeEnum popupTypeEnum = (PopupTypeEnum)parameter;
                    if (popupTypeEnum == PopupTypeEnum.ExecutionConfirm)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.ExecutionConfirm.ToString()) == false)
                        {
                            BorderWin myTradingPopupWin = new BorderWin();
                            myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.ExecutionConfirm.ToString(), "经纪商成交", "STC.Execute.Plugin");
                            myTradingPopupWin.SetCommandParameter($"{((int)PopupTypeEnum.ExecutionConfirm).ToString()}");

                            ExecuteOrderView executeOrderView = new ExecuteOrderView();
                            executeOrderView.detailView.setButtonVisibility(System.Windows.Visibility.Collapsed);
                            myTradingPopupWin.SetMainControl(executeOrderView);
                            myTradingPopupWin.Width = 1340;
                            myTradingPopupWin.Height = 500;
                            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow($"STC.Execute.Plugin-{PopupTypeEnum.ExecutionConfirm.ToString()}", myTradingPopupWin);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                LogHelper.Error($"PopupWindowCommand {parameter}",e);
            }
        }
      
    }
}
