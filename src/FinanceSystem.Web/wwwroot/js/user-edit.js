/**
 * Gerenciamento de edição de usuário
 */
document.addEventListener('DOMContentLoaded', function () {
    initializeStatusToggle();
    initializeRolesManager();
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
        }
    });

    // Remover perfil (delegação de eventos)
    document.addEventListener('click', function (e) {
        if (e.target.closest('.remove-role')) {
            e.preventDefault();
            const badge = e.target.closest('.role-badge');
            if (badge) {
                badge.remove();
            }
        }
    });
}