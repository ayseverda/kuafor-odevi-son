using kuafordeneme.Models;
using System.ComponentModel.DataAnnotations;

public class Randevu
{
    [Key]
    public int RandevuID { get; set; }           // Randevu ID (Primary Key)
    public int KullaniciID { get; set; }         // Kullanıcı ID (Foreign Key)
    public int IslemID { get; set; }             // İşlem ID (Foreign Key to Islem)
    public int CalisanID { get; set; }           // Çalışan ID (Foreign Key to Calisan)
    public DateTime RandevuZamani { get; set; }  // Randevu zamanı
    public DateTime RandevuBitisZamani { get; set; }
    public string Durum { get; set; }            // Randevu durumu (örn. Onaylandı, İptal Edildi)

    // Navigation properties
    public Kullanicilar Kullanici { get; set; }     // Kullanıcı (Navigation Property)
    public Islemler Islem { get; set; }             // İşlem (Navigation Property)
    public Calisanlar Calisan { get; set; }         // Çalışan (Navigation Property)
}
