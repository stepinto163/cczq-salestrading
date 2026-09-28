using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace STC.Common.Utils
{
    /// <summary>
    /// 扩展方法工具类
    /// </summary>
    public static class ExtensionHelper
    {
        /// <summary>
        /// 获取枚举类型的描述
        /// </summary>
        /// <param name="enumeration"></param>
        /// <returns></returns>
        public static string GetOldDescription(this Enum enumeration)
        {
            Type type = enumeration.GetType();
            MemberInfo[] memInfo = type.GetMember(enumeration.ToString());
            if (null != memInfo && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (null != attrs && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return enumeration.ToString();
        }

        public static string ConvertToString(this object obj)
        {
            return obj == null ? string.Empty : obj.ToString();
        }

        public static bool ConvertToBool(this object obj)
        {
            bool result = false;
            bool.TryParse(obj.ConvertToString(), out result);
            return result;
        }

        public static int ConvertToInt(this object obj)
        {
            int result;
            int.TryParse(obj.ConvertToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static long ConvertToLong(this object obj)
        {
            long result;
            long.TryParse(obj.ConvertToString(),System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }
        public static decimal ConvertToDecimal(this object obj)
        {
            decimal result;
            decimal.TryParse(obj.ConvertToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static double ConvertToDouble(this object obj)
        {
            double result;
            double.TryParse(obj.ConvertToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
            return result;
        }

        public static bool SetValue(this DataRow dr, string columnName, object value)
        {
            bool flag = false;
            try
            {
                if (dr != null && dr.Table.Columns.Contains(columnName))
                {
                    if (dr[columnName] is System.DBNull)
                    {
                        dr[columnName] = value;
                        flag = true;
                        return flag;
                    }

                    if (dr.Table.Columns[columnName].DataType == typeof(string))
                    {
                        string convertValue = value.ConvertToString();
                        if ((string)dr[columnName] != convertValue)
                        {
                            dr[columnName] = convertValue;
                            flag = true;
                        }
                    }
                    else if (dr.Table.Columns[columnName].DataType == typeof(bool))
                    {
                        bool convertValue = value.ConvertToBool();
                        if ((bool)dr[columnName] != convertValue)
                        {
                            dr[columnName] = convertValue;
                            flag = true;
                        }
                    }
                    else if (dr.Table.Columns[columnName].DataType == typeof(int))
                    {
                        int convertValue = value.ConvertToInt();
                        if ((int)dr[columnName] != convertValue)
                        {
                            dr[columnName] = convertValue;
                            flag = true;
                        }
                    }
                    else if (dr.Table.Columns[columnName].DataType == typeof(long))
                    {
                        long convertValue = value.ConvertToLong();
                        if ((long)dr[columnName] != convertValue)
                        {
                            dr[columnName] = convertValue;
                            flag = true;
                        }
                    }
                    else if (dr.Table.Columns[columnName].DataType == typeof(decimal))
                    {
                        decimal convertValue = value.ConvertToDecimal();
                        if ((decimal)dr[columnName] != convertValue)
                        {
                            dr[columnName] = convertValue;
                            flag = true;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                LogHandler.Error(e);
            }
            return flag;
        }

        public static void AppendText(this RichTextBox box, string text, string color)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }

        public static void AppendText_bg(this RichTextBox box, string text, string color)
        {
            BrushConverter bc = new BrushConverter();
            TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
            tr.Text = text;
            try
            {
                tr.ApplyPropertyValue(TextElement.BackgroundProperty,
                    bc.ConvertFromString(color));
            }
            catch (FormatException) { }
        }
    }
}
