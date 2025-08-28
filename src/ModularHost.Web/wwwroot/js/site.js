// Dark mode toggle
function toggleDarkMode() {
    const html = document.documentElement;
    const isDark = html.classList.contains('dark');
    
    if (isDark) {
        html.classList.remove('dark');
        localStorage.setItem('theme', 'light');
        document.cookie = "theme=light;path=/";
    } else {
        html.classList.add('dark');
        localStorage.setItem('theme', 'dark');
        document.cookie = "theme=dark;path=/";
    }
}

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', function() {
    const theme = localStorage.getItem('theme') || 'light';
    if (theme === 'dark') {
        document.documentElement.classList.add('dark');
    }
    
    // Mobile menu toggle
    const mobileMenuBtn = document.getElementById('mobile-menu-btn');
    const sidebar = document.getElementById('sidebar');
    
    if (mobileMenuBtn && sidebar) {
        mobileMenuBtn.addEventListener('click', function() {
            sidebar.classList.toggle('-translate-x-full');
        });
    }
    
    // Notification dropdown
    const notificationBtn = document.getElementById('notification-btn');
    const notificationDropdown = document.getElementById('notification-dropdown');
    
    if (notificationBtn && notificationDropdown) {
        notificationBtn.addEventListener('click', function() {
            notificationDropdown.classList.toggle('hidden');
        });
        
        // Close dropdown when clicking outside
        document.addEventListener('click', function(event) {
            if (!notificationBtn.contains(event.target) && !notificationDropdown.contains(event.target)) {
                notificationDropdown.classList.add('hidden');
            }
        });
    }
});

// SignalR connection for notifications
let notificationConnection = null;

function initializeSignalR() {
    if (typeof signalR === 'undefined') return;
    
    notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    
    notificationConnection.on("ReceiveNotification", function (title, message) {
        showNotification(title, message);
    });
    
    notificationConnection.start().then(function () {
        console.log("SignalR Connected");
    }).catch(function (err) {
        console.error(err.toString());
        setTimeout(initializeSignalR, 5000);
    });
    
    notificationConnection.onclose(function() {
        setTimeout(initializeSignalR, 5000);
    });
}

function showNotification(title, message, type = 'info') {
    const container = document.getElementById('notification-container') || createNotificationContainer();
    
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} flex items-center justify-between shadow-lg mb-2 animate-slide-in`;
    notification.innerHTML = `
        <div>
            <strong>${title}</strong>
            <p class="text-sm mt-1">${message}</p>
        </div>
        <button onclick="this.parentElement.remove()" class="ml-4 text-lg">&times;</button>
    `;
    
    container.appendChild(notification);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

function createNotificationContainer() {
    const container = document.createElement('div');
    container.id = 'notification-container';
    container.className = 'fixed top-20 right-4 z-50 max-w-sm';
    document.body.appendChild(container);
    return container;
}

// Form validation helpers
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return false;
    
    let isValid = true;
    const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            input.classList.add('is-invalid');
            isValid = false;
            
            let feedback = input.nextElementSibling;
            if (!feedback || !feedback.classList.contains('invalid-feedback')) {
                feedback = document.createElement('div');
                feedback.className = 'invalid-feedback';
                feedback.textContent = 'This field is required';
                input.parentNode.insertBefore(feedback, input.nextSibling);
            }
        } else {
            input.classList.remove('is-invalid');
            const feedback = input.nextElementSibling;
            if (feedback && feedback.classList.contains('invalid-feedback')) {
                feedback.remove();
            }
        }
    });
    
    return isValid;
}

// AJAX helpers
function ajaxGet(url, callback) {
    fetch(url, {
        method: 'GET',
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
    .then(response => response.json())
    .then(data => callback(data))
    .catch(error => console.error('Error:', error));
}

function ajaxPost(url, data, callback) {
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-Requested-With': 'XMLHttpRequest'
        },
        body: JSON.stringify(data)
    })
    .then(response => response.json())
    .then(data => callback(data))
    .catch(error => console.error('Error:', error));
}

// Initialize SignalR when page loads
if (document.querySelector('meta[name="authenticated"]')?.content === 'true') {
    initializeSignalR();
}

// Toastr notification functions
const notification = {
    success: function(message, title = 'Success') {
        if (typeof toastr !== 'undefined') {
            toastr.success(message, title);
        } else {
            showNotification(title, message, 'success');
        }
    },
    error: function(message, title = 'Error') {
        if (typeof toastr !== 'undefined') {
            toastr.error(message, title);
        } else {
            showNotification(title, message, 'danger');
        }
    },
    warning: function(message, title = 'Warning') {
        if (typeof toastr !== 'undefined') {
            toastr.warning(message, title);
        } else {
            showNotification(title, message, 'warning');
        }
    },
    info: function(message, title = 'Information') {
        if (typeof toastr !== 'undefined') {
            toastr.info(message, title);
        } else {
            showNotification(title, message, 'info');
        }
    }
};

// Loader functions
const loader = {
    element: null,
    show: function(text = 'Loading...') {
        if (!this.element) {
            this.element = document.createElement('div');
            this.element.id = 'global-loader';
            this.element.className = 'fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-[9999]';
            this.element.innerHTML = `
                <div class="bg-white dark:bg-gray-800 rounded-lg p-6 flex flex-col items-center">
                    <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
                    <p class="mt-4 text-gray-700 dark:text-gray-300" id="loader-text">${text}</p>
                </div>
            `;
            document.body.appendChild(this.element);
        } else {
            document.getElementById('loader-text').textContent = text;
            this.element.classList.remove('hidden');
        }
    },
    hide: function() {
        if (this.element) {
            this.element.classList.add('hidden');
        }
    },
    updateText: function(text) {
        const textElement = document.getElementById('loader-text');
        if (textElement) {
            textElement.textContent = text;
        }
    }
};

// Modal helper functions
function openModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('hidden');
        document.body.classList.add('overflow-hidden');
    }
}

function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
        document.body.classList.remove('overflow-hidden');
    }
}

// Close modal when clicking outside
document.addEventListener('click', function(e) {
    if (e.target.classList.contains('modal-backdrop')) {
        const modal = e.target.closest('.modal');
        if (modal) {
            modal.classList.add('hidden');
            document.body.classList.remove('overflow-hidden');
        }
    }
});

// Slug generation helper
function generateSlug(text) {
    return text.toString().toLowerCase()
        .replace(/\s+/g, '-')           // Replace spaces with -
        .replace(/[^\w\-]+/g, '')       // Remove all non-word chars
        .replace(/\-\-+/g, '-')          // Replace multiple - with single -
        .replace(/^-+/, '')              // Trim - from start of text
        .replace(/-+$/, '');             // Trim - from end of text
}

// Permission helper functions
const permissions = {
    _userPermissions: [],
    _userRoles: [],
    _userId: null,
    _isAuthenticated: false,
    
    init: function(data) {
        this._userPermissions = data.permissions || [];
        this._userRoles = data.roles || [];
        this._userId = data.userId || null;
        this._isAuthenticated = data.isAuthenticated || false;
    },
    
    hasPermission: function(permission) {
        return this._userPermissions.includes(permission);
    },
    
    hasRole: function(role) {
        return this._userRoles.includes(role);
    },
    
    isAdmin: function() {
        return this.hasRole('Admin') || this.hasRole('SuperAdmin');
    },
    
    isSuperAdmin: function() {
        return this.hasRole('SuperAdmin');
    },
    
    isAuthenticated: function() {
        return this._isAuthenticated;
    },
    
    canEdit: function(resourceOwnerId) {
        if (!this.isAuthenticated()) return false;
        if (this.isAdmin()) return true;
        return resourceOwnerId === this._userId;
    },
    
    canDelete: function(resourceOwnerId) {
        if (!this.isAuthenticated()) return false;
        if (this.isSuperAdmin()) return true;
        if (this.isAdmin()) return true;
        return resourceOwnerId === this._userId;
    },
    
    showElement: function(elementId, permission = null, role = null) {
        const element = document.getElementById(elementId);
        if (!element) return;
        
        let shouldShow = true;
        
        if (permission && !this.hasPermission(permission)) {
            shouldShow = false;
        }
        
        if (role && !this.hasRole(role)) {
            shouldShow = false;
        }
        
        element.style.display = shouldShow ? '' : 'none';
    },
    
    hideForGuests: function(selector) {
        if (!this.isAuthenticated()) {
            document.querySelectorAll(selector).forEach(el => {
                el.style.display = 'none';
            });
        }
    },
    
    showForAdmins: function(selector) {
        if (this.isAdmin()) {
            document.querySelectorAll(selector).forEach(el => {
                el.style.display = '';
            });
        } else {
            document.querySelectorAll(selector).forEach(el => {
                el.style.display = 'none';
            });
        }
    }
};
