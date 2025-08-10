using DynamicRoleMenuSystem.Core.Common;
using DynamicRoleMenuSystem.Infrastructure.Extensions;
using DynamicRoleMenuSystem.Application.Interfaces;
using DynamicRoleMenuSystem.Application.Services;
using DynamicRoleMenuSystem.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpContextAccessor for accessing current user in repositories
builder.Services.AddHttpContextAccessor();

// Configure AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Add Infrastructure and Application services
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationServices();

// Add area discovery service
builder.Services.AddScoped<IAreaDiscoveryService, AreaDiscoveryService>();

// Add Log Repository and Service
builder.Services.AddScoped<DynamicRoleMenuSystem.Core.Interfaces.ILogRepository, DynamicRoleMenuSystem.Infrastructure.Repositories.LogRepository>();
builder.Services.AddScoped<ILogService, LogService>();

// Add Background Service for Log Archiving
builder.Services.AddHostedService<DynamicRoleMenuSystem.Web.Services.LogArchiveBackgroundService>();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
});

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Add Permission Middleware
app.UseMiddleware<PermissionMiddleware>();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database
using (var scope = app.Services.CreateScope())
{
    await DynamicRoleMenuSystem.Infrastructure.Data.DbInitializer.InitializeAsync(scope.ServiceProvider);
    
    // Ensure Site Settings menu exists and is assigned to SuperAdmin
    await DynamicRoleMenuSystem.Web.Data.EnsureSiteSettingsMenu.EnsureMenuExistsAsync(scope.ServiceProvider);
    
    // Seed comprehensive menus for SuperAdmin with all permissions
    var context = scope.ServiceProvider.GetRequiredService<DynamicRoleMenuSystem.Infrastructure.Data.ApplicationDbContext>();
    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<DynamicRoleMenuSystem.Core.Entities.ApplicationRole>>();
    await DynamicRoleMenuSystem.Infrastructure.Data.SuperAdminMenuSeeder.SeedAllMenusForSuperAdminAsync(context, roleManager);
}

app.Run();
