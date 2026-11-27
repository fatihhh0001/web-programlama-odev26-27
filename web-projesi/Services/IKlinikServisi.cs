using web_projesi.Models.Dtos;

namespace web_projesi.Services;

// MVC sayfaları, REST API ve MCP araçlarının ortak kullanacağı iş kuralları arayüzü
public interface IKlinikServisi
{
    // 1. Tüm veteriner hekimleri ve uzmanlık/hizmet bilgilerini DTO olarak listeler
    Task<List<HekimListeDto>> TumHekimleriGetirAsync();

    // 2. Belirli bir tarihte, seçilen hizmet için uygun hekimleri ve boş saat dilimlerini hesaplar
    Task<List<HekimUygunSaatDto>> UygunHekimVeSaatleriGetirAsync(int hizmetId, DateOnly tarih);

    // 3. Bir evcil hayvanın geçmiş ve yaklaşan aşı takvimini (yetki kontrolüyle) getirir
    Task<HayvanAsiTakvimDto?> HayvanAsiTakvimiGetirAsync(
        int? hayvanId,
        string? hayvanAdi,
        string? istekYapanKullaniciId = null,
        bool tamYetkiliMi = false);

    // 4. Hizmet süresi ve hekim mesaisini dikkate alarak çakışma kontrolü yapar ve "Bekliyor" durumunda randevu oluşturur
    Task<IslemSonucuDto> RandevuOlusturAsync(
        int evcilHayvanId,
        int hekimId,
        int klinikHizmetId,
        DateTime baslangicZamani,
        string? istekYapanKullaniciId = null,
        bool tamYetkiliMi = false);

    // 5. Hizmet türüne göre randevu sayısı ve ciro raporunu LINQ ile hesaplar
    Task<List<HizmetCiroRaporDto>> HizmetBazliCiroRaporuGetirAsync();
}
