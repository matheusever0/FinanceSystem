/**
 * Funcionalidades de menu para o sistema financeiro
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeMenuToggle();
    initializeSubMenus();
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
    const subMenuToggleButtons = document.querySelectorAll('.sidebar-menu-link[data-bs-toggle="collapse"]');

    subMenuToggleButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            const icon = this.querySelector('.fa-angle-down, .fa-angle-right');
            if (icon) {
                icon.classList.toggle('fa-angle-down');
                icon.classList.toggle('fa-angle-right');
            }

            // Se em modo colapsado, expand o sidebar
            const sidebar = document.getElementById('sidebar');
            if (sidebar && sidebar.classList.contains('collapsed')) {
                sidebar.classList.remove('collapsed');
                document.getElementById('topbar').classList.remove('expanded');
                document.getElementById('main-content').classList.remove('expanded');
                localStorage.setItem('sidebarCollapsed', 'false');
            }
        });
    });

    // Garante que os ícones estejam corretos para os menus já expandidos
    document.querySelectorAll('.collapse.show').forEach(collapse => {
        const toggle = document.querySelector(`[data-bs-target="#${collapse.id}"]`);
        if (toggle) {
            const icon = toggle.querySelector('.fa-angle-right');
            if (icon) {
                icon.classList.remove('fa-angle-right');
                icon.classList.add('fa-angle-down');
            }
        }
    });
}