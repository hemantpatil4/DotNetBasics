/*
╔══════════════════════════════════════════════════════════════════════════════╗
║                      LIST<T> - COMPREHENSIVE DEMO                             ║
║                                                                               ║
║  All operations on List<T> with FX Trading examples                           ║
║  Run with: dotnet run                                                         ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

using System;
using System.Collections.Generic;
using System.Linq;

namespace ListDemo
{
    class Program
    {
        // Renamed to avoid multiple entry points - call ListDemo.Program.Run() if needed
        public static void Run(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              LIST<T> - COMPREHENSIVE DEMO                    ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");
            
            Demo1_Creation();
            Demo2_Adding();
            Demo3_Removing();
            Demo4_Accessing();
            Demo5_Searching();
            Demo6_Sorting();
            Demo7_Transforming();
            Demo8_CapacityManagement();
            Demo9_CommonPatterns();
            Demo10_FXTradingExample();
            
            Console.WriteLine("\n✓ All demos completed!");
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 1: Creation
        // ════════════════════════════════════════════════════════════════════
        static void Demo1_Creation()
        {
            PrintHeader("DEMO 1: List Creation");
            
            // Method 1: Empty list
            List<int> list1 = new List<int>();
            Console.WriteLine($"  Empty list - Count: {list1.Count}, Capacity: {list1.Capacity}");
            
            // Method 2: With initial capacity (recommended when size known)
            List<int> list2 = new List<int>(100);
            Console.WriteLine($"  With capacity 100 - Count: {list2.Count}, Capacity: {list2.Capacity}");
            
            // Method 3: Collection initializer
            List<int> list3 = new List<int> { 1, 2, 3, 4, 5 };
            Console.WriteLine($"  Initializer {{1,2,3,4,5}} - Count: {list3.Count}, Capacity: {list3.Capacity}");
            
            // Method 4: From array
            int[] array = { 10, 20, 30 };
            List<int> list4 = new List<int>(array);
            Console.WriteLine($"  From array - Count: {list4.Count}");
            
            // Method 5: LINQ ToList()
            List<int> list5 = Enumerable.Range(1, 5).ToList();
            Console.WriteLine($"  From Range(1,5) - [{string.Join(", ", list5)}]");
            
            // Method 6: Collection expression (C# 12+)
            List<string> list6 = ["EUR/USD", "GBP/USD", "USD/JPY"];
            Console.WriteLine($"  Collection expression - [{string.Join(", ", list6)}]");
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 2: Adding Elements
        // ════════════════════════════════════════════════════════════════════
        static void Demo2_Adding()
        {
            PrintHeader("DEMO 2: Adding Elements");
            
            List<string> trades = new List<string>();
            
            // Add single element - O(1) amortized
            Console.WriteLine("  Add(\"TRD001\"):");
            trades.Add("TRD001");
            PrintList("    ", trades);
            
            // Insert at specific index - O(n)
            Console.WriteLine("  Insert(0, \"TRD000\") at beginning:");
            trades.Insert(0, "TRD000");
            PrintList("    ", trades);
            
            // AddRange - O(n)
            Console.WriteLine("  AddRange([\"TRD002\", \"TRD003\"]):");
            trades.AddRange(new[] { "TRD002", "TRD003" });
            PrintList("    ", trades);
            
            // InsertRange
            Console.WriteLine("  InsertRange(2, [\"TRD001A\", \"TRD001B\"]):");
            trades.InsertRange(2, new[] { "TRD001A", "TRD001B" });
            PrintList("    ", trades);
            
            // Show capacity growth
            Console.WriteLine("\n  Capacity Growth Demo:");
            List<int> nums = new List<int>();
            for (int i = 1; i <= 10; i++)
            {
                int oldCap = nums.Capacity;
                nums.Add(i);
                if (nums.Capacity != oldCap)
                {
                    Console.WriteLine($"    After Add({i}): Count={nums.Count}, Capacity changed {oldCap} → {nums.Capacity}");
                }
            }
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 3: Removing Elements
        // ════════════════════════════════════════════════════════════════════
        static void Demo3_Removing()
        {
            PrintHeader("DEMO 3: Removing Elements");
            
            List<int> numbers = new List<int> { 1, 2, 3, 2, 4, 5, 2, 6 };
            Console.WriteLine($"  Original: [{string.Join(", ", numbers)}]");
            
            // Remove first occurrence
            Console.WriteLine("\n  Remove(2) - removes first occurrence:");
            bool removed = numbers.Remove(2);
            Console.WriteLine($"    Removed: {removed}, Result: [{string.Join(", ", numbers)}]");
            
            // Remove at index
            Console.WriteLine("\n  RemoveAt(0) - removes at index 0:");
            numbers.RemoveAt(0);
            Console.WriteLine($"    Result: [{string.Join(", ", numbers)}]");
            
            // Remove range
            List<int> nums2 = new List<int> { 10, 20, 30, 40, 50 };
            Console.WriteLine($"\n  List: [{string.Join(", ", nums2)}]");
            Console.WriteLine("  RemoveRange(1, 2) - remove 2 items starting at index 1:");
            nums2.RemoveRange(1, 2);
            Console.WriteLine($"    Result: [{string.Join(", ", nums2)}]");
            
            // RemoveAll with predicate
            List<int> nums3 = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Console.WriteLine($"\n  List: [{string.Join(", ", nums3)}]");
            Console.WriteLine("  RemoveAll(x => x % 2 == 0) - remove all even numbers:");
            int count = nums3.RemoveAll(x => x % 2 == 0);
            Console.WriteLine($"    Removed {count} items, Result: [{string.Join(", ", nums3)}]");
            
            // Clear
            nums3.Clear();
            Console.WriteLine($"\n  After Clear(): Count = {nums3.Count}");
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 4: Accessing Elements
        // ════════════════════════════════════════════════════════════════════
        static void Demo4_Accessing()
        {
            PrintHeader("DEMO 4: Accessing Elements");
            
            List<string> pairs = new List<string> { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD", "USD/CHF" };
            Console.WriteLine($"  List: [{string.Join(", ", pairs)}]");
            
            // By index - O(1)
            Console.WriteLine($"\n  By index [0]: {pairs[0]}");
            Console.WriteLine($"  By index [2]: {pairs[2]}");
            Console.WriteLine($"  Last (Count-1): {pairs[pairs.Count - 1]}");
            
            // LINQ methods
            Console.WriteLine($"\n  First(): {pairs.First()}");
            Console.WriteLine($"  Last(): {pairs.Last()}");
            Console.WriteLine($"  ElementAt(2): {pairs.ElementAt(2)}");
            
            // Safe access
            Console.WriteLine($"\n  ElementAtOrDefault(10): '{pairs.ElementAtOrDefault(10) ?? "null"}'");
            Console.WriteLine($"  FirstOrDefault(x => x.StartsWith(\"X\")): '{pairs.FirstOrDefault(x => x.StartsWith("X")) ?? "null"}'");
            
            // Slicing with GetRange
            Console.WriteLine($"\n  GetRange(1, 3): [{string.Join(", ", pairs.GetRange(1, 3))}]");
            
            // Using Span/Range (C# 8+)
            Console.WriteLine($"\n  Using Range syntax (LINQ):");
            Console.WriteLine($"    Skip(1).Take(3): [{string.Join(", ", pairs.Skip(1).Take(3))}]");
            Console.WriteLine($"    TakeLast(2): [{string.Join(", ", pairs.TakeLast(2))}]");
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 5: Searching
        // ════════════════════════════════════════════════════════════════════
        static void Demo5_Searching()
        {
            PrintHeader("DEMO 5: Searching");
            
            List<int> numbers = new List<int> { 10, 20, 30, 40, 30, 50, 60 };
            Console.WriteLine($"  List: [{string.Join(", ", numbers)}]");
            
            // Contains - O(n)
            Console.WriteLine($"\n  Contains(30): {numbers.Contains(30)}");
            Console.WriteLine($"  Contains(100): {numbers.Contains(100)}");
            
            // IndexOf / LastIndexOf - O(n)
            Console.WriteLine($"\n  IndexOf(30): {numbers.IndexOf(30)}  (first occurrence)");
            Console.WriteLine($"  LastIndexOf(30): {numbers.LastIndexOf(30)}  (last occurrence)");
            Console.WriteLine($"  IndexOf(100): {numbers.IndexOf(100)}  (-1 = not found)");
            
            // Find methods - O(n)
            Console.WriteLine($"\n  Find(x => x > 25): {numbers.Find(x => x > 25)}  (first match)");
            Console.WriteLine($"  FindLast(x => x > 25): {numbers.FindLast(x => x > 25)}  (last match)");
            Console.WriteLine($"  FindIndex(x => x > 25): {numbers.FindIndex(x => x > 25)}");
            Console.WriteLine($"  FindAll(x => x > 25): [{string.Join(", ", numbers.FindAll(x => x > 25))}]");
            
            // Exists / TrueForAll
            Console.WriteLine($"\n  Exists(x => x > 50): {numbers.Exists(x => x > 50)}");
            Console.WriteLine($"  TrueForAll(x => x > 0): {numbers.TrueForAll(x => x > 0)}");
            
            // Binary Search (MUST BE SORTED!)
            List<int> sorted = new List<int> { 10, 20, 30, 40, 50 };
            Console.WriteLine($"\n  Sorted List: [{string.Join(", ", sorted)}]");
            Console.WriteLine($"  BinarySearch(30): {sorted.BinarySearch(30)}  (O(log n))");
            Console.WriteLine($"  BinarySearch(35): {sorted.BinarySearch(35)}  (negative = not found)");
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 6: Sorting
        // ════════════════════════════════════════════════════════════════════
        static void Demo6_Sorting()
        {
            PrintHeader("DEMO 6: Sorting");
            
            // Basic sort
            List<int> numbers = new List<int> { 5, 2, 8, 1, 9, 3 };
            Console.WriteLine($"  Original: [{string.Join(", ", numbers)}]");
            
            numbers.Sort();  // In-place, O(n log n)
            Console.WriteLine($"  After Sort(): [{string.Join(", ", numbers)}]");
            
            // Descending
            numbers.Sort((a, b) => b.CompareTo(a));
            Console.WriteLine($"  Descending: [{string.Join(", ", numbers)}]");
            
            // Reverse
            numbers.Reverse();
            Console.WriteLine($"  After Reverse(): [{string.Join(", ", numbers)}]");
            
            // Custom object sorting
            Console.WriteLine("\n  Custom Object Sorting (Trades):");
            List<Trade> trades = new List<Trade>
            {
                new Trade("TRD001", "EUR/USD", 100000, 1.0850m),
                new Trade("TRD002", "GBP/USD", 50000, 1.2650m),
                new Trade("TRD003", "EUR/USD", 200000, 1.0855m),
                new Trade("TRD004", "USD/JPY", 75000, 149.50m)
            };
            
            Console.WriteLine("    Original:");
            trades.ForEach(t => Console.WriteLine($"      {t}"));
            
            // Sort by Amount
            trades.Sort((t1, t2) => t1.Amount.CompareTo(t2.Amount));
            Console.WriteLine("\n    Sorted by Amount (ascending):");
            trades.ForEach(t => Console.WriteLine($"      {t}"));
            
            // LINQ OrderBy (doesn't modify original)
            var sortedByPair = trades.OrderBy(t => t.CurrencyPair)
                                    .ThenByDescending(t => t.Amount)
                                    .ToList();
            Console.WriteLine("\n    LINQ OrderBy Pair, ThenByDescending Amount:");
            sortedByPair.ForEach(t => Console.WriteLine($"      {t}"));
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 7: Transforming
        // ════════════════════════════════════════════════════════════════════
        static void Demo7_Transforming()
        {
            PrintHeader("DEMO 7: Transforming");
            
            List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
            Console.WriteLine($"  Original: [{string.Join(", ", numbers)}]");
            
            // ConvertAll
            List<string> strings = numbers.ConvertAll(x => $"#{x}");
            Console.WriteLine($"  ConvertAll(x => $\"#{{x}}\"): [{string.Join(", ", strings)}]");
            
            // LINQ Select
            List<int> doubled = numbers.Select(x => x * 2).ToList();
            Console.WriteLine($"  Select(x => x * 2): [{string.Join(", ", doubled)}]");
            
            // LINQ Where (filter)
            List<int> evens = numbers.Where(x => x % 2 == 0).ToList();
            Console.WriteLine($"  Where(x => x % 2 == 0): [{string.Join(", ", evens)}]");
            
            // Combined operations
            var result = numbers
                .Where(x => x > 2)
                .Select(x => x * 10)
                .ToList();
            Console.WriteLine($"  Where(>2).Select(*10): [{string.Join(", ", result)}]");
            
            // Aggregate operations
            Console.WriteLine($"\n  Sum(): {numbers.Sum()}");
            Console.WriteLine($"  Average(): {numbers.Average()}");
            Console.WriteLine($"  Max(): {numbers.Max()}");
            Console.WriteLine($"  Min(): {numbers.Min()}");
            
            // ToArray
            int[] array = numbers.ToArray();
            Console.WriteLine($"  ToArray(): [{string.Join(", ", array)}]");
            
            // Distinct
            List<int> withDups = new List<int> { 1, 2, 2, 3, 3, 3, 4 };
            Console.WriteLine($"\n  [{string.Join(", ", withDups)}]");
            Console.WriteLine($"  Distinct(): [{string.Join(", ", withDups.Distinct())}]");
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 8: Capacity Management
        // ════════════════════════════════════════════════════════════════════
        static void Demo8_CapacityManagement()
        {
            PrintHeader("DEMO 8: Capacity Management");
            
            // Show capacity growth
            Console.WriteLine("  Capacity Growth Pattern:");
            List<int> list = new List<int>();
            int lastCapacity = 0;
            
            for (int i = 0; i < 20; i++)
            {
                list.Add(i);
                if (list.Capacity != lastCapacity)
                {
                    Console.WriteLine($"    Count={list.Count,2}: Capacity changed to {list.Capacity}");
                    lastCapacity = list.Capacity;
                }
            }
            
            // Pre-allocating capacity
            Console.WriteLine("\n  Pre-allocating capacity:");
            List<int> optimized = new List<int>(1000);
            Console.WriteLine($"    new List<int>(1000): Capacity = {optimized.Capacity}, Count = {optimized.Count}");
            
            // TrimExcess
            list.TrimExcess();
            Console.WriteLine($"\n  After TrimExcess(): Capacity = {list.Capacity}, Count = {list.Count}");
            
            // EnsureCapacity
            list.EnsureCapacity(100);
            Console.WriteLine($"  After EnsureCapacity(100): Capacity = {list.Capacity}");
            
            // Performance comparison
            Console.WriteLine("\n  Performance Tip:");
            Console.WriteLine("    ❌ Adding 10000 items without capacity: multiple resizes");
            Console.WriteLine("    ✓ new List<int>(10000): single allocation");
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 9: Common Patterns
        // ════════════════════════════════════════════════════════════════════
        static void Demo9_CommonPatterns()
        {
            PrintHeader("DEMO 9: Common Patterns");
            
            // Pattern 1: Removing while iterating
            Console.WriteLine("  Pattern 1: Removing while iterating");
            List<int> nums = new List<int> { 1, 2, 3, 4, 5, 6 };
            Console.WriteLine($"    Original: [{string.Join(", ", nums)}]");
            Console.WriteLine("    Remove all evens:");
            
            // ❌ WRONG WAY (would throw)
            // foreach (var n in nums) if (n % 2 == 0) nums.Remove(n);
            
            // ✓ Option 1: Iterate backwards
            for (int i = nums.Count - 1; i >= 0; i--)
            {
                if (nums[i] % 2 == 0) nums.RemoveAt(i);
            }
            Console.WriteLine($"    After backward loop: [{string.Join(", ", nums)}]");
            
            // ✓ Option 2: RemoveAll
            nums = new List<int> { 1, 2, 3, 4, 5, 6 };
            nums.RemoveAll(n => n % 2 == 0);
            Console.WriteLine($"    Using RemoveAll: [{string.Join(", ", nums)}]");
            
            // Pattern 2: Safe dictionary-like access
            Console.WriteLine("\n  Pattern 2: Safe index access");
            List<string> items = new List<string> { "A", "B", "C" };
            int index = 10;
            string value = index < items.Count ? items[index] : "default";
            Console.WriteLine($"    Safe access at index {index}: '{value}'");
            
            // Pattern 3: Chunking
            Console.WriteLine("\n  Pattern 3: Chunking (batching)");
            List<int> allItems = Enumerable.Range(1, 10).ToList();
            Console.WriteLine($"    Original: [{string.Join(", ", allItems)}]");
            
            int chunkSize = 3;
            var chunks = allItems.Chunk(chunkSize);  // .NET 6+
            int chunkNum = 1;
            foreach (var chunk in chunks)
            {
                Console.WriteLine($"    Chunk {chunkNum++}: [{string.Join(", ", chunk)}]");
            }
            
            // Pattern 4: Deduplication while preserving order
            Console.WriteLine("\n  Pattern 4: Deduplicate preserving order");
            List<int> withDups = new List<int> { 1, 3, 2, 3, 1, 4, 2, 5 };
            Console.WriteLine($"    Original: [{string.Join(", ", withDups)}]");
            
            var seen = new HashSet<int>();
            var deduplicated = withDups.Where(x => seen.Add(x)).ToList();
            Console.WriteLine($"    Deduplicated: [{string.Join(", ", deduplicated)}]");
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // DEMO 10: FX Trading Example
        // ════════════════════════════════════════════════════════════════════
        static void Demo10_FXTradingExample()
        {
            PrintHeader("DEMO 10: FX Trading Example");
            
            // Simulating an order book
            List<Order> orderBook = new List<Order>
            {
                new Order("ORD001", "BUY", "EUR/USD", 100000, 1.0850m),
                new Order("ORD002", "SELL", "EUR/USD", 50000, 1.0855m),
                new Order("ORD003", "BUY", "EUR/USD", 75000, 1.0848m),
                new Order("ORD004", "SELL", "EUR/USD", 200000, 1.0860m),
                new Order("ORD005", "BUY", "GBP/USD", 150000, 1.2650m),
                new Order("ORD006", "SELL", "GBP/USD", 80000, 1.2655m),
            };
            
            Console.WriteLine("  Order Book:");
            orderBook.ForEach(o => Console.WriteLine($"    {o}"));
            
            // Get all EUR/USD orders
            Console.WriteLine("\n  EUR/USD Orders:");
            var eurUsdOrders = orderBook.Where(o => o.CurrencyPair == "EUR/USD").ToList();
            eurUsdOrders.ForEach(o => Console.WriteLine($"    {o}"));
            
            // Get best bid (highest BUY price)
            var bestBid = orderBook
                .Where(o => o.Side == "BUY" && o.CurrencyPair == "EUR/USD")
                .OrderByDescending(o => o.Price)
                .FirstOrDefault();
            Console.WriteLine($"\n  Best Bid (EUR/USD): {bestBid?.Price:F5}");
            
            // Get best ask (lowest SELL price)
            var bestAsk = orderBook
                .Where(o => o.Side == "SELL" && o.CurrencyPair == "EUR/USD")
                .OrderBy(o => o.Price)
                .FirstOrDefault();
            Console.WriteLine($"  Best Ask (EUR/USD): {bestAsk?.Price:F5}");
            
            // Total volume by side
            Console.WriteLine("\n  Total Volume by Side:");
            var volumeBySide = orderBook
                .GroupBy(o => o.Side)
                .Select(g => new { Side = g.Key, TotalVolume = g.Sum(o => o.Amount) });
            foreach (var v in volumeBySide)
            {
                Console.WriteLine($"    {v.Side}: {v.TotalVolume:N0}");
            }
            
            // Match orders (simplified)
            Console.WriteLine("\n  Order Matching (EUR/USD):");
            var buyOrders = orderBook
                .Where(o => o.Side == "BUY" && o.CurrencyPair == "EUR/USD")
                .OrderByDescending(o => o.Price)
                .ToList();
            var sellOrders = orderBook
                .Where(o => o.Side == "SELL" && o.CurrencyPair == "EUR/USD")
                .OrderBy(o => o.Price)
                .ToList();
            
            if (buyOrders.Any() && sellOrders.Any())
            {
                var topBuy = buyOrders.First();
                var topSell = sellOrders.First();
                
                if (topBuy.Price >= topSell.Price)
                {
                    Console.WriteLine($"    ✓ Match possible: BUY@{topBuy.Price:F5} >= SELL@{topSell.Price:F5}");
                }
                else
                {
                    Console.WriteLine($"    ✗ No match: BUY@{topBuy.Price:F5} < SELL@{topSell.Price:F5}");
                }
            }
            
            PrintFooter();
        }
        
        // ════════════════════════════════════════════════════════════════════
        // Helper Classes
        // ════════════════════════════════════════════════════════════════════
        
        record Trade(string TradeId, string CurrencyPair, decimal Amount, decimal Rate)
        {
            public override string ToString() => 
                $"{TradeId}: {CurrencyPair} {Amount:N0} @ {Rate:F4}";
        }
        
        record Order(string OrderId, string Side, string CurrencyPair, decimal Amount, decimal Price)
        {
            public override string ToString() => 
                $"{OrderId}: {Side} {CurrencyPair} {Amount:N0} @ {Price:F5}";
        }
        
        // ════════════════════════════════════════════════════════════════════
        // Helper Methods
        // ════════════════════════════════════════════════════════════════════
        
        static void PrintHeader(string title)
        {
            Console.WriteLine($"{'═',60}");
            Console.WriteLine($"  {title}");
            Console.WriteLine($"{'═',60}");
        }
        
        static void PrintFooter()
        {
            Console.WriteLine();
        }
        
        static void PrintList<T>(string prefix, List<T> list)
        {
            Console.WriteLine($"{prefix}[{string.Join(", ", list)}]");
        }
    }
}
