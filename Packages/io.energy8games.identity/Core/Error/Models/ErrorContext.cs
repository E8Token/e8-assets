using System;

namespace Energy8.Identity.Core.Error
{
    public class ErrorContext
    {
        public Exception Exception { get; }
        public string Operation { get; }
        public int RetryCount { get; }
        public TimeSpan Elapsed { get; }

        public ErrorContext(
            Exception exception, 
            string operation, 
            int retryCount = 0, 
            TimeSpan? elapsed = null)
        {
            Exception = exception;
            Operation = operation;
            RetryCount = retryCount;
            Elapsed = elapsed ?? TimeSpan.Zero;
        }
    }
}