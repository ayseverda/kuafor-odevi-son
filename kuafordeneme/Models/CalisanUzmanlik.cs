namespace kuafordeneme.Models
{
    public class CalisanUzmanlik
    {
        public int CalisanID { get; set; }
        public Calisanlar Calisan { get; set; }

        public int IslemID { get; set; }
        public Islemler Islem { get; set; }
    }
}
