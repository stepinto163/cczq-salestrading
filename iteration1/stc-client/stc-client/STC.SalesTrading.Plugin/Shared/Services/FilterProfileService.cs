using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 关注债券分组 / 筛选分组的会话内存储（原型 favBondGroupsCredit/Rate、filterGroupsCredit/Rate 字段）。
    /// 后端契约到位前数据仅存内存（与 Mock Store 同策略）；保存 = 用弹窗工作副本整体替换。
    /// 同时提供筛选分组管理右栏的「条件目录」（原型 availableMarketFilterGroupData：从侧栏 DOM 抽取，
    /// WPF 侧为静态数据，2026-09-12 按原型 DOM 全量对齐）。
    /// </summary>
    public static class FilterProfileService
    {
        public enum MarketKind { Credit, Rate }

        /// <summary>分组数据变化（保存整体替换 / 右键建组加入）时通知：侧栏 chip 重建、激活集失效清理。</summary>
        public static event Action DataChanged;

        /// <summary>外部（右键菜单等不经管理弹窗的入口）改完集合后手动触发 <see cref="DataChanged"/>。</summary>
        public static void RaiseDataChanged()
        {
            DataChanged?.Invoke();
        }

        private static readonly ObservableCollection<FavBondGroup> _favCredit = new ObservableCollection<FavBondGroup>();
        private static readonly ObservableCollection<FavBondGroup> _favRate = new ObservableCollection<FavBondGroup>();
        private static readonly ObservableCollection<FilterNamedGroup> _groupsCredit = new ObservableCollection<FilterNamedGroup>();
        private static readonly ObservableCollection<FilterNamedGroup> _groupsRate = new ObservableCollection<FilterNamedGroup>();

        static FilterProfileService()
        {
            // 种子数据：原型侧栏默认有「我的关注」chip（我的关注 · 10个条件）；先给一组示例条件，
            // 后端接入后由服务端数据替换
            _groupsCredit.Add(BuildSeedGroup());
            _favCredit.Add(BuildSeedFavGroup());
        }

        private static FilterNamedGroup BuildSeedGroup()
        {
            var g = new FilterNamedGroup { Name = "我的关注" };
            var sg = new FilterSubgroup();
            sg.Conditions.Add(new FilterCondition("tag", "热门标签", "城投"));
            sg.Conditions.Add(new FilterCondition("ent", "企业性质", "国企"));
            sg.Conditions.Add(new FilterCondition("rt", "剩余期限", "1-3Y"));
            sg.Conditions.Add(new FilterCondition("mkt", "上市场所", "银行间"));
            g.Subgroups.Add(sg);
            return g;
        }

        private static FavBondGroup BuildSeedFavGroup()
        {
            var g = new FavBondGroup { Name = "我的关注" };
            return g;
        }

        public static ObservableCollection<FavBondGroup> GetFavGroups(MarketKind market)
            => market == MarketKind.Rate ? _favRate : _favCredit;

        public static ObservableCollection<FilterNamedGroup> GetFilterGroups(MarketKind market)
            => market == MarketKind.Rate ? _groupsRate : _groupsCredit;

        /// <summary>保存关注分组（整体替换；弹窗取消时不调用）。</summary>
        public static void SaveFavGroups(MarketKind market, IEnumerable<FavBondGroup> groups)
        {
            var target = GetFavGroups(market);
            target.Clear();
            foreach (var g in groups)
            {
                target.Add(g);
            }
            DataChanged?.Invoke();
        }

        /// <summary>保存筛选分组（整体替换；弹窗取消时不调用）。</summary>
        public static void SaveFilterGroups(MarketKind market, IEnumerable<FilterNamedGroup> groups)
        {
            var target = GetFilterGroups(market);
            target.Clear();
            foreach (var g in groups)
            {
                target.Add(g);
            }
            DataChanged?.Invoke();
        }

        // ============ 条件目录（原型 availableMarketFilterGroupData 的 WPF 静态版） ============
        // 2026-09-12 按原型侧栏 DOM 全量重对齐（证据 .tmp/shots/_catalog_proto2.txt，提取脚本 _extract_catalog2.py）：
        //  1) 目录不含「报价筛选」（原型 MARKET_FILTER_GROUP_SECTIONS 只放行 债券/流动性/信用/条款/区域）；
        //  2) key 一律用原型 data-bond-filter-key（type/tag/rt/liq/mkt/city/...），置灰组无 key（空串）；
        //  3) 单项置灰 = filter-control-unsupported（TagSpec 的 "!" 前缀）；
        //     整组置灰 = filter-group-unsupported（Enabled=false + 全部项置灰）；
        //  4) 「地区」原型是 3807 项省/市/区县级联下拉，静态目录先取 [省级] 34 项，市/区县级联待接；
        //  5) 条目顺序与 DOM 一致（置灰项保留原位）。

        public static List<CatalogSection> GetCatalog(MarketKind market)
        {
            bool credit = market == MarketKind.Credit;
            var sections = new List<CatalogSection>();
            sections.Add(credit ? BuildCreditBondSection() : BuildRateBondSection());
            sections.Add(BuildLiquiditySection(credit));
            if (credit)
            {
                sections.Add(BuildCreditSection());
            }
            sections.Add(BuildClauseSection(credit));
            sections.Add(BuildRegionSection(credit));
            return sections;
        }

        private static CatalogSection BuildCreditBondSection()
        {
            var s = new CatalogSection("债券筛选");
            s.Groups.Add(Tags("type", "细分券种", "短融", "中票", "企业债", "公司债", "金融债", "PPN", "ABS", "可转债", "NCD", "其他"));
            s.Groups.Add(TagSpec("tag", "热门标签",
                "商金债", "银行二级", "银行永续", "政策性次级", "保险次级", "券商债", "保险债", "铁道债",
                "!可质押", "跨市场", "城投", "房地产", "产能过剩", "公用事业", "上市公司", "!科创债", "TLAC债",
                "!新上市", "!不含假期", "!绿色债"));
            s.Groups.Add(DisabledGroup("NCD类型", "国有", "股份", "城商", "农商", "外资", "其他"));
            s.Groups.Add(TermTags());
            s.Groups.Add(Text("issuer", "发行人"));
            s.Groups.Add(TagSpec("listed", "债券上市状态", "上市", "!非上市"));
            s.Groups.Add(TagSpec("offer", "发行方式", "公募", "!小公募", "私募"));
            return s;
        }

        private static CatalogSection BuildRateBondSection()
        {
            var s = new CatalogSection("债券筛选");
            s.Groups.Add(Tags("type", "细分券种", "国债", "国开", "农发", "口行", "地方债"));
            s.Groups.Add(Tags("tax", "是否免税", "免税", "非免税"));
            s.Groups.Add(Tags("lg_type", "地方债类型", "一般债", "专项债"));
            s.Groups.Add(DisabledSelect("地方债专项", "取值逻辑待确认", "棚改", "土储", "收费公路", "其他"));
            s.Groups.Add(TermTags());
            s.Groups.Add(Text("issuer", "发行人"));
            return s;
        }

        /// <summary>剩余期限（原型 key=rt）：11 档标签 + 自定义区间（Y/M/D 单位折算成年）。</summary>
        private static CatalogConditionGroup TermTags()
        {
            var g = Tags("rt", "剩余期限", "<3M", "3-6M", "6-9M", "9-12M", "1-3Y", "3-5Y", "5-7Y", "7-10Y", "10-15Y", "15-20Y", ">20Y");
            g.HasRange = true;
            g.HasUnitSelect = true;
            return g;
        }

        private static CatalogSection BuildLiquiditySection(bool credit)
        {
            var s = new CatalogSection("流动性筛选");
            s.Groups.Add(Tags("liq", "债券流动性", "S", "A", "B", "C", "D"));
            if (credit)
            {
                s.Groups.Add(Tags("market_maker", "做市标的", "上证信用", "深证信用"));
            }
            s.Groups.Add(Tags("mkt", "上市场所", "银行间", "上交所", "深交所", "北交所"));
            return s;
        }

        private static CatalogSection BuildCreditSection()
        {
            var s = new CatalogSection("信用筛选");
            s.Groups.Add(DisabledGroup("银行业", "国有行", "股份制", "城商行", "农商行"));
            s.Groups.Add(Select("ind", "行业", "城投", "房地产", "煤炭", "钢铁", "化工", "建筑", "交运", "公用事业"));
            s.Groups.Add(Tags("ent", "企业性质", "央企", "国企", "民企", "其他"));
            s.Groups.Add(TagSpec("gur", "担保", "有担保", "!无担保", "!全国性担保", "!地方性担保"));
            s.Groups.Add(Text("guarantor", "担保人"));
            s.Groups.Add(DisabledGroup("主体评级", "AAA", "AA+", "AA", "AA-", "A+", "A", "更多"));
            s.Groups.Add(DisabledGroup("中债资信", "AAA+", "AAA", "AAA-", "AA+", "更多"));
            s.Groups.Add(DisabledGroup("评级展望", "正面", "稳定", "观察", "负面"));
            s.Groups.Add(DisabledGroup("债项评级", "AAA", "A-1", "AA+", "AA", "AA-", "更多"));
            s.Groups.Add(Tags("cir", "中债隐含", "AAA+", "AAA", "AAA-", "AA+", "更多"));
            s.Groups.Add(DisabledGroup("YY评分", "投资级", "投机级", "风险级"));
            s.Groups.Add(Tags("csir", "中证隐含", "AAA+", "AAA", "AA+", "AA", "AA2", "更多"));
            return s;
        }

        private static CatalogSection BuildClauseSection(bool credit)
        {
            var s = new CatalogSection("条款筛选");
            s.Groups.Add(TagSpec("opt", "含权属性", "!不含权", "含权", "赎回权", "回售权", "!其他权"));
            s.Groups.Add(Tags("repay", "偿还方式", "到期", "提前还本"));
            s.Groups.Add(Tags("interest_pay", "付息方式", "按月", "按季", "半年", "按年", "其他"));
            if (credit)
            {
                s.Groups.Add(TagSpec("perp", "永续债", "城投永续", "产业永续", "金融永续", "!非永续"));
                s.Groups.Add(Tags("sub", "次级债", "次级", "非次级"));
            }
            s.Groups.Add(Tags("rate", "利率方式", "SHIBOR", "DEPO", "LPR", "固息"));
            s.Groups.Add(DisabledGroup("发行年份", "2026", "2025", "2024", "2023", "更多"));
            return s;
        }

        private static CatalogSection BuildRegionSection(bool credit)
        {
            var s = new CatalogSection("区域筛选");
            if (credit)
            {
                s.Groups.Add(Tags("city", "城投", "城投", "非城投"));
                s.Groups.Add(Tags("key_province", "重点省份", "12重点省", "非12重点省"));
                s.Groups.Add(DisabledGroup("城投层级", "省级", "市级", "区县级", "其他"));
            }
            s.Groups.Add(DisabledSelect("地区等级", "取值逻辑待确认"));
            s.Groups.Add(Select("region", "地区",
                "[省级] 上海市", "[省级] 云南省", "[省级] 内蒙古自治区", "[省级] 北京市", "[省级] 台湾省",
                "[省级] 吉林省", "[省级] 四川省", "[省级] 天津市", "[省级] 宁夏回族自治区", "[省级] 安徽省",
                "[省级] 山东省", "[省级] 山西省", "[省级] 广东省", "[省级] 广西壮族自治区", "[省级] 新疆维吾尔自治区",
                "[省级] 江苏省", "[省级] 江西省", "[省级] 河北省", "[省级] 河南省", "[省级] 浙江省",
                "[省级] 海南省", "[省级] 湖北省", "[省级] 湖南省", "[省级] 澳门特别行政区", "[省级] 甘肃省",
                "[省级] 福建省", "[省级] 西藏自治区", "[省级] 贵州省", "[省级] 辽宁省", "[省级] 重庆市",
                "[省级] 陕西省", "[省级] 青海省", "[省级] 香港特别行政区", "[省级] 黑龙江省"));
            if (credit)
            {
                s.Groups.Add(DisabledSelect("省份", "当前筛选条件不可用",
                    "北京市", "上海市", "广东省", "江苏省", "浙江省", "山东省", "四川省", "湖北省", "湖南省",
                    "河南省", "河北省", "福建省", "安徽省", "江西省", "山西省", "陕西省", "重庆市", "天津市",
                    "云南省", "贵州省", "广西壮族自治区", "甘肃省", "吉林省", "黑龙江省", "辽宁省",
                    "内蒙古自治区", "宁夏回族自治区", "青海省", "西藏自治区", "新疆维吾尔自治区", "海南省",
                    "深圳市", "厦门市", "宁波市", "青岛市", "大连市"));
            }
            return s;
        }

        // ---- 工厂（TagSpec 的 "!" 前缀 = 该项置灰，保留 DOM 原位顺序） ----

        private static CatalogConditionGroup Tags(string key, string label, params string[] items)
        {
            return TagSpec(key, label, items);
        }

        private static CatalogConditionGroup TagSpec(string key, string label, params string[] items)
        {
            var g = new CatalogConditionGroup(label, key, CatalogConditionGroup.ConditionKind.Tags);
            foreach (var it in items)
            {
                if (it.StartsWith("!"))
                {
                    g.Items.Add(new CatalogItem(it.Substring(1), enabled: false));
                }
                else
                {
                    g.Items.Add(new CatalogItem(it));
                }
            }
            return g;
        }

        /// <summary>整组置灰（原型 filter-group-unsupported：无 key，全部项 filter-control-unsupported）。</summary>
        private static CatalogConditionGroup DisabledGroup(string label, params string[] items)
        {
            var g = new CatalogConditionGroup(label, "", CatalogConditionGroup.ConditionKind.Tags);
            foreach (var it in items)
            {
                g.Items.Add(new CatalogItem(it, enabled: false));
            }
            g.Enabled = false;
            g.DisabledReason = "当前筛选条件不可用";
            return g;
        }

        private static CatalogConditionGroup Text(string key, string label)
        {
            return new CatalogConditionGroup(label, key, CatalogConditionGroup.ConditionKind.Text);
        }

        private static CatalogConditionGroup Select(string key, string label, params string[] items)
        {
            var g = new CatalogConditionGroup(label, key, CatalogConditionGroup.ConditionKind.Select);
            foreach (var it in items)
            {
                g.Items.Add(new CatalogItem(it));
            }
            return g;
        }

        /// <summary>置灰下拉组（原型 filter-group-unsupported 的 select 形态，如 地区等级/地方债专项/省份）。</summary>
        private static CatalogConditionGroup DisabledSelect(string label, string reason, params string[] items)
        {
            var g = new CatalogConditionGroup(label, "", CatalogConditionGroup.ConditionKind.Select);
            foreach (var it in items)
            {
                g.Items.Add(new CatalogItem(it, enabled: false));
            }
            g.Enabled = false;
            g.DisabledReason = reason;
            return g;
        }
    }
}
