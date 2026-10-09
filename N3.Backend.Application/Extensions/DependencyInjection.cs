using Microsoft.Extensions.DependencyInjection;
using N3.Backend.Application.Interfaces;
using N3.Backend.Application.Services;

namespace N3.Backend.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();

        return services;
    }
}
