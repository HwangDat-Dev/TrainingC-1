using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Infrastructure.Persistence;

public class ReadDbContext : DbContext, IReadDbContext
{
    public ReadDbContext(DbContextOptions<ReadDbContext> options) : base(options) { }

    public DbSet<Expense> ExpensesSet => Set<Expense>();
    public DbSet<User> UsersSet => Set<User>();

    public IQueryable<Expense> Expenses => ExpensesSet.AsNoTracking();
    public IQueryable<User> Users => UsersSet.AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Username).HasColumnName("username");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.Role).HasColumnName("role");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.ToTable("expenses");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Amount).HasColumnName("amount");
            e.Property(x => x.Category).HasColumnName("category");
            e.Property(x => x.Note).HasColumnName("note");
            e.Property(x => x.SpendDate).HasColumnName("spend_date");
            e.Property(x => x.CreatedAt).HasColumnName("created_at");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            e.Property(x => x.ImageUrl).HasColumnName("image_url");
            e.Property(x => x.ImageUrlId).HasColumnName("image_url_id");
        });
    }
}