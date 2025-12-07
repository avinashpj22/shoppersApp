using OrderService.API;
using Microsoft.AspNetCore.Diagnostics;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES CONFIGURATION ==========

// Add controllers
builder.Services.AddControllers();

// Add Order Service dependencies (custom extension method)
builder.Services.AddOrderServiceDependencies(builder.Configuration);

// Add API documentation
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "Order management service"
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Add CORS (for frontend and other services)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add logging
builder.Services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// ========== BUILD APPLICATION ==========

var app = builder.Build();

// ========== MIDDLEWARE CONFIGURATION ==========

// Use exception handler middleware
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>();
        
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception?.Error switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new
        {
            message = exception?.Error?.Message ?? "An unexpected error occurred",
            type = exception?.Error?.GetType().Name,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(response);
    });
});

// Swagger UI (development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
    });
    app.UseDeveloperExceptionPage();
}

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowFrontend");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health");

// Controllers
app.MapControllers();

// Initialize event subscriptions
// Note: Event subscriptions disabled for now - needs async disposal pattern
// var logger = app.Services.GetRequiredService<ILogger<Program>>();
// try
// {
//     await app.Services.InitializeEventSubscriptionsAsync(logger);
//     logger.LogInformation("Order Service initialized successfully");
// }
// catch (Exception ex)
// {
//     logger.LogError(ex, "Error during Order Service initialization");
// }

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Order Service starting on {Environment}", app.Environment.EnvironmentName);

// ========== RUN APPLICATION ==========

app.Run();
