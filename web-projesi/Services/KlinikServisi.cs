using Microsoft.EntityFrameworkCore;
using web_projesi.Data;
using web_projesi.Models.Dtos;
using web_projesi.Models.Entities;

namespace web_projesi.Services;

// MVC Controller, REST API ve MCP araçlarının ortak kullandığı merkezi iş katmanı
public class KlinikServisi : IKlinikServisi
{
    private readonly KlinikDbContext _db;

    public KlinikServisi(KlinikDbContext db)
    {
        _db = db;
    }

    public async Task<List<HekimListeDto>> TumHekimleriGetirAsync()
    {
        return await _db.Hekimler
            .Include(h => h.VerdigiHizmetler)
            .OrderBy(h => h.AdSoyad)
            .Select(h => new HekimListeDto
            {
                HekimId = h.Id,
                AdSoyad = h.AdSoyad,
                UzmanlikAlani = h.UzmanlikAlani,
                MesaiSaatleri = $"{h.MesaiBaslangic:HH:mm} - {h.MesaiBitis:HH:mm}",
                VerdigiHizmetler = h.VerdigiHizmetler
                    .OrderBy(vh => vh.HizmetAdi)
                    .Select(vh => vh.HizmetAdi)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<List<HekimUygunSaatDto>> UygunHekimVeSaatleriGetirAsync(int hizmetId, DateOnly tarih)
    {
        var hizmet = await _db.KlinikHizmetler.FindAsync(hizmetId);
        if (hizmet == null)
        {
            return new List<HekimUygunSaatDto>();
        }

        // Seçilen hizmeti verebilen hekimleri getir
        var uygunHekimler = await _db.Hekimler
            .Include(h => h.VerdigiHizmetler)
            .Where(h => h.VerdigiHizmetler.Any(vh => vh.Id == hizmetId))
            .OrderBy(h => h.AdSoyad)
            .ToListAsync();

        // O günkü reddedilmemiş randevuları çek
        var gunBaslangici = tarih.ToDateTime(TimeOnly.MinValue);
        var gunBitisi = tarih.ToDateTime(TimeOnly.MaxValue);

        var oGunkuRandevular = await _db.Randevular
            .Where(r => r.Durum != RandevuDurumu.Reddedildi
                        && r.BaslangicZamani >= gunBaslangici
                        && r.BaslangicZamani <= gunBitisi)
            .ToListAsync();

        var sonucListesi = new List<HekimUygunSaatDto>();

        foreach (var hekim in uygunHekimler)
        {
            var hekiminRandevulari = oGunkuRandevular
                .Where(r => r.HekimId == hekim.Id)
                .ToList();

            var bosSaatler = new List<string>();
            var adaySaat = hekim.MesaiBaslangic;

            // Hekimin mesai başlangıcından bitişine kadar 30'ar dakikalık dilimleri kontrol et
            while (adaySaat.AddMinutes(hizmet.SureDakika) <= hekim.MesaiBitis)
            {
                // Gece yarısı taşmasını önle (Örn: işlem süresi mesai bitişini aşıyorsa)
                if (adaySaat.AddMinutes(hizmet.SureDakika) < adaySaat)
                {
                    break;
                }

                var adayBaslangic = tarih.ToDateTime(adaySaat);
                var adayBitis = adayBaslangic.AddMinutes(hizmet.SureDakika);

                // Çakışma formülü: (YeniBaşlangıç < MevcutBitiş) VE (YeniBitiş > MevcutBaşlangıç)
                bool cakismaVarMi = hekiminRandevulari.Any(mevcut =>
                    adayBaslangic < mevcut.BitisZamani && adayBitis > mevcut.BaslangicZamani);

                if (!cakismaVarMi)
                {
                    bosSaatler.Add(adaySaat.ToString("HH:mm"));
                }

                adaySaat = adaySaat.AddMinutes(30);
            }

            if (bosSaatler.Count > 0)
            {
                sonucListesi.Add(new HekimUygunSaatDto
                {
                    HekimId = hekim.Id,
                    HekimAdSoyad = hekim.AdSoyad,
                    UzmanlikAlani = hekim.UzmanlikAlani,
                    HizmetId = hizmet.Id,
                    HizmetAdi = hizmet.HizmetAdi,
                    HizmetSuresiDakika = hizmet.SureDakika,
                    Tarih = tarih.ToString("yyyy-MM-dd"),
                    BosSaatDilimleri = bosSaatler
                });
            }
        }

        return sonucListesi;
    }

    public async Task<HayvanAsiTakvimDto?> HayvanAsiTakvimiGetirAsync(
        int? hayvanId,
        string? hayvanAdi,
        string? istekYapanKullaniciId = null,
        bool tamYetkiliMi = false)
    {
        var sorgu = _db.EvcilHayvanlar
            .Include(eh => eh.Sahip)
            .Include(eh => eh.AsiKayitlari)
            .AsQueryable();

        // Eğer istek yapan kişi Admin veya Veteriner değilse, yalnızca kendi hayvanını görebilir
        if (!tamYetkiliMi)
        {
            if (string.IsNullOrEmpty(istekYapanKullaniciId))
            {
                return null;
            }
            sorgu = sorgu.Where(eh => eh.SahipId == istekYapanKullaniciId);
        }

        EvcilHayvan? hayvan = null;

        if (hayvanId.HasValue)
        {
            hayvan = await sorgu.FirstOrDefaultAsync(eh => eh.Id == hayvanId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(hayvanAdi))
        {
            var arananAd = hayvanAdi.Trim().ToLower();
            hayvan = await sorgu.FirstOrDefaultAsync(eh => eh.Ad.ToLower() == arananAd);
        }

        if (hayvan == null)
        {
            return null;
        }

        var bugun = DateOnly.FromDateTime(DateTime.Today);

        var gecmisAsilar = hayvan.AsiKayitlari
            .OrderByDescending(a => a.YapildigiTarih)
            .Select(a => new AsiDetayDto
            {
                AsiId = a.Id,
                AsiAdi = a.AsiAdi,
                YapildigiTarih = a.YapildigiTarih.ToString("yyyy-MM-dd"),
                SonrakiDozTarihi = a.SonrakiDozTarihi?.ToString("yyyy-MM-dd"),
                Notlar = a.Notlar
            })
            .ToList();

        var yaklasanAsilar = hayvan.AsiKayitlari
            .Where(a => a.SonrakiDozTarihi.HasValue && a.SonrakiDozTarihi.Value >= bugun)
            .OrderBy(a => a.SonrakiDozTarihi)
            .Select(a => new AsiDetayDto
            {
                AsiId = a.Id,
                AsiAdi = a.AsiAdi,
                YapildigiTarih = a.YapildigiTarih.ToString("yyyy-MM-dd"),
                SonrakiDozTarihi = a.SonrakiDozTarihi?.ToString("yyyy-MM-dd"),
                Notlar = a.Notlar
            })
            .ToList();

        return new HayvanAsiTakvimDto
        {
            HayvanId = hayvan.Id,
            HayvanAdi = hayvan.Ad,
            Tur = hayvan.Tur,
            Irk = hayvan.Irk,
            SahipAdSoyad = hayvan.Sahip?.AdSoyad ?? "Belirtilmemiş",
            GecmisAsilar = gecmisAsilar,
            YaklasanAsilar = yaklasanAsilar
        };
    }

    public async Task<IslemSonucuDto> RandevuOlusturAsync(
        int evcilHayvanId,
        int hekimId,
        int klinikHizmetId,
        DateTime baslangicZamani,
        string? istekYapanKullaniciId = null,
        bool tamYetkiliMi = false)
    {
        // 1. Evcil hayvan ve sahiplik kontrolü
        var hayvan = await _db.EvcilHayvanlar.FindAsync(evcilHayvanId);
        if (hayvan == null)
        {
            return IslemSonucuDto.Hatali("Seçilen evcil hayvan sistemde bulunamadı.");
        }

        if (!tamYetkiliMi && (string.IsNullOrEmpty(istekYapanKullaniciId) || hayvan.SahipId != istekYapanKullaniciId))
        {
            return IslemSonucuDto.Hatali("Yalnızca kendi evcil hayvanınız için randevu oluşturabilirsiniz.");
        }

        // 2. Hizmet kontrolü
        var hizmet = await _db.KlinikHizmetler.FindAsync(klinikHizmetId);
        if (hizmet == null)
        {
            return IslemSonucuDto.Hatali("Seçilen klinik hizmeti bulunamadı.");
        }

        // 3. Hekim ve hizmeti verebilme yetkisi kontrolü
        var hekim = await _db.Hekimler
            .Include(h => h.VerdigiHizmetler)
            .FirstOrDefaultAsync(h => h.Id == hekimId);

        if (hekim == null)
        {
            return IslemSonucuDto.Hatali("Seçilen veteriner hekim bulunamadı.");
        }

        if (!hekim.VerdigiHizmetler.Any(vh => vh.Id == klinikHizmetId))
        {
            return IslemSonucuDto.Hatali($"{hekim.AdSoyad}, '{hizmet.HizmetAdi}' hizmetini vermemektedir. Lütfen bu hizmeti veren uygun bir hekim seçiniz.");
        }

        // 4. Geçmiş tarih kontrolü
        if (baslangicZamani < DateTime.Now.AddMinutes(-5))
        {
            return IslemSonucuDto.Hatali("Geçmiş bir tarih veya saate randevu alınamaz.");
        }

        // 5. Hizmet süresine göre bitiş zamanını hesapla ve hekimin mesai saatlerini kontrol et
        var bitisZamani = baslangicZamani.AddMinutes(hizmet.SureDakika);
        var randevuBaslangicSaati = TimeOnly.FromDateTime(baslangicZamani);
        var randevuBitisSaati = TimeOnly.FromDateTime(bitisZamani);

        if (baslangicZamani.Date != bitisZamani.Date ||
            randevuBaslangicSaati < hekim.MesaiBaslangic ||
            randevuBitisSaati > hekim.MesaiBitis)
        {
            return IslemSonucuDto.Hatali(
                $"{hekim.AdSoyad} yalnızca {hekim.MesaiBaslangic:HH:mm} - {hekim.MesaiBitis:HH:mm} saatleri arasında hizmet vermektedir. " +
                $"Seçtiğiniz '{hizmet.HizmetAdi}' hizmeti {hizmet.SureDakika} dakika sürdüğü için ({randevuBaslangicSaati:HH:mm} - {randevuBitisSaati:HH:mm}) mesai saatleri dışına taşmaktadır.");
        }

        // 6. Hekimin önceki randevularıyla (hizmet süresi dahil) çakışma kontrolü
        var cakisanRandevu = await _db.Randevular
            .Include(r => r.KlinikHizmet)
            .Where(r => r.HekimId == hekimId
                        && r.Durum != RandevuDurumu.Reddedildi
                        && baslangicZamani < r.BitisZamani
                        && bitisZamani > r.BaslangicZamani)
            .FirstOrDefaultAsync();

        if (cakisanRandevu != null)
        {
            return IslemSonucuDto.Hatali(
                $"Seçilen saat aralığı uygun değildir! {hekim.AdSoyad} için {cakisanRandevu.BaslangicZamani:dd.MM.yyyy HH:mm} - {cakisanRandevu.BitisZamani:HH:mm} " +
                $"saatleri arasında başka bir randevu bulunmaktadır. '{hizmet.HizmetAdi}' hizmet süresi ({hizmet.SureDakika} dk) hesaba katıldığında çakışma oluşmaktadır.");
        }

        // 7. Tüm kontroller başarılıysa randevuyu "Bekliyor" durumunda kaydet
        var yeniRandevu = new Randevu
        {
            EvcilHayvanId = evcilHayvanId,
            HekimId = hekimId,
            KlinikHizmetId = klinikHizmetId,
            BaslangicZamani = baslangicZamani,
            BitisZamani = bitisZamani,
            SureDakika = hizmet.SureDakika,
            Ucret = hizmet.Ucret,
            Durum = RandevuDurumu.Bekliyor
        };

        _db.Randevular.Add(yeniRandevu);
        await _db.SaveChangesAsync();

        return IslemSonucuDto.Basarili(
            $"Randevunuz '{hizmet.HizmetAdi}' ({hizmet.SureDakika} dk - {hizmet.Ucret:N2} TL) için {baslangicZamani:dd.MM.yyyy HH:mm} saatine 'Bekliyor' durumunda oluşturuldu.",
            yeniRandevu.Id);
    }

    public async Task<List<HizmetCiroRaporDto>> HizmetBazliCiroRaporuGetirAsync()
    {
        // Hizmet türüne göre randevu sayısı ve toplam ciro hesaplayan LINQ sorgusu
        return await _db.KlinikHizmetler
            .Select(h => new HizmetCiroRaporDto
            {
                HizmetId = h.Id,
                HizmetAdi = h.HizmetAdi,
                BirimUcret = h.Ucret,
                ToplamRandevuSayisi = h.Randevular.Count(),
                OnayliVeyaTamamlananSayisi = h.Randevular.Count(r =>
                    r.Durum == RandevuDurumu.Onaylandi || r.Durum == RandevuDurumu.Tamamlandi),
                ToplamCiro = h.Randevular
                    .Where(r => r.Durum == RandevuDurumu.Onaylandi || r.Durum == RandevuDurumu.Tamamlandi)
                    .Sum(r => (decimal?)r.Ucret) ?? 0m
            })
            .OrderByDescending(r => r.ToplamCiro)
            .ThenBy(r => r.HizmetAdi)
            .ToListAsync();
    }
}
