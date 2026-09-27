# C# Keywords Quick Reference

> **Quick lookup for interviews**

---

## Inheritance & Polymorphism Keywords

### `virtual`

Allows a method/property to be overridden in derived classes.

```csharp
public virtual void Speak() { }  // CAN be overridden
```

### `override`

Provides new implementation of a virtual/abstract member.

```csharp
public override void Speak() { }  // Overrides base
```

### `abstract`

- **On class**: Cannot be instantiated, may have abstract members
- **On member**: No implementation, MUST be overridden

```csharp
public abstract class Shape { }
public abstract double GetArea();
```

### `sealed`

- **On class**: Cannot be inherited
- **On method**: Cannot be overridden further

```csharp
public sealed class Final { }
public sealed override void Method() { }
```

### `new` (hiding)

Hides base class member (breaks polymorphism).

```csharp
public new void Method() { }  // Hides, doesn't override
```

### `base`

References the base class.

```csharp
base.Method();  // Call base implementation
public Derived() : base() { }  // Call base constructor
```

---

## Access Modifiers

| Keyword              | Accessibility             |
| -------------------- | ------------------------- |
| `public`             | Everywhere                |
| `private`            | Same class only           |
| `protected`          | Same class + derived      |
| `internal`           | Same assembly             |
| `protected internal` | Same assembly OR derived  |
| `private protected`  | Same assembly AND derived |

---

## Type & Member Modifiers

### `static`

Belongs to type, not instance.

```csharp
public static int Count;
public static void Method() { }
public static class Helper { }
```

### `const`

Compile-time constant. Implicitly static.

```csharp
public const double Pi = 3.14159;  // Must be known at compile-time
```

### `readonly`

Can only be set at declaration or in constructor.

```csharp
public readonly DateTime Created = DateTime.Now;  // Runtime OK
```

### `partial`

Split definition across files.

```csharp
public partial class Person { }  // In File1.cs
public partial class Person { }  // In File2.cs
```

### `volatile`

Field may be modified by multiple threads.

```csharp
private volatile bool _running;
```

---

## Type Declarations

### `class`

Reference type with implementation.

```csharp
public class Person { }
```

### `interface`

Contract with no implementation (before C# 8).

```csharp
public interface IDisposable { void Dispose(); }
```

### `struct`

Value type (stack allocated for local variables).

```csharp
public struct Point { public int X, Y; }
```

### `record`

Immutable reference type with value semantics (C# 9+).

```csharp
public record Person(string Name, int Age);
```

### `enum`

Set of named constants.

```csharp
public enum Status { Pending, Active, Completed }
```

---

## Parameter Modifiers

### `ref`

Pass by reference. Must be initialized before call.

```csharp
void Modify(ref int x) { x = 10; }
int n = 5;
Modify(ref n);  // n is now 10
```

### `out`

Pass by reference. Must be assigned in method.

```csharp
void Init(out int x) { x = 10; }
int n;
Init(out n);  // n is now 10
```

### `in`

Pass by reference. Cannot be modified (readonly).

```csharp
void Read(in int x) { /* x is readonly */ }
```

### `params`

Variable number of arguments.

```csharp
void Print(params string[] items) { }
Print("a", "b", "c");  // Array created automatically
```

---

## Common Interview Comparison

### virtual vs abstract

| virtual              | abstract             |
| -------------------- | -------------------- |
| Has implementation   | No implementation    |
| Override is optional | Override is required |
| Any class            | Abstract class only  |

### override vs new

| override                  | new                   |
| ------------------------- | --------------------- |
| Polymorphic               | Not polymorphic       |
| Runtime dispatch          | Compile-time dispatch |
| Requires virtual/abstract | Hides any member      |

### const vs readonly

| const             | readonly          |
| ----------------- | ----------------- |
| Compile-time only | Runtime values OK |
| Implicitly static | Can be instance   |
| Inlined at use    | Stored in memory  |

### class vs struct

| class                | struct             |
| -------------------- | ------------------ |
| Reference type       | Value type         |
| Heap allocated       | Stack (for locals) |
| Supports inheritance | No inheritance     |
| Default null         | Default zero       |

---

_Quick reference for interviews. February 2026_
