namespace web_projesi.Models.Dtos;

// Servis katmanındaki ekleme/güncelleme işlemlerinin sonucunu ve hata/başarı mesajını taşıyan sınıf
public class IslemSonucuDto
{
    public bool BasariliMi { get; set; }
    public string Mesaj { get; set; } = string.Empty;
    public int? KayitId { get; set; }

    public static IslemSonucuDto Basarili(string mesaj, int? kayitId = null) =>
        new() { BasariliMi = true, Mesaj = mesaj, KayitId = kayitId };

    public static IslemSonucuDto Hatali(string mesaj) =>
        new() { BasariliMi = false, Mesaj = mesaj };
}

// Veteriner hekimlerin özet bilgilerini dışarıya (API/UI) açan DTO
public class HekimListeDto
{
    public int HekimId { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string UzmanlikAlani { get; set; } = string.Empty;
    public string MesaiSaatleri { get; set; } = string.Empty;
    public List<string> VerdigiHizmetler { get; set; } = new();
}

// Belirli bir hizmet ve tarihte hekimin müsait olduğu boş saat dilimlerini taşıyan DTO
public class HekimUygunSaatDto
{
    public int HekimId { get; set; }
    public string HekimAdSoyad { get; set; } = string.Empty;
    public string UzmanlikAlani { get; set; } = string.Empty;
    public int HizmetId { get; set; }
    public string HizmetAdi { get; set; } = string.Empty;
    public int HizmetSuresiDakika { get; set; }
    public string Tarih { get; set; } = string.Empty;
    public List<string> BosSaatDilimleri { get; set; } = new();
}

// Tek bir aşı kaydının DTO karşılığı
public class AsiDetayDto
{
    public int AsiId { get; set; }
    public string AsiAdi { get; set; } = string.Empty;
    public string YapildigiTarih { get; set; } = string.Empty;
    public string? SonrakiDozTarihi { get; set; }
    public string? Notlar { get; set; }
}

// Bir evcil hayvanın geçmiş ve yaklaşan aşı takvimini taşıyan DTO
public class HayvanAsiTakvimDto
{
    public int HayvanId { get; set; }
    public string HayvanAdi { get; set; } = string.Empty;
    public string Tur { get; set; } = string.Empty;
    public string Irk { get; set; } = string.Empty;
    public string SahipAdSoyad { get; set; } = string.Empty;
    public List<AsiDetayDto> GecmisAsilar { get; set; } = new();
    public List<AsiDetayDto> YaklasanAsilar { get; set; } = new();
}

// Hizmet türüne göre randevu sayısı ve toplam ciro raporunu taşıyan DTO
public class HizmetCiroRaporDto
{
    public int HizmetId { get; set; }
    public string HizmetAdi { get; set; } = string.Empty;
    public decimal BirimUcret { get; set; }
    public int ToplamRandevuSayisi { get; set; }
    public int OnayliVeyaTamamlananSayisi { get; set; }
    public decimal ToplamCiro { get; set; }
}
