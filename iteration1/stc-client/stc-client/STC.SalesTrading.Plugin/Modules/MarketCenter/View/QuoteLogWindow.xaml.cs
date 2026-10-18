using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>与其他表格复用分页条，翻页保留版本边界，刷新才纳入新增记录。</summary>
    public sealed class QuoteLogViewModel : ObservableObject, IDisposable
    {
        private readonly IQuoteService service;
        private readonly string quoteId;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private string snapshotRevision;
        private int page = 1;
        private int loadedPage = 1;
        private int requestedPage = 1;
        private bool requestedRefresh;
        private bool isLoading;
        private bool disposed;
        private string errorMessage;
        private int totalCount;
        private IReadOnlyList<QuoteLogEntry> records = Array.Empty<QuoteLogEntry>();
        public const int PageSize = 20;

        public QuoteLogViewModel(IQuoteService service, string quoteId)
        {
            this.service = service;
            this.quoteId = quoteId;
            RefreshCommand = new AsyncRelayCommand(() => LoadPageAsync(1, true), () => !IsLoading);
            RetryCommand = new AsyncRelayCommand(() => LoadPageAsync(requestedPage, requestedRefresh),
                () => !IsLoading && !string.IsNullOrEmpty(ErrorMessage));
        }

        public IAsyncRelayCommand RefreshCommand { get; }
        public IAsyncRelayCommand RetryCommand { get; }
        public IReadOnlyList<QuoteLogEntry> Records => records;
        public int TotalCount => totalCount;
        public int TotalPages => Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
        public string ErrorMessage => errorMessage;
        public bool IsLoading => isLoading;
        public int Page
        {
            get => page;
            set
            {
                if (value == page || isLoading || value < 1 || value > TotalPages) return;
                _ = LoadPageAsync(value, false);
            }
        }

        public async Task LoadPageAsync(int targetPage, bool refresh = false)
        {
            if (isLoading || disposed) return;
            requestedPage = targetPage;
            requestedRefresh = refresh;
            isLoading = true;
            errorMessage = null;
            NotifyLoading();
            try
            {
                var result = await service.LoadQuoteLogAsync(quoteId, targetPage, PageSize,
                    refresh ? null : snapshotRevision, lifetime.Token);
                if (disposed) return;
                records = result.Records;
                totalCount = result.TotalCount;
                snapshotRevision = result.SnapshotRevision;
                loadedPage = result.PageNumber;
                page = loadedPage;
                OnPropertyChanged(nameof(Records));
                OnPropertyChanged(nameof(TotalCount));
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(Page));
            }
            catch (OperationCanceledException) when (disposed) { }
            catch (Exception error)
            {
                if (disposed) return;
                // 失败时保留上次成功的内容和页码，不能将旧数据标为新页。
                page = loadedPage;
                errorMessage = "报价日志加载失败：" + error.Message;
                OnPropertyChanged(nameof(Page));
            }
            finally
            {
                isLoading = false;
                NotifyLoading();
            }
        }

        private void NotifyLoading()
        {
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(ErrorMessage));
            RefreshCommand.NotifyCanExecuteChanged();
            RetryCommand.NotifyCanExecuteChanged();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            lifetime.Cancel();
            lifetime.Dispose();
        }
    }

    public partial class QuoteLogWindow : Window
    {
        private QuoteLogWindow()
        {
            InitializeComponent();
        }

        public static void Show(Window owner, string quoteId, string label, IQuoteService service)
        {
            var window = new QuoteLogWindow();
            var model = new QuoteLogViewModel(service, quoteId);
            window.DataContext = model;
            var host = owner ?? Application.Current?.MainWindow;
            if (host != null) window.Owner = host;
            window.TitleText.Text = "报价日志 · " + (label ?? quoteId);
            window.Loaded += async (sender, args) => await model.LoadPageAsync(1, true);
            window.Closed += (sender, args) => model.Dispose();
            window.ShowDialog();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            try { DragMove(); }
            catch (InvalidOperationException) { }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
