﻿// <auto-generated />
using System;
using CocApi.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CocApi.Test.Migrations
{
    [DbContext(typeof(CacheDbContext))]
    partial class CocApiCacheContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CocApi.Cache.Context.CachedClan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Download")
                        .HasColumnType("boolean");

                    b.Property<bool>("DownloadMembers")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("DownloadedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool?>("IsWarLogPublic")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("KeepUntil")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RawContent")
                        .HasColumnType("text");

                    b.Property<int?>("StatusCode")
                        .HasColumnType("integer");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("Tag")
                        .IsUnique();

                    b.ToTable("clan", (string)null);
                });

            modelBuilder.Entity("CocApi.Cache.Context.CachedPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClanTag")
                        .HasColumnType("text");

                    b.Property<bool>("Download")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("DownloadedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("KeepUntil")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RawContent")
                        .HasColumnType("text");

                    b.Property<int?>("StatusCode")
                        .HasColumnType("integer");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ClanTag");

                    b.HasIndex("ExpiresAt");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("KeepUntil");

                    b.HasIndex("Tag")
                        .IsUnique();

                    b.ToTable("player", (string)null);
                });

            modelBuilder.Entity("CocApi.Cache.Context.CachedWar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Announcements")
                        .HasColumnType("integer");

                    b.Property<string>("ClanTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Download")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("DownloadedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsFinal")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("KeepUntil")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("OpponentTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("PreparationStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RawContent")
                        .HasColumnType("text");

                    b.Property<DateTime?>("Season")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("State")
                        .HasColumnType("integer");

                    b.Property<int?>("StatusCode")
                        .HasColumnType("integer");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<string>("WarTag")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ClanTag");

                    b.HasIndex("ExpiresAt");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("IsFinal");

                    b.HasIndex("KeepUntil");

                    b.HasIndex("OpponentTag");

                    b.HasIndex("Season");

                    b.HasIndex("WarTag");

                    b.HasIndex("PreparationStartTime", "ClanTag", "OpponentTag")
                        .IsUnique();

                    b.ToTable("war", (string)null);
                });

            modelBuilder.Entity("CocApi.Cache.Context.CachedClan", b =>
                {
                    b.OwnsOne("CocApi.Cache.Context.CachedClanWar", "CurrentWar", b1 =>
                        {
                            b1.Property<int>("CachedClanId")
                                .HasColumnType("integer");

                            b1.Property<bool>("Added")
                                .HasColumnType("boolean");

                            b1.Property<bool>("Download")
                                .HasColumnType("boolean");

                            b1.Property<DateTime?>("DownloadedAt")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<string>("EnemyTag")
                                .HasColumnType("text");

                            b1.Property<DateTime?>("ExpiresAt")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<DateTime?>("KeepUntil")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<DateTime?>("PreparationStartTime")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<string>("RawContent")
                                .HasColumnType("text");

                            b1.Property<int?>("State")
                                .HasColumnType("integer");

                            b1.Property<int?>("StatusCode")
                                .HasColumnType("integer");

                            b1.Property<int?>("Type")
                                .HasColumnType("integer");

                            b1.HasKey("CachedClanId");

                            b1.HasIndex("CachedClanId", "Download");

                            b1.HasIndex("Added", "CachedClanId", "State");

                            b1.ToTable("current_war", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("CachedClanId");
                        });

                    b.OwnsOne("CocApi.Cache.Context.CachedClanWarLeagueGroup", "Group", b1 =>
                        {
                            b1.Property<int>("CachedClanId")
                                .HasColumnType("integer");

                            b1.Property<bool>("Added")
                                .HasColumnType("boolean");

                            b1.Property<bool>("Download")
                                .HasColumnType("boolean");

                            b1.Property<DateTime?>("DownloadedAt")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<DateTime?>("ExpiresAt")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<DateTime?>("KeepUntil")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<string>("RawContent")
                                .HasColumnType("text");

                            b1.Property<DateTime?>("Season")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<int?>("State")
                                .HasColumnType("integer");

                            b1.Property<int?>("StatusCode")
                                .HasColumnType("integer");

                            b1.HasKey("CachedClanId");

                            b1.HasIndex("ExpiresAt");

                            b1.HasIndex("KeepUntil");

                            b1.ToTable("group", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("CachedClanId");
                        });

                    b.OwnsOne("CocApi.Cache.Context.CachedClanWarLog", "WarLog", b1 =>
                        {
                            b1.Property<int>("CachedClanId")
                                .HasColumnType("integer");

                            b1.Property<bool>("Download")
                                .HasColumnType("boolean");

                            b1.Property<DateTime?>("DownloadedAt")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<DateTime?>("ExpiresAt")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<DateTime?>("KeepUntil")
                                .HasColumnType("timestamp with time zone");

                            b1.Property<string>("RawContent")
                                .HasColumnType("text");

                            b1.Property<int?>("StatusCode")
                                .HasColumnType("integer");

                            b1.HasKey("CachedClanId");

                            b1.HasIndex("ExpiresAt");

                            b1.HasIndex("KeepUntil");

                            b1.ToTable("war_log", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("CachedClanId");
                        });

                    b.Navigation("CurrentWar")
                        .IsRequired();

                    b.Navigation("Group")
                        .IsRequired();

                    b.Navigation("WarLog")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
