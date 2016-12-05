using System;

namespace TaskEngine
{
    internal sealed class NilOutput : IOutput
    {
        internal static readonly NilOutput Instance = new NilOutput();

        private NilOutput()
        {

        }

        public void Write(object data)
        {
            //Do nothing 
        }
    }
}