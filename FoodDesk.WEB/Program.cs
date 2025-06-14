using FoodDesk.Application.Extensions;
using FoodDesk.Application.Interfaces.Services;
using FoodDesk.Infrastructure.Extensions;
using FoodDesk.Infrastructure.Identity;
using FoodDesk.Infrastructure.Services;
using FoodDesk.Persistence.Context;
using FoodDesk.Persistence.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FoodDesk.WEB.Logging;
using FoodDesk.WEB.Hubs;
using Microsoft.Extensions.Caching.StackExchangeRedis;

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

        // Добавляем поддержку сессий
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30); // Время жизни сессии
            options.Cookie.HttpOnly = true; // Защита cookie
            options.Cookie.IsEssential = true; // Для GDPR
        });

        // Добавляем SignalR
        builder.Services.AddSignalR();

        // Добавляем Redis кэширование
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = builder.Configuration.GetSection("Redis:Configuration").Value;
            options.InstanceName = "FoodDesk_";
        });

        builder.Services.AddApplicationLayer();
        builder.Services.AddInfrastructureLayer();
        builder.Services.AddPersistenceLayer(builder.Configuration);

        builder.Services.AddControllersWithViews();

        builder.Services.AddScoped<IAuthService, AuthService>();

        builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
        builder.Services.AddScoped<IEmailSender, EmailSender>(sp =>
        {
            var emailSettings = sp.GetRequiredService<IOptions<EmailSettings>>().Value;
            return new EmailSender(
                emailSettings.SmtpHost,
                emailSettings.SmtpPort,
                emailSettings.SmtpUser,
                emailSettings.SmtpPass
            );
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var errorFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorFeature != null)
                    {
                        context.Response.StatusCode = 500;
                    }
                });
            });
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/Error/{0}");

        app.UseHttpsRedirection();
        app.UseStaticFiles(); // Если MapStaticAssets не настроен

        app.UseRouting();

        // Добавляем middleware для сессий
        app.UseSession();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<UserActionLoggingMiddleware>();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<NotificationHub>("/notificationHub");
            endpoints.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
            );
            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });

        app.MapStaticAssets();
        app.Run();
    }
}