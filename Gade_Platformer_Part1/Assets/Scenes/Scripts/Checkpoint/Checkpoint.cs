using UnityEngine;

// ============================================================
// Checkpoint.cs  —  Custom Array-Based Stack ADT
//
// Implements a LIFO (Last-In First-Out) Stack using a plain
// array and an integer 'top' pointer. This is a hand-written
// data structure — it does NOT use System.Collections.Generic.Stack<T>
// or any other built-in collection class.
//
// HOW IT WORKS:
//   top = -1        → stack is empty
//   Push: top++, write item at items[top]
//   Pop:  read items[top], then top--
//   Peek: read items[top] without decrementing
// ============================================================
public class Checkpoint
{
    // Internal fixed-size array holding CheckpointData entries.
    // Capacity is set at construction time (default 50).
    private CheckpointData[] items;

    // Points to the index of the topmost item.
    // -1 means the stack is empty (no items have been pushed yet).
    private int top;

    // Maximum number of items the stack can hold.
    private readonly int capacity;

    // ---- Constructor ----
    // Allocates the internal array and initialises the empty state.
    public Checkpoint(int capacity = 50)
    {
        this.capacity = capacity;
        items = new CheckpointData[capacity];
        top = -1; // empty stack: top below index 0
    }

    // ---- Push ----
    // Adds a new CheckpointData entry on top of the stack.
    // Increments 'top' before writing so index 0 is the first slot used.
    // Prints a warning and discards the entry if the array is full.
    public void Push(CheckpointData data)
    {
        if (top >= capacity - 1)
        {
            Debug.LogWarning("Checkpoint Stack is full — oldest checkpoint overwritten.");
            return;
        }
        top++;            // advance pointer to the next free slot
        items[top] = data; // store the checkpoint snapshot
    }

    // ---- Pop ----
    // Removes and returns the top CheckpointData entry.
    // Decrements 'top' after reading, making the slot logically free.
    // Returns a zeroed entry if the stack is empty (safe default).
    public CheckpointData Pop()
    {
        if (IsEmpty())
        {
            Debug.LogWarning("Cannot Pop — Checkpoint Stack is empty.");
            return new CheckpointData(UnityEngine.Vector3.zero, 0, 0);
        }
        CheckpointData topItem = items[top]; // read current top
        top--;                                // move pointer down
        return topItem;
    }

    // ---- Peek ----
    // Returns the top CheckpointData WITHOUT removing it.
    // Used on death so the same checkpoint can be reused on
    // repeated deaths until the player reaches a new checkpoint.
    public CheckpointData Peek()
    {
        if (IsEmpty())
        {
            Debug.LogWarning("Cannot Peek — Checkpoint Stack is empty.");
            return new CheckpointData(UnityEngine.Vector3.zero, 0, 0);
        }
        return items[top]; // read without decrementing top
    }

    // ---- IsEmpty ----
    // Returns true when top == -1, meaning no items have been pushed.
    public bool IsEmpty()
    {
        return top == -1;
    }

    // ---- Count (convenience property) ----
    // Returns the number of items currently on the stack.
    public int Count => top + 1;
}

// ============================================================
// CheckpointData  —  Value snapshot stored in the Stack
//
// Captures the player's world position, remaining lives, and
// current score at the moment a checkpoint was activated.
// The Stack keeps one entry per checkpoint visited; Peek()
// retrieves the most recent without discarding it.
// ============================================================
[System.Serializable]
public class CheckpointData
{
    public UnityEngine.Vector3 position; // world position to respawn at
    public int lives;                    // lives at the time of checkpoint
    public int score;                    // score at the time of checkpoint

    // Constructor assigns all three fields in one call.
    public CheckpointData(UnityEngine.Vector3 positionPLY, int livesPLY, int scorePLY)
    {
        this.position = positionPLY;
        this.lives    = livesPLY;
        this.score    = scorePLY;
    }
}
