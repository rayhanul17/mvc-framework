using Nexora.Core.Common;
using Nexora.Infrastructure.Extensions;
using Nexora.Application.Interfaces;
using Nexora.Application.Services;
using Nexora.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using Nexora.Infrastructure.Data;
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

// Add File Document Service
builder.Services.AddScoped<IFileDocumentService, FileDocumentService>();

// Add Comment Service
builder.Services.AddScoped<ICommentService, CommentService>();

// Add Background Services
builder.Services.AddHostedService<Nexora.Web.Services.LogArchiveBackgroundService>();
builder.Services.AddHostedService<Nexora.Web.Services.HeartbeatService>();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Error/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    
    // Custom redirect behavior for AJAX requests
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") || 
            context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = 401;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
    
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") || 
            context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.StatusCode = 403;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }
        return Task.CompletedTask;
    };
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
        // Handle exceptions and display custom error page
        app.UseExceptionHandler("/Error/ServerError");
        
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        // In development, show detailed error page
        app.UseDeveloperExceptionPage();
    }

// Custom error handling middleware for status codes
app.UseStatusCodePagesWithReExecute("/Error/{0}");

// Store original path for error pages
app.Use(async (context, next) =>
{
    context.Items["originalPath"] = context.Request.Path.Value;
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Add Permission Middleware
app.UseMiddleware<PermissionMiddleware>();

// Blog post details route with slug support
app.MapControllerRoute(
    name: "blogpost-details",
    pattern: "BlogPost/Details/{id:int}/{slug?}",
    defaults: new { controller = "BlogPost", action = "Details" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        // Get the database context
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Apply any pending migrations automatically
        Log.Information("Checking for pending database migrations...");
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        
        if (pendingMigrations.Any())
        {
            Log.Information($"Found {pendingMigrations.Count()} pending migration(s). Applying now...");
            await context.Database.MigrateAsync();
            Log.Information("Database migrations applied successfully");
        }
        else
        {
            Log.Information("Database is up to date - no pending migrations");
        }
        
        // Initialize database with seed data
        await Nexora.Infrastructure.Data.DbInitializer.InitializeAsync(services);
        
        // Ensure Site Settings menu exists and is assigned to SuperAdmin
        await Nexora.Web.Data.EnsureSiteSettingsMenu.EnsureMenuExistsAsync(services);
        
        // Update Audit Log menu to use new controller
        await Nexora.Web.Data.UpdateAuditLogMenuSeed.UpdateAuditLogMenu(context);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations");
        
        // In development, you might want to throw to see the error
        if (app.Environment.IsDevelopment())
        {
            throw;
        }
        // In production, log the error but continue (the app might still work with existing DB)
    }
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
