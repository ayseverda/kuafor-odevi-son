using kuafordeneme.Data;
using kuafordeneme.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Collections.Generic;
using System.Linq;


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



        [HttpGet]
        public IActionResult RandevuAl()
        {
            // İşlemleri al
            var islemler = _context.Islemler.ToList();
            ViewBag.Islemler = islemler;

            // Çalışanları al ve sadece müsait olanları göster
            var calisanlar = _context.Calisanlar.Where(c => c.Musaitlik).ToList();
            ViewBag.Calisanlar = calisanlar;

            return View();
        }
            [HttpPost]
            public IActionResult RandevuAl(string adSoyad, int islemID, int calisanID, DateTime randevuZamani)
            {
                var kullaniciID = 1; // Bu, o anki oturumdaki kullanıcının ID'si olmalı.
                var islemSuresi = 30; // Bu değeri işlemin süresine göre dinamik almak gerekebilir.

                var calisan = _context.Calisanlar.FirstOrDefault(c => c.CalisanID == calisanID);

                if (calisan == null || !calisan.Musaitlik)
                {
                    TempData["Error"] = "Bu çalışan o saatte müsait değil!";
                    return RedirectToAction("RandevuAl");
                }

                // Randevu bitiş zamanını hesapla (randevu süresi kadar)
                DateTime randevuBitisZamani = randevuZamani.AddMinutes(islemSuresi);

                // Yeni randevu oluştur
                var randevu = new Randevu
                {
                    KullaniciID = kullaniciID,
                    IslemID = islemID,
                    CalisanID = calisanID,
                    RandevuZamani = randevuZamani,
                    RandevuBitisZamani = randevuBitisZamani,
                    Durum = "Bekliyor"
                };

                // Randevuyu veritabanına kaydet
                _context.Randevular.Add(randevu);

                // Çalışanın müsaitlik durumunu değiştirme (Randevu alındığında)
                calisan.Musaitlik = false;

                // Değişiklikleri kaydet
                _context.SaveChanges();

                TempData["Success"] = "Randevunuz başarıyla alındı!";
                return RedirectToAction("RandevuAl");
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
        public async Task<IActionResult> CalisanEkleGuncelle(int? calisanID, string calisanAd, int uzmanlikID, bool musaitlik, string islem)
        {
            try
            {
                if (islem == "ekle")
                {
                    var yeniCalisan = new Calisanlar
                    {
                        CalisanAd = calisanAd,
                        UzmanlikID = uzmanlikID,
                        Musaitlik = musaitlik
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
       