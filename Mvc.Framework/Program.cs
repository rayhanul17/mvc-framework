using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using mvc.framework;
using mvc.framework.Data;
using mvc.framework.Handlers;
using mvc.framework.Services;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseMySql(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                new MySqlServerVersion(new Version(8, 0, 36))));

        builder.Services.AddDefaultIdentity<mvc.framework.Models.ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
        builder.Services.AddRazorPages();
        builder.Services.AddMemoryCache();

        builder.Services.AddScoped<IDataAccessService, DataAccessService>();
        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();

        // Book Area DI
        builder.Services.AddScoped<mvc.framework.Areas.Book.Data.CategoryRepository>();
        builder.Services.AddScoped<mvc.framework.Areas.Book.Services.CategoryService>();

        //when we use single policy it needs to register like this
        //services.AddAuthorization(options =>
        //{
        //	options.AddPolicy("Authorization", policyCorrectUser =>
        //	{
        //		policyCorrectUser.Requirements.Add(new AuthorizationRequirement());
        //	});
        //});

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            DbInitializer.Initialize(app);
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();


        app.MapControllerRoute(
            name: "areaRoute",
            pattern: "{area:exists}/{controller}/{action}",
            defaults: new { action = "Index" });

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        app.Run();
    }
}