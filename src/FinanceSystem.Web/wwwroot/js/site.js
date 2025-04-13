/**
 * Scripts principais do site Finance System
 */

// Aguardar que o DOM esteja completamente carregado
document.addEventListener('DOMContentLoaded', function () {
    initializeSidebar();
    initializeAlerts();
    initializeAjaxInterceptors();
});

/**
 * Inicializa a funcionalidade do sidebar
 */
function initializeSidebar() {
    const menuToggle = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('sidebar');
    const topbar = document.getElementById('topbar');
    const mainContent = document.getElementById('main-content');

    if (menuToggle && sidebar) {
        menuToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            if (topbar) topbar.classList.toggle('expanded');
            if (mainContent) mainContent.classList.toggle('expanded');
        });
    }
}

/**
 * Inicializa o auto-fechamento dos alertas
 */
function initializeAlerts() {
    const alerts = document.querySelectorAll('.alert-dismissible');

    if (alerts.length > 0) {
        alerts.forEach(function (alert) {
            setTimeout(function () {
                // Verificar se o Bootstrap está disponível
                if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                    const bsAlert = new bootstrap.Alert(alert);
                    bsAlert.close();
                } else {
                    // Fallback se o Bootstrap não estiver disponível
                    alert.style.display = 'none';
                }
            }, 5000); // Alertas desaparecem após 5 segundos
        });
    }
}

/**
 * Inicializa os interceptores de AJAX e fetch
 */
function initializeAjaxInterceptors() {
    // Interceptar erros de requisições jQuery AJAX
    if (typeof $ !== 'undefined' && $.ajaxError) {
        $(document).ajaxError(function (event, jqXHR, settings, thrownError) {
            if (jqXHR.status === 401) {
                window.location.href = '/Account/Login?expired=true';
            }
        });
    }

    // Interceptar erros de fetch API
    const originalFetch = window.fetch;
    if (originalFetch) {
        window.fetch = function (url, options) {
            return originalFetch(url, options)
                .then(response => {
                    if (response.status === 401) {
                        window.location.href = '/Account/Login?expired=true';
                        return Promise.reject('Unauthorized');
                    }
                    return response;
                });
        };
    }
}

/**
 * Função para confirmar ação de exclusão
 * @param {string} formId - ID do formulário a ser submetido
 * @param {string} message - Mensagem de confirmação opcional
 * @returns {boolean} - Resultado da confirmação
 */
function confirmDelete(formId, message) {
    message = message || 'Tem certeza que deseja excluir este item? Esta ação não pode ser desfeita.';
    if (confirm(message)) {
        const form = document.getElementById(formId);
        if (form) form.submit();
    }
    return false;
}