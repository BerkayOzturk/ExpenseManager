using ExpenseManager.Domain.Entities;
using ExpenseManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ExpenseManager.Infrastructure.Persistence;

public sealed class ExpenseManagerDbContext : IdentityDbContext<AppUser>
{
    public ExpenseManagerDbContext(DbContextOptions<ExpenseManagerDbContext> options) : base(options) { }

    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<UserSettings> UserSettings => Set<UserSettings>();
    public DbSet<RecurringExpense> RecurringExpenses => Set<RecurringExpense>();
    public DbSet<PasswordResetCode> PasswordResetCodes => Set<PasswordResetCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var dateOnlyConverter = new ValueConverter<DateOnly, string>(
            d => d.ToString("yyyy-MM-dd"),
            s => DateOnly.ParseExact(s, "yyyy-MM-dd"));

        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("Categories");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.SortOrder).IsRequired();
            b.HasIndex(x => new { x.UserId, x.Name }).IsUnique();
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc);
        });

        modelBuilder.Entity<Expense>(b =>
        {
            b.ToTable("Expenses");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();

            b.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(x => x.Currency).HasMaxLength(5).IsRequired();

            b.Property(x => x.OccurredOn)
                .HasConversion(dateOnlyConverter)
                .HasMaxLength(10)
                .IsRequired();

            b.Property(x => x.Description).HasMaxLength(500);

            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc);

            b.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.OccurredOn);
            b.HasIndex(x => x.CategoryId);
        });

        modelBuilder.Entity<Budget>(b =>
        {
            b.ToTable("Budgets");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(x => x.Currency).HasMaxLength(5).IsRequired();
            b.Property(x => x.Year).IsRequired();
            b.Property(x => x.Month);
            b.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<UserSettings>(b =>
        {
            b.ToTable("UserSettings");
            b.HasKey(x => x.UserId);
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.DefaultCurrency).HasMaxLength(5).IsRequired();
            b.Property(x => x.DateFormat).HasMaxLength(50).IsRequired();
            b.Property(x => x.Theme).HasMaxLength(20).IsRequired();
            b.Property(x => x.Language).HasMaxLength(10);
        });

        modelBuilder.Entity<RecurringExpense>(b =>
        {
            b.ToTable("RecurringExpenses");
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)").IsRequired();
            b.Property(x => x.Currency).HasMaxLength(5).IsRequired();
            b.Property(x => x.FirstOccurrenceOn).HasConversion(dateOnlyConverter).HasMaxLength(10).IsRequired();
            b.Property(x => x.Description).HasMaxLength(500);
            b.Property(x => x.EndOn).HasConversion(dateOnlyConverter).HasMaxLength(10);
            b.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            b.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<PasswordResetCode>(b =>
        {
            b.ToTable("PasswordResetCodes");
            b.HasKey(x => x.Email);
            b.Property(x => x.Email).HasMaxLength(256).IsRequired();
            b.Property(x => x.Code).HasMaxLength(10).IsRequired();
            b.Property(x => x.ResetToken).IsRequired();
            b.Property(x => x.ExpiresAtUtc).IsRequired();
        });
    }
}

