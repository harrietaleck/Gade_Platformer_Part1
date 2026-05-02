using System.Collections.Generic;

// FIFO queue for dialogue (Queue ADT)
public class DialogueQueue<T>
{
    private readonly List<T> items = new List<T>();

    public int Count => items.Count;

    public bool IsEmpty() => items.Count == 0;

    public void Enqueue(T item) => items.Add(item);

    public T Dequeue()
    {
        if (IsEmpty())
            throw new System.InvalidOperationException("Queue is empty.");

        T first = items[0];
        items.RemoveAt(0);
        return first;
    }

    public T Peek()
    {
        if (IsEmpty())
            throw new System.InvalidOperationException("Queue is empty.");

        return items[0];
    }

    public void Clear() => items.Clear();
}