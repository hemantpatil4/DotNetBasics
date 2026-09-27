# OOP Pillars in FX Trading Domain – Real-World Interview Guide

> **For 3+ years experienced .NET developer in FX/Trading systems**
> Domain: FX Quote Books, Trade Parsing, Pre/Post Trade Workflows, CBS Data, Spread Management, Rule-Based Systems

---

## Table of Contents

1. [Encapsulation in FX Trading](#1-encapsulation-in-fx-trading)
2. [Abstraction in FX Trading](#2-abstraction-in-fx-trading)
3. [Polymorphism in FX Trading](#3-polymorphism-in-fx-trading)
4. [Inheritance in FX Trading](#4-inheritance-in-fx-trading)
5. [Complete FX Trading System Example](#5-complete-fx-trading-system-example)
6. [Interview Q&A with Domain Examples](#6-interview-qa-with-domain-examples)

---

## 1. Encapsulation in FX Trading

### Concept

**Encapsulation** = Bundling data and methods together, hiding internal details, exposing only what's necessary.

### Why It Matters in Trading Systems

- **Data Integrity**: Prevent invalid quote prices, trade amounts
- **Audit Trail**: Control how sensitive data is modified
- **Regulatory Compliance**: Ensure fields like MiFID flags can't be arbitrarily changed
- **Thread Safety**: Protect shared state in high-frequency systems

---

### Example 1: FX Quote with Price Protection

```csharp
/// <summary>
/// Represents an FX Quote in the Quote Book.
/// Encapsulates bid/ask prices with validation and spread calculation.
/// </summary>
public class FxQuote
{
    // Private fields - internal state protected
    private decimal _bidPrice;
    private decimal _askPrice;
    private readonly string _currencyPair;
    private readonly DateTime _createdAt;
    private DateTime _lastUpdatedAt;

    // Constants for validation
    private const decimal MinSpread = 0.00001m;  // 0.1 pip
    private const decimal MaxSpread = 0.01m;     // 100 pips

    public string CurrencyPair => _currencyPair;
    public string QuoteId { get; }
    public string LiquidityProvider { get; }
    public DateTime CreatedAt => _createdAt;
    public DateTime LastUpdatedAt => _lastUpdatedAt;

    // Read-only calculated properties
    public decimal BidPrice => _bidPrice;
    public decimal AskPrice => _askPrice;
    public decimal Spread => _askPrice - _bidPrice;
    public decimal MidPrice => (_bidPrice + _askPrice) / 2;

    public FxQuote(string quoteId, string currencyPair, string liquidityProvider,
                   decimal bidPrice, decimal askPrice)
    {
        QuoteId = quoteId ?? throw new ArgumentNullException(nameof(quoteId));
        _currencyPair = ValidateCurrencyPair(currencyPair);
        LiquidityProvider = liquidityProvider;
        _createdAt = DateTime.UtcNow;
        _lastUpdatedAt = _createdAt;

        // Validate and set prices through controlled method
        UpdatePrices(bidPrice, askPrice);
    }

    /// <summary>
    /// Updates quote prices with full validation.
    /// This is the ONLY way to modify prices - ensures data integrity.
    /// </summary>
    public void UpdatePrices(decimal newBid, decimal newAsk)
    {
        // Validation 1: Prices must be positive
        if (newBid <= 0 || newAsk <= 0)
            throw new InvalidQuoteException($"Prices must be positive. Bid: {newBid}, Ask: {newAsk}");

        // Validation 2: Ask must be greater than Bid
        if (newAsk <= newBid)
            throw new InvalidQuoteException($"Ask ({newAsk}) must be greater than Bid ({newBid})");

        // Validation 3: Spread within acceptable range
        decimal spread = newAsk - newBid;
        if (spread < MinSpread || spread > MaxSpread)
            throw new InvalidQuoteException(
                $"Spread {spread} outside acceptable range [{MinSpread}, {MaxSpread}]");

        // Validation 4: Price movement check (prevent fat finger errors)
        if (_bidPrice > 0)  // Not initial set
        {
            decimal bidChange = Math.Abs(newBid - _bidPrice) / _bidPrice;
            if (bidChange > 0.05m)  // More than 5% change
                throw new SuspiciousPriceMovementException(
                    $"Bid price change of {bidChange:P2} exceeds threshold");
        }

        // All validations passed - update state
        _bidPrice = newBid;
        _askPrice = newAsk;
        _lastUpdatedAt = DateTime.UtcNow;
    }

    private static string ValidateCurrencyPair(string pair)
    {
        if (string.IsNullOrEmpty(pair) || pair.Length != 6)
            throw new ArgumentException("Currency pair must be 6 characters (e.g., EURUSD)");
        return pair.ToUpperInvariant();
    }
}

// Usage - cannot bypass validation
var quote = new FxQuote("Q001", "EURUSD", "Reuters", 1.0850m, 1.0852m);

// ✅ Controlled update with validation
quote.UpdatePrices(1.0851m, 1.0853m);

// ❌ Cannot directly modify prices
// quote._bidPrice = -1;  // COMPILE ERROR - private field
// quote.BidPrice = 1.0;  // COMPILE ERROR - no setter
```

---

### Example 2: Trade with Audit Trail

```csharp
/// <summary>
/// Represents an FX Trade with full encapsulation of state transitions.
/// All status changes are controlled and audited.
/// </summary>
public class FxTrade
{
    // Immutable trade details
    public string TradeId { get; }
    public string CurrencyPair { get; }
    public decimal Notional { get; }
    public TradeSide Side { get; }
    public decimal ExecutionRate { get; }
    public DateTime TradeDate { get; }
    public DateTime ValueDate { get; }
    public string CounterpartyId { get; }

    // Mutable but controlled state
    private TradeStatus _status;
    private readonly List<TradeStatusChange> _statusHistory;

    public TradeStatus Status => _status;
    public IReadOnlyList<TradeStatusChange> StatusHistory => _statusHistory.AsReadOnly();

    public FxTrade(string tradeId, string currencyPair, decimal notional,
                   TradeSide side, decimal rate, string counterpartyId,
                   DateTime valueDate)
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

    /// <summary>
    /// Transitions trade to new status with validation and audit.
    /// Encapsulates all business rules for valid transitions.
    /// </summary>
    public void TransitionTo(TradeStatus newStatus, string reason, string userId)
    {
        // Validate transition is allowed
        if (!IsValidTransition(_status, newStatus))
            throw new InvalidTradeTransitionException(
                $"Cannot transition from {_status} to {newStatus}");

        // Additional business rules
        if (newStatus == TradeStatus.Settled && DateTime.UtcNow < ValueDate)
            throw new InvalidOperationException("Cannot settle before value date");

        // Record change for audit
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
            (TradeStatus.Confirmed, TradeStatus.Amended) => true,
            (TradeStatus.Matched, TradeStatus.Settled) => true,
            (TradeStatus.Matched, TradeStatus.Failed) => true,
            (TradeStatus.Amended, TradeStatus.Confirmed) => true,
            _ => false
        };
    }
}

public enum TradeStatus
{
    Pending,
    Confirmed,
    Rejected,
    Matched,
    Amended,
    Settled,
    Failed
}

public enum TradeSide { Buy, Sell }

public record TradeStatusChange(
    TradeStatus NewStatus,
    string Reason,
    string UserId,
    DateTime Timestamp = default)
{
    public DateTime Timestamp { get; } = Timestamp == default ? DateTime.UtcNow : Timestamp;
}
```

---

### Example 3: Customer Spread Configuration

```csharp
/// <summary>
/// Encapsulates customer-specific spread configuration.
/// Protects spread markup rules from invalid modifications.
/// </summary>
public class CustomerSpreadConfig
{
    private readonly string _customerId;
    private readonly Dictionary<string, decimal> _pairSpreadMarkups;
    private decimal _defaultMarkup;
    private readonly decimal _maxAllowedMarkup;

    public string CustomerId => _customerId;
    public decimal DefaultMarkup => _defaultMarkup;

    public CustomerSpreadConfig(string customerId, decimal defaultMarkup,
                                 decimal maxAllowedMarkup = 0.001m)
    {
        _customerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        _maxAllowedMarkup = maxAllowedMarkup;
        _pairSpreadMarkups = new Dictionary<string, decimal>();

        SetDefaultMarkup(defaultMarkup);
    }

    /// <summary>
    /// Sets default spread markup with validation.
    /// </summary>
    public void SetDefaultMarkup(decimal markup)
    {
        ValidateMarkup(markup, "Default");
        _defaultMarkup = markup;
    }

    /// <summary>
    /// Sets spread markup for specific currency pair.
    /// </summary>
    public void SetPairMarkup(string currencyPair, decimal markup)
    {
        ValidateMarkup(markup, currencyPair);
        _pairSpreadMarkups[currencyPair.ToUpperInvariant()] = markup;
    }

    /// <summary>
    /// Gets the applicable spread markup for a currency pair.
    /// Returns pair-specific markup if configured, otherwise default.
    /// </summary>
    public decimal GetMarkupForPair(string currencyPair)
    {
        return _pairSpreadMarkups.TryGetValue(currencyPair.ToUpperInvariant(), out var markup)
            ? markup
            : _defaultMarkup;
    }

    /// <summary>
    /// Calculates customer-specific quote with markup applied.
    /// Encapsulates the spread calculation logic.
    /// </summary>
    public (decimal Bid, decimal Ask) ApplyMarkup(FxQuote baseQuote)
    {
        decimal markup = GetMarkupForPair(baseQuote.CurrencyPair);
        decimal halfMarkup = markup / 2;

        return (
            Bid: baseQuote.BidPrice - halfMarkup,  // Widen bid down
            Ask: baseQuote.AskPrice + halfMarkup   // Widen ask up
        );
    }

    private void ValidateMarkup(decimal markup, string context)
    {
        if (markup < 0)
            throw new ArgumentException($"{context} markup cannot be negative");
        if (markup > _maxAllowedMarkup)
            throw new ArgumentException(
                $"{context} markup {markup} exceeds maximum allowed {_maxAllowedMarkup}");
    }
}
```

---

## 2. Abstraction in FX Trading

### Concept

**Abstraction** = Hiding complex implementation details, showing only essential features.

### Why It Matters in Trading Systems

- **Simplify Integration**: Hide complexity of connecting to multiple liquidity providers
- **Unified Interface**: Same API regardless of underlying market data source
- **Focus on Business Logic**: Developers work with trades, not socket connections
- **Reduce Errors**: Abstract away low-level parsing details

---

### Example 1: Market Data Feed Abstraction

```csharp
/// <summary>
/// Abstracts market data retrieval from various sources.
/// Consumer doesn't need to know if data comes from Reuters, Bloomberg, or internal cache.
/// </summary>
public interface IMarketDataProvider
{
    Task<FxQuote> GetQuoteAsync(string currencyPair);
    Task<IEnumerable<FxQuote>> GetAllQuotesAsync();
    IAsyncEnumerable<FxQuote> StreamQuotesAsync(string currencyPair, CancellationToken ct);
}

// Consumer code - simple and clean
public class PricingService
{
    private readonly IMarketDataProvider _marketData;

    public PricingService(IMarketDataProvider marketData)
    {
        _marketData = marketData;  // Don't know or care about implementation
    }

    public async Task<decimal> GetMidPriceAsync(string pair)
    {
        var quote = await _marketData.GetQuoteAsync(pair);
        return quote.MidPrice;  // Simple!
    }
}

// Complex implementation hidden behind interface
public class ReutersMarketDataProvider : IMarketDataProvider
{
    private readonly TcpClient _connection;
    private readonly ReutersProtocolHandler _protocol;
    private readonly IMemoryCache _cache;
    private readonly ILogger _logger;

    public async Task<FxQuote> GetQuoteAsync(string currencyPair)
    {
        // Check cache first
        if (_cache.TryGetValue($"quote:{currencyPair}", out FxQuote cached))
            return cached;

        // Build Reuters-specific request
        var request = new ReutersRicRequest
        {
            Ric = ConvertToRic(currencyPair),
            Fields = new[] { "BID", "ASK", "TIMESTAMP" }
        };

        // Send request and parse response
        var response = await _protocol.SendRequestAsync(request);

        // Parse Reuters-specific format
        var quote = ParseReutersResponse(response, currencyPair);

        // Cache for 100ms
        _cache.Set($"quote:{currencyPair}", quote, TimeSpan.FromMilliseconds(100));

        return quote;
    }

    // All this complexity is HIDDEN from the consumer
    private string ConvertToRic(string pair) => $"{pair}=R";

    private FxQuote ParseReutersResponse(ReutersResponse response, string pair)
    {
        // Complex parsing logic...
        return new FxQuote(
            Guid.NewGuid().ToString(),
            pair,
            "Reuters",
            decimal.Parse(response.Fields["BID"]),
            decimal.Parse(response.Fields["ASK"])
        );
    }

    // ... other methods
}
```

---

### Example 2: Trade Message Parser Abstraction

```csharp
/// <summary>
/// Abstracts the parsing of trade messages from different formats.
/// Hides FIX, FPML, JSON parsing complexity.
/// </summary>
public interface ITradeMessageParser
{
    FxTrade Parse(string rawMessage);
    bool CanParse(string rawMessage);
}

// Consumer doesn't know about message formats
public class TradeIngestionService
{
    private readonly IEnumerable<ITradeMessageParser> _parsers;

    public TradeIngestionService(IEnumerable<ITradeMessageParser> parsers)
    {
        _parsers = parsers;
    }

    public FxTrade ProcessMessage(string rawMessage)
    {
        var parser = _parsers.FirstOrDefault(p => p.CanParse(rawMessage))
            ?? throw new UnsupportedMessageFormatException();

        return parser.Parse(rawMessage);  // Simple!
    }
}

// Complex FIX parsing hidden
public class FixTradeParser : ITradeMessageParser
{
    public bool CanParse(string message) => message.StartsWith("8=FIX");

    public FxTrade Parse(string rawMessage)
    {
        // Complex FIX protocol parsing...
        var fields = ParseFixFields(rawMessage);

        return new FxTrade(
            tradeId: fields["17"],           // ExecID
            currencyPair: fields["55"],       // Symbol
            notional: decimal.Parse(fields["32"]),  // LastQty
            side: fields["54"] == "1" ? TradeSide.Buy : TradeSide.Sell,
            rate: decimal.Parse(fields["31"]), // LastPx
            counterpartyId: fields["49"],     // SenderCompID
            valueDate: ParseFixDate(fields["64"])  // SettlDate
        );
    }

    private Dictionary<string, string> ParseFixFields(string message)
    {
        // SOH-delimited field parsing...
    }
}

// Complex FPML parsing hidden
public class FpmlTradeParser : ITradeMessageParser
{
    public bool CanParse(string message) => message.Contains("<FpML");

    public FxTrade Parse(string rawMessage)
    {
        var doc = XDocument.Parse(rawMessage);
        var ns = doc.Root.GetDefaultNamespace();

        // Complex XML navigation...
        var fxTrade = doc.Descendants(ns + "fxSingleLeg").First();

        return new FxTrade(
            tradeId: fxTrade.Element(ns + "tradeId")?.Value,
            // ... more parsing
        );
    }
}
```

---

### Example 3: Settlement Instruction Abstraction

```csharp
/// <summary>
/// Abstracts settlement processing across different clearing systems.
/// Hides CLS, RTGS, correspondent bank differences.
/// </summary>
public interface ISettlementService
{
    Task<SettlementResult> SettleTradeAsync(FxTrade trade);
    Task<SettlementStatus> GetStatusAsync(string settlementId);
}

// Usage - developer doesn't need to know CLS vs RTGS details
public class PostTradeProcessor
{
    private readonly ISettlementService _settlement;

    public async Task ProcessSettlementAsync(FxTrade trade)
    {
        var result = await _settlement.SettleTradeAsync(trade);
        // Simple API regardless of settlement method
    }
}

// CLS implementation - complex netting and messaging hidden
public class ClsSettlementService : ISettlementService
{
    private readonly IClsGateway _clsGateway;
    private readonly INettingEngine _nettingEngine;

    public async Task<SettlementResult> SettleTradeAsync(FxTrade trade)
    {
        // 1. Check CLS eligibility (currency pair, value date, etc.)
        if (!IsClsEligible(trade))
            throw new SettlementException("Trade not CLS eligible");

        // 2. Submit to netting window
        var nettingBatch = await _nettingEngine.AddToCurrentBatchAsync(trade);

        // 3. Generate CLS message format
        var clsMessage = BuildClsMessage(trade, nettingBatch);

        // 4. Submit to CLS
        var response = await _clsGateway.SubmitAsync(clsMessage);

        return new SettlementResult
        {
            SettlementId = response.ClsReference,
            Method = SettlementMethod.CLS,
            ExpectedSettlementTime = response.SettlementTime
        };
    }

    // All CLS-specific complexity hidden...
}
```

---

## 3. Polymorphism in FX Trading

### Concept

**Polymorphism** = Same operation behaves differently based on the object type.

### Why It Matters in Trading Systems

- **Flexible Workflows**: Different trade types processed uniformly
- **Extensible Rules**: Add new rule types without changing engine
- **Multiple LP Integration**: Same interface, different implementations
- **Validation Chains**: Different validators for different scenarios

---

### Example 1: Trade Validation with Different Rules

```csharp
/// <summary>
/// Base interface for trade validation rules.
/// Polymorphism allows adding new rules without changing validation engine.
/// </summary>
public interface ITradeValidationRule
{
    string RuleName { get; }
    ValidationResult Validate(FxTrade trade);
}

// Validation engine - works with ANY rule polymorphically
public class TradeValidationEngine
{
    private readonly IEnumerable<ITradeValidationRule> _rules;

    public TradeValidationEngine(IEnumerable<ITradeValidationRule> rules)
    {
        _rules = rules;
    }

    public ValidationSummary ValidateTrade(FxTrade trade)
    {
        var results = new List<ValidationResult>();

        foreach (var rule in _rules)
        {
            // Polymorphic call - each rule has different logic
            var result = rule.Validate(trade);
            results.Add(result);
        }

        return new ValidationSummary(results);
    }
}

// Rule 1: Notional limit check
public class NotionalLimitRule : ITradeValidationRule
{
    private readonly decimal _maxNotional;

    public string RuleName => "NotionalLimit";

    public NotionalLimitRule(decimal maxNotional)
    {
        _maxNotional = maxNotional;
    }

    public ValidationResult Validate(FxTrade trade)
    {
        if (trade.Notional > _maxNotional)
            return ValidationResult.Fail(RuleName,
                $"Notional {trade.Notional:N0} exceeds limit {_maxNotional:N0}");

        return ValidationResult.Pass(RuleName);
    }
}

// Rule 2: Counterparty credit check
public class CounterpartyCreditRule : ITradeValidationRule
{
    private readonly ICreditService _creditService;

    public string RuleName => "CounterpartyCredit";

    public CounterpartyCreditRule(ICreditService creditService)
    {
        _creditService = creditService;
    }

    public ValidationResult Validate(FxTrade trade)
    {
        var creditLimit = _creditService.GetAvailableCredit(trade.CounterpartyId);
        var tradeExposure = CalculateExposure(trade);

        if (tradeExposure > creditLimit)
            return ValidationResult.Fail(RuleName,
                $"Insufficient credit. Required: {tradeExposure:N0}, Available: {creditLimit:N0}");

        return ValidationResult.Pass(RuleName);
    }

    private decimal CalculateExposure(FxTrade trade)
    {
        // Calculate potential exposure...
        return trade.Notional * 0.05m;  // Simplified
    }
}

// Rule 3: Value date validation
public class ValueDateRule : ITradeValidationRule
{
    private readonly ICalendarService _calendar;

    public string RuleName => "ValueDate";

    public ValueDateRule(ICalendarService calendar)
    {
        _calendar = calendar;
    }

    public ValidationResult Validate(FxTrade trade)
    {
        if (trade.ValueDate < DateTime.Today)
            return ValidationResult.Fail(RuleName, "Value date cannot be in the past");

        if (!_calendar.IsBusinessDay(trade.ValueDate, trade.CurrencyPair))
            return ValidationResult.Fail(RuleName,
                $"{trade.ValueDate:d} is not a business day for {trade.CurrencyPair}");

        return ValidationResult.Pass(RuleName);
    }
}

// Rule 4: MiFID compliance
public class MifidComplianceRule : ITradeValidationRule
{
    public string RuleName => "MiFIDCompliance";

    public ValidationResult Validate(FxTrade trade)
    {
        // MiFID II specific checks...
        if (string.IsNullOrEmpty(trade.CounterpartyId))
            return ValidationResult.Fail(RuleName, "LEI required for MiFID reporting");

        return ValidationResult.Pass(RuleName);
    }
}

// Usage - add new rules without changing engine
var rules = new ITradeValidationRule[]
{
    new NotionalLimitRule(10_000_000),
    new CounterpartyCreditRule(creditService),
    new ValueDateRule(calendarService),
    new MifidComplianceRule()
};

var engine = new TradeValidationEngine(rules);
var result = engine.ValidateTrade(trade);  // All rules executed polymorphically
```

---

### Example 2: Pricing Strategy for Different Customer Tiers

```csharp
/// <summary>
/// Polymorphic pricing strategies for different customer segments.
/// </summary>
public abstract class PricingStrategy
{
    public abstract string StrategyName { get; }
    public abstract FxQuote ApplyPricing(FxQuote baseQuote, Customer customer);
}

// Pricing for retail customers - wider spreads
public class RetailPricingStrategy : PricingStrategy
{
    public override string StrategyName => "Retail";

    public override FxQuote ApplyPricing(FxQuote baseQuote, Customer customer)
    {
        decimal spreadMarkup = 0.0005m;  // 5 pips

        return new FxQuote(
            Guid.NewGuid().ToString(),
            baseQuote.CurrencyPair,
            "Internal",
            baseQuote.BidPrice - spreadMarkup,
            baseQuote.AskPrice + spreadMarkup
        );
    }
}

// Pricing for corporate customers - tighter spreads based on volume
public class CorporatePricingStrategy : PricingStrategy
{
    private readonly IVolumeTracker _volumeTracker;

    public override string StrategyName => "Corporate";

    public CorporatePricingStrategy(IVolumeTracker volumeTracker)
    {
        _volumeTracker = volumeTracker;
    }

    public override FxQuote ApplyPricing(FxQuote baseQuote, Customer customer)
    {
        var monthlyVolume = _volumeTracker.GetMonthlyVolume(customer.Id);

        decimal spreadMarkup = monthlyVolume switch
        {
            > 100_000_000 => 0.0001m,   // 1 pip for high volume
            > 10_000_000 => 0.0002m,    // 2 pips for medium volume
            _ => 0.0003m                 // 3 pips for low volume
        };

        return new FxQuote(
            Guid.NewGuid().ToString(),
            baseQuote.CurrencyPair,
            "Internal",
            baseQuote.BidPrice - spreadMarkup,
            baseQuote.AskPrice + spreadMarkup
        );
    }
}

// Pricing for institutional clients - custom negotiated spreads
public class InstitutionalPricingStrategy : PricingStrategy
{
    private readonly ISpreadConfigRepository _spreadConfig;

    public override string StrategyName => "Institutional";

    public InstitutionalPricingStrategy(ISpreadConfigRepository spreadConfig)
    {
        _spreadConfig = spreadConfig;
    }

    public override FxQuote ApplyPricing(FxQuote baseQuote, Customer customer)
    {
        // Get customer-specific negotiated spread
        var config = _spreadConfig.GetCustomerConfig(customer.Id);
        decimal spreadMarkup = config.GetMarkupForPair(baseQuote.CurrencyPair);

        return new FxQuote(
            Guid.NewGuid().ToString(),
            baseQuote.CurrencyPair,
            "Internal",
            baseQuote.BidPrice - spreadMarkup / 2,
            baseQuote.AskPrice + spreadMarkup / 2
        );
    }
}

// Pricing engine - uses strategy polymorphically
public class PricingEngine
{
    private readonly Dictionary<CustomerTier, PricingStrategy> _strategies;

    public PricingEngine(IEnumerable<PricingStrategy> strategies)
    {
        // Map strategies...
    }

    public FxQuote GetCustomerQuote(FxQuote marketQuote, Customer customer)
    {
        var strategy = _strategies[customer.Tier];

        // Polymorphic call - different pricing logic executed
        return strategy.ApplyPricing(marketQuote, customer);
    }
}
```

---

### Example 3: Order Execution Across Different Venues

```csharp
/// <summary>
/// Polymorphic order execution across different liquidity venues.
/// </summary>
public interface IExecutionVenue
{
    string VenueName { get; }
    bool IsAvailable { get; }
    decimal GetQuote(string currencyPair, decimal amount, TradeSide side);
    Task<ExecutionResult> ExecuteAsync(OrderRequest order);
}

// Primary market execution
public class PrimaryMarketVenue : IExecutionVenue
{
    public string VenueName => "Primary";
    public bool IsAvailable => true;

    public decimal GetQuote(string currencyPair, decimal amount, TradeSide side)
    {
        // Get quote from primary LP...
    }

    public async Task<ExecutionResult> ExecuteAsync(OrderRequest order)
    {
        // Execute on primary market...
    }
}

// ECN execution
public class EcnVenue : IExecutionVenue
{
    private readonly IEcnConnector _ecn;

    public string VenueName => "ECN";
    public bool IsAvailable => _ecn.IsConnected;

    public decimal GetQuote(string currencyPair, decimal amount, TradeSide side)
    {
        // Aggregate ECN book...
    }

    public async Task<ExecutionResult> ExecuteAsync(OrderRequest order)
    {
        // Execute on ECN with order routing logic...
    }
}

// Internal crossing (match with other client orders)
public class InternalCrossingVenue : IExecutionVenue
{
    private readonly IOrderBook _internalBook;

    public string VenueName => "InternalCross";
    public bool IsAvailable => _internalBook.HasLiquidity;

    public decimal GetQuote(string currencyPair, decimal amount, TradeSide side)
    {
        // Check internal book...
    }

    public async Task<ExecutionResult> ExecuteAsync(OrderRequest order)
    {
        // Match internally first for best execution...
    }
}

// Smart Order Router - uses venues polymorphically
public class SmartOrderRouter
{
    private readonly IEnumerable<IExecutionVenue> _venues;

    public SmartOrderRouter(IEnumerable<IExecutionVenue> venues)
    {
        _venues = venues;
    }

    public async Task<ExecutionResult> RouteOrderAsync(OrderRequest order)
    {
        // Get quotes from all available venues (polymorphic calls)
        var quotes = _venues
            .Where(v => v.IsAvailable)
            .Select(v => new
            {
                Venue = v,
                Quote = v.GetQuote(order.CurrencyPair, order.Amount, order.Side)
            })
            .ToList();

        // Select best venue
        var best = order.Side == TradeSide.Buy
            ? quotes.OrderBy(q => q.Quote).First()   // Lowest ask for buy
            : quotes.OrderByDescending(q => q.Quote).First();  // Highest bid for sell

        // Execute on best venue (polymorphic call)
        return await best.Venue.ExecuteAsync(order);
    }
}
```

---

## 4. Inheritance in FX Trading

### Concept

**Inheritance** = Creating new classes from existing classes, reusing and extending functionality.

### Why It Matters in Trading Systems

- **Trade Hierarchies**: Spot, Forward, Swap share common trade attributes
- **Message Types**: Different message types share base structure
- **Workflow Steps**: Base workflow with specialized steps

---

### Example 1: FX Trade Type Hierarchy

```csharp
/// <summary>
/// Base class for all FX trade types.
/// Contains common properties and behavior.
/// </summary>
public abstract class FxTradeBase
{
    // Common to ALL FX trades
    public string TradeId { get; }
    public string CurrencyPair { get; }
    public TradeSide Side { get; }
    public decimal Notional { get; }
    public string CounterpartyId { get; }
    public DateTime TradeDate { get; }
    public TradeStatus Status { get; protected set; }

    protected FxTradeBase(string tradeId, string currencyPair, TradeSide side,
                          decimal notional, string counterpartyId)
    {
        TradeId = tradeId;
        CurrencyPair = currencyPair;
        Side = side;
        Notional = notional;
        CounterpartyId = counterpartyId;
        TradeDate = DateTime.UtcNow;
        Status = TradeStatus.Pending;
    }

    // Common behavior
    public string BaseCurrency => CurrencyPair[..3];
    public string QuoteCurrency => CurrencyPair[3..];

    // Abstract - each trade type calculates P&L differently
    public abstract decimal CalculatePnL(decimal currentRate);

    // Abstract - each trade type has different settlement
    public abstract IEnumerable<CashFlow> GetCashFlows();

    // Virtual - can be overridden if needed
    public virtual string GetTradeDescription()
    {
        return $"{Side} {Notional:N0} {CurrencyPair}";
    }
}

/// <summary>
/// FX Spot trade - settles T+2
/// </summary>
public class FxSpotTrade : FxTradeBase
{
    public decimal SpotRate { get; }
    public DateTime ValueDate { get; }

    public FxSpotTrade(string tradeId, string currencyPair, TradeSide side,
                       decimal notional, string counterpartyId, decimal spotRate)
        : base(tradeId, currencyPair, side, notional, counterpartyId)
    {
        SpotRate = spotRate;
        ValueDate = CalculateSpotDate();
    }

    public override decimal CalculatePnL(decimal currentRate)
    {
        decimal rateMove = currentRate - SpotRate;
        return Side == TradeSide.Buy
            ? Notional * rateMove
            : Notional * -rateMove;
    }

    public override IEnumerable<CashFlow> GetCashFlows()
    {
        if (Side == TradeSide.Buy)
        {
            yield return new CashFlow(ValueDate, -Notional * SpotRate, QuoteCurrency);
            yield return new CashFlow(ValueDate, Notional, BaseCurrency);
        }
        else
        {
            yield return new CashFlow(ValueDate, -Notional, BaseCurrency);
            yield return new CashFlow(ValueDate, Notional * SpotRate, QuoteCurrency);
        }
    }

    private DateTime CalculateSpotDate()
    {
        // T+2 business days
        return BusinessDayCalculator.AddBusinessDays(DateTime.Today, 2, CurrencyPair);
    }
}

/// <summary>
/// FX Forward trade - settles on specific future date
/// </summary>
public class FxForwardTrade : FxTradeBase
{
    public decimal ForwardRate { get; }
    public decimal SpotRate { get; }
    public DateTime ValueDate { get; }
    public decimal ForwardPoints => ForwardRate - SpotRate;

    public FxForwardTrade(string tradeId, string currencyPair, TradeSide side,
                          decimal notional, string counterpartyId,
                          decimal spotRate, decimal forwardRate, DateTime valueDate)
        : base(tradeId, currencyPair, side, notional, counterpartyId)
    {
        SpotRate = spotRate;
        ForwardRate = forwardRate;
        ValueDate = valueDate;
    }

    public override decimal CalculatePnL(decimal currentRate)
    {
        // Forward P&L includes forward points
        decimal rateMove = currentRate - ForwardRate;
        return Side == TradeSide.Buy
            ? Notional * rateMove
            : Notional * -rateMove;
    }

    public override IEnumerable<CashFlow> GetCashFlows()
    {
        if (Side == TradeSide.Buy)
        {
            yield return new CashFlow(ValueDate, -Notional * ForwardRate, QuoteCurrency);
            yield return new CashFlow(ValueDate, Notional, BaseCurrency);
        }
        else
        {
            yield return new CashFlow(ValueDate, -Notional, BaseCurrency);
            yield return new CashFlow(ValueDate, Notional * ForwardRate, QuoteCurrency);
        }
    }

    public override string GetTradeDescription()
    {
        return $"{base.GetTradeDescription()} Forward @ {ForwardRate} for {ValueDate:d}";
    }
}

/// <summary>
/// FX Swap trade - combination of spot and forward
/// </summary>
public class FxSwapTrade : FxTradeBase
{
    public FxSpotTrade NearLeg { get; }
    public FxForwardTrade FarLeg { get; }
    public decimal SwapPoints => FarLeg.ForwardRate - NearLeg.SpotRate;

    public FxSwapTrade(string tradeId, string currencyPair, TradeSide nearSide,
                       decimal notional, string counterpartyId,
                       decimal spotRate, decimal forwardRate, DateTime farDate)
        : base(tradeId, currencyPair, nearSide, notional, counterpartyId)
    {
        // Near leg - opposite side to far leg for a swap
        NearLeg = new FxSpotTrade(
            $"{tradeId}-NEAR", currencyPair, nearSide,
            notional, counterpartyId, spotRate);

        // Far leg - reverse of near leg
        var farSide = nearSide == TradeSide.Buy ? TradeSide.Sell : TradeSide.Buy;
        FarLeg = new FxForwardTrade(
            $"{tradeId}-FAR", currencyPair, farSide,
            notional, counterpartyId, spotRate, forwardRate, farDate);
    }

    public override decimal CalculatePnL(decimal currentRate)
    {
        return NearLeg.CalculatePnL(currentRate) + FarLeg.CalculatePnL(currentRate);
    }

    public override IEnumerable<CashFlow> GetCashFlows()
    {
        foreach (var cf in NearLeg.GetCashFlows())
            yield return cf;
        foreach (var cf in FarLeg.GetCashFlows())
            yield return cf;
    }

    public override string GetTradeDescription()
    {
        return $"SWAP: {NearLeg.GetTradeDescription()} / {FarLeg.GetTradeDescription()}";
    }
}

// Usage - polymorphism through inheritance
List<FxTradeBase> portfolio = new()
{
    new FxSpotTrade("T001", "EURUSD", TradeSide.Buy, 1_000_000, "CUST01", 1.0850m),
    new FxForwardTrade("T002", "GBPUSD", TradeSide.Sell, 500_000, "CUST02",
                       1.2700m, 1.2720m, DateTime.Today.AddMonths(3)),
    new FxSwapTrade("T003", "USDJPY", TradeSide.Buy, 2_000_000, "CUST01",
                    150.50m, 150.75m, DateTime.Today.AddMonths(1))
};

// Calculate portfolio P&L - polymorphic call
decimal totalPnL = portfolio.Sum(t => t.CalculatePnL(currentRates[t.CurrencyPair]));

// Get all cash flows - polymorphic call
var allCashFlows = portfolio.SelectMany(t => t.GetCashFlows());
```

---

### Example 2: Pre-Trade and Post-Trade Workflow Steps

```csharp
/// <summary>
/// Base class for workflow steps with common functionality.
/// </summary>
public abstract class WorkflowStep
{
    public string StepName { get; }
    public string StepId { get; }
    public WorkflowStepStatus Status { get; protected set; }
    public DateTime? StartTime { get; protected set; }
    public DateTime? EndTime { get; protected set; }
    public string? ErrorMessage { get; protected set; }

    protected WorkflowStep(string stepId, string stepName)
    {
        StepId = stepId;
        StepName = stepName;
        Status = WorkflowStepStatus.Pending;
    }

    // Template method pattern
    public async Task<WorkflowStepResult> ExecuteAsync(WorkflowContext context)
    {
        Status = WorkflowStepStatus.Running;
        StartTime = DateTime.UtcNow;

        try
        {
            // Pre-execution hook
            await OnBeforeExecuteAsync(context);

            // Main execution - implemented by derived classes
            var result = await ExecuteStepAsync(context);

            // Post-execution hook
            await OnAfterExecuteAsync(context, result);

            Status = result.Success
                ? WorkflowStepStatus.Completed
                : WorkflowStepStatus.Failed;

            return result;
        }
        catch (Exception ex)
        {
            Status = WorkflowStepStatus.Failed;
            ErrorMessage = ex.Message;
            return WorkflowStepResult.Failure(ex.Message);
        }
        finally
        {
            EndTime = DateTime.UtcNow;
        }
    }

    // Abstract - each step has different logic
    protected abstract Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext context);

    // Virtual hooks - can be overridden
    protected virtual Task OnBeforeExecuteAsync(WorkflowContext context)
        => Task.CompletedTask;
    protected virtual Task OnAfterExecuteAsync(WorkflowContext context, WorkflowStepResult result)
        => Task.CompletedTask;
}

// PRE-TRADE STEPS

public class CreditCheckStep : WorkflowStep
{
    private readonly ICreditService _creditService;

    public CreditCheckStep(ICreditService creditService)
        : base("PRE-001", "Credit Check")
    {
        _creditService = creditService;
    }

    protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext context)
    {
        var trade = context.Trade;
        var creditLimit = await _creditService.GetAvailableCreditAsync(trade.CounterpartyId);
        var exposure = trade.Notional * 0.05m;  // Simple exposure calc

        if (exposure > creditLimit)
        {
            return WorkflowStepResult.Failure(
                $"Credit check failed. Required: {exposure:N0}, Available: {creditLimit:N0}");
        }

        context.SetData("CreditExposure", exposure);
        return WorkflowStepResult.Success();
    }
}

public class ComplianceCheckStep : WorkflowStep
{
    private readonly IComplianceService _compliance;

    public ComplianceCheckStep(IComplianceService compliance)
        : base("PRE-002", "Compliance Check")
    {
        _compliance = compliance;
    }

    protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext context)
    {
        var trade = context.Trade;

        // Sanctions check
        var sanctionsResult = await _compliance.CheckSanctionsAsync(trade.CounterpartyId);
        if (!sanctionsResult.Passed)
            return WorkflowStepResult.Failure($"Sanctions check failed: {sanctionsResult.Reason}");

        // Trade restriction check
        var restrictionResult = await _compliance.CheckTradeRestrictionsAsync(
            trade.CurrencyPair, trade.Notional);
        if (!restrictionResult.Passed)
            return WorkflowStepResult.Failure($"Trade restriction: {restrictionResult.Reason}");

        return WorkflowStepResult.Success();
    }
}

// POST-TRADE STEPS

public class TradeConfirmationStep : WorkflowStep
{
    private readonly IConfirmationService _confirmationService;

    public TradeConfirmationStep(IConfirmationService confirmationService)
        : base("POST-001", "Trade Confirmation")
    {
        _confirmationService = confirmationService;
    }

    protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext context)
    {
        var confirmation = await _confirmationService.GenerateConfirmationAsync(context.Trade);
        await _confirmationService.SendConfirmationAsync(confirmation);

        context.SetData("ConfirmationId", confirmation.Id);
        return WorkflowStepResult.Success();
    }
}

public class SettlementInstructionStep : WorkflowStep
{
    private readonly ISettlementService _settlement;

    public SettlementInstructionStep(ISettlementService settlement)
        : base("POST-002", "Settlement Instruction")
    {
        _settlement = settlement;
    }

    protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext context)
    {
        var trade = context.Trade;
        var instructions = await _settlement.GenerateInstructionsAsync(trade);

        foreach (var instruction in instructions)
        {
            await _settlement.SubmitInstructionAsync(instruction);
        }

        return WorkflowStepResult.Success();
    }
}

public class RegulatoryReportingStep : WorkflowStep
{
    private readonly IRegulatoryReporter _reporter;

    public RegulatoryReportingStep(IRegulatoryReporter reporter)
        : base("POST-003", "Regulatory Reporting")
    {
        _reporter = reporter;
    }

    protected override async Task<WorkflowStepResult> ExecuteStepAsync(WorkflowContext context)
    {
        var trade = context.Trade;

        // MiFID II reporting
        await _reporter.ReportMifidAsync(trade);

        // EMIR reporting
        await _reporter.ReportEmirAsync(trade);

        // CFTC reporting (if applicable)
        if (IsUsFacing(trade))
            await _reporter.ReportCftcAsync(trade);

        return WorkflowStepResult.Success();
    }

    private bool IsUsFacing(FxTradeBase trade)
    {
        // Check if counterparty is US entity...
        return false;
    }
}

// Workflow engine - executes steps polymorphically
public class TradeWorkflowEngine
{
    public async Task<WorkflowResult> ExecuteWorkflowAsync(
        IEnumerable<WorkflowStep> steps,
        WorkflowContext context)
    {
        var results = new List<(WorkflowStep Step, WorkflowStepResult Result)>();

        foreach (var step in steps)
        {
            // Polymorphic execution - each step runs its own logic
            var result = await step.ExecuteAsync(context);
            results.Add((step, result));

            if (!result.Success)
            {
                // Stop workflow on failure
                return new WorkflowResult(false, results);
            }
        }

        return new WorkflowResult(true, results);
    }
}
```

---

## 5. Complete FX Trading System Example

```csharp
/// <summary>
/// Complete example showing all four OOP pillars working together
/// in an FX Quote Book system.
/// </summary>

// ENCAPSULATION: Quote Book with protected state
public class FxQuoteBook
{
    private readonly ConcurrentDictionary<string, SortedList<decimal, FxQuote>> _bids;
    private readonly ConcurrentDictionary<string, SortedList<decimal, FxQuote>> _asks;
    private readonly ReaderWriterLockSlim _lock;

    public FxQuoteBook()
    {
        _bids = new ConcurrentDictionary<string, SortedList<decimal, FxQuote>>();
        _asks = new ConcurrentDictionary<string, SortedList<decimal, FxQuote>>();
        _lock = new ReaderWriterLockSlim();
    }

    // Controlled access to add quotes
    public void AddQuote(FxQuote quote)
    {
        _lock.EnterWriteLock();
        try
        {
            // Add to appropriate side
            var bids = _bids.GetOrAdd(quote.CurrencyPair, _ => new SortedList<decimal, FxQuote>());
            var asks = _asks.GetOrAdd(quote.CurrencyPair, _ => new SortedList<decimal, FxQuote>());

            bids[quote.BidPrice] = quote;
            asks[quote.AskPrice] = quote;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    // Read-only access to best prices
    public (FxQuote? BestBid, FxQuote? BestAsk) GetBestQuotes(string currencyPair)
    {
        _lock.EnterReadLock();
        try
        {
            FxQuote? bestBid = null;
            FxQuote? bestAsk = null;

            if (_bids.TryGetValue(currencyPair, out var bids) && bids.Count > 0)
                bestBid = bids.Values[^1];  // Highest bid

            if (_asks.TryGetValue(currencyPair, out var asks) && asks.Count > 0)
                bestAsk = asks.Values[0];   // Lowest ask

            return (bestBid, bestAsk);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }
}

// ABSTRACTION: Liquidity Provider interface
public interface ILiquidityProvider
{
    string ProviderId { get; }
    Task<FxQuote> RequestQuoteAsync(string currencyPair, decimal amount);
    Task<ExecutionResult> ExecuteAsync(OrderRequest order);
}

// INHERITANCE: Base trade with specialized types
public abstract class Trade
{
    public string TradeId { get; protected set; }
    public abstract string ProductType { get; }
    public abstract decimal CalculateRisk();
}

public class SpotTrade : Trade
{
    public override string ProductType => "SPOT";
    public override decimal CalculateRisk() => Notional * 0.02m;
    public decimal Notional { get; set; }
}

public class ForwardTrade : Trade
{
    public override string ProductType => "FORWARD";
    public override decimal CalculateRisk() => Notional * DaysToMaturity * 0.0001m;
    public decimal Notional { get; set; }
    public int DaysToMaturity { get; set; }
}

// POLYMORPHISM: Rule-based spread management
public interface ISpreadRule
{
    string RuleName { get; }
    int Priority { get; }
    bool Applies(SpreadContext context);
    decimal CalculateSpread(SpreadContext context);
}

public class VolatilitySpreadRule : ISpreadRule
{
    public string RuleName => "Volatility";
    public int Priority => 1;

    public bool Applies(SpreadContext context)
        => context.Volatility > 0.02m;

    public decimal CalculateSpread(SpreadContext context)
        => context.BaseSpread * (1 + context.Volatility * 10);
}

public class CustomerTierSpreadRule : ISpreadRule
{
    public string RuleName => "CustomerTier";
    public int Priority => 2;

    public bool Applies(SpreadContext context)
        => context.Customer.Tier == CustomerTier.Premium;

    public decimal CalculateSpread(SpreadContext context)
        => context.BaseSpread * 0.8m;  // 20% discount
}

public class TimeOfDaySpreadRule : ISpreadRule
{
    public string RuleName => "TimeOfDay";
    public int Priority => 3;

    public bool Applies(SpreadContext context)
    {
        var hour = DateTime.UtcNow.Hour;
        return hour < 7 || hour > 20;  // Off-hours
    }

    public decimal CalculateSpread(SpreadContext context)
        => context.BaseSpread * 1.5m;  // 50% wider
}

// Spread engine using polymorphism
public class SpreadEngine
{
    private readonly List<ISpreadRule> _rules;

    public SpreadEngine(IEnumerable<ISpreadRule> rules)
    {
        _rules = rules.OrderBy(r => r.Priority).ToList();
    }

    public decimal CalculateSpread(SpreadContext context)
    {
        decimal spread = context.BaseSpread;

        foreach (var rule in _rules)
        {
            if (rule.Applies(context))
            {
                spread = rule.CalculateSpread(context with { BaseSpread = spread });
            }
        }

        return spread;
    }
}
```

---

## 6. Interview Q&A with Domain Examples

### Q1: Explain encapsulation with a trading example.

**Answer:** In our FX trading system, we encapsulate FxQuote prices with validation:

```csharp
public class FxQuote
{
    private decimal _bidPrice;
    private decimal _askPrice;

    public decimal BidPrice => _bidPrice;  // Read-only

    public void UpdatePrices(decimal bid, decimal ask)
    {
        // Validation: ask > bid, spread within limits, no fat finger
        if (ask <= bid) throw new InvalidQuoteException();
        _bidPrice = bid;
        _askPrice = ask;
    }
}
```

This prevents invalid states like negative spreads or incorrect prices, which could cause significant financial loss.

---

### Q2: How does abstraction help in integrating multiple liquidity providers?

**Answer:** We abstract LP integration behind `IMarketDataProvider`:

```csharp
public interface IMarketDataProvider
{
    Task<FxQuote> GetQuoteAsync(string pair);
}

// Reuters implementation has complex protocol handling
// Bloomberg implementation has different API
// Both hidden behind same interface

var quote = await _marketData.GetQuoteAsync("EURUSD");
// Consumer doesn't know if it's Reuters, Bloomberg, or cache
```

When we onboard a new LP (e.g., Refinitiv), we just implement the interface - no changes to business logic.

---

### Q3: Give a polymorphism example from trade validation.

**Answer:** Our validation engine uses polymorphic rules:

```csharp
interface ITradeValidationRule
{
    ValidationResult Validate(FxTrade trade);
}

// Different rules: CreditCheck, NotionalLimit, MiFID, Sanctions
// Engine doesn't know which rules exist:
foreach (var rule in _rules)
    rule.Validate(trade);  // Polymorphic call
```

Adding new regulatory rules (like new MiFID requirements) doesn't change the engine - just add a new rule class.

---

### Q4: How does inheritance help in FX trade types?

**Answer:** All FX trades share common attributes but differ in specifics:

```csharp
abstract class FxTradeBase
{
    // Common: TradeId, CurrencyPair, Notional, Counterparty
    public abstract decimal CalculatePnL(decimal currentRate);
}

class FxSpotTrade : FxTradeBase
{
    public override decimal CalculatePnL(decimal rate) => /* spot P&L */;
}

class FxForwardTrade : FxTradeBase
{
    public override decimal CalculatePnL(decimal rate) => /* includes forward points */;
}

class FxSwapTrade : FxTradeBase
{
    public override decimal CalculatePnL(decimal rate) => /* near + far leg */;
}

// Portfolio can hold any trade type
List<FxTradeBase> portfolio = GetTrades();
decimal totalPnL = portfolio.Sum(t => t.CalculatePnL(currentRate));
```

---

### Q5: How would you design a rule-based spread management system?

**Answer:** Using polymorphism with strategy pattern:

```csharp
interface ISpreadRule
{
    bool Applies(SpreadContext ctx);
    decimal CalculateSpread(SpreadContext ctx);
}

// Rules: Volatility, CustomerTier, TimeOfDay, LiquidityDepth
class SpreadEngine
{
    public decimal GetSpread(SpreadContext ctx)
    {
        decimal spread = baseSpread;
        foreach (var rule in _rules.Where(r => r.Applies(ctx)))
            spread = rule.CalculateSpread(ctx);
        return spread;
    }
}
```

Business users can add new spread rules without code changes to the engine.

---

_Document created for FX Trading domain interviews. February 2026_
