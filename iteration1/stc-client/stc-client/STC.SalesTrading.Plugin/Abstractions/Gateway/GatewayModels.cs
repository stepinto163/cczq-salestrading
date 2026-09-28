using System;
using System.Collections.Generic;

namespace STC.SalesTrading.Plugin.Abstractions.Gateway
{
    public sealed class GatewayRequest
    {
        public string ServiceId { get; set; }
        public string Topic { get; set; }
        public object Payload { get; set; }
        public TimeSpan? Timeout { get; set; }
    }

    public sealed class GatewayResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string RequestId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }

        public static GatewayResponse<T> Ok(T data)
        {
            return new GatewayResponse<T> { Success = true, Data = data };
        }

        public static GatewayResponse<T> Fail(string code, string message, Exception exception = null)
        {
            return new GatewayResponse<T>
            {
                Success = false,
                ErrorCode = code,
                ErrorMessage = message,
                Exception = exception,
            };
        }
    }

    public sealed class GatewaySubscription
    {
        public string ServiceId { get; set; }
        public IReadOnlyList<string> Topics { get; set; }
        public string SignalId { get; set; }
        public bool IncludeImage { get; set; }
    }

    public sealed class GatewayMessage
    {
        public string ServiceId { get; set; }
        public IReadOnlyList<string> Topics { get; set; }
        public object Payload { get; set; }
        public DateTime ReceivedAt { get; set; }
    }

    public sealed class GatewayConnectionChanged
    {
        public object RawState { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
