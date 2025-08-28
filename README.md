# Modular ASP.NET Core MVC Role & Permission Framework

A complete modular ASP.NET Core MVC application with plug-and-play modules, role-based URL permissions, multi-role users, dynamic menu generation, audit logging, and modern UI with dark/light mode support.

## Features

### Core Features
- **Modular Architecture**: Plug-and-play module system with dynamic loading
- **Role-Based Permissions**: URL-based authorization with granular control
- **Multi-Role Users**: Users can have multiple roles simultaneously
- **Dynamic Menu System**: Hierarchical menu structure with parent/grandparent support
- **Audit Logging**: Dual support for MySQL and MongoDB with hourly archiving
- **Authentication**: Cookie-based authentication with 2FA support via email
- **Notifications**: Real-time notifications using SignalR
- **Dark/Light Mode**: Modern UI with Tailwind CSS and theme switching
- **Blog Module**: Example module with posts, comments, and replies

### Technical Features
- Dynamic module loading at startup
- Repository and Unit of Work patterns
- Base service classes for common operations
- Memory caching for permissions
- Automatic cache invalidation on role changes
- Dynamic database schema updates
- No EF migrations required (uses EnsureCreated)
- Super-admin bypass for all permissions

## Prerequisites

- .NET SDK 9.0 or later
- MySQL Server
- MongoDB (optional - for audit logs)
- Visual Studio 2022/2023 or VS Code

## Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/modular-mvc-framework.git
cd modular-mvc-framework
```

### 2. Configure Database Connection
Edit `ModularHost.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=modular_app;User=root;Password=YourPassword;"
  },
  "Audit": {
    "UseMongo": false,
    "MongoConnection": "mongodb://localhost:27017",
    "MongoDatabase": "auditdb"
  }
}
```

### 3. Initialize Database
Run the SQL script in `db-scripts/init.sql` to create initial data.

### 4. Build and Run
```bash
cd src
dotnet build
dotnet run --project ModularHost.Web
```

### 5. Access the Application
Open your browser and navigate to `http://localhost:5000`

**Default Admin Credentials:**
- Username: `admin`
- Password: `Admin123!`

## Project Structure

```
/ModularHost/
├── ModularHost.sln
├── /src/
│   ├── /ModularHost.Web/          # Main MVC host application
│   │   ├── /Controllers/          # Core controllers
│   │   ├── /Views/                # Razor views
│   │   ├── /Data/                 # DbContext and configurations
│   │   ├── /Services/             # Core services
│   │   ├── /Middleware/           # Custom middleware
│   │   ├── /Repositories/         # Repository implementations
│   │   ├── /Modules/              # Module assemblies folder
│   │   │   └── /BlogModule/       # Example blog module
│   │   └── /wwwroot/              # Static files
│   ├── /Shared/                   # Shared models and interfaces
│   └── /db-scripts/               # Database initialization scripts
```

## Creating a New Module

Modules are completely self-contained with their own models, views, controllers, and services. The core application has no dependencies on modules.

### 1. Create a Razor Class Library
```bash
cd ModularHost.Web/Modules
dotnet new classlib -n YourModule
```

### 2. Update Project File
Change the SDK to support Razor views:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>
  
  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <ProjectReference Include="../../../Shared/ModularHost.Shared.csproj" />
  </ItemGroup>
</Project>
```

### 3. Create Module Structure
```
/YourModule/
├── Areas/
│   └── YourModule/
│       └── Views/
│           ├── _ViewStart.cshtml
│           ├── _ViewImports.cshtml
│           └── YourController/
│               └── Index.cshtml
├── Controllers/
│   └── YourController.cs
├── Models/
│   └── YourModel.cs
├── Data/
│   └── YourModelConfiguration.cs
├── Services/
│   └── YourService.cs
└── ModuleInitializer.cs
```

### 4. Create Module Initializer
```csharp
namespace YourModule
{
    public static class ModuleInitializer
    {
        public static void Initialize(IServiceCollection services)
        {
            // Register module services
            services.AddScoped<YourService>();
            
            // Register module info
            services.AddSingleton<IModuleInfo>(new ModuleInfo
            {
                Name = "YourModule",
                Version = "1.0.0",
                Description = "Module description",
                Author = "Your Name"
            });
        }
    }
}
```

### 5. Create Controllers with Area
```csharp
[Area("YourModule")]
[Route("YourModule")]
public class YourController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }
}
```

### 6. Add Entity Configuration
```csharp
public class YourModelConfiguration : IEntityTypeConfiguration<YourModel>
{
    public void Configure(EntityTypeBuilder<YourModel> builder)
    {
        builder.ToTable("YourModels");
        // Configure your entity
    }
}
```

### 7. Build and Deploy
Build your module and copy the DLL to the `Modules` folder:
```bash
dotnet build
copy bin/Debug/net9.0/YourModule.dll ../../Modules/
```

The host will automatically:
- Load the module DLL
- Register services via ModuleInitializer
- Discover and register controllers
- Apply entity configurations to the database
- Serve views from the module

## Permission System

### URL-Based Authorization
Apply authorization attributes to controllers or actions:
```csharp
[UrlAuthorize(Policy = AuthorizationPolicyType.Authorize)]
public IActionResult SecureAction()
{
    // Only authorized users can access
}
```

### Policy Types
- `Anonymous`: Anyone can access
- `Authenticate`: Requires login
- `Authorize`: Requires specific permissions

### Managing Permissions
Permissions are stored in the `RolePermissions` table:
- `RoleId`: The role ID
- `Url`: URL pattern (supports wildcards)
- `HttpMethod`: Optional HTTP method restriction

## Audit Logging

The system supports dual logging to MySQL or MongoDB:

### MySQL Logging
- Logs stored in `Logs` table
- Archived hourly to `LogArchives` table

### MongoDB Logging
Enable in `appsettings.json`:
```json
{
  "Audit": {
    "UseMongo": true,
    "MongoConnection": "mongodb://localhost:27017",
    "MongoDatabase": "auditdb"
  }
}
```

## UI Components

### Dark/Light Mode Toggle
```javascript
toggleDarkMode(); // Toggle between themes
```

### Notification System
Real-time notifications via SignalR:
```javascript
showNotification('Title', 'Message', 'success');
```

### Tailwind CSS
The project uses Tailwind CSS via CDN. Customize in `wwwroot/css/site.css`.

## Security Considerations

1. **Password Hashing**: Uses BCrypt with salt
2. **CSRF Protection**: ValidateAntiForgeryToken on POST actions
3. **SQL Injection**: Protected via Entity Framework parameterization
4. **XSS Protection**: Razor automatically HTML-encodes output
5. **Cookie Security**: HttpOnly and Secure flags set

## Deployment

### Production Checklist
- [ ] Update connection strings
- [ ] Set strong JWT secret key
- [ ] Configure SMTP for email
- [ ] Enable HTTPS
- [ ] Set appropriate log levels
- [ ] Review and restrict CORS if needed
- [ ] Enable distributed caching (Redis) for scale-out

### Docker Support
Create a `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "ModularHost.Web.dll"]
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions, please use the GitHub issue tracker.

## Acknowledgments

- Built with ASP.NET Core 9.0
- Uses Pomelo.EntityFrameworkCore.MySql for MySQL support
- Tailwind CSS for modern UI
- SignalR for real-time features