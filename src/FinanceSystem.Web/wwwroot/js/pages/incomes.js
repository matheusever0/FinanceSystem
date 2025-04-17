/**
 * Scripts específicos para as páginas de receitas
 */

document.addEventListener('DOMContentLoaded', function () {
    initializeIncomeForm();
    initializeIncomesList();
    initializeIncomeFilters();
    initializeIncomeActions();
});

/**
 * Inicializa o formulário de receitas
 */
function initializeIncomeForm() {
    const incomeForm = document.getElementById('income-form');
    if (!incomeForm) return;

    // Validação de campos específicos
    const dueDateField = document.getElementById('DueDate');
    const amountField = document.getElementById('Amount');

    if (dueDateField) {
        dueDateField.addEventListener('change', function () {
            validateDateField(this, 'data de vencimento');
        });
    }

    if (amountField) {
        amountField.addEventListener('change', function () {
            validateAmountField(this);
        });
    }

    // Controle de receita recorrente
    const isRecurringSwitch = document.getElementById('isRecurringSwitch');
    const installmentsInput = document.getElementById('NumberOfInstallments');

    if (isRecurringSwitch && installmentsInput) {
        isRecurringSwitch.addEventListener('change', function () {
            if (this.checked) {
                installmentsInput.value = '1';
                installmentsInput.disabled = true;
            } else {
                installmentsInput.disabled = false;
            }
        });

        // Executar no carregamento para definir estado inicial
        if (isRecurringSwitch.checked) {
            installmentsInput.value = '1';
            installmentsInput.disabled = true;
        }
    }

    // Validação personalizada
    if (incomeForm.addEventListener) {
        incomeForm.addEventListener('submit', validateIncomeForm);
    }
}

/**
 * Valida um campo de data
 * @param {HTMLElement} field - Campo a ser validado
 * @param {string} fieldName - Nome do campo para mensagem de erro
 */
function validateDateField(field, fieldName) {
    const value = field.value;
    if (!value) {
        field.setCustomValidity(`Por favor, informe a ${fieldName}`);
    } else {
        field.setCustomValidity('');
    }
}

/**
 * Valida um campo de valor
 * @param {HTMLElement} field - Campo a ser validado
 */
function validateAmountField(field) {
    let value = field.value;

    // Converter formato brasileiro para decimal
    if (typeof value === 'string') {
        value = value.replace(/\./g, '').replace(',', '.');
    }

    value = parseFloat(value);

    if (isNaN(value) || value <= 0) {
        field.setCustomValidity('O valor deve ser maior que zero');
    } else {
        field.setCustomValidity('');
    }
}

/**
 * Valida o formulário de receita
 * @param {Event} event - Evento de submissão
 */
function validateIncomeForm(event) {
    let isValid = true;
    const form = event.target;

    // Valida descrição
    const description = form.querySelector('#Description');
    if (description && description.value.trim() === '') {
        isValid = false;
        showError(description, 'A descrição é obrigatória');
    }

    // Valida valor
    const amount = form.querySelector('#Amount');
    if (amount) {
        let value = amount.value;
        if (typeof value === 'string') {
            value = value.replace(/\./g, '').replace(',', '.');
        }
        value = parseFloat(value);

        if (isNaN(value) || value <= 0) {
            isValid = false;
            showError(amount, 'Informe um valor válido');
        }
    }

    // Valida data de vencimento
    const dueDate = form.querySelector('#DueDate');
    if (dueDate && dueDate.value === '') {
        isValid = false;
        showError(dueDate, 'A data de vencimento é obrigatória');
    }

    // Valida tipo de receita
    const incomeTypeId = form.querySelector('#IncomeTypeId');
    if (incomeTypeId && incomeTypeId.value === '') {
        isValid = false;
        showError(incomeTypeId, 'O tipo de receita é obrigatório');
    }

    if (!isValid) {
        event.preventDefault();
    }
}

/**
 * Exibe mensagem de erro para um campo
 * @param {HTMLElement} field - Campo com erro
 * @param {string} message - Mensagem de erro
 */
function showError(field, message) {
    const errorSpan = field.nextElementSibling;

    if (errorSpan && errorSpan.classList.contains('text-danger')) {
        errorSpan.textContent = message;
    } else {
        const span = document.createElement('span');
        span.className = 'text-danger';
        span.textContent = message;

        field.parentNode.insertBefore(span, field.nextSibling);
    }

    field.classList.add('is-invalid');
}

/**
 * Inicializa a lista de receitas
 */
function initializeIncomesList() {
    const incomeTable = document.querySelector('.table-incomes');
    if (!incomeTable) return;

    // Adiciona classes para estilização
    const rows = incomeTable.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const statusCell = row.querySelector('.income-status');
        if (!statusCell) return;

        const status = statusCell.textContent.trim().toLowerCase();

        if (status.includes('recebido')) {
            row.classList.add('table-success');
        } else if (status.includes('pendente')) {
            row.classList.add('table-warning');
        } else if (status.includes('cancelado')) {
            row.classList.add('table-secondary');
        }
    });
}

/**
 * Inicializa filtros de receitas
 */
function initializeIncomeFilters() {
    const filterButtons = document.querySelectorAll('.income-filter');

    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Remove classe ativa de todos os botões
            filterButtons.forEach(btn => btn.classList.remove('active'));

            // Adiciona classe ativa ao botão clicado
            this.classList.add('active');

            // Filtra a tabela
            const filterValue = this.getAttribute('data-filter');
            filterIncomeTable(filterValue);
        });
    });

    // Filtro de pesquisa
    const searchInput = document.getElementById('income-search');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            filterIncomeTableByText(this.value);
        });
    }

    // Filtro de mês/ano
    const monthSelect = document.getElementById('month-filter');
    const yearSelect = document.getElementById('year-filter');

    if (monthSelect && yearSelect) {
        function updateMonthYearFilter() {
            const month = monthSelect.value;
            const year = yearSelect.value;

            if (month && year) {
                window.location.href = `/Incomes/ByMonth?month=${month}&year=${year}`;
            }
        }

        monthSelect.addEventListener('change', updateMonthYearFilter);
        yearSelect.addEventListener('change', updateMonthYearFilter);
    }
}

/**
 * Filtra a tabela de receitas por status
 * @param {string} status - Status para filtro
 */
function filterIncomeTable(status) {
    const rows = document.querySelectorAll('.table-incomes tbody tr');

    rows.forEach(row => {
        const statusCell = row.querySelector('.income-status');
        if (!statusCell) return;

        const rowStatus = statusCell.textContent.trim().toLowerCase();

        if (status === 'all' || rowStatus.includes(status)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

/**
 * Filtra a tabela de receitas por texto
 * @param {string} text - Texto para filtro
 */
function filterIncomeTableByText(text) {
    const rows = document.querySelectorAll('.table-incomes tbody tr');
    text = text.toLowerCase();

    rows.forEach(row => {
        const content = row.textContent.toLowerCase();

        if (content.includes(text)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

/**
 * Inicializa ações em receitas
 */
function initializeIncomeActions() {
    // Botão para marcar como recebido
    const receiveButtons = document.querySelectorAll('.btn-mark-received');

    receiveButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const incomeId = this.getAttribute('data-id');
            const modal = document.getElementById('markReceivedModal');

            if (modal) {
                const form = modal.querySelector('form');
                form.setAttribute('action', `/Incomes/${incomeId}/MarkAsReceived`);
            }
        });
    });

    // Botão para cancelar receita
    const cancelButtons = document.querySelectorAll('.btn-cancel-income');

    cancelButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            if (confirm('Tem certeza que deseja cancelar esta receita?')) {
                const incomeId = this.getAttribute('data-id');
                const form = document.createElement('form');

                form.method = 'POST';
                form.action = `/Incomes/${incomeId}/Cancel`;

                // Adiciona token anti-forgery
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                if (tokenInput) {
                    const input = document.createElement('input');
                    input.type = 'hidden';
                    input.name = '__RequestVerificationToken';
                    input.value = tokenInput.value;
                    form.appendChild(input);
                }

                document.body.appendChild(form);
                form.submit();
            }
        });
    });

    // Botão para excluir receita
    const deleteButtons = document.querySelectorAll('.btn-delete-income');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            return confirm('Tem certeza que deseja excluir esta receita? Esta ação não pode ser desfeita.');
        });
    });

    // Inicializar botões de marcar parcela como recebida
    const markReceivedButtons = document.querySelectorAll('.mark-received-installment');
    markReceivedButtons.forEach(button => {
        button.addEventListener('click', function () {
            const installmentId = this.getAttribute('data-installment-id');
            document.getElementById('installmentId').value = installmentId;
        });
    });
}