using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//Procedures
builder.Services.AddScoped<IStorageService, AzureStorageService>();

var app = builder.Build();

//Views

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

app.MapRazorPages();

app.Run();
