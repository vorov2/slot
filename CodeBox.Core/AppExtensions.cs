using CodeBox.Core;
using CodeBox.Core.CommandModel;
using CodeBox.Core.ComponentModel;
using System;

namespace CodeBox
{
    public static class AppExtensions
    {
        public static bool RunCommand(this IAppExtensions _, IExecutionContext ctx, Identifier key, params object[] args)
        {
            var disp = App.Catalog<ICommandDispatcher>().GetComponent(key.Namespace);
            return disp != null ? disp.Execute(ctx, key, args) : false;
        }
    }
}
