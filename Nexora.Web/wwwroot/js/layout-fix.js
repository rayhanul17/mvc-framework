// Layout Fix - Simple and reliable event handlers
$(document).ready(function() {
    console.log('Layout fix script loaded');
    
    // Theme Toggle
    $(document).on('click', '#header-theme-toggle, #mobile-theme-toggle', function(e) {
        e.preventDefault();
        e.stopPropagation();
        console.log('Theme button clicked');
        
        $('body').toggleClass('dark-theme');
        const isDark = $('body').hasClass('dark-theme');
        
        // Update icons
        $('#header-theme-icon, #mobile-theme-icon, #theme-icon').each(function() {
            $(this).removeClass('fa-moon fa-sun');
            $(this).addClass(isDark ? 'fa-sun' : 'fa-moon');
        });
        
        // Save preference
        localStorage.setItem('theme', isDark ? 'dark' : 'light');
        console.log('Theme changed to:', isDark ? 'dark' : 'light');
    });
    
    // Sidebar Collapse - Desktop
    $(document).on('click', '#sidebarCollapse, #desktopMenuToggle', function(e) {
        e.preventDefault();
        e.stopPropagation();
        console.log('Sidebar toggle clicked');
        
        if (window.innerWidth >= 992) {
            $('#sidebar').toggleClass('collapsed');
            $('#content').toggleClass('expanded');
            
            // Update icon
            const isCollapsed = $('#sidebar').hasClass('collapsed');
            $('#sidebarCollapse i').removeClass('fa-angle-left fa-angle-right');
            $('#sidebarCollapse i').addClass(isCollapsed ? 'fa-angle-right' : 'fa-angle-left');
            
            // Trigger resize event to update any responsive elements
            $(window).trigger('resize');
            
            // Save state
            localStorage.setItem('sidebarCollapsed', isCollapsed);
            console.log('Sidebar:', isCollapsed ? 'collapsed' : 'expanded');
        }
    });
    
    // Mobile Menu Toggle
    $(document).on('click', '#mobileMenuBtn', function(e) {
        e.preventDefault();
        e.stopPropagation();
        console.log('Mobile menu clicked');
        
        $('#sidebar').addClass('mobile-show');
        $('#mobileMenuOverlay').addClass('show');
        $('body').addClass('mobile-menu-open');
    });
    
    // Mobile Menu Close
    $(document).on('click', '#mobileMenuOverlay', function() {
        $('#sidebar').removeClass('mobile-show');
        $('#mobileMenuOverlay').removeClass('show');
        $('body').removeClass('mobile-menu-open');
    });
    
    // Dropdown Menus
    $(document).on('click', '.sidebar-dropdown-toggle', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        const $dropdown = $(this).closest('.sidebar-dropdown');
        const wasActive = $dropdown.hasClass('active');
        
        // Close all other dropdowns
        $('.sidebar-dropdown.active').not($dropdown).removeClass('active');
        
        // Toggle current dropdown
        $dropdown.toggleClass('active');
        
        // Save state
        saveMenuStates();
    });
    
    // Restore preferences on load
    restorePreferences();
    
    function restorePreferences() {
        // Restore theme
        const savedTheme = localStorage.getItem('theme');
        if (savedTheme === 'dark') {
            $('body').addClass('dark-theme');
            $('#header-theme-icon, #mobile-theme-icon, #theme-icon').removeClass('fa-moon').addClass('fa-sun');
        }
        
        // Restore sidebar state (desktop only)
        if (window.innerWidth >= 992) {
            const sidebarCollapsed = localStorage.getItem('sidebarCollapsed');
            if (sidebarCollapsed === 'true') {
                $('#sidebar').addClass('collapsed');
                $('#content').addClass('expanded');
                $('#sidebarCollapse i').removeClass('fa-angle-left').addClass('fa-angle-right');
            }
        }
        
        // Restore menu states
        const savedStates = localStorage.getItem('menuStates');
        if (savedStates) {
            try {
                const menuStates = JSON.parse(savedStates);
                $('.sidebar-dropdown').each(function(index) {
                    if (menuStates['dropdown_' + index]) {
                        $(this).addClass('active');
                    }
                });
            } catch (e) {
                console.error('Error restoring menu states:', e);
            }
        }
    }
    
    function saveMenuStates() {
        const menuStates = {};
        $('.sidebar-dropdown').each(function(index) {
            menuStates['dropdown_' + index] = $(this).hasClass('active');
        });
        localStorage.setItem('menuStates', JSON.stringify(menuStates));
    }
    
    // Search functionality
    $('#menuSearchInput').on('input', function() {
        const searchTerm = $(this).val().toLowerCase();
        
        $('.sidebar-item').each(function() {
            const menuText = $(this).text().toLowerCase();
            const shouldShow = menuText.includes(searchTerm);
            
            $(this).toggle(shouldShow);
            
            // If it's a dropdown item and it matches, show parent
            if (shouldShow && $(this).closest('.sidebar-dropdown').length) {
                const $parentDropdown = $(this).closest('.sidebar-dropdown');
                $parentDropdown.show();
                if (searchTerm) {
                    $parentDropdown.addClass('active');
                }
            }
        });
        
        // Restore states if search is cleared
        if (!searchTerm) {
            restorePreferences();
        }
    });
    
    // Close mobile menu when clicking links
    $('.sidebar-link:not(.sidebar-dropdown-toggle), .sidebar-dropdown-item').on('click', function() {
        if (window.innerWidth < 992) {
            $('#sidebar').removeClass('mobile-show');
            $('#mobileMenuOverlay').removeClass('show');
            $('body').removeClass('mobile-menu-open');
        }
    });
    
    // Handle window resize
    let resizeTimeout;
    $(window).on('resize', function() {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(function() {
            if (window.innerWidth >= 992) {
                $('#sidebar').removeClass('mobile-show');
                $('#mobileMenuOverlay').removeClass('show');
                $('body').removeClass('mobile-menu-open');
            }
        }, 250);
    });
    
    console.log('Layout fix initialization complete');
});