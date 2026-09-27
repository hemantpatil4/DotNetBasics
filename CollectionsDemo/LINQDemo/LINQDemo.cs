using System;
using System.Collections.Generic;
using System.Linq;

namespace CollectionsDemo.LINQDemo;

/// <summary>
/// Comprehensive LINQ examples for interview preparation
/// Run: dotnet run
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                  LINQ COMPREHENSIVE DEMO                      ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");
        
        Demo1_QueryVsMethodSyntax();
        Demo2_DeferredVsImmediateExecution();
        Demo3_FilteringOperations();
        Demo4_ProjectionOperations();
        Demo5_OrderingOperations();
        Demo6_GroupingOperations();
        Demo7_AggregationOperations();
        Demo8_SetOperations();
        Demo9_JoinOperations();
        Demo10_FXTradingAnalysis();
        
        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    ALL DEMOS COMPLETE                         ");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
    }
    
    static void Demo1_QueryVsMethodSyntax()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 1: Query Syntax vs Method Syntax                       │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = GetSampleTrades();
        
        // Query syntax (SQL-like)
        var queryResult = from t in trades
                         where t.Amount > 100000
                         orderby t.Amount descending
                         select t;
        
        // Method syntax (fluent)
        var methodResult = trades
            .Where(t => t.Amount > 100000)
            .OrderByDescending(t => t.Amount);
        
        Console.WriteLine("Query Syntax (trades > 100000, sorted desc):");
        foreach (var t in queryResult.Take(3))
        {
            Console.WriteLine($"  {t.TradeId}: {t.Amount:N0}");
        }
        
        Console.WriteLine("\nMethod Syntax (same result):");
        foreach (var t in methodResult.Take(3))
        {
            Console.WriteLine($"  {t.TradeId}: {t.Amount:N0}");
        }
        
        // Method syntax can do more (not all operators have query syntax)
        var methodOnly = trades
            .DistinctBy(t => t.Pair)
            .Select(t => t.Pair);
        
        Console.WriteLine($"\nMethod-only operators (DistinctBy): {string.Join(", ", methodOnly)}");
        
        Console.WriteLine();
    }
    
    static void Demo2_DeferredVsImmediateExecution()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 2: Deferred vs Immediate Execution                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var numbers = new List<int> { 1, 2, 3, 4, 5 };
        
        // DEFERRED - query not executed yet
        var query = numbers.Where(n => n > 2);
        Console.WriteLine("Query defined (deferred)");
        
        // Modify the source
        numbers.Add(6);
        numbers.Add(7);
        Console.WriteLine("Added 6 and 7 to source list");
        
        // NOW the query executes - includes new elements!
        Console.WriteLine($"Query result: {string.Join(", ", query)}");
        Console.WriteLine("Notice: includes 6 and 7 added AFTER query was defined!");
        
        // IMMEDIATE - executes immediately
        var immediate = numbers.Where(n => n > 2).ToList();
        Console.WriteLine($"\nImmediate (ToList): {string.Join(", ", immediate)}");
        
        numbers.Add(8);
        Console.WriteLine("Added 8 to source");
        Console.WriteLine($"Immediate result: {string.Join(", ", immediate)} (no 8!)");
        Console.WriteLine($"Deferred query now: {string.Join(", ", query)} (includes 8!)");
        
        // ⚠️ Common pitfall: multiple enumeration
        Console.WriteLine("\n⚠️ Multiple Enumeration Warning:");
        var expensiveQuery = numbers.Where(n =>
        {
            Console.WriteLine($"    Evaluating {n}...");
            return n > 5;
        });
        
        Console.WriteLine("  First enumeration:");
        var count1 = expensiveQuery.Count();
        Console.WriteLine("  Second enumeration:");
        var count2 = expensiveQuery.Count();
        Console.WriteLine($"  Query executed TWICE! (each returned {count1})");
        
        Console.WriteLine();
    }
    
    static void Demo3_FilteringOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 3: Filtering Operations                                │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = GetSampleTrades();
        
        // Where
        var eurTrades = trades.Where(t => t.Pair == "EUR/USD");
        Console.WriteLine($"EUR/USD trades: {eurTrades.Count()}");
        
        // Multiple conditions
        var bigEurTrades = trades.Where(t => t.Pair == "EUR/USD" && t.Amount > 100000);
        Console.WriteLine($"Big EUR/USD trades (>100K): {bigEurTrades.Count()}");
        
        // Distinct
        var uniquePairs = trades.Select(t => t.Pair).Distinct();
        Console.WriteLine($"Unique pairs: {string.Join(", ", uniquePairs)}");
        
        // DistinctBy (NET 6+)
        var firstOfEachPair = trades.DistinctBy(t => t.Pair);
        Console.WriteLine($"First trade of each pair: {firstOfEachPair.Count()}");
        
        // Take / Skip
        var firstThree = trades.Take(3);
        var afterThree = trades.Skip(3).Take(3);
        Console.WriteLine($"First 3: {string.Join(", ", firstThree.Select(t => t.TradeId))}");
        Console.WriteLine($"Next 3: {string.Join(", ", afterThree.Select(t => t.TradeId))}");
        
        // TakeWhile / SkipWhile
        var sorted = trades.OrderBy(t => t.Amount).ToList();
        var smallTrades = sorted.TakeWhile(t => t.Amount < 100000);
        Console.WriteLine($"Trades until amount >= 100K: {smallTrades.Count()}");
        
        // OfType - filter by type
        var mixed = new object[] { 1, "hello", 2, "world", 3 };
        var strings = mixed.OfType<string>();
        Console.WriteLine($"Strings from mixed: {string.Join(", ", strings)}");
        
        Console.WriteLine();
    }
    
    static void Demo4_ProjectionOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 4: Projection (Select) Operations                      │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = GetSampleTrades();
        
        // Select - simple property
        var tradeIds = trades.Select(t => t.TradeId);
        Console.WriteLine($"Trade IDs: {string.Join(", ", tradeIds.Take(5))}...");
        
        // Select - anonymous type
        var summaries = trades
            .Take(3)
            .Select(t => new 
            { 
                t.TradeId, 
                t.Pair, 
                FormattedAmount = $"${t.Amount:N0}" 
            });
        
        Console.WriteLine("\nTrade summaries:");
        foreach (var s in summaries)
        {
            Console.WriteLine($"  {s.TradeId}: {s.Pair} {s.FormattedAmount}");
        }
        
        // Select with index
        var indexed = trades.Take(3).Select((t, i) => $"{i + 1}. {t.TradeId}");
        Console.WriteLine($"\nIndexed: {string.Join(", ", indexed)}");
        
        // SelectMany - flatten
        var clients = new List<Client>
        {
            new Client("C1", new List<Order> { new Order("O1", 1000), new Order("O2", 2000) }),
            new Client("C2", new List<Order> { new Order("O3", 3000) }),
            new Client("C3", new List<Order> { new Order("O4", 4000), new Order("O5", 5000) })
        };
        
        // Get all orders from all clients (flatten)
        var allOrders = clients.SelectMany(c => c.Orders);
        Console.WriteLine($"\nAll orders (flattened): {allOrders.Count()}");
        
        // SelectMany with result selector (include parent info)
        var orderDetails = clients.SelectMany(
            c => c.Orders,
            (client, order) => new { ClientId = client.Id, order.OrderId, order.Amount });
        
        Console.WriteLine("Order details with client:");
        foreach (var od in orderDetails)
        {
            Console.WriteLine($"  {od.ClientId} → {od.OrderId}: ${od.Amount}");
        }
        
        Console.WriteLine();
    }
    
    static void Demo5_OrderingOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 5: Ordering Operations                                 │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = GetSampleTrades();
        
        // OrderBy - ascending
        var byAmount = trades.OrderBy(t => t.Amount).Take(3);
        Console.WriteLine("By amount (ascending):");
        foreach (var t in byAmount)
        {
            Console.WriteLine($"  {t.TradeId}: {t.Amount:N0}");
        }
        
        // OrderByDescending
        var byAmountDesc = trades.OrderByDescending(t => t.Amount).Take(3);
        Console.WriteLine("\nBy amount (descending):");
        foreach (var t in byAmountDesc)
        {
            Console.WriteLine($"  {t.TradeId}: {t.Amount:N0}");
        }
        
        // ThenBy - secondary sort
        var multiSort = trades
            .OrderBy(t => t.Pair)
            .ThenByDescending(t => t.Amount)
            .Take(6);
        
        Console.WriteLine("\nBy pair, then by amount (desc):");
        foreach (var t in multiSort)
        {
            Console.WriteLine($"  {t.Pair}: {t.TradeId} - {t.Amount:N0}");
        }
        
        // Reverse
        var reversed = trades.Take(3).Reverse();
        Console.WriteLine($"\nReversed first 3: {string.Join(", ", reversed.Select(t => t.TradeId))}");
        
        Console.WriteLine();
    }
    
    static void Demo6_GroupingOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 6: Grouping Operations                                 │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = GetSampleTrades();
        
        // GroupBy - basic
        var byPair = trades.GroupBy(t => t.Pair);
        
        Console.WriteLine("Trades grouped by pair:");
        foreach (var group in byPair)
        {
            Console.WriteLine($"  {group.Key}: {group.Count()} trades");
        }
        
        // GroupBy with aggregation
        var summaryByPair = trades
            .GroupBy(t => t.Pair)
            .Select(g => new
            {
                Pair = g.Key,
                TradeCount = g.Count(),
                TotalAmount = g.Sum(t => t.Amount),
                AvgAmount = g.Average(t => t.Amount)
            })
            .OrderByDescending(s => s.TotalAmount);
        
        Console.WriteLine("\nSummary by pair:");
        foreach (var s in summaryByPair)
        {
            Console.WriteLine($"  {s.Pair}: {s.TradeCount} trades, " +
                            $"Total: ${s.TotalAmount:N0}, Avg: ${s.AvgAmount:N0}");
        }
        
        // ToLookup - immediate execution grouping
        var lookup = trades.ToLookup(t => t.Pair);
        
        Console.WriteLine($"\nLookup EUR/USD: {lookup["EUR/USD"].Count()} trades");
        Console.WriteLine($"Lookup GBP/USD: {lookup["GBP/USD"].Count()} trades");
        Console.WriteLine($"Lookup non-existent: {lookup["XYZ/ABC"].Count()} trades");
        
        // Chunk (NET 6+) - split into batches
        var batches = trades.Chunk(3);
        Console.WriteLine($"\nChunked into batches of 3: {batches.Count()} batches");
        
        Console.WriteLine();
    }
    
    static void Demo7_AggregationOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 7: Aggregation Operations                              │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = GetSampleTrades();
        
        // Basic aggregations
        Console.WriteLine("Basic aggregations:");
        Console.WriteLine($"  Count: {trades.Count()}");
        Console.WriteLine($"  Sum: ${trades.Sum(t => t.Amount):N0}");
        Console.WriteLine($"  Average: ${trades.Average(t => t.Amount):N0}");
        Console.WriteLine($"  Min: ${trades.Min(t => t.Amount):N0}");
        Console.WriteLine($"  Max: ${trades.Max(t => t.Amount):N0}");
        
        // MinBy / MaxBy (NET 6+)
        var smallestTrade = trades.MinBy(t => t.Amount);
        var largestTrade = trades.MaxBy(t => t.Amount);
        Console.WriteLine($"\n  Smallest trade: {smallestTrade?.TradeId} (${smallestTrade?.Amount:N0})");
        Console.WriteLine($"  Largest trade: {largestTrade?.TradeId} (${largestTrade?.Amount:N0})");
        
        // Count with predicate
        var bigTradeCount = trades.Count(t => t.Amount > 100000);
        Console.WriteLine($"\n  Trades > 100K: {bigTradeCount}");
        
        // Aggregate (reduce)
        var totalAmount = trades.Aggregate(0m, (sum, t) => sum + t.Amount);
        Console.WriteLine($"  Aggregate sum: ${totalAmount:N0}");
        
        // Aggregate with multiple values
        var stats = trades.Aggregate(
            (Count: 0, Total: 0m, Min: decimal.MaxValue, Max: decimal.MinValue),
            (acc, t) => (
                acc.Count + 1,
                acc.Total + t.Amount,
                Math.Min(acc.Min, t.Amount),
                Math.Max(acc.Max, t.Amount)
            ));
        
        Console.WriteLine($"\n  Aggregate multiple: Count={stats.Count}, Total=${stats.Total:N0}, " +
                         $"Min=${stats.Min:N0}, Max=${stats.Max:N0}");
        
        // Quantifiers
        Console.WriteLine("\nQuantifiers:");
        Console.WriteLine($"  Any trades: {trades.Any()}");
        Console.WriteLine($"  Any EUR/USD: {trades.Any(t => t.Pair == "EUR/USD")}");
        Console.WriteLine($"  All > 10K: {trades.All(t => t.Amount > 10000)}");
        Console.WriteLine($"  All > 1M: {trades.All(t => t.Amount > 1000000)}");
        
        Console.WriteLine();
    }
    
    static void Demo8_SetOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 8: Set Operations                                      │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var desk1Pairs = new[] { "EUR/USD", "GBP/USD", "USD/JPY" };
        var desk2Pairs = new[] { "EUR/USD", "AUD/USD", "NZD/USD" };
        
        Console.WriteLine($"Desk 1: {string.Join(", ", desk1Pairs)}");
        Console.WriteLine($"Desk 2: {string.Join(", ", desk2Pairs)}");
        
        // Union
        var allPairs = desk1Pairs.Union(desk2Pairs);
        Console.WriteLine($"\nUnion: {string.Join(", ", allPairs)}");
        
        // Intersect
        var commonPairs = desk1Pairs.Intersect(desk2Pairs);
        Console.WriteLine($"Intersect: {string.Join(", ", commonPairs)}");
        
        // Except
        var desk1Only = desk1Pairs.Except(desk2Pairs);
        var desk2Only = desk2Pairs.Except(desk1Pairs);
        Console.WriteLine($"Desk 1 only (Except): {string.Join(", ", desk1Only)}");
        Console.WriteLine($"Desk 2 only (Except): {string.Join(", ", desk2Only)}");
        
        // Distinct
        var withDups = new[] { "EUR/USD", "EUR/USD", "GBP/USD", "EUR/USD" };
        var unique = withDups.Distinct();
        Console.WriteLine($"\nDistinct from [{string.Join(", ", withDups)}]: {string.Join(", ", unique)}");
        
        // Concat (allows duplicates)
        var concatenated = desk1Pairs.Concat(desk2Pairs);
        Console.WriteLine($"\nConcat (with dups): {string.Join(", ", concatenated)}");
        
        // SequenceEqual
        var list1 = new[] { 1, 2, 3 };
        var list2 = new[] { 1, 2, 3 };
        var list3 = new[] { 3, 2, 1 };
        Console.WriteLine($"\n[1,2,3] SequenceEqual [1,2,3]: {list1.SequenceEqual(list2)}");
        Console.WriteLine($"[1,2,3] SequenceEqual [3,2,1]: {list1.SequenceEqual(list3)}");
        
        Console.WriteLine();
    }
    
    static void Demo9_JoinOperations()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 9: Join Operations                                     │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = new[]
        {
            new { TradeId = "T1", Pair = "EUR/USD", Amount = 100000m },
            new { TradeId = "T2", Pair = "GBP/USD", Amount = 50000m },
            new { TradeId = "T3", Pair = "EUR/USD", Amount = 200000m },
            new { TradeId = "T4", Pair = "USD/JPY", Amount = 150000m }
        };
        
        var rates = new[]
        {
            new { Pair = "EUR/USD", Rate = 1.0850m },
            new { Pair = "GBP/USD", Rate = 1.2650m },
            new { Pair = "USD/JPY", Rate = 149.50m }
        };
        
        // Join (inner join)
        var joined = trades.Join(
            rates,
            t => t.Pair,
            r => r.Pair,
            (trade, rate) => new 
            { 
                trade.TradeId, 
                trade.Pair, 
                trade.Amount, 
                rate.Rate,
                Value = trade.Amount * rate.Rate 
            });
        
        Console.WriteLine("Inner Join (trades with rates):");
        foreach (var item in joined)
        {
            Console.WriteLine($"  {item.TradeId}: {item.Pair} {item.Amount:N0} @ {item.Rate} = {item.Value:N2}");
        }
        
        // GroupJoin (left join with grouping)
        var clients = new[]
        {
            new { ClientId = "C1", Name = "Client A" },
            new { ClientId = "C2", Name = "Client B" },
            new { ClientId = "C3", Name = "Client C" }
        };
        
        var orders = new[]
        {
            new { OrderId = "O1", ClientId = "C1", Amount = 1000m },
            new { OrderId = "O2", ClientId = "C1", Amount = 2000m },
            new { OrderId = "O3", ClientId = "C2", Amount = 3000m }
        };
        
        var clientOrders = clients.GroupJoin(
            orders,
            c => c.ClientId,
            o => o.ClientId,
            (client, clientOrderList) => new
            {
                client.Name,
                OrderCount = clientOrderList.Count(),
                TotalAmount = clientOrderList.Sum(o => o.Amount)
            });
        
        Console.WriteLine("\nGroup Join (clients with order summary):");
        foreach (var co in clientOrders)
        {
            Console.WriteLine($"  {co.Name}: {co.OrderCount} orders, Total: ${co.TotalAmount:N0}");
        }
        
        // Zip
        var names = new[] { "Alice", "Bob", "Charlie" };
        var ages = new[] { 25, 30, 35 };
        
        var zipped = names.Zip(ages, (name, age) => $"{name} is {age}");
        Console.WriteLine($"\nZip: {string.Join("; ", zipped)}");
        
        // Zip to tuples (NET 6+)
        var tuples = names.Zip(ages);
        Console.WriteLine($"Zip to tuples: {string.Join("; ", tuples)}");
        
        Console.WriteLine();
    }
    
    static void Demo10_FXTradingAnalysis()
    {
        Console.WriteLine("┌─────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│ Demo 10: FX Trading Analysis with LINQ                      │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────┘\n");
        
        var trades = GetSampleTrades();
        
        // Daily trading summary
        Console.WriteLine("=== Daily Trading Summary ===\n");
        
        var summary = trades
            .GroupBy(t => t.Pair)
            .Select(g => new
            {
                Pair = g.Key,
                TradeCount = g.Count(),
                BuyCount = g.Count(t => t.Side == "BUY"),
                SellCount = g.Count(t => t.Side == "SELL"),
                TotalVolume = g.Sum(t => t.Amount),
                AvgSize = g.Average(t => t.Amount),
                LargestTrade = g.MaxBy(t => t.Amount)
            })
            .OrderByDescending(s => s.TotalVolume);
        
        foreach (var s in summary)
        {
            Console.WriteLine($"{s.Pair}:");
            Console.WriteLine($"  Trades: {s.TradeCount} ({s.BuyCount} buys, {s.SellCount} sells)");
            Console.WriteLine($"  Volume: ${s.TotalVolume:N0}");
            Console.WriteLine($"  Avg Size: ${s.AvgSize:N0}");
            Console.WriteLine($"  Largest: {s.LargestTrade?.TradeId} (${s.LargestTrade?.Amount:N0})");
            Console.WriteLine();
        }
        
        // Position calculation
        Console.WriteLine("=== Net Positions ===\n");
        
        var positions = trades
            .GroupBy(t => t.Pair)
            .Select(g => new
            {
                Pair = g.Key,
                NetPosition = g.Sum(t => t.Side == "BUY" ? t.Amount : -t.Amount)
            })
            .Where(p => p.NetPosition != 0);
        
        foreach (var pos in positions)
        {
            var direction = pos.NetPosition > 0 ? "LONG" : "SHORT";
            Console.WriteLine($"  {pos.Pair}: {direction} ${Math.Abs(pos.NetPosition):N0}");
        }
        
        // Top traders (by volume)
        Console.WriteLine("\n=== Top 3 Trades by Size ===\n");
        
        var topTrades = trades
            .OrderByDescending(t => t.Amount)
            .Take(3)
            .Select((t, i) => $"{i + 1}. {t.TradeId}: {t.Side} {t.Pair} ${t.Amount:N0}");
        
        foreach (var t in topTrades)
        {
            Console.WriteLine($"  {t}");
        }
        
        // Trading patterns
        Console.WriteLine("\n=== Buy/Sell Ratio by Pair ===\n");
        
        var ratios = trades
            .GroupBy(t => t.Pair)
            .Select(g => new
            {
                Pair = g.Key,
                BuyVolume = g.Where(t => t.Side == "BUY").Sum(t => t.Amount),
                SellVolume = g.Where(t => t.Side == "SELL").Sum(t => t.Amount)
            })
            .Select(r => new
            {
                r.Pair,
                r.BuyVolume,
                r.SellVolume,
                Ratio = r.SellVolume > 0 ? r.BuyVolume / r.SellVolume : 0
            });
        
        foreach (var r in ratios)
        {
            var sentiment = r.Ratio > 1 ? "Bullish" : r.Ratio < 1 ? "Bearish" : "Neutral";
            Console.WriteLine($"  {r.Pair}: Buy ${r.BuyVolume:N0} / Sell ${r.SellVolume:N0} = {r.Ratio:F2} ({sentiment})");
        }
        
        Console.WriteLine();
    }
    
    // Sample data generators
    
    static List<Trade> GetSampleTrades()
    {
        return new List<Trade>
        {
            new Trade("T001", "EUR/USD", 100000m, "BUY"),
            new Trade("T002", "EUR/USD", 150000m, "SELL"),
            new Trade("T003", "GBP/USD", 200000m, "BUY"),
            new Trade("T004", "EUR/USD", 75000m, "BUY"),
            new Trade("T005", "USD/JPY", 300000m, "SELL"),
            new Trade("T006", "GBP/USD", 125000m, "SELL"),
            new Trade("T007", "EUR/USD", 250000m, "BUY"),
            new Trade("T008", "USD/JPY", 180000m, "BUY"),
            new Trade("T009", "AUD/USD", 90000m, "SELL"),
            new Trade("T010", "EUR/USD", 50000m, "SELL"),
            new Trade("T011", "GBP/USD", 175000m, "BUY"),
            new Trade("T012", "USD/JPY", 220000m, "SELL")
        };
    }
}

// Supporting types

public record Trade(string TradeId, string Pair, decimal Amount, string Side);

public record Client(string Id, List<Order> Orders);

public record Order(string OrderId, decimal Amount);
