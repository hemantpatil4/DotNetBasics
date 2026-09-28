# Project, Assembly, Build, and Publish

Let's start before the program is running.

This folder answers one question:

> After I write a Web API, what files does .NET create, and what does each file do?

The next folder answers the other question: [who starts those files and how a request reaches your code](../HostRuntime/HowTheHostRunsYourApi.md).

------------------------------------------------------------------------

## 1. Your project is just folders and files

A small Web API looks like this:

``` text
MyApi/
 ├── MyApi.csproj          ← project file
 ├── Program.cs            ← where the app starts
 ├── appsettings.json      ← settings
 ├── Controllers/          ← your HTTP endpoints
 └── Services/             ← your business code
```

This is **source code**.

It is not running yet. The computer cannot execute `Program.cs` directly.

------------------------------------------------------------------------

## 2. What an assembly is

When you compile the project, your C# becomes an **assembly**.

For a Web API, the main assembly is usually:

``` text
MyApi.dll
```

Think:

> **Assembly = your compiled program, packed into a DLL.**

Inside `MyApi.dll` you have:

- Your compiled code
- The names of the types you wrote (`FXRateController`, `FXRateService`, and so on)
- Information about which other libraries your app needs

The DLL holds **IL** (Intermediate Language). That is not the final CPU instructions yet. The runtime turns IL into machine code later, when the app runs.

So:

``` text
Program.cs  +  Controllers  +  Services
                │
                │  compile
                ▼
            MyApi.dll
```

`MyApi.dll` is your application. It does not start by itself.

------------------------------------------------------------------------

## 3. What the project file is doing

`MyApi.csproj` tells .NET what kind of app this is.

A Web API project file is roughly:

``` xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

Very simple meaning:

| Piece | What it means |
| --- | --- |
| `Microsoft.NET.Sdk.Web` | This is a web app. Bring in ASP.NET Core. |
| `net8.0` | Build for .NET 8. |

If you add a NuGet package, that reference is also stored in the `.csproj`. The compiler uses it to know which extra DLLs your app needs.

------------------------------------------------------------------------

## 4. Build and publish are not the same

### `dotnet build`

Build checks the code and compiles it so you can run it on your machine.

``` text
bin/Debug/net8.0/
 ├── MyApi.dll
 ├── MyApi.deps.json
 ├── MyApi.runtimeconfig.json
 ├── MyApi.pdb
 └── appsettings.json
```

This folder is for **local development**.

### `dotnet publish`

Publish prepares the files you actually **deploy**.

``` bash
dotnet publish -c Release -o publish
```

``` text
publish/
 ├── MyApi.dll
 ├── MyApi.deps.json
 ├── MyApi.runtimeconfig.json
 ├── MyApi.pdb
 ├── appsettings.json
 └── other DLLs your app needs
```

Think:

> **Build = compile for me. Publish = pack the files that will run on the server.**

------------------------------------------------------------------------

## 5. What each publish file does

These are the important ones.

### `MyApi.dll`

Your application assembly.

This is the compiled Web API: controllers, services, `Program.cs`.

### `MyApi.deps.json`

The dependency list.

It tells the host:

- Which assemblies to load
- Which NuGet packages you used
- Which version of each library is required

Think:

> **deps.json = the shopping list of DLLs.**

### `MyApi.runtimeconfig.json`

The runtime settings for this app.

It says things like:

- This app targets `net8.0`
- Use the shared framework `Microsoft.NETCore.App`
- For a Web API, also use `Microsoft.AspNetCore.App`

A simplified look:

``` json
{
  "runtimeOptions": {
    "tfm": "net8.0",
    "frameworks": [
      { "name": "Microsoft.NETCore.App", "version": "8.0.0" },
      { "name": "Microsoft.AspNetCore.App", "version": "8.0.0" }
    ]
  }
}
```

Think:

> **runtimeconfig.json = which .NET runtime this app expects.**

### `MyApi.pdb`

Debug symbols.

They map the compiled code back to your `.cs` files, so a stack trace can show a line number. The app can run without this file.

### `appsettings.json`

Your configuration. It is copied next to the DLL. It is not compiled into the assembly.

### Other DLLs

Libraries that are **not** already inside the shared .NET runtime.

Example: a NuGet package you added yourself. Those DLLs are copied into `publish/` so the app can find them.

### `MyApi` or `MyApi.exe`

Sometimes publish also creates a small native program with the project name.

- On Windows it is often `MyApi.exe`
- On Linux it is often a file named `MyApi`

This file is the **app host**. It is a tiny starter. It is not your C# code. Your C# code is still `MyApi.dll`.

You can start the app in two ways:

``` bash
dotnet MyApi.dll
```

or, if the app host was created:

``` bash
./MyApi
```

Both end up loading the same DLL. A container often uses the first one:

``` dockerfile
ENTRYPOINT ["dotnet", "MyApi.dll"]
```

------------------------------------------------------------------------

## 6. What publish does not copy

For a normal Web API publish, the big .NET runtime is **not** copied into the `publish` folder.

The ASP.NET runtime image already has it:

``` dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
```

That image contains the shared frameworks. Your published folder contains **your app**.

``` text
Container image
├── .NET runtime              ← already in the aspnet image
├── ASP.NET Core shared DLLs  ← already in the aspnet image
└── publish/
     ├── MyApi.dll            ← your app
     ├── MyApi.deps.json
     ├── MyApi.runtimeconfig.json
     └── your extra package DLLs
```

This style is called **framework-dependent**. The app depends on a runtime that is already installed in the image.

There is another style, **self-contained** publish. That copies the runtime next to your app, so the folder is much bigger. You do not need that when the image is `dotnet/aspnet`.

------------------------------------------------------------------------

## 7. One picture

``` text
You write
  Program.cs, Controllers, Services, appsettings.json
        │
        │  dotnet publish
        ▼
publish/
  MyApi.dll                 your compiled app
  MyApi.deps.json           which DLLs to load
  MyApi.runtimeconfig.json  which runtime to use
  appsettings.json          settings
  extra package DLLs        libraries you added
        │
        │  copied into the container
        ▼
dotnet MyApi.dll
```

The DLL is the product of your project. It still needs something to start it.

That "something" is the .NET host and the .NET runtime. That story is here: [How the host runs your API](../HostRuntime/HowTheHostRunsYourApi.md).

------------------------------------------------------------------------

## Memory trick

``` text
.csproj     → what kind of project this is
Source      → the code you wrote
Assembly    → that code compiled into MyApi.dll
deps.json   → which DLLs are needed
runtimeconfig.json → which runtime is needed
publish/    → the folder you deploy
aspnet image → the runtime that is already installed
```
