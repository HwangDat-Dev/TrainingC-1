using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Application.Interfaces;

public interface IReadDbContext
{
    IQueryable<Expense> Expenses { get; }
    IQueryable<User> Users { get; }
}