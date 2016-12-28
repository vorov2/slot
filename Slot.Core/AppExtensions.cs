using System;
using Slot.Core;
using Slot.Core.CommandModel;
using Slot.Core.Messages;
using Slot.Core.Output;
using Slot.Core.ViewModel;

namespace Slot
{
    public static class AppExtensions
    {
        private readonly static Identifier logKey = new Identifier("log.application");

        public static bool Run(this IAppExtensions _, Identifier key, params object[] args)
        {
            var view = App.Component<IViewManager>().ActiveView;
            return Run(null, view.Editor, key, args);
        }

        public static bool Run(this IAppExtensions _, IEditor editor, Identifier key, params object[] args)
        {
            var disp = App.Catalog<ICommandDispatcher>().GetComponent(key.Namespace);
            return disp != null ? disp.Execute(editor, key, args) : false;
        }

        public static void Log(this IAppExtensions _, string message, EntryType type)
        {
            App.Catalog<ILogComponent>().GetComponent(logKey).Write(message, type);
        }

        public static MessageButtons Show(this IAppExtensions _, string caption, string text, MessageButtons buttons = MessageButtons.Close)
        {
            return App.Catalog<IMessageBox>().Default().Show(caption, text, buttons);
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
