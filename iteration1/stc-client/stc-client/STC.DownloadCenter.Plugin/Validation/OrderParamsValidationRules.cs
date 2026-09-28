using STC.Common.Utils;
using STC.DownloadCenter.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace STC.DownloadCenter.Plugin.Validation
{
    public class OrderParamsValidationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is BindingExpression)
                {
                    var binding = value as BindingExpression;
                    AddTaskViewModel detail;

                    if (binding != null && binding.DataItem != null && binding.DataItem is AddTaskViewModel)
                    {
                        detail = binding.DataItem as AddTaskViewModel;

                        if (!IsValidTime(detail.StartDate.Value, detail.StartHour, detail.StartMinute, detail.StartSecond, detail.EndDate.Value, detail.EndHour, detail.EndMinute, detail.EndSecond))
                        {
                            return new ValidationResult(false, "开始时间不得大于结束时间");
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return new ValidationResult(true, null);
        }
        private bool IsValidTime(DateTime startDate, string startHour, string startMin, string startSecond, DateTime endDate, string endHour, string endMin, string endSecond)
        {
            DateTime startTime = new DateTime(startDate.Year, startDate.Month, startDate.Day, startHour.ConvertToInt(), startMin.ConvertToInt(), startSecond.ConvertToInt());
            DateTime endTime = new DateTime(endDate.Year, endDate.Month, endDate.Day, endHour.ConvertToInt(), endMin.ConvertToInt(), endSecond.ConvertToInt());
            return startTime.CompareTo(endTime) <= 0;
        }

    }
}
