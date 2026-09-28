using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SalesTrading.UiTests
{
    public sealed class TestStep
    {
        public string Name { get; set; }
        public string Action { get; set; }
        public JObject Parameters { get; set; } = new JObject();
    }

    public sealed class CaseDefinition
    {
        public int Version { get; set; } = 1;
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TestStep> Steps { get; set; } = new List<TestStep>();

        public void Validate()
        {
            if (Version != 1 || !Regex.IsMatch(Id ?? "", "^[a-zA-Z0-9][a-zA-Z0-9_-]{0,63}$"))
            {
                throw new InvalidOperationException("用例 ID 只允许字母、数字、下划线和短横线，最长 64 位；版本必须为 1。");
            }
            if (string.IsNullOrWhiteSpace(Name) || Steps == null || Steps.Count == 0)
            {
                throw new InvalidOperationException("请填写名称并添加至少一个步骤。");
            }
            foreach (TestStep step in Steps)
            {
                if (step == null || string.IsNullOrWhiteSpace(step.Name) || !Actions.Fields.ContainsKey(step.Action ?? ""))
                {
                    throw new InvalidOperationException("每个步骤必须有名称及有效动作。");
                }
                string[] fields = Actions.Fields[step.Action];
                if (step.Parameters == null || step.Parameters.Properties().Any(p => !fields.Contains(p.Name)))
                {
                    throw new InvalidOperationException("步骤参数不匹配：" + step.Name);
                }
                foreach (string field in fields)
                {
                    if (string.IsNullOrWhiteSpace((string)step.Parameters[field]))
                    {
                        throw new InvalidOperationException(step.Name + " 缺少参数：" + field);
                    }
                }
            }
        }

        public static CaseDefinition Read(string file)
        {
            CaseDefinition result = JsonConvert.DeserializeObject<CaseDefinition>(File.ReadAllText(file, Encoding.UTF8));
            if (result == null) throw new InvalidOperationException("用例为空：" + file);
            result.Validate();
            return result;
        }

        public void Save(string file)
        {
            Validate();
            string temporary = file + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(this, Formatting.Indented), new UTF8Encoding(false));
            if (File.Exists(file)) File.Replace(temporary, file, null);
            else File.Move(temporary, file);
        }
    }

    public static class Actions
    {
        public static readonly Dictionary<string, string[]> Fields = new Dictionary<string, string[]>
        {
            { "manual.open", new string[0] },
            { "manual.fill", new[] { "securityId", "price", "quantity", "side", "settlType" } },
            { "manual.click", new[] { "button" } },
            { "quote.assert", new[] { "status", "price" } },
            { "pending.assert", new string[0] },
            { "market.assert", new string[0] },
            { "management.assert", new string[0] },
            { "history.assert", new[] { "minimumCount" } },
            { "parse", new[] { "text", "expectedRows" } },
            { "parse.select", new[] { "row", "selected", "expectedCount" } },
            { "parse.confirm", new[] { "expectedCount" } },
            { "parse.assert", new[] { "securityId", "institution", "trader", "price" } }
        };

        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            { "manual.open", "点击手工报价" }, { "manual.fill", "填写手工报价" },
            { "manual.click", "点击报价操作按钮" }, { "quote.assert", "核对服务端报价状态及价格" },
            { "pending.assert", "核对报价确认页推送" }, { "market.assert", "核对行情中心推送" },
            { "management.assert", "核对报价管理推送" }, { "history.assert", "核对报价变更历史" },
            { "parse", "输入文本并点击识别报价" }, { "parse.select", "勾选/取消识别结果" },
            { "parse.confirm", "点击确认报价并核对入库" }, { "parse.assert", "核对识别债券、机构、交易员" }
        };

        public static string Hint = "每例自动登录、收集截图及日志并清理本例报价。参数值支持 ${securityId}。" +
            "\r\nbutton：保存待确认/确认/保存修改/Refer/Unrefer/删除；row 从 1 开始；selected：true/false。" +
            "\r\nside：1=Bid，2=Ofr；settlType：1=T+0，2=T+1；机构和交易员填写 CRM 真实名称。";
    }
}
