using System.Collections.Generic;
using Newtonsoft.Json;

namespace SalesTrading.UiTests.Contracts
{
    /// <summary>NotificationQueryData</summary>
    public class NotificationQueryData : PageData
    {
        [JsonProperty("notifications")]
        public List<NotificationRecord> Notifications { get; set; }

    }
}
