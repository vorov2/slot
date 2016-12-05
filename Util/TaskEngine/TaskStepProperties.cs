using System;
using System.Collections.Generic;

namespace TaskEngine
{
    public sealed class TaskStepProperties
    {
        public TaskStepProperties()
        {
            ExtendedProperties = new Dictionary<string, object>();
        }

        private TaskStepProperties(Dictionary<string, object> dict)
        {
            ExtendedProperties = new Dictionary<string, object>(dict);
        }

        internal TaskStepProperties MergeWith(TaskStepProperties props)
        {
            var ret = new TaskStepProperties(ExtendedProperties);
            ret.WorkingDirectory = WorkingDirectory ?? props.WorkingDirectory;
            ret.CreateWindow = CreateWindow ?? props.CreateWindow;
            ret.CaptureOutput = CaptureOutput ?? props.CaptureOutput;
            ret.ShellExecute = ShellExecute ?? props.ShellExecute;

            foreach (var kv in props.ExtendedProperties)
            {
                if (ExtendedProperties.ContainsKey(kv.Key))
                    ret.ExtendedProperties.Add(kv.Key, kv.Value);
            }

            return ret;
        }

        internal TaskStepProperties Clone()
        {
            var ret = (TaskStepProperties)MemberwiseClone();
            ret.ExtendedProperties = new Dictionary<string, object>(ExtendedProperties);
            return ret;
        }

        public string WorkingDirectory { get; set; }

        public bool? CreateWindow { get; set; }

        public bool? CaptureOutput { get; set; }

        public bool? ShellExecute { get; set; }

        public Dictionary<string, object> ExtendedProperties { get; private set; }
    }
}