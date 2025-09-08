using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using MRCMS.Core.Extensions;
using MRCMS.Core.Services;
using MRCMS.Core.Services.Interfaces;
using MRCMS.Core.Models;
using MRCMS.Core.Models.Entities;
using MRCMS.Core.Infrastructure;
using MRCMS.Middleware;
using MRCMS.Services;
using MRCMS.Services.Interfaces;
using Serilog;
using Serilog.Events;

// Configure Serilog
Serilog.Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information) // Log SQL commands
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .MinimumLevel.Override("MRCMS.Middleware", LogEventLevel.Debug) // Log middleware activities
    .Enrich.FromLogContext()
    .Enrich.WithProperty("MachineName", Environment.MachineName)
    .Enrich.WithProperty("ProcessId", Environment.ProcessId)
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760, // 10MB per file
        rollOnFileSizeLimit: true)
    .WriteTo.File(
        path: "logs/error-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] [{ProcessId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760,
        rollOnFileSizeLimit: true)
    .WriteTo.File(
        path: "logs/crash-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        restrictedToMinimumLevel: LogEventLevel.Fatal,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{MachineName}] [{ProcessId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}{NewLine}",
        fileSizeLimitBytes: 10485760,
        rollOnFileSizeLimit: true)
    .WriteTo.File(
        path: "logs/database-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        restrictedToMinimumLevel: LogEventLevel.Debug,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760,
        rollOnFileSizeLimit: true)
    .CreateLogger();

try
{
    Serilog.Log.Information("Starting MRCMS application");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog
    builder.Host.UseSerilog();

// Add services to the container.
var services = builder.Services;
var configuration = builder.Configuration;

// Add HttpContextAccessor
services.AddHttpContextAccessor();

// Register logging services
services.AddSingleton<Serilog.ILogger>(Serilog.Log.Logger);
services.AddScoped<ILoggerService, LoggerService>();
services.AddScoped<IEmailService, EmailService>();

// Register PermissionHelper
services.AddScoped<MRCMS.Core.Extensions.IPermissionHelper, MRCMS.Core.Extensions.PermissionHelper>();

// Register audit interceptor
services.AddScoped<AuditInterceptor>();

// Configure Entity Framework with MySQL
var connectionString = configuration.GetConnectionString("Default");
var serverVersion = ServerVersion.AutoDetect(connectionString);
services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var auditInterceptor = serviceProvider.GetService<AuditInterceptor>();
    
    options.UseMySql(connectionString, serverVersion, mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        })
        .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
    
    if (auditInterceptor != null)
    {
        options.AddInterceptors(auditInterceptor);
    }
});

// Register core infrastructure services (these are required before dynamic registration)
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register DataTable service
services.AddScoped<IDataTableService, DataTableService>();

// Register Settings service
services.AddScoped<ISettingsService, SettingsService>();

// Add Memory Cache for settings caching
services.AddMemoryCache();

// Add hosted service for log archiving (before dynamic registration to avoid conflicts)
services.AddHostedService<HourlyLogArchiverHostedService>();

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
    .AddTokenProvider<EmailTokenProvider<User>>("Email")
    .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

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

// Add memory cache for permissions
services.AddMemoryCache();

// Add SignalR for real-time notifications
services.AddSignalR();

// Configure AutoMapper
services.AddAutoMapper(typeof(Program).Assembly);

// Configure MVC with dynamic module loading
var mvcBuilder = services.AddControllersWithViews();

// Configure Razor view engine to look for views in modules
services.Configure<RazorViewEngineOptions>(options =>
{
    options.ViewLocationExpanders.Add(new MRCMS.Core.Infrastructure.ModularViewLocationExpander());
});
var partManager = mvcBuilder.PartManager;

// Register all services and repositories dynamically
// This will automatically discover and register:
// 1. All classes ending with "Service" or "Repository"
// 2. All classes inheriting from BaseService<T> or implementing IRepository<T>
// 3. Services from both main application and modules
services.RegisterDynamicServices();

// Load modules dynamically (this also registers module services)
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

// Log application startup
using (var startupScope = app.Services.CreateScope())
{
    var loggerService = startupScope.ServiceProvider.GetRequiredService<ILoggerService>();
    await loggerService.LogStartupAsync("Application starting - Environment: {Environment}, Version: {Version}", 
        app.Environment.EnvironmentName, 
        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0");
}

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Apply pending migrations
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
    
    // Seed core data
    await MRCMS.Core.Infrastructure.DataSeeder.SeedAsync(scope.ServiceProvider);
    
    // Execute module seeders
    await ModuleLoader.ExecuteModuleSeedersAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
// Add diagnostic middleware first to track all requests
app.UseMiddleware<DiagnosticMiddleware>();

// Add global exception handler middleware
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Add status code pages middleware for handling 404 and other status codes
app.UseStatusCodePagesWithReExecute("/Error/{0}");

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

// Register application lifetime events
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    Serilog.Log.Information("Application has started successfully | ProcessId: {ProcessId} | MachineName: {MachineName}", 
        Environment.ProcessId, Environment.MachineName);
});

lifetime.ApplicationStopping.Register(() =>
{
    Serilog.Log.Warning("Application is shutting down | ProcessId: {ProcessId}", Environment.ProcessId);
});

lifetime.ApplicationStopped.Register(() =>
{
    Serilog.Log.Warning("Application has stopped | ProcessId: {ProcessId}", Environment.ProcessId);
});

app.Run();
}
catch (Exception ex)
{
    Serilog.Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Serilog.Log.CloseAndFlush();
}
