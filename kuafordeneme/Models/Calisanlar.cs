using System.ComponentModel.DataAnnotations;

namespace kuafordeneme.Models
{
    public class Calisanlar
    {
        [Key]
        public int CalisanID { get; set; }           // Çalışan ID (Primary Key)
        public string CalisanAd { get; set; }        // Çalışanın adı
        public int UzmanlikID { get; set; }          // Uzmanlık (Foreign Key to Islem)
        public bool Musaitlik { get; set; }          // Müsait mi (true/false)

        // Navigation property
        public decimal GunlukKazanc { get; set; }
        public Islemler Islem { get; set; }     // Çalışanın uzmanlık yaptığı işlem (Navigation Property)
    }

}
