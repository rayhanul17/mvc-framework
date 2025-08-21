// Enhanced Validation Script for Nexora Framework
(function () {
    'use strict';

    // Add validation styles
    const style = document.createElement('style');
    style.innerHTML = `
        .field-validation-error {
            color: #dc3545;
            font-size: 0.875rem;
            margin-top: 0.25rem;
            display: block;
        }
        
        .input-validation-error {
            border-color: #dc3545 !important;
            box-shadow: 0 0 0 0.2rem rgba(220, 53, 69, 0.25) !important;
        }
        
        .validation-summary-errors {
            background-color: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
            padding: 0.75rem 1.25rem;
            border-radius: 0.25rem;
            margin-bottom: 1rem;
        }
        
        .validation-summary-errors ul {
            margin-bottom: 0;
            padding-left: 1.25rem;
        }
        
        .form-control.is-valid {
            border-color: #28a745;
            padding-right: calc(1.5em + 0.75rem);
            background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 8 8'%3e%3cpath fill='%2328a745' d='M2.3 6.73L.6 4.53c-.4-1.04.46-1.4 1.1-.8l1.1 1.4 3.4-3.8c.6-.63 1.6-.27 1.2.7l-4 4.6c-.43.5-.8.4-1.1.1z'/%3e%3c/svg%3e");
            background-repeat: no-repeat;
            background-position: right calc(0.375em + 0.1875rem) center;
            background-size: calc(0.75em + 0.375rem) calc(0.75em + 0.375rem);
        }
        
        .form-control.is-invalid {
            border-color: #dc3545;
            padding-right: calc(1.5em + 0.75rem);
            background-image: url("data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' fill='%23dc3545' viewBox='-2 -2 7 7'%3e%3cpath stroke='%23dc3545' d='M0 0l3 3m0-3L0 3'/%3e%3ccircle r='.5'/%3e%3ccircle cx='3' r='.5'/%3e%3ccircle cy='3' r='.5'/%3e%3ccircle cx='3' cy='3' r='.5'/%3e%3c/svg%3E");
            background-repeat: no-repeat;
            background-position: right calc(0.375em + 0.1875rem) center;
            background-size: calc(0.75em + 0.375rem) calc(0.75em + 0.375rem);
        }
        
        .password-strength {
            height: 5px;
            margin-top: 5px;
            border-radius: 3px;
            transition: all 0.3s ease;
        }
        
        .password-strength.weak { background-color: #dc3545; width: 33%; }
        .password-strength.medium { background-color: #ffc107; width: 66%; }
        .password-strength.strong { background-color: #28a745; width: 100%; }
        
        .password-requirements {
            font-size: 0.875rem;
            margin-top: 0.5rem;
        }
        
        .password-requirements li {
            list-style: none;
            padding-left: 1.5rem;
            position: relative;
        }
        
        .password-requirements li:before {
            content: '✗';
            position: absolute;
            left: 0;
            color: #dc3545;
        }
        
        .password-requirements li.met:before {
            content: '✓';
            color: #28a745;
        }
    `;
    document.head.appendChild(style);

    // Enhanced validation functions
    window.EnhancedValidation = {
        // Email validation
        validateEmail: function(email) {
            const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return re.test(email);
        },

        // Password strength checker
        checkPasswordStrength: function(password) {
            let strength = 0;
            const requirements = {
                length: password.length >= 8,
                uppercase: /[A-Z]/.test(password),
                lowercase: /[a-z]/.test(password),
                number: /[0-9]/.test(password),
                special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
            };

            Object.values(requirements).forEach(met => {
                if (met) strength++;
            });

            return {
                score: strength,
                requirements: requirements,
                level: strength < 3 ? 'weak' : strength < 4 ? 'medium' : 'strong'
            };
        },

        // Phone number validation
        validatePhone: function(phone) {
            const re = /^[\d\s\-\+\(\)]+$/;
            return re.test(phone) && phone.replace(/\D/g, '').length >= 10;
        },

        // URL validation
        validateUrl: function(url) {
            try {
                new URL(url);
                return true;
            } catch {
                return false;
            }
        },

        // Required field validation
        validateRequired: function(value) {
            return value && value.trim().length > 0;
        },

        // Min/Max length validation
        validateLength: function(value, min, max) {
            const length = value ? value.length : 0;
            return length >= min && (max ? length <= max : true);
        },

        // Number range validation
        validateRange: function(value, min, max) {
            const num = parseFloat(value);
            return !isNaN(num) && num >= min && num <= max;
        },

        // Initialize validation for a form
        initializeForm: function(formSelector) {
            const form = document.querySelector(formSelector);
            if (!form) return;

            // Add real-time validation
            form.querySelectorAll('input, textarea, select').forEach(field => {
                // Skip hidden fields
                if (field.type === 'hidden') return;

                // Add validation on blur
                field.addEventListener('blur', function() {
                    EnhancedValidation.validateField(this);
                });

                // Add validation on input for certain fields
                if (field.type === 'email' || field.type === 'password' || field.type === 'tel' || field.type === 'url') {
                    field.addEventListener('input', function() {
                        EnhancedValidation.validateField(this);
                    });
                }

                // Special handling for password fields
                if (field.type === 'password' && field.name.toLowerCase().includes('password') && !field.name.toLowerCase().includes('confirm')) {
                    EnhancedValidation.addPasswordStrengthIndicator(field);
                }
            });

            // Form submit validation
            form.addEventListener('submit', function(e) {
                let isValid = true;
                form.querySelectorAll('input, textarea, select').forEach(field => {
                    if (field.type !== 'hidden' && !EnhancedValidation.validateField(field)) {
                        isValid = false;
                    }
                });

                if (!isValid) {
                    e.preventDefault();
                    // Scroll to first error
                    const firstError = form.querySelector('.is-invalid');
                    if (firstError) {
                        firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        firstError.focus();
                    }
                }
            });
        },

        // Validate individual field
        validateField: function(field) {
            let isValid = true;
            let errorMessage = '';

            // Remove existing validation classes
            field.classList.remove('is-valid', 'is-invalid');

            // Required validation
            if (field.hasAttribute('required') || field.hasAttribute('data-val-required')) {
                if (!EnhancedValidation.validateRequired(field.value)) {
                    isValid = false;
                    errorMessage = field.getAttribute('data-val-required') || 'This field is required.';
                }
            }

            // Type-specific validation
            if (isValid && field.value) {
                switch (field.type) {
                    case 'email':
                        if (!EnhancedValidation.validateEmail(field.value)) {
                            isValid = false;
                            errorMessage = 'Please enter a valid email address.';
                        }
                        break;
                    case 'tel':
                        if (!EnhancedValidation.validatePhone(field.value)) {
                            isValid = false;
                            errorMessage = 'Please enter a valid phone number.';
                        }
                        break;
                    case 'url':
                        if (!EnhancedValidation.validateUrl(field.value)) {
                            isValid = false;
                            errorMessage = 'Please enter a valid URL.';
                        }
                        break;
                    case 'number':
                        const min = field.getAttribute('min');
                        const max = field.getAttribute('max');
                        if (min && max && !EnhancedValidation.validateRange(field.value, min, max)) {
                            isValid = false;
                            errorMessage = `Please enter a number between ${min} and ${max}.`;
                        }
                        break;
                }

                // Length validation
                const minLength = field.getAttribute('minlength') || field.getAttribute('data-val-length-min');
                const maxLength = field.getAttribute('maxlength') || field.getAttribute('data-val-length-max');
                if (minLength || maxLength) {
                    if (!EnhancedValidation.validateLength(field.value, minLength || 0, maxLength)) {
                        isValid = false;
                        if (minLength && maxLength) {
                            errorMessage = `Must be between ${minLength} and ${maxLength} characters.`;
                        } else if (minLength) {
                            errorMessage = `Must be at least ${minLength} characters.`;
                        } else {
                            errorMessage = `Must be no more than ${maxLength} characters.`;
                        }
                    }
                }

                // Pattern validation
                const pattern = field.getAttribute('pattern');
                if (pattern && !new RegExp(pattern).test(field.value)) {
                    isValid = false;
                    errorMessage = field.getAttribute('title') || 'Please match the requested format.';
                }
            }

            // Update validation UI
            if (field.value || field.hasAttribute('required')) {
                field.classList.add(isValid ? 'is-valid' : 'is-invalid');
                
                // Update or create error message
                let errorElement = field.parentElement.querySelector('.field-validation-error');
                if (!isValid) {
                    if (!errorElement) {
                        errorElement = document.createElement('span');
                        errorElement.className = 'field-validation-error';
                        field.parentElement.appendChild(errorElement);
                    }
                    errorElement.textContent = errorMessage;
                } else if (errorElement) {
                    errorElement.remove();
                }
            }

            return isValid;
        },

        // Add password strength indicator
        addPasswordStrengthIndicator: function(field) {
            const container = document.createElement('div');
            container.className = 'password-strength-container';
            
            const strengthBar = document.createElement('div');
            strengthBar.className = 'password-strength';
            
            const requirements = document.createElement('ul');
            requirements.className = 'password-requirements';
            requirements.innerHTML = `
                <li data-requirement="length">At least 8 characters</li>
                <li data-requirement="uppercase">One uppercase letter</li>
                <li data-requirement="lowercase">One lowercase letter</li>
                <li data-requirement="number">One number</li>
                <li data-requirement="special">One special character</li>
            `;
            
            container.appendChild(strengthBar);
            container.appendChild(requirements);
            field.parentElement.appendChild(container);

            field.addEventListener('input', function() {
                const result = EnhancedValidation.checkPasswordStrength(this.value);
                
                strengthBar.className = 'password-strength ' + result.level;
                
                Object.keys(result.requirements).forEach(req => {
                    const li = requirements.querySelector(`[data-requirement="${req}"]`);
                    if (li) {
                        li.classList.toggle('met', result.requirements[req]);
                    }
                });
            });
        },

        // Initialize all forms on page
        initializeAll: function() {
            document.querySelectorAll('form').forEach(form => {
                // Skip forms that should not have validation
                if (form.classList.contains('no-validation')) return;
                
                EnhancedValidation.initializeForm('#' + (form.id || form.name));
            });
        }
    };

    // Auto-initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', EnhancedValidation.initializeAll);
    } else {
        EnhancedValidation.initializeAll();
    }
})();