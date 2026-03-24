using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---
builder.Services.AddScoped<ISearchService, SearchService > ();
builder.Services.AddScoped<IStorageService, AzureStorageService>();
builder.Services.AddTransient<IEmailSender, OutlookEmailSender>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SchoolManager.Areas.Medical.Filters.MedicalPermissionFilter>();
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/UserMng/Account/Login";
        options.AccessDeniedPath = "/UserMng/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SchoolManager.Auth";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

// -- PROCEDURE REPORTS ---
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

var app = builder.Build();

// --- MIDDLEWARE ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// --- ROUTES ---
app.MapAreaControllerRoute(
    name: "user_mng",
    areaName: "UserMng",
    pattern: "UserMng/{controller=Account}/{action=Login}/{id?}");

app.MapAreaControllerRoute(
    name: "social_service",
    areaName: "SocialService",
    pattern: "SocialService/{controller=Account}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "procedures",
    areaName: "Procedures",
    pattern: "Procedures/{controller=Dashboard}/{action=Index}/{id?}");

app.MapAreaControllerRoute(   
    name: "medical",
    areaName: "Medical",
    pattern: "Medical/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{area=MainScreen}/{controller=MainScreen}/{action=Index}/{id?}");


//app.MapControllerRoute(
//    name: "areas",
//    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
