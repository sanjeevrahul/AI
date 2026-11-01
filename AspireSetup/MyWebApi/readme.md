# .NET Aspire Setup

This project demonstrates a standard **project-based .NET Aspire application** with an Aspire AppHost orchestrating multiple .NET applications.

The setup can be extended with ASP.NET Core Web Apps, Web APIs, Console Apps, Workers, Redis, databases, MCP servers, and AI services.

---

## 1. Solution Structure

The current solution follows the standard Aspire project structure:

```text
AspireSetup/
│
├── AspireSetup.sln
│
├── AspireSetup.AppHost/
│   ├── AspireSetup.AppHost.csproj
│   └── Program.cs
│
├── MyConsoleApp/
│   ├── MyConsoleApp.csproj
│   └── Program.cs
│
└── MyWebApp/
    ├── MyWebApp.csproj
    └── Program.cs
```

### Project responsibilities

| Project               | Responsibility                                |
| --------------------- | --------------------------------------------- |
| `AspireSetup.AppHost` | Aspire orchestration and application topology |
| `MyConsoleApp`        | Console application                           |
| `MyWebApp`            | ASP.NET Core web application                  |

The important concept is:

```text
                    Aspire AppHost
                         │
             ┌───────────┴───────────┐
             │                       │
             ▼                       ▼
       MyConsoleApp              MyWebApp
```

The **AppHost does not contain the business application code**. It describes how the applications and infrastructure are composed and run.

---

# 2. Prerequisites

Install:

* .NET SDK
* .NET Aspire CLI
* Visual Studio or VS Code

Verify the installations:

```powershell
dotnet --version
aspire --version
```

---

# 3. Create a New Solution

Create a working directory:

```powershell
mkdir AspireSetup
cd AspireSetup
```

Create the solution:

```powershell
dotnet new sln -n AspireSetup
```

Result:

```text
AspireSetup/
└── AspireSetup.sln
```

---

# 4. Create a Console Application

Create the console application:

```powershell
dotnet new console -n MyConsoleApp
```

Add it to the solution:

```powershell
dotnet sln add .\MyConsoleApp\MyConsoleApp.csproj
```

Test it:

```powershell
dotnet run --project .\MyConsoleApp
```

Expected output:

```text
Hello, World!
```

---

# 5. Initialise Aspire

From the solution root:

```powershell
aspire init
```

Aspire creates the AppHost project.

The resulting structure should be similar to:

```text
AspireSetup/
│
├── AspireSetup.sln
│
├── AspireSetup.AppHost/
│   ├── AspireSetup.AppHost.csproj
│   └── Program.cs
│
└── MyConsoleApp/
    ├── MyConsoleApp.csproj
    └── Program.cs
```

> The exact generated project names may differ depending on the Aspire CLI version.

---

# 6. Add a Project to the Aspire AppHost

The AppHost needs a project reference to the application it will orchestrate.

For the console application:

```powershell
dotnet add .\AspireSetup.AppHost\AspireSetup.AppHost.csproj reference .\MyConsoleApp\MyConsoleApp.csproj
```

This creates the relationship:

```text
AspireSetup.AppHost
        │
        │ ProjectReference
        ▼
MyConsoleApp
```

---

# 7. Configure the AppHost

Open:

```text
AspireSetup.AppHost/Program.cs
```

Add the console application:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyConsoleApp>("console-app");

builder.Build().Run();
```

### Explanation

```csharp
var builder = DistributedApplication.CreateBuilder(args);
```

Creates the Aspire distributed application builder.

```csharp
builder.AddProject<Projects.MyConsoleApp>("console-app");
```

Registers the console application as an Aspire resource.

```csharp
builder.Build().Run();
```

Builds and starts the distributed application.

---

# 8. Add an ASP.NET Core Web Application

Create a web application from the solution root:

```powershell
dotnet new web -n MyWebApp
```

Add it to the solution:

```powershell
dotnet sln add .\MyWebApp\MyWebApp.csproj
```

Add a project reference from the AppHost:

```powershell
dotnet add .\AspireSetup.AppHost\AspireSetup.AppHost.csproj reference .\MyWebApp\MyWebApp.csproj
```

The solution becomes:

```text
AspireSetup/
│
├── AspireSetup.sln
│
├── AspireSetup.AppHost/
│   ├── AspireSetup.AppHost.csproj
│   └── Program.cs
│
├── MyConsoleApp/
│   ├── MyConsoleApp.csproj
│   └── Program.cs
│
└── MyWebApp/
    ├── MyWebApp.csproj
    └── Program.cs
```

---

# 9. Register the Web Application

Update:

```text
AspireSetup.AppHost/Program.cs
```

to:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyConsoleApp>("console-app");

builder.AddProject<Projects.MyWebApp>("web-app");

builder.Build().Run();
```

The application topology is now:

```text
                    Aspire AppHost
                         │
              ┌──────────┴──────────┐
              │                     │
              ▼                     ▼
        Console App              Web App
       MyConsoleApp             MyWebApp
```

---

# 10. Run Aspire

From the solution root:

```powershell
aspire run
```

Aspire starts the AppHost and launches the registered resources.

The Aspire Dashboard provides information about:

* Applications
* Resource status
* Endpoints
* Logs
* Traces
* Metrics
* Environment information

The web application endpoint can be opened directly from the Aspire Dashboard.

---

# 11. Build the Solution

Before running Aspire, the complete solution can be built with:

```powershell
dotnet build
```

This validates the AppHost and application projects.

---

# 12. Add an ASP.NET Core Web API

For a Web API instead of a basic web application:

```powershell
dotnet new webapi -n MyWebApi
```

Add it to the solution:

```powershell
dotnet sln add .\MyWebApi\MyWebApi.csproj
```

Add the AppHost reference:

```powershell
dotnet add .\AspireSetup.AppHost\AspireSetup.AppHost.csproj reference .\MyWebApi\MyWebApi.csproj
```

Register it:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MyConsoleApp>("console-app");

builder.AddProject<Projects.MyWebApp>("web-app");

builder.AddProject<Projects.MyWebApi>("api");

builder.Build().Run();
```

---

# 13. Adding Infrastructure

One of the main benefits of Aspire is that the AppHost can also define infrastructure dependencies.

For example, Redis:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

builder.AddProject<Projects.MyWebApi>("api")
       .WithReference(cache);

builder.Build().Run();
```

The resulting topology becomes:

```text
                 Aspire AppHost
                       │
                       ▼
                    Web API
                       │
                       │ WithReference
                       ▼
                     Redis
```

Aspire manages the local resource and provides the connection information to the application.

---

# 14. Example Distributed Application

A larger application can look like:

```text
                         Aspire AppHost
                              │
          ┌───────────────────┼───────────────────┐
          │                   │                   │
          ▼                   ▼                   ▼
       Web App              API              Background Worker
          │                   │
          │                   ├───────────────┐
          │                   │               │
          ▼                   ▼               ▼
                         Redis             Database
```

The AppHost becomes the definition of the local distributed system.

---

# 15. Microsoft AI / MCP Extension

This Aspire structure is also suitable for Microsoft AI development.

A future solution could contain:

```text
MicrosoftAIPlayground/
│
├── MicrosoftAIPlayground.AppHost/
│
├── AI.Web/
│   └── ASP.NET Core Web API
│
├── AI.Agent/
│   └── Microsoft Agent Framework
│
├── AI.McpServer/
│   └── MCP Server
│
├── AI.Worker/
│   └── Background Worker
│
└── AI.Shared/
    └── Shared .NET components
```

The AppHost can orchestrate the components:

```text
                     Aspire AppHost
                           │
       ┌───────────────────┼───────────────────┐
       │                   │                   │
       ▼                   ▼                   ▼
    AI.Web             AI.Agent           MCP Server
       │                   │                   │
       │                   └───────┬───────────┘
       │                           │
       ▼                           ▼
    Redis                    External Tools
       │
       ▼
   Database
```

This provides a useful foundation for experimenting with:

* .NET
* ASP.NET Core
* Microsoft Foundry
* Microsoft Agent Framework
* MCP
* RAG
* Azure services
* Redis
* SQL databases
* Background workers

---

# 16. Important Aspire Concepts

## AppHost

The AppHost defines the distributed application's topology.

It answers:

> What applications and infrastructure make up my system?

---

## Application Project

An application project contains the actual application logic.

Examples:

```text
MyWebApp
MyWebApi
MyConsoleApp
MyWorker
AI.Agent
MCP.Server
```

---

## Resource

Anything managed by Aspire can be treated as a resource.

Examples:

```text
Web API
Console App
Worker
Redis
SQL Server
PostgreSQL
RabbitMQ
Azure services
```

---

## Reference

A reference expresses a dependency between resources.

For example:

```csharp
api.WithReference(cache);
```

means the API depends on the Redis resource.

---

# 17. Recommended Development Workflow

When adding a new application:

### 1. Create the project

```powershell
dotnet new <template> -n MyNewApp
```

### 2. Add it to the solution

```powershell
dotnet sln add .\MyNewApp\MyNewApp.csproj
```

### 3. Add it to AppHost

```powershell
dotnet add .\AspireSetup.AppHost\AspireSetup.AppHost.csproj reference .\MyNewApp\MyNewApp.csproj
```

### 4. Register it

```csharp
builder.AddProject<Projects.MyNewApp>("my-new-app");
```

### 5. Run

```powershell
aspire run
```

---

# 18. Project-Based vs File-Based AppHost

Aspire also supports a newer file-based AppHost approach using:

```text
apphost.cs
```

For example:

```csharp
#:sdk Aspire.AppHost.Sdk@13.6.0
#:property AspireUseCliBundle=true

var builder = DistributedApplication.CreateBuilder(args);

builder.Build().Run();
```

This is different from the traditional project-based structure.

For a solution containing multiple applications, the **project-based AppHost structure used in this README** provides a clear separation between:

```text
AppHost
   │
   ├── Web App
   ├── API
   ├── Worker
   └── Infrastructure
```

and is the structure used for this playground.

---

# 19. Quick Reference

### Create solution

```powershell
dotnet new sln -n AspireSetup
```

### Create console app

```powershell
dotnet new console -n MyConsoleApp
dotnet sln add .\MyConsoleApp\MyConsoleApp.csproj
```

### Initialise Aspire

```powershell
aspire init
```

### Add project to AppHost

```powershell
dotnet add .\AspireSetup.AppHost\AspireSetup.AppHost.csproj reference .\MyConsoleApp\MyConsoleApp.csproj
```

### Create web app

```powershell
dotnet new web -n MyWebApp
dotnet sln add .\MyWebApp\MyWebApp.csproj
```

### Add web app to AppHost

```powershell
dotnet add .\AspireSetup.AppHost\AspireSetup.AppHost.csproj reference .\MyWebApp\MyWebApp.csproj
```

### Build

```powershell
dotnet build
```

### Run Aspire

```powershell
aspire run
```

---

# 20. Core Mental Model

The most important concept to remember is:

```text
                    ┌─────────────────────┐
                    │    Aspire AppHost   │
                    │                     │
                    │ Application         │
                    │ Topology            │
                    │ Dependencies        │
                    │ Infrastructure      │
                    └──────────┬──────────┘
                               │
             ┌─────────────────┼─────────────────┐
             │                 │                 │
             ▼                 ▼                 ▼
          Web App             API             Worker
             │                 │                 │
             └─────────────────┼─────────────────┘
                               │
                     ┌─────────┴─────────┐
                     ▼                   ▼
                   Redis              Database
```

**Aspire is the orchestrator.**

Your Web/API/Console/Worker projects remain normal .NET applications.

The AppHost describes how those applications work together.

This separation is the foundation for building larger distributed .NET and AI applications with Aspire.
