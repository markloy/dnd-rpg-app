using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DndRpgApi.Data;
using DndRpgApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURE SERVICES =====
// Services are registered here for dependency injection

// Add Entity Framework and SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Get connection string from appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
    
    // Enable sensitive data logging in development (shows SQL parameters)
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// Add Identity services for user management
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

// Add API services
builder.Services.AddControllers();

// Add API documentation (Swagger)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "D&D RPG API", 
        Version = "v1",
        Description = "A D&D 5e character management and combat API"
    });
});

// Add CORS for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // React development server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add HTTP client for D&D API calls
builder.Services.AddHttpClient("DndApi", client =>
{
    client.BaseAddress = new Uri("https://www.dnd5eapi.co/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ===== BUILD THE APPLICATION =====
var app = builder.Build();

// ===== CONFIGURE MIDDLEWARE PIPELINE =====
// Order matters! Each middleware processes requests in order

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "D&D RPG API v1");
        c.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

// Security middleware
app.UseHttpsRedirection();

// CORS must come before authentication
app.UseCors("AllowReactApp");

// Authentication & Authorization
app.UseAuthentication(); // Who are you?
app.UseAuthorization();  // What can you do?

// Map controllers (traditional approach)
app.MapControllers();

// ===== MINIMAL API ENDPOINTS =====
// Modern .NET approach - simpler than controllers for basic operations

// Health check endpoint
app.MapGet("/api/health", () => new { 
    Status = "Healthy", 
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName
})
.WithName("HealthCheck")
.WithTags("System");

// Test database connection
app.MapGet("/api/test-db", async (ApplicationDbContext context) =>
{
    try
    {
        // Try to connect to database
        await context.Database.CanConnectAsync();
        return Results.Ok(new { 
            Status = "Database connection successful",
            Timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
})
.WithName("TestDatabase")
.WithTags("System");

// ===== DATABASE INITIALIZATION =====
// Ensure database is created and up to date
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Apply any pending migrations
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database");
        throw; // Re-throw to prevent app from starting with bad database
    }
}

// ===== START THE APPLICATION =====
app.Run();

// Make Program class accessible for testing
public partial class Program { }