using System.ComponentModel.DataAnnotations;

namespace web_projesi.Models.ViewModels;

// Kullanıcı giriş ekranı için form doğrulama modeli
public class GirisModeli
{
    [Required(ErrorMessage = "E-posta adresi giriniz.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-Posta Adresi")]
    public string Eposta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifrenizi giriniz.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Display(Name = "Beni Hatırla")]
    public bool BeniHatirla { get; set; }
}

// Yeni hayvan sahibi (Üye) kayıt ekranı için form doğrulama modeli
public class KayitModeli
{
    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad Soyad 3 ile 100 karakter arasında olmalıdır.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-Posta Adresi")]
    public string Eposta { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(25)]
    [Display(Name = "Telefon Numarası")]
    public string? Telefon { get; set; }

    [Required(ErrorMessage = "Şifre alanı zorunludur.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Şifre en az 3 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Sifre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare("Sifre", ErrorMessage = "Girilen şifreler birbiriyle eşleşmiyor.")]
    [Display(Name = "Şifre Tekrar")]
    public string SifreTekrar { get; set; } = string.Empty;
}
