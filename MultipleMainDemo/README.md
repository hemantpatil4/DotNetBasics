# Multiple Main Methods Demo

This demo shows how to define multiple `Main` methods in a single C# project.

## Files

- `FirstMain.cs`: Contains `FirstMain.Main()`
- `SecondMain.cs`: Contains `SecondMain.Main()`

## How it works

- You can define multiple `Main` methods, but only one can be the entry point when running the project.
- If you try to build/run without specifying, the compiler will throw an error about multiple entry points.

## How to specify which Main to use

- Edit the `.csproj` file and add:
  ```xml
  <PropertyGroup>
    <StartupObject>MultipleMainDemo.FirstMain</StartupObject>
  </PropertyGroup>
  ```
  or
  ```xml
  <PropertyGroup>
    <StartupObject>MultipleMainDemo.SecondMain</StartupObject>
  </PropertyGroup>
  ```
- Or, in Visual Studio: Project Properties > Application > Startup object.

## Example

To run `FirstMain` as the entry point, set `StartupObject` to `MultipleMainDemo.FirstMain`.

---

Only one `Main` method can be used as the entry point at runtime.
