using System;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.Components;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// CRM 联想取数适配（公共 <see cref="LazySearchComboBox"/> 与 CRM 目录之间的业务薄层）：
    /// 机构 / 交易员两类命令；交易员经 args.Parameter 传机构 Id（null 跨机构）。
    /// 组合根注入 <see cref="Directory"/>；查询失败 toast（控件不感知错误呈现）。
    /// </summary>
    public static class CrmLazySearch
    {
        /// <summary>CRM 目录服务（组合根装配时注入）。</summary>
        public static IBaseDirectory Directory { get; set; }

        /// <summary>机构联想命令（XAML 经 x:Static 直绑；无状态，参数全在 args）。</summary>
        public static LazySearchCommand Institution { get; } = new LazySearchCommand(async a =>
        {
            var result = await SearchCore("INSTITUTION", a, null);
            a.Items = ToObjects(result.Items);
            a.HasMore = result.HasMore;
        });

        /// <summary>交易员联想命令：args.Parameter = 机构 Id（限定其联系人；null 跨机构）。</summary>
        public static LazySearchCommand Contact { get; } = new LazySearchCommand(async a =>
        {
            var result = await SearchCore("CONTACT", a, a.Parameter as string);
            a.Items = ToObjects(result.Items);
            a.HasMore = result.HasMore;
        });

        private static object[] ToObjects(System.Collections.Generic.IReadOnlyList<LazySearchItem> items)
        {
            var result = new object[items.Count];
            for (int i = 0; i < items.Count; i++) result[i] = items[i];
            return result;
        }

        private static async Task<LazySearchResult> SearchCore(string kind, LazySearchArgs args, string institutionId)
        {
            try
            {
                var result = await Directory.SearchPageAsync(kind, args.Text, institutionId,
                    (args.PageIndex - 1) * args.PageSize, args.PageSize, CancellationToken.None);
                return result;
            }
            catch (Exception error)
            {
                ToastService.Show("CRM 联想失败：" + error.Message, ToastKind.Error);
                return new LazySearchResult();
            }
        }
    }
}
