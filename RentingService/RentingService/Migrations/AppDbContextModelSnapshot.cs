﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using RentingService.Data;

#nullable disable

namespace RentingService.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.12")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("RentingService.Data.Entities.Rent", b =>
                {
                    b.Property<Guid>("leaseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("bookId")
                        .HasColumnType("uuid");

                    b.Property<string>("customerName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("leaseStartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("returnDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("leaseId");

                    b.ToTable("Renting", "rentings");
                });

            modelBuilder.Entity("RentingService.Data.Entities.Reservation", b =>
                {
                    b.Property<Guid>("reservationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("bookId")
                        .HasColumnType("uuid");

                    b.Property<string>("customerName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("reservedUntil")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("reservationId");

                    b.ToTable("Reservations", "rentings");
                });
#pragma warning restore 612, 618
        }
    }
}
