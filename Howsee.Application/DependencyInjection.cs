using FluentValidation;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Invoices;
using Howsee.Application.Interfaces.Payments;
using Howsee.Application.Services;

namespace Howsee.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInvoiceService, InvoiceService>();

        TypeAdapterConfig.GlobalSettings.Scan(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
