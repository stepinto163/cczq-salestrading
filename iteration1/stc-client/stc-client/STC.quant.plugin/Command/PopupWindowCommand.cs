using com.gateway.log;
using STC.Common.Utils;
using STC.Common.View.UI;
using STC.Frame.CustomUI;
using STC.quant.plugin.Utils;
using STC.quant.plugin.View;
using STC.quant.plugin.View.Component;
using System;
using System.Windows.Input;

namespace STC.quant.plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 策略控制台
        /// </summary>
        Strategy

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
                    if (popupTypeEnum == PopupTypeEnum.Strategy)
                    {
                        if (windowManager.IsExist(PopupTypeEnum.Strategy.ToString()) == false)
                        {
                            BorderWin myTradingPopupWin = new BorderWin();
                            myTradingPopupWin.SetTitleAndKey(PopupTypeEnum.Strategy.ToString(), "策略控制台", "STC.quant.plugin");
                            myTradingPopupWin.SetCommandParameter($"{((int)PopupTypeEnum.Strategy).ToString()}");

                          
                            MainView executeOrderView = new MainView();
                            executeOrderView.setButtonVisibility(System.Windows.Visibility.Collapsed);
                            myTradingPopupWin.SetMainControl(executeOrderView);
                            myTradingPopupWin.Width = 1340;
                            myTradingPopupWin.Height = 500;
                            myTradingPopupWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenOnlyOneWindow( $"STC.quant.plugin-{PopupTypeEnum.Strategy.ToString()}", myTradingPopupWin);
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
