# Tailwind CSS Setup

This project uses Tailwind CSS for styling with automatic compilation on build.

## Setup

The project has been configured to automatically compile Tailwind CSS during the build process. No manual setup is required.

## Commands

### Build CSS once
```bash
npm run build-css
```

### Watch for changes (development)
```bash
npm run watch-css
# or
npm run dev
```

### Build project (includes CSS compilation)
```bash
dotnet build
```

## File Structure

- `tailwind.config.js` - Tailwind configuration
- `wwwroot/css/tailwind.input.css` - Input file with Tailwind directives
- `wwwroot/lib/tailwind/tailwind-compiled.css` - Compiled output CSS
- `package.json` - NPM scripts and dependencies

## How it Works

1. **Automatic Compilation**: When you run `dotnet build`, it automatically:
   - Installs npm packages if needed
   - Compiles Tailwind CSS to `wwwroot/lib/tailwind/tailwind-compiled.css`
   
2. **Development Mode**: In Debug configuration, the build also starts a watcher that recompiles CSS on changes

3. **Content Paths**: Tailwind scans these paths for classes:
   - `./Views/**/*.{cshtml,html,js}`
   - `./wwwroot/**/*.{html,js}`
   - `./Modules/**/Views/**/*.{cshtml,html,js}`
   - `./Areas/**/Views/**/*.{cshtml,html,js}`

## Customization

### Adding Custom Styles

Edit `wwwroot/css/tailwind.input.css` to add custom component styles:

```css
@layer components {
  .btn-custom {
    @apply px-4 py-2 bg-blue-500 text-white rounded;
  }
}
```

### Extending Theme

Edit `tailwind.config.js` to customize colors, fonts, etc:

```javascript
theme: {
  extend: {
    colors: {
      'brand': '#123456'
    }
  }
}
```

## Troubleshooting

- If styles aren't updating, run `npm run build-css` manually
- Make sure Node.js is installed on your system
- Check that npm packages are installed: `npm install`