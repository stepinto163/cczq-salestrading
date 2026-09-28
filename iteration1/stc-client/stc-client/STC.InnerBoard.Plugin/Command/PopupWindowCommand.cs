using com.gateway.log;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Card;
using STC.Common.View.UI;
using STC.Common.ViewModel;
using STC.Frame.CustomUI;
using STC.InnerBoard.Plugin.View;
using STC.InnerBoard.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.InnerBoard.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 内部看板
        /// </summary>
        InnerBoard,
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

                    if (popupTypeEnum == PopupTypeEnum.InnerBoard)
                    {
                        if (args.Length > 1)
                        {
                            InnerBoardView control = args[1] as InnerBoardView;
                            if (control != null && control.DataContext != null && control.DataContext is InnerBoardViewModel)
                            {
                                InnerBoardViewModel vm = control.DataContext as InnerBoardViewModel;
                                if (windowManager.IsExist($"STC.InnerBoard.Plugin-{PopupTypeEnum.InnerBoard.ToString()}") == false)
                                {
                                    BorderWin innerBoardWin = new BorderWin();
                                    innerBoardWin.SetFixVisible(true);
                                    innerBoardWin.SetTitleAndKey(PopupTypeEnum.InnerBoard.ToString(), "内部看板", "STC.InnerBoard.Plugin");
                                    innerBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.InnerBoard).ToString()}");

                                    InnerBoardView innerBoardView = new InnerBoardView() { DataContext = vm };
                                    innerBoardView.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                                    innerBoardWin.SetMainControl(innerBoardView);
                                    innerBoardWin.Width = 1560;
                                    innerBoardWin.Height = 500;
                                    innerBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                    windowManager.OpenWindow($"STC.InnerBoard.Plugin-{PopupTypeEnum.InnerBoard.ToString()}", innerBoardWin);
                                }
                            }
                        }
                        else
                        {
                            if (windowManager.IsExist($"STC.InnerBoard.Plugin-{PopupTypeEnum.InnerBoard.ToString()}") == false)
                            {
                                BorderWin innerBoardWin = new BorderWin();
                                innerBoardWin.SetTitleAndKey(PopupTypeEnum.InnerBoard.ToString(), "内部看板", "STC.InnerBoard.Plugin");
                                innerBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.InnerBoard).ToString()}");

                                InnerBoardView innerBoardView = new InnerBoardView() { DataContext = new InnerBoardViewModel() };
                                innerBoardView.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                                innerBoardWin.SetMainControl(innerBoardView);
                                innerBoardWin.Width = 1560;
                                innerBoardWin.Height = 500;
                                innerBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                                windowManager.OpenOnlyOneWindow($"STC.InnerBoard.Plugin-{PopupTypeEnum.InnerBoard.ToString()}", innerBoardWin);
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
