/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Views/**/*.{cshtml,html,js}',
    './wwwroot/**/*.{html,js}',
    './Modules/**/Views/**/*.{cshtml,html,js}',
    './Areas/**/Views/**/*.{cshtml,html,js}'
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Brand Colors
        primary: {
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a',
          950: '#172554'
        },
        secondary: {
          50: '#f8fafc',
          100: '#f1f5f9',
          200: '#e2e8f0',
          300: '#cbd5e1',
          400: '#94a3b8',
          500: '#64748b',
          600: '#475569',
          700: '#334155',
          800: '#1e293b',
          900: '#0f172a',
          950: '#020617'
        },
        // Status Colors
        success: {
          50: '#ecfdf5',
          100: '#d1fae5',
          200: '#a7f3d0',
          300: '#6ee7b7',
          400: '#34d399',
          500: '#10b981',
          600: '#059669',
          700: '#047857',
          800: '#065f46',
          900: '#064e3b',
          950: '#022c22'
        },
        danger: {
          50: '#fef2f2',
          100: '#fee2e2',
          200: '#fecaca',
          300: '#fca5a5',
          400: '#f87171',
          500: '#ef4444',
          600: '#dc2626',
          700: '#b91c1c',
          800: '#991b1b',
          900: '#7f1d1d',
          950: '#450a0a'
        },
        warning: {
          50: '#fffbeb',
          100: '#fef3c7',
          200: '#fde68a',
          300: '#fcd34d',
          400: '#fbbf24',
          500: '#f59e0b',
          600: '#d97706',
          700: '#b45309',
          800: '#92400e',
          900: '#78350f',
          950: '#451a03'
        },
        info: {
          50: '#f0f9ff',
          100: '#e0f2fe',
          200: '#bae6fd',
          300: '#7dd3fc',
          400: '#38bdf8',
          500: '#0ea5e9',
          600: '#0284c7',
          700: '#0369a1',
          800: '#075985',
          900: '#0c4a6e',
          950: '#082f49'
        }
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['JetBrains Mono', 'ui-monospace', 'monospace']
      },
      fontSize: {
        '2xs': ['0.625rem', { lineHeight: '0.75rem' }],
        '3xl': ['1.875rem', { lineHeight: '2.25rem' }],
        '4xl': ['2.25rem', { lineHeight: '2.5rem' }],
        '5xl': ['3rem', { lineHeight: '1' }],
        '6xl': ['3.75rem', { lineHeight: '1' }]
      },
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '128': '32rem',
        '144': '36rem'
      },
      borderRadius: {
        'xl': '0.75rem',
        '2xl': '1rem',
        '3xl': '1.5rem'
      },
      boxShadow: {
        'soft': '0 2px 15px -3px rgba(0, 0, 0, 0.07), 0 10px 20px -2px rgba(0, 0, 0, 0.04)',
        'medium': '0 4px 25px -2px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)',
        'hard': '0 10px 40px rgba(0, 0, 0, 0.1)',
        'inner-soft': 'inset 0 2px 4px 0 rgba(0, 0, 0, 0.05)',
        'colored-primary': '0 4px 14px 0 rgba(59, 130, 246, 0.39)',
        'colored-success': '0 4px 14px 0 rgba(16, 185, 129, 0.39)',
        'colored-danger': '0 4px 14px 0 rgba(239, 68, 68, 0.39)'
      },
      animation: {
        'fade-in': 'fadeIn 0.5s ease-in-out',
        'fade-out': 'fadeOut 0.5s ease-in-out',
        'slide-in-right': 'slideInRight 0.3s ease-out',
        'slide-in-left': 'slideInLeft 0.3s ease-out',
        'slide-in-up': 'slideInUp 0.3s ease-out',
        'slide-in-down': 'slideInDown 0.3s ease-out',
        'bounce-subtle': 'bounceSubtle 0.6s ease-in-out',
        'pulse-soft': 'pulseSoft 2s cubic-bezier(0.4, 0, 0.6, 1) infinite',
        'shimmer': 'shimmer 1.5s linear infinite'
      },
      keyframes: {
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' }
        },
        fadeOut: {
          '0%': { opacity: '1' },
          '100%': { opacity: '0' }
        },
        slideInRight: {
          '0%': { transform: 'translateX(100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' }
        },
        slideInLeft: {
          '0%': { transform: 'translateX(-100%)', opacity: '0' },
          '100%': { transform: 'translateX(0)', opacity: '1' }
        },
        slideInUp: {
          '0%': { transform: 'translateY(100%)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' }
        },
        slideInDown: {
          '0%': { transform: 'translateY(-100%)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' }
        },
        bounceSubtle: {
          '0%, 100%': { transform: 'translateY(0)' },
          '50%': { transform: 'translateY(-4px)' }
        },
        pulseSoft: {
          '0%, 100%': { opacity: '1' },
          '50%': { opacity: '0.8' }
        },
        shimmer: {
          '0%': { transform: 'translateX(-100%)' },
          '100%': { transform: 'translateX(100%)' }
        }
      },
      backdropBlur: {
        xs: '2px'
      }
    }
  },
  plugins: [
    require('@tailwindcss/forms'),
    require('@tailwindcss/typography'),
    function({ addComponents, theme }) {
      addComponents({
        // Button Components
        '.btn': {
          '@apply px-4 py-2 rounded-lg font-medium transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2': {},
        },
        '.btn-primary': {
          '@apply bg-primary-500 text-white hover:bg-primary-600 focus:ring-primary-500 shadow-soft hover:shadow-colored-primary': {},
        },
        '.btn-secondary': {
          '@apply bg-secondary-500 text-white hover:bg-secondary-600 focus:ring-secondary-500 shadow-soft': {},
        },
        '.btn-success': {
          '@apply bg-success-500 text-white hover:bg-success-600 focus:ring-success-500 shadow-soft hover:shadow-colored-success': {},
        },
        '.btn-danger': {
          '@apply bg-danger-500 text-white hover:bg-danger-600 focus:ring-danger-500 shadow-soft hover:shadow-colored-danger': {},
        },
        '.btn-warning': {
          '@apply bg-warning-500 text-white hover:bg-warning-600 focus:ring-warning-500 shadow-soft': {},
        },
        '.btn-info': {
          '@apply bg-info-500 text-white hover:bg-info-600 focus:ring-info-500 shadow-soft': {},
        },
        '.btn-outline': {
          '@apply border border-current bg-transparent hover:bg-current hover:text-white': {},
        },
        '.btn-ghost': {
          '@apply bg-transparent hover:bg-gray-100 dark:hover:bg-gray-800': {},
        },
        '.btn-sm': {
          '@apply px-3 py-1.5 text-sm': {},
        },
        '.btn-lg': {
          '@apply px-6 py-3 text-lg': {},
        },
        '.btn-xl': {
          '@apply px-8 py-4 text-xl': {},
        },
        
        // Card Components
        '.card': {
          '@apply bg-white dark:bg-gray-800 rounded-xl shadow-soft border border-gray-200 dark:border-gray-700': {},
        },
        '.card-elevated': {
          '@apply card shadow-medium hover:shadow-hard transition-shadow duration-300': {},
        },
        '.card-header': {
          '@apply px-6 py-4 border-b border-gray-200 dark:border-gray-700': {},
        },
        '.card-body': {
          '@apply px-6 py-4': {},
        },
        '.card-footer': {
          '@apply px-6 py-4 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700/50 rounded-b-xl': {},
        },
        
        // Form Components
        '.form-input': {
          '@apply block w-full rounded-lg border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-800 text-gray-900 dark:text-gray-100 shadow-soft focus:border-primary-500 focus:ring-primary-500 transition-colors duration-200': {},
        },
        '.form-select': {
          '@apply form-input pr-10': {},
        },
        '.form-textarea': {
          '@apply form-input resize-none': {},
        },
        '.form-label': {
          '@apply block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2': {},
        },
        '.form-error': {
          '@apply text-sm text-danger-600 dark:text-danger-400 mt-1': {},
        },
        '.form-help': {
          '@apply text-sm text-gray-500 dark:text-gray-400 mt-1': {},
        },
        
        // Alert Components
        '.alert': {
          '@apply p-4 rounded-lg border': {},
        },
        '.alert-primary': {
          '@apply bg-primary-50 border-primary-200 text-primary-800 dark:bg-primary-900/20 dark:border-primary-800 dark:text-primary-200': {},
        },
        '.alert-success': {
          '@apply bg-success-50 border-success-200 text-success-800 dark:bg-success-900/20 dark:border-success-800 dark:text-success-200': {},
        },
        '.alert-danger': {
          '@apply bg-danger-50 border-danger-200 text-danger-800 dark:bg-danger-900/20 dark:border-danger-800 dark:text-danger-200': {},
        },
        '.alert-warning': {
          '@apply bg-warning-50 border-warning-200 text-warning-800 dark:bg-warning-900/20 dark:border-warning-800 dark:text-warning-200': {},
        },
        '.alert-info': {
          '@apply bg-info-50 border-info-200 text-info-800 dark:bg-info-900/20 dark:border-info-800 dark:text-info-200': {},
        },
        
        // Badge Components
        '.badge': {
          '@apply inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium': {},
        },
        '.badge-primary': {
          '@apply bg-primary-100 text-primary-800 dark:bg-primary-900 dark:text-primary-200': {},
        },
        '.badge-success': {
          '@apply bg-success-100 text-success-800 dark:bg-success-900 dark:text-success-200': {},
        },
        '.badge-danger': {
          '@apply bg-danger-100 text-danger-800 dark:bg-danger-900 dark:text-danger-200': {},
        },
        '.badge-warning': {
          '@apply bg-warning-100 text-warning-800 dark:bg-warning-900 dark:text-warning-200': {},
        },
        '.badge-info': {
          '@apply bg-info-100 text-info-800 dark:bg-info-900 dark:text-info-200': {},
        },
        '.badge-secondary': {
          '@apply bg-secondary-100 text-secondary-800 dark:bg-secondary-900 dark:text-secondary-200': {},
        },
        
        // Loading Components
        '.loading-skeleton': {
          '@apply animate-pulse bg-gradient-to-r from-gray-200 via-gray-300 to-gray-200 dark:from-gray-700 dark:via-gray-600 dark:to-gray-700 bg-[length:200%_100%] animate-shimmer': {},
        }
      })
    }
  ]
}