// Print and Export Functionality
const PrintExport = {
    // Initialize print handlers
    init: function() {
        // Add print button handlers
        document.querySelectorAll('[data-print]').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                const target = this.dataset.print;
                PrintExport.printElement(target);
            });
        });
        
        // Add export button handlers
        document.querySelectorAll('[data-export]').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                const format = this.dataset.export;
                const url = this.dataset.url;
                PrintExport.exportData(format, url);
            });
        });
    },
    
    // Print specific element or page
    printElement: function(elementId) {
        if (elementId === 'page') {
            this.printPage();
        } else {
            const element = document.getElementById(elementId);
            if (element) {
                this.printContent(element.innerHTML);
            }
        }
    },
    
    // Print entire page with optimizations
    printPage: function() {
        // Add print-specific class to body
        document.body.classList.add('printing');
        
        // Prepare charts for printing if they exist
        this.prepareChartsForPrint();
        
        // Add print header with metadata
        this.addPrintHeader();
        
        // Trigger print
        window.print();
        
        // Clean up after printing
        setTimeout(() => {
            document.body.classList.remove('printing');
            this.removePrintHeader();
        }, 1000);
    },
    
    // Print specific content in new window
    printContent: function(content, title = 'Print') {
        const printWindow = window.open('', '_blank', 'width=800,height=600');
        const printDocument = printWindow.document;
        
        printDocument.write(`
            <!DOCTYPE html>
            <html>
            <head>
                <title>${title}</title>
                <link rel="stylesheet" href="/css/site.css">
                <link rel="stylesheet" href="/css/print.css">
                <style>
                    body { padding: 20px; }
                    @media print {
                        body { padding: 0; }
                    }
                </style>
            </head>
            <body>
                <div class="print-header">
                    <h2>${document.title}</h2>
                    <p>Printed on: ${new Date().toLocaleString()}</p>
                </div>
                ${content}
                <div class="print-footer">
                    <p>Page <span class="page-number"></span></p>
                </div>
            </body>
            </html>
        `);
        
        printDocument.close();
        
        // Wait for content to load then print
        printWindow.onload = function() {
            setTimeout(() => {
                printWindow.print();
                printWindow.close();
            }, 250);
        };
    },
    
    // Export data in various formats
    exportData: function(format, url) {
        switch(format.toLowerCase()) {
            case 'pdf':
                this.exportPdf(url);
                break;
            case 'excel':
                this.exportExcel(url);
                break;
            case 'csv':
                this.exportCsv(url);
                break;
            case 'json':
                this.exportJson(url);
                break;
            default:
                console.error('Unsupported export format:', format);
        }
    },
    
    // Export to PDF
    exportPdf: function(url) {
        // Show loading indicator
        this.showLoading('Generating PDF...');
        
        fetch(url || window.location.href + '/export/pdf', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({
                url: window.location.href,
                title: document.title,
                filters: this.getActiveFilters()
            })
        })
        .then(response => response.blob())
        .then(blob => {
            // Create download link
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${this.generateFileName()}.pdf`;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
            this.hideLoading();
        })
        .catch(error => {
            console.error('PDF export failed:', error);
            this.hideLoading();
            this.showError('Failed to generate PDF. Please try again.');
        });
    },
    
    // Export to Excel
    exportExcel: function(url) {
        this.showLoading('Generating Excel file...');
        
        const tables = document.querySelectorAll('table');
        const data = [];
        
        tables.forEach(table => {
            const tableData = this.tableToArray(table);
            if (tableData.length > 0) {
                data.push(tableData);
            }
        });
        
        fetch(url || '/api/export/excel', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                data: data,
                title: document.title,
                metadata: {
                    generated: new Date().toISOString(),
                    user: this.getCurrentUser()
                }
            })
        })
        .then(response => response.blob())
        .then(blob => {
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = `${this.generateFileName()}.xlsx`;
            a.click();
            window.URL.revokeObjectURL(url);
            this.hideLoading();
        })
        .catch(error => {
            console.error('Excel export failed:', error);
            this.hideLoading();
            // Fallback to CSV
            this.exportCsv();
        });
    },
    
    // Export to CSV
    exportCsv: function(url) {
        const tables = document.querySelectorAll('table');
        if (tables.length === 0) {
            this.showError('No data to export');
            return;
        }
        
        let csv = '';
        tables.forEach((table, index) => {
            if (index > 0) csv += '\n\n';
            csv += this.tableToCSV(table);
        });
        
        const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = `${this.generateFileName()}.csv`;
        link.click();
    },
    
    // Export to JSON
    exportJson: function(url) {
        const tables = document.querySelectorAll('table');
        const data = {
            title: document.title,
            exported: new Date().toISOString(),
            url: window.location.href,
            tables: []
        };
        
        tables.forEach(table => {
            const tableData = this.tableToJson(table);
            if (tableData.length > 0) {
                data.tables.push(tableData);
            }
        });
        
        const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = `${this.generateFileName()}.json`;
        link.click();
    },
    
    // Helper: Convert table to array
    tableToArray: function(table) {
        const data = [];
        const rows = table.querySelectorAll('tr');
        
        rows.forEach(row => {
            const rowData = [];
            const cells = row.querySelectorAll('th, td');
            cells.forEach(cell => {
                rowData.push(cell.textContent.trim());
            });
            if (rowData.length > 0) {
                data.push(rowData);
            }
        });
        
        return data;
    },
    
    // Helper: Convert table to CSV
    tableToCSV: function(table) {
        const rows = [];
        const tableRows = table.querySelectorAll('tr');
        
        tableRows.forEach(row => {
            const cols = row.querySelectorAll('td, th');
            const rowData = [];
            
            cols.forEach(col => {
                let data = col.textContent.trim();
                // Escape quotes and wrap in quotes if contains comma
                data = data.replace(/"/g, '""');
                if (data.includes(',') || data.includes('"') || data.includes('\n')) {
                    data = `"${data}"`;
                }
                rowData.push(data);
            });
            
            rows.push(rowData.join(','));
        });
        
        return rows.join('\n');
    },
    
    // Helper: Convert table to JSON
    tableToJson: function(table) {
        const headers = [];
        const data = [];
        
        // Get headers
        const headerRow = table.querySelector('thead tr');
        if (headerRow) {
            headerRow.querySelectorAll('th').forEach(th => {
                headers.push(th.textContent.trim());
            });
        }
        
        // Get data
        const bodyRows = table.querySelectorAll('tbody tr');
        bodyRows.forEach(row => {
            const rowData = {};
            const cells = row.querySelectorAll('td');
            
            cells.forEach((cell, index) => {
                const key = headers[index] || `column_${index}`;
                rowData[key] = cell.textContent.trim();
            });
            
            data.push(rowData);
        });
        
        return data;
    },
    
    // Prepare charts for printing (convert to static images)
    prepareChartsForPrint: function() {
        const charts = document.querySelectorAll('canvas');
        charts.forEach(canvas => {
            const img = document.createElement('img');
            img.src = canvas.toDataURL('image/png');
            img.className = 'print-only chart-image';
            img.style.maxWidth = '100%';
            canvas.parentNode.insertBefore(img, canvas);
            canvas.classList.add('no-print');
        });
    },
    
    // Add print header with metadata
    addPrintHeader: function() {
        const header = document.createElement('div');
        header.className = 'print-only print-header';
        header.innerHTML = `
            <h2>${document.title}</h2>
            <p>Generated: ${new Date().toLocaleString()}</p>
            <p>URL: ${window.location.href}</p>
        `;
        document.body.insertBefore(header, document.body.firstChild);
    },
    
    // Remove print header
    removePrintHeader: function() {
        const headers = document.querySelectorAll('.print-header.print-only');
        headers.forEach(header => header.remove());
        
        // Also remove temporary chart images
        document.querySelectorAll('.chart-image.print-only').forEach(img => img.remove());
        document.querySelectorAll('canvas.no-print').forEach(canvas => {
            canvas.classList.remove('no-print');
        });
    },
    
    // Get active filters for export
    getActiveFilters: function() {
        const filters = {};
        
        // Get all select filters
        document.querySelectorAll('select[name*="filter"]').forEach(select => {
            if (select.value) {
                filters[select.name] = select.value;
            }
        });
        
        // Get all input filters
        document.querySelectorAll('input[name*="filter"], input[name*="search"]').forEach(input => {
            if (input.value) {
                filters[input.name] = input.value;
            }
        });
        
        return filters;
    },
    
    // Generate filename based on page context
    generateFileName: function() {
        const title = document.title.replace(/[^a-z0-9]/gi, '_').toLowerCase();
        const date = new Date().toISOString().split('T')[0];
        return `${title}_${date}`;
    },
    
    // Get current user (if available)
    getCurrentUser: function() {
        const userElement = document.querySelector('[data-user-name]');
        return userElement ? userElement.dataset.userName : 'Unknown';
    },
    
    // Show loading indicator
    showLoading: function(message = 'Loading...') {
        const loader = document.createElement('div');
        loader.id = 'export-loader';
        loader.className = 'export-loader';
        loader.innerHTML = `
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">${message}</span>
            </div>
            <p>${message}</p>
        `;
        document.body.appendChild(loader);
    },
    
    // Hide loading indicator
    hideLoading: function() {
        const loader = document.getElementById('export-loader');
        if (loader) {
            loader.remove();
        }
    },
    
    // Show error message
    showError: function(message) {
        const toast = document.createElement('div');
        toast.className = 'toast show position-fixed top-0 end-0 m-3';
        toast.innerHTML = `
            <div class="toast-header bg-danger text-white">
                <strong class="me-auto">Export Error</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;
        document.body.appendChild(toast);
        
        setTimeout(() => toast.remove(), 5000);
    }
};

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function() {
    PrintExport.init();
});

// Add CSS for loader
const style = document.createElement('style');
style.textContent = `
    .export-loader {
        position: fixed;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        background: white;
        padding: 30px;
        border-radius: 10px;
        box-shadow: 0 0 20px rgba(0,0,0,0.3);
        z-index: 9999;
        text-align: center;
    }
    
    .printing .export-loader {
        display: none;
    }
`;
document.head.appendChild(style);