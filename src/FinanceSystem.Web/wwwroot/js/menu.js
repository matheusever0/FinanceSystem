/**
 * Funcionalidades de menu para o sistema financeiro
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeMenuToggle();
    initializeSubMenus();
    initializeMobileMenu();
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
            sidebar.classList.toggle('collapsed');
            topbar.classList.toggle('expanded');
            mainContent.classList.toggle('expanded');

            // Salvar estado na sessão
            localStorage.setItem('sidebarCollapsed', sidebar.classList.contains('collapsed'));
        });

        // Carregar estado salvo
        const sidebarCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (sidebarCollapsed) {
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
    // Se o Bootstrap estiver disponível, ele gerenciará o colapso
    if (typeof bootstrap === 'undefined') {
        // Implementação manual
        const subMenuToggleButtons = document.querySelectorAll('.sidebar-menu-link[data-bs-toggle="collapse"]');

        subMenuToggleButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();

                const targetId = this.getAttribute('data-bs-target') || this.getAttribute('href');
                if (!targetId) return;

                const targetElement = document.querySelector(targetId);
                if (!targetElement) return;

                // Toggle do submenu
                const isVisible = targetElement.classList.contains('show');

                // Fechar outros submenus
                document.querySelectorAll('.collapse.show').forEach(element => {
                    if (element !== targetElement) {
                        element.classList.remove('show');

                        // Atualizar ícones de outros submenus
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

                // Toggle do submenu atual
                targetElement.classList.toggle('show');

                // Atualizar ícone do submenu
                const icon = this.querySelector('.fa-angle-down, .fa-angle-right');
                if (icon) {
                    icon.classList.toggle('fa-angle-down');
                    icon.classList.toggle('fa-angle-right');
                }

                // Se em modo colapsado, expande o sidebar
                const sidebar = document.getElementById('sidebar');
                if (sidebar && sidebar.classList.contains('collapsed')) {
                    sidebar.classList.remove('collapsed');
                    document.getElementById('topbar').classList.remove('expanded');
                    document.getElementById('main-content').classList.remove('expanded');
                    localStorage.setItem('sidebarCollapsed', 'false');
                }
            });
        });
    }

    // Garante que os ícones estejam corretos para os menus já expandidos
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
    const menuToggle = document.getElementById('menu-toggle');
    const sidebar = document.getElementById('sidebar');

    // Detectar cliques fora do menu para fechá-lo em dispositivos móveis
    if (sidebar) {
        document.addEventListener('click', function (e) {
            const isMobile = window.innerWidth < 992;

            if (isMobile &&
                sidebar.classList.contains('show') &&
                !sidebar.contains(e.target) &&
                e.target !== menuToggle) {

                sidebar.classList.remove('show');
            }
        });
    }

    // Adicionar classe 'show' ao menu quando o botão de toggle é clicado em dispositivos móveis
    if (menuToggle && sidebar) {
        menuToggle.addEventListener('click', function () {
            if (window.innerWidth < 992) {
                sidebar.classList.toggle('show');
            }
        });
    }

    // Fechar menu ao clicar em um item em dispositivos móveis
    const menuItems = document.querySelectorAll('.sidebar-menu-link');
    menuItems.forEach(item => {
        item.addEventListener('click', function () {
            if (window.innerWidth < 992 && !this.classList.contains('has-submenu')) {
                sidebar.classList.remove('show');
            }
        });
    });

    // Ajustar menu ao redimensionar a janela
    window.addEventListener('resize', function () {
        if (window.innerWidth >= 992) {
            sidebar.classList.remove('show');
        }
    });
}

/**
 * Marca o item de menu ativo com base na URL atual
 */
function highlightActiveMenuItem() {
    const currentPath = window.location.pathname;

    // Encontrar todos os links do menu
    const menuLinks = document.querySelectorAll('.sidebar-menu-link');

    // Remover classe 'active' de todos os links
    menuLinks.forEach(link => {
        link.classList.remove('active');
    });

    // Encontrar e marcar o link que corresponde à URL atual
    menuLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && (href === currentPath || currentPath.startsWith(href))) {
            link.classList.add('active');

            // Se o link estiver em um submenu, expandir o submenu
            const parentCollapse = link.closest('.collapse');
            if (parentCollapse) {
                parentCollapse.classList.add('show');

                // Atualizar o ícone do botão de toggle
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

// Executar highLightActiveMenuItem quando a página carregar
document.addEventListener('DOMContentLoaded', highlightActiveMenuItem);