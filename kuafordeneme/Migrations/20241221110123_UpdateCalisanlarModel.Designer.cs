﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using kuafordeneme.Data;

#nullable disable

namespace kuafordeneme.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241221110123_UpdateCalisanlarModel")]
    partial class UpdateCalisanlarModel
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Calisanlar", b =>
                {
                    b.Property<int>("CalisanID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CalisanID"));

                    b.Property<string>("CalisanAd")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("GunlukKazanc")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("IslemID")
                        .HasColumnType("integer");

                    b.Property<bool>("Musaitlik")
                        .HasColumnType("boolean");

                    b.Property<int?>("UzmanlikID")
                        .HasColumnType("integer");

                    b.HasKey("CalisanID");

                    b.HasIndex("IslemID");

                    b.ToTable("Calisanlar");
                });

            modelBuilder.Entity("Randevu", b =>
                {
                    b.Property<int>("RandevuID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("RandevuID"));

                    b.Property<int>("CalisanID")
                        .HasColumnType("integer");

                    b.Property<string>("Durum")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("IslemID")
                        .HasColumnType("integer");

                    b.Property<int>("KullaniciID")
                        .HasColumnType("integer");

                    b.Property<DateTime>("RandevuBitisZamani")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("RandevuZamani")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("RandevuID");

                    b.HasIndex("CalisanID");

                    b.HasIndex("IslemID");

                    b.HasIndex("KullaniciID");

                    b.ToTable("Randevular");
                });

            modelBuilder.Entity("kuafordeneme.Models.Islemler", b =>
                {
                    b.Property<int>("IslemID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("IslemID"));

                    b.Property<string>("IslemAd")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("IslemSuresi")
                        .HasColumnType("integer");

                    b.Property<string>("Tanim")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Ucret")
                        .HasColumnType("numeric");

                    b.HasKey("IslemID");

                    b.ToTable("Islemler");
                });

            modelBuilder.Entity("kuafordeneme.Models.Kullanicilar", b =>
                {
                    b.Property<int>("KullaniciID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("KullaniciID"));

                    b.Property<string>("AdSoyad")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean");

                    b.Property<string>("Sifre")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("KullaniciID");

                    b.ToTable("Kullanicilar");
                });

            modelBuilder.Entity("kuafordeneme.Models.Mesaj", b =>
                {
                    b.Property<int>("MesajID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("MesajID"));

                    b.Property<string>("Aciklama")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Konu")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("KullaniciID")
                        .HasColumnType("integer");

                    b.Property<string>("MusteriAd")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Tarih")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("MesajID");

                    b.HasIndex("KullaniciID");

                    b.ToTable("Mesajlar");
                });

            modelBuilder.Entity("Calisanlar", b =>
                {
                    b.HasOne("kuafordeneme.Models.Islemler", "Islem")
                        .WithMany("Calisanlar")
                        .HasForeignKey("IslemID");

                    b.Navigation("Islem");
                });

            modelBuilder.Entity("Randevu", b =>
                {
                    b.HasOne("Calisanlar", "Calisan")
                        .WithMany()
                        .HasForeignKey("CalisanID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("kuafordeneme.Models.Islemler", "Islem")
                        .WithMany()
                        .HasForeignKey("IslemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("kuafordeneme.Models.Kullanicilar", "Kullanici")
                        .WithMany()
                        .HasForeignKey("KullaniciID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Calisan");

                    b.Navigation("Islem");

                    b.Navigation("Kullanici");
                });

            modelBuilder.Entity("kuafordeneme.Models.Mesaj", b =>
                {
                    b.HasOne("kuafordeneme.Models.Kullanicilar", "Kullanici")
                        .WithMany()
                        .HasForeignKey("KullaniciID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Kullanici");
                });

            modelBuilder.Entity("kuafordeneme.Models.Islemler", b =>
                {
                    b.Navigation("Calisanlar");
                });
#pragma warning restore 612, 618
        }
    }
}
