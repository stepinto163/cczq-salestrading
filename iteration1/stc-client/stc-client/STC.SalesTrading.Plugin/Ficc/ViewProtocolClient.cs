using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STC.SalesTrading.Plugin.Ficc.Protocol;

namespace STC.SalesTrading.Plugin.Ficc
{
    /// <summary>
    /// 将报价、价格、提醒三个业务域的强类型视图协议适配为页面共用操作。
    /// 报价按 quoteId、价格按 securityId、提醒按 notificationId 标识，不能互换主键或结果集合。
    /// JObject 只作为现有表格的展示投影；流元数据与成员变更仍按协议模型解析。
    /// </summary>
    internal sealed class ViewProtocolClient
    {
        private readonly FiccApiClient api;
        private readonly string domain;

        public ViewProtocolClient(FiccApiClient api, string domain)
        {
            this.api = api;
            this.domain = domain;
        }

        public Task<StreamOpenData> OpenStreamAsync()
        {
            switch (domain)
            {
                case "quote": return api.RequestAsync(SalesTradingApi.QuoteStreamOpen, new Request());
                case "price": return api.RequestAsync(SalesTradingApi.PriceStreamOpen, new Request());
                case "notification": return api.RequestAsync(SalesTradingApi.NotificationStreamOpen, new Request());
                default: throw new ArgumentException("Unknown view domain: " + domain);
            }
        }

        public async Task<JObject> OpenAsync(string viewId, int pageSize, Filter filter,
            string ownerScope, List<string> statuses, string priceMode)
        {
            switch (domain)
            {
                case "quote":
                    var quoteReply = await api.RequestAsync(SalesTradingApi.QuoteViewOpen, new QuoteViewOpenRequest
                    {
                        ViewId = viewId, PageSize = pageSize, Filter = filter,
                        OwnerScope = ownerScope, Status = statuses
                    });
                    long projectionStarted = ClientPerformanceTrace.Start();
                    JObject projection = quoteReply.ToJson();
                    ClientPerformanceTrace.End("view.projection:quote", projectionStarted);
                    return projection;
                case "price":
                    return (await api.RequestAsync(SalesTradingApi.PriceViewOpen, new PriceViewOpenRequest
                    {
                        ViewId = viewId, PageSize = pageSize, Filter = filter, Mode = priceMode
                    })).ToJson();
                case "notification":
                    return (await api.RequestAsync(SalesTradingApi.NotificationViewOpen, new NotificationViewOpenRequest
                    {
                        ViewId = viewId, PageSize = pageSize, Filter = filter, ReadState = "ALL"
                    })).ToJson();
                default: throw new ArgumentException("Unknown view domain: " + domain);
            }
        }

        public async Task<JObject> QueryAsync(string viewId, int pageNumber, int pageSize)
        {
            var request = new ViewQueryRequest { ViewId = viewId, PageNumber = pageNumber, PageSize = pageSize };
            switch (domain)
            {
                case "quote": return (await api.RequestAsync(SalesTradingApi.QuoteViewQuery, request)).ToJson();
                case "price": return (await api.RequestAsync(SalesTradingApi.PriceViewQuery, request)).ToJson();
                case "notification": return (await api.RequestAsync(SalesTradingApi.NotificationViewQuery, request)).ToJson();
                default: throw new ArgumentException("Unknown view domain: " + domain);
            }
        }

        public Task<ViewCloseData> CloseAsync(string viewId)
        {
            var request = new ViewCloseRequest { ViewId = viewId };
            switch (domain)
            {
                case "quote": return api.RequestAsync(SalesTradingApi.QuoteViewClose, request);
                case "price": return api.RequestAsync(SalesTradingApi.PriceViewClose, request);
                case "notification": return api.RequestAsync(SalesTradingApi.NotificationViewClose, request);
                default: throw new ArgumentException("Unknown view domain: " + domain);
            }
        }

        public StreamMessage ReadPush(JObject message, out List<ViewDelta> views, out List<JObject> records)
        {
            switch (domain)
            {
                case "quote":
                    QuoteStreamBatch quotes = ProtocolCodec.Decode<QuoteStreamBatch>(message);
                    views = quotes.Views ?? new List<ViewDelta>();
                    records = quotes.Quotes?.Select(row => row.ToJson()).ToList() ?? new List<JObject>();
                    return quotes;
                case "price":
                    PriceStreamBatch prices = ProtocolCodec.Decode<PriceStreamBatch>(message);
                    views = prices.Views ?? new List<ViewDelta>();
                    records = prices.Books?.Select(row => row.ToJson()).ToList() ?? new List<JObject>();
                    return prices;
                case "notification":
                    NotificationStreamBatch notifications = ProtocolCodec.Decode<NotificationStreamBatch>(message);
                    views = notifications.Views ?? new List<ViewDelta>();
                    records = notifications.Notifications?.Select(row => row.ToJson()).ToList() ?? new List<JObject>();
                    return notifications;
                default: throw new ArgumentException("Unknown view domain: " + domain);
            }
        }
    }
}
