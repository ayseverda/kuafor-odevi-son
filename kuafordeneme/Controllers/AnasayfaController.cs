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

            // Kullanıcıyı email adresiyle bulma
            var kullanici = _context.Kullanicilar
                                    .FirstOrDefault(k => k.Email == email);

            // Eğer kullanıcı yoksa, sadece email ile mesajı kaydediyoruz
            if (kullanici != null)
            {
                // Mesajı veritabanına ekleme
                var mesajGonder = new Mesaj
                {
                    MusteriAd = adSoyad,
                    Email = email,
                    Konu = konu,
                    Aciklama = mesaj,
                    Tarih = DateTime.UtcNow,   // UTC kullanımı
                    KullaniciID = kullanici.KullaniciID  // Kullanıcıyı ilişkilendiriyoruz
                };

                // Veritabanına ekleme
                _context.Mesaj.Add(mesajGonder);
                _context.SaveChanges();
                TempData["Success"] = "Mesajınız başarıyla gönderildi!";
            }
            else
            {
                TempData["Error"] = "Kullanıcı bulunamadı!";
            }

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
        [HttpPost]
        public async Task<IActionResult> YapayZeka(IFormFile photo, string hairStyle)
        {
            if (photo == null || photo.Length == 0 || string.IsNullOrEmpty(hairStyle))
            {
                TempData["Error"] = "Lütfen geçerli bir fotoğraf yükleyin ve saç stilinizi yazın.";
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

                // DeepAI API'ye istek göndermek için HTTP istemcisi
                using var client = new HttpClient();
                var form = new MultipartFormDataContent();

                // Fotoğrafı byte dizisine çevirme
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                var byteArrayContent = new ByteArrayContent(fileBytes);
                form.Add(byteArrayContent, "image", uniqueFileName);

                // Saç stili açıklaması ekleme
                form.Add(new StringContent(hairStyle), "prompt");

                // DeepAI API anahtarı ekleme
                client.DefaultRequestHeaders.Add("Api-Key", "7d1025af-aa5d-4406-b6cd-8bb44cb569e8");

                // API isteği
                var response = await client.PostAsync("https://api.deepai.org/api/text2img", form);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "API Hatası: " + errorContent;
                    return RedirectToAction("YapayZeka");
                }

                // API'den gelen yanıtı işleme
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonConvert.DeserializeObject<dynamic>(responseBody);

                // Yeni fotoğrafın URL'sini al
                string updatedImageUrl = jsonResponse.output_url;

                // Yeni fotoğrafı View'a gönder
                ViewBag.ImageUrl = updatedImageUrl;
                ViewBag.OriginalImage = "/uploads/" + uniqueFileName; // Orijinal yüklenen fotoğrafın URL'si

                // Sunucuda depolanan dosyayı sil
                System.IO.File.Delete(filePath);

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction("YapayZeka");
            }
        }










        [HttpGet]
        public async Task<IActionResult> GetCalisanlar(int islemID)
        {
            try
            {
                // Uzmanlık ID'sine göre çalışanları getir
                var calisanlar = await _context.Calisanlar
                    .Where(c => c.UzmanlikID == islemID)
                    .Select(c => new
                    {
                        c.CalisanID,
                        c.CalisanAd,
                        c.Musaitlik
                    })
                    .ToListAsync();

                return Json(calisanlar);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return Json(new { message = $"Hata: {ex.Message}" });
            }
        }

        public async Task<IActionResult> RandevuAl()
        {
            ViewBag.Islemler = await _context.Islemler.ToListAsync();
            ViewBag.Calisanlar = await _context.Calisanlar.ToListAsync();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> RandevuAl(string adSoyad, int islemID, int calisanID, DateTime randevuZamani)
        {
            // Kullanıcı giriş kontrolü
            var kullaniciID = HttpContext.Session.GetInt32("KullaniciID");

            if (kullaniciID == null)
            {
                TempData["Error"] = "Randevu alabilmek için giriş yapmalısınız.";
                return RedirectToAction("GirisYap");
            }
            // Kullanıcı bilgisi doğrulama
            var oturumdakiKullanici = await _context.Kullanicilar
                                                    .FirstOrDefaultAsync(k => k.KullaniciID == kullaniciID);

            if (oturumdakiKullanici == null)
            {
                TempData["Error"] = "Geçersiz kullanıcı oturumu. Lütfen yeniden giriş yapın.";
                return RedirectToAction("GirisYap");
            }
            // Sadece kendi adına randevu alabilme kontrolü
            if (!string.Equals(oturumdakiKullanici.AdSoyad, adSoyad, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "Sadece kendi adınıza randevu alabilirsiniz.";
                return RedirectToAction("RandevuAl");
            }

            if (ModelState.IsValid)
            {
                var kullanici = await _context.Kullanicilar
                                              .FirstOrDefaultAsync(k => k.AdSoyad == adSoyad);

                if (kullanici == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("RandevuAl");
                }
                var islem = await _context.Islemler
                                 .FirstOrDefaultAsync(i => i.IslemID == islemID);

                if (islem == null)
                {
                    TempData["Error"] = "İşlem bulunamadı.";
                    return RedirectToAction("RandevuAl");
                }

                if (randevuZamani.Kind == DateTimeKind.Unspecified)
                {
                    randevuZamani = DateTime.SpecifyKind(randevuZamani, DateTimeKind.Utc);
                }


                var salonCalismaSaatleri = new { BaslangicSaati = 9, BitisSaati = 18 };
                if (randevuZamani.Hour < salonCalismaSaatleri.BaslangicSaati || randevuZamani.Hour >= salonCalismaSaatleri.BitisSaati)
                {
                    TempData["Error"] = "Randevu saatleri 09:00 - 18:00 arasında olmalıdır.";
                    return RedirectToAction("RandevuAl");
                }
              var randevuBitisZamani = randevuZamani.AddMinutes(islem.IslemSuresi);

        // Çakışma kontrolü
        var mevcutRandevu = await _context.Randevular
            .Where(r => r.CalisanID == calisanID &&
                        (r.RandevuZamani < randevuBitisZamani && r.RandevuBitisZamani > randevuZamani))
            .FirstOrDefaultAsync();

        if (mevcutRandevu != null)
        {
            TempData["Error"] = "Seçtiğiniz çalışan bu saat aralığında başka bir randevuda.";
            return RedirectToAction("RandevuAl");
        }


                var ikiHaftaSonra = DateTime.Now.AddDays(14);
                if (randevuZamani < DateTime.Now || randevuZamani > ikiHaftaSonra)
                {
                    TempData["Error"] = "Randevu yalnızca sonraki 2 hafta için alınabilir.";
                    return RedirectToAction("RandevuAl");
                }

                // İşlem süresini kullanarak randevu bitiş zamanı hesaplama
                var randevu = new Randevu
                {
                    KullaniciID = kullanici.KullaniciID,
                    IslemID = islemID,
                    CalisanID = calisanID,
                    RandevuZamani = randevuZamani,
                    RandevuBitisZamani = randevuZamani.AddMinutes(islem.IslemSuresi),
                    Durum = "Bekliyor"
                };


                _context.Randevular.Add(randevu);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Randevunuz başarıyla alındı.";
                return RedirectToAction("RandevuAl");
            }

            // Model doğrulanamazsa işlemler ve çalışanları tekrar yükle
            ViewBag.Islemler = await _context.Islemler.ToListAsync();
            ViewBag.Calisanlar = await _context.Calisanlar.ToListAsync();
            TempData["Error"] = "Lütfen tüm alanları doldurun.";
            return View();
        }
        public async Task<IActionResult> Randevularim()
        {
            var kullaniciID = HttpContext.Session.GetInt32("KullaniciID");

            if (kullaniciID == null)
            {
                TempData["Error"] = "Randevularınızı görmek için giriş yapmalısınız.";
                return RedirectToAction("GirisYap");
            }

            var kullanici = await _context.Kullanicilar
                                           .FirstOrDefaultAsync(k => k.KullaniciID == kullaniciID);

            if (kullanici == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index");
            }

            // Randevuları veritabanından yeniden alıyoruz, böylece en güncel hali elde ediyoruz.
            var randevular = await _context.Randevular
                                           .Where(r => r.KullaniciID == kullanici.KullaniciID)
                                           .Include(r => r.Islem)
                                           .Include(r => r.Calisan)
                                           .ToListAsync();

            // Veriyi ViewBag'e aktaralım
            ViewBag.Randevular = randevular;

            return View();
        }




        public async Task<IActionResult> AdminPanel()
        {
            try
            {
                var mesajlar = _context.Mesaj.ToList(); // Örnek: Mesajlar, veri tabanındaki mesajlar tablosu
                ViewBag.Mesaj = mesajlar;

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

                // Verimlilik Hesaplama
                var aylikKazanc = calisanlar.Select(c => new
                {
                    CalisanAd = c.CalisanAd,
                    AylikKazanc = randevular
                        .Where(r => r.CalisanAd == c.CalisanAd && r.RandevuZamani.Month == DateTime.Now.Month)
                        .Sum(r =>
                        {
                            var islem = islemler.FirstOrDefault(i => i.IslemAd == r.IslemAd);
                            return islem != null ? islem.Ucret : 0;
                        })
                }).ToList();

                ViewBag.Islemler = islemler;
                ViewBag.Calisanlar = calisanlar;
                ViewBag.Kullanicilar = kullanicilar;
                ViewBag.Randevular = randevular;
                ViewBag.AylikKazanc = aylikKazanc;
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
        public async Task<IActionResult> CalisanEkleGuncelle(int? calisanID, string calisanAd, int uzmanlikID, bool musaitlik, string islem, int gunlukKazanc)
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
                if (randevu == null)
                {
                    TempData["Error"] = "Randevu bulunamadı.";
                    return RedirectToAction("AdminPanel", "Anasayfa");  // Admin paneline geri dön
                }

                // Duruma göre işlem yap
                if (islem == "onayla")
                {
                    randevu.Durum = "Onaylı";  // Onayla
                    TempData["Message"] = "Randevu onaylandı.";
                }
                else if (islem == "red")
                {
                    randevu.Durum = "Red";  // Red
                    TempData["Message"] = "Randevu reddedildi.";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("AdminPanel", "Anasayfa");  // Admin paneline yönlendirme
        }



    }
}
       