using kuafordeneme.Models;
using Microsoft.EntityFrameworkCore;

namespace kuafordeneme.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Veritabanı tablolarını temsil eden DbSet'ler
        public DbSet<Kullanicilar> Kullanicilar { get; set; }
        public DbSet<Islemler> Islemler { get; set; }
        public DbSet<Calisanlar> Calisanlar { get; set; }
        public DbSet<Randevu> Randevular { get; set; }
        public DbSet<Mesaj> Mesaj { get; set; }
        // Yeni: Ara tablo
        public DbSet<CalisanUzmanlik> CalisanUzmanliklar { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Çoktan çoğa ilişki için ara tablo yapılandırması
            modelBuilder.Entity<CalisanUzmanlik>()
                .HasKey(cu => new { cu.CalisanID, cu.IslemID }); // Birleşik birincil anahtar

            modelBuilder.Entity<CalisanUzmanlik>()
                .HasOne(cu => cu.Calisan)
                .WithMany(c => c.Uzmanliklar)
                .HasForeignKey(cu => cu.CalisanID);

            modelBuilder.Entity<CalisanUzmanlik>()
                .HasOne(cu => cu.Islem)
                .WithMany(i => i.Uzmanliklar)
                .HasForeignKey(cu => cu.IslemID);

           
        }

    }
}





