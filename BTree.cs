namespace DSA_TESTING;

using System;
using System.Collections.Generic;

public class BTree<TKey, TValue> : IInsertable<TKey, TValue>, ISearchable<TKey, TValue>, IDeletable<TKey>
    where TKey : IComparable<TKey>
{
    private class Node
    {
        public int Degree;
        public List<TKey> Keys = new List<TKey>();
        public List<TValue> Values = new List<TValue>();
        public List<Node> Children = new List<Node>();
        public bool Leaf = true;

        public Node(int degree)
        {
            Degree = degree;
        }
    }

    private Node root;
    private int degree;

    public BTree(int degree)
    {
        this.degree = degree;
        root = new Node(degree);
    }

    public void Insert(TKey key, TValue value)
    {
        Node rootOld = root;
        if (rootOld.Keys.Count == 2 * degree - 1)
        {
            Node newNode = new Node(degree);
            root = newNode;
            newNode.Children.Add(rootOld);
            newNode.Leaf = false;
            SplitChild(newNode, 0);
            InsertNonFull(newNode, key, value);
        }
        else
        {
            InsertNonFull(rootOld, key, value);
        }
    }

    private void SplitChild(Node parent, int index)
    {
        Node oldNode = parent.Children[index];
        Node newNode = new Node(degree);
        newNode.Leaf = oldNode.Leaf;

        int midIndex = degree - 1;
        newNode.Keys.AddRange(oldNode.Keys.GetRange(midIndex + 1, midIndex));
        newNode.Values.AddRange(oldNode.Values.GetRange(midIndex + 1, midIndex));

        if (!newNode.Leaf)
        {
            newNode.Children.AddRange(oldNode.Children.GetRange(midIndex + 1, degree));
        }

        parent.Keys.Insert(index, oldNode.Keys[midIndex]);
        parent.Values.Insert(index, oldNode.Values[midIndex]);
        parent.Children.Insert(index + 1, newNode);

        oldNode.Keys.RemoveRange(midIndex, degree);
        oldNode.Values.RemoveRange(midIndex, degree);
        if (!oldNode.Leaf)
        {
            oldNode.Children.RemoveRange(midIndex + 1, degree);
        }
    }

    private void InsertNonFull(Node node, TKey key, TValue value)
    {
        int i = node.Keys.Count - 1;
        if (node.Leaf)
        {
            while (i >= 0 && key.CompareTo(node.Keys[i]) < 0)
            {
                i--;
            }

            node.Keys.Insert(i + 1, key);
            node.Values.Insert(i + 1, value);
        }
        else
        {
            while (i >= 0 && key.CompareTo(node.Keys[i]) < 0)
            {
                i--;
            }

            i++;
            if (node.Children[i].Keys.Count == 2 * degree - 1)
            {
                SplitChild(node, i);
                if (key.CompareTo(node.Keys[i]) > 0)
                {
                    i++;
                }
            }

            InsertNonFull(node.Children[i], key, value);
        }
    }

    public TValue Search(TKey key)
    {
        return SearchInternal(root, key);
    }

    private TValue SearchInternal(Node node, TKey key)
    {
        int i = 0;
        while (i < node.Keys.Count && key.CompareTo(node.Keys[i]) > 0)
        {
            i++;
        }

        if (i < node.Keys.Count && key.CompareTo(node.Keys[i]) == 0)
        {
            return node.Values[i];
        }

        if (node.Leaf)
        {
            throw new Exception("Key not found.");
        }
        else
        {
            return SearchInternal(node.Children[i], key);
        }
    }

    public void Delete(TKey key)
    {
        // Start the deletion process from the root node.
        DeleteInternal(root, key);

        // After deletion, check if the root has no keys and is not a leaf.
        // If so, the new root should be the first child of the current root,
        // effectively lowering the height of the tree.
        if (root.Keys.Count == 0 && !root.Leaf)
        {
            root =  root.Children[0];
        }
    }

    private void DeleteInternal(Node node, TKey key)
    {
        // Find the index of the key in the node.
        int idx = FindKey(node, key);

        // If the key is found in the node at index idx.
        if (idx < node.Keys.Count && key.CompareTo(node.Keys[idx]) == 0)
        {
            // If the node is a leaf, simply remove the key and its associated value.
            if (node.Leaf)
            {
                node.Keys.RemoveAt(idx);
                node.Values.RemoveAt(idx);
            }
            else
            {
                // If the node is an internal node, perform deletion from the internal node.
                DeleteFromInternalNode(node, idx);
            }
        }
        else
        {
            // The key is not found and this is an internal node.
            if (node.Leaf)
            {
                // If it's a leaf, then the key isn't present in the tree.
                throw new Exception("Key not found.");
            }

            // If the child where the key might exist has fewer keys than the minimum degree,
            // attempt to refill that child before proceeding.
            bool flag = (idx == node.Keys.Count);
            if (node.Children[idx].Keys.Count < degree)
            {
                Fill(node, idx);
            }

            // After fill, if the last key was checked and the number of keys has decreased,
            // move to the child to the left.
            if (flag && idx > node.Keys.Count)
            {
                DeleteInternal(node.Children[idx - 1], key);
            }
            else
            {
                // Otherwise, continue with the appropriate child.
                DeleteInternal(node.Children[idx], key);
            }
        }
    }

    private void DeleteFromInternalNode(Node node, int idx)
    {
        TKey key = node.Keys[idx];

        // If the left child has at least the minimum degree of keys,
        // find the predecessor key to replace the deleted key.
        if (node.Children[idx].Keys.Count >= degree)
        {
            Node pred = GetPredecessor(node, idx);
            node.Keys[idx] = pred.Keys.Last();
            node.Values[idx] = pred.Values.Last();
            DeleteInternal(pred, pred.Keys.Last());
        }
        // If the right child has at least the minimum degree of keys,
        // find the successor key to replace the deleted key.
        else if (node.Children[idx + 1].Keys.Count >= degree)
        {
            Node succ = GetSuccessor(node, idx);
            node.Keys[idx] = succ.Keys.First();
            node.Values[idx] = succ.Values.First();
            DeleteInternal(succ, succ.Keys.First());
        }
        else
        {
            // If neither child has the minimum degree, merge the children.
            Merge(node, idx);
            DeleteInternal(node.Children[idx], key);
        }
    }

    private Node GetPredecessor(Node node, int idx)
    {
        Node cur = node.Children[idx];
        while (!cur.Leaf)
        {
            cur = cur.Children[cur.Keys.Count];
        }

        return cur;
    }

    private Node GetSuccessor(Node node, int idx)
    {
        Node cur = node.Children[idx + 1];
        while (!cur.Leaf)
        {
            cur = cur.Children[0];
        }

        return cur;
    }

    private void Fill(Node node, int idx)
    {
        if (idx != 0 && node.Children[idx - 1].Keys.Count >= degree)
        {
            BorrowFromPrev(node, idx);
        }
        else if (idx != node.Keys.Count && node.Children[idx + 1].Keys.Count >= degree)
        {
            BorrowFromNext(node, idx);
        }
        else
        {
            if (idx != node.Keys.Count)
            {
                Merge(node, idx);
            }
            else
            {
                Merge(node, idx - 1);
            }
        }
    }

    private void BorrowFromPrev(Node node, int idx)
    {
        Node child = node.Children[idx];
        Node sibling = node.Children[idx - 1];

        child.Keys.Insert(0, node.Keys[idx - 1]);
        child.Values.Insert(0, node.Values[idx - 1]);
        if (!child.Leaf)
        {
            child.Children.Insert(0, sibling.Children.Last());
        }

        node.Keys[idx - 1] = sibling.Keys.Last();
        node.Values[idx - 1] = sibling.Values.Last();

        sibling.Keys.RemoveAt(sibling.Keys.Count - 1);
        sibling.Values.RemoveAt(sibling.Values.Count - 1);
        if (!sibling.Leaf)
        {
            sibling.Children.RemoveAt(sibling.Children.Count - 1);
        }
    }

    private void BorrowFromNext(Node node, int idx)
    {
        Node child = node.Children[idx];
        Node sibling = node.Children[idx + 1];

        child.Keys.Add(node.Keys[idx]);
        child.Values.Add(node.Values[idx]);
        if (!child.Leaf)
        {
            child.Children.Add(sibling.Children.First());
        }

        node.Keys[idx] = sibling.Keys.First();
        node.Values[idx] = sibling.Values.First();

        sibling.Keys.RemoveAt(0);
        sibling.Values.RemoveAt(0);
        if (!sibling.Leaf)
        {
            sibling.Children.RemoveAt(0);
        }
    }

    private void Merge(Node node, int idx)
    {
        Node child = node.Children[idx];
        Node sibling = node.Children[idx + 1];

        child.Keys.Add(node.Keys[idx]);
        child.Values.Add(node.Values[idx]);

        for (int i = 0; i < sibling.Keys.Count; i++)
        {
            child.Keys.Add(sibling.Keys[i]);
            child.Values.Add(sibling.Values[i]);
        }

        if (!child.Leaf)
        {
            for (int i = 0; i <= sibling.Keys.Count; i++)
            {
                child.Children.Add(sibling.Children[i]);
            }
        }

        node.Keys.RemoveAt(idx);
        node.Values.RemoveAt(idx);
        node.Children.RemoveAt(idx + 1);
    }

    private int FindKey(Node node, TKey key)
    {
        int idx = 0;
        while (idx < node.Keys.Count && node.Keys[idx].CompareTo(key) < 0)
        {
            idx++;
        }

        return idx;
    }

    public void Traverse()
    {
        if (root != null)
        {
            TraverseNode(root);
        }
    }

    private void TraverseNode(Node node)
    {
        int i;
        for (i = 0; i < node.Keys.Count; i++)
        {
            // If this is not a leaf, go to the child before this key
            if (!node.Leaf)
            {
                TraverseNode(node.Children[i]);
            }

            // Print the key and value
            Console.WriteLine($"Key: {node.Keys[i]}, Value: {node.Values[i]}");
        }

        // Finally, visit the rightmost child
        if (!node.Leaf)
        {
            TraverseNode(node.Children[i]);
        }
    }
}