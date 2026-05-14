using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using ASC.Utilities;
using ASC.Web.Configuration;
using ASC.Web.Data;
using ASC.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASC.Web.Services
{
    public static class DependencyInjection
    {
        // Config services
        public static IServiceCollection AddConfig(
            this IServiceCollection services,
            IConfiguration config)
        {
            var connectionString =
                config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddOptions();

            services.Configure<ApplicationSettings>(
                config.GetSection("AppSettings"));

            // Google Authentication
            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    IConfigurationSection googleAuthNSection =
                        config.GetSection("Authentication:Google");

                    options.ClientId =
                        googleAuthNSection["ClientId"];

                    options.ClientSecret =
                        googleAuthNSection["ClientSecret"];
                });

            return services;
        }

        // Add services
        public static IServiceCollection AddMyDependencyGroup(
            this IServiceCollection services)
        {
            // Add ApplicationDbContext
            services.AddScoped<DbContext, ApplicationDbContext>();

            // Add Identity
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Add services
            services.AddTransient<ASC.Web.Services.IEmailSender,
                AuthMessageSender>();

            services.AddTransient<ISmsSender,
                AuthMessageSender>();

            services.AddScoped<IIdentitySeed,
                IdentitySeed>();

            services.AddScoped<IUnitOfWork,
                UnitOfWork>();

            // Add Cache, Session
            services.AddSession();

            services.AddSingleton<IHttpContextAccessor,
                HttpContextAccessor>();

            services.AddDistributedMemoryCache();

            services.AddSingleton<INavigationCacheOperations,
                NavigationCacheOperations>();

            // Add Razor Pages / MVC
            services.AddRazorPages();

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddControllersWithViews();

            return services;
        }
    }
}