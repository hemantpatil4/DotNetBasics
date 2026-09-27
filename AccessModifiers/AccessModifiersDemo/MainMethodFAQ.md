# C# Main Method FAQ

## Can you have multiple `Main` methods in a C# project?

- **You can have multiple `Main` methods** in different classes in the same project.
- However, **only one `Main` method can be the entry point** when you build and run the application.
- If there is more than one `Main` method, the compiler will ask you to specify which one to use as the entry point (using project settings or build options).

## Why not have many `Main` methods?

- The `Main` method is the starting point of a C# application. The runtime needs a single, unambiguous entry point.
- Multiple `Main` methods can exist for testing or as examples, but only one can be used to launch the program.
- If you try to build with more than one `Main` and don't specify which to use, you'll get a compiler error.

## Example

```csharp
public class ProgramA
{
    public static void Main() { /* ... */ }
}

public class ProgramB
{
    public static void Main() { /* ... */ }
}
```

## How to specify the entry point (if needed)

- In Visual Studio: Right-click the project > Properties > Application > Startup object.
- With the CLI: Use the `<StartupObject>` property in the `.csproj` file.

## Summary

- You can define multiple `Main` methods, but only one is used as the entry point.
- The compiler/runtime needs to know which one to start with.
