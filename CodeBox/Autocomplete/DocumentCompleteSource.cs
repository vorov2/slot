﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;
using CodeBox.Lexing;

namespace CodeBox.Autocomplete
{
    public sealed class DocumentCompleteSource : ICompleteSource
    {
        private Dictionary<int, Dictionary<string, object>> completes = new Dictionary<int, Dictionary<string, object>>();
        private volatile bool busy;
        private volatile int lastUpdate;
        private volatile int lastEdits = -1;
        private IEditorContext context;
        private const string SEPS = " \r\n\t`~!@#$%^&*()=+[{]}\\|;:'\",.<>/?";

        public void Initialize(IEditorContext context)
        {
            this.context = context;
        }

        public IEnumerable<string> GetItems()
        {
            var t = default(Task);

            if (!busy && 
                (lastUpdate == 0 || DateTime.Now - new DateTime(lastUpdate * TimeSpan.TicksPerMillisecond) > TimeSpan.FromSeconds(10)))
                t = Task.Run(() => WalkDocument(context));

            if (completes.Count == 0 && t != null)
                t.Wait();

            var id = context.Buffer.Document.Lines[context.Buffer.Selections.Main.Caret.Line].GrammarId;
            Dictionary<string, object> dict;

            if (completes.TryGetValue(id, out dict))
            {
                var items = dict.Keys.ToList();
                items.Sort();
                return items;
            }
            else
                return Enumerable.Empty<string>();
        }

        private void WalkDocument(IEditorContext ctx)
        {
            if (ctx.Buffer.Edits == lastEdits)
                return;

            busy = true;
            var arr = ctx.Buffer.Document.Lines.ToList();

            foreach (var line in arr)
            {
                if (line.Length < 2)
                    continue;

                var seps = ctx.GrammarManager.GetNonWordSymbols(line);
                var dict = default(Dictionary<string, object>);

                if (!completes.TryGetValue(line.GrammarId, out dict))
                    completes.Add(line.GrammarId, dict = new Dictionary<string, object>());

                foreach (var str in line.Text.Split((" \t" + seps).ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    if (str.Length > 1 && !dict.ContainsKey(str) && !char.IsDigit(str[0]))
                        dict.Add(str, null);
                }
            }

            lastUpdate = DateTime.Now.Millisecond;
            busy = false;
        }
    }
}
