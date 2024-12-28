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
using System.Drawing.Imaging;
using System.Drawing;



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

            return RedirectToAction("Iletisim");
        }




        // Anasayfa
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Index123()
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

        // GET: /Anasayfa/YapayZeka
        // GET: /YapayZeka
        public IActionResult YapayZeka()
        {
            return View();
        }

        // POST: /YapayZeka
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
                // Fotoğrafı Base64 formatına çevirme
                var base64String = await ConvertToBase64(photo);

                // Model için açıklama oluşturma
                string prompt = $"Bu fotoğraftaki kişi için saç stili önerisi ver. Fotoğraf (Base64 kodu): {base64String}";

                // API'ye istek gönder
                var response = await GetGPTModelResponse(prompt);

                // Modelden gelen cevabı al
                string suggestion = response?.Trim() ?? "Saç stili önerisi alınamadı.";

                // Sonucu ViewBag'e atama
                ViewBag.Suggestion = suggestion;

                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction("YapayZeka");
            }
        }

        // Fotoğrafı küçük boyuta indirip Base64 formatına çevirme
        private async Task<string> ConvertToBase64(IFormFile photo)
        {
            using (var memoryStream = new MemoryStream())
            {
                await photo.CopyToAsync(memoryStream);

                // Küçük bir boyuta indirgeme işlemi
                using (var image = Image.FromStream(new MemoryStream(memoryStream.ToArray())))
                {
                    var newWidth = 300; // Genişliği 300px olarak ayarlıyoruz
                    var newHeight = (int)(image.Height * (newWidth / (double)image.Width)); // Yüksekliği oranla ayarlıyoruz
                    var resizedImage = new Bitmap(image, newWidth, newHeight);

                    using (var resizedStream = new MemoryStream())
                    {
                        resizedImage.Save(resizedStream, ImageFormat.Jpeg); // JPEG formatında kaydediyoruz
                        return Convert.ToBase64String(resizedStream.ToArray());
                    }
                }
            }
        }

        // OpenAI API'ye istek gönderme
        private async Task<string> GetGPTModelResponse(string prompt)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer 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"); // Buraya kendi API anahtarınızı ekleyin.

            var requestBody = new
            {
                model = "gpt-3.5-turbo", // veya "gpt-4"
                messages = new[]
                {
                new { role = "system", content = "You are a helpful assistant." },
                new { role = "user", content = prompt }
            },
                max_tokens = 150
            };

            var response = await client.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception($"API isteği başarısız oldu: {errorMessage}");
            }

            // JSON yanıtını okuma
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.Linq.JObject.Parse(responseJson);
            var suggestion = result["choices"]?[0]?["message"]?["content"]?.ToString();

            return suggestion;
        }


        [HttpGet]
        public async Task<IActionResult> GetCalisanlar(int islemID)
        {
            try
            {
                // İşlem ID'sine göre, CalisanUzmanlik tablosundan çalışanları getirelim
                var calisanlar = await _context.CalisanUzmanliklar
                    .Where(cu => cu.IslemID == islemID)  // İlgili işlemi yapan çalışanlar
                    .Join(_context.Calisanlar, cu => cu.CalisanID, c => c.CalisanID, (cu, c) => new
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

                // Çalışanın kazancını güncelleme
                var calisan = await _context.Calisanlar.FirstOrDefaultAsync(c => c.CalisanID == calisanID);
                if (calisan != null)
                {
                    // Sadece randevu tarihi bugün ise günlük kazancı güncelle
                    if (randevuZamani.Date == DateTime.Today)
                    {
                        calisan.GunlukKazanc += islem.Ucret;
                    }
                    _context.Calisanlar.Update(calisan);
                    await _context.SaveChangesAsync();
                }
            }
         
            // Model doğrulanamazsa işlemler ve çalışanları tekrar yükle
            ViewBag.Islemler = await _context.Islemler.ToListAsync();
            ViewBag.Calisanlar = await _context.Calisanlar.ToListAsync();
            TempData["Error"] = "Lütfen tüm alanları doldurun.";
            return View();
        }

        public void GunlukKazanciSifirla()
        {
            using (var scope = _context.Database.BeginTransaction())
            {
                try
                {
                    var calisanlar = _context.Calisanlar.ToList();
                    foreach (var calisan in calisanlar)
                    {
                        calisan.GunlukKazanc = 0;
                    }
                    _context.SaveChanges();
                    scope.Commit();
                }
                catch (Exception ex)
                {
                    scope.Rollback();
                    // Loglama yapabilirsiniz: _logger.LogError(ex, "Günlük kazanç sıfırlama hatası");
                }
            }
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
            ViewBag.Uzmanliklar = await _context.Islemler.ToListAsync(); // Tüm işlemleri alıyoruz
            ViewBag.Calisanlar = await _context.Calisanlar.Include(c => c.Uzmanliklar)
                                                          .ThenInclude(cu => cu.Islem) // Çalışanın işlemlerini dahil ediyoruz
                                                          .ToListAsync();
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
                    // Yeni çalışan eklerken
                    var yeniCalisan = new Calisanlar
                    {
                        CalisanAd = calisanAd,
                        UzmanlikID = uzmanlikID,  // Valid Islem ID olmalı
                        Musaitlik = musaitlik,
                        GunlukKazanc = gunlukKazanc,
                    };

                    // Çalışan ekleme
                    _context.Calisanlar.Add(yeniCalisan);

                    // Eğer uzmanlık ID'si varsa, o uzmanlıkla ilgili işlemleri ekleyelim
                    if (uzmanlikID > 0)
                    {
                        var islemListesi = await _context.Islemler
                                                          .Where(i => i.IslemID == uzmanlikID)
                                                          .ToListAsync();

                        foreach (var islemItem in islemListesi)
                        {
                            var calisanUzmanlik = new CalisanUzmanlik
                            {
                                Calisan = yeniCalisan,
                                Islem = islemItem
                            };
                            _context.CalisanUzmanliklar.Add(calisanUzmanlik);
                        }
                    }
                }
                else if (islem == "guncelle" && calisanID.HasValue)
                {
                    // Çalışan güncelleme
                    var mevcutCalisan = await _context.Calisanlar.FindAsync(calisanID.Value);
                    if (mevcutCalisan != null)
                    {
                        mevcutCalisan.CalisanAd = calisanAd;
                        mevcutCalisan.UzmanlikID = uzmanlikID;
                        mevcutCalisan.Musaitlik = musaitlik;
                        mevcutCalisan.GunlukKazanc = gunlukKazanc;

                        // Uzmanlıkla ilgili işlemleri güncelle
                        // Önce eski ilişkileri temizleyelim
                        var mevcutUzmanliklar = await _context.CalisanUzmanliklar
                                                               .Where(cu => cu.CalisanID == calisanID.Value)
                                                               .ToListAsync();
                        _context.CalisanUzmanliklar.RemoveRange(mevcutUzmanliklar);

                        // Yeni ilişkileri ekleyelim
                        if (uzmanlikID > 0)
                        {
                            var yeniIslemListesi = await _context.Islemler
                                                                  .Where(i => i.IslemID == uzmanlikID)
                                                                  .ToListAsync();

                            foreach (var islemItem in yeniIslemListesi)
                            {
                                var calisanUzmanlik = new CalisanUzmanlik
                                {
                                    CalisanID = calisanID.Value,
                                    IslemID = islemItem.IslemID
                                };
                                _context.CalisanUzmanliklar.Add(calisanUzmanlik);
                            }
                        }
                    }
                }
                else if (islem == "sil" && calisanID.HasValue)
                {
                    // Çalışan silme
                    var calisanSil = await _context.Calisanlar.FindAsync(calisanID.Value);
                    if (calisanSil != null)
                    {
                        // Silme işleminde çalışanın ilişkili olduğu uzmanlıkları da silmeliyiz
                        var calisanUzmanliklar = await _context.CalisanUzmanliklar
                                                                .Where(cu => cu.CalisanID == calisanID.Value)
                                                                .ToListAsync();

                        _context.CalisanUzmanliklar.RemoveRange(calisanUzmanliklar);
                        _context.Calisanlar.Remove(calisanSil);
                    }
                }

                // Değişiklikleri kaydedelim
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