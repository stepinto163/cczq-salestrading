using CommunityToolkit.Mvvm.ComponentModel;
using STC.SalesTrading.Plugin.Abstractions.Models;
using System;
using System.Collections.ObjectModel;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 报价确认批次 VM（分组头：来源徽标 IDEAL蓝/QT绿 + 机构 + 交易员 + 到报时间 + N条·状态 + 全选）。
    /// IsExpanded 驱动主表行的详情展开/折叠（原型点击组头折叠）。
    /// </summary>
    public partial class QuoteConfirmBatchViewModel : ObservableObject
    {
        public QuoteConfirmBatchViewModel(QuoteConfirmBatch batch)
        {
            BatchId = batch.BatchId;
            SourceType = (batch.SourceType ?? "ideal").ToLowerInvariant();
            Institution = batch.Institution ?? string.Empty;
            Trader = batch.Trader ?? string.Empty;
            ReceivedAt = batch.ReceivedAt?.ToString("HH:mm") ?? string.Empty;
            Status = StatusDisplay(batch.Status);
            Rows = new ObservableCollection<QuoteConfirmRowViewModel>();
        }

        public string BatchId { get; }

        /// <summary>来源渠道（ideal/qt，驱动徽标配色）。</summary>
        public string SourceType { get; }

        /// <summary>来源徽标文案（IDEAL/QT 大写）。</summary>
        public string SourceBadge { get { return SourceType.ToUpperInvariant(); } }

        /// <summary>报价机构。</summary>
        public string Institution { get; }

        /// <summary>报价交易员。</summary>
        public string Trader { get; }

        /// <summary>到报时间（HH:mm）。</summary>
        public string ReceivedAt { get; }

        /// <summary>批次状态（待确认/部分识别/识别失败/部分确认）。</summary>
        public string Status { get; }

        public ObservableCollection<QuoteConfirmRowViewModel> Rows { get; }

        /// <summary>右对齐汇总文案（原型：N条 · 状态）。</summary>
        public string Summary
        {
            get { return string.Format("{0} 条 · {1}", Rows.Count, Status); }
        }

        /// <summary>行集合变化后由父 VM 调用，刷新 Summary 绑定。</summary>
        public void NotifySummaryChanged()
        {
            OnPropertyChanged(nameof(Summary));
        }

        [ObservableProperty]
        private bool _isExpanded = true;

        /// <summary>组级全选（三态：null=部分选中）。</summary>
        [ObservableProperty]
        private bool? _isGroupChecked = false;

        public static string StatusDisplay(BatchStatus status)
        {
            switch (status)
            {
                case BatchStatus.PendingReview: return "待确认";
                case BatchStatus.PartialFailed: return "部分识别";
                case BatchStatus.RecognitionFailed: return "识别失败";
                case BatchStatus.PartiallyConfirmed: return "部分确认";
                default: return "待确认";
            }
        }
    }
}
