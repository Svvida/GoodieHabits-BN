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

            builder.Property(t => t.Type)
                .IsRequired();

            builder.Property(t => t.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(t => t.OccurredOn)
                .HasColumnType("date")
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
        }
    }
}
