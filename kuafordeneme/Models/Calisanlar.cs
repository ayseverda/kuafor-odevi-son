using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kuafordeneme.Models
{
    public class Calisanlar
    {
        [Key]
        public int CalisanID { get; set; } // Çalışan ID (Primary Key)

        [Required]
        public string CalisanAd { get; set; } // Çalışan adı

        public int? UzmanlikID { get; set; } // Uzmanlık (Nullable Foreign Key)

        [Required]
        public bool Musaitlik { get; set; } // Müsaitlik durumu (True/False)

        [Column(TypeName = "decimal(18,2)")]
        public decimal GunlukKazanc { get; set; } = 0; // Varsayılan 0.00 olacak

        // Navigation property
        public Islemler? Islem { get; set; } // Çalışanın uzmanlık yaptığı işlem (Nullable Navigation Property)
    }
}
