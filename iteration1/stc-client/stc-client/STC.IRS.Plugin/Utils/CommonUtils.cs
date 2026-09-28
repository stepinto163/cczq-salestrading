using com.gateway.log;
using netapi.Api;
using Newtonsoft.Json;
using STC.Common.Api;
using STC.Common.Data;
using STC.Common.Handler.Base;
using STC.Common.Model.Fix.Message;
using STC.Common.Model.Table;
using STC.Common.Utils;
using STC.Common.View.Component;
using STC.Common.ViewModel.Data;
using STC.Frame.Common;
using STC.IRS.Plugin.Command;
using STC.IRS.Plugin.ViewModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace STC.IRS.Plugin.Utils
{
    public class CommonUtils
    {
        /// <summary>
        /// 弹窗命令处理类
        /// </summary>
        public static PopupWindowCommand PopupWindowCommand = new PopupWindowCommand();

        // <summary>
        /// 把基础剩余时间转换成时间段, 转成‘年’ 
        /// 如：2Y+ 5M + 6D -> 2 + 5 / 12 + 6 / 365 
        /// </summary>
        /// <param name="remainingTime">基础剩余时间</param>
        /// <returns>筛选时间段</returns>
        public static decimal ConvertRemainingTime(string remainingTime)
        {
            string remainingTimeLevel = string.Empty;
            decimal convertTime = 0;
            if (string.IsNullOrEmpty(remainingTime) == false)
            {
                string[] splitArr = remainingTime.Split(CommonConstants.PLUS_CHAR, StringSplitOptions.RemoveEmptyEntries);
                if (splitArr != null && splitArr.Length > 0)
                {
                    for (int i = 0; i < splitArr.Length; i++)
                    {
                        string data;
                        data = splitArr[i].Trim();
                        if (data.EndsWith("Y", StringComparison.OrdinalIgnoreCase))
                        {
                            string yearStr = data.ToUpper().Replace("Y", "");
                            decimal year = yearStr.ConvertToDecimal();
                            convertTime += year;
                        }
                        else if (data.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                        {
                            string monStr = data.ToUpper().Replace("M", "");
                            decimal month = monStr.ConvertToDecimal() / 12;
                            convertTime += month;
                        }
                        else if (data.EndsWith("D"))
                        {
                            string dayStr = data.ToUpper().Replace("D", "");
                            decimal day = dayStr.ConvertToDecimal() / 365;
                            convertTime += day;
                        }
                    }
                }
            }

            return convertTime;
        }


        public static void Prompt2Second(string title, string text)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Sub_Prompt2Second(title, text);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    Sub_Prompt2Second(title, text);
                }));
            }
        }


        private static void Sub_Prompt2Second(string title, string text)
        {
            Prompt2Seconds prompt = new Prompt2Seconds();
            prompt.PromptTitle = title;
            prompt.PromptContext = text;
            prompt.Owner = Application.Current.MainWindow;
            prompt.Show();
        }

        public static string ToBinary(long x)
        {
            char[] buff = new char[64];

            for (int i = 63; i >= 0; i--)
            {
                buff[i] = x % 2 == 1 ? '1' : '0';
                x = x / 2;
            }

            return new string(buff);
        }

        public static long ToLong(string binaryStr)
        {
            long l = 0;
            char[] arr = binaryStr.ToCharArray();
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                l += (long)Math.Pow(2, 64 - i - 1) * arr[i].ConvertToLong();
            }
            return l;
        }
    }
}
