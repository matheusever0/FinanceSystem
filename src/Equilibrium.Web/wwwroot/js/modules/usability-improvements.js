document.addEventListener('DOMContentLoaded', function () {
    initEmailAssistant();
});
/**
 * Inicializa o assistente de preenchimento de emails
 */
function initEmailAssistant() {
    // Lista de provedores comuns de email
    const commonProviders = [
        '@gmail.com',
        '@hotmail.com',
        '@outlook.com',
        '@yahoo.com',
        '@icloud.com',
        '@protonmail.com',
        '@live.com',
        '@uol.com.br',
        '@bol.com.br',
        '@terra.com.br'
    ];

    // Encontra todos os campos de email
    const emailFields = document.querySelectorAll('input[type="email"], input[id*="Email"], input[name*="Email"]');

    emailFields.forEach(field => {
        // Cria o wrapper para o campo de email e o dropdown
        const wrapper = document.createElement('div');
        wrapper.className = 'email-field-wrapper position-relative';

        // Substitui o campo original pelo wrapper
        field.parentNode.insertBefore(wrapper, field);
        wrapper.appendChild(field);

        // Cria o botão do dropdown
        const dropdownBtn = document.createElement('button');
        dropdownBtn.className = 'btn btn-sm btn-outline-secondary dropdown-toggle email-provider-toggle';
        dropdownBtn.type = 'button';
        dropdownBtn.innerHTML = '<i class="fas fa-at"></i>';
        dropdownBtn.setAttribute('data-bs-toggle', 'dropdown');
        dropdownBtn.setAttribute('aria-expanded', 'false');

        // Cria o dropdown
        const dropdown = document.createElement('ul');
        dropdown.className = 'dropdown-menu email-providers-dropdown';

        // Adiciona os provedores ao dropdown
        commonProviders.forEach(provider => {
            const item = document.createElement('li');
            const link = document.createElement('a');
            link.className = 'dropdown-item';
            link.href = '#';
            link.textContent = provider;
            link.addEventListener('click', function (e) {
                e.preventDefault();

                // Extrai o nome de usuário se já houver um @ no campo
                let username = field.value;
                if (username.includes('@')) {
                    username = username.split('@')[0];
                }

                field.value = username + provider;

                // Dispara o evento change para acionar validações
                const event = new Event('change');
                field.dispatchEvent(event);
            });

            item.appendChild(link);
            dropdown.appendChild(item);
        });

        // Adiciona tudo ao wrapper
        const inputGroup = document.createElement('div');
        inputGroup.className = 'input-group';

        // Rearranja os elementos
        field.parentNode.insertBefore(inputGroup, field);
        inputGroup.appendChild(field);
        inputGroup.appendChild(dropdownBtn);
        inputGroup.appendChild(dropdown);

        // Adiciona estilo CSS inline para posicionar corretamente o dropdown
        const style = document.createElement('style');
        style.textContent = `
            .email-field-wrapper {
                position: relative;
            }
            .email-providers-dropdown {
                width: auto;
                min-width: 120px;
            }
        `;
        document.head.appendChild(style);
    });
}