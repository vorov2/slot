using System;

namespace CodeBox.Core
{
    public static class ExecExtensions
    {
        public static ExecResult Exec<T>(this T obj, Action<T> act)
        {
            try
            {
                act(obj);
                return ExecResult.Ok;
            }
            catch (Exception ex)
            {
                return ExecResult.Failure(ex.Message);
            }
        }
    }

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
