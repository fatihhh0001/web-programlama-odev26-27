using System.ComponentModel.DataAnnotations;

namespace web_projesi.Models.ViewModels;

// Veteriner hekim ekleme ve düzenleme ekranlarında kullanılan form modeli
public class HekimFormModeli
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Hekim adı ve soyadı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Hekim Adı Soyadı")]
    public string AdSoyad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uzmanlık alanı belirtilmelidir.")]
    [StringLength(120, ErrorMessage = "Uzmanlık alanı en fazla 120 karakter olabilir.")]
    [Display(Name = "Uzmanlık Alanı")]
    public string UzmanlikAlani { get; set; } = string.Empty;

    [StringLength(25)]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [Required(ErrorMessage = "Mesai başlangıç saati seçilmelidir.")]
    [Display(Name = "Mesai Başlangıç Saati")]
    public TimeOnly MesaiBaslangic { get; set; } = new TimeOnly(9, 0);

    [Required(ErrorMessage = "Mesai bitiş saati seçilmelidir.")]
    [Display(Name = "Mesai Bitiş Saati")]
    public TimeOnly MesaiBitis { get; set; } = new TimeOnly(17, 0);

    [Display(Name = "Verebildiği Hizmetler")]
    public List<int> SecilenHizmetIdleri { get; set; } = new();

    // Hekimin sisteme "Veteriner" rolüyle giriş yapacağı hesap bilgileri
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "Giriş E-Posta Adresi")]
    public string? HesapEposta { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Giriş Şifresi")]
    public string? HesapSifre { get; set; }
}
