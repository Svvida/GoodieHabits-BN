﻿// <auto-generated />
using System;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Domain.Models.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("HashPassword")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Username")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique()
                        .HasFilter("[Username] IS NOT NULL");

                    b.ToTable("Accounts", (string)null);
                });

            modelBuilder.Entity("Domain.Models.DailyQuest", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(10000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Emoji")
                        .HasMaxLength(10)
                        .HasColumnType("NVARCHAR");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("LastCompleted")
                        .HasColumnType("datetime2");

                    b.Property<int?>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Daily_Quests", (string)null);
                });

            modelBuilder.Entity("Domain.Models.MonthlyQuest", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(10000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Emoji")
                        .HasMaxLength(10)
                        .HasColumnType("NVARCHAR");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("EndDay")
                        .HasColumnType("int");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<int?>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("StartDay")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Monthly_Quests", (string)null);
                });

            modelBuilder.Entity("Domain.Models.OneTimeQuest", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(10000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Emoji")
                        .HasMaxLength(10)
                        .HasColumnType("NVARCHAR");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<int?>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("One_Time_Quests", (string)null);
                });

            modelBuilder.Entity("Domain.Models.QuestLabel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("BackgroundColor")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("nvarchar(7)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("TextColor")
                        .IsRequired()
                        .HasMaxLength(7)
                        .HasColumnType("nvarchar(7)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("Value");

                    b.ToTable("Quest_Labels", (string)null);
                });

            modelBuilder.Entity("Domain.Models.QuestMetadata", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("QuestType")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("QuestType");

                    b.HasIndex("AccountId", "QuestType");

                    b.ToTable("Quests", (string)null);
                });

            modelBuilder.Entity("Domain.Models.QuestMetadata_QuestLabel", b =>
                {
                    b.Property<int>("QuestMetadataId")
                        .HasColumnType("int");

                    b.Property<int>("QuestLabelId")
                        .HasColumnType("int");

                    b.HasKey("QuestMetadataId", "QuestLabelId");

                    b.HasIndex("QuestLabelId");

                    b.ToTable("QuestMetadata_QuestLabel", (string)null);
                });

            modelBuilder.Entity("Domain.Models.SeasonalQuest", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(10000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Emoji")
                        .HasMaxLength(10)
                        .HasColumnType("NVARCHAR");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<int?>("Priority")
                        .HasColumnType("int");

                    b.Property<int>("Season")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Seasonal_Quests", (string)null);
                });

            modelBuilder.Entity("Domain.Models.WeeklyQuest", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasMaxLength(10000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Emoji")
                        .HasMaxLength(10)
                        .HasColumnType("NVARCHAR");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<int?>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("WeekdaysSerialized")
                        .IsRequired()
                        .HasColumnType("NVARCHAR(MAX)");

                    b.HasKey("Id");

                    b.ToTable("Weekly_Quests", (string)null);
                });

            modelBuilder.Entity("Domain.Models.DailyQuest", b =>
                {
                    b.HasOne("Domain.Models.QuestMetadata", "QuestMetadata")
                        .WithOne("DailyQuest")
                        .HasForeignKey("Domain.Models.DailyQuest", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("QuestMetadata");
                });

            modelBuilder.Entity("Domain.Models.MonthlyQuest", b =>
                {
                    b.HasOne("Domain.Models.QuestMetadata", "QuestMetadata")
                        .WithOne("MonthlyQuest")
                        .HasForeignKey("Domain.Models.MonthlyQuest", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("QuestMetadata");
                });

            modelBuilder.Entity("Domain.Models.OneTimeQuest", b =>
                {
                    b.HasOne("Domain.Models.QuestMetadata", "QuestMetadata")
                        .WithOne("OneTimeQuest")
                        .HasForeignKey("Domain.Models.OneTimeQuest", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("QuestMetadata");
                });

            modelBuilder.Entity("Domain.Models.QuestLabel", b =>
                {
                    b.HasOne("Domain.Models.Account", "Account")
                        .WithMany("Labels")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Domain.Models.QuestMetadata", b =>
                {
                    b.HasOne("Domain.Models.Account", "Account")
                        .WithMany("Quests")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");
                });

            modelBuilder.Entity("Domain.Models.QuestMetadata_QuestLabel", b =>
                {
                    b.HasOne("Domain.Models.QuestLabel", "QuestLabel")
                        .WithMany("QuestMetadataRelations")
                        .HasForeignKey("QuestLabelId")
                        .IsRequired();

                    b.HasOne("Domain.Models.QuestMetadata", "QuestMetadata")
                        .WithMany("QuestLabels")
                        .HasForeignKey("QuestMetadataId")
                        .IsRequired();

                    b.Navigation("QuestLabel");

                    b.Navigation("QuestMetadata");
                });

            modelBuilder.Entity("Domain.Models.SeasonalQuest", b =>
                {
                    b.HasOne("Domain.Models.QuestMetadata", "QuestMetadata")
                        .WithOne("SeasonalQuest")
                        .HasForeignKey("Domain.Models.SeasonalQuest", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("QuestMetadata");
                });

            modelBuilder.Entity("Domain.Models.WeeklyQuest", b =>
                {
                    b.HasOne("Domain.Models.QuestMetadata", "QuestMetadata")
                        .WithOne("WeeklyQuest")
                        .HasForeignKey("Domain.Models.WeeklyQuest", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("QuestMetadata");
                });

            modelBuilder.Entity("Domain.Models.Account", b =>
                {
                    b.Navigation("Labels");

                    b.Navigation("Quests");
                });

            modelBuilder.Entity("Domain.Models.QuestLabel", b =>
                {
                    b.Navigation("QuestMetadataRelations");
                });

            modelBuilder.Entity("Domain.Models.QuestMetadata", b =>
                {
                    b.Navigation("DailyQuest");

                    b.Navigation("MonthlyQuest");

                    b.Navigation("OneTimeQuest");

                    b.Navigation("QuestLabels");

                    b.Navigation("SeasonalQuest");

                    b.Navigation("WeeklyQuest");
                });
#pragma warning restore 612, 618
        }
    }
}
