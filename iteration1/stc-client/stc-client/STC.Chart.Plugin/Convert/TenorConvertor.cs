using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace STC.Chart.Plugin.Converts
{
    public class TenorChartConst
    {
        public const double ValueX_0D = 0;
        public const double ValueX_1M = 5;
        public const double ValueX_2M = 10;
        public const double ValueX_3M = 15;
        //public const double ValueX_4M = 4;
        //public const double ValueX_5M = 5;
        public const double ValueX_6M = 30;
        public const double ValueX_9M = 45;
        public const double ValueX_1Y = 60;

        public const double ValueX_2Y = 75;
        public const double ValueX_3Y = 90;
        public const double ValueX_4Y = 105;
        public const double ValueX_5Y = 120;

        public const double ValueX_6Y = 132;
        public const double ValueX_7Y = 144;
        public const double ValueX_8Y = 156;
        public const double ValueX_9Y = 168;
        public const double ValueX_10Y = 180;

        public const double ValueX_15Y = 210;
        public const double ValueX_20Y = 240;

        public const double ValueX_30Y = 300;
        public const double ValueX_40Y = 360;
        public const double ValueX_50Y = 420;

        public static List<double> TenorList { get; set; }

        static TenorChartConst()
        {
            TenorList = new List<double>();
            TenorList.Add(ValueX_0D);
            TenorList.Add(ValueX_1M);
            TenorList.Add(ValueX_2M);
            TenorList.Add(ValueX_3M);
            //TenorList.Add(ValueX_4M);
            //TenorList.Add(ValueX_5M);
            TenorList.Add(ValueX_6M);
            TenorList.Add(ValueX_9M);
            TenorList.Add(ValueX_1Y);

            TenorList.Add(ValueX_2Y);
            TenorList.Add(ValueX_3Y);
            TenorList.Add(ValueX_4Y);
            TenorList.Add(ValueX_5Y);

            TenorList.Add(ValueX_6Y);
            TenorList.Add(ValueX_7Y);
            TenorList.Add(ValueX_8Y);
            TenorList.Add(ValueX_9Y);
            TenorList.Add(ValueX_10Y);

            TenorList.Add(ValueX_15Y);
            TenorList.Add(ValueX_20Y);
            TenorList.Add(ValueX_30Y);
            TenorList.Add(ValueX_40Y);
            TenorList.Add(ValueX_50Y);
        }

        public static string ToTenor(double dval)
        {
            int val = (int)dval;
            int d;
            //if (val % 5 == 0)
            //{
            string str = "";
            if (val <= ValueX_1Y)
            {
                int count = val / 5;
                if (count == 12)
                {
                    str = "1Y";
                }
                else
                {
                    str = count + "M";
                }
                //str = val / 5 + "M";
            }
            else if (val <= ValueX_5Y)
            {
                d = val - (int)ValueX_1Y;
                if (d % 15 == 0)
                {
                    str = d / 15 + 1 + "Y";
                }
            }
            else if (val <= ValueX_10Y)
            {
                d = val - (int)ValueX_5Y;
                if (d % 12 == 0)
                {
                    str = d / 12 + 5 + "Y";
                }
            }
            else
            {
                d = val - (int)ValueX_10Y;
                if (d % 30 == 0)
                {
                    str = d / 30 * 5 + 10 + "Y";
                }
            }
            return str;
            //}
            //return "";
        }

        public static DateTime ToDateTime(double dval, DateTime dtBase)
        {
            DateTime result = dtBase;
            double d;
            int days = 0, totalDays = 0;
            if (dval <= ValueX_1Y)
            {
                totalDays = CalcDays(dtBase, 0, 1);
                d = ValueX_1Y;
                days = (int)(dval / d * totalDays);
                result = dtBase.AddDays(days);
            }
            else if (dval <= ValueX_5Y)
            {
                totalDays = CalcDays(dtBase, 1, 5);
                d = ValueX_5Y - ValueX_1Y;
                days = (int)((dval - ValueX_1Y) / d * totalDays);
                result = dtBase.AddDays(days);
            }
            else if (dval <= ValueX_10Y)
            {
                totalDays = CalcDays(dtBase, 5, 10);
                d = ValueX_10Y - ValueX_5Y;
                days = (int)((dval - ValueX_5Y) / d * totalDays);
                result = dtBase.AddDays(days);
            }
            else
            {
                totalDays = CalcDays(dtBase, 10, 50);
                d = ValueX_50Y - ValueX_10Y;
                days = (int)((dval - ValueX_10Y) / d * totalDays);
                result = dtBase.AddDays(days);
            }
            return result;
        }

        private static int CalcDays(DateTime dtBase, int startYears, int endYears)
        {
            DateTime dtStart = dtBase.AddYears(startYears);
            DateTime dtEnd = dtBase.AddYears(endYears);
            TimeSpan timeSpan = dtEnd - dtStart;
            return timeSpan.Days;
        }
    }


    public class TenorRange
    {
        public int Max { get; set; }
        public int Min { get; set; }
        public int Step { get; set; }
        public int ActMax { get; set; }
        public int ActMin { get; set; }
        public int ActStep { get; set; }
        public int TotalSteps { get; set; }
        public TenorUnitEnum Unit { get; set; }

        public TenorRange()
        {
        }

        public TenorRange(int min,int max,int step,int actMin,int actMax,TenorUnitEnum unit)
        {
            this.Min = min;
            this.Max = max;
            this.Step = step;
            this.ActMin = actMin;
            this.ActMax = actMax;
            this.Unit = unit;
            TotalSteps = (Max - Min) / Step;
            ActStep = (ActMax - ActMin) / TotalSteps;
        }

        internal static TenorRange ActInside(int unitCount, TenorUnitEnum unit, object tenorRangeList)
        {
            throw new NotImplementedException();
        }

        public bool IsInside(double val)
        {
            return val >= Min && val <= Max;
        }

        public bool IsActInside(int val, TenorUnitEnum unit)
        {
            if (Unit != unit)
            {
                int umax = ActMax, umin = ActMin;
                if (this.Unit == TenorUnitEnum.Year)
                {
                    umax *= 12;
                    umin *= 12;
                }
                int uval = val;
                if (unit == TenorUnitEnum.Year)
                {
                    uval *= 12;
                }
                return uval >= umin && uval <= umax;
            }
            else
            {
                return val >= ActMin && val <= ActMax;
            }
        }

        public int ActToInternal(int val, TenorUnitEnum unit)
        {
            int steps = (val - ActMin) / ActStep;
            return Min + steps * this.Step;
        }

        public string ToTenorString(double val)
        {
            string str = "";
            int valInt = (int)val;
            int d = valInt - Min;
            if (d % Step == 0)
            {
                int count = d / Step;
                count = ActMin + count * ActStep;
                switch (Unit)
                {
                    case TenorUnitEnum.Month:
                        if (count >= 12)
                        {
                            if (count % 12 == 0)
                            {
                                str = count / 12 + "Y";
                            }
                            else
                            {
                                str = count + "M";
                            }
                        }
                        else if (count > 0)
                        {
                            str = count + "M";
                        }
                        break;
                    case TenorUnitEnum.Year:
                        str = count + "Y";
                        break;
                }
            }
            return str;
        }

        public DateTime ToDateTime(double val,DateTime dtBase)
        {
            DateTime result = dtBase;
            int valInt = (int)val;
            int d = valInt - Min;
            int count = d / Step;
            double left = val - Min - count * Step;
            count = ActMin + count * ActStep;
            left = left / Step * ActStep;
            int days = 0;
            switch (Unit)
            {
                case TenorUnitEnum.Month:
                    result = result.AddMonths(count);
                    days = (int)(left * 30);
                    result = result.AddDays(days);
                    break;
                case TenorUnitEnum.Year:
                    result = result.AddYears(count);
                    days = (int)(left * 365);
                    result = result.AddDays(days);
                    break;
            }
            return result;
        }

        public static List<TenorRange> Sample()
        {
            List<TenorRange> list = new List<TenorRange>();
            list.Add(new TenorRange(0, 60, 5, 0, 12, TenorUnitEnum.Month));
            list.Add(new TenorRange(60, 120, 15, 1, 5, TenorUnitEnum.Year));
            list.Add(new TenorRange(120, 180, 12, 5, 10, TenorUnitEnum.Year));
            list.Add(new TenorRange(180, 240, 30, 10, 20, TenorUnitEnum.Year));
            list.Add(new TenorRange(240, 420, 60, 20, 50, TenorUnitEnum.Year));
            return list;
        }

        public static TenorRange ActInside(int val,TenorUnitEnum unit,List<TenorRange> list)
        {
            foreach(TenorRange item in list)
            {
                if (item.IsActInside(val, unit))
                {
                    return item;
                }
            }
            return null;
        }

        public static TenorRange InternalInside(double val, List<TenorRange> list)
        {
            foreach (TenorRange item in list)
            {
                if (item.IsInside(val))
                {
                    return item;
                }
            }
            return null;
        }

        public static string ToTenorString(double d, List<TenorRange> list)
        {
            TenorRange range = InternalInside(d, list);
            if (range != null)
            {
                return range.ToTenorString(d);
            }
            return "";
        }

        public static DateTime ToDateTime(double d, DateTime dtBase, List<TenorRange> list)
        {
            TenorRange range = InternalInside(d, list);
            if (range != null)
            {
                return range.ToDateTime(d,dtBase);
            }
            return dtBase;
        }
    }

    public enum TenorUnitEnum
    {
        None,//
        Month,//
        Year
    }

    public class TenorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;
            return TenorChartConst.ToTenor(d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

    public class DiffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;
            return "4.55";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }

    public class DateConverter : IValueConverter
    {
        public DateTime DtBase { get; set; }
        public List<TenorRange> TenorRangeList { get; set; }

        public DateConverter()
        {
            DtBase = DateTime.Now;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = (double)value;
            DateTime dt = TenorRange.ToDateTime(d, DtBase, TenorRangeList);
            return dt.ToString("yyyy-MM-dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
