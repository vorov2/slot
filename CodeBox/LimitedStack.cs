using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeBox
{
    internal sealed class LimitedStack<T> : IEnumerable<T>
    {
        private const int CAPACITY = 100;
        private T[] items;
        private int top = 0;

        public LimitedStack() : this(CAPACITY)
        {

        }

        public LimitedStack(int capacity)
        {
            items = new T[capacity];
        }

        public void Push(T item)
        {
            items[top] = item;
            top = (top + 1) % items.Length;
        }

        public T Pop()
        {
            top = (items.Length + top - 1) % items.Length;
            return items[top];
        }

        public T Peek()
        {
            var idx = (items.Length + top - 1) % items.Length;
            return items[idx];
        }

        public IEnumerator<T> GetEnumerator()
        {
            return items.Take(top).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => top;
    }
}
