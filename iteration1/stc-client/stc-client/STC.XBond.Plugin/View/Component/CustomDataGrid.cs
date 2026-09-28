using Ats.Foundation.Message;
using com.gateway.log;
using STC.Common.Model.TBook;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.XBond.Plugin.Utils;
using STC.XBond.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.XBond.Plugin.View.Component
{
    /// <summary>
    /// CustomDataGrid.
    /// </summary>
    public class CustomDataGrid : ExDataGrid
    {
        /// <inheritdoc/>
        protected override void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            try
            {
                if (args != null)
                {
                    var grid = args.Source as DataGrid;
                    if (grid.Name == "dg_VenuesExecution")
                    {
                        CopyOneData(grid);
                        //CopyData(grid);
                        return;
                    }

                    base.OnExecutedCopy(args);
                    return;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("OnExecutedCopy", ex);
            }
        }


        public void CopyDataByStatus(DataView dataView)
        {
            DataTable dt = dataView.ToTable();
           
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();
            DataView newDataView = new DataView(dt);

            newDataView.RowFilter = $"{VenuesExecutionViewModel.STATUS_FIELD} = '处理中' or " + $"{VenuesExecutionViewModel.STATUS_FIELD} = '撤销中' or " + $"{VenuesExecutionViewModel.STATUS_FIELD} = '待处理' or  " + $"{VenuesExecutionViewModel.STATUS_FIELD} = '部分成交'";
            foreach (DataRow drv in dt.Rows)
            {
                string status = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.STATUS_FIELD]);
                //待处理:0, 撤销中:7, 处理中:3, 部分成交:1
                if (!(!string.IsNullOrEmpty(status) && (status == "处理中" || status == "撤销中" || status == "待处理" || status == "部分成交")))
                    continue;

                string broker = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.VENUES_FIELD]);
                broker = ComboSourceUtil.Instance.FindEnum(broker, "venues");
                string side = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SIDE_FIELD]);
                string no = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.NO_FIELD]);
                string color = side == "2" ? "#58B3FF" : "#FB943C";

                drv["SideBrush"] = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(color));

                side = ComboSourceUtil.Instance.FindEnum(side, "side");
                string securityID = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SECURITYID_FIELD]);
                string firmFlag = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.FIRMFLAG_FIELD]);
                firmFlag = ComboSourceUtil.Instance.FindEnum(firmFlag, "firmflag");

                string price = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.PX_FIELD]);
                decimal d = 0;
                decimal.TryParse(price, out d);


                string px = d.ToString("N4") + "%";
                string qty = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.QTY_FIELD]);
                string settlType = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SETTLTYPE_FIELD]);
                string info3 = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.INFO3]);
                info3 = GetBondOrderInfo(info3);
                drv[VenuesExecutionViewModel.BOND_INFO] = info3;
                settlType = ComboSourceUtil.Instance.FindEnum(settlType, "settletype");
                if (html.Length > 0)
                {
                    html.Append("</br>");
                }

                if (string.IsNullOrEmpty(info3))
                    html.Append($"<p><span style=\"color: {color}\">{side}</span>&#9;{securityID}&#9;{firmFlag}&#9;{px}&#9;{qty}&#9;{settlType}&#9;（{broker}&#9;{no}）</p>");
                else
                    html.Append($"<p><span style=\"color: {color}\">{side}</span>&#9;{securityID}&#9;{firmFlag}&#9;{px}&#9;{qty}&#9;{settlType}&#9;（{broker}&#9;{no}）&#9;{info3}</p>");

                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n\r");
                }

                if (string.IsNullOrEmpty(info3))
                    plainTxt.Append($"{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}\t（{broker}\t{no}）");
                else
                    plainTxt.Append($"{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}\t（{broker}\t{no}）\t{info3}");
            }

            string htmlStr = html.ToString();
            string plainStr = plainTxt.ToString();

            if (!string.IsNullOrEmpty(htmlStr) && !string.IsNullOrEmpty(plainStr))
            {
                //复制
                ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                //展示
                VenuesExecutionCopyWindow copyWin = new VenuesExecutionCopyWindow();
                copyWin.copyList.ItemsSource = newDataView;//dg.SelectedItems;
                copyWin.Show();
            }

        }

        /// <summary>
        /// 渠道执行 复制功能,带状态过滤
        /// </summary>
        /// <param name="dg"></param>
        public void CopyData(DataGrid dg)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();
            DataRow dr = dg.CurrentItem as DataRow;

            StringBuilder sbKey = new StringBuilder();
            foreach (DataRowView drv in dg.SelectedItems)
            {
                string status = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.STATUS_FIELD]);
                //处理中:0, 撤销中:7, 处理中:3, 部分成交:1
                if (!(!string.IsNullOrEmpty(status) && (status == "处理中" || status == "撤销中" || status == "待处理" || status == "部分成交")))
                    continue;

                if (sbKey.Length > 0)
                {
                    sbKey.Append(" or ");
                }
                sbKey.Append($"{VenuesExecutionViewModel.ID} = '{drv[VenuesExecutionViewModel.ID]}'");

                string side = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SIDE_FIELD]);
                string color = side == "2" ? "#58B3FF" : "#FB943C";
                side = ComboSourceUtil.Instance.FindEnum(side, "side");

                drv["SideBrush"] = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
                
                string securityID = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SECURITYID_FIELD]);
                string firmFlag = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.FIRMFLAG_FIELD]);
                firmFlag = ComboSourceUtil.Instance.FindEnum(firmFlag, "firmflag");
                string px = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.PX_FIELD]) + "%";
                string qty = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.QTY_FIELD]);
                string settlType = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SETTLTYPE_FIELD]);
                string info3 = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.INFO3]);
                info3 = GetBondOrderInfo(info3);
                drv[VenuesExecutionViewModel.BOND_INFO] = info3;
                settlType = ComboSourceUtil.Instance.FindEnum(settlType, "settletype");
                if (html.Length > 0)
                {
                    html.Append("</br>");
                }

                if (string.IsNullOrEmpty(info3))
                    html.Append($"<p><span style=\"color: {color}\">{side}</span>&#9;{securityID}&#9;{firmFlag}&#9;{px}&#9;{qty}&#9;{settlType}</p>");
                else
                    html.Append($"<p><span style=\"color: {color}\">{side}</span>&#9;{securityID}&#9;{firmFlag}&#9;{px}&#9;{qty}&#9;{settlType}&#9;{info3}</p>");

                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n\r");
                }

                if (string.IsNullOrEmpty(info3))
                    plainTxt.Append($"{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}");
                else
                    plainTxt.Append($"{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}\t{info3}");
                
            }

            string htmlStr = html.ToString();
            string plainStr = plainTxt.ToString();

            if (!string.IsNullOrEmpty(htmlStr) && !string.IsNullOrEmpty(plainStr))
            {
                DataView newDataView = new DataView((dg.ItemsSource as DataView).ToTable());

                StringBuilder sbFilter = new StringBuilder();
                if (sbKey.Length > 0)
                {
                    sbFilter.Append($"({sbKey})");
                }

                sbFilter.Append(" and ");
                sbFilter.Append($"({VenuesExecutionViewModel.STATUS_FIELD} = '处理中' or " + $"{VenuesExecutionViewModel.STATUS_FIELD} = '撤销中' or " 
                    + $"{VenuesExecutionViewModel.STATUS_FIELD} = '待处理' or  " + $"{VenuesExecutionViewModel.STATUS_FIELD} = '部分成交')");

                newDataView.RowFilter = sbFilter.ToString();

                //复制
                ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

                //展示
                VenuesExecutionCopyWindow copyWin = new VenuesExecutionCopyWindow();
                copyWin.copyList.ItemsSource = newDataView;//dg.SelectedItems;
                copyWin.Show();
            }
        }

        private string GetBondOrderInfo(string info3)
        {
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;
            string str = "{0},";
            if (!string.IsNullOrEmpty(info3))
            {
                try
                {
                    TBondOrder bondOrder = MsgHelper.Deserialize<TBondOrder>(info3);
                    if (!string.IsNullOrEmpty(bondOrder.BargainFlag))
                    {
                        sb.Append(bondOrder.BargainFlag.Equals("1") ? "*," : bondOrder.BargainFlag.Equals("2") ? "**," : string.Empty);

                    }
                    //if (!string.IsNullOrEmpty(bondOrder.ClearSpeed))
                    //    sb.Append(string.Format(str, bondOrder.ClearSpeed));
                    ////if (!string.IsNullOrEmpty(bondOrder.OcoFlag))
                    ////    sb.Append(string.Format(str, bondOrder.OcoFlag));
                    //if (!string.IsNullOrEmpty(bondOrder.Text))
                    //{
                    //    if (string.IsNullOrEmpty(bondOrder.ClearSpeed) || !bondOrder.Text.Equals(bondOrder.ClearSpeed))
                    //        sb.Append(string.Format(str, bondOrder.Text));
                    //}
                    //if (!string.IsNullOrEmpty(bondOrder.WholeFlag))
                    //    sb.Append(string.Format(str, bondOrder.WholeFlag.Equals("1")?"整量":string.Empty));
                    if (!string.IsNullOrEmpty(bondOrder.OcoFlag))
                        sb.Append(string.Format(str, bondOrder.OcoFlag.Equals("2")?"打包": bondOrder.OcoFlag.Equals("1")?"OCO":string.Empty));

                    result = sb.ToString();

                    if (result.Length > 0)
                        result = result.Substring(0, result.Length - 1);
                    
                    result = result.Length > 0 ? "(" + result + ")" : string.Empty;
                }
                catch (Exception e)
                {
                    LogHelper.Error(e);
                }
            }
            return result;
        }

        /// <summary>
        /// 渠道执行 复制功能
        /// </summary>
        /// <param name="dg"></param>
        public void CopyOneData(DataGrid dg, bool reSetPosition = false)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder plainTxt = new StringBuilder();
            DataRowView drv = dg.CurrentItem as DataRowView;

            string status = CommonMapUtil.FindEnum(CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.STATUS_FIELD]), "venuestatus"); // 订单状态

            string broker = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.VENUES_FIELD]);
            broker = ComboSourceUtil.Instance.FindEnum(broker, "venues");
            string no = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.NO_FIELD]);
            string side = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SIDE_FIELD]);
            string color = side == "2" ? "#58B3FF" : "#FB943C";
            side = ComboSourceUtil.Instance.FindEnum(side, "side");

            drv["SideBrush"] = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString(color));

            string securityID = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SECURITYID_FIELD]);
            string firmFlag = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.FIRMFLAG_FIELD]);
            firmFlag = ComboSourceUtil.Instance.FindEnum(firmFlag, "firmflag");
            string px = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.PX_FIELD]);
            decimal d = 0;
            if (decimal.TryParse(px, out d))
                px = string.Format("{0:N4}%", d);
            string qty = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.QTY_FIELD]);
            string settlType = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.SETTLTYPE_FIELD]);

            string info3 = CommonUtils.ConvertToString(drv[VenuesExecutionViewModel.INFO3]);
            info3 = GetBondOrderInfo(info3);
            drv[VenuesExecutionViewModel.BOND_INFO] = info3;

            settlType = ComboSourceUtil.Instance.FindEnum(settlType, "settletype");
            if (html.Length > 0)
            {
                html.Append("</br>");
            }
            
            html.Append($"<p><span style=\"color: {color}\">{side}</span>&#9;{securityID}&#9;{firmFlag}&#9;{px}&#9;{qty}&#9;{settlType}&#9;（{broker}&#9;{no}）</p>");
            if (plainTxt.Length > 0)
            {
                plainTxt.Append("\n\r");
            }

            VenuesExecutionCopyWindow copyWin = new VenuesExecutionCopyWindow();
            if (status == "6" || status == "7")//待撤销", "6";撤销中", "7"
            {
                if (string.IsNullOrEmpty(info3))
                    plainTxt.Append($"Refer\t{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}\t（{broker}\t{no}）");
                else
                    plainTxt.Append($"Refer\t{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}\t（{broker}\t{no}）\t{info3}");
                
                copyWin.tbTitle.Text = "撤销已复制";
                copyWin.tbRefer.Visibility = Visibility.Visible;
                copyWin.copyList.Margin = new Thickness(-15, 16, 20, 16);
            }
            else
            {
                if (string.IsNullOrEmpty(info3))
                    plainTxt.Append($"{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}\t（{broker}\t{no}）");
                else
                    plainTxt.Append($"{side}\t{securityID}\t{firmFlag}\t{px}\t{qty}\t{settlType}\t（{broker}\t{no}）\t{info3}");
            }
            

            //复制
            ClipboardHelper.CopyToClipboard(html.ToString(), plainTxt.ToString());

            //展示

            IList<DataRowView> newList = new List<DataRowView>();
            newList.Add(dg.CurrentItem as DataRowView);
            copyWin.copyList.ItemsSource = newList;// dg.SelectedItems;

            if (reSetPosition)
            {
                ResetMainScreen();

                copyWin.WindowStartupLocation = WindowStartupLocation.Manual;

                Point newLocation = new Point();
                newLocation.X = mainScreen.WorkingArea.Width / 2 - 200;
                newLocation.Y = mainScreen.WorkingArea.Height / 2 - 200;

                copyWin.Left = newLocation.X;
                copyWin.Top = newLocation.Y;
            }
            copyWin.Show();
        }


        private static System.Windows.Forms.Screen mainScreen = System.Windows.Forms.Screen.PrimaryScreen;
        private static void ResetMainScreen()
        {
            if (Application.Current.MainWindow != null)
            {
                mainScreen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)Application.Current.MainWindow.Left, (int)Application.Current.MainWindow.Top));
            }
        }
    }
}
