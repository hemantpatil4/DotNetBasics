/*
╔══════════════════════════════════════════════════════════════════════════════╗
║           THREAD-SAFE FX RATE STREAMING - RUNNABLE DEMO                       ║
║                                                                               ║
║  Complete implementation of a thread-safe FX rate streaming system            ║
║  demonstrating real-world multithreading patterns.                            ║
║                                                                               ║
║  Run with: dotnet run                                                         ║
╚══════════════════════════════════════════════════════════════════════════════╝
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FXRateStreamingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          THREAD-SAFE FX RATE STREAMING DEMONSTRATION                 ║");
            Console.WriteLine("║                                                                      ║");
            Console.WriteLine("║  This demo shows a production-style FX rate streaming system:        ║");
            Console.WriteLine("║  • Thread-safe rate cache (ConcurrentDictionary)                     ║");
            Console.WriteLine("║  • Producer-Consumer pattern (BlockingCollection)                    ║");
            Console.WriteLine("║  • Lock-free statistics (Interlocked)                                ║");
            Console.WriteLine("║  • ReaderWriterLockSlim for complex operations                       ║");
            Console.WriteLine("║  • Background monitoring and stale rate cleanup                      ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════════════╝\n");
            
            using var service = new RateStreamingService();
            
            // Add multiple rate providers (simulating Reuters, Bloomberg, EBS)
            service.AddProvider("Reuters", new[] { "EUR/USD", "GBP/USD", "USD/JPY", "AUD/USD" });
            service.AddProvider("Bloomberg", new[] { "EUR/USD", "GBP/USD", "USD/CHF", "USD/CAD" });
            service.AddProvider("EBS", new[] { "EUR/USD", "USD/JPY", "EUR/GBP" });
            
            // Start the service
            service.Start();
            
            Console.WriteLine("\nPress Enter to stop the service...\n");
            Console.ReadLine();
            
            // Stop gracefully
            service.Stop();
            
            // Final statistics
            var finalStats = service.Cache.GetStatistics();
            Console.WriteLine($"\n{'═',70}");
            Console.WriteLine($"FINAL STATISTICS:");
            Console.WriteLine($"  {finalStats}");
            Console.WriteLine($"{'═',70}");
        }
    }
    
    #region Immutable Rate Model
    
    /// <summary>
    /// Immutable FX Rate - inherently thread-safe.
    /// Once created, cannot be modified - safe to share across threads.
    /// </summary>
    public sealed class FxRate
    {
        public string CurrencyPair { get; }
        public decimal Bid { get; }
        public decimal Ask { get; }
        public decimal Mid => (Bid + Ask) / 2;
        public decimal Spread => Ask - Bid;
        public decimal SpreadBps => Spread / Mid * 10000;  // Spread in basis points
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
        }
        
        public bool IsStale(TimeSpan maxAge) => DateTime.UtcNow - Timestamp > maxAge;
        
        public override string ToString() => 
            $"{CurrencyPair}: {Bid:F5}/{Ask:F5} (Spread: {SpreadBps:F1}bps) [{Provider}]";
    }
    
    public sealed class RateUpdateEventArgs : EventArgs
    {
        public FxRate? OldRate { get; }
        public FxRate? NewRate { get; }
        public RateUpdateType UpdateType { get; }
        
        public RateUpdateEventArgs(FxRate? oldRate, FxRate? newRate, RateUpdateType updateType)
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
    
    #endregion
    
    #region Thread-Safe Rate Cache
    
    /// <summary>
    /// Thread-safe cache for FX rates using:
    /// - ConcurrentDictionary for storage
    /// - ReaderWriterLockSlim for complex operations
    /// - Interlocked for statistics
    /// </summary>
    public sealed class ThreadSafeRateCache : IDisposable
    {
        private readonly ConcurrentDictionary<string, FxRate> _rates;
        private readonly ReaderWriterLockSlim _complexOpLock;
        private readonly TimeSpan _staleThreshold;
        
        // Lock-free statistics
        private long _totalUpdates;
        private long _totalReads;
        private long _staleRateCount;
        
        public event EventHandler<RateUpdateEventArgs>? RateUpdated;
        
        public ThreadSafeRateCache(TimeSpan? staleThreshold = null)
        {
            _rates = new ConcurrentDictionary<string, FxRate>(StringComparer.OrdinalIgnoreCase);
            _complexOpLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _staleThreshold = staleThreshold ?? TimeSpan.FromSeconds(5);
        }
        
        #region Basic Operations (Thread-Safe)
        
        public FxRate? GetRate(string currencyPair)
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
        
        public void UpdateRate(FxRate newRate)
        {
            if (newRate == null) throw new ArgumentNullException(nameof(newRate));
            
            FxRate? oldRate = null;
            var updateType = RateUpdateType.New;
            
            _rates.AddOrUpdate(
                newRate.CurrencyPair,
                addValueFactory: key => newRate,
                updateValueFactory: (key, existing) =>
                {
                    oldRate = existing;
                    // Only update if newer
                    if (newRate.SequenceNumber > existing.SequenceNumber)
                    {
                        updateType = RateUpdateType.Updated;
                        return newRate;
                    }
                    return existing;
                });
            
            Interlocked.Increment(ref _totalUpdates);
            
            // Only raise event if actually updated
            if (oldRate == null || updateType == RateUpdateType.Updated)
            {
                OnRateUpdated(new RateUpdateEventArgs(oldRate, newRate, updateType));
            }
        }
        
        public bool RemoveRate(string currencyPair)
        {
            if (_rates.TryRemove(currencyPair, out var removed))
            {
                OnRateUpdated(new RateUpdateEventArgs(removed, null, RateUpdateType.Removed));
                return true;
            }
            return false;
        }
        
        public IReadOnlyList<FxRate> GetAllRates()
        {
            Interlocked.Increment(ref _totalReads);
            return _rates.Values.ToList();
        }
        
        public int Count => _rates.Count;
        
        #endregion
        
        #region Complex Operations (Need Locking)
        
        public FxRate? GetBestBid(string baseCurrency)
        {
            _complexOpLock.EnterReadLock();
            try
            {
                return _rates.Values
                    .Where(r => r.CurrencyPair.StartsWith(baseCurrency + "/") && !r.IsStale(_staleThreshold))
                    .OrderByDescending(r => r.Bid)
                    .FirstOrDefault();
            }
            finally
            {
                _complexOpLock.ExitReadLock();
            }
        }
        
        public IReadOnlyList<FxRate> GetRatesWithTightSpread(decimal maxSpreadBps)
        {
            _complexOpLock.EnterReadLock();
            try
            {
                return _rates.Values
                    .Where(r => r.SpreadBps <= maxSpreadBps && !r.IsStale(_staleThreshold))
                    .ToList();
            }
            finally
            {
                _complexOpLock.ExitReadLock();
            }
        }
        
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
        
        #region Statistics
        
        public RateCacheStatistics GetStatistics()
        {
            return new RateCacheStatistics
            {
                TotalRates = _rates.Count,
                TotalUpdates = Interlocked.Read(ref _totalUpdates),
                TotalReads = Interlocked.Read(ref _totalReads),
                StaleRateAccessCount = Interlocked.Read(ref _staleRateCount),
                ActiveRates = _rates.Values.Count(r => !r.IsStale(_staleThreshold)),
                StaleRates = _rates.Values.Count(r => r.IsStale(_staleThreshold))
            };
        }
        
        #endregion
        
        private void OnRateUpdated(RateUpdateEventArgs args)
        {
            var handler = Volatile.Read(ref RateUpdated);
            handler?.Invoke(this, args);
        }
        
        public void Dispose()
        {
            _complexOpLock?.Dispose();
        }
    }
    
    public class RateCacheStatistics
    {
        public int TotalRates { get; init; }
        public long TotalUpdates { get; init; }
        public long TotalReads { get; init; }
        public long StaleRateAccessCount { get; init; }
        public int ActiveRates { get; init; }
        public int StaleRates { get; init; }
        
        public override string ToString() =>
            $"Rates: {TotalRates} (Active: {ActiveRates}, Stale: {StaleRates}) | " +
            $"Updates: {TotalUpdates:N0} | Reads: {TotalReads:N0}";
    }
    
    #endregion
    
    #region Rate Provider (Producer)
    
    /// <summary>
    /// Simulates a rate feed provider (e.g., Reuters, Bloomberg).
    /// Produces rates on its own thread and publishes to a buffer.
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
        private readonly object _stateLock = new();
        
        // Base rates for simulation
        private readonly Dictionary<string, (decimal Bid, decimal Ask)> _baseRates = new()
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
        
        public string ProviderName => _providerName;
        public bool IsRunning => _isRunning;
        
        public RateProvider(string providerName, BlockingCollection<FxRate> rateBuffer, string[] currencyPairs)
        {
            _providerName = providerName;
            _rateBuffer = rateBuffer;
            _currencyPairs = currencyPairs;
            _random = new Random(providerName.GetHashCode());
            _cts = new CancellationTokenSource();
            
            _producerThread = new Thread(ProducerLoop)
            {
                Name = $"{providerName}_Producer",
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
            }
        }
        
        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isRunning) return;
                _cts.Cancel();
                _producerThread.Join(TimeSpan.FromSeconds(3));
                _isRunning = false;
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
                        
                        if (!_rateBuffer.TryAdd(rate, 50, _cts.Token))
                        {
                            // Buffer full - rate dropped (back-pressure)
                        }
                    }
                    
                    Thread.Sleep(_random.Next(20, 80));  // Random refresh interval
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{_providerName}] Error: {ex.Message}");
                    Thread.Sleep(1000);
                }
            }
        }
        
        private FxRate GenerateRate(string pair)
        {
            var seq = Interlocked.Increment(ref _sequenceNumber);
            
            if (_baseRates.TryGetValue(pair, out var baseRate))
            {
                var variation = (decimal)(_random.NextDouble() - 0.5) * 0.001m;
                var bid = baseRate.Bid + variation;
                var spread = baseRate.Ask - baseRate.Bid;
                var ask = bid + spread;
                
                return new FxRate(pair, bid, ask, _providerName, seq);
            }
            
            return new FxRate(pair, 1.0m, 1.0001m, _providerName, seq);
        }
        
        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
    
    #endregion
    
    #region Rate Aggregator (Consumer)
    
    /// <summary>
    /// Consumes rates from buffer and updates the cache.
    /// Implements producer-consumer pattern.
    /// </summary>
    public class RateAggregator : IDisposable
    {
        private readonly ThreadSafeRateCache _cache;
        private readonly BlockingCollection<FxRate> _rateBuffer;
        private readonly CancellationTokenSource _cts;
        private readonly List<Thread> _consumerThreads;
        private readonly int _consumerCount;
        
        private long _processedCount;
        private bool _isRunning;
        private readonly object _stateLock = new();
        
        public RateAggregator(ThreadSafeRateCache cache, BlockingCollection<FxRate> rateBuffer, int consumerCount = 2)
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
            }
        }
        
        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isRunning) return;
                _cts.Cancel();
                
                foreach (var thread in _consumerThreads)
                {
                    thread.Join(TimeSpan.FromSeconds(3));
                }
                _consumerThreads.Clear();
                _isRunning = false;
            }
        }
        
        private void ConsumerLoop()
        {
            try
            {
                foreach (var rate in _rateBuffer.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        _cache.UpdateRate(rate);
                        Interlocked.Increment(ref _processedCount);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Consumer] Error: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown
            }
        }
        
        public long ProcessedCount => Interlocked.Read(ref _processedCount);
        
        public void Dispose()
        {
            Stop();
            _cts.Dispose();
        }
    }
    
    #endregion
    
    #region Rate Streaming Service (Orchestrator)
    
    /// <summary>
    /// Main orchestrator for the rate streaming system.
    /// Coordinates providers, aggregator, cache, and background tasks.
    /// </summary>
    public class RateStreamingService : IDisposable
    {
        private readonly ThreadSafeRateCache _cache;
        private readonly BlockingCollection<FxRate> _rateBuffer;
        private readonly List<RateProvider> _providers;
        private readonly RateAggregator _aggregator;
        private readonly CancellationTokenSource _monitorCts;
        
        private Thread? _monitorThread;
        private Thread? _cleanupThread;
        private bool _isRunning;
        private readonly object _stateLock = new();
        
        public ThreadSafeRateCache Cache => _cache;
        public bool IsRunning => _isRunning;
        
        public RateStreamingService(int bufferSize = 5000)
        {
            _cache = new ThreadSafeRateCache(TimeSpan.FromSeconds(5));
            _rateBuffer = new BlockingCollection<FxRate>(bufferSize);
            _providers = new List<RateProvider>();
            _aggregator = new RateAggregator(_cache, _rateBuffer, consumerCount: 2);
            _monitorCts = new CancellationTokenSource();
        }
        
        public void AddProvider(string name, string[] pairs)
        {
            var provider = new RateProvider(name, _rateBuffer, pairs);
            _providers.Add(provider);
            Console.WriteLine($"[Service] Added provider: {name} for {string.Join(", ", pairs)}");
        }
        
        public void Start()
        {
            lock (_stateLock)
            {
                if (_isRunning) return;
                _isRunning = true;
                
                Console.WriteLine("\n[Service] Starting rate streaming service...");
                
                // Start aggregator (consumers) first
                _aggregator.Start();
                Console.WriteLine("[Service] Aggregator started with 2 consumer threads");
                
                // Start all providers (producers)
                foreach (var provider in _providers)
                {
                    provider.Start();
                }
                Console.WriteLine($"[Service] Started {_providers.Count} rate providers");
                
                // Start monitoring
                _monitorThread = new Thread(MonitorLoop) { Name = "Monitor", IsBackground = true };
                _monitorThread.Start();
                
                // Start cleanup
                _cleanupThread = new Thread(CleanupLoop) { Name = "Cleanup", IsBackground = true };
                _cleanupThread.Start();
                
                Console.WriteLine("[Service] Background monitoring started\n");
            }
        }
        
        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isRunning) return;
                
                Console.WriteLine("\n[Service] Stopping rate streaming service...");
                _monitorCts.Cancel();
                
                foreach (var provider in _providers)
                {
                    provider.Stop();
                }
                Console.WriteLine("[Service] Providers stopped");
                
                _rateBuffer.CompleteAdding();
                _aggregator.Stop();
                Console.WriteLine("[Service] Aggregator stopped");
                
                _monitorThread?.Join(TimeSpan.FromSeconds(2));
                _cleanupThread?.Join(TimeSpan.FromSeconds(2));
                
                _isRunning = false;
                Console.WriteLine("[Service] Service stopped successfully");
            }
        }
        
        private void MonitorLoop()
        {
            int iteration = 0;
            while (!_monitorCts.Token.IsCancellationRequested)
            {
                try
                {
                    Thread.Sleep(3000);
                    if (_monitorCts.Token.IsCancellationRequested) break;
                    
                    iteration++;
                    var stats = _cache.GetStatistics();
                    
                    Console.WriteLine($"\n╔═══ MONITORING UPDATE #{iteration} ═══╗");
                    Console.WriteLine($"  {stats}");
                    Console.WriteLine($"  Buffer: {_rateBuffer.Count} pending");
                    Console.WriteLine($"  Processed: {_aggregator.ProcessedCount:N0} total");
                    
                    // Sample rates
                    Console.WriteLine("\n  Sample Rates:");
                    foreach (var pair in new[] { "EUR/USD", "GBP/USD", "USD/JPY" })
                    {
                        var rate = _cache.GetRate(pair);
                        if (rate != null)
                        {
                            var age = DateTime.UtcNow - rate.Timestamp;
                            var indicator = age > TimeSpan.FromSeconds(2) ? " ⚠️ STALE" : "";
                            Console.WriteLine($"    {rate}{indicator}");
                        }
                    }
                    Console.WriteLine("╚════════════════════════════════════╝");
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
                    Thread.Sleep(10000);  // Every 10 seconds
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
    
    #endregion
}
