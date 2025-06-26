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

            // Get port from environment variable or default to 8080
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

            // Configure the web host to listen on the right port
            builder.WebHost.UseUrls($"http://*:{port}");

            // Your usual service registrations here
            var connectionString = builder.Configuration.GetConnectionString("SDbContextConnection") ?? throw new InvalidOperationException("Connection string 'SDbContextConnection' not found.");
            builder.Services.AddDbContext<SDbContext>(options => options.UseSqlServer(connectionString));
            builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<SDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Middleware pipeline and routing here
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
