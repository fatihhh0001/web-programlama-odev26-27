using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using web_projesi.Models.Entities;
using web_projesi.Models.ViewModels;

namespace web_projesi.Controllers;

// Kullanıcı giriş, yeni üye kaydı ve çıkış işlemlerini yöneten Controller
public class HesapController : Controller
{
    private readonly UserManager<Kullanici> _kullaniciYoneticisi;
    private readonly SignInManager<Kullanici> _oturumYoneticisi;

    public HesapController(
        UserManager<Kullanici> kullaniciYoneticisi,
        SignInManager<Kullanici> oturumYoneticisi)
    {
        _kullaniciYoneticisi = kullaniciYoneticisi;
        _oturumYoneticisi = oturumYoneticisi;
    }

    [HttpGet]
    public IActionResult Giris(string? returnUrl = null)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new GirisModeli());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Giris(GirisModeli model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var kullanici = await _kullaniciYoneticisi.FindByEmailAsync(model.Eposta);
        if (kullanici == null)
        {
            ModelState.AddModelError(string.Empty, "E-posta adresi veya şifre hatalı.");
            return View(model);
        }

        var sonuc = await _oturumYoneticisi.PasswordSignInAsync(
            kullanici.UserName!,
            model.Sifre,
            model.BeniHatirla,
            lockoutOnFailure: false);

        if (sonuc.Succeeded)
        {
            TempData["BilgiMesaji"] = $"Hoş geldiniz, {kullanici.AdSoyad}!";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(string.Empty, "E-posta adresi veya şifre hatalı.");
        return View(model);
    }

    [HttpGet]
    public IActionResult Kayit()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new KayitModeli());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kayit(KayitModeli model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var mevcutKullanici = await _kullaniciYoneticisi.FindByEmailAsync(model.Eposta);
        if (mevcutKullanici != null)
        {
            ModelState.AddModelError("Eposta", "Bu e-posta adresi zaten sistemde kayıtlı.");
            return View(model);
        }

        var yeniUye = new Kullanici
        {
            UserName = model.Eposta,
            Email = model.Eposta,
            AdSoyad = model.AdSoyad,
            PhoneNumber = model.Telefon,
            EmailConfirmed = true,
            KayitTarihi = DateTime.UtcNow
        };

        var olusturmaSonucu = await _kullaniciYoneticisi.CreateAsync(yeniUye, model.Sifre);
        if (olusturmaSonucu.Succeeded)
        {
            // Kayıt olan her yeni kullanıcı varsayılan olarak "Uye" (hayvan sahibi) rolünü alır
            await _kullaniciYoneticisi.AddToRoleAsync(yeniUye, "Uye");
            await _oturumYoneticisi.SignInAsync(yeniUye, isPersistent: false);

            TempData["BilgiMesaji"] = "Üyelik kaydınız başarıyla oluşturuldu.";
            return RedirectToAction("Index", "Home");
        }

        foreach (var hata in olusturmaSonucu.Errors)
        {
            ModelState.AddModelError(string.Empty, hata.Description);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cikis()
    {
        await _oturumYoneticisi.SignOutAsync();
        TempData["BilgiMesaji"] = "Oturumunuz güvenli bir şekilde kapatıldı.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ErisimEngellendi()
    {
        return View();
    }
}
