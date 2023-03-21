﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Snap.Hutao.Server.Model.Context;

#nullable disable

namespace Snap.Hutao.Server.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230321123443_UseAuth")]
    partial class UseAuth
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClaimType")
                        .HasColumnType("longtext");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.Banned", b =>
                {
                    b.Property<string>("Uid")
                        .HasMaxLength(9)
                        .HasColumnType("varchar(9)");

                    b.HasKey("Uid");

                    b.ToTable("banned");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityAvatar", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("ActivedConstellationNumber")
                        .HasColumnType("int");

                    b.Property<int>("AvatarId")
                        .HasColumnType("int");

                    b.Property<long>("RecordId")
                        .HasColumnType("bigint");

                    b.Property<string>("ReliquarySet")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("WeaponId")
                        .HasColumnType("int");

                    b.HasKey("PrimaryId");

                    b.HasIndex("RecordId");

                    b.ToTable("avatars");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityDamageRank", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("AvatarId")
                        .HasColumnType("int");

                    b.Property<long>("SpiralAbyssId")
                        .HasColumnType("bigint");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("PrimaryId");

                    b.HasIndex("SpiralAbyssId")
                        .IsUnique();

                    b.HasIndex("Value");

                    b.ToTable("damage_ranks");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityFloor", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("Index")
                        .HasColumnType("int");

                    b.Property<string>("Levels")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("SpiralAbyssId")
                        .HasColumnType("bigint");

                    b.Property<int>("Star")
                        .HasColumnType("int");

                    b.HasKey("PrimaryId");

                    b.HasIndex("SpiralAbyssId");

                    b.ToTable("spiral_abysses_floors");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityRecord", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasMaxLength(9)
                        .HasColumnType("varchar(9)");

                    b.Property<long>("UploadTime")
                        .HasColumnType("bigint");

                    b.Property<string>("Uploader")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("PrimaryId");

                    b.ToTable("records");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntitySpiralAbyss", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long>("RecordId")
                        .HasColumnType("bigint");

                    b.Property<int>("TotalBattleTimes")
                        .HasColumnType("int");

                    b.Property<int>("TotalWinTimes")
                        .HasColumnType("int");

                    b.HasKey("PrimaryId");

                    b.HasIndex("RecordId")
                        .IsUnique();

                    b.ToTable("spiral_abysses");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityTakeDamageRank", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("AvatarId")
                        .HasColumnType("int");

                    b.Property<long>("SpiralAbyssId")
                        .HasColumnType("bigint");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Value")
                        .HasColumnType("int");

                    b.HasKey("PrimaryId");

                    b.HasIndex("SpiralAbyssId")
                        .IsUnique();

                    b.HasIndex("Value");

                    b.ToTable("take_damage_ranks");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.HutaoLog", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<string>("Info")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("Resolved")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("PrimaryId");

                    b.ToTable("hutao_logs");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.HutaoLogSingleItem", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("DeviceId")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.Property<long>("LogId")
                        .HasColumnType("bigint");

                    b.Property<long>("Time")
                        .HasColumnType("bigint");

                    b.HasKey("PrimaryId");

                    b.HasIndex("LogId");

                    b.ToTable("hutao_log_items");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.HutaoUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("longtext");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("longtext");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("longtext");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("varchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.LegacyStatistics", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("ScheduleId")
                        .HasColumnType("int");

                    b.HasKey("PrimaryId");

                    b.ToTable("spiral_abysses_statistics");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.RequestStatistics", b =>
                {
                    b.Property<long>("PrimaryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<long>("Count")
                        .HasColumnType("bigint");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserAgent")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("PrimaryId");

                    b.ToTable("request_statistics");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<int>", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.HutaoUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<int>", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.HutaoUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<int>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<int>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Snap.Hutao.Server.Model.Entity.HutaoUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<int>", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.HutaoUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityAvatar", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.EntityRecord", "Record")
                        .WithMany("Avatars")
                        .HasForeignKey("RecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Record");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityDamageRank", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.EntitySpiralAbyss", "SpiralAbyss")
                        .WithOne("Damage")
                        .HasForeignKey("Snap.Hutao.Server.Model.Entity.EntityDamageRank", "SpiralAbyssId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SpiralAbyss");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityFloor", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.EntitySpiralAbyss", "SpiralAbyss")
                        .WithMany("Floors")
                        .HasForeignKey("SpiralAbyssId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SpiralAbyss");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntitySpiralAbyss", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.EntityRecord", "Record")
                        .WithOne("SpiralAbyss")
                        .HasForeignKey("Snap.Hutao.Server.Model.Entity.EntitySpiralAbyss", "RecordId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Record");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityTakeDamageRank", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.EntitySpiralAbyss", "SpiralAbyss")
                        .WithOne("TakeDamage")
                        .HasForeignKey("Snap.Hutao.Server.Model.Entity.EntityTakeDamageRank", "SpiralAbyssId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SpiralAbyss");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.HutaoLogSingleItem", b =>
                {
                    b.HasOne("Snap.Hutao.Server.Model.Entity.HutaoLog", "Log")
                        .WithMany()
                        .HasForeignKey("LogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Log");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntityRecord", b =>
                {
                    b.Navigation("Avatars");

                    b.Navigation("SpiralAbyss");
                });

            modelBuilder.Entity("Snap.Hutao.Server.Model.Entity.EntitySpiralAbyss", b =>
                {
                    b.Navigation("Damage")
                        .IsRequired();

                    b.Navigation("Floors");

                    b.Navigation("TakeDamage")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
