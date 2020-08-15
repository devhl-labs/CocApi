﻿// <auto-generated />
using System;
using CocApi.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CocApi.Cache.Migrations
{
    [DbContext(typeof(CacheContext))]
    [Migration("20200815165821_Migration0")]
    partial class Migration0
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0-preview.7.20365.15");

            modelBuilder.Entity("CocApi.Cache.Models.Cache.CachedClan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanTag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("DownloadClan")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DownloadCurrentWar")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DownloadCwl")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("DownloadVillages")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ObjectState")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("QueryType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ClanTag")
                        .IsUnique();

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("CocApi.Cache.Models.Cache.CachedItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DownloadDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LocalExpirationDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Raw")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ServerExpirationDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("Path")
                        .IsUnique();

                    b.ToTable("Items");
                });

            modelBuilder.Entity("CocApi.Cache.Models.Cache.CachedVillage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("VillageTag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Villages");
                });

            modelBuilder.Entity("CocApi.Cache.Models.Cache.CachedWar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Announcements")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanTag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsAvailableByClan")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsAvailableByOpponent")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFinal")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Json")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("ObjectState")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OpponentTag")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("PrepStartTime")
                        .HasColumnType("TEXT");

                    b.Property<int?>("QueryType")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarState")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WarTag")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClanTag");

                    b.HasIndex("OpponentTag");

                    b.HasIndex("PrepStartTime", "ClanTag")
                        .IsUnique();

                    b.HasIndex("PrepStartTime", "OpponentTag")
                        .IsUnique();

                    b.ToTable("Wars");
                });
#pragma warning restore 612, 618
        }
    }
}
