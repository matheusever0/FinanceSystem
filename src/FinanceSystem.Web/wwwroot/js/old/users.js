/**
 * Scripts específicos para as páginas de gerenciamento de usuários
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeUserForm();
    initializeRoleManager();
    initializeUsersList();
    initializeStatusToggle();
});

/**
 * Inicializa o formulário de usuário
 */
function initializeUserForm() {
    const userForm = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');
    if (!userForm) return;

    // Validação de campos específicos
    const usernameField = document.getElementById('Username');
    const emailField = document.getElementById('Email');
    const passwordField = document.getElementById('Password');

    if (usernameField) {
        usernameField.addEventListener('blur', function () {
            validateUsername(this);
        });
    }

    if (emailField) {
        emailField.addEventListener('blur', function () {
            validateEmail(this);
        });
    }

    if (passwordField) {
        // Se estiver em modo de edição, a senha é opcional
        const isEditMode = userForm.getAttribute('asp-action') === 'Edit';

        if (isEditMode) {
            passwordField.setAttribute('placeholder', 'Deixe em branco para manter a senha atual');
            passwordField.required = false;
        } else {
            passwordField.required = true;
        }
    }

    // Verificação de formulário na submissão
    userForm.addEventListener('submit', function (e) {
        // Verificar se pelo menos um perfil foi selecionado
        const selectedRoles = document.querySelectorAll('#selectedRolesContainer input[name="selectedRoles"]');

        if (selectedRoles.length === 0) {
            e.preventDefault();

            // Mostrar mensagem de erro
            let errorMessage = document.getElementById('roles-error-message');

            if (!errorMessage) {
                errorMessage = document.createElement('div');
                errorMessage.id = 'roles-error-message';
                errorMessage.className = 'text-danger mt-2';
                errorMessage.textContent = 'É necessário selecionar pelo menos um perfil.';

                const rolesContainer = document.getElementById('selectedRolesContainer');
                if (rolesContainer) {
                    rolesContainer.parentNode.appendChild(errorMessage);
                }
            } else {
                errorMessage.style.display = 'block';
            }

            // Rolagem para o erro
            errorMessage.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    });
}

/**
 * Inicializa o gerenciador de perfis (roles)
 */
function initializeRoleManager() {
    const addRoleBtn = document.getElementById('addRoleBtn');
    if (!addRoleBtn) return;

    const roleSelector = document.getElementById('roleSelector');
    const selectedRolesContainer = document.getElementById('selectedRolesContainer');

    // Adicionar novo perfil
    addRoleBtn.addEventListener('click', function () {
        if (!roleSelector || !selectedRolesContainer) return;

        const selectedRole = roleSelector.value;
        if (!selectedRole) return;

        // Verificar se o perfil já está selecionado
        const selectedInputs = selectedRolesContainer.querySelectorAll('input[name="selectedRoles"]');
        let isDuplicate = false;

        selectedInputs.forEach(function (input) {
            if (input.value === selectedRole) {
                isDuplicate = true;
            }
        });

        if (!isDuplicate) {
            // Adicionar o novo perfil
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

            selectedRolesContainer.appendChild(newRoleBadge);
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
    if (!roleSelector) return;

    const selectedRoles = Array.from(document.querySelectorAll('#selectedRolesContainer input[name="selectedRoles"]'))
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
 * Inicializa a lista de usuários
 */
function initializeUsersList() {
    const usersList = document.querySelector('.table-users');
    if (!usersList) return;

    // Adicionar classes para linhas com status inativo
    const rows = usersList.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const statusBadge = row.querySelector('.badge');
        if (statusBadge && statusBadge.textContent.trim() === 'Inativo') {
            row.classList.add('table-secondary');
        }
    });

    // Confirmação para exclusão de usuário
    const deleteButtons = usersList.querySelectorAll('.btn-danger');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            if (!confirm('Tem certeza que deseja excluir este usuário? Esta ação não pode ser desfeita.')) {
                e.preventDefault();
            }
        });
    });
}

/**
 * Inicializa o toggle de status (ativo/inativo)
 */
function initializeStatusToggle() {
    const statusSwitch = document.getElementById('IsActive');
    if (!statusSwitch) return;

    // Função para atualizar o label de status
    function updateStatusLabel() {
        const statusLabel = document.getElementById('statusLabel');
        if (!statusLabel) return;

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
 * Valida o nome de usuário
 * @param {HTMLElement} field - Campo a ser validado
 */
function validateUsername(field) {
    const value = field.value.trim();

    if (value.length < 3) {
        field.setCustomValidity('O nome de usuário deve ter pelo menos 3 caracteres');
    } else if (!/^[a-zA-Z0-9._-]+$/.test(value)) {
        field.setCustomValidity('O nome de usuário pode conter apenas letras, números, pontos, hífens e underscores');
    } else {
        field.setCustomValidity('');
    }
}

/**
 * Valida o email
 * @param {HTMLElement} field - Campo a ser validado
 */
function validateEmail(field) {
    const value = field.value.trim();
    const regex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;

    if (!regex.test(value)) {
        field.setCustomValidity('Digite um email válido');
    } else {
        field.setCustomValidity('');
    }
}