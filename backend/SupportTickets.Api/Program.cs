using System.Threading.RateLimiting;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SupportTickets.Api.Api.Constants;
using SupportTickets.Api.Api.Middleware;
using SupportTickets.Api.Api.Validation;
using SupportTickets.Api.Application;
using SupportTickets.Api.Application.Contracts;
using SupportTickets.Api.Infrastructure;
using SupportTickets.Api.Infrastructure.Authentication;
using SupportTickets.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var jwtSigningKey = builder.Configuration["Authentication:Jwt:SigningKey"];
if (string.IsNullOrWhiteSpace(jwtSigningKey))
{
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
        throw new InvalidOperationException("Configure Authentication:Jwt:SigningKey.");
    jwtSigningKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
}

if (Encoding.UTF8.GetByteCount(jwtSigningKey) < 32)
    throw new InvalidOperationException("Configure Authentication:Jwt:SigningKey with at least 32 bytes.");

var jwtIssuer = builder.Configuration["Authentication:Jwt:Issuer"] ?? "SupportTickets.Api";
var jwtAudience = builder.Configuration["Authentication:Jwt:Audience"] ?? "SupportTickets.Client";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSigningKey));

if (builder.Environment.IsProduction() &&
    (string.IsNullOrWhiteSpace(builder.Configuration["AllowedHosts"]) ||
     builder.Configuration["AllowedHosts"] == "*"))
{
    throw new InvalidOperationException("Configure AllowedHosts for production.");
}

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
});

builder.Services.AddScoped<RequestValidationFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<RequestValidationFilter>());
builder.Services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
builder.Services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(30));
builder.Services.AddSwaggerGen();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
            ValidateLifetime = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAccessTokenIssuer>(
    new JwtAccessTokenIssuer(signingKey, jwtIssuer, jwtAudience, TimeProvider.System));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    if (builder.Environment.IsProduction() && allowedOrigins.Length == 0)
        throw new InvalidOperationException("Configure Cors:AllowedOrigins for production.");

    options.AddDefaultPolicy(policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var permitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 100);
var windowSeconds = builder.Configuration.GetValue("RateLimiting:WindowSeconds", 60);
var authPermitLimit = builder.Configuration.GetValue("RateLimiting:AuthPermitLimit", 10);
if (permitLimit < 1 || windowSeconds < 1 || authPermitLimit < 1)
    throw new InvalidOperationException("Rate limiting values must be positive.");

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("tickets-api", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy("auth-api", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authPermitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too many requests",
            Detail = ApiMessages.RateLimitExceeded,
            Type = "https://httpstatuses.com/429"
        };
        problem.Extensions["code"] = ApiErrorCodes.RateLimitExceeded;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    };
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TicketDbContext>();
    if (dbContext.Database.IsRelational())
        await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<ApiExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
if (app.Environment.IsProduction())
    app.UseHsts();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.Run();

public partial class Program;
