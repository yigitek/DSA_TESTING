namespace DSA_TESTING;

using System;

public class LinearProbingHashTable<TKey, TValue> : IInsertable<TKey, TValue>, ISearchable<TKey, TValue>, IDeletable<TKey>
    where TKey : IComparable<TKey>
{
    private class Entry
    {
        public TKey Key;
        public TValue Value;
        public bool IsActive; // Indicates if the entry is actively used
    }

    private Entry[] entries;
    private int count;
    private int capacity;
    private const double LoadFactor = 0.75;

    public LinearProbingHashTable(int capacity = 16)
    {
        this.capacity = Math.Max(capacity, 16); // Ensure minimum capacity
        this.entries = new Entry[this.capacity];
        this.count = 0;
    }

    private int GetHash(TKey key)
    {
        return Math.Abs(key.GetHashCode()) % capacity;
    }

    public void Insert(TKey key, TValue value)
    {
        if (count >= capacity * LoadFactor)
            Resize();

        int hash = GetHash(key);
        int firstEmptyIndex = -1;

        while (entries[hash] != null)
        {
            if (entries[hash].IsActive && entries[hash].Key.Equals(key))
            {
                // Overwrite existing key
                entries[hash].Value = value;
                return;
            }

            if (!entries[hash].IsActive && firstEmptyIndex == -1)
                firstEmptyIndex = hash; // Track the first empty slot

            hash = (hash + 1) % capacity;
        }

        if (firstEmptyIndex != -1)
            hash = firstEmptyIndex; // Use the first encountered empty slot

        if (entries[hash] == null)
            entries[hash] = new Entry();

        entries[hash].Key = key;
        entries[hash].Value = value;
        entries[hash].IsActive = true;
        count++;
    }

    private void Resize()
    {
        Entry[] oldEntries = entries;
        capacity *= 2;
        entries = new Entry[capacity];
        count = 0;

        foreach (Entry entry in oldEntries)
        {
            if (entry != null && entry.IsActive)
                Insert(entry.Key, entry.Value);
        }
    }

    public TValue Search(TKey key)
    {
        int hash = GetHash(key);

        while (entries[hash] != null)
        {
            if (entries[hash].IsActive && entries[hash].Key.Equals(key))
                return entries[hash].Value;
            hash = (hash + 1) % capacity;
        }

        throw new Exception("Key not found.");
    }

    public void Delete(TKey key)
    {
        int hash = GetHash(key);

        while (entries[hash] != null)
        {
            if (entries[hash].IsActive && entries[hash].Key.Equals(key))
            {
                entries[hash].IsActive = false;
                count--;
                return;
            }
            hash = (hash + 1) % capacity;
        }

        throw new Exception("Key not found.");
    }

    public void Traverse()
    {
        foreach (Entry entry in entries)
        {
            if (entry != null && entry.IsActive)
                Console.WriteLine($"Key: {entry.Key}, Value: {entry.Value}");
        }
    }
}
