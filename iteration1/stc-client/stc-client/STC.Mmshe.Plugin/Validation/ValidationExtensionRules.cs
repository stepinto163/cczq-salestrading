using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using STC.Mmshe.Plugin.Utils;

namespace STC.Mmshe.Plugin.Validation
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
            if (string.IsNullOrEmpty(text) || !"多".Equals(text) && !MMsheCommonUtils.IsDecimal(text))
            {
                return new ValidationResult("不合法");
            }
            return ValidationResult.Success;
        }
    }
}
