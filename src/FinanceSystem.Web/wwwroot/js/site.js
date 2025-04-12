/**
 * Scripts principais do site
 */

// Gerenciamento do sidebar responsivo
const FinanceApp = {
    // Elementos DOM
    elements: {
        menuToggle: document.getElementById('menu-toggle'),
        sidebar: document.getElementById('sidebar'),
        topbar: document.getElementById('topbar'),
        mainContent: document.getElementById('main-content'),
        alerts: document.querySelectorAll('.alert-dismissible')
    },

    // Inicialização
    init: function () {
        this.setupEventListeners();
        this.setupAutoCloseAlerts();
    },

    // Configurar os event listeners
    setupEventListeners: function () {
        // Toggle do sidebar
        if (this.elements.menuToggle) {
            this.elements.menuToggle.addEventListener('click', this.toggleSidebar.bind(this));
        }
    },

    // Alternar a visibilidade do sidebar
    toggleSidebar: function () {
        if (window.innerWidth < 992) {
            // Em dispositivos móveis, apenas mostra/esconde o sidebar
            this.elements.sidebar.classList.toggle('show');
        } else {
            // Em desktop, colapsa o sidebar e ajusta o layout
            this.elements.sidebar.classList.toggle('collapsed');
            this.elements.topbar.classList.toggle('expanded');
            this.elements.mainContent.classList.toggle('expanded');
        }
    },

    // Auto-fechar alertas após um tempo
    setupAutoCloseAlerts: function () {
        this.elements.alerts.forEach(function (alert) {
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
    },

    // Confirmar ação de exclusão
    confirmDelete: function (formId, message) {
        message = message || 'Tem certeza que deseja excluir este item? Esta ação não pode ser desfeita.';
        if (confirm(message)) {
            document.getElementById(formId).submit();
        }
        return false;
    }
};

// Inicializar quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    FinanceApp.init();
});