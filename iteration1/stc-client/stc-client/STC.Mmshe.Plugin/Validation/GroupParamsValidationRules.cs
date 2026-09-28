using STC.Common.Utils;
using STC.Mmshe.Plugin.Utils;
using STC.Mmshe.Plugin.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace STC.Mmshe.Plugin.Validation
{
    class GroupParamsValidationRules : ValidationRule
    {
        public GroupParamsValidationRules()
        {
        }

        public GroupParamsValidationRules(ValidationStep validationStep, bool validatesOnTargetUpdated) : base(validationStep, validatesOnTargetUpdated)
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is BindingExpression)
                {
                    var binding = value as BindingExpression;
                    GroupDetailViewModel detail;


                    if (binding != null && binding.DataItem != null && binding.DataItem is GroupDetailViewModel)
                    {
                        detail = binding.DataItem as GroupDetailViewModel;

                        if (binding.ResolvedSourcePropertyName == "bidBaseQty")
                        {
                            if (string.IsNullOrEmpty(detail.Data.bidBaseQty) || (!"多".Equals(detail.Data.bidBaseQty) && !MMsheCommonUtils.IsDecimal(detail.Data.bidBaseQty)))
                            {
                                return new ValidationResult(false, "Bid单笔报量为空");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(detail.Data.bidBaseQty) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.bidBaseQty, 1000000, 10, 0))
                                {
                                    return new ValidationResult(false, $"bid单笔报量需大于等于10且小于等于1000000");
                                }
                            }
                        }


                        if (binding.ResolvedSourcePropertyName == "ofrBaseQty")
                        {
                            if (string.IsNullOrEmpty(detail.Data.ofrBaseQty) || (!"多".Equals(detail.Data.ofrBaseQty) && !MMsheCommonUtils.IsDecimal(detail.Data.ofrBaseQty)))
                            {
                                return new ValidationResult(false, "Ofr单笔报量为空");
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(detail.Data.ofrBaseQty) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.ofrBaseQty, 1000000, 10, 0))
                                {
                                    return new ValidationResult(false, $"ofr单笔报量需大于等于10且小于等于1000000");

                                }
                            }
                        }

                        if (binding.ResolvedSourcePropertyName == "stepLength")
                        {
                            if (string.IsNullOrEmpty(detail.Data.stepLength) || !MMsheCommonUtils.CheckDecimalValid(detail.Data.stepLength, 100m, 0.001m, 3))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        
                        if (binding.ResolvedSourcePropertyName == "manualBid")
                        {
                            if (string.IsNullOrEmpty(detail.Data.manualBid) || !MMsheCommonUtils.CheckDecimalValid(detail.Data.manualBid, 9999, -99.999m, 3))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        if (binding.ResolvedSourcePropertyName == "bidAlgoDefenValue")
                        {
                            //此处是防守报价
                            if (string.IsNullOrEmpty(detail.Data.bidAlgoDefenValue) || (!"多".Equals(detail.Data.bidAlgoDefenValue) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.bidAlgoDefenValue, 100, -100, 3)))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        if (binding.ResolvedSourcePropertyName == "manualOfr")
                        {
                            if (string.IsNullOrEmpty(detail.Data.manualOfr) || !MMsheCommonUtils.CheckDecimalValid(detail.Data.manualOfr, 9999, -99.999m, 3))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        if (binding.ResolvedSourcePropertyName == "ofrAlgoDefenValue")
                        {
                            //此处是防守报价
                            if (string.IsNullOrEmpty(detail.Data.ofrAlgoDefenValue) || (!"多".Equals(detail.Data.ofrAlgoDefenValue) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.ofrAlgoDefenValue, 100, -100, 3)))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }

                        //bid重发阈值
                        if (binding.ResolvedSourcePropertyName == "bidBaseReSendValue")
                        {
                            if (string.IsNullOrEmpty(detail.Data.bidBaseReSendValue) || (!"多".Equals(detail.Data.bidBaseReSendValue) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.bidBaseReSendValue, 1000, 0, 3)))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }

                        //ofr重发阈值
                        if (binding.ResolvedSourcePropertyName == "ofrBaseReSendValue")
                        {
                            if (string.IsNullOrEmpty(detail.Data.ofrBaseReSendValue) || (!"多".Equals(detail.Data.ofrBaseReSendValue) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.ofrBaseReSendValue, 1000, 0, 3)))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }

                        if (binding.ResolvedSourcePropertyName == "bidDefenCDCValue")
                        {
                            if (("1".Equals(detail.Data.defenCDCEnable)) && (string.IsNullOrEmpty(detail.Data.bidDefenCDCValue) || (!"多".Equals(detail.Data.bidDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.bidDefenCDCValue, 1000, 0, 2))))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        if (binding.ResolvedSourcePropertyName == "ofrDefenCDCValue")
                        {
                            if (("1".Equals(detail.Data.defenCDCEnable)) && (string.IsNullOrEmpty(detail.Data.ofrDefenCDCValue) || (!"多".Equals(detail.Data.ofrDefenCDCValue) && !MMsheCommonUtils.CheckDecimalValid(detail.Data.ofrDefenCDCValue, 1000, 0, 2))))
                            {
                                return new ValidationResult(false, "不合法");
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
