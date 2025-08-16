# Nexora - Enterprise MVC Framework

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square)](https://docs.microsoft.com/en-us/aspnet/core/)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=flat-square)](https://docs.microsoft.com/en-us/ef/core/)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.1-7952B3?style=flat-square&logo=bootstrap)](https://getbootstrap.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

## 📋 Table of Contents
- [Overview](#-overview)
- [Why Nexora?](#-why-nexora)
- [Features](#-features)
- [Architecture](#-architecture)
- [Technology Stack](#-technology-stack)
- [Getting Started](#-getting-started)
- [Project Structure](#-project-structure)
- [Key Components](#-key-components)
- [Configuration](#-configuration)
- [Contributing](#-contributing)
- [License](#-license)

## 🎯 Overview

**Nexora** is a modern, enterprise-ready MVC framework built on ASP.NET Core 8.0. It provides a robust foundation for building scalable web applications with a focus on clean architecture, modularity, and developer productivity. The framework includes pre-built modules for authentication, role-based access control, dynamic menu systems, content management, and customer support ticketing.

### Mission Statement
To provide developers with a production-ready framework that eliminates boilerplate code while maintaining flexibility and following best practices in software architecture.

## 💡 Why Nexora?

### The Name
**Nexora** combines two powerful concepts:
- **Nexus** - representing a central connection point, reflecting the framework's role as a hub connecting various business modules and areas
- **Aurora/Era** - symbolizing a new dawn or era in development, emphasizing the modern, forward-thinking approach

The name was chosen to reflect:
- **Connectivity**: Seamless integration between different modules and areas
- **Innovation**: Modern architecture and cutting-edge technology
- **Scalability**: Built to grow from small projects to enterprise solutions
- **Professionalism**: A memorable, brandable name suitable for business applications

### Philosophy
Nexora embodies the principle of "Convention over Configuration" while maintaining the flexibility to customize when needed. It's designed to be the nexus point where business requirements meet technical excellence.

## ✨ Features

### Core Features
- 🔐 **Advanced Authentication & Authorization**
  - ASP.NET Core Identity integration
  - Role-based access control (RBAC)
  - Dynamic permission system
  - Multi-factor authentication support

- 📊 **Dynamic Menu System**
  - Database-driven menu configuration
  - Role-based menu visibility
  - Hierarchical menu structure
  - Real-time menu updates

- 🎨 **Theme Management**
  - Dark/Light mode toggle
  - Customizable color schemes
  - Responsive design
  - Persistent theme preferences

- 📝 **Content Management**
  - Blog system with categories
  - Rich text editing
  - SEO-friendly URLs
  - Media management

- 🎫 **Customer Support Module**
  - Ticket management system
  - Priority-based routing
  - Agent assignment
  - Customer communication portal

### Technical Features
- 🏗️ **Clean Architecture**
  - Separation of concerns
  - Dependency injection
  - Repository pattern
  - Unit of Work pattern

- 📊 **Comprehensive Logging**
  - Audit trail system
  - Activity logging
  - Error tracking
  - Performance monitoring

- 🔧 **Developer Friendly**
  - Modular structure
  - Extensive documentation
  - Code scaffolding
  - Hot reload support

## 🏛️ Architecture

Nexora follows Domain-Driven Design (DDD) principles with a clean architecture approach:

```
┌─────────────────────────────────────────────────┐
│                 Presentation Layer               │
│                  (Nexora.Web)                   │
├─────────────────────────────────────────────────┤
│                Application Layer                 │
│              (Nexora.Application)               │
├─────────────────────────────────────────────────┤
│                Infrastructure Layer              │
│            (Nexora.Infrastructure)              │
├─────────────────────────────────────────────────┤
│                  Domain Layer                    │
│                 (Nexora.Core)                   │
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities

- **Nexora.Core**: Domain entities, interfaces, enums, and business logic
- **Nexora.Infrastructure**: Data access, external services, persistence
- **Nexora.Application**: Application services, DTOs, business workflows
- **Nexora.Web**: Controllers, views, API endpoints, middleware

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **ORM**: Entity Framework Core 8.0
- **Database**: MySQL (via Pomelo.EntityFrameworkCore.MySql)
- **Authentication**: ASP.NET Core Identity
- **Dependency Injection**: Built-in .NET DI Container

### Frontend
- **View Engine**: Razor Pages
- **CSS Framework**: Bootstrap 5.1
- **JavaScript**: Vanilla JS with jQuery
- **Icons**: Font Awesome 6.4
- **Fonts**: Inter (Google Fonts)

### Development Tools
- **IDE**: Visual Studio 2022 / VS Code
- **Package Manager**: NuGet
- **Version Control**: Git
- **Database Migrations**: EF Core Migrations

## 📁 Project Structure

```
Nexora/
├── Nexora.Core/                 # Domain Layer
│   ├── Common/                  # Common utilities and results
│   ├── Entities/                # Domain entities
│   ├── Enums/                   # Enumerations
│   └── Interfaces/              # Domain interfaces
│
├── Nexora.Infrastructure/       # Infrastructure Layer
│   ├── Data/                    # DbContext and configurations
│   ├── Repositories/            # Repository implementations
│   └── Extensions/              # Service extensions
│
├── Nexora.Application/          # Application Layer
│   ├── Services/                # Application services
│   └── Interfaces/              # Service interfaces
│
├── Nexora.Web/                  # Presentation Layer
│   ├── Areas/                   # Feature areas (e.g., CustomerSupport)
│   ├── Controllers/             # MVC controllers
│   ├── Views/                   # Razor views
│   ├── ViewComponents/          # Reusable view components
│   ├── Middleware/              # Custom middleware
│   ├── wwwroot/                 # Static files (CSS, JS, images)
│   └── Program.cs               # Application entry point
│
└── Nexora.sln                   # Solution file
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- MySQL Server 8.0 or later
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/nexora.git
   cd nexora
   ```

2. **Configure the database connection**
   
   Update `appsettings.json` in `Nexora.Web`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=NexoraDB;User=root;Password=yourpassword;"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   dotnet ef database update --project Nexora.Web
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run --project Nexora.Web
   ```

6. **Access the application**
   
   Navigate to `https://localhost:7184` or `http://localhost:5266`

### Default Credentials
- **Email**: admin@nexora.com
- **Password**: Admin@123

## 🔑 Key Components

### Dynamic Menu System
Allows administrators to create and manage application menus through the database:
- Hierarchical menu structure
- Role-based visibility
- Icon support
- Dynamic URL generation

### Role Management
Comprehensive role and permission system:
- Create custom roles
- Assign permissions dynamically
- User-role mapping
- Role hierarchies

### Audit System
Complete audit trail for all entity changes:
- Track create, update, delete operations
- Store old and new values
- User identification
- Timestamp tracking

### Theme System
Customizable theming with:
- Dark/Light mode toggle
- Custom color schemes
- Persistent preferences
- Responsive design

### Customer Support Module
Integrated ticketing system:
- Ticket creation and management
- Priority levels
- Agent assignment
- Comment threads
- File attachments
- Email notifications

## ⚙️ Configuration

### Application Settings
Key configuration files:
- `appsettings.json` - Main configuration
- `appsettings.Development.json` - Development environment settings
- Site settings stored in database for runtime configuration

### Environment Variables
Supported environment variables:
- `ASPNETCORE_ENVIRONMENT` - Set to Development, Staging, or Production
- `ASPNETCORE_URLS` - Configure listening URLs

### Database Configuration
Supports multiple database providers:
- MySQL (default)
- SQL Server (with minor modifications)
- PostgreSQL (with minor modifications)

### Site Settings
Dynamic settings manageable through UI:
- Organization name and details
- Email configuration
- Theme colors
- Feature toggles
- SEO settings

## 🤝 Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards
- Follow C# coding conventions
- Use meaningful variable and method names
- Write XML documentation for public methods
- Include unit tests for new features
- Ensure all tests pass before submitting PR

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Raj** - *Initial work and architecture*

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Bootstrap team for the responsive CSS framework
- All contributors who help improve Nexora

## 📞 Support

For support, please create an issue in the GitHub repository or contact the maintainers.

---

**Built with ❤️ using ASP.NET Core**