/**
 * Finance System - UI
 * Componentes e funcionalidades de interface reutilizáveis
 */

// Namespace global para o sistema
var FinanceSystem = FinanceSystem || {};

// Módulo UI
FinanceSystem.UI = (function () {
    /**
     * Inicializa todos os componentes de UI
     */
    function initialize() {
        initializeSidebar();
        initializeAlerts();
        initializeDropdowns();
        initializeTooltips();
    }

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
        initializeSubmenus();

        // Destacar item ativo do menu
        highlightActiveMenuItem();
    }

    /**
     * Inicializa os submenus expandíveis
     */
    function initializeSubmenus() {
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
     * Marca o item de menu ativo com base na URL atual
     */
    function highlightActiveMenuItem() {
        const currentPath = window.location.pathname;

        // Encontra todos os links de menu
        const menuLinks = document.querySelectorAll('.sidebar-menu-link');

        // Remove a classe 'active' de todos os links
        menuLinks.forEach(link => {
            link.classList.remove('active');
        });

        // Encontra e marca o link que corresponde à URL atual
        menuLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href && (href === currentPath || currentPath.startsWith(href))) {
                link.classList.add('active');

                // Se o link estiver em um submenu, expande o submenu
                const parentCollapse = link.closest('.collapse');
                if (parentCollapse) {
                    parentCollapse.classList.add('show');

                    // Atualiza o ícone do botão de toggle
                    const toggle = document.querySelector(`[data-bs-target="#${parentCollapse.id}"]`) ||
                        document.querySelector(`[href="#${parentCollapse.id}"]`);
                    if (toggle) {
                        const icon = toggle.querySelector('.fa-angle-right');
                        if (icon) {
                            icon.classList.remove('fa-angle-right');
                            icon.classList.add('fa-angle-down');
                        }
                    }
                }
            }
        });
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
     * Inicializa um modal do Bootstrap
     * @param {string} modalId - ID do elemento modal
     * @returns {object|null} - Instância do modal ou null
     */
    function initializeModal(modalId) {
        const modalElement = document.getElementById(modalId);
        if (!modalElement) return null;

        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            return new bootstrap.Modal(modalElement);
        }
        return null;
    }

    /**
     * Mostra um modal do Bootstrap
     * @param {string} modalId - ID do elemento modal
     */
    function showModal(modalId) {
        const modal = initializeModal(modalId);
        if (modal) {
            modal.show();
        }
    }

    /**
     * Esconde um modal do Bootstrap
     * @param {string} modalId - ID do elemento modal
     */
    function hideModal(modalId) {
        const modal = initializeModal(modalId);
        if (modal) {
            modal.hide();
        }
    }

    /**
     * Mostra uma mensagem de alerta
     * @param {string} message - Mensagem a ser exibida
     * @param {string} type - Tipo de alerta (success, danger, warning, info)
     * @param {string} containerId - ID do elemento onde o alerta será inserido
     * @param {number} timeout - Tempo em ms para o alerta desaparecer (0 para não desaparecer)
     */
    function showAlert(message, type = 'info', containerId = 'alert-container', timeout = 5000) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
        alertDiv.role = 'alert';

        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;

        container.appendChild(alertDiv);

        if (timeout > 0) {
            setTimeout(() => {
                if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                    const bsAlert = new bootstrap.Alert(alertDiv);
                    bsAlert.close();
                } else {
                    alertDiv.style.display = 'none';
                    // Remover do DOM após a animação
                    setTimeout(() => {
                        if (alertDiv.parentNode) {
                            alertDiv.parentNode.removeChild(alertDiv);
                        }
                    }, 500);
                }
            }, timeout);
        }
    }

    /**
     * Alterna o estado de elemento collapsible
     * @param {string} elementId - ID do elemento collapsible
     */
    function toggleCollapse(elementId) {
        const element = document.getElementById(elementId);
        if (!element) return;

        if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
            const bsCollapse = new bootstrap.Collapse(element);
            bsCollapse.toggle();
        } else {
            element.classList.toggle('show');
        }
    }

    // API pública do módulo
    return {
        initialize: initialize,
        initializeSidebar: initializeSidebar,
        highlightActiveMenuItem: highlightActiveMenuItem,
        initializeAlerts: initializeAlerts,
        initializeDropdowns: initializeDropdowns,
        initializeTooltips: initializeTooltips,
        initializeModal: initializeModal,
        showModal: showModal,
        hideModal: hideModal,
        showAlert: showAlert,
        toggleCollapse: toggleCollapse
    };
})();