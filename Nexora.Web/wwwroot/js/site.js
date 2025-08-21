// ========================================
// Global Loading Mask System
// ========================================
const LoadingMask = {
    // Configuration
    config: {
        defaultMessage: 'Loading...',
        defaultAnimation: 'breathing', // breathing, pulse, rotate, bounce
        minDisplayTime: 300, // Minimum time to show loading mask (ms)
        fadeSpeed: 200, // Fade in/out speed (ms)
    },
    
    // State tracking
    state: {
        isVisible: false,
        showTime: null,
        hideTimer: null,
        requestCounter: 0, // Track multiple simultaneous requests
    },
    
    // Show loading mask
    show: function(options = {}) {
        const settings = {
            message: options.message || this.config.defaultMessage,
            animation: options.animation || this.config.defaultAnimation,
            showProgress: options.showProgress || false,
            immediate: options.immediate || false
        };
        
        // Increment request counter
        this.state.requestCounter++;
        
        // Clear any pending hide timer
        if (this.state.hideTimer) {
            clearTimeout(this.state.hideTimer);
            this.state.hideTimer = null;
        }
        
        // If already visible, just update the message
        if (this.state.isVisible) {
            this.updateMessage(settings.message);
            return;
        }
        
        // Record show time
        this.state.showTime = Date.now();
        this.state.isVisible = true;
        
        // Get the mask element
        const mask = document.getElementById('loadingMask');
        if (!mask) return;
        
        // Update message
        const messageElement = mask.querySelector('.loading-message');
        if (messageElement) {
            messageElement.textContent = settings.message;
        }
        
        // Set animation class
        mask.className = `loading-mask ${settings.animation}`;
        
        // Add progress bar if requested
        if (settings.showProgress) {
            this.addProgressBar();
        }
        
        // Show the mask
        if (settings.immediate) {
            mask.style.display = 'flex';
            mask.style.opacity = '1';
        } else {
            mask.style.display = 'flex';
            mask.style.opacity = '0';
            setTimeout(() => {
                mask.style.transition = `opacity ${this.config.fadeSpeed}ms ease`;
                mask.style.opacity = '1';
            }, 10);
        }
    },
    
    // Hide loading mask
    hide: function(immediate = false) {
        // Decrement request counter
        this.state.requestCounter = Math.max(0, this.state.requestCounter - 1);
        
        // If there are still pending requests, don't hide
        if (this.state.requestCounter > 0) {
            return;
        }
        
        if (!this.state.isVisible) return;
        
        const mask = document.getElementById('loadingMask');
        if (!mask) return;
        
        // Calculate how long the mask has been shown
        const shownTime = Date.now() - this.state.showTime;
        const remainingTime = Math.max(0, this.config.minDisplayTime - shownTime);
        
        // Hide after minimum display time
        const hideFunction = () => {
            if (immediate) {
                mask.style.display = 'none';
                this.state.isVisible = false;
            } else {
                mask.style.transition = `opacity ${this.config.fadeSpeed}ms ease`;
                mask.style.opacity = '0';
                setTimeout(() => {
                    mask.style.display = 'none';
                    this.state.isVisible = false;
                    this.removeProgressBar();
                }, this.config.fadeSpeed);
            }
        };
        
        if (immediate || remainingTime === 0) {
            hideFunction();
        } else {
            this.state.hideTimer = setTimeout(hideFunction, remainingTime);
        }
    },
    
    // Force hide (ignores request counter)
    forceHide: function() {
        this.state.requestCounter = 0;
        this.hide(true);
    },
    
    // Update message
    updateMessage: function(message) {
        const mask = document.getElementById('loadingMask');
        if (!mask) return;
        
        const messageElement = mask.querySelector('.loading-message');
        if (messageElement) {
            messageElement.textContent = message;
        }
    },
    
    // Change animation
    changeAnimation: function(animation) {
        const mask = document.getElementById('loadingMask');
        if (!mask) return;
        
        mask.className = `loading-mask ${animation}`;
    },
    
    // Add progress bar
    addProgressBar: function() {
        const mask = document.getElementById('loadingMask');
        if (!mask) return;
        
        const loadingText = mask.querySelector('.loading-text');
        if (!loadingText) return;
        
        // Check if progress bar already exists
        if (mask.querySelector('.loading-progress')) return;
        
        const progressBar = document.createElement('div');
        progressBar.className = 'loading-progress';
        progressBar.innerHTML = '<div class="loading-progress-bar"></div>';
        loadingText.appendChild(progressBar);
    },
    
    // Remove progress bar
    removeProgressBar: function() {
        const mask = document.getElementById('loadingMask');
        if (!mask) return;
        
        const progressBar = mask.querySelector('.loading-progress');
        if (progressBar) {
            progressBar.remove();
        }
    },
    
    // Show with progress updates
    showWithProgress: function(message) {
        this.show({
            message: message,
            showProgress: true
        });
    },
    
    // Quick show/hide methods
    showQuick: function(message) {
        this.show({ message: message, immediate: true });
    },
    
    hideQuick: function() {
        this.hide(true);
    }
};

// ========================================
// jQuery AJAX Global Handlers
// ========================================
$(document).ready(function() {
    // Configure jQuery AJAX defaults
    $.ajaxSetup({
        beforeSend: function(xhr, settings) {
            // Add CSRF token to all AJAX requests
            const token = $('input[name="__RequestVerificationToken"]').val();
            if (token) {
                xhr.setRequestHeader('RequestVerificationToken', token);
            }
            
            // Skip loading mask if explicitly disabled
            if (settings.skipLoadingMask) {
                return true;
            }
            
            // Skip loading mask for DataTables requests (they handle their own processing indicator)
            const isDataTablesRequest = settings.url && (
                settings.url.includes('GetLogs') || 
                settings.url.includes('GetAuditLogs') ||
                settings.data && typeof settings.data === 'string' && settings.data.includes('draw=')
            );
            
            // Skip loading mask for image uploads (they handle their own)
            const isImageUpload = settings.url && settings.url.includes('UploadImage');
            
            // Show loading mask for non-GET requests or if specified, but not for DataTables or image uploads
            if (!isDataTablesRequest && !isImageUpload && (settings.type !== 'GET' || settings.showLoading)) {
                const loadingMessage = settings.loadingMessage || 'Processing...';
                LoadingMask.show({ message: loadingMessage });
            }
        },
        complete: function(xhr, status) {
            // Skip if explicitly disabled
            if (this.skipLoadingMask) {
                return;
            }
            
            // Only hide loading mask if it was shown (not for DataTables)
            const isDataTablesRequest = this.url && (
                this.url.includes('GetLogs') || 
                this.url.includes('GetAuditLogs') ||
                this.data && typeof this.data === 'string' && this.data.includes('draw=')
            );
            
            // Skip for image uploads (they handle their own)
            const isImageUpload = this.url && this.url.includes('UploadImage');
            
            if (!isDataTablesRequest && !isImageUpload) {
                LoadingMask.hide();
            }
        },
        error: function(xhr, status, error) {
            // Skip if explicitly disabled
            if (this.skipLoadingMask) {
                return;
            }
            
            // Only hide loading mask if it was shown (not for DataTables)
            const isDataTablesRequest = this.url && (
                this.url.includes('GetLogs') || 
                this.url.includes('GetAuditLogs') ||
                this.data && typeof this.data === 'string' && this.data.includes('draw=')
            );
            
            // Skip for image uploads (they handle their own)
            const isImageUpload = this.url && this.url.includes('UploadImage');
            
            if (!isDataTablesRequest && !isImageUpload) {
                LoadingMask.hide();
            }
            
            // Show error notification if not handled
            if (!xhr.handled) {
                let errorMessage = 'An error occurred. Please try again.';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage = xhr.responseJSON.message;
                } else if (xhr.responseText) {
                    try {
                        const response = JSON.parse(xhr.responseText);
                        if (response.message) {
                            errorMessage = response.message;
                        }
                    } catch (e) {
                        // Use default message
                    }
                }
                
                if (typeof toastr !== 'undefined') {
                    toastr.error(errorMessage);
                } else {
                    alert(errorMessage);
                }
            }
        }
    });
    
    // Intercept form submissions
    $('form[data-ajax="true"]').on('submit', function(e) {
        e.preventDefault();
        
        const form = $(this);
        const formData = new FormData(this);
        const submitButton = form.find('button[type="submit"]');
        
        // Disable submit button
        submitButton.prop('disabled', true);
        
        // Show loading mask
        LoadingMask.show({
            message: form.data('loading-message') || 'Submitting...'
        });
        
        $.ajax({
            url: form.attr('action'),
            type: form.attr('method') || 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function(response) {
                // Handle success
                if (form.data('success-callback')) {
                    window[form.data('success-callback')](response);
                } else if (form.data('success-url')) {
                    window.location.href = form.data('success-url');
                } else {
                    if (typeof toastr !== 'undefined') {
                        toastr.success('Operation completed successfully');
                    }
                    // Reset form if specified
                    if (form.data('reset-on-success')) {
                        form[0].reset();
                    }
                }
            },
            error: function(xhr) {
                xhr.handled = true; // Prevent default error handling
                
                // Show validation errors if any
                if (xhr.status === 400 && xhr.responseJSON && xhr.responseJSON.errors) {
                    const errors = xhr.responseJSON.errors;
                    let errorHtml = '<ul class="mb-0">';
                    for (let field in errors) {
                        errors[field].forEach(function(error) {
                            errorHtml += `<li>${error}</li>`;
                        });
                    }
                    errorHtml += '</ul>';
                    
                    if (typeof toastr !== 'undefined') {
                        toastr.error(errorHtml, 'Validation Errors');
                    } else {
                        alert('Validation errors occurred. Please check your input.');
                    }
                } else {
                    if (typeof toastr !== 'undefined') {
                        toastr.error('An error occurred while processing your request.');
                    } else {
                        alert('An error occurred. Please try again.');
                    }
                }
            },
            complete: function() {
                // Re-enable submit button
                submitButton.prop('disabled', false);
                LoadingMask.hide();
            }
        });
    });
    
    // Add loading mask to DataTables
    if ($.fn.DataTable) {
        $.fn.dataTable.ext.errMode = 'none';
        
        // Let DataTables handle its own processing indicator
        // Don't interfere with DataTables' built-in loading system
    }
});

// ========================================
// Utility Functions
// ========================================

// Show loading for specific operation
function showLoading(message, animation) {
    LoadingMask.show({ 
        message: message || 'Loading...', 
        animation: animation || 'breathing' 
    });
}

// Hide loading
function hideLoading() {
    LoadingMask.hide();
}

// Execute with loading
function executeWithLoading(func, message) {
    LoadingMask.show({ message: message || 'Processing...' });
    
    try {
        const result = func();
        
        // Handle promises
        if (result && typeof result.then === 'function') {
            return result
                .then(response => {
                    LoadingMask.hide();
                    return response;
                })
                .catch(error => {
                    LoadingMask.hide();
                    throw error;
                });
        } else {
            LoadingMask.hide();
            return result;
        }
    } catch (error) {
        LoadingMask.hide();
        throw error;
    }
}

// AJAX helper with loading
function ajaxWithLoading(options) {
    const loadingMessage = options.loadingMessage || 'Loading...';
    delete options.loadingMessage;
    
    LoadingMask.show({ message: loadingMessage });
    
    return $.ajax(options)
        .always(function() {
            LoadingMask.hide();
        });
}

// Form submit with loading
function submitFormWithLoading(formId, message) {
    const form = document.getElementById(formId);
    if (!form) return;
    
    LoadingMask.show({ message: message || 'Submitting...' });
    form.submit();
}

// ========================================
// Export for use in other modules
// ========================================
window.LoadingMask = LoadingMask;
window.showLoading = showLoading;
window.hideLoading = hideLoading;
window.executeWithLoading = executeWithLoading;
window.ajaxWithLoading = ajaxWithLoading;
window.submitFormWithLoading = submitFormWithLoading;