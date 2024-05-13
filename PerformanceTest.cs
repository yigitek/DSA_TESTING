using System;
using System.Diagnostics;
using System.IO;

namespace DSA_TESTING
{
    public class PerformanceTest
    {
        private LinearProbingHashTable<string, string> hashTable;
        private BTree<string, string> bTree;
        private BPlusTree<string, string> bPlusTree;
        private List<string> keys, values;
        private string filePath;

        public PerformanceTest(string filePath)
        {
            this.keys = new List<string>();
            this.values = new List<string>();
            this.filePath = filePath;
            this.hashTable = new LinearProbingHashTable<string, string>(1000);
            this.bTree = new BTree<string, string>(1000);
            this.bPlusTree = new BPlusTree<string, string>(1000);

            // Load the data from file
            LoadData();
        }

        private void LoadData()
        {
            Console.WriteLine("Loading File Data...");
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2)
                {
                    keys.Add(parts[0]);
                    values.Add(parts[1]);
                }
            }
        }

        public void RunHashTableTests()
        {
            Console.WriteLine("\nRunning Hash Table Tests:");
            InsertData(hashTable);
            SearchData(hashTable);
            DeleteData(hashTable);
         }

        public void RunBTreeTests()
        {
            Console.WriteLine("\nRunning B-Tree Tests:");
            InsertData(bTree);
            SearchData(bTree);
            DeleteData(bTree);
        }

        public void RunBPlusTreeTests() // Add method for BPlusTree tests
        {
            Console.WriteLine("\nRunning B+ Tree Tests:");
            InsertData(bPlusTree);
            SearchData(bPlusTree);
            DeleteData(bPlusTree);
        }

        private void InsertData<T>(T dataStructure) where T : IInsertable<string, string>
        {
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Starting insertion test...");
            stopwatch.Start();

            for (int i = 0; i < keys.Count; i++)
            {
                dataStructure.Insert(keys[i].Trim(), values[i].Trim());
            }

            stopwatch.Stop();
            Console.WriteLine($"Insertion completed in {stopwatch.ElapsedMilliseconds} ms");
        }

        private void SearchData<T>(T dataStructure) where T : ISearchable<string, string>
        {
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Starting search test...");
            stopwatch.Start();

            for (int i = 0; i < keys.Count; i++)
            {
                try
                {
                    dataStructure.Search(keys[i].Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error searching for key {keys[0]}: {ex.Message}");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Search completed in {stopwatch.ElapsedMilliseconds} ms");
        }

        private void DeleteData<T>(T dataStructure) where T : IDeletable<string>
        {
            Stopwatch stopwatch = new Stopwatch();

            Console.WriteLine("Starting deletion test...");
            stopwatch.Start();

            for (int i = 0; i < keys.Count; i++)
            {
                try
                {
                    dataStructure.Delete(keys[i].Trim());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting key {keys[0]}: {ex.Message}");
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Deletion completed in {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    public interface IInsertable<TKey, TValue>
    {
        void Insert(TKey key, TValue value);
    }

    public interface ISearchable<TKey, TValue>
    {
        TValue Search(TKey key);
    }

    public interface IDeletable<TKey>
    {
        void Delete(TKey key);
    }
}