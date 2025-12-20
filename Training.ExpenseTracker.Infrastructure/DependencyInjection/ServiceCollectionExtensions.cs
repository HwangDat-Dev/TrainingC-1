using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Infrastructure.Persistence;
using Training.ExpenseTracker.Infrastructure.Security;
using Training.ExpenseTracker.Infrastructure.Storage;

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
        
        
        var cloudinarySection = config.GetSection("Cloudinary");

        var cloudinaryUrl = config["Cloudinary:CloudinaryUrl"];
        var cloudinary = new Cloudinary(cloudinaryUrl);
        cloudinary.Api.Secure = true;

        services.AddSingleton(cloudinary);
        services.AddScoped<IReceiptStorage, CloudinaryReceiptStorage>();
        return services;
    }
}