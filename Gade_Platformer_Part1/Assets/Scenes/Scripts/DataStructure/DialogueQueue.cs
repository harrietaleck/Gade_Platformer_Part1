using UnityEngine;

// ============================================================
// DialogueQueue<T>  —  Custom Array-Based Queue ADT
//
// Implements a FIFO (First-In First-Out) Queue using a fixed-size
// circular array and head / tail integer pointers.
// This is a hand-written data structure — it does NOT use
// System.Collections.Generic.Queue<T>, List<T>, or any other
// built-in collection.
//
// HOW IT WORKS (circular buffer):
//   head  = index of the next item to Dequeue
//   tail  = index where the next Enqueue will write
//   count = number of items currently in the queue
//
//   Enqueue: write at items[tail], advance tail = (tail+1) % capacity
//   Dequeue: read items[head],    advance head = (head+1) % capacity
//   Peek:    read items[head] without advancing head
//   IsEmpty: count == 0
// ============================================================
public class DialogueQueue<T>
{
    // Fixed-size array that stores queue entries.
    private T[] items;

    // Index of the oldest item (next to be dequeued).
    private int head;

    // Index where the next item will be written.
    private int tail;

    // Number of items currently in the queue.
    private int count;

    // Maximum items the queue can hold.
    private readonly int capacity;

    // ---- Constructor ----
    // Allocates the backing array and initialises the empty state.
    public DialogueQueue(int capacity = 100)
    {
        this.capacity = capacity;
        items = new T[capacity];
        head  = 0;
        tail  = 0;
        count = 0;
    }

    // ---- Count (convenience property) ----
    public int Count => count;

    // ---- IsEmpty ----
    // Returns true when no items have been enqueued (or all have been dequeued).
    public bool IsEmpty() => count == 0;

    // ---- Enqueue ----
    // Adds a new item at the back of the queue.
    // Advances tail around the circular array after writing.
    // Logs a warning and discards the item if the array is full.
    public void Enqueue(T item)
    {
        if (count >= capacity)
        {
            Debug.LogWarning("DialogueQueue is full — item discarded.");
            return;
        }
        items[tail] = item;               // write at the current tail slot
        tail = (tail + 1) % capacity;     // advance tail, wrapping at end
        count++;
    }

    // ---- Dequeue ----
    // Removes and returns the front (oldest) item.
    // Advances head after reading so the slot is logically free.
    // Throws if the queue is empty (caller should check IsEmpty first).
    public T Dequeue()
    {
        if (IsEmpty())
            throw new System.InvalidOperationException("DialogueQueue is empty.");

        T frontItem = items[head];         // read the oldest item
        items[head] = default;             // clear the slot (helps GC for ref types)
        head = (head + 1) % capacity;      // advance head, wrapping at end
        count--;
        return frontItem;
    }

    // ---- Peek ----
    // Returns the front item WITHOUT removing it.
    // Useful for inspecting the next line before showing it.
    // Throws if the queue is empty.
    public T Peek()
    {
        if (IsEmpty())
            throw new System.InvalidOperationException("DialogueQueue is empty.");

        return items[head]; // read without moving head
    }

    // ---- Clear ----
    // Resets the queue to empty without reallocating the array.
    public void Clear()
    {
        head  = 0;
        tail  = 0;
        count = 0;
    }
}
