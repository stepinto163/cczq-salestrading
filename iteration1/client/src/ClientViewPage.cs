using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    internal static class ClientViewPage {
        internal static bool Apply(JObject message, string viewId, string collection, string idField,
            int pageNumber, int pageSize, List<string> ids, Dictionary<string, JObject> records) {
            bool refill = false;
            var incoming = ((JArray)message[collection] ?? new JArray()).OfType<JObject>()
                .Where(row => collection != "books" || (string)row["viewId"] == viewId)
                .ToDictionary(row => (string)row[idField]);
            foreach (JObject delta in (JArray)message["views"] ?? new JArray()) {
                if ((string)delta["viewId"] != viewId) continue;
                foreach (string id in ((JArray)delta["removeIds"] ?? new JArray()).Values<string>()) {
                    refill |= ids.Remove(id);
                }
                foreach (string id in ((JArray)delta["upsertIds"] ?? new JArray()).Values<string>()) {
                    if (pageNumber != 1 && !ids.Contains(id)) continue;
                    if (!incoming.TryGetValue(id, out JObject row)) {
                        throw new InvalidOperationException("View update is missing data for " + id);
                    }
                    if (records.TryGetValue(id, out JObject previous)
                        && Time(row) < Time(previous)) refill = true;
                    records[id] = row;
                    if (!ids.Contains(id)) ids.Add(id);
                }
            }
            if (pageNumber == 1) {
                ids.Sort((left, right) => {
                    int comparison = Time(records[right]).CompareTo(Time(records[left]));
                    return comparison != 0 ? comparison : StringComparer.Ordinal.Compare(left, right);
                });
                if (ids.Count > pageSize) ids.RemoveRange(pageSize, ids.Count - pageSize);
            }
            var visible = new HashSet<string>(ids);
            foreach (string id in records.Keys.Where(id => !visible.Contains(id)).ToArray()) records.Remove(id);
            return refill && pageNumber == 1;
        }

        private static DateTimeOffset Time(JObject row)
        {
            JToken timestamp = row["updateTime"];
            if (timestamp is JValue date && timestamp.Type == JTokenType.Date)
            {
                if (date.Value is DateTimeOffset offset)
                {
                    return offset;
                }
                return new DateTimeOffset((DateTime)date.Value);
            }
            return DateTimeOffset.TryParse((string)timestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTimeOffset value)
                ? value : DateTimeOffset.MinValue;
        }
    }
}
