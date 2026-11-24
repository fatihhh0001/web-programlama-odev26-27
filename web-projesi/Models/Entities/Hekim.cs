using System.ComponentModel.DataAnnotations;

namespace web_projesi.Models.Entities;

// Klinikte görev yapan veteriner hekimleri, uzmanlıklarını ve çalışma saatlerini tutan tablo
public class Hekim
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
    [Display(Name = "Mesai Başlangıç")]
    public TimeOnly MesaiBaslangic { get; set; } = new TimeOnly(9, 0);

    [Required(ErrorMessage = "Mesai bitiş saati seçilmelidir.")]
    [Display(Name = "Mesai Bitiş")]
    public TimeOnly MesaiBitis { get; set; } = new TimeOnly(17, 0);

    // Veterinerin sisteme giriş yapıp kendi randevularını yönetebilmesi için Kullanıcı (Identity) bağlantısı
    [Display(Name = "Kullanıcı Hesabı")]
    public string? KullaniciId { get; set; }
    public Kullanici? Kullanici { get; set; }

    // Çoka-Çok ilişki: Hekimin verebildiği klinik hizmetleri
    [Display(Name = "Verebildiği Hizmetler")]
    public List<KlinikHizmet> VerdigiHizmetler { get; set; } = new();

    // 1-Çok ilişki: Hekime ait randevular
    public List<Randevu> Randevular { get; set; } = new();
}
