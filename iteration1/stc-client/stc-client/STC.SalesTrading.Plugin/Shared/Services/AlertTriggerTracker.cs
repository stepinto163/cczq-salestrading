using System.Collections.Generic;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>首份快照/手动刷新只建立基线；增量仅弹出尚未见过的报价版本实例。</summary>
    internal sealed class AlertTriggerTracker
    {
        private readonly HashSet<string> seen = new HashSet<string>();
        private bool initialized;

        public List<KeyValuePair<AlertRow, AlertTrigger>> Observe(IEnumerable<AlertRow> rows, bool notify)
        {
            var fresh = new List<KeyValuePair<AlertRow, AlertTrigger>>();
            foreach (var row in rows)
            {
                // 旧服务端未提供独立键时保持列表可用，不用聚合 ID 猜测并误弹。
                foreach (var trigger in row.Triggers ?? new AlertTrigger[0])
                {
                    if (!string.IsNullOrEmpty(trigger.AlertKey) && seen.Add(trigger.AlertKey)
                        && initialized && notify && !row.IsRead)
                    {
                        fresh.Add(new KeyValuePair<AlertRow, AlertTrigger>(row, trigger));
                    }
                }
            }
            initialized = true;
            return fresh;
        }
    }
}
