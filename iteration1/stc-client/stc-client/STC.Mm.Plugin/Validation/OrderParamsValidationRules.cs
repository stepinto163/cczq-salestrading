using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using STC.Mm.Plugin.ViewModel;
using STC.Mm.Plugin.Utils;
using STC.Common.Utils;

namespace STC.Mm.Plugin.Validation
{
    public class OrderParamsValidationRules : ValidationRule
    {
        public OrderParamsValidationRules()
        {
        }

        public OrderParamsValidationRules(ValidationStep validationStep, bool validatesOnTargetUpdated) : base(validationStep, validatesOnTargetUpdated)
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is BindingExpression)
                {
                    var binding = value as BindingExpression;
                    OrderDetailViewModel detail;


                    if (binding != null && binding.DataItem != null && binding.DataItem is OrderDetailViewModel) 
                    {
                        detail = binding.DataItem as OrderDetailViewModel;

                        if (binding.ResolvedSourcePropertyName == "bidBaseQty")
                        {
                            if (string.IsNullOrEmpty(detail.Data.bidBaseQty) || !MMCommonUtils.IsDecimal(detail.Data.bidBaseQty))
                            {
                                return new ValidationResult(false, "Bid单笔报量为空");
                            }
                            else
                            {
                                //if (!string.IsNullOrEmpty(detail.Data.bidBaseQty))
                                //{
                                //    if (detail.Data.bidBaseQty.ConvertToDecimal().CompareTo(detail.Data.circulSize.ConvertToDecimal() * detail.Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                                //    {
                                //        return new ValidationResult(false, $"bid单笔报量需大于等于{detail.Data.riskMinQtySize.ConvertToDecimal() / 1000}且小于等于{detail.Data.circulSize.ConvertToDecimal() * detail.Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");
                                //    }

                                //    if (detail.Data.bidBaseQty.ConvertToDecimal().CompareTo(detail.Data.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
                                //    {
                                //        return new ValidationResult(false, $"bid单笔报量需大于等于" + detail.Data.riskMinQtySize.ConvertToDecimal() / 1000);
                                //    }
                                //}
                            }
                        }


                        if (binding.ResolvedSourcePropertyName == "ofrBaseQty")
                        {
                            if (string.IsNullOrEmpty(detail.Data.ofrBaseQty) || !MMCommonUtils.IsDecimal(detail.Data.ofrBaseQty))
                            {
                                return new ValidationResult(false, "Ofr单笔报量为空");
                            }
                            else
                            {
                                //if (!string.IsNullOrEmpty(detail.Data.ofrBaseQty))
                                //{
                                //    if (detail.Data.ofrBaseQty.ConvertToDecimal().CompareTo(detail.Data.circulSize.ConvertToDecimal() * detail.Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10) > 0)
                                //    {
                                //        return new ValidationResult(false, $"ofr单笔报量需大于等于{detail.Data.riskMinQtySize.ConvertToDecimal() / 1000}且小于等于{detail.Data.circulSize.ConvertToDecimal() * detail.Data.riskMaxQtyRatio.ConvertToDecimal() / 100 * 10}");
                                //    }
                                //}

                                //if (detail.Data.ofrBaseQty.ConvertToDecimal().CompareTo(detail.Data.riskMinQtySize.ConvertToDecimal() / 1000) < 0)
                                //{
                                //    return new ValidationResult(false, $"ofr单笔报量需大于等于" + detail.Data.riskMinQtySize.ConvertToDecimal() / 1000);
                                //}
                            }
                        }

                       if (binding.ResolvedSourcePropertyName == "stepLength")
                        {
                            if (string.IsNullOrEmpty(detail.Data.stepLength) || !MMCommonUtils.CheckDecimalValid(detail.Data.stepLength, 99.99m, 0.01m, 2))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        if (binding.ResolvedSourcePropertyName == "posFakSpread")
                        {
                            if ("1".Equals(detail.Data.posFakEnable) && (string.IsNullOrEmpty(detail.Data.posFakSpread) || !MMCommonUtils.CheckDecimalValid(detail.Data.posFakSpread, 99.99m, 0, 2)))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                            else 
                            {
                                return new ValidationResult(true, null);
                            }
                        }

                        if (binding.ResolvedSourcePropertyName == "maxFloor")
                        {
                            if ("1".Equals(detail.Data.maxFloorEnable) && (string.IsNullOrEmpty(detail.Data.maxFloor) || !MMCommonUtils.IsDecimal(detail.Data.maxFloor) || CommonUtils.ConvertToLong(detail.Data.maxFloor) < 1 || CommonUtils.ConvertToLong(detail.Data.maxFloor) > 999))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                            else
                            {
                                return new ValidationResult(true, null);
                            }
                        }
                        string bidQuoteModel = detail.Data.bidQuoteModel;
                        string ofrQuoteModel = detail.Data.ofrQuoteModel;



                        //switch (bidQuoteModel)
                        //{
                        //    case "Manual":
                                if (binding.ResolvedSourcePropertyName == "manualBid")
                                {
                                    if (string.IsNullOrEmpty(detail.Data.manualBid) || !MMCommonUtils.CheckDecimalValid(detail.Data.manualBid,9999,-99.9999m,4))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                            //    else if (binding.ResolvedSourcePropertyName == "bidAlgoDefenValue")
                            //    {
                            //        return new ValidationResult(true, null);
                            //    }
                            //    else if (binding.ResolvedSourcePropertyName == "bidAlgoSafeValue")
                            //    {
                            //        return new ValidationResult(true, null);
                            //    }
                            //    else if (binding.ResolvedSourcePropertyName == "bidAlgoTrigValue")
                            //    {
                            //        return new ValidationResult(true, null);
                            //    }

                            //    break;

                            //case "Defend":
                                if (binding.ResolvedSourcePropertyName == "bidAlgoDefenValue")
                                {
                                    //此处是防守报价
                                    if (string.IsNullOrEmpty(detail.Data.bidAlgoDefenValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidAlgoDefenValue,1000,0,2))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                                else if (binding.ResolvedSourcePropertyName == "bidAlgoTrigValue")
                                {
                                    //此处是防守报价
                                    if (string.IsNullOrEmpty(detail.Data.bidAlgoTrigValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidAlgoTrigValue,1000,0,2))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                                else if (binding.ResolvedSourcePropertyName == "bidAlgoSafeValue")
                                {
                                    //此处是防守报价
                                    if (string.IsNullOrEmpty(detail.Data.bidAlgoSafeValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidAlgoSafeValue,1000, -1000, 2))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                        if (binding.ResolvedSourcePropertyName == "bidQdmespSafeValue")
                        {
                            //此处是防守报价
                            if (string.IsNullOrEmpty(detail.Data.bidQdmespSafeValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidQdmespSafeValue, 1000, -1000, 2))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        //    else if (binding.ResolvedSourcePropertyName == "manualBid")
                        //    {
                        //        //此处是手动报价 不做判断
                        //        return new ValidationResult(true, null);
                        //    }
                        //    break;

                        //case "Algorithm":
                        //if (binding.ResolvedSourcePropertyName == "manualBid")
                        //{
                        //    return new ValidationResult(true, null);
                        //}
                        //else if (binding.ResolvedSourcePropertyName == "bidAlgoDefenValue")
                        //{
                        //    return new ValidationResult(true, null);
                        //}
                        //else if (binding.ResolvedSourcePropertyName == "bidAlgoSafeValue")
                        //{
                        //    return new ValidationResult(true, null);
                        //}
                        //else if (binding.ResolvedSourcePropertyName == "bidAlgoTrigValue")
                        //{
                        //    return new ValidationResult(true, null);
                        //}

                        //break;

                        //    case "API":
                        //        if (binding.ResolvedSourcePropertyName == "manualBid")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "bidAlgoDefenValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "bidAlgoSafeValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "bidAlgoTrigValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }

                        //        break;
                        //}


                        //switch (ofrQuoteModel)
                        //{
                        //    case "Manual":
                        if (binding.ResolvedSourcePropertyName == "manualOfr")
                                {
                                    if (string.IsNullOrEmpty(detail.Data.manualOfr) || !MMCommonUtils.CheckDecimalValid(detail.Data.manualOfr,9999,-99.9999m,4))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                            //    else if (binding.ResolvedSourcePropertyName == "ofrAlgoDefenValue")
                            //    {
                            //        return new ValidationResult(true, null);
                            //    }
                            //    else if (binding.ResolvedSourcePropertyName == "ofrAlgoSafeValue")
                            //    {
                            //        return new ValidationResult(true, null);
                            //    }
                            //    else if (binding.ResolvedSourcePropertyName == "ofrAlgoTrigValue")
                            //    {
                            //        return new ValidationResult(true, null);
                            //    }

                            //    break;

                            //case "Defend":
                                if (binding.ResolvedSourcePropertyName == "ofrAlgoDefenValue")
                                {
                                    //此处是防守报价
                                    if (string.IsNullOrEmpty(detail.Data.ofrAlgoDefenValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrAlgoDefenValue,1000,0,2))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                                else if (binding.ResolvedSourcePropertyName == "ofrAlgoTrigValue")
                                {
                                    //此处是防守报价
                                    if (string.IsNullOrEmpty(detail.Data.ofrAlgoTrigValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrAlgoTrigValue,1000,0,2))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                                else if (binding.ResolvedSourcePropertyName == "ofrAlgoSafeValue")
                                {
                                    //此处是防守报价
                                    if (string.IsNullOrEmpty(detail.Data.ofrAlgoSafeValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrAlgoSafeValue,1000, -1000, 2))
                                    {
                                        return new ValidationResult(false, "不合法");
                                    }
                                }
                        if (binding.ResolvedSourcePropertyName == "ofrQdmespSafeValue")
                        {
                            //此处是防守报价
                            if (string.IsNullOrEmpty(detail.Data.ofrQdmespSafeValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrQdmespSafeValue, 1000, -1000, 2))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        //        else if (binding.ResolvedSourcePropertyName == "manualOfr")
                        //        {
                        //            //此处是手动报价 不做判断
                        //            return new ValidationResult(true, null);
                        //        }

                        //        break;

                        //    case "Algorithm":
                        //        if (binding.ResolvedSourcePropertyName == "manualOfr")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "ofrAlgoDefenValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "ofrAlgoSafeValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "ofrAlgoTrigValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }

                        //        break;


                        //    case "API":
                        //        if (binding.ResolvedSourcePropertyName == "manualOfr")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "ofrAlgoDefenValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "ofrAlgoSafeValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }
                        //        else if (binding.ResolvedSourcePropertyName == "ofrAlgoTrigValue")
                        //        {
                        //            return new ValidationResult(true, null);
                        //        }

                        //        break;
                        //}

                        //对算法交易统一处理
                        if (binding.ResolvedSourcePropertyName == "algoMidType")
                        {
                            //此处是算法报价
                            //if (bidQuoteModel == "Algorithm" || ofrQuoteModel == "Algorithm")
                            //{
                                if (string.IsNullOrEmpty(detail.Data.algoMidType) || !MMCommonUtils.IsDecimal(detail.Data.algoMidType))
                                {
                                    return new ValidationResult(false, "不合法");
                                }
                            //}
                            //else { return new ValidationResult(true, null); }
                        }
                        else if (binding.ResolvedSourcePropertyName == "algoBaseSecurityid")
                        {
                            //此处是算法报价
                            if (detail.Data.algoBaseEnable == "1" && string.IsNullOrEmpty(detail.Data.algoBaseSecurityid))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                            else 
                            { 
                                return new ValidationResult(true, null); 
                            }
                        }
                        else if (binding.ResolvedSourcePropertyName == "algoMidDetailDefined")
                        {
                            //此处是算法报价
                            //if (bidQuoteModel == "Algorithm" || ofrQuoteModel == "Algorithm")
                            //{
                                if (string.IsNullOrEmpty(detail.Data.algoMidDetailDefined) || !MMCommonUtils.CheckDecimalValid(detail.Data.algoMidDetailDefined,1000,0,4))
                                {
                                    return new ValidationResult(false, "不合法");
                                }
                            //}
                            //else { return new ValidationResult(true, null); }
                        }
                        else if (binding.ResolvedSourcePropertyName == "algoMidOffsetValue")
                        {
                            //此处是算法报价
                            //if (bidQuoteModel == "Algorithm" || ofrQuoteModel == "Algorithm")
                            //{
                                if (string.IsNullOrEmpty(detail.Data.algoMidOffsetValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.algoMidOffsetValue,1000,-1000,2))
                                {
                                    return new ValidationResult(false, "不合法");
                                }
                            //}
                            //else { return new ValidationResult(true, null); }
                        }
                        else if (binding.ResolvedSourcePropertyName == "algoValue")
                        {
                            //此处是算法报价
                            //if (bidQuoteModel == "Algorithm" || ofrQuoteModel == "Algorithm")
                            //{
                                if (string.IsNullOrEmpty(detail.Data.algoValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.algoValue,1000,0.01m,2))
                                {
                                    return new ValidationResult(false, "不合法");
                                }
                            //}
                            //else { return new ValidationResult(true, null); }
                        }

                        //bid重发阈值
                        if (binding.ResolvedSourcePropertyName == "bidBaseReSendValue")
                        {
                            if (string.IsNullOrEmpty(detail.Data.bidBaseReSendValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidBaseReSendValue,1000,0,2))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }

                        //ofr重发阈值
                        if (binding.ResolvedSourcePropertyName == "ofrBaseReSendValue")
                        {
                            if (string.IsNullOrEmpty(detail.Data.ofrBaseReSendValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrBaseReSendValue, 1000, 0, 2))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }

                        //成交报价设置bid拉宽
                        if (binding.ResolvedSourcePropertyName == "bidBaseWidenValue")
                        {
                            if (string.IsNullOrEmpty(detail.Data.bidBaseWidenValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidBaseWidenValue, 1000, 0, 2))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }

                        //成交报价设置ofr拉宽
                        if (binding.ResolvedSourcePropertyName == "ofrBaseWidenValue")
                        {
                            if (string.IsNullOrEmpty(detail.Data.ofrBaseWidenValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrBaseWidenValue, 1000, 0, 2))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }

                        if (binding.ResolvedSourcePropertyName == "bidDefenCDCValue")
                        {
                            if (("1".Equals(detail.Data.defenCDCEnable)) && (string.IsNullOrEmpty(detail.Data.bidDefenCDCValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidDefenCDCValue, 1000, -1000, 0)))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        if (binding.ResolvedSourcePropertyName == "ofrDefenCDCValue")
                        {
                            if (("1".Equals(detail.Data.defenCDCEnable)) && (string.IsNullOrEmpty(detail.Data.ofrDefenCDCValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrDefenCDCValue, 1000, -1000, 0)))
                            {
                                return new ValidationResult(false, "不合法");
                            }
                        }
                        ////积极报价设置bid
                        //if (binding.ResolvedSourcePropertyName == "bidPosValue")
                        //{
                        //    if (string.IsNullOrEmpty(detail.Data.bidPosValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.bidPosValue, 1000, 0, 2))
                        //    {
                        //        return new ValidationResult(false, "不合法");
                        //    }
                        //}

                        ////积极报价设置bid
                        //if (binding.ResolvedSourcePropertyName == "ofrPosValue")
                        //{
                        //    if (string.IsNullOrEmpty(detail.Data.ofrPosValue) || !MMCommonUtils.CheckDecimalValid(detail.Data.ofrPosValue, 1000, 0, 2))
                        //    {
                        //        return new ValidationResult(false, "不合法");
                        //    }
                        //}
                    }
                }


            }

  
            catch(Exception e)
            {

            }

            return new ValidationResult(true, null);

        }
    }
}
