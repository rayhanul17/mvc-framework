# Dark/Light Mode Theme Guide

## Quick Start: Adding New Elements with Theme Support

### Method 1: Use Theme Utility Classes (Easiest)

For any new element you add, simply use the pre-built theme classes:

```html
<!-- Card Example -->
<div class="theme-card">
    <div class="theme-card-header">
        <h3 class="theme-text">My New Feature</h3>
    </div>
    <div class="card-body">
        <p class="theme-text-secondary">This automatically works in both light and dark modes!</p>
        <button class="theme-btn">Click Me</button>
    </div>
</div>

<!-- Form Example -->
<form>
    <label class="theme-text">Username</label>
    <input type="text" class="theme-input" placeholder="Enter username">
    
    <label class="theme-text">Description</label>
    <textarea class="theme-input" rows="3"></textarea>
    
    <button type="submit" class="btn btn-primary">Submit</button>
</form>

<!-- Table Example -->
<table class="theme-table theme-table-striped">
    <thead>
        <tr>
            <th>Name</th>
            <th>Status</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>Item 1</td>
            <td><span class="theme-badge">Active</span></td>
        </tr>
    </tbody>
</table>
```

### Method 2: Use CSS Variables (For Custom Styling)

When creating custom CSS for new components, use theme variables:

```css
.my-custom-component {
    /* Backgrounds */
    background-color: var(--theme-bg-surface);
    
    /* Text Colors */
    color: var(--theme-text-primary);
    
    /* Borders */
    border: 1px solid var(--theme-border-primary);
    
    /* Shadows */
    box-shadow: var(--theme-shadow-sm);
    
    /* Transitions for smooth theme switching */
    transition: all 0.3s ease;
}

.my-custom-component:hover {
    background-color: var(--theme-overlay-hover);
    box-shadow: var(--theme-shadow-md);
}

.my-custom-component .subtitle {
    color: var(--theme-text-secondary);
}

.my-custom-component.disabled {
    color: var(--theme-text-disabled);
    background-color: var(--theme-overlay-hover);
}
```

### Method 3: Combine Bootstrap with Theme Classes

You can mix Bootstrap classes with theme classes:

```html
<div class="card theme-card mb-3">
    <div class="card-header theme-card-header d-flex justify-content-between">
        <h5 class="theme-text mb-0">Dashboard</h5>
        <span class="badge bg-primary">New</span>
    </div>
    <div class="card-body">
        <div class="row">
            <div class="col-md-6">
                <p class="theme-text">Primary text content</p>
                <small class="theme-text-tertiary">Supporting text</small>
            </div>
        </div>
    </div>
</div>
```

## Available Theme Variables

### Background Colors
- `--theme-bg-primary`: Main background (white/dark)
- `--theme-bg-secondary`: Secondary background
- `--theme-bg-tertiary`: Tertiary background
- `--theme-bg-surface`: Surface/card backgrounds
- `--theme-bg-surface-variant`: Alternative surface

### Text Colors
- `--theme-text-primary`: Main text color
- `--theme-text-secondary`: Secondary/muted text
- `--theme-text-tertiary`: Very muted text
- `--theme-text-disabled`: Disabled state text
- `--theme-text-inverse`: Inverse text color

### Borders
- `--theme-border-primary`: Main border color
- `--theme-border-secondary`: Subtle border color
- `--theme-border-focus`: Focus state border

### Overlays (for hover/active states)
- `--theme-overlay-hover`: Hover state overlay
- `--theme-overlay-focus`: Focus state overlay
- `--theme-overlay-selected`: Selected state overlay
- `--theme-overlay-activated`: Activated state overlay

### Shadows
- `--theme-shadow-sm`: Small shadow
- `--theme-shadow-md`: Medium shadow
- `--theme-shadow-lg`: Large shadow
- `--theme-shadow-xl`: Extra large shadow

## Available Utility Classes

### Basic Elements
- `.theme-bg`: Primary background
- `.theme-bg-secondary`: Secondary background
- `.theme-bg-tertiary`: Tertiary background
- `.theme-surface`: Surface background
- `.theme-surface-variant`: Variant surface
- `.theme-text`: Primary text
- `.theme-text-secondary`: Secondary text
- `.theme-text-tertiary`: Tertiary text
- `.theme-text-disabled`: Disabled text
- `.theme-border`: Primary border
- `.theme-border-secondary`: Secondary border

### Components
- `.theme-card`, `.theme-card-header`, `.theme-card-footer`
- `.theme-input` (for inputs, selects, textareas)
- `.theme-btn` (theme-aware button)
- `.theme-table`, `.theme-table-striped`
- `.theme-list`, `.theme-list-item`
- `.theme-modal`, `.theme-modal-header`, `.theme-modal-footer`
- `.theme-alert` (with variants: `-info`, `-success`, `-warning`, `-danger`)
- `.theme-badge`
- `.theme-code`, `.theme-code-block`
- `.theme-dropdown`, `.theme-dropdown-item`
- `.theme-progress`, `.theme-progress-bar`
- `.theme-tabs`, `.theme-tab`
- `.theme-pagination`, `.theme-page-link`
- `.theme-tooltip`

## Real-World Examples

### Example 1: Custom Dashboard Widget

```html
<div class="theme-card">
    <div class="theme-card-header">
        <h4 class="theme-text mb-0">Sales Overview</h4>
    </div>
    <div class="card-body">
        <div class="d-flex justify-content-between mb-3">
            <span class="theme-text">Total Revenue</span>
            <strong class="theme-text">$12,345</strong>
        </div>
        <div class="theme-progress">
            <div class="theme-progress-bar" style="width: 75%"></div>
        </div>
        <small class="theme-text-tertiary">75% of monthly goal</small>
    </div>
</div>
```

### Example 2: Custom Notification Component

```css
/* In your CSS file */
.notification-panel {
    background: var(--theme-bg-surface);
    border: 1px solid var(--theme-border-primary);
    border-radius: 8px;
    padding: 1rem;
    margin-bottom: 1rem;
    box-shadow: var(--theme-shadow-sm);
    transition: all 0.3s ease;
}

.notification-panel:hover {
    box-shadow: var(--theme-shadow-md);
    background: var(--theme-overlay-hover);
}

.notification-title {
    color: var(--theme-text-primary);
    font-weight: 600;
    margin-bottom: 0.5rem;
}

.notification-message {
    color: var(--theme-text-secondary);
    font-size: 0.9rem;
}

.notification-time {
    color: var(--theme-text-tertiary);
    font-size: 0.8rem;
}
```

```html
<!-- In your HTML -->
<div class="notification-panel">
    <div class="notification-title">New Message</div>
    <div class="notification-message">You have received a new message from John Doe</div>
    <div class="notification-time">5 minutes ago</div>
</div>
```

### Example 3: Dynamic JavaScript Component

```javascript
// Creating elements dynamically with theme support
function createThemeAwareCard(title, content) {
    const card = document.createElement('div');
    card.className = 'theme-card mb-3';
    
    card.innerHTML = `
        <div class="theme-card-header">
            <h5 class="theme-text mb-0">${title}</h5>
        </div>
        <div class="card-body">
            <p class="theme-text">${content}</p>
            <button class="theme-btn">Learn More</button>
        </div>
    `;
    
    return card;
}

// Add to page
const container = document.getElementById('content');
container.appendChild(createThemeAwareCard('Dynamic Card', 'This card automatically supports dark/light mode!'));
```

## Best Practices

1. **Always use theme variables or classes** for colors, backgrounds, and borders
2. **Include transitions** in your custom CSS for smooth theme switching
3. **Test in both modes** after adding new elements
4. **Use semantic variable names** (e.g., `--theme-text-primary` not `--theme-white`)
5. **Combine with Bootstrap** classes for layout, use theme classes for colors
6. **Don't hardcode colors** - always reference theme variables

## Testing Your New Components

1. Add your component to a page
2. Toggle between light and dark modes using the theme toggle button
3. Verify all text is readable in both modes
4. Check hover/focus states work correctly
5. Ensure borders and shadows are visible in both themes

## Common Pitfalls to Avoid

❌ **Don't do this:**
```css
.my-component {
    background: white;
    color: black;
    border: 1px solid #ccc;
}
```

✅ **Do this instead:**
```css
.my-component {
    background: var(--theme-bg-surface);
    color: var(--theme-text-primary);
    border: 1px solid var(--theme-border-primary);
}
```

❌ **Don't hardcode dark mode separately:**
```css
.my-component { background: white; }
.dark-theme .my-component { background: #2d2d2d; }
```

✅ **Use variables that auto-switch:**
```css
.my-component {
    background: var(--theme-bg-surface);
}
```

## File Locations

- **Theme Variables CSS**: `/wwwroot/css/theme-variables.css`
- **Main Site CSS**: `/wwwroot/css/site.css`
- **Layout File**: `/Views/Shared/_Layout.cshtml`

## Support

If you need to add a new color or variable:
1. Add it to both `:root` (light) and `.dark-theme` (dark) sections in `theme-variables.css`
2. Use the new variable in your components
3. Test in both themes

Remember: Every element you add using these theme classes or variables will automatically work in both light and dark modes without any additional code!