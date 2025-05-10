/**
 * Finance System - Users Page
 * Scripts específicos para a página de usuários
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

FinanceSystem.Pages.Users = (function () {
    /**
     * Inicializa a página de usuários
     */
    function initialize() {

        initializeUserForm();
        initializeUsersList();
        initializeUserDetails();

    }

    /**
     * Inicializa formulário de usuário
     */
    function initializeUserForm() {
        const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');

        initializeRoleManager();

        initializeStatusToggle();

        setupFormValidation(form);
    }

    /**
     * Inicializa o gerenciador de perfis (roles)
     */
    function initializeRoleManager() {
        const container = document.querySelector('.card-body[data-is-read-only]');

        const isReadOnly = container?.getAttribute('data-is-read-only') === 'true';

        const addRoleBtn = document.getElementById('addRoleBtn');
        const roleSelector = document.getElementById('roleSelector');

        if (!addRoleBtn || !roleSelector) return;

        if (isReadOnly) {
            addRoleBtn.setAttribute('disabled', 'true');
            roleSelector.setAttribute('disabled', 'true');

            document.querySelectorAll('.remove-role').forEach(el => {
                el.style.display = 'none';
            });

            return;
        }

        addRoleBtn.addEventListener('click', function () {
            const selectedRole = roleSelector.value;
            if (!selectedRole) return;

            const selectedInputs = document.querySelectorAll("#selectedRolesContainer input[name='selectedRoles']");
            const isDuplicate = Array.from(selectedInputs).some(input => input.value === selectedRole);

            if (!isDuplicate) {
                const newRoleBadge = document.createElement('div');
                newRoleBadge.className = 'badge bg-primary p-2 me-2 mb-2 role-badge';

                const hiddenInputVisual = document.createElement('input');
                hiddenInputVisual.type = 'hidden';
                hiddenInputVisual.name = 'selectedRoles';
                hiddenInputVisual.value = selectedRole;

                const hiddenInputReal = document.createElement('input');
                hiddenInputReal.type = 'hidden';
                hiddenInputReal.name = 'Roles';
                hiddenInputReal.value = selectedRole;

                const removeLink = document.createElement('a');
                removeLink.href = '#';
                removeLink.className = 'ms-1 text-white remove-role';
                removeLink.innerHTML = '<i class="fas fa-times"></i>';

                newRoleBadge.appendChild(hiddenInputVisual);
                newRoleBadge.appendChild(hiddenInputReal);
                newRoleBadge.appendChild(document.createTextNode(selectedRole));
                newRoleBadge.appendChild(removeLink);

                const container = document.getElementById('selectedRolesContainer');
                container.appendChild(newRoleBadge);
                roleSelector.value = '';

                updateRoleSelector();

                const errorMessage = document.getElementById('roles-error-message');
                if (errorMessage) {
                    errorMessage.style.display = 'none';
                }
            }
        });

        document.addEventListener('click', function (e) {
            if (e.target.closest('.remove-role')) {
                e.preventDefault();
                const badge = e.target.closest('.role-badge');
                if (badge) {
                    badge.remove();
                    updateRoleSelector();
                }
            }
        });

        document.getElementById('togglePassword').addEventListener('click', function () {
            const input = document.getElementById('passwordInput');
            const icon = document.getElementById('eyeIcon');
            const isPassword = input.type === 'password';

            input.type = isPassword ? 'text' : 'password';
            icon.classList.toggle('fa-eye');
            icon.classList.toggle('fa-eye-slash');
        });

        updateRoleSelector();
    }

    /**
     * Atualiza o seletor de perfis para esconder as opções já selecionadas
     */
    function updateRoleSelector() {
        const roleSelector = document.getElementById('roleSelector');
        if (!roleSelector) return;

        const selectedRoles = Array.from(document.querySelectorAll("#selectedRolesContainer input[name='selectedRoles']"))
            .map(input => input.value);

        Array.from(roleSelector.options).forEach(option => {
            if (option.value && selectedRoles.includes(option.value)) {
                option.style.display = 'none';
            } else {
                option.style.display = '';
            }
        });
    }

    /**
     * Inicializa o toggle de status (ativo/inativo)
     */
    function initializeStatusToggle() {
        const statusSwitch = document.getElementById('IsActive');
        if (!statusSwitch) return;

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

        updateStatusLabel();

        statusSwitch.addEventListener('change', updateStatusLabel);
    }

    /**
     * Configura validação personalizada para o formulário
     * @param {HTMLFormElement} form - Formulário
     */
    function setupFormValidation(form) {
        if (!form) return;

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
            const isEditMode = form.getAttribute('asp-action') === 'Edit';

            if (isEditMode) {
                passwordField.setAttribute('placeholder', 'Deixe em branco para manter a senha atual');
                passwordField.required = false;
            } else {
                passwordField.required = true;
            }

            passwordField.addEventListener('input', function () {
                validatePassword(this);
            });
        }

        form.addEventListener('submit', function (e) {
            const selectedRoles = document.querySelectorAll('#selectedRolesContainer input[name="selectedRoles"]');

            if (selectedRoles.length === 0) {
                e.preventDefault();

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

                errorMessage.scrollIntoView({ behavior: 'smooth', block: 'center' });
            }
        });
    }

    /**
     * Valida o nome de usuário
     * @param {HTMLElement} field - Campo a ser validado
     */
    function validateUsername(field) {
        const value = field.value.trim();

        if (value.length < 3) {
            showFieldError(field, 'O nome de usuário deve ter pelo menos 3 caracteres');
            return false;
        } else if (!/^[a-zA-Z0-9._-]+$/.test(value)) {
            showFieldError(field, 'O nome de usuário pode conter apenas letras, números, pontos, hífens e underscores');
            return false;
        } else {
            clearFieldError(field);
            return true;
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
            showFieldError(field, 'Digite um email válido');
            return false;
        } else {
            clearFieldError(field);
            return true;
        }
    }

    /**
     * Valida a senha
     * @param {HTMLElement} field - Campo a ser validado
     */
    function validatePassword(field) {
        const isEditMode = field.closest('form').getAttribute('asp-action') === 'Edit';
        if (isEditMode && field.value === '') {
            clearFieldError(field);
            return true;
        }

        const value = field.value;
        if (value.length < 6) {
            showFieldError(field, 'A senha deve ter pelo menos 6 caracteres');
            return false;
        } else if (!/[a-zA-Z]/.test(value) || !/\d/.test(value)) {
            showFieldError(field, 'A senha deve conter pelo menos uma letra e um número');
            return false;
        } else {
            clearFieldError(field);
            return true;
        }
    }

    /**
     * Mostra mensagem de erro para um campo
     * @param {HTMLElement} input - Campo com erro
     * @param {string} message - Mensagem de erro
     */
    function showFieldError(input, message) {
        if (FinanceSystem.Validation && FinanceSystem.Validation.showFieldError) {
            FinanceSystem.Validation.showFieldError(input, message);
            return;
        }

        let errorElement = input.parentElement.querySelector('.text-danger');
        if (!errorElement) {
            errorElement = document.createElement('span');
            errorElement.classList.add('text-danger');
            input.parentElement.appendChild(errorElement);
        }
        errorElement.innerText = message;
        input.classList.add('is-invalid');
    }

    /**
     * Remove mensagem de erro de um campo
     * @param {HTMLElement} input - Campo
     */
    function clearFieldError(input) {
        if (FinanceSystem.Validation && FinanceSystem.Validation.clearFieldError) {
            FinanceSystem.Validation.clearFieldError(input);
            return;
        }

        const errorElement = input.parentElement.querySelector('.text-danger');
        if (errorElement) {
            errorElement.innerText = '';
        }
        input.classList.remove('is-invalid');
    }

    /**
     * Inicializa a lista de usuários
     */
    function initializeUsersList() {
        const usersList = document.querySelector('.table-users');
        if (!usersList) return;

        const rows = usersList.querySelectorAll('tbody tr');
        rows.forEach(row => {
            const statusBadge = row.querySelector('.badge');
            if (statusBadge && statusBadge.textContent.trim() === 'Inativo') {
                row.classList.add('table-secondary');
            }
        });

        initializeUsersDataTable();

        initializeUserActionButtons();
    }

    /**
     * Inicializa DataTables para a tabela de usuários
     */
    function initializeUsersDataTable() {
        if (typeof $.fn.DataTable !== 'undefined') {
            $('.table-users').DataTable({
                language: {
                    url: '//cdn.datatables.net/plug-ins/1.10.25/i18n/Portuguese-Brasil.json'
                },
                responsive: true,
                pageLength: 10,
                lengthMenu: [[10, 25, 50, -1], [10, 25, 50, "Todos"]],
                columnDefs: [
                    { orderable: false, targets: -1 } // Desabilita ordenação na coluna de ações
                ]
            });
        } else if (FinanceSystem.Modules && FinanceSystem.Modules.Tables) {
            FinanceSystem.Modules.Tables.initializeTableSort();
        }
    }

    /**
     * Inicializa botões de ação para usuários
     */
    function initializeUserActionButtons() {
        const deleteButtons = document.querySelectorAll('.btn-delete-user');
        deleteButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!confirm('Tem certeza que deseja excluir este usuário? Esta ação não pode ser desfeita.')) {
                    e.preventDefault();
                }
            });
        });

        const toggleStatusButtons = document.querySelectorAll('.btn-toggle-status');
        toggleStatusButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                e.preventDefault();
                const userId = this.getAttribute('data-user-id');
                const currentStatus = this.getAttribute('data-status');
                toggleUserStatus(userId, currentStatus, this);
            });
        });
    }

    /**
     * Alterna o status de um usuário
     * @param {string} userId - ID do usuário
     * @param {string} currentStatus - Status atual
     * @param {HTMLElement} button - Botão de toggle
     */
    function toggleUserStatus(userId, currentStatus, button) {
        const url = button.getAttribute('data-url');
        if (!url) return;

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ id: userId })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    const newStatus = currentStatus === 'active' ? 'inactive' : 'active';
                    button.setAttribute('data-status', newStatus);

                    if (newStatus === 'active') {
                        button.innerHTML = '<i class="fas fa-toggle-on"></i>';
                        button.classList.remove('btn-secondary');
                        button.classList.add('btn-success');
                        button.title = 'Desativar usuário';
                    } else {
                        button.innerHTML = '<i class="fas fa-toggle-off"></i>';
                        button.classList.remove('btn-success');
                        button.classList.add('btn-secondary');
                        button.title = 'Ativar usuário';
                    }

                    const row = button.closest('tr');
                    const statusBadge = row.querySelector('.status-badge');
                    if (statusBadge) {
                        if (newStatus === 'active') {
                            statusBadge.textContent = 'Ativo';
                            statusBadge.classList.remove('bg-danger');
                            statusBadge.classList.add('bg-success');
                            row.classList.remove('table-secondary');
                        } else {
                            statusBadge.textContent = 'Inativo';
                            statusBadge.classList.remove('bg-success');
                            statusBadge.classList.add('bg-danger');
                            row.classList.add('table-secondary');
                        }
                    }
                } else {
                    alert('Erro ao alterar o status do usuário: ' + (data.message || 'Tente novamente mais tarde.'));
                }
            })
            .catch(error => {
                console.error('Erro:', error);
                alert('Ocorreu um erro ao alterar o status do usuário.');
            });
    }

    /**
     * Inicializa a página de detalhes do usuário
     */
    function initializeUserDetails() {
        initializeUserTabs();

        initializeActivityLog();
    }

    /**
     * Inicializa abas na página de detalhes
     */
    function initializeUserTabs() {
        const tabLinks = document.querySelectorAll('.user-tab-link');
        if (tabLinks.length === 0) return;

        tabLinks.forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();

                tabLinks.forEach(tab => tab.classList.remove('active'));

                this.classList.add('active');

                const tabContents = document.querySelectorAll('.user-tab-content');
                tabContents.forEach(content => content.style.display = 'none');

                const targetId = this.getAttribute('href').substring(1);
                const targetContent = document.getElementById(targetId);
                if (targetContent) {
                    targetContent.style.display = 'block';
                }
            });
        });

        if (tabLinks[0]) {
            tabLinks[0].click();
        }
    }

    /**
     * Inicializa histórico de atividades
     */
    function initializeActivityLog() {
        const activityLog = document.querySelector('.activity-log');
        if (!activityLog) return;

        const loadMoreBtn = document.getElementById('load-more-activities');
        if (loadMoreBtn) {
            loadMoreBtn.addEventListener('click', function () {
                loadMoreActivities();
            });
        }
    }

    /**
     * Carrega mais atividades no histórico
     */
    function loadMoreActivities() {
        const activityLog = document.querySelector('.activity-log');
        const loadMoreBtn = document.getElementById('load-more-activities');
        if (!activityLog || !loadMoreBtn) return;

        const userId = loadMoreBtn.getAttribute('data-user-id');
        const page = parseInt(loadMoreBtn.getAttribute('data-page') || '1') + 1;
        const url = `/User/GetActivityLog?userId=${userId}&page=${page}`;

        loadMoreBtn.textContent = 'Carregando...';
        loadMoreBtn.disabled = true;

        fetch(url)
            .then(response => response.text())
            .then(html => {
                if (html.trim() === '') {
                    loadMoreBtn.textContent = 'Não há mais atividades';
                    loadMoreBtn.disabled = true;
                } else {
                    const activityItems = document.querySelector('.activity-items');
                    activityItems.insertAdjacentHTML('beforeend', html);

                    loadMoreBtn.textContent = 'Carregar mais';
                    loadMoreBtn.disabled = false;

                    loadMoreBtn.setAttribute('data-page', page.toString());
                }
            })
            .catch(error => {
                console.error('Erro ao carregar atividades:', error);
                loadMoreBtn.textContent = 'Erro ao carregar. Tente novamente.';
                loadMoreBtn.disabled = false;
            });
    }

    return {
        initialize: initialize,
        initializeUserForm: initializeUserForm,
        initializeUsersList: initializeUsersList,
        initializeUserDetails: initializeUserDetails,
        updateRoleSelector: updateRoleSelector,
        initializeStatusToggle: initializeStatusToggle,
        initializeRoleManager: initializeRoleManager
    };
})();