namespace kuafordeneme.Models
{
    public class Randevu
    {
        public int Id { get; set; }
        public string MusteriAdi { get; set; }
        public DateTime TarihSaat { get; set; }
        public decimal Ucret { get; set; } // İşlemin ücretini buradan alabiliriz
        public int CalisanId { get; set; } // Hangi çalışanın yaptığı
        public int IslemId { get; set; } // Hangi işlemi yaptığı
        public Islem Islem { get; set; } // İşlem nesnesi
        public Calisan Calisan { get; set; } // Çalışan nesnesi
    }
}
