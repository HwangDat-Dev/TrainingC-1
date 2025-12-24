using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Application.Interfaces;

public interface IReadDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<Expense> Expenses { get; }
}
