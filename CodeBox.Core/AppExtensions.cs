using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using CodeBox.Core.Output;
using System;

namespace CodeBox
{
    public static class AppExtensions
    {
        private readonly static Identifier logKey = new Identifier("log.application");

        public static bool Run(this IAppExtensions _, IExecutionContext ctx, Identifier key, params object[] args)
        {
            var disp = App.Catalog<ICommandDispatcher>().GetComponent(key.Namespace);
            return disp != null ? disp.Execute(ctx, key, args) : false;
        }

        public static void Log(this IAppExtensions _, string message, EntryType type)
        {
            App.Catalog<ILogComponent>().GetComponent(logKey).Write(message, type);
        }

        public static ExecResult Handle(this IAppExtensions _, Action act)
        {
            try
            {
                act();
                return ExecResult.Ok;
            }
            catch (Exception ex)
            {
                return ExecResult.Failure(ex.Message);
            }
        }
    }
}
