namespace kuafordeneme.Models
{
    public class RandevuViewModel
    {
        public int RandevuID { get; set; }          // Randevu ID
        public string KullaniciAd { get; set; }    // Kullanıcı Adı
        public string IslemAd { get; set; }        // İşlem Adı
        public string CalisanAd { get; set; }      // Çalışan Adı
        public DateTime RandevuZamani { get; set; } // Randevu Zamanı
        public string Durum { get; set; }          // Randevu Durumu (Onaylandı, Bekliyor, İptal Edildi gibi)
    }
}
