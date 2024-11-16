namespace WindControlLib
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class CFifoConcurrentQueue<T>
    {
        private readonly ConcurrentQueue<T> queue;

        // Example: CFifoBuffer<byte> data_in;
        public CFifoConcurrentQueue()
        {
            queue = new();
        }

        // Clear the buffer
        public void Clear()
        {
            queue.Clear();
        }

        // Add an item to the buffer
        public void Push(T item)
        {
            queue.Enqueue(item);
        }

        // Add multiple items to the buffer (from an array)
        public void Push(T[] items)
        {
            ArgumentNullException.ThrowIfNull(items);
            foreach (T item in items)
                queue.Enqueue(item);
        }

        // Add multiple items to the buffer (from a list)
        public void Push(List<T> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            foreach (T item in items)
            {
                queue.Enqueue(item);
            }
        }

        // Remove and return the oldest item from the buffer
        public T? Pop()
        {
            queue.TryDequeue(out T? item);
            return item;
        }

        // Remove and return the oldest n items from the buffer
        public T[]? Pop(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "The number of items to pop cannot be negative.");
            }

            List<T> items = [];
            for (int i = 0; i < n; i++)
            {
                if (queue.TryDequeue(out T? item))
                {
                    items.Add(item);
                }
                else
                {
                    break; // Stop if there are no more items to dequeue
                }
            }
            return [.. items];
        }

        public void Pop(ref T[] dataFromQueue)
        {
            if (dataFromQueue.Length <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dataFromQueue), "The size of dataFromQueue cannot be 0 or negative.");

            }
            if (Count < dataFromQueue.Length)
            {
                dataFromQueue = [];
                return;
            }

            for (int i = 0; i < dataFromQueue.Length; i++)
            {
                if (queue.TryDequeue(out T? item))
                {
                    dataFromQueue[i] = item;
                }
            }
        }


        // Peek at the oldest item without removing it
        public T? Peek()
        {
            if (queue.IsEmpty)
            {
                throw new InvalidOperationException("The buffer is empty.");
            }

            queue.TryPeek(out T? pk);
            return pk;
        }

        // Peek at the nth item without removing it
        public T Peek(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "Index cannot be negative.");
            }

            using var enumerator = queue.GetEnumerator();
            int currentIndex = 0;

            while (enumerator.MoveNext())
            {
                if (currentIndex == n)
                {
                    return enumerator.Current;
                }
                currentIndex++;
            }

            // If we reach here, it means the requested index is out of range
            throw new ArgumentOutOfRangeException(nameof(n), "Index is out of range.");
        }


        // Check if the buffer is empty
        public bool IsEmpty => queue.IsEmpty;

        // Get the number of items in the buffer
        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        // Remove and return all items from the buffer
        public T[]? PopAll()
        {
            // If the queue is empty, return null and set isOK to false
            if (queue.IsEmpty)
            {
                return null;
            }

            // Use a List to accumulate dequeued items
            List<T> items = [];
            while (queue.TryDequeue(out T? item))
            {
                items.Add(item);
            }

            // Return the dequeued items as an array
            return [.. items];
        }
    }
}