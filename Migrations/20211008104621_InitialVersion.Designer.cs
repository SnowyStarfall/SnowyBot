// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SnowyBot.Database;

namespace SnowyBot.Migrations
{
    [DbContext(typeof(GuildContext))]
    [Migration("20211008104621_InitialVersion")]
    partial class InitialVersion
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("SnowyBot.Database.Guild", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Prefix")
                        .HasColumnType("longtext");

                    b.HasKey("ID");

                    b.ToTable("Guilds");
                });
#pragma warning restore 612, 618
        }
    }
}
