using DndRpgApi.Data;
using DndRpgApi.Services;
using DndRpgApi.Filters;
using DndRpgApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using System.Reflection;
using HealthChecks.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURATION =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// ===== SERVICES =====

// Database Context with Azure SQL optimizations + your existing setup
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Enable retry on failure for Azure SQL Database
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
        
        // Command timeout for large operations
        sqlOptions.CommandTimeout(60);
    });
    
    // Enable sensitive data logging in development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Identity services for user management (your existing config)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password requirements (you can adjust these)
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Repository Pattern Services (NEW)
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IMonsterRepository, MonsterRepository>();

// API Controllers with global exception handling (ENHANCED)
builder.Services.AddControllers(options =>
{
    // Global exception handling
    options.Filters.Add<GlobalExceptionFilter>();
});

// Enhanced Swagger Documentation (ENHANCED)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "D&D RPG API",
        Version = "v1",
        Description = "A comprehensive Dungeons & Dragons 5th Edition character management and combat API with D&D 5e API integration",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com",
            Url = new Uri("https://github.com/markloy/dnd-rpg-app")
        }
    });
    
    // Include XML comments for better documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// CORS for React frontend (your existing + enhancements)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow React dev server
            policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Production: Restrict to your actual frontend domain
            policy.WithOrigins("https://your-frontend-domain.com")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// HTTP Client for D&D 5e API (your existing - keeping it!)
builder.Services.AddHttpClient("DndApi", client =>
{
    client.BaseAddress = new Uri("https://www.dnd5eapi.co/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "DnD-RPG-API/1.0");
});

// Health Checks for Azure (SIMPLIFIED - removed EF Core specific checks)
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString)
    .AddDbContextCheck<ApplicationDbContext>()
    .AddUrlGroup(new Uri("https://www.dnd5eapi.co/api"), "dnd5e-api"); // Check D&D API too!

// Response caching (NEW)
builder.Services.AddResponseCaching();

// Output caching (NEW - replaces the problematic CacheOutput)
builder.Services.AddOutputCache();

// Request logging (NEW)
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPropertiesAndHeaders |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponsePropertiesAndHeaders;
});

var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// Request logging (development only)
if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
}

// Global exception handling (NEW)
app.UseExceptionHandler("/error");

// Swagger (available in all environments for this demo)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "D&D RPG API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
        c.DocumentTitle = "D&D RPG API Documentation";
    });
}

// Security headers (FIXED - use indexer instead of Add)
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    await next();
});

// Security middleware
app.UseHttpsRedirection();

// CORS must come before authentication
app.UseCors("AllowReactApp");

// Response caching (NEW)
app.UseResponseCaching();

// Output caching (NEW)
app.UseOutputCache();

// Authentication & Authorization
app.UseAuthentication(); // Who are you?
app.UseAuthorization();  // What can you do?

// Health checks (NEW)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Map controllers
app.MapControllers();

// ===== MINIMAL API ENDPOINTS (your existing + enhanced) =====

// Health check endpoint (your existing - FIXED caching)
app.MapGet("/api/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    Version = "1.0.0"
})
.WithName("HealthCheck")
.WithTags("System")
.CacheOutput(policy => policy.Expire(TimeSpan.FromMinutes(1))); // FIXED

// Test database connection (your existing - enhanced)
app.MapGet("/api/test-db", async (ApplicationDbContext context) =>
{
    try
    {
        // Try to connect to database and check if tables exist
        var canConnect = await context.Database.CanConnectAsync();
        var characterCount = await context.Characters.CountAsync();
        var monsterCount = await context.Monsters.CountAsync();
        
        return Results.Ok(new { 
            Status = "Database connection successful",
            Timestamp = DateTime.UtcNow,
            CharacterCount = characterCount,
            MonsterCount = monsterCount,
            DatabaseProvider = context.Database.ProviderName
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
})
.WithName("TestDatabase")
.WithTags("System");

// Test D&D 5e API connection (NEW - checks your external API)
app.MapGet("/api/test-dnd-api", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var httpClient = httpClientFactory.CreateClient("DndApi");
        var response = await httpClient.GetAsync("api/classes");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Results.Ok(new
            {
                Status = "D&D 5e API connection successful",
                Timestamp = DateTime.UtcNow,
                ResponseSize = content.Length,
                ApiUrl = "https://www.dnd5eapi.co/"
            });
        }
        else
        {
            return Results.Problem($"D&D 5e API returned {response.StatusCode}");
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"D&D 5e API connection failed: {ex.Message}");
    }
})
.WithName("TestDndApi")
.WithTags("System");

// ===== DATABASE INITIALIZATION (FIXED logger scope issues) =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var startupLogger = services.GetRequiredService<ILogger<Program>>(); // RENAMED to avoid conflict
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        startupLogger.LogInformation("Applying database migrations...");
        
        // Apply any pending migrations
        await context.Database.MigrateAsync();
        
        startupLogger.LogInformation("Database migrations applied successfully");
        
        // Seed initial data (NEW)
        await DatabaseSeeder.SeedAsync(context, startupLogger);
        
        startupLogger.LogInformation("Database seeding completed");
    }
    catch (Exception ex)
    {
        startupLogger.LogError(ex, "An error occurred while applying migrations or seeding the database");
        
        // In production, you might want to fail fast
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}

// ===== STARTUP MESSAGE (FIXED logger scope) =====
var appLogger = app.Services.GetRequiredService<ILogger<Program>>(); // RENAMED to avoid conflict
var environment = app.Environment.EnvironmentName;
var connectionSummary = connectionString.Contains("database.windows.net") ? "Azure SQL Database" : "Local SQL Server";

appLogger.LogInformation("🐉 D&D RPG API starting in {Environment} environment", environment);
appLogger.LogInformation("📊 Using {DatabaseType}", connectionSummary);
appLogger.LogInformation("🎲 D&D 5e API integration enabled at https://www.dnd5eapi.co/");
appLogger.LogInformation("🚀 Application ready!");

// ===== START THE APPLICATION =====
app.Run();

// Make Program class accessible for testing
public partial class Program { }