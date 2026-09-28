using BDB.Trade.Plugin.View;
using BDB.Trade.Plugin.View.Trade.Card;
using com.gateway.log;
using STC.Common.Utils;
using STC.Common.View.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BDB.Trade.Plugin.Command
{
    public enum PopupTypeEnum
    {
        /// <summary>
        /// 主页
        /// </summary>
        HomePage,

        /// <summary>
        /// 历史记录
        /// </summary>
        HistoryRecords,

        /// <summary>
        /// 深度行情
        /// </summary>
        TradeBook,

        /// <summary>
        /// 渠道执行
        /// </summary>
        VenuesExecution,

        /// <summary>
        /// 市场成交
        /// </summary>
        MarketExecution,

        /// <summary>
        /// 我的订单
        /// </summary>
        MyOrders,

        /// <summary>
        /// 我的成交
        /// </summary>
        MyExecutions,

        /// <summary>
        /// 我的已撤订单
        /// </summary>
        MyCancelOrders,

        /// <summary>
        /// 交易包管理
        /// </summary>
        BookManagement,
    }

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

                    if (popupTypeEnum == PopupTypeEnum.HomePage)
                    {
                        if (windowManager.IsExist($"BDB.Price.Plugin-{PopupTypeEnum.HomePage.ToString()}") == false)
                        {
                            BorderWin shellWin = new BorderWin();
                            PageCardHandler.Instance.AddPageCardItemView(shellWin.GetHashCode().ToString());
                            shellWin.Closed += ShellWin_Closed;
                            shellWin.SetFixVisible(true);
                            shellWin.SetTitleAndKey(PopupTypeEnum.HomePage.ToString(), "行情看板", "BDB.Price.Plugin");
                            shellWin.SetCommandParameter($"{((int)PopupTypeEnum.HomePage).ToString()}");

                            TradeMainView tradeMainView = new TradeMainView();
                            tradeMainView.btnPopup.Visibility = System.Windows.Visibility.Collapsed;
                            shellWin.SetMainControl(tradeMainView);
                            shellWin.Width = 1340;
                            shellWin.Height = 500;
                            shellWin.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
                            windowManager.OpenWindow($"BDB.Price.Plugin-{PopupTypeEnum.HomePage.ToString()}", shellWin);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                LogHelper.Error($"PopupWindowCommand {parameter}", e);
            }
        }

        private void ShellWin_Closed(object sender, EventArgs e)
        {
            BorderWin shellWin = sender as BorderWin;
            if (shellWin != null)
            {
                PageCardHandler.Instance.RemovePageCardItemView(shellWin.GetHashCode().ToString());
            }
        }
    }
}
