namespace DSA_TESTING;

class Program
{
    static void Main(string[] args)
    {
        string filePath = Directory.GetCurrentDirectory() + "\\large_data_file.csv";
        
        // Initialize the PerformanceTest object
        PerformanceTest performanceTest = new PerformanceTest(filePath);

        // Run the tests for each data structure
        performanceTest.RunHashTableTests();
        performanceTest.RunBTreeTests();
        performanceTest.RunBPlusTreeTests();
    }
}