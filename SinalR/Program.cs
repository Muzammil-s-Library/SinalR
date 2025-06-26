using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SinalR.Areas.Identity.Data;

namespace SinalR
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

// ✅ Use Railway-assigned port if it exists
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://*:{port}");
}

// Continue setting up services
var connectionString = builder.Configuration.GetConnectionString("SDbContextConnection")
    ?? throw new InvalidOperationException("Connection string 'SDbContextConnection' not found.");
builder.Services.AddDbContext<SDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<SDbContext>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapRazorPages();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<ChatHub>("/chathub").RequireAuthorization();

app.Run();

        }
    }
}
