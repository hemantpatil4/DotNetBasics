# OOP Fundamentals in C# – Complete Interview Guide

> **Target Audience:** 3+ years .NET developer preparing for product-based company interviews

---

## Table of Contents

1. [Four Pillars of OOP](#1-four-pillars-of-oop)
2. [Polymorphism Deep Dive](#2-polymorphism-deep-dive)
3. [Virtual and Override](#3-virtual-and-override)
4. [Abstract Classes](#4-abstract-classes)
5. [Interfaces](#5-interfaces)
6. [Abstract Class vs Interface](#6-abstract-class-vs-interface)
7. [Method Hiding (new keyword)](#7-method-hiding-new-keyword)
8. [Sealed Keyword](#8-sealed-keyword)
9. [Static Keyword](#9-static-keyword)
10. [Partial Keyword](#10-partial-keyword)
11. [readonly vs const](#11-readonly-vs-const)
12. [Important Keywords Summary](#12-important-keywords-summary)
13. [Interview Questions](#13-interview-questions)

---

## 1. Four Pillars of OOP

### 1.1 Overview

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    FOUR PILLARS OF OOP                                  │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐   │
│   │ENCAPSULATION│  │ INHERITANCE │  │POLYMORPHISM │  │ ABSTRACTION │   │
│   └─────────────┘  └─────────────┘  └─────────────┘  └─────────────┘   │
│         │                │                │                │           │
│         ▼                ▼                ▼                ▼           │
│   Hide internal     Reuse code       One interface,   Hide complex    │
│   details behind    from parent      multiple         implementation  │
│   public interface  classes          implementations                   │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.2 Encapsulation

**Definition:** Bundling data (fields) and methods that operate on that data within a single unit (class), and restricting direct access to internal details.

```csharp
// ❌ Without Encapsulation - data exposed
public class BankAccount
{
    public decimal balance;  // Anyone can modify!
}

var account = new BankAccount();
account.balance = -1000000;  // Invalid state allowed!

// ✅ With Encapsulation - data protected
public class BankAccount
{
    private decimal _balance;  // Hidden

    public decimal Balance => _balance;  // Read-only access

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        _balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        if (amount > _balance)
            throw new InvalidOperationException("Insufficient funds");
        _balance -= amount;
    }
}
```

**Key Benefits:**

- Data validation before modification
- Internal implementation can change without affecting consumers
- Prevents invalid states

### 1.3 Inheritance

**Definition:** Mechanism where a new class (derived/child) acquires properties and behaviors of an existing class (base/parent).

```csharp
// Base class
public class Animal
{
    public string Name { get; set; }

    public void Eat()
    {
        Console.WriteLine($"{Name} is eating");
    }
}

// Derived class
public class Dog : Animal
{
    public void Bark()
    {
        Console.WriteLine($"{Name} says: Woof!");
    }
}

// Usage
var dog = new Dog { Name = "Rex" };
dog.Eat();   // Inherited from Animal
dog.Bark();  // Defined in Dog
```

**Types of Inheritance in C#:**

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    INHERITANCE TYPES                                    │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   SINGLE INHERITANCE (Supported)     MULTIPLE CLASS INHERITANCE (❌)   │
│   ┌───────┐                          ┌───────┐   ┌───────┐             │
│   │   A   │                          │   A   │   │   B   │             │
│   └───┬───┘                          └───┬───┘   └───┬───┘             │
│       │                                  └─────┬─────┘                 │
│       ▼                                        ▼                       │
│   ┌───────┐                              ┌───────┐                     │
│   │   B   │                              │   C   │  NOT ALLOWED!       │
│   └───────┘                              └───────┘                     │
│                                                                         │
│   MULTILEVEL (Supported)             MULTIPLE INTERFACE (✅)           │
│   ┌───────┐                          ┌───────┐   ┌───────┐             │
│   │   A   │                          │  IA   │   │  IB   │             │
│   └───┬───┘                          └───┬───┘   └───┬───┘             │
│       │                                  └─────┬─────┘                 │
│       ▼                                        ▼                       │
│   ┌───────┐                              ┌───────┐                     │
│   │   B   │                              │   C   │  ALLOWED!           │
│   └───┬───┘                              └───────┘                     │
│       │                                                                │
│       ▼                                                                │
│   ┌───────┐                                                            │
│   │   C   │                                                            │
│   └───────┘                                                            │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 1.4 Polymorphism

**Definition:** Ability of objects to take multiple forms. Same method call behaves differently based on the object type.

```csharp
public class Shape
{
    public virtual double GetArea() => 0;
}

public class Circle : Shape
{
    public double Radius { get; set; }
    public override double GetArea() => Math.PI * Radius * Radius;
}

public class Rectangle : Shape
{
    public double Width { get; set; }
    public double Height { get; set; }
    public override double GetArea() => Width * Height;
}

// Polymorphism in action
Shape[] shapes = { new Circle { Radius = 5 }, new Rectangle { Width = 4, Height = 6 } };

foreach (Shape shape in shapes)
{
    Console.WriteLine(shape.GetArea());  // Different behavior for each type!
}
```

### 1.5 Abstraction

**Definition:** Hiding complex implementation details and showing only necessary features to the user.

```csharp
// User sees only this simple interface
public interface IEmailService
{
    void SendEmail(string to, string subject, string body);
}

// Complex implementation hidden
public class SmtpEmailService : IEmailService
{
    public void SendEmail(string to, string subject, string body)
    {
        // Complex SMTP setup, authentication, connection pooling,
        // retry logic, error handling - all hidden from user
        ConnectToServer();
        Authenticate();
        CreateMessage(to, subject, body);
        Send();
        Disconnect();
    }

    private void ConnectToServer() { /* ... */ }
    private void Authenticate() { /* ... */ }
    private void CreateMessage(string to, string subject, string body) { /* ... */ }
    private void Send() { /* ... */ }
    private void Disconnect() { /* ... */ }
}

// Usage - simple!
IEmailService email = new SmtpEmailService();
email.SendEmail("user@example.com", "Hello", "World");
```

---

## 2. Polymorphism Deep Dive

### 2.1 Types of Polymorphism

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    TYPES OF POLYMORPHISM                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│                        POLYMORPHISM                                     │
│                             │                                           │
│              ┌──────────────┴──────────────┐                           │
│              │                             │                           │
│              ▼                             ▼                           │
│   ┌─────────────────────┐       ┌─────────────────────┐               │
│   │ COMPILE-TIME        │       │ RUNTIME             │               │
│   │ (Static/Early)      │       │ (Dynamic/Late)      │               │
│   │                     │       │                     │               │
│   │ • Method Overloading│       │ • Method Overriding │               │
│   │ • Operator Overload │       │ • Virtual methods   │               │
│   │ • Generics          │       │ • Abstract methods  │               │
│   │                     │       │ • Interface methods │               │
│   │ Resolved at         │       │ Resolved at         │               │
│   │ COMPILE time        │       │ RUNTIME             │               │
│   └─────────────────────┘       └─────────────────────┘               │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Compile-Time Polymorphism (Method Overloading)

**Definition:** Same method name, different parameters (number, type, or order).

```csharp
public class Calculator
{
    // Different number of parameters
    public int Add(int a, int b) => a + b;
    public int Add(int a, int b, int c) => a + b + c;

    // Different parameter types
    public double Add(double a, double b) => a + b;
    public string Add(string a, string b) => a + b;

    // Different parameter order
    public void Display(string message, int count)
        => Console.WriteLine($"{message} x {count}");
    public void Display(int count, string message)
        => Console.WriteLine($"{count}: {message}");
}

// Compiler decides which method to call based on arguments
var calc = new Calculator();
calc.Add(1, 2);           // Calls Add(int, int)
calc.Add(1, 2, 3);        // Calls Add(int, int, int)
calc.Add(1.5, 2.5);       // Calls Add(double, double)
calc.Add("Hello", "World");  // Calls Add(string, string)
```

**Key Rules:**

- Return type alone is NOT enough to distinguish overloads
- `ref`, `out`, `in` are considered different from normal parameters
- Optional parameters can cause ambiguity

```csharp
// ❌ Invalid - differs only by return type
public int Calculate() => 1;
public double Calculate() => 1.0;  // COMPILE ERROR!

// ✅ Valid - ref/out are different
public void Process(int x) { }
public void Process(ref int x) { }

// ⚠️ Ambiguity with optional parameters
public void Log(string msg) { }
public void Log(string msg, int level = 0) { }
Log("Hello");  // Which one? Calls first (more specific)
```

### 2.3 Runtime Polymorphism (Method Overriding)

**Definition:** Derived class provides specific implementation of a method already defined in base class using `virtual` and `override`.

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

// Runtime polymorphism - method called depends on ACTUAL object type
Animal animal1 = new Dog();
Animal animal2 = new Cat();
Animal animal3 = new Animal();

animal1.Speak();  // "Dog barks: Woof!"
animal2.Speak();  // "Cat meows: Meow!"
animal3.Speak();  // "Animal makes a sound"
```

### 2.4 How Virtual Dispatch Works (CLR Internals)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    VIRTUAL METHOD TABLE (VTABLE)                        │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Animal animal = new Dog();                                            │
│   animal.Speak();  // How does CLR know to call Dog.Speak()?            │
│                                                                         │
│   STACK                    HEAP                                         │
│   ┌─────────────┐          ┌─────────────────────────────────┐         │
│   │   animal    │─────────>│     Dog Object                  │         │
│   │ (reference) │          │  ┌─────────────────────────────┐│         │
│   └─────────────┘          │  │ Type Handle ────────────┐   ││         │
│                            │  │ ...fields...            │   ││         │
│                            │  └─────────────────────────┼───┘│         │
│                            └────────────────────────────┼────┘         │
│                                                         │               │
│                            ┌────────────────────────────▼────┐         │
│                            │    Dog Method Table             │         │
│                            │  ┌─────────────────────────────┐│         │
│                            │  │ Speak() ──> Dog.Speak       ││         │
│                            │  │ ToString() ──> Object.ToString│        │
│                            │  │ ...                         ││         │
│                            │  └─────────────────────────────┘│         │
│                            └─────────────────────────────────┘         │
│                                                                         │
│   CLR looks up the ACTUAL object's method table at runtime              │
│   to find which implementation to call                                  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Virtual and Override

### 3.1 The `virtual` Keyword

**Definition:** Marks a method/property as overridable in derived classes.

```csharp
public class Vehicle
{
    // Virtual method - CAN be overridden
    public virtual void Start()
    {
        Console.WriteLine("Vehicle starting...");
    }

    // Non-virtual method - CANNOT be overridden (only hidden)
    public void Stop()
    {
        Console.WriteLine("Vehicle stopping...");
    }

    // Virtual property
    public virtual int MaxSpeed => 100;
}
```

**What can be virtual?**

- ✅ Instance methods
- ✅ Properties (get/set)
- ✅ Indexers
- ✅ Events
- ❌ Static members
- ❌ Fields
- ❌ Constructors

### 3.2 The `override` Keyword

**Definition:** Provides a new implementation of a virtual/abstract member inherited from base class.

```csharp
public class Car : Vehicle
{
    // Override virtual method
    public override void Start()
    {
        Console.WriteLine("Car engine starting... vroom!");
    }

    // Override virtual property
    public override int MaxSpeed => 200;
}

public class ElectricCar : Car
{
    // Override again - multilevel
    public override void Start()
    {
        Console.WriteLine("Electric car starting silently...");
    }
}
```

### 3.3 Rules for Override

```csharp
public class Base
{
    public virtual void Method1() { }
    public void Method2() { }  // Not virtual
    private virtual void Method3() { }  // ❌ Error: private can't be virtual
}

public class Derived : Base
{
    // ✅ Correct override
    public override void Method1() { }

    // ❌ Cannot override non-virtual
    // public override void Method2() { }  // Error!

    // Override must have:
    // - Same name
    // - Same return type (or covariant return in C# 9+)
    // - Same parameters
    // - Same or less restrictive access modifier
}
```

### 3.4 Calling Base Implementation

```csharp
public class Animal
{
    public virtual void Speak()
    {
        Console.WriteLine("Animal sound");
    }
}

public class Dog : Animal
{
    public override void Speak()
    {
        base.Speak();  // Call base class implementation first
        Console.WriteLine("Woof!");
    }
}

var dog = new Dog();
dog.Speak();
// Output:
// Animal sound
// Woof!
```

### 3.5 Preventing Further Override with `sealed`

```csharp
public class Animal
{
    public virtual void Speak() { }
}

public class Dog : Animal
{
    // Seal this override - no further overriding allowed
    public sealed override void Speak()
    {
        Console.WriteLine("Woof!");
    }
}

public class Bulldog : Dog
{
    // ❌ Error: cannot override sealed method
    // public override void Speak() { }
}
```

### 3.6 Virtual vs Override - Memory Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    VIRTUAL DISPATCH IN ACTION                           │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Animal a = new Dog();                                                 │
│   a.Speak();                                                            │
│                                                                         │
│   Step 1: 'a' is of type Animal (compile-time type)                     │
│   Step 2: 'a' points to Dog object (runtime type)                       │
│   Step 3: CLR checks if Speak() is virtual - YES                        │
│   Step 4: CLR looks up Dog's method table                               │
│   Step 5: Finds Dog.Speak() and calls it                                │
│                                                                         │
│   ┌──────────────────┐        ┌──────────────────┐                     │
│   │ Reference 'a'    │        │ Dog Object       │                     │
│   │ Type: Animal     │───────>│ Runtime Type: Dog│                     │
│   │                  │        │                  │                     │
│   └──────────────────┘        └────────┬─────────┘                     │
│                                        │                               │
│                                        ▼                               │
│                               ┌──────────────────┐                     │
│                               │ Dog Method Table │                     │
│                               │ Speak() -> Dog   │                     │
│                               └──────────────────┘                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 4. Abstract Classes

### 4.1 What is an Abstract Class?

**Definition:** A class that cannot be instantiated and may contain abstract (unimplemented) members that derived classes MUST implement.

```csharp
public abstract class Shape
{
    // Abstract method - NO implementation, derived class MUST implement
    public abstract double GetArea();

    // Abstract property
    public abstract string Name { get; }

    // Regular method - HAS implementation, CAN be overridden
    public virtual void Display()
    {
        Console.WriteLine($"Shape: {Name}, Area: {GetArea()}");
    }

    // Regular method - HAS implementation, CANNOT be overridden
    public void PrintType()
    {
        Console.WriteLine($"Type: {GetType().Name}");
    }
}

// ❌ Cannot instantiate abstract class
// var shape = new Shape();  // COMPILE ERROR!

// ✅ Must create derived class
public class Circle : Shape
{
    public double Radius { get; set; }

    // MUST implement abstract members
    public override double GetArea() => Math.PI * Radius * Radius;
    public override string Name => "Circle";
}
```

### 4.2 Abstract Class Components

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    ABSTRACT CLASS ANATOMY                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   public abstract class MyAbstractClass                                 │
│   {                                                                     │
│       // ═══════════════════════════════════════════════════════════   │
│       // CAN HAVE:                                                      │
│       // ═══════════════════════════════════════════════════════════   │
│                                                                         │
│       // 1. Fields (instance and static)                                │
│       private int _field;                                               │
│       protected static string StaticField;                              │
│                                                                         │
│       // 2. Constructors (called by derived class)                      │
│       protected MyAbstractClass(int value) { _field = value; }          │
│                                                                         │
│       // 3. Abstract methods (NO body, derived MUST implement)          │
│       public abstract void AbstractMethod();                            │
│                                                                         │
│       // 4. Virtual methods (HAS body, derived CAN override)            │
│       public virtual void VirtualMethod() { }                           │
│                                                                         │
│       // 5. Regular methods (HAS body, derived CANNOT override)         │
│       public void RegularMethod() { }                                   │
│                                                                         │
│       // 6. Properties (abstract, virtual, or regular)                  │
│       public abstract int AbstractProperty { get; }                     │
│       public virtual int VirtualProperty => 10;                         │
│       public int RegularProperty { get; set; }                          │
│                                                                         │
│       // 7. Events                                                      │
│       public event EventHandler MyEvent;                                │
│                                                                         │
│       // ═══════════════════════════════════════════════════════════   │
│       // CANNOT:                                                        │
│       // ═══════════════════════════════════════════════════════════   │
│       // • Be instantiated directly (new MyAbstractClass() - ERROR)     │
│       // • Be sealed (sealed abstract - contradiction)                  │
│       // • Have abstract static members                                 │
│   }                                                                     │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 4.3 When to Use Abstract Class

```csharp
// Use abstract class when:
// 1. You have common code to share
// 2. You need fields/state
// 3. You want to define a template (Template Method Pattern)

public abstract class DataProcessor
{
    // Shared field
    protected readonly ILogger _logger;

    // Constructor
    protected DataProcessor(ILogger logger)
    {
        _logger = logger;
    }

    // Template Method Pattern
    public void Process()
    {
        _logger.Log("Starting processing...");

        var data = FetchData();        // Abstract - derived implements
        var processed = Transform(data); // Abstract - derived implements
        Save(processed);                // Concrete - shared implementation

        _logger.Log("Processing complete.");
    }

    protected abstract string FetchData();
    protected abstract string Transform(string data);

    protected void Save(string data)
    {
        // Common save logic
        File.WriteAllText("output.txt", data);
    }
}

public class CsvProcessor : DataProcessor
{
    public CsvProcessor(ILogger logger) : base(logger) { }

    protected override string FetchData() => File.ReadAllText("data.csv");
    protected override string Transform(string data) => data.ToUpper();
}
```

### 4.4 Abstract Class Inheritance Rules

```csharp
// Rule 1: Derived class MUST implement ALL abstract members (or be abstract itself)
public abstract class A
{
    public abstract void Method1();
    public abstract void Method2();
}

// Option 1: Implement all
public class B : A
{
    public override void Method1() { }
    public override void Method2() { }
}

// Option 2: Be abstract and implement some
public abstract class C : A
{
    public override void Method1() { }
    // Method2 still abstract
}

public class D : C
{
    public override void Method2() { }  // Must implement remaining
}
```

---

## 5. Interfaces

### 5.1 What is an Interface?

**Definition:** A contract that defines a set of members that implementing classes MUST provide. Contains NO implementation (before C# 8).

```csharp
// Interface definition
public interface IVehicle
{
    // Properties
    string Brand { get; set; }
    int MaxSpeed { get; }

    // Methods
    void Start();
    void Stop();

    // Events
    event EventHandler OnStarted;
}

// Implementation
public class Car : IVehicle
{
    public string Brand { get; set; }
    public int MaxSpeed => 200;

    public event EventHandler OnStarted;

    public void Start()
    {
        Console.WriteLine($"{Brand} starting...");
        OnStarted?.Invoke(this, EventArgs.Empty);
    }

    public void Stop()
    {
        Console.WriteLine($"{Brand} stopping...");
    }
}
```

### 5.2 Interface Evolution (C# 8+)

```csharp
// C# 8+ features
public interface IModernInterface
{
    // Regular abstract member (must be implemented)
    void RequiredMethod();

    // Default implementation (C# 8+)
    // Implementing class CAN override, but doesn't have to
    void OptionalMethod()
    {
        Console.WriteLine("Default implementation");
    }

    // Static members (C# 8+)
    static string Version => "1.0";

    // Private methods for internal use (C# 8+)
    private void HelperMethod() { }

    // Static abstract members (C# 11+)
    // static abstract int StaticAbstractMethod();
}
```

### 5.3 Multiple Interface Implementation

```csharp
public interface IFlyable
{
    void Fly();
}

public interface ISwimmable
{
    void Swim();
}

public interface IWalkable
{
    void Walk();
}

// A class can implement multiple interfaces
public class Duck : IFlyable, ISwimmable, IWalkable
{
    public void Fly() => Console.WriteLine("Duck flying");
    public void Swim() => Console.WriteLine("Duck swimming");
    public void Walk() => Console.WriteLine("Duck walking");
}

// Polymorphism with interfaces
IFlyable flyingThing = new Duck();
ISwimmable swimmingThing = new Duck();

flyingThing.Fly();      // "Duck flying"
swimmingThing.Swim();   // "Duck swimming"
```

### 5.4 Explicit Interface Implementation

When two interfaces have same method signature:

```csharp
public interface IPrinter
{
    void Print();
}

public interface IScanner
{
    void Print();  // Same signature as IPrinter!
}

public class MultiFunctionDevice : IPrinter, IScanner
{
    // Implicit implementation - used for both
    public void Print()
    {
        Console.WriteLine("Default print");
    }

    // Explicit implementation for IPrinter
    void IPrinter.Print()
    {
        Console.WriteLine("Printing document...");
    }

    // Explicit implementation for IScanner
    void IScanner.Print()
    {
        Console.WriteLine("Printing scan results...");
    }
}

// Usage
var device = new MultiFunctionDevice();
device.Print();  // "Default print"

IPrinter printer = device;
printer.Print();  // "Printing document..."

IScanner scanner = device;
scanner.Print();  // "Printing scan results..."
```

### 5.5 Interface Segregation Principle

```csharp
// ❌ BAD: Fat interface
public interface IWorker
{
    void Work();
    void Eat();
    void Sleep();
}

// Robot can't eat or sleep!
public class Robot : IWorker
{
    public void Work() => Console.WriteLine("Working");
    public void Eat() => throw new NotImplementedException();  // ❌
    public void Sleep() => throw new NotImplementedException();  // ❌
}

// ✅ GOOD: Segregated interfaces
public interface IWorkable
{
    void Work();
}

public interface IFeedable
{
    void Eat();
}

public interface ISleepable
{
    void Sleep();
}

public class Human : IWorkable, IFeedable, ISleepable
{
    public void Work() => Console.WriteLine("Working");
    public void Eat() => Console.WriteLine("Eating");
    public void Sleep() => Console.WriteLine("Sleeping");
}

public class Robot : IWorkable
{
    public void Work() => Console.WriteLine("Working 24/7");
    // No need to implement Eat or Sleep!
}
```

---

## 6. Abstract Class vs Interface

### 6.1 Comparison Table

| Feature                   | Abstract Class        | Interface                     |
| ------------------------- | --------------------- | ----------------------------- |
| **Instantiation**         | ❌ Cannot instantiate | ❌ Cannot instantiate         |
| **Multiple inheritance**  | ❌ Single only        | ✅ Multiple allowed           |
| **Fields**                | ✅ Can have           | ❌ Cannot have                |
| **Constructors**          | ✅ Can have           | ❌ Cannot have                |
| **Access modifiers**      | ✅ All allowed        | ✅ All allowed (C# 8+)        |
| **Method implementation** | ✅ Can have           | ✅ Default impl (C# 8+)       |
| **Static members**        | ✅ Can have           | ✅ Can have (C# 8+)           |
| **Abstract members**      | ✅ Can have           | ✅ All members are abstract\* |
| **Purpose**               | IS-A relationship     | CAN-DO capability             |
| **Versioning**            | Easier to add members | Breaking change to add        |

### 6.2 When to Use What?

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    DECISION FLOWCHART                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   Need to share code/fields among related classes?                      │
│   │                                                                     │
│   ├─ YES ──> Use ABSTRACT CLASS                                         │
│   │                                                                     │
│   └─ NO ──> Need multiple "inheritance"?                                │
│             │                                                           │
│             ├─ YES ──> Use INTERFACE                                    │
│             │                                                           │
│             └─ NO ──> Is it describing a capability?                    │
│                       │                                                 │
│                       ├─ YES ──> Use INTERFACE (IDisposable, IComparable)│
│                       │                                                 │
│                       └─ NO ──> Is it describing what something IS?     │
│                                 │                                       │
│                                 ├─ YES ──> Use ABSTRACT CLASS           │
│                                 │                                       │
│                                 └─ Either works, prefer INTERFACE       │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 6.3 Practical Example

```csharp
// INTERFACE: Defines capability (what it CAN DO)
public interface ISerializable
{
    byte[] Serialize();
    void Deserialize(byte[] data);
}

public interface IValidatable
{
    bool IsValid();
    IEnumerable<string> GetErrors();
}

// ABSTRACT CLASS: Defines what it IS with shared code
public abstract class Entity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? ModifiedAt { get; protected set; }

    protected Entity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public abstract bool Validate();
}

// Concrete class: IS an Entity, CAN BE serialized and validated
public class User : Entity, ISerializable, IValidatable
{
    public string Name { get; set; }
    public string Email { get; set; }

    public override bool Validate()
    {
        return !string.IsNullOrEmpty(Name) && Email.Contains("@");
    }

    public bool IsValid() => Validate();

    public IEnumerable<string> GetErrors()
    {
        if (string.IsNullOrEmpty(Name)) yield return "Name is required";
        if (!Email.Contains("@")) yield return "Invalid email";
    }

    public byte[] Serialize() => JsonSerializer.SerializeToUtf8Bytes(this);
    public void Deserialize(byte[] data) { /* ... */ }
}
```

---

## 7. Method Hiding (new keyword)

### 7.1 What is Method Hiding?

**Definition:** Using the `new` keyword to hide a base class member with a new implementation. Unlike override, this breaks polymorphism.

```csharp
public class Animal
{
    public void Speak()  // Not virtual!
    {
        Console.WriteLine("Animal speaks");
    }
}

public class Dog : Animal
{
    // 'new' hides the base method
    public new void Speak()
    {
        Console.WriteLine("Dog barks");
    }
}

// The difference matters with polymorphism:
Dog dog = new Dog();
Animal animal = dog;  // Same object, different reference type

dog.Speak();     // "Dog barks" - calls Dog.Speak
animal.Speak();  // "Animal speaks" - calls Animal.Speak (NOT Dog!)
```

### 7.2 Override vs New - Critical Difference

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    OVERRIDE vs NEW                                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   WITH OVERRIDE (virtual/override):                                     │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │ Animal animal = new Dog();                                      │  │
│   │ animal.Speak();  // "Dog barks" ✓                               │  │
│   │                                                                 │  │
│   │ Runtime looks at ACTUAL object type (Dog)                       │  │
│   │ Virtual dispatch ensures correct method is called               │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                                                                         │
│   WITH NEW (method hiding):                                             │
│   ┌─────────────────────────────────────────────────────────────────┐  │
│   │ Animal animal = new Dog();                                      │  │
│   │ animal.Speak();  // "Animal speaks" ✗                           │  │
│   │                                                                 │  │
│   │ Compiler looks at REFERENCE type (Animal)                       │  │
│   │ No virtual dispatch - base method called                        │  │
│   └─────────────────────────────────────────────────────────────────┘  │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### 7.3 When to Use `new` (Rarely!)

```csharp
// Scenario: You don't control the base class and need different behavior
// This is usually a CODE SMELL - indicates design problem

// Legitimate use: Breaking change in library
// Library v1
public class ThirdPartyBase
{
    // No virtual - they didn't anticipate extension
    public void Process() { }
}

// Your code that extends it
public class MyClass : ThirdPartyBase
{
    // You NEED different behavior
    public new void Process() { }
}

// BETTER SOLUTION: Composition over inheritance
public class MyBetterClass
{
    private readonly ThirdPartyBase _base = new();

    public void Process()
    {
        // Your logic
        _base.Process();  // Delegate if needed
    }
}
```

---

## 8. Sealed Keyword

### 8.1 Sealed Class

**Definition:** A class that cannot be inherited.

```csharp
public sealed class FinalClass
{
    public void Method() { }
}

// ❌ Cannot inherit from sealed class
// public class DerivedClass : FinalClass { }  // COMPILE ERROR!
```

**Why seal a class?**

1. **Security**: Prevent tampering with critical classes
2. **Performance**: JIT can optimize sealed methods
3. **Design**: Class not designed for inheritance

```csharp
// .NET examples of sealed classes:
// - String
// - StringBuilder (actually not sealed, but similar concept)
// - DateTime
// - Many exception classes
```

### 8.2 Sealed Method

**Definition:** Prevents further overriding of a virtual method.

```csharp
public class A
{
    public virtual void Method() { }
}

public class B : A
{
    public sealed override void Method() { }  // Sealed here
}

public class C : B
{
    // ❌ Cannot override sealed method
    // public override void Method() { }  // COMPILE ERROR!
}
```

### 8.3 Performance Benefit

```csharp
// JIT optimization with sealed
public sealed class SealedClass
{
    public void FastMethod() { }  // JIT can inline this
}

public class RegularClass
{
    public virtual void SlowMethod() { }  // JIT cannot inline (might be overridden)
}

// The performance difference is usually negligible in modern .NET
// Seal for design reasons, not performance
```

---

## 9. Static Keyword

### 9.1 Static Members

```csharp
public class Counter
{
    // Static field - shared across ALL instances
    private static int _count = 0;

    // Instance field - unique to each instance
    private int _instanceId;

    // Static property
    public static int TotalCount => _count;

    // Static method
    public static void ResetCount() => _count = 0;

    // Constructor increments static count
    public Counter()
    {
        _count++;
        _instanceId = _count;
    }
}

var c1 = new Counter();  // _count = 1
var c2 = new Counter();  // _count = 2
var c3 = new Counter();  // _count = 3

Console.WriteLine(Counter.TotalCount);  // 3
```

### 9.2 Static Class

```csharp
// Static class - cannot be instantiated, all members must be static
public static class MathHelper
{
    public static double Pi => 3.14159;

    public static int Square(int x) => x * x;

    public static double CircleArea(double radius) => Pi * radius * radius;

    // ❌ Cannot have instance members
    // public int InstanceField;  // ERROR

    // ❌ Cannot have constructors (except static constructor)
    // public MathHelper() { }  // ERROR

    // ✅ Can have static constructor
    static MathHelper()
    {
        Console.WriteLine("Static class initialized");
    }
}

// Usage
double area = MathHelper.CircleArea(5);

// ❌ Cannot instantiate
// var helper = new MathHelper();  // ERROR
```

### 9.3 Static Constructor

```csharp
public class Configuration
{
    public static string ConnectionString { get; private set; }

    // Static constructor - called ONCE before first access
    static Configuration()
    {
        Console.WriteLine("Loading configuration...");
        ConnectionString = File.ReadAllText("config.txt");
    }

    // Instance constructor
    public Configuration()
    {
        Console.WriteLine("Creating instance...");
    }
}

// First access triggers static constructor
Console.WriteLine(Configuration.ConnectionString);
// Output:
// Loading configuration...
// (connection string)

var config = new Configuration();
// Output:
// Creating instance...
// (static constructor NOT called again)
```

### 9.4 Static vs Instance

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    STATIC vs INSTANCE                                   │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│   MEMORY LAYOUT:                                                        │
│                                                                         │
│   Static Members                    Instance Members                    │
│   ┌─────────────────────────┐      ┌─────────────────────────┐         │
│   │ HIGH FREQUENCY HEAP     │      │ HEAP (per object)       │         │
│   │ (or special area)       │      │                         │         │
│   │                         │      │ ┌─────────┐ ┌─────────┐ │         │
│   │ ┌───────────────────┐   │      │ │Object 1 │ │Object 2 │ │         │
│   │ │ static _count = 3 │   │      │ │ _id = 1 │ │ _id = 2 │ │         │
│   │ └───────────────────┘   │      │ └─────────┘ └─────────┘ │         │
│   │                         │      │                         │         │
│   │ ONE copy for all        │      │ SEPARATE copy per       │         │
│   │                         │      │ instance                │         │
│   └─────────────────────────┘      └─────────────────────────┘         │
│                                                                         │
│   LIFETIME:                                                             │
│   Static: Application lifetime (until AppDomain unloaded)               │
│   Instance: Until garbage collected (no more references)                │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 10. Partial Keyword

### 10.1 Partial Classes

**Definition:** Split a class definition across multiple files.

```csharp
// File: Person.cs
public partial class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// File: Person.Methods.cs
public partial class Person
{
    public string GetFullName() => $"{FirstName} {LastName}";
}

// File: Person.Validation.cs
public partial class Person
{
    public bool IsValid() => !string.IsNullOrEmpty(FirstName);
}

// At compile time, merged into single class
```

**Use cases:**

1. **Code generation**: Generated code in one file, custom code in another
2. **Large classes**: Split for readability
3. **Multiple developers**: Work on different parts simultaneously

### 10.2 Partial Methods

```csharp
// File: Generated.cs (by tool)
public partial class DataEntity
{
    partial void OnCreated();  // Declaration only
    partial void OnModified();

    public void Create()
    {
        // ... creation logic
        OnCreated();  // May or may not have implementation
    }
}

// File: Custom.cs (your code)
public partial class DataEntity
{
    partial void OnCreated()  // Optional implementation
    {
        Console.WriteLine("Entity created!");
    }

    // OnModified not implemented - call is removed by compiler
}
```

---

## 11. readonly vs const

### 11.1 const

```csharp
public class Constants
{
    // const - compile-time constant
    public const double Pi = 3.14159;
    public const string AppName = "MyApp";

    // ❌ Cannot assign non-compile-time value
    // public const DateTime Now = DateTime.Now;  // ERROR

    // const is implicitly static
    // Constants.Pi  (no instance needed)
}

// IMPORTANT: const is "baked" into calling code at compile time
// If you change const value, ALL dependent assemblies must be recompiled!
```

### 11.2 readonly

```csharp
public class Configuration
{
    // readonly - can be set in constructor or declaration
    private readonly string _connectionString;
    private readonly DateTime _createdAt = DateTime.Now;  // ✅ Runtime value OK

    // readonly static - like const but for runtime values
    public static readonly DateTime StartTime = DateTime.Now;

    public Configuration(string connectionString)
    {
        _connectionString = connectionString;  // ✅ Can set in constructor
    }

    public void SomeMethod()
    {
        // ❌ Cannot modify after construction
        // _connectionString = "new value";  // ERROR
    }
}
```

### 11.3 Comparison

| Feature             | const                                | readonly                      |
| ------------------- | ------------------------------------ | ----------------------------- |
| **Initialization**  | Declaration only                     | Declaration or constructor    |
| **Value type**      | Compile-time constant                | Runtime value OK              |
| **Implicit static** | Yes                                  | No (unless explicitly static) |
| **Memory**          | Inlined at call site                 | Stored in memory              |
| **Change impact**   | Requires recompilation of dependents | No recompilation needed       |
| **Performance**     | Slightly faster (inlined)            | Normal field access           |

---

## 12. Important Keywords Summary

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    C# KEYWORDS QUICK REFERENCE                          │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  INHERITANCE & POLYMORPHISM:                                            │
│  ────────────────────────────                                           │
│  virtual    → Allows method to be overridden in derived class           │
│  override   → Provides new implementation of virtual/abstract member    │
│  abstract   → Must be implemented by derived class (no body)            │
│  sealed     → Prevents inheritance (class) or further override (method) │
│  new        → Hides base class member (breaks polymorphism)             │
│  base       → Refers to base class instance                             │
│                                                                         │
│  ACCESS & VISIBILITY:                                                   │
│  ───────────────────────                                                │
│  public     → Accessible from anywhere                                  │
│  private    → Accessible only within containing type                    │
│  protected  → Accessible within containing type and derived types       │
│  internal   → Accessible within same assembly                           │
│  protected internal → protected OR internal                             │
│  private protected  → protected AND internal                            │
│                                                                         │
│  TYPE MODIFIERS:                                                        │
│  ───────────────                                                        │
│  static     → Belongs to type, not instance                             │
│  const      → Compile-time constant                                     │
│  readonly   → Can only be set at declaration or in constructor          │
│  partial    → Type/method definition split across files                 │
│  volatile   → Field may be modified by multiple threads                 │
│                                                                         │
│  TYPE DECLARATIONS:                                                     │
│  ───────────────────                                                    │
│  class      → Reference type with implementation                        │
│  interface  → Contract (abstract by default)                            │
│  struct     → Value type                                                │
│  record     → Immutable reference type with value semantics (C# 9+)     │
│  enum       → Set of named constants                                    │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 13. Interview Questions

### Q1: What is the difference between virtual and abstract?

**Answer:**

| Aspect                 | virtual                   | abstract                     |
| ---------------------- | ------------------------- | ---------------------------- |
| **Has implementation** | Yes (default)             | No                           |
| **Override required**  | No (optional)             | Yes (must override)          |
| **Class type**         | Can be in regular class   | Only in abstract class       |
| **Instantiation**      | Class can be instantiated | Class cannot be instantiated |

```csharp
public abstract class Shape
{
    public abstract double GetArea();      // No body, MUST override
    public virtual void Draw() { }         // Has body, CAN override
}
```

---

### Q2: What happens if you don't use `virtual` but try to override?

**Answer:** You cannot override without `virtual`. If you use the same method signature, you're **hiding** the method (which requires `new` keyword, or compiler will warn).

```csharp
public class Base
{
    public void Method() { }  // Not virtual
}

public class Derived : Base
{
    public new void Method() { }  // Hiding, NOT overriding
}

Base b = new Derived();
b.Method();  // Calls Base.Method, NOT Derived!
```

---

### Q3: Can an abstract class have a constructor?

**Answer:** Yes! Abstract classes can have constructors. They're called when derived class is instantiated.

```csharp
public abstract class Entity
{
    public int Id { get; }

    protected Entity(int id)  // Constructor
    {
        Id = id;
    }
}

public class User : Entity
{
    public User(int id) : base(id) { }  // Must call base constructor
}
```

---

### Q4: Why can't we instantiate an abstract class?

**Answer:** Abstract classes may have abstract members (methods without implementation). If instantiation were allowed, calling those methods would have no code to execute. Abstract classes are designed to be base classes - they define a template that derived classes complete.

---

### Q5: Difference between `override` and `new`?

**Answer:**

- **override**: Provides polymorphic behavior. Runtime type determines which method runs.
- **new**: Hides base method. Reference type determines which method runs.

```csharp
Base b = new Derived();
b.Method();
// With override: Derived.Method() runs
// With new: Base.Method() runs
```

---

### Q6: Can we have static methods in interface?

**Answer:**

- Before C# 8: No
- C# 8+: Yes, interfaces can have static members
- C# 11+: Interfaces can have static abstract members

```csharp
public interface IFactory<T>
{
    static abstract T Create();  // C# 11+
}
```

---

### Q7: When would you use interface default implementation?

**Answer:**

1. Adding new methods to existing interface without breaking implementers
2. Providing optional functionality
3. Trait-like composition

```csharp
public interface ILogger
{
    void Log(string message);

    // Added later without breaking existing implementations
    void LogError(string message) => Log($"ERROR: {message}");
}
```

---

### Q8: What is the diamond problem and how does C# solve it?

**Answer:** Diamond problem occurs when a class inherits from two classes that have a common base, causing ambiguity.

C# solves this by:

1. Not allowing multiple class inheritance
2. For interfaces with default implementations: compiler error if ambiguous, must explicitly implement

```csharp
interface IA { void M() => Console.WriteLine("A"); }
interface IB { void M() => Console.WriteLine("B"); }

class C : IA, IB
{
    // Must explicitly implement to resolve ambiguity
    public void M() => ((IA)this).M();  // Choose which one
}
```

---

_Document created for interview preparation. Last updated: February 2026_
