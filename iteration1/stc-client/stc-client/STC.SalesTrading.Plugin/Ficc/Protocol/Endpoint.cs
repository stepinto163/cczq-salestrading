using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Ficc.Protocol
{
    public abstract class Endpoint
    {
        public string Topic { get; protected set; }
        public abstract Type RequestType { get; }
        public abstract Type DataType { get; }
    }

    public sealed class Endpoint<TRequest, TData> : Endpoint
        where TRequest : Request where TData : ProtocolModel
    {
        public override Type RequestType => typeof(TRequest);
        public override Type DataType => typeof(TData);
        public Endpoint(string topic) { Topic = topic; }
    }
}
