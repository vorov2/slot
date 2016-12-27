using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Slot.Editor.ObjectModel;
using Slot.Editor.Lexing;
using Slot.Editor.Affinity;
using Slot.Core.ComponentModel;
using Slot.Core.ViewModel;

namespace Slot.Editor.Autocomplete
{
    public sealed class DocumentCompleteSource : ICompleteSource
    {
        private List<string> completes = new List<string>();
        private volatile bool busy;
        private DateTime lastUpdate;
        private int lastEdits = -1;
        private EditorControl ed;
        private const string SEPS = " \r\n\t`~!@#$%^&*()=+[{]}\\|;:'\",.<>/?";

        public void Initialize(IView view)
        {
            this.ed = (EditorControl)view.Editor;
        }

        public IEnumerable<string> GetItems()
        {
            var t = default(Task);

            if (!busy &&
                (lastUpdate == DateTime.MinValue || DateTime.Now - lastUpdate > TimeSpan.FromSeconds(10)))
            {
                Console.WriteLine("Generate CompleteSource");
                t = Task.Run(() => WalkDocument());
            }

            if (completes.Count == 0 && t != null)
                t.Wait();

            return completes;
        }

        private void WalkDocument()
        {
            if (ed.Buffer.Edits == lastEdits)
                return;

            busy = true;
            completes.Clear();
            var grm = ed.AffinityManager.GetRootAffinity();
            var txt = ed.Buffer.GetContents();
            var seps = grm.GetNonWordSymbols(ed);
            var dict = new Dictionary<string, object>();

            foreach (var str in txt.Split((" \t\r\n" + seps).ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (str.Length > 1 && !dict.ContainsKey(str) && !char.IsDigit(str[0]))
                    dict.Add(str, null);
            }

            completes.AddRange(dict.Keys);
            completes.Sort();
            lastUpdate = DateTime.Now;
            busy = false;
        }
    }
}
