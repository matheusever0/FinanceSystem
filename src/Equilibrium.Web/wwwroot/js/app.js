/**
 * Finance System - Arquivo principal de inicialização
 * Este arquivo coordena a inicialização de todos os módulos do sistema
 */

document.addEventListener('DOMContentLoaded', function () {

    if (typeof FinanceSystem === 'undefined') {
        return;
    }

    if (FinanceSystem.UI && typeof FinanceSystem.UI.initialize === 'function') {
        FinanceSystem.UI.initialize();
    }

    if (FinanceSystem.Core && typeof FinanceSystem.Core.initialize === 'function') {
        FinanceSystem.Core.initialize();
    }

    if (FinanceSystem.Validation && typeof FinanceSystem.Validation.initialize === 'function') {
        FinanceSystem.Validation.initialize();
    }

    initializePageModules();
});

function initializePageModules() {
    if (FinanceSystem.Pages) {
        const pageModules = [
            'Dashboard',
            'Payments',
            'CreditCards',
            'Financings',
            'Users'
        ];

        pageModules.forEach(module => {
            if (FinanceSystem.Pages[module] && typeof FinanceSystem.Pages[module].initialize === 'function') {
                console.log(`Inicializando módulo: ${module}`);
                FinanceSystem.Pages[module].initialize();
            }
        });

        if (FinanceSystem.Modules.Tables && typeof FinanceSystem.Modules.Tables.initialize === 'function') {
            FinanceSystem.Modules.Tables.initialize();
        }
    }
}

var FinanceSystem = FinanceSystem || {};