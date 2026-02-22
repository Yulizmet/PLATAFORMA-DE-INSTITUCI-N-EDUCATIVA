using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ISearchService, SearchService > ();
builder.Services.AddScoped<IStorageService, AzureStorageService>();
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.MapControllerRoute(
    name: "ProceduresArea",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=MainScreen}/{action=SistemaEscolar}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "social_service_root",
    areaName: "SocialService",
    pattern: "SocialService/{controller=Account}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
