using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_projesi.Data;
using web_projesi.Models.Entities;

// PostgreSQL DateTime uyumluluğu için
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// 1. PostgreSQL Veritabanı Bağlantısı
var baglantiDizesi = builder.Configuration.GetConnectionString("KlinikBaglantisi");
builder.Services.AddDbContext<KlinikDbContext>(secenek =>
    secenek.UseNpgsql(baglantiDizesi));

// 2. ASP.NET Core Identity (Kullanıcı ve Rol Yönetimi)
// Not: Ödevde Admin şifresi "sau" (3 harf) istendiği için şifre kuralları buna uygun ayarlandı.
builder.Services.AddIdentity<Kullanici, IdentityRole>(ayar =>
{
    ayar.Password.RequiredLength = 3;
    ayar.Password.RequireDigit = false;
    ayar.Password.RequireLowercase = false;
    ayar.Password.RequireUppercase = false;
    ayar.Password.RequireNonAlphanumeric = false;
    ayar.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<KlinikDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. Başlangıç verilerini (Roller, Admin hesabı, Örnek hizmet ve kayıtlar) yükle
await BaslangicVerisi.YukleAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
