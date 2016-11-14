using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox.Lexing
{
    public sealed class StringTable
    {
        private const int BUCKET_SIZE = 3;
        private readonly List<Bucket> buckets = new List<Bucket>();
        private Bucket lastBucket;
        private Bucket lbBucket;

        class Bucket
        {
            public readonly char Char;
            public bool BucketValue;
            public readonly List<Bucket> Buckets = new List<Bucket>();
            public readonly List<char[]> Strings = new List<char[]>();
            public readonly List<int> Styles = new List<int>();

            public Bucket(char c)
            {
                Char = c;
            }
        }

        public StringTable(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        public void Add(string value, int code)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentNullException(nameof(value));

            InternalAdd(IgnoreCase ? value.ToUpper() : value, 0, buckets, code);
            Count++;
        }

        public void AddRange(string value, int code)
        {
            if (value == null || value.Length == 0)
                throw new ArgumentNullException(nameof(value));

            var arr = value.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Array.Sort(arr);

            foreach (var s in arr)
            {
                InternalAdd(s, 0, buckets, code);
                Count++;
            }
        }

        private void InternalAdd(string value, int offset, List<Bucket> buckets, int code)
        {
            var c = value[offset];

            foreach (Bucket b in buckets)
                if (b.Char == c)
                {
                    if (offset == BUCKET_SIZE - 1 || value.Length == offset + 1)
                    {
                        b.Strings.Add(value.ToCharArray());
                        b.Styles.Add(code);

                        if (value.Length == offset + 1)
                            b.BucketValue = true;
                    }
                    else
                        InternalAdd(value, ++offset, b.Buckets, code);

                    return;
                }

            var newBuck = new Bucket(c);
            buckets.Add(newBuck);

            if (value.Length == offset + 1)
                newBuck.BucketValue = true;

            if (offset == BUCKET_SIZE - 1 || value.Length == offset + 1)
            {
                newBuck.Strings.Add(value.ToCharArray());
                newBuck.Styles.Add(code);
            }
            else
                InternalAdd(value, ++offset, newBuck.Buckets, code);
        }

        [DebuggerHidden]
        public int Match(char c)
        {
            c = IgnoreCase ? char.ToUpper(c) : c;

            if (++Offset < BUCKET_SIZE)
            {
                var bList = Offset == 0 ? buckets : lastBucket.Buckets;
                int bCount = bList.Count;

                for (var i = 0; i < bCount; i++)
                {
                    var b = bList[i];

                    if (b.Char - c == 0)
                    {
                        lbBucket = lastBucket;
                        lastBucket = b;
                        var min = int.MaxValue;
                        var jj = 0;

                        for (var j = 0; j < b.Strings.Count; j++)
                        {
                            if (b.Strings[j].Length < min)
                            {
                                min = b.Strings[j].Length;
                                jj = j;
                            }
                        }

                        return b.BucketValue ? b.Styles[jj] : 0;
                    }
                }

                Offset--;
            }
            else
            {
                var sCount = lastBucket.Strings.Count;

                for (var i = 0; i < sCount; i++)
                {
                    var cz = lastBucket.Strings[i];

                    if (cz.Length >= Offset + 1 && cz[Offset] - c == 0)
                        return Offset == cz.Length - 1 ? lastBucket.Styles[i] : 0;
                }

                Offset--;
            }

            return -1;
        }

        [DebuggerHidden]
        public void Reset() => Offset = -1;

        [DebuggerHidden]
        public void ResetStep()
        {
            if (lbBucket == null)
                throw new ArgumentNullException(nameof(lbBucket));

            lastBucket = lbBucket;
            Offset--;
        }

        public int Offset { get; private set; } = -1;

        public int Count { get; private set; }

        public bool IgnoreCase { get; }
    }
}
