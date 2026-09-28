using com.gateway.log;
using STC.Common.Model.Admin;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View;
using STC.Common.View.Card;
using STC.Common.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using STC.Common.View.UI;
using STC.Common.ViewModel.Card;

namespace STC.Common.Command
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

        /// <summary>
        /// 行情小窗
        /// </summary>
        QuotesMiniWindows,
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
                    if (popupTypeEnum == PopupTypeEnum.QuotesMiniWindows)
                    {
                        string securtiyID = (string)args[1];
                        if (windowManager.IsExist($"{PopupTypeEnum.TradeBook.ToString()}-{securtiyID}") == false)
                        {
                            PopupNoBorder myTradingPopupWin = new PopupNoBorder();
                            myTradingPopupWin.Title = securtiyID;

                            PriceAndOrderView tradeBookView = new PriceAndOrderView();
                            PriceAndOrderViewModel viewModel = new PriceAndOrderViewModel();
                            if (securtiyID != null)
                            {
                                viewModel.SecurityID = securtiyID;
                                //BondsInfoPo bondsInfoPo = CommonUtils.GetBondsInfoPoBySecurityID(securtiyID);
                                //if (bondsInfoPo != null)
                                //{
                                //    viewModel.Symbol = bondsInfoPo.Short_Name;
                                //}
                            }

                            tradeBookView.DataContext = viewModel;
                            myTradingPopupWin.SetMainControl(tradeBookView);
                            myTradingPopupWin.Width = 290;
                            myTradingPopupWin.Height = 460;
                            windowManager.OpenOnlyOneWindow($"{PopupTypeEnum.QuotesMiniWindows.ToString()}-{securtiyID}", myTradingPopupWin);

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
