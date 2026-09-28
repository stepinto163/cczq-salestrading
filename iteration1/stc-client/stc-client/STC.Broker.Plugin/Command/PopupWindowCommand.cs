using com.gateway.log;
using STC.Broker.Plugin.View.Transaction;
using STC.Broker.Plugin.ViewModel.Transaction;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace STC.Broker.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 交易处理
        /// </summary>
        TransactionPanel,
        /// <summary>
        /// 明细,过桥
        /// </summary>
        CopyDetailUI,
        /// <summary>
        /// 明细,过桥
        /// </summary>
        BridgeWin,
        /// <summary>
        /// 线下成交
        /// </summary>
        UnderLineExecWin,
        /// <summary>
        /// 已删除成交单
        /// </summary>
        DeleteOrderWin
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
                if (parameter is object[]|| parameter is object)
                {
                    PopupTypeEnum popupTypeEnum = PopupTypeEnum.TransactionPanel;
                    if (parameter is object[])
                    {
                        object[] args = parameter as object[];
                        popupTypeEnum = (PopupTypeEnum)args[0];
                    }
                    else {
                        popupTypeEnum = (PopupTypeEnum)parameter;
                    }
                   

                    if (popupTypeEnum == PopupTypeEnum.TransactionPanel)
                    {
                        BorderWin innerBoardWin = new BorderWin();
                        innerBoardWin.SetTitleAndKey(PopupTypeEnum.TransactionPanel.ToString(), "交易处理", "Transaction");
                        innerBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.TransactionPanel).ToString()}");

                        TransactionPanel transactionPanel = new TransactionPanel();
                        transactionPanel.DataContext = new TransactionPanelViewModel(transactionPanel);
                        (transactionPanel.DataContext as TransactionPanelViewModel).Init(transactionPanel);
                        //innerBoardView.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                        innerBoardWin.SetMainControl(transactionPanel);
                        innerBoardWin.Width = 1560;
                        innerBoardWin.Height = 500;
                        
                        innerBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        windowManager.OpenOnlyOneWindow($"Transaction-{PopupTypeEnum.TransactionPanel.ToString()}", innerBoardWin);
                        innerBoardWin.SizeChanged += InnerBoardWin_SizeChanged;
                        //if (args.Length > 1)
                        //{
                        //    TransactionPanel control = args[1] as TransactionPanel;
                        //    if (control != null && control.DataContext != null && control.DataContext is TransactionPanel)
                        //    {
                        //        TransactionPanelViewModel vm = control.DataContext as TransactionPanelViewModel;
                        //        if (windowManager.IsExist($"STC.Broker.Plugin-{PopupTypeEnum.TransactionPanel.ToString()}") == false)
                        //        {
                        //            BorderWin innerBoardWin = new BorderWin();
                        //            innerBoardWin.SetTitleAndKey(PopupTypeEnum.TransactionPanel.ToString(), "交易处理", "STC.Broker.Plugin");
                        //            innerBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.TransactionPanel).ToString()}");

                        //            TransactionPanel transactionPanel = new TransactionPanel() { DataContext = vm };
                        //            //transactionPanel.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                        //            innerBoardWin.SetMainControl(transactionPanel);
                        //            innerBoardWin.Width = 1560;
                        //            innerBoardWin.Height = 500;
                        //            innerBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        //            windowManager.OpenOnlyOneWindow($"STC.Broker.Plugin-{PopupTypeEnum.TransactionPanel.ToString()}", innerBoardWin);
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    if (windowManager.IsExist($"STC.Broker.Plugin-{PopupTypeEnum.TransactionPanel.ToString()}") == false)
                        //    {
                        //        BorderWin innerBoardWin = new BorderWin();
                        //        innerBoardWin.SetTitleAndKey(PopupTypeEnum.TransactionPanel.ToString(), "内部看板", "STC.InnerBoard.Plugin");
                        //        innerBoardWin.SetCommandParameter($"{((int)PopupTypeEnum.TransactionPanel).ToString()}");

                        //        TransactionPanel transactionPanel = new TransactionPanel();
                        //        transactionPanel.DataContext = new TransactionPanelViewModel(transactionPanel);
                        //        //innerBoardView.btnFullScreen.Visibility = System.Windows.Visibility.Collapsed;
                        //        innerBoardWin.SetMainControl(transactionPanel);
                        //        innerBoardWin.Width = 1560;
                        //        innerBoardWin.Height = 500;
                        //        innerBoardWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                        //        windowManager.OpenOnlyOneWindow($"STC.Broker.Plugin-{PopupTypeEnum.TransactionPanel.ToString()}", innerBoardWin);
                        //    }
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Error($"PopupWindowCommand {parameter}", e);
            }
        }
        /// <summary>
        /// 大小变化事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void InnerBoardWin_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            BorderWin innerBoardWin = sender as BorderWin;
            if (innerBoardWin!=null) {
                TransactionPanel transactionPanel = innerBoardWin.GetMainControl() as TransactionPanel;
                if (transactionPanel!=null) {

                    transactionPanel.Width = innerBoardWin.ActualWidth - 10;
                }
            }
        }
    }
}
