using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        var connectionString = config.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connectionString));

        // services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("WriteConnection")));

        services.AddDbContext<ReadDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("ReadConnection")));
        
        services.AddScoped<IWriteDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IReadDbContext>(sp => sp.GetRequiredService<ReadDbContext>());

        services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<IExpenseService, ExpenseService>();
        
        services.AddScoped<CreateExpenseCommandHandler>();
        services.AddScoped<UpdateExpenseCommandHandler>();
        services.AddScoped<DeleteExpenseCommandHandler>();
        services.AddScoped<GetExpenseByIdQueryHandler>();
        services.AddScoped<GetExpensesQueryHandler>();
        services.AddScoped<GetExpenseSummaryQueryHandler>();
        services.AddScoped<AddReceiptImageToExpenseCommandHandler>();


        
        var cloudinarySection = config.GetSection("Cloudinary");

        var cloudinaryUrl = config["Cloudinary:CloudinaryUrl"];
        var cloudinary = new Cloudinary(cloudinaryUrl);
        cloudinary.Api.Secure = true;

        services.AddSingleton(cloudinary);
        services.AddScoped<IReceiptStorage, CloudinaryReceiptStorage>();
        return services;
    }
}