using CommunityToolkit.Mvvm.ComponentModel;
using STC.SalesTrading.Plugin.Abstractions.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>
    /// 右下角提醒弹窗的**分组**（原型 0910：一个提醒类型一个弹窗，头部显示「类型 (新增数量)」与「查看 / 关闭」，
    /// 正文最多展示最近 3 条）。
    ///
    /// 已读与关闭语义按原型区分：
    /// - 查看：标记该弹窗的 alertKey 为已读并跳转/定位对应提醒行，随后关闭弹窗；
    /// - 关闭：只关闭 UI，**不**标记已读。
    /// </summary>
    public partial class AlertPopupGroup : ObservableObject
    {
        /// <summary>正文最多展示的条目数（原型 0910：最多展示本次最近 3 条）。</summary>
        public const int MaxVisibleItems = 3;

        public AlertPopupGroup(string typeLabel, AlertType alertType)
        {
            TypeLabel = typeLabel;
            AlertType = alertType;
        }

        public string TypeLabel { get; }

        public AlertType AlertType { get; }

        /// <summary>正文展示的条目（最多 <see cref="MaxVisibleItems"/> 条，最新在前）。</summary>
        public ObservableCollection<AlertPopupItem> Items { get; } = new ObservableCollection<AlertPopupItem>();

        /// <summary>本次该类型累计新增提醒数（头部括号里的数字，可能大于正文条数）。</summary>
        [ObservableProperty]
        private int _totalCount;

        partial void OnTotalCountChanged(int value)
        {
            OnPropertyChanged(nameof(HeaderText));
        }

        /// <summary>头部文案：「类型 (新增数量)」。</summary>
        public string HeaderText
        {
            get { return string.Format("{0} ({1})", TypeLabel, TotalCount); }
        }

        /// <summary>已关闭（服务据此从队列移除）。</summary>
        [ObservableProperty]
        private bool _isClosed;

        /// <summary>该弹窗内全部条目的 alertKey（供上层批量标记已读）。</summary>
        public IReadOnlyList<string> AlertKeys
        {
            get { return Items.Select(i => i.AlertKey).Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList(); }
        }

        /// <summary>追加一条提醒：累计计数，正文只保留最近 <see cref="MaxVisibleItems"/> 条。</summary>
        public void Append(AlertPopupItem item)
        {
            Items.Insert(0, item);
            while (Items.Count > MaxVisibleItems)
            {
                Items.RemoveAt(Items.Count - 1);
            }

            TotalCount++;
        }

        /// <summary>「查看」：标记已读 + 跳转定位，然后关闭弹窗。</summary>
        public void View()
        {
            foreach (var item in Items.ToList())
            {
                item.InvokeView();
            }

            IsClosed = true;
        }

        /// <summary>「关闭」：只关闭 UI，不标记已读。</summary>
        public void Close()
        {
            IsClosed = true;
        }
    }
}
