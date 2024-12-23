using System.ComponentModel.DataAnnotations;

namespace kuafordeneme.Models
{
    public class Mesaj
    {
        [Key]
        public int MesajID { get; set; }
        public string MusteriAd { get; set; }
        public string Email { get; set; }
        public string Konu { get; set; }
        public string Aciklama { get; set; }
        public DateTime Tarih { get; set; }

        // Kullanıcıyı ilişkilendiriyoruz
        public int KullaniciID { get; set; }  // Foreign Key
        public virtual Kullanicilar Kullanici { get; set; }  // Navigation property
    }

}
