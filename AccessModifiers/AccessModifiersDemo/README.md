# C# Access Modifiers Demo

This demo explains and demonstrates all C# access modifiers with code examples.

## Access Modifiers Overview

| Modifier             | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| `public`             | Accessible from anywhere                                     |
| `private`            | Accessible only within the containing class                  |
| `protected`          | Accessible within the containing class and derived classes   |
| `internal`           | Accessible within the same assembly/project                  |
| `protected internal` | Accessible within the same assembly OR from derived classes  |
| `private protected`  | Accessible within the same assembly AND from derived classes |

## Example Classes

- `PublicClass`: Public class, accessible from anywhere.
- `InternalClass`: Internal class, accessible only within the same assembly.
- `ModifierExamples`: Demonstrates all field-level access modifiers.
- `Derived`: Shows which fields are accessible in a derived class.
- `Program`: Runs the demo.

## How to Run

1. Open this folder in your terminal.
2. Compile and run the demo:

```sh
dotnet run --project AccessModifiersDemo.csproj
```

Or, if using a single file:

```sh
dotnet run AccessModifiersDemo.cs
```

## Key Points

- Use access modifiers to control visibility and encapsulation.
- Choose the most restrictive modifier that still allows your code to work.
