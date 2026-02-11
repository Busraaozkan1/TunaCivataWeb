using Microsoft.EntityFrameworkCore;
using TunaCivataWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. ADIM: Veritabanı Servisi (SQLite olarak güncellendi)
// SQL Server yerine UseSqlite kullanıyoruz, böylece "servis başlatılamadı" hatası almazsın.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ADIM: Oturum Yönetimi (Session) Servisi
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Standart Servisler
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor(); 

var app = builder.Build();

// HTTP Pipeline Yapılandırması
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // ÖNEMLİ: Resimlerin ve CSS'lerin görünmesi için şarttır.

app.UseRouting();

// 3. ADIM: Oturum Yönetimi Aktif (Sıralama Önemli!)
app.UseSession(); 

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();