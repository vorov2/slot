using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeBox.ObjectModel;

namespace CodeBox.Autocomplete
{
    public sealed class DocumentCompleteSource : ICompleteSource
    {
        private List<string> items;
        private volatile bool busy;
        private volatile int lastUpdate;

        public IEnumerable<string> GetItems(DocumentBuffer buffer)
        {
            var t = default(Task);

            if (!busy && 
                (lastUpdate == 0 || DateTime.Now - new DateTime(lastUpdate * TimeSpan.TicksPerMillisecond) > TimeSpan.FromSeconds(10)))
                t = Task.Run(() => WalkDocument(buffer));

            if (items == null && t != null)
                t.Wait();

            return items;
        }

        private void WalkDocument(DocumentBuffer buffer)
        {
            busy = true;
            const string seps = " \r\n\t`~!@#$%^&*()_+=-[].,></?\"';:|/\\";
            var dict = new Dictionary<string, object>();
            var digits = "0123456789".ToCharArray();

            foreach (var str in buffer.GetText().Split(seps.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
            {
                if (str.Length > 2 && !dict.ContainsKey(str) && str.IndexOfAny(digits) == -1)
                    dict.Add(str, null);
            }

            items = dict.Keys.ToList();
            items.Sort();
            lastUpdate = DateTime.Now.Millisecond;
            busy = false;
        }
    }
}
