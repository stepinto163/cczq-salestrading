namespace STC.SalesTrading.Plugin.Abstractions.Models
{
    /// <summary>账户级提醒开关；Revision 用于保存时的并发校验。</summary>
    public sealed class AlertPreferences
    {
        public bool WatchlistEnabled { get; set; }
        public bool FilterGroupEnabled { get; set; }
        public bool CounterQuoteEnabled { get; set; }
        public bool MyBestEnabled { get; set; }
        public string Revision { get; set; }

        public bool IsEnabled(AlertType type)
        {
            switch (type)
            {
                case AlertType.FavoriteBond: return WatchlistEnabled;
                case AlertType.FilterGroup: return FilterGroupEnabled;
                case AlertType.PriceMatch: return CounterQuoteEnabled;
                case AlertType.MyBestQuote: return MyBestEnabled;
                default: return false;
            }
        }
    }
}
