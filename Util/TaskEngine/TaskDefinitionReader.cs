using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace TaskEngine
{
    public sealed class TaskDefinitionReader
    {
        public TaskDefinitionReader(string fileName)
        {
            FileName = fileName;
        }

        public Task Read()
        {
            var ser = new Json.JsonParser(File.ReadAllText(FileName)) { SkipNulls = true };
            var dict = ser.Parse() as Dictionary<string, object>;

            var task = new Task
            {
                Name = GetParamStr(dict, "name", true)
            };
            task.DefaultProperties = ReadProperties(dict);

            var vars = GetParam(dict, "variables") as Dictionary<string, object>;

            if (vars != null)
                foreach (var kv in vars)
                    task.Variables.Add(kv.Key, (kv.Value == null ? "" : kv.Value).ToString());

            var steps = GetParam(dict, "steps", true) as List<object>;

            if (steps != null)
            {
                foreach (var o in steps)
                {
                    var stepDic = o as Dictionary<string, object>;
                    
                    if (stepDic != null)
                        task.Steps.Add(CreateStep(stepDic, task));
                }
            }

            return task;
        }

        private TaskStepProperties ReadProperties(Dictionary<string, object> dict)
        {
            var props = new TaskStepProperties();

            foreach (var kv in dict)
            {
                switch (kv.Key)
                {
                    case "name":
                    case "command":
                    case "arguments":
                    case "variables":
                        break;
                    case "workingDirectory":
                        props.WorkingDirectory = (kv.Value ?? "").ToString();
                        break;
                    case "captureOutput":
                        props.CaptureOutput = kv.Value is bool ? (bool)kv.Value : false;
                        break;
                    case "shellExecute":
                        props.ShellExecute = kv.Value is bool ? (bool)kv.Value : false;
                        break;
                    case "createWindow":
                        props.CreateWindow = kv.Value is bool ? (bool)kv.Value : false;
                        break;
                    default:
                        if (kv.Value != null && !props.ExtendedProperties.ContainsKey(kv.Key))
                            props.ExtendedProperties.Add(kv.Key, kv.Value);
                        break;
                }
            }

            return props;
        }

        private TaskStep CreateStep(Dictionary<string, object> dict, Task parent)
        {
            var step = new DefaultTaskStep(parent)
            {
                Name = GetParamStr(dict, "name", true),
                Command = GetParamStr(dict, "command", true)
            };
            step.Properties = ReadProperties(dict);

            var args = GetParam(dict, "arguments");
            var argList = args as List<object>;

            if (argList != null)
                step.Arguments.AddRange(argList.Select(Argument.FromObject));
            else
            {
                var argDict = args as Dictionary<string, object>;
                if (argDict != null)
                    step.Arguments.AddRange(argDict.Select(kv => Argument.FromObject(kv.Key, kv.Value)));
            }

            return step;
        }

        private string GetParamStr(Dictionary<string, object> dict, string key, bool required = false)
        {
            var ret = GetParam(dict, key, required);
            return ret != null ? ret.ToString() : null;
        }

        private object GetParam(Dictionary<string, object> dict, string key, bool required = false)
        {
            var val = default(object);

            if (!dict.TryGetValue(key, out val) || val == null)
            {
                if (required)
                    throw new Exception($"Missing required attribute '{key}'.");
                else
                    return null;
            }
            else
                return val;
        }

        public string FileName { get; }
    }
}