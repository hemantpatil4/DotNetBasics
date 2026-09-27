# Thread-Safe FX Rate Streaming – Real-World Example

> **Domain:** FX Trading Systems - Live Rate Streaming  
> **Focus:** Thread Safety, Concurrent Access, Producer-Consumer Pattern

---

## Business Context

In an FX trading system, **rate streaming** is a critical component:

- **Multiple producers**: Rate feeds from Reuters, Bloomberg, EBS, etc.
- **Multiple consumers**: Trading engines, risk calculators, UI clients
- **High frequency**: Thousands of rate updates per second
- **Low latency**: Stale rates = financial loss
- **Thread safety**: Multiple threads reading/writing simultaneously

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                FX RATE STREAMING ARCHITECTURE                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  RATE PROVIDERS                    RATE ENGINE                          │
│  ───────────────                   ───────────                          │
│  ┌───────────┐                    ┌─────────────────────────────────┐  │
│  │  Reuters  │──┐                 │      ThreadSafeRateCache        │  │
│  │  Feed     │  │                 │   ┌─────────────────────────┐   │  │
│  └───────────┘  │                 │   │ ConcurrentDictionary    │   │  │
│  ┌───────────┐  │    ┌────────┐   │   │ EUR/USD → Rate          │   │  │
│  │ Bloomberg │──┼───►│ Rate   │──►│   │ GBP/USD → Rate          │   │  │
│  │   Feed    │  │    │Aggregator│ │   │ USD/JPY → Rate          │   │  │
│  └───────────┘  │    └────────┘   │   └─────────────────────────┘   │  │
│  ┌───────────┐  │                 │                                 │  │
│  │   EBS     │──┘                 │   ReaderWriterLockSlim for     │  │
│  │   Feed    │                    │   complex operations            │  │
│  └───────────┘                    └──────────────┬──────────────────┘  │
│                                                  │                     │
│                                                  ▼                     │
│                                   ┌─────────────────────────────────┐  │
│                                   │        CONSUMERS                │  │
│                                   │  ┌──────────┐ ┌──────────┐     │  │
│                                   │  │ Trading  │ │   Risk   │     │  │
│                                   │  │ Engine   │ │Calculator│     │  │
│                                   │  └──────────┘ └──────────┘     │  │
│                                   │  ┌──────────┐ ┌──────────┐     │  │
│                                   │  │   UI     │ │ Analytics│     │  │
│                                   │  │ Clients  │ │  Engine  │     │  │
│                                   │  └──────────┘ └──────────┘     │  │
│                                   └─────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Complete Implementation

### 1. Immutable Rate Model

```csharp
using System;

namespace FXRateStreaming.Models
{
    /// <summary>
    /// Immutable FX Rate - inherently thread-safe
    /// Once created, cannot be modified - safe to share across threads
    /// </summary>
    public sealed class FxRate
    {
        public string CurrencyPair { get; }
        public decimal Bid { get; }
        public decimal Ask { get; }
        public decimal Mid => (Bid + Ask) / 2;
        public decimal Spread => Ask - Bid;
        public DateTime Timestamp { get; }
        public string Provider { get; }
        public long SequenceNumber { get; }

        public FxRate(
            string currencyPair,
            decimal bid,
            decimal ask,
            string provider,
            long sequenceNumber)
        {
            CurrencyPair = currencyPair ?? throw new ArgumentNullException(nameof(currencyPair));
            Bid = bid;
            Ask = ask;
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            SequenceNumber = sequenceNumber;
            Timestamp = DateTime.UtcNow;

            ValidateRate();
        }

        private void ValidateRate()
        {
            if (Bid <= 0) throw new ArgumentException("Bid must be positive");
            if (Ask <= 0) throw new ArgumentException("Ask must be positive");
            if (Bid > Ask) throw new ArgumentException("Bid cannot be greater than Ask");
        }

        /// <summary>
        /// Check if this rate is stale (older than threshold)
        /// </summary>
        public bool IsStale(TimeSpan maxAge) => DateTime.UtcNow - Timestamp > maxAge;

        public override string ToString() =>
            $"{CurrencyPair}: {Bid:F5}/{Ask:F5} (Spread: {Spread:F5}) [{Provider}]";
    }

    /// <summary>
    /// Rate update event arguments - immutable
    /// </summary>
    public sealed class RateUpdateEventArgs : EventArgs
    {
        public FxRate OldRate { get; }
        public FxRate NewRate { get; }
        public RateUpdateType UpdateType { get; }

        public RateUpdateEventArgs(FxRate oldRate, FxRate newRate, RateUpdateType updateType)
        {
            OldRate = oldRate;
            NewRate = newRate;
            UpdateType = updateType;
        }
    }

    public enum RateUpdateType
    {
        New,
        Updated,
        Stale,
        Removed
    }
}
```

### 2. Thread-Safe Rate Cache

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FXRateStreaming.Core
{
    /// <summary>
    /// Thread-safe cache for FX rates
    /// Supports concurrent read/write operations from multiple threads
    /// </summary>
    public sealed class ThreadSafeRateCache : IDisposable
    {
        // Thread-safe dictionary for storing rates
        private readonly ConcurrentDictionary<string, FxRate> _rates;

        // ReaderWriterLockSlim for complex operations that need atomicity
        private readonly ReaderWriterLockSlim _complexOpLock;

        // Statistics using Interlocked for lock-free updates
        private long _totalUpdates;
        private long _totalReads;
        private long _staleRateCount;

        // Stale rate threshold
        private readonly TimeSpan _staleThreshold;

        // Event for rate updates (thread-safe invocation)
        public event EventHandler<RateUpdateEventArgs> RateUpdated;

        public ThreadSafeRateCache(TimeSpan? staleThreshold = null)
        {
            _rates = new ConcurrentDictionary<string, FxRate>(StringComparer.OrdinalIgnoreCase);
            _complexOpLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _staleThreshold = staleThreshold ?? TimeSpan.FromSeconds(5);
        }

        #region Basic Thread-Safe Operations

        /// <summary>
        /// Get a single rate - thread-safe read
        /// </summary>
        public FxRate GetRate(string currencyPair)
        {
            Interlocked.Increment(ref _totalReads);

            if (_rates.TryGetValue(currencyPair, out var rate))
            {
                if (rate.IsStale(_staleThreshold))
                {
                    Interlocked.Increment(ref _staleRateCount);
                }
                return rate;
            }

            return null;
        }

        /// <summary>
        /// Update or add a rate - thread-safe write
        /// </summary>
        public void UpdateRate(FxRate newRate)
        {
            if (newRate == null) throw new ArgumentNullException(nameof(newRate));

            FxRate oldRate = null;
            RateUpdateType updateType;

            var result = _rates.AddOrUpdate(
                newRate.CurrencyPair,
                // Add factory - called if key doesn't exist
                key =>
                {
                    updateType = RateUpdateType.New;
                    return newRate;
                },
                // Update factory - called if key exists
                (key, existing) =>
                {
                    oldRate = existing;

                    // Only update if newer (using sequence number)
                    if (newRate.SequenceNumber > existing.SequenceNumber)
                    {
                        return newRate;
                    }
                    return existing;  // Keep old rate if new one is stale
                });

            Interlocked.Increment(ref _totalUpdates);

            // Determine update type for event
            if (oldRate == null)
            {
                updateType = RateUpdateType.New;
            }
            else if (result == newRate)
            {
                updateType = RateUpdateType.Updated;
            }
            else
            {
                return; // Rate was not actually updated (older sequence)
            }

            // Raise event (thread-safe)
            OnRateUpdated(new RateUpdateEventArgs(oldRate, result, updateType));
        }

        /// <summary>
        /// Remove a rate - thread-safe delete
        /// </summary>
        public bool RemoveRate(string currencyPair)
        {
            if (_rates.TryRemove(currencyPair, out var removed))
            {
                OnRateUpdated(new RateUpdateEventArgs(removed, null, RateUpdateType.Removed));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if rate exists - thread-safe read
        /// </summary>
        public bool HasRate(string currencyPair) => _rates.ContainsKey(currencyPair);

        /// <summary>
        /// Get all rates - returns snapshot (thread-safe)
        /// </summary>
        public IReadOnlyList<FxRate> GetAllRates()
        {
            Interlocked.Increment(ref _totalReads);
            return _rates.Values.ToList();
        }

        /// <summary>
        /// Get count - thread-safe
        /// </summary>
        public int Count => _rates.Count;

        #endregion

        #region Complex Operations (Need Locking)

        /// <summary>
        /// Get best bid across all rates for a currency
        /// Uses ReaderWriterLockSlim for consistent snapshot
        /// </summary>
        public FxRate GetBestBid(string baseCurrency)
        {
            _complexOpLock.EnterReadLock();
            try
            {
                return _rates.Values
                    .Where(r => r.CurrencyPair.StartsWith(baseCurrency + "/"))
                    .OrderByDescending(r => r.Bid)
                    .FirstOrDefault();
            }
            finally
            {
                _complexOpLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Batch update rates - atomic operation
        /// </summary>
        public void BatchUpdateRates(IEnumerable<FxRate> rates)
        {
            _complexOpLock.EnterWriteLock();
            try
            {
                foreach (var rate in rates)
                {
                    _rates.AddOrUpdate(
                        rate.CurrencyPair,
                        rate,
                        (_, existing) => rate.SequenceNumber > existing.SequenceNumber
                            ? rate
                            : existing);
                }

                Interlocked.Add(ref _totalUpdates, rates.Count());
            }
            finally
            {
                _complexOpLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Get rates with spread less than threshold
        /// Read operation that needs consistent view
        /// </summary>
        public IReadOnlyList<FxRate> GetRatesWithTightSpread(decimal maxSpread)
        {
            _complexOpLock.EnterReadLock();
            try
            {
                return _rates.Values
                    .Where(r => r.Spread <= maxSpread && !r.IsStale(_staleThreshold))
                    .ToList();
            }
            finally
            {
                _complexOpLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Clear all stale rates - atomic cleanup
        /// </summary>
        public int ClearStaleRates()
        {
            _complexOpLock.EnterWriteLock();
            try
            {
                var staleKeys = _rates
                    .Where(kvp => kvp.Value.IsStale(_staleThreshold))
                    .Select(kvp => kvp.Key)
                    .ToList();

                int removed = 0;
                foreach (var key in staleKeys)
                {
                    if (_rates.TryRemove(key, out var rate))
                    {
                        removed++;
                        OnRateUpdated(new RateUpdateEventArgs(rate, null, RateUpdateType.Stale));
                    }
                }

                return removed;
            }
            finally
            {
                _complexOpLock.ExitWriteLock();
            }
        }

        #endregion

        #region Statistics (Lock-Free)

        public RateCacheStatistics GetStatistics()
        {
            return new RateCacheStatistics
            {
                TotalRates = _rates.Count,
                TotalUpdates = Interlocked.Read(ref _totalUpdates),
                TotalReads = Interlocked.Read(ref _totalReads),
                StaleRateCount = Interlocked.Read(ref _staleRateCount),
                ActiveRates = _rates.Values.Count(r => !r.IsStale(_staleThreshold)),
                StaleRates = _rates.Values.Count(r => r.IsStale(_staleThreshold))
            };
        }

        public void ResetStatistics()
        {
            Interlocked.Exchange(ref _totalUpdates, 0);
            Interlocked.Exchange(ref _totalReads, 0);
            Interlocked.Exchange(ref _staleRateCount, 0);
        }

        #endregion

        #region Thread-Safe Event Invocation

        private void OnRateUpdated(RateUpdateEventArgs args)
        {
            // Thread-safe event invocation
            var handler = Volatile.Read(ref RateUpdated);
            handler?.Invoke(this, args);
        }

        #endregion

        #region Dispose Pattern

        private bool _disposed = false;

        public void Dispose()
        {
            if (!_disposed)
            {
                _complexOpLock?.Dispose();
                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Statistics for monitoring - immutable snapshot
    /// </summary>
    public class RateCacheStatistics
    {
        public int TotalRates { get; init; }
        public long TotalUpdates { get; init; }
        public long TotalReads { get; init; }
        public long StaleRateCount { get; init; }
        public int ActiveRates { get; init; }
        public int StaleRates { get; init; }

        public override string ToString() =>
            $"Rates: {TotalRates} (Active: {ActiveRates}, Stale: {StaleRates}) | " +
            $"Updates: {TotalUpdates} | Reads: {TotalReads}";
    }
}
```

### 3. Rate Provider (Producer)

```csharp
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace FXRateStreaming.Providers
{
    /// <summary>
    /// Simulates a rate provider feed (e.g., Reuters, Bloomberg)
    /// Produces rates on its own thread
    /// </summary>
    public class RateProvider : IDisposable
    {
        private readonly string _providerName;
        private readonly BlockingCollection<FxRate> _rateBuffer;
        private readonly string[] _currencyPairs;
        private readonly Random _random;
        private readonly CancellationTokenSource _cts;
        private readonly Thread _producerThread;

        private long _sequenceNumber;
        private bool _isRunning;
        private readonly object _stateLock = new object();

        // Base rates for simulation
        private readonly ConcurrentDictionary<string, (decimal Bid, decimal Ask)> _baseRates;

        public string ProviderName => _providerName;
        public bool IsRunning => _isRunning;

        public RateProvider(
            string providerName,
            BlockingCollection<FxRate> rateBuffer,
            string[] currencyPairs)
        {
            _providerName = providerName;
            _rateBuffer = rateBuffer;
            _currencyPairs = currencyPairs;
            _random = new Random();
            _cts = new CancellationTokenSource();

            _baseRates = new ConcurrentDictionary<string, (decimal, decimal)>
            {
                ["EUR/USD"] = (1.0850m, 1.0852m),
                ["GBP/USD"] = (1.2650m, 1.2653m),
                ["USD/JPY"] = (149.50m, 149.53m),
                ["AUD/USD"] = (0.6550m, 0.6553m),
                ["USD/CHF"] = (0.8750m, 0.8753m),
                ["USD/CAD"] = (1.3550m, 1.3553m),
                ["NZD/USD"] = (0.6150m, 0.6153m),
                ["EUR/GBP"] = (0.8580m, 0.8583m)
            };

            _producerThread = new Thread(ProducerLoop)
            {
                Name = $"{providerName}_ProducerThread",
                IsBackground = true
            };
        }

        public void Start()
        {
            lock (_stateLock)
            {
                if (_isRunning) return;
                _isRunning = true;
                _producerThread.Start();
                Console.WriteLine($"[{_providerName}] Started rate feed");
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isRunning) return;

                _cts.Cancel();
                _producerThread.Join(TimeSpan.FromSeconds(5));
                _isRunning = false;

                Console.WriteLine($"[{_providerName}] Stopped rate feed");
            }
        }

        private void ProducerLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    foreach (var pair in _currencyPairs)
                    {
                        if (_cts.Token.IsCancellationRequested) break;

                        var rate = GenerateRate(pair);

                        // Try to add to buffer - will block if buffer is full
                        if (!_rateBuffer.TryAdd(rate, 100, _cts.Token))
                        {
                            Console.WriteLine($"[{_providerName}] Buffer full, dropping rate for {pair}");
                        }
                    }

                    // Simulate rate refresh interval
                    Thread.Sleep(_random.Next(10, 50));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{_providerName}] Error: {ex.Message}");
                    Thread.Sleep(1000);  // Back off on error
                }
            }
        }

        private FxRate GenerateRate(string pair)
        {
            var seq = Interlocked.Increment(ref _sequenceNumber);

            // Get base rate and add random variation
            if (_baseRates.TryGetValue(pair, out var baseRate))
            {
                var variation = (decimal)(_random.NextDouble() - 0.5) * 0.001m;
                var bid = baseRate.Bid + variation;
                var ask = bid + baseRate.Ask - baseRate.Bid;  // Maintain spread

                return new FxRate(pair, bid, ask, _providerName, seq);
            }

            // Default rate if pair not found
            return new FxRate(pair, 1.0m, 1.0001m, _providerName, seq);
        }

        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
}
```

### 4. Rate Aggregator (Consumer/Coordinator)

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FXRateStreaming.Core
{
    /// <summary>
    /// Aggregates rates from multiple providers
    /// Consumes from buffer and updates the cache
    /// Implements producer-consumer pattern
    /// </summary>
    public class RateAggregator : IDisposable
    {
        private readonly ThreadSafeRateCache _cache;
        private readonly BlockingCollection<FxRate> _rateBuffer;
        private readonly CancellationTokenSource _cts;
        private readonly List<Thread> _consumerThreads;
        private readonly int _consumerCount;

        // Statistics
        private long _processedCount;
        private long _duplicateCount;
        private long _errorCount;

        private bool _isRunning;
        private readonly object _stateLock = new object();

        public RateAggregator(
            ThreadSafeRateCache cache,
            BlockingCollection<FxRate> rateBuffer,
            int consumerCount = 2)
        {
            _cache = cache;
            _rateBuffer = rateBuffer;
            _consumerCount = consumerCount;
            _cts = new CancellationTokenSource();
            _consumerThreads = new List<Thread>();
        }

        public void Start()
        {
            lock (_stateLock)
            {
                if (_isRunning) return;
                _isRunning = true;

                // Start multiple consumer threads
                for (int i = 0; i < _consumerCount; i++)
                {
                    var thread = new Thread(ConsumerLoop)
                    {
                        Name = $"RateConsumer_{i}",
                        IsBackground = true
                    };
                    _consumerThreads.Add(thread);
                    thread.Start();
                }

                Console.WriteLine($"[Aggregator] Started {_consumerCount} consumer threads");
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isRunning) return;

                _cts.Cancel();

                // Wait for all consumers to finish
                foreach (var thread in _consumerThreads)
                {
                    thread.Join(TimeSpan.FromSeconds(5));
                }

                _consumerThreads.Clear();
                _isRunning = false;

                Console.WriteLine("[Aggregator] Stopped all consumer threads");
            }
        }

        private void ConsumerLoop()
        {
            var threadName = Thread.CurrentThread.Name;

            try
            {
                // GetConsumingEnumerable blocks when buffer is empty
                foreach (var rate in _rateBuffer.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        ProcessRate(rate);
                        Interlocked.Increment(ref _processedCount);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref _errorCount);
                        Console.WriteLine($"[{threadName}] Error processing rate: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[{threadName}] Consumer stopped");
            }
        }

        private void ProcessRate(FxRate newRate)
        {
            // Get existing rate to check if this is actually newer
            var existingRate = _cache.GetRate(newRate.CurrencyPair);

            if (existingRate != null &&
                existingRate.SequenceNumber >= newRate.SequenceNumber &&
                existingRate.Provider == newRate.Provider)
            {
                // Duplicate or old rate
                Interlocked.Increment(ref _duplicateCount);
                return;
            }

            // Best execution logic: if different provider, take best rate
            if (existingRate != null && existingRate.Provider != newRate.Provider)
            {
                // Take the one with tighter spread
                if (newRate.Spread >= existingRate.Spread && !existingRate.IsStale(TimeSpan.FromSeconds(1)))
                {
                    // Existing rate is better and fresh
                    return;
                }
            }

            // Update cache
            _cache.UpdateRate(newRate);
        }

        public AggregatorStatistics GetStatistics()
        {
            return new AggregatorStatistics
            {
                ProcessedCount = Interlocked.Read(ref _processedCount),
                DuplicateCount = Interlocked.Read(ref _duplicateCount),
                ErrorCount = Interlocked.Read(ref _errorCount),
                BufferCount = _rateBuffer.Count
            };
        }

        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }

    public class AggregatorStatistics
    {
        public long ProcessedCount { get; init; }
        public long DuplicateCount { get; init; }
        public long ErrorCount { get; init; }
        public int BufferCount { get; init; }

        public override string ToString() =>
            $"Processed: {ProcessedCount} | Duplicates: {DuplicateCount} | " +
            $"Errors: {ErrorCount} | Buffer: {BufferCount}";
    }
}
```

### 5. Rate Streaming Service (Orchestrator)

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FXRateStreaming.Services
{
    /// <summary>
    /// Main orchestrator for the rate streaming system
    /// Coordinates providers, aggregator, and cache
    /// </summary>
    public class RateStreamingService : IDisposable
    {
        private readonly ThreadSafeRateCache _cache;
        private readonly BlockingCollection<FxRate> _rateBuffer;
        private readonly List<RateProvider> _providers;
        private readonly RateAggregator _aggregator;

        private readonly CancellationTokenSource _monitorCts;
        private Thread _monitorThread;
        private Thread _cleanupThread;

        private bool _isRunning;
        private readonly object _stateLock = new object();

        // Default currency pairs
        private static readonly string[] DefaultPairs =
        {
            "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD",
            "USD/CHF", "USD/CAD", "NZD/USD", "EUR/GBP"
        };

        public ThreadSafeRateCache Cache => _cache;
        public bool IsRunning => _isRunning;

        public RateStreamingService(int bufferSize = 10000)
        {
            _cache = new ThreadSafeRateCache(TimeSpan.FromSeconds(5));
            _rateBuffer = new BlockingCollection<FxRate>(bufferSize);
            _providers = new List<RateProvider>();
            _aggregator = new RateAggregator(_cache, _rateBuffer, consumerCount: 2);
            _monitorCts = new CancellationTokenSource();

            // Subscribe to rate updates
            _cache.RateUpdated += OnRateUpdated;
        }

        public void AddProvider(string name, string[] pairs = null)
        {
            var provider = new RateProvider(name, _rateBuffer, pairs ?? DefaultPairs);
            _providers.Add(provider);
        }

        public void Start()
        {
            lock (_stateLock)
            {
                if (_isRunning) return;
                _isRunning = true;

                Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║           FX RATE STREAMING SERVICE STARTING                 ║");
                Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

                // Start aggregator (consumers)
                _aggregator.Start();

                // Start all providers (producers)
                foreach (var provider in _providers)
                {
                    provider.Start();
                }

                // Start monitoring thread
                _monitorThread = new Thread(MonitorLoop)
                {
                    Name = "RateMonitor",
                    IsBackground = true
                };
                _monitorThread.Start();

                // Start cleanup thread
                _cleanupThread = new Thread(CleanupLoop)
                {
                    Name = "StaleRateCleaner",
                    IsBackground = true
                };
                _cleanupThread.Start();

                Console.WriteLine("[Service] All components started successfully\n");
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isRunning) return;

                Console.WriteLine("\n[Service] Shutting down...");

                // Stop in reverse order
                _monitorCts.Cancel();

                // Stop providers first (stop producing)
                foreach (var provider in _providers)
                {
                    provider.Stop();
                }

                // Signal no more items
                _rateBuffer.CompleteAdding();

                // Stop aggregator (consumers will drain buffer)
                _aggregator.Stop();

                // Wait for background threads
                _monitorThread?.Join(TimeSpan.FromSeconds(2));
                _cleanupThread?.Join(TimeSpan.FromSeconds(2));

                _isRunning = false;

                Console.WriteLine("[Service] Shutdown complete");
            }
        }

        private void MonitorLoop()
        {
            while (!_monitorCts.Token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(2000);

                    if (_monitorCts.Token.IsCancellationRequested) break;

                    var cacheStats = _cache.GetStatistics();
                    var aggStats = _aggregator.GetStatistics();

                    Console.WriteLine($"\n═══ RATE STREAMING STATS ═══");
                    Console.WriteLine($"Cache: {cacheStats}");
                    Console.WriteLine($"Aggregator: {aggStats}");

                    // Print sample rates
                    Console.WriteLine("\nSample Rates:");
                    foreach (var pair in new[] { "EUR/USD", "GBP/USD", "USD/JPY" })
                    {
                        var rate = _cache.GetRate(pair);
                        if (rate != null)
                        {
                            var staleIndicator = rate.IsStale(TimeSpan.FromSeconds(1)) ? " [STALE]" : "";
                            Console.WriteLine($"  {rate}{staleIndicator}");
                        }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
            }
        }

        private void CleanupLoop()
        {
            while (!_monitorCts.Token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(5000);

                    if (_monitorCts.Token.IsCancellationRequested) break;

                    int cleaned = _cache.ClearStaleRates();
                    if (cleaned > 0)
                    {
                        Console.WriteLine($"[Cleanup] Removed {cleaned} stale rates");
                    }
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
            }
        }

        private void OnRateUpdated(object sender, RateUpdateEventArgs e)
        {
            // Handle rate update events (could publish to clients, log, etc.)
            // This runs on the thread that updated the cache
        }

        public void Dispose()
        {
            Stop();

            foreach (var provider in _providers)
            {
                provider.Dispose();
            }

            _aggregator.Dispose();
            _cache.Dispose();
            _rateBuffer.Dispose();
            _monitorCts.Dispose();
        }
    }
}
```

### 6. Main Demo Program

```csharp
using System;
using System.Threading;
using FXRateStreaming.Services;

namespace FXRateStreaming
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      THREAD-SAFE FX RATE STREAMING DEMONSTRATION             ║");
            Console.WriteLine("║                                                              ║");
            Console.WriteLine("║  This demo shows:                                            ║");
            Console.WriteLine("║  • Thread-safe rate cache (ConcurrentDictionary)             ║");
            Console.WriteLine("║  • Producer-Consumer pattern (BlockingCollection)            ║");
            Console.WriteLine("║  • Lock-free statistics (Interlocked)                        ║");
            Console.WriteLine("║  • ReaderWriterLockSlim for complex operations               ║");
            Console.WriteLine("║  • Background threads for monitoring and cleanup             ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            using var service = new RateStreamingService();

            // Add multiple rate providers (simulating Reuters, Bloomberg, EBS)
            service.AddProvider("Reuters");
            service.AddProvider("Bloomberg");
            service.AddProvider("EBS");

            // Start the service
            service.Start();

            Console.WriteLine("Press Enter to stop the service...\n");
            Console.ReadLine();

            // Stop gracefully
            service.Stop();

            // Final statistics
            var finalStats = service.Cache.GetStatistics();
            Console.WriteLine($"\nFinal Statistics: {finalStats}");
        }
    }
}
```

---

## Thread Safety Techniques Used

| Technique              | Where Used         | Purpose                         |
| ---------------------- | ------------------ | ------------------------------- |
| `ConcurrentDictionary` | Rate Cache         | Thread-safe rate storage        |
| `BlockingCollection`   | Rate Buffer        | Producer-consumer communication |
| `Interlocked`          | Statistics         | Lock-free counter updates       |
| `ReaderWriterLockSlim` | Complex Operations | Read-heavy optimization         |
| `volatile`             | State flags        | Visibility across threads       |
| Immutable objects      | FxRate class       | Inherent thread safety          |
| Lock object            | State management   | Coordinated state changes       |

---

## Interview Talking Points

1. **Why ConcurrentDictionary instead of Dictionary + lock?**
   - Fine-grained locking (per bucket, not entire dictionary)
   - Atomic compound operations (AddOrUpdate, GetOrAdd)
   - Better performance under high contention

2. **Why BlockingCollection for producer-consumer?**
   - Built-in blocking when empty/full
   - Thread-safe by design
   - Supports bounded capacity (back-pressure)
   - GetConsumingEnumerable for clean consumer loops

3. **Why Interlocked for statistics?**
   - Lock-free = better performance
   - Atomic operations guaranteed
   - No deadlock risk

4. **Why immutable FxRate?**
   - No synchronization needed for reads
   - Safe to share across threads
   - No defensive copying required

---

_Document created for interview preparation. February 2026_
