using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Rfq.Plugin.Utils
{
    public class RfqTools
    {
        /// <summary>
        /// 转换Cfets时间
        /// </summary>
        /// <param name="validuntiltime"></param>
        /// <returns></returns>
        public static DateTime ConvertCfetsDataTime(string validuntiltime)
        {
            DateTime date = DateTime.Now;
            try
            {
                string[] format = { "yyyyMMdd-HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss.fff" };

                if (!string.IsNullOrEmpty(validuntiltime))
                {
                    if (DateTime.TryParseExact(validuntiltime, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out date))
                    {
                    }
                }
            }
            catch
            {
            }
            return date;
        }
        /// <summary>
        /// 格式化时间为阅读方式
        /// </summary>
        /// <param name="diffTime"></param>
        /// <returns></returns>
        public static string FormatTime(int diffTime)
        {
            string result = "";
            int hh = int.Parse(Math.Floor(diffTime / 3600.0).ToString());
            int rel = diffTime % 3600;
            int mm = int.Parse(Math.Floor(rel / 60.0).ToString());
            int relsec = diffTime - hh * 3600 - mm * 60;
            if (hh > 0)
            {
                result = $"{hh}小时{mm}分钟{relsec}秒";
            }

            if (hh == 0 && mm > 0)
            {
                result = $"{mm}分钟{relsec}秒";
            }

            if (hh == 0 && mm == 0)
            {
                result = $"{relsec}秒";
            }

            if (hh == 0 && mm == 0 && relsec == 0)
            {
                result = $"";
            }

            result = result.Replace("钟0秒", "钟").Replace("时0分钟", "时");
            return result;
        }

        /// <summary>
        /// 获取用于显示的状态分类
        /// </summary>
        /// <param name="quoteStatus"></param>
        /// <returns></returns>
        public static string GetStatus(string quoteStatus)
        {
            /// <summary>
            ///待回复1 ==>30,31,32,37,42,40
            ///已发送2 ==>33,5,46,47,48
            ///已成交3 ==>34,36
            ///已拒绝4 ==>38
            ///已失效5 ==>39,44,45
            /// ---------------------------
            /// * Taker发送请求 30  
            ///* Maker锁定  31
            ///* Maker解锁  32
            ///* Maker确认报价 33
            ///* Taker确认报价 34
            ///* Maker接受报价 35
            ///* 订单完成   36
            ///* Taker拒绝报价 37
            ///* Maker拒绝报价 38
            ///* 请求超时      39
            ///* Maker撤回报价  40
            ///* 回复过期   42
            ///* 取消报价  44
            ///* 对手方与其它机构成交 45
            ///* 操作失败 5
            ///* 报价中 46
            ///* 撤单中 47
            ///* 拒绝中 48
            /// </summary>
            string res = string.Empty;
            switch (quoteStatus)
            {
                case "30":
                case "31":
                case "32":
                case "37":
                case "42":
                case "40":
                    //待回复1 ==>30,31,32,37,42,40
                    res = "1";
                    break;
                case "33":
                case "5":
                case "46":
                case "47":
                case "48":
                    //已发送2 ==> 33,5,46,47,48
                    res = "2";
                    break;
                case "34":
                case "36":
                    //已成交3 ==>34,36,45
                    res = "3";
                    break;
                case "38":
                    //已拒绝4 ==>38
                    res = "4";
                    break;
                case "39":
                case "44":
                case "45":
                    //已失效 ==> 39,45,44
                    res = "5";
                    break;
            }
            return res;
        }

        /// <summary>
        /// 获取查询用的过滤状态列表
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static List<string> GetQuoteStatusList(string status)
        {
            ///All 0==> 无
            ///待回复1 ==>30,31,32,37,42,40,44
            ///已发送2 ==>33,5,46,47,48
            ///已成交3 ==>34,36
            ///已拒绝4 ==>38
            ///已失效5 ==>39,45
            List<string> res = new List<string>();
            switch (status)
            {
                case "0":
                    //All 0
                    break;
                case "1":
                    //待回复1 ==>30,31,32,37,42,40
                    res.Add("30");
                    res.Add("31");
                    res.Add("32");
                    res.Add("37");
                    res.Add("42");
                    res.Add("40");
                    break;
                case "2":
                    //已发送2 ==> 33,5,46,47,48
                    res.Add("33");
                    res.Add("5");
                    res.Add("46");
                    res.Add("47");
                    res.Add("48");
                    break;
                case "3":
                    //已成交3 ==>34,36
                    res.Add("34");
                    res.Add("36");
                    break;
                case "4":
                    //已拒绝4 ==>38
                    res.Add("38");
                    
                    break;
                case "5":
                    //已失效5 ==> 39,45,44
                    res.Add("39");
                    res.Add("44");
                    res.Add("45");
                    break;
            }
            return res;
        }

        /// <summary>
        /// 获取状态排序
        /// </summary>
        /// <param name="quoteStatus"></param>
        /// <returns></returns>
        public static string GetStautsOrder(string quoteStatus)
        {
            string res = string.Empty;
            ///待回复1 ==>30,31,32,37,42,40
            ///已发送2 ==>33,5,46,47,48
            ///已成交3 ==>34,36
            ///已拒绝4 ==>38
            ///已失效5 ==>39,45,44
            ///待回复、已发送状态，以剩余请求有效时间排序，优先显示。
            ///已成交、已拒绝、已失效三类为终态，排至末尾，以更新时间排序。
            switch (quoteStatus)
            {
                case "34":
                case "36":
                case "38":
                case "39":
                case "44":
                case "45":
                    res = "0";
                    break;
                default:
                    res = "1";
                    break;
            }

            return res;
        }

        /// <summary>
        /// 是否能回价拒绝
        /// </summary>
        /// <param name="IsSelfRfq"></param>
        /// <param name="Status"></param>
        /// <param name="QuoteStatus"></param>
        /// <param name="IsOutTime"></param>
        /// <returns></returns>
        public static bool GetIsCanReplyRefuseOrder(string IsSelfRfq, string Status, string QuoteStatus, int IsOutTime)
        {
            bool res = false;
            ///待回复1 ==>30,31,32,37,42,40
            ///已发送2 ==>33,5,46,47,48
            ///已成交3 ==>34,36
            ///已拒绝4 ==>38
            ///已失效5 ==>39,45,44
            //已成交、已拒绝、已失效三类为终态
            if (Status == "3" || Status == "4" || Status == "5" || IsOutTime == 1)
            {
                res = false;
            }
            else
            {
                if (IsSelfRfq == "1")
                {
                    if (Status == "2")
                    {
                        res = false;
                    }
                    else
                    {
                        if (Status == "1")
                            res = true;
                        else
                            res = false;
                    }
                }
                else
                {
                    /// * Taker发送请求 30  
                    ///* Maker锁定  31
                    ///* Maker解锁  32
                    ///* Maker确认报价 33
                    ///* Taker确认报价 34
                    ///* Maker接受报价 35
                    ///* 订单完成   36
                    ///* Taker拒绝报价 37
                    ///* Maker拒绝报价 38
                    ///* 请求超时      39
                    ///* Maker撤回报价  40
                    ///* 回复过期   42
                    ///* 取消报价  44 (take 撤销)
                    ///* 对手方与其它机构成交 45
                    ///* 报价中 46
                    ///* 撤单中 47
                    ///* 拒绝中 48
                    ///* 操作失败 5
                    if (QuoteStatus == "30" || QuoteStatus == "32" || QuoteStatus == "42" || QuoteStatus == "40")
                        res = true;
                    else
                        res = false;
                }
            }

            return res;
        }

        /// <summary>
        /// 是否能改价撤单
        /// </summary>
        /// <param name="IsSelfRfq"></param>
        /// <param name="Status"></param>
        /// <param name="QuoteStatus"></param>
        /// <param name="IsOutTime"></param>
        /// <returns></returns>
        public static bool GetIsCanChangeCancelOrder(string IsSelfRfq, string Status, string QuoteStatus, int IsOutTime)
        {
            bool res = false;
            //已成交、已拒绝、已失效三类为终态
            if (Status == "3" || Status == "4" || Status == "5" || IsOutTime == 1)
            {
                res = false;
            }
            else
            {
                res = !GetIsCanReplyRefuseOrder(IsSelfRfq, Status, QuoteStatus, IsOutTime);
                if (res)
                {
                    if (IsSelfRfq == "0")
                    {
                        res = false;
                    }
                }
            }

            return res;
        }
    }
}
