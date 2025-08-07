# Dynamic Role and Menu Management System

A comprehensive ASP.NET Core MVC application with dynamic role-based access control and menu management system.

## Features

- **Dynamic Role Management**: Create, update, and delete roles dynamically
- **Dynamic Menu System**: Create hierarchical menus with automatic controller/action discovery
- **Permission-based Access Control**: Fine-grained permissions (View, Create, Edit, Delete) per menu item
- **User Management**: Extended Identity with custom user properties
- **N-Tier Architecture**: Clean separation of concerns with Core, Infrastructure, Application, and Web layers
- **Repository Pattern**: Generic repository with Unit of Work pattern
- **Result Pattern**: Consistent error handling across the application
- **Auto Dependency Injection**: Automatic service registration based on naming conventions

## Technology Stack

- ASP.NET Core 9.0 MVC
- Entity Framework Core 9.0
- MySQL (via Pomelo.EntityFrameworkCore.MySql)
- ASP.NET Core Identity
- Bootstrap 5

## Project Structure

```
DynamicRoleMenuSystem/
├── DynamicRoleMenuSystem.Core/        # Domain entities and interfaces
├── DynamicRoleMenuSystem.Infrastructure/  # Data access and external services
├── DynamicRoleMenuSystem.Application/ # Business logic and services
└── DynamicRoleMenuSystem.Web/         # MVC web application
```

## Setup Instructions

### Prerequisites

- .NET 9.0 SDK
- MySQL Server
- Visual Studio 2022 or VS Code

### Database Configuration

1. Update the connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DynamicRoleMenuDB;User=root;Password=yourpassword;"
  }
}
```

### Running the Application

1. Navigate to the Web project:
```bash
cd DynamicRoleMenuSystem.Web
```

2. Install EF Core tools (if not already installed):
```bash
dotnet tool install --global dotnet-ef
```

3. Create initial migration:
```bash
dotnet ef migrations add InitialCreate
```

4. Run the application:
```bash
dotnet run
```

The database will be created automatically with seed data on first run.

## Default Credentials

- **Admin Email**: admin@example.com
- **Admin Password**: Admin@123

## Key Features Explained

### 1. Dynamic Role CRUD
- Create custom roles with descriptions
- Assign/remove roles to/from users
- Soft delete functionality

### 2. Dynamic Menu System
- Hierarchical menu structure (parent-child relationships)
- Automatic discovery of areas, controllers, and actions
- Icon support for visual enhancement
- Order management for menu items

### 3. Permission Middleware
- Intercepts every request to check permissions
- Based on user roles and menu assignments
- Granular permissions: View, Create, Edit, Delete

### 4. Base Architecture
- **BaseController**: Common functionality for all controllers
- **BaseService**: Generic CRUD operations with Result pattern
- **BaseRepository**: Generic data access layer
- **Auto DI**: Services and repositories are registered automatically based on naming convention

### 5. Extended Identity
- Custom ApplicationUser with:
  - FullName
  - AvatarUrl
  - Description
  - IsActive flag
  - Timestamps (CreatedAt, UpdatedAt)

## Usage

### Creating a New Role
1. Navigate to Roles page
2. Click "Create New Role"
3. Enter role name and description
4. Save

### Creating a Menu
1. Navigate to Menus page
2. Click "Create New Menu"
3. Fill in menu details:
   - Name and Display Name
   - Select Area/Controller/Action (auto-discovered)
   - Choose parent menu (optional)
   - Set order and icon
4. Assign to roles
5. Save

### Assigning Permissions
1. Go to Menu Management
2. Select a menu item
3. Choose roles and set permissions
4. Save changes

## Security Features

- Password requirements enforced
- Cookie-based authentication
- Permission-based authorization
- Secure session management
- HTTPS enforcement in production

## Extension Points

The system is designed to be easily extended:

- Add new entities in Core layer
- Create services following naming convention (ends with "Service")
- Create repositories following naming convention (ends with "Repository")
- Auto DI will pick them up automatically

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is open source and available under the MIT License.