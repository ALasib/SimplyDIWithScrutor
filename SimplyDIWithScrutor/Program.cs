using SimplyDIWithScrutor.Writers;

var builder = WebApplication.CreateBuilder(args);

// Option 1: Use the factory pattern (recommended for multiple implementations)
builder.Services.Scan(scan => scan
    // Scan the assembly where our services are defined
    .FromAssemblyOf<IWriter>()
    // Find all public, non-abstract classes
    .AddClasses(publicOnly: true)
    // Register each class against the interface(s) it implements
    .AsImplementedInterfaces()
    // Also register each class as itself (e.g., ReportGenerator -> ReportGenerator)
    .AsSelf()
    // Set the lifetime for all registered services to "Scoped"
    .WithScopedLifetime()
);

// Option 2: Alternative approach using named services (uncomment if you prefer this approach)
/*
builder.Services.Scan(scan => scan
    .FromAssemblyOf<IWriter>()
    .AddClasses(publicOnly: true)
    .AsImplementedInterfaces()
    .AsSelf()
    .WithScopedLifetime()
    .Named(implementation => 
    {
        // Create meaningful names for multiple implementations
        return implementation.Name switch
        {
            "ExcelWriter" => "excel",
            "PdfWriter" => "pdf",
            _ => implementation.Name.ToLower()
        };
    })
);
*/

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
