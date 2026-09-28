using System;

namespace STC.SalesTrading.Plugin.Abstractions.Results
{
    public enum MutationStatus
    {
        Succeeded,
        Failed,
        PartiallySucceeded,
        Conflict,
        ValidationFailed,
    }

    /// <summary>
    /// 写操作的稳定 UI 结果语义，暂不猜测后端字段或错误码。
    /// </summary>
    public sealed class MutationResult
    {
        public MutationStatus Status { get; private set; }
        public int SucceededCount { get; private set; }
        public int FailedCount { get; private set; }
        public string Message { get; private set; }
        public Exception Exception { get; private set; }

        public bool IsSuccess
        {
            get { return Status == MutationStatus.Succeeded; }
        }

        public static MutationResult Success(int count, string message = null)
        {
            return new MutationResult
            {
                Status = MutationStatus.Succeeded,
                SucceededCount = Math.Max(0, count),
                Message = message,
            };
        }

        public static MutationResult Failure(string message, Exception exception = null)
        {
            return new MutationResult
            {
                Status = MutationStatus.Failed,
                Message = message,
                Exception = exception,
            };
        }

        public static MutationResult Partial(int succeeded, int failed, string message = null)
        {
            return new MutationResult
            {
                Status = MutationStatus.PartiallySucceeded,
                SucceededCount = Math.Max(0, succeeded),
                FailedCount = Math.Max(0, failed),
                Message = message,
            };
        }

        public static MutationResult Conflict(string message)
        {
            return new MutationResult { Status = MutationStatus.Conflict, Message = message };
        }

        public static MutationResult Validation(string message)
        {
            return new MutationResult { Status = MutationStatus.ValidationFailed, Message = message };
        }
    }
}
