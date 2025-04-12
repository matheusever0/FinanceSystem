/**
 * Scripts principais do site Finance System
 */

document.addEventListener('DOMContentLoaded', function () {
    // Elementos do DOM
    const menuToggle = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('sidebar');
    const topbar = document.getElementById('topbar');
    const mainContent = document.getElementById('main-content');
    const alerts = document.querySelectorAll('.alert-dismissible');

    // Toggle do sidebar
    if (menuToggle) {
        menuToggle.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                // Em dispositivos móveis, apenas mostra/esconde o sidebar
                sidebar.classList.toggle('show');
            } else {
                // Em desktop, colapsa o sidebar e ajusta o layout
                sidebar.classList.toggle('collapsed');
                topbar.classList.toggle('expanded');
                mainContent.classList.toggle('expanded');
            }
        });
    }

    // Auto-fechar alertas após um tempo
    if (alerts.length > 0) {
        alerts.forEach(function (alert) {
            setTimeout(function () {
                // Verificar se o Bootstrap está disponível
                if (typeof bootstrap !== 'undefined') {
                    const bsAlert = new bootstrap.Alert(alert);
                    bsAlert.close();
                } else {
                    // Fallback se o Bootstrap não estiver disponível
                    alert.style.display = 'none';
                }
            }, 5000); // Alertas desaparecem após 5 segundos
        });
    }
});

/**
 * Função para confirmar ação de exclusão
 * @param {string} formId - ID do formulário a ser submetido
 * @param {string} message - Mensagem de confirmação
 * @returns {boolean} - Resultado da confirmação
 */
function confirmDelete(formId, message) {
    message = message || 'Tem certeza que deseja excluir este item? Esta ação não pode ser desfeita.';
    if (confirm(message)) {
        document.getElementById(formId).submit();
    }
    return false;
}