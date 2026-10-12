using System;
using System.Globalization;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.Common.Model.Admin;

namespace STC.SalesTrading.Plugin.Ficc
{
    internal static class DpsModelMapper
    {
        public static Dictionary<string, string> UserNames(IEnumerable<StcTbookUsers> users,
            string currentUserId, string currentName, string currentLoginName)
        {
            var names = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (StcTbookUsers user in users ?? new StcTbookUsers[0])
            {
                if (user != null && !string.IsNullOrWhiteSpace(user.Userid))
                {
                    names[user.Userid] = !string.IsNullOrWhiteSpace(user.Name) ? user.Name : user.Username;
                }
            }
            if (!string.IsNullOrEmpty(currentUserId))
            {
                // 登录资料在 AdminSvr 用户目录暂未加载时仍可识别本人的报价。
                if (!string.IsNullOrWhiteSpace(currentName))
                {
                    names[currentUserId] = currentName;
                }
                else if (!names.ContainsKey(currentUserId))
                {
                    names[currentUserId] = currentLoginName;
                }
            }
            return names;
        }

        // 行情档位和报价行共用展示映射，保留对手与本方归属两个独立身份。
        public static JObject MarketQuote(JObject book, JObject entry)
        {
            JObject reference = book["quoteRefs"]?[(string)entry["MDEntryID"]] as JObject;
            return new JObject
            {
                ["securityId"] = book["securityId"]?.DeepClone(),
                ["quoteId"] = reference?["quoteId"]?.DeepClone(),
                ["revision"] = reference?["quoteRevision"]?.DeepClone(),
                ["side"] = (string)entry["MDEntryType"] == "0" ? "1" : "2",
                ["quoteType"] = "1",
                ["yieldType"] = (string)entry["ExerciseFlag"] == "0" ? "2" : "1",
                ["price"] = entry["MDEntryPx"]?.DeepClone(),
                ["quantity"] = entry["MDEntrySize"]?.DeepClone(),
                ["settlType"] = entry["SettlType"]?.DeepClone(),
                ["updateTime"] = book["updateTime"]?.DeepClone(),
                ["ownerUserId"] = entry["UserID"]?.DeepClone(),
                ["institutionId"] = entry["InstitutionID"]?.DeepClone(),
                ["institutionName"] = entry["InstitutionName"]?.DeepClone(),
                ["contactId"] = entry["TraderID"]?.DeepClone(),
                ["contactName"] = entry["TraderName"]?.DeepClone()
            };
        }

        public static void ApplyOwnerDisplay(JObject quote, IDictionary<string, string> names, string currentUserId)
        {
            string ownerId = (string)quote["ownerUserId"];
            quote["isMine"] = !string.IsNullOrEmpty(ownerId)
                && string.Equals(ownerId, currentUserId, StringComparison.Ordinal);
            if (!string.IsNullOrEmpty(ownerId))
            {
                // 用户资料暂不可用时明确显示原 userid，不能把当前登录人或对手误当作归属人。
                quote["ownerUserName"] = names.TryGetValue(ownerId, out string name)
                    && !string.IsNullOrWhiteSpace(name) ? name : ownerId;
            }
        }

        public static JObject Display(JObject source)
        {
            JObject row = new JObject();
            string[] mappings =
            {
                "QuoteId|quoteId", "Revision|revision", "Code|securityId", "Name|securityName", "Price|price", "Volume|quantity",
                "Institution|institutionName", "Trader|contactName", "InstitutionId|institutionId", "ContactId|contactId",
                "WallTrader|ownerUserName", "OwnerAccountId|ownerUserId", "IsMine|isMine",
                "LastPrice|lastTradePrice", "BondLibrary|bondSubtype", "IssuerRating|issuerRating",
                "BondRating|bondRating", "NzNetPrice|valuationCleanPrice", "NzYtm|valuationYield", "CsNetPrice|csiValuationCleanPrice",
                "CsFullPrice|csiValuationFullPrice", "CsYtm|csiValuationYield", "QuoteTime|updateTime", "TradeDate|tradeDate"
            };
            foreach (string mapping in mappings)
            {
                string[] names = mapping.Split('|');
                if (source[names[1]] != null && source[names[1]].Type != JTokenType.Null)
                {
                    row[names[0]] = source[names[1]].DeepClone();
                }
            }
            string sourceType = (string)source["source"]?["type"];
            row["Mode"] = (int)(sourceType == "QT" ? QuoteMode.Qt : sourceType == "IDEAL" ? QuoteMode.Ideal : QuoteMode.Manual);
            row["Status"] = (int)((string)source["status"] == "REFERRED" ? QuoteStatus.Revoked : QuoteStatus.Active);
            row["Maturity"] = source["remainingYears"] == null ? "" : source["remainingYears"] + "Y";
            row["Direction"] = (int)((string)source["side"] == "2" ? QuoteDirection.Ofr : QuoteDirection.Bid);
            row["PriceType"] = (int)((string)source["quoteType"] == "2" ? PriceType.CleanPrice
                : (string)source["quoteType"] == "3" ? PriceType.DirtyPrice
                : (string)source["yieldType"] == "2" ? PriceType.YieldToExercise : PriceType.YieldToMaturity);
            string settlement = (string)source["settlType"];
            row["Settlement"] = (int)(settlement == "1" ? SettlementType.T0 : settlement == "2" ? SettlementType.T1
                : settlement == "3" ? SettlementType.T2 : settlement == "4" ? SettlementType.T3 : SettlementType.Forward);
            row["SettlementCode"] = settlement;
            row["NzDeviation"] = source[(string)source["side"] == "2" ? "offerValuationSpread" : "bidValuationSpread"]?.DeepClone();
            QuoteFlag flags = QuoteFlag.None;
            if ((bool?)source["urgent"] == true)
            {
                flags |= QuoteFlag.Urgent;
            }
            if ((bool?)source["recommended"] == true)
            {
                flags |= QuoteFlag.Recommend;
            }
            if ((bool?)source["whole"] == true)
            {
                flags |= QuoteFlag.Whole;
            }
            if ((bool?)source["oco"] == true)
            {
                flags |= QuoteFlag.Oco;
            }
            if ((bool?)source["packing"] == true)
            {
                flags |= QuoteFlag.Packing;
            }
            row["Flags"] = (int)flags;
            row["RecognitionId"] = source["quoteId"]?.DeepClone();
            string sourceMessageId = (string)source["sourceMessageId"];
            row["BatchId"] = string.IsNullOrWhiteSpace(sourceMessageId)
                ? (string)source["quoteId"] : sourceMessageId;
            row["SourceType"] = sourceType ?? "MANUAL";
            row["ReceivedAt"] = source["createTime"]?.DeepClone();
            row["TextContent"] = source["source"]?["text"]?.Type == JTokenType.String
                ? source["source"]["text"].DeepClone() : source["remark"]?.DeepClone();
            return row;
        }

        public static JObject Input(QuoteConfirmRow row)
        {
            if (row.PriceType == PriceType.Spread || row.PriceType == PriceType.Par)
            {
                throw new NotSupportedException("QuoteSvr尚未支持利差／面值录入，请选择收益率、净价或全价。");
            }
            string settlement = row.Settlement == SettlementType.T0 ? "1" : row.Settlement == SettlementType.T1 ? "2"
                : row.Settlement == SettlementType.T2 ? "3" : row.Settlement == SettlementType.T3 ? "4" : row.SettlementCode;
            if (string.IsNullOrEmpty(settlement))
            {
                throw new InvalidOperationException("远期必须选择具体清算代码，不能用“远期”代替实际日期。");
            }
            return new JObject
            {
                ["securityId"] = row.Code, ["side"] = row.Direction == QuoteDirection.Bid ? "1" : "2",
                ["quoteType"] = row.PriceType == PriceType.CleanPrice ? "2" : row.PriceType == PriceType.DirtyPrice ? "3" : "1",
                ["yieldType"] = row.PriceType == PriceType.YieldToExercise ? "2"
                    : row.PriceType == PriceType.YieldToMaturity ? "1" : null,
                ["price"] = row.Price?.ToString(CultureInfo.InvariantCulture),
                ["quantity"] = row.Volume?.ToString(CultureInfo.InvariantCulture), ["quantityUnit"] = "WAN", ["currency"] = "CNY",
                ["settlType"] = settlement, ["tradeDate"] = (row.TradeDate ?? DateTime.Today).ToString("yyyy-MM-dd"),
                ["institutionId"] = row.InstitutionId, ["institutionName"] = row.Institution,
                ["contactId"] = row.ContactId, ["contactName"] = row.Trader,
                ["urgent"] = row.Flags.HasFlag(QuoteFlag.Urgent), ["recommended"] = row.Flags.HasFlag(QuoteFlag.Recommend),
                ["whole"] = row.Flags.HasFlag(QuoteFlag.Whole), ["oco"] = row.Flags.HasFlag(QuoteFlag.Oco),
                ["packing"] = row.Flags.HasFlag(QuoteFlag.Packing),
                ["remark"] = row.TextContent
            };
        }
    }
}
