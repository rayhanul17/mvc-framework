// Complete Layout JavaScript - Menu and UI Interactions

(function() {
    'use strict';
    
    // Wait for DOM to be fully loaded
    document.addEventListener('DOMContentLoaded', function() {
        console.log('Initializing layout components...');
        initializeSidebar();
        initializeThemeToggle();
        initializeMobileMenu();
        initializeDropdownMenus();
        restoreUserPreferences();
        console.log('Layout initialization complete');
    });
    
    // Initialize Sidebar
    function initializeSidebar() {
        const sidebar = document.getElementById('sidebar');
        const content = document.getElementById('content');
        const sidebarCollapseBtn = document.getElementById('sidebarCollapse');
        const desktopMenuToggle = document.getElementById('desktopMenuToggle');
        
        console.log('Sidebar elements:', { sidebar, content, sidebarCollapseBtn, desktopMenuToggle });
        
        function toggleSidebar(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('Toggle sidebar clicked');
            
            if (window.innerWidth >= 992) {
                sidebar.classList.toggle('collapsed');
                content.classList.toggle('expanded');
                
                // Update collapse button icon
                if (sidebarCollapseBtn) {
                    const icon = sidebarCollapseBtn.querySelector('i');
                    if (icon) {
                        icon.className = sidebar.classList.contains('collapsed') ? 
                            'fas fa-angle-right' : 'fas fa-angle-left';
                    }
                }
                
                localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
                console.log('Sidebar toggled:', sidebar.classList.contains('collapsed') ? 'collapsed' : 'expanded');
            }
        }
        
        // Desktop sidebar collapse
        if (sidebarCollapseBtn) {
            sidebarCollapseBtn.addEventListener('click', toggleSidebar);
            console.log('Sidebar collapse button listener added');
        }
        
        if (desktopMenuToggle) {
            desktopMenuToggle.addEventListener('click', toggleSidebar);
            console.log('Desktop menu toggle listener added');
        }
    }
    
    // Initialize Dropdown Menus
    function initializeDropdownMenus() {
        const dropdownToggles = document.querySelectorAll('.sidebar-dropdown-toggle');
        
        dropdownToggles.forEach(toggle => {
            toggle.addEventListener('click', function(e) {
                e.preventDefault();
                const dropdown = this.closest('.sidebar-dropdown');
                
                // Close other dropdowns
                const otherDropdowns = document.querySelectorAll('.sidebar-dropdown.active');
                otherDropdowns.forEach(other => {
                    if (other !== dropdown) {
                        other.classList.remove('active');
                    }
                });
                
                // Toggle current dropdown
                dropdown.classList.toggle('active');
                
                // Save state
                const menuStates = {};
                document.querySelectorAll('.sidebar-dropdown').forEach((item, index) => {
                    menuStates['dropdown_' + index] = item.classList.contains('active');
                });
                localStorage.setItem('menuStates', JSON.stringify(menuStates));
            });
        });
        
        // Restore dropdown states
        const savedStates = localStorage.getItem('menuStates');
        if (savedStates) {
            try {
                const menuStates = JSON.parse(savedStates);
                document.querySelectorAll('.sidebar-dropdown').forEach((item, index) => {
                    if (menuStates['dropdown_' + index]) {
                        item.classList.add('active');
                    }
                });
            } catch (e) {
                console.error('Error restoring menu states:', e);
            }
        }
    }
    
    // Initialize Theme Toggle
    function initializeThemeToggle() {
        const themeToggleButtons = document.querySelectorAll('#header-theme-toggle, #mobile-theme-toggle');
        
        console.log('Theme toggle buttons found:', themeToggleButtons.length);
        
        function toggleTheme(e) {
            e.preventDefault();
            e.stopPropagation();
            console.log('Theme toggle clicked');
            
            document.body.classList.toggle('dark-theme');
            const isDark = document.body.classList.contains('dark-theme');
            localStorage.setItem('theme', isDark ? 'dark' : 'light');
            
            // Update all theme icons
            const themeIcons = document.querySelectorAll('#header-theme-icon, #mobile-theme-icon, #theme-icon');
            themeIcons.forEach(icon => {
                if (icon) {
                    icon.className = isDark ? 'fas fa-sun' : 'fas fa-moon';
                }
            });
            
            console.log('Theme toggled to:', isDark ? 'dark' : 'light');
        }
        
        themeToggleButtons.forEach(button => {
            if (button) {
                button.addEventListener('click', toggleTheme);
                console.log('Theme toggle listener added to button');
            }
        });
    }
    
    // Initialize Mobile Menu
    function initializeMobileMenu() {
        const mobileMenuBtn = document.getElementById('mobileMenuBtn');
        const mobileMenuOverlay = document.getElementById('mobileMenuOverlay');
        const sidebar = document.getElementById('sidebar');
        
        if (mobileMenuBtn) {
            mobileMenuBtn.addEventListener('click', openMobileMenu);
        }
        
        if (mobileMenuOverlay) {
            mobileMenuOverlay.addEventListener('click', closeMobileMenu);
        }
        
        function openMobileMenu() {
            sidebar.classList.add('mobile-show');
            mobileMenuOverlay.classList.add('show');
            document.body.classList.add('mobile-menu-open');
        }
        
        function closeMobileMenu() {
            sidebar.classList.remove('mobile-show');
            mobileMenuOverlay.classList.remove('show');
            document.body.classList.remove('mobile-menu-open');
        }
        
        // Close mobile menu when clicking on a link
        const sidebarLinks = document.querySelectorAll('.sidebar-link:not(.sidebar-dropdown-toggle), .sidebar-dropdown-item');
        sidebarLinks.forEach(link => {
            link.addEventListener('click', function() {
                if (window.innerWidth < 992) {
                    closeMobileMenu();
                }
            });
        });
        
        // Handle window resize
        let resizeTimeout;
        window.addEventListener('resize', function() {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(function() {
                if (window.innerWidth >= 992) {
                    closeMobileMenu();
                }
            }, 250);
        });
    }
    
    // Restore User Preferences
    function restoreUserPreferences() {
        // Restore theme
        const savedTheme = localStorage.getItem('theme');
        if (savedTheme === 'dark') {
            document.body.classList.add('dark-theme');
            const themeIcons = document.querySelectorAll('#header-theme-icon, #mobile-theme-icon, #theme-icon');
            themeIcons.forEach(icon => {
                if (icon) {
                    icon.className = 'fas fa-sun';
                }
            });
        }
        
        // Restore sidebar state (desktop only)
        if (window.innerWidth >= 992) {
            const sidebarCollapsed = localStorage.getItem('sidebarCollapsed');
            if (sidebarCollapsed === 'true') {
                const sidebar = document.getElementById('sidebar');
                const content = document.getElementById('content');
                if (sidebar && content) {
                    sidebar.classList.add('collapsed');
                    content.classList.add('expanded');
                }
            }
        }
    }
    
    // Search functionality
    const menuSearchInput = document.getElementById('menuSearchInput');
    if (menuSearchInput) {
        menuSearchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            const menuItems = document.querySelectorAll('.sidebar-item');
            
            menuItems.forEach(item => {
                const menuText = item.textContent.toLowerCase();
                const shouldShow = menuText.includes(searchTerm);
                
                item.style.display = shouldShow ? '' : 'none';
                
                // If it's a dropdown item and it matches, show parent dropdown
                if (shouldShow && item.closest('.sidebar-dropdown')) {
                    const parentDropdown = item.closest('.sidebar-dropdown');
                    parentDropdown.style.display = '';
                    if (searchTerm) {
                        parentDropdown.classList.add('active');
                    }
                }
            });
            
            // If search is cleared, restore original states
            if (!searchTerm) {
                const savedStates = localStorage.getItem('menuStates');
                if (savedStates) {
                    try {
                        const menuStates = JSON.parse(savedStates);
                        document.querySelectorAll('.sidebar-dropdown').forEach((item, index) => {
                            item.classList.toggle('active', menuStates['dropdown_' + index] || false);
                        });
                    } catch (e) {
                        console.error('Error restoring menu states:', e);
                    }
                }
            }
        });
    }
    
    // Highlight active menu based on current URL
    function highlightActiveMenu() {
        const currentPath = window.location.pathname.toLowerCase();
        const currentSearch = window.location.search.toLowerCase();
        const fullPath = currentPath + currentSearch;
        
        // Remove all active classes first
        document.querySelectorAll('.sidebar-item.active, .sidebar-dropdown-item.active').forEach(item => {
            item.classList.remove('active');
        });
        
        // Find and highlight the matching menu item
        let activeFound = false;
        document.querySelectorAll('.sidebar-link, .sidebar-dropdown-item').forEach(link => {
            const href = link.getAttribute('href');
            if (href) {
                const linkPath = href.toLowerCase();
                if (fullPath === linkPath || 
                    (currentPath === linkPath) || 
                    (linkPath !== '/' && currentPath.startsWith(linkPath))) {
                    
                    const parentItem = link.closest('.sidebar-item');
                    if (parentItem) {
                        parentItem.classList.add('active');
                        activeFound = true;
                    }
                    
                    // If it's a dropdown item, also activate parent dropdown
                    const parentDropdown = link.closest('.sidebar-dropdown');
                    if (parentDropdown) {
                        parentDropdown.classList.add('active');
                    }
                    
                    if (link.classList.contains('sidebar-dropdown-item')) {
                        link.classList.add('active');
                    }
                }
            }
        });
        
        // If no active menu found, try to match by controller/action
        if (!activeFound) {
            const pathParts = currentPath.split('/').filter(p => p);
            if (pathParts.length > 0) {
                const controller = pathParts[0];
                document.querySelectorAll('.sidebar-link').forEach(link => {
                    const href = link.getAttribute('href');
                    if (href && href.toLowerCase().includes('/' + controller)) {
                        const parentItem = link.closest('.sidebar-item');
                        if (parentItem) {
                            parentItem.classList.add('active');
                        }
                    }
                });
            }
        }
    }
    
    // Call highlight active menu on page load
    highlightActiveMenu();
    
    // Handle loading states
    document.querySelectorAll('.sidebar-link, .sidebar-dropdown-item').forEach(link => {
        if (link.getAttribute('href') && link.getAttribute('href') !== '#') {
            link.addEventListener('click', function() {
                this.classList.add('loading');
            });
        }
    });
    
})();