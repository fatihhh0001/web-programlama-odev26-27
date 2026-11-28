using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_projesi.Data;
using web_projesi.Models.Entities;

namespace web_projesi.Controllers;

// Kliniğin sunduğu hizmetlerin (muayene, aşı, kısırlaştırma vb.) Admin tarafından yönetildiği Controller
[Authorize(Roles = "Admin")]
public class HizmetController : Controller
{
    private readonly KlinikDbContext _db;

    public HizmetController(KlinikDbContext db)
    {
        _db = db;
    }

    // Tüm hizmetleri listele (Read)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var hizmetler = await _db.KlinikHizmetler
            .Include(h => h.Hekimler)
            .Include(h => h.Randevular)
            .OrderBy(h => h.HizmetAdi)
            .ToListAsync();

        return View(hizmetler);
    }

    // Hizmet detayını ve bu hizmeti veren hekimleri görüntüle (Read - Detay)
    [HttpGet]
    public async Task<IActionResult> Detay(int id)
    {
        var hizmet = await _db.KlinikHizmetler
            .Include(h => h.Hekimler)
            .Include(h => h.Randevular)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hizmet == null)
        {
            return NotFound();
        }

        return View(hizmet);
    }

    // Yeni hizmet ekleme formu (Create - GET)
    [HttpGet]
    public IActionResult Ekle()
    {
        return View(new KlinikHizmet { SureDakika = 30, Ucret = 500m });
    }

    // Yeni hizmet kaydetme (Create - POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(KlinikHizmet model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ayniIsimVarMi = await _db.KlinikHizmetler
            .AnyAsync(h => h.HizmetAdi.ToLower() == model.HizmetAdi.Trim().ToLower());

        if (ayniIsimVarMi)
        {
            ModelState.AddModelError("HizmetAdi", "Bu isimde bir klinik hizmeti zaten tanımlı.");
            return View(model);
        }

        model.HizmetAdi = model.HizmetAdi.Trim();
        model.Aciklama = model.Aciklama?.Trim();

        _db.KlinikHizmetler.Add(model);
        await _db.SaveChangesAsync();

        TempData["BilgiMesaji"] = $"'{model.HizmetAdi}' hizmeti başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // Hizmet düzenleme formu (Update - GET)
    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var hizmet = await _db.KlinikHizmetler.FindAsync(id);
        if (hizmet == null)
        {
            return NotFound();
        }

        return View(hizmet);
    }

    // Hizmet güncelleme (Update - POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, KlinikHizmet model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var mevcut = await _db.KlinikHizmetler.FindAsync(id);
        if (mevcut == null)
        {
            return NotFound();
        }

        var ayniIsimVarMi = await _db.KlinikHizmetler
            .AnyAsync(h => h.Id != id && h.HizmetAdi.ToLower() == model.HizmetAdi.Trim().ToLower());

        if (ayniIsimVarMi)
        {
            ModelState.AddModelError("HizmetAdi", "Bu isimde başka bir klinik hizmeti zaten mevcut.");
            return View(model);
        }

        mevcut.HizmetAdi = model.HizmetAdi.Trim();
        mevcut.Aciklama = model.Aciklama?.Trim();
        mevcut.SureDakika = model.SureDakika;
        mevcut.Ucret = model.Ucret;

        await _db.SaveChangesAsync();

        TempData["BilgiMesaji"] = $"'{mevcut.HizmetAdi}' hizmeti güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Hizmet silme onay ekranı (Delete - GET)
    [HttpGet]
    public async Task<IActionResult> Sil(int id)
    {
        var hizmet = await _db.KlinikHizmetler
            .Include(h => h.Hekimler)
            .Include(h => h.Randevular)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hizmet == null)
        {
            return NotFound();
        }

        return View(hizmet);
    }

    // Hizmet silme işlemi (Delete - POST)
    [HttpPost, ActionName("Sil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SilOnay(int id)
    {
        var hizmet = await _db.KlinikHizmetler.FindAsync(id);
        if (hizmet == null)
        {
            return NotFound();
        }

        // Randevusu olan bir hizmet silinirse geçmiş randevu kayıtları bozulmasın diye kontrol ediyoruz
        bool randevusuVarMi = await _db.Randevular.AnyAsync(r => r.KlinikHizmetId == id);
        if (randevusuVarMi)
        {
            TempData["HataMesaji"] = $"'{hizmet.HizmetAdi}' hizmetine ait kayıtlı randevular bulunduğu için silinemez.";
            return RedirectToAction(nameof(Index));
        }

        _db.KlinikHizmetler.Remove(hizmet);
        await _db.SaveChangesAsync();

        TempData["BilgiMesaji"] = $"'{hizmet.HizmetAdi}' hizmeti sistemden silindi.";
        return RedirectToAction(nameof(Index));
    }
}
