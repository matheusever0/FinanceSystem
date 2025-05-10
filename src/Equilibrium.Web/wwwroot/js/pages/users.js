/**
 * Finance System - Users Page
 * Scripts específicos para a página de usuários
 */

var FinanceSystem = FinanceSystem || {};
FinanceSystem.Pages = FinanceSystem.Pages || {};

FinanceSystem.Pages.Users = (function () {
    function initialize() {

        initializeUserForm();
        initializeUsersList();
        initializeUserDetails();
        initEmailAssistant();

    }

    function initializeUserForm() {
        const form = document.querySelector('form[asp-action="Create"], form[asp-action="Edit"]');

        initializeRoleManager();

        initializeStatusToggle();

        setupFormValidation(form);
    }

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

    function initializeUserDetails() {
        initializeUserTabs();

        initializeActivityLog();
    }

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

    function initEmailAssistant() {
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

        const emailFields = document.querySelectorAll('input[type="email"], input[id*="Email"], input[name*="Email"]');

        emailFields.forEach(field => {
            const wrapper = document.createElement('div');
            wrapper.className = 'email-field-wrapper position-relative';

            field.parentNode.insertBefore(wrapper, field);
            wrapper.appendChild(field);

            const dropdownBtn = document.createElement('button');
            dropdownBtn.className = 'btn btn-sm btn-outline-secondary dropdown-toggle email-provider-toggle';
            dropdownBtn.type = 'button';
            dropdownBtn.innerHTML = '<i class="fas fa-at"></i>';
            dropdownBtn.setAttribute('data-bs-toggle', 'dropdown');
            dropdownBtn.setAttribute('aria-expanded', 'false');

            const dropdown = document.createElement('ul');
            dropdown.className = 'dropdown-menu email-providers-dropdown';

            commonProviders.forEach(provider => {
                const item = document.createElement('li');
                const link = document.createElement('a');
                link.className = 'dropdown-item';
                link.href = '#';
                link.textContent = provider;
                link.addEventListener('click', function (e) {
                    e.preventDefault();

                    let username = field.value;
                    if (username.includes('@')) {
                        username = username.split('@')[0];
                    }

                    field.value = username + provider;

                    const event = new Event('change');
                    field.dispatchEvent(event);
                });

                item.appendChild(link);
                dropdown.appendChild(item);
            });

            const inputGroup = document.createElement('div');
            inputGroup.className = 'input-group';

            field.parentNode.insertBefore(inputGroup, field);
            inputGroup.appendChild(field);
            inputGroup.appendChild(dropdownBtn);
            inputGroup.appendChild(dropdown);

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

    return {
        initialize: initialize,
        initializeUserForm: initializeUserForm,
        initializeUsersList: initializeUsersList,
        initializeUserDetails: initializeUserDetails,
        updateRoleSelector: updateRoleSelector,
        initializeStatusToggle: initializeStatusToggle,
        initializeRoleManager: initializeRoleManager,
        initEmailAssistant: initEmailAssistant
    };
})();