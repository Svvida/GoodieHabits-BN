using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class FinanceTransactionConfiguration : IEntityTypeConfiguration<FinanceTransaction>
    {
        public void Configure(EntityTypeBuilder<FinanceTransaction> builder)
        {
            builder.ToTable("FinanceTransactions");
            builder.HasKey(t => t.Id);

            builder.HasIndex(t => new { t.UserProfileId, t.OccurredOn });
            builder.HasIndex(t => new { t.UserProfileId, t.CategoryId, t.OccurredOn });
            builder.HasIndex(t => t.CorrectsTransactionId);
            builder.HasIndex(t => t.RecurringTransactionId);

            // Computed from Amount and CorrectedAmount — never stored.
            builder.Ignore(t => t.NetAmount);
            builder.Ignore(t => t.IsCorrection);

            builder.Property(t => t.Type)
                .IsRequired();

            builder.Property(t => t.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(t => t.CorrectedAmount)
                .HasPrecision(18, 2)
                .HasDefaultValue(0m)
                .IsRequired();

            builder.Property(t => t.OccurredOn)
                .HasColumnType("date")
                .IsRequired();

            // Existing rows are things that already happened, so they backfill to paid via the column default.
            builder.Property(t => t.IsPaid)
                .HasDefaultValue(true)
                .IsRequired();

            builder.Property(t => t.Note)
                .HasMaxLength(FinanceTransaction.NoteMaxLength);

            builder.HasOne(t => t.UserProfile)
                .WithMany(u => u.FinanceTransactions)
                .HasForeignKey(t => t.UserProfileId)
                .OnDelete(DeleteBehavior.NoAction);

            // Category is optional (uncategorized allowed). Restrict blocks deleting a category still in use;
            // the application layer also guards this to return a friendly error.
            builder.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Provenance link to the template that generated this row. Restrict, with the delete handler
            // clearing the FK first: a materialized transaction is the user's own record and must outlive the
            // template it came from.
            builder.HasOne(t => t.RecurringTransaction)
                .WithMany(r => r.Transactions)
                .HasForeignKey(t => t.RecurringTransactionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-reference: a correction points at the transaction it corrects. Restrict, same posture as
            // FinanceCategory.ParentCategoryId — the application layer removes corrections first and returns a
            // friendly error rather than letting the database cascade.
            builder.HasMany(t => t.Corrections)
                .WithOne(t => t.CorrectsTransaction)
                .HasForeignKey(t => t.CorrectsTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
