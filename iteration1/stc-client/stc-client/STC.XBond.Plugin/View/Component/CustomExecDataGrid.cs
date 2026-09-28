using com.gateway.log;
using STC.Common.DataProcesser;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.Utils.DataProcesser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.XBond.Plugin.View.Component
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
            Dictionary<ExeKey, long> exeDic = new Dictionary<ExeKey, long>();
            foreach (DataRowView drv in dg.SelectedItems)
            {
                string securityId = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SECURITYID_FIELD]);
                string symbolName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SYMBOL_FIELD]);

                string px = CommonUtils.ConvertToString(drv[MyExecutionProcesser.LASTPX_FIELD]);
                //decimal d = 0;
                //if (decimal.TryParse(px, out d))
                //    px = string.Format("{0:N4}%", d);

                string qty = CommonUtils.ConvertToString(drv[MyExecutionProcesser.LASTQTY_FIELD]);
                string side = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SIDE_FIELD]);
                string partySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.PARTYSUBNAME_FIELD]);
                //string counterPartySubName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.COUNTERPARTYNAME_FIELD]);

                string settle = CommonUtils.ConvertToString(drv[MyExecutionProcesser.SETTLTYPE_FIELD]);

                settle = ComboSourceUtil.Instance.FindEnum(settle, "settletype");

                //string color = side == "2" ? "#58B3FF" : "#FB943C";

                //side = ComboSourceUtil.Instance.FindEnum(drv[MyExecutionProcesser.SIDE_FIELD] as string, "side");

                string institutionName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.INSTITUTIONNAME_FIELD], "--");
                string tradeName = CommonUtils.ConvertToString(drv[MyExecutionProcesser.TRADENAME_FIELD], "--");

                ExeKey key = new ExeKey()
                {
                    securityId = securityId,
                    symbolName = symbolName,
                    px = CommonUtils.ConvertToDecimal(px),
                    settle = settle,
                    side = side,
                    partySubName = partySubName,
                    InstitutionName = institutionName,
                    TradeName = tradeName
                };

                if (exeDic.ContainsKey(key))
                {
                    exeDic[key] += CommonUtils.ConvertToLong(qty);
                }
                else
                {
                    exeDic[key] = CommonUtils.ConvertToLong(qty);
                }
            }

            foreach(KeyValuePair<ExeKey, long> pair in exeDic)
            {
                if (html.Length > 0)
                {
                    html.Append("</br>");
                }

                string htmltxt = string.Empty;
                string plaintxt = string.Empty;
                if  (pair.Key.side == "1")
                {
                    htmltxt = $"&#9;{pair.Key.partySubName}&#9;to&#9;{pair.Key.InstitutionName}:{pair.Key.TradeName}";
                    plaintxt = $"\t{pair.Key.partySubName}\tto\t{pair.Key.InstitutionName}:{pair.Key.TradeName}";
                }
                else
                {
                    htmltxt = $"&#9;{pair.Key.InstitutionName}:{pair.Key.TradeName}&#9;to&#9;{pair.Key.partySubName}";
                    plaintxt = $"\t{pair.Key.InstitutionName}:{pair.Key.TradeName}\tto\t{pair.Key.partySubName}";
                }
                html.Append($"<p>&#9;{pair.Key.securityId}&#9;{pair.Key.symbolName}&#9;{string.Format("{0:N4}%", pair.Key.px)}&#9;{pair.Value}w{htmltxt}&#9;{pair.Key.settle}</p>");


                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                plainTxt.Append($"{pair.Key.securityId}\t{pair.Key.symbolName}\t{string.Format("{0:N4}%", pair.Key.px)}\t{pair.Value}w{plaintxt}\t{pair.Key.settle}");


            }
            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());
        }

        public struct ExeKey
        {
            internal string securityId;
            internal string symbolName;
            internal decimal px;
            internal string partySubName;
            internal string settle;
            internal string side;
            internal string InstitutionName;
            internal string TradeName;
        }
    }
}
