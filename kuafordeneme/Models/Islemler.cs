using System.ComponentModel.DataAnnotations;

namespace kuafordeneme.Models
{
    public class Islemler
    {
        [Key]
        public int IslemID { get; set; }
        public string IslemAd { get; set; }
        public int IslemSuresi { get; set; }
        public decimal Ucret { get; set; }
        public string Tanim { get; set; }
    }


}
