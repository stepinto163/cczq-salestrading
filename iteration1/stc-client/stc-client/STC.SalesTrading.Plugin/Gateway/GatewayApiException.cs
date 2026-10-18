using System;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// FICC 后端接口异常。承载规范 §2.2 的错误码与 error.type / error.field，
    /// 供上层按码分派处理（如 1004 乐观锁冲突让用户重新读取后决定）。
    /// </summary>
    public sealed class GatewayApiException : Exception
    {
        /// <summary>顶层 Code（"0000" 为成功，业务不会走到这里）。</summary>
        public string Code { get; private set; }

        /// <summary>error.type，如 VALIDATION_FAILED / REVISION_CONFLICT。</summary>
        public string ErrorType { get; private set; }

        /// <summary>error.field，字段级校验失败时指出字段名。</summary>
        public string ErrorField { get; private set; }

        public GatewayApiException(string code, string message)
            : base(FormatMessage(code, message))
        {
            Code = code;
        }

        public GatewayApiException(string code, string message, string errorType, string errorField)
            : base(FormatMessage(code, message))
        {
            Code = code;
            ErrorType = errorType;
            ErrorField = errorField;
        }

        /// <summary>从应答信封构造；错误项逐条汇总为单条异常。</summary>
        public static GatewayApiException FromReply(string code, string message,
            string errorType, string errorField)
        {
            return new GatewayApiException(code, message, errorType, errorField);
        }

        private static string FormatMessage(string code, string message)
        {
            return string.IsNullOrEmpty(message) ? code : code + ": " + message;
        }
    }
}
