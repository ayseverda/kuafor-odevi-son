namespace kuafordeneme.Models
{
    public class Mesaj
    {
        public int MesajID { get; set; }             // Mesaj ID (Primary Key)
        public int KullaniciID { get; set; }         // Kullanıcı ID (Foreign Key to Kullanici)
        public string Konu { get; set; }             // Mesaj konusu
        public string Aciklama { get; set; }         // Mesaj içeriği
        public DateTime Tarih { get; set; }          // Mesaj gönderilme tarihi

        // Navigation property
        public Kullanici Kullanici { get; set; }     // Mesajı gönderen kullanıcı (Navigation Property)
    }

}
