using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace web_projesi.Models.Entities;

// Sistemdeki tüm kullanıcıları (Admin, Veteriner, Uye) temsil eden tablo
public class Kullanici : IdentityUser
{
    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Display(Name = "Kayıt Tarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    // Eğer kullanıcı bir Veteriner Hekim ise 1-1 bağlı olduğu hekim profili
    public Hekim? HekimProfili { get; set; }

    // Eğer kullanıcı bir hayvan sahibi (Üye) ise sahip olduğu evcil hayvanlar (1-Çok ilişki)
    public List<EvcilHayvan> EvcilHayvanlar { get; set; } = new();
}
