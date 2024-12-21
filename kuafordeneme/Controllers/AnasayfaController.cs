using kuafordeneme.Data;
using kuafordeneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;



namespace kuafordeneme.Controllers
{
    public class AnasayfaController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Constructor ile DbContext enjeksiyonu
        public AnasayfaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult MesajGonder(string adSoyad, string email, string konu, string mesaj)
        {
            // Boş alan kontrolü
            if (string.IsNullOrEmpty(adSoyad) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(konu) || string.IsNullOrEmpty(mesaj))
            {
                TempData["Error"] = "Tüm alanları doldurmanız gerekiyor!";
                return RedirectToAction("Index");
            }

            // Mesajı veritabanına ekleme
            var mesajGonder = new Mesaj
            {
                MusteriAd = adSoyad,        // Kullanıcı adı
                Email = email,              // Kullanıcı e-posta
                Konu = konu,                // Mesaj konusu
                Aciklama = mesaj,           // Mesaj içeriği
                Tarih = DateTime.Now       // Mesaj gönderilme tarihi
            };

            // Veritabanına ekleme
            _context.Mesajlar.Add(mesajGonder);
            _context.SaveChanges();

            TempData["Success"] = "Mesajınız başarıyla gönderildi!";
            return RedirectToAction("Index");
        }


        // Anasayfa
        public IActionResult Index()
        {
            return View();
        }


        // Hizmetlerimiz
        public IActionResult Hizmetlerimiz()
        {
            return View();
        }

        // İletişim
        public IActionResult Iletisim()
        {
            return View();
        }

        public IActionResult GirisYap()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GirisYap(string email, string sifre)
        {
            try
            {
                // Kullanıcıyı veritabanından EF ile sorgulama
                var kullanici = await _context.Kullanicilar
                    .FirstOrDefaultAsync(k => k.Email == email);

                if (kullanici != null) // Kullanıcı bulunduysa
                {
                    if (kullanici.Sifre == sifre) // Şifre doğruysa
                    {
                        // Kullanıcı bilgilerini session'a kaydet
                        HttpContext.Session.SetInt32("KullaniciID", kullanici.KullaniciID);
                        HttpContext.Session.SetString("AdSoyad", kullanici.AdSoyad);
                        HttpContext.Session.SetString("Email", kullanici.Email);

                        // Kullanıcı rolünü belirle
                        if (kullanici.IsAdmin)
                        {
                            HttpContext.Session.SetString("UserRole", "Admin");
                            return RedirectToAction("AdminPanel"); // Admin için yönlendirme
                        }
                        else
                        {
                            HttpContext.Session.SetString("UserRole", "User");
                            return RedirectToAction("Index"); // Müşteri için yönlendirme
                        }
                    }
                    else
                    {
                        TempData["Error"] = "Geçersiz şifre.";
                    }
                }
                else
                {
                    TempData["Error"] = "Geçersiz e-posta.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu!";
                Console.WriteLine($"Hata: {ex.Message}");
            }

            return View(); // Hatalı girişte aynı sayfaya döner
        }

        public IActionResult KayitOl()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> KayitOl(string adSoyad, string email, string sifre, string sifreOnay)
        {
            if (sifre != sifreOnay)
            {
                TempData["Error"] = "Şifreler uyuşmuyor!";
                return View();
            }

            // Kullanıcıyı veritabanına ekleme
            try
            {
                var kullanici = new Kullanicilar
                {
                    AdSoyad = adSoyad,
                    Email = email,
                    Sifre = sifre,  // Şifreyi düz metinle kaydediyoruz
                    IsAdmin = false  // Varsayılan olarak kullanıcılar admin değil
                };

                // DbContext üzerinden kullanıcıyı ekliyoruz
                _context.Kullanicilar.Add(kullanici);
                await _context.SaveChangesAsync();  // Değişiklikleri kaydediyoruz

                ViewData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("GirisYap");  // Kayıt işlemi başarılıysa giriş sayfasına yönlendirilir
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Kayıt sırasında bir hata oluştu!";
                Console.WriteLine($"Hata: {ex.Message}");
            }

            return View();  // Hata varsa tekrar kayıt ol sayfasına yönlendirme
        }

        public IActionResult CikisYap()
        {
            HttpContext.Session.Clear(); // Tüm session bilgilerini temizle
            return RedirectToAction("GirisYap", "Anasayfa");
        }
        private readonly string apiKey = "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"; // OpenAI API Anahtarınızı buraya ekleyin

        // GET: /Anasayfa/YapayZeka
        public IActionResult YapayZeka()
        {
            return View();
        }

        // POST: /Anasayfa/YapayZeka
        [HttpPost]
        public async Task<IActionResult> YapayZeka(IFormFile photo)
{
    if (photo == null || photo.Length == 0)
    {
        TempData["Error"] = "Lütfen geçerli bir fotoğraf yükleyin.";
        return RedirectToAction("YapayZeka");
    }

    try
    {
        // Fotoğrafı kaydet
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream);
        }

        // OpenAI API'ye istek göndermek için HTTP istemcisi
        var client = new HttpClient();

        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
        form.Add(fileContent, "image", uniqueFileName);

        // OpenAI API anahtarı
        client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_OPENAI_API_KEY");

        // Görsel düzenleme isteği
        var response = await client.PostAsync("https://api.openai.com/v1/images/generations", form);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Saç stili eklenirken bir hata oluştu.";
            return RedirectToAction("YapayZeka");
        }

        // API'den gelen yanıtı al
        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);
        string updatedImageUrl = jsonResponse.data[0].url;

        // Yeni fotoğrafı kullanıcıya göster
        ViewBag.ImageUrl = updatedImageUrl;

        return View();
    }
    catch (Exception ex)
    {
        TempData["Error"] = "Bir hata oluştu: " + ex.Message;
        return RedirectToAction("YapayZeka");
    }
}


        public async Task<IActionResult> RandevuAl(string adSoyad, int islemID, int calisanID, DateTime randevuZamani)
        {
            if (ModelState.IsValid)
            {
                // Kullanıcıyı bul
                var kullanici = await _context.Kullanicilar
                                              .FirstOrDefaultAsync(k => k.AdSoyad == adSoyad);

                if (kullanici == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("RandevuAl");
                }

                // Eğer zaman belirtilmemişse (Unspecified), UTC'ye dönüştür
                if (randevuZamani.Kind == DateTimeKind.Unspecified)
                {
                    randevuZamani = DateTime.SpecifyKind(randevuZamani, DateTimeKind.Utc);
                }

                // Randevu oluşturma işlemi
                var randevu = new Randevu
                {
                    KullaniciID = kullanici.KullaniciID,
                    IslemID = islemID,
                    CalisanID = calisanID,
                    RandevuZamani = randevuZamani,
                    RandevuBitisZamani = randevuZamani.AddMinutes(30), // Örnek olarak 30 dakikalık işlem süresi
                    Durum = "Bekliyor"
                };

                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Randevunuz başarıyla alındı.";
                return RedirectToAction("RandevuAl");
            }

            // Model valid değilse islemleri ve calisanlari tekrar gönder
            ViewBag.Islemler = _context.Islemler.ToList();
            ViewBag.Calisanlar = _context.Calisanlar.ToList();
            TempData["Error"] = "Lütfen tüm alanları doldurun.";
            return View();
        }



        public async Task<IActionResult> AdminPanel()
        {
            try
            {
                var islemler = await _context.Islemler.ToListAsync();
                var calisanlar = await _context.Calisanlar.ToListAsync();
                var kullanicilar = await _context.Kullanicilar.ToListAsync();
                var randevular = await (from r in _context.Randevular
                                        join k in _context.Kullanicilar on r.KullaniciID equals k.KullaniciID
                                        join i in _context.Islemler on r.IslemID equals i.IslemID
                                        join c in _context.Calisanlar on r.CalisanID equals c.CalisanID
                                        select new RandevuViewModel
                                        {
                                            RandevuID = r.RandevuID,
                                            KullaniciAd = k.AdSoyad,
                                            IslemAd = i.IslemAd,
                                            CalisanAd = c.CalisanAd,
                                            RandevuZamani = r.RandevuZamani,
                                            Durum = r.Durum
                                        }).ToListAsync();

                ViewBag.Islemler = islemler;
                ViewBag.Calisanlar = calisanlar;
                ViewBag.Kullanicilar = kullanicilar;
                ViewBag.Randevular = randevular;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
            }

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> IslemEkleGuncelle(int? islemID, string islemAd, int islemSuresi, decimal ucret, string tanim, string islem)
        {
            try
            {
                if (islem == "ekle")
                {
                    var yeniIslem = new Islemler
                    {
                        IslemAd = islemAd,
                        IslemSuresi = islemSuresi,
                        Ucret = ucret,
                        Tanim = tanim
                    };
                    _context.Islemler.Add(yeniIslem);
                }
                else if (islem == "guncelle" && islemID.HasValue)
                {
                    var mevcutIslem = await _context.Islemler.FindAsync(islemID.Value);
                    if (mevcutIslem != null)
                    {
                        mevcutIslem.IslemAd = islemAd;
                        mevcutIslem.IslemSuresi = islemSuresi;
                        mevcutIslem.Ucret = ucret;
                        mevcutIslem.Tanim = tanim;
                    }
                }
                else if (islem == "sil" && islemID.HasValue)
                {
                    var islemSil = await _context.Islemler.FindAsync(islemID.Value);
                    if (islemSil != null)
                    {
                        _context.Islemler.Remove(islemSil);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("AdminPanel");
        }


        [HttpPost]
        public async Task<IActionResult> CalisanEkleGuncelle(int? calisanID, string calisanAd, int uzmanlikID, bool musaitlik, string islem,int gunlukKazanc)
        {
            try
            {
                if (islem == "ekle")
                {
                    var yeniCalisan = new Calisanlar
                    {
                        CalisanAd = calisanAd,
                        UzmanlikID = uzmanlikID,  // Valid Islem ID olmalı
                        Musaitlik = musaitlik,
                        GunlukKazanc = gunlukKazanc,
                    };
                    _context.Calisanlar.Add(yeniCalisan);
                }
                else if (islem == "guncelle" && calisanID.HasValue)
                {
                    var mevcutCalisan = await _context.Calisanlar.FindAsync(calisanID.Value);
                    if (mevcutCalisan != null)
                    {
                        mevcutCalisan.CalisanAd = calisanAd;
                        mevcutCalisan.UzmanlikID = uzmanlikID;
                        mevcutCalisan.Musaitlik = musaitlik;
                    }
                }
                else if (islem == "sil" && calisanID.HasValue)
                {
                    var calisanSil = await _context.Calisanlar.FindAsync(calisanID.Value);
                    if (calisanSil != null)
                    {
                        _context.Calisanlar.Remove(calisanSil);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("AdminPanel");
        }


        [HttpPost]
        public async Task<IActionResult> KullaniciEkleGuncelle(int? kullaniciID, string adSoyad, string email, string sifre, bool rol, string islem)
        {
            try
            {
                if (islem == "ekle")
                {
                    var yeniKullanici = new Kullanicilar
                    {
                        AdSoyad = adSoyad,
                        Email = email,
                        Sifre = sifre,
                        IsAdmin = rol
                    };
                    _context.Kullanicilar.Add(yeniKullanici);
                }
                else if (islem == "guncelle" && kullaniciID.HasValue)
                {
                    var mevcutKullanici = await _context.Kullanicilar.FindAsync(kullaniciID.Value);
                    if (mevcutKullanici != null)
                    {
                        mevcutKullanici.AdSoyad = adSoyad;
                        mevcutKullanici.Email = email;
                        mevcutKullanici.Sifre = sifre;
                        mevcutKullanici.IsAdmin = rol;
                    }
                }
                else if (islem == "sil" && kullaniciID.HasValue)
                {
                    var kullaniciSil = await _context.Kullanicilar.FindAsync(kullaniciID.Value);
                    if (kullaniciSil != null)
                    {
                        _context.Kullanicilar.Remove(kullaniciSil);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("AdminPanel");
        }
        
        [HttpPost]
        public async Task<IActionResult> RandevuOnayRed(int randevuID, string islem)
        {
            try
            {
                var randevu = await _context.Randevular.FindAsync(randevuID);
                if (randevu != null)
                {
                    randevu.Durum = islem == "onayla" ? "Onaylandı" : "Reddedildi";
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Randevu durumu başarıyla güncellendi!";
                }
                else
                {
                    TempData["Error"] = "Randevu bulunamadı.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("AdminPanel");
        }






    }

}
       