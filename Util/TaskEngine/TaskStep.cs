using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using StringMacro;

namespace TaskEngine
{
    public abstract class TaskStep
    {
        protected TaskStep(Task parent)
        {
            Arguments = new List<Argument>();
            ParentTask = parent;
        }

        protected string ExpandVariables(string value)
        {
            if (value == null)
                return value;

            var macros = new MacroParser(ParentTask.Variables, VariableProviders.Default);
            var val = macros.Parse(value);

            if (val.IndexOfAny(new char[] { ' ', '\t' }) != -1)
                val = $"\"{value}\"";
            
            return val;
        }

        protected string QuoteString(string str)
        {
            if (str == null)
                return str;

            if (str.IndexOfAny(new char[] { ' ', '\t' }) != -1)
                str = $"\"{str}\"";

            return str;
        }

        protected string QualifyFileName(string fileName)
        {
            if (fileName == null)
                return fileName;
            else
                fileName = ExpandVariables(fileName);

            if (Properties?.WorkingDirectory != null
                && fileName.IndexOfAny(new char[] { '\\', '/' }) == -1)
                fileName = Path.Combine(Properties?.WorkingDirectory, fileName);

            return QuoteString(fileName);
        }

        protected string ArgumentsToString()
        {
            var sb = new StringBuilder();

            foreach (var a in Arguments)
            {
                if (a.Value != null)
                {
                    var str = QuoteString(ExpandVariables(a.Value.ToString()));
                    sb.Append(str);
                    sb.Append(' ');
                }
            }

            return sb.ToString();
        }

        protected ProcessStartInfo CreateProcessStartInfo()
        {
            return new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = Properties?.ShellExecute ?? false,
                WorkingDirectory = Properties?.WorkingDirectory,
                CreateNoWindow = !(Properties?.CreateWindow ?? false),
                FileName = QuoteString(QualifyFileName(Command)),
                Arguments = ArgumentsToString()
            };
        }

        protected void WriteOutput(string data, IOutput output)
        {
            if (!String.IsNullOrEmpty(data))
                output.Write($">>>{data.Trim()}");
        }

        protected ExecMonitor RunProcess(ProcessStartInfo psi, IOutput output, Action<string,IOutput> write)
        {
            var prc = new Process { 
                StartInfo = psi
            };

            if (Properties?.CaptureOutput ?? false)
            {
                prc.ErrorDataReceived += (o,e) => write(e.Data, output);
                prc.OutputDataReceived += (o,e) => write(e.Data, output);
            }
            
            var monitor = new ExecMonitor(
                () => RunProcess(prc, output, write),
                () => KillProcess(prc)
            );

            return monitor;
        }

        private bool RunProcess(Process prc, IOutput output, Action<string, IOutput> write)
        {
            try
            {
                prc.Start();
                prc.BeginOutputReadLine();
                prc.BeginErrorReadLine();
                prc.WaitForExit();
                return true;
            }
            catch (Exception ex) 
            {
                write(ex.Message, output);
                return false;
            }
        }

        private bool KillProcess(Process prc)
        {
            try 
            {
                prc.Kill();
                return true;
            }
            catch
            { 
                return false;
            }
        }

        public abstract ExecMonitor Start(IOutput output);

        protected Task ParentTask { get; }

        public string Name { get; set; }

        public string Command { get; set; }

        public List<Argument> Arguments { get; }

        public TaskStepProperties Properties { get; set; }
    }
}