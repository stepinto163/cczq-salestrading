using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.quant.plugin.Validation
{
    public class ValidationExtensionRules
    {
        public static ValidationResult NotEmpty(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new ValidationResult("不能为空");
            }
            return ValidationResult.Success;
        }
    }
}
