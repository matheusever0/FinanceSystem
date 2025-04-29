/**
 * Scripts principais do site Finance System
 */

// Aguardar que o DOM esteja completamente carregado
document.addEventListener('DOMContentLoaded', function () {
    initializeSidebar();
    initializeAlerts();
    initializeAjaxInterceptors();
    initializeDropdowns();
    initializeTooltips();
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

            // Salvar estado no localStorage
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });

        // Restaurar estado do sidebar do localStorage
        const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (sidebarCollapsed) {
            sidebar.classList.add('collapsed');
            if (topbar) topbar.classList.add('expanded');
            if (mainContent) mainContent.classList.add('expanded');
        }

        // Adicionar evento para expansão em telas pequenas
        const handleResize = function () {
            if (window.innerWidth < 992) {
                sidebar.classList.add('collapsed');
                if (topbar) topbar.classList.add('expanded');
                if (mainContent) mainContent.classList.add('expanded');
            }
        };

        window.addEventListener('resize', handleResize);
        // Executar ao carregar
        handleResize();
    }

    // Inicializar submenus
    const submenus = document.querySelectorAll('.sidebar-menu-link[data-bs-toggle="collapse"]');
    if (submenus.length > 0) {
        submenus.forEach(function (menuItem) {
            menuItem.addEventListener('click', function () {
                const icon = this.querySelector('.fa-angle-down, .fa-angle-right');
                if (icon) {
                    icon.classList.toggle('fa-angle-down');
                    icon.classList.toggle('fa-angle-right');
                }
            });
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
 * Inicializa os dropdowns do Bootstrap
 */
function initializeDropdowns() {
    // Verificar se o Bootstrap está disponível
    if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
        const dropdownElements = document.querySelectorAll('[data-bs-toggle="dropdown"]');
        dropdownElements.forEach(element => {
            new bootstrap.Dropdown(element);
        });
    } else {
        // Implementação manual simples de dropdown
        const dropdownToggles = document.querySelectorAll('[data-bs-toggle="dropdown"]');
        dropdownToggles.forEach(toggle => {
            toggle.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                const target = document.querySelector(this.getAttribute('data-bs-target') || this.getAttribute('href'));
                if (target) {
                    target.classList.toggle('show');
                }
            });
        });

        // Fechar dropdowns ao clicar fora
        document.addEventListener('click', function (e) {
            const dropdownMenus = document.querySelectorAll('.dropdown-menu.show');
            dropdownMenus.forEach(menu => {
                if (!menu.contains(e.target)) {
                    menu.classList.remove('show');
                }
            });
        });
    }
}

/**
 * Inicializa tooltips do Bootstrap
 */
function initializeTooltips() {
    // Verificar se o Bootstrap está disponível
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
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

/**
 * Formata um valor numérico como moeda brasileira
 * @param {number} value - Valor a ser formatado
 * @returns {string} - Valor formatado
 */
function formatCurrency(value) {
    return new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(value);
}

/**
 * Formata uma data no padrão brasileiro
 * @param {Date|string} date - Data a ser formatada
 * @returns {string} - Data formatada
 */
function formatDate(date) {
    if (typeof date === 'string') {
        date = new Date(date);
    }
    return date.toLocaleDateString('pt-BR');
}

/**
 * Cria um evento customizado
 * @param {string} eventName - Nome do evento
 * @param {object} detail - Detalhes do evento
 * @returns {CustomEvent} - Evento customizado
 */
function createCustomEvent(eventName, detail = {}) {
    return new CustomEvent(eventName, {
        bubbles: true,
        cancelable: true,
        detail: detail
    });
}

/**
 * Dispara um evento customizado
 * @param {string} eventName - Nome do evento
 * @param {HTMLElement} element - Elemento que disparará o evento
 * @param {object} detail - Detalhes do evento
 */
function dispatchCustomEvent(eventName, element, detail = {}) {
    const event = createCustomEvent(eventName, detail);
    element.dispatchEvent(event);
}