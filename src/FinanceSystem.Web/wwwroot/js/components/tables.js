/**
 * Scripts para tabelas do Finance System
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeDataTables();
    initializeTableFilters();
    initializeTableExport();
    initializeTableActions();
});

// Inicializa DataTables
function initializeDataTables() {
    // Verifica se a biblioteca DataTables está disponível
    if (typeof $.fn.DataTable !== 'undefined') {
        $('.datatable').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
            },
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]]
        });

        // DataTables com configurações personalizadas
        $('.datatable-payments').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
            },
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]],
            order: [[1, 'desc']], // Ordena por data de vencimento decrescente
            columnDefs: [
                { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
            ]
        });
    } else {
        // Implementação básica de ordenação se DataTables não estiver disponível
        implementBasicTableSorting();
    }
}

// Implementa ordenação básica para tabelas
function implementBasicTableSorting() {
    const tables = document.querySelectorAll('table.sortable');

    tables.forEach(table => {
        const headers = table.querySelectorAll('th');

        headers.forEach((header, index) => {
            if (!header.classList.contains('no-sort')) {
                header.addEventListener('click', function () {
                    sortTable(table, index);
                });
                header.style.cursor = 'pointer';

                // Adiciona ícone de ordenação
                const span = document.createElement('span');
                span.className = 'sort-icon';
                span.innerHTML = ' ⇵';
                header.appendChild(span);
            }
        });
    });
}

function sortTable(table, columnIndex) {
    const tbody = table.querySelector('tbody');
    const rows = Array.from(tbody.querySelectorAll('tr'));
    const header = table.querySelectorAll('th')[columnIndex];
    const isAsc = header.classList.contains('sort-asc');

    // Remove classes de ordenação de todos os cabeçalhos
    table.querySelectorAll('th').forEach(th => {
        th.classList.remove('sort-asc', 'sort-desc');
    });

    // Adiciona classe de ordenação ao cabeçalho clicado
    header.classList.add(isAsc ? 'sort-desc' : 'sort-asc');

    // Ordena as linhas
    rows.sort((a, b) => {
        const aValue = a.querySelectorAll('td')[columnIndex].textContent.trim();
        const bValue = b.querySelectorAll('td')[columnIndex].textContent.trim();

        // Verifica se os valores são datas
        if (isDate(aValue) && isDate(bValue)) {
            const aDate = parseDate(aValue);
            const bDate = parseDate(bValue);
            return isAsc ? aDate - bDate : bDate - aDate;
        }

        // Verifica se os valores são números
        if (isNumber(aValue) && isNumber(bValue)) {
            const aNum = parseFloat(aValue.replace(/[^\d.-]/g, ''));
            const bNum = parseFloat(bValue.replace(/[^\d.-]/g, ''));
            return isAsc ? aNum - bNum : bNum - aNum;
        }

        // Ordenação de texto
        return isAsc ?
            aValue.localeCompare(bValue, 'pt-BR') :
            bValue.localeCompare(aValue, 'pt-BR');
    });

    // Reordena as linhas na tabela
    rows.forEach(row => {
        tbody.appendChild(row);
    });
}

function isDate(value) {
    // Verifica se o valor é uma data no formato dd/mm/yyyy
    return /^\d{1,2}\/\d{1,2}\/\d{4}$/.test(value);
}

function parseDate(dateStr) {
    // Converte data no formato dd/mm/yyyy para objeto Date
    const parts = dateStr.split('/');
    return new Date(parts[2], parts[1] - 1, parts[0]);
}

function isNumber(value) {
    // Verifica se o valor é um número (com ou sem formatação)
    return /^[R$\s]*[\d.,]+%?$/.test(value);
}

// Inicializa filtros de tabela
function initializeTableFilters() {
    const filterInputs = document.querySelectorAll('.table-filter');

    filterInputs.forEach(input => {
        input.addEventListener('keyup', function () {
            const tableId = this.getAttribute('data-table');
            const table = document.getElementById(tableId);

            if (!table) return;

            const filterValue = this.value.toLowerCase();
            const rows = table.querySelectorAll('tbody tr');

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                row.style.display = text.includes(filterValue) ? '' : 'none';
            });
        });
    });

    // Filtros por coluna
    const columnFilters = document.querySelectorAll('.column-filter');

    columnFilters.forEach(filter => {
        filter.addEventListener('change', function () {
            const tableId = this.getAttribute('data-table');
            const columnIndex = this.getAttribute('data-column');
            const table = document.getElementById(tableId);

            if (!table || !columnIndex) return;

            const filterValue = this.value.toLowerCase();
            const rows = table.querySelectorAll('tbody tr');

            rows.forEach(row => {
                const cell = row.querySelectorAll('td')[columnIndex];
                if (!cell) return;

                const text = cell.textContent.toLowerCase();

                if (filterValue === '' || text.includes(filterValue)) {
                    row.classList.remove('filtered-out');
                } else {
                    row.classList.add('filtered-out');
                }
            });
        });
    });
}

// Inicializa exportação de tabela
function initializeTableExport() {
    const exportButtons = document.querySelectorAll('.export-table');

    exportButtons.forEach(button => {
        button.addEventListener('click', function () {
            const tableId = this.getAttribute('data-table');
            const format = this.getAttribute('data-format') || 'csv';
            const table = document.getElementById(tableId);

            if (!table) return;

            if (format === 'csv') {
                exportTableToCSV(table, `${tableId}_export.csv`);
            } else if (format === 'excel') {
                exportTableToExcel(table, `${tableId}_export.xlsx`);
            } else if (format === 'pdf') {
                exportTableToPDF(table, `${tableId}_export.pdf`);
            }
        });
    });
}

function exportTableToCSV(table, filename) {
    const rows = table.querySelectorAll('tr');
    let csv = [];

    for (let i = 0; i < rows.length; i++) {
        const row = [], cols = rows[i].querySelectorAll('td, th');

        for (let j = 0; j < cols.length; j++) {
            // Remove HTML e limpa texto
            let text = cols[j].innerText.replace(/(\r\n|\n|\r)/gm, '').trim();

            // Escapa aspas duplas
            text = text.replace(/"/g, '""');

            // Adiciona aspas para lidar com vírgulas
            row.push(`"${text}"`);
        }

        csv.push(row.join(','));
    }

    downloadCSV(csv.join('\n'), filename);
}

function downloadCSV(csv, filename) {
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);

    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function exportTableToExcel(table, filename) {
    // Esta função requer a biblioteca SheetJS ou uma implementação personalizada
    console.log('Exportação para Excel requer biblioteca adicional');

    if (typeof $(table).tableExport === 'function') {
        $(table).tableExport({
            type: 'xlsx',
            fileName: filename,
            exportHiddenCells: false
        });
    }
}

function exportTableToPDF(table, filename) {
    // Esta função requer a biblioteca jsPDF ou uma implementação personalizada
    console.log('Exportação para PDF requer biblioteca adicional');

    // Exemplo básico usando jsPDF e jsPDF-AutoTable
    if (typeof jsPDF !== 'undefined' && typeof jsPDF.autoTable !== 'undefined') {
        const doc = new jsPDF();

        // Adiciona título
        doc.text(filename.replace('.pdf', ''), 14, 16);

        // Exporta a tabela
        doc.autoTable({ html: table });

        // Salva o PDF
        doc.save(filename);
    }
}

// Inicializa ações de tabela
function initializeTableActions() {
    // Botões de ação em linhas de tabela
    const actionButtons = document.querySelectorAll('.table-action-btn');

    actionButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const action = this.getAttribute('data-action');
            const rowId = this.closest('tr').getAttribute('data-id');

            if (action === 'delete') {
                // Confirmação de exclusão
                e.preventDefault();
                if (confirm('Tem certeza que deseja excluir este item?')) {
                    // Continua com a exclusão
                    this.closest('form').submit();
                }
            } else if (action === 'toggle') {
                // Toggle de status
                e.preventDefault();
                toggleStatus(rowId, this);
            }
        });
    });

    // Ações em massa
    const bulkActionButton = document.getElementById('bulk-action-btn');
    if (bulkActionButton) {
        bulkActionButton.addEventListener('click', function () {
            const action = document.getElementById('bulk-action').value;
            const selectedRows = document.querySelectorAll('input.row-checkbox:checked');

            if (selectedRows.length === 0) {
                alert('Selecione pelo menos um item');
                return;
            }

            if (action === '') {
                alert('Selecione uma ação');
                return;
            }

            if (action === 'delete' && !confirm(`Tem certeza que deseja excluir ${selectedRows.length} item(s)?`)) {
                return;
            }

            // Coleta IDs dos itens selecionados
            const ids = Array.from(selectedRows).map(checkbox => checkbox.value);

            // Executa a ação
            executeBulkAction(action, ids);
        });
    }

    // Checkbox para selecionar/deselecionar todos
    const selectAllCheckbox = document.getElementById('select-all');
    if (selectAllCheckbox) {
        selectAllCheckbox.addEventListener('change', function () {
            const checkboxes = document.querySelectorAll('input.row-checkbox');
            checkboxes.forEach(checkbox => {
                checkbox.checked = this.checked;
            });
        });
    }
}

function toggleStatus(id, button) {
    const url = button.getAttribute('data-url');
    const currentStatus = button.getAttribute('data-status');
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ id: id })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Atualiza o botão
                if (currentStatus === 'active') {
                    button.setAttribute('data-status', 'inactive');
                    button.innerHTML = '<i class="fas fa-toggle-off"></i>';
                    button.classList.remove('btn-success');
                    button.classList.add('btn-secondary');
                } else {
                    button.setAttribute('data-status', 'active');
                    button.innerHTML = '<i class="fas fa-toggle-on"></i>';
                    button.classList.remove('btn-secondary');
                    button.classList.add('btn-success');
                }

                // Atualiza a linha da tabela se necessário
                const statusCell = button.closest('tr').querySelector('.status-cell');
                if (statusCell) {
                    if (currentStatus === 'active') {
                        statusCell.innerHTML = '<span class="badge bg-secondary">Inativo</span>';
                    } else {
                        statusCell.innerHTML = '<span class="badge bg-success">Ativo</span>';
                    }
                }
            } else {
                alert('Erro ao alterar o status: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Erro:', error);
            alert('Ocorreu um erro ao processar a solicitação');
        });
}

function executeBulkAction(action, ids) {
    const url = document.getElementById('bulk-action-form').getAttribute('data-url');
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ action: action, ids: ids })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Recarrega a página ou atualiza a tabela
                window.location.reload();
            } else {
                alert('Erro ao executar a ação: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Erro:', error);
            alert('Ocorreu um erro ao processar a solicitação');
        });
}