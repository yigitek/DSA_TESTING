namespace DSA_TESTING;

using System;
using System.Collections.Generic;
using System.Linq;

public class BPlusTree<TKey, TValue> : IInsertable<TKey, TValue>, ISearchable<TKey, TValue>, IDeletable<TKey>
    where TKey : IComparable<TKey>
{
    private class Node
    {
        public List<TKey> Keys = new List<TKey>();
        public Node Parent;
        public bool IsLeaf;
        public List<Node> Children = new List<Node>();
        public Node Next; // for linked list of leaf nodes
        public Node Previous; // for linked list of leaf nodes
        public List<TValue> Values = new List<TValue>(); // only used if node is a leaf
    }

    private Node root;
    private int degree;

    public BPlusTree(int degree)
    {
        if (degree < 2) throw new ArgumentException("Degree must be at least 2", nameof(degree));
        this.degree = degree;
        root = new Node { IsLeaf = true };
    }

    public void Insert(TKey key, TValue value)
    {
        Node node = root;
        while (!node.IsLeaf)
        {
            int childIndex = 0;
            while (childIndex < node.Keys.Count && key.CompareTo(node.Keys[childIndex]) > 0)
            {
                childIndex++;
            }
            node = node.Children[childIndex];
        }

        int position = 0;
        while (position < node.Keys.Count && key.CompareTo(node.Keys[position]) > 0)
        {
            position++;
        }
        node.Keys.Insert(position, key);
        node.Values.Insert(position, value);
        
        if (node.Keys.Count >= degree)
        {
            SplitLeafNode(node);
        }
    }

    private void SplitLeafNode(Node node)
    {
        int midIndex = degree / 2;
        Node newNode = new Node { IsLeaf = true };

        newNode.Keys.AddRange(node.Keys.GetRange(midIndex, node.Keys.Count - midIndex));
        newNode.Values.AddRange(node.Values.GetRange(midIndex, node.Values.Count - midIndex));
        node.Keys.RemoveRange(midIndex, node.Keys.Count - midIndex);
        node.Values.RemoveRange(midIndex, node.Values.Count - midIndex);

        newNode.Next = node.Next;
        newNode.Previous = node;
        if (newNode.Next != null)
        {
            newNode.Next.Previous = newNode;
        }
        node.Next = newNode;

        InsertIntoParent(node, newNode.Keys[0], newNode);
    }

    private void InsertIntoParent(Node oldNode, TKey newKey, Node newNode)
    {
        if (oldNode == root)
        {
            root = new Node { IsLeaf = false };
            root.Keys.Add(newKey);
            root.Children.Add(oldNode);
            root.Children.Add(newNode);
            oldNode.Parent = root;
            newNode.Parent = root;
            return;
        }

        Node parent = oldNode.Parent;
        int index = 0;
        while (index < parent.Keys.Count && newKey.CompareTo(parent.Keys[index]) > 0)
        {
            index++;
        }
        parent.Keys.Insert(index, newKey);
        parent.Children.Insert(index + 1, newNode);
        newNode.Parent = parent;

        if (parent.Keys.Count >= degree)
        {
            SplitInternalNode(parent);
        }
    }

    private void SplitInternalNode(Node node)
    {
        int midIndex = degree / 2;
        TKey upKey = node.Keys[midIndex];
        Node newNode = new Node { IsLeaf = false };

        newNode.Keys.AddRange(node.Keys.GetRange(midIndex + 1, node.Keys.Count - midIndex - 1));
        newNode.Children.AddRange(node.Children.GetRange(midIndex + 1, node.Children.Count - midIndex - 1));
        node.Keys.RemoveRange(midIndex, node.Keys.Count - midIndex);
        node.Children.RemoveRange(midIndex + 1, node.Children.Count - midIndex);

        foreach (var child in newNode.Children)
        {
            child.Parent = newNode;
        }

        InsertIntoParent(node, upKey, newNode);
    }

    public TValue Search(TKey key)
    {
        Node node = root;
        while (!node.IsLeaf)
        {
            int childIndex = 0;
            while (childIndex < node.Keys.Count && key.CompareTo(node.Keys[childIndex]) > 0)
            {
                childIndex++;
            }
            node = node.Children[childIndex];
        }

        int position = 0;
        while (position < node.Keys.Count && key.CompareTo(node.Keys[position]) != 0)
        {
            position++;
        }
        if (position < node.Keys.Count) return node.Values[position];
        return default(TValue);
    }

    public IEnumerable<TValue> RangeQuery(TKey low, TKey high)
    {
        Node node = root;
        while (!node.IsLeaf)
        {
            int childIndex = 0;
            while (childIndex < node.Keys.Count && low.CompareTo(node.Keys[childIndex]) > 0)
            {
                childIndex++;
            }
            node = node.Children[childIndex];
        }

        while (node != null)
        {
            for (int i = 0; i < node.Keys.Count && node.Keys[i].CompareTo(high) <= 0; i++)
            {
                if (node.Keys[i].CompareTo(low) >= 0)
                {
                    yield return node.Values[i];
                }
            }
            node = node.Next;
        }
    }

    public void Delete(TKey key)
    {
        Node node = root;
        while (!node.IsLeaf)
        {
            int childIndex = 0;
            while (childIndex < node.Keys.Count && key.CompareTo(node.Keys[childIndex]) > 0)
            {
                childIndex++;
            }
            node = node.Children[childIndex];
        }

        int position = 0;
        while (position < node.Keys.Count && key.CompareTo(node.Keys[position]) != 0)
        {
            position++;
        }

        if (position < node.Keys.Count) // Check if key was actually found
        {
            node.Keys.RemoveAt(position);
            node.Values.RemoveAt(position);
            if (node != root && node.Keys.Count < degree / 2)
            {
                HandleUnderflow(node);
            }
        }
    }

    private void HandleUnderflow(Node node)
    {
        Node sibling;
        int siblingIndex;
        int nodeIndex = node.Parent.Children.IndexOf(node);

        // Try to borrow from the left sibling
        if (nodeIndex > 0)
        {
            siblingIndex = nodeIndex - 1;
            sibling = node.Parent.Children[siblingIndex];
            if (sibling.Keys.Count > degree / 2)
            {
                // Borrow the last key from the left sibling
                TKey borrowedKey = sibling.Keys.Last();
                TValue borrowedValue = sibling.IsLeaf ? sibling.Values.Last() : default(TValue);
                sibling.Keys.RemoveAt(sibling.Keys.Count - 1);
                if (sibling.IsLeaf)
                {
                    sibling.Values.RemoveAt(sibling.Values.Count - 1);
                }
                node.Keys.Insert(0, borrowedKey);
                if (node.IsLeaf)
                {
                    node.Values.Insert(0, borrowedValue);
                }
                node.Parent.Keys[siblingIndex] = node.Keys.First();
                return;
            }
        }

        // Try to borrow from the right sibling
        if (nodeIndex < node.Parent.Children.Count - 1)
        {
            siblingIndex = nodeIndex + 1;
            sibling = node.Parent.Children[siblingIndex];
            if (sibling.Keys.Count > degree / 2)
            {
                // Borrow the first key from the right sibling
                TKey borrowedKey = sibling.Keys.First();
                TValue borrowedValue = sibling.IsLeaf ? sibling.Values.First() : default(TValue);
                sibling.Keys.RemoveAt(0);
                if (sibling.IsLeaf)
                {
                    sibling.Values.RemoveAt(0);
                }
                node.Keys.Add(borrowedKey);
                if (node.IsLeaf)
                {
                    node.Values.Add(borrowedValue);
                }
                node.Parent.Keys[nodeIndex] = sibling.Keys.First();
                return;
            }
        }

        // Merge with a sibling if borrowing is not possible
        if (nodeIndex > 0)
        {
            // Merge with the left sibling
            siblingIndex = nodeIndex - 1;
            sibling = node.Parent.Children[siblingIndex];
            sibling.Keys.AddRange(node.Keys);
            if (node.IsLeaf)
            {
                sibling.Values.AddRange(node.Values);
                sibling.Next = node.Next;
                if (node.Next != null)
                {
                    node.Next.Previous = sibling;
                }
            }
            node.Parent.Keys.RemoveAt(siblingIndex);
            node.Parent.Children.RemoveAt(nodeIndex);
        }
        else
        {
            // Merge with the right sibling
            siblingIndex = nodeIndex + 1;
            sibling = node.Parent.Children[siblingIndex];
            node.Keys.AddRange(sibling.Keys);
            if (node.IsLeaf)
            {
                node.Values.AddRange(sibling.Values);
                node.Next = sibling.Next;
                if (sibling.Next != null)
                {
                    sibling.Next.Previous = node;
                }
            }
            node.Parent.Keys.RemoveAt(nodeIndex);
            node.Parent.Children.RemoveAt(siblingIndex);
        }

        if (node.Parent.Keys.Count < degree / 2 && node.Parent != root)
        {
            HandleUnderflow(node.Parent);
        }
        else if (node.Parent == root && root.Keys.Count == 0)
        {
            if (root.Children.Count > 0)
            {
                root = root.Children[0];
                root.Parent = null;
            }
        }

    }

    public void Traverse()
    {
        Node current = root;
        while (!current.IsLeaf)
            current = current.Children[0];

        while (current != null)
        {
            foreach (var key in current.Keys)
                Console.WriteLine($"Key: {key}");
            current = current.Next;
        }
    }
}