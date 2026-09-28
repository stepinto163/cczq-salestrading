using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Mm.Plugin.Utils;

namespace STC.Mm.Plugin.Validation
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

        public static ValidationResult NotEmptyIsDecimal(string text)
        {
            if (string.IsNullOrEmpty(text) || !"多".Equals(text) && !MMCommonUtils.IsDecimal(text))
            {
                return new ValidationResult("不合法");
            }
            return ValidationResult.Success;
        }
    }
}
