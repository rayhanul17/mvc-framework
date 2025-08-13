/**
 * Notifications and Modal Management System
 * Provides unified notification toasts and modal dialogs throughout the application
 */

// Initialize toastr options on document ready
$(document).ready(function() {
    // Configure toastr defaults
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    // Initialize delete modal if it doesn't exist
    if ($('#deleteModal').length === 0) {
        NotificationSystem.createDeleteModal();
    }

    // Initialize confirmation modal if it doesn't exist
    if ($('#confirmModal').length === 0) {
        NotificationSystem.createConfirmModal();
    }

    // Check for server-side messages and display them
    NotificationSystem.checkServerMessages();
});

// Main Notification System
const NotificationSystem = {
    // Success notification
    success: function(message, title = 'Success', options = {}) {
        const opts = { ...toastr.options, ...options };
        toastr.success(message, title, opts);
    },

    // Error notification
    error: function(message, title = 'Error', options = {}) {
        const opts = { ...toastr.options, ...options };
        toastr.error(message, title, opts);
    },

    // Info notification
    info: function(message, title = 'Information', options = {}) {
        const opts = { ...toastr.options, ...options };
        toastr.info(message, title, opts);
    },

    // Warning notification
    warning: function(message, title = 'Warning', options = {}) {
        const opts = { ...toastr.options, ...options };
        toastr.warning(message, title, opts);
    },

    // Clear all notifications
    clear: function() {
        toastr.clear();
    },

    // Show loading notification
    loading: function(message = 'Processing...', title = 'Please wait') {
        const opts = {
            ...toastr.options,
            timeOut: 0,
            extendedTimeOut: 0,
            tapToDismiss: false,
            progressBar: false,
            closeButton: false
        };
        return toastr.info(message, title, opts);
    },

    // Hide specific notification
    hide: function(toast) {
        if (toast) {
            toastr.clear(toast);
        }
    },

    // Check for server-side messages in TempData/ViewBag
    checkServerMessages: function() {
        // Check for success message
        const successMsg = $('[data-notification-success]').data('notification-success');
        if (successMsg) {
            this.success(successMsg);
        }

        // Check for error message
        const errorMsg = $('[data-notification-error]').data('notification-error');
        if (errorMsg) {
            this.error(errorMsg);
        }

        // Check for info message
        const infoMsg = $('[data-notification-info]').data('notification-info');
        if (infoMsg) {
            this.info(infoMsg);
        }

        // Check for warning message
        const warningMsg = $('[data-notification-warning]').data('notification-warning');
        if (warningMsg) {
            this.warning(warningMsg);
        }
    },

    // Create delete modal dynamically
    createDeleteModal: function() {
        const modalHtml = `
            <div class="modal fade" id="deleteModal" tabindex="-1" aria-labelledby="deleteModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-danger text-white">
                            <h5 class="modal-title" id="deleteModalLabel">
                                <i class="fas fa-exclamation-triangle me-2"></i>Confirm Delete
                            </h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div class="text-center mb-3">
                                <i class="fas fa-trash-alt fa-4x text-danger"></i>
                            </div>
                            <p class="text-center fs-5" id="deleteMessage">Are you sure you want to delete this item?</p>
                            <p class="text-center text-muted" id="deleteSubMessage">This action cannot be undone.</p>
                            <div id="deleteDetails" class="mt-3"></div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                                <i class="fas fa-times me-2"></i>Cancel
                            </button>
                            <form id="deleteForm" method="post" class="d-inline">
                                <input type="hidden" name="__RequestVerificationToken" value="" />
                                <button type="submit" class="btn btn-danger" id="deleteConfirmBtn">
                                    <i class="fas fa-trash me-2"></i>Delete
                                </button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>`;
        $('body').append(modalHtml);
    },

    // Create confirmation modal dynamically
    createConfirmModal: function() {
        const modalHtml = `
            <div class="modal fade" id="confirmModal" tabindex="-1" aria-labelledby="confirmModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header bg-primary text-white">
                            <h5 class="modal-title" id="confirmModalLabel">
                                <i class="fas fa-question-circle me-2"></i>Confirm Action
                            </h5>
                            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <p class="fs-5" id="confirmMessage">Are you sure you want to proceed?</p>
                            <div id="confirmDetails" class="mt-3"></div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                                <i class="fas fa-times me-2"></i>Cancel
                            </button>
                            <button type="button" class="btn btn-primary" id="confirmActionBtn">
                                <i class="fas fa-check me-2"></i>Confirm
                            </button>
                        </div>
                    </div>
                </div>
            </div>`;
        $('body').append(modalHtml);
    }
};

// Modal Management System
const ModalSystem = {
    // Show delete confirmation modal
    showDeleteModal: function(url, itemName, itemType, additionalDetails, callback) {
        const modal = document.getElementById('deleteModal');
        const form = document.getElementById('deleteForm');
        const message = document.getElementById('deleteMessage');
        const subMessage = document.getElementById('deleteSubMessage');
        const details = document.getElementById('deleteDetails');
        
        // Set form action
        form.action = url;
        
        // Set anti-forgery token
        const token = $('input[name="__RequestVerificationToken"]').first().val();
        if (token) {
            form.querySelector('input[name="__RequestVerificationToken"]').value = token;
        }
        
        // Set custom message
        if (itemName) {
            message.innerHTML = `Are you sure you want to delete <strong>${this.escapeHtml(itemName)}</strong>?`;
        }
        
        if (itemType) {
            subMessage.innerHTML = `This ${itemType} will be permanently deleted and cannot be recovered.`;
        }
        
        // Add additional details if provided
        if (additionalDetails) {
            details.innerHTML = additionalDetails;
        } else {
            details.innerHTML = '';
        }

        // Handle form submission with AJAX
        $('#deleteForm').off('submit').on('submit', function(e) {
            e.preventDefault();
            const $form = $(this);
            const submitBtn = $('#deleteConfirmBtn');
            
            // Show loading state
            submitBtn.prop('disabled', true);
            submitBtn.html('<i class="fas fa-spinner fa-spin me-2"></i>Deleting...');
            
            $.ajax({
                url: $form.attr('action'),
                type: 'POST',
                data: $form.serialize(),
                success: function(response) {
                    // Close modal
                    bootstrap.Modal.getInstance(modal).hide();
                    
                    // Show success message
                    NotificationSystem.success('Item deleted successfully');
                    
                    // Call callback if provided
                    if (callback && typeof callback === 'function') {
                        callback(response);
                    } else {
                        // Default: reload page after short delay
                        setTimeout(function() {
                            window.location.reload();
                        }, 1000);
                    }
                },
                error: function(xhr) {
                    // Reset button
                    submitBtn.prop('disabled', false);
                    submitBtn.html('<i class="fas fa-trash me-2"></i>Delete');
                    
                    // Show error message
                    const errorMsg = xhr.responseJSON?.message || 'Failed to delete item. Please try again.';
                    NotificationSystem.error(errorMsg);
                }
            });
        });
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
        
        return false; // Prevent default action
    },

    // Show confirmation modal
    showConfirmModal: function(message, details, onConfirm, options = {}) {
        const modal = document.getElementById('confirmModal');
        const msgElement = document.getElementById('confirmMessage');
        const detailsElement = document.getElementById('confirmDetails');
        const confirmBtn = document.getElementById('confirmActionBtn');
        
        // Set message and details
        msgElement.innerHTML = message;
        detailsElement.innerHTML = details || '';
        
        // Set button text if provided
        if (options.confirmText) {
            confirmBtn.innerHTML = `<i class="fas fa-check me-2"></i>${options.confirmText}`;
        }
        
        // Set header color if provided
        if (options.headerClass) {
            const header = modal.querySelector('.modal-header');
            header.className = `modal-header ${options.headerClass} text-white`;
        }
        
        // Handle confirm action
        $('#confirmActionBtn').off('click').on('click', function() {
            if (onConfirm && typeof onConfirm === 'function') {
                onConfirm();
            }
            bootstrap.Modal.getInstance(modal).hide();
        });
        
        // Show modal
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    },

    // Show loading modal
    showLoadingModal: function(message = 'Processing...') {
        const modalHtml = `
            <div class="modal fade" id="loadingModal" tabindex="-1" data-bs-backdrop="static" data-bs-keyboard="false">
                <div class="modal-dialog modal-dialog-centered modal-sm">
                    <div class="modal-content">
                        <div class="modal-body text-center py-4">
                            <div class="spinner-border text-primary mb-3" role="status">
                                <span class="visually-hidden">Loading...</span>
                            </div>
                            <p class="mb-0">${message}</p>
                        </div>
                    </div>
                </div>
            </div>`;
        
        if ($('#loadingModal').length === 0) {
            $('body').append(modalHtml);
        }
        
        const modal = new bootstrap.Modal(document.getElementById('loadingModal'));
        modal.show();
        
        return modal;
    },

    // Hide loading modal
    hideLoadingModal: function() {
        const modal = document.getElementById('loadingModal');
        if (modal) {
            const bsModal = bootstrap.Modal.getInstance(modal);
            if (bsModal) {
                bsModal.hide();
            }
        }
    },

    // Escape HTML to prevent XSS
    escapeHtml: function(text) {
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    }
};

// Global functions for backward compatibility
function showDeleteModal(url, itemName, itemType, additionalDetails) {
    return ModalSystem.showDeleteModal(url, itemName, itemType, additionalDetails);
}

function showConfirmModal(message, details, onConfirm, options) {
    return ModalSystem.showConfirmModal(message, details, onConfirm, options);
}

// AJAX Setup - Add notifications to all AJAX calls
$(document).ajaxError(function(event, jqXHR, ajaxSettings, thrownError) {
    if (jqXHR.status === 401) {
        NotificationSystem.error('Your session has expired. Please login again.', 'Authentication Required');
        setTimeout(function() {
            window.location.href = '/Account/Login';
        }, 2000);
    } else if (jqXHR.status === 403) {
        NotificationSystem.error('You do not have permission to perform this action.', 'Access Denied');
    } else if (jqXHR.status === 404) {
        NotificationSystem.error('The requested resource was not found.', 'Not Found');
    } else if (jqXHR.status === 500) {
        NotificationSystem.error('An internal server error occurred. Please try again later.', 'Server Error');
    }
});

// Helper function to handle form validation messages
function showValidationErrors(errors) {
    if (Array.isArray(errors)) {
        errors.forEach(error => {
            NotificationSystem.error(error);
        });
    } else if (typeof errors === 'object') {
        Object.keys(errors).forEach(key => {
            if (Array.isArray(errors[key])) {
                errors[key].forEach(error => {
                    NotificationSystem.error(error);
                });
            } else {
                NotificationSystem.error(errors[key]);
            }
        });
    } else {
        NotificationSystem.error('Validation failed. Please check your input.');
    }
}

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        NotificationSystem,
        ModalSystem
    };
}