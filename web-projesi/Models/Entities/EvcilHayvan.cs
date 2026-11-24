using System.ComponentModel.DataAnnotations;

namespace web_projesi.Models.Entities;

// Üyelerin (hayvan sahiplerinin) sisteme kaydettiği evcil hayvanları tutan tablo
public class EvcilHayvan
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Evcil hayvanın adı zorunludur.")]
    [StringLength(60, ErrorMessage = "Hayvan adı en fazla 60 karakter olabilir.")]
    [Display(Name = "Hayvanın Adı")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hayvan türü (Kedi, Köpek vb.) zorunludur.")]
    [StringLength(40, ErrorMessage = "Tür en fazla 40 karakter olabilir.")]
    [Display(Name = "Türü")]
    public string Tur { get; set; } = string.Empty;

    [Required(ErrorMessage = "Irk bilgisi zorunludur.")]
    [StringLength(60, ErrorMessage = "Irk en fazla 60 karakter olabilir.")]
    [Display(Name = "Irkı")]
    public string Irk { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğum tarihi girilmelidir.")]
    [Display(Name = "Doğum Tarihi")]
    public DateOnly DogumTarihi { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-1));

    [Required(ErrorMessage = "Kilo bilgisi girilmelidir.")]
    [Range(0.1, 250.0, ErrorMessage = "Kilo 0.1 kg ile 250 kg arasında olmalıdır.")]
    [Display(Name = "Kilosu (kg)")]
    public double Kilo { get; set; }

    // Hayvanın sahibi olan üye (Foreign Key)
    [Required]
    [Display(Name = "Hayvan Sahibi")]
    public string SahipId { get; set; } = string.Empty;
    public Kullanici? Sahip { get; set; }

    // 1-Çok ilişki: Hayvanın aşı geçmişi ve takvimi
    public List<AsiKaydi> AsiKayitlari { get; set; } = new();

    // 1-Çok ilişki: Hayvanın randevu geçmişi
    public List<Randevu> Randevular { get; set; } = new();
}
