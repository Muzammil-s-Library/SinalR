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
            var connectionString = builder.Configuration.GetConnectionString("SDbContextConnection") ?? throw new InvalidOperationException("Connection string 'SDbContextConnection' not found.");

            builder.Services.AddDbContext<SDbContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<SDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            // Add SignalR service
            builder.Services.AddSignalR();

            var app = builder.Build();

            // Get port from environment variable or default to 8080
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Urls.Add($"http://*:{port}");

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
