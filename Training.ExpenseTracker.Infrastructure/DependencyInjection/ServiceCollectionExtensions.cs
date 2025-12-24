using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Training.ExpenseTracker.Application.Features.Auth.Commands.Register;
using Training.ExpenseTracker.Application.Features.Auth.Query.GetUserInfo;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.CreateExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.DeleteExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.UpdateExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Commands.UploadImageExpense;
using Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseById;
using Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenses;
using Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseSummary;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Infrastructure.Persistence;
using Training.ExpenseTracker.Infrastructure.Security;
using Training.ExpenseTracker.Infrastructure.Storage;

namespace Training.ExpenseTracker.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var writeConnection = config.GetConnectionString("WriteConnection") 
                              ?? config.GetConnectionString("DefaultConnection");
        var readConnection = config.GetConnectionString("ReadConnection") 
                             ?? writeConnection;

        if (string.IsNullOrWhiteSpace(writeConnection))
            throw new InvalidOperationException("Connection string 'WriteConnection' or 'DefaultConnection' not found.");

        Console.WriteLine($"[DB] WriteConnection configured: {ExtractHost(writeConnection)}");
        Console.WriteLine($"[DB] ReadConnection configured: {ExtractHost(readConnection)}");

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(writeConnection));

        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseNpgsql(readConnection));

        services.AddScoped<IWriteDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<ReadDbContext>());

        services.AddScoped<RegisterCommandHandler>();
        services.AddScoped<LoginCommandHandler>();
        services.AddScoped<GetUserInfoQueryHandler>();

        services.AddScoped<CreateExpenseCommandHandler>();
        services.AddScoped<UpdateExpenseCommandHandler>();
        services.AddScoped<DeleteExpenseCommandHandler>();
        services.AddScoped<GetExpenseByIdQueryHandler>();
        services.AddScoped<GetExpensesQueryHandler>();
        services.AddScoped<GetExpenseSummaryQueryHandler>();
        services.AddScoped<AddReceiptImageToExpenseCommandHandler>();

        var cloudinaryUrl = config["Cloudinary:CloudinaryUrl"];
        var cloudinary = new Cloudinary(cloudinaryUrl);
        cloudinary.Api.Secure = true;

        services.AddSingleton(cloudinary);
        services.AddScoped<IReceiptStorage, CloudinaryReceiptStorage>();
        return services;
    }

    private static string ExtractHost(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "N/A";

        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length == 2 && keyValue[0].Trim().Equals("Host", StringComparison.OrdinalIgnoreCase))
            {
                return keyValue[1].Trim();
            }
        }
        return "unknown";
    }
}