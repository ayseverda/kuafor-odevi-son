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
        public DbSet<Mesaj> Mesajlar { get; set; }
    }
}
