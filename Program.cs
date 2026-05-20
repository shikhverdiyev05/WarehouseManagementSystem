using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WarehouseMS.Data;
using WarehouseMS.Models;

var builder = WebApplication.CreateBuilder(args);
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
// 1. Database Bağlantısı (SQL Server)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Sistemi (İstifadəçi idarəetməsi)
builder.Services.AddIdentity<AppUser, IdentityRole>(options => {
    // Test mərhələsi üçün şifrə tələblərini sadələşdiririk
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// 3. SESSION və CACHE Servisləri (Xətanın həlli buradadır)
builder.Services.AddDistributedMemoryCache(); // Session dataları üçün yaddaş
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dəqiqə aktivlik olmasa session sıfırlansın
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 4. MVC və Controller servisləri
builder.Services.AddControllersWithViews();

// 5. Cookie Ayarları (Giriş etdikdən sonra yönləndirmələr üçün)
builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(1); // Giriş 1 gün qalsın
});

var app = builder.Build();

// --- MIDDLEWARE ARDICILLIĞI (Bura çox önəmlidir!) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting(); // 1. Routing həmişə birinci gəlir

// SESSION mütləq Authentication-dan əvvəl gəlməlidir!
app.UseSession();        // 2. Session-ı aktiv et

app.UseAuthentication(); // 3. Kim olduğunu yoxla (Cookie/Identity)
app.UseAuthorization();  // 4. İcazəni yoxla

// 6. Marşrutlaşdırma (Default olaraq Login səhifəsi açılsın)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();