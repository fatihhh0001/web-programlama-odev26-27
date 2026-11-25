using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_projesi.Models.Entities;

namespace web_projesi.Data;

// Uygulama ilk kez çalıştığında rolleri, admin hesabını ve örnek klinik kayıtlarını oluşturan sınıf
public static class BaslangicVerisi
{
    public const string AdminEposta = "g231210007@sakarya.edu.tr";
    public const string VarsayilanSifre = "sau";

    public static async Task YukleAsync(IServiceProvider servisSaglayici)
    {
        using var kapsam = servisSaglayici.CreateScope();
        var db = kapsam.ServiceProvider.GetRequiredService<KlinikDbContext>();
        var rolYoneticisi = kapsam.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var kullaniciYoneticisi = kapsam.ServiceProvider.GetRequiredService<UserManager<Kullanici>>();

        // Bekleyen migration varsa veritabanına uygula
        await db.Database.MigrateAsync();

        // 1. Rolleri oluştur (Admin, Veteriner, Uye)
        string[] roller = { "Admin", "Veteriner", "Uye" };
        foreach (var rol in roller)
        {
            if (!await rolYoneticisi.RoleExistsAsync(rol))
            {
                await rolYoneticisi.CreateAsync(new IdentityRole(rol));
            }
        }

        // 2. Zorunlu Admin kullanıcısını oluştur (g231210007@sakarya.edu.tr / sau)
        var adminKullanici = await kullaniciYoneticisi.FindByEmailAsync(AdminEposta);
        if (adminKullanici == null)
        {
            adminKullanici = new Kullanici
            {
                UserName = AdminEposta,
                Email = AdminEposta,
                AdSoyad = "Klinik Yöneticisi",
                EmailConfirmed = true
            };

            var sonuc = await kullaniciYoneticisi.CreateAsync(adminKullanici, VarsayilanSifre);
            if (sonuc.Succeeded)
            {
                await kullaniciYoneticisi.AddToRoleAsync(adminKullanici, "Admin");
            }
        }

        // 3. Klinik hizmetleri boşsa temel hizmetleri tanımla
        if (!await db.KlinikHizmetler.AnyAsync())
        {
            var muayene = new KlinikHizmet
            {
                HizmetAdi = "Genel Muayene",
                Aciklama = "Rutin fiziksel kontrol, ateş ve kilo ölçümü",
                SureDakika = 30,
                Ucret = 650m
            };
            var asiUygulama = new KlinikHizmet
            {
                HizmetAdi = "Aşı Uygulaması",
                Aciklama = "Periyodik karma, kuduz veya iç-dış parazit aşısı",
                SureDakika = 20,
                Ucret = 500m
            };
            var kisirlastirma = new KlinikHizmet
            {
                HizmetAdi = "Kısırlaştırma Operasyonu",
                Aciklama = "Anestezi altında cerrahi kısırlaştırma işlemi",
                SureDakika = 60,
                Ucret = 2800m
            };
            var disTemizligi = new KlinikHizmet
            {
                HizmetAdi = "Diş Taşı Temizliği",
                Aciklama = "Ultrasonik cihazla diş taşı temizliği ve ağız bakımı",
                SureDakika = 45,
                Ucret = 1200m
            };
            var tirasBakim = new KlinikHizmet
            {
                HizmetAdi = "Tıraş ve Tırnak Bakımı",
                Aciklama = "Makas/makine tıraşı, banyo ve tırnak kesimi",
                SureDakika = 40,
                Ucret = 750m
            };

            db.KlinikHizmetler.AddRange(muayene, asiUygulama, kisirlastirma, disTemizligi, tirasBakim);
            await db.SaveChangesAsync();

            // 4. Örnek Veteriner Hekim hesapları ve hekim profilleri (1-1 ve Çoka-Çok ilişkili)
            var hekim1Hesap = new Kullanici
            {
                UserName = "vet1@ornek.com",
                Email = "vet1@ornek.com",
                AdSoyad = "Vet. Hekim Emre Karahan",
                EmailConfirmed = true
            };
            if (await kullaniciYoneticisi.FindByEmailAsync(hekim1Hesap.Email) == null)
            {
                await kullaniciYoneticisi.CreateAsync(hekim1Hesap, VarsayilanSifre);
                await kullaniciYoneticisi.AddToRoleAsync(hekim1Hesap, "Veteriner");
            }

            var hekim2Hesap = new Kullanici
            {
                UserName = "vet2@ornek.com",
                Email = "vet2@ornek.com",
                AdSoyad = "Vet. Hekim Selin Demir",
                EmailConfirmed = true
            };
            if (await kullaniciYoneticisi.FindByEmailAsync(hekim2Hesap.Email) == null)
            {
                await kullaniciYoneticisi.CreateAsync(hekim2Hesap, VarsayilanSifre);
                await kullaniciYoneticisi.AddToRoleAsync(hekim2Hesap, "Veteriner");
            }

            var hekim1 = new Hekim
            {
                AdSoyad = "Vet. Hekim Emre Karahan",
                UzmanlikAlani = "Küçük Hayvan ve Cerrahi",
                Telefon = "0500 111 22 33",
                MesaiBaslangic = new TimeOnly(9, 0),
                MesaiBitis = new TimeOnly(17, 0),
                KullaniciId = hekim1Hesap.Id,
                VerdigiHizmetler = new List<KlinikHizmet> { muayene, asiUygulama, kisirlastirma }
            };

            var hekim2 = new Hekim
            {
                AdSoyad = "Vet. Hekim Selin Demir",
                UzmanlikAlani = "Dermatoloji ve Diş Sağlığı",
                Telefon = "0500 444 55 66",
                MesaiBaslangic = new TimeOnly(10, 0),
                MesaiBitis = new TimeOnly(18, 0),
                KullaniciId = hekim2Hesap.Id,
                VerdigiHizmetler = new List<KlinikHizmet> { muayene, asiUygulama, disTemizligi, tirasBakim }
            };

            db.Hekimler.AddRange(hekim1, hekim2);
            await db.SaveChangesAsync();

            // 5. Örnek Hayvan Sahibi (Üye), Evcil Hayvan ("Pamuk"), Aşı Kaydı ve Randevu
            var ornekUye = new Kullanici
            {
                UserName = "uye1@ornek.com",
                Email = "uye1@ornek.com",
                AdSoyad = "Ali Yılmaz",
                EmailConfirmed = true
            };
            if (await kullaniciYoneticisi.FindByEmailAsync(ornekUye.Email) == null)
            {
                await kullaniciYoneticisi.CreateAsync(ornekUye, VarsayilanSifre);
                await kullaniciYoneticisi.AddToRoleAsync(ornekUye, "Uye");

                var pamuk = new EvcilHayvan
                {
                    Ad = "Pamuk",
                    Tur = "Kedi",
                    Irk = "Tekir",
                    DogumTarihi = new DateOnly(2024, 4, 15),
                    Kilo = 3.8,
                    SahipId = ornekUye.Id
                };

                var tarcan = new EvcilHayvan
                {
                    Ad = "Tarçın",
                    Tur = "Köpek",
                    Irk = "Golden Retriever",
                    DogumTarihi = new DateOnly(2022, 8, 10),
                    Kilo = 24.5,
                    SahipId = ornekUye.Id
                };

                db.EvcilHayvanlar.AddRange(pamuk, tarcan);
                await db.SaveChangesAsync();

                // Pamuk ve Tarçın için geçmiş ve yaklaşan aşı kayıtları
                db.AsiKayitlari.AddRange(
                    new AsiKaydi
                    {
                        EvcilHayvanId = pamuk.Id,
                        AsiAdi = "Kedi Karma Aşısı (1. Doz)",
                        YapildigiTarih = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                        SonrakiDozTarihi = DateOnly.FromDateTime(DateTime.Today.AddDays(14)),
                        Notlar = "İlk doz sorunsuz tamamlandı, 2 hafta sonra tekrar dozu yapılacak."
                    },
                    new AsiKaydi
                    {
                        EvcilHayvanId = tarcan.Id,
                        AsiAdi = "Kuduz Aşısı",
                        YapildigiTarih = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)),
                        SonrakiDozTarihi = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
                        Notlar = "Yıllık rutin kuduz aşısı."
                    }
                );

                // Örnek bir randevu kaydı
                var yarinSaat11 = DateTime.Today.AddDays(1).AddHours(11);
                db.Randevular.Add(new Randevu
                {
                    EvcilHayvanId = pamuk.Id,
                    HekimId = hekim1.Id,
                    KlinikHizmetId = muayene.Id,
                    BaslangicZamani = yarinSaat11,
                    BitisZamani = yarinSaat11.AddMinutes(muayene.SureDakika),
                    SureDakika = muayene.SureDakika,
                    Ucret = muayene.Ucret,
                    Durum = RandevuDurumu.Onaylandi,
                    MuayeneNotu = "Rutin kontrol randevusu onaylandı."
                });

                await db.SaveChangesAsync();
            }
        }
    }
}
