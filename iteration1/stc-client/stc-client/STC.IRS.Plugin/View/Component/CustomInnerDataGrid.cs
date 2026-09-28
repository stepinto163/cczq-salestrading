using com.gateway.log;
using STC.Common.Model.Base;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.IRS.Plugin.ViewModel;
using STC.IRS.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace STC.IRS.Plugin.View.Component
{
    public class CustomInnerDataGrid : ExDataGrid
    {
        private string tabStr = "    ";

        public CustomInnerDataGrid()
        {
        }

        protected override void OnExecutedCopy(ExecutedRoutedEventArgs args)
        {
            try
            {
                if (this.Name == "InnerBoard_dataGrid_IRS")
                {
                    CopyData_Single();
                }
                else if (this.Name == "InnerBoard_Best_dataGrid_IRS")
                {
                    CopyData_Best();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("CustomInnerDataGrid OnExecutedCopy", ex);
            }
        }

        /// <summary>
        /// //"剩余期限（包括含权期限，是否休假）"、"债券代码（.IB .SH .SZ）"、"债券简称"、 "价格（bid/ofr，--/ofr）（"全""净"）"、"券面（万为单位，如 3000，3000/3000）"、"中债估值 （含权债默认为含权估值"权"）"
        /// </summary>
        /// <param name="dg"></param>
        public void CopyData_Single(bool needPrompt = true)
        {
            StringBuilder plainTxt = new StringBuilder();

            IEnumerable<InnerOrderData> list = this.SelectedItems.Cast<InnerOrderData>();

            foreach (InnerOrderData data in list)
            {
                string side = data.Side;
                string term = string.IsNullOrEmpty(data.Term) ? string.Empty : data.Term.Replace(" ", "");
                
                //int index = remainingTime.IndexOf("+");
                //if (index >= 0)
                //{
                //    remainingTime = remainingTime.Substring(0, index);
                //}

                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string px = data.Price.HasValue ?  data.Price?.ToString("0.00##") : (side == "1" ? "Bid" : "Ofr");
                string px_Str;
                //string qty = data.Qty.HasValue ? data.Qty?.ToString("0") : "--";
                string qty = data.CopyInfo;
                string qty_Str;

                if (side == "1")
                {
                    px_Str = $"{px}/--";
                    qty_Str = $"{qty}/--";
                }
                else
                {
                    px_Str = $"--/{px}";
                    qty_Str = $"--/{qty}";
                }

                //string CCDCValuation = data.CCDCValuation?.ToString("0.00##");

                //if (CCDCValuation == null)
                //{
                //    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(securityId);
                //    if (ccdcValueModel != null)
                //    {
                //        CCDCValuation = ccdcValueModel.CCDCValuation;
                //    }
                //}

                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                plainTxt.Append($"{term}{tabStr}{securityId}{tabStr}{px_Str}{tabStr}{qty_Str}");
            }
            
            //复制,不加try会导致程序无法退出
            try
            {
                if (plainTxt.Length > 0 && needPrompt)
                {
                    ClipboardHelper.CopyToClipboard(plainTxt.ToString(), true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }


        /// <summary>
        /// "剩余期限（包括含权期限，是否休假）"、"债券代码（.IB .SH .SZ）"、"债券简称"、 "价格（bid/ofr，--/ofr）（"全""净"）"、"券面（万为单位，如 3000，3000/3000）"、"中债估值 （含权债默认为含权估值"权"）"
        /// </summary>
        /// <param name="dg"></param>
        public void CopyData_Best(bool needPrompt = true)
        {
            StringBuilder plainTxt = new StringBuilder();


            IEnumerable<InnerOrderData> list = this.SelectedItems.Cast<InnerOrderData>();

            foreach (InnerOrderData data in list)
            {
                string side = data.Side;
                string term = string.IsNullOrEmpty(data.Term) ? string.Empty : data.Term.Replace(" ", "");

                //int index = remainingTime.IndexOf("+");
                //if (index >= 0)
                //{
                //    remainingTime = remainingTime.Substring(0, index);
                //}
                string securityId = data.SecurityID;
                string symbolName = data.Symbol;
                string px_Str = (data.Price_InnerBid.HasValue ? data.Price_InnerBid?.ToString("0.00##") : (data.Count_InnerBid > 0 ? "Bid" : "--")) + "/" + (data.Price_InnerOfr.HasValue ? data.Price_InnerOfr?.ToString("0.00##") : (data.Count_InnerOfr > 0 ? "Ofr" : "--"));
                string qty_Str = (string.IsNullOrEmpty(data.CopyInfo_InnerBid) ? "--" : data.CopyInfo_InnerBid) + "/" + (string.IsNullOrEmpty(data.CopyInfo_InnerOfr) ? "--" : data.CopyInfo_InnerOfr);

                //string CCDCValuation = data.CCDCValuation?.ToString("0.00##");

                //if (CCDCValuation == null)
                //{
                //    CCDCValuaModel ccdcValueModel = CommonUtils.GetCCDCValua(securityId);
                //    if (ccdcValueModel != null)
                //    {
                //        CCDCValuation = ccdcValueModel.CCDCValuation;
                //    }
                //}

                if (plainTxt.Length > 0)
                {
                    plainTxt.Append("\n");
                }

                plainTxt.Append($"{term}{tabStr}{securityId}{tabStr}{px_Str}{tabStr}{qty_Str}");
            }
            //复制
            //复制,不加try会导致程序无法退出
            try
            {
                if (plainTxt.Length > 0)
                {
                    ClipboardHelper.CopyToClipboard(plainTxt.ToString(), true);
                }
            }
            catch { }
            finally
            {
                if (plainTxt.Length > 0 && needPrompt)
                    CommonUtils.Prompt2Second("提示", "已复制");
            }
        }
    }
}
