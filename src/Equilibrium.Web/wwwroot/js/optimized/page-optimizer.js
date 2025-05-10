/**
 * Finance System - Page Module Optimizer
 * Script para otimizar inicialização de módulos de página
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};

// Função auxiliar para inicializar componentes comuns em todas as páginas
FinanceSystem.initializeCommonComponents = function() {
    // Inicializa componentes de interface se disponíveis
    if (FinanceSystem.UI && typeof FinanceSystem.UI.initialize === 'function') {
        FinanceSystem.UI.initialize();
    }

    // Inicializa o módulo Utils
    if (FinanceSystem.Utils && typeof FinanceSystem.Utils.initialize === 'function') {
        FinanceSystem.Utils.initialize();
    }
    
    // Inicializa o módulo de formulários aprimorado
    if (FinanceSystem.Modules && FinanceSystem.Modules.FormsEnhanced && 
        typeof FinanceSystem.Modules.FormsEnhanced.initialize === 'function') {
        FinanceSystem.Modules.FormsEnhanced.initialize();
    }
    
    // Inicializa o gerenciador de tabelas
    if (FinanceSystem.Modules && FinanceSystem.Modules.TableManager && 
        typeof FinanceSystem.Modules.TableManager.initialize === 'function') {
        FinanceSystem.Modules.TableManager.initialize();
    }
    
    // Inicializa validação
    if (FinanceSystem.Validation && typeof FinanceSystem.Validation.initialize === 'function') {
        FinanceSystem.Validation.initialize();
    }
};

// Função para inicializar componentes monetários
FinanceSystem.initializeMoneyComponents = function(selectors) {
    if (!selectors || !Array.isArray(selectors)) return;
    
    // Tenta usar o módulo FormsEnhanced primeiro
    if (FinanceSystem.Modules && FinanceSystem.Modules.FormsEnhanced) {
        selectors.forEach(selector => {
            FinanceSystem.Modules.FormsEnhanced.initializeMoneyMask(selector);
        });
    } 
    // Fallback para o módulo Financial
    else if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
        selectors.forEach(selector => {
            FinanceSystem.Modules.Financial.initializeMoneyMask(selector);
        });
    }
};

// Função para inicializar DataTables comuns
FinanceSystem.initializeTableWithExport = function(selector, options = {}) {
    // Tenta usar o TableManager primeiro
    if (FinanceSystem.Modules && FinanceSystem.Modules.TableManager) {
        FinanceSystem.Modules.TableManager.initializeExportTable(selector, options);
    } 
    // Fallback para o jQuery diretamente
    else if (typeof $ !== 'undefined' && typeof $.fn.DataTable !== 'undefined') {
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
        
        const mergedOptions = {...defaultOptions, ...options};
        $(selector).DataTable(mergedOptions);
    }
};

// Função para inicializar filtros financeiros comuns
FinanceSystem.initializeFinancialFilters = function() {
    const filterButtons = document.querySelectorAll('.payment-filter, .income-filter');

    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Remove classe ativa de todos os botões
            filterButtons.forEach(btn => btn.classList.remove('active'));

            // Adiciona classe ativa ao botão clicado
            this.classList.add('active');

            // Filtra a tabela
            const filterValue = this.getAttribute('data-filter');
            
            // Usa TableManager se disponível
            if (FinanceSystem.Modules && FinanceSystem.Modules.TableManager) {
                FinanceSystem.Modules.TableManager.filterTableByStatus(filterValue);
            } 
            // Fallback para módulo Financial
            else if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
                FinanceSystem.Modules.Financial.filterFinancialTable(filterValue);
            }
            // Implementação direta se nenhum módulo estiver disponível
            else {
                const rows = document.querySelectorAll('.table-payments tbody tr, .table-incomes tbody tr');
                rows.forEach(row => {
                    const statusCell = row.querySelector('.payment-status, .badge');
                    if (!statusCell) return;

                    const rowStatus = statusCell.textContent.trim().toLowerCase();

                    if (filterValue === 'all' || rowStatus.includes(filterValue)) {
                        row.style.display = '';
                    } else {
                        row.style.display = 'none';
                    }
                });
            }
        });
    });

    // Filtro de pesquisa
    const searchInput = document.getElementById('financial-search');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            // Usa TableManager se disponível
            if (FinanceSystem.Modules && FinanceSystem.Modules.TableManager) {
                FinanceSystem.Modules.TableManager.filterTableByText(this.value);
            } 
            // Fallback para módulo Financial
            else if (FinanceSystem.Modules && FinanceSystem.Modules.Financial) {
                FinanceSystem.Modules.Financial.filterTableByText(this.value);
            }
            // Implementação direta se nenhum módulo estiver disponível
            else {
                const rows = document.querySelectorAll('.table-payments tbody tr, .table-incomes tbody tr');
                const text = this.value.toLowerCase();
                
                rows.forEach(row => {
                    const content = row.textContent.toLowerCase();

                    if (content.includes(text)) {
                        row.style.display = '';
                    } else {
                        row.style.display = 'none';
                    }
                });
            }
        });
    }
};
