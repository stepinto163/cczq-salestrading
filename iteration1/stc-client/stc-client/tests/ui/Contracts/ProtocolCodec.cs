using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SalesTrading.UiTests.Contracts
{
    public static class ProtocolCodec
    {
        private sealed class ProtocolProperty
        {
            public PropertyInfo Property { get; set; }
            public string Field { get; set; }
        }

        private static readonly ConcurrentDictionary<Type, ProtocolProperty[]> Properties =
            new ConcurrentDictionary<Type, ProtocolProperty[]>();

        // 元数据按类型缓存；JsonSerializer 仍按操作创建，不能跨线程共享可变实例。
        private static ProtocolProperty[] GetProperties(Type type)
        {
            return Properties.GetOrAdd(type, modelType => modelType.GetProperties()
                .Select(property => new ProtocolProperty {
                    Property = property,
                    Field = property.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName
                })
                .Where(property => property.Field != null)
                .ToArray());
        }

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
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            long deserializeStarted = ClientPerformanceTrace.Start();
            ProtocolModel model = (ProtocolModel)input.ToObject(type, Serializer());
            ClientPerformanceTrace.End("codec.deserialize:" + type.Name, deserializeStarted);
            long attachStarted = ClientPerformanceTrace.Start();
            Attach(model, input, Serializer());
            ClientPerformanceTrace.End("codec.attach:" + type.Name, attachStarted);
            return model;
        }

        private static void Attach(object value, JToken input, JsonSerializer serializer)
        {
            if (value is ProtocolModel model && input is JObject source)
            {
                foreach (ProtocolProperty property in GetProperties(model.GetType()))
                {
                    Attach(property.Property.GetValue(model), source[property.Field], serializer);
                }
                model.Original = (JObject)source.DeepClone();
                model.Baseline = Snapshot(model, serializer, true);
            }
            else if (value is IList list && input is JArray array)
            {
                for (int index = 0; index < list.Count; index++)
                {
                    Attach(list[index], array[index], serializer);
                }
            }
            else if (value is IDictionary dictionary && input is JObject map)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    Attach(entry.Value, map[entry.Key.ToString()], serializer);
                }
            }
        }

        private static JObject Snapshot(ProtocolModel model, JsonSerializer serializer, bool initializing = false)
        {
            JObject result = new JObject();
            foreach (ProtocolProperty property in GetProperties(model.GetType()))
            {
                result[property.Field] = Tree(property.Property.GetValue(model), serializer, initializing);
            }
            return result;
        }

        private static JToken Tree(object value, JsonSerializer serializer, bool initializing)
        {
            if (value == null)
            {
                return JValue.CreateNull();
            }
            if (value is ProtocolModel model)
            {
                if (initializing && model.Original != null)
                {
                    // 子模型已完成 Attach，此时尚无编辑，编码结果就是其原始 JSON。
                    // 直接复制，避免每层父模型重新递归编码整棵子树。
                    // 标量仍取规范化属性值，以保留数字转字符串后的修改比较语义。
                    return model.Original.DeepClone();
                }
                return Encode(model, serializer);
            }
            if (value is JToken token)
            {
                return token.DeepClone();
            }
            if (value is IDictionary dictionary)
            {
                JObject result = new JObject();
                foreach (DictionaryEntry entry in dictionary)
                {
                    result[entry.Key.ToString()] = Tree(entry.Value, serializer, initializing);
                }
                return result;
            }
            if (value is IList list)
            {
                JArray result = new JArray();
                foreach (object item in list)
                {
                    result.Add(Tree(item, serializer, initializing));
                }
                return result;
            }
            return JToken.FromObject(value, serializer);
        }

        public static JObject Encode(ProtocolModel model)
        {
            return Encode(model, Serializer());
        }

        private static JObject Encode(ProtocolModel model, JsonSerializer serializer)
        {
            JObject result = Merge(model.Original, model.Baseline, Snapshot(model, serializer));
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
            foreach (string field in model.ExplicitNulls)
            {
                output[field] = JValue.CreateNull();
            }
            foreach (ProtocolProperty property in GetProperties(model.GetType()))
            {
                string field = property.Field;
                object value = property.Property.GetValue(model);
                if (value is ProtocolModel child && output[field] is JObject childJson)
                {
                    ApplyExplicitNulls(childJson, child);
                }
                else if (value is IList list && output[field] is JArray array)
                {
                    for (int index = 0; index < list.Count; index++)
                    {
                        if (list[index] is ProtocolModel item && array[index] is JObject itemJson)
                        {
                            ApplyExplicitNulls(itemJson, item);
                        }
                    }
                }
            }
        }
    }
}
