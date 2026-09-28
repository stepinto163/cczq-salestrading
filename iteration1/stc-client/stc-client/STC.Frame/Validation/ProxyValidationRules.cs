using STC.Frame.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace STC.Frame.Validation
{
    public class ProxyValidationRules : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is BindingExpression)
                {
                    var binding = value as BindingExpression;
                    NetworkSettingViewModel detail;

                    if (binding != null && binding.DataItem != null && binding.DataItem is NetworkSettingViewModel)
                    {
                        detail = binding.DataItem as NetworkSettingViewModel;

                        if (detail.AccessType == "3")
                        {
                            if (binding.ResolvedSourcePropertyName == "ProxyHost" && string.IsNullOrEmpty(detail.ProxyHost))
                            {
                                return new ValidationResult(false, "地址不能为空");
                            }
                            else if (binding.ResolvedSourcePropertyName == "ProxyPort" && string.IsNullOrEmpty(detail.ProxyPort))
                            {
                                return new ValidationResult(false, "端口不能为空");
                            }
                            //else if (binding.ResolvedSourcePropertyName == "ProxyUser" && string.IsNullOrEmpty(detail.ProxyUser))
                            //{
                            //    return new ValidationResult(false, "用户不能为空");
                            //}
                            //else if (binding.ResolvedSourcePropertyName == "ProxyPassword" && string.IsNullOrEmpty(detail.ProxyPassword))
                            //{
                            //    return new ValidationResult(false, "密码不能为空");
                            //}
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
