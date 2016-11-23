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
                cmd.Invoke(this, args);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ResolveCommands()
        {
            if (commands != null)
                return;

            commands = new Dictionary<string, MethodInfo>();
            foreach (var mi in GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                commands.Add(mi.Name.ToLower(), mi);
        }
    }
}
