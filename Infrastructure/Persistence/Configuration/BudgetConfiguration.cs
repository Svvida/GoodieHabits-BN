using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class BudgetConfiguration : IEntityTypeConfiguration<Budget>
    {
        public void Configure(EntityTypeBuilder<Budget> builder)
        {
            builder.ToTable("Budgets");
            builder.HasKey(b => b.Id);

            // Enforces one budget per category-scope/period at the DB level. NOTE: for overall budgets
            // (CategoryId NULL) SQL Server treats NULLs as distinct, so duplicate "overall" budgets are
            // additionally guarded in the application layer (IBudgetRepository.ExistsForPeriodAsync).
            builder.HasIndex(b => new { b.UserProfileId, b.CategoryId, b.Period, b.Year, b.Month })
                .IsUnique();

            builder.Property(b => b.Period)
                .IsRequired();

            builder.Property(b => b.Year)
                .IsRequired();

            builder.Property(b => b.LimitAmount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.HasOne(b => b.UserProfile)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(b => b.Category)
                .WithMany()
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
