using System;

namespace TaskEngine
{
    public interface ITaskWaitHandle
    {
        void Wait();

        bool Wait(TimeSpan timeout);

        bool Terminate();

        bool Terminate(TimeSpan timeout);
    }
}