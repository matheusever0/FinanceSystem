/**
 * Gerenciamento de edição de usuário
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeStatusToggle();
    initializeRolesManager();
    updateRoleSelector(); // Inicialmente atualiza a lista de roles disponíveis
    setupFormValidation(); // Adiciona validação personalizada ao formulário
});

/**
 * Inicializa o toggle de status (ativo/inativo)
 */
function initializeStatusToggle() {
    const statusSwitch = document.getElementById('IsActive');
    if (!statusSwitch) return;

    // Função para atualizar o label de status
    function updateStatusLabel() {
        const statusLabel = document.getElementById('statusLabel');
        if (statusSwitch.checked) {
            statusLabel.classList.remove('bg-danger');
            statusLabel.classList.add('bg-success');
            statusLabel.textContent = 'Ativo';
        } else {
            statusLabel.classList.remove('bg-success');
            statusLabel.classList.add('bg-danger');
            statusLabel.textContent = 'Inativo';
        }
    }

    // Inicializar o status
    updateStatusLabel();

    // Atualizar quando o switch mudar
    statusSwitch.addEventListener('change', updateStatusLabel);
}

/**
 * Inicializa o gerenciador de perfis (roles)
 */
function initializeRolesManager() {
    const addRoleBtn = document.getElementById('addRoleBtn');
    if (!addRoleBtn) return;

    // Adicionar novo perfil
    addRoleBtn.addEventListener('click', function () {
        const roleSelector = document.getElementById('roleSelector');
        const selectedRole = roleSelector.value;
        if (!selectedRole) return;

        // Verificar se o perfil já está selecionado
        const selectedInputs = document.querySelectorAll("#selectedRolesContainer input[name='selectedRoles']");
        let isDuplicate = false;

        selectedInputs.forEach(function (input) {
            if (input.value === selectedRole) {
                isDuplicate = true;
            }
        });

        if (!isDuplicate) {
            // Adicionar o novo perfil
            const container = document.getElementById('selectedRolesContainer');
            const newRoleBadge = document.createElement('div');
            newRoleBadge.className = 'badge bg-primary p-2 me-2 mb-2 role-badge';

            const hiddenInput = document.createElement('input');
            hiddenInput.type = 'hidden';
            hiddenInput.name = 'selectedRoles';
            hiddenInput.value = selectedRole;

            const removeLink = document.createElement('a');
            removeLink.href = '#';
            removeLink.className = 'ms-1 text-white remove-role';
            removeLink.innerHTML = '<i class="fas fa-times"></i>';

            newRoleBadge.appendChild(hiddenInput);
            newRoleBadge.appendChild(document.createTextNode(selectedRole));
            newRoleBadge.appendChild(removeLink);

            container.appendChild(newRoleBadge);
            roleSelector.value = '';

            // Atualizar o seletor de perfis
            updateRoleSelector();

            // Esconder mensagem de erro se existir
            const errorMessage = document.getElementById('roles-error-message');
            if (errorMessage) {
                errorMessage.style.display = 'none';
            }
        }
    });

    // Remover perfil (delegação de eventos)
    document.addEventListener('click', function (e) {
        if (e.target.closest('.remove-role')) {
            e.preventDefault();
            const badge = e.target.closest('.role-badge');
            if (badge) {
                badge.remove();

                // Atualizar o seletor de perfis após remover
                updateRoleSelector();
            }
        }
    });
}

/**
 * Atualiza o seletor de perfis para esconder as opções já selecionadas
 */
function updateRoleSelector() {
    const roleSelector = document.getElementById('roleSelector');
    const selectedRoles = Array.from(document.querySelectorAll("#selectedRolesContainer input[name='selectedRoles']"))
        .map(input => input.value);

    // Iterar pelas opções do selector e esconder as que já estão selecionadas
    Array.from(roleSelector.options).forEach(option => {
        if (option.value && selectedRoles.includes(option.value)) {
            option.style.display = 'none';
        } else {
            option.style.display = '';
        }
    });
}

/**
 * Configura a validação do formulário
 */
function setupFormValidation() {
    const form = document.querySelector('form[asp-action="Edit"], form[asp-action="Create"]');
    if (!form) return;

    form.addEventListener('submit', function (e) {
        const selectedRoles = document.querySelectorAll("#selectedRolesContainer input[name='selectedRoles']");
        const rolesContainer = document.getElementById('selectedRolesContainer');

        // Verificar se existe alguma role selecionada
        if (selectedRoles.length === 0) {
            e.preventDefault(); // Impedir envio do formulário

            // Verificar se já existe uma mensagem de erro
            let errorMessage = document.getElementById('roles-error-message');

            // Se não existe, criar
            if (!errorMessage) {
                errorMessage = document.createElement('div');
                errorMessage.id = 'roles-error-message';
                errorMessage.className = 'text-danger mt-2';
                errorMessage.textContent = 'É necessário selecionar pelo menos um perfil.';
                rolesContainer.parentNode.appendChild(errorMessage);
            } else {
                // Se já existe, mostrar
                errorMessage.style.display = 'block';
            }

            // Rolar até o erro
            errorMessage.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    });
}