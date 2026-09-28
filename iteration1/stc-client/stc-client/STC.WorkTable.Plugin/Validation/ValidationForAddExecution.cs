using STC.Common.Data.DataModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace STC.WorkTable.Plugin.Validation
{
    public class ValidationForAddExecution : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && value is System.Windows.Data.BindingExpression)
            {
                System.Windows.Data.BindingExpression binding = value as System.Windows.Data.BindingExpression;

                if (binding.ResolvedSourcePropertyName == "CounterPartyName"
                     || binding.ResolvedSourcePropertyName == "CounterPartySubName"
                     || binding.ResolvedSourcePropertyName == "Price"
                     || binding.ResolvedSourcePropertyName == "SettlType"
                     || binding.ResolvedSourcePropertyName == "BookQty")
                {
                    IntentionExecItem currentEditItem = binding.DataItem as IntentionExecItem;

                    if (binding.ResolvedSourcePropertyName == "CounterPartyName")
                    {
                        if (string.IsNullOrEmpty(currentEditItem.CounterPartyName))
                            return new ValidationResult(false, "对手机构不能为空");
                    }
                    else if (binding.ResolvedSourcePropertyName == "CounterPartySubName")
                    {
                        if (string.IsNullOrEmpty(currentEditItem.CounterPartySubName))
                            return new ValidationResult(false, "对手方不能为空");
                    }
                    else if (binding.ResolvedSourcePropertyName == "Price")
                    {
                        if (currentEditItem.Price == null)
                            return new ValidationResult(false, "成交价不能为空");
                    }
                    else if (binding.ResolvedSourcePropertyName == "SettlType")
                    {
                        if (string.IsNullOrEmpty(currentEditItem.SettlType))
                            return new ValidationResult(false, "清算速度不能为空");
                    }
                    else if (binding.ResolvedSourcePropertyName == "BookQty")
                    {
                        if (currentEditItem.BookQty == null)
                            return new ValidationResult(false, "成交量不能为空");
                    }
                }  
            }
            return new ValidationResult(true, null);
        }
    }
}
