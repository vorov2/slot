using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.Core.ComponentModel;
using System.Reflection;

namespace CodeBox.Core.CommandModel
{
    public abstract class CommandDispatcher : ICommandDispatcher
    {
        private Dictionary<string, MethodInfo> commands;

        public bool Execute(IExecutionContext ctx, Identifier commandKey, params object[] args)
        {
            ResolveCommands();
            MethodInfo cmd;

            if (!commands.TryGetValue(commandKey.Name, out cmd))
                return false;

            try
            {
                var vals = args;
                var pars = cmd.GetParameters();

                if (args != null && args.Length > 0)
                {
                    vals = new object[pars.Length];

                    for (var i = 0; i < pars.Length; i++)
                    {
                        if (!pars[i].HasDefaultValue)
                        {
                            ProcessNotEnoughArguments(ctx, commandKey);
                            return false;
                        }

                        var cval = args.Length > i ? args[i] : pars[i].DefaultValue;
                        vals[i] = cval;
                    }
                }

                if (vals.Length < pars.Length)
                {
                    ProcessNotEnoughArguments(ctx, commandKey);
                    return false;
                }

                cmd.Invoke(this, vals);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected abstract void ProcessNotEnoughArguments(IExecutionContext ctx, Identifier commandKey);

        private void ResolveCommands()
        {
            if (commands != null)
                return;

            commands = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var mi in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                if (Attribute.IsDefined(mi, typeof(CommandAttribute)))
                    commands.Add(mi.Name.ToLower(), mi);
        }
    }
}
