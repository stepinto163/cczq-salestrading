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
        // 页面路径按产品导航排序；旧用例仍可读取，并归入未分类。
        public string Page { get; set; } = "未分类";
        public string Feature { get; set; } = "通用";
        public string Priority { get; set; } = "P1";
        public List<TestStep> Steps { get; set; } = new List<TestStep>();

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Page) || Page.Split('/').Any(part => string.IsNullOrWhiteSpace(part))
                || string.IsNullOrWhiteSpace(Feature) || !new[] { "P0", "P1", "P2" }.Contains(Priority))
            {
                throw new InvalidOperationException("请填写页面路径、功能和优先级（P0/P1/P2）。");
            }
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
                    if (step.Action == "realtime.lifecycle" && field == "source")
                    {
                        continue; // 默认精确报价；QT 通过真实接入接口准备私有待确认记录。
                    }
                    if (step.Action == "qtrade.publish" && field == "groupId")
                    {
                        continue; // 空群号表示单聊，收件人由实际登录用户确定。
                    }
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
            { "realtime.lifecycle", new[] { "page", "tab", "view", "securityId", "price", "source" } },
            { "page.open", new[] { "page", "tab", "view" } },
            { "fuzzy.preview", new string[0] },
            { "precise.input", new string[0] },
            { "qtrade.publish", new[] { "appId", "senderQtAccount", "groupId", "mode", "securityId", "symbol", "price", "expectedRows" } },
            { "qtrade.confirmPublished", new[] { "contactName", "institutionName" } },
            { "management.edit", new[] { "securityId", "price", "quantity", "side", "settlType" } },
            { "management.save", new string[0] },
            { "management.reject", new[] { "button", "message" } },
            { "management.operate", new[] { "button" } },
            { "management.select", new string[0] },
            { "filter.prepare", new[] { "securityId" } },
            { "filter.combination", new[] { "page", "tab", "view", "ranges", "tags", "search", "expected", "reset" } },
            { "filter.latency", new[] { "quotes", "samples", "securityId" } },
            { "quote.assert", new[] { "status", "price" } },
            { "pending.assert", new string[0] },
            { "pending.toolbar", new string[0] },
            { "groups.manage", new string[0] },
            { "market.assert", new string[0] },
            { "recommendation.assert", new string[0] },
            { "recommendation.typography", new string[0] },
            { "management.assert", new string[0] },
            { "history.assert", new[] { "minimumCount" } },
            { "history.dialog", new string[0] },
            { "precise.crm", new[] { "trader" } },
            { "parse.keyboard", new[] { "text", "expectedRows" } },
            { "parse.toolbar", new[] { "control", "value" } },
            { "parse", new[] { "text", "expectedRows" } },
            { "parse.select", new[] { "row", "selected", "expectedCount" } },
            { "parse.confirm", new[] { "expectedCount" } },
            { "parse.assert", new[] { "securityId", "institution", "trader", "price" } },
            { "quote.fields", new[] { "side", "quantity", "settlType" } },
            { "sidebar.range", new[] { "page", "field", "min", "max", "visible" } },
            { "sidebar.reset", new[] { "page" } },
            { "sidebar.search", new[] { "page", "text", "visible" } },
            { "parse.row", new[] { "row", "securityId", "side", "price", "quantity" } },
            { "parse.count", new[] { "rows", "selected" } },
            { "parse.click", new[] { "button" } },
            { "parse.warning", new[] { "message" } },
            { "parse.settlement", new[] { "row", "settlement", "dayOffset", "priceType" } },
            { "notification.prepare", new[] { "securityId" } },
            { "notification.read", new string[0] },
            { "qtrade.negotiation", new string[0] },
            { "qtrade.business-key", new string[0] }
        };

        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            { "realtime.lifecycle", "高级双账号：确认、Refer、Unrefer、删除的实时推送与隔离" },
            { "page.open", "切换实际页签并核对页面、查询状态及表格" },
            { "fuzzy.preview", "模糊报价预览：原文、选择计数、批量范围与删除" },
            { "precise.input", "精确报价输入：示例、清空、保留原文与页面切换" },
            { "qtrade.publish", "发布真实 QTrade 意向（groupId 为空则发给登录用户）" },
            { "qtrade.confirmPublished", "确认收到的 QTrade 报价并核对行情" },
            { "management.edit", "报价管理打开修改表单并填写" },
            { "management.save", "保存报价修改" },
            { "management.reject", "核对修改拒绝且版本不变" },
            { "management.operate", "报价管理Refer/Unrefer/删除" },
            { "quote.assert", "核对服务端报价状态及价格" },

            { "groups.manage", "关注债券与筛选分组：保存、重读、激活和取消" },
            { "pending.toolbar", "验证报价确认工具栏全部编辑控件、持久化与删除" },
            { "management.select", "选中本例管理报价，验证筛选刷新清除选择" },
            { "filter.prepare", "通过精确报价准备六笔组合筛选样本" },
            { "filter.combination", "操作组合筛选并核对精确报价集合（支持最优价格）" },
            { "filter.latency", "组合筛选延迟（需显式授权，最多500报价）" },
            { "pending.assert", "核对报价确认页推送" }, { "market.assert", "核对行情中心推送" },
            { "management.assert", "核对报价管理推送" }, { "history.assert", "核对报价变更历史" },
            { "history.dialog", "打开报价日志并核对单页边界、刷新和关闭" },
            { "precise.crm", "精确报价行跨机构搜索交易员并回填机构" },
            { "parse.keyboard", "通过 Enter 换行后识别" },
            { "parse.toolbar", "修改精确报价批量工具栏" },
            { "parse", "输入文本并点击识别报价" }, { "parse.select", "勾选/取消识别结果" },
            { "parse.confirm", "点击确认报价并核对入库" }, { "parse.assert", "核对识别债券、机构、交易员" },
            { "quote.fields", "核对持久化方向、数量及清算" },
            { "sidebar.range", "通过侧栏输入价格区间并核对本例报价" },
            { "sidebar.reset", "点击清除过滤并核对报价恢复" },
            { "sidebar.search", "通过侧栏文本搜索并核对本例报价" },
            { "parse.row", "核对指定识别行字段" },
            { "parse.count", "核对预览条数和选中数" },
            { "parse.click", "点击识别页工具栏按钮" },
            { "parse.warning", "核对识别警告" },
            { "parse.settlement", "核对识别交易日期、清算和计价方式" },
            { "notification.prepare", "准备独立关注分组（自动恢复偏好）" },
            { "notification.read", "核对本例提醒推送并通过界面标记已读" },
            { "qtrade.negotiation", "QT 双边提案经 GW 入库、UI 确认及行情隔离" },
            { "qtrade.business-key", "QT 不同 NegotiationID 按完整业务键确认替换" }
        };

        public static string Hint = "每例自动登录、收集截图及日志并清理本例报价。参数值支持 ${securityId}。" +
            "\r\nmanagement.operate 的 button：Refer/Unrefer/删除；management.reject 的 button：保存修改；row 从 1 开始；selected：true/false。" +
            "\r\nside：1=Bid，2=Ofr；settlType：1=T+0，2=T+1；机构和交易员填写 CRM 真实名称。" +
            "\r\n侧栏 page：mgmt/market；visible：true/false；range 的 min/max 均必填；parse.row 的 side：Bid/Ofr。" +
            "\r\nfilter.combination：ranges/tags 填 JSON 对象文本；区间为两个字符串，空字符串表示无界；" +
            "expected 填 B1,B2 等别名或 NONE；reset=false 保留未指定条件；search=<empty> 清空搜索。计价方式单选，清算速度可多选。";
    }
}
