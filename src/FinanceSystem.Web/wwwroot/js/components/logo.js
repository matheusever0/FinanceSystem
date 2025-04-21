/**
 * Integração do logo para FinanceSystem by SeveroTech
 * Este script adiciona dinamicamente o logo ao sidebar
 */

document.addEventListener('DOMContentLoaded', function () {
    // Adiciona logo ao sidebar
    const sidebarBrand = document.querySelector('.sidebar-brand');
    if (sidebarBrand) {
        // Limpa conteúdo existente
        sidebarBrand.innerHTML = '';

        // Cria container do logo
        const logoContainer = document.createElement('div');
        logoContainer.classList.add('sidebar-logo-container');

        // Obtém apenas o monograma FS do SVG para o sidebar
        const logoSvg = `
    <svg width="40" height="40" viewBox="0 0 64 64" xmlns="http://www.w3.org/2000/svg">
      <defs>
        <linearGradient id="logoGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="#3b5998" />
          <stop offset="100%" stop-color="#8b9dc3" />
        </linearGradient>
        <linearGradient id="accentGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stop-color="#1cc88a" />
          <stop offset="100%" stop-color="#0bb36e" />
        </linearGradient>
      </defs>
      
      <g transform="translate(8, 8) scale(0.75)">
        <!-- F -->
        <path d="M12,12 h18 v6 h-12 v8 h10 v6 h-10 v16" stroke="url(#logoGradient)" stroke-width="6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        
        <!-- Chart line -->
        <path d="M16,26 l4,-4 l8,2 l6,-6" stroke="url(#accentGradient)" stroke-width="2" fill="none" stroke-linecap="round"/>
        
        <!-- S -->
        <path d="M40,16 c-4,-3 -8,-2 -11,1 c-3,3 -4,8 -1,11 c3,3 9,5 12,8 c3,3 4,7 1,10 c-3,3 -8,3 -12,0" stroke="url(#logoGradient)" stroke-width="6" stroke-linecap="round" stroke-linejoin="round" fill="none"/>
        <path d="M36,10 v28" stroke="url(#logoGradient)" stroke-width="3" stroke-linecap="round" fill="none"/>
      </g>
    </svg>
    `;

        logoContainer.innerHTML = logoSvg;

        // Cria texto da marca
        const brandText = document.createElement('span');
        brandText.textContent = 'FinanceSystem';
        brandText.style.marginLeft = '10px';
        brandText.style.fontSize = '1.2rem';
        brandText.style.fontWeight = '600';

        // Adiciona elementos
        sidebarBrand.appendChild(logoContainer);
        sidebarBrand.appendChild(brandText);

        // Trata estado colapsado
        const sidebar = document.getElementById('sidebar');
        if (sidebar && sidebar.classList.contains('collapsed')) {
            brandText.style.display = 'none';
        }

        // Escuta o toggle do sidebar
        const menuToggle = document.getElementById('menu-toggle');
        if (menuToggle) {
            menuToggle.addEventListener('click', function () {
                if (sidebar.classList.contains('collapsed')) {
                    brandText.style.display = 'none';
                } else {
                    brandText.style.display = '';
                }
            });
        }
    }
});