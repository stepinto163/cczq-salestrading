using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    public partial class BondDetailViewModel
    {
        private IRecommendationService recommendationService;
        private CancellationTokenSource recommendationCancellation;
        private DateTime loadedRecommendationStart;
        private DateTime loadedRecommendationEnd;
        [ObservableProperty]
        private string _recommendationStatus = "请选择债券查看推荐";
        [ObservableProperty]
        private bool _recommendationLoading;

        public void AttachRecommendations(IRecommendationService service)
        {
            recommendationService = service;
            ScheduleRecommendations();
        }

        public void StopRecommendations()
        {
            recommendationCancellation?.Cancel();
            recommendationCancellation?.Dispose();
            recommendationCancellation = null;
        }

        private void ScheduleRecommendations()
        {
            if (recommendationService == null) return;
            // 日期选择的连续变化只发出最后一次请求，迟到响应不能覆盖新窗口。
            _ = ReloadRecommendationsAsync();
        }

        [RelayCommand]
        public async Task ReloadRecommendationsAsync()
        {
            StopRecommendations();
            var cancellation = new CancellationTokenSource();
            recommendationCancellation = cancellation;
            var token = cancellation.Token;
            Recommendations.Clear();
            RefreshRec();
            RecommendationLoading = true;
            RecommendationStatus = "正在加载推荐…";
            try
            {
                if (recommendationService == null) throw new InvalidOperationException("推荐服务未接入");
                DateTime end = RecEndDate?.Date ?? DateTime.Today;
                int days = SelectedRecWindow == "7d" ? 7 : SelectedRecWindow == "90d" ? 90 : 30;
                DateTime start = RecStartDate?.Date ?? end.AddDays(1 - days);
                if (SelectedRecWindow == "custom" && (!RecStartDate.HasValue || !RecEndDate.HasValue))
                    throw new InvalidOperationException("请选择完整的开始和结束日期");
                if (end < start || (end - start).TotalDays >= 365)
                    throw new InvalidOperationException("推荐时间窗口必须为1至365天");
                await Task.Delay(150, token);
                var rows = await recommendationService.QueryRecommendationsAsync(Code, start, end, token);
                token.ThrowIfCancellationRequested();
                loadedRecommendationStart = start;
                loadedRecommendationEnd = end;
                foreach (var row in rows) Recommendations.Add(row);
                RefreshRec();
                RecommendationStatus = rows.Count == 0 ? "所选窗口内暂无正式报价证据" : "";
            }
            catch (OperationCanceledException) { }
            catch (Exception error)
            {
                if (!token.IsCancellationRequested) RecommendationStatus = "推荐加载失败：" + error.Message;
            }
            finally
            {
                if (!token.IsCancellationRequested) RecommendationLoading = false;
            }
        }

        public async Task SubmitRecommendationFeedbackAsync(RecommendationRow row, string feedback)
        {
            if (row == null || !row.CanFeedback || recommendationService == null) return;
            try
            {
                row.Feedback = await recommendationService.SubmitFeedbackAsync(Code, loadedRecommendationStart,
                    loadedRecommendationEnd, row.RecommendationId, feedback);
            }
            catch (Exception error)
            {
                RecommendationStatus = "反馈保存失败：" + error.Message;
            }
        }
    }
}
