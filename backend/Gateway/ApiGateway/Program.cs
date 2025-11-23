using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ========== SERVICES CONFIGURATION ==========

// Add YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
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

// Exception handler
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

// Request logging
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Routing {Method} {Path} to backend service", context.Request.Method, context.Request.Path);
    await next();
});

// Health checks
app.MapHealthChecks("/health");

// YARP Reverse Proxy
app.MapReverseProxy();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("API Gateway starting");
logger.LogInformation("Configured routes:");
var config = builder.Configuration.GetSection("ReverseProxy:Routes").GetChildren();
foreach (var route in config)
{
    logger.LogInformation("  - {RouteId}: {Pattern}", 
        route.Key, 
        route["Match:Path"]);
}

// ========== RUN APPLICATION ==========

app.Run();
