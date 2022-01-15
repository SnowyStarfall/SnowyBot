﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SnowyBot.Database;

#nullable disable

namespace SnowyBot.Migrations
{
    [DbContext(typeof(GuildContext))]
    [Migration("20220108041900_FourthVersion")]
    partial class FourthVersion
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("SnowyBot.Database.Guild", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<bool>("DeleteMusic")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("GoodbyeMessage")
                        .HasColumnType("longtext");

                    b.Property<string>("Prefix")
                        .HasColumnType("longtext");

                    b.Property<string>("Roles")
                        .HasColumnType("longtext");

                    b.Property<string>("UserPoint")
                        .HasColumnType("longtext");

                    b.Property<string>("WelcomeMessage")
                        .HasColumnType("longtext");

                    b.Property<string>("PointGain")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.ToTable("Guilds");
                });
#pragma warning restore 612, 618
        }
    }
}