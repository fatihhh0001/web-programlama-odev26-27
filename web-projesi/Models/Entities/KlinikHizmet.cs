using System.ComponentModel.DataAnnotations;

namespace web_projesi.Models.Entities;

// Kliniğin sunduğu muayene, aşı, kısırlaştırma, tıraş gibi hizmetleri tutan tablo
public class KlinikHizmet
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Hizmet adı boş bırakılamaz.")]
    [StringLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olmalıdır.")]
    [Display(Name = "Hizmet Adı")]
    public string HizmetAdi { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
    [Display(Name = "Hizmet Açıklaması")]
    public string? Aciklama { get; set; }

    [Required(ErrorMessage = "Hizmet süresi (dakika) girilmelidir.")]
    [Range(10, 360, ErrorMessage = "Hizmet süresi 10 ile 360 dakika arasında olmalıdır.")]
    [Display(Name = "Süre (Dakika)")]
    public int SureDakika { get; set; } = 30;

    [Required(ErrorMessage = "Hizmet ücreti girilmelidir.")]
    [Range(0, 100000, ErrorMessage = "Ücret 0 ile 100.000 TL arasında olmalıdır.")]
    [Display(Name = "Ücret (TL)")]
    public decimal Ucret { get; set; }

    // Çoka-Çok ilişki: Bu hizmeti verebilen veteriner hekimler
    public List<Hekim> Hekimler { get; set; } = new();

    // 1-Çok ilişki: Bu hizmet için alınan randevular
    public List<Randevu> Randevular { get; set; } = new();
}
