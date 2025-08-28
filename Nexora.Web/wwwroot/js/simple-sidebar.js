// Simple Sidebar Management
(function() {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function() {
        // Elements
        const sidebar = document.querySelector('.sidebar');
        const toggleBtn = document.querySelector('[data-toggle="sidebar"]');
        const overlay = document.querySelector('.sidebar-overlay');
        
        // Toggle sidebar on mobile
        if (toggleBtn) {
            toggleBtn.addEventListener('click', function() {
                sidebar?.classList.toggle('show');
                overlay?.classList.toggle('show');
            });
        }
        
        // Close sidebar when clicking overlay
        if (overlay) {
            overlay.addEventListener('click', function() {
                sidebar?.classList.remove('show');
                overlay.classList.remove('show');
            });
        }
        
        // Handle sidebar dropdown menus
        const sidebarDropdowns = document.querySelectorAll('.sidebar .nav-item.dropdown > .nav-link');
        sidebarDropdowns.forEach(toggle => {
            toggle.addEventListener('click', function(e) {
                e.preventDefault();
                const parent = this.closest('.nav-item');
                
                // Close other dropdowns first
                document.querySelectorAll('.sidebar .nav-item.dropdown.show').forEach(item => {
                    if (item !== parent) {
                        item.classList.remove('show');
                    }
                });
                
                // Toggle current dropdown
                parent?.classList.toggle('show');
            });
        });
        
        // Mark active menu based on current URL
        const currentPath = window.location.pathname.toLowerCase();
        document.querySelectorAll('.nav-link').forEach(link => {
            const href = link.getAttribute('href');
            if (href && currentPath.includes(href.toLowerCase())) {
                link.classList.add('active');
                
                // Expand parent dropdown if in a submenu
                const parentDropdown = link.closest('.dropdown');
                if (parentDropdown) {
                    parentDropdown.classList.add('show');
                }
            }
        });
        
        // Collapse sidebar on desktop (optional)
        const collapseBtn = document.querySelector('[data-toggle="sidebar-collapse"]');
        if (collapseBtn) {
            collapseBtn.addEventListener('click', function() {
                sidebar?.classList.toggle('collapsed');
                
                // Save state to localStorage
                const isCollapsed = sidebar?.classList.contains('collapsed');
                localStorage.setItem('sidebar-collapsed', isCollapsed);
            });
        }
        
        // Restore sidebar state
        const isCollapsed = localStorage.getItem('sidebar-collapsed') === 'true';
        if (isCollapsed && sidebar) {
            sidebar.classList.add('collapsed');
        }
        
        // Theme toggle
        const themeToggle = document.querySelector('[data-toggle="theme"]');
        if (themeToggle) {
            themeToggle.addEventListener('click', function() {
                const currentTheme = document.body.getAttribute('data-theme') || 'light';
                const newTheme = currentTheme === 'light' ? 'dark' : 'light';
                
                document.body.setAttribute('data-theme', newTheme);
                localStorage.setItem('theme', newTheme);
                
                // Update icon
                const icon = this.querySelector('i');
                if (icon) {
                    if (newTheme === 'dark') {
                        icon.classList.remove('fa-moon');
                        icon.classList.add('fa-sun');
                    } else {
                        icon.classList.remove('fa-sun');
                        icon.classList.add('fa-moon');
                    }
                }
            });
        }
        
        // Apply saved theme on load
        const savedTheme = localStorage.getItem('theme') || 'light';
        document.body.setAttribute('data-theme', savedTheme);
        if (savedTheme === 'dark' && themeToggle) {
            const icon = themeToggle.querySelector('i');
            if (icon) {
                icon.classList.remove('fa-moon');
                icon.classList.add('fa-sun');
            }
        }
    });
})();