using System.Text.Json;
using Domain.Models;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    public class ActiveUserEffectConfiguration : IEntityTypeConfiguration<ActiveUserEffect>
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            // This ensures the [JsonPolymorphic] attributes are respected
            // and handles Case Insensitivity which is safer for DB JSON
            PropertyNameCaseInsensitive = true
        };
        public void Configure(EntityTypeBuilder<ActiveUserEffect> builder)
        {
            builder.ToTable("ActiveUserEffects");
            builder.HasKey(aue => aue.Id);

            builder.Property(aue => aue.UserProfileId)
                .IsRequired();

            builder.Property(aue => aue.EffectType)
                .IsRequired();

            builder.Property(aue => aue.ExpiresAt)
                .IsRequired(false);

            builder.Property(aue => aue.UsageCount)
                .IsRequired(false);

            builder.Property(aue => aue.Values)
                .HasColumnName("EffectDataJson")
                .HasColumnType("nvarchar(max)")
                .IsRequired()
                .HasConversion(
                    v => JsonSerializer.Serialize(v, _jsonOptions),
                    v => JsonSerializer.Deserialize<ActiveEffectValues>(v, _jsonOptions)!
                );

            builder.HasOne(aue => aue.UserProfile)
                .WithMany(up => up.ActiveUserEffects)
                .HasForeignKey(aue => aue.UserProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(aue => aue.SourceItem)
                .WithMany(si => si.ActiveUserEffects)
                .HasForeignKey(aue => aue.SourceItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
