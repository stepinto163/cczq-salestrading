using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace STC.Broker.Plugin.Utils
{
    public static class Extension
    {
        /// <summary>
        /// 添加非空字符串到list中，
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AddNotEmptyString(this List<string> list, string value)
        {
            bool result = false;
            if (!string.IsNullOrEmpty(value))
            {
                list.Add(value);
            }
            return result;
        }


        /// <summary>
        /// 检查文本是否是数字或者点
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNumberOrDot(this string text)
        {
            Regex isNumberRegex = new Regex("^[0-9|.]*$");
            return isNumberRegex.IsMatch(text);
        }

        /// <summary>
        /// 数字不包含0
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNumberWithoutZero(this string text)
        {
            Regex isNumberRegex = new Regex("^[1-9]*$");
            return isNumberRegex.IsMatch(text);
        }

        /// <summary>
        /// 数字包含0
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNumber(this string text)
        {
            Regex isNumberRegex = new Regex("^[0-9]*$");
            return isNumberRegex.IsMatch(text);
        }


        /// <summary>
        /// 比较两个string list 内容是否相同
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="compareList"></param>
        /// <returns></returns>
        public static bool IsSameList(this List<string> origin, List<string> compareList)
        {
            var result = (origin == null || origin.Count == 0) && (compareList == null || compareList.Count == 0);

            if (result)
            {
                result = origin.Count == compareList.Count;
            }
            if (result)
            {
                foreach (var item in origin)
                {
                    result = compareList.Any(x => x == item);
                    if (!result)
                    {
                        break;
                    }
                }
            }

            return result;
        }
    }
}
