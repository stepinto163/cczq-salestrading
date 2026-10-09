using System;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    public static class ProtocolCodec
    {
        private static JsonSerializer Serializer() => JsonSerializer.Create(new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Include,
            DateParseHandling = DateParseHandling.None
        });

        public static T Decode<T>(JObject input) where T : ProtocolModel
        {
            return (T)Decode(input, typeof(T));
        }

        public static ProtocolModel Decode(JObject input, Type type)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            long deserializeStarted = ClientPerformanceTrace.Start();
            ProtocolModel model = (ProtocolModel)input.ToObject(type, Serializer());
            ClientPerformanceTrace.End("codec.deserialize:" + type.Name, deserializeStarted);
            long attachStarted = ClientPerformanceTrace.Start();
            Attach(model, input);
            ClientPerformanceTrace.End("codec.attach:" + type.Name, attachStarted);
            return model;
        }

        private static void Attach(object value, JToken input)
        {
            if (value is ProtocolModel model && input is JObject source)
            {
                foreach (PropertyInfo property in model.GetType().GetProperties())
                {
                    string field = property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
                    if (field != null) Attach(property.GetValue(model), source[field]);
                }
                model.Original = (JObject)source.DeepClone();
                model.Baseline = Snapshot(model);
            }
            else if (value is IList list && input is JArray array)
            {
                for (int index = 0; index < list.Count; index++) Attach(list[index], array[index]);
            }
            else if (value is IDictionary dictionary && input is JObject map)
            {
                foreach (DictionaryEntry entry in dictionary) Attach(entry.Value, map[entry.Key.ToString()]);
            }
        }

        private static JObject Snapshot(ProtocolModel model)
        {
            JObject result = new JObject();
            foreach (PropertyInfo property in model.GetType().GetProperties())
            {
                string field = property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
                if (field != null) result[field] = Tree(property.GetValue(model));
            }
            return result;
        }

        private static JToken Tree(object value)
        {
            if (value == null) return JValue.CreateNull();
            if (value is ProtocolModel model) return Encode(model);
            if (value is JToken token) return token.DeepClone();
            if (value is IDictionary dictionary)
            {
                JObject result = new JObject();
                foreach (DictionaryEntry entry in dictionary) result[entry.Key.ToString()] = Tree(entry.Value);
                return result;
            }
            if (value is IList list)
            {
                JArray result = new JArray();
                foreach (object item in list) result.Add(Tree(item));
                return result;
            }
            return JToken.FromObject(value, Serializer());
        }

        public static JObject Encode(ProtocolModel model)
        {
            JObject result = Merge(model.Original, model.Baseline, Snapshot(model));
            ApplyExplicitNulls(result, model);
            return result;
        }

        private static JObject Merge(JObject original, JObject baseline, JObject current)
        {
            JObject result = original == null ? new JObject() : (JObject)original.DeepClone();
            foreach (JProperty property in current.Properties())
            {
                JToken previous = baseline?[property.Name];
                JToken value = property.Value;
                if (JToken.DeepEquals(value, previous) || (previous == null && value.Type == JTokenType.Null)) continue;
                result[property.Name] = value.DeepClone();
            }
            return result;
        }

        private static void ApplyExplicitNulls(JObject output, ProtocolModel model)
        {
            foreach (string field in model.ExplicitNulls) output[field] = JValue.CreateNull();
            foreach (PropertyInfo property in model.GetType().GetProperties())
            {
                string field = property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName;
                if (field == null) continue;
                object value = property.GetValue(model);
                if (value is ProtocolModel child && output[field] is JObject childJson)
                    ApplyExplicitNulls(childJson, child);
                else if (value is IList list && output[field] is JArray array)
                    for (int index = 0; index < list.Count; index++)
                        if (list[index] is ProtocolModel item && array[index] is JObject itemJson)
                            ApplyExplicitNulls(itemJson, item);
            }
        }
    }
}
