namespace kuafordeneme.Models
{
    public class Islem
    {
        public int IslemID { get; set; }             // İşlem ID (Primary Key)
        public string IslemAd { get; set; }          // İşlem adı (örn. Saç kesimi)
        public int IslemSuresi { get; set; }         // İşlem süresi (dakika cinsinden)
        public decimal Ucret { get; set; }           // İşlem ücreti
        public string Tanim { get; set; }            // İşlem açıklaması
    }

}
