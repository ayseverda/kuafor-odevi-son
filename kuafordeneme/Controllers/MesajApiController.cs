using Microsoft.AspNetCore.Mvc;
using kuafordeneme.Models;
using System.Linq;
using System;
using kuafordeneme.Data;

namespace kuafordeneme.Controllers
{
    [Route("api/mesaj")]
    [ApiController]
    public class MesajlarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MesajlarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/mesajlar
        [HttpGet]
        public IActionResult GetMesajlar()
        {
            var mesajlar = _context.Mesaj
                .Select(m => new
                {
                    m.MesajID,
                    m.MusteriAd,
                    m.Email,
                    m.Konu,
                    m.Aciklama,
                    m.Tarih
                }).ToList();

            return Ok(mesajlar);
        }
    }
}
