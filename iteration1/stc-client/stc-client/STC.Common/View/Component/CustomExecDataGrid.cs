using com.gateway.log;
using STC.Common.DataProcesser;
using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.Common.View.Component
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
                    if (grid.Name == "MyExecutions_dataGrid")
                    {
                        CopyOneData(grid);
                        //CopyData(grid);
                        return;
                    }
                    else if (grid.Name == "MyOrders_dataGrid")
                    {
                        CopyMyOrderOneData(grid);
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

        public void CopyMyOrderOneData(DataGrid dg)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();


            foreach (DataRowView drv in dg.SelectedItems)
            {
                
                string side = drv[MyOrderProcesser.SIDE_ORDER_FIELD] as string;
                string securityId = drv[MyOrderProcesser.SECURITYID_ORDER_FIELD] as string;
                string symbolName = drv[MyOrderProcesser.SYMBOL_ORDER_FIELD] as string;

                string px = string.Format("{0:N4}", drv[MyOrderProcesser.PRICE_ORDER_FIELD]) + "%";
                string qty = drv[MyOrderProcesser.QTY_ORDER_FIELD] + "w";
                //string partySubName = Constants.SHANXIZHENGQUAN;//drv[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string;
                string settle = ComboSourceUtil.Instance.FindEnum(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype_myorder");

                string color = side == "2" ? "#58B3FF" : "#FB943C";

                if (html.Length > 0)
                {
                    html.Append("</br>");
                }
                
                html.Append($"<p><span style=\"color: {color}\">{side}</span>&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{qty}&#9;{settle}</p>");
                side = ComboSourceUtil.Instance.FindEnum(drv[MyOrderProcesser.SIDE_ORDER_FIELD] as string, "side");
                
                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n\r");
                }

                plainTxt.Append($"{side}\t{securityId}\t{symbolName}\t{px}\t{qty}\t{settle}");
                
            }
            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

        }


        public void CopyOneData(DataGrid dg, bool reSetPosition = false)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();
            foreach (DataRowView drv in dg.SelectedItems)
            {


                string securityId = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SECURITYID_FIELD]);
                string symbolName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SYMBOL_FIELD]);

                string px = CommonUtils.ConvertToString(drv[MyExecutionProcesser.LASTPX_FIELD]);
                decimal d = 0;
                if (decimal.TryParse(px, out d))
                    px = string.Format("{0:N4}%", d);

                string qty = CommonUtils.ConvertToString(drv[MyExecutionProcesser.LASTQTY_FIELD]);

                string side = drv[MyExecutionProcesser.SIDE_FIELD] as string;
                string partySubName = string.Empty;
                string counterPartySubName = string.Empty;
                if (side.Equals("1"))
                {
                    partySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD], "--");
                    //counterPartySubName = Constants.SHANXIZHENGQUAN;//CommonUtils.ConvertToString(drv[MyExecutionProcesser.PARTYNAME_FIELD]);
                    counterPartySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.INSTITUTIONNAME_FIELD], "--");
                }
                else
                {
                    //partySubName = Constants.SHANXIZHENGQUAN;//CommonUtils.ConvertToString(drv[MyExecutionProcesser.PARTYNAME_FIELD]);
                    partySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.INSTITUTIONNAME_FIELD], "--");
                    counterPartySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD], "--");
                }



                //string partySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.PARTYNAME_FIELD]);
                //string counterPartySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]);

                string settle = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SETTLTYPE_FIELD]);

                settle = ComboSourceUtil.Instance.FindEnum(settle, "settletype");

                side = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SIDE_FIELD]);
                string color = side == "2" ? "#58B3FF" : "#FB943C";

                side = ComboSourceUtil.Instance.FindEnum(drv[MyExecutionProcesser.SIDE_FIELD] as string, "side");

                if (html.Length > 0)
                {
                    html.Append("</br>");
                }

                html.Append($"<p>&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{qty}w&#9;{partySubName}&#9;to&#9;{counterPartySubName}&#9;{settle}</p>");


                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n\r");
                }

                plainTxt.Append($"{securityId}\t{symbolName}\t{px}\t{qty}w\t{partySubName}\tto\t{counterPartySubName}\t{settle}");
            }

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());


        }

    }
}
