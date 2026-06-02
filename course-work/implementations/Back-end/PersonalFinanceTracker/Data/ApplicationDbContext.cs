using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Models.Entities;

namespace PersonalFinanceTracker.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FinancialRecord> FinancialRecords => Set<FinancialRecord>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(50).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(100).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.DefaultCurrency).HasConversion<string>().HasMaxLength(3).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(user => user.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasIndex(category => new { category.UserId, category.Name }).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(50).IsRequired();
            entity.Property(category => category.CategoryType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(category => category.Description).HasMaxLength(255);
            entity.Property(category => category.ColorHex).HasMaxLength(7);
            entity.Property(category => category.IconName).HasMaxLength(50);
            entity.Property(category => category.IsActive).HasDefaultValue(true);
            entity.HasOne(category => category.User)
                .WithMany(user => user.Categories)
                .HasForeignKey(category => category.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FinancialRecord>(entity =>
        {
            entity.ToTable("FinancialRecords");
            entity.Property(record => record.RecordType).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(record => record.Amount).HasPrecision(18, 2);
            entity.Property(record => record.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            entity.Property(record => record.Description).HasMaxLength(255);
            entity.Property(record => record.Note).HasMaxLength(1000);
            entity.HasOne(record => record.User)
                .WithMany(user => user.FinancialRecords)
                .HasForeignKey(record => record.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(record => record.Category)
                .WithMany(category => category.FinancialRecords)
                .HasForeignKey(record => record.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.HasIndex(subscription => new { subscription.UserId, subscription.Name });
            entity.HasIndex(subscription => subscription.NextDueDate);
            entity.Property(subscription => subscription.Name).HasMaxLength(100).IsRequired();
            entity.Property(subscription => subscription.Amount).HasPrecision(18, 2);
            entity.Property(subscription => subscription.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            entity.Property(subscription => subscription.Cycle).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(subscription => subscription.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(subscription => subscription.Description).HasMaxLength(255);
            entity.Property(subscription => subscription.IncludeInMonthlyForecast).HasDefaultValue(true);
            entity.HasOne(subscription => subscription.User)
                .WithMany(user => user.Subscriptions)
                .HasForeignKey(subscription => subscription.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(subscription => subscription.Category)
                .WithMany(category => category.Subscriptions)
                .HasForeignKey(subscription => subscription.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("Budgets");
            entity.HasIndex(budget => new { budget.UserId, budget.CategoryId, budget.Month, budget.Year }).IsUnique();
            entity.Property(budget => budget.LimitAmount).HasPrecision(18, 2);
            entity.Property(budget => budget.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            entity.Property(budget => budget.Description).HasMaxLength(255);
            entity.Property(budget => budget.IsActive).HasDefaultValue(true);
            entity.HasOne(budget => budget.User)
                .WithMany(user => user.Budgets)
                .HasForeignKey(budget => budget.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(budget => budget.Category)
                .WithMany(category => category.Budgets)
                .HasForeignKey(budget => budget.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SavingsGoal>(entity =>
        {
            entity.ToTable("SavingsGoals");
            entity.HasIndex(goal => new { goal.UserId, goal.Name });
            entity.Property(goal => goal.Name).HasMaxLength(100).IsRequired();
            entity.Property(goal => goal.TargetAmount).HasPrecision(18, 2);
            entity.Property(goal => goal.CurrentAmount).HasPrecision(18, 2);
            entity.Property(goal => goal.Currency).HasConversion<string>().HasMaxLength(3).IsRequired();
            entity.Property(goal => goal.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
            entity.Property(goal => goal.Description).HasMaxLength(255);
            entity.Property(goal => goal.Priority).HasDefaultValue(3);
            entity.HasOne(goal => goal.User)
                .WithMany(user => user.SavingsGoals)
                .HasForeignKey(goal => goal.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
