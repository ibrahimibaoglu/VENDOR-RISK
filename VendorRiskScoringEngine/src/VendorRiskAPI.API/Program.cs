using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using VendorRiskAPI.API.Middleware;
using VendorRiskAPI.Application.Extensions;
using VendorRiskAPI.Infrastructure.Extensions;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "VendorRiskScoringEngine")
    .CreateLogger();

try
{
    Log.Information("Starting VendorRiskScoringEngine API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend",
            policy =>
            {
                policy.WithOrigins("http://localhost:3000") // Frontend URL
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .WithExposedHeaders("X-Total-Count", "X-Page", "X-Page-Size");
            });
    });

    builder.Services.AddControllers();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck<VendorRiskAPI.API.HealthChecks.DatabaseHealthCheck>("database")
        .AddCheck<VendorRiskAPI.API.HealthChecks.RedisHealthCheck>("redis");

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "Vendor Risk Scoring API",
            Version = "v1",
            Description = @"A comprehensive rule-based vendor risk assessment system.

## Features
- **Risk Scoring**: Calculate financial, operational, and security compliance risks
- **CRUD Operations**: Manage vendor profiles
- **Risk Assessment**: Generate detailed risk assessments with explanations
- **Pagination**: Support for large datasets
- **Caching**: Redis-based caching for improved performance

## Risk Formula
Final Risk Score = (Financial × 0.4) + (Operational × 0.3) + (Security × 0.3)

## Risk Levels
- **Low**: 0.00 - 0.25
- **Medium**: 0.25 - 0.50
- **High**: 0.50 - 0.75
- **Critical**: 0.75 - 1.00",
            Contact = new()
            {
                Name = "Vendor Risk Team",
                Email = "info@vendorrisk.com",
                Url = new Uri("https://github.com/vendorrisk")
            },
            License = new()
            {
                Name = "MIT License",
                Url = new Uri("https://opensource.org/licenses/MIT")
            }
        });

        // Add XML comments if available
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // Add security definition (for future OAuth/JWT)
        c.AddSecurityDefinition("Bearer", new()
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        // Group endpoints by tags
        c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default" });
        c.DocInclusionPredicate((name, api) => true);
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // Add request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Vendor Risk Scoring API v1");
            c.RoutePrefix = string.Empty; // Swagger UI at root
        });
    }

    app.UseHttpsRedirection();
    
    app.UseCors("AllowFrontend");

    app.UseAuthorization();
    app.MapControllers();

    // Map health check endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

    // Seed database in development environment
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var seeder = scope.ServiceProvider.GetRequiredService<VendorRiskAPI.Infrastructure.Seeders.DatabaseSeeder>();
                await seeder.SeedAsync();
                Log.Information("Database seeding process completed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while seeding the database");
            }
        }
    }

    Log.Information("VendorRiskScoringEngine API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public so test projects can access it
public partial class Program { }
