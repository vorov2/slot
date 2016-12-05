using System;
using System.Collections.Generic;
using TT = System.Threading.Tasks;

namespace TaskEngine
{
    public sealed class Task
    {
        private ExecMonitor currentStep;
        private volatile bool running;

        private sealed class WaitHandle : ITaskWaitHandle
        {
            private readonly TT.Task task;
            private readonly Task self;

            internal WaitHandle(Task self, TT.Task task)
            {
                this.task = task;
                this.self = self;
            }

            void ITaskWaitHandle.Wait()
            {
                task.Wait();
            }

            bool ITaskWaitHandle.Wait(TimeSpan timeout)
            {
                return task.Wait(timeout);
            }

            bool ITaskWaitHandle.Terminate()
            {
                return self.Terminate();
            }

            bool ITaskWaitHandle.Terminate(TimeSpan timeout)
            {
                if (!task.Wait(timeout))
                    return self.Terminate();
                else
                    return true;
            }

            public bool Running
            {
                get { return self.running; }
            }
        }

        public Task()
        {
            Steps = new List<TaskStep>();
            Variables = new Dictionary<string, string>();
        }

        public ITaskWaitHandle Run()
        {
            var tt = TT.Task.Run(() => InternalRun());
            return new WaitHandle(this, tt);
        }

        private void InternalRun()
        {
            running = true;
            Write($"Task {Name}");
            var errors = 0;
            var idx = 0;

            foreach (var s in Steps)
            {
                if (s.Properties != null && DefaultProperties != null)
                    s.Properties = s.Properties.MergeWith(DefaultProperties);
                else if (DefaultProperties != null)
                    s.Properties = DefaultProperties.Clone();

                Write($"{++idx}. Step {s.Name}");

                var res = false;
                currentStep = s.Start(Output);
                var tt = TT.Task.Run(() => res = currentStep.Wait());
                tt.Wait();
                
                if (!res)
                    errors++;

                if (!res && Behavior == TaskBehavior.FailFirst)
                {
                    Write($"Task terminated ({Behavior}).");
                    break;
                }
            }

            Write($"Task completed. Errors: {errors}. Passed: {Steps.Count-errors}");
            running = false;
        }

        private bool Terminate()
        {
            if (running && currentStep != null)
                return currentStep.Kill();

            return false;
        }

        private void Write(object data)
        {
            if (!SuppressOwnOutput)
                Output.Write(data);
        }

        public override string ToString()
        {
            return $"Task:{Name};Steps:{Steps.Count};Behavior:{Behavior}";
        }

        public string Name { get; set; }

        public TaskBehavior Behavior { get; set; }

        private IOutput _output;
        public IOutput Output 
        {
            get { return _output ?? NilOutput.Instance; }
            set { _output = value; }
        }

        public Dictionary<string, string> Variables { get; }

        public TaskStepProperties DefaultProperties { get; set; }

        public bool SuppressOwnOutput { get; set; }

        public List<TaskStep> Steps { get; }
    }
}