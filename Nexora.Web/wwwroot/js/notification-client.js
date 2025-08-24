// Real-time Notification Client
const NotificationClient = {
    connection: null,
    isConnected: false,
    reconnectAttempts: 0,
    maxReconnectAttempts: 5,
    reconnectDelay: 5000,
    
    // Initialize notification client
    init: async function() {
        if (!window.signalR) {
            console.error('SignalR library not loaded');
            return;
        }
        
        // Create connection
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount === this.maxReconnectAttempts) {
                        return null; // Stop reconnecting
                    }
                    return Math.min(retryContext.previousRetryCount * 2000, 30000);
                }
            })
            .configureLogging(signalR.LogLevel.Warning)
            .build();
        
        // Set up event handlers
        this.setupEventHandlers();
        
        // Start connection
        await this.start();
        
        // Load initial notifications
        await this.loadNotifications();
    },
    
    // Setup SignalR event handlers
    setupEventHandlers: function() {
        // Handle incoming notifications
        this.connection.on("ReceiveNotification", (notification) => {
            this.handleNotification(notification);
        });
        
        // Handle ticket updates
        this.connection.on("TicketUpdated", (ticketData) => {
            this.handleTicketUpdate(ticketData);
        });
        
        // Handle connection events
        this.connection.onreconnecting(() => {
            console.log('Reconnecting to notification hub...');
            this.updateConnectionStatus('reconnecting');
        });
        
        this.connection.onreconnected(() => {
            console.log('Reconnected to notification hub');
            this.updateConnectionStatus('connected');
            this.reconnectAttempts = 0;
        });
        
        this.connection.onclose(() => {
            console.log('Disconnected from notification hub');
            this.updateConnectionStatus('disconnected');
            this.isConnected = false;
        });
    },
    
    // Start SignalR connection
    start: async function() {
        try {
            await this.connection.start();
            console.log('Connected to notification hub');
            this.isConnected = true;
            this.updateConnectionStatus('connected');
            
            // Subscribe to current ticket if on ticket details page
            const ticketId = this.getCurrentTicketId();
            if (ticketId) {
                await this.subscribeToTicket(ticketId);
            }
        } catch (err) {
            console.error('Failed to connect to notification hub:', err);
            this.updateConnectionStatus('error');
            
            // Retry connection
            if (this.reconnectAttempts < this.maxReconnectAttempts) {
                this.reconnectAttempts++;
                setTimeout(() => this.start(), this.reconnectDelay);
            }
        }
    },
    
    // Handle incoming notification
    handleNotification: function(notification) {
        // Update notification badge
        this.updateNotificationBadge();
        
        // Add to notification dropdown
        this.addNotificationToDropdown(notification);
        
        // Show toast notification
        this.showToastNotification(notification);
        
        // Play notification sound if enabled
        if (this.isNotificationSoundEnabled()) {
            this.playNotificationSound();
        }
        
        // Update page content if relevant
        this.updatePageContent(notification);
    },
    
    // Handle ticket update
    handleTicketUpdate: function(ticketData) {
        // Update ticket details if on the same ticket page
        if (this.isOnTicketPage(ticketData.ticketId)) {
            this.refreshTicketDetails(ticketData);
        }
        
        // Update ticket list if visible
        this.updateTicketInList(ticketData);
    },
    
    // Load initial notifications
    loadNotifications: async function() {
        try {
            const response = await fetch('/api/notifications/unread');
            if (response.ok) {
                const notifications = await response.json();
                this.displayNotifications(notifications);
            }
        } catch (err) {
            console.error('Failed to load notifications:', err);
        }
    },
    
    // Update notification badge
    updateNotificationBadge: function() {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            const count = parseInt(badge.textContent || '0') + 1;
            badge.textContent = count;
            badge.style.display = count > 0 ? 'inline-block' : 'none';
        }
    },
    
    // Add notification to dropdown
    addNotificationToDropdown: function(notification) {
        const dropdown = document.querySelector('.notification-dropdown-menu');
        if (!dropdown) return;
        
        const item = document.createElement('div');
        item.className = 'notification-item unread';
        item.dataset.notificationId = notification.id;
        item.innerHTML = `
            <div class="notification-icon">
                <i class="fas ${this.getNotificationIcon(notification.type)}"></i>
            </div>
            <div class="notification-content">
                <div class="notification-title">${this.escapeHtml(notification.title)}</div>
                <div class="notification-message">${this.escapeHtml(notification.message)}</div>
                <div class="notification-time">${this.formatTime(notification.createdAt)}</div>
            </div>
            <button class="btn btn-sm btn-link mark-read" onclick="NotificationClient.markAsRead(${notification.id})">
                <i class="fas fa-check"></i>
            </button>
        `;
        
        // Add to top of dropdown
        const firstItem = dropdown.querySelector('.notification-item');
        if (firstItem) {
            dropdown.insertBefore(item, firstItem);
        } else {
            dropdown.appendChild(item);
        }
        
        // Limit dropdown items
        const items = dropdown.querySelectorAll('.notification-item');
        if (items.length > 10) {
            items[items.length - 1].remove();
        }
    },
    
    // Show toast notification
    showToastNotification: function(notification) {
        if (typeof toastr !== 'undefined') {
            const options = {
                closeButton: true,
                progressBar: true,
                positionClass: "toast-top-right",
                timeOut: 5000,
                onclick: () => this.handleNotificationClick(notification)
            };
            
            switch (notification.type.toLowerCase()) {
                case 'success':
                    toastr.success(notification.message, notification.title, options);
                    break;
                case 'warning':
                    toastr.warning(notification.message, notification.title, options);
                    break;
                case 'error':
                    toastr.error(notification.message, notification.title, options);
                    break;
                default:
                    toastr.info(notification.message, notification.title, options);
            }
        }
    },
    
    // Mark notification as read
    markAsRead: async function(notificationId) {
        try {
            const response = await fetch(`/api/notifications/${notificationId}/read`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (response.ok) {
                // Update UI
                const item = document.querySelector(`[data-notification-id="${notificationId}"]`);
                if (item) {
                    item.classList.remove('unread');
                }
                
                // Update badge count
                const badge = document.querySelector('.notification-badge');
                if (badge) {
                    const count = Math.max(0, parseInt(badge.textContent || '0') - 1);
                    badge.textContent = count;
                    badge.style.display = count > 0 ? 'inline-block' : 'none';
                }
            }
        } catch (err) {
            console.error('Failed to mark notification as read:', err);
        }
    },
    
    // Mark all as read
    markAllAsRead: async function() {
        try {
            const response = await fetch('/api/notifications/read-all', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            
            if (response.ok) {
                // Update UI
                document.querySelectorAll('.notification-item.unread').forEach(item => {
                    item.classList.remove('unread');
                });
                
                // Clear badge
                const badge = document.querySelector('.notification-badge');
                if (badge) {
                    badge.textContent = '0';
                    badge.style.display = 'none';
                }
            }
        } catch (err) {
            console.error('Failed to mark all notifications as read:', err);
        }
    },
    
    // Subscribe to ticket updates
    subscribeToTicket: async function(ticketId) {
        if (this.isConnected) {
            try {
                await this.connection.invoke("SubscribeToTicket", ticketId);
                console.log(`Subscribed to ticket ${ticketId} updates`);
            } catch (err) {
                console.error('Failed to subscribe to ticket:', err);
            }
        }
    },
    
    // Unsubscribe from ticket updates
    unsubscribeFromTicket: async function(ticketId) {
        if (this.isConnected) {
            try {
                await this.connection.invoke("UnsubscribeFromTicket", ticketId);
                console.log(`Unsubscribed from ticket ${ticketId} updates`);
            } catch (err) {
                console.error('Failed to unsubscribe from ticket:', err);
            }
        }
    },
    
    // Helper functions
    getCurrentTicketId: function() {
        const match = window.location.pathname.match(/\/ticket\/(\d+)/i);
        return match ? parseInt(match[1]) : null;
    },
    
    isOnTicketPage: function(ticketId) {
        return this.getCurrentTicketId() === ticketId;
    },
    
    getNotificationIcon: function(type) {
        const icons = {
            'success': 'fa-check-circle text-success',
            'warning': 'fa-exclamation-triangle text-warning',
            'error': 'fa-times-circle text-danger',
            'info': 'fa-info-circle text-info',
            'ticketassigned': 'fa-user-plus text-primary',
            'ticketupdated': 'fa-sync text-info',
            'ticketresolved': 'fa-check-double text-success',
            'comment': 'fa-comment text-primary'
        };
        return icons[type.toLowerCase()] || 'fa-bell text-secondary';
    },
    
    formatTime: function(dateString) {
        const date = new Date(dateString);
        const now = new Date();
        const diff = Math.floor((now - date) / 1000); // seconds
        
        if (diff < 60) return 'just now';
        if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
        if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
        if (diff < 604800) return `${Math.floor(diff / 86400)}d ago`;
        
        return date.toLocaleDateString();
    },
    
    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },
    
    updateConnectionStatus: function(status) {
        const indicator = document.querySelector('.connection-status');
        if (indicator) {
            indicator.className = `connection-status ${status}`;
            indicator.title = `Connection: ${status}`;
        }
    },
    
    isNotificationSoundEnabled: function() {
        return localStorage.getItem('notificationSound') !== 'false';
    },
    
    playNotificationSound: function() {
        const audio = new Audio('/sounds/notification.mp3');
        audio.volume = 0.3;
        audio.play().catch(err => console.log('Could not play notification sound:', err));
    },
    
    handleNotificationClick: function(notification) {
        if (notification.url) {
            window.location.href = notification.url;
        }
    },
    
    refreshTicketDetails: function(ticketData) {
        // Refresh specific sections of the ticket page
        if (ticketData.status) {
            const statusElement = document.querySelector('.ticket-status');
            if (statusElement) {
                statusElement.textContent = ticketData.status;
                statusElement.className = `ticket-status badge bg-${this.getStatusColor(ticketData.status)}`;
            }
        }
        
        if (ticketData.assignedTo) {
            const assignedElement = document.querySelector('.ticket-assigned');
            if (assignedElement) {
                assignedElement.textContent = ticketData.assignedTo;
            }
        }
    },
    
    updateTicketInList: function(ticketData) {
        const row = document.querySelector(`tr[data-ticket-id="${ticketData.ticketId}"]`);
        if (row) {
            // Update status cell
            const statusCell = row.querySelector('.status-cell');
            if (statusCell) {
                statusCell.innerHTML = `<span class="badge bg-${this.getStatusColor(ticketData.status)}">${ticketData.status}</span>`;
            }
            
            // Add update indicator
            row.classList.add('recently-updated');
            setTimeout(() => row.classList.remove('recently-updated'), 3000);
        }
    },
    
    getStatusColor: function(status) {
        const colors = {
            'new': 'info',
            'open': 'warning',
            'inprogress': 'primary',
            'resolved': 'success',
            'closed': 'secondary'
        };
        return colors[status.toLowerCase()] || 'secondary';
    },
    
    displayNotifications: function(notifications) {
        const badge = document.querySelector('.notification-badge');
        if (badge) {
            badge.textContent = notifications.length;
            badge.style.display = notifications.length > 0 ? 'inline-block' : 'none';
        }
        
        const dropdown = document.querySelector('.notification-dropdown-menu');
        if (dropdown && notifications.length > 0) {
            dropdown.innerHTML = '';
            notifications.forEach(notification => {
                this.addNotificationToDropdown(notification);
            });
        }
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', async function() {
    // Load SignalR library if not already loaded
    if (!window.signalR) {
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/@microsoft/signalr@6.0.0/dist/browser/signalr.min.js';
        script.onload = () => NotificationClient.init();
        document.head.appendChild(script);
    } else {
        await NotificationClient.init();
    }
});

// Add notification styles
const notificationStyles = document.createElement('style');
notificationStyles.textContent = `
    .notification-badge {
        position: absolute;
        top: -5px;
        right: -5px;
        background: #dc3545;
        color: white;
        border-radius: 10px;
        padding: 2px 6px;
        font-size: 11px;
        font-weight: bold;
        display: none;
    }
    
    .notification-dropdown-menu {
        max-height: 400px;
        overflow-y: auto;
        width: 350px;
    }
    
    .notification-item {
        display: flex;
        padding: 10px;
        border-bottom: 1px solid #e9ecef;
        cursor: pointer;
        transition: background-color 0.2s;
    }
    
    .notification-item:hover {
        background-color: #f8f9fa;
    }
    
    .notification-item.unread {
        background-color: #e7f3ff;
    }
    
    .notification-icon {
        margin-right: 10px;
        font-size: 20px;
    }
    
    .notification-content {
        flex: 1;
    }
    
    .notification-title {
        font-weight: 600;
        margin-bottom: 2px;
    }
    
    .notification-message {
        font-size: 13px;
        color: #6c757d;
        margin-bottom: 2px;
    }
    
    .notification-time {
        font-size: 11px;
        color: #adb5bd;
    }
    
    .connection-status {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        display: inline-block;
        margin-left: 5px;
    }
    
    .connection-status.connected {
        background-color: #28a745;
    }
    
    .connection-status.reconnecting {
        background-color: #ffc107;
        animation: pulse 1s infinite;
    }
    
    .connection-status.disconnected {
        background-color: #dc3545;
    }
    
    .connection-status.error {
        background-color: #6c757d;
    }
    
    @keyframes pulse {
        0% { opacity: 1; }
        50% { opacity: 0.5; }
        100% { opacity: 1; }
    }
    
    .recently-updated {
        animation: highlight 3s ease-out;
    }
    
    @keyframes highlight {
        0% { background-color: #fff3cd; }
        100% { background-color: transparent; }
    }
`;
document.head.appendChild(notificationStyles);