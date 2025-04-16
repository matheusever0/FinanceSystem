/**
 * Scripts específicos para as páginas de pagamentos
 */

document.addEventListener('DOMContentLoaded', function () {
    initializePaymentForm();
    initializePaymentList();
    initializePaymentFilters();
    initializePaymentActions();
});

/**
 * Inicializa o formulário de pagamentos
 */
function initializePaymentForm() {
    const paymentForm = document.getElementById('payment-form');
    if (!paymentForm) return;

    // Manipula a seleção de método de pagamento
    const paymentMethodSelect = document.getElementById('paymentMethodSelect');
    if (paymentMethodSelect) {
        paymentMethodSelect.addEventListener('change', function () {
            togglePaymentMethodFields(this.value, this.options[this.selectedIndex].getAttribute('data-type'));
        });

        // Inicializa com o valor atual
        if (paymentMethodSelect.value) {
            togglePaymentMethodFields(
                paymentMethodSelect.value,
                paymentMethodSelect.options[paymentMethodSelect.selectedIndex].getAttribute('data-type')
            );
        }
    }

    // Manipula parcelamento
    const installmentsInput = document.getElementById('NumberOfInstallments');
    const isRecurringCheckbox = document.getElementById('IsRecurring');

    if (installmentsInput && isRecurringCheckbox) {
        isRecurringCheckbox.addEventListener('change', function () {
            if (this.checked) {
                installmentsInput.value = '1';
                installmentsInput.disabled = true;
            } else {
                installmentsInput.disabled = false;
            }
        });

        // Inicializa com o valor atual
        if (isRecurringCheckbox.checked) {
            installmentsInput.value = '1';
            installmentsInput.disabled = true;
        }
    }

    // Validação personalizada
    if (paymentForm.addEventListener) {
        paymentForm.addEventListener('submit', validatePaymentForm);
    }
}

/**
 * Alterna a visibilidade dos campos conforme o método de pagamento
 * @param {string} methodId - ID do método de pagamento
 * @param {string} methodType - Tipo do método de pagamento
 */
function togglePaymentMethodFields(methodId, methodType) {
    const creditCardSection = document.getElementById('creditCardSection');
    const bankAccountSection = document.getElementById('bankAccountSection');

    // Esconde todos os campos específicos
    if (creditCardSection) creditCardSection.style.display = 'none';
    if (bankAccountSection) bankAccountSection.style.display = 'none';

    // Mostra campos específicos conforme o tipo
    if (methodType === '2' && creditCardSection) { // Cartão de crédito
        creditCardSection.style.display = 'block';

        // Torna o campo obrigatório
        const creditCardSelect = document.getElementById('CreditCardId');
        if (creditCardSelect) creditCardSelect.required = true;
    } else if (methodType === '4' && bankAccountSection) { // Transferência bancária
        bankAccountSection.style.display = 'block';
    }
}

/**
 * Valida o formulário de pagamento
 * @param {Event} event - Evento de submissão
 */
function validatePaymentForm(event) {
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
    if (amount && (!isValidNumber(amount.value) || parseFloat(amount.value) <= 0)) {
        isValid = false;
        showError(amount, 'Informe um valor válido');
    }

    // Valida data de vencimento
    const dueDate = form.querySelector('#DueDate');
    if (dueDate && dueDate.value === '') {
        isValid = false;
        showError(dueDate, 'A data de vencimento é obrigatória');
    }

    // Valida cartão de crédito se necessário
    const paymentMethodSelect = form.querySelector('#paymentMethodSelect');
    const creditCardSelect = form.querySelector('#CreditCardId');

    if (paymentMethodSelect && creditCardSelect) {
        const methodType = paymentMethodSelect.options[paymentMethodSelect.selectedIndex].getAttribute('data-type');

        if (methodType === '2' && creditCardSelect.value === '') {
            isValid = false;
            showError(creditCardSelect, 'Selecione um cartão de crédito');
        }
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
 * Inicializa a lista de pagamentos
 */
function initializePaymentList() {
    const paymentTable = document.querySelector('.table-payments');
    if (!paymentTable) return;

    // Adiciona classes para estilização
    const rows = paymentTable.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const statusCell = row.querySelector('.payment-status');
        if (!statusCell) return;

        const status = statusCell.textContent.trim().toLowerCase();

        if (status.includes('pago')) {
            row.classList.add('table-success');
        } else if (status.includes('vencido')) {
            row.classList.add('table-danger');
        } else if (status.includes('pendente')) {
            row.classList.add('table-warning');
        } else if (status.includes('cancelado')) {
            row.classList.add('table-secondary');
        }
    });
}

/**
 * Inicializa filtros de pagamentos
 */
function initializePaymentFilters() {
    const filterButtons = document.querySelectorAll('.payment-filter');

    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Remove classe ativa de todos os botões
            filterButtons.forEach(btn => btn.classList.remove('active'));

            // Adiciona classe ativa ao botão clicado
            this.classList.add('active');

            // Filtra a tabela
            const filterValue = this.getAttribute('data-filter');
            filterPaymentTable(filterValue);
        });
    });

    // Filtro de pesquisa
    const searchInput = document.getElementById('payment-search');
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            filterPaymentTableByText(this.value);
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
                window.location.href = `/Payments/ByMonth?month=${month}&year=${year}`;
            }
        }

        monthSelect.addEventListener('change', updateMonthYearFilter);
        yearSelect.addEventListener('change', updateMonthYearFilter);
    }
}

/**
 * Filtra a tabela de pagamentos por status
 * @param {string} status - Status para filtro
 */
function filterPaymentTable(status) {
    const rows = document.querySelectorAll('.table-payments tbody tr');

    rows.forEach(row => {
        const statusCell = row.querySelector('.payment-status');
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
 * Filtra a tabela de pagamentos por texto
 * @param {string} text - Texto para filtro
 */
function filterPaymentTableByText(text) {
    const rows = document.querySelectorAll('.table-payments tbody tr');
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
 * Inicializa ações em pagamentos
 */
function initializePaymentActions() {
    // Botão para marcar como pago
    const paymentButtons = document.querySelectorAll('.btn-mark-paid');

    paymentButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const paymentId = this.getAttribute('data-id');
            const modal = document.getElementById('markPaidModal');

            if (modal) {
                const form = modal.querySelector('form');
                form.setAttribute('action', `/Payments/${paymentId}/MarkAsPaid`);
            }
        });
    });

    // Botão para cancelar pagamento
    const cancelButtons = document.querySelectorAll('.btn-cancel-payment');

    cancelButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();

            if (confirm('Tem certeza que deseja cancelar este pagamento?')) {
                const paymentId = this.getAttribute('data-id');
                const form = document.createElement('form');

                form.method = 'POST';
                form.action = `/Payments/${paymentId}/Cancel`;

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

    // Botão para excluir pagamento
    const deleteButtons = document.querySelectorAll('.btn-delete-payment');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            return confirm('Tem certeza que deseja excluir este pagamento? Esta ação não pode ser desfeita.');
        });
    });
}