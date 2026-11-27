using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using web_projesi.Data;
using web_projesi.Models.Entities;
using web_projesi.Services;

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

// Yetkisiz girişlerde yönlendirilecek sayfalar
builder.Services.ConfigureApplicationCookie(cerez =>
{
    cerez.LoginPath = "/Hesap/Giris";
    cerez.LogoutPath = "/Hesap/Cikis";
    cerez.AccessDeniedPath = "/Hesap/ErisimEngellendi";
});

// 3. Ortak İş Katmanı (MVC, REST API ve MCP araçları aynı servisi kullanır)
builder.Services.AddScoped<IKlinikServisi, KlinikServisi>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 4. Başlangıç verilerini (Roller, Admin hesabı, Örnek hizmet ve kayıtlar) yükle
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
