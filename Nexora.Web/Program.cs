using Nexora.Core.Common;
using Nexora.Infrastructure.Extensions;
using Nexora.Application.Interfaces;
using Nexora.Application.Services;
using Nexora.Web.Middleware;
using Serilog;
using Serilog.Events;

// Configure Serilog with multiple sinks
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(evt => evt.Properties.ContainsKey("SourceContext") && 
                                       evt.Properties["SourceContext"].ToString().Contains("Heartbeat"))
        .WriteTo.File(
            path: "Logs/heartbeat-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [HEARTBEAT] {Message:lj}{NewLine}",
            retainedFileCountLimit: 7))
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    Log.Warning("Application startup - This is a test warning to verify logging configuration");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog
    builder.Host.UseSerilog();

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
builder.Services.AddScoped<Nexora.Core.Interfaces.ILogRepository, Nexora.Infrastructure.Repositories.LogRepository>();
builder.Services.AddScoped<ILogService, LogService>();

// Add Background Services
builder.Services.AddHostedService<Nexora.Web.Services.LogArchiveBackgroundService>();
builder.Services.AddHostedService<Nexora.Web.Services.HeartbeatService>();

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

    // Add Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

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
    await Nexora.Infrastructure.Data.DbInitializer.InitializeAsync(scope.ServiceProvider);
    
    // Ensure Site Settings menu exists and is assigned to SuperAdmin
    await Nexora.Web.Data.EnsureSiteSettingsMenu.EnsureMenuExistsAsync(scope.ServiceProvider);
    
    // Database initialization is handled by DbInitializer
}

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
