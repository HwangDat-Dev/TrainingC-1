using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Infrastructure.Persistence;

public class AppDbContext : DbContext, IWriteDbContext, IReadDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Expense> Expenses => Set<Expense>();

    IQueryable<User> IReadDbContext.Users => Set<User>().AsNoTracking();
    IQueryable<Expense> IReadDbContext.Expenses => Set<Expense>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id");

            e.Property(x => x.Username)
                .HasColumnName("username")
                .HasMaxLength(50)
                .IsRequired();

            e.HasIndex(x => x.Username).IsUnique();

            e.Property(x => x.PasswordHash)
                .HasColumnName("password_hash")
                .IsRequired();

            e.Property(x => x.Role)
                .HasColumnName("role")
                .HasMaxLength(20)
                .HasDefaultValue("User")
                .IsRequired();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            e.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired(false);
        });

        modelBuilder.Entity<Expense>(e =>
        {
            e.ToTable("expenses");

            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnName("id");

            e.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            e.Property(x => x.Amount)
                .HasColumnName("amount")
                .HasPrecision(18, 2)
                .IsRequired();

            e.Property(x => x.Category)
                .HasColumnName("category")
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.Note)
                .HasColumnName("note")
                .HasMaxLength(255);

            e.Property(x => x.SpendDate)
                .HasColumnName("spend_date")
                .IsRequired();

            e.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            e.Property(x => x.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired(false);

            e.HasOne(x => x.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            e.Property(x => x.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(1000);

            e.Property(x => x.ImageUrlId)
                .HasColumnName("image_url_id")
                .HasMaxLength(255);
        });
    }
}
