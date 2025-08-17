# Simplifying Dependency Injection in .NET Using Scrutor

## Introduction

Dependency Injection (DI) is a fundamental design pattern in .NET that promotes loose coupling, better testability, and maintainable code. While .NET's built-in DI container is powerful, manually registering multiple services can become verbose and error-prone, especially in large applications.

**Scrutor** is a lightweight, open-source library that extends the Microsoft DI container with assembly scanning and decoration support, making service registration much more elegant and maintainable.

## The Problem with Manual DI Registration

Consider a typical .NET application with multiple services. Without Scrutor, you'd need to manually register each service:

```csharp
// Traditional manual registration approach
builder.Services.AddScoped<IReportGenerator, ReportGenerator>();
builder.Services.AddScoped<IWriter, ExcelWriter>();
builder.Services.AddScoped<IWriter, PdfWriter>();
builder.Services.AddScoped<INotifier, EmailNotifier>();
builder.Services.AddScoped<INotifier, SmsNotifier>();
builder.Services.AddScoped<IReportArchiver, FileSystemArchiver>();
```

This approach has several drawbacks:
- **Repetitive code**: Similar registration patterns repeated for each service
- **Error-prone**: Easy to forget registering a service or register it with wrong lifetime
- **Maintenance overhead**: Adding new services requires updating multiple places
- **Inconsistency**: Different developers might use different registration patterns

## Enter Scrutor: The Solution

Scrutor provides assembly scanning capabilities that automatically discover and register services based on conventions. Let's explore how it works using a real project example.

## Project Structure Overview

Our example project demonstrates a report generation system with the following components:

```
SimplyDIWithScrutor/
├── Services/
│   ├── IReportGenerator.cs
│   ├── ReportGenerator.cs
│   └── NamedWriterService.cs
├── Writers/
│   ├── IWriter.cs
│   ├── ExcelWriter.cs
│   ├── PdfWriter.cs
│   ├── IWriterFactory.cs
│   └── WriterFactory.cs
├── Notifiers/
│   ├── INotifier.cs
│   ├── EmailNotifier.cs
│   └── SmsNotifier.cs
├── Archivers/
│   ├── IReportArchiver.cs
│   └── FileSystemArchiver.cs
└── Controllers/
    └── ReportController.cs
```

## Installing Scrutor

First, add the Scrutor NuGet package to your project:

```xml
<PackageReference Include="Scrutor" Version="6.1.0" />
```

Or using the .NET CLI:

```bash
dotnet add package Scrutor
```

## Implementing Assembly Scanning

The magic happens in your `Program.cs` file. Here's how Scrutor simplifies service registration:

```csharp
builder.Services.Scan(scan => scan
    // Scan the assembly where our services are defined
    .FromAssemblyOf<IWriter>()
    // Find all public, non-abstract classes
    .AddClasses(publicOnly: true)
    // Register each class against the interface(s) it implements
    .AsImplementedInterfaces()
    // Also register each class as itself
    .AsSelf()
    // Set the lifetime for all registered services
    .WithScopedLifetime()
);
```

Let's break down what each method does:

### 1. `.FromAssemblyOf<IWriter>()`
This method scans the assembly containing the `IWriter` type. You can use any class from your services assembly as the reference point.

### 2. `.AddClasses(publicOnly: true)`
Finds all public, non-abstract classes in the assembly. The `publicOnly: true` parameter ensures only public classes are registered.

### 3. `.AsImplementedInterfaces()`
Automatically registers each class against all interfaces it implements. For example:
- `ExcelWriter` → `IWriter`
- `PdfWriter` → `IWriter`
- `EmailNotifier` → `INotifier`
- `SmsNotifier` → `INotifier`

### 4. `.AsSelf()`
Registers each class as itself, allowing you to inject either the interface or the concrete class.

### 5. `.WithScopedLifetime()`
Sets the service lifetime for all registered services. Other options include:
- `.WithSingletonLifetime()`
- `.WithTransientLifetime()`

## Handling Multiple Implementations of the Same Interface

One of the key challenges when using Scrutor with multiple implementations of the same interface is that the DI container won't know which implementation to inject by default. Our project demonstrates this with `ExcelWriter` and `PdfWriter` both implementing `IWriter`.

### Solution 1: Factory Pattern (Recommended)

We've implemented a factory pattern to handle multiple writer implementations:

```csharp
public interface IWriterFactory
{
    IWriter CreateWriter(string format);
    IEnumerable<string> GetAvailableFormats();
}

public class WriterFactory : IWriterFactory
{
    private readonly IServiceProvider _serviceProvider;

    public WriterFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IWriter CreateWriter(string format)
    {
        return format.ToLower() switch
        {
            "excel" => _serviceProvider.GetRequiredService<ExcelWriter>(),
            "pdf" => _serviceProvider.GetRequiredService<PdfWriter>(),
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    public IEnumerable<string> GetAvailableFormats()
    {
        return new[] { "excel", "pdf" };
    }
}
```

### Solution 2: Named Services

Alternatively, you can use Scrutor's named services feature:

```csharp
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IWriter>()
    .AddClasses(publicOnly: true)
    .AsImplementedInterfaces()
    .AsSelf()
    .WithScopedLifetime()
    .Named(implementation => 
    {
        return implementation.Name switch
        {
            "ExcelWriter" => "excel",
            "PdfWriter" => "pdf",
            _ => implementation.Name.ToLower()
        };
    })
);
```

## Service Implementation Examples

Let's examine how our services are implemented and how Scrutor automatically registers them:

### Writers

```csharp
public interface IWriter
{
    string Write(string reportName);
}

public class ExcelWriter : IWriter
{
    public string Write(string reportName)
    {
        var message = $"Successfully generated Excel report: {reportName}.xlsx";
        Console.WriteLine(message);
        return message;
    }
}

public class PdfWriter : IWriter
{
    public string Write(string reportName)
    {
        var message = $"Successfully generated PDF report: {reportName}.pdf";
        Console.WriteLine(message);
        return message;
    }
}
```

### Notifiers

```csharp
public interface INotifier
{
    void Send(string message);
}

public class EmailNotifier : INotifier
{
    public void Send(string message)
    {
        Console.WriteLine($"EMAIL NOTIFICATION: {message}");
    }
}

public class SmsNotifier : INotifier
{
    public void Send(string message)
    {
        Console.WriteLine($"SMS NOTIFICATION: {message}");
    }
}
```

### Report Generator Service (Updated for Factory Pattern)

```csharp
public class ReportGenerator : IReportGenerator
{
    private readonly IWriterFactory _writerFactory;
    private readonly IEnumerable<INotifier> _notifiers;

    public ReportGenerator(IWriterFactory writerFactory, IEnumerable<INotifier> notifiers)
    {
        _writerFactory = writerFactory;
        _notifiers = notifiers;
    }

    public string GenerateReport(string reportName, string format = "excel")
    {
        var writer = _writerFactory.CreateWriter(format);
        var result = writer.Write(reportName);

        // Notify all registered notifiers
        foreach (var notifier in _notifiers)
        {
            notifier.Send($"Report '{reportName}' was successfully generated in {format} format.");
        }

        return result;
    }

    public IEnumerable<string> GetAvailableFormats()
    {
        return _writerFactory.GetAvailableFormats();
    }
}
```

## Advanced Scrutor Features

### 1. Filtering Classes

You can filter which classes to register based on specific criteria:

```csharp
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IWriter>()
    .AddClasses(classes => classes
        .Where(type => type.Name.EndsWith("Service") || 
                      type.Name.EndsWith("Writer") ||
                      type.Name.EndsWith("Notifier")))
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);
```

### 2. Registering by Base Class

Register classes that inherit from a specific base class:

```csharp
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IWriter>()
    .AddClasses()
    .AssignableTo<BaseService>()
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);
```

### 3. Multiple Assembly Scanning

Scan multiple assemblies at once:

```csharp
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IWriter>()
    .FromAssemblyOf<SomeOtherService>()
    .AddClasses()
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);
```

### 4. Conditional Registration

Register services conditionally based on environment or configuration:

```csharp
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IWriter>()
    .AddClasses()
    .AsImplementedInterfaces()
    .WithLifetime(ServiceLifetime.Scoped)
    .Where((service, descriptor) => 
        service.Name.Contains("Production") || 
        !service.Name.Contains("Development"))
);
```

## Controller Usage

With Scrutor handling all the DI registration and the factory pattern resolving multiple implementations, your controllers become clean and focused:

```csharp
[ApiController]
[Route("[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportGenerator _reportGenerator;
    private readonly IReportArchiver _reportArchiver;

    public ReportController(IReportGenerator reportGenerator, IReportArchiver reportArchiver)
    {
        _reportGenerator = reportGenerator;
        _reportArchiver = reportArchiver;
    }

    [HttpGet("generate/")]
    public IActionResult Get()
    {
        string reportName = "Monthly_Sales";
        var generationResult = _reportGenerator.GenerateReport(reportName);
        var archiveResult = _reportArchiver.Archive(reportName);

        var finalResult = new StringBuilder();
        finalResult.AppendLine(generationResult);
        finalResult.AppendLine(archiveResult);

        return Ok(finalResult.ToString());
    }

    [HttpGet("generate/{format}")]
    public IActionResult GetWithFormat(string format)
    {
        try
        {
            string reportName = "Monthly_Sales";
            var generationResult = _reportGenerator.GenerateReport(reportName, format);
            var archiveResult = _reportArchiver.Archive(reportName);

            var finalResult = new StringBuilder();
            finalResult.AppendLine(generationResult);
            finalResult.AppendLine(archiveResult);

            return Ok(finalResult.ToString());
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("formats")]
    public IActionResult GetAvailableFormats()
    {
        var formats = _reportGenerator.GetAvailableFormats();
        return Ok(new { AvailableFormats = formats });
    }
}
```

The DI container automatically resolves the entire dependency tree:
- `ReportController` needs `IReportGenerator` and `IReportArchiver`
- `ReportGenerator` needs `IWriterFactory` and `IEnumerable<INotifier>`
- `WriterFactory` can create specific writer implementations as needed
- All dependencies are automatically resolved and injected

## Benefits of Using Scrutor

### 1. **Reduced Boilerplate**
- No more repetitive `.AddScoped<TInterface, TImplementation>()` calls
- Clean, maintainable startup code

### 2. **Automatic Discovery**
- New services are automatically picked up when added to the assembly
- No need to remember to register new services

### 3. **Consistent Registration**
- All services follow the same registration pattern
- Enforces consistent architecture and naming conventions

### 4. **Better Maintainability**
- Centralized service registration logic
- Easy to modify registration behavior globally

### 5. **Reduced Errors**
- Eliminates manual registration mistakes
- Consistent lifetime management across services

### 6. **Handles Multiple Implementations**
- Factory pattern elegantly resolves multiple implementations
- Named services provide alternative approach
- No more DI container confusion

## Best Practices

### 1. **Naming Conventions**
Follow consistent naming patterns for interfaces and implementations:
- `IFoo` → `Foo`
- `IFooService` → `FooService`
- `IFooRepository` → `FooRepository`

### 2. **Assembly Organization**
Group related services in the same assembly to make scanning more targeted and efficient.

### 3. **Lifetime Management**
Be consistent with service lifetimes. Use Scrutor's lifetime methods to set appropriate defaults.

### 4. **Testing**
Scrutor works seamlessly with testing frameworks. You can still mock interfaces and test your services independently.

### 5. **Multiple Implementations**
- Use factory pattern for runtime selection of implementations
- Use named services for configuration-based selection
- Document which approach your project uses

## Common Pitfalls and Solutions

### 1. **Multiple Implementations of Same Interface** ✅ SOLVED
Our project demonstrates the solution using a factory pattern:

```csharp
// The factory pattern handles multiple implementations elegantly
public class ReportGenerator
{
    private readonly IWriterFactory _writerFactory;
    
    public string GenerateReport(string reportName, string format)
    {
        var writer = _writerFactory.CreateWriter(format);
        return writer.Write(reportName);
    }
}
```

### 2. **Circular Dependencies**
Scrutor won't solve circular dependency issues. Ensure your service design avoids circular references.

### 3. **Performance Considerations**
Assembly scanning happens at startup, so the performance impact is minimal. However, avoid scanning very large assemblies unnecessarily.

### 4. **Service Resolution Order**
When multiple implementations exist, be explicit about which one to use through factories or named services.

## Migration Strategy

If you're migrating from manual registration to Scrutor:

1. **Start Small**: Begin with one assembly or service category
2. **Test Thoroughly**: Ensure all services are properly registered and resolved
3. **Gradual Migration**: Move services incrementally to avoid breaking changes
4. **Monitor Performance**: Verify that startup time remains acceptable
5. **Handle Multiple Implementations**: Implement factory pattern or named services early

## Conclusion

Scrutor significantly simplifies dependency injection in .NET applications by eliminating repetitive service registration code and providing automatic service discovery. By following naming conventions and organizing your code properly, you can achieve clean, maintainable DI configuration that automatically scales with your application.

The key benefits include:
- **Reduced boilerplate code**
- **Automatic service discovery**
- **Consistent registration patterns**
- **Better maintainability**
- **Fewer configuration errors**
- **Elegant handling of multiple implementations**

In our example project, Scrutor automatically registered 8 services with just 5 lines of configuration code, compared to 8 separate manual registration calls. The factory pattern elegantly handles multiple `IWriter` implementations, making the system flexible and maintainable.

Scrutor is particularly valuable in large enterprise applications where manual service registration becomes unwieldy and error-prone. It encourages good architectural practices and makes your codebase more maintainable in the long run.

## Resources

- [Scrutor GitHub Repository](https://github.com/khellang/Scrutor)
- [NuGet Package](https://www.nuget.org/packages/Scrutor)
- [Microsoft Dependency Injection Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)

---

*This article demonstrates Scrutor using a real .NET project that showcases automatic service registration, dependency resolution, clean architecture patterns, and practical solutions for handling multiple implementations of the same interface.*
