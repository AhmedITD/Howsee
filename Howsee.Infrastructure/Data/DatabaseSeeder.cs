using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Auth;
using Howsee.Domain.Entities;
using Howsee.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Howsee.Infrastructure.Data;

public class DatabaseSeeder
{
    public const string SeedSectionName = "Seed";

    private readonly IHowseeDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IHowseeDbContext db,
        IPasswordService passwordService,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _db = db;
        _passwordService = passwordService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedCurrenciesAsync(cancellationToken);
        await SeedPricingPlansAsync(cancellationToken);
        await SeedAdminUserAsync(cancellationToken);
    }

    private async Task SeedCurrenciesAsync(CancellationToken cancellationToken)
    {
        var iqd = await _db.Currencies.FirstOrDefaultAsync(c => c.Code == "IQD", cancellationToken);
        if (iqd != null) return;

        _db.Currencies.Add(new Currency
        {
            Code = "IQD",
            Name = "Iraqi Dinar",
            Symbol = "ع.د",
            IsActive = true
        });
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded currency: IQD");
    }

    private async Task SeedPricingPlansAsync(CancellationToken cancellationToken)
    {
        var iqd = await _db.Currencies.FirstOrDefaultAsync(c => c.Code == "IQD", cancellationToken);
        if (iqd == null)
        {
            _logger.LogWarning("Currency IQD not found; skipping pricing plan seed.");
            return;
        }

        var planDefaults = new (string Key, string Name, decimal Amount, string Unit, UserRole? Role)[]
        {
            ("Price_Basic", "Basic", 50_000m, "month", UserRole.Buyer),
            ("Price_Pro", "Pro", 95_000m, "month", UserRole.Contractor),
            ("Price_Enterprise", "Enterprise", 250_000m, "month", UserRole.Agency),
            ("Price_PerTour", "Per Tour", 15_000m, "tour", null)
        };

        for (var i = 0; i < planDefaults.Length; i++)
        {
            var (key, name, amount, unit, role) = planDefaults[i];
            var existing = await _db.PricingPlans.FirstOrDefaultAsync(p => p.Key == key, cancellationToken);
            if (existing != null)
            {
                if (existing.Role != role)
                {
                    existing.Role = role;
                    _logger.LogInformation("Updated pricing plan role: {Key} -> {Role}", key, role);
                }
                continue;
            }

            _db.PricingPlans.Add(new PricingPlan
            {
                Key = key,
                Name = name,
                Amount = amount,
                CurrencyId = iqd.Id,
                Unit = unit,
                Role = role,
                IsActive = true,
                SortOrder = i + 1
            });
            _logger.LogInformation("Seeded pricing plan: {Key}", key);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminUserAsync(CancellationToken cancellationToken)
    {
        var section = _configuration.GetSection(SeedSectionName);
        var phone = section["AdminPhone"]?.Trim();
        var password = section["AdminPassword"];
        var fullName = section["AdminFullName"]?.Trim() ?? "Administrator";

        if (string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(password))
            return;

        var exists = await _db.Users.AnyAsync(u => u.Phone == phone, cancellationToken);
        if (exists)
            return;

        var user = new User
        {
            FullName = fullName,
            Phone = phone,
            PasswordHash = _passwordService.HashPassword(password),
            Role = UserRole.Administrator
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded admin user: {Phone}", phone);
    }
}
