using System.ComponentModel.DataAnnotations;

namespace kuafordeneme.Models
{
    public class Kullanicilar
    {
        [Key]
            public int KullaniciID { get; set; }
            public string AdSoyad { get; set; }
            public string Email { get; set; }
            public string Sifre { get; set; }
            public bool IsAdmin { get; set; }
            public DateTime CreatedDate { get; set; }
       

    }
}
