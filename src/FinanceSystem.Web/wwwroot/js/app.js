/**
 * Finance System - Arquivo principal de inicialização
 * Este arquivo coordena a inicialização de todos os módulos do sistema
 */

// Espera o DOM estar completamente carregado antes de inicializar
document.addEventListener('DOMContentLoaded', function () {

    // Garantir que os namespaces existam
    if (typeof FinanceSystem === 'undefined') {
        return;
    }

    // Inicializa os componentes de interface do usuário
    if (FinanceSystem.UI && typeof FinanceSystem.UI.initialize === 'function') {
        FinanceSystem.UI.initialize();
    }

    // Inicializa funcionalidades principais
    if (FinanceSystem.Core && typeof FinanceSystem.Core.initialize === 'function') {
        FinanceSystem.Core.initialize();
    }

    // Inicializa módulos específicos de validação
    if (FinanceSystem.Validation && typeof FinanceSystem.Validation.initialize === 'function') {
        FinanceSystem.Validation.initialize();
    }

    // Detecta e inicializa módulos específicos de página
    initializePageModules();
});

/**
 * Detecta qual página está sendo exibida e inicializa os módulos correspondentes
 */
function initializePageModules() {
    if (FinanceSystem.Pages) {
        if (FinanceSystem.Pages.Dashboard && typeof FinanceSystem.Pages.Dashboard.initialize === 'function') {
            FinanceSystem.Pages.Dashboard.initialize();
        }

        if (FinanceSystem.Pages.Payments && typeof FinanceSystem.Pages.Payments.initialize === 'function') {
            FinanceSystem.Pages.Payments.initialize();
        }

        if (FinanceSystem.Pages.CreditCards && typeof FinanceSystem.Pages.CreditCards.initialize === 'function') {
            FinanceSystem.Pages.CreditCards.initialize();
        }

        if (FinanceSystem.Pages.Financings && typeof FinanceSystem.Pages.Financings.initialize === 'function') {
            FinanceSystem.Pages.Financings.initialize();
        }

        if (FinanceSystem.Pages.Investments && typeof FinanceSystem.Pages.Investments.initialize === 'function') {
            FinanceSystem.Pages.Investments.initialize();
        }

        if (FinanceSystem.Pages.Users && typeof FinanceSystem.Pages.Users.initialize === 'function') {
            FinanceSystem.Pages.Users.initialize();
        }
    }
}

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};