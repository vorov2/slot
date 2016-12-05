using System;

namespace TaskEngine
{
    public class ExecMonitor
    {
        private readonly Func<bool> waitHandler;
        private readonly Func<bool> killHandler;

        public ExecMonitor(Func<bool> waitHandler, Func<bool> killHandler)
        {
            this.waitHandler = waitHandler;
            this.killHandler = killHandler;
        }

        public virtual bool Wait()
        {
            return waitHandler();
        }

        public virtual bool Kill()
        {
            return killHandler();
        }
    }
}