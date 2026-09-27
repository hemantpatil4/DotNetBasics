using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OOPFundamentals.FXDomain
{
    /// <summary>
    /// Complete runnable demo of OOP concepts in FX Trading Domain
    /// Covers: Quote Books, Trade Parsing, Pre/Post Trade Workflows,
    /// CBS Data, Spread Management, Rule-Based Systems
    /// </summary>
    class FXDomainDemo
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     OOP IN FX TRADING DOMAIN - COMPREHENSIVE DEMO            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝\n");

            // Demo 1: Encapsulation - FX Quote with Price Protection
            Demo1_Encapsulation_FxQuote();

            // Demo 2: Encapsulation - Trade State Management
            Demo2_Encapsulation_TradeState();

            // Demo 3: Encapsulation - Customer Spread Configuration
            Demo3_Encapsulation_SpreadConfig();

            // Demo 4: Abstraction - Market Data Providers
            await Demo4_Abstraction_MarketData();

            // Demo 5: Abstraction - Trade Message Parsing
            Demo5_Abstraction_TradeParsing();

            // Demo 6: Polymorphism - Trade Validation Rules
            Demo6_Polymorphism_ValidationRules();

            // Demo 7: Polymorphism - Customer Pricing Strategies
            Demo7_Polymorphism_PricingStrategies();

            // Demo 8: Polymorphism - Rule-Based Spread Management
            Demo8_Polymorphism_SpreadRules();

            // Demo 9: Inheritance - FX Trade Hierarchy
            Demo9_Inheritance_TradeTypes();

            // Demo 10: Inheritance - Pre/Post Trade Workflows
            await Demo10_Inheritance_Workflows();

            // Demo 11: Complete System - Quote Book
            Demo11_Complete_QuoteBook();

            Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    DEMO COMPLETED!                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        }

        // ============================================================
        // DEMO 1: ENCAPSULATION - FX Quote with Price Protection
        // ============================================================
        static void Demo1_Encapsulation_FxQuote()
        {
            PrintHeader("DEMO 1: Encapsulation - FX Quote Price Protection");

            var quote = new FxQuote("Q001", "EURUSD", "Reuters", 1.0850m, 1.0852m);
            Console.WriteLine($"  Created Quote: {quote}");
            Console.WriteLine($"  Spread: {quote.Spread:F5} ({quote.SpreadInPips:F1} pips)");
            Console.WriteLine($"  Mid Price: {quote.MidPrice:F5}");

            // Valid update
            quote.UpdatePrices(1.0851m, 1.0853m);
            Console.WriteLine($"\n  After update: {quote}");

            // Try invalid operations
            Console.WriteLine("\n  Testing validation:");

            try
            {
                quote.UpdatePrices(1.0855m, 1.0850m); // Ask < Bid
            }
            catch (InvalidQuoteException ex)
            {
                Console.WriteLine($"  ❌ Ask < Bid rejected: {ex.Message}");
            }

            try
            {
                quote.UpdatePrices(-1.0m, 1.0852m); // Negative price
            }
            catch (InvalidQuoteException ex)
            {
                Console.WriteLine($"  ❌ Negative price rejected: {ex.Message}");
            }

            try
            {
                quote.UpdatePrices(1.0850m, 1.0950m); // Spread too wide
            }
            catch (InvalidQuoteException ex)
            {
                Console.WriteLine($"  ❌ Wide spread rejected: {ex.Message}");
            }

            Console.WriteLine("\n  ✅ Encapsulation protects quote integrity!");
            PrintFooter();
        }

        // ============================================================
        // DEMO 2: ENCAPSULATION - Trade State Management
        // ============================================================
        static void Demo2_Encapsulation_TradeState()
        {
            PrintHeader("DEMO 2: Encapsulation - Trade State Machine");

            var trade = new FxTrade("T001", "EURUSD", 1_000_000m, TradeSide.Buy,
                                    1.0850m, "CUST001", DateTime.Today.AddDays(2));

            Console.WriteLine($"  Created trade: {trade}");
            Console.WriteLine($"  Initial status: {trade.Status}");

            // Valid transitions
            trade.TransitionTo(TradeStatus.Confirmed, "Trade matched", "TRADER1");
            Console.WriteLine($"  After confirm: {trade.Status}");

            trade.TransitionTo(TradeStatus.Matched, "Counterparty confirmed", "SYSTEM");
            Console.WriteLine($"  After match: {trade.Status}");

            // Try invalid transition
            try
            {
                trade.TransitionTo(TradeStatus.Pending, "Go back", "TRADER1");
            }
            catch (InvalidTradeTransitionException ex)
            {
                Console.WriteLine($"\n  ❌ Invalid transition rejected: {ex.Message}");
            }

            // Show audit trail
            Console.WriteLine("\n  Audit Trail:");
            foreach (var change in trade.StatusHistory)
            {
                Console.WriteLine($"    [{change.Timestamp:HH:mm:ss}] {change.NewStatus} - {change.Reason} ({change.UserId})");
            }

            PrintFooter();
        }

        // ============================================================
        // DEMO 3: ENCAPSULATION - Customer Spread Configuration
        // ============================================================
        static void Demo3_Encapsulation_SpreadConfig()
        {
            PrintHeader("DEMO 3: Encapsulation - Customer Spread Config");

            var config = new CustomerSpreadConfig("CUST001", defaultMarkup: 0.0002m);
            Console.WriteLine($"  Customer: {config.CustomerId}");
            Console.WriteLine($"  Default markup: {config.DefaultMarkup * 10000:F1} pips");

            // Set pair-specific markups
            config.SetPairMarkup("EURUSD", 0.0001m); // Tighter for liquid pair
            config.SetPairMarkup("USDTRY", 0.0005m); // Wider for exotic

            Console.WriteLine("\n  Pair-specific markups:");
            Console.WriteLine($"    EURUSD: {config.GetMarkupForPair("EURUSD") * 10000:F1} pips");
            Console.WriteLine($"    USDTRY: {config.GetMarkupForPair("USDTRY") * 10000:F1} pips");
            Console.WriteLine($"    GBPUSD: {config.GetMarkupForPair("GBPUSD") * 10000:F1} pips (default)");

            // Apply to quote
            var marketQuote = new FxQuote("Q001", "EURUSD", "Market", 1.0850m, 1.0851m);
            var (customerBid, customerAsk) = config.ApplyMarkup(marketQuote);

            Console.WriteLine($"\n  Market quote: {marketQuote.BidPrice} / {marketQuote.AskPrice}");
            Console.WriteLine($"  Customer quote: {customerBid:F5} / {customerAsk:F5}");

            // Try invalid markup
            try
            {
                config.SetPairMarkup("USDJPY", 0.01m); // Too wide
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"\n  ❌ Excessive markup rejected: {ex.Message}");
            }

            PrintFooter();
        }

        // ============================================================
        // DEMO 4: ABSTRACTION - Market Data Providers
        // ============================================================
        static async Task Demo4_Abstraction_MarketData()
        {
            PrintHeader("DEMO 4: Abstraction - Market Data Providers");

            // Different implementations, same interface
            IMarketDataProvider reuters = new ReutersMarketDataProvider();
            IMarketDataProvider bloomberg = new BloombergMarketDataProvider();
            IMarketDataProvider cached = new CachedMarketDataProvider();

            Console.WriteLine("  Getting EURUSD quotes from different providers:\n");

            var providers = new[] { reuters, bloomberg, cached };
            foreach (var provider in providers)
            {
                var quote = await provider.GetQuoteAsync("EURUSD");
                Console.WriteLine($"    {provider.GetType().Name,-30} → {quote}");
            }

            Console.WriteLine("\n  ✅ Same interface, different implementations!");
            Console.WriteLine("  ✅ Consumer code doesn't change when adding new LP");

            PrintFooter();
        }

        // ============================================================
        // DEMO 5: ABSTRACTION - Trade Message Parsing
        // ============================================================
        static void Demo5_Abstraction_TradeParsing()
        {
            PrintHeader("DEMO 5: Abstraction - Trade Message Parsing");

            // Different message formats
            var fixMessage = "8=FIX.4.4|35=8|17=EXEC001|55=EURUSD|32=1000000|54=1|31=1.0850|49=BANK1|64=20260215|";
            var jsonMessage = "{\"tradeId\":\"T002\",\"pair\":\"GBPUSD\",\"amount\":500000,\"side\":\"sell\",\"rate\":1.2700}";
            var fpmlMessage = "<FpML><fxSingleLeg><tradeId>T003</tradeId><pair>USDJPY</pair></fxSingleLeg></FpML>";

            // Parsers
            var parsers = new ITradeMessageParser[]
            {
                new FixTradeParser(),
                new JsonTradeParser(),
                new FpmlTradeParser()
            };

            var service = new TradeIngestionService(parsers);

            Console.WriteLine("  Parsing different message formats:\n");

            var messages = new[] { fixMessage, jsonMessage, fpmlMessage };
            foreach (var msg in messages)
            {
                var trade = service.ProcessMessage(msg);
                Console.WriteLine($"    Format: {trade.SourceFormat,-6} → {trade}");
            }

            Console.WriteLine("\n  ✅ Single API parses any format!");
            Console.WriteLine("  ✅ Adding new format = just add new parser");

            PrintFooter();
        }

        // ============================================================
        // DEMO 6: POLYMORPHISM - Trade Validation Rules
        // ============================================================
        static void Demo6_Polymorphism_ValidationRules()
        {
            PrintHeader("DEMO 6: Polymorphism - Trade Validation Rules");

            // Different validation rules
            var rules = new ITradeValidationRule[]
            {
                new NotionalLimitRule(5_000_000m),
                new CurrencyPairRule(new[] { "EURUSD", "GBPUSD", "USDJPY" }),
                new ValueDateRule(),
                new SanctionsRule(new[] { "BLOCKED001", "BLOCKED002" })
            };

            var engine = new TradeValidationEngine(rules);

            Console.WriteLine("  Validating trades with polymorphic rules:\n");

            // Test trades
            var trades = new[]
            {
                new FxTrade("T1", "EURUSD", 1_000_000m, TradeSide.Buy, 1.0850m, "CUST001", DateTime.Today.AddDays(2)),
                new FxTrade("T2", "EURUSD", 10_000_000m, TradeSide.Buy, 1.0850m, "CUST001", DateTime.Today.AddDays(2)),
                new FxTrade("T3", "USDCNY", 1_000_000m, TradeSide.Buy, 7.20m, "CUST001", DateTime.Today.AddDays(2)),
                new FxTrade("T4", "EURUSD", 1_000_000m, TradeSide.Buy, 1.0850m, "BLOCKED001", DateTime.Today.AddDays(2))
            };

            foreach (var trade in trades)
            {
                var result = engine.ValidateTrade(trade);
                var status = result.IsValid ? "✅ PASS" : "❌ FAIL";
                Console.WriteLine($"    {trade.TradeId}: {status}");
                if (!result.IsValid)
                {
                    foreach (var error in result.Errors)
                        Console.WriteLine($"       └─ {error}");
                }
            }

            Console.WriteLine("\n  ✅ Adding new rule = just implement ITradeValidationRule");
            PrintFooter();
        }

        // ============================================================
        // DEMO 7: POLYMORPHISM - Customer Pricing Strategies
        // ============================================================
        static void Demo7_Polymorphism_PricingStrategies()
        {
            PrintHeader("DEMO 7: Polymorphism - Customer Pricing Strategies");

            var marketQuote = new FxQuote("Q001", "EURUSD", "Market", 1.0850m, 1.0851m);
            Console.WriteLine($"  Market quote: {marketQuote.BidPrice} / {marketQuote.AskPrice}");
            Console.WriteLine($"  Market spread: {marketQuote.SpreadInPips:F1} pips\n");

            // Different pricing strategies
            var strategies = new PricingStrategy[]
            {
                new RetailPricingStrategy(),
                new CorporatePricingStrategy(50_000_000m), // 50M monthly volume
                new InstitutionalPricingStrategy(0.00005m) // Negotiated 0.5 pip markup
            };

            Console.WriteLine("  Customer quotes by tier:\n");
            foreach (var strategy in strategies)
            {
                var customerQuote = strategy.ApplyPricing(marketQuote);
                Console.WriteLine($"    {strategy.StrategyName,-15}: {customerQuote.BidPrice:F5} / {customerQuote.AskPrice:F5} " +
                                  $"(spread: {customerQuote.SpreadInPips:F1} pips)");
            }

            Console.WriteLine("\n  ✅ Same method call, different pricing logic!");
            PrintFooter();
        }

        // ============================================================
        // DEMO 8: POLYMORPHISM - Rule-Based Spread Management
        // ============================================================
        static void Demo8_Polymorphism_SpreadRules()
        {
            PrintHeader("DEMO 8: Polymorphism - Rule-Based Spread Management");

            var rules = new ISpreadRule[]
            {
                new BaseSpreadRule(),
                new VolatilitySpreadRule(),
                new TimeOfDaySpreadRule(),
                new LiquiditySpreadRule()
            };

            var engine = new SpreadEngine(rules);

            // Test different market conditions
            var scenarios = new[]
            {
                new SpreadContext("EURUSD", 0.01m, CustomerTier.Standard, 1000000),  // Normal
                new SpreadContext("EURUSD", 0.05m, CustomerTier.Standard, 1000000),  // High vol
                new SpreadContext("USDJPY", 0.01m, CustomerTier.Premium, 1000000),   // Premium customer
                new SpreadContext("USDTRY", 0.03m, CustomerTier.Standard, 100000)    // Low liquidity
            };

            Console.WriteLine("  Spread calculation with multiple rules:\n");
            Console.WriteLine($"    {"Pair",-8} {"Vol",-8} {"Tier",-10} {"Amount",-12} {"Spread (pips)",-15} Applied Rules");
            Console.WriteLine($"    {new string('-', 75)}");

            foreach (var ctx in scenarios)
            {
                var (spread, appliedRules) = engine.CalculateSpreadWithDetails(ctx);
                Console.WriteLine($"    {ctx.CurrencyPair,-8} {ctx.Volatility:P1,-8} {ctx.CustomerTier,-10} " +
                                  $"{ctx.Amount:N0,-12} {spread * 10000:F1,-15} {string.Join(", ", appliedRules)}");
            }

            Console.WriteLine("\n  ✅ Rules applied polymorphically based on conditions");
            PrintFooter();
        }

        // ============================================================
        // DEMO 9: INHERITANCE - FX Trade Type Hierarchy
        // ============================================================
        static void Demo9_Inheritance_TradeTypes()
        {
            PrintHeader("DEMO 9: Inheritance - FX Trade Type Hierarchy");

            // Create different trade types
            var spot = new FxSpotTrade("SPOT001", "EURUSD", TradeSide.Buy, 1_000_000m, "CUST01", 1.0850m);
            var forward = new FxForwardTrade("FWD001", "GBPUSD", TradeSide.Sell, 500_000m, "CUST02",
                                              1.2700m, 1.2720m, DateTime.Today.AddMonths(3));
            var swap = new FxSwapTrade("SWAP001", "USDJPY", TradeSide.Buy, 2_000_000m, "CUST01",
                                        150.50m, 150.75m, DateTime.Today.AddMonths(1));

            // Polymorphic collection
            List<FxTradeBase> portfolio = new() { spot, forward, swap };

            Console.WriteLine("  Portfolio of different trade types:\n");
            foreach (var trade in portfolio)
            {
                Console.WriteLine($"    {trade.TradeId} ({trade.ProductType}):");
                Console.WriteLine($"      Description: {trade.GetTradeDescription()}");
                Console.WriteLine($"      Cash flows:");
                foreach (var cf in trade.GetCashFlows())
                    Console.WriteLine($"        {cf}");
                Console.WriteLine();
            }

            // Calculate portfolio risk
            Console.WriteLine("  Portfolio Risk (polymorphic calculation):");
            foreach (var trade in portfolio)
            {
                Console.WriteLine($"    {trade.TradeId}: Risk = {trade.CalculateRisk():N0}");
            }

            var totalRisk = portfolio.Sum(t => t.CalculateRisk());
            Console.WriteLine($"    TOTAL: {totalRisk:N0}");

            PrintFooter();
        }

        // ============================================================
        // DEMO 10: INHERITANCE - Pre/Post Trade Workflows
        // ============================================================
        static async Task Demo10_Inheritance_Workflows()
        {
            PrintHeader("DEMO 10: Inheritance - Pre/Post Trade Workflows");

            var trade = new FxTrade("T001", "EURUSD", 1_000_000m, TradeSide.Buy,
                                    1.0850m, "CUST001", DateTime.Today.AddDays(2));

            // Pre-trade steps
            var preTradeSteps = new WorkflowStep[]
            {
                new CreditCheckStep(),
                new ComplianceCheckStep(),
                new PriceValidationStep()
            };

            // Post-trade steps
            var postTradeSteps = new WorkflowStep[]
            {
                new ConfirmationStep(),
                new SettlementInstructionStep(),
                new RegulatoryReportingStep()
            };

            var engine = new TradeWorkflowEngine();
            var context = new WorkflowContext(trade);

            Console.WriteLine("  PRE-TRADE WORKFLOW:");
            Console.WriteLine("  " + new string('-', 50));
            var preResult = await engine.ExecuteWorkflowAsync(preTradeSteps, context);
            Console.WriteLine($"\n  Pre-trade result: {(preResult.Success ? "✅ PASSED" : "❌ FAILED")}\n");

            Console.WriteLine("  POST-TRADE WORKFLOW:");
            Console.WriteLine("  " + new string('-', 50));
            var postResult = await engine.ExecuteWorkflowAsync(postTradeSteps, context);
            Console.WriteLine($"\n  Post-trade result: {(postResult.Success ? "✅ PASSED" : "❌ FAILED")}");

            PrintFooter();
        }

        // ============================================================
        // DEMO 11: COMPLETE SYSTEM - Quote Book
        // ============================================================
        static void Demo11_Complete_QuoteBook()
        {
            PrintHeader("DEMO 11: Complete System - FX Quote Book");

            var quoteBook = new FxQuoteBook();

            // Simulate multiple LPs sending quotes
            var lps = new[] { "Reuters", "Bloomberg", "Barclays", "Citi" };
            var random = new Random(42);

            Console.WriteLine("  Adding quotes from multiple liquidity providers:\n");

            foreach (var lp in lps)
            {
                var baseRate = 1.0850m + (decimal)(random.NextDouble() * 0.001 - 0.0005);
                var spread = 0.0001m + (decimal)(random.NextDouble() * 0.0001);

                var quote = new FxQuote(
                    Guid.NewGuid().ToString(),
                    "EURUSD",
                    lp,
                    baseRate,
                    baseRate + spread
                );

                quoteBook.AddQuote(quote);
                Console.WriteLine($"    {lp,-12}: {quote.BidPrice:F5} / {quote.AskPrice:F5}");
            }

            // Get best quotes
            var (bestBid, bestAsk) = quoteBook.GetBestQuotes("EURUSD");

            Console.WriteLine("\n  Best executable prices:");
            Console.WriteLine($"    Best Bid: {bestBid?.BidPrice:F5} ({bestBid?.LiquidityProvider})");
            Console.WriteLine($"    Best Ask: {bestAsk?.AskPrice:F5} ({bestAsk?.LiquidityProvider})");

            // Get aggregated depth
            Console.WriteLine("\n  Order book depth:");
            var depth = quoteBook.GetDepth("EURUSD", 3);
            Console.WriteLine($"    {"Level",-6} {"Bid",-12} {"Ask",-12}");
            for (int i = 0; i < depth.Bids.Count; i++)
            {
                Console.WriteLine($"    {i + 1,-6} {depth.Bids[i].Price:F5,-12} {depth.Asks[i].Price:F5,-12}");
            }

            PrintFooter();
        }

        // ============================================================
        // Helper Methods
        // ============================================================
        static void PrintHeader(string title)
        {
            Console.WriteLine($"\n┌──────────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ {title,-60} │");
            Console.WriteLine($"└──────────────────────────────────────────────────────────────┘");
        }

        static void PrintFooter()
        {
            Console.WriteLine("─────────────────────────────────────────────────────────────────");
        }
    }

    // ================================================================
    // DOMAIN CLASSES
    // ================================================================

    #region Exceptions

    public class InvalidQuoteException : Exception
    {
        public InvalidQuoteException(string message) : base(message) { }
    }

    public class InvalidTradeTransitionException : Exception
    {
        public InvalidTradeTransitionException(string message) : base(message) { }
    }

    #endregion

    #region Enums

    public enum TradeSide { Buy, Sell }
    public enum TradeStatus { Pending, Confirmed, Rejected, Matched, Amended, Settled, Failed }
    public enum CustomerTier { Retail, Standard, Corporate, Premium, Institutional }

    #endregion

    #region FX Quote (Encapsulation Demo)

    public class FxQuote
    {
        private decimal _bidPrice;
        private decimal _askPrice;
        private readonly string _currencyPair;
        private DateTime _lastUpdatedAt;

        private const decimal MinSpread = 0.00001m;
        private const decimal MaxSpread = 0.01m;

        public string QuoteId { get; }
        public string CurrencyPair => _currencyPair;
        public string LiquidityProvider { get; }
        public decimal BidPrice => _bidPrice;
        public decimal AskPrice => _askPrice;
        public decimal Spread => _askPrice - _bidPrice;
        public decimal SpreadInPips => Spread * 10000;
        public decimal MidPrice => (_bidPrice + _askPrice) / 2;
        public DateTime LastUpdatedAt => _lastUpdatedAt;

        public FxQuote(string quoteId, string currencyPair, string lp, decimal bid, decimal ask)
        {
            QuoteId = quoteId;
            _currencyPair = currencyPair?.ToUpperInvariant() ?? throw new ArgumentNullException(nameof(currencyPair));
            LiquidityProvider = lp;
            UpdatePrices(bid, ask);
        }

        public void UpdatePrices(decimal newBid, decimal newAsk)
        {
            if (newBid <= 0 || newAsk <= 0)
                throw new InvalidQuoteException($"Prices must be positive. Bid: {newBid}, Ask: {newAsk}");

            if (newAsk <= newBid)
                throw new InvalidQuoteException($"Ask ({newAsk}) must be greater than Bid ({newBid})");

            decimal spread = newAsk - newBid;
            if (spread > MaxSpread)
                throw new InvalidQuoteException($"Spread {spread} exceeds maximum {MaxSpread}");

            _bidPrice = newBid;
            _askPrice = newAsk;
            _lastUpdatedAt = DateTime.UtcNow;
        }

        public override string ToString() => $"{CurrencyPair} {BidPrice:F5}/{AskPrice:F5} [{LiquidityProvider}]";
    }

    #endregion

    #region FX Trade (Encapsulation Demo)

    public class FxTrade
    {
        public string TradeId { get; }
        public string CurrencyPair { get; }
        public decimal Notional { get; }
        public TradeSide Side { get; }
        public decimal ExecutionRate { get; }
        public DateTime TradeDate { get; }
        public DateTime ValueDate { get; }
        public string CounterpartyId { get; }
        public string SourceFormat { get; set; } = "Internal";

        private TradeStatus _status;
        private readonly List<TradeStatusChange> _statusHistory;

        public TradeStatus Status => _status;
        public IReadOnlyList<TradeStatusChange> StatusHistory => _statusHistory.AsReadOnly();

        public FxTrade(string tradeId, string currencyPair, decimal notional,
                       TradeSide side, decimal rate, string counterpartyId, DateTime valueDate)
        {
            TradeId = tradeId;
            CurrencyPair = currencyPair;
            Notional = notional;
            Side = side;
            ExecutionRate = rate;
            CounterpartyId = counterpartyId;
            TradeDate = DateTime.UtcNow;
            ValueDate = valueDate;
            _status = TradeStatus.Pending;
            _statusHistory = new List<TradeStatusChange>
            {
                new(TradeStatus.Pending, "Trade created", "SYSTEM")
            };
        }

        public void TransitionTo(TradeStatus newStatus, string reason, string userId)
        {
            if (!IsValidTransition(_status, newStatus))
                throw new InvalidTradeTransitionException($"Cannot transition from {_status} to {newStatus}");

            _statusHistory.Add(new TradeStatusChange(newStatus, reason, userId));
            _status = newStatus;
        }

        private static bool IsValidTransition(TradeStatus from, TradeStatus to)
        {
            return (from, to) switch
            {
                (TradeStatus.Pending, TradeStatus.Confirmed) => true,
                (TradeStatus.Pending, TradeStatus.Rejected) => true,
                (TradeStatus.Confirmed, TradeStatus.Matched) => true,
                (TradeStatus.Matched, TradeStatus.Settled) => true,
                _ => false
            };
        }

        public override string ToString() =>
            $"{TradeId}: {Side} {Notional:N0} {CurrencyPair} @ {ExecutionRate:F5}";
    }

    public record TradeStatusChange(TradeStatus NewStatus, string Reason, string UserId)
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    #endregion

    #region Customer Spread Config (Encapsulation Demo)

    public class CustomerSpreadConfig
    {
        private readonly string _customerId;
        private readonly Dictionary<string, decimal> _pairMarkups;
        private decimal _defaultMarkup;
        private readonly decimal _maxMarkup;

        public string CustomerId => _customerId;
        public decimal DefaultMarkup => _defaultMarkup;

        public CustomerSpreadConfig(string customerId, decimal defaultMarkup, decimal maxMarkup = 0.001m)
        {
            _customerId = customerId;
            _maxMarkup = maxMarkup;
            _pairMarkups = new Dictionary<string, decimal>();
            SetDefaultMarkup(defaultMarkup);
        }

        public void SetDefaultMarkup(decimal markup)
        {
            ValidateMarkup(markup);
            _defaultMarkup = markup;
        }

        public void SetPairMarkup(string pair, decimal markup)
        {
            ValidateMarkup(markup);
            _pairMarkups[pair.ToUpperInvariant()] = markup;
        }

        public decimal GetMarkupForPair(string pair) =>
            _pairMarkups.TryGetValue(pair.ToUpperInvariant(), out var m) ? m : _defaultMarkup;

        public (decimal Bid, decimal Ask) ApplyMarkup(FxQuote quote)
        {
            var markup = GetMarkupForPair(quote.CurrencyPair);
            return (quote.BidPrice - markup / 2, quote.AskPrice + markup / 2);
        }

        private void ValidateMarkup(decimal markup)
        {
            if (markup < 0) throw new ArgumentException("Markup cannot be negative");
            if (markup > _maxMarkup) throw new ArgumentException($"Markup {markup} exceeds max {_maxMarkup}");
        }
    }

    #endregion

    #region Market Data Provider (Abstraction Demo)

    public interface IMarketDataProvider
    {
        Task<FxQuote> GetQuoteAsync(string currencyPair);
    }

    public class ReutersMarketDataProvider : IMarketDataProvider
    {
        public async Task<FxQuote> GetQuoteAsync(string currencyPair)
        {
            await Task.Delay(10); // Simulate network
            return new FxQuote(Guid.NewGuid().ToString(), currencyPair, "Reuters", 1.0850m, 1.0852m);
        }
    }

    public class BloombergMarketDataProvider : IMarketDataProvider
    {
        public async Task<FxQuote> GetQuoteAsync(string currencyPair)
        {
            await Task.Delay(15);
            return new FxQuote(Guid.NewGuid().ToString(), currencyPair, "Bloomberg", 1.0851m, 1.0853m);
        }
    }

    public class CachedMarketDataProvider : IMarketDataProvider
    {
        public async Task<FxQuote> GetQuoteAsync(string currencyPair)
        {
            await Task.Delay(1);
            return new FxQuote(Guid.NewGuid().ToString(), currencyPair, "Cache", 1.0849m, 1.0851m);
        }
    }

    #endregion

    #region Trade Message Parser (Abstraction Demo)

    public interface ITradeMessageParser
    {
        bool CanParse(string message);
        FxTrade Parse(string message);
    }

    public class FixTradeParser : ITradeMessageParser
    {
        public bool CanParse(string msg) => msg.StartsWith("8=FIX");

        public FxTrade Parse(string msg)
        {
            var fields = msg.Split('|').Where(f => f.Contains('='))
                .ToDictionary(f => f.Split('=')[0], f => f.Split('=')[1]);

            var trade = new FxTrade(
                fields.GetValueOrDefault("17", "FIX001"),
                fields.GetValueOrDefault("55", "EURUSD"),
                decimal.Parse(fields.GetValueOrDefault("32", "1000000")),
                fields.GetValueOrDefault("54", "1") == "1" ? TradeSide.Buy : TradeSide.Sell,
                decimal.Parse(fields.GetValueOrDefault("31", "1.0850")),
                fields.GetValueOrDefault("49", "UNKNOWN"),
                DateTime.Today.AddDays(2)
            );
            trade.SourceFormat = "FIX";
            return trade;
        }
    }

    public class JsonTradeParser : ITradeMessageParser
    {
        public bool CanParse(string msg) => msg.TrimStart().StartsWith("{");

        public FxTrade Parse(string msg)
        {
            // Simplified JSON parsing
            var trade = new FxTrade("JSON001", "GBPUSD", 500000, TradeSide.Sell, 1.2700m, "CUST01", DateTime.Today.AddDays(2));
            trade.SourceFormat = "JSON";
            return trade;
        }
    }

    public class FpmlTradeParser : ITradeMessageParser
    {
        public bool CanParse(string msg) => msg.Contains("<FpML");

        public FxTrade Parse(string msg)
        {
            var trade = new FxTrade("FPML001", "USDJPY", 2000000, TradeSide.Buy, 150.50m, "CUST02", DateTime.Today.AddDays(2));
            trade.SourceFormat = "FPML";
            return trade;
        }
    }

    public class TradeIngestionService
    {
        private readonly IEnumerable<ITradeMessageParser> _parsers;

        public TradeIngestionService(IEnumerable<ITradeMessageParser> parsers) => _parsers = parsers;

        public FxTrade ProcessMessage(string message)
        {
            var parser = _parsers.FirstOrDefault(p => p.CanParse(message))
                ?? throw new Exception("Unsupported message format");
            return parser.Parse(message);
        }
    }

    #endregion

    #region Trade Validation Rules (Polymorphism Demo)

    public interface ITradeValidationRule
    {
        string RuleName { get; }
        ValidationResult Validate(FxTrade trade);
    }

    public class ValidationResult
    {
        public bool IsValid { get; }
        public List<string> Errors { get; } = new();

        private ValidationResult(bool isValid) => IsValid = isValid;

        public static ValidationResult Pass() => new(true);
        public static ValidationResult Fail(string error) => new(false) { Errors = { error } };
    }

    public class ValidationSummary
    {
        public bool IsValid { get; }
        public List<string> Errors { get; }

        public ValidationSummary(IEnumerable<ValidationResult> results)
        {
            Errors = results.SelectMany(r => r.Errors).ToList();
            IsValid = !Errors.Any();
        }
    }

    public class NotionalLimitRule : ITradeValidationRule
    {
        private readonly decimal _maxNotional;
        public string RuleName => "NotionalLimit";

        public NotionalLimitRule(decimal max) => _maxNotional = max;

        public ValidationResult Validate(FxTrade trade) =>
            trade.Notional > _maxNotional
                ? ValidationResult.Fail($"Notional {trade.Notional:N0} exceeds limit {_maxNotional:N0}")
                : ValidationResult.Pass();
    }

    public class CurrencyPairRule : ITradeValidationRule
    {
        private readonly HashSet<string> _allowedPairs;
        public string RuleName => "CurrencyPair";

        public CurrencyPairRule(IEnumerable<string> allowed) => _allowedPairs = allowed.ToHashSet();

        public ValidationResult Validate(FxTrade trade) =>
            !_allowedPairs.Contains(trade.CurrencyPair)
                ? ValidationResult.Fail($"Currency pair {trade.CurrencyPair} not allowed")
                : ValidationResult.Pass();
    }

    public class ValueDateRule : ITradeValidationRule
    {
        public string RuleName => "ValueDate";

        public ValidationResult Validate(FxTrade trade) =>
            trade.ValueDate < DateTime.Today
                ? ValidationResult.Fail("Value date cannot be in the past")
                : ValidationResult.Pass();
    }

    public class SanctionsRule : ITradeValidationRule
    {
        private readonly HashSet<string> _blockedEntities;
        public string RuleName => "Sanctions";

        public SanctionsRule(IEnumerable<string> blocked) => _blockedEntities = blocked.ToHashSet();

        public ValidationResult Validate(FxTrade trade) =>
            _blockedEntities.Contains(trade.CounterpartyId)
                ? ValidationResult.Fail($"Counterparty {trade.CounterpartyId} is sanctioned")
                : ValidationResult.Pass();
    }

    public class TradeValidationEngine
    {
        private readonly IEnumerable<ITradeValidationRule> _rules;

        public TradeValidationEngine(IEnumerable<ITradeValidationRule> rules) => _rules = rules;

        public ValidationSummary ValidateTrade(FxTrade trade) =>
            new(_rules.Select(r => r.Validate(trade)));
    }

    #endregion

    #region Pricing Strategies (Polymorphism Demo)

    public abstract class PricingStrategy
    {
        public abstract string StrategyName { get; }
        public abstract FxQuote ApplyPricing(FxQuote marketQuote);
    }

    public class RetailPricingStrategy : PricingStrategy
    {
        public override string StrategyName => "Retail";

        public override FxQuote ApplyPricing(FxQuote q) =>
            new(Guid.NewGuid().ToString(), q.CurrencyPair, "Internal",
                q.BidPrice - 0.0005m, q.AskPrice + 0.0005m);
    }

    public class CorporatePricingStrategy : PricingStrategy
    {
        private readonly decimal _monthlyVolume;
        public override string StrategyName => "Corporate";

        public CorporatePricingStrategy(decimal volume) => _monthlyVolume = volume;

        public override FxQuote ApplyPricing(FxQuote q)
        {
            var markup = _monthlyVolume > 100_000_000 ? 0.0001m : _monthlyVolume > 10_000_000 ? 0.0002m : 0.0003m;
            return new(Guid.NewGuid().ToString(), q.CurrencyPair, "Internal", q.BidPrice - markup, q.AskPrice + markup);
        }
    }

    public class InstitutionalPricingStrategy : PricingStrategy
    {
        private readonly decimal _negotiatedMarkup;
        public override string StrategyName => "Institutional";

        public InstitutionalPricingStrategy(decimal markup) => _negotiatedMarkup = markup;

        public override FxQuote ApplyPricing(FxQuote q) =>
            new(Guid.NewGuid().ToString(), q.CurrencyPair, "Internal",
                q.BidPrice - _negotiatedMarkup, q.AskPrice + _negotiatedMarkup);
    }

    #endregion

    #region Spread Rules (Polymorphism Demo)

    public record SpreadContext(string CurrencyPair, decimal Volatility, CustomerTier CustomerTier, decimal Amount);

    public interface ISpreadRule
    {
        string RuleName { get; }
        int Priority { get; }
        bool Applies(SpreadContext ctx);
        decimal CalculateSpread(decimal currentSpread, SpreadContext ctx);
    }

    public class BaseSpreadRule : ISpreadRule
    {
        public string RuleName => "Base";
        public int Priority => 0;
        public bool Applies(SpreadContext ctx) => true;
        public decimal CalculateSpread(decimal current, SpreadContext ctx) => 0.0001m; // 1 pip base
    }

    public class VolatilitySpreadRule : ISpreadRule
    {
        public string RuleName => "Volatility";
        public int Priority => 1;
        public bool Applies(SpreadContext ctx) => ctx.Volatility > 0.02m;
        public decimal CalculateSpread(decimal current, SpreadContext ctx) =>
            current * (1 + ctx.Volatility * 5);
    }

    public class TimeOfDaySpreadRule : ISpreadRule
    {
        public string RuleName => "TimeOfDay";
        public int Priority => 2;
        public bool Applies(SpreadContext ctx) => DateTime.UtcNow.Hour < 7 || DateTime.UtcNow.Hour > 20;
        public decimal CalculateSpread(decimal current, SpreadContext ctx) => current * 1.5m;
    }

    public class LiquiditySpreadRule : ISpreadRule
    {
        public string RuleName => "Liquidity";
        public int Priority => 3;
        public bool Applies(SpreadContext ctx) => ctx.Amount < 500000 || ctx.CurrencyPair.Contains("TRY");
        public decimal CalculateSpread(decimal current, SpreadContext ctx) => current * 1.3m;
    }

    public class SpreadEngine
    {
        private readonly List<ISpreadRule> _rules;

        public SpreadEngine(IEnumerable<ISpreadRule> rules) =>
            _rules = rules.OrderBy(r => r.Priority).ToList();

        public (decimal Spread, List<string> AppliedRules) CalculateSpreadWithDetails(SpreadContext ctx)
        {
            decimal spread = 0;
            var applied = new List<string>();

            foreach (var rule in _rules.Where(r => r.Applies(ctx)))
            {
                spread = rule.CalculateSpread(spread, ctx);
                applied.Add(rule.RuleName);
            }

            return (spread, applied);
        }
    }

    #endregion

    #region FX Trade Hierarchy (Inheritance Demo)

    public abstract class FxTradeBase
    {
        public string TradeId { get; }
        public string CurrencyPair { get; }
        public TradeSide Side { get; }
        public decimal Notional { get; }
        public string CounterpartyId { get; }

        protected FxTradeBase(string tradeId, string pair, TradeSide side, decimal notional, string counterparty)
        {
            TradeId = tradeId;
            CurrencyPair = pair;
            Side = side;
            Notional = notional;
            CounterpartyId = counterparty;
        }

        public abstract string ProductType { get; }
        public abstract decimal CalculateRisk();
        public abstract IEnumerable<CashFlow> GetCashFlows();

        public virtual string GetTradeDescription() => $"{Side} {Notional:N0} {CurrencyPair}";
    }

    public record CashFlow(DateTime Date, decimal Amount, string Currency)
    {
        public override string ToString() => $"{Date:yyyy-MM-dd}: {Amount:N0} {Currency}";
    }

    public class FxSpotTrade : FxTradeBase
    {
        public decimal SpotRate { get; }
        public DateTime ValueDate { get; }

        public override string ProductType => "SPOT";

        public FxSpotTrade(string id, string pair, TradeSide side, decimal notional, string cpty, decimal rate)
            : base(id, pair, side, notional, cpty)
        {
            SpotRate = rate;
            ValueDate = DateTime.Today.AddDays(2);
        }

        public override decimal CalculateRisk() => Notional * 0.02m;

        public override IEnumerable<CashFlow> GetCashFlows()
        {
            var baseCcy = CurrencyPair[..3];
            var quoteCcy = CurrencyPair[3..];

            if (Side == TradeSide.Buy)
            {
                yield return new CashFlow(ValueDate, -Notional * SpotRate, quoteCcy);
                yield return new CashFlow(ValueDate, Notional, baseCcy);
            }
            else
            {
                yield return new CashFlow(ValueDate, -Notional, baseCcy);
                yield return new CashFlow(ValueDate, Notional * SpotRate, quoteCcy);
            }
        }
    }

    public class FxForwardTrade : FxTradeBase
    {
        public decimal SpotRate { get; }
        public decimal ForwardRate { get; }
        public DateTime ValueDate { get; }

        public override string ProductType => "FORWARD";

        public FxForwardTrade(string id, string pair, TradeSide side, decimal notional, string cpty,
                              decimal spotRate, decimal fwdRate, DateTime valueDate)
            : base(id, pair, side, notional, cpty)
        {
            SpotRate = spotRate;
            ForwardRate = fwdRate;
            ValueDate = valueDate;
        }

        public override decimal CalculateRisk()
        {
            var daysToMaturity = (ValueDate - DateTime.Today).Days;
            return Notional * daysToMaturity * 0.0002m;
        }

        public override IEnumerable<CashFlow> GetCashFlows()
        {
            var baseCcy = CurrencyPair[..3];
            var quoteCcy = CurrencyPair[3..];

            if (Side == TradeSide.Buy)
            {
                yield return new CashFlow(ValueDate, -Notional * ForwardRate, quoteCcy);
                yield return new CashFlow(ValueDate, Notional, baseCcy);
            }
            else
            {
                yield return new CashFlow(ValueDate, -Notional, baseCcy);
                yield return new CashFlow(ValueDate, Notional * ForwardRate, quoteCcy);
            }
        }

        public override string GetTradeDescription() =>
            $"{base.GetTradeDescription()} Forward @ {ForwardRate} for {ValueDate:d}";
    }

    public class FxSwapTrade : FxTradeBase
    {
        public FxSpotTrade NearLeg { get; }
        public FxForwardTrade FarLeg { get; }

        public override string ProductType => "SWAP";

        public FxSwapTrade(string id, string pair, TradeSide nearSide, decimal notional, string cpty,
                           decimal spotRate, decimal fwdRate, DateTime farDate)
            : base(id, pair, nearSide, notional, cpty)
        {
            NearLeg = new FxSpotTrade($"{id}-N", pair, nearSide, notional, cpty, spotRate);
            var farSide = nearSide == TradeSide.Buy ? TradeSide.Sell : TradeSide.Buy;
            FarLeg = new FxForwardTrade($"{id}-F", pair, farSide, notional, cpty, spotRate, fwdRate, farDate);
        }

        public override decimal CalculateRisk() => NearLeg.CalculateRisk() + FarLeg.CalculateRisk();

        public override IEnumerable<CashFlow> GetCashFlows() =>
            NearLeg.GetCashFlows().Concat(FarLeg.GetCashFlows());

        public override string GetTradeDescription() =>
            $"SWAP: {NearLeg.GetTradeDescription()} ↔ {FarLeg.GetTradeDescription()}";
    }

    #endregion

    #region Workflow Steps (Inheritance Demo)

    public abstract class WorkflowStep
    {
        public string StepName { get; }
        public string StepId { get; }

        protected WorkflowStep(string stepId, string stepName)
        {
            StepId = stepId;
            StepName = stepName;
        }

        public async Task<WorkflowStepResult> ExecuteAsync(WorkflowContext context)
        {
            Console.WriteLine($"    ▶ Executing: {StepName}");
            try
            {
                var result = await ExecuteStepAsync(context);
                var status = result.Success ? "✅" : "❌";
                Console.WriteLine($"      {status} {StepName}: {result.Message}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ❌ {StepName} failed: {ex.Message}");
                return WorkflowStepResult.Failure(ex.Message);
            }
        }

        protected abstract Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext context);
    }

    public class WorkflowContext
    {
        public FxTrade Trade { get; }
        public Dictionary<string, object> Data { get; } = new();

        public WorkflowContext(FxTrade trade) => Trade = trade;

        public void SetData(string key, object value) => Data[key] = value;
        public T GetData<T>(string key) => (T)Data[key];
    }

    public record WorkflowStepResult(bool Success, string Message)
    {
        public static WorkflowStepResult Ok(string msg = "Completed") => new(true, msg);
        public static WorkflowStepResult Failure(string msg) => new(false, msg);
    }

    public record WorkflowResult(bool Success, IEnumerable<(WorkflowStep Step, WorkflowStepResult Result)> Steps);

    // Pre-trade steps
    public class CreditCheckStep : WorkflowStep
    {
        public CreditCheckStep() : base("PRE-001", "Credit Check") { }

        protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext ctx)
        {
            await Task.Delay(50);
            var creditLimit = 10_000_000m;
            var exposure = ctx.Trade.Notional * 0.05m;
            ctx.SetData("CreditExposure", exposure);
            return exposure <= creditLimit
                ? WorkflowStepResult.Ok($"Credit OK. Exposure: {exposure:N0}")
                : WorkflowStepResult.Failure($"Credit exceeded. Limit: {creditLimit:N0}");
        }
    }

    public class ComplianceCheckStep : WorkflowStep
    {
        public ComplianceCheckStep() : base("PRE-002", "Compliance Check") { }

        protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext ctx)
        {
            await Task.Delay(30);
            return WorkflowStepResult.Ok("No sanctions/restrictions found");
        }
    }

    public class PriceValidationStep : WorkflowStep
    {
        public PriceValidationStep() : base("PRE-003", "Price Validation") { }

        protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext ctx)
        {
            await Task.Delay(20);
            return WorkflowStepResult.Ok("Price within market tolerance");
        }
    }

    // Post-trade steps
    public class ConfirmationStep : WorkflowStep
    {
        public ConfirmationStep() : base("POST-001", "Trade Confirmation") { }

        protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext ctx)
        {
            await Task.Delay(40);
            var confId = $"CONF-{ctx.Trade.TradeId}";
            ctx.SetData("ConfirmationId", confId);
            return WorkflowStepResult.Ok($"Confirmation sent: {confId}");
        }
    }

    public class SettlementInstructionStep : WorkflowStep
    {
        public SettlementInstructionStep() : base("POST-002", "Settlement Instructions") { }

        protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext ctx)
        {
            await Task.Delay(60);
            return WorkflowStepResult.Ok("Settlement instructions submitted to CLS");
        }
    }

    public class RegulatoryReportingStep : WorkflowStep
    {
        public RegulatoryReportingStep() : base("POST-003", "Regulatory Reporting") { }

        protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext ctx)
        {
            await Task.Delay(50);
            return WorkflowStepResult.Ok("MiFID II and EMIR reports submitted");
        }
    }

    public class TradeWorkflowEngine
    {
        public async Task<WorkflowResult> ExecuteWorkflowAsync(
            IEnumerable<WorkflowStep> steps, WorkflowContext context)
        {
            var results = new List<(WorkflowStep, WorkflowStepResult)>();

            foreach (var step in steps)
            {
                var result = await step.ExecuteAsync(context);
                results.Add((step, result));

                if (!result.Success)
                    return new WorkflowResult(false, results);
            }

            return new WorkflowResult(true, results);
        }
    }

    #endregion

    #region Quote Book (Complete System)

    public class FxQuoteBook
    {
        private readonly ConcurrentDictionary<string, SortedList<decimal, FxQuote>> _bids = new();
        private readonly ConcurrentDictionary<string, SortedList<decimal, FxQuote>> _asks = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public void AddQuote(FxQuote quote)
        {
            _lock.EnterWriteLock();
            try
            {
                var bids = _bids.GetOrAdd(quote.CurrencyPair, _ => new SortedList<decimal, FxQuote>());
                var asks = _asks.GetOrAdd(quote.CurrencyPair, _ => new SortedList<decimal, FxQuote>());

                bids[quote.BidPrice] = quote;
                asks[quote.AskPrice] = quote;
            }
            finally { _lock.ExitWriteLock(); }
        }

        public (FxQuote? BestBid, FxQuote? BestAsk) GetBestQuotes(string pair)
        {
            _lock.EnterReadLock();
            try
            {
                FxQuote? bestBid = null, bestAsk = null;

                if (_bids.TryGetValue(pair, out var bids) && bids.Count > 0)
                    bestBid = bids.Values[^1];

                if (_asks.TryGetValue(pair, out var asks) && asks.Count > 0)
                    bestAsk = asks.Values[0];

                return (bestBid, bestAsk);
            }
            finally { _lock.ExitReadLock(); }
        }

        public (List<(decimal Price, string LP)> Bids, List<(decimal Price, string LP)> Asks) GetDepth(string pair, int levels)
        {
            _lock.EnterReadLock();
            try
            {
                var bids = new List<(decimal, string)>();
                var asks = new List<(decimal, string)>();

                if (_bids.TryGetValue(pair, out var bidBook))
                    bids = bidBook.Reverse().Take(levels).Select(kv => (kv.Key, kv.Value.LiquidityProvider)).ToList();

                if (_asks.TryGetValue(pair, out var askBook))
                    asks = askBook.Take(levels).Select(kv => (kv.Key, kv.Value.LiquidityProvider)).ToList();

                return (bids, asks);
            }
            finally { _lock.ExitReadLock(); }
        }
    }

    #endregion
}
