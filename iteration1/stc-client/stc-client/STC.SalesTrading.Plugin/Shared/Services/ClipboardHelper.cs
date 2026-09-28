using System;
using STC.SalesTrading.Plugin.Shared.ViewModel;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>剪贴板帮助：复制 + 统一 toast（原型 Copy 成功「已复制报价文本」/失败「复制失败」），
    /// 此前该 try/catch + toast 模式在 5 个 VM 里逐字重复。</summary>
    internal static class ClipboardHelper
    {
        /// <summary>复制文本；成功/失败均弹 toast（文案与原型一致）。返回是否成功。</summary>
        public static bool CopyWithToast(string text)
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
                ToastService.Show("已复制报价文本");
                return true;
            }
            catch (Exception)
            {
                ToastService.Show("复制失败", ToastKind.Error);
                return false;
            }
        }
    }
}
