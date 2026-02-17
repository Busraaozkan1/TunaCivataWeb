using Microsoft.EntityFrameworkCore;
using TunaCivataWeb.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. ADIM: Veritabanı Servisi
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. ADIM: Oturum Yönetimi (Session) Servisi
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor(); 

var app = builder.Build();

// --- EKLEDİĞİMİZ KRİTİK KISIM BAŞLANGICI ---
// Bu kısım, Railway'de tablo yoksa otomatik oluşturur.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated(); // Veritabanı ve tablolar yoksa oluşturur.
    }
    catch (Exception ex)
    {
        // Hata olursa loglara yazması için (isteğe bağlı)
        Console.WriteLine("Veritabanı oluşturulurken hata: " + ex.Message);
    }
}
// --- EKLEDİĞİMİZ KRİTİK KISIM BİTİŞİ ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); 

app.UseRouting();
app.UseSession(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
