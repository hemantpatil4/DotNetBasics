/*
╔══════════════════════════════════════════════════════════════════════════════╗
║                    LIST<T> PRACTICE - TEST INPUTS                             ║
║                                                                               ║
║  Ready-to-use test cases for all List practice problems                       ║
║  Run with: dotnet run                                                         ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

using System;
using System.Collections.Generic;
using System.Linq;

public class ListTestInputs
{
    public static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("              LIST<T> PRACTICE - TEST ALL PROBLEMS              ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        TestProblem1_TwoSum();
        TestProblem2_RemoveDuplicates();
        TestProblem3_RotateList();
        TestProblem4_MergeSortedLists();
        TestProblem5_FindMissingNumber();
        TestProblem6_MoveZeroes();
        TestProblem7_MaxProfit();
        TestProblem8_GroupByPair();
        TestProblem9_FindPairsWithSpread();
        TestProblem10_MovingAverage();
        TestProblem11_RateLimiter();

        Console.WriteLine("\n✓ All tests completed!");
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 1: Two Sum
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem1_TwoSum()
    {
        Console.WriteLine("═══ Problem 1: Two Sum ═══");
        
        var testCases = new[]
        {
            (nums: new List<int> { 2, 7, 11, 15 }, target: 9, expected: new[] { 0, 1 }),
            (nums: new List<int> { 3, 2, 4 }, target: 6, expected: new[] { 1, 2 }),
            (nums: new List<int> { 3, 3 }, target: 6, expected: new[] { 0, 1 }),
            (nums: new List<int> { 1, 5, 8, 3, 9, 2 }, target: 11, expected: new[] { 1, 3 }), // 5+8=13? No, 8+3=11
            (nums: new List<int> { -1, -2, -3, -4, -5 }, target: -8, expected: new[] { 2, 4 }), // -3+-5=-8
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    nums = [{string.Join(", ", tc.nums)}], target = {tc.target}");
            Console.WriteLine($"    Expected: [{string.Join(", ", tc.expected)}]");
            
            // Uncomment to test your solution:
            // var result = YourSolution.TwoSum(tc.nums, tc.target);
            // Console.WriteLine($"    Result:   [{string.Join(", ", result)}]");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 2: Remove Duplicates from Sorted Array
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem2_RemoveDuplicates()
    {
        Console.WriteLine("═══ Problem 2: Remove Duplicates (Sorted Array) ═══");
        
        var testCases = new[]
        {
            (nums: new List<int> { 1, 1, 2 }, expectedLength: 2, expectedList: new[] { 1, 2 }),
            (nums: new List<int> { 0, 0, 1, 1, 1, 2, 2, 3, 3, 4 }, expectedLength: 5, expectedList: new[] { 0, 1, 2, 3, 4 }),
            (nums: new List<int> { 1, 1, 1, 1, 1 }, expectedLength: 1, expectedList: new[] { 1 }),
            (nums: new List<int> { 1, 2, 3, 4, 5 }, expectedLength: 5, expectedList: new[] { 1, 2, 3, 4, 5 }),  // No duplicates
            (nums: new List<int> { }, expectedLength: 0, expectedList: Array.Empty<int>()),  // Empty list
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    [{string.Join(", ", tc.nums)}]");
            Console.WriteLine($"    Expected: length = {tc.expectedLength}, list = [{string.Join(", ", tc.expectedList)}]");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 3: Rotate List
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem3_RotateList()
    {
        Console.WriteLine("═══ Problem 3: Rotate List ═══");
        
        var testCases = new[]
        {
            (nums: new List<int> { 1, 2, 3, 4, 5 }, k: 2, expected: new[] { 4, 5, 1, 2, 3 }),
            (nums: new List<int> { -1, -100, 3, 99 }, k: 2, expected: new[] { 3, 99, -1, -100 }),
            (nums: new List<int> { 1, 2, 3 }, k: 4, expected: new[] { 3, 1, 2 }),  // k > length
            (nums: new List<int> { 1, 2, 3, 4, 5, 6, 7 }, k: 3, expected: new[] { 5, 6, 7, 1, 2, 3, 4 }),
            (nums: new List<int> { 1 }, k: 0, expected: new[] { 1 }),  // k = 0
            (nums: new List<int> { 1, 2 }, k: 2, expected: new[] { 1, 2 }),  // k = length (no change)
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    [{string.Join(", ", tc.nums)}], k = {tc.k}");
            Console.WriteLine($"    Expected: [{string.Join(", ", tc.expected)}]");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 4: Merge Sorted Lists
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem4_MergeSortedLists()
    {
        Console.WriteLine("═══ Problem 4: Merge Sorted Lists ═══");
        
        var testCases = new[]
        {
            (list1: new List<int> { 1, 3, 5 }, list2: new List<int> { 2, 4, 6 }, expected: new[] { 1, 2, 3, 4, 5, 6 }),
            (list1: new List<int> { 1, 2, 4 }, list2: new List<int> { 1, 3, 4 }, expected: new[] { 1, 1, 2, 3, 4, 4 }),
            (list1: new List<int> { }, list2: new List<int> { 0 }, expected: new[] { 0 }),
            (list1: new List<int> { }, list2: new List<int> { }, expected: Array.Empty<int>()),
            (list1: new List<int> { 1, 5, 9, 13 }, list2: new List<int> { 2, 3, 4, 10, 11, 12 }, expected: new[] { 1, 2, 3, 4, 5, 9, 10, 11, 12, 13 }),
            (list1: new List<int> { -5, -3, 0, 5 }, list2: new List<int> { -4, -2, 1, 6 }, expected: new[] { -5, -4, -3, -2, 0, 1, 5, 6 }),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    list1 = [{string.Join(", ", tc.list1)}], list2 = [{string.Join(", ", tc.list2)}]");
            Console.WriteLine($"    Expected: [{string.Join(", ", tc.expected)}]");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 5: Find Missing Number
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem5_FindMissingNumber()
    {
        Console.WriteLine("═══ Problem 5: Find Missing Number ═══");
        
        var testCases = new[]
        {
            (nums: new List<int> { 3, 0, 1 }, expected: 2),
            (nums: new List<int> { 0, 1 }, expected: 2),
            (nums: new List<int> { 9, 6, 4, 2, 3, 5, 7, 0, 1 }, expected: 8),
            (nums: new List<int> { 0 }, expected: 1),
            (nums: new List<int> { 1 }, expected: 0),
            (nums: new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10 }, expected: 9),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    [{string.Join(", ", tc.nums)}] (n = {tc.nums.Count})");
            Console.WriteLine($"    Expected: {tc.expected}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 6: Move Zeroes
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem6_MoveZeroes()
    {
        Console.WriteLine("═══ Problem 6: Move Zeroes ═══");
        
        var testCases = new[]
        {
            (nums: new List<int> { 0, 1, 0, 3, 12 }, expected: new[] { 1, 3, 12, 0, 0 }),
            (nums: new List<int> { 0 }, expected: new[] { 0 }),
            (nums: new List<int> { 1, 2, 3 }, expected: new[] { 1, 2, 3 }),  // No zeros
            (nums: new List<int> { 0, 0, 0, 1 }, expected: new[] { 1, 0, 0, 0 }),
            (nums: new List<int> { 1, 0, 2, 0, 3, 0, 4 }, expected: new[] { 1, 2, 3, 4, 0, 0, 0 }),
            (nums: new List<int> { 0, 0, 0, 0, 0 }, expected: new[] { 0, 0, 0, 0, 0 }),  // All zeros
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    [{string.Join(", ", tc.nums)}]");
            Console.WriteLine($"    Expected: [{string.Join(", ", tc.expected)}]");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 7: FX Rate Max Profit (Buy Low, Sell High)
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem7_MaxProfit()
    {
        Console.WriteLine("═══ Problem 7: FX Rate Max Profit ═══");
        
        var testCases = new[]
        {
            (rates: new List<decimal> { 1.0800m, 1.0750m, 1.0850m, 1.0720m, 1.0900m, 1.0820m }, expected: 0.0180m),  // Buy 1.0720, Sell 1.0900
            (rates: new List<decimal> { 7.0m, 1.0m, 5.0m, 3.0m, 6.0m, 4.0m }, expected: 5.0m),  // Buy 1, Sell 6
            (rates: new List<decimal> { 7.0m, 6.0m, 4.0m, 3.0m, 1.0m }, expected: 0.0m),  // Declining - no profit
            (rates: new List<decimal> { 1.0m, 2.0m }, expected: 1.0m),
            (rates: new List<decimal> { 2.0m, 4.0m, 1.0m }, expected: 2.0m),  // Buy 2, Sell 4 (before the dip)
            (rates: new List<decimal> { 1.1000m, 1.1050m, 1.1025m, 1.1100m, 1.1080m, 1.1150m }, expected: 0.0150m),  // Buy 1.1000, Sell 1.1150
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    [{string.Join(", ", tc.rates)}]");
            Console.WriteLine($"    Expected: {tc.expected} profit");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 8: Group Trades by Currency Pair
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem8_GroupByPair()
    {
        Console.WriteLine("═══ Problem 8: Group Trades by Currency Pair ═══");
        
        // Test Case 1
        Console.WriteLine("\n  Test 1:");
        Console.WriteLine("    Input Trades:");
        Console.WriteLine("      - TRD001: EUR/USD, 100000");
        Console.WriteLine("      - TRD002: GBP/USD, 50000");
        Console.WriteLine("      - TRD003: EUR/USD, 75000");
        Console.WriteLine("      - TRD004: USD/JPY, 200000");
        Console.WriteLine("      - TRD005: EUR/USD, 25000");
        Console.WriteLine("    Expected: { EUR/USD: 200000, GBP/USD: 50000, USD/JPY: 200000 }");

        // Test Case 2
        Console.WriteLine("\n  Test 2:");
        Console.WriteLine("    Input Trades:");
        Console.WriteLine("      - TRD001: USD/CHF, 150000");
        Console.WriteLine("      - TRD002: USD/CHF, 100000");
        Console.WriteLine("    Expected: { USD/CHF: 250000 }");

        // Test Case 3
        Console.WriteLine("\n  Test 3:");
        Console.WriteLine("    Input Trades: [] (empty)");
        Console.WriteLine("    Expected: { } (empty dictionary)");

        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 9: Find Pairs with Target Spread
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem9_FindPairsWithSpread()
    {
        Console.WriteLine("═══ Problem 9: Find Pairs with Target Spread ═══");
        
        var testCases = new[]
        {
            (rates: new List<decimal> { 1.08m, 1.10m, 1.12m, 1.06m, 1.14m }, spread: 0.04m, 
             expected: "[(1.06, 1.10), (1.08, 1.12), (1.10, 1.14)]"),
            (rates: new List<decimal> { 1.0m, 2.0m, 3.0m, 4.0m, 5.0m }, spread: 1.0m, 
             expected: "[(1, 2), (2, 3), (3, 4), (4, 5)]"),
            (rates: new List<decimal> { 1.0m, 1.5m, 2.0m, 2.5m, 3.0m }, spread: 0.5m, 
             expected: "[(1, 1.5), (1.5, 2), (2, 2.5), (2.5, 3)]"),
            (rates: new List<decimal> { 1.0m, 5.0m, 10.0m }, spread: 2.0m, 
             expected: "[] (no pairs with spread 2)"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    rates = [{string.Join(", ", tc.rates)}], spread = {tc.spread}");
            Console.WriteLine($"    Expected: {tc.expected}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 10: Sliding Window / Moving Average
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem10_MovingAverage()
    {
        Console.WriteLine("═══ Problem 10: Moving Average ═══");
        
        var testCases = new[]
        {
            (rates: new List<decimal> { 1.08m, 1.09m, 1.10m, 1.08m, 1.07m, 1.11m }, window: 3,
             expected: "[1.09, 1.09, 1.0833, 1.0867]"),
            (rates: new List<decimal> { 1.0m, 2.0m, 3.0m, 4.0m, 5.0m }, window: 2,
             expected: "[1.5, 2.5, 3.5, 4.5]"),
            (rates: new List<decimal> { 10.0m, 20.0m, 30.0m, 40.0m, 50.0m }, window: 3,
             expected: "[20.0, 30.0, 40.0]"),
            (rates: new List<decimal> { 1.0m, 1.0m, 1.0m, 1.0m }, window: 2,
             expected: "[1.0, 1.0, 1.0]"),
            (rates: new List<decimal> { 5.0m }, window: 3,
             expected: "[] (window > length)"),
        };

        for (int i = 0; i < testCases.Length; i++)
        {
            var tc = testCases[i];
            Console.WriteLine($"\n  Test {i + 1}:");
            Console.WriteLine($"    Input:    rates = [{string.Join(", ", tc.rates)}], window = {tc.window}");
            Console.WriteLine($"    Expected: {tc.expected}");
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════════════
    // Problem 11: Rate Limiter
    // ════════════════════════════════════════════════════════════════════
    static void TestProblem11_RateLimiter()
    {
        Console.WriteLine("═══ Problem 11: Rate Limiter ═══");
        
        Console.WriteLine("\n  Test 1: Basic rate limiting");
        Console.WriteLine("    Config: maxRequests = 3, windowSeconds = 10");
        Console.WriteLine("    Scenario:");
        Console.WriteLine("      AllowRequest() at t=0  → true  (1/3)");
        Console.WriteLine("      AllowRequest() at t=1  → true  (2/3)");
        Console.WriteLine("      AllowRequest() at t=2  → true  (3/3)");
        Console.WriteLine("      AllowRequest() at t=3  → false (rate limited!)");
        Console.WriteLine("      AllowRequest() at t=11 → true  (first request expired)");

        Console.WriteLine("\n  Test 2: Burst then wait");
        Console.WriteLine("    Config: maxRequests = 5, windowSeconds = 60");
        Console.WriteLine("    Scenario:");
        Console.WriteLine("      5 requests at t=0      → all true");
        Console.WriteLine("      Request at t=30        → false (within window)");
        Console.WriteLine("      Request at t=61        → true  (window reset)");

        Console.WriteLine();
    }
}

// ════════════════════════════════════════════════════════════════════
// Trade record for Problem 8
// ════════════════════════════════════════════════════════════════════
public record Trade(string TradeId, string Pair, decimal Amount);
