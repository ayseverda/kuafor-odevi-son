namespace kuafordeneme.Models
{
    public class Calisan
    {
        public int Id { get; set; }
        public string AdSoyad { get; set; }
        public string UzmanlikAlanlari { get; set; }
        public bool MusaitlikDurumu { get; set; }

        // Bu çalışanın yapabileceği işlemleri tutuyoruz
        public List<Islem> YapabilecegiIslemler { get; set; }
    }
}
