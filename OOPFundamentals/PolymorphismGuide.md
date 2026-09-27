# Polymorphism in C# – Complete Interview Guide

> **One of the Core Pillars of OOP**

---

## Quick Definition

**Polymorphism** = "Many Forms" - The ability of objects to take multiple forms. Same operation behaves differently on different types.

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    POLYMORPHISM AT A GLANCE                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Shape shape = new Circle();                                           │
│   shape.Draw();  // Which Draw() is called?                             │
│                                                                         │
│   Answer: Circle's Draw() - because actual object is Circle             │
│                                                                         │
│   This is RUNTIME POLYMORPHISM - behavior determined at runtime         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Types of Polymorphism

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    TYPES OF POLYMORPHISM                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│                        POLYMORPHISM                                     │
│                             │                                           │
│          ┌──────────────────┴──────────────────┐                       │
│          │                                     │                       │
│          ▼                                     ▼                       │
│  ┌───────────────────┐               ┌───────────────────┐             │
│  │   COMPILE-TIME    │               │    RUN-TIME       │             │
│  │   (Static Binding)│               │ (Dynamic Binding) │             │
│  │                   │               │                   │             │
│  │ Resolved by       │               │ Resolved by       │             │
│  │ COMPILER          │               │ CLR at RUNTIME    │             │
│  │                   │               │                   │             │
│  │ ┌───────────────┐ │               │ ┌───────────────┐ │             │
│  │ │ Method        │ │               │ │ Method        │ │             │
│  │ │ Overloading   │ │               │ │ Overriding    │ │             │
│  │ └───────────────┘ │               │ │ (virtual/     │ │             │
│  │ ┌───────────────┐ │               │ │  override)    │ │             │
│  │ │ Operator      │ │               │ └───────────────┘ │             │
│  │ │ Overloading   │ │               │ ┌───────────────┐ │             │
│  │ └───────────────┘ │               │ │ Interface     │ │             │
│  │ ┌───────────────┐ │               │ │ Implementation│ │             │
│  │ │ Generics      │ │               │ └───────────────┘ │             │
│  │ └───────────────┘ │               │ ┌───────────────┐ │             │
│  └───────────────────┘               │ │ Abstract      │ │             │
│                                      │ │ Methods       │ │             │
│                                      │ └───────────────┘ │             │
│                                      └───────────────────┘             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 1. Compile-Time Polymorphism

### 1.1 Method Overloading

**Same method name, different parameters** (number, type, or order)

```csharp
public class Calculator
{
    // Different NUMBER of parameters
    public int Add(int a, int b) => a + b;
    public int Add(int a, int b, int c) => a + b + c;
    public int Add(int a, int b, int c, int d) => a + b + c + d;

    // Different TYPES of parameters
    public double Add(double a, double b) => a + b;
    public decimal Add(decimal a, decimal b) => a + b;
    public string Add(string a, string b) => a + b;

    // Different ORDER of parameters
    public void Print(string msg, int times)
        => Console.WriteLine($"{msg} x {times}");
    public void Print(int times, string msg)
        => Console.WriteLine($"{times}: {msg}");
}

// Compiler decides which method to call based on arguments
var calc = new Calculator();
calc.Add(1, 2);           // int Add(int, int)
calc.Add(1, 2, 3);        // int Add(int, int, int)
calc.Add(1.5, 2.5);       // double Add(double, double)
calc.Add("Hello", "World"); // string Add(string, string)
```

**Key Rules:**

```csharp
// ❌ Return type alone is NOT enough
public int Process() => 1;
public double Process() => 1.0;  // COMPILE ERROR!

// ✅ ref/out/in count as different
public void Method(int x) { }
public void Method(ref int x) { }  // OK - different

// ⚠️ params can cause ambiguity
public void Log(params string[] messages) { }
public void Log(string first, string second) { }
Log("a", "b");  // Which one? - Calls the non-params version (more specific)
```

### 1.2 Operator Overloading

```csharp
public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    // Operator overloading
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot add different currencies");
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies");
        return new Money(a.Amount - b.Amount, a.Currency);
    }

    public static Money operator *(Money a, decimal multiplier)
    {
        return new Money(a.Amount * multiplier, a.Currency);
    }

    public static bool operator ==(Money a, Money b)
    {
        return a.Amount == b.Amount && a.Currency == b.Currency;
    }

    public static bool operator !=(Money a, Money b) => !(a == b);

    public override string ToString() => $"{Currency} {Amount:F2}";
}

// Usage
var price1 = new Money(100, "USD");
var price2 = new Money(50, "USD");

var total = price1 + price2;      // Money + Money
var discounted = total * 0.9m;    // Money * decimal
Console.WriteLine(total);         // "USD 150.00"
Console.WriteLine(discounted);    // "USD 135.00"
```

### 1.3 Why Called "Compile-Time"?

The compiler determines which method to call based on:

- Method signature
- Argument types and count
- At **compile time** - before the program runs

```csharp
Calculator calc = new Calculator();
calc.Add(1, 2);  // Compiler knows at compile time: calls Add(int, int)

// IL code shows direct method call - no runtime lookup
// call int Calculator::Add(int, int)
```

---

## 2. Runtime Polymorphism

### 2.1 Method Overriding (virtual/override)

```csharp
public class Animal
{
    public virtual void Speak()
    {
        Console.WriteLine("Animal makes a sound");
    }
}

public class Dog : Animal
{
    public override void Speak()
    {
        Console.WriteLine("Dog barks: Woof!");
    }
}

public class Cat : Animal
{
    public override void Speak()
    {
        Console.WriteLine("Cat meows: Meow!");
    }
}

// Runtime polymorphism in action
Animal[] animals = { new Dog(), new Cat(), new Animal() };

foreach (Animal animal in animals)
{
    animal.Speak();  // Which Speak() is called?
}
// Output:
// Dog barks: Woof!
// Cat meows: Meow!
// Animal makes a sound
```

### 2.2 How CLR Determines Which Method (Virtual Dispatch)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    VIRTUAL METHOD DISPATCH                              │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Animal animal = new Dog();                                            │
│   animal.Speak();                                                       │
│                                                                         │
│   STEP 1: Get the actual object's type (Dog)                            │
│   STEP 2: Look up Dog's method table (vtable)                           │
│   STEP 3: Find Speak() entry → points to Dog.Speak()                    │
│   STEP 4: Call Dog.Speak()                                              │
│                                                                         │
│   ┌─────────────┐                                                       │
│   │ Reference   │                                                       │
│   │ animal      │──────┐                                                │
│   │ Type: Animal│      │                                                │
│   └─────────────┘      │                                                │
│                        │                                                │
│                        ▼                                                │
│                  ┌─────────────┐     ┌─────────────────────┐           │
│                  │ Dog Object  │────>│ Dog Method Table    │           │
│                  │             │     │                     │           │
│                  │ Actual type:│     │ Speak → Dog.Speak() │           │
│                  │ Dog         │     │ ToString → ...      │           │
│                  └─────────────┘     └─────────────────────┘           │
│                                                                         │
│   The ACTUAL object type (Dog) determines the method called,            │
│   NOT the reference type (Animal)                                       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Abstract Methods

```csharp
public abstract class Shape
{
    // Abstract method - NO implementation
    public abstract double CalculateArea();
    public abstract double CalculatePerimeter();

    // Concrete method - shared implementation
    public void DisplayInfo()
    {
        Console.WriteLine($"Area: {CalculateArea():F2}");
        Console.WriteLine($"Perimeter: {CalculatePerimeter():F2}");
    }
}

public class Circle : Shape
{
    public double Radius { get; set; }

    public override double CalculateArea() => Math.PI * Radius * Radius;
    public override double CalculatePerimeter() => 2 * Math.PI * Radius;
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public override double CalculateArea() => Width * Height;
    public override double CalculatePerimeter() => 2 * (Width + Height);
}

// Polymorphism with abstract class
Shape[] shapes = { new Circle { Radius = 5 }, new Rectangle { Width = 4, Height = 6 } };

foreach (Shape shape in shapes)
{
    shape.DisplayInfo();  // Calls the correct CalculateArea/Perimeter
}
```

### 2.4 Interface Polymorphism

```csharp
public interface IPaymentProcessor
{
    PaymentResult Process(decimal amount);
}

public class CreditCardProcessor : IPaymentProcessor
{
    public PaymentResult Process(decimal amount)
    {
        Console.WriteLine($"Processing ${amount} via Credit Card");
        return new PaymentResult { Success = true };
    }
}

public class PayPalProcessor : IPaymentProcessor
{
    public PaymentResult Process(decimal amount)
    {
        Console.WriteLine($"Processing ${amount} via PayPal");
        return new PaymentResult { Success = true };
    }
}

public class CryptoProcessor : IPaymentProcessor
{
    public PaymentResult Process(decimal amount)
    {
        Console.WriteLine($"Processing ${amount} via Cryptocurrency");
        return new PaymentResult { Success = true };
    }
}

// Polymorphism with interfaces
public class PaymentService
{
    public PaymentResult ProcessPayment(IPaymentProcessor processor, decimal amount)
    {
        // Doesn't know or care which processor it is
        return processor.Process(amount);
    }
}

// Usage
var service = new PaymentService();
service.ProcessPayment(new CreditCardProcessor(), 100);  // Credit card flow
service.ProcessPayment(new PayPalProcessor(), 100);      // PayPal flow
service.ProcessPayment(new CryptoProcessor(), 100);      // Crypto flow
```

---

## 3. Key Differences Summarized

### Compile-Time vs Runtime

| Aspect           | Compile-Time                        | Runtime                       |
| ---------------- | ----------------------------------- | ----------------------------- |
| **When decided** | At compilation                      | During execution              |
| **Mechanism**    | Method signature matching           | Virtual dispatch (vtable)     |
| **Keywords**     | Method name + params                | virtual, override, abstract   |
| **Performance**  | Faster (direct call)                | Slight overhead (indirection) |
| **Flexibility**  | Less flexible                       | More flexible                 |
| **Example**      | Add(int, int) vs Add(int, int, int) | Dog.Speak() vs Cat.Speak()    |

### Overloading vs Overriding

| Aspect              | Overloading                    | Overriding                       |
| ------------------- | ------------------------------ | -------------------------------- |
| **Methods**         | Same name, different signature | Same name, same signature        |
| **Inheritance**     | Not required                   | Required (base-derived)          |
| **Keywords**        | None required                  | virtual + override               |
| **Binding**         | Compile-time                   | Runtime                          |
| **Return type**     | Can differ                     | Must be same (or covariant)      |
| **Access modifier** | Can differ                     | Must be same or less restrictive |

---

## 4. Practical Examples

### 4.1 Factory Pattern with Polymorphism

```csharp
public abstract class Document
{
    public abstract void Create();
    public abstract void Open();
    public abstract void Save();
}

public class PdfDocument : Document
{
    public override void Create() => Console.WriteLine("Creating PDF");
    public override void Open() => Console.WriteLine("Opening PDF in reader");
    public override void Save() => Console.WriteLine("Saving as .pdf");
}

public class WordDocument : Document
{
    public override void Create() => Console.WriteLine("Creating Word doc");
    public override void Open() => Console.WriteLine("Opening in Word");
    public override void Save() => Console.WriteLine("Saving as .docx");
}

public class ExcelDocument : Document
{
    public override void Create() => Console.WriteLine("Creating Excel");
    public override void Open() => Console.WriteLine("Opening in Excel");
    public override void Save() => Console.WriteLine("Saving as .xlsx");
}

// Factory method
public static class DocumentFactory
{
    public static Document Create(string type)
    {
        return type.ToLower() switch
        {
            "pdf" => new PdfDocument(),
            "word" => new WordDocument(),
            "excel" => new ExcelDocument(),
            _ => throw new ArgumentException("Unknown document type")
        };
    }
}

// Usage - polymorphism allows uniform handling
Document doc = DocumentFactory.Create("pdf");
doc.Create();
doc.Save();
// Works the same way regardless of document type!
```

### 4.2 Strategy Pattern

```csharp
public interface ISortStrategy
{
    void Sort(int[] array);
}

public class BubbleSort : ISortStrategy
{
    public void Sort(int[] array)
    {
        Console.WriteLine("Sorting using Bubble Sort");
        // Bubble sort implementation
    }
}

public class QuickSort : ISortStrategy
{
    public void Sort(int[] array)
    {
        Console.WriteLine("Sorting using Quick Sort");
        // Quick sort implementation
    }
}

public class MergeSort : ISortStrategy
{
    public void Sort(int[] array)
    {
        Console.WriteLine("Sorting using Merge Sort");
        // Merge sort implementation
    }
}

// Context class using polymorphism
public class Sorter
{
    private ISortStrategy _strategy;

    public void SetStrategy(ISortStrategy strategy)
    {
        _strategy = strategy;
    }

    public void SortArray(int[] array)
    {
        _strategy.Sort(array);  // Polymorphic call
    }
}

// Usage
var sorter = new Sorter();
int[] data = { 5, 2, 8, 1, 9 };

sorter.SetStrategy(new BubbleSort());
sorter.SortArray(data);  // Uses bubble sort

sorter.SetStrategy(new QuickSort());
sorter.SortArray(data);  // Uses quick sort
```

### 4.3 Plugin System

```csharp
public interface IPlugin
{
    string Name { get; }
    void Execute();
}

public class AuthPlugin : IPlugin
{
    public string Name => "Authentication";
    public void Execute() => Console.WriteLine("Authenticating user...");
}

public class LoggingPlugin : IPlugin
{
    public string Name => "Logging";
    public void Execute() => Console.WriteLine("Logging request...");
}

public class CachePlugin : IPlugin
{
    public string Name => "Caching";
    public void Execute() => Console.WriteLine("Checking cache...");
}

// Plugin manager - uses polymorphism
public class PluginManager
{
    private readonly List<IPlugin> _plugins = new();

    public void Register(IPlugin plugin) => _plugins.Add(plugin);

    public void ExecuteAll()
    {
        foreach (var plugin in _plugins)
        {
            Console.WriteLine($"Running {plugin.Name}...");
            plugin.Execute();  // Polymorphic call
        }
    }
}

// Usage
var manager = new PluginManager();
manager.Register(new AuthPlugin());
manager.Register(new LoggingPlugin());
manager.Register(new CachePlugin());

manager.ExecuteAll();  // Each plugin executes its own logic
```

---

## 5. Interview Questions

### Q1: What is polymorphism? Give a real-world example.

**Answer:** Polymorphism means "many forms" - the ability of objects to be treated as instances of their parent class while executing behavior specific to their actual type.

**Real-world example:** A universal remote control (interface) can control TV, AC, or Music System (different implementations). Same button "Power" does different things based on what device is selected.

---

### Q2: Difference between compile-time and runtime polymorphism?

**Answer:**

- **Compile-time**: Method to call is decided by compiler based on method signature (overloading)
- **Runtime**: Method to call is decided by CLR at runtime based on actual object type (overriding)

```csharp
// Compile-time - compiler knows which Add to call
calc.Add(1, 2);  // int version
calc.Add(1.0, 2.0);  // double version

// Runtime - CLR decides at runtime
Animal a = new Dog();
a.Speak();  // Dog.Speak() called (decided at runtime)
```

---

### Q3: Can we achieve polymorphism without inheritance?

**Answer:** Yes! Using **interfaces**. Classes implementing the same interface can be used polymorphically without any inheritance relationship.

```csharp
public interface IDrawable
{
    void Draw();
}

public class Circle : IDrawable { public void Draw() { } }
public class Car : IDrawable { public void Draw() { } }  // Unrelated to Circle!

IDrawable[] items = { new Circle(), new Car() };
foreach (var item in items) item.Draw();  // Polymorphism!
```

---

### Q4: What is covariant return type?

**Answer:** C# 9+ allows overriding methods to return a more derived type than the base method.

```csharp
public class Animal { }
public class Dog : Animal { }

public class AnimalFactory
{
    public virtual Animal Create() => new Animal();
}

public class DogFactory : AnimalFactory
{
    public override Dog Create() => new Dog();  // Covariant return (C# 9+)
}
```

---

### Q5: Why is runtime polymorphism slower than compile-time?

**Answer:** Runtime polymorphism requires:

1. Looking up the actual object type
2. Finding the method table (vtable)
3. Locating the correct method entry
4. Making an indirect call

Compile-time polymorphism is a direct call - the method address is known at compile time.

**However**, the difference is negligible in modern systems. Don't avoid polymorphism for performance reasons - design comes first.

---

### Q6: What is the 'new' keyword in context of polymorphism?

**Answer:** The `new` keyword **hides** a base class method instead of overriding it. This **breaks** polymorphism.

```csharp
class Base { public void Method() { Console.WriteLine("Base"); } }
class Derived : Base { public new void Method() { Console.WriteLine("Derived"); } }

Base b = new Derived();
b.Method();  // "Base" - NOT polymorphic!
```

Use `virtual/override` for true polymorphism.

---

_Document created for interview preparation. February 2026_
