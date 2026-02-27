using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Currency;
using Howsee.Application.Interfaces.Invoices;
using Howsee.Application.Interfaces.Payments;
using Howsee.Application.Interfaces.Pricing;
using Howsee.Application.Interfaces.PropertyCategory;
using Howsee.Application.Interfaces.Properties;
using Howsee.Application.Interfaces.Subscriptions;
using Howsee.Application.Interfaces.Tours;
using Howsee.Application.Interfaces.Users;
using Howsee.Application.Services;

namespace Howsee.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IPricingPlanService, PricingPlanService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<ITourService, TourService>();
        services.AddScoped<IPropertyService, PropertyService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IPropertyCategoryService, PropertyCategoryService>();

        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
