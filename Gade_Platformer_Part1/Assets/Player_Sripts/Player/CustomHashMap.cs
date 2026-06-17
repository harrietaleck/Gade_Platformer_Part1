using UnityEngine;

// ============================================================
// HashMapNode<TKey, TValue>
//
// A single key-value entry in the hash map's linked list chain.
// When two keys hash to the same bucket index (a collision),
// they form a chain via the 'Next' pointer � this is called
// separate chaining collision resolution.
// ============================================================
public class HashMapNode<TKey, TValue>
{
    public TKey Key;    // the lookup key (e.g. "jump")
    public TValue Value;  // the stored value (e.g. an AudioClip)

    // Points to the next node in this bucket's chain.
    // null means this is the last (or only) node in the chain.
    public HashMapNode<TKey, TValue> Next;

    public HashMapNode(TKey key, TValue value)
    {
        Key = key;
        Value = value;
        Next = null; // new nodes start with no chain
    }
}

// ============================================================
// CustomHashMap<TKey, TValue>  �  Hand-Written Hash Map (Part 3 D3)
//
// Implements a hash map using separate chaining:
//   - An array of 'buckets', each being the head of a linked list.
//   - Keys are mapped to a bucket index via GetHashCode() % capacity.
//   - Collisions are resolved by appending to the linked list at
//     that bucket (separate chaining).
//
// Average time complexity: O(1) for Put, Get, ContainsKey, Remove
// (assuming a good hash function and low load factor).
//
// Used by SFXManager to map string keys -> AudioClip references.
// ============================================================
public class CustomHashMap<TKey, TValue>
{
    // Array of bucket heads � each slot is the start of a linked list.
    private HashMapNode<TKey, TValue>[] buckets;

    private int capacity; // total number of buckets
    private int count;    // number of key-value pairs stored

    // Read-only access to internal stats (useful for debugging).
    public int Count => count;
    public int Capacity => capacity;

    // Constructor: choose a prime capacity to reduce clustering.
    // A prime number spreads keys more evenly across buckets.
    public CustomHashMap(int initialCapacity = 37)
    {
        capacity = initialCapacity > 0 ? initialCapacity : 37;
        buckets = new HashMapNode<TKey, TValue>[capacity];
        count = 0;
    }

    // Convert any key to a valid bucket index.
    // Math.Abs prevents negative indices from negative hash codes.
    private int GetBucketIndex(TKey key) =>
        System.Math.Abs(key.GetHashCode()) % capacity;

    // ------------------------------------------------------------------
    // Put � insert a new key-value pair, or update an existing one.
    // Walks the chain at the target bucket:
    //   - If the key is found, update its value in-place.
    //   - If not found, prepend a new node at the bucket head (O(1)).
    // ------------------------------------------------------------------
    public void Put(TKey key, TValue value)
    {
        int index = GetBucketIndex(key);

        // Walk the existing chain looking for a matching key.
        for (var n = buckets[index]; n != null; n = n.Next)
            if (n.Key.Equals(key)) { n.Value = value; return; } // update

        // Key not found � prepend a new node at the head of the chain.
        var node = new HashMapNode<TKey, TValue>(key, value);
        node.Next = buckets[index]; // link old head as next
        buckets[index] = node;           // new node becomes the head
        count++;
    }

    // ------------------------------------------------------------------
    // Get � retrieve a value by key.
    // Throws KeyNotFoundException if the key doesn't exist.
    // Use TryGet instead when absence is a normal case.
    // ------------------------------------------------------------------
    public TValue Get(TKey key)
    {
        for (var n = buckets[GetBucketIndex(key)]; n != null; n = n.Next)
            if (n.Key.Equals(key)) return n.Value;

        throw new System.Collections.Generic.KeyNotFoundException(
            "CustomHashMap: key '" + key + "' not found.");
    }

    // ------------------------------------------------------------------
    // TryGet � safe get that returns false instead of throwing.
    // Used by SFXManager.PlaySound() so missing clips log a warning
    // instead of crashing the game.
    // ------------------------------------------------------------------
    public bool TryGet(TKey key, out TValue value)
    {
        for (var n = buckets[GetBucketIndex(key)]; n != null; n = n.Next)
            if (n.Key.Equals(key)) { value = n.Value; return true; }

        value = default; // set out param to type default (null for objects)
        return false;
    }

    // ------------------------------------------------------------------
    // ContainsKey � check existence without retrieving the value.
    // ------------------------------------------------------------------
    public bool ContainsKey(TKey key)
    {
        for (var n = buckets[GetBucketIndex(key)]; n != null; n = n.Next)
            if (n.Key.Equals(key)) return true;
        return false;
    }

    // ------------------------------------------------------------------
    // Remove � unlink a node from its chain and decrement count.
    // Tracks the previous node so we can splice the target out.
    // Returns false if the key wasn't present.
    // ------------------------------------------------------------------
    public bool Remove(TKey key)
    {
        int index = GetBucketIndex(key);
        HashMapNode<TKey, TValue> prev = null;

        for (var n = buckets[index]; n != null; prev = n, n = n.Next)
        {
            if (!n.Key.Equals(key)) continue;

            // Splice out: if no previous node, the next node becomes head.
            if (prev == null) buckets[index] = n.Next;
            else prev.Next = n.Next;
            count--;
            return true;
        }
        return false; // key wasn't in the map
    }

    // ------------------------------------------------------------------
    // Clear � remove all entries without reallocating the bucket array.
    // Faster than creating a new map when you want to reuse the capacity.
    // ------------------------------------------------------------------
    public void Clear()
    {
        for (int i = 0; i < capacity; i++) buckets[i] = null;
        count = 0;
    }
}
