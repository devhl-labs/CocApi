﻿// <auto-generated />
using System;
using CocApi.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CocApi.Test.Migrations
{
    [DbContext(typeof(CocApiCacheContext))]
    partial class CocApiCacheContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 63)
                .HasAnnotation("ProductVersion", "5.0.3")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            modelBuilder.Entity("CocApi.Cache.Context.CachedItems.CachedClan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<bool>("Download")
                        .HasColumnType("boolean");

                    b.Property<bool>("DownloadMembers")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("DownloadedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool?>("IsWarLogPublic")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("KeepUntil")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RawContent")
                        .HasColumnType("text");

                    b.Property<int?>("StatusCode")
                        .HasColumnType("integer");

                    b.Property<string>("Tag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ExpiresAt");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("KeepUntil");

                    b.HasIndex("Tag")
                        .IsUnique();

                    b.ToTable("clan");
                });

            modelBuilder.Entity("CocApi.Cache.Context.CachedItems.CachedPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ClanTag")
                        .HasColumnType("text");

                    b.Property<bool>("Download")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("DownloadedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("KeepUntil")
                        .HasColumnType("timestamp without time zone");

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

                    b.ToTable("player");
                });

            modelBuilder.Entity("CocApi.Cache.Context.CachedItems.CachedWar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Announcements")
                        .HasColumnType("integer");

                    b.Property<string>("ClanTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("Download")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("DownloadedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("ExpiresAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("IsFinal")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("KeepUntil")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("OpponentTag")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("PreparationStartTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RawContent")
                        .HasColumnType("text");

                    b.Property<DateTime?>("Season")
                        .HasColumnType("timestamp without time zone");

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

                    b.ToTable("war");
                });

            modelBuilder.Entity("CocApi.Cache.Context.CachedItems.CachedClan", b =>
                {
                    b.OwnsOne("CocApi.Cache.Context.CachedItems.CachedClanWar", "CurrentWar", b1 =>
                        {
                            b1.Property<int>("CachedClanId")
                                .HasColumnType("integer");

                            b1.Property<bool>("Added")
                                .HasColumnType("boolean");

                            b1.Property<bool>("Download")
                                .HasColumnType("boolean");

                            b1.Property<DateTime?>("DownloadedAt")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<string>("EnemyTag")
                                .HasColumnType("text");

                            b1.Property<DateTime?>("ExpiresAt")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime?>("KeepUntil")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime?>("PreparationStartTime")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<string>("RawContent")
                                .HasColumnType("text");

                            b1.Property<int?>("State")
                                .HasColumnType("integer");

                            b1.Property<int?>("StatusCode")
                                .HasColumnType("integer");

                            b1.Property<int?>("Type")
                                .HasColumnType("integer");

                            b1.HasKey("CachedClanId");

                            b1.HasIndex("EnemyTag");

                            b1.HasIndex("ExpiresAt");

                            b1.HasIndex("KeepUntil");

                            b1.ToTable("current_war");

                            b1.WithOwner()
                                .HasForeignKey("CachedClanId");
                        });

                    b.OwnsOne("CocApi.Cache.Context.CachedItems.CachedClanWarLeagueGroup", "Group", b1 =>
                        {
                            b1.Property<int>("CachedClanId")
                                .HasColumnType("integer");

                            b1.Property<bool>("Added")
                                .HasColumnType("boolean");

                            b1.Property<bool>("Download")
                                .HasColumnType("boolean");

                            b1.Property<DateTime?>("DownloadedAt")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime?>("ExpiresAt")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime?>("KeepUntil")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<string>("RawContent")
                                .HasColumnType("text");

                            b1.Property<DateTime?>("Season")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<int?>("State")
                                .HasColumnType("integer");

                            b1.Property<int?>("StatusCode")
                                .HasColumnType("integer");

                            b1.HasKey("CachedClanId");

                            b1.HasIndex("ExpiresAt");

                            b1.HasIndex("KeepUntil");

                            b1.ToTable("group");

                            b1.WithOwner()
                                .HasForeignKey("CachedClanId");
                        });

                    b.OwnsOne("CocApi.Cache.Context.CachedItems.CachedClanWarLog", "WarLog", b1 =>
                        {
                            b1.Property<int>("CachedClanId")
                                .HasColumnType("integer");

                            b1.Property<bool>("Download")
                                .HasColumnType("boolean");

                            b1.Property<DateTime?>("DownloadedAt")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime?>("ExpiresAt")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<DateTime?>("KeepUntil")
                                .HasColumnType("timestamp without time zone");

                            b1.Property<string>("RawContent")
                                .HasColumnType("text");

                            b1.Property<int?>("StatusCode")
                                .HasColumnType("integer");

                            b1.HasKey("CachedClanId");

                            b1.HasIndex("ExpiresAt");

                            b1.HasIndex("KeepUntil");

                            b1.ToTable("war_log");

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
