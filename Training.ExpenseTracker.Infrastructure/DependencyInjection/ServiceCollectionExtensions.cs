using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Infrastructure.Persistence;
using Training.ExpenseTracker.Infrastructure.Security;

namespace Training.ExpenseTracker.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        return services;
    }
}