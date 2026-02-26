using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Services;

var builder = WebApplication.CreateBuilder(args);

// --- SERVICES ---
builder.Services.AddScoped<ISearchService, SearchService > ();
builder.Services.AddScoped<IStorageService, AzureStorageService>();
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.UseAuthorization();

// --- ROUTES ---
app.MapAreaControllerRoute(
    name: "social_service",
    areaName: "SocialService",
    pattern: "SocialService/{controller=Account}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "procedures",
    areaName: "Procedures",
    pattern: "Procedures/{controller=Dashboard}/{action=Index}/{id?}");

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
app.UseStaticFiles();
app.Run();
