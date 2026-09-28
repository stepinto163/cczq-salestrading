using STC.Common.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.XBond.Plugin.Convertor
{
    /// <summary>
    /// 国债期货/债券价格 显示%或者元
    /// </summary>
    public class TreasuryFutureConvertor : IMultiValueConverter
    {
        public const string MARKETINDICATOR_FUTURE = "5";
        public const string TYPE_AMOUNT = "Amount";
        public const string TYPE_PRICE = "Price";
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //多参数绑定
            if (values == null || values.Length < 3)
                return "--";
            object bindingParam = values[0];
            if(bindingParam==null || bindingParam == System.Windows.DependencyProperty.UnsetValue || string.IsNullOrEmpty(bindingParam.ToString()))
            {
                return "--";
            }
            object marketindicator = values[1];
            object type = values[2];
            //转换同步状态 转换成交渠道
            if ("synchflag".Equals(parameter))
            {
                if (MARKETINDICATOR_FUTURE.Equals(marketindicator))
                {
                    return "--";
                }
                string key = bindingParam.ToString();
                string[] keyArr = key.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                string param = parameter.ToString();
                StringBuilder sb = new StringBuilder();
                foreach (string item in keyArr)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(CommonConstants.COMMA_CHAR);
                    }
                    sb.Append(ComboSourceUtil.Instance.FindEnum(item.Trim(), param));
                }
                return sb.ToString();
            }
            if ("venues_exe".Equals(parameter))
            {
                if (MARKETINDICATOR_FUTURE.Equals(marketindicator))
                {
                    return "TradeBook";
                }
                string key = bindingParam.ToString();
                string[] keyArr = key.Split(CommonConstants.COMMA_CHAR, StringSplitOptions.RemoveEmptyEntries);
                string param = parameter.ToString();
                StringBuilder sb = new StringBuilder();
                foreach (string item in keyArr)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(CommonConstants.COMMA_CHAR);
                    }
                    sb.Append(ComboSourceUtil.Instance.FindEnum(item.Trim(), param));
                }
                return sb.ToString();
            }

            if (TYPE_AMOUNT.Equals(type))
            {
                if (MARKETINDICATOR_FUTURE.Equals(marketindicator))
                {
                    long amount = bindingParam.ToString().ConvertToLong();
                    return amount == 0 ? "--" : amount.ToString();
                }
                else
                {
                    return "--";
                }
            }
            if (TYPE_PRICE.Equals(parameter))
            {
                int digit = 4;
                string typeStr = type != null ? type.ToString() : string.Empty;
                switch (typeStr)
                {
                    case Consts.PRICE_MIN_CHANGE_00001:
                        digit = 4;
                        break;
                    case Consts.PRICE_MIN_CHANGE_0001:
                    case Consts.PRICE_MIN_CHANGE_0002:
                    case Consts.PRICE_MIN_CHANGE_0005:
                        digit = 3;
                        break;
                    case Consts.PRICE_MIN_CHANGE_002:
                    case Consts.PRICE_MIN_CHANGE_005:
                        digit = 2;
                        break;
                    case Consts.PRICE_MIN_CHANGE_01:
                    case Consts.PRICE_MIN_CHANGE_02:
                    case Consts.PRICE_MIN_CHANGE_05:
                        digit = 1;
                        break;
                    case Consts.PRICE_MIN_CHANGE_1:
                    case Consts.PRICE_MIN_CHANGE_2:
                    case Consts.PRICE_MIN_CHANGE_5:
                    case Consts.PRICE_MIN_CHANGE_10:
                        digit = 0;
                        break;
                }
                decimal price;
                if (decimal.TryParse(bindingParam.ToString(), out price))
                {
                    return price.ToString("F" + digit);
                }
                return bindingParam.ToString();
            }
            return "--";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
