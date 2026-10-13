using System.Collections.Generic;
using Newtonsoft.Json;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    /// <summary>统一基础资料/CRM记录；扩展属性由字段字典定义，未识别属性在序列化边界保留。</summary>
    public class ReferenceRecord : ProtocolModel
    {
        [JsonProperty("bidOfferSpread")]
        public string BidOfferSpread { get; set; }

        [JsonProperty("market")]
        public string Market { get; set; }

        [JsonProperty("valuationBasis")]
        public string ValuationBasis { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("securityId")]
        public string SecurityId { get; set; }

        [JsonProperty("securityName")]
        public string SecurityName { get; set; }

        [JsonProperty("institutionId")]
        public string InstitutionId { get; set; }

        [JsonProperty("institutionName")]
        public string InstitutionName { get; set; }

        [JsonProperty("institutionShortName")]
        public string InstitutionShortName { get; set; }

        [JsonProperty("contactId")]
        public string ContactId { get; set; }

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("positionName")]
        public string PositionName { get; set; }

        [JsonProperty("institutionIds")]
        public List<string> InstitutionIds { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("shortName")]
        public string ShortName { get; set; }

        [JsonProperty("remainingYears")]
        public string RemainingYears { get; set; }

        [JsonProperty("bondType")]
        public string BondType { get; set; }

        [JsonProperty("bondSubtype")]
        public string BondSubtype { get; set; }

        [JsonProperty("valuationYield")]
        public string ValuationYield { get; set; }

        [JsonProperty("valuationCleanPrice")]
        public string ValuationCleanPrice { get; set; }

        [JsonProperty("couponRate")]
        public string CouponRate { get; set; }

        [JsonProperty("issuerId")]
        public string IssuerId { get; set; }

        [JsonProperty("issuerName")]
        public string IssuerName { get; set; }

        [JsonProperty("province")]
        public string Province { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("ownershipType")]
        public string OwnershipType { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("isHighYield")]
        public bool? IsHighYield { get; set; }

        [JsonProperty("isPreListing")]
        public bool? IsPreListing { get; set; }

        [JsonProperty("outstandingAmount")]
        public string OutstandingAmount { get; set; }

        [JsonProperty("regionCategory")]
        public string RegionCategory { get; set; }

        [JsonProperty("localGovernmentRegion")]
        public string LocalGovernmentRegion { get; set; }

        [JsonProperty("isTaxExempt")]
        public bool? IsTaxExempt { get; set; }

        [JsonProperty("couponFrequency")]
        public string CouponFrequency { get; set; }

        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("hotTags")]
        public List<string> HotTags { get; set; }

        [JsonProperty("ncdType")]
        public string NcdType { get; set; }

        [JsonProperty("listingStatus")]
        public string ListingStatus { get; set; }

        [JsonProperty("issueMethod")]
        public string IssueMethod { get; set; }

        [JsonProperty("csiValuationYield")]
        public string CsiValuationYield { get; set; }

        [JsonProperty("csiValuationCleanPrice") ]
        public string CsiValuationCleanPrice { get; set; }

        [JsonProperty("csiValuationFullPrice") ]
        public string CsiValuationFullPrice { get; set; }

        [JsonProperty("csiValuationDate") ]
        public string CsiValuationDate { get; set; }

        [JsonProperty("csiExerciseYield") ]
        public string CsiExerciseYield { get; set; }

        [JsonProperty("csiExerciseCleanPrice") ]
        public string CsiExerciseCleanPrice { get; set; }

        [JsonProperty("csiExerciseFullPrice") ]
        public string CsiExerciseFullPrice { get; set; }

        [JsonProperty("csiRecommendedBasis") ]
        public string CsiRecommendedBasis { get; set; }

        [JsonProperty("csiValuationStatus") ]
        public string CsiValuationStatus { get; set; }


        [JsonProperty("liquidityGrade")]
        public string LiquidityGrade { get; set; }

        [JsonProperty("marketMakerTags")]
        public List<string> MarketMakerTags { get; set; }

        [JsonProperty("listedMarket")]
        public string ListedMarket { get; set; }

        [JsonProperty("issuerType")]
        public string IssuerType { get; set; }

        [JsonProperty("bankType")]
        public string BankType { get; set; }

        [JsonProperty("industry")]
        public string Industry { get; set; }

        [JsonProperty("guaranteeType")]
        public string GuaranteeType { get; set; }

        [JsonProperty("guarantorName")]
        public string GuarantorName { get; set; }

        [JsonProperty("issuerRating")]
        public string IssuerRating { get; set; }

        [JsonProperty("issuerCbrRating")]
        public string IssuerCbrRating { get; set; }

        [JsonProperty("ratingOutlook")]
        public string RatingOutlook { get; set; }

        [JsonProperty("bondRating")]
        public string BondRating { get; set; }

        [JsonProperty("bondCbrRating")]
        public string BondCbrRating { get; set; }

        [JsonProperty("yyRating")]
        public string YyRating { get; set; }

        [JsonProperty("csiRating")]
        public string CsiRating { get; set; }

        [JsonProperty("optionType")]
        public string OptionType { get; set; }

        [JsonProperty("hasExerciseOption")]
        public bool? HasExerciseOption { get; set; }

        [JsonProperty("repaymentType")]
        public string RepaymentType { get; set; }

        [JsonProperty("couponType")]
        public string CouponType { get; set; }

        [JsonProperty("perpetualType")]
        public string PerpetualType { get; set; }

        [JsonProperty("isPerpetual")]
        public bool? IsPerpetual { get; set; }

        [JsonProperty("isSubordinated")]
        public bool? IsSubordinated { get; set; }

        [JsonProperty("rateIndex")]
        public string RateIndex { get; set; }

        [JsonProperty("issueYear")]
        public string IssueYear { get; set; }

        [JsonProperty("isMunicipal")]
        public bool? IsMunicipal { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("regionCode")]
        public string RegionCode { get; set; }

        [JsonProperty("keyProvince")]
        public string KeyProvince { get; set; }

        [JsonProperty("municipalLevel")]
        public string MunicipalLevel { get; set; }

        [JsonProperty("regionalRating")]
        public string RegionalRating { get; set; }

        [JsonProperty("localGovernmentType")]
        public string LocalGovernmentType { get; set; }

        [JsonProperty("localGovernmentUse")]
        public string LocalGovernmentUse { get; set; }

        [JsonProperty("isCrossMarket")]
        public bool? IsCrossMarket { get; set; }

        [JsonProperty("isMortgage")]
        public bool? IsMortgage { get; set; }

    }
}
