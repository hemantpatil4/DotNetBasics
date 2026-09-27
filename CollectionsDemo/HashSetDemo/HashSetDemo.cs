using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectionsDemo.HashSetDemo;

/// <summary>
/// Comprehensive HashSet<T> examples for interview preparation
/// Run: dotnet run
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("               HASHSET<T> COMPREHENSIVE DEMO                   ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        Demo1_Creation();
        Demo2_AddingElements();
        Demo3_ContainsAndCount();
        Demo4_RemovingElements();
        Demo5_SetOperationsUnion();
        Demo6_SetOperationsIntersection();
        Demo7_SetOperationsExcept();
        Demo8_SetComparisons();
        Demo9_CustomObjects();
        Demo10_FXTradingExample();
        
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    ALL DEMOS COMPLETE                         ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }
    
    static void Demo1_Creation()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 1: HashSet Creation                                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Empty HashSet
        HashSet<string> set1 = new HashSet<string>();
        Console.WriteLine($"Empty HashSet count: {set1.Count}");
        
        // With initial values (collection initializer)
        HashSet<string> set2 = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
        Console.WriteLine($"Initialized HashSet: {string.Join(", ", set2)}");
        
        // From existing collection - removes duplicates!
        List<string> listWithDups = new List<string> { "EUR/USD", "EUR/USD", "GBP/USD", "GBP/USD" };
        HashSet<string> set3 = new HashSet<string>(listWithDups);
        Console.WriteLine($"From list with {listWithDups.Count} items (2 dups): {set3.Count} unique items");
        
        // With custom comparer (case-insensitive)
        HashSet<string> set4 = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "EUR/USD"
        };
        set4.Add("eur/usd");  // Won't add - same as EUR/USD with case-insensitive
        Console.WriteLine($"Case-insensitive set (added EUR/USD and eur/usd): {set4.Count} item(s)");
        
        // From LINQ ToHashSet
        var set5 = listWithDups.ToHashSet();
        Console.WriteLine($"Using ToHashSet(): {set5.Count} unique items");
        
        Console.WriteLine();
    }
    
    static void Demo2_AddingElements()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 2: Adding Elements                                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var set = new HashSet<string>();
        
        // Add returns bool - true if added, false if existed
        bool added1 = set.Add("EUR/USD");
        bool added2 = set.Add("EUR/USD");  // Duplicate!
        bool added3 = set.Add("GBP/USD");
        
        Console.WriteLine($"Add EUR/USD first time: {added1}");
        Console.WriteLine($"Add EUR/USD second time: {added2} (duplicate ignored!)");
        Console.WriteLine($"Add GBP/USD: {added3}");
        Console.WriteLine($"Set count: {set.Count}");
        
        // Add multiple items using UnionWith
        var newPairs = new[] { "USD/JPY", "AUD/USD", "EUR/USD" };  // EUR/USD is duplicate
        int countBefore = set.Count;
        set.UnionWith(newPairs);
        int countAfter = set.Count;
        Console.WriteLine($"\nUnionWith 3 items (1 duplicate): Added {countAfter - countBefore} new items");
        Console.WriteLine($"Set now: {string.Join(", ", set)}");
        
        Console.WriteLine();
    }
    
    static void Demo3_ContainsAndCount()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 3: Contains and Count                                  │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var activePairs = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD" };
        
        // Contains - O(1) average
        Console.WriteLine($"Contains EUR/USD: {activePairs.Contains("EUR/USD")}");
        Console.WriteLine($"Contains XYZ/ABC: {activePairs.Contains("XYZ/ABC")}");
        
        // Count
        Console.WriteLine($"Count: {activePairs.Count}");
        
        // Any (from LINQ)
        Console.WriteLine($"Any elements: {activePairs.Any()}");
        Console.WriteLine($"Any USD pairs: {activePairs.Any(p => p.Contains("USD"))}");
        
        // TryGetValue - useful with custom comparers
        var caseInsensitive = new HashSet<string>(activePairs, StringComparer.OrdinalIgnoreCase);
        if (caseInsensitive.TryGetValue("eur/usd", out string? actual))
        {
            Console.WriteLine($"TryGetValue 'eur/usd' found actual: '{actual}'");
        }
        
        Console.WriteLine();
    }
    
    static void Demo4_RemovingElements()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 4: Removing Elements                                   │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var set = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD", "NZD/USD" };
        Console.WriteLine($"Initial: {string.Join(", ", set)}");
        
        // Remove single - returns bool
        bool removed1 = set.Remove("EUR/USD");
        bool removed2 = set.Remove("XYZ/ABC");  // Doesn't exist
        Console.WriteLine($"Remove EUR/USD: {removed1}");
        Console.WriteLine($"Remove XYZ/ABC: {removed2}");
        
        // RemoveWhere - remove by condition
        int removedCount = set.RemoveWhere(pair => pair.EndsWith("USD"));
        Console.WriteLine($"RemoveWhere ends with USD: removed {removedCount} items");
        Console.WriteLine($"Remaining: {string.Join(", ", set)}");
        
        // Clear all
        set.Clear();
        Console.WriteLine($"After Clear: {set.Count} items");
        
        Console.WriteLine();
    }
    
    static void Demo5_SetOperationsUnion()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 5: Set Operations - Union                              │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var desk1 = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
        var desk2 = new HashSet<string> { "EUR/USD", "AUD/USD", "NZD/USD" };
        
        Console.WriteLine($"Desk 1: {string.Join(", ", desk1)}");
        Console.WriteLine($"Desk 2: {string.Join(", ", desk2)}");
        
        // Union - combines all unique elements
        var allPairs = new HashSet<string>(desk1);  // Copy first
        allPairs.UnionWith(desk2);
        Console.WriteLine($"\nUnion (all pairs): {string.Join(", ", allPairs)}");
        
        // Static method (doesn't modify original)
        var union = desk1.Union(desk2);  // LINQ extension
        Console.WriteLine($"LINQ Union: {string.Join(", ", union)}");
        
        Console.WriteLine();
    }
    
    static void Demo6_SetOperationsIntersection()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 6: Set Operations - Intersection                       │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var desk1 = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
        var desk2 = new HashSet<string> { "EUR/USD", "AUD/USD", "USD/JPY" };
        
        Console.WriteLine($"Desk 1: {string.Join(", ", desk1)}");
        Console.WriteLine($"Desk 2: {string.Join(", ", desk2)}");
        
        // Intersection - only elements in BOTH sets
        var commonPairs = new HashSet<string>(desk1);
        commonPairs.IntersectWith(desk2);
        Console.WriteLine($"\nIntersection (common pairs): {string.Join(", ", commonPairs)}");
        
        // LINQ Intersect (doesn't modify original)
        var common = desk1.Intersect(desk2);
        Console.WriteLine($"LINQ Intersect: {string.Join(", ", common)}");
        
        Console.WriteLine();
    }
    
    static void Demo7_SetOperationsExcept()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 7: Set Operations - Except & Symmetric Except          │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var desk1 = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY" };
        var desk2 = new HashSet<string> { "EUR/USD", "AUD/USD", "NZD/USD" };
        
        Console.WriteLine($"Desk 1: {string.Join(", ", desk1)}");
        Console.WriteLine($"Desk 2: {string.Join(", ", desk2)}");
        
        // Except - elements in first but not in second
        var desk1Only = new HashSet<string>(desk1);
        desk1Only.ExceptWith(desk2);
        Console.WriteLine($"\nDesk 1 only (Except): {string.Join(", ", desk1Only)}");
        
        var desk2Only = new HashSet<string>(desk2);
        desk2Only.ExceptWith(desk1);
        Console.WriteLine($"Desk 2 only (Except): {string.Join(", ", desk2Only)}");
        
        // Symmetric Except - elements in either but not both
        var exclusive = new HashSet<string>(desk1);
        exclusive.SymmetricExceptWith(desk2);
        Console.WriteLine($"\nExclusive to one desk (SymmetricExcept): {string.Join(", ", exclusive)}");
        
        Console.WriteLine();
    }
    
    static void Demo8_SetComparisons()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 8: Set Comparisons                                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var majorPairs = new HashSet<string> { "EUR/USD", "GBP/USD" };
        var allPairs = new HashSet<string> { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD" };
        var samePairs = new HashSet<string> { "GBP/USD", "EUR/USD" };  // Same as majorPairs
        var differentPairs = new HashSet<string> { "NZD/USD", "CAD/USD" };
        
        Console.WriteLine($"Major pairs: {string.Join(", ", majorPairs)}");
        Console.WriteLine($"All pairs: {string.Join(", ", allPairs)}");
        Console.WriteLine($"Same pairs: {string.Join(", ", samePairs)}");
        Console.WriteLine($"Different pairs: {string.Join(", ", differentPairs)}");
        
        // Subset checks
        Console.WriteLine($"\nmajorPairs.IsSubsetOf(allPairs): {majorPairs.IsSubsetOf(allPairs)}");
        Console.WriteLine($"majorPairs.IsProperSubsetOf(allPairs): {majorPairs.IsProperSubsetOf(allPairs)}");
        
        // Superset checks
        Console.WriteLine($"\nallPairs.IsSupersetOf(majorPairs): {allPairs.IsSupersetOf(majorPairs)}");
        Console.WriteLine($"allPairs.IsProperSupersetOf(majorPairs): {allPairs.IsProperSupersetOf(majorPairs)}");
        
        // Equality
        Console.WriteLine($"\nmajorPairs.SetEquals(samePairs): {majorPairs.SetEquals(samePairs)}");
        Console.WriteLine($"majorPairs.SetEquals(allPairs): {majorPairs.SetEquals(allPairs)}");
        
        // Overlap check
        Console.WriteLine($"\nmajorPairs.Overlaps(allPairs): {majorPairs.Overlaps(allPairs)}");
        Console.WriteLine($"majorPairs.Overlaps(differentPairs): {majorPairs.Overlaps(differentPairs)}");
        
        Console.WriteLine();
    }
    
    static void Demo9_CustomObjects()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 9: Custom Objects in HashSet                           │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Using record (auto-generates GetHashCode and Equals)
        var tradeSet = new HashSet<TradeRecord>();
        tradeSet.Add(new TradeRecord("T001", "EUR/USD", 100000m));
        tradeSet.Add(new TradeRecord("T001", "EUR/USD", 100000m));  // Duplicate!
        tradeSet.Add(new TradeRecord("T002", "GBP/USD", 50000m));
        
        Console.WriteLine($"Record HashSet (added T001 twice, T002 once):");
        Console.WriteLine($"  Count: {tradeSet.Count}");
        foreach (var trade in tradeSet)
        {
            Console.WriteLine($"  {trade}");
        }
        
        // Using class without proper equality - PROBLEM!
        var badSet = new HashSet<TradeClass>();
        badSet.Add(new TradeClass { TradeId = "T001", Pair = "EUR/USD" });
        badSet.Add(new TradeClass { TradeId = "T001", Pair = "EUR/USD" });  // Added as duplicate!
        
        Console.WriteLine($"\nClass without GetHashCode/Equals:");
        Console.WriteLine($"  Count (should be 1, but is): {badSet.Count}");
        
        // Using custom IEqualityComparer
        var goodSet = new HashSet<TradeClass>(new TradeIdComparer());
        goodSet.Add(new TradeClass { TradeId = "T001", Pair = "EUR/USD" });
        goodSet.Add(new TradeClass { TradeId = "T001", Pair = "GBP/USD" });  // Same TradeId!
        
        Console.WriteLine($"\nClass with IEqualityComparer<TradeClass> (by TradeId):");
        Console.WriteLine($"  Count: {goodSet.Count}");
        
        Console.WriteLine();
    }
    
    static void Demo10_FXTradingExample()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 10: FX Trading - Active Pairs & Permission System      │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Tradable currency pairs by region
        var londonPairs = new HashSet<string> 
        { 
            "EUR/USD", "GBP/USD", "EUR/GBP", "EUR/JPY", "GBP/JPY" 
        };
        
        var newYorkPairs = new HashSet<string> 
        { 
            "EUR/USD", "USD/JPY", "USD/CAD", "USD/MXN", "USD/BRL" 
        };
        
        var tokyoPairs = new HashSet<string> 
        { 
            "USD/JPY", "EUR/JPY", "GBP/JPY", "AUD/JPY", "NZD/JPY" 
        };
        
        Console.WriteLine("Trading desk pairs:");
        Console.WriteLine($"  London: {string.Join(", ", londonPairs)}");
        Console.WriteLine($"  New York: {string.Join(", ", newYorkPairs)}");
        Console.WriteLine($"  Tokyo: {string.Join(", ", tokyoPairs)}");
        
        // Find pairs tradable in all locations
        var globalPairs = new HashSet<string>(londonPairs);
        globalPairs.IntersectWith(newYorkPairs);
        globalPairs.IntersectWith(tokyoPairs);
        Console.WriteLine($"\nGlobal pairs (all locations): {string.Join(", ", globalPairs)}");
        
        // Find pairs exclusive to each region
        var londonExclusive = new HashSet<string>(londonPairs);
        londonExclusive.ExceptWith(newYorkPairs);
        londonExclusive.ExceptWith(tokyoPairs);
        Console.WriteLine($"London exclusive: {string.Join(", ", londonExclusive)}");
        
        // Rate limiter - track processed trades
        var rateLimiter = new TradeLimiter(maxTradesPerSecond: 10);
        
        Console.WriteLine("\nRate limiter test:");
        for (int i = 1; i <= 15; i++)
        {
            string tradeId = $"T{i:D3}";
            bool allowed = rateLimiter.AllowTrade(tradeId);
            Console.WriteLine($"  Trade {tradeId}: {(allowed ? "✓ Allowed" : "✗ Rejected (duplicate or limit)")}");
        }
        
        // Validate incoming quotes
        var validPairs = new HashSet<string>(londonPairs);
        validPairs.UnionWith(newYorkPairs);
        validPairs.UnionWith(tokyoPairs);
        
        var incomingQuotes = new[] { "EUR/USD", "XYZ/ABC", "USD/JPY", "INVALID/PAIR" };
        Console.WriteLine("\nQuote validation:");
        foreach (var pair in incomingQuotes)
        {
            bool isValid = validPairs.Contains(pair);
            Console.WriteLine($"  {pair}: {(isValid ? "✓ Valid" : "✗ Invalid")}");
        }
        
        Console.WriteLine();
    }
}

// Supporting types

public record TradeRecord(string TradeId, string Pair, decimal Amount);

public class TradeClass
{
    public string TradeId { get; set; } = "";
    public string Pair { get; set; } = "";
}

public class TradeIdComparer : IEqualityComparer<TradeClass>
{
    public bool Equals(TradeClass? x, TradeClass? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;
        return x.TradeId == y.TradeId;
    }
    
    public int GetHashCode(TradeClass obj)
    {
        return obj.TradeId?.GetHashCode() ?? 0;
    }
}

public class TradeLimiter
{
    private readonly HashSet<string> _processedTrades = new();
    private readonly int _maxTrades;
    private int _tradeCount;
    
    public TradeLimiter(int maxTradesPerSecond)
    {
        _maxTrades = maxTradesPerSecond;
    }
    
    public bool AllowTrade(string tradeId)
    {
        // Check for duplicate
        if (!_processedTrades.Add(tradeId))
        {
            return false;  // Duplicate trade
        }
        
        // Check rate limit
        if (_tradeCount >= _maxTrades)
        {
            return false;  // Rate limit exceeded
        }
        
        _tradeCount++;
        return true;
    }
    
    public void Reset()
    {
        _processedTrades.Clear();
        _tradeCount = 0;
    }
}
