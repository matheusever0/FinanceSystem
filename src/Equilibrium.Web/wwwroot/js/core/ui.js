/**
 * Finance System - UI
 * Componentes e funcionalidades de interface reutilizáveis
 */

var FinanceSystem = FinanceSystem || {};

FinanceSystem.UI = (function () {

    function initialize() {
        initializeSidebar();
        setupMobileBackdrop();
        initializeAlerts();
        initializeDropdowns();
        initializeTooltips();
    }

    function initializeSidebar() {
        const menuToggle = document.getElementById('menu-toggle');
        const sidebar = document.getElementById('sidebar');
        const topbar = document.getElementById('topbar');
        const mainContent = document.getElementById('main-content');

        if (menuToggle && sidebar) {
            menuToggle.addEventListener('click', function () {
                if (window.innerWidth < 992) {
                    sidebar.classList.toggle('show');
                } else {
                    sidebar.classList.toggle('collapsed');
                    if (topbar) topbar.classList.toggle('expanded');
                    if (mainContent) mainContent.classList.toggle('expanded');

                    localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
                }
            });

            if (window.innerWidth >= 992) {
                const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
                if (sidebarCollapsed) {
                    sidebar.classList.add('collapsed');
                    if (topbar) topbar.classList.add('expanded');
                    if (mainContent) mainContent.classList.add('expanded');
                }
            }

            document.addEventListener('click', function (e) {
                if (window.innerWidth < 992 &&
                    sidebar.classList.contains('show') &&
                    !sidebar.contains(e.target) &&
                    !menuToggle.contains(e.target)) {
                    sidebar.classList.remove('show');
                }
            });

            const handleResize = function () {
                if (window.innerWidth < 992) {
                    sidebar.classList.remove('show'); // Esconde em resize
                    sidebar.classList.add('collapsed');
                    if (topbar) topbar.classList.add('expanded');
                    if (mainContent) mainContent.classList.add('expanded');
                }
            };

            window.addEventListener('resize', handleResize);
            handleResize();
        }

        initializeSubmenus();

        highlightActiveMenuItem();
    }

    function initializeSubmenus() {
        const submenus = document.querySelectorAll('.sidebar-menu-link[data-bs-toggle="collapse"]');

        if (submenus.length > 0) {
            submenus.forEach(function (menuItem) {
                if (typeof bootstrap !== 'undefined' && bootstrap.Collapse) {
                    const targetId = menuItem.getAttribute('data-bs-target') ||
                        menuItem.getAttribute('href');

                    if (targetId) {
                        const collapseElement = document.querySelector(targetId);
                        if (collapseElement) {
                            const bsCollapse = new bootstrap.Collapse(collapseElement, {
                                toggle: false
                            });

                            menuItem.addEventListener('click', function (e) {
                                e.preventDefault();
                                bsCollapse.toggle();

                                const icon = this.querySelector('.fa-angle-down, .fa-angle-right');
                                if (icon) {
                                    icon.classList.toggle('fa-angle-down');
                                    icon.classList.toggle('fa-angle-right');
                                }
                            });
                        }
                    }
                } else {
                    menuItem.addEventListener('click', function (e) {
                        e.preventDefault();

                        const targetId = this.getAttribute('data-bs-target') ||
                            this.getAttribute('href');

                        if (targetId) {
                            const targetElement = document.querySelector(targetId);
                            if (targetElement) {
                                targetElement.classList.toggle('show');

                                const icon = this.querySelector('.fa-angle-down, .fa-angle-right');
                                if (icon) {
                                    icon.classList.toggle('fa-angle-down');
                                    icon.classList.toggle('fa-angle-right');
                                }
                            }
                        }
                    });
                }
            });
        }
    }

    function highlightActiveMenuItem() {
        const currentPath = window.location.pathname;

        const menuLinks = document.querySelectorAll('.sidebar-menu-link');

        menuLinks.forEach(link => {
            link.classList.remove('active');
        });

        menuLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href && (href === currentPath || currentPath.startsWith(href))) {
                link.classList.add('active');

                const parentCollapse = link.closest('.collapse');
                if (parentCollapse) {
                    parentCollapse.classList.add('show');

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

    function initializeAlerts() {
        const alerts = document.querySelectorAll('.alert-dismissible');

        if (alerts.length > 0) {
            alerts.forEach(function (alert) {
                setTimeout(function () {
                    if (typeof bootstrap !== 'undefined' && bootstrap.Alert) {
                        const bsAlert = new bootstrap.Alert(alert);
                        bsAlert.close();
                    } else {
                        alert.style.display = 'none';
                    }
                }, 5000); // Alertas desaparecem após 5 segundos
            });
        }
    }

    function initializeDropdowns() {
        if (typeof bootstrap !== 'undefined' && bootstrap.Dropdown) {
            const dropdownElements = document.querySelectorAll('[data-bs-toggle="dropdown"]');
            dropdownElements.forEach(element => {
                new bootstrap.Dropdown(element);
            });
        } else {
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

    function initializeTooltips() {
        if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
            const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
            tooltipTriggerList.map(function (tooltipTriggerEl) {
                return new bootstrap.Tooltip(tooltipTriggerEl);
            });
        }
    }

    function initializeModal(modalId) {
        const modalElement = document.getElementById(modalId);
        if (!modalElement) return null;

        if (typeof bootstrap !== 'undefined' && bootstrap.Modal) {
            return new bootstrap.Modal(modalElement);
        }
        return null;
    }

    function showModal(modalId) {
        const modal = initializeModal(modalId);
        if (modal) {
            modal.show();
        }
    }

    function hideModal(modalId) {
        const modal = initializeModal(modalId);
        if (modal) {
            modal.hide();
        }
    }

    function showAlert(message, type = 'info', containerId = 'alert-container', timeout = 15000) {
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
                    setTimeout(() => {
                        if (alertDiv.parentNode) {
                            alertDiv.parentNode.removeChild(alertDiv);
                        }
                    }, 500);
                }
            }, timeout);
        }
    }

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

    function setupMobileBackdrop() {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar) return;

        let backdrop = document.querySelector('.sidebar-backdrop');
        if (!backdrop) {
            backdrop = document.createElement('div');
            backdrop.className = 'sidebar-backdrop';
            document.body.appendChild(backdrop);
        }

        backdrop.addEventListener('click', function () {
            sidebar.classList.remove('show');
        });
    }

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