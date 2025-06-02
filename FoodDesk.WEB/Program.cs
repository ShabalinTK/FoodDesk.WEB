using FoodDesk.Application.Extensions;
using FoodDesk.Application.Interfaces.Services;
using FoodDesk.Infrastructure.Extensions;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.Infrastructure.Services;
using FoodDesk.Persistence.Context;
using FoodDesk.Persistence.Context;
using FoodDesk.Persistence.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodDesk.WEB;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Add services to the container.
        builder.Services.AddApplicationLayer();
        builder.Services.AddInfrastructureLayer();
        builder.Services.AddPersistenceLayer(builder.Configuration);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<IAuthService, AuthService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
              name: "areas",
              pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
            );
        });

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
