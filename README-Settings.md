# Dynamic Application Settings

This application features a comprehensive settings system that allows you to customize every aspect of your application's branding, theming, and functionality through configuration files.

## 🎨 Customization Features

### Branding Settings
- **Organization Name & Logo**: Set your company name, logo, and favicon
- **Contact Information**: Configure email, phone, address, and social media links
- **Version Information**: Display custom version numbers and slogans

### Theme System
- **Color Scheme**: Customize primary, secondary, and semantic colors
- **Typography**: Set custom fonts for body text, headings, and code
- **Dark/Light Mode**: Enable theme switching with user preferences
- **CSS Variables**: Dynamic theming with instant updates

### Layout Configuration
- **Header**: Control breadcrumbs, search, notifications, and user profile display
- **Sidebar**: Configure width, collapsibility, icons, and tooltips
- **Footer**: Customize copyright text, links, version display, and social media
- **Content**: Set max width, padding, and page title visibility

### Feature Toggles
- **Authentication**: Enable/disable user registration, email confirmation
- **Security**: Configure session timeout, password policies, 2FA
- **Dashboard**: Control welcome messages, statistics, and activity feeds
- **Performance**: Toggle caching, compression, and rate limiting

## 🚀 Quick Setup

### 1. Basic Configuration
Edit the `AppSettings` section in `appsettings.json`:

```json
{
  "AppSettings": {
    "Branding": {
      "OrganizationName": "Your Company Name",
      "ShortName": "YCN",
      "Slogan": "Your company slogan",
      "LogoPath": "/images/your-logo.png",
      "Version": "1.0.0"
    }
  }
}
```

### 2. Theme Customization
Change colors to match your brand:

```json
{
  "AppSettings": {
    "Theme": {
      "Colors": {
        "Primary": "#your-brand-color",
        "Secondary": "#your-secondary-color",
        "Success": "#10b981",
        "Danger": "#ef4444"
      }
    }
  }
}
```

### 3. Layout Adjustments
Customize the layout to fit your needs:

```json
{
  "AppSettings": {
    "Layout": {
      "Header": {
        "ShowSearch": true,
        "ShowNotifications": false,
        "Height": "60px"
      },
      "Footer": {
        "Show": true,
        "ShowVersion": true
      }
    }
  }
}
```

## 📋 Complete Configuration Reference

See `sample-custom-settings.json` for a complete configuration example with all available options.

## 🎯 How It Works

### 1. Configuration Binding
The application automatically binds the `AppSettings` section to a strongly-typed configuration object:

```csharp
// Program.cs
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
```

### 2. Dynamic Injection
Settings are injected into views and components using the options pattern:

```csharp
@inject IOptions<AppSettings> AppSettingsOptions
```

### 3. Real-time Updates
CSS variables are generated dynamically based on your configuration:

```css
:root {
    --bs-primary: #your-color;
    --app-font-family: 'Your Font';
    --sidebar-width: 280px;
}
```

### 4. ViewComponents
Reusable components automatically adapt to your settings:

```html
@await Component.InvokeAsync("AppBranding", new { type = "compact" })
@await Component.InvokeAsync("ThemeStyles")
```

## 🛠️ Advanced Customization

### Custom Branding Types
The `AppBranding` ViewComponent supports multiple display types:
- `"full"` - Complete branding with logo, name, and slogan
- `"compact"` - Logo and short name for headers
- `"logo-only"` - Just the logo
- `"text-only"` - Text-based branding
- `"sidebar"` - Optimized for sidebar display

### Environment-Specific Settings
Use different configuration files for different environments:

```json
// appsettings.Development.json
{
  "AppSettings": {
    "Branding": {
      "OrganizationName": "YourApp (Development)",
      "Version": "dev-build"
    },
    "Features": {
      "EnableUserRegistration": true
    }
  }
}
```

```json
// appsettings.Production.json
{
  "AppSettings": {
    "Security": {
      "RequireHttps": true,
      "EnableTwoFactor": true
    },
    "Features": {
      "EnableUserRegistration": false
    }
  }
}
```

## 🎨 Theme Examples

### Corporate Blue Theme
```json
{
  "Theme": {
    "Colors": {
      "Primary": "#1e40af",
      "Secondary": "#64748b",
      "Background": "#f8fafc",
      "Surface": "#ffffff"
    }
  }
}
```

### Modern Purple Theme
```json
{
  "Theme": {
    "Colors": {
      "Primary": "#7c3aed",
      "Secondary": "#a855f7",
      "Background": "#faf5ff",
      "Surface": "#f3e8ff"
    }
  }
}
```

### Dark Professional Theme
```json
{
  "Theme": {
    "Colors": {
      "Primary": "#3b82f6",
      "Background": "#111827",
      "Surface": "#1f2937",
      "OnBackground": "#f9fafb",
      "OnSurface": "#e5e7eb"
    },
    "DefaultTheme": "dark"
  }
}
```

## 📱 Responsive Design

All settings automatically adapt to different screen sizes:
- Mobile-first responsive design
- Collapsible sidebar on small screens
- Optimized navigation for touch devices
- Fluid typography scaling

## 🔄 Instant Updates

Changes to the configuration file are automatically reflected when you:
1. Restart the application
2. The IOptionsSnapshot pattern ensures settings are current
3. Theme changes are applied immediately via CSS variables
4. No code recompilation required for most settings

## 🎯 Best Practices

1. **Keep it Simple**: Start with basic branding and expand as needed
2. **Test Themes**: Verify colors work in both light and dark modes
3. **Mobile First**: Test layout changes on mobile devices
4. **Performance**: Enable caching and compression in production
5. **Security**: Use appropriate security settings for your environment
6. **Backup**: Keep copies of your custom configurations

## 🆘 Troubleshooting

**Settings not applying?**
- Check JSON syntax with a validator
- Ensure the AppSettings section exists
- Restart the application
- Check browser cache for CSS changes

**Colors not updating?**
- Verify color format (hex, rgb, etc.)
- Check CSS custom properties in browser dev tools
- Clear browser cache

**Layout issues?**
- Test on different screen sizes
- Check CSS conflicts with custom styles
- Verify Bootstrap version compatibility

## 📞 Support

For additional customization help or questions about the settings system, please refer to the documentation or contact support.

---

*This settings system provides maximum flexibility while maintaining ease of use. Customize your application to perfectly match your brand and requirements!*