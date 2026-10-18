using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SalesTrading.UiTests.Contracts
{
    public class ProtocolModel
    {
        internal JObject Original;
        internal JObject Baseline;
        internal readonly HashSet<string> ExplicitNulls = new HashSet<string>();

        // New patch: null property means omitted; ClearField explicitly requests null.
        public void ClearField(string wireField)
        {
            PropertyInfo property = GetType().GetProperties().FirstOrDefault(value =>
                value.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName == wireField);
            if (property == null)
            {
                throw new ArgumentException("Unknown protocol field: " + wireField);
            }
            property.SetValue(this, null);
            ExplicitNulls.Add(wireField);
        }

        public JObject ToJson() => ProtocolCodec.Encode(this);
    }
}
