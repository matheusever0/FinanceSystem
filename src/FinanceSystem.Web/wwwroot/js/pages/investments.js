/**
 * Scripts for the investments index page
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeInvestmentsTable();
});

/**
 * Initialize the investments table with DataTables if available
 */
function initializeInvestmentsTable() {
    const table = document.getElementById('investments-table');
    if (!table) return;

    // Initialize DataTables if available
    if (typeof $.fn.DataTable !== 'undefined') {
        $('#investments-table').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
            },
            responsive: true,
            pageLength: 10,
            order: [[8, 'desc']], // Order by gain/loss column
            columnDefs: [
                { orderable: false, targets: -1 } // Disable sorting on actions column
            ]
        });
    } else {
        // If DataTables is not available, add basic sorting
        const tableHeader = table.querySelector('thead');
        if (tableHeader) {
            const headers = tableHeader.querySelectorAll('th');
            headers.forEach((header, index) => {
                if (index !== headers.length - 1) { // Skip the last column (actions)
                    header.style.cursor = 'pointer';
                    header.addEventListener('click', () => {
                        sortTable(table, index);
                    });
                }
            });
        }
    }
}

/**
 * Basic table sorting function
 */
function sortTable(table, columnIndex) {
    const rows = Array.from(table.querySelectorAll('tbody tr'));
    const header = table.querySelectorAll('th')[columnIndex];
    const isAscending = header.classList.contains('asc');

    // Clear all sorting classes
    table.querySelectorAll('th').forEach(th => {
        th.classList.remove('asc', 'desc');
    });

    // Set the new sorting class
    header.classList.add(isAscending ? 'desc' : 'asc');

    // Sort rows
    rows.sort((a, b) => {
        let aValue = a.cells[columnIndex].textContent.trim();
        let bValue = b.cells[columnIndex].textContent.trim();

        // Check if values contain currency
        if (aValue.includes('R$')) {
            aValue = parseFloat(aValue.replace('R$', '').replace('.', '').replace(',', '.'));
            bValue = parseFloat(bValue.replace('R$', '').replace('.', '').replace(',', '.'));
        }

        // Check if values contain percentages
        else if (aValue.includes('%')) {
            aValue = parseFloat(aValue.replace('%', '').replace(',', '.'));
            bValue = parseFloat(bValue.replace('%', '').replace(',', '.'));
        }

        // Check if values are numbers
        else if (!isNaN(parseFloat(aValue)) && !isNaN(parseFloat(bValue))) {
            aValue = parseFloat(aValue.replace(',', '.'));
            bValue = parseFloat(bValue.replace(',', '.'));
        }

        // Perform sorting
        if (aValue < bValue) return isAscending ? -1 : 1;
        if (aValue > bValue) return isAscending ? 1 : -1;
        return 0;
    });

    // Reorder rows in the table
    const tbody = table.querySelector('tbody');
    rows.forEach(row => tbody.appendChild(row));
}