namespace WindControlLib
{
    using System;
    using System.Collections.Generic;

    public class CFifoBuffer<T>
    {
        private Queue<T> queue;
        private readonly object lockObject = new();

        // Example: CFifoBuffer<byte> data_in;
        public CFifoBuffer()
        {
            queue = new Queue<T>();
        }

        // Clear the buffer
        public void Clear()
        {
            lock (lockObject)
            {
                queue.Clear();
            }
        }

        // Add an item to the buffer
        public void Push(T item)
        {
            lock (lockObject)
            {
                queue.Enqueue(item);
            }
        }

        // Add multiple items to the buffer (from an array)
        public void Push(T[] items)
        {
            ArgumentNullException.ThrowIfNull(items);

            lock (lockObject)
            {
                foreach (T item in items)
                {
                    queue.Enqueue(item);
                }
            }
        }

        // Add multiple items to the buffer (from a list)
        public void Push(List<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            lock (lockObject)
            {
                foreach (T item in items)
                {
                    queue.Enqueue(item);
                }
            }
        }

        // Remove and return the oldest item from the buffer
        public T? Pop()
        {
            lock (lockObject)
            {
                if (queue.Count == 0)
                {
                    return default; // Return the default value for the type T
                }

                return queue.Dequeue();
            }
        }

        // Remove and return the oldest n items from the buffer
        public T[]? Pop(int n)
        {
            lock (lockObject)
            {
                if (n < 0 || n > queue.Count)
                {
                    return default;
                }

                T[] ret = new T[n];
                for (int i = 0; i < n; i++)
                {
                    ret[i] = queue.Dequeue();
                }

                return ret;
            }
        }

        // Peek at the oldest item without removing it
        public T Peek()
        {
            lock (lockObject)
            {
                if (queue.Count == 0)
                {
                    throw new InvalidOperationException("The buffer is empty.");
                }

                return queue.Peek();
            }
        }

        // Peek at the nth item without removing it
        public T Peek(int n)
        {
            lock (lockObject)
            {
                if (n < 0 || n >= queue.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(n), "Index is out of range.");
                }

                using var enumerator = queue.GetEnumerator();
                for (int i = 0; i <= n; i++)
                {
                    enumerator.MoveNext();
                }
                return enumerator.Current;
            }
        }

        // Check if the buffer is empty
        public bool IsEmpty
        {
            get
            {
                lock (lockObject)
                {
                    return queue.Count == 0;
                }
            }
        }

        // Get the number of items in the buffer
        public int Count
        {
            get
            {
                lock (lockObject)
                {
                    return queue.Count;
                }
            }
        }

        // Remove and return all items from the buffer
        public T[] PopAll()
        {
            lock (lockObject)
            {
                T[] ret = new T[queue.Count];

                for (int i = 0; i < ret.Length; i++)
                {
                    ret[i] = queue.Dequeue();
                }

                return ret;
            }
        }
    }
}