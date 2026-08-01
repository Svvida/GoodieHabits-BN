using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
    {
        public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
        {
            builder.ToTable("RecurringTransactions");
            builder.HasKey(r => r.Id);

            // The materialization sweep filters on exactly these two columns, across all users.
            builder.HasIndex(r => new { r.IsActive, r.LastMaterializedOn });
            builder.HasIndex(r => r.UserProfileId);

            builder.Property(r => r.Type)
                .IsRequired();

            builder.Property(r => r.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(r => r.DayOfMonth)
                .IsRequired();

            builder.Property(r => r.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(r => r.LastMaterializedOn)
                .HasColumnType("date");

            builder.Property(r => r.Note)
                .HasMaxLength(FinanceTransaction.NoteMaxLength);

            builder.HasOne(r => r.UserProfile)
                .WithMany(u => u.RecurringTransactions)
                .HasForeignKey(r => r.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
