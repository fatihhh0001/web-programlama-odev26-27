using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web_projesi.Models.Entities;

namespace web_projesi.Data;

// Veritabanı tablolarını ve aralarındaki ilişkileri (Foreign Key) yöneten ana sınıf
public class KlinikDbContext : IdentityDbContext<Kullanici>
{
    public KlinikDbContext(DbContextOptions<KlinikDbContext> options) : base(options)
    {
    }

    public DbSet<KlinikHizmet> KlinikHizmetler => Set<KlinikHizmet>();
    public DbSet<Hekim> Hekimler => Set<Hekim>();
    public DbSet<EvcilHayvan> EvcilHayvanlar => Set<EvcilHayvan>();
    public DbSet<AsiKaydi> AsiKayitlari => Set<AsiKaydi>();
    public DbSet<Randevu> Randevular => Set<Randevu>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Para birimi (Ücret) sütunları için hassasiyet ayarı
        builder.Entity<KlinikHizmet>()
            .Property(h => h.Ucret)
            .HasPrecision(10, 2);

        builder.Entity<Randevu>()
            .Property(r => r.Ucret)
            .HasPrecision(10, 2);

        // 1. Kullanıcı (Veteriner) <-> Hekim Profili (1'e 1 İlişki)
        builder.Entity<Hekim>()
            .HasOne(h => h.Kullanici)
            .WithOne(k => k.HekimProfili)
            .HasForeignKey<Hekim>(h => h.KullaniciId)
            .OnDelete(DeleteBehavior.SetNull);

        // 2. Hekim <-> KlinikHizmet (Çoka-Çok İlişki: Hekimin verebildiği hizmetler)
        builder.Entity<Hekim>()
            .HasMany(h => h.VerdigiHizmetler)
            .WithMany(kh => kh.Hekimler)
            .UsingEntity(j => j.ToTable("HekimHizmetleri"));

        // 3. Kullanıcı (Üye) <-> EvcilHayvan (1'e Çok İlişki: Hayvan sahibi)
        builder.Entity<EvcilHayvan>()
            .HasOne(eh => eh.Sahip)
            .WithMany(k => k.EvcilHayvanlar)
            .HasForeignKey(eh => eh.SahipId)
            .OnDelete(DeleteBehavior.Cascade);

        // 4. EvcilHayvan <-> AsiKaydi (1'e Çok İlişki: Hayvan silinirse aşı kayıtları da silinir)
        builder.Entity<AsiKaydi>()
            .HasOne(a => a.EvcilHayvan)
            .WithMany(eh => eh.AsiKayitlari)
            .HasForeignKey(a => a.EvcilHayvanId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5. EvcilHayvan <-> Randevu (1'e Çok İlişki)
        builder.Entity<Randevu>()
            .HasOne(r => r.EvcilHayvan)
            .WithMany(eh => eh.Randevular)
            .HasForeignKey(r => r.EvcilHayvanId)
            .OnDelete(DeleteBehavior.Cascade);

        // 6. Hekim <-> Randevu (1'e Çok İlişki: Randevusu olan hekim yanlışlıkla silinemesin)
        builder.Entity<Randevu>()
            .HasOne(r => r.Hekim)
            .WithMany(h => h.Randevular)
            .HasForeignKey(r => r.HekimId)
            .OnDelete(DeleteBehavior.Restrict);

        // 7. KlinikHizmet <-> Randevu (1'e Çok İlişki: Randevusu olan hizmet yanlışlıkla silinemesin)
        builder.Entity<Randevu>()
            .HasOne(r => r.KlinikHizmet)
            .WithMany(kh => kh.Randevular)
            .HasForeignKey(r => r.KlinikHizmetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
