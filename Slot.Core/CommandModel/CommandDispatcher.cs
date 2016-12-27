using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using Slot.Core.Output;
using Slot.Core.ViewModel;

namespace Slot.Core.CommandModel
{
    public abstract class CommandDispatcher : ICommandDispatcher
    {
        [Import]
        protected IViewManager ViewManager = null;

        [Import]
        protected ICommandProvider CommandProvider = null;

        private Dictionary<string, MethodInfo> commands;

        public bool Execute(IEditor ed, Identifier commandKey, params object[] args) 
        {
            ResolveCommands();
            MethodInfo cmd;

            if (!commands.TryGetValue(commandKey.Name, out cmd))
                return false;

            var meta = CommandProvider.GetCommandByKey(commandKey);
            var mode = ViewManager.GetActiveView()?.Mode;

            if (meta.Mode != null && meta.Mode != mode)
            {
                App.Ext.Log($"Unable to execute command '{commandKey}' in mode '{mode}'.", EntryType.Error);
                return false;
            }

            try
            {
                var vals = args;
                var pars = cmd.GetParameters();

                if (args != null)
                {
                    vals = new object[pars.Length];

                    for (var i = 0; i < pars.Length; i++)
                    {
                        if (args.Length <= i && !pars[i].HasDefaultValue)
                        {
                            ProcessNotEnoughArguments(commandKey, args);
                            return false;
                        }

                        var cval = args.Length > i ? args[i] : pars[i].DefaultValue;
                        object fval = cval;

                        if (!Converter.Convert(cval, pars[i].ParameterType, out fval) && cval != null)
                        {
                            App.Ext.Log($"Invalid type of an argument '{pars[i].Name}' of command '{commandKey}'.", EntryType.Error);
                            return false;
                        }

                        vals[i] = fval;
                    }
                }

                if (vals.Length < pars.Length)
                {
                    ProcessNotEnoughArguments(commandKey, args);
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

        protected virtual void ProcessNotEnoughArguments(Identifier commandKey, object[] args)
        {
            var cmd = App.Catalog<ICommandProvider>().Default().GetCommandByKey(commandKey);
            App.Catalog<ICommandBar>().Default().Show(cmd.Alias, args);
        }

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
