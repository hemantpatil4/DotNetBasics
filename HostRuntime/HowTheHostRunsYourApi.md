# How the .NET Host Runs Your Web API

The other folder explains the files: [what build and publish produce](../ProjectAndPublish/AssemblyAndPublishOutput.md).

This folder answers the next question:

> Those files are sitting in a folder. Who starts them, and how does an HTTP request reach my code?

Let's build the picture slowly.

------------------------------------------------------------------------

## 1. Start with your Web API

You write something like:

``` csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/hello", () => "Hello");

app.Run();
```

This is **your application code**.

After publish, that code lives in:

``` text
MyApi.dll
```

Now the question is:

> Who runs this DLL?

The answer is: **the .NET host starts the application, and the .NET runtime provides the environment in which the .NET code executes.**

------------------------------------------------------------------------

## 2. The four things we need to distinguish

Think of these as layers inside **one process**:

``` text
.NET Host
    ↓
.NET Runtime
    ↓
ASP.NET Core
    ↓
Kestrel
    ↓
Your Web API
```

- **Host and Runtime** start and execute your application.
- **ASP.NET Core** is the web framework.
- **Kestrel** is the web server.
- **Your Web API** is the application you wrote.

They are not four separate applications. They are not four separate processes. They work together.

------------------------------------------------------------------------

## 3. .NET Host

The **.NET host is responsible for starting your .NET application**.

When you run:

``` bash
dotnet MyApi.dll
```

the `dotnet` program is the host you can see.

``` text
dotnet MyApi.dll
       ↑
    .NET Host
```

Think:

> **Host = "I need to start this .NET application."**

The host does a few practical jobs:

- Reads `MyApi.runtimeconfig.json` to find which runtime you need
- Reads `MyApi.deps.json` to find which assemblies to load
- Finds the runtime that is already installed (for example inside the ASP.NET image)
- Loads `MyApi.dll`
- Hands control to your application

Don't worry about the internal host pieces yet.

For the interview, remember:

> **The host starts the .NET application.**

------------------------------------------------------------------------

## 4. .NET Runtime

Once the application is started, the **.NET runtime provides the environment in which your .NET code executes**.

Think:

> **Runtime = "I provide the environment required to execute .NET code."**

Your code:

``` csharp
app.MapGet("/hello", () => "Hello");
```

runs under the .NET runtime.

The runtime provides:

- Garbage collection
- Memory management
- The type system
- Exception handling
- JIT compilation (turning IL in the DLL into CPU instructions)
- Threading

``` text
.NET Host
   ↓
starts
   ↓
.NET Runtime
   ↓
runs
   ↓
Your .NET code
```

The host starts things. The runtime executes your code.

------------------------------------------------------------------------

## 5. ASP.NET Core

.NET itself does not mean "Web API."

.NET can be used for:

``` text
Console applications
Web APIs
Background services
Desktop applications
```

**ASP.NET Core is the web framework.**

It gives your application:

``` text
Routing
Middleware
Controllers
Dependency injection
Authentication / Authorization
HTTP handling
Configuration
```

``` text
.NET
  │
  └── ASP.NET Core
          │
          └── Web application
```

Your Web API is an **ASP.NET Core application running on .NET**.

`WebApplication.CreateBuilder` and `app.MapGet` are ASP.NET Core. They are not the runtime itself.

------------------------------------------------------------------------

## 6. Kestrel

Ask this next:

> ASP.NET Core is my web framework. Who actually listens for HTTP requests?

That is **Kestrel**.

Kestrel is the **web server** used by ASP.NET Core.

``` text
Client
   │
   │ HTTP request
   ↓
Kestrel
   ↓
ASP.NET Core
   ↓
Your code
```

For `GET /hello`:

1. Kestrel receives the network request.
2. ASP.NET Core walks the request through routing and middleware.
3. Your code runs: `() => "Hello"`.

`app.Run()` is what keeps the process alive and lets Kestrel keep listening.

------------------------------------------------------------------------

## 7. Which one is the application?

Suppose the project is called `MyApi`.

| Thing | Very basic meaning |
| --- | --- |
| **Your Web API** | Your application. The code you wrote. |
| **ASP.NET Core** | The web framework that application is built on. |
| **Kestrel** | The web server that listens for HTTP. |
| **.NET Runtime** | The environment that executes your .NET code. |
| **.NET Host** | The starter. It launches the runtime and loads your DLL. |

**MyApi is your application.** The others are how it gets started and how it receives HTTP.

------------------------------------------------------------------------

## 8. Linux, then a container

Suppose the app runs on a Linux server.

``` text
Linux Server
│
├── CPU
├── RAM
├── Disk
└── Network
```

If your API runs code, the CPU does the machine-level work. If your API does `new Customer()`, that memory comes from the machine's RAM.

> **Your application uses resources from the operating system and the machine.**

A container packages the app and its dependencies, and isolates them.

``` text
Linux Worker Node
│
│  Linux Kernel
│  CPU / RAM / Network
│
├── Container
│    ├── .NET runtime          ← from the aspnet image
│    ├── MyApi.dll             ← from publish
│    ├── ASP.NET Core
│    └── Kestrel
│
└── Other containers
```

A container does **not** contain a full Linux kernel. Containers share the worker node's kernel. They are isolated from each other, but they use the node's CPU and RAM.

------------------------------------------------------------------------

## 9. Where CPU and RAM come from

Suppose the worker node has:

``` text
8 CPU
16 GB RAM
```

The container does not have its own physical CPU. Kubernetes or OpenShift can limit what the container may use:

``` text
CPU request:    500m
CPU limit:      1 CPU
Memory request: 512 MB
Memory limit:   1 GB
```

> **The application consumes CPU, memory, network, and storage from the worker node.**

------------------------------------------------------------------------

## 10. What happens when the container starts

A typical Dockerfile:

``` dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY publish/ .

ENTRYPOINT ["dotnet", "MyApi.dll"]
```

The image contains:

``` text
MyApi Image
├── .NET runtime
├── MyApi.dll
├── Other DLLs
├── Configuration
└── Startup command
     └── dotnet MyApi.dll
```

When the container starts:

``` text
Container starts
      │
      ▼
ENTRYPOINT
      │
      ▼
dotnet MyApi.dll
      │
      ▼
.NET Host reads runtimeconfig + deps
      │
      ▼
.NET Runtime executes MyApi.dll
      │
      ▼
ASP.NET Core builds the app
      │
      ▼
Kestrel listens for HTTP
```

Kubernetes or OpenShift does not execute your DLL itself. It starts the Pod. The container's startup command runs `dotnet MyApi.dll`.

``` text
Kubernetes / OpenShift
        │  decides where the Pod runs, restarts it, limits CPU and RAM
        ▼
      Pod
        │
        ▼
    Container
        │
        ▼
dotnet MyApi.dll
        │
        ▼
Your API
```

------------------------------------------------------------------------

## 11. A request through the whole system

``` text
GET /api/fx/rate
```

``` text
Client
  │
  │ HTTP
  ▼
Load Balancer / Route
  │
  ▼
Kubernetes Service
  │
  ▼
Pod → Container
  │
  ▼
Kestrel
  │
  ▼
ASP.NET Core pipeline
  │
  ▼
Your controller
  │
  ▼
Your service
  │
  ▼
Database
```

The request does not jump straight to your controller. It reaches Kestrel first, then the ASP.NET Core pipeline, then your code.

------------------------------------------------------------------------

## 12. The picture to memorize

``` text
Linux Worker Node
  CPU / RAM / Network
        │
        ▼
    Container
        │
        ▼
  dotnet MyApi.dll
        │
        ▼
    .NET Host          starts the app
        │
        ▼
    .NET Runtime       executes the DLL
        │
        ▼
    ASP.NET Core       web framework
        │
        ▼
    Kestrel            listens for HTTP
        │
        ▼
    Your API code
```

------------------------------------------------------------------------

## Interview answer

If someone asks how the API runs after deployment:

> After publish, the Web API is a DLL plus a few config files. The container image already has the .NET runtime. When the Pod starts, the container runs `dotnet MyApi.dll`. The host reads the config, loads the DLL, and the runtime executes it. ASP.NET Core is the web framework. Kestrel is the web server that listens for HTTP. The process uses CPU and memory from the Linux worker node.

------------------------------------------------------------------------

## Memory trick

``` text
Host      → starts
Runtime   → executes
ASP.NET   → web framework
Kestrel   → web server
API       → your application
Container → packages and isolates the app
Pod       → the Kubernetes unit that runs the container
Worker node → the machine that provides CPU and RAM
```
