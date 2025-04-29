/**
 * Finance System - Arquivo principal de inicialização
 * Este arquivo coordena a inicialização de todos os módulos do sistema
 */

// Espera o DOM estar completamente carregado antes de inicializar
document.addEventListener('DOMContentLoaded', function () {
    // Inicializa os componentes de interface do usuário
    FinanceSystem.UI.initialize();

    // Inicializa funcionalidades principais
    FinanceSystem.Core.initialize();

    // Detecta e inicializa módulos específicos de página
    initializePageModules();
});

/**
 * Detecta qual página está sendo exibida e inicializa os módulos correspondentes
 */
function initializePageModules() {
    // Dashboard
    if (document.querySelector('[data-page="dashboard"]')) {
        if (typeof FinanceSystem.Pages.Dashboard !== 'undefined') {
            FinanceSystem.Pages.Dashboard.initialize();
        }
    }

    // Pagamentos
    if (document.querySelector('[data-page="payments"]')) {
        if (typeof FinanceSystem.Pages.Payments !== 'undefined') {
            FinanceSystem.Pages.Payments.initialize();
        }
    }

    // Cartões de Crédito
    if (document.querySelector('[data-page="creditcards"]')) {
        if (typeof FinanceSystem.Pages.CreditCards !== 'undefined') {
            FinanceSystem.Pages.CreditCards.initialize();
        }
    }

    // Financiamentos
    if (document.querySelector('[data-page="financings"]')) {
        if (typeof FinanceSystem.Pages.Financings !== 'undefined') {
            FinanceSystem.Pages.Financings.initialize();
        }
    }

    // Investimentos
    if (document.querySelector('[data-page="investments"]')) {
        if (typeof FinanceSystem.Pages.Investments !== 'undefined') {
            FinanceSystem.Pages.Investments.initialize();
        }
    }

    // Usuários e Perfis
    if (document.querySelector('[data-page="users"]')) {
        if (typeof FinanceSystem.Pages.Users !== 'undefined') {
            FinanceSystem.Pages.Users.initialize();
        }
    }
}

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};