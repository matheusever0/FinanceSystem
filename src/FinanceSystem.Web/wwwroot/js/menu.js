/**
 * Funcionalidades de menu para o sistema financeiro
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeMenuToggle();
    initializeSubMenus();
    initializeMobileMenu();
    highlightActiveMenuItem(); // Ensure this runs on page load
});

/**
 * Inicializa o toggle do menu lateral
 */
function initializeMenuToggle() {
    const menuToggle = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('sidebar');
    const topbar = document.getElementById('topbar');
    const mainContent = document.getElementById('main-content');

    if (menuToggle && sidebar && topbar && mainContent) {
        menuToggle.addEventListener('click', function () {
            // For mobile devices
            if (window.innerWidth < 992) {
                sidebar.classList.toggle('show');
                return;
            }

            // For desktop
            sidebar.classList.toggle('collapsed');
            topbar.classList.toggle('expanded');
            mainContent.classList.toggle('expanded');

            // Salvar estado na sessão
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });

        // Carregar estado salvo
        const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (sidebarCollapsed && window.innerWidth >= 992) {
            sidebar.classList.add('collapsed');
            topbar.classList.add('expanded');
            mainContent.classList.add('expanded');
        }
    }
}

/**
 * Inicializa os submenus expansíveis
 */
function initializeSubMenus() {
    // If Bootstrap is available, it will manage the collapse
    if (typeof bootstrap === 'undefined') {
        // Manual implementation
        const subMenuToggleButtons = document.querySelectorAll('.sidebar-menu-link[data-bs-toggle="collapse"]');

        subMenuToggleButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                const targetId = this.getAttribute('data-bs-target') || this.getAttribute('href');
                if (!targetId) return;

                const targetElement = document.querySelector(targetId);
                if (!targetElement) return;

                // Toggle submenu
                const isVisible = targetElement.classList.contains('show');

                // Close other submenus
                document.querySelectorAll('.collapse.show').forEach(element => {
                    if (element !== targetElement) {
                        element.classList.remove('show');

                        // Update icons of other submenus
                        const toggle = document.querySelector(`[data-bs-target="#${element.id}"]`) ||
                            document.querySelector(`[href="#${element.id}"]`);
                        if (toggle) {
                            const icon = toggle.querySelector('.fa-angle-down');
                            if (icon) {
                                icon.classList.remove('fa-angle-down');
                                icon.classList.add('fa-angle-right');
                            }
                        }
                    }
                });

                // Toggle current submenu
                targetElement.classList.toggle('show');

                // Update submenu icon
                const icon = this.querySelector('.fa-angle-down, .fa-angle-right');
                if (icon) {
                    icon.classList.toggle('fa-angle-down');
                    icon.classList.toggle('fa-angle-right');
                }

                // If in collapsed mode, expand sidebar on desktop only
                const sidebar = document.getElementById('sidebar');
                if (sidebar && sidebar.classList.contains('collapsed') && window.innerWidth >= 992) {
                    sidebar.classList.remove('collapsed');
                    document.getElementById('topbar').classList.remove('expanded');
                    document.getElementById('main-content').classList.remove('expanded');
                    localStorage.setItem('sidebarCollapsed', 'false');
                }
            });
        });
    }

    // Ensure icons are correct for already expanded menus
    document.querySelectorAll('.collapse.show').forEach(collapse => {
        const toggle = document.querySelector(`[data-bs-target="#${collapse.id}"]`) ||
            document.querySelector(`[href="#${collapse.id}"]`);
        if (toggle) {
            const icon = toggle.querySelector('.fa-angle-right');
            if (icon) {
                icon.classList.remove('fa-angle-right');
                icon.classList.add('fa-angle-down');
            }
        }
    });
}

/**
 * Inicializa o comportamento do menu em dispositivos móveis
 */
function initializeMobileMenu() {
    const sidebar = document.getElementById('sidebar');

    // Close menu when clicking outside on mobile
    if (sidebar) {
        document.addEventListener('click', function (e) {
            const isMobile = window.innerWidth < 992;
            const menuToggle = document.getElementById('menu-toggle');

            if (isMobile &&
                sidebar.classList.contains('show') &&
                !sidebar.contains(e.target) &&
                e.target !== menuToggle &&
                !menuToggle.contains(e.target)) {
                sidebar.classList.remove('show');
            }
        });
    }

    // Close menu when clicking a menu item on mobile
    const menuItems = document.querySelectorAll('.sidebar-menu-link');
    menuItems.forEach(item => {
        item.addEventListener('click', function (e) {
            if (window.innerWidth < 992 && !this.hasAttribute('data-bs-toggle')) {
                sidebar.classList.remove('show');
            }
        });
    });

    // Handle menu on window resize
    window.addEventListener('resize', function () {
        if (sidebar) {
            if (window.innerWidth >= 992) {
                // Switch to desktop mode
                sidebar.classList.remove('show');

                // Apply collapsed state from localStorage
                const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
                if (sidebarCollapsed) {
                    sidebar.classList.add('collapsed');
                    document.getElementById('topbar').classList.add('expanded');
                    document.getElementById('main-content').classList.add('expanded');
                } else {
                    sidebar.classList.remove('collapsed');
                    document.getElementById('topbar').classList.remove('expanded');
                    document.getElementById('main-content').classList.remove('expanded');
                }
            }
        }
    });
}

/**
 * Marca o item de menu ativo com base na URL atual
 */
function highlightActiveMenuItem() {
    const currentPath = window.location.pathname;

    // Find all menu links
    const menuLinks = document.querySelectorAll('.sidebar-menu-link');

    // Remove 'active' class from all links
    menuLinks.forEach(link => {
        link.classList.remove('active');
    });

    // Find and mark the link that matches the current URL
    menuLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && (href === currentPath || currentPath.startsWith(href))) {
            link.classList.add('active');

            // If the link is in a submenu, expand the submenu
            const parentCollapse = link.closest('.collapse');
            if (parentCollapse) {
                parentCollapse.classList.add('show');

                // Update the toggle button icon
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