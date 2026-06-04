using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using sabmero.Data;
using sabmero.Services;


var builder = WebApplication.CreateBuilder(args);

// ═══════════════════════════════════════════════════════════════════════════════
// 1. DATABASE — PostgreSQL
//    Railway gives you a DATABASE_URL environment variable automatically.
//    We read it here. If not on Railway, falls back to appsettings.json.
// ═══════════════════════════════════════════════════════════════════════════════
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    // ── Railway / Production ─────────────────────────────────────────────────
    // Railway provides DATABASE_URL in this format:
    // postgresql://username:password@host:port/database
    // We convert it to the format Npgsql (the PostgreSQL driver) expects.
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    // ── Local development ────────────────────────────────────────────────────
    // Falls back to appsettings.json
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ═══════════════════════════════════════════════════════════════════════════════
// 2. JWT AUTHENTICATION
//    Reads secret key from appsettings.json (local) or Railway env vars.
// ═══════════════════════════════════════════════════════════════════════════════
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                  ?? builder.Configuration["JwtSettings:SecretKey"]!;
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]!;
var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// ═══════════════════════════════════════════════════════════════════════════════
// 3. REGISTER SERVICES (Dependency Injection)
//    When AuthController needs IAuthService, ASP.NET gives it AuthService.
//    When AuthService needs IJwtService, ASP.NET gives it JwtService.
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// ═══════════════════════════════════════════════════════════════════════════════
// 4. CONTROLLERS
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddControllers();

// ═══════════════════════════════════════════════════════════════════════════════
// 5. CORS — Allow Flutter mobile app to call this API from any origin
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// ═══════════════════════════════════════════════════════════════════════════════
// 6. SWAGGER — Interactive API docs at  /swagger
//    App developers open this in browser to see and test all endpoints.
// ═══════════════════════════════════════════════════════════════════════════════
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "sabmero API",
        Version = "v1",
        Description = "Backend API for sabmero Multi-Vendor E-Commerce & On-Site Service App"
    });

    // This adds a "Authorize" button in Swagger so developers can test protected endpoints
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste your JWT token here. Format:  Bearer eyJhbGci...",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        },
        Array.Empty<string>()
    }});
});

// ═══════════════════════════════════════════════════════════════════════════════
// BUILD
// ═══════════════════════════════════════════════════════════════════════════════
var app = builder.Build();

// ═══════════════════════════════════════════════════════════════════════════════
// 7. AUTO-MIGRATE DATABASE ON STARTUP
//    This is the KEY feature for Railway:
//    When the app starts on Railway, it automatically:
//    - Creates the "SabmeroDb" database if it doesn't exist
//    - Runs all pending migrations (creates all 11 tables)
//    - Inserts seed data (admin user + 4 categories)
//    NO MANUAL COMMANDS NEEDED ON THE SERVER.
// ═══════════════════════════════════════════════════════════════════════════════
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();   // ← creates tables + seeds data automatically
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations.");
        throw;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// 8. MIDDLEWARE PIPELINE
//    Order matters! Each request passes through these layers top to bottom.
// ═══════════════════════════════════════════════════════════════════════════════

// Always show Swagger (useful even in production for your app developer)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "sabmero API v1");
    c.RoutePrefix = "swagger";   // Swagger at: https://your-app.railway.app/swagger
});

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();    // ← must be BEFORE UseAuthorization
app.UseAuthorization();
app.MapControllers();

// Simple root URL response so Railway health checks pass
app.MapGet("/", () => Results.Ok(new
{
    app = "sabmero Backend API",
    status = "running",
    swagger = "/swagger"
}));

app.Run();