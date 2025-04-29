/**
 * Finance System - Tables Module
 * Funções reutilizáveis para manipulação de tabelas
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

// Módulo Tables
FinanceSystem.Modules.Tables = (function () {
    /**
     * Inicializa o módulo de tabelas
     */
    function initialize() {
        initializeDataTables();
        initializeTableFilters();
        initializeTableExport();
        initializeTableActions();
        initializeTableSort();
    }

    /**
     * Inicializa DataTables
     */
    function initializeDataTables() {
        // Verifica se a biblioteca DataTables está disponível
        if (typeof $.fn.DataTable !== 'undefined') {
            // Configuração padrão para todas as tabelas de dados
            $('.datatable').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]]
            });

            // DataTables com configurações personalizadas
            initializeCustomDataTables();
        } else {
            // Implementação básica de ordenação se DataTables não estiver disponível
            implementBasicTableSorting();
        }
    }

    /**
     * Inicializa DataTables com configurações específicas
     */
    function initializeCustomDataTables() {
        // Tabela de pagamentos
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
            ],
            buttons: [
                {
                    extend: 'excel',
                    text: '<i class="fas fa-file-excel me-1"></i> Excel',
                    className: 'btn btn-sm btn-success'
                },
                {
                    extend: 'pdf',
                    text: '<i class="fas fa-file-pdf me-1"></i> PDF',
                    className: 'btn btn-sm btn-danger'
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print me-1"></i> Imprimir',
                    className: 'btn btn-sm btn-primary'
                }
            ],
            dom: '<"dtTop"<"dtFilters"f><"dtInfo"l>><"dtScroll"t><"dtBottom"<"dtPagination"p>><"dtButtons"B>',
            drawCallback: function () {
                stylePaymentRows();
            }
        });

        // Tabela de usuários
        $('.datatable-users').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
            },
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]],
            order: [[1, 'desc']], // Ordena por data de vencimento decrescente
            columnDefs: [
                { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
            ],
            buttons: [
                {
                    extend: 'excel',
                    text: '<i class="fas fa-file-excel me-1"></i> Excel',
                    className: 'btn btn-sm btn-success'
                },
                {
                    extend: 'pdf',
                    text: '<i class="fas fa-file-pdf me-1"></i> PDF',
                    className: 'btn btn-sm btn-danger'
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print me-1"></i> Imprimir',
                    className: 'btn btn-sm btn-primary'
                }
            ],
            dom: '<"dtTop"<"dtFilters"f><"dtInfo"l>><"dtScroll"t><"dtBottom"<"dtPagination"p>><"dtButtons"B>',
            drawCallback: function () {
                stylePaymentRows();
            }
        });

        // Tabela de cartões de crédito
        $('.datatable-creditcards').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
            },
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]],
            order: [[1, 'desc']], // Ordena por data de vencimento decrescente
            columnDefs: [
                { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
            ],
            buttons: [
                {
                    extend: 'excel',
                    text: '<i class="fas fa-file-excel me-1"></i> Excel',
                    className: 'btn btn-sm btn-success'
                },
                {
                    extend: 'pdf',
                    text: '<i class="fas fa-file-pdf me-1"></i> PDF',
                    className: 'btn btn-sm btn-danger'
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print me-1"></i> Imprimir',
                    className: 'btn btn-sm btn-primary'
                }
            ],
            dom: '<"dtTop"<"dtFilters"f><"dtInfo"l>><"dtScroll"t><"dtBottom"<"dtPagination"p>><"dtButtons"B>',
            drawCallback: function () {
                stylePaymentRows();
            }
        });

        // Tabela de investimentos
        $('.datatable-investments').DataTable({
            language: {
                url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
            },
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]],
            order: [[1, 'desc']], // Ordena por data de vencimento decrescente
            columnDefs: [
                { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
            ],
            buttons: [
                {
                    extend: 'excel',
                    text: '<i class="fas fa-file-excel me-1"></i> Excel',
                    className: 'btn btn-sm btn-success'
                },
                {
                    extend: 'pdf',
                    text: '<i class="fas fa-file-pdf me-1"></i> PDF',
                    className: 'btn btn-sm btn-danger'
                },
                {
                    extend: 'print',
                    text: '<i class="fas fa-print me-1"></i> Imprimir',
                    className: 'btn btn-sm btn-primary'
                }
            ],
            dom: '<"dtTop"<"dtFilters"f><"dtInfo"l>><"dtScroll"t><"dtBottom"<"dtPagination"p>><"dtButtons"B>',
            drawCallback: function () {
                stylePaymentRows();
            }
        });
    }

    /**
     * Implementa ordenação básica para tabelas sem DataTables
     */
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

    /**
     * Ordena uma tabela com base na coluna selecionada
     * @param {HTMLTableElement} table - Tabela a ser ordenada
     * @param {number} columnIndex - Índice da coluna para ordenação
     */
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

    /**
     * Verifica se um valor é uma data
     * @param {string} value - Valor a ser verificado
     * @returns {boolean} - Resultado da verificação
     */
    function isDate(value) {
        // Verifica se o valor é uma data no formato dd/mm/yyyy
        return /^\d{1,2}\/\d{1,2}\/\d{4}$/.test(value);
    }

    /**
     * Converte uma string de data para objeto Date
     * @param {string} dateStr - String de data
     * @returns {Date} - Objeto Date
     */
    function parseDate(dateStr) {
        // Converte data no formato dd/mm/yyyy para objeto Date
        const parts = dateStr.split('/');
        return new Date(parts[2], parts[1] - 1, parts[0]);
    }

    /**
     * Verifica se um valor é um número
     * @param {string} value - Valor a ser verificado
     * @returns {boolean} - Resultado da verificação
     */
    function isNumber(value) {
        // Verifica se o valor é um número (com ou sem formatação)
        return /^[R$\s]*[\d.,]+%?$/.test(value);
    }

    /**
     * Inicializa filtros de tabela
     */
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
        initializeColumnFilters();
    }

    /**
     * Inicializa filtros por coluna
     */
    function initializeColumnFilters() {
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

    /**
     * Inicializa exportação de tabela
     */
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

    /**
     * Exporta tabela para CSV
     * @param {HTMLTableElement} table - Tabela a ser exportada
     * @param {string} filename - Nome do arquivo
     */
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

    /**
     * Realiza o download do arquivo CSV
     * @param {string} csv - Conteúdo CSV
     * @param {string} filename - Nome do arquivo
     */
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

    /**
     * Exporta tabela para Excel
     * @param {HTMLTableElement} table - Tabela a ser exportada
     * @param {string} filename - Nome do arquivo
     */
    function exportTableToExcel(table, filename) {
        // Esta função requer a biblioteca SheetJS ou uma implementação personalizada
        if (typeof $(table).tableExport === 'function') {
            $(table).tableExport({
                type: 'xlsx',
                fileName: filename,
                exportHiddenCells: false
            });
        } else {
            alert('Exportação para Excel requer a biblioteca tableExport.js');
            console.log('Exportação para Excel requer biblioteca adicional');
        }
    }

    /**
     * Exporta tabela para PDF
     * @param {HTMLTableElement} table - Tabela a ser exportada
     * @param {string} filename - Nome do arquivo
     */
    function exportTableToPDF(table, filename) {
        // Esta função requer a biblioteca jsPDF ou uma implementação personalizada
        if (typeof jsPDF !== 'undefined' && typeof jsPDF.autoTable !== 'undefined') {
            const doc = new jsPDF();

            // Adiciona título
            doc.text(filename.replace('.pdf', ''), 14, 16);

            // Exporta a tabela
            doc.autoTable({ html: table });

            // Salva o PDF
            doc.save(filename);
        } else {
            alert('Exportação para PDF requer as bibliotecas jsPDF e jsPDF-AutoTable');
            console.log('Exportação para PDF requer biblioteca adicional');
        }
    }

    /**
     * Inicializa ações de tabela
     */
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

        // Checkbox para selecionar/deselecionar todos
        initializeSelectAll();

        // Ações em massa
        initializeBulkActions();
    }

    /**
     * Inicializa checkbox para selecionar/deselecionar todos
     */
    function initializeSelectAll() {
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

    /**
     * Inicializa ações em massa
     */
    function initializeBulkActions() {
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
    }

    /**
     * Alterna o status de um item
     * @param {string} id - ID do item
     * @param {HTMLElement} button - Botão de toggle
     */
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

    /**
     * Executa uma ação em massa
     * @param {string} action - Ação a ser executada
     * @param {Array} ids - IDs dos itens selecionados
     */
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

    /**
     * Inicializa ordenação de tabelas
     */
    function initializeTableSort() {
        document.querySelectorAll('table.table-sort').forEach(table => {
            const headers = table.querySelectorAll('th[data-sort]');
            headers.forEach(header => {
                header.style.cursor = 'pointer';

                // Adiciona ícone de ordenação
                const sortIcon = document.createElement('span');
                sortIcon.className = 'ms-1 sort-icon';
                sortIcon.innerHTML = '⇵';
                header.appendChild(sortIcon);

                // Adiciona evento de clique
                header.addEventListener('click', function () {
                    const column = this.getAttribute('data-sort');
                    const direction = this.getAttribute('data-direction') === 'asc' ? 'desc' : 'asc';

                    // Remove direção de todos os cabeçalhos
                    headers.forEach(h => h.removeAttribute('data-direction'));

                    // Define direção neste cabeçalho
                    this.setAttribute('data-direction', direction);

                    // Atualiza ícone
                    table.querySelectorAll('.sort-icon').forEach(icon => {
                        icon.innerHTML = '⇵';
                    });
                    sortIcon.innerHTML = direction === 'asc' ? '↑' : '↓';

                    // Ordena a tabela
                    sortTableByColumn(table, column, direction);
                });
            });
        });
    }

    /**
     * Ordena tabela por coluna específica
     * @param {HTMLTableElement} table - Tabela a ser ordenada
     * @param {string} column - Nome da coluna para ordenação
     * @param {string} direction - Direção da ordenação (asc, desc)
     */
    function sortTableByColumn(table, column, direction) {
        const tbody = table.querySelector('tbody');
        const rows = Array.from(tbody.querySelectorAll('tr'));

        rows.sort((a, b) => {
            const aValue = a.querySelector(`td[data-column="${column}"]`).textContent.trim();
            const bValue = b.querySelector(`td[data-column="${column}"]`).textContent.trim();

            if (isDate(aValue) && isDate(bValue)) {
                const aDate = parseDate(aValue);
                const bDate = parseDate(bValue);
                return direction === 'asc' ? aDate - bDate : bDate - aDate;
            }

            if (isNumber(aValue) && isNumber(bValue)) {
                const aNum = parseFloat(aValue.replace(/[^\d.-]/g, ''));
                const bNum = parseFloat(bValue.replace(/[^\d.-]/g, ''));
                return direction === 'asc' ? aNum - bNum : bNum - aNum;
            }

            return direction === 'asc'
                ? aValue.localeCompare(bValue, 'pt-BR')
                : bValue.localeCompare(aValue, 'pt-BR');
        });

        // Limpa e reconstrói a tabela
        rows.forEach(row => tbody.appendChild(row));
    }

    /**
     * Filtra tabela com base em valores de filtro
     * @param {string} tableId - ID da tabela
     * @param {string} filterValue - Valor do filtro
     * @param {number|string} columnIndex - Índice ou nome da coluna (opcional)
     */
    function filterTable(tableId, filterValue, columnIndex) {
        const table = document.getElementById(tableId);
        if (!table) return;

        const rows = table.querySelectorAll('tbody tr');
        filterValue = filterValue.toLowerCase();

        rows.forEach(row => {
            let content;

            if (columnIndex !== undefined) {
                // Filtrar apenas uma coluna específica
                const cell = typeof columnIndex === 'number'
                    ? row.querySelectorAll('td')[columnIndex]
                    : row.querySelector(`td[data-column="${columnIndex}"]`);

                if (!cell) return;
                content = cell.textContent.toLowerCase();
            } else {
                // Filtrar toda a linha
                content = row.textContent.toLowerCase();
            }

            if (content.includes(filterValue)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    }

    /**
     * Inicializa pesquisa em tabela
     * @param {string} inputSelector - Seletor do campo de pesquisa
     * @param {string} tableId - ID da tabela
     * @param {number|string} columnIndex - Índice ou nome da coluna (opcional)
     */
    function initializeTableSearch(inputSelector, tableId, columnIndex) {
        const searchInput = document.querySelector(inputSelector);
        if (!searchInput) return;

        searchInput.addEventListener('input', function () {
            filterTable(tableId, this.value, columnIndex);
        });
    }

    /**
     * Adiciona linha à tabela
     * @param {string} tableId - ID da tabela
     * @param {Array} cells - Array de células da linha
     * @param {Object} options - Opções adicionais (classes, atributos, etc.)
     * @returns {HTMLTableRowElement} - Elemento da linha adicionada
     */
    function addTableRow(tableId, cells, options = {}) {
        const table = document.getElementById(tableId);
        if (!table) return null;

        const tbody = table.querySelector('tbody') || table.createTBody();
        const row = tbody.insertRow();

        // Adiciona classes à linha
        if (options.classes) {
            if (Array.isArray(options.classes)) {
                row.classList.add(...options.classes);
            } else {
                row.classList.add(options.classes);
            }
        }

        // Adiciona atributos à linha
        if (options.attributes) {
            Object.keys(options.attributes).forEach(attr => {
                row.setAttribute(attr, options.attributes[attr]);
            });
        }

        // Adiciona células
        cells.forEach((cell, index) => {
            const td = row.insertCell();

            // Se a célula for um objeto com configurações
            if (typeof cell === 'object' && cell !== null && !Array.isArray(cell) && !(cell instanceof Node)) {
                if (cell.html) {
                    td.innerHTML = cell.html;
                } else if (cell.text) {
                    td.textContent = cell.text;
                }

                // Adiciona classes à célula
                if (cell.classes) {
                    if (Array.isArray(cell.classes)) {
                        td.classList.add(...cell.classes);
                    } else {
                        td.classList.add(cell.classes);
                    }
                }

                // Adiciona atributos à célula
                if (cell.attributes) {
                    Object.keys(cell.attributes).forEach(attr => {
                        td.setAttribute(attr, cell.attributes[attr]);
                    });
                }
            } else if (cell instanceof Node) {
                td.appendChild(cell);
            } else {
                td.innerHTML = cell;
            }
        });

        return row;
    }

    /**
     * Remove uma linha da tabela
     * @param {HTMLElement} row - Elemento da linha ou botão dentro da linha
     * @param {boolean} withConfirmation - Indica se deve pedir confirmação
     * @param {string} message - Mensagem de confirmação
     * @returns {boolean} - Indica se a linha foi removida
     */
    function removeTableRow(row, withConfirmation = true, message = 'Tem certeza que deseja remover este item?') {
        // Se for um botão ou elemento dentro da linha, busca a linha
        if (!row.tagName || row.tagName.toLowerCase() !== 'tr') {
            row = row.closest('tr');
        }

        if (!row) return false;

        if (withConfirmation && !confirm(message)) {
            return false;
        }

        row.parentNode.removeChild(row);
        return true;
    }

    /**
     * Limpa todas as linhas de uma tabela
     * @param {string} tableId - ID da tabela
     * @param {boolean} keepHeader - Indica se deve manter o cabeçalho
     */
    function clearTable(tableId, keepHeader = true) {
        const table = document.getElementById(tableId);
        if (!table) return;

        const tbody = table.querySelector('tbody');
        if (tbody) {
            tbody.innerHTML = '';
        }

        if (!keepHeader) {
            const thead = table.querySelector('thead');
            if (thead) {
                thead.innerHTML = '';
            }
        }
    }

    // API pública do módulo
    return {
        initialize: initialize,
        sortTable: sortTable,
        filterTable: filterTable,
        exportTableToCSV: exportTableToCSV,
        exportTableToExcel: exportTableToExcel,
        exportTableToPDF: exportTableToPDF,
        addTableRow: addTableRow,
        removeTableRow: removeTableRow,
        clearTable: clearTable,
        initializeTableSearch: initializeTableSearch
    };
})();