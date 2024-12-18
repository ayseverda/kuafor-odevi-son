using System.ComponentModel.DataAnnotations;

namespace kuafordeneme.Models
{
    public class Mesaj
    {
        [Key]
        public int MesajID { get; set; }             // Mesaj ID (Primary Key)
        public string Email { get; set; }            // E-posta
        public string MusteriAd { get; set; }        // Mesaj gönderen kişinin adı
        public string Konu { get; set; }             // Mesaj konusu
        public string Aciklama { get; set; }         // Mesaj içeriği
        public DateTime Tarih { get; set; }          // Mesaj gönderilme tarihi

        // Navigation property
        public Kullanicilar Kullanici { get; set; }  // Mesajı gönderen kullanıcı
    }
}
