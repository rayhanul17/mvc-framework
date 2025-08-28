using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using ModularHost.Web.Core.Extensions;
using ModularHost.Web.Core.Services.Interfaces;
using ModularHost.Web.Core.Models;
using ModularHost.Web.Core.Models.Entities;
using ModularHost.Web.Core.Infrastructure;
using ModularHost.Web.Middleware;
using ModularHost.Web.Services;
using ModularHost.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

// Add HttpContextAccessor
services.AddHttpContextAccessor();

// Configure Entity Framework with MySQL
var connectionString = configuration.GetConnectionString("Default");
var serverVersion = ServerVersion.AutoDetect(connectionString);
services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, serverVersion)
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors());

// Register repositories and unit of work
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register permission service
services.AddScoped<IPermissionService, PermissionService>();

// Register permission helper
services.AddScoped<IPermissionHelper, PermissionHelper>();

// Register audit logger based on configuration
var useMongo = configuration.GetValue<bool>("Audit:UseMongo");
if (useMongo)
{
    services.AddScoped<IAuditLogger, MongoAuditLogger>();
}
else
{
    services.AddScoped<IAuditLogger, MySqlAuditLogger>();
}

// Add hosted service for log archiving
services.AddHostedService<HourlyLogArchiverHostedService>();

// Configure Identity
services.AddIdentity<User, Role>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false; // Set to true in production
        
        // Two factor settings
        options.Tokens.EmailConfirmationTokenProvider = "Email";
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddTokenProvider<EmailTokenProvider<User>>("Email");

// Configure authentication cookie
services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// Register Email Service
services.AddScoped<IEmailService, EmailService>();

// Add memory cache for permissions
services.AddMemoryCache();

// Add SignalR for real-time notifications
services.AddSignalR();

// Configure MVC with dynamic module loading
var mvcBuilder = services.AddControllersWithViews();
var partManager = mvcBuilder.PartManager;

// Load modules dynamically
ModuleLoader.LoadModules(services, partManager);

// Add session support
services.AddDistributedMemoryCache();
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Build the application
var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Apply pending migrations
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
    
    // Seed initial data
    await ModularHost.Web.Core.Infrastructure.DataSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();

// Add custom authorization middleware
app.UseMiddleware<AuthorizationMiddleware>();

app.UseAuthorization();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
