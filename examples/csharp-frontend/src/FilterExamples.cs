using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ficc.FrontendLearning
{
    // 学习入口：业务属性 -> filter.conditions。示例值可按联调数据修改。
    public static class FilterExamples
    {
        public static readonly string[] Names =
        {
            "不限属性", "价格区间", "券种与流动性", "信用评级", "条款", "区域", "组合条件", "远期", "中债偏离", "付息与免税"
        };

        public static JObject Build(string name, string securityId = "")
        {
            var filter = QuoteExamples.BondFilter(securityId);
            var conditions = (JArray)filter["conditions"];
            switch (name)
            {
                case "不限属性":
                    break;
                case "价格区间":
                    conditions.Add(Range("price", "2.0000", "3.0000"));
                    break;
                case "券种与流动性":
                    conditions.Add(AnyOf("bondSubtype", "MTN", "CorporatebBonds"));
                    conditions.Add(AnyOf("liquidityGrade", "S", "A"));
                    break;
                case "信用评级":
                    conditions.Add(AnyOf("bondRating", "AAA", "AA+"));
                    conditions.Add(Condition("issuerRating", "EQ", "AAA"));
                    conditions.Add(Condition("yyRating", "EQ", "INVESTMENT_GRADE"));
                    break;
                case "条款":
                    conditions.Add(Condition("hasExerciseOption", "EQ", false));
                    conditions.Add(Condition("isSubordinated", "EQ", false));
                    conditions.Add(Condition("repaymentType", "EQ", "DQCF"));
                    break;
                case "区域":
                    conditions.Add(Condition("region", "EQ", "310000"));
                    conditions.Add(Condition("isMunicipal", "EQ", true));
                    break;
                case "组合条件":
                    conditions.Add(Range("price", "2.0000", "3.0000"));
                    conditions.Add(AnyOf("bondRating", "AAA", "AA+"));
                    conditions.Add(AnyOf("liquidityGrade", "S", "A", "B"));
                    conditions.Add(Condition("region", "EQ", "310000"));
                    conditions.Add(Condition("hasExerciseOption", "EQ", false));
                    break;
                case "远期":
                    conditions.Add(AnyOf("settlType", "3", "4", "5", "9", "10", "11", "12", "13", "14", "15", "16"));
                    break;
                case "中债偏离":
                    conditions.Add(Range("bidValuationSpread", "-20", "20"));
                    break;
                case "付息与免税":
                    conditions.Add(AnyOf("couponFrequency", "Y1"));
                    conditions.Add(Condition("isTaxExempt", "EQ", true));
                    break;
                default:
                    throw new ArgumentException("Unknown filter example: " + name);
            }
            return filter;
        }

        public static JObject Condition(string field, string operation, JToken value)
        {
            return new JObject
            {
                ["field"] = field,
                ["operator"] = operation,
                ["value"] = value
            };
        }

        // 同字段多条件是 OR；上下限必须放在一个 BETWEEN 中。
        public static JObject Range(string field, string minimum, string maximum)
        {
            return Condition(field, "BETWEEN", new JArray(minimum, maximum));
        }

        public static JObject AnyOf(string field, params string[] values)
        {
            return Condition(field, "IN", new JArray(values.Select(value => new JValue(value))));
        }
    }
}
