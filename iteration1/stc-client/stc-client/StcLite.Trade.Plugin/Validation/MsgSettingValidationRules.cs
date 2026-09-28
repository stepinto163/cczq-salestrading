using BDB.Price.Plugin.ViewModel.Data;
using BDB.Price.Plugin.ViewModel.Trade;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace BDB.Price.Plugin.Validation
{
    public class MsgSettingValidationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is BindingExpression)
                {
                    var binding = value as BindingExpression;
                    MsgSettingItem detail;


                    if (binding != null && binding.DataItem != null && binding.DataItem is MsgSettingViewModel)
                    {
                        detail = (binding.DataItem as MsgSettingViewModel).Item;

                        if (binding.ResolvedSourcePropertyName == "BestBidLimit_Edit")
                        {
                            if (detail.IsBidLimit_Edit == "1" && string.IsNullOrEmpty(detail.BestBidLimit_Edit))
                            {
                                return new ValidationResult(false, "不可为空");
                            }
                        }

                        if (binding.ResolvedSourcePropertyName == "BestOfrLimit_Edit")
                        {
                            if (detail.IsOfrLimit_Edit == "1" && string.IsNullOrEmpty(detail.BestOfrLimit_Edit))
                            {
                                return new ValidationResult(false, "不可为空");
                            }
                        }

                        if (binding.ResolvedSourcePropertyName == "BestBp_Edit")
                        {
                            if (detail.IsBestBp_Edit == "1" && string.IsNullOrEmpty(detail.BestBp_Edit))
                            {
                                return new ValidationResult(false, "不可为空");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            return new ValidationResult(true, null);
        }
    }
}
