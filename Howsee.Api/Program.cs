using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Howsee.Api.Json;
using Howsee.Application;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Sinks;
using Howsee.Application.Common;
using Howsee.Domain.Enums;
using Howsee.Infrastructure.Constants;
using Howsee.Infrastructure.Data;
using Howsee.Infrastructure.Data.Interceptors;
using Howsee.Infrastructure.Services;
using Howsee.Infrastructure.Services.Auth;
using Howsee.Infrastructure.Services.Sinks;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Payments;
using Howsee.Application.Interfaces.Tours;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableUtcDateTimeConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors?.Count > 0)
                .ToDictionary(
                    ms => ms.Key,
                    ms => ms.Value!.Errors.Select(e => e.ErrorMessage ?? "").ToArray());
            var response = ApiResponse<object>.ErrorResponse("Validation failed.", errors, code: ErrorCodes.ValidationFailed);
            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Howsee API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<AuditLogInterceptor>();
builder.Services.AddScoped<IAuditLogSink, EfCoreAuditLogSink>();
builder.Services.AddScoped<IAuditEventLogger, AuditEventLogger>();

builder.Services.AddDbContext<HowseeDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(sp.GetRequiredService<AuditLogInterceptor>());
});
builder.Services.AddScoped<IHowseeDbContext, HowseeDbContext>();
builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddApplicationServices();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddHttpClient<IOtpiqService, OtpiqService>();
builder.Services.AddHttpClient<IQiCardService, QiCardService>();

builder.Services.Configure<Howsee.Infrastructure.Services.MatterportApiOptions>(
    builder.Configuration.GetSection(Howsee.Infrastructure.Services.MatterportApiOptions.SectionName));
builder.Services.AddHttpClient<Howsee.Application.Interfaces.Tours.IMatterportApiClient, Howsee.Infrastructure.Services.MatterportApiClient>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(UserRole.Administrator.ToStringValue(), policy =>
        policy.RequireClaim(ClaimTypes.Role, UserRole.Administrator.ToStringValue()));
    options.AddPolicy(UserRole.Contractor.ToStringValue(), policy =>
        policy.RequireClaim(ClaimTypes.Role, UserRole.Contractor.ToStringValue()));
    options.AddPolicy(UserRole.Agency.ToStringValue(), policy =>
        policy.RequireClaim(ClaimTypes.Role, UserRole.Agency.ToStringValue()));
    options.AddPolicy(UserRole.Buyer.ToStringValue(), policy =>
        policy.RequireClaim(ClaimTypes.Role, UserRole.Buyer.ToStringValue()));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = AuthConstants.JwtScheme;
    options.DefaultChallengeScheme = AuthConstants.JwtScheme;
    options.DefaultScheme = AuthConstants.JwtScheme;
})
.AddJwtBearer(AuthConstants.JwtScheme, options =>
{
    var secret = builder.Configuration["JwtSettings:Secret"];
    if (string.IsNullOrEmpty(secret))
        throw new InvalidOperationException("JwtSettings:Secret is not set in configuration.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("tour-public", config =>
    {
        config.Window = TimeSpan.FromMinutes(1);
        config.PermitLimit = 60;
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    try
    {
        await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database seeding failed.");
    }
}

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI(o => o.DisplayRequestDuration());

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler(_ => { });

app.MapControllers();

app.Run();

public partial class Program { }
