using STC.Broker.Plugin.Model.Transaction;
using STC.Common.Model.Base;
using STC.Common.Model.PT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    /// <summary>
    /// 成交数量分析（数值大于200以及以w,kw ,e,亿等结尾的字符串）
    /// </summary>
    public class DataAnalysisHelper
    {

        public static string AnalysisPrice(string price, TBondValuationPo vi)
        {
            string result = string.Empty;

            decimal d = 0;
            if (decimal.TryParse(price, out d))
            {
                if (d == 0)
                {
                    result = "0";
                    return result;
                }

                if (price.Contains("."))
                    result = price;
                else
                {
                    if (!price.StartsWith("0") && price.Length == 1)
                    {
                        result = d.ToString("N2");
                        return result;
                    }

                    if (vi == null)
                        result = price;
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(vi.valuation_yield))
                        {
                            decimal dvaluationyield = decimal.Zero;
                            decimal valuationyieldn = decimal.Zero;
                            if (vi.valuation_yield.IndexOf('|') != -1)
                            {
                                string[] valuationyield = vi.valuation_yield.Split('|');
                                if (valuationyield.Length == 2)
                                {
                                    if (decimal.TryParse(!string.IsNullOrWhiteSpace(valuationyield[0]) ? valuationyield[0] : valuationyield[1], out dvaluationyield))
                                    {
                                        valuationyieldn = dvaluationyield;
                                    }
                                }
                                else
                                {
                                    if (decimal.TryParse(valuationyield[0], out dvaluationyield))
                                    {
                                        valuationyieldn = dvaluationyield;
                                    }
                                }
                            }
                            else
                            {
                                if (decimal.TryParse(vi.valuation_yield, out dvaluationyield))
                                {
                                    valuationyieldn = dvaluationyield;
                                }
                            }

                            decimal pow = (decimal)Math.Pow(10, price.Length);
                            decimal bigNum = Math.Truncate(valuationyieldn);
                            decimal pre = bigNum - 1 + (d / pow);
                            decimal cur = bigNum + (d / pow);
                            decimal next = bigNum + 1 + (d / pow);

                            decimal preDiffer = Math.Abs(pre - valuationyieldn);
                            decimal curDiffer = Math.Abs(cur - valuationyieldn);
                            decimal nextDiffer = Math.Abs(next - valuationyieldn);

                            decimal min = 0;
                            List<decimal> temList = new List<decimal>();
                            temList.Add(preDiffer);
                            temList.Add(curDiffer);
                            temList.Add(nextDiffer);
                            min = temList.Min();
                            if (min == preDiffer)
                                result = pre + "";
                            else if (min == curDiffer)
                                result = cur + "";
                            else if (min == nextDiffer)
                                result = next + "";
                        }
                    }

                }

            }
            return result;
        }

        public static int CompareDateDay(string start)
        {
            int result = 0;
            try
            {
                DateTime dtStart = DateTime.ParseExact(start, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture);

                result = (DateTime.Now - dtStart).Days;
            }
            catch (Exception e)
            {
            }

            return result;
        }

        /// <summary>
        /// 分析数量
        /// </summary>
        /// <param name="qty"></param>
        /// <returns></returns>
        public static string AnalysisQty(string qty)
        {
            string result = string.Empty;

            decimal d = 0;
            if (decimal.TryParse(qty, out d))
                result = qty;
            else if (qty.EndsWith("K") || qty.EndsWith("k") || qty.EndsWith("e") || qty.EndsWith("亿") || qty.EndsWith("E"))
            {
                string[] arr = qty.Split(new string[] { "k", "K" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 1 && decimal.TryParse(arr[0], out d))
                {
                    result = d * 1000 + "";
                }
                else
                {
                    arr = qty.Split(new string[] { "E", "e", "亿" }, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length == 1 && decimal.TryParse(arr[0], out d))
                        result = d * 10000 + "";
                }
            }
            else if (qty.ToLower().EndsWith("kw"))
            {
                string[] arr = qty.Split(new string[] { "kw", "Kw", "kW", "KW" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 1 && decimal.TryParse(arr[0], out d))
                {
                    result = d * 1000 + "";
                }
            }
            else if (qty.EndsWith("W") || qty.EndsWith("w"))
            {
                string[] arr = qty.Split(new string[] { "W", "w" }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length == 1 && decimal.TryParse(arr[0], out d))
                {
                    result = d + "";
                }
            }

            return result;
        }
    }
}
