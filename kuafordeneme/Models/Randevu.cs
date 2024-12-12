namespace kuafordeneme.Models
{
    public class Randevu
    {
        public int RandevuID { get; set; }           // Randevu ID (Primary Key)
        public int KullaniciID { get; set; }         // Kullanıcı ID (Foreign Key to Kullanici)
        public int IslemID { get; set; }             // İşlem ID (Foreign Key to Islem)
        public int CalisanID { get; set; }           // Çalışan ID (Foreign Key to Calisan)
        public DateTime RandevuZamani { get; set; }  // Randevu zamanı
        public string Durum { get; set; }            // Randevu durumu (örn. Onaylandı, İptal Edildi)

        // Dinamik listeleme için ek özellikler
        public List<Islemler> Islemler { get; set; }    // İşlem listesi (Dinamik doldurulacak)
        public List<Calisanlar> Calisanlar { get; set; } // Çalışan listesi (Dinamik doldurulacak)

        // Navigation properties
        public Kullanicilar Kullanici { get; set; }     // Kullanıcı (Navigation Property)
        public Islemler Islem { get; set; }             // İşlem (Navigation Property)
        public Calisanlar Calisan { get; set; }      // Çalışan (Navigation Property)
    }
}
