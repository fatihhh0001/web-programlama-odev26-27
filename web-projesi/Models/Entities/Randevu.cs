using System.ComponentModel.DataAnnotations;

namespace web_projesi.Models.Entities;

public enum RandevuDurumu
{
    [Display(Name = "Bekliyor")]
    Bekliyor = 0,

    [Display(Name = "Onaylandı")]
    Onaylandi = 1,

    [Display(Name = "Reddedildi")]
    Reddedildi = 2,

    [Display(Name = "Tamamlandı")]
    Tamamlandi = 3
}

// Hayvan, Hekim ve Hizmeti belirli bir tarih/saatte birleştiren randevu tablosu
public class Randevu
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Evcil hayvan seçilmelidir.")]
    [Display(Name = "Evcil Hayvan")]
    public int EvcilHayvanId { get; set; }
    public EvcilHayvan? EvcilHayvan { get; set; }

    [Required(ErrorMessage = "Veteriner hekim seçilmelidir.")]
    [Display(Name = "Veteriner Hekim")]
    public int HekimId { get; set; }
    public Hekim? Hekim { get; set; }

    [Required(ErrorMessage = "Alınacak hizmet seçilmelidir.")]
    [Display(Name = "Hizmet")]
    public int KlinikHizmetId { get; set; }
    public KlinikHizmet? KlinikHizmet { get; set; }

    [Required(ErrorMessage = "Randevu tarih ve saati seçilmelidir.")]
    [Display(Name = "Başlangıç Zamanı")]
    public DateTime BaslangicZamani { get; set; }

    // Başlangıç zamanına seçilen hizmetin süresi eklenerek hesaplanır (çakışma kontrolü için)
    [Display(Name = "Bitiş Zamanı")]
    public DateTime BitisZamani { get; set; }

    [Display(Name = "Hizmet Süresi (Dk)")]
    public int SureDakika { get; set; }

    [Display(Name = "Hizmet Ücreti (TL)")]
    public decimal Ucret { get; set; }

    [Display(Name = "Randevu Durumu")]
    public RandevuDurumu Durum { get; set; } = RandevuDurumu.Bekliyor;

    [StringLength(600, ErrorMessage = "Muayene notu en fazla 600 karakter olabilir.")]
    [Display(Name = "Hekim Muayene Notu")]
    public string? MuayeneNotu { get; set; }
}
