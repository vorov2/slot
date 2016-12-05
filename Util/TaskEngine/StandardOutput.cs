using System;

namespace TaskEngine
{
    public sealed class StandardOutput : IOutput
    {
        public void Write(object data)
        {
            if (data != null)
                Console.WriteLine(data);
        }
    }
}