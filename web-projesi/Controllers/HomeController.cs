using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_projesi.Data;
using web_projesi.Models;

namespace web_projesi.Controllers;

public class HomeController : Controller
{
    private readonly KlinikDbContext _db;

    public HomeController(KlinikDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        // Ana sayfada kliniğin sunduğu hizmetleri ve görevli hekimleri veritabanından çekiyoruz
        ViewBag.Hizmetler = await _db.KlinikHizmetler
            .OrderBy(h => h.HizmetAdi)
            .ToListAsync();

        ViewBag.Hekimler = await _db.Hekimler
            .Include(h => h.VerdigiHizmetler)
            .OrderBy(h => h.AdSoyad)
            .ToListAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
