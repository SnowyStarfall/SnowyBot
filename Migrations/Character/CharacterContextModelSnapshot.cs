﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SnowyBot.Database;

namespace SnowyBot.Migrations.Character
{
    [DbContext(typeof(CharacterContext))]
    partial class CharacterContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.11");

            modelBuilder.Entity("SnowyBot.Database.Character", b =>
                {
                    b.Property<string>("CharacterID")
                        .HasColumnType("varchar(95)");

                    b.Property<string>("Age")
                        .HasColumnType("longtext");

                    b.Property<string>("AvatarURL")
                        .HasColumnType("longtext");

                    b.Property<string>("CreationDate")
                        .HasColumnType("longtext");

                    b.Property<string>("Description")
                        .HasColumnType("longtext");

                    b.Property<string>("Gender")
                        .HasColumnType("longtext");

                    b.Property<string>("Height")
                        .HasColumnType("longtext");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.Property<string>("Orientation")
                        .HasColumnType("longtext");

                    b.Property<string>("Prefix")
                        .HasColumnType("longtext");

                    b.Property<string>("ReferenceURL")
                        .HasColumnType("longtext");

                    b.Property<string>("Sex")
                        .HasColumnType("longtext");

                    b.Property<string>("Species")
                        .HasColumnType("longtext");

                    b.Property<ulong>("UserID")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Weight")
                        .HasColumnType("longtext");

                    b.HasKey("CharacterID");

                    b.ToTable("Characters");
                });
#pragma warning restore 612, 618
        }
    }
}