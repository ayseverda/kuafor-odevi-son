using Microsoft.AspNetCore.Mvc;

namespace kuafordeneme.Controllers
{
    public class AnasayfaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RandevuAl()
        {
            return View();

        }

        public IActionResult Hizmetlerimiz()
        {
            return View();

        }




        public IActionResult GirisYap()
        {
            return View();
        }

        // POST: Giriş yapma işlemi
        [HttpPost]
        public async Task<IActionResult> GirisYap(string email, string sifre)
        {
            // Statik admin bilgileri
            var adminEmail = "ayseeverda@gmail.com";
            var adminPassword = "1";
            var userEmail = "verdagulcemal@gmail.com";
            var userPassword = "1";

            // Admin kontrolü
            if (email == adminEmail && sifre == adminPassword)
            {
                // Admin girişi yapıldığında yetkilendirme yapılacak
                TempData["UserRole"] = "Admin"; // Admin olarak giriş yaptı
                return RedirectToAction("AdminPanel"); // Admin paneline yönlendir
            }

            // Kullanıcı kontrolü
            if (email == userEmail && sifre == userPassword)
            {
                // Kullanıcı girişi yapıldığında normal kullanıcı yönlendirilecek
                TempData["UserRole"] = "User"; // Kullanıcı olarak giriş yaptı
                return RedirectToAction("Index", "Anasayfa"); // Anasayfaya yönlendir
            }

            // Hatalı giriş
            TempData["Error"] = "Geçersiz e-posta veya şifre.";
            return View(); // Hata mesajıyla aynı sayfaya geri dön
        }

        // Admin paneline yönlendirme
        public IActionResult AdminPanel()
        {
            if (TempData["UserRole"]?.ToString() != "Admin")
            {
                return RedirectToAction("GirisYap"); // Admin değilse giriş sayfasına yönlendir
            }

            return View(); // Admin panelini göster
        }

            // Kullanıcı kaydı sayfası
            public IActionResult KayitOl()
            {
                return View();
            }

            // Kayıt olma işlemi
            [HttpPost]
            public IActionResult KayitOl(string email, string sifre, string sifreOnay)
            {
                // Şifre onayı kontrolü
                if (sifre != sifreOnay)
                {
                    TempData["Error"] = "Şifreler uyuşmuyor!";
                    return View();
                }

                // Statik kullanıcı verisi eklenebilir (örneğin veritabanına eklenebilir)
                TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction("GirisYap"); // Kayıt başarılıysa giriş sayfasına yönlendir
            }
        }
    }

