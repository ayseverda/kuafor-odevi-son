using Microsoft.AspNetCore.Mvc;
using Npgsql; // PostgreSQL bağlantısı için
using System;

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

        // Randevu Al
        public IActionResult RandevuAl()
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
            // Kullanıcıyı veritabanından kontrol etme
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
                            if (reader.Read()) // Kullanıcı veritabanında varsa
                            {
                                var dbSifre = reader["Sifre"].ToString();

                                // Şifreyi doğrulama (eğer şifre doğruysa giriş yapılır)
                                if (dbSifre == sifre) // Şifre doğruysa
                                {
                                    var isAdmin = Convert.ToBoolean(reader["IsAdmin"]);
                                    TempData["UserRole"] = isAdmin ? "Admin" : "User"; // Admin mi kullanıcı mı olduğunu kontrol ediyoruz
                                    return RedirectToAction(isAdmin ? "AdminPanel" : "Index"); // Admin veya kullanıcı yönlendirmesi
                                }
                                else
                                {
                                    // Şifre yanlışsa hata mesajı ver
                                    TempData["Error"] = "Geçersiz şifre.";
                                }
                            }
                            else
                            {
                                // E-posta yanlışsa hata mesajı ver
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

            return View(); // Hatalı giriş olursa tekrar aynı sayfaya yönlendiriyoruz
        }


        // Admin Paneline Yönlendirme
        public IActionResult AdminPanel()
        {
            if (TempData["UserRole"]?.ToString() != "Admin")
            {
                return RedirectToAction("GirisYap"); // Admin değilse giriş sayfasına yönlendir
            }

            return View();
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

            // Kullanıcıyı veritabanına kaydediyoruz
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand("INSERT INTO Kullanicilar (AdSoyad, Email, Sifre) VALUES (@AdSoyad, @Email, @Sifre)", connection))
                    {
                        command.Parameters.AddWithValue("@AdSoyad", adSoyad);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Sifre", sifre);
                        command.ExecuteNonQuery();
                    }

                    TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                    return RedirectToAction("GirisYap");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Kayıt sırasında bir hata oluştu!";
                    Console.WriteLine($"Hata: {ex.Message}");
                }
            }

            return View();
        }
    }
}
