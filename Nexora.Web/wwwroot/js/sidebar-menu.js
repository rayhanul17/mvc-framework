/**
 * NEXORA SIDEBAR MENU SYSTEM
 * Version: 2.0
 * 
 * A clean, modular sidebar navigation system with:
 * - Mobile responsive behavior
 * - Touch gesture support
 * - Theme switching
 * - Search functionality
 * - State persistence
 */

(function() {
    'use strict';

    // ========================================
    // CONFIGURATION
    // ========================================
    const CONFIG = {
        breakpoints: {
            mobile: 768,
            tablet: 992
        },
        storage: {
            sidebarState: 'sidebarCollapsed',
            theme: 'theme'
        },
        animation: {
            duration: 300,
            swipeThreshold: 50
        },
        selectors: {
            sidebar: '#sidebar',
            content: '#content',
            sidebarToggle: '#sidebarCollapse',
            mobileMenuBtn: '#mobileMenuBtn',
            mobileOverlay: '#mobileMenuOverlay',
            themeToggle: '#theme-toggle',
            mobileThemeToggle: '#mobile-theme-toggle',
            searchInput: '#menuSearchInput',
            dropdownToggles: '.sidebar-dropdown-toggle'
        }
    };

    // ========================================
    // STATE MANAGEMENT
    // ========================================
    const state = {
        isMobile: false,
        isTablet: false,
        isCollapsed: false,
        isMobileMenuOpen: false,
        currentTheme: 'light',
        touchStartX: 0,
        touchEndX: 0
    };

    // ========================================
    // DOM ELEMENTS CACHE
    // ========================================
    const elements = {};

    // ========================================
    // INITIALIZATION
    // ========================================
    function init() {
        cacheElements();
        loadSavedState();
        bindEvents();
        updateResponsiveState();
        initializeTheme();
        initializeDropdowns();
    }

    /**
     * Cache DOM elements for better performance
     */
    function cacheElements() {
        Object.keys(CONFIG.selectors).forEach(key => {
            elements[key] = document.querySelector(CONFIG.selectors[key]);
        });
        
        // Additional elements
        elements.body = document.body;
        elements.themeIcon = document.getElementById('theme-icon');
        elements.mobileThemeIcon = document.getElementById('mobile-theme-icon');
        elements.dropdowns = document.querySelectorAll('.sidebar-dropdown');
    }

    /**
     * Load saved state from localStorage
     */
    function loadSavedState() {
        // Load sidebar collapsed state
        const savedCollapsed = localStorage.getItem(CONFIG.storage.sidebarState);
        if (savedCollapsed === 'true' && !isMobileView()) {
            state.isCollapsed = true;
            elements.sidebar?.classList.add('collapsed');
            elements.content?.classList.add('expanded');
        }

        // Load theme
        const savedTheme = localStorage.getItem(CONFIG.storage.theme) || 'light';
        state.currentTheme = savedTheme;
    }

    // ========================================
    // EVENT BINDING
    // ========================================
    function bindEvents() {
        // Sidebar toggle (desktop)
        elements.sidebarToggle?.addEventListener('click', toggleSidebar);

        // Mobile menu
        elements.mobileMenuBtn?.addEventListener('click', toggleMobileMenu);
        elements.mobileOverlay?.addEventListener('click', closeMobileMenu);

        // Theme toggle
        elements.themeToggle?.addEventListener('click', toggleTheme);
        elements.mobileThemeToggle?.addEventListener('click', toggleTheme);

        // Search
        elements.searchInput?.addEventListener('input', handleSearch);
        elements.searchInput?.addEventListener('keydown', handleSearchKeydown);

        // Window events
        window.addEventListener('resize', debounce(handleResize, 250));
        window.addEventListener('orientationchange', handleResize);

        // Touch events for swipe gestures
        if ('ontouchstart' in window) {
            document.addEventListener('touchstart', handleTouchStart, { passive: true });
            document.addEventListener('touchend', handleTouchEnd, { passive: true });
        }

        // Keyboard events
        document.addEventListener('keydown', handleKeyboard);

        // Menu link clicks
        document.addEventListener('click', handleMenuClick);

        // Page unload
        window.addEventListener('beforeunload', handleBeforeUnload);
    }

    // ========================================
    // SIDEBAR FUNCTIONALITY
    // ========================================
    
    /**
     * Toggle sidebar collapsed state (desktop)
     */
    function toggleSidebar() {
        if (isMobileView()) return;

        state.isCollapsed = !state.isCollapsed;
        
        elements.sidebar?.classList.toggle('collapsed');
        elements.content?.classList.toggle('expanded');
        
        localStorage.setItem(CONFIG.storage.sidebarState, state.isCollapsed);
        
        // Dispatch custom event
        dispatchEvent('sidebarToggled', { collapsed: state.isCollapsed });
    }

    /**
     * Initialize dropdown menus
     */
    function initializeDropdowns() {
        // Mark active menu and expand parent dropdowns on page load
        markActiveMenu();
        
        // Handle dropdown toggle clicks
        document.querySelectorAll('.sidebar-dropdown-toggle').forEach(toggle => {
            toggle.addEventListener('click', function(e) {
                e.preventDefault();
                const dropdown = this.closest('.sidebar-dropdown');
                
                // Close other dropdowns if not holding Ctrl
                if (!e.ctrlKey) {
                    document.querySelectorAll('.sidebar-dropdown.active')
                        .forEach(d => {
                            if (d !== dropdown) d.classList.remove('active');
                        });
                }
                
                dropdown?.classList.toggle('active');
            });
        });
        
        // Handle clicks on non-dropdown menu items (like Dashboard)
        // These should collapse all dropdowns when clicked
        document.querySelectorAll('.sidebar-item:not(.sidebar-dropdown) > .sidebar-link').forEach(link => {
            link.addEventListener('click', function(e) {
                // Don't prevent default - we want navigation to happen
                // Just collapse all dropdowns
                collapseAllDropdowns();
            });
        });
    }
    
    /**
     * Mark active menu based on current URL
     */
    function markActiveMenu() {
        const currentPath = window.location.pathname.toLowerCase();
        const currentUrl = window.location.pathname + window.location.search;
        
        // First, collapse ALL dropdowns
        document.querySelectorAll('.sidebar-dropdown.active').forEach(dropdown => {
            dropdown.classList.remove('active');
        });
        
        // Remove all active menu items
        document.querySelectorAll('.sidebar-item.active').forEach(el => {
            el.classList.remove('active');
        });
        
        // Find and mark active menu
        let activeFound = false;
        document.querySelectorAll('.sidebar-link').forEach(link => {
            const href = link.getAttribute('href');
            if (!href) return;
            
            const linkPath = href.toLowerCase();
            const isActive = currentPath === linkPath || currentUrl === href;
            
            if (isActive && !activeFound) {
                const item = link.closest('.sidebar-item');
                item?.classList.add('active');
                
                // ONLY expand parent dropdown if this item is inside a dropdown
                const parentDropdown = link.closest('.sidebar-dropdown');
                if (parentDropdown) {
                    parentDropdown.classList.add('active');
                }
                
                activeFound = true;
            }
        });
        
        // If no exact match found, try to match by controller/area
        if (!activeFound) {
            const pathParts = currentPath.split('/').filter(p => p);
            if (pathParts.length > 0) {
                document.querySelectorAll('.sidebar-link').forEach(link => {
                    const href = link.getAttribute('href');
                    if (!href) return;
                    
                    const linkParts = href.toLowerCase().split('/').filter(p => p);
                    // Check if first part matches (controller or area)
                    if (linkParts[0] === pathParts[0]) {
                        const item = link.closest('.sidebar-item');
                        item?.classList.add('active');
                        
                        const parentDropdown = link.closest('.sidebar-dropdown');
                        if (parentDropdown) {
                            parentDropdown.classList.add('active');
                        }
                        activeFound = true;
                        return false; // Break the loop
                    }
                });
            }
        }
    }

    // ========================================
    // MOBILE MENU FUNCTIONALITY
    // ========================================
    
    /**
     * Toggle mobile menu
     */
    function toggleMobileMenu(e) {
        if (e) {
            e.preventDefault();
            e.stopPropagation();
        }

        if (state.isMobileMenuOpen) {
            closeMobileMenu();
        } else {
            openMobileMenu();
        }
    }

    /**
     * Open mobile menu
     */
    function openMobileMenu() {
        state.isMobileMenuOpen = true;
        
        elements.sidebar?.classList.add('mobile-show');
        elements.mobileOverlay?.classList.add('show');
        elements.body?.classList.add('mobile-menu-open');
        
        // Prevent body scroll on iOS
        const scrollY = window.scrollY;
        elements.body.style.position = 'fixed';
        elements.body.style.top = `-${scrollY}px`;
        elements.body.style.width = '100%';
        
        dispatchEvent('mobileMenuOpened');
    }

    /**
     * Close mobile menu
     */
    function closeMobileMenu() {
        if (!state.isMobileMenuOpen) return;
        
        state.isMobileMenuOpen = false;
        
        elements.sidebar?.classList.remove('mobile-show');
        elements.mobileOverlay?.classList.remove('show');
        elements.body?.classList.remove('mobile-menu-open');
        
        // Restore body scroll
        const scrollY = elements.body.style.top;
        elements.body.style.position = '';
        elements.body.style.top = '';
        elements.body.style.width = '';
        window.scrollTo(0, parseInt(scrollY || '0') * -1);
        
        dispatchEvent('mobileMenuClosed');
    }

    // ========================================
    // THEME FUNCTIONALITY
    // ========================================
    
    /**
     * Initialize theme
     */
    function initializeTheme() {
        applyTheme(state.currentTheme);
    }

    /**
     * Toggle theme between light and dark
     */
    function toggleTheme() {
        state.currentTheme = state.currentTheme === 'dark' ? 'light' : 'dark';
        applyTheme(state.currentTheme);
        localStorage.setItem(CONFIG.storage.theme, state.currentTheme);
    }

    /**
     * Apply theme to the document
     */
    function applyTheme(theme) {
        const isDark = theme === 'dark';
        
        elements.body.classList.toggle('dark-theme', isDark);
        
        // Update icons
        if (elements.themeIcon) {
            elements.themeIcon.className = isDark ? 'fas fa-sun' : 'fas fa-moon';
        }
        if (elements.mobileThemeIcon) {
            elements.mobileThemeIcon.className = isDark ? 'fas fa-sun' : 'fas fa-moon';
        }
        
        // Update theme text
        document.querySelectorAll('.theme-text').forEach(text => {
            text.textContent = isDark ? 'Light Mode' : 'Dark Mode';
        });
        
        // Update CSS variables
        updateCSSVariables(isDark);
        
        dispatchEvent('themeChanged', { theme });
    }

    /**
     * Update CSS variables for theme
     */
    function updateCSSVariables(isDark) {
        const root = document.documentElement;
        
        if (isDark) {
            root.style.setProperty('--app-bg', '#1a1a1a');
            root.style.setProperty('--app-surface', '#2d2d2d');
            root.style.setProperty('--app-on-background', '#ffffff');
            root.style.setProperty('--app-on-surface', '#ffffff');
            root.style.setProperty('--bs-light', '#404040');
        } else {
            root.style.setProperty('--app-bg', '#ffffff');
            root.style.setProperty('--app-surface', '#f8f9fa');
            root.style.setProperty('--app-on-background', '#212529');
            root.style.setProperty('--app-on-surface', '#212529');
            root.style.setProperty('--bs-light', '#f8f9fa');
        }
    }

    // ========================================
    // SEARCH FUNCTIONALITY
    // ========================================
    
    /**
     * Handle search input
     */
    function handleSearch(e) {
        const searchTerm = e.target.value.toLowerCase().trim();
        filterMenuItems(searchTerm);
    }

    /**
     * Handle search keyboard events
     */
    function handleSearchKeydown(e) {
        if (e.key === 'Escape') {
            e.target.value = '';
            filterMenuItems('');
        }
    }

    /**
     * Filter menu items based on search term
     */
    function filterMenuItems(searchTerm) {
        const menuItems = document.querySelectorAll('.sidebar-item');
        
        // Reset all items
        menuItems.forEach(item => {
            item.style.display = '';
            removeHighlights(item);
        });
        
        // Close all dropdowns
        if (searchTerm === '') {
            document.querySelectorAll('.sidebar-dropdown').forEach(dropdown => {
                dropdown.classList.remove('active');
            });
            return;
        }
        
        // Filter and highlight
        menuItems.forEach(item => {
            const text = item.textContent.toLowerCase();
            const matches = text.includes(searchTerm);
            
            if (matches) {
                item.style.display = '';
                highlightText(item, searchTerm);
                
                // Expand parent dropdown if child matches
                const parentDropdown = item.closest('.sidebar-dropdown');
                if (parentDropdown) {
                    parentDropdown.classList.add('active');
                }
            } else {
                item.style.display = 'none';
            }
        });
    }

    /**
     * Highlight matching text
     */
    function highlightText(element, searchTerm) {
        const regex = new RegExp(`(${escapeRegex(searchTerm)})`, 'gi');
        
        element.querySelectorAll('.sidebar-link span, .sidebar-dropdown-item').forEach(el => {
            const text = el.textContent;
            if (text.toLowerCase().includes(searchTerm)) {
                el.innerHTML = text.replace(regex, '<mark class="menu-highlight">$1</mark>');
            }
        });
    }

    /**
     * Remove highlights from element
     */
    function removeHighlights(element) {
        element.querySelectorAll('mark').forEach(mark => {
            const parent = mark.parentNode;
            parent.replaceChild(document.createTextNode(mark.textContent), mark);
            parent.normalize();
        });
    }

    // ========================================
    // TOUCH & GESTURE SUPPORT
    // ========================================
    
    /**
     * Handle touch start
     */
    function handleTouchStart(e) {
        state.touchStartX = e.changedTouches[0].screenX;
    }

    /**
     * Handle touch end
     */
    function handleTouchEnd(e) {
        state.touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    }

    /**
     * Handle swipe gestures
     */
    function handleSwipe() {
        const diff = state.touchEndX - state.touchStartX;
        const threshold = CONFIG.animation.swipeThreshold;
        
        // Swipe right to open menu (from left edge)
        if (diff > threshold && state.touchStartX < 20 && !state.isMobileMenuOpen) {
            openMobileMenu();
        }
        // Swipe left to close menu
        else if (diff < -threshold && state.isMobileMenuOpen) {
            closeMobileMenu();
        }
    }

    // ========================================
    // RESPONSIVE HANDLING
    // ========================================
    
    /**
     * Handle window resize
     */
    function handleResize() {
        updateResponsiveState();
        
        // Close mobile menu on desktop
        if (!isMobileView() && state.isMobileMenuOpen) {
            closeMobileMenu();
        }
    }

    /**
     * Update responsive state
     */
    function updateResponsiveState() {
        const width = window.innerWidth;
        state.isMobile = width < CONFIG.breakpoints.mobile;
        state.isTablet = width >= CONFIG.breakpoints.mobile && width < CONFIG.breakpoints.tablet;
        
        // Handle portrait tablets
        if (state.isTablet && window.matchMedia("(orientation: portrait)").matches) {
            state.isMobile = true;
        }
    }

    /**
     * Check if mobile view
     */
    function isMobileView() {
        return window.innerWidth < CONFIG.breakpoints.tablet;
    }

    // ========================================
    // EVENT HANDLERS
    // ========================================
    
    /**
     * Handle keyboard events
     */
    function handleKeyboard(e) {
        // Escape key closes mobile menu
        if (e.key === 'Escape' && state.isMobileMenuOpen) {
            closeMobileMenu();
        }
        
        // Alt+M toggles menu
        if (e.altKey && e.key === 'm') {
            e.preventDefault();
            if (isMobileView()) {
                toggleMobileMenu();
            } else {
                toggleSidebar();
            }
        }
    }

    /**
     * Handle menu link clicks
     */
    function handleMenuClick(e) {
        const link = e.target.closest('.sidebar-link');
        
        if (link && !link.classList.contains('sidebar-dropdown-toggle')) {
            // When clicking any non-dropdown link (including those without children like Dashboard),
            // collapse ALL dropdowns since we're navigating to a different page
            document.querySelectorAll('.sidebar-dropdown.active').forEach(dropdown => {
                dropdown.classList.remove('active');
            });
            
            // Close mobile menu after navigation (with delay for better UX)
            if (isMobileView()) {
                setTimeout(closeMobileMenu, 250);
            }
        }
    }

    /**
     * Handle before unload
     */
    function handleBeforeUnload() {
        closeMobileMenu();
    }

    // ========================================
    // UTILITY FUNCTIONS
    // ========================================
    
    /**
     * Debounce function for performance
     */
    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    /**
     * Escape regex special characters
     */
    function escapeRegex(str) {
        return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    }

    /**
     * Dispatch custom event
     */
    function dispatchEvent(eventName, detail = {}) {
        document.dispatchEvent(new CustomEvent(eventName, { detail }));
    }

    /**
     * Collapse all dropdown menus
     */
    function collapseAllDropdowns() {
        document.querySelectorAll('.sidebar-dropdown.active').forEach(dropdown => {
            dropdown.classList.remove('active');
        });
    }
    
    // ========================================
    // PUBLIC API
    // ========================================
    window.SidebarMenu = {
        init,
        toggle: toggleSidebar,
        toggleMobile: toggleMobileMenu,
        openMobile: openMobileMenu,
        closeMobile: closeMobileMenu,
        toggleTheme,
        search: filterMenuItems,
        collapseAllDropdowns,
        markActiveMenu,
        getState: () => ({ ...state }),
        isCollapsed: () => state.isCollapsed,
        isMobileOpen: () => state.isMobileMenuOpen
    };

    // ========================================
    // AUTO-INITIALIZATION
    // ========================================
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

})();