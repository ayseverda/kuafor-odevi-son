namespace kuafordeneme.Models
{
    public class Islem
    {
        public int Id { get; set; }
        public string IslemAdi { get; set; } // Örnek: "Saç Kesimi", "Boyama"
        public int Sure { get; set; } // Dakika cinsinden süre (Örnek: 30 dk)
        public decimal Ucret { get; set; } // Ücret (Örnek: 50 TL)
        public string Uzmanikim { get; set; } // Hangi çalışan türü (Örnek: "Saç Kesimi Uzmanı")
    }
}
