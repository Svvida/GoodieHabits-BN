﻿using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configuration
{
    internal class SeasonalQuestConfiguration : IEntityTypeConfiguration<SeasonalQuest>
    {
        public void Configure(EntityTypeBuilder<SeasonalQuest> builder)
        {
            builder.ToTable("Seasonal_Quests");

            builder.HasKey(sq => sq.Id);

            builder.Property(sq => sq.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sq => sq.Description)
                .HasMaxLength(1000);

            builder.Property(sq => sq.StartDate)
                .IsRequired();

            builder.Property(sq => sq.EndDate)
                .IsRequired();

            builder.Property(sq => sq.IsCompleted)
                .IsRequired();

            builder.Property(sq => sq.Emoji)
                .HasMaxLength(10)
                .HasColumnType("NVARCHAR");

            builder.Property(sq => sq.CreatedAt)
                .IsRequired();

            builder.Property(sq => sq.UpdatedAt)
                .IsRequired(false);

            builder.HasOne(sq => sq.Quest)
                .WithOne()
                .HasForeignKey<SeasonalQuest>(sq => sq.Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
