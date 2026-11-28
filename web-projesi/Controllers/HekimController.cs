using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web_projesi.Data;
using web_projesi.Models.Entities;
using web_projesi.Models.ViewModels;

namespace web_projesi.Controllers;

// Veteriner hekimlerin, uzmanlık/mesai saatlerinin ve verebildikleri hizmetlerin Admin tarafından yönetildiği Controller
[Authorize(Roles = "Admin")]
public class HekimController : Controller
{
    private readonly KlinikDbContext _db;
    private readonly UserManager<Kullanici> _kullaniciYoneticisi;

    public HekimController(KlinikDbContext db, UserManager<Kullanici> kullaniciYoneticisi)
    {
        _db = db;
        _kullaniciYoneticisi = kullaniciYoneticisi;
    }

    // Tüm veteriner hekimleri listele (Read)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var hekimler = await _db.Hekimler
            .Include(h => h.Kullanici)
            .Include(h => h.VerdigiHizmetler)
            .Include(h => h.Randevular)
            .OrderBy(h => h.AdSoyad)
            .ToListAsync();

        return View(hekimler);
    }

    // Hekim detayını, verdiği hizmetleri ve mesai bilgilerini görüntüle (Read - Detay)
    [HttpGet]
    public async Task<IActionResult> Detay(int id)
    {
        var hekim = await _db.Hekimler
            .Include(h => h.Kullanici)
            .Include(h => h.VerdigiHizmetler)
            .Include(h => h.Randevular)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hekim == null)
        {
            return NotFound();
        }

        return View(hekim);
    }

    // Yeni hekim ekleme formu (Create - GET)
    [HttpGet]
    public async Task<IActionResult> Ekle()
    {
        ViewBag.TumHizmetler = await _db.KlinikHizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();
        return View(new HekimFormModeli
        {
            MesaiBaslangic = new TimeOnly(9, 0),
            MesaiBitis = new TimeOnly(17, 0),
            HesapSifre = "sau"
        });
    }

    // Yeni hekim ve bağlı Veteriner kullanıcı hesabını oluşturma (Create - POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ekle(HekimFormModeli model)
    {
        if (model.MesaiBitis <= model.MesaiBaslangic)
        {
            ModelState.AddModelError("MesaiBitis", "Mesai bitiş saati başlangıç saatinden sonra olmalıdır.");
        }

        if (model.SecilenHizmetIdleri == null || model.SecilenHizmetIdleri.Count == 0)
        {
            ModelState.AddModelError("SecilenHizmetIdleri", "Hekimin verebildiği en az bir hizmet seçilmelidir.");
        }

        if (string.IsNullOrWhiteSpace(model.HesapEposta))
        {
            ModelState.AddModelError("HesapEposta", "Hekimin sisteme giriş yapabilmesi için e-posta adresi zorunludur.");
        }

        if (string.IsNullOrWhiteSpace(model.HesapSifre) || model.HesapSifre.Length < 3)
        {
            ModelState.AddModelError("HesapSifre", "Giriş şifresi en az 3 karakter olmalıdır.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.TumHizmetler = await _db.KlinikHizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();
            return View(model);
        }

        var eposta = model.HesapEposta!.Trim();
        if (await _kullaniciYoneticisi.FindByEmailAsync(eposta) != null)
        {
            ModelState.AddModelError("HesapEposta", "Bu e-posta adresi sistemde zaten kayıtlı.");
            ViewBag.TumHizmetler = await _db.KlinikHizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();
            return View(model);
        }

        // 1. Veteriner rolünde kullanıcı hesabını oluştur
        var yeniKullanici = new Kullanici
        {
            UserName = eposta,
            Email = eposta,
            AdSoyad = model.AdSoyad.Trim(),
            PhoneNumber = model.Telefon?.Trim(),
            EmailConfirmed = true
        };

        var hesapSonucu = await _kullaniciYoneticisi.CreateAsync(yeniKullanici, model.HesapSifre!);
        if (!hesapSonucu.Succeeded)
        {
            foreach (var hata in hesapSonucu.Errors)
            {
                ModelState.AddModelError(string.Empty, hata.Description);
            }
            ViewBag.TumHizmetler = await _db.KlinikHizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();
            return View(model);
        }

        await _kullaniciYoneticisi.AddToRoleAsync(yeniKullanici, "Veteriner");

        // 2. Seçilen hizmetleri getir ve Hekim kaydını oluştur
        var secilenHizmetler = await _db.KlinikHizmetler
            .Where(kh => model.SecilenHizmetIdleri!.Contains(kh.Id))
            .ToListAsync();

        var yeniHekim = new Hekim
        {
            AdSoyad = model.AdSoyad.Trim(),
            UzmanlikAlani = model.UzmanlikAlani.Trim(),
            Telefon = model.Telefon?.Trim(),
            MesaiBaslangic = model.MesaiBaslangic,
            MesaiBitis = model.MesaiBitis,
            KullaniciId = yeniKullanici.Id,
            VerdigiHizmetler = secilenHizmetler
        };

        _db.Hekimler.Add(yeniHekim);
        await _db.SaveChangesAsync();

        TempData["BilgiMesaji"] = $"'{yeniHekim.AdSoyad}' hekim kaydı ve '{eposta}' giriş hesabı oluşturuldu.";
        return RedirectToAction(nameof(Index));
    }

    // Hekim düzenleme formu (Update - GET)
    [HttpGet]
    public async Task<IActionResult> Duzenle(int id)
    {
        var hekim = await _db.Hekimler
            .Include(h => h.Kullanici)
            .Include(h => h.VerdigiHizmetler)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hekim == null)
        {
            return NotFound();
        }

        ViewBag.TumHizmetler = await _db.KlinikHizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();

        var model = new HekimFormModeli
        {
            Id = hekim.Id,
            AdSoyad = hekim.AdSoyad,
            UzmanlikAlani = hekim.UzmanlikAlani,
            Telefon = hekim.Telefon,
            MesaiBaslangic = hekim.MesaiBaslangic,
            MesaiBitis = hekim.MesaiBitis,
            HesapEposta = hekim.Kullanici?.Email,
            SecilenHizmetIdleri = hekim.VerdigiHizmetler.Select(vh => vh.Id).ToList()
        };

        return View(model);
    }

    // Hekim bilgilerini ve verdiği hizmetleri güncelleme (Update - POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Duzenle(int id, HekimFormModeli model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (model.MesaiBitis <= model.MesaiBaslangic)
        {
            ModelState.AddModelError("MesaiBitis", "Mesai bitiş saati başlangıç saatinden sonra olmalıdır.");
        }

        if (model.SecilenHizmetIdleri == null || model.SecilenHizmetIdleri.Count == 0)
        {
            ModelState.AddModelError("SecilenHizmetIdleri", "Hekimin verebildiği en az bir hizmet seçilmelidir.");
        }

        if (!string.IsNullOrWhiteSpace(model.HesapSifre) && model.HesapSifre.Length < 3)
        {
            ModelState.AddModelError("HesapSifre", "Yeni şifre girilecekse en az 3 karakter olmalıdır.");
        }

        var hekim = await _db.Hekimler
            .Include(h => h.Kullanici)
            .Include(h => h.VerdigiHizmetler)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hekim == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            model.HesapEposta = hekim.Kullanici?.Email;
            ViewBag.TumHizmetler = await _db.KlinikHizmetler.OrderBy(h => h.HizmetAdi).ToListAsync();
            return View(model);
        }

        var secilenHizmetler = await _db.KlinikHizmetler
            .Where(kh => model.SecilenHizmetIdleri!.Contains(kh.Id))
            .ToListAsync();

        hekim.AdSoyad = model.AdSoyad.Trim();
        hekim.UzmanlikAlani = model.UzmanlikAlani.Trim();
        hekim.Telefon = model.Telefon?.Trim();
        hekim.MesaiBaslangic = model.MesaiBaslangic;
        hekim.MesaiBitis = model.MesaiBitis;

        // Çoka-Çok hizmet listesini güncelle
        hekim.VerdigiHizmetler.Clear();
        hekim.VerdigiHizmetler.AddRange(secilenHizmetler);

        // Bağlı kullanıcı hesabı varsa adını ve (girildiyse) yeni şifresini güncelle
        if (hekim.Kullanici != null)
        {
            hekim.Kullanici.AdSoyad = hekim.AdSoyad;
            hekim.Kullanici.PhoneNumber = hekim.Telefon;
            await _kullaniciYoneticisi.UpdateAsync(hekim.Kullanici);

            if (!string.IsNullOrWhiteSpace(model.HesapSifre))
            {
                var sifirlamaTokeni = await _kullaniciYoneticisi.GeneratePasswordResetTokenAsync(hekim.Kullanici);
                await _kullaniciYoneticisi.ResetPasswordAsync(hekim.Kullanici, sifirlamaTokeni, model.HesapSifre);
            }
        }

        await _db.SaveChangesAsync();

        TempData["BilgiMesaji"] = $"'{hekim.AdSoyad}' bilgileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // Hekim silme onay ekranı (Delete - GET)
    [HttpGet]
    public async Task<IActionResult> Sil(int id)
    {
        var hekim = await _db.Hekimler
            .Include(h => h.Kullanici)
            .Include(h => h.VerdigiHizmetler)
            .Include(h => h.Randevular)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hekim == null)
        {
            return NotFound();
        }

        return View(hekim);
    }

    // Hekim silme işlemi (Delete - POST)
    [HttpPost, ActionName("Sil")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SilOnay(int id)
    {
        var hekim = await _db.Hekimler
            .Include(h => h.Kullanici)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hekim == null)
        {
            return NotFound();
        }

        bool randevusuVarMi = await _db.Randevular.AnyAsync(r => r.HekimId == id);
        if (randevusuVarMi)
        {
            TempData["HataMesaji"] = $"'{hekim.AdSoyad}' hekimine ait kayıtlı randevular bulunduğu için silinemez.";
            return RedirectToAction(nameof(Index));
        }

        var bagliKullanici = hekim.Kullanici;

        _db.Hekimler.Remove(hekim);
        await _db.SaveChangesAsync();

        // Bağlı hesap Admin değilse Veteriner giriş hesabını da temizle
        if (bagliKullanici != null && !await _kullaniciYoneticisi.IsInRoleAsync(bagliKullanici, "Admin"))
        {
            await _kullaniciYoneticisi.DeleteAsync(bagliKullanici);
        }

        TempData["BilgiMesaji"] = $"'{hekim.AdSoyad}' hekim kaydı sistemden silindi.";
        return RedirectToAction(nameof(Index));
    }
}
