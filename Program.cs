using DotNetEnv;
using Sonic.Models;
using Sonic.API.Data;
using Sonic.API.Services;
using Sonic.API.Mapping;
using Sonic.API.Controllers;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog; // Add Serilog
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Mapster;

// Configure Serilog for file logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file (only in development)
if (builder.Environment.IsDevelopment())
{
    Env.Load();
}

// Add Mapster mapping configuration
MappingProfile.Register();

// Replace default logging with Serilog
builder.Host.UseSerilog();

// Add services to the container
// Add OpenAPI/Swagger support
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.WithOrigins(
                "http://localhost:4173", 
                "http://localhost:5153", 
                "https://localhost:7153",
                "https://sonic-api-*-uc.a.run.app" // Cloud Run URL pattern
                ) 
                   .AllowAnyMethod()
                   .AllowCredentials()
                   .AllowAnyHeader();
        });
});

// Helper method to get environment variables (works with both .env and Cloud Run)
static string GetEnvironmentVariable(string key)
{
    return Environment.GetEnvironmentVariable(key) ?? Env.GetString(key) ?? "";
}

// Helper method to get Cloud SQL connection string with proper format for Cloud Run
static string GetCloudSqlConnectionString()
{
    var connectionString = GetEnvironmentVariable("SONIC_AUTH_DB_CONNECTION_STRING");
    
    // If running in Cloud Run (production), ensure we use Unix socket format
    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
    {
        // Check if connection string already uses Unix socket format
        if (!connectionString.Contains("/cloudsql/"))
        {
            // Convert TCP format to Unix socket format for Cloud SQL
            // Expected format: Host=/cloudsql/PROJECT_ID:REGION:INSTANCE_NAME;Database=...;Username=...;Password=...
            var cloudSqlInstance = "sonic-467415:us-central1:sonic-postgre";
            
            // Extract database, username, password from existing connection string
            var parts = connectionString.Split(';');
            var dbName = "sonic";
            var username = "postgres";
            var password = "";
            
            foreach (var part in parts)
            {
                if (part.StartsWith("Database=")) dbName = part.Split('=')[1];
                if (part.StartsWith("Username=") || part.StartsWith("User ID=")) username = part.Split('=')[1];
                if (part.StartsWith("Password=")) password = part.Split('=')[1];
            }
            
            connectionString = $"Host=/cloudsql/{cloudSqlInstance};Database={dbName};Username={username};Password={password}";
            Log.Information("Converted connection string to Cloud SQL Unix socket format");
        }
    }
    
    return connectionString;
}

// Register KCRMDbContext with PostgreSQL provider using Cloud SQL connection string
builder.Services.AddDbContext<SonicDbContext>(options =>
{
    options.UseNpgsql(GetCloudSqlConnectionString());
});
// Register IAuthService with AuthService implementation
builder.Services.AddScoped<IAuthService, AuthService>();
// Register IUserService with UserService implementation
builder.Services.AddScoped<IUserService, UserService>();
// Register ISpotifyService with SpotifyService implementation
builder.Services.AddScoped<ISpotifyService, SpotifyService>();
// Register IMapsService with MapsService implementation
builder.Services.AddScoped<IMapsService, MapsService>();

// Register EntityService with specific type mappings
builder.Services.AddScoped<IEntityService<VenueDto, VenueCreateDto, Venue>>(provider =>
    new EntityService<VenueDto, VenueCreateDto, Venue>(provider.GetRequiredService<SonicDbContext>()));

builder.Services.AddScoped<IEntityService<EventDto, EventCreateDto, Event>>(provider =>
    new EntityService<EventDto, EventCreateDto, Event>(provider.GetRequiredService<SonicDbContext>()));

// Replace the generic registration for Song
// builder.Services.AddScoped<IGenericEntityService<SongDto, SongCreateDto>, GenericEntityService<Song, SongDto, SongCreateDto, SongUpdateDto>>();

// With the specific service
builder.Services.AddScoped<IEntityService<SongDto, SongCreateDto, Song>, EntityService<SongDto, SongCreateDto, Song>>();

// Register IEntityService for Instrument
builder.Services.AddScoped<IEntityService<InstrumentDto, InstrumentCreateDto, Instrument>>(provider =>
    new EntityService<InstrumentDto, InstrumentCreateDto, Instrument>(provider.GetRequiredService<SonicDbContext>()));

// Add HttpClient for SpotifyService if it needs to make external API calls
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>(client =>
{
    client.BaseAddress = new Uri(GetEnvironmentVariable("SPOTIFY_TOKEN_URL"));
});
// Add HttpClient for MapsHttpService
builder.Services.AddHttpClient<IMapsHttpService, MapsHttpService>(client =>
{
    client.BaseAddress = new Uri("https://places.googleapis.com/");
});

// Add authentication and authorization services
builder.Services.AddAuthentication("Bearer")
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = GetEnvironmentVariable("GOOGLEAUTH_CLIENT_ID");
        options.ClientSecret = GetEnvironmentVariable("GOOGLEAUTH_CLIENT_SECRET");
        options.SaveTokens = true; // Save tokens for later use
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Configure your token validation parameters here
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = GetEnvironmentVariable("ISSUER"),
            ValidAudience = GetEnvironmentVariable("AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(GetEnvironmentVariable("TOKEN")))
        };
    });

builder.Services.AddAuthorization(//options =>
                                  //{
                                  //options.AddPolicy("ApiScope", policy =>
                                  //    policy.RequireClaim("scope", "api"));
                                  //}
);

// In Program.cs after builder.Services configuration
TypeAdapterConfig.GlobalSettings.Default
    .PreserveReference(true)
    .MaxDepth(2);


var app = builder.Build();

// Apply database migrations on startup (for production deployment)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SonicDbContext>();
        
        Log.Information("Applying database migrations...");
        context.Database.Migrate();
        Log.Information("Database migrations applied successfully.");
    }
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while applying database migrations.");
    // Don't throw - let the app start even if migrations fail (for debugging)
}

// Make sure this comes BEFORE your endpoints
app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapGoogleAuthEndpoints();
app.MapSpotifyEndpoints();
app.MapEntityEndpoints<VenueDto, VenueCreateDto, Venue>(Role.Player, Role.Manager);
app.MapEntityEndpoints<EventDto, EventCreateDto, Event>(Role.Player, Role.Manager);
app.MapEntityEndpoints<SongDto, SongCreateDto, Song>(Role.Player, Role.Player);
app.MapEntityEndpoints<InstrumentDto, InstrumentCreateDto, Instrument>(Role.Player, Role.Manager);
app.MapMapsEndpoints();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Only use HTTPS redirection when HTTPS is configured
if (!app.Environment.IsProduction() || 
    app.Configuration["ASPNETCORE_URLS"]?.Contains("https") == true)
{
    app.UseHttpsRedirection();
}

// ✅ Then CORS
app.UseCors("AllowAllOrigins");

// Enable authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Start the application
app.Run();

