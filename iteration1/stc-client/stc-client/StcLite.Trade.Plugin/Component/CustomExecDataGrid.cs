using com.gateway.log;
using BDB.Trade.Plugin.Utils;
using BDB.Trade.Plugin.Handler.DataProcesser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using STC.Common.Utils;
using ClipboardHelper = BDB.Trade.Plugin.Utils.ClipboardHelper;

namespace BDB.Trade.Plugin.Component
{
    public class CustomExecDataGrid : DataGrid
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
                    else if (grid.Name == "MyCancels_dataGrid")
                    {
                        CopyMyCancelOrderOneData(grid);
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
                //string partySubName = CommonConstants.SHANXIZHENGQUAN;//drv[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string;
                string settle = STC.Common.Utils.ComboSourceUtil.Instance.FindEnum(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype_myorder");

                string color = side == "2" ? "#58B3FF" : "#FB943C";

                string broker = STC.Common.Utils.ComboSourceUtil.Instance.FindEnum(drv[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venuesorder");
                string STCCode = drv[MyOrderProcesser.STCCODE_ORDER_FIELD] as string;
                if (html.Length > 0)
                {
                    html.Append("</br>");
                }
                
                html.Append($"<p><span style=\"color: {color}\">{side}</span>&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{qty}&#9;{settle}</p>");
                side = STC.Common.Utils.ComboSourceUtil.Instance.FindEnum(drv[MyOrderProcesser.SIDE_ORDER_FIELD] as string, "side");
                
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
                    partySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]);
                    counterPartySubName = STC.Common.Utils.CommonConstants.SHANXIZHENGQUAN;//CommonUtils.ConvertToString(drv[MyExecutionProcesser.PARTYNAME_FIELD]);
                }
                else
                {
                    partySubName = STC.Common.Utils.CommonConstants.SHANXIZHENGQUAN;//CommonUtils.ConvertToString(drv[MyExecutionProcesser.PARTYNAME_FIELD]);
                    counterPartySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]);
                }



                //string partySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.PARTYNAME_FIELD]);
                //string counterPartySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]);

                string settle = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SETTLTYPE_FIELD]);

                settle = ComboSourceUtil.Instance.FindEnum(settle, "settletype");

                side = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SIDE_FIELD]);
                string color = side == "2" ? "#58B3FF" : "#FB943C";

                side = ComboSourceUtil.Instance.FindEnum(drv[MyExecutionProcesser.SIDE_FIELD] as string, "side");


                string broker = ComboSourceUtil.Instance.FindEnum(drv[MyExecutionProcesser.VENUES_FIELD] as string, "venuesorder") ;
                string STCCode = drv[MyExecutionProcesser.STRATEGY_ORDER_ID_FIELD] as string;
                string execid = drv[MyExecutionProcesser.EXECID_FIELD] as string;
                if (html.Length > 0)
                {
                    html.Append("</br>");
                }

                html.Append($"<p>&#9;{side}&#9;{securityId}&#9;{symbolName}&#9;{px}&#9;{qty}w&#9;{settle}&#9;{broker}&#9;{execid}</p>");


                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n\r");
                }

                plainTxt.Append($"{side}\t{securityId}\t{symbolName}\t{px}\t{qty}w\t{settle}\t{broker}\t{execid}");
            }

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());
        }

        public void CopyMyCancelOrderOneData(DataGrid dg)
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
                //string partySubName = CommonConstants.SHANXIZHENGQUAN;//drv[MyOrderProcesser.MYDEALER_ORDER_FIELD] as string;
                string settle = ComboSourceUtil.Instance.FindEnum(drv[MyOrderProcesser.SETTLETYPE_ORDER_FIELD] as string, "settletype_myorder");

                string color = side == "2" ? "#58B3FF" : "#FB943C";

                /*string broker = ComboSourceUtil.Instance.FindEnum(drv[MyOrderProcesser.VENEUES_ORDER_FIELD] as string, "venuesorder");
                string STCCode = drv[MyOrderProcesser.STCCODE_ORDER_FIELD] as string;*/
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

    }
}
