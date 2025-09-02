// App.js - Main application JavaScript
(function (window, $) {
    'use strict';

    // Global app namespace
    window.App = window.App || {};

    // Loading mask functionality
    App.Loading = {
        show: function (message) {
            message = message || 'Loading...';
            if ($('#loadingMask').length === 0) {
                $('body').append(`
                    <div id="loadingMask" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
                        <div class="bg-white dark:bg-gray-800 rounded-lg p-6 shadow-xl">
                            <div class="flex items-center space-x-3">
                                <svg class="animate-spin h-8 w-8 text-blue-500" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                    <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                                    <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                </svg>
                                <span class="text-gray-700 dark:text-gray-200 font-medium">${message}</span>
                            </div>
                        </div>
                    </div>
                `);
            }
            $('#loadingMask').fadeIn(200);
        },
        hide: function () {
            $('#loadingMask').fadeOut(200, function() {
                $(this).remove();
            });
        }
    };

    // Toast notifications
    App.Toast = {
        success: function (message, title) {
            toastr.success(message, title || 'Success');
        },
        error: function (message, title) {
            toastr.error(message, title || 'Error');
        },
        warning: function (message, title) {
            toastr.warning(message, title || 'Warning');
        },
        info: function (message, title) {
            toastr.info(message, title || 'Information');
        }
    };

    // Modal functionality
    App.Modal = {
        show: function (options) {
            const defaults = {
                title: 'Modal',
                content: '',
                size: 'md', // sm, md, lg, xl
                buttons: [],
                closeButton: true,
                backdrop: true,
                keyboard: true,
                onShow: null,
                onHidden: null
            };

            const settings = $.extend({}, defaults, options);
            
            // Remove existing modal if any
            $('#appModal').remove();

            // Size classes
            const sizeClasses = {
                'sm': 'max-w-md',
                'md': 'max-w-lg',
                'lg': 'max-w-2xl',
                'xl': 'max-w-4xl'
            };

            // Build buttons HTML
            let buttonsHtml = '';
            settings.buttons.forEach(button => {
                const btnClass = button.class || 'bg-gray-500 hover:bg-gray-700 text-white';
                buttonsHtml += `<button type="button" class="px-4 py-2 ${btnClass} font-bold rounded" data-action="${button.action || ''}">${button.text}</button>`;
            });

            // Build modal HTML
            const modalHtml = `
                <div id="appModal" class="fixed inset-0 z-50 overflow-y-auto hidden">
                    <div class="flex items-center justify-center min-h-screen px-4 pt-4 pb-20 text-center sm:block sm:p-0">
                        <div class="fixed inset-0 transition-opacity" aria-hidden="true">
                            <div class="absolute inset-0 bg-gray-500 opacity-75"></div>
                        </div>
                        <span class="hidden sm:inline-block sm:align-middle sm:h-screen" aria-hidden="true">&#8203;</span>
                        <div class="inline-block align-bottom bg-white dark:bg-gray-800 rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle ${sizeClasses[settings.size]} w-full">
                            <div class="bg-white dark:bg-gray-800 px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                                <div class="flex justify-between items-center mb-4">
                                    <h3 class="text-lg font-medium text-gray-900 dark:text-gray-100">${settings.title}</h3>
                                    ${settings.closeButton ? '<button type="button" class="text-gray-400 hover:text-gray-500 dark:hover:text-gray-300" data-action="close"><i class="fas fa-times"></i></button>' : ''}
                                </div>
                                <div class="modal-content">
                                    ${settings.content}
                                </div>
                            </div>
                            ${buttonsHtml ? `<div class="bg-gray-50 dark:bg-gray-700 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse gap-2">${buttonsHtml}</div>` : ''}
                        </div>
                    </div>
                </div>
            `;

            // Append to body
            $('body').append(modalHtml);

            const $modal = $('#appModal');

            // Bind button events
            $modal.find('[data-action]').on('click', function () {
                const action = $(this).data('action');
                if (action === 'close') {
                    App.Modal.hide();
                } else {
                    settings.buttons.forEach(button => {
                        if (button.action === action && button.onClick) {
                            button.onClick();
                        }
                    });
                }
            });

            // Show modal
            $modal.removeClass('hidden');
            
            if (settings.onShow) {
                settings.onShow();
            }

            // Backdrop click
            if (settings.backdrop) {
                $modal.on('click', function (e) {
                    if ($(e.target).hasClass('fixed')) {
                        App.Modal.hide();
                    }
                });
            }

            // Keyboard ESC
            if (settings.keyboard) {
                $(document).on('keydown.modal', function (e) {
                    if (e.keyCode === 27) {
                        App.Modal.hide();
                    }
                });
            }

            $modal.data('onHidden', settings.onHidden);
        },
        
        hide: function () {
            const $modal = $('#appModal');
            const onHidden = $modal.data('onHidden');
            
            $modal.fadeOut(200, function () {
                $(this).remove();
                $(document).off('keydown.modal');
                if (onHidden) {
                    onHidden();
                }
            });
        },

        confirm: function (options) {
            const defaults = {
                title: 'Confirm',
                message: 'Are you sure?',
                confirmText: 'Yes',
                cancelText: 'Cancel',
                confirmClass: 'bg-red-500 hover:bg-red-700 text-white',
                onConfirm: null,
                onCancel: null
            };

            const settings = $.extend({}, defaults, options);

            this.show({
                title: settings.title,
                content: `<p class="text-gray-700 dark:text-gray-300">${settings.message}</p>`,
                size: 'sm',
                buttons: [
                    {
                        text: settings.confirmText,
                        class: settings.confirmClass,
                        action: 'confirm',
                        onClick: function () {
                            App.Modal.hide();
                            if (settings.onConfirm) settings.onConfirm();
                        }
                    },
                    {
                        text: settings.cancelText,
                        class: 'bg-gray-300 hover:bg-gray-400 text-gray-800',
                        action: 'cancel',
                        onClick: function () {
                            App.Modal.hide();
                            if (settings.onCancel) settings.onCancel();
                        }
                    }
                ]
            });
        }
    };

    // AJAX helpers
    App.Ajax = {
        post: function (url, data, options) {
            const defaults = {
                showLoading: true,
                loadingMessage: 'Processing...',
                onSuccess: null,
                onError: null,
                onComplete: null,
                contentType: 'application/json',
                processData: true
            };

            const settings = $.extend({}, defaults, options);

            if (settings.showLoading) {
                App.Loading.show(settings.loadingMessage);
            }

            // Get anti-forgery token
            const token = $('input[name="__RequestVerificationToken"]').val() || 
                         $('meta[name="csrf-token"]').attr('content');

            // Prepare AJAX settings
            const ajaxSettings = {
                url: url,
                type: 'POST',
                headers: {
                    'RequestVerificationToken': token
                }
            };

            // Handle data based on content type
            if (settings.contentType === 'application/json') {
                ajaxSettings.contentType = 'application/json; charset=utf-8';
                ajaxSettings.data = typeof data === 'string' ? data : JSON.stringify(data);
            } else {
                ajaxSettings.data = data;
                ajaxSettings.processData = settings.processData;
            }

            return $.ajax($.extend(ajaxSettings, {
                success: function (response) {
                    if (settings.onSuccess) {
                        settings.onSuccess(response);
                    }
                },
                error: function (xhr, status, error) {
                    console.error('AJAX Error:', error);
                    App.Toast.error('An error occurred: ' + (xhr.responseJSON?.message || error));
                    if (settings.onError) {
                        settings.onError(xhr, status, error);
                    }
                },
                complete: function () {
                    if (settings.showLoading) {
                        App.Loading.hide();
                    }
                    if (settings.onComplete) {
                        settings.onComplete();
                    }
                }
            }));
        },

        get: function (url, data, options) {
            const defaults = {
                showLoading: true,
                loadingMessage: 'Loading...',
                onSuccess: null,
                onError: null,
                onComplete: null
            };

            const settings = $.extend({}, defaults, options);

            if (settings.showLoading) {
                App.Loading.show(settings.loadingMessage);
            }

            return $.ajax({
                url: url,
                type: 'GET',
                data: data,
                success: function (response) {
                    if (settings.onSuccess) {
                        settings.onSuccess(response);
                    }
                },
                error: function (xhr, status, error) {
                    console.error('AJAX Error:', error);
                    App.Toast.error('An error occurred: ' + (xhr.responseJSON?.message || error));
                    if (settings.onError) {
                        settings.onError(xhr, status, error);
                    }
                },
                complete: function () {
                    if (settings.showLoading) {
                        App.Loading.hide();
                    }
                    if (settings.onComplete) {
                        settings.onComplete();
                    }
                }
            });
        }
    };

    // Form helpers
    App.Form = {
        validate: function (formSelector) {
            const $form = $(formSelector);
            let isValid = true;

            // Remove previous errors
            $form.find('.field-validation-error').text('');
            $form.find('.border-red-500').removeClass('border-red-500');
            
            // Check required fields
            $form.find('[required]').each(function () {
                const $field = $(this);
                if (!$field.val() || $field.val().trim() === '') {
                    isValid = false;
                    $field.addClass('border-red-500');
                    const $error = $field.siblings('.field-validation-error');
                    if ($error.length) {
                        $error.text('This field is required');
                    }
                }
            });

            return isValid;
        },

        serialize: function (formSelector) {
            const $form = $(formSelector);
            const data = {};
            
            $form.find('input, select, textarea').each(function () {
                const $field = $(this);
                const name = $field.attr('name');
                if (name && name !== '__RequestVerificationToken') {
                    if ($field.attr('type') === 'checkbox') {
                        data[name] = $field.is(':checked');
                    } else if ($field.attr('type') === 'radio') {
                        if ($field.is(':checked')) {
                            data[name] = $field.val();
                        }
                    } else {
                        data[name] = $field.val() || '';
                    }
                }
            });

            return data;
        },

        reset: function (formSelector) {
            const $form = $(formSelector);
            $form[0].reset();
            $form.find('.field-validation-error').text('');
            $form.find('.border-red-500').removeClass('border-red-500');
        }
    };

    // DataTable helpers
    App.DataTable = {
        init: function (selector, options) {
            const defaults = {
                processing: true,
                serverSide: false,
                responsive: true,
                autoWidth: false,
                dom: '<"flex flex-col sm:flex-row justify-between mb-4"<"mb-2 sm:mb-0"l><"mb-2 sm:mb-0"f>>rtip',
                language: {
                    search: "_INPUT_",
                    searchPlaceholder: "Search...",
                    lengthMenu: "Show _MENU_ entries",
                    info: "Showing _START_ to _END_ of _TOTAL_ entries",
                    paginate: {
                        previous: '<i class="fas fa-chevron-left"></i>',
                        next: '<i class="fas fa-chevron-right"></i>'
                    }
                },
                pageLength: 10,
                lengthMenu: [[10, 25, 50, 100], [10, 25, 50, 100]]
            };

            const settings = $.extend({}, defaults, options);
            return $(selector).DataTable(settings);
        },

        reload: function (table) {
            if (table) {
                table.ajax.reload(null, false);
            }
        }
    };

    // Initialize on document ready
    $(document).ready(function () {
        // Configure toastr
        if (typeof toastr !== 'undefined') {
            toastr.options = {
                closeButton: true,
                debug: false,
                newestOnTop: true,
                progressBar: true,
                positionClass: "toast-top-right",
                preventDuplicates: false,
                onclick: null,
                showDuration: "300",
                hideDuration: "1000",
                timeOut: "5000",
                extendedTimeOut: "1000",
                showEasing: "swing",
                hideEasing: "linear",
                showMethod: "fadeIn",
                hideMethod: "fadeOut"
            };
        }

        // CSRF token for AJAX
        $.ajaxSetup({
            beforeSend: function (xhr, settings) {
                if (settings.type === 'POST' || settings.type === 'PUT' || settings.type === 'DELETE') {
                    const token = $('input[name="__RequestVerificationToken"]').val();
                    if (token) {
                        xhr.setRequestHeader("RequestVerificationToken", token);
                    }
                }
            }
        });

        // Global error handler
        $(document).ajaxError(function (event, xhr, settings, error) {
            if (xhr.status === 401) {
                App.Toast.error('Your session has expired. Please login again.');
                setTimeout(function () {
                    window.location.href = '/Account/Login';
                }, 2000);
            } else if (xhr.status === 403) {
                App.Toast.error('You do not have permission to perform this action.');
            }
        });
    });

})(window, jQuery);