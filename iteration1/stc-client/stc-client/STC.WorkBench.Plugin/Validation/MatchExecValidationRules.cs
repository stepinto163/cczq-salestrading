using STC.Common.Utils;
using STC.WorkBench.Plugin.ViewModel.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace STC.WorkBench.Plugin.Validation
{
    public class MatchExecValidationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is BindingExpression)
                {
                    var binding = value as BindingExpression;
                    PtExecordersData detail;

                    if (binding != null && binding.DataItem != null && binding.DataItem is PtExecordersData)
                    {
                        detail = binding.DataItem as PtExecordersData;
                        decimal bindQty = detail.BindQty_Edit.HasValue ? detail.BindQty_Edit.Value : decimal.Zero;
                        decimal lastQty = detail.LastQty.ConvertToDecimal();
                        decimal otherUsedQty = detail.OtherUsedQty;

                        if (bindQty > lastQty - otherUsedQty) 
                        {
                            return new ValidationResult(false, "匹配成交量大于本单可用数量");
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
