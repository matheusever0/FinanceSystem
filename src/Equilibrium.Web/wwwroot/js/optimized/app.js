/**
 * Finance System - Arquivo principal de inicialização otimizado
 */

// Garantir que os namespaces existam
var FinanceSystem = FinanceSystem || {};

// Espera o DOM estar completamente carregado antes de inicializar
document.addEventListener('DOMContentLoaded', function () {
    // Inicializa componentes comuns para todas as páginas
    FinanceSystem.initializeCommonComponents();

    // Detecta e inicializa módulos específicos de página
    initializePageModules();
});

/**
 * Detecta qual página está sendo exibida e inicializa os módulos correspondentes
 */
function initializePageModules() {
    if (FinanceSystem.Pages) {
        // Lista de módulos de página para verificar
        const pageModules = [
            'Dashboard',
            'Payments',
            'CreditCards',
            'Financings',
            'Investments',
            'Users'
        ];
        
        // Verifica cada módulo e inicializa se existir
        pageModules.forEach(module => {
            if (FinanceSystem.Pages[module] && typeof FinanceSystem.Pages[module].initialize === 'function') {
                console.log(`Inicializando módulo: ${module}`);
                FinanceSystem.Pages[module].initialize();
            }
        });
        
        // Inicializa módulos adicionais se necessário
        if (FinanceSystem.Modules.Tables && typeof FinanceSystem.Modules.Tables.initialize === 'function') {
            FinanceSystem.Modules.Tables.initialize();
        }
    }
}
