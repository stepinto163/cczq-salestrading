using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STC.Common.Utils
{
    public class SignParser
    {
        public static string GenIfRepay(Dictionary<string, string> signDic3)
        {
            string ifRepay = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Consts.IF_REPAY))
                {
                    ifRepay = STC.Common.Utils.CommonUtils.GetTagNameByCode(signDic3[Consts.IF_REPAY], Consts.IF_REPAY);
                }
            }

            return ifRepay;
        }

        public static string GenLevel(Dictionary<string, string> singDic3)
        {
            string level = string.Empty;
            if (singDic3 != null)
            {
                if (singDic3.ContainsKey(Consts.ADMIN_LEVEL))
                {
                    level = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic3[Consts.ADMIN_LEVEL], Consts.ADMIN_LEVEL);
                }
            }

            return level;
        }

        public static string GenRegionType(Dictionary<string, string> singDic2)
        {
            string regionType = string.Empty;
            if (singDic2 != null)
            {
                if (singDic2.ContainsKey(Consts.REGION_TYPE))
                {
                    regionType = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic2[Consts.REGION_TYPE], Consts.REGION_TYPE);
                }
            }

            return regionType;
        }

        public static string GenEnterpriseType(Dictionary<string, string> singDic1)
        {
            string enterpriseType = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Consts.ENTERPRISE_TYPE))
                {
                    enterpriseType = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic1[Consts.ENTERPRISE_TYPE], Consts.ENTERPRISE_TYPE);
                }
            }

            return enterpriseType;
        }

        public static string GenIssueType(Dictionary<string, string> singDic1)
        {
            string issueType = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Consts.ISSUE_TYPE))
                {
                    issueType = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic1[Consts.ISSUE_TYPE], Consts.ISSUE_TYPE);
                }
            }

            return issueType;
        }

        public static bool GenIfGuarantee(Dictionary<string, string> signDic1)
        {
            string IfGuarantee = string.Empty;
            if (signDic1 != null)
            {
                if (signDic1.ContainsKey(Consts.IF_GUARANTEE))
                {
                    IfGuarantee = signDic1[Consts.IF_GUARANTEE];
                }
            }

            return IfGuarantee == "0001";
        }

        public static bool IsCumType(Dictionary<string, string> signDic1)
        {
            bool isCumType = false;
            if (signDic1 != null)
            {
                if (signDic1.ContainsKey(Consts.CUM_TYPE))
                {
                    if (signDic1[Consts.CUM_TYPE] == "01"
                        || signDic1[Consts.CUM_TYPE] == "11")
                    {
                        isCumType = true;
                    }
                }
            }
            return isCumType;
        }

        public static string GetCopyInfo(string qty, STC.Common.Model.Table.WorkBrench.IntentionQuoteInfo er)
        {
            string nonfirmStar = string.Empty;
            if (er.firmFlag == null || er.firmFlag == "0")
            {
                nonfirmStar = "*";
            }
            StringBuilder copyInfo = new StringBuilder();
            string settlTypeDisplay = ComboSourceUtil.Instance.FindEnum(er.settlType, "settletype");
            if (er.settlType != "1" && er.settlType != "2" && er.settlType != "2,3")
            {
                if (copyInfo.Length > 0)
                {
                    copyInfo.Append(",");
                }
                copyInfo.Append(settlTypeDisplay);
            }
            if (er.wholeFlag == "1")
            {
                if (copyInfo.Length > 0)
                {
                    copyInfo.Append(",");
                }
                copyInfo.Append("整量");
            }

            string copyText = string.IsNullOrEmpty(qty) ? "--" : qty;
            if (nonfirmStar.Length > 0)
            {
                copyText = $"{copyText}{nonfirmStar}";
            }
            if (copyInfo.Length > 0)
            {
                copyText = $"{copyText}({copyInfo})";
            }
            return copyText;
        }

        public static string GenBondCategory(Dictionary<string, string> singDic, Dictionary<string, string> singDic1)
        {
            string client = string.Empty;
            string localGovBondTypeDefault = string.Empty;
            string finBondType = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Consts.BOND_SUB_TYPE))
                {
                    client = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic1[Consts.BOND_SUB_TYPE], Consts.BOND_SUB_TYPE);
                }

                if ("金融债" == client)
                {
                    if (singDic.ContainsKey(Consts.FINBONDTYPE))
                    {
                        finBondType = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic[Consts.FINBONDTYPE], Consts.FINBONDTYPE);
                    }

                    if (!string.IsNullOrEmpty(finBondType))
                    {
                        client = $"{client}({finBondType})";
                    }
                }


                if (singDic1.ContainsKey(Consts.LOCAL_GOV_BOND_TYPE))
                {
                    localGovBondTypeDefault = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic1[Consts.LOCAL_GOV_BOND_TYPE], Consts.LOCAL_GOV_BOND_TYPE).Replace("债", "");
                }

                if (!string.IsNullOrEmpty(localGovBondTypeDefault))
                {
                    client = $"{client}({localGovBondTypeDefault})";
                }

            }

            return client;
        }

        public static string GenDQType(Dictionary<string, string> dic, bool isMe, string _innerType)
        {
            string dqtype = string.Empty;

            if (_innerType == "1" || (_innerType == "2" && isMe))
            {
                if (dic != null)
                {
                    if (dic.ContainsKey(Consts.DQTYPE))
                    {
                        if (dic[Consts.DQTYPE] == "0010")
                        {
                            dqtype = "!";
                        }
                    }
                }
            }

            return dqtype;
        }

        public static string GenRating(Dictionary<string, string> singDic1, Dictionary<string, string> signDic3, string securityID)
        {
            string rating_current = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Consts.RATING_CURRENT))
                {
                    rating_current = STC.Common.Utils.CommonUtils.GetTagNameByCode(signDic3[Consts.RATING_CURRENT], Consts.RATING_CURRENT);
                }
            }
            rating_current = string.IsNullOrEmpty(rating_current) ? "--" : rating_current;

            //if (rating_current.StartsWith("其他"))
            //{
            //    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            //    if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Rating_Current))
            //    {
            //        rating_current = bondsInfoPo.Rating_Current;
            //    }
            //    else
            //    {
            //        rating_current = "--";
            //    }
            //}

            string ranking_type = string.Empty;
            if (singDic1 != null)
            {
                if (singDic1.ContainsKey(Consts.RANKING_TYPE))
                {
                    ranking_type = STC.Common.Utils.CommonUtils.GetTagNameByCode(singDic1[Consts.RANKING_TYPE], Consts.RANKING_TYPE);
                }
            }
            ranking_type = string.IsNullOrEmpty(ranking_type) ? "--" : ranking_type;

            //if (ranking_type.StartsWith("其他"))
            //{
            //    BondsInfoPo bondsInfoPo = Common.Utils.CommonUtils.GetBondsInfoPoBySecurityID(securityID);
            //    if (bondsInfoPo != null && !string.IsNullOrEmpty(bondsInfoPo.Issuer_Rating_Current))
            //    {
            //        ranking_type = bondsInfoPo.Issuer_Rating_Current;
            //    }
            //    else
            //    {
            //        ranking_type = "--";
            //    }
            //}

            return $"{ranking_type}/{rating_current}";
        }

        public static string GetCDCImplitedRating(Dictionary<string, string> signDic3)
        {
            string cdc_implited_ratings = string.Empty;
            if (signDic3 != null)
            {
                if (signDic3.ContainsKey(Consts.CDC_IMPLITED_RATING))
                {
                    cdc_implited_ratings = STC.Common.Utils.CommonUtils.GetTagNameByCode(signDic3[Consts.CDC_IMPLITED_RATING], Consts.CDC_IMPLITED_RATING);
                }
            }
            cdc_implited_ratings = string.IsNullOrEmpty(cdc_implited_ratings) ? "--" : cdc_implited_ratings;

            return cdc_implited_ratings;
        }

        public static string GetTaxFree(Dictionary<string, string> signDic4)
        {
            string tax_Free = string.Empty;
            if (signDic4 != null)
            {
                if (signDic4.ContainsKey(Consts.IF_TAX_FREE))
                {
                    tax_Free = signDic4[Consts.IF_TAX_FREE];
                }
            }
            return tax_Free;
        }

    }
}
