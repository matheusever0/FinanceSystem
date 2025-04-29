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

    FinanceSystem.Pages.Dashboard.initialize();
    FinanceSystem.Pages.Payments.initialize();
    FinanceSystem.Pages.CreditCards.initialize();
    FinanceSystem.Pages.Financings.initialize();
    FinanceSystem.Pages.Investments.initialize();
    FinanceSystem.Pages.Users.initialize();
}

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};