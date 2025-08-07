# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ASP.NET Core 9.0 MVC application with Identity authentication, using MySQL as the database through Entity Framework Core. The application follows a layered architecture with Areas for modular organization.

## Database Configuration

The application uses MySQL 8.0.36 with Pomelo.EntityFrameworkCore.MySql provider. Connection string is configured in `appsettings.json`:
- Default database: `mvc_framework`
- Default credentials: root/123456 (localhost:3306)

## Architecture

### Core Components
- **ApplicationDbContext**: Main EF Core context inheriting from IdentityDbContext
- **Areas**: Modular features organized by area (e.g., Book, Identity)
- **Base Classes**: `BaseEntity`, `BaseController<T>`, `BaseService<T>`, `BaseRepository<T>` for common CRUD operations
- **Authentication**: ASP.NET Identity with custom ApplicationUser
- **Authorization**: Dynamic permission system using NavigationMenu and RoleMenuPermission tables

### Key Patterns
- Repository pattern with dependency injection
- Service layer between controllers and repositories
- ViewComponents for reusable UI elements (navigation menus)
- Areas for feature separation

## Development Commands

### Build and Run
```bash
# Build the project
dotnet build

# Run the application (Development mode)
dotnet run --project Mvc.Framework

# Run with specific URL
dotnet run --project Mvc.Framework --urls "https://localhost:5001;http://localhost:5000"
```

### Database Operations
```bash
# Create a new migration
dotnet ef migrations add <MigrationName> --project Mvc.Framework

# Update database
dotnet ef database update --project Mvc.Framework

# Remove last migration
dotnet ef migrations remove --project Mvc.Framework
```

### Testing
```bash
# Run tests (when available)
dotnet test
```

## Default Users and Roles

The application seeds these default users (all with password: `P@ssw0rd`):
- admin@test.com (Admin role)
- manager@test.com (Manager role)
- employee@test.com (Employee role)

## Project Structure

- **Areas/**: Feature modules
  - **Book/**: Sample module with Category CRUD
  - **Identity/**: ASP.NET Identity pages
- **Controllers/**: Main controllers (Home, Admin, Base)
- **Data/**: EF Core context and repositories
- **Services/**: Business logic layer
- **ViewComponents/**: Reusable UI components
- **wwwroot/**: Static files (CSS, JS, libraries)
- **Handlers/**: Authorization handlers and policies

## Important Files

- `Program.cs`: Application configuration and DI setup
- `DbInitializer.cs`: Database seeding logic
- `ApplicationDbContext.cs`: EF Core context configuration
- `appsettings.json`: Configuration including connection strings

## Adding New Features

When adding new modules:
1. Create a new Area with its own Controllers, Views, Models structure
2. Register services in Program.cs DI container
3. Add DbSet to ApplicationDbContext if new entities are needed
4. Create migrations for database changes
5. Update NavigationMenu entries in DbInitializer for menu items

## DataTables Integration

The project has DataTables library files in:
- CSS: `wwwroot/css/datatables-bootstrap5.css`
- JS: `wwwroot/js/datatables-bootstrap5.js`

To use DataTables on any table, add the DataTable initialization script to the view.