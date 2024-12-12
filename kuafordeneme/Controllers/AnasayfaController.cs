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
        public IActionResult AdminPanel()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                // İşlem tablosunu alıyoruz
                var islemCommand = new NpgsqlCommand("SELECT * FROM Islem", connection);
                var islemler = new List<Islemler>();
                using (var reader = islemCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        islemler.Add(new Islemler
                        {
                            IslemID = reader.GetInt32(0),
                            IslemAd = reader.GetString(1),
                            IslemSuresi = reader.GetInt32(2),
                            Ucret = reader.GetDecimal(3),
                            Tanim = reader.IsDBNull(4) ? null : reader.GetString(4)
                        });
                    }
                }

                // Çalışan tablosunu alıyoruz
                var calisanCommand = new NpgsqlCommand("SELECT * FROM Calisanlar", connection);
                var calisanlar = new List<Calisanlar>();
                using (var reader = calisanCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        calisanlar.Add(new Calisanlar
                        {
                            CalisanID = reader.GetInt32(0),
                            CalisanAd = reader.GetString(1),
                            UzmanlikID = reader.GetInt32(2),
                            Musaitlik = reader.GetBoolean(3)
                        });
                    }
                }

                // Randevuları alıyoruz
                var randevuCommand = new NpgsqlCommand(
                    "SELECT r.RandevuID, k.AdSoyad AS KullaniciAd, i.IslemAd, c.CalisanAd, r.RandevuZamani, r.Durum " +
                    "FROM Randevu r " +
                    "JOIN Kullanicilar k ON r.KullaniciID = k.KullaniciID " +
                    "JOIN Islem i ON r.IslemID = i.IslemID " +
                    "JOIN Calisanlar c ON r.CalisanID = c.CalisanID", connection);

                var randevular = new List<RandevuViewModel>();
                using (var reader = randevuCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        randevular.Add(new RandevuViewModel
                        {
                            RandevuID = reader.GetInt32(0),
                            KullaniciAd = reader.GetString(1),
                            IslemAd = reader.GetString(2),
                            CalisanAd = reader.GetString(3),
                            RandevuZamani = reader.GetDateTime(4),
                            Durum = reader.GetString(5)
                        });
                    }
                }

                // Kullanıcıları alıyoruz
                var kullaniciCommand = new NpgsqlCommand("SELECT * FROM Kullanicilar", connection);
                var kullanicilar = new List<Kullanicilar>();
                using (var reader = kullaniciCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        kullanicilar.Add(new Kullanicilar
                        {
                            KullaniciID = reader.GetInt32(0),
                            AdSoyad = reader.GetString(1),
                            Email = reader.GetString(2),
                            IsAdmin = reader.GetBoolean(4)
                        });
                    }
                }

                // ViewBag'e verileri aktarıyoruz
                ViewBag.Islemler = islemler;
                ViewBag.Calisanlar = calisanlar;
                ViewBag.Randevular = randevular;
                ViewBag.Kullanicilar = kullanicilar;
            }

            return View();
        }


        [HttpPost]
        public IActionResult IslemEkleGuncelle(int? islemID, string islemAd, int islemSuresi, decimal ucret, string tanim, string islem)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    if (islem == "ekle") // Yeni işlem ekleme
                    {
                        var command = new NpgsqlCommand(
                            "INSERT INTO Islem (IslemAd, IslemSuresi, Ucret, Tanim) VALUES (@IslemAd, @IslemSuresi, @Ucret, @Tanim)",
                            connection);

                        command.Parameters.AddWithValue("@IslemAd", islemAd ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IslemSuresi", islemSuresi);
                        command.Parameters.AddWithValue("@Ucret", ucret);
                        command.Parameters.AddWithValue("@Tanim", tanim ?? (object)DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                    else if (islem == "guncelle" && islemID.HasValue) // Güncelleme
                    {
                        var command = new NpgsqlCommand(
                            "UPDATE Islem SET IslemAd = @IslemAd, IslemSuresi = @IslemSuresi, Ucret = @Ucret, Tanim = @Tanim WHERE IslemID = @IslemID",
                            connection);

                        command.Parameters.AddWithValue("@IslemAd", islemAd ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IslemSuresi", islemSuresi);
                        command.Parameters.AddWithValue("@Ucret", ucret);
                        command.Parameters.AddWithValue("@Tanim", tanim ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IslemID", islemID.Value);

                        command.ExecuteNonQuery();
                    }
                    else if (islem == "sil" && islemID.HasValue) // Silme
                    {
                        var command = new NpgsqlCommand(
                            "DELETE FROM Islem WHERE IslemID = @IslemID",
                            connection);

                        command.Parameters.AddWithValue("@IslemID", islemID.Value);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bir hata oluştu: {ex.Message}");
            }

            return RedirectToAction("AdminPanel");
        }

        [HttpPost]
        public IActionResult CalisanEkleGuncelle(int? calisanID, string calisanAd, int uzmanlikID, bool musaitlik, string islem)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    if (islem == "ekle") // Yeni çalışan ekle
                    {
                        var command = new NpgsqlCommand(
                            "INSERT INTO Calisanlar (CalisanAd, UzmanlikID, Musaitlik) VALUES (@CalisanAd, @UzmanlikID, @Musaitlik)",
                            connection);

                        command.Parameters.AddWithValue("@CalisanAd", calisanAd);
                        command.Parameters.AddWithValue("@UzmanlikID", uzmanlikID);
                        command.Parameters.AddWithValue("@Musaitlik", musaitlik);
                        command.ExecuteNonQuery();
                    }
                    else if (islem == "guncelle" && calisanID.HasValue) // Güncelleme
                    {
                        var command = new NpgsqlCommand(
                            "UPDATE Calisanlar SET CalisanAd = @CalisanAd, UzmanlikID = @UzmanlikID, Musaitlik = @Musaitlik WHERE CalisanID = @CalisanID",
                            connection);

                        command.Parameters.AddWithValue("@CalisanAd", calisanAd);
                        command.Parameters.AddWithValue("@UzmanlikID", uzmanlikID);
                        command.Parameters.AddWithValue("@Musaitlik", musaitlik);
                        command.Parameters.AddWithValue("@CalisanID", calisanID.Value);
                        command.ExecuteNonQuery();
                    }
                    else if (islem == "sil" && calisanID.HasValue) // Silme
                    {
                        var command = new NpgsqlCommand(
                            "DELETE FROM Calisanlar WHERE CalisanID = @CalisanID",
                            connection);

                        command.Parameters.AddWithValue("@CalisanID", calisanID.Value);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bir hata oluştu: {ex.Message}");
            }

            return RedirectToAction("AdminPanel"); // Yönlendirme
        }

        [HttpPost]
        public IActionResult KullaniciEkleGuncelle(int? kullaniciID, string adSoyad, string email, string sifre, bool rol, string islem)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                if (islem == "ekle")
                {
                    // Yeni kullanıcı ekle
                    var command = new NpgsqlCommand(
                        "INSERT INTO Kullanicilar (AdSoyad, Email, Sifre, IsAdmin) VALUES (@AdSoyad, @Email, @Sifre, @IsAdmin)", connection);
                    command.Parameters.AddWithValue("@AdSoyad", adSoyad);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Sifre", sifre);
                    command.Parameters.AddWithValue("@IsAdmin", rol);
                    command.ExecuteNonQuery();
                }
                else if (islem == "guncelle" && kullaniciID.HasValue)
                {
                    // Kullanıcıyı güncelle
                    var command = new NpgsqlCommand(
                        "UPDATE Kullanicilar SET AdSoyad = @AdSoyad, Email = @Email, Sifre = @Sifre, IsAdmin = @IsAdmin WHERE KullaniciID = @KullaniciID", connection);
                    command.Parameters.AddWithValue("@AdSoyad", adSoyad);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Sifre", sifre);
                    command.Parameters.AddWithValue("@IsAdmin", rol);
                    command.Parameters.AddWithValue("@KullaniciID", kullaniciID.Value);
                    command.ExecuteNonQuery();
                }
                else if (islem == "sil" && kullaniciID.HasValue)
                {
                    // Kullanıcıyı sil
                    var command = new NpgsqlCommand("DELETE FROM Kullanicilar WHERE KullaniciID = @KullaniciID", connection);
                    command.Parameters.AddWithValue("@KullaniciID", kullaniciID.Value);
                    command.ExecuteNonQuery();
                }
            }

            return RedirectToAction("AdminPanel"); // İşlem tamamlandığında admin paneline yönlendir
        }



        [HttpPost]
        public IActionResult RandevuOnayla(int randevuID, string durum)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                var command = new NpgsqlCommand(
                    "UPDATE Randevu SET Durum = @Durum WHERE RandevuID = @RandevuID",
                    connection);

                command.Parameters.AddWithValue("@Durum", durum);  // "Onaylandı" veya "Reddedildi"
                command.Parameters.AddWithValue("@RandevuID", randevuID);

                command.ExecuteNonQuery();  // Sorguyu çalıştırıyoruz
            }

            return RedirectToAction("AdminPanel");  // Durum güncelleme sonrası Admin Paneline yönlendir
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

                    ViewData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                    return RedirectToAction("GirisYap"); // Kayıt işlemi başarılıysa giriş sayfasına yönlendirilir
                }
                catch (Exception ex)
                {
                    ViewData["Error"] = "Kayıt sırasında bir hata oluştu!";
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }

            return View();  // Hata varsa tekrar kayıt ol sayfasına yönlendirme
        }


    }

}
