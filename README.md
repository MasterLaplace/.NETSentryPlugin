# MySentry.Plugin

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=.net)
![Sentry](https://img.shields.io/badge/Sentry-4.12.1-362D59?style=for-the-badge&logo=sentry)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)
![CI](https://img.shields.io/badge/CI-GitLab%20%7C%20GitHub-orange?style=for-the-badge)
![Tests](https://img.shields.io/badge/Tests-60%2F60-brightgreen?style=for-the-badge)

**An enterprise-grade Sentry integration plugin for .NET applications**

[Quick Start](#-30-minute-setup-guide) • [Features](#-features) • [Documentation](#-documentation) • [Architecture](#-architecture) • [Troubleshooting](#-troubleshooting)

</div>

---

## 📑 Table of Contents

- [Overview](#-overview)
- [30-Minute Setup Guide](#-30-minute-setup-guide)
  - [Prerequisites](#prerequisites)
  - [Step 1: Sentry Account Setup (5 min)](#step-1-sentry-account-setup-5-min)
  - [Step 2: Project Configuration (5 min)](#step-2-project-configuration-5-min)
  - [Step 3: Code Integration (10 min)](#step-3-code-integration-10-min)
  - [Step 4: GitLab CI/CD Setup (10 min)](#step-4-gitlab-cicd-setup-10-min)
- [Features](#-features)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Usage Examples](#-usage-examples)
- [Documentation](#-documentation)
- [Architecture](#-architecture)
- [Testing](#-testing)
- [Troubleshooting](#-troubleshooting)
- [License](#-license)

---

## 🎯 Overview

**MySentry.Plugin** is a comprehensive, production-ready wrapper around the Sentry .NET SDK that provides:

- 🛡️ **Clean API** - Fluent builder pattern for intuitive configuration
- 🏗️ **SOLID Architecture** - Interface-segregated design for maximum flexibility
- 🔧 **Testability** - Full abstraction layer enables easy mocking and testing
- 📊 **Full Sentry Coverage** - Error capture, tracing, profiling, breadcrumbs, crons, and more
- ⚡ **Performance** - Automatic instrumentation with minimal overhead

---

## 🚀 30-Minute Setup Guide

> **Goal**: Go from zero to production-ready Sentry integration with GitLab CI/CD

### Prerequisites

- [ ] .NET 8.0 SDK installed
- [ ] Git repository (GitLab or GitHub)
- [ ] Sentry account (free tier works)
- [ ] 30 minutes of your time ☕

### Step 1: Sentry Account Setup (5 min)

#### 1.1 Create Sentry Account

1. Go to [sentry.io](https://sentry.io) and sign up
2. Create a new organization (or use existing)
3. Create a new project:
   - Select **.NET** as platform
   - Name it (e.g., `my-app-backend`)
   - Click **Create Project**

#### 1.2 Get Your DSN

1. After project creation, you'll see your DSN
2. Copy it - it looks like:
   ```
   https://abc123@o123456.ingest.sentry.io/7890123
   ```

#### 1.3 Create Auth Token

1. Go to **User Settings** → **Auth Tokens** → **Create New Token**
2. Select scopes:
   - ✅ `project:releases`
   - ✅ `project:read`
   - ✅ `org:read`
3. Copy the token (you won't see it again!)

> 📖 Detailed guide: [docs/SETUP_SENTRY.md](docs/SETUP_SENTRY.md)

---

### Step 2: Project Configuration (5 min)

#### 2.1 Install Packages

```bash
dotnet add package MySentry.Plugin
```

#### 2.2 Configure appsettings.json

Add to your `appsettings.json`:

```json
{
  "MySentry": {
    "Dsn": "",
    "Environment": "Development",
    "Enabled": true,
    "EnableTracing": true,
    "TracesSampleRate": 1.0,
    "ProfilesSampleRate": 0.1,
    "Release": "1.0.0",
    "Debug": false
  }
}
```

#### 2.3 Create Environment Files

**Create `.env.example`** (commit this):
```bash
# Sentry Configuration
SENTRY_DSN=https://your-key@your-org.ingest.sentry.io/project-id
SENTRY_AUTH_TOKEN=sntrys_eyJ...
SENTRY_ORG=your-organization
SENTRY_PROJECT=your-project-name
SENTRY_ENVIRONMENT=Development
```

**Create `.env`** (DO NOT commit - add to `.gitignore`):
```bash
SENTRY_DSN=https://actual-key@actual-org.ingest.sentry.io/actual-id
SENTRY_AUTH_TOKEN=sntrys_actual_token...
```

---

### Step 3: Code Integration (10 min)

#### 3.1 Program.cs Setup

```csharp
using MySentry.Plugin.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add MySentry
builder.AddMySentry(options =>
{
    // Use environment variable or appsettings
    options.Dsn = Environment.GetEnvironmentVariable("SENTRY_DSN") 
        ?? builder.Configuration["MySentry:Dsn"];
    options.Environment = builder.Environment.EnvironmentName;
    options.Release = "1.0.0";
    options.EnableTracing = true;
    options.TracesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.2;
});

// ... other services ...

var app = builder.Build();

// Use MySentry middleware
app.UseMySentry();

// ... other middleware ...

app.Run();
```

#### 3.2 Service Integration

```csharp
public class OrderService
{
    private readonly ISentryPlugin _sentry;
    
    public OrderService(ISentryPlugin sentry)
    {
        _sentry = sentry;
    }
    
    public async Task<Order> ProcessOrderAsync(OrderRequest request)
    {
        // Start a transaction
        using var transaction = _sentry.StartTransaction(
            "process-order", 
            "order.process");
        
        try
        {
            // Add breadcrumb for debugging
            _sentry.AddBreadcrumb(
                $"Processing order for customer {request.CustomerId}",
                "order");
            
            // Your business logic with spans
            using (var span = transaction.StartChild("validation", "Validate order"))
            {
                ValidateOrder(request);
                span.SetStatus(PluginSpanStatus.Ok);
            }
            
            using (var span = transaction.StartChild("db.insert", "Save to database"))
            {
                await SaveOrderAsync(request);
                span.SetStatus(PluginSpanStatus.Ok);
            }
            
            transaction.SetStatus(PluginSpanStatus.Ok);
            return order;
        }
        catch (Exception ex)
        {
            transaction.SetStatus(PluginSpanStatus.InternalError);
            _sentry.CaptureException(ex);
            throw;
        }
    }
}
```

#### 3.3 Test Locally

```bash
# Set environment variable
$env:SENTRY_DSN = "your-dsn-here"  # PowerShell
# or
export SENTRY_DSN="your-dsn-here"  # Bash

# Run the application
dotnet run

# Trigger a test error
curl https://localhost:5001/api/test/error/exception
```

Check Sentry dashboard - you should see the error! ✅

---

### Step 4: GitLab CI/CD Setup (10 min)

#### 4.1 Add GitLab CI Variables

In GitLab → **Settings** → **CI/CD** → **Variables**:

| Variable | Value | Protected | Masked |
|----------|-------|-----------|--------|
| `SENTRY_DSN` | Your DSN | ✅ | ✅ |
| `SENTRY_AUTH_TOKEN` | Your auth token | ✅ | ✅ |
| `SENTRY_ORG` | Your organization slug | ❌ | ❌ |
| `SENTRY_PROJECT` | Your project slug | ❌ | ❌ |

#### 4.2 The CI Pipeline

A complete `.gitlab-ci.yml` is included in this repository. Key features:

```yaml
# Highlights of the pipeline:

# 1. Build & Test
build:
  script:
    - dotnet restore
    - dotnet build --no-restore
    - dotnet test --no-build

# 2. Create Release in Sentry
sentry-release:
  script:
    - sentry-cli releases new $VERSION
    - sentry-cli releases set-commits $VERSION --auto
    - sentry-cli releases finalize $VERSION

# 3. Deploy & Notify
deploy:
  script:
    - ./deploy.sh
    - sentry-cli releases deploys $VERSION new -e production
```

#### 4.3 Connect GitLab to Sentry

1. In Sentry: **Settings** → **Integrations** → **GitLab**
2. Follow the connection wizard
3. Select your repository

> 📖 Detailed guide: [docs/SETUP_GITLAB.md](docs/SETUP_GITLAB.md)

---

### ✅ Setup Complete!

You now have:
- ✅ Error tracking in Sentry
- ✅ Performance monitoring with traces
- ✅ Automated releases via CI/CD
- ✅ Commit association for debugging
- ✅ Deploy tracking in Sentry

**What happens now:**
1. Push code → GitLab builds and tests
2. On success → Sentry release created with commits
3. Deploy → Sentry notified of deployment
4. Errors → Full stack trace + which release introduced it

---



## ✨ Features

| Feature | Description |
|---------|-------------|
| **Error Capture** | Automatic and manual exception handling with full context |
| **Performance Monitoring** | Distributed tracing with spans and transactions |
| **Profiling** | CPU profiling for performance bottleneck detection |
| **Breadcrumbs** | Automatic trail of events leading to errors |
| **User Context** | Associate errors with user information |
| **Scope Management** | Isolate and configure context per operation |
| **Cron Monitoring** | Track scheduled job health and performance |
| **User Feedback** | Capture user-submitted feedback for errors |
| **Structured Logging** | Integration with Microsoft.Extensions.Logging |

## 📦 Installation

### NuGet Package

```bash
dotnet add package MySentry.Plugin
```

### Package Manager Console

```powershell
Install-Package MySentry.Plugin
```

## 🚀 Quick Start

### 1. Configure via Fluent Builder (Recommended)

```csharp
using MySentry.Plugin.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddMySentry(options =>
{
    options.Dsn = "https://your-key@sentry.io/project";
    options.Environment = builder.Environment.EnvironmentName;
    options.EnableTracing = true;
    options.TracesSampleRate = 0.2;
    options.ProfilesSampleRate = 0.1;
    
    // Configure tracing
    options.Tracing.EnableAutoInstrumentation = true;
    
    // Configure profiling
    options.Profiling.Enabled = true;
});

var app = builder.Build();
app.UseMySentry();
app.Run();
```

### 2. Configure via appsettings.json

```json
{
  "MySentry": {
    "Dsn": "https://your-key@sentry.io/project",
    "Environment": "Production",
    "EnableTracing": true,
    "TracesSampleRate": 0.2,
    "ProfilesSampleRate": 0.1,
    "Tracing": {
      "EnableAutoInstrumentation": true
    },
    "Profiling": {
      "Enabled": true
    }
  }
}
```

```csharp
builder.AddMySentry(); // Reads from appsettings.json automatically
```

## 📖 Usage Examples

### Capture Exceptions

```csharp
public class MyService
{
    private readonly ISentryPlugin _sentry;

    public MyService(ISentryPlugin sentry)
    {
        _sentry = sentry;
    }

    public void DoWork()
    {
        try
        {
            // Your code
        }
        catch (Exception ex)
        {
            var eventId = _sentry.CaptureException(ex);
            _logger.LogError("Error captured: {EventId}", eventId);
        }
    }
}
```

### Add Breadcrumbs

```csharp
_sentry.AddBreadcrumb("User clicked submit", "ui.click");
_sentry.AddBreadcrumb("Processing payment", "payment", BreadcrumbLevel.Info,
    new Dictionary<string, string> { ["amount"] = "99.99" });
```

### Set User Context

```csharp
_sentry.SetUser(new SentryUser
{
    Id = "user-123",
    Email = "user@example.com",
    Username = "johndoe"
});
```

### Create Transactions

```csharp
using var transaction = _sentry.StartTransaction("order-processing", "task");

using (var span = transaction.StartChild("db.query", "Fetch order"))
{
    // Database operation
    span.SetStatus(SpanStatus.Ok);
}

using (var span = transaction.StartChild("http.client", "Call payment API"))
{
    // HTTP call
    span.SetStatus(SpanStatus.Ok);
}

transaction.SetStatus(SpanStatus.Ok);
transaction.Finish();
```

### Using Extension Methods

```csharp
// Simple span wrapper
await _sentry.WithSpanAsync("process-data", "Processing user data", async () =>
{
    await ProcessDataAsync();
});

// With return value
var result = await _sentry.WithTransactionAsync("complex-operation", "task", async () =>
{
    return await DoComplexWorkAsync();
});
```

### Cron Job Monitoring

```csharp
// Using extension methods
await _sentry.WithCronMonitoringAsync("daily-cleanup", async () =>
{
    await CleanupOldRecordsAsync();
});

// Manual control
using var monitor = _sentry.MonitorCronJob("hourly-sync");
try
{
    await SyncDataAsync();
    monitor.Complete();
}
catch
{
    monitor.Fail();
    throw;
}
```

### User Feedback

```csharp
// Capture exception and attach feedback
var eventId = _sentry.CaptureExceptionWithFeedback(
    exception,
    "user@email.com",
    "The app crashed when I clicked submit",
    "John Doe");
```

## 🏛️ Architecture

### Design Principles

This plugin follows **SOLID** principles:

- **S**ingle Responsibility - Each class has one job
- **O**pen/Closed - Extended via composition, not modification
- **L**iskov Substitution - Implementations are fully interchangeable
- **I**nterface Segregation - Fine-grained interfaces for specific needs
- **D**ependency Inversion - Depend on abstractions, not concretions

### Project Structure

```
MySentry.Plugin/
├── Abstractions/         # Interfaces and core types
│   ├── ISentryPlugin.cs  # Main composite interface
│   ├── IErrorCapture.cs  # Error capture operations
│   ├── IPerformanceMonitor.cs
│   └── ...
├── Configuration/        # Options and builders
│   ├── SentryPluginOptions.cs
│   ├── SentryPluginBuilder.cs
│   └── ...
├── Core/                 # Main implementations
│   ├── SentryPlugin.cs   # Primary service
│   └── ...
├── Extensions/           # DI and helper extensions
│   ├── ServiceCollectionExtensions.cs
│   └── ...
├── Features/             # Feature-specific modules
│   ├── Tracing/
│   ├── Profiling/
│   ├── Crons/
│   └── ...
├── Middleware/           # ASP.NET Core middleware
│   ├── MySentryMiddleware.cs
│   └── PerformanceMiddleware.cs
└── Enrichers/            # Event enrichment
```

### Key Interfaces

| Interface | Purpose |
|-----------|---------|
| `ISentryPlugin` | Main entry point - composes all other interfaces |
| `IErrorCapture` | Exception and message capture |
| `IPerformanceMonitor` | Transaction and span management |
| `IBreadcrumbTracker` | Breadcrumb trail management |
| `IUserContextProvider` | User identity management |
| `IScopeManager` | Scope configuration and isolation |
| `ICronMonitor` | Scheduled job monitoring |
| `IUserFeedbackCapture` | User feedback submission |

## ⚙️ Configuration Reference

### SentryPluginOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Dsn` | `string` | - | Your Sentry DSN (required) |
| `Environment` | `string` | - | Deployment environment |
| `Debug` | `bool` | `false` | Enable debug mode |
| `EnableTracing` | `bool` | `false` | Enable performance monitoring |
| `TracesSampleRate` | `double` | `0.0` | Traces sample rate (0.0-1.0) |
| `ProfilesSampleRate` | `double` | `0.0` | Profiles sample rate (0.0-1.0) |
| `AttachStacktrace` | `bool` | `true` | Attach stack traces to messages |
| `MaxBreadcrumbs` | `int` | `100` | Maximum breadcrumbs to keep |
| `SendDefaultPii` | `bool` | `false` | Send personally identifiable info |
| `Release` | `string` | - | Release/version identifier |
| `ServerName` | `string` | - | Server name tag |

### TracingOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `EnableAutoInstrumentation` | `bool` | `true` | Auto-instrument HTTP requests |
| `CaptureBodyContent` | `bool` | `false` | Capture request/response bodies |
| `RecordHttpTimings` | `bool` | `true` | Record HTTP timing data |

### ProfilingOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Enabled` | `bool` | `false` | Enable CPU profiling |
| `SampleRate` | `double` | - | Override sample rate |

## 🧪 Testing

The plugin is designed for testability. All functionality is exposed through interfaces:

```csharp
public class MyServiceTests
{
    [Fact]
    public void DoWork_WhenError_CapturesException()
    {
        // Arrange
        var mockSentry = new Mock<ISentryPlugin>();
        var service = new MyService(mockSentry.Object);

        // Act
        service.DoWorkThatFails();

        // Assert
        mockSentry.Verify(x => x.CaptureException(It.IsAny<Exception>()), Times.Once);
    }
}
```

## 📁 Sample Application

The `MySentry.TestApp` project demonstrates all plugin capabilities:

```bash
cd src/MySentry.TestApp
dotnet run
```

Then visit:
- Swagger UI: `https://localhost:5001/swagger`
- Health: `https://localhost:5001/health`

Available test endpoints:
- `GET /api/test/error/exception` - Capture exception
- `GET /api/test/breadcrumbs` - Add breadcrumbs
- `GET /api/test/tracing/transaction` - Create transaction
- `GET /api/test/crons/simple` - Cron monitoring
- `POST /api/test/feedback` - User feedback

## 📜 License

This project is licensed under the MIT License.

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [SETUP_SENTRY.md](docs/SETUP_SENTRY.md) | Complete Sentry account & project setup |
| [SETUP_GITLAB.md](docs/SETUP_GITLAB.md) | GitLab CI/CD integration guide |
| [SETUP_TOKENS.md](docs/SETUP_TOKENS.md) | Token & secret reference |
| [TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md) | Common issues & solutions |
| [FEATURE_AUDIT.md](FEATURE_AUDIT.md) | Plugin vs SDK feature comparison |

---

## ❓ Troubleshooting

### Quick Fixes

| Issue | Solution |
|-------|----------|
| Events not appearing | Check DSN, enable `Debug = true` |
| 401 errors in CI | Verify `SENTRY_AUTH_TOKEN` variable |
| No traces | Set `TracesSampleRate > 0` |
| No commits in release | Connect GitLab integration |

> 📖 Full guide: [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)

---

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines before submitting PRs.

---

<div align="center">
Made with ❤️ for the .NET community
</div>
