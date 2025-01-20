using System.Text.Json;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class RepeatableQuestConfiguration : IEntityTypeConfiguration<RepeatableQuest>
    {
        public void Configure(EntityTypeBuilder<RepeatableQuest> builder)
        {
            builder.ToTable("Repeatable_Quests");

            builder.HasKey(rq => rq.RepeatableQuestId);

            builder.Property(rq => rq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(rq => rq.Description)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(rq => rq.Emoji)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(otq => otq.StartDate)
                .IsRequired(false);

            builder.Property(otq => otq.EndDate)
                .IsRequired(false);

            builder.Property(rq => rq.RepeatTime)
                .IsRequired();

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Configure RepeatInterval as a JSON column
            builder.Property(rq => rq.RepeatInterval)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),  // Serialize to JSON
                    v => JsonSerializer.Deserialize<RepeatInterval>(v, jsonOptions) // Deserialize to object
                )
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            builder.HasOne(rq => rq.Account)
                .WithMany(a => a.RecurringQuests)
                .HasForeignKey(rq => rq.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
