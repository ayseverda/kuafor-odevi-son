using kuafordeneme.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql; // PostgreSQL bağlantısı için
using System;
using System.Collections.Generic;

namespace kuafordeneme.Controllers
{
    public class AnasayfaController : Controller
    {
        private readonly string _connectionString = "Host=localhost;Port=5432;Database=yenidb;Username=postgres;Password=123456;"; // Bağlantı dizesi

        // Anasayfa
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult RandevuAl()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // İşlemleri al
                var islemCommand = new NpgsqlCommand("SELECT * FROM Islem", connection);
                var islemler = new List<Islemler>();
                using (var reader = islemCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        islemler.Add(new Islemler
                        {
                            IslemID = reader.GetInt32(0),
                            IslemAd = reader.GetString(1)
                        });
                    }
                }
                ViewBag.Islemler = islemler;

                // Çalışanları al ve sadece müsait olanları göster
                var calisanCommand = new NpgsqlCommand("SELECT * FROM Calisanlar WHERE Musaitlik = TRUE", connection);
                var calisanlar = new List<Calisanlar>();
                using (var reader = calisanCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        calisanlar.Add(new Calisanlar
                        {
                            CalisanID = reader.GetInt32(0),
                            CalisanAd = reader.GetString(1)
                        });
                    }
                }
                ViewBag.Calisanlar = calisanlar;
            }

            return View();
        }

        [HttpPost]
        public IActionResult RandevuAl(string adSoyad, int islemID, int calisanID, DateTime randevuZamani)
        {
            var kullaniciID = 1; // Bu, o anki oturumdaki kullanıcının ID'si olmalı.
            var islemSuresi = 30; // Bu değeri işlemin süresine göre dinamik almak gerekebilir.

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // Çalışanın müsait olup olmadığını kontrol et
                var checkCommand = new NpgsqlCommand("SELECT Musaitlik FROM Calisanlar WHERE CalisanID = @CalisanID", connection);
                checkCommand.Parameters.AddWithValue("@CalisanID", calisanID);
                var calisanMüsait = Convert.ToBoolean(checkCommand.ExecuteScalar());

                if (!calisanMüsait)
                {
                    TempData["Error"] = "Bu çalışan o saatte müsait değil!";
                    return RedirectToAction("RandevuAl");
                }

                // Randevu bitiş zamanını hesapla (randevu süresi kadar)
                DateTime randevuBitisZamani = randevuZamani.AddMinutes(islemSuresi);

                // Randevu kaydetme
                var insertCommand = new NpgsqlCommand("INSERT INTO Randevu (KullaniciID, IslemID, CalisanID, RandevuZamani, RandevuBitisZamani, Durum) VALUES (@KullaniciID, @IslemID, @CalisanID, @RandevuZamani, @RandevuBitisZamani, 'Bekliyor')", connection);
                insertCommand.Parameters.AddWithValue("@KullaniciID", kullaniciID);
                insertCommand.Parameters.AddWithValue("@IslemID", islemID);
                insertCommand.Parameters.AddWithValue("@CalisanID", calisanID);
                insertCommand.Parameters.AddWithValue("@RandevuZamani", randevuZamani);
                insertCommand.Parameters.AddWithValue("@RandevuBitisZamani", randevuBitisZamani);

                insertCommand.ExecuteNonQuery();

                // Çalışan müsaitlik durumunu değiştirme (Randevu alındığında)
                var updateCommand = new NpgsqlCommand("UPDATE Calisanlar SET Musaitlik = FALSE WHERE CalisanID = @CalisanID", connection);
                updateCommand.Parameters.AddWithValue("@CalisanID", calisanID);
                updateCommand.ExecuteNonQuery();

                TempData["Success"] = "Randevunuz başarıyla alındı!";
            }

            return RedirectToAction("RandevuAl");
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

        [HttpPost]
        public IActionResult MesajGonder(string adSoyad, string email, string konu, string mesaj)
        {
            if (string.IsNullOrEmpty(adSoyad) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(konu) || string.IsNullOrEmpty(mesaj))
            {
                TempData["Error"] = "Tüm alanları doldurmanız gerekiyor!";
                return RedirectToAction("Index");
            }

            // Mesajı veritabanına ekleme
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Veritabanına bağlantı başarılı!");

                    // Mesajı veritabanına ekliyoruz
                    using (var command = new NpgsqlCommand("INSERT INTO Mesajlar (MusteriAd, Email, Konu, Aciklama) VALUES (@MusteriAd, @Email, @Konu, @Aciklama)", connection))
                    {
                        // Parametreleri ekliyoruz
                        command.Parameters.AddWithValue("@MusteriAd", adSoyad);   // Ad Soyad
                        command.Parameters.AddWithValue("@Email", email);         // E-posta
                        command.Parameters.AddWithValue("@Konu", konu);           // Konu
                        command.Parameters.AddWithValue("@Aciklama", mesaj);      // Mesaj içeriği
                        command.ExecuteNonQuery();  // Sorguyu çalıştırıyoruz
                    }

                    TempData["Success"] = "Mesajınız başarıyla gönderildi!";
                }
                catch (Exception ex)
                {
                    // Hata mesajını daha ayrıntılı şekilde yazdırıyoruz
                    TempData["Error"] = "Mesaj gönderilirken bir hata oluştu!";
                    Console.WriteLine($"Mesaj gönderme hatası: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");  // Hata izi (stack trace) ekliyoruz
                }
            }

            return RedirectToAction("Index");  // Ana sayfaya yönlendirme
        }
        // Giriş Yapma
        public IActionResult GirisYap()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GirisYap(string email, string sifre)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand("SELECT * FROM Kullanicilar WHERE Email = @Email", connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read()) // Kullanıcı bulunduysa
                            {
                                var dbSifre = reader["Sifre"].ToString();
                                if (dbSifre == sifre) // Şifre doğruysa
                                {
                                    // Kullanıcı bilgilerini session'a kaydet
                                    HttpContext.Session.SetInt32("KullaniciID", Convert.ToInt32(reader["KullaniciID"]));
                                    HttpContext.Session.SetString("AdSoyad", reader["AdSoyad"].ToString());
                                    HttpContext.Session.SetString("Email", reader["Email"].ToString());

                                    var isAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                                    if (isAdmin)
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
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Bir hata oluştu!";
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }

            return View(); // Hatalı girişte aynı sayfaya döner
        }

        public IActionResult CikisYap()
        {
            HttpContext.Session.Clear(); // Tüm session bilgilerini temizle
            return RedirectToAction("GirisYap", "Anasayfa");
        }


        // Admin Paneline Yönlendirme
        public IActionResult AdminPanel()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Admin")
            {
                TempData["Error"] = "Admin yetkisine sahip değilsiniz.";
                return RedirectToAction("GirisYap");
            }

            return View(); // Admin panel sayfası
        }

        // Kullanıcı Kaydı
        public IActionResult KayitOl()
        {
            return View();
        }

        [HttpPost]
        public IActionResult KayitOl(string adSoyad, string email, string sifre, string sifreOnay)
        {
            if (sifre != sifreOnay)
            {
                TempData["Error"] = "Şifreler uyuşmuyor!";
                return View();
            }

            // Kullanıcıyı veritabanına ekleme
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var command = new NpgsqlCommand("INSERT INTO Kullanicilar (AdSoyad, Email, Sifre, IsAdmin) VALUES (@AdSoyad, @Email, @Sifre, @IsAdmin)", connection);

                    // Parametreleri ekliyoruz
                    command.Parameters.AddWithValue("@AdSoyad", adSoyad);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Sifre", sifre);  // Şifreyi düz metinle kaydediyoruz, güvenlik için hashlenmiş olmalı
                    command.Parameters.AddWithValue("@IsAdmin", false);  // Varsayılan olarak kullanıcılar admin değil

                    // Veritabanına kaydediyoruz
                    command.ExecuteNonQuery();

                    TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                    return RedirectToAction("GirisYap"); // Kayıt işlemi başarılıysa giriş sayfasına yönlendirilir
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Kayıt sırasında bir hata oluştu!";
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }

            return View();  // Hata varsa tekrar kayıt ol sayfasına yönlendirme
        }


    }

}
