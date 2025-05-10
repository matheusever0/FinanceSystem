/**
 * Equilibrium Finance System - Table Manager
 * Módulo otimizado para gerenciamento de tabelas
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};
FinanceSystem.Modules = FinanceSystem.Modules || {};

// Módulo TableManager
FinanceSystem.Modules.TableManager = (function () {
    /**
     * Inicializa o módulo de tabelas
     */
    function initialize() {
        initializeDataTables();
        initializeTableFilters();
        initializeTableSort();
    }

    /**
     * Inicializa DataTables para tabelas comuns
     * @param {string} selector - Seletor CSS para as tabelas (opcional)
     * @param {object} options - Opções adicionais para DataTables
     */
    function initializeDataTables(selector = '.datatable', options = {}) {
        // Verifica se a biblioteca DataTables está disponível
        if (FinanceSystem.Utils.isLibraryAvailable('datatable')) {
            // Configuração padrão
            const defaultOptions = {
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]]
            };

            // Mescla opções padrão com opções fornecidas
            const mergedOptions = {...defaultOptions, ...options};

            // Inicializa DataTables
            $(selector).DataTable(mergedOptions);
        } else {
            // Implementação básica de ordenação se DataTables não estiver disponível
            initializeBasicTableSorting();
        }
    }

    /**
     * Inicializa tabela com botões de exportação
     * @param {string} selector - Seletor CSS para a tabela
     * @param {object} options - Opções adicionais
     */
    function initializeExportTable(selector, options = {}) {
        // Verifica se a biblioteca DataTables está disponível
        if (FinanceSystem.Utils.isLibraryAvailable('datatable')) {
            // Configuração padrão com botões
            const defaultOptions = {
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 20,
                lengthMenu: [[20, 40, 60, 80, 100, -1], [20, 40, 60, 80, 100, "Todos"]],
                dom: '<"top"Bfl>rt<"bottom"ip>',
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
                ]
            };

            // Mescla opções padrão com opções fornecidas
            const mergedOptions = {...defaultOptions, ...options};

            // Inicializa DataTables com botões
            $(selector).DataTable(mergedOptions);
        } else {
            // Fallback para tabela básica
            initializeBasicTableSorting();
        }
    }

    /**
     * Implementa ordenação básica para tabelas sem DataTables
     */
    function initializeBasicTableSorting() {
        const tables = document.querySelectorAll('table.sortable, table.datatable');

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
     * Filtra tabela por status
     * @param {string} status - Status para filtro
     * @param {string} selector - Seletor CSS para as tabelas 
     */
    function filterTableByStatus(status, selector = '.table-payments tbody tr, .table-incomes tbody tr') {
        const rows = document.querySelectorAll(selector);

        rows.forEach(row => {
            const statusCell = row.querySelector('.payment-status, .badge');
            if (!statusCell) return;

            const rowStatus = statusCell.textContent.trim().toLowerCase();

            if (status === 'all' || rowStatus.includes(status)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    }

    /**
     * Filtra tabela por texto
     * @param {string} text - Texto para filtro
     * @param {string} selector - Seletor CSS para as tabelas
     */
    function filterTableByText(text, selector = '.table-payments tbody tr, .table-incomes tbody tr') {
        const rows = document.querySelectorAll(selector);
        text = text.toLowerCase();

        rows.forEach(row => {
            const content = row.textContent.toLowerCase();

            if (content.includes(text)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
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

    // API pública do módulo
    return {
        initialize: initialize,
        initializeDataTables: initializeDataTables,
        initializeExportTable: initializeExportTable,
        sortTable: sortTable,
        filterTable: filterTable,
        filterTableByStatus: filterTableByStatus,
        filterTableByText: filterTableByText,
        addTableRow: addTableRow
    };
})();
