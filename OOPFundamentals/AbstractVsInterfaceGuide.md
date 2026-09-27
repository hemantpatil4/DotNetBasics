# Abstract Class vs Interface – Complete Interview Guide

> **The #1 Most Asked OOP Interview Question**

---

## Quick Answer (For Interviews)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    30-SECOND ANSWER                                     │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ABSTRACT CLASS = "What something IS" (IS-A relationship)               │
│  • Use when classes share common code/state                             │
│  • Dog IS-A Animal                                                      │
│                                                                         │
│  INTERFACE = "What something CAN DO" (HAS-A capability)                 │
│  • Use when unrelated classes need same behavior                        │
│  • Bird, Airplane, Superman all CAN fly (IFlyable)                      │
│                                                                         │
│  KEY DIFFERENCE:                                                        │
│  • Abstract: Single inheritance only                                    │
│  • Interface: Multiple inheritance allowed                              │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Detailed Comparison

### Feature-by-Feature Table

| Feature                         | Abstract Class           | Interface                    |
| ------------------------------- | ------------------------ | ---------------------------- |
| **Inheritance**                 | Single only              | Multiple allowed             |
| **Fields**                      | ✅ Yes (any type)        | ❌ No (only constants)       |
| **Constructors**                | ✅ Yes                   | ❌ No                        |
| **Method implementation**       | ✅ Yes                   | ✅ Yes (C# 8+ default)       |
| **Access modifiers on members** | Any                      | public (C# 8+ allows others) |
| **Static members**              | ✅ Yes                   | ✅ Yes (C# 8+)               |
| **Properties**                  | ✅ Full implementation   | ✅ Signature only\*          |
| **Events**                      | ✅ Yes                   | ✅ Yes                       |
| **Purpose**                     | Base for related classes | Contract for capability      |
| **When to add members**         | Easy (non-breaking)      | Breaking change\*\*          |

\*Properties in interfaces can have default implementation in C# 8+
\*\*Adding abstract member to interface breaks all implementers

---

## When to Use Abstract Class

### 1. Shared Code Among Related Classes

```csharp
// All database entities share common behavior
public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? ModifiedAt { get; protected set; }

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    // Shared validation logic
    public virtual bool IsNew() => Id == 0;

    // Each entity validates differently
    public abstract bool Validate();
}

public class User : Entity
{
    public string Name { get; set; }
    public string Email { get; set; }

    public override bool Validate()
    {
        return !string.IsNullOrEmpty(Name) && Email.Contains("@");
    }
}

public class Product : Entity
{
    public string Title { get; set; }
    public decimal Price { get; set; }

    public override bool Validate()
    {
        return !string.IsNullOrEmpty(Title) && Price > 0;
    }
}
```

### 2. State/Fields Required

```csharp
public abstract class GameCharacter
{
    // Abstract class can have fields!
    protected int _health = 100;
    protected int _mana = 50;
    protected readonly string _name;

    protected GameCharacter(string name)
    {
        _name = name;
    }

    public bool IsAlive => _health > 0;

    public virtual void TakeDamage(int damage)
    {
        _health -= damage;
        Console.WriteLine($"{_name} took {damage} damage. Health: {_health}");
    }

    public abstract void Attack(GameCharacter target);
}

public class Warrior : GameCharacter
{
    public Warrior(string name) : base(name) { }

    public override void Attack(GameCharacter target)
    {
        Console.WriteLine($"{_name} swings sword!");
        target.TakeDamage(25);
    }
}

public class Mage : GameCharacter
{
    public Mage(string name) : base(name) { }

    public override void Attack(GameCharacter target)
    {
        if (_mana >= 10)
        {
            _mana -= 10;
            Console.WriteLine($"{_name} casts fireball!");
            target.TakeDamage(40);
        }
        else
        {
            Console.WriteLine($"{_name} is out of mana!");
        }
    }
}
```

### 3. Template Method Pattern

```csharp
public abstract class DataProcessor
{
    // Template method - defines the algorithm structure
    public void Process()
    {
        var data = FetchData();      // Step 1
        var validated = Validate(data);  // Step 2
        var transformed = Transform(validated);  // Step 3
        Save(transformed);           // Step 4
        NotifyComplete();            // Step 5
    }

    // Abstract - derived classes must implement
    protected abstract string FetchData();
    protected abstract string Transform(string data);

    // Virtual - can be overridden if needed
    protected virtual string Validate(string data)
    {
        if (string.IsNullOrEmpty(data))
            throw new InvalidDataException("Data cannot be empty");
        return data;
    }

    // Concrete - shared implementation
    protected void Save(string data)
    {
        File.WriteAllText("output.txt", data);
    }

    protected void NotifyComplete()
    {
        Console.WriteLine("Processing complete!");
    }
}
```

---

## When to Use Interface

### 1. Unrelated Classes Need Same Behavior

```csharp
// These classes have NOTHING in common except they can be compared
public interface IComparable<T>
{
    int CompareTo(T other);
}

// Completely unrelated classes implementing same interface
public class Person : IComparable<Person>
{
    public string Name { get; set; }
    public int Age { get; set; }

    public int CompareTo(Person other) => Age.CompareTo(other.Age);
}

public class Product : IComparable<Product>
{
    public string Name { get; set; }
    public decimal Price { get; set; }

    public int CompareTo(Product other) => Price.CompareTo(other.Price);
}

public class File : IComparable<File>
{
    public string Path { get; set; }
    public long Size { get; set; }

    public int CompareTo(File other) => Size.CompareTo(other.Size);
}
```

### 2. Multiple Inheritance Needed

```csharp
public interface ISerializable
{
    byte[] Serialize();
    void Deserialize(byte[] data);
}

public interface IValidatable
{
    bool IsValid();
    IEnumerable<string> GetValidationErrors();
}

public interface IAuditable
{
    DateTime CreatedAt { get; }
    DateTime? ModifiedAt { get; }
    string CreatedBy { get; }
}

// One class, multiple capabilities
public class Order : ISerializable, IValidatable, IAuditable
{
    public int Id { get; set; }
    public List<OrderItem> Items { get; set; }

    // IAuditable
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; private set; }
    public string CreatedBy { get; init; }

    // ISerializable
    public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes(this);
    public void Deserialize(byte[] data) { /* ... */ }

    // IValidatable
    public bool IsValid() => Items?.Count > 0;
    public IEnumerable<string> GetValidationErrors()
    {
        if (Items == null || Items.Count == 0)
            yield return "Order must have at least one item";
    }
}
```

### 3. Dependency Injection & Testing

```csharp
// Interface for abstraction
public interface IEmailService
{
    Task SendAsync(string to, string subject, string body);
}

public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task SaveAsync(User user);
}

// Production implementation
public class SmtpEmailService : IEmailService
{
    public async Task SendAsync(string to, string subject, string body)
    {
        // Real SMTP logic
    }
}

// Test mock
public class MockEmailService : IEmailService
{
    public List<(string To, string Subject, string Body)> SentEmails { get; } = new();

    public Task SendAsync(string to, string subject, string body)
    {
        SentEmails.Add((to, subject, body));
        return Task.CompletedTask;
    }
}

// Usage with DI
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;

    public UserService(IUserRepository repository, IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task RegisterAsync(User user)
    {
        await _repository.SaveAsync(user);
        await _emailService.SendAsync(user.Email, "Welcome!", "Thanks for registering");
    }
}
```

### 4. Plugin Architecture

```csharp
// Plugin interface
public interface IPlugin
{
    string Name { get; }
    string Version { get; }
    void Initialize();
    void Execute();
    void Shutdown();
}

// Plugin manager works with any plugin
public class PluginManager
{
    private readonly List<IPlugin> _plugins = new();

    public void RegisterPlugin(IPlugin plugin)
    {
        _plugins.Add(plugin);
        plugin.Initialize();
    }

    public void ExecuteAll()
    {
        foreach (var plugin in _plugins)
        {
            Console.WriteLine($"Executing {plugin.Name} v{plugin.Version}");
            plugin.Execute();
        }
    }
}

// Plugins can be anything
public class LoggingPlugin : IPlugin
{
    public string Name => "Logger";
    public string Version => "1.0";
    public void Initialize() => Console.WriteLine("Logger initialized");
    public void Execute() => Console.WriteLine("Logging...");
    public void Shutdown() => Console.WriteLine("Logger shutdown");
}

public class CachePlugin : IPlugin
{
    public string Name => "Cache";
    public string Version => "2.1";
    public void Initialize() => Console.WriteLine("Cache initialized");
    public void Execute() => Console.WriteLine("Caching...");
    public void Shutdown() => Console.WriteLine("Cache cleared");
}
```

---

## Decision Flowchart

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    WHICH ONE TO USE?                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   START                                                                 │
│     │                                                                   │
│     ▼                                                                   │
│   ┌─────────────────────────────────────────┐                          │
│   │ Do you need to share code/implementation │                          │
│   │ among related classes?                   │                          │
│   └────────────────┬────────────────────────┘                          │
│                    │                                                    │
│         ┌─────────┴─────────┐                                          │
│         │                   │                                          │
│        YES                  NO                                         │
│         │                   │                                          │
│         ▼                   ▼                                          │
│   ┌───────────┐    ┌─────────────────────────────────┐                │
│   │ ABSTRACT  │    │ Need multiple inheritance?       │                │
│   │ CLASS     │    └───────────────┬─────────────────┘                │
│   └───────────┘                    │                                   │
│                         ┌──────────┴──────────┐                        │
│                         │                     │                        │
│                        YES                    NO                       │
│                         │                     │                        │
│                         ▼                     ▼                        │
│                   ┌───────────┐    ┌─────────────────────────┐        │
│                   │ INTERFACE │    │ Is it a capability that  │        │
│                   └───────────┘    │ unrelated classes share? │        │
│                                    └────────────┬────────────┘        │
│                                                 │                      │
│                                      ┌──────────┴──────────┐          │
│                                      │                     │          │
│                                     YES                    NO         │
│                                      │                     │          │
│                                      ▼                     ▼          │
│                                ┌───────────┐    ┌─────────────────┐  │
│                                │ INTERFACE │    │ Either works.   │  │
│                                └───────────┘    │ Prefer INTERFACE│  │
│                                                 │ for flexibility │  │
│                                                 └─────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Real-World Example: Combining Both

```csharp
// INTERFACES: Define capabilities
public interface ISerializable
{
    string Serialize();
}

public interface IPersistable
{
    Task SaveAsync();
    Task LoadAsync(int id);
}

public interface INotifiable
{
    void SendNotification(string message);
}

// ABSTRACT CLASS: Common implementation for related classes
public abstract class Entity : ISerializable
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    // Shared implementation
    public virtual string Serialize()
    {
        return JsonSerializer.Serialize(this, GetType());
    }

    // Abstract - each entity has different validation
    public abstract bool Validate();
}

// CONCRETE CLASSES: Combine abstract class + interfaces
public class User : Entity, IPersistable, INotifiable
{
    public string Name { get; set; }
    public string Email { get; set; }

    public override bool Validate()
        => !string.IsNullOrEmpty(Name) && Email.Contains("@");

    public async Task SaveAsync()
        => await Database.SaveUserAsync(this);

    public async Task LoadAsync(int id)
        => /* load logic */;

    public void SendNotification(string message)
        => EmailService.Send(Email, message);
}

public class Product : Entity, IPersistable
{
    public string Title { get; set; }
    public decimal Price { get; set; }

    public override bool Validate()
        => !string.IsNullOrEmpty(Title) && Price > 0;

    public async Task SaveAsync()
        => await Database.SaveProductAsync(this);

    public async Task LoadAsync(int id)
        => /* load logic */;
}
```

---

## Interview Questions

### Q1: Can you have both abstract class and interface together?

**Answer:** Yes! A class can inherit from ONE abstract class and implement MULTIPLE interfaces. This is a common and powerful pattern.

```csharp
public abstract class Vehicle { }
public interface IDriveable { }
public interface IFuelable { }

public class Car : Vehicle, IDriveable, IFuelable { }
```

---

### Q2: What happens if interface and abstract class have same method?

**Answer:** The derived class must implement both. If the abstract class's method satisfies the interface signature, it can serve as the implementation.

```csharp
public interface IProcessor
{
    void Process();
}

public abstract class BaseProcessor : IProcessor
{
    public abstract void Process();  // Satisfies interface requirement
}

public class ConcreteProcessor : BaseProcessor
{
    public override void Process()  // Implements both
    {
        Console.WriteLine("Processing...");
    }
}
```

---

### Q3: Can abstract class implement interface partially?

**Answer:** Yes! Abstract class can implement some interface methods and leave others abstract.

```csharp
public interface IDataService
{
    void Load();
    void Save();
    void Delete();
}

public abstract class BaseDataService : IDataService
{
    public void Load() => Console.WriteLine("Loading...");  // Implemented
    public abstract void Save();  // Left abstract
    public abstract void Delete();  // Left abstract
}
```

---

### Q4: Why use interface when abstract class can do everything now (C# 8+)?

**Answer:**

1. **Multiple inheritance** - Still only interfaces allow this
2. **Semantic clarity** - Interface = capability, Abstract = type
3. **Lighter weight** - No state/fields overhead
4. **Better decoupling** - Less tight coupling than inheritance
5. **Testing** - Easier to mock interfaces

---

### Q5: What are default interface methods (C# 8+) and when to use them?

**Answer:** Default methods allow adding new methods to interfaces without breaking existing implementations.

```csharp
public interface ILogger
{
    void Log(string message);

    // Default implementation - added without breaking existing code
    void LogError(string message) => Log($"ERROR: {message}");
    void LogWarning(string message) => Log($"WARNING: {message}");
}

// Old implementation still works - doesn't need to implement new methods
public class ConsoleLogger : ILogger
{
    public void Log(string message) => Console.WriteLine(message);
    // LogError and LogWarning use default implementation
}
```

**Use cases:**

- Evolving APIs without breaking changes
- Providing optional functionality
- Trait-like composition

---

_Document created for interview preparation. February 2026_
