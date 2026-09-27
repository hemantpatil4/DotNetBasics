using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectionsDemo.DictionaryDemo;

/// <summary>
/// Comprehensive Dictionary<K,V> examples for interview preparation
/// Run: dotnet run
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("              DICTIONARY<K,V> COMPREHENSIVE DEMO               ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        Demo1_Creation();
        Demo2_AddingAndUpdating();
        Demo3_Retrieving();
        Demo4_Removing();
        Demo5_Iterating();
        Demo6_CustomKeyTypes();
        Demo7_CountingPattern();
        Demo8_GroupingPattern();
        Demo9_CachingPattern();
        Demo10_FXTradingExample();
        
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    ALL DEMOS COMPLETE                         ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }
    
    static void Demo1_Creation()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 1: Dictionary Creation                                 │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Empty dictionary
        Dictionary<string, decimal> dict1 = new Dictionary<string, decimal>();
        Console.WriteLine($"Empty dictionary count: {dict1.Count}");
        
        // With initial capacity (recommended when size known)
        Dictionary<string, decimal> dict2 = new Dictionary<string, decimal>(100);
        Console.WriteLine($"Pre-sized dictionary capacity allocated for 100 entries");
        
        // Collection initializer
        Dictionary<string, decimal> dict3 = new Dictionary<string, decimal>
        {
            { "EUR/USD", 1.0850m },
            { "GBP/USD", 1.2650m }
        };
        Console.WriteLine($"Collection initializer count: {dict3.Count}");
        
        // Index initializer (C# 6+) - cleaner syntax
        Dictionary<string, decimal> dict4 = new Dictionary<string, decimal>
        {
            ["EUR/USD"] = 1.0850m,
            ["GBP/USD"] = 1.2650m,
            ["USD/JPY"] = 149.50m
        };
        Console.WriteLine($"Index initializer count: {dict4.Count}");
        
        // With custom comparer (case-insensitive keys)
        Dictionary<string, decimal> dict5 = new Dictionary<string, decimal>(
            StringComparer.OrdinalIgnoreCase)
        {
            ["EUR/USD"] = 1.0850m
        };
        Console.WriteLine($"Case-insensitive lookup: eur/usd = {dict5["eur/usd"]}");
        
        // From array using LINQ
        var pairs = new[] { ("EUR/USD", 1.0850m), ("GBP/USD", 1.2650m) };
        var dict6 = pairs.ToDictionary(p => p.Item1, p => p.Item2);
        Console.WriteLine($"From LINQ count: {dict6.Count}");
        
        Console.WriteLine();
    }
    
    static void Demo2_AddingAndUpdating()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 2: Adding and Updating                                 │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var rates = new Dictionary<string, decimal>();
        
        // Add() - throws if key exists
        rates.Add("EUR/USD", 1.0850m);
        Console.WriteLine($"After Add: EUR/USD = {rates["EUR/USD"]}");
        
        // Try to add duplicate
        try
        {
            rates.Add("EUR/USD", 1.0900m);
        }
        catch (ArgumentException)
        {
            Console.WriteLine("❌ Add() throws ArgumentException on duplicate key");
        }
        
        // Indexer - adds OR updates (no exception)
        rates["EUR/USD"] = 1.0855m;  // Updates existing
        Console.WriteLine($"After indexer update: EUR/USD = {rates["EUR/USD"]}");
        
        rates["GBP/USD"] = 1.2650m;  // Adds new
        Console.WriteLine($"After indexer add: GBP/USD = {rates["GBP/USD"]}");
        
        // TryAdd() - returns bool, no exception
        bool added1 = rates.TryAdd("USD/JPY", 149.50m);
        bool added2 = rates.TryAdd("EUR/USD", 1.0900m);
        Console.WriteLine($"TryAdd USD/JPY: {added1}, TryAdd EUR/USD (existing): {added2}");
        
        // Bulk add
        var newRates = new Dictionary<string, decimal>
        {
            ["AUD/USD"] = 0.6550m,
            ["NZD/USD"] = 0.5950m
        };
        foreach (var kvp in newRates)
        {
            rates[kvp.Key] = kvp.Value;
        }
        Console.WriteLine($"After bulk add, total pairs: {rates.Count}");
        
        Console.WriteLine();
    }
    
    static void Demo3_Retrieving()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 3: Retrieving Values                                   │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var rates = new Dictionary<string, decimal>
        {
            ["EUR/USD"] = 1.0850m,
            ["GBP/USD"] = 1.2650m,
            ["USD/JPY"] = 149.50m
        };
        
        // Indexer - throws if not found
        decimal eurRate = rates["EUR/USD"];
        Console.WriteLine($"Indexer: EUR/USD = {eurRate}");
        
        try
        {
            decimal unknown = rates["XYZ/ABC"];
        }
        catch (KeyNotFoundException)
        {
            Console.WriteLine("❌ Indexer throws KeyNotFoundException for missing key");
        }
        
        // TryGetValue - SAFE and PREFERRED
        if (rates.TryGetValue("GBP/USD", out decimal gbpRate))
        {
            Console.WriteLine($"TryGetValue success: GBP/USD = {gbpRate}");
        }
        
        if (!rates.TryGetValue("XYZ/ABC", out decimal unknownRate))
        {
            Console.WriteLine($"TryGetValue for missing key returns false, out = {unknownRate}");
        }
        
        // ContainsKey - O(1)
        Console.WriteLine($"ContainsKey EUR/USD: {rates.ContainsKey("EUR/USD")}");
        Console.WriteLine($"ContainsKey XYZ/ABC: {rates.ContainsKey("XYZ/ABC")}");
        
        // ContainsValue - O(n) ⚠️
        Console.WriteLine($"ContainsValue 1.0850: {rates.ContainsValue(1.0850m)} (O(n)!)");
        
        // GetValueOrDefault - returns default if not found
        decimal rate1 = rates.GetValueOrDefault("EUR/USD", 0m);
        decimal rate2 = rates.GetValueOrDefault("XYZ/ABC", 0m);
        Console.WriteLine($"GetValueOrDefault EUR/USD: {rate1}, XYZ/ABC: {rate2}");
        
        Console.WriteLine();
    }
    
    static void Demo4_Removing()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 4: Removing Entries                                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var rates = new Dictionary<string, decimal>
        {
            ["EUR/USD"] = 1.0850m,
            ["GBP/USD"] = 1.2650m,
            ["USD/JPY"] = 149.50m
        };
        
        Console.WriteLine($"Initial count: {rates.Count}");
        
        // Remove by key - returns bool
        bool removed1 = rates.Remove("EUR/USD");
        bool removed2 = rates.Remove("XYZ/ABC");
        Console.WriteLine($"Remove EUR/USD: {removed1}, Remove XYZ/ABC: {removed2}");
        Console.WriteLine($"After Remove: {rates.Count}");
        
        // Remove with out parameter - get removed value
        if (rates.Remove("GBP/USD", out decimal removedRate))
        {
            Console.WriteLine($"Removed GBP/USD with rate: {removedRate}");
        }
        
        // Clear all
        rates.Clear();
        Console.WriteLine($"After Clear: {rates.Count}");
        
        // Safe removal during iteration
        rates = new Dictionary<string, decimal>
        {
            ["EUR/USD"] = 1.0850m,
            ["GBP/USD"] = 1.2650m,
            ["USD/JPY"] = 149.50m,
            ["AUD/USD"] = 0.6550m
        };
        
        // Remove all pairs with rate < 1.0
        var keysToRemove = rates.Where(kvp => kvp.Value < 1.0m)
                                .Select(kvp => kvp.Key)
                                .ToList();
        
        foreach (var key in keysToRemove)
        {
            rates.Remove(key);
        }
        Console.WriteLine($"After removing rates < 1.0: {string.Join(", ", rates.Keys)}");
        
        Console.WriteLine();
    }
    
    static void Demo5_Iterating()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 5: Iteration Patterns                                  │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var rates = new Dictionary<string, decimal>
        {
            ["EUR/USD"] = 1.0850m,
            ["GBP/USD"] = 1.2650m,
            ["USD/JPY"] = 149.50m
        };
        
        // KeyValuePair iteration
        Console.WriteLine("KeyValuePair iteration:");
        foreach (KeyValuePair<string, decimal> kvp in rates)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        // var iteration
        Console.WriteLine("\nvar iteration:");
        foreach (var kvp in rates)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        // Deconstruction (C# 7+)
        Console.WriteLine("\nDeconstruction:");
        foreach (var (pair, rate) in rates)
        {
            Console.WriteLine($"  {pair}: {rate}");
        }
        
        // Keys only
        Console.WriteLine("\nKeys only:");
        foreach (var key in rates.Keys)
        {
            Console.Write($"{key} ");
        }
        Console.WriteLine();
        
        // Values only
        Console.WriteLine("\nValues only:");
        foreach (var value in rates.Values)
        {
            Console.Write($"{value} ");
        }
        Console.WriteLine();
        
        // LINQ operations
        Console.WriteLine("\nLINQ - rates > 1.0:");
        var highRates = rates.Where(kvp => kvp.Value > 1.0m);
        foreach (var kvp in highRates)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        Console.WriteLine();
    }
    
    static void Demo6_CustomKeyTypes()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 6: Custom Key Types                                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        // Using record (automatically implements GetHashCode and Equals)
        var ratesWithRecord = new Dictionary<CurrencyPairRecord, decimal>();
        ratesWithRecord[new CurrencyPairRecord("EUR", "USD")] = 1.0850m;
        ratesWithRecord[new CurrencyPairRecord("GBP", "USD")] = 1.2650m;
        
        // This works because records implement equality
        var key = new CurrencyPairRecord("EUR", "USD");
        Console.WriteLine($"Record key lookup: EUR/USD = {ratesWithRecord[key]}");
        
        // Using class with proper overrides
        var ratesWithClass = new Dictionary<CurrencyPairClass, decimal>();
        ratesWithClass[new CurrencyPairClass("EUR", "USD")] = 1.0850m;
        ratesWithClass[new CurrencyPairClass("GBP", "USD")] = 1.2650m;
        
        // This works because we override GetHashCode and Equals
        var classKey = new CurrencyPairClass("EUR", "USD");
        Console.WriteLine($"Class key lookup: EUR/USD = {ratesWithClass[classKey]}");
        
        // Demonstrate what happens without proper overrides
        var badDict = new Dictionary<BadCurrencyPair, decimal>();
        badDict[new BadCurrencyPair("EUR", "USD")] = 1.0850m;
        
        // This will NOT find the entry because default GetHashCode uses reference
        var badKey = new BadCurrencyPair("EUR", "USD");
        Console.WriteLine($"Bad key ContainsKey: {badDict.ContainsKey(badKey)} (should be true!)");
        
        Console.WriteLine();
    }
    
    static void Demo7_CountingPattern()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 7: Counting Pattern                                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = new[] { "EUR/USD", "GBP/USD", "EUR/USD", "USD/JPY", "EUR/USD", "GBP/USD" };
        
        // Method 1: Traditional approach
        var count1 = new Dictionary<string, int>();
        foreach (var pair in trades)
        {
            if (count1.ContainsKey(pair))
                count1[pair]++;
            else
                count1[pair] = 1;
        }
        
        // Method 2: TryGetValue (more efficient - single lookup)
        var count2 = new Dictionary<string, int>();
        foreach (var pair in trades)
        {
            count2.TryGetValue(pair, out int current);
            count2[pair] = current + 1;
        }
        
        // Method 3: LINQ
        var count3 = trades.GroupBy(p => p)
                          .ToDictionary(g => g.Key, g => g.Count());
        
        Console.WriteLine("Trade counts:");
        foreach (var kvp in count3.OrderByDescending(kvp => kvp.Value))
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        // Find most traded pair
        var mostTraded = count3.MaxBy(kvp => kvp.Value);
        Console.WriteLine($"\nMost traded: {mostTraded.Key} ({mostTraded.Value} trades)");
        
        Console.WriteLine();
    }
    
    static void Demo8_GroupingPattern()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 8: Grouping Pattern                                    │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = new List<Trade>
        {
            new Trade("T1", "EUR/USD", 100000m, "BUY"),
            new Trade("T2", "GBP/USD", 50000m, "SELL"),
            new Trade("T3", "EUR/USD", 200000m, "SELL"),
            new Trade("T4", "USD/JPY", 150000m, "BUY"),
            new Trade("T5", "EUR/USD", 75000m, "BUY")
        };
        
        // Group trades by currency pair
        var tradesByPair = new Dictionary<string, List<Trade>>();
        foreach (var trade in trades)
        {
            if (!tradesByPair.ContainsKey(trade.CurrencyPair))
            {
                tradesByPair[trade.CurrencyPair] = new List<Trade>();
            }
            tradesByPair[trade.CurrencyPair].Add(trade);
        }
        
        Console.WriteLine("Trades by currency pair:");
        foreach (var kvp in tradesByPair)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value.Count} trades, " +
                            $"Total: {kvp.Value.Sum(t => t.Amount):N0}");
        }
        
        // LINQ GroupBy + ToDictionary
        var grouped = trades
            .GroupBy(t => t.Side)
            .ToDictionary(g => g.Key, g => g.ToList());
        
        Console.WriteLine("\nTrades by side (LINQ):");
        foreach (var kvp in grouped)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value.Count} trades");
        }
        
        // ToLookup - immutable grouping
        var lookup = trades.ToLookup(t => t.CurrencyPair);
        Console.WriteLine($"\nLookup EUR/USD count: {lookup["EUR/USD"].Count()}");
        
        Console.WriteLine();
    }
    
    static void Demo9_CachingPattern()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 9: Caching Pattern                                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var cache = new RateCache(TimeSpan.FromSeconds(2));
        
        // Simulate rate fetching with cache
        Console.WriteLine("First fetch (cache miss):");
        var rate1 = cache.GetRate("EUR/USD", pair =>
        {
            Console.WriteLine($"  → Fetching {pair} from external source...");
            return 1.0850m;
        });
        Console.WriteLine($"  Rate: {rate1}");
        
        Console.WriteLine("\nSecond fetch (cache hit):");
        var rate2 = cache.GetRate("EUR/USD", pair =>
        {
            Console.WriteLine($"  → Fetching {pair} from external source...");
            return 1.0900m;  // Different value, but won't be used
        });
        Console.WriteLine($"  Rate: {rate2}");
        
        Console.WriteLine("\nDifferent pair (cache miss):");
        var rate3 = cache.GetRate("GBP/USD", pair =>
        {
            Console.WriteLine($"  → Fetching {pair} from external source...");
            return 1.2650m;
        });
        Console.WriteLine($"  Rate: {rate3}");
        
        Console.WriteLine();
    }
    
    static void Demo10_FXTradingExample()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 10: FX Trading - Quote Book Management                 │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var quoteBook = new QuoteBook();
        
        // Add quotes
        quoteBook.UpdateQuote("EUR/USD", 1.0845m, 1.0850m, "Bank A");
        quoteBook.UpdateQuote("EUR/USD", 1.0846m, 1.0851m, "Bank B");
        quoteBook.UpdateQuote("GBP/USD", 1.2645m, 1.2650m, "Bank A");
        quoteBook.UpdateQuote("USD/JPY", 149.45m, 149.55m, "Bank C");
        
        Console.WriteLine("Current Quote Book:");
        quoteBook.PrintQuotes();
        
        // Get best quote
        Console.WriteLine("\nBest quotes:");
        var (bestBid, bestAsk, bestSpread) = quoteBook.GetBestQuote("EUR/USD");
        Console.WriteLine($"  EUR/USD - Best Bid: {bestBid}, Best Ask: {bestAsk}, Spread: {bestSpread:F4}");
        
        // Position management
        var positions = new Dictionary<string, decimal>
        {
            ["EUR/USD"] = 1000000m,   // Long 1M EUR
            ["GBP/USD"] = -500000m,   // Short 500K GBP
            ["USD/JPY"] = 2000000m    // Long 2M USD (in JPY terms)
        };
        
        Console.WriteLine("\nPositions:");
        foreach (var (pair, amount) in positions)
        {
            var direction = amount >= 0 ? "LONG" : "SHORT";
            Console.WriteLine($"  {pair}: {direction} {Math.Abs(amount):N0}");
        }
        
        // Calculate P&L (simplified)
        decimal totalPnL = 0m;
        foreach (var (pair, position) in positions)
        {
            var quote = quoteBook.GetBestQuote(pair);
            if (quote.bestBid > 0)
            {
                // Use mid price for P&L
                var mid = (quote.bestBid + quote.bestAsk) / 2;
                var pnl = position * 0.0010m;  // Simplified: assume 10 pip move
                totalPnL += pnl;
            }
        }
        Console.WriteLine($"\nTotal P&L (simulated): ${totalPnL:N2}");
        
        Console.WriteLine();
    }
}

// Supporting types

public record CurrencyPairRecord(string Base, string Quote);

public class CurrencyPairClass
{
    public string Base { get; }
    public string Quote { get; }
    
    public CurrencyPairClass(string baseCurrency, string quoteCurrency)
    {
        Base = baseCurrency;
        Quote = quoteCurrency;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is CurrencyPairClass other)
        {
            return Base == other.Base && Quote == other.Quote;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Base, Quote);
    }
}

// This class does NOT override GetHashCode/Equals - demonstrates the problem
public class BadCurrencyPair
{
    public string Base { get; }
    public string Quote { get; }
    
    public BadCurrencyPair(string baseCurrency, string quoteCurrency)
    {
        Base = baseCurrency;
        Quote = quoteCurrency;
    }
    // Missing GetHashCode and Equals overrides!
}

public record Trade(string TradeId, string CurrencyPair, decimal Amount, string Side);

public class RateCache
{
    private readonly Dictionary<string, (decimal Rate, DateTime Timestamp)> _cache = new();
    private readonly TimeSpan _expiry;
    
    public RateCache(TimeSpan expiry)
    {
        _expiry = expiry;
    }
    
    public decimal GetRate(string pair, Func<string, decimal> fetchRate)
    {
        if (_cache.TryGetValue(pair, out var cached))
        {
            if (DateTime.UtcNow - cached.Timestamp < _expiry)
            {
                Console.WriteLine($"  ✓ Cache hit for {pair}");
                return cached.Rate;
            }
            Console.WriteLine($"  ⏰ Cache expired for {pair}");
        }
        else
        {
            Console.WriteLine($"  ✗ Cache miss for {pair}");
        }
        
        decimal rate = fetchRate(pair);
        _cache[pair] = (rate, DateTime.UtcNow);
        return rate;
    }
}

public class QuoteBook
{
    // Pair → Source → Quote
    private readonly Dictionary<string, Dictionary<string, Quote>> _quotes = new();
    
    public void UpdateQuote(string pair, decimal bid, decimal ask, string source)
    {
        if (!_quotes.ContainsKey(pair))
        {
            _quotes[pair] = new Dictionary<string, Quote>();
        }
        
        _quotes[pair][source] = new Quote(bid, ask, source, DateTime.UtcNow);
    }
    
    public (decimal bestBid, decimal bestAsk, decimal bestSpread) GetBestQuote(string pair)
    {
        if (!_quotes.TryGetValue(pair, out var sources))
        {
            return (0, 0, 0);
        }
        
        decimal bestBid = sources.Values.Max(q => q.Bid);
        decimal bestAsk = sources.Values.Min(q => q.Ask);
        decimal spread = bestAsk - bestBid;
        
        return (bestBid, bestAsk, spread);
    }
    
    public void PrintQuotes()
    {
        foreach (var (pair, sources) in _quotes)
        {
            Console.WriteLine($"  {pair}:");
            foreach (var (source, quote) in sources)
            {
                Console.WriteLine($"    {source}: {quote.Bid}/{quote.Ask} " +
                                $"(spread: {quote.Ask - quote.Bid:F4})");
            }
        }
    }
}

public record Quote(decimal Bid, decimal Ask, string Source, DateTime Timestamp);
