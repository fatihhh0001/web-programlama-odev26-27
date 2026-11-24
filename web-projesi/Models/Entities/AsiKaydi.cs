using System.ComponentModel.DataAnnotations;

namespace web_projesi.Models.Entities;

// Evcil hayvanların yapılan ve gelecekte yapılacak aşılarını tutan tablo
public class AsiKaydi
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Evcil hayvan seçilmelidir.")]
    [Display(Name = "Evcil Hayvan")]
    public int EvcilHayvanId { get; set; }
    public EvcilHayvan? EvcilHayvan { get; set; }

    [Required(ErrorMessage = "Aşı adı boş bırakılamaz.")]
    [StringLength(100, ErrorMessage = "Aşı adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Aşı Adı")]
    public string AsiAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Aşının yapıldığı tarih girilmelidir.")]
    [Display(Name = "Yapıldığı Tarih")]
    public DateOnly YapildigiTarih { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Bir Sonraki Doz Tarihi")]
    public DateOnly? SonrakiDozTarihi { get; set; }

    [StringLength(250, ErrorMessage = "Not en fazla 250 karakter olabilir.")]
    [Display(Name = "Açıklama / Not")]
    public string? Notlar { get; set; }
}
