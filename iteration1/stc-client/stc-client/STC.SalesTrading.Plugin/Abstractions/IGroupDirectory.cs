using System.Collections.Generic;
using System.Collections.ObjectModel;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 关注分组 / 筛选分组目录（数据接缝，方案 §3.2.7）。
    /// 签名与 <see cref="FilterProfileService"/> 静态内存版同构（按 MarketKind 进出、整体替换语义），
    /// 弹窗 VM 替换数据源时零逻辑改动；开发期由 Mock 桥接静态内存，接网关后落库。
    /// </summary>
    public interface IGroupDirectory
    {
        /// <summary>读取某市场的关注分组（含全部成员）。</summary>
        ObservableCollection<FavBondGroup> GetFavGroups(FilterProfileService.MarketKind market);

        /// <summary>读取某市场的筛选分组（含全部子组条件）。</summary>
        ObservableCollection<FilterNamedGroup> GetFilterGroups(FilterProfileService.MarketKind market);

        /// <summary>整体替换关注分组集合（新增/更新逐组保存、服务端多余的组删除）。</summary>
        void SaveFavGroups(FilterProfileService.MarketKind market, IEnumerable<FavBondGroup> groups);

        /// <summary>只保存（upsert）单个关注分组，不触碰服务端其它组（右键加入分组等轻量入口）。
        /// 新组建档后回填服务端 Id 到 group.Id。</summary>
        void UpsertFavGroup(FilterProfileService.MarketKind market, FavBondGroup group);

        /// <summary>整体替换筛选分组集合（同上）。</summary>
        void SaveFilterGroups(FilterProfileService.MarketKind market, IEnumerable<FilterNamedGroup> groups);

        /// <summary>
        /// 筛选字段目录（ficc.quote.dictionary.query，H14 filterFields）。
        /// 条件落库的 field/operator/value 唯一事实源；网关不可用时返回空列表（VM 回退静态目录）。
        /// </summary>
        IReadOnlyList<FilterFieldEntry> GetFilterFields(FilterProfileService.MarketKind market);
    }
}
