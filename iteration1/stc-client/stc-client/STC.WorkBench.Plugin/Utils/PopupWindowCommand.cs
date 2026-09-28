using com.gateway.log;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Frame.CustomUI;
using STC.WorkBench.Plugin.View.WorkTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.WorkBench.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 报价（工作台）
        /// </summary>
        IntentionWorkTable,
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
                        if (windowManager.IsExist($"STC.WorkBench.Plugin-{PopupTypeEnum.IntentionWorkTable.ToString()}") == false)
                        {
                            BorderWin intentionWorkTableWin = new BorderWin();
                            intentionWorkTableWin.SetFixVisible(true);
                            intentionWorkTableWin.SetTitleAndKey(PopupTypeEnum.IntentionWorkTable.ToString(), "需求工作台", "STC.WorkBench.Plugin");
                            intentionWorkTableWin.SetCommandParameter($"{((int)PopupTypeEnum.IntentionWorkTable).ToString()}");

                            WorkTableView workTableView = new WorkTableView();
                            workTableView.MyOrderDefaultView.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                            intentionWorkTableWin.SetMainControl(workTableView);
                            intentionWorkTableWin.Width = 1340;
                            intentionWorkTableWin.Height = 500;
                            intentionWorkTableWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenWindow($"STC.WorkBench.Plugin-{PopupTypeEnum.IntentionWorkTable.ToString()}", intentionWorkTableWin);
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
