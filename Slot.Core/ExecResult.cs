using System;

namespace Slot.Core
{
    public sealed class ExecResult
    {
        public static readonly ExecResult Ok = new ExecResult(true, null);

        private ExecResult(bool success, string reason)
        {
            Success = success;
            Reason = reason;
        }

        public bool Success { get; }

        public string Reason { get; }

        public static ExecResult Failure(string reason)
        {
            return new ExecResult(false, reason);
        }
    }
}
