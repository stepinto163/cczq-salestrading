using com.gateway.log;
using STC.Common.View.Component;
using STC.Execute.Plugin.Handler.Process;
using STC.Execute.Plugin.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.Execute.Plugin.View.Component
{
    public class CustomExecDataGrid : ExDataGrid
    {
        protected override void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            try
            {
                if (args != null)
                {
                    var grid = args.Source as DataGrid;
                    //if (grid.Name == "MyExecutions_dataGrid")
                    //{
                    //    CopyOneData(grid);
                    //    //CopyData(grid);
                    //    return;
                    //}
                    //else if (grid.Name == "MyOrders_dataGrid")
                    {
                        CopyOrderData(grid);
                        //CopyData(grid);
                        return;
                    }

                    base.OnExecutedCopy(args);
                    return;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("CustomExecDataGrid OnExecutedCopy", ex);
            }
        }

        public void CopyOrderData(DataGrid dg)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();


            foreach (DataRowView drv in dg.SelectedItems)
            {

                string side = drv[ExecOrderProcesser.Side] + "";
                string securityId = drv[ExecOrderProcesser.SecurityID] + "";
                string symbolName = drv[ExecOrderProcesser.Symbol] + "";

                string qty = drv[ExecOrderProcesser.AllocQty] + "w";
                //string partySubName = Constants.SHANXIZHENGQUAN;//drv[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string;
                string trade= drv[ExecOrderProcesser.TradeDate] + "";
                string settle = ComboSourceUtil.Instance.FindEnum(drv[ExecOrderProcesser.SettlType] + "", "settletype_execute");
                settle = ExecOrderProcesser.Instance.getSettleType(trade, settle);
                string tradeType = ComboSourceUtil.Instance.FindEnum(drv[ExecOrderProcesser.TrdType] + "", "trdType");
                string color = side == "2" ? "#58B3FF" : "#FB943C";
                string partyName = drv[ExecOrderProcesser.PartyName] + "";
                string partySubName = drv[ExecOrderProcesser.PartySubName] + "";
                string counterPartyName = drv[ExecOrderProcesser.CounterPartyName] + "";
                string counterPartySubName = drv[ExecOrderProcesser.CounterPartySubName] + "";
                string paymentFixedRate = drv[ExecOrderProcesser.PaymentFixedRate] + "";
                if (!string.IsNullOrEmpty(paymentFixedRate))
                    paymentFixedRate = string.Format("F{0}", paymentFixedRate);
                string transType = drv[ExecOrderProcesser.TransactionPriceType] + "";
                string px="";
                if (!string.IsNullOrEmpty(transType))
                {
                    switch (transType)
                    {
                        case "到期":
                            px = string.Format("{0:N4}", drv[ExecOrderProcesser.AvgPx]) + "%";
                            break;
                        case "行权":
                            px = string.Format("{0:N4}", drv[ExecOrderProcesser.StipulationValue]) + "%";
                            break;
                        case "净价":
                            px = drv[ExecOrderProcesser.CleanPrice] + "";
                            break;
                        case "全价":
                            px = drv[ExecOrderProcesser.DirtyPrice] + "";
                            break;
                    }
                }
                string endTxt = string.Empty;
                if (side == "1")
                {
                    endTxt = $"{counterPartyName}{counterPartySubName}\t出给\t{partyName}{partySubName}";
                }
                else
                    endTxt = $"{partyName}{partySubName}\t出给\t{counterPartyName}{counterPartySubName}";


                if (html.Length > 0)
                {
                    html.Append("</br>");
                }

                html.Append($"<p>{securityId}&#9;{symbolName}&#9;{px}&#9;{transType}&#9;{qty}&#9;&#9;{paymentFixedRate}&#9;{tradeType}&#9;{endTxt}</p>");
                //side = ComboSourceUtil.Instance.FindEnum(drv[ExecOrderProcesser.Side] as string, "side");

                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n\r");
                }

                
                plainTxt.Append($"{securityId}\t{symbolName}\t{px}\t{transType}\t{qty}\t{paymentFixedRate}\t{settle}\t{tradeType}\t{endTxt}");

            }
            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

        }


    }
}
